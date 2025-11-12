using System.Numerics;
using Ajiva.Ecs;
using Common.SpacialPartitioning;
using MemoryPack;

namespace Common.Ecs;

// Core transform components - separated for better granularity and MMO optimization
[EcsComponent(ComponentScope.Shared, ComponentSync.ServerToClient,
    ComponentFlags.Dense | ComponentFlags.UnreliableSync)]
public partial struct Position(Vector3 value) : IComponent
{
    public Vector3 Value = value;
    public i32 Level;
}

[EcsComponent(ComponentScope.Shared, ComponentSync.ServerToClient,
    ComponentFlags.Dense | ComponentFlags.DirtyTracked | ComponentFlags.UnreliableSync)]
public partial struct Rotation(Quaternion value) : IComponent
{
    public Quaternion Value = value;
}

[EcsComponent(ComponentScope.Shared, ComponentSync.ServerToClient,
    ComponentFlags.Dense | ComponentFlags.DirtyTracked | ComponentFlags.UnreliableSync)]
public partial struct Scale : IComponent
{
    public Vector3 Value;
}

[EcsComponent(ComponentScope.Shared, ComponentSync.ServerToClient,
    ComponentFlags.Dense | ComponentFlags.DirtyTracked | ComponentFlags.UnreliableSync)]
public partial struct Velocity : IComponent
{
    public Vector3 Linear;
    public Vector3 Angular;
}

// Player input - client sends to server
[EcsComponent(ComponentScope.Shared, ComponentSync.ClientToServer, ComponentFlags.Dense)]
public partial struct PlayerInput : IComponent
{
    public Vector3 MovementInput;
    public bool JumpPressed;
}

// Entity metadata
[EcsComponent(ComponentScope.Shared, ComponentSync.ServerToClient, ComponentFlags.Dense | ComponentFlags.ReliableSync)]
public partial struct EntityType : IComponent
{
    public u32 TypeId;
}

// Server-only spatial partitioning component
[EcsComponent(ComponentScope.Server, ComponentSync.None, ComponentFlags.Dense)]
public partial struct SpatialData : IComponent
{
    public ChunkCoord CurrentChunk;
    public ChunkCoord PreviousChunk;
}

// Server-only entity ownership component - tracks which server is authoritative
[EcsComponent(ComponentScope.Server, ComponentSync.None, ComponentFlags.Dense)]
public partial struct EntityOwnership : IComponent
{
    public ServerId OwnerServerId; // Which server owns this entity
    public bool IsProxy; // True if this is a proxy entity from another server
    public bool IsInProxyRange; // True if entity is in proxy range of neighboring servers
    public i64 LastSyncTick; // Last time we received an update (for proxies)
}

// Server-only authority transfer component - for entities moving between server regions
[EcsComponent(ComponentScope.Server, ComponentSync.None, ComponentFlags.Sparse)]
public partial struct AuthorityTransfer : IComponent
{
    public ServerId TargetServerId; // Server we're transferring to
    public i64 InitiatedAt; // When the transfer started
    public bool IsTransferring; // Transfer in progress
}

// Client-only rendering components
[EcsComponent(ComponentScope.Client, ComponentSync.None, ComponentFlags.Dense)]
public partial struct RenderMesh : IComponent
{
    public u32 MeshId;
    public u32 EntityId;
}

// Client-only render asset component - maps entity type to renderable asset
[EcsComponent(ComponentScope.Client, ComponentSync.None, ComponentFlags.Dense)]
public partial struct RenderAsset : IComponent
{
    public string AssetPath; // e.g., "meshes/crate.obj", "meshes/player.obj"
    public string MaterialPath; // e.g., "materials/wood.mat", "materials/metal.mat"
    public bool IsLoaded;
}

// Client-side prediction component
[EcsComponent(ComponentScope.Client, ComponentSync.None, ComponentFlags.Dense)]
public partial struct PredictedPosition : IComponent
{
    public Vector3 Value;
    public f32 Confidence;
}

// Example/test components
[EcsComponent(ComponentScope.Shared, ComponentSync.ServerToClient,
    ComponentFlags.Dense | ComponentFlags.DirtyTracked | ComponentFlags.ImmediateSync | ComponentFlags.ReliableSync)]
public partial struct Health : IComponent
{
    public i32 Current;
    public i32 Max;
}

[EcsComponent(ComponentScope.Client, ComponentSync.None, ComponentFlags.Dense)]
public partial struct NetState : IComponent
{
    public NetEntId Id;
    public u64? DeletedTick;
    public u64 CreatedTick;
    public SpatialChunk? GhostChunk;
}

[MemoryPackable]
public partial struct Transform() : IComponent
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3 Scale { get; set; } = Vector3.One;
}