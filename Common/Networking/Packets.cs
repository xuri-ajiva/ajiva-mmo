using MemoryPack;
using NetworkPackets;

namespace Common.Networking;

[NetworkPacketGroup("*")]
public abstract partial class Packets
{
    public enum ServerType
    {
        Unknown = 0,
        EntryPoint = 1,
        GameServer = 2,
        Management = 3
    }

    [NetworkPacket(0xFF01)]
    [MemoryPackable]
    public partial record struct HelloPacket(u64 Version, ServerType ServerType);

    [NetworkPacket(0xFF02)]
    [MemoryPackable]
    public partial record struct DisconnectPacket(string? Reason);

    [NetworkPacket(0xFF03)]
    [MemoryPackable]
    public partial record struct PingPacket(i64 Timestamp);
}