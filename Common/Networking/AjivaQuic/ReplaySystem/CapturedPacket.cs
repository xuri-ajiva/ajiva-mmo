using Interception;
using NetworkPackets;

namespace Common.Networking.AjivaQuic.ReplaySystem;

[ExcludeFromInterception]
public record CapturedPacket(TimeSpan Delta, i32 PacketType, u8[] Data)
{
    public static ReadOnlySpan<u8> Header => "AJNP"u8;
}