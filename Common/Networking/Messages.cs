using Common.Ecs;
using NetworkPackets;

namespace Common.Networking;

[NetworkPacket(300)]
public partial struct BeginWorldStatePacket(int entityCount);

[NetworkPacket(301)]
public partial struct EntityCreatedPacket(int entityId, TransformComponent transform);

[NetworkPacket(302)]
public partial struct EntityUpdatePacket(int entityId, TransformComponent transform);

[NetworkPacket(303)]
public partial struct EntityDeletePacket(int entityId);

[NetworkPacket(304)]
public partial struct LoadLevelPacket(string levelName);
