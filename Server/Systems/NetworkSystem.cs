using System.Diagnostics;
using System.Net;
using Client.LayerSystem;
using Common;
using Common.Networking.AjivaQuic;
using Server.Core;

namespace Server.Systems;

/// <summary>
/// Handles network communication and packet processing
/// </summary>
public partial class NetworkSystem(
    ILogger<NetworkSystem> logger,
    Instrumentation instrumentation,
    QFactory qFactory,
    NServerEcs world)
    : IServerSystem, IAsyncDisposable
{
    private readonly QServer _server = qFactory.CreateServer(0);
    internal readonly Dictionary<QConnection, ConnectionContext> _connectionContexts = new();
    private readonly CancellationTokenSource _stoppingCts = new();

    private Thread? _networkTask;
    private ChunkSystem _chunkSystem = null!;

    public async Task InitializeAsync(ISystemManager manager, CancellationToken token = default)
    {
        logger.LogInformation("Initializing network system");
        _server.Start(this, instrumentation);
        await _server.WaitForReady();
        _chunkSystem = manager.GetSystem<ChunkSystem>()!;
        logger.LogInformation("Network system initialized on {EndPoint}", _server.LocalEndPoint);
    }

    public IPEndPoint? LocalEndPoint => _server.LocalEndPoint;

    public Task StartAsync(CancellationToken token = default)
    {
        _networkTask = new Thread(() => NetworkLoop(_stoppingCts.Token))
        {
            Name = "Network Thread",
            IsBackground = true
        };

        _networkTask.Start();

        logger.LogInformation("Network system started");
        return Task.CompletedTask;
    }

    public void Tick(UpdateInfo info)
    {
        // No-op: Network loop runs in its own thread
    }

    private void NetworkLoop(CancellationToken token)
    {
        const f32 tps = 10f; // Network tick rate
        const f32 traceIntervalSeconds = 10f; // Trace interval

        f32 traceIntervalElapsed = 0;
        var lastUpdate = Stopwatch.GetTimestamp();
        while (!token.IsCancellationRequested)
        {
            if (traceIntervalElapsed > traceIntervalSeconds) Instrumentation.Instance = instrumentation;

            using var activity = Instrumentation.Instance?.StartActivity(ActivityKind.Server);

            var now = Stopwatch.GetTimestamp();
            var elapsed = Stopwatch.GetElapsedTime(lastUpdate, now);
            var delta = (f32)elapsed.TotalSeconds;

            using var exec = Instrumentation.Instance?.StartNamedActivity($"Executing Network Tick");
            Poll();

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

    private void Poll()
    {
        using var poll = Instrumentation.NamedTrace("PollPackets");
        _server.HandlePackets(DispatchPacket);
    }

    public void RemovePlayer(ConnectionContext context) => world.DoLocked(() => InternalRemovePlayer(context));

    private void InternalRemovePlayer(ConnectionContext context)
    {
        _chunkSystem.RemovePlayerFromChunks(context);
        _connectionContexts.Remove(context.Connection);
    }

    public async Task StopAsync(CancellationToken token = default)
    {
        await _stoppingCts.CancelAsync();
        _server.Stop();
        await _server.WaitForShutdown();
    }

    public async ValueTask DisposeAsync()
    {
        if (!_stoppingCts.IsCancellationRequested)
            await StopAsync();

        _networkTask?.Join(TimeSpan.FromSeconds(5));

        await _server.DisposeAsync();
        _stoppingCts.Dispose();

        logger.LogInformation("Network system disposed");
    }
}