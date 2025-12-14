using System.Net;
using Common;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;
using NetworkPackets;

namespace Client.Networking;

public class PerServerManagementWorker(ManagementClient mmClient) : IWorkerManagementPacketHandler<QConnection>
{
    public ServerInfo Server { get; set; }
    public RegionAssignment Region { get; set; }
    public QClient? Client { get; set; }
    
    public async Task Connect(QFactory factory, Instrumentation instrumentation)
    {
        if (Client is { IsConnected: true }) return;
        Client = factory.CreateClient(new IPEndPoint(IPAddress.Parse(Server.Address), Server.ManagementPort));
        await Client.ConnectAsync(instrumentation);
    }

    private ILogger? _logger;

    public void Update(ILogger logger)
    {
        _logger = logger;
        Client?.HandlePackets((type, scoped ref reader, data) =>
        {
            logger.LogDebug("Received packet of type {Type} from server {ServerId}", type,
                Server.ServerId.ToString());
            ((IWorkerManagementPacketHandler<QConnection>)this).AutoHandle(type, ref reader, data);
        });
    }

    private void Log(string messageType, QConnection userData)
    {
        _logger?.LogDebug("Received {MessageType} from server {ServerId}", messageType, Server.ServerId.ToString());
    }

    public void Handle(WorkerManagement.ServerHandshake message, QConnection userData) =>
        Log("ServerHandshake", userData);

    public void Handle(WorkerManagement.BulkEntityTransfer message, QConnection userData) =>
        Log("BulkEntityTransfer", userData);

    public void Handle(WorkerManagement.UpdateEntity message, QConnection userData) => Log("UpdateEntity", userData);

    public void Handle(WorkerManagement.BulkDeleteEntity message, QConnection userData) =>
        Log("BulkDeleteEntity", userData);

    public void Handle(WorkerManagement.SpawnPrefabRequest message, QConnection userData) =>
        Log("SpawnPrefabRequest", userData);

    public void Handle(WorkerManagement.SpawnPrefabReady message, QConnection userData) =>
        mmClient.SpawnPrefabReady(message, Server.ServerId);

    public void Handle(WorkerManagement.SpawnPrefabExecute message, QConnection userData) =>
        Log("SpawnPrefabExecute", userData);

    public void Handle(Packets.Hello message, QConnection userData) => Log("Hello", userData);
    public void Handle(Packets.Disconnect message, QConnection userData) => Log("Disconnect", userData);
    public void Handle(Packets.Ping message, QConnection userData) => Log("Ping", userData);
}