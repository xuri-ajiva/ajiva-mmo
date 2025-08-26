using Common.Ecs;
using MemoryPack;
using NetworkPackets;

namespace Common.Networking;

[NetworkPacket(10)]
[MemoryPackable]
public partial struct PingPacket(i64 timestamp);

[NetworkPacket(300)]
[MemoryPackable]
public partial struct BeginWorldStatePacket(u32 entityCount);

[NetworkPacket(301)]
[MemoryPackable]
public partial struct EntityCreatedPacket(u32 entityId, Transform transform, u32 typeId);

[NetworkPacket(302)]
[MemoryPackable]
public partial struct EntityUpdatePacket(u32 entityId, Transform transform);

[NetworkPacket(303)]
[MemoryPackable]
public partial struct EntityDestroyedPacket(u32 entityId);

[NetworkPacket(304)]
[MemoryPackable]
public partial record struct LoadLevelPacket(string LevelName);

[NetworkPacket(305)]
[MemoryPackable]
public partial struct EntityAssignedPacket(u32 entityId);
