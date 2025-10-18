using MemoryPack;
using NetworkPackets;

namespace Common.Networking;

[NetworkPacketGroup("EntryPoint")]
public partial class Management
{
    [NetworkPacket(100)]
    [MemoryPackable]
    public partial record struct ConnectPacket(Guid Token);
}

[NetworkPacketGroup("EntryPointClient")]
public partial class Management
{
    [NetworkPacket(1001)]
    [MemoryPackable]
    public partial record struct ServerList(string[] IpEndpoints);
}

[NetworkPacketGroup("Management")]
public partial class Management
{
    [NetworkPacket(100)]
    [MemoryPackable]
    public partial record struct RegisterServerPacket(string Name, i32 GamePort, i32 ManagementPort);
}