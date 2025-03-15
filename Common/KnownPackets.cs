using NetworkPackets;

namespace ServiceDefaults;

[NetworkPacket(0)]
public partial class UnknownPacket();
[NetworkPacket(20)]
public partial class PingPacket(u64 timestamp);
[NetworkPacket(21)]
public partial class PongPacket(u64 timestamp);
[NetworkPacket(100)]
public partial class ConnectPacket(u32 protocolVersion);
[NetworkPacket(101)]
public partial class ConnectAckPacket(u32 protocolVersion);
[NetworkPacket(102)]
public partial class DisconnectPacket(string reason);

[NetworkPacket(200)]
public partial class NewEntityPacket(u32 id, u32 type, f32 x, f32 y, f32 z);
[NetworkPacket(201)]
public partial class UpdateEntityPacket(u32 id, f32 x, f32 y, f32 z);
[NetworkPacket(202)]
public partial class DeleteEntityPacket(u32 id);


