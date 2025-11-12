using MemoryPack;
using NetworkPackets;
using Common.SpacialPartitioning;
using Common.Ecs;
using System.Numerics;

namespace Common.Networking;

[NetworkPacketGroup("Gateway")]
public partial class GatewayPackets
{
    [NetworkPacket(100)]
    [MemoryPackable]
    public partial record struct ConnectPacket(Guid Token);
}

[NetworkPacketGroup("GatewayClient")]
public partial class GatewayPackets
{
    [NetworkPacket(1001)]
    [MemoryPackable]
    public partial record struct ServerList(string[] IpEndpoints);

    [NetworkPacket(304)]
    [MemoryPackable]
    public partial record struct LoadLevelPacket(string LevelName, SpacialGridInfo GridInfo);
}

[NetworkPacketGroup("Management")]
public partial class ManagementPackets
{
    // Cluster Management Packets (100-149)

    [NetworkPacket(100)]
    [MemoryPackable]
    public partial record struct RegisterServer(string Name, i32 ManagementPort);

    [NetworkPacket(101)]
    [MemoryPackable]
    public partial record struct InitServer(ServerId ServerId, SpacialGridInfo GridInfo, RegionAssignment Assignment);

    [NetworkPacket(102)]
    [MemoryPackable]
    public partial record struct ClusterMapUpdate(RegionAssignment[] Assignments);

    [NetworkPacket(103)]
    [MemoryPackable]
    public partial record struct ServerMapUpdate(ServerInfo[] Servers);

    [NetworkPacket(104)]
    [MemoryPackable]
    public partial record struct ServerReady(ServerId ServerId, i32 GamePort);

    // Server-to-Server Entity Sync Packets (200-249)
    // These are part of Management protocol to allow god/dev clients to intercept and control

    /// <summary>
    /// Establish connection between neighboring servers
    /// </summary>
    [NetworkPacket(200)]
    [MemoryPackable]
    public partial record struct ServerHandshake(ServerId ServerId, string ServerName);

    /// <summary>
    /// Update proxy entity position/state from owning server
    /// </summary>
    [NetworkPacket(201)]
    [MemoryPackable]
    public partial record struct ProxyEntityUpdate(
        NetEntId EntityId,
        Vector3 Position,
        Quaternion Rotation,
        Vector3 Velocity,
        i64 Tick
    );

    /// <summary>
    /// Batch update for multiple proxy entities
    /// </summary>
    [NetworkPacket(202)]
    [MemoryPackable]
    public partial record struct ProxyEntityBatchUpdate(
        ProxyEntityUpdate[] Updates
    );

    /// <summary>
    /// Create a new proxy entity on neighboring server
    /// </summary>
    [NetworkPacket(203)]
    [MemoryPackable]
    public partial record struct CreateProxyEntity(
        NetEntId EntityId,
        ServerId OwnerServerId,
        Vector3 Position,
        Quaternion Rotation,
        Vector3 Scale,
        u32 EntityTypeId,
        ChunkCoord CurrentChunk
    );

    /// <summary>
    /// Destroy a proxy entity (entity left proxy range)
    /// </summary>
    [NetworkPacket(204)]
    [MemoryPackable]
    public partial record struct DestroyProxyEntity(NetEntId EntityId);

    /// <summary>
    /// Request authority transfer (entity moved into our region)
    /// </summary>
    [NetworkPacket(205)]
    [MemoryPackable]
    public partial record struct RequestAuthorityTransfer(
        NetEntId EntityId,
        ServerId RequestingServerId,
        ChunkCoord NewChunk,
        string Reason
    );

    /// <summary>
    /// Accept authority transfer
    /// </summary>
    [NetworkPacket(206)]
    [MemoryPackable]
    public partial record struct AcceptAuthorityTransfer(
        NetEntId EntityId,
        ServerId NewOwnerServerId
    );

    /// <summary>
    /// Transfer complete - full entity state
    /// </summary>
    [NetworkPacket(207)]
    [MemoryPackable]
    public partial record struct TransferEntityState(
        NetEntId EntityId,
        Vector3 Position,
        Quaternion Rotation,
        Vector3 Scale,
        Vector3 LinearVelocity,
        Vector3 AngularVelocity,
        u32 EntityTypeId,
        byte[] AdditionalComponentData // Serialized component data
    );

    /// <summary>
    /// Report collision with proxy entity
    /// </summary>
    [NetworkPacket(208)]
    [MemoryPackable]
    public partial record struct ProxyCollisionReport(
        NetEntId ProxyEntityId,
        NetEntId LocalEntityId,
        Vector3 CollisionPoint,
        Vector3 CollisionNormal
    );

    // Dev/God Client Packets (250-299)
    // TODO: Add packets for world editing, entity spawning, server control, etc.
}