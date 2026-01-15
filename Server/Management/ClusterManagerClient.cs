using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using Common;
using Common.Config;
using Common.Helpers;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;
using NetworkPackets;
using Polly;

namespace Server.Management;

public partial class ClusterManagerClient(
    ILogger<ClusterManagerClient> logger,
    QFactory factory,
    Instrumentation instrumentation,
    ServerConfiguration configuration,
    ClusterStateCache clusterStateCache,
    WorkerManagementServer mmServer) : IHostedLifecycleService, IClusterWorkerPacketHandler<IQConnection>
{
    private readonly QClient _client = factory.CreateClient();

    public void ReadyServer(ServerId server, IPEndPoint gameLocalEndPoint)
    {
        new ClusterManagementPackets.ServerReady(server, gameLocalEndPoint.Port).Send(_client.Connection!);
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
        new ClusterManagementPackets.RegisterServer(
            Guid.NewGuid().ToString("D"),
            mmServer.LocalEndPoint!.Port, Stopwatch.GetTimestamp()
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
                        logger.LogInformation("Received cluster managing packet: {PacketType}", packetType);
                        ((IClusterWorkerPacketHandler<IQConnection>)this).AutoHandle(packetType, ref reader, userData);
                    });
                }

                await Task.Delay(10, token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in cluster managing packet handling loop");
            }
        }
    }

    // Handle packets from ClusterManager
    public void Handle(Packets.Hello message, IQConnection connection) => Ignore.ForNow();

    public void Handle(Packets.Disconnect message, IQConnection connection) => Ignore.ForNow();

    public void Handle(Packets.Ping message, IQConnection connection) => Ignore.ForNow();

    public void Handle(ClusterManagementPackets.InitServer message, IQConnection connection)
    {
        logger.LogDebug("Received init server packet for server {ServerId}", message.ServerId);
        mmServer.InitServer(message);
    }

    public void Handle(ClusterManagementPackets.ClusterMapUpdate message, IQConnection connection)
    {
        logger.LogDebug("Received cluster map update with {Count} assignments",
            message.Assignments.Length);
        clusterStateCache.UpdateClusterMap(message.Assignments);
        mmServer.NotifyServerListUpdated();
    }

    public void Handle(ClusterManagementPackets.ServerMapUpdate message, IQConnection connection)
    {
        logger.LogDebug("Received server map update with {Count} servers",
            message.Servers.Length);
        clusterStateCache.UpdateServerMap(message.Servers);
        mmServer.NotifyServerListUpdated();
    }
    

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}