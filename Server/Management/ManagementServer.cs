using System.Collections.Concurrent;
using System.Net;
using Amazon.Runtime.Internal.Util;
using Common;
using Common.Helpers;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;
using NetworkPackets;
using Server.Core;

namespace Server.Management;

/// <summary>
/// Manages server-to-server connections for proxy entity synchronization
/// </summary>
public class ManagementServer(
    ILogger<ManagementServer> logger,
    IServiceProvider provider,
    Instrumentation instrumentation,
    QFactory factory) : IHostedService, IManagementPacketHandler<ServerContext>, QServerEventListener
{
    public QServer Server { get; } = factory.CreateServer(0);

    private readonly ConcurrentDictionary<ServerId, QConnection> _serverConnections = new();
    private readonly ConcurrentDictionary<QConnection, ServerContext> _connectionContexts = new();

    public ServerId LocalServerId { get; private set; } = ServerId.Invalid;
    public ClusterMap ClusterMap { get; } = new();
    public ServerMap ServerMap { get; } = new();
    public RegionAssignment AssignedRegion { get; private set; }
    public IPEndPoint? LocalEndPoint => Server.LocalEndPoint;
    public ManagementClient Client { get; set; } = null!;

    private GameServerInstance? _worker;
    private AsyncServiceScope _workerScope;

    private bool _hasServerMap;
    private bool _hasClusterMap;

    public void InitServer(ManagementPackets.InitServer init)
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
            config.TickInterval = init.TickInterval;
            config.InitialRegion = AssignedRegion;
            config.ManagementServer = this;

            _worker = services.GetRequiredService<GameServerInstance>();

            await _worker.InitializeAsync();
            await _worker.StartAsync();
            Client.ReadyServer(LocalServerId, _worker.ClientEndPoint!);
        }, logger);
    }

    public void OnClusterMapUpdated(RegionAssignment[] assignments)
    {
        foreach (var assignment in assignments)
        {
            ClusterMap.AssignRegion(assignment);
        }

        logger.LogInformation("Cluster map updated with {Count} assignments", assignments.Length);

        _hasClusterMap = true;
        TryConnectToNeighbors();
    }

    public void OnServerMapUpdated(ServerInfo[] servers)
    {
        foreach (var server in servers)
        {
            ServerMap.RegisterServer(server);
        }

        logger.LogInformation("Server map updated with {Count} servers", servers.Length);

        _hasServerMap = true;
        TryConnectToNeighbors();
    }

    private void TryConnectToNeighbors()
    {
        // Only connect once we have both the server map and cluster map
        if (_hasServerMap && _hasClusterMap)
        {
            _ = Task.Run(ConnectToNeighbors);
        }
    }

    private async Task ConnectToNeighbors()
    {
        if (LocalServerId == ServerId.Invalid)
            return;

        var neighborServers = GetNeighboringServers();
        logger.LogDebug("We ({ServerId}) have {Count} neighboring servers to connect to",
            LocalServerId, neighborServers.Count);

        foreach (var serverId in neighborServers)
        {
            if (_serverConnections.ContainsKey(serverId))
                continue;

            var endpoint = ServerMap.GetManagementEndPoint(serverId);
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
                    new ManagementPackets.ServerHandshake(LocalServerId.Value, "GameServer").Send(client.Connection);

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

    public HashSet<ServerId> GetNeighboringServers()
    {
        if (LocalServerId == ServerId.Invalid)
            return new HashSet<ServerId>();
        return ClusterMap.GetNeighboringServersByRegion(AssignedRegion);
    }
    

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Server.Start(this, instrumentation);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var connection in _serverConnections.Values)
        {
            await connection.DisposeAsync();
        }
    }

    public void OnClientConnected(System.Net.Quic.QuicListener server, QConnection connection)
    {
        var context = new ServerContext(connection);
        _connectionContexts[connection] = context;
        logger.LogInformation("Server connected: {RemoteEndPoint}", connection.RemoteEndPoint);
    }

    public void OnClientDisconnected(QConnection connection)
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

    public void HandlePackets()
    {
        Server.HandlePackets((packetType, ref reader, connection) =>
        {
            if (_connectionContexts.TryGetValue(connection, out var context))
            {
                ((IManagementPacketHandler<ServerContext>)this).AutoHandle(packetType, ref reader, context);
            }
        });
    }

    // Packet handlers
    public void Handle(Packets.Hello message, ServerContext userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(Packets.Disconnect message, ServerContext userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(Packets.Ping message, ServerContext userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.RegisterServer message, ServerContext userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.InitServer message, ServerContext userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ClusterMapUpdate message, ServerContext userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ServerMapUpdate message, ServerContext userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ServerReady message, ServerContext userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ServerHandshake message, ServerContext context)
    {
        context.ServerId = message.ServerId;
        _serverConnections[message.ServerId] = context.Connection;
        logger.LogInformation("Received handshake from server {ServerId} ({Name})",
            message.ServerId, message.ServerName);
    }

    public void Handle(ManagementPackets.ProxyEntityUpdate message, ServerContext context)
    {
    }

    public void Handle(ManagementPackets.ProxyEntityBatchUpdate message, ServerContext context)
    {
    }

    public void Handle(ManagementPackets.CreateProxyEntity message, ServerContext context)
    {
    }

    public void Handle(ManagementPackets.DestroyProxyEntity message, ServerContext context)
    {
    }

    public void Handle(ManagementPackets.RequestAuthorityTransfer message, ServerContext context)
    {
    }

    public void Handle(ManagementPackets.AcceptAuthorityTransfer message, ServerContext context)
    {
    }

    public void Handle(ManagementPackets.TransferEntityState message, ServerContext context)
    {
    }

    public void Handle(ManagementPackets.ProxyCollisionReport message, ServerContext context)
    {
    }
}

public class ServerContext
{
    public QConnection Connection { get; }
    public ServerId? ServerId { get; set; }

    public ServerContext(QConnection connection)
    {
        Connection = connection;
    }
}