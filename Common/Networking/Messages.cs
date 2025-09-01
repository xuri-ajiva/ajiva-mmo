using Common.Ecs;
using Common.SpacialPartitioning;
using MemoryPack;
using NetworkPackets;

namespace Common.Networking;

[NetworkPacket(10)]
[MemoryPackable]
public partial record struct PingPacket(i64 Timestamp);

[NetworkPacket(300)]
[MemoryPackable]
public partial record struct BeginWorldStatePacket(u32 EntityCount);

[NetworkPacket(301)]
[MemoryPackable]
public partial record struct EntityCreatedPacket(EntityId Id, Transform Transform, u32 TypeId);

[NetworkPacket(302)]
[MemoryPackable]
public partial record struct EntityUpdatePacket(EntityId Id, Transform Transform);

[NetworkPacket(303)]
[MemoryPackable]
public partial record struct EntityDestroyedPacket(EntityId Id);

[NetworkPacket(304)]
[MemoryPackable]
public partial record struct LoadLevelPacket(string LevelName);

[NetworkPacket(305)]
[MemoryPackable]
public partial record struct EntityAssignedPacket(EntityId Id);

// Chunk-based updates
[NetworkPacket(400)]
[MemoryPackable]
public partial record struct ChunkUpdatePacket(ChunkCoord Coord, ChunkData Data);

[NetworkPacket(401)]
[MemoryPackable]
public partial record struct ChunkUnloadPacket(ChunkCoord ChunkCoord, EntityId[] EntityIds);

// Chunk data structure - contains all entities and their components in a chunk
[MemoryPackable]
public partial record struct ChunkData
{
    public EntityId[] EntityIds;
    public Transform[] Transforms; // Position, Rotation, Scale combined
    public u32[] EntityTypes;
    public Velocity[] Velocities; // Optional - sparse array, null entries for entities without velocity
}
