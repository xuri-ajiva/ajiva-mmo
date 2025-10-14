using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using Common;
using Common.Ecs;
using Common.Networking;
using Common.Networking.AjivaQuic;
using NetworkPackets;

namespace Server;

public class ServerWorker : IHostedService, IUniversalServerMessageHandler<QConnection>, QServerEventListener
{
    private readonly Thread? _serverUpdateTask;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly CancellationTokenSource _stoppingCts;

    private readonly ILogger<ServerWorker> _logger;
    private readonly QServer _server;
    private readonly Instrumentation _instrumentation;
    private readonly ServerGameLogic _serverGameLogic;
    private readonly ConcurrentQueue<PlayerMovementPacket> _entityUpdateQueue = new();

    public ServerWorker(ILogger<ServerWorker> logger, QServer server, Instrumentation instrumentation,
        ServerGameLogic serverGameLogic)
    {
        _logger = logger;
        _server = server;
        _instrumentation = instrumentation;
        _serverGameLogic = serverGameLogic;
        _stoppingCts = new CancellationTokenSource();
        _serverUpdateTask = new Thread(() => RunLoop(_stoppingCts.Token, loopName: "GameTick",
            histogram: _instrumentation.Meter?.CreateHistogram<f64>("server.tick.duration",
                unit: "ms", description: "Duration of server tick"),
            lockedAction: CoreUpdate, tps: 100f)
        );
    }

    private void CoreUpdate(f32 delta)
    {
        while (_entityUpdateQueue.TryDequeue(out var packet))
        {
            _serverGameLogic.HandleEntityUpdate((EntityId)packet.Id, packet.Transform);
        }

        _serverGameLogic.Update(delta);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.Start(this, this);
        _serverUpdateTask?.Start();
        return Task.CompletedTask;
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

    public void OnConnectionFailed(IPEndPoint? connection, Exception exception)
    {
        _logger.LogCritical(exception, "Server connection failed: {@Client}", connection);
    }

    public void OnClientConnected(QConnection connection)
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

        if (_serverUpdateTask is null)
            return;

        try
        {
            // Signal cancellation to the executing method
            await _stoppingCts.CancelAsync();
        }
        finally
        {
            _serverUpdateTask?.Join();
        }
    }

    public void Handle(PingPacket message, QConnection userData)
    {
        if (!_serverGameLogic.GetContextForConnection(userData, out var context)) return;
    }

    public void Handle(PlayerMovementPacket message, QConnection userData)
    {
        if (!_serverGameLogic.GetContextForConnection(userData, out var context)) return;
        if (context.PrimaryEntity == (EntityId)message.Id)
            _entityUpdateQueue.Enqueue(message);
    }

    public void MessageNotHandled(string messageType, QConnection connection)
    {
        if (!_serverGameLogic.GetContextForConnection(connection, out var context)) return;
        _logger.LogWarning("Message not handled: {MessageType}", messageType);
        _serverGameLogic.RemovePlayer(context);
        context.Kick("Message not handled: " + messageType);
    }

    public void Handle(HelloPacket message, QConnection userData)
    {
        if (!_serverGameLogic.GetContextForConnection(userData, out var context)) return;
        _logger.LogInformation("Received Hello from client Version: {Version}", message.Version);
        // TODO Implement
    }

    public void Handle(DisconnectPacket message, QConnection userData)
    {
        if (!_serverGameLogic.GetContextForConnection(userData, out var context)) return;
        _logger.LogError("Client disconnected: {Reason}", message.Reason);
        _serverGameLogic.RemovePlayer(context);
        // TODO Implement
    }
}