using System.Net;
using System.Net.Quic;
using Common.Networking;
using Common.Networking.AjivaQuic;
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
        logger.LogInformation("Registered game server {ServerName} at {RemoteEndPoint}", message.Name,
            userData.Connection.RemoteEndPoint);
        userData.ServerName = message.Name;
        userData.GamePort = message.GamePort;
        userData.ManagementPort = message.ManagementPort;
        userData.IsReady = true;
        gatewayServer.NotifyServerListUpdated();
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
    }

    public void OnClientDisconnected(QConnection connection)
    {
        while (!GameServers.TryRemove(connection, out _))
        {
            Thread.Yield();
        }

        logger.LogInformation("Game server disconnected: {RemoteEndPoint}", connection.RemoteEndPoint);
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