using System.Net;
using System.Net.Quic;
using Common;
using Common.Config;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;
using Polly;
using NetworkPackets;
using MemoryPack;

namespace Server;

public partial class ManagementClient(
    ILogger<ManagementClient> logger,
    QFactory factory,
    Instrumentation instrumentation,
    ServerConfiguration configuration,
    ManagementServer mmServer) : IHostedLifecycleService, QServerEventListener, IManagementPacketHandler<QConnection>
{
    private readonly QClient _client = factory.CreateClient();

    public void ReadyServer(ServerId server, IPEndPoint gameLocalEndPoint)
    {
        new ManagementPackets.ServerReady(server, gameLocalEndPoint.Port).Send(_client.Connection!);
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        mmServer.Client = this;
        await Policy.Handle<Exception>()
            .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(5),
                (exception, timeSpan) => logger.LogWarning(exception,
                    "Failed to connect to ClusterManager, retrying in {TimeSpan}", timeSpan))
            .ExecuteAsync(
                async ct => await _client.ConnectAsync(instrumentation, configuration.ClusterManagerAddress, ct),
                cancellationToken);
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        if (!_client.IsConnected)
        {
            throw new Exception("Failed to connect to ClusterManager");
        }

        await mmServer.Server.WaitForReady();
        logger.LogInformation("Registering server with ClusterManager");
        new ManagementPackets.RegisterServer(
            Guid.NewGuid().ToString("D"),
            mmServer.LocalEndPoint!.Port
        ).Send(_client.Connection!);

        // Start handling management packets
        _ = Task.Run(async () => await HandlePacketsLoop(cancellationToken), cancellationToken);
    }

    private async Task HandlePacketsLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (_client.Connection != null)
                {
                    _client.HandlePackets((packetType, ref reader, userData) =>
                    {
                        logger.LogInformation("Received management packet: {PacketType}", packetType);
                        ((IManagementPacketHandler<QConnection>)this).AutoHandle(packetType, ref reader, userData);
                    });
                }

                await Task.Delay(10, token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in management packet handling loop");
            }
        }
    }

    // Handle packets from ClusterManager
    public void Handle(Packets.Hello message, QConnection connection) => Ignore();

    public void Handle(Packets.Disconnect message, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void Handle(Packets.Ping message, QConnection connection) => Ignore();

    public void Handle(ManagementPackets.RegisterServer message, QConnection connection) => Ignore();

    public void Handle(ManagementPackets.InitServer message, QConnection connection)
    {
        logger.LogDebug("Received init server packet for server {ServerId}", message.ServerId);
        mmServer.InitServer(message);
    }

    public void Handle(ManagementPackets.ClusterMapUpdate message, QConnection connection)
    {
        logger.LogDebug("Received cluster map update with {Count} assignments",
            message.Assignments.Length);
        mmServer.OnClusterMapUpdated(message.Assignments);
    }

    public void Handle(ManagementPackets.ServerMapUpdate message, QConnection connection)
    {
        logger.LogDebug("Received server map update with {Count} servers",
            message.Servers.Length);
        mmServer.OnServerMapUpdated(message.Servers);
    }

    public void Handle(ManagementPackets.ServerReady message, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ServerHandshake message, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ProxyEntityUpdate message, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ProxyEntityBatchUpdate message, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.CreateProxyEntity message, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.DestroyProxyEntity message, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.RequestAuthorityTransfer message, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.AcceptAuthorityTransfer message, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.TransferEntityState message, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void Handle(ManagementPackets.ProxyCollisionReport message, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void OnClientConnected(QuicListener server, QConnection connection)
    {
        logger.LogInformation("Incoming S2S connection: {RemoteEndPoint}", connection.RemoteEndPoint);
    }

    public void OnClientDisconnected(QConnection connection)
    {
        logger.LogInformation("S2S connection disconnected: {RemoteEndPoint}", connection.RemoteEndPoint);
    }

    public void OnConnectionFailed(QuicListener server, IPEndPoint? connection, Exception exception)
    {
        logger.LogWarning(exception, "S2S connection failed: {RemoteEndPoint}", connection);
    }

    static void Ignore()
    {
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}