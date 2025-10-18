using System.Collections.Concurrent;
using System.Net;
using System.Net.Quic;
using Common;
using Common.Config;
using Common.Networking;
using Common.Networking.AjivaQuic;
using MemoryPack;
using NetworkPackets;

namespace ClusterManager;

public class ClusterServer(
    ILogger<ClusterServer> logger,
    QFactory factory,
    ClusterConfiguration configuration,
    Instrumentation instrumentation) : IHostedService, IManagementPacketHandler<GameServerInfo>, QServerEventListener
{
    private readonly QServer _gatewayServer = factory.CreateServer(configuration.EntryPort);
    private readonly QServer _managementServer = factory.CreateServer(configuration.ManagementPort);
    private Thread _serverThread = null!;
    private readonly CancellationTokenSource _cts = new();


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _serverThread = new Thread(RunServers) { IsBackground = true };
        _serverThread.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        _serverThread.Join();
        return Task.CompletedTask;
    }

    private void RunServers()
    {
        try
        {
            _gatewayServer.Start(this, instrumentation);
            _managementServer.Start(this, instrumentation);
            logger.LogInformation(
                "ClusterManager listening on ports {EntryPort} (clients) and {ManagementPort} (game servers)",
                configuration.EntryPort, configuration.ManagementPort);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error starting ClusterManager servers");
            return;
        }

        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                _gatewayServer.HandlePackets(HandlePackets);
                _managementServer.HandlePackets(HandlePackets);
                Thread.Sleep(10);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in ClusterManager main loop");
                Thread.Sleep(1000);
            }
        }
    }

    private ConcurrentDictionary<QConnection, GameServerInfo> GameServers { get; } = [];

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

    public string[] FindServersForConnection(QConnection connection)
    {
        return GameServers.Values
            .Where(x => x.IsReady)
            .Select(gs => new IPEndPoint(gs.Connection.RemoteEndPoint.Address, gs.GamePort).ToString())
            .ToArray();
    }

    public void NotifyServerListUpdated()
    {
        foreach (var client in _gatewayServer.GetConnections())
        {
            var servers = FindServersForConnection(client);
            new Management.ServerList(servers).Send(client);
        }
    }


    public void Handle(Management.RegisterServerPacket message, GameServerInfo userData)
    {
        logger.LogInformation("Registered game server {ServerName} at {RemoteEndPoint}", message.Name,
            userData.Connection.RemoteEndPoint);
        userData.ServerName = message.Name;
        userData.GamePort = message.GamePort;
        userData.ManagementPort = message.ManagementPort;
        userData.IsReady = true;
        NotifyServerListUpdated();
    }

    public void Handle(Packets.HelloPacket message, GameServerInfo userData)
    {
        logger.LogInformation("Received hello from {RemoteEndPoint}", userData.Connection.RemoteEndPoint);
    }

    public void Handle(Packets.DisconnectPacket message, GameServerInfo userData)
    {
        logger.LogInformation("Received disconnect from {RemoteEndPoint}: {Reason}",
            userData.Connection.RemoteEndPoint, message.Reason);
        using var dc = new Packets.DisconnectPacket("Disconnect packet received").ToMessage();
        userData.Connection.Close(dc);
    }

    public void Handle(Packets.PingPacket message, GameServerInfo userData)
    {
        logger.LogDebug("Received ping from {RemoteEndPoint}", userData.Connection.RemoteEndPoint);
    }

    public void OnClientConnected(QuicListener server, QConnection connection)
    {
        while (!GameServers.TryAdd(connection, new GameServerInfo(connection)))
        {
            Thread.Yield();
        }

        logger.LogInformation("Client connected from {EndPoint}, sending server list", connection.RemoteEndPoint);
        new Packets.HelloPacket(0, Packets.ServerType.EntryPoint).Send(connection);
        var servers = FindServersForConnection(connection);
        logger.LogInformation("Servers for client {Client}: {Servers}", connection, string.Join(", ", servers));
        new Management.ServerList(servers).Send(connection);
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
}