using System.Diagnostics;
using System.Diagnostics.Metrics;
using Common;
using Common.Config;
using Common.Networking;
using NetworkPackets;
using OpenTelemetry.Trace;
using Riptide;

namespace Server;

public class ServerWorker : IHostedService, IUniversalMessageHandler
{
    private readonly Thread _serverTickTask, _serverUpdateTask;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly CancellationTokenSource _stoppingCts;

    private readonly ILogger<ServerWorker> _logger;
    private readonly Riptide.Server _server;
    private readonly Instrumentation _instrumentation;
    private readonly ServerConfiguration _configuration;
    private readonly ServerGameLogic _serverGameLogic;

    public ServerWorker(ILogger<ServerWorker> logger, Riptide.Server server, Instrumentation instrumentation,
        ServerConfiguration configuration, ServerGameLogic serverGameLogic)
    {
        _logger = logger;
        _server = server;
        _instrumentation = instrumentation;
        _configuration = configuration;
        _serverGameLogic = serverGameLogic;
        _stoppingCts = new CancellationTokenSource();
        _serverTickTask = new Thread(() => RunLoop(_stoppingCts.Token, loopName: "ServerUpdate",
                histogram: _instrumentation.Meter.CreateHistogram<f64>("server.update.duration",
                    unit: "ms", description: "Duration of server update"),
                lockedAction: _ => _server.Update(), tps: 10f) // 100 ms interval equivalent
        );
        _serverUpdateTask = new Thread(() => RunLoop(_stoppingCts.Token, loopName: "GameTick",
            histogram: _instrumentation.Meter.CreateHistogram<f64>("server.tick.duration",
                unit: "ms", description: "Duration of server tick"),
            lockedAction: delta => _serverGameLogic.Update(delta), tps: 30f)
        );
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server.MessageReceived += ServerOnMessageReceived;
        _server.ClientConnected += ServerOnClientConnected;
        _server.ClientDisconnected += ServerOnClientDisconnected;
        _server.ConnectionFailed += ServerOnConnectionFailed;
        _server.Start(_configuration.Port, _configuration.MaxConnections);
        _serverUpdateTask.Start();
        _serverTickTask.Start();
        return Task.CompletedTask;
    }


    private void RunLoop(CancellationToken token, string loopName, Histogram<f64> histogram, Action<f32> lockedAction,
        f32 tps, u32 traceIntervallSeconds = 10)
    {
        f32 traceIntervallElapsed = 0;
        var lastUpdate = Stopwatch.GetTimestamp();
        while (!token.IsCancellationRequested)
        {
            if (traceIntervallElapsed > traceIntervallSeconds) Instrumentation.Instance = _instrumentation;

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

            histogram.Record(elapsed.TotalMilliseconds);
            lastUpdate = now;

            var sleepTime = 1f / tps - delta;
            using var wait = Instrumentation.Instance?.StartNamedActivity("Waiting",
                tags: [new KeyValuePair<string, object?>("sleepTime", sleepTime)]);
            Thread.Sleep(Math.Max(1, (i32)(sleepTime * 1000)));
            if (traceIntervallElapsed > traceIntervallSeconds)
            {
                Instrumentation.Instance = null;
                traceIntervallElapsed = 0;
            }

            traceIntervallElapsed += delta;
        }
    }


    private void ServerOnConnectionFailed(object? sender, ServerConnectionFailedEventArgs e)
    {
        _logger.LogCritical("Server connection failed: {@Client}", e.Client);
    }

    private void ServerOnClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
    {
        _logger.LogInformation("Client disconnected: {@Client}", e.Client);
        _serverGameLogic.RemovePlayer(e.Client);
    }

    private void ServerOnClientConnected(object? sender, ServerConnectedEventArgs e)
    {
        _logger.LogInformation("Client connected: {@Client}", e.Client);
        _serverGameLogic.SendWorldState(e.Client);
        _serverGameLogic.CreatePlayerEntity(e.Client);
    }

    private void ServerOnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        this.HandleMessage(e.Message!);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _server.Stop();

        if (_serverUpdateTask is null && _serverTickTask is null)
            return;

        try
        {
            // Signal cancellation to the executing method
            await _stoppingCts!.CancelAsync();
        }
        finally
        {
            _serverUpdateTask?.Join();
            _serverTickTask?.Join();
        }
    }

    public void Handle(EntityUpdatePacket message)
    {
        _serverGameLogic.HandleEntityUpdate(message);
    }

    public void MessageNotHandled(string messageType)
    {
        _logger.LogWarning("Message not handled: {MessageType}", messageType);
    }
}
