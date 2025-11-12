using System.Net;
using System.Net.Quic;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;
using MemoryPack;
using NetworkPackets;

namespace ClusterManager.Net;

public record ClientInfo(QConnection Connection);

public partial class ClusterServer
{
    private void HandlePackets(PacketType packetType, scoped ref MemoryPackReader reader, QConnection userData)
    {
        try
        {
            if (!GameServers.TryGetValue(userData, out var context)) return;
            ((IManagementPacketHandler<GameServerInfo>)this).AutoHandle(packetType, ref reader, context);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling packet {PacketType} from {Client}", packetType, userData);
        }
    }

    public void Handle(ManagementPackets.RegisterServer message, GameServerInfo userData)
    {
        logger.LogInformation("Registering game server {ServerName} at {RemoteEndPoint}", message.Name,
            userData.Connection.RemoteEndPoint);

        // Get server addresses
        var address = userData.Connection.RemoteEndPoint.Address.ToString();

        // Register server and assign region (serverId is allocated by ClusterMapService)
        var (serverId, assignment) =
            clusterMapService.RegisterServerAndAssignRegion(message.Name, address, message.ManagementPort);

        // Update userData
        userData.ServerId = serverId;
        userData.ServerName = message.Name;
        userData.ManagementPort = message.ManagementPort;

        // Track server ID mapping
        ServerIdToConnection[serverId] = userData.Connection;

        // Send region assignment to the server
        new ManagementPackets.InitServer(serverId, _spacialInfo, assignment).Send(userData.Connection);


        // Broadcast server map update to all other servers
        BroadcastMapUpdates();

        gatewayServer.NotifyServerListUpdated();

        logger.LogInformation("Game server {ServerName} ({ServerId}) registered successfully",
            message.Name, serverId);
    }

    public void Handle(ManagementPackets.InitServer message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ClusterMapUpdate message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ServerMapUpdate message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    private void BroadcastMapUpdates()
    {
        // Send full cluster state (all regions and servers)
        var (assignments, servers) = clusterMapService.GetClusterState();

        using var pServers = new ManagementPackets.ServerMapUpdate(servers).ToMessage();
        using var pAssignments = new ManagementPackets.ClusterMapUpdate(assignments).ToMessage();

        foreach (var gameServer in GameServers.Values.Where(gs => gs.IsReady))
        {
            gameServer.Connection.Send(pAssignments);
            gameServer.Connection.Send(pServers);
        }
    }

    public void Handle(ManagementPackets.ServerReady message, GameServerInfo userData)
    {
        userData.GamePort = message.GamePort;
        userData.IsReady = true;
        gatewayServer.NotifyServerListUpdated();

        if (userData.ServerId.HasValue)
        {
            clusterMapService.MarkServerReady(userData.ServerId.Value, message.GamePort);
            logger.LogInformation("Server {ServerId} marked as ready", userData.ServerId.Value);
        }

        BroadcastMapUpdates();
    }

    public void Handle(ManagementPackets.ServerHandshake message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ProxyEntityUpdate message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ProxyEntityBatchUpdate message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.CreateProxyEntity message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.DestroyProxyEntity message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.RequestAuthorityTransfer message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.AcceptAuthorityTransfer message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.TransferEntityState message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ProxyCollisionReport message, GameServerInfo userData)
    {
        throw new NotImplementedException();
    }

    public void Handle(Packets.Disconnect message, GameServerInfo userData)
    {
        logger.LogInformation("Received disconnect from {RemoteEndPoint}: {Reason}",
            userData.Connection.RemoteEndPoint, message.Reason);
        using var dc = new Packets.Disconnect("Disconnect packet received").ToMessage();
        userData.Connection.Close(dc);
    }


    public void OnClientConnected(QuicListener server, QConnection connection)
    {
        while (!GameServers.TryAdd(connection, new GameServerInfo(connection)))
        {
            Thread.Yield();
        }

        logger.LogInformation("Game server connected: {RemoteEndPoint}", connection.RemoteEndPoint);
        new Packets.Hello(0, Packets.ServerType.Management).Send(connection);
    }

    public void OnClientDisconnected(QConnection connection)
    {
        if (GameServers.TryRemove(connection, out var gameServer))
        {
            // Unregister server from cluster map
            if (gameServer.ServerId.HasValue)
            {
                ServerIdToConnection.TryRemove(gameServer.ServerId.Value, out _);
                clusterMapService.UnregisterServer(gameServer.ServerId.Value);
                BroadcastMapUpdates();
            }

            logger.LogInformation("Game server disconnected: {RemoteEndPoint}", connection.RemoteEndPoint);
        }
    }

    public void OnConnectionFailed(QuicListener server, IPEndPoint? connection, Exception exception)
    {
        logger.LogWarning(exception, "Connection failed: {RemoteEndPoint}", connection);
    }

    public void Handle(Packets.Ping message, GameServerInfo userData) => Ignore();
    public void Handle(Packets.Hello message, GameServerInfo userData) => Ignore();

    private static void Ignore()
    {
    }
}