using System.Collections.Concurrent;
using System.Net;
using Amazon.Runtime.Internal.Util;
using Client.LayerSystem;
using Common;
using Common.Helpers;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;
using NetworkPackets;
using Server.Core;
using Server.Systems;

namespace Server.Management;

/// <summary>
/// Manages server-to-server connections for proxy entity synchronization
/// </summary>
public partial class WorkerManagementServer(
    ILogger<WorkerManagementServer> logger,
    IServiceProvider provider,
    Instrumentation instrumentation,
    ClusterStateCache clusterStateCache,
    QFactory factory) : IHostedService, IWorkerManagementPacketHandler<ServerContext>, QServerEventListener,
    IAsyncDisposable
{
    public QServer Server { get; } = factory.CreateServer(0);

    private readonly ConcurrentDictionary<ServerId, IQConnection> _serverConnections = new();
    private readonly ConcurrentDictionary<IQConnection, ServerContext> _connectionContexts = new();

    public ServerId LocalServerId { get; private set; } = ServerId.Invalid;
    public RegionAssignment AssignedRegion { get; private set; }
    public IPEndPoint? LocalEndPoint => Server.LocalEndPoint;
    public ClusterManagerClient Client { get; set; } = null!;

    private GameServerInstance? _worker;
    private AsyncServiceScope _workerScope;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _workerTask;

    public void InitServer(ClusterManagementPackets.InitServer init)
    {
        AssignedRegion = init.Assignment;
        LocalServerId = init.ServerId;

        AsyncHelper.Run(async () =>
        {
            if (_worker is not null)
            {
                await _worker.DisposeAsync();
                await _workerScope.DisposeAsync();
            }

            _workerScope = provider.CreateAsyncScope();
            var services = _workerScope.ServiceProvider;

            var config = services.GetRequiredService<GameServerInstanceConfig>();

            config.GridInfo = init.GridInfo;
            config.Id = LocalServerId;
            config.TickInfo = init.TickInfo;
            config.InitialRegion = AssignedRegion;
            config.ManagementServer = this;

            _worker = services.GetRequiredService<GameServerInstance>();

            await _worker.InitializeAsync();
            await _worker.StartAsync();
            Client.ReadyServer(LocalServerId, _worker.ClientEndPoint!);
        }, logger);
    }

    public void NotifyServerListUpdated() => TryConnectToNeighbors();

    private void TryConnectToNeighbors()
    {
        // Only connect once we have both the server map and cluster map
        if (clusterStateCache is { ServerMap: not null, ClusterMap: not null })
        {
            _ = Task.Run(ConnectToNeighbors);
        }
    }

    private readonly SemaphoreSlim _connectSemaphore = new(1, 1);
    private async Task ConnectToNeighbors()
    {

        if (LocalServerId == ServerId.Invalid || clusterStateCache.ServerMap == null ||
            clusterStateCache.ClusterMap == null)
            return;

        await _connectSemaphore.WaitAsync();
        try
        {
            var neighborServers = clusterStateCache.GetNeighboringServers(AssignedRegion);
            logger.LogDebug("We ({ServerId}) have {Count} neighboring servers to connect to", LocalServerId,
                neighborServers.Count);

            foreach (var serverId in neighborServers)
            {
                if (_serverConnections.ContainsKey(serverId)) continue;
                // Only connect to higher ID servers to avoid double connections
                if (serverId.Value > LocalServerId.Value) continue;

                var endpoint = clusterStateCache.ServerMap.GetManagementEndPoint(serverId);
                if (endpoint == null)
                {
                    logger.LogWarning("No management endpoint found for neighboring server {ServerId}", serverId);
                    continue;
                }

                try
                {
                    var client = factory.CreateClient();
                    await client.ConnectAsync(instrumentation, endpoint.ToString(), CancellationToken.None);

                    if (client.Connection != null)
                    {
                        _serverConnections[serverId] = client.Connection;
                        _connectionContexts[client.Connection] = new ServerContext(client.Connection)
                        {
                            ServerId = serverId
                        };

                        // Send handshake
                        new WorkerManagement.ServerHandshake(LocalServerId, $"GameServer({serverId})").Send(
                            client.Connection);

                        logger.LogInformation("Connected to neighboring server {ServerId} at {Endpoint}",
                            serverId, endpoint);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to connect to server {ServerId}", serverId);
                }
            }
        }
        finally
        {
            _connectSemaphore.Release();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Server.Start(this, instrumentation);
        _workerTask = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    HandlePackets();
                    await Task.Delay(10, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in WorkerManagementServer main loop");
                }
            }
        }, _cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cancellationTokenSource.CancelAsync();
        if (_workerTask != null) await _workerTask;
        using var msg = new Packets.Disconnect("Server shutting down").ToMessage();
        foreach (var connection in _serverConnections.Values)
        {
            connection.Close(msg);
        }
    }

    public void OnClientConnected(System.Net.Quic.QuicListener server, IQConnection connection)
    {
        var context = new ServerContext(connection);
        _connectionContexts[connection] = context;
        logger.LogInformation("Server connected: {RemoteEndPoint}", connection.RemoteEndPoint);
    }

    public void OnClientDisconnected(IQConnection connection)
    {
        if (_connectionContexts.TryRemove(connection, out var context))
        {
            if (context.ServerId.HasValue)
            {
                _serverConnections.TryRemove(context.ServerId.Value, out _);
            }

            logger.LogInformation("Server disconnected: {RemoteEndPoint}", connection.RemoteEndPoint);
        }
    }

    public void OnConnectionFailed(System.Net.Quic.QuicListener server, IPEndPoint? connection,
        Exception exception)
    {
        logger.LogWarning(exception, "Server connection failed: {RemoteEndPoint}", connection);
    }

    public async ValueTask DisposeAsync()
    {
        _cancellationTokenSource.Dispose();
        _workerTask?.Dispose();
        await Server.DisposeAsync();
    }
}

public class ServerContext
{
    public IQConnection Connection { get; }
    public ServerId? ServerId { get; set; }

    public ServerContext(IQConnection connection)
    {
        Connection = connection;
    }
}