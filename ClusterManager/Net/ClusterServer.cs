using System.Collections.Concurrent;
using System.Net;
using Common;
using Common.Config;
using Common.Networking.AjivaQuic;
using NetworkPackets;

namespace ClusterManager.Net;


public partial class ClusterServer(
    ILogger<ClusterServer> logger,
    QFactory factory,
    ClusterConfiguration configuration,
    Instrumentation instrumentation,
    GatewayServer gatewayServer) : IHostedService, IManagementPacketHandler<GameServerInfo>, QServerEventListener
{
    private readonly QServer _managementServer = factory.CreateServer(configuration.ManagementPort);
    private Thread _serverThread = null!;
    private readonly CancellationTokenSource _cts = new();
    private ConcurrentDictionary<QConnection, GameServerInfo> GameServers { get; } = [];

    public string[] FindServersForConnection(QConnection connection)
    {
        return GameServers.Values
            .Where(x => x.IsReady)
            .Select(gs => new IPEndPoint(gs.Connection.RemoteEndPoint.Address, gs.GamePort).ToString())
            .ToArray();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _serverThread = new Thread(RunServers) { IsBackground = true };
        _serverThread.Start();
        gatewayServer.Cluster = this;
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
            _managementServer.Start(this, instrumentation);
            gatewayServer.Start();
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
                gatewayServer.HandlePackets();
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
}