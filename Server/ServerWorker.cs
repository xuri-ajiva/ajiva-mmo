using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Quic;
using Common;
using Common.Config;
using Common.Ecs;
using Common.Networking;
using Common.Networking.AjivaQuic;
using MemoryPack;
using NetworkPackets;

namespace Server;

public class ServerWorker : IHostedService, IServerPacketHandler<ConnectionContext>, QServerEventListener
{
    private readonly Thread? _serverUpdateTask, _serverNetworkTask;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly CancellationTokenSource _stoppingCts;

    private readonly ILogger<ServerWorker> _logger;
    private readonly QServer _server;
    private readonly ManagementService _managementService;
    private readonly Instrumentation _instrumentation;
    private readonly ServerGameLogic _serverGameLogic;

    public ServerWorker(ILogger<ServerWorker> logger, QFactory factory, ServerConfiguration configuration,
        ManagementService managementService, Instrumentation instrumentation,
        ServerGameLogic serverGameLogic)
    {
        _logger = logger;
        _server = factory.CreateServer(0);
        _managementService = managementService;
        _instrumentation = instrumentation;
        _serverGameLogic = serverGameLogic;
        _stoppingCts = new CancellationTokenSource();
        _serverUpdateTask = new Thread(() => RunLoop(_stoppingCts.Token, loopName: "GameTick",
            histogram: _instrumentation.Meter?.CreateHistogram<f64>("server.tick.duration",
                unit: "ms", description: "Duration of server tick"),
            lockedAction: CoreUpdate, tps: 100f)
        );
        _serverNetworkTask = new Thread(() => RunLoop(_stoppingCts.Token, loopName: "GameTick",
            histogram: _instrumentation.Meter?.CreateHistogram<f64>("server.tick.duration",
                unit: "ms", description: "Duration of server tick"),
            lockedAction: CorePull, tps: 100f)
        );
    }

    private void CoreUpdate(f32 delta)
    {
        _serverGameLogic.Update(delta);
    }

    private void CorePull(f32 delta)
    {
        _server.HandlePackets(DispatchPacket);
    }

    private void DispatchPacket(PacketType packetType, scoped ref MemoryPackReader reader, QConnection connection)
    {
        try
        {
            if (!_serverGameLogic.GetContextForConnection(connection, out var context)) return;
            ((IServerPacketHandler<ConnectionContext>)this).AutoHandle(packetType, ref reader, context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error handling packet {PacketType} from {Client}", packetType, connection);
            //_serverGameLogic.RemovePlayer(_connectionContexts[connection]);
            //_connectionContexts[connection].Kick("Error handling packet: " + packetType);
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start(this, _instrumentation);
        await _server.WaitForReady();
        _serverUpdateTask?.Start();
        _serverNetworkTask?.Start();
        await Task.Delay(Random.Shared.Next(1, 5) * 1000, cancellationToken);
        await _managementService.RegisterServer(_server.LocalEndPoint, cancellationToken);
    }


    private void RunLoop(CancellationToken token, string loopName, Histogram<f64>? histogram, Action<f32> lockedAction,
        f32 tps, u32 traceIntervalSeconds = 10)
    {
        f32 traceIntervalElapsed = 0;
        var lastUpdate = Stopwatch.GetTimestamp();
        while (!token.IsCancellationRequested)
        {
            if (traceIntervalElapsed > traceIntervalSeconds) Instrumentation.Instance = _instrumentation;

            using var activity = Instrumentation.Instance?.StartActivity(ActivityKind.Server);

            var now = Stopwatch.GetTimestamp();
            var elapsed = Stopwatch.GetElapsedTime(lastUpdate, now);
            var delta = (f32)elapsed.TotalSeconds;

            try
            {
                using (Instrumentation.Instance?.StartNamedActivity($"Entering Locked Section ({loopName})"))
                    _semaphore.Wait(token);
                using var exec = Instrumentation.Instance?.StartNamedActivity($"Executing ({loopName})");
                lockedAction.Invoke(delta);
            }
            finally
            {
                _semaphore.Release();
            }

            histogram?.Record(elapsed.TotalMilliseconds);
            lastUpdate = now;

            var sleepTime = 1f / tps - delta;
            using var wait = Instrumentation.Instance?.StartNamedActivity("Waiting",
                tags: [new KeyValuePair<string, object?>("sleepTime", sleepTime)]);
            Thread.Sleep(Math.Max(1, (i32)(sleepTime * 1000)));
            if (traceIntervalElapsed > traceIntervalSeconds)
            {
                Instrumentation.Instance = null;
                traceIntervalElapsed = 0;
            }

            traceIntervalElapsed += delta;
        }
    }


    public void OnClientDisconnected(QConnection connection)
    {
        if (!_serverGameLogic.GetContextForConnection(connection, out var context)) return;
        _logger.LogInformation("Client disconnected: {Client}", context);
        _serverGameLogic.RemovePlayer(context);
    }

    public void OnConnectionFailed(QuicListener server, IPEndPoint? connection, Exception exception)
    {
        _logger.LogCritical(exception, "Server connection failed: {@Client}", connection);
    }

    public void OnClientConnected(QuicListener server, QConnection connection)
    {
        Instrumentation.Instance = _instrumentation;
        using var activity = Instrumentation.NamedTrace("ClientConnected", ActivityKind.Server);
        // _serverGameLogic.SendWorldState(connection);
        _logger.LogInformation("Client connected: {Client}", connection);
        _serverGameLogic.SetupPlayer(connection);
    }


    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _server.DisposeAsync();

        if (_serverUpdateTask is null && _serverNetworkTask is null)
            return;

        try
        {
            // Signal cancellation to the executing method
            await _stoppingCts.CancelAsync();
        }
        finally
        {
            _serverUpdateTask?.Join();
            _serverNetworkTask?.Join();
            _stoppingCts.Dispose();
        }
    }

    public void Handle(Packets.PingPacket message, ConnectionContext context)
    {
    }

    public void Handle(ToServerPackets.PlayerMovementPacket message, ConnectionContext context)
    {
        var entityId = (EntityId)message.Id;
        if (context.PrimaryEntity == entityId)
            _serverGameLogic.HandleEntityUpdate(entityId, message.Transform);
    }

    public void MessageNotHandled(string messageType, ConnectionContext context)
    {
        _logger.LogWarning("Message not handled: {MessageType}", messageType);
        _serverGameLogic.RemovePlayer(context);
        context.Kick("Message not handled: " + messageType);
    }

    public void Handle(Packets.HelloPacket message, ConnectionContext context)
    {
        _logger.LogInformation("Received Hello from client Version: {Version}", message.Version);
        // TODO Implement
    }

    public void Handle(Packets.DisconnectPacket message, ConnectionContext context)
    {
        _logger.LogError("Client disconnected: {Reason}", message.Reason);
        _serverGameLogic.RemovePlayer(context);
        // TODO Implement
    }
}