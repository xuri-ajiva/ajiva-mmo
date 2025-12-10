using Common.Helpers;
using Common.Networking;
using NetworkPackets;
using Server.Systems;

namespace Server.Management;

public partial class WorkerManagementServer
{
    public void HandlePackets()
    {
        Server.HandlePackets((packetType, ref reader, connection) =>
        {
            if (_connectionContexts.TryGetValue(connection, out var context))
            {
                ((IWorkerManagementPacketHandler<ServerContext>)this).AutoHandle(packetType, ref reader, context);
            }
        });
    }
    public void Handle(Packets.Hello message, ServerContext userData) => Ignore.ForNow();

    public void Handle(Packets.Disconnect message, ServerContext userData) => Ignore.ForNow();

    public void Handle(Packets.Ping message, ServerContext userData) => Ignore.ForNow();

    public void Handle(WorkerManagement.ServerHandshake message, ServerContext context)
    {
        context.ServerId = message.ServerId;
        _serverConnections[message.ServerId] = context.Connection;
        logger.LogInformation("Received handshake from server {ServerId} ({Name})",
            message.ServerId, message.ServerName);
    }

    private TransactionSystem? TransactionSystem => _worker?.TransactionSystem;

    public void Handle(WorkerManagement.BulkEntityTransfer message, ServerContext userData)
    {
        TransactionSystem?.TransferEntities(message.OriginalServerId, message.Entities);
    }

    public void Handle(WorkerManagement.UpdateEntity message, ServerContext userData)
    {
        TransactionSystem?.UpdateEntity(message.EntityId, message.Position, message.Rotation, message.Velocity);
    }

    public void Handle(WorkerManagement.BulkDeleteEntity message, ServerContext userData)
    {
        TransactionSystem?.DeleteEntities(message.EntityIds);
    }

    public void Handle(WorkerManagement.SpawnPrefabRequest message, ServerContext userData)
    {
        TransactionSystem?.PrepareSpawn(message.Transaction, message.Prefab, message.MinBounds, message.MaxBounds,
            message.Spacing, tick =>
            {
                new WorkerManagement.SpawnPrefabReady(message.Transaction, LocalServerId, tick).Send(
                    userData.Connection);
            });
    }

    public void Handle(WorkerManagement.SpawnPrefabReady message, ServerContext userData) => Ignore.ForNow();

    public void Handle(WorkerManagement.SpawnPrefabExecute message, ServerContext userData)
    {
        TransactionSystem?.SetSpawnExecutionTick(message.Transaction, message.ExecutionTick);
    }
}