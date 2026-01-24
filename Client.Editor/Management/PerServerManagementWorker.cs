using System.Net;
using Client.Editor.Management;
using Common;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;
using NetworkPackets;

namespace Client.Networking;

public class PerServerManagementWorker(ManagementClient mmClient) : IWorkerManagementPacketHandler<IQConnection>
{
    public ServerInfo Server { get; set; }
    public RegionAssignment Region { get; set; }
    public u32? RegionGizmoEntityId { get; set; }
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
            ((IWorkerManagementPacketHandler<IQConnection>)this).AutoHandle(type, ref reader, data);
        });
    }

    private void Log(string messageType, IQConnection userData)
    {
        _logger?.LogDebug("Received {MessageType} from server {ServerId}", messageType, Server.ServerId.ToString());
    }

    public void Handle(WorkerManagement.ServerHandshake message, IQConnection userData) =>
        Log("ServerHandshake", userData);

    public void Handle(WorkerManagement.BulkEntityTransfer message, IQConnection userData) =>
        Log("BulkEntityTransfer", userData);

    public void Handle(WorkerManagement.UpdateEntity message, IQConnection userData) => Log("UpdateEntity", userData);

    public void Handle(WorkerManagement.BulkDeleteEntity message, IQConnection userData) =>
        Log("BulkDeleteEntity", userData);

    public void Handle(WorkerManagement.SpawnPrefabRequest message, IQConnection userData) =>
        Log("SpawnPrefabRequest", userData);

    public void Handle(WorkerManagement.SpawnPrefabReady message, IQConnection userData) =>
        mmClient.SpawnPrefabReady(message, Server.ServerId);

    public void Handle(WorkerManagement.SpawnPrefabExecute message, IQConnection userData) =>
        Log("SpawnPrefabExecute", userData);

    public void Handle(Packets.Hello message, IQConnection userData) => Log("Hello", userData);
    public void Handle(Packets.Disconnect message, IQConnection userData) => Log("Disconnect", userData);
    public void Handle(Packets.Ping message, IQConnection userData) => Log("Ping", userData);
}