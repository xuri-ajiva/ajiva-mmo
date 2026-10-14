using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using Client.Game;
using Client.Networking;
using Common;
using Common.Config;
using Common.Helpers;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.Rpc;
using Common.SpacialPartitioning;
using NetworkPackets;
using Polly;

namespace Client.Editor.Management;

public partial class ManagementClient(
    ClientConfiguration config,
    ILogger<ClientNetworkManager> logger,
    ClientGameLogic game,
    QFactory factory,
    Instrumentation instrumentation) : IHostedLifecycleService, IClusterWorkerPacketHandler<IQConnection>
{
    public readonly QClient ClusterManager = factory.CreateClient();
    private Task? _backgroundTask;
    private readonly CancellationTokenSource _cts = new();
    public Dictionary<ServerId, PerServerManagementWorker> Servers = new();


    public Task StartedAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(config.ClusterManagerAddress)) return Task.CompletedTask;
        _backgroundTask = Task.Run(() =>
        {
            try
            {
                return Work();
            }
            catch (Exception e)
            {
                logger.LogError(e, "ManagementClient background task failed");
                return Task.FromException(e);
            }
        }, _cts.Token);
        return Task.CompletedTask;
    }

    private async Task? Work()
    {
        await Policy.Handle<Exception>()
            .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(5))
            .ExecuteAsync(
                async ct => await ClusterManager.ConnectAsync(instrumentation, config.ClusterManagerAddress, ct),
                _cts.Token);
        Debug.Assert(ClusterManager.Connection != null, "ClusterManager.Connection != null");

        ClusterManager.GetProxy<IGatewayServerForWorkers>()
            .RegisterObserver();

        ClusterManager.
        
        new ClusterManagementPackets.RegisterObserver().Send(ClusterManager.Connection);

        while (!_cts.IsCancellationRequested)
        {
            ClusterManager.HandlePackets((type, scoped ref reader, data) =>
            {
                ((IClusterWorkerPacketHandler<IQConnection>)this).AutoHandle(type, ref reader, data);
            });
            foreach (var serverInfoAndRegion in Servers.Values)
            {
                serverInfoAndRegion.Update(logger);
            }

            await Task.Delay(100, _cts.Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }


    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartingAsync(CancellationToken cancellationToken) => _backgroundTask ?? Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => _cts.CancelAsync();


    public void Handle(Packets.Disconnect message, IQConnection userData) => _ = Task.Run(userData.DisposeAsync);
    public void Handle(Packets.Ping message, IQConnection userData) => Ignore.ForNow();
    public void Handle(ClusterManagementPackets.InitServer message, IQConnection userData) => Ignore.ForNow();

    public void Handle(Packets.Hello message, IQConnection userData)
    {
        logger.LogInformation("Connected to ClusterManager {Version} {Type}", message.Version, message.ServerType);
    }


    public void Handle(ClusterManagementPackets.ClusterMapUpdate message, IQConnection userData)
    {
        foreach (var region in message.Assignments)
        {
            if (!Servers.TryGetValue(region.ServerId, out var existing))
            {
                existing = Servers[region.ServerId] = new PerServerManagementWorker(this);
            }

            existing.Region = region;
        }
    }

    public void Handle(ClusterManagementPackets.ServerMapUpdate message, IQConnection userData)
    {
        foreach (var server in message.Servers)
        {
            if (!Servers.TryGetValue(server.ServerId, out var existing))
            {
                existing = Servers[server.ServerId] = new PerServerManagementWorker(this);
            }

            existing.Server = server;
        }
    }

    private readonly ConcurrentDictionary<TransactionId, TransactionCollector> _spawnReduction = new();

    public class TransactionCollector
    {
        public u32 ReadyServers { get; set; }
        public u32 TotalServers { get; set; }
        public u64 MaxReadyTick { get; set; }
        public List<PerServerManagementWorker> Servers { get; } = new();
    }

    public void SpawnTestEntities()
    {
        HyperAreaCoord area = new(new ChunkCoord(-2, 5, -2, 0), new ChunkExtent(4, 1, 4, 1));

        var size = game.Grid?.GridInfo.ChunkSize.ToVector4().AsVector3() ?? new Vector3(16, 16, 16);
        var transactionId = new TransactionId(Guid.NewGuid());
        var collector = _spawnReduction[transactionId] = new TransactionCollector();
        foreach (var serverInfoAndRegion in Servers.Values)
        {
            var spawnArea = area.Intersect(serverInfoAndRegion.Region.Area);

            if (spawnArea.Extent.X <= 0 || spawnArea.Extent.Z <= 0)
                continue;

            collector.TotalServers++;
            collector.Servers.Add(serverInfoAndRegion);

            _ = Task.Run(async () =>
            {
                await serverInfoAndRegion.Connect(factory, instrumentation);
                new WorkerManagement.SpawnPrefabRequest(transactionId, new PrefabId(0),
                    spawnArea.Min.ToVector4().AsVector3() * size,
                    spawnArea.Max.ToVector4().AsVector3() * size, 2).Send(serverInfoAndRegion.Client!.Connection!);
            });

            logger.LogInformation("Sent SpawnPrefabRequest to server {ServerId} for area {Area}",
                serverInfoAndRegion.Server.ServerId, spawnArea.ToReadableSpan());
        }

        logger.LogInformation("SpawnPrefabRequest sent to {Count} servers for transaction {Transaction}",
            collector.TotalServers, transactionId);
    }

    public void SpawnPrefabReady(WorkerManagement.SpawnPrefabReady message, ServerId serverServerId)
    {
        if (_spawnReduction.TryGetValue(message.Transaction, out var collector))
        {
            collector.ReadyServers++;
            collector.MaxReadyTick = Math.Max(collector.MaxReadyTick, message.ReadyTick);

            if (collector.ReadyServers == collector.TotalServers)
            {
                logger.LogInformation("All servers ready for transaction {Transaction}, executing at tick {Tick}",
                    message.Transaction, collector.MaxReadyTick);
                using var executeMgs =
                    new WorkerManagement.SpawnPrefabExecute(message.Transaction, collector.MaxReadyTick).ToMessage();
                foreach (var server in collector.Servers)
                {
                    server.Client!.Connection!.Send(executeMgs);
                }

                _spawnReduction.TryRemove(message.Transaction, out _);
            }
        }
    }
}