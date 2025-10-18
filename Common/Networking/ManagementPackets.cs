using MemoryPack;
using NetworkPackets;

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
}

[NetworkPacketGroup("Management")]
public partial class ManagementPackets
{
    [NetworkPacket(100)]
    [MemoryPackable]
    public partial record struct RegisterServer(string Name, i32 GamePort, i32 ManagementPort);
}