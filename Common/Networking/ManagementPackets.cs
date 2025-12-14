using MemoryPack;
using NetworkPackets;
using Common.SpacialPartitioning;
using Common.Ecs;
using System.Numerics;

namespace Common.Networking;

[NetworkPacketGroup("WorkerManagement")]
public partial class WorkerManagement
{
    [NetworkPacket(200)]
    [MemoryPackable]
    public partial record struct ServerHandshake(ServerId ServerId, string ServerName);


    [NetworkPacket(250)]
    [MemoryPackable]
    public partial record struct BulkEntityTransfer(
        ServerId OriginalServerId,
        CoreEntityData[] Entities
    );

    [NetworkPacket(251)]
    [MemoryPackable]
    public partial record struct UpdateEntity(
        NetEntId EntityId,
        Vector3? Position,
        Quaternion? Rotation,
        Vector3? Velocity
    );

    [NetworkPacket(252)]
    [MemoryPackable]
    public partial record struct BulkDeleteEntity(
        NetEntId[] EntityIds
    );

    // Transactional Spawn (260-269)

    [NetworkPacket(260)]
    [MemoryPackable]
    public partial record struct SpawnPrefabRequest(
        TransactionId Transaction,
        PrefabId Prefab,
        Vector3 MinBounds,
        Vector3 MaxBounds,
        f32 Spacing
    );

    [NetworkPacket(261)]
    [MemoryPackable]
    public partial record struct SpawnPrefabReady(
        TransactionId Transaction,
        ServerId Server,
        u64 ReadyTick
    );

    [NetworkPacket(262)]
    [MemoryPackable]
    public partial record struct SpawnPrefabExecute(
        TransactionId Transaction,
        u64 ExecutionTick
    );
}

[MemoryPackable]
public partial record struct PrefabId(u32 Id);

[MemoryPackable]
public partial record struct TransactionId(Guid Id);

[MemoryPackable]
public partial record struct CoreEntityData(
    EntityId EntityId,
    Transform Transform,
    Velocity Velocity,
    u32 EntityTypeId
);