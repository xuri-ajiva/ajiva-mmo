using System.Net;
using System.Net.Quic;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;
using MemoryPack;
using NetworkPackets;

namespace ClusterManager.Net;

public partial class GatewayServer
{
    private static void Ignore()
    {
    }

    public void HandlePackets()
    {
        _gatewayServer.HandlePackets(HandlePackets);
    }

    private void HandlePackets(PacketType packetType, scoped ref MemoryPackReader reader, QConnection userData)
    {
        try
        {
            if (!_clients.TryGetValue(userData, out var context)) return;
            ((IGatewayPacketHandler<ClientInfo>)this).AutoHandle(packetType, ref reader, context);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling packet {PacketType} from {Client}", packetType, userData);
        }
    }

    public void OnClientConnected(QuicListener server, QConnection connection)
    {
        while (!_clients.TryAdd(connection, new ClientInfo(connection)))
        {
            Thread.Yield();
        }

        logger.LogInformation("Client connected from {EndPoint}, sending server list", connection.RemoteEndPoint);
        new Packets.Hello(0, Packets.ServerType.EntryPoint).Send(connection);
        var servers = Cluster.FindServersForConnection(connection);
        new GatewayPackets.ServerList(servers).Send(connection);
        new GatewayPackets.LoadLevelPacket("Sponza", new SpacialGridInfo(configuration.GridSize)).Send(connection);
    }

    public void OnClientDisconnected(QConnection connection)
    {
        while (!_clients.TryRemove(connection, out _))
        {
            Thread.Yield();
        }

        logger.LogInformation("Client disconnected from {EndPoint}", connection.RemoteEndPoint);
    }

    public void OnConnectionFailed(QuicListener server, IPEndPoint? connection, Exception exception)
    {
        logger.LogWarning(exception, "Connection failed: {RemoteEndPoint}", connection);
    }

    public void Handle(GatewayPackets.ConnectPacket message, ClientInfo userData)
    {
        var servers = Cluster.FindServersForConnection(userData.Connection);
        logger.LogInformation("Servers for client {Client}: {Servers}", userData, string.Join(", ", servers));
        new GatewayPackets.ServerList(servers).Send(userData.Connection);
    }

    public void Handle(Packets.Disconnect message, ClientInfo userData)
    {
        logger.LogInformation("Received disconnect from {RemoteEndPoint}: {Reason}",
            userData.Connection.RemoteEndPoint, message.Reason);
        using var dc = new Packets.Disconnect("Disconnect packet received").ToMessage();
        userData.Connection.Close(dc);
    }

    public void Handle(Packets.Hello message, ClientInfo userData) => Ignore();
    public void Handle(Packets.Ping message, ClientInfo userData) => Ignore();
}