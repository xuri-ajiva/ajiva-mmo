using Common.Ecs;
using NetworkPackets;

namespace Common.Networking;

[NetworkPacket(300)]
public partial struct BeginWorldStatePacket(u32 entityCount);

[NetworkPacket(301)]
public partial struct EntityCreatedPacket(u32 entityId, Transform transform, u32 typeId);

[NetworkPacket(302)]
public partial struct EntityUpdatePacket(u32 entityId, Transform transform);

[NetworkPacket(303)]
public partial struct EntityDestroyedPacket(u32 entityId);

[NetworkPacket(304)]
public partial struct LoadLevelPacket(string levelName);
[NetworkPacket(305)]
public partial struct EntityAssignedPacket(u32 entityId);
