using Common.SpacialPartitioning;
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

    [NetworkPacket(304)]
    [MemoryPackable]
    public partial record struct LoadLevelPacket(string LevelName, SpacialGridInfo GridInfo);
}

[NetworkPacketGroup("ClusterGateway")]
public partial class ClusterManagementPackets
{
    [NetworkPacket(100)]
    [MemoryPackable]
    public partial record struct RegisterServer(string Name, i32 ManagementPort, i64 ClientTimestamp);

    [NetworkPacket(101)]
    [MemoryPackable]
    public partial record struct ServerReady(ServerId ServerId, i32 GamePort);
    
    [NetworkPacket(200)]
    [MemoryPackable]
    public partial record struct RegisterObserver();
}

[NetworkPacketGroup("ClusterWorker")]
public partial class ClusterManagementPackets
{
    [NetworkPacket(101)]
    [MemoryPackable]
    public partial record struct InitServer(
        ServerId ServerId,
        SpacialGridInfo GridInfo,
        RegionAssignment Assignment,
        GlobalTickInfo TickInfo);

    [NetworkPacket(102)]
    [MemoryPackable]
    public partial record struct ClusterMapUpdate(RegionAssignment[] Assignments);

    [NetworkPacket(103)]
    [MemoryPackable]
    public partial record struct ServerMapUpdate(ServerInfo[] Servers);
}

public partial record struct GlobalTickInfo(TimeSpan TickInterval, u64 TickServer, i64 TimestampServer, i64 TimestampClient);