using System.Numerics;
using Ajiva.Ecs;
using Common.SpacialPartitioning;
using MemoryPack;

namespace Common.Ecs;

// Core transform components - separated for better granularity and MMO optimization
[EcsComponent(ComponentScope.Shared, ComponentSync.ServerToClient,
    ComponentFlags.Dense | ComponentFlags.DirtyTracked | ComponentFlags.UnreliableSync)]
public partial struct Position : IComponent
{
    public Vector3 Value;
    public i32 Level;
}

[EcsComponent(ComponentScope.Shared, ComponentSync.ServerToClient,
    ComponentFlags.Dense | ComponentFlags.DirtyTracked | ComponentFlags.UnreliableSync)]
public partial struct Rotation : IComponent
{
    public Quaternion Value;
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
    public bool HasMoved;
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
    public float Confidence;
}

// Example/test components
[EcsComponent(ComponentScope.Shared, ComponentSync.ServerToClient,
    ComponentFlags.Dense | ComponentFlags.DirtyTracked | ComponentFlags.ImmediateSync | ComponentFlags.ReliableSync)]
public partial struct Health : IComponent
{
    public int Current;
    public int Max;
}


[MemoryPackable]
public partial struct Transform() : IComponent
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3 Scale { get; set; } = Vector3.One;

}
