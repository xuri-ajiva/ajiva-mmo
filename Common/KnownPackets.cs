using NetworkPackets;

namespace ServiceDefaults;

[NetworkPacket(0)]
public partial struct UnknownPacket();
[NetworkPacket(20)]
public partial struct PingPacket(u64 timestamp);
[NetworkPacket(21)]
public partial struct PongPacket(u64 timestamp);
[NetworkPacket(100)]
public partial struct ConnectPacket(u32 protocolVersion);
[NetworkPacket(101)]
public partial struct ConnectAckPacket(u32 protocolVersion);
[NetworkPacket(102)]
public partial struct DisconnectPacket(string reason);

[NetworkPacket(200)]
public partial struct NewEntityPacket(u32 id, u32 type, f32 x, f32 y, f32 z);
[NetworkPacket(201)]
public partial struct UpdateEntityPacket(u32 id, f32 x, f32 y, f32 z);
[NetworkPacket(202)]
public partial struct DeleteEntityPacket(u32 id);


