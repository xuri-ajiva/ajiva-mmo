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

public partial class GatewayServer(
    ILogger<GatewayServer> logger,
    QFactory factory,
    ClusterConfiguration configuration,
    Instrumentation instrumentation) : QServerEventListener, IGatewayPacketHandler<ClientInfo>
{
    private readonly QServer _gatewayServer = factory.CreateServer(configuration.EntryPort);
    private readonly ConcurrentDictionary<QConnection, ClientInfo> _clients = [];
    public ClusterServer Cluster { get; internal set; } = null!;

    public void Start()
    {
        _gatewayServer.Start(this, instrumentation);
    }

    public void NotifyServerListUpdated()
    {
        foreach (var client in _gatewayServer.GetConnections())
        {
            var servers = Cluster.FindServersForConnection(client);
            new GatewayPackets.ServerList(servers).Send(client);
        }
    }
}