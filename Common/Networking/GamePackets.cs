using System.Numerics;
using Common.Ecs;
using Common.SpacialPartitioning;
using MemoryPack;
using NetworkPackets;

namespace Common.Networking;

[NetworkPacketGroup("Client")]
public abstract partial class ToClientPackets
{

    [NetworkPacket(300)]
    [MemoryPackable]
    public partial record struct BeginWorldStatePacket(u32 EntityCount);

    [NetworkPacket(301)]
    [MemoryPackable]
    public partial record struct EntityCreatedPacket(NetEntId Id, Transform Transform, u32 TypeId);

    [NetworkPacket(302)]
    [MemoryPackable]
    public partial record struct EntityUpdatePacket(NetEntId Id, Transform Transform);

    [NetworkPacket(303)]
    [MemoryPackable]
    public partial record struct EntityDestroyedPacket(NetEntId Id);

// Chunk-based updates
    [NetworkPacket(400)]
    [MemoryPackable]
    public partial record struct ChunkLoadPacket(ChunkCoord Coord, List<NetEntId> NetEntIds);

    [NetworkPacket(402)]
    [MemoryPackable]
    public partial record struct ChunkDeltaPacket(ChunkCoord Coord, List<NetEntId> NewIds, List<NetEntId> RemovedIds);

    [NetworkPacket(403)]
    [MemoryPackable]
    public partial record struct ChunkUpdatePacket(ChunkCoord Coord, IChunkData Data);

    [NetworkPacket(404)]
    [MemoryPackable]
    public partial record struct ChunkUnloadPacket(ChunkCoord ChunkCoord, List<NetEntId> NetEntIds);
}

[NetworkPacketGroup("Server")]
public abstract partial class ToServerPackets
{
    [NetworkPacket(306)]
    [MemoryPackable]
    public partial record struct PlayerMovementPacket(Position Position, Rotation Rotation);
}

[MemoryPackable]
//[MemoryPackUnion(0, typeof(PositionChunkData))]
[MemoryPackUnion(0, typeof(ChunkData<PositionChunkData>))]
[MemoryPackUnion(1, typeof(ChunkData<RotationChunkData>))]
[MemoryPackUnion(2, typeof(ChunkData<ScaleChunkData>))]
public partial interface IChunkData;

[MemoryPackable]
public readonly partial record struct PositionChunkData(u16 Qx, u16 Qy, u16 Qz)
{
    public static PositionChunkData QuantizeFor(ChunkCoord chunkCoord, SpacialGridInfo info, Vector3 position)
    {
        var baseX = chunkCoord.X * info.ChunkSize.X;
        var baseY = chunkCoord.Y * info.ChunkSize.Y;
        var baseZ = chunkCoord.Z * info.ChunkSize.Z;

        // Clamp to [0, size) before quantizing, just in case
        var localX = Math.Clamp(position.X - baseX, 0, info.ChunkSize.X);
        var localY = Math.Clamp(position.Y - baseY, 0, info.ChunkSize.Y);
        var localZ = Math.Clamp(position.Z - baseZ, 0, info.ChunkSize.Z);

        // Quantize to [0..65535]

        return new PositionChunkData(
            (u16)Math.Round((localX / info.ChunkSize.X) * u16.MaxValue),
            (u16)Math.Round((localY / info.ChunkSize.Y) * u16.MaxValue),
            (u16)Math.Round((localZ / info.ChunkSize.Z) * u16.MaxValue)
        );
    }

    public Vector3 Restore(ChunkCoord packetCoord, SpacialGridInfo info)
    {
        var baseX = packetCoord.X * info.ChunkSize.X;
        var baseY = packetCoord.Y * info.ChunkSize.Y;
        var baseZ = packetCoord.Z * info.ChunkSize.Z;

        // Dequantize back to float in [0..size]
        var localX = (Qx / (f32)u16.MaxValue) * info.ChunkSize.X;
        var localY = (Qy / (f32)u16.MaxValue) * info.ChunkSize.Y;
        var localZ = (Qz / (f32)u16.MaxValue) * info.ChunkSize.Z;
        return new Vector3(baseX + localX, baseY + localY, baseZ + localZ);
    }
}

[MemoryPackable]
public readonly partial record struct RotationChunkData(u16 Qx, u16 Qy, u16 Qz, u16 Qw)
{
    public static RotationChunkData Quantize(Quaternion rotation)
    {
        // Normalize to ensure valid quaternion
        rotation = Quaternion.Normalize(rotation);

        // Map from [-1, 1] to [0, 65535]
        return new RotationChunkData(
            (u16)Math.Round(((rotation.X + 1) / 2) * u16.MaxValue),
            (u16)Math.Round(((rotation.Y + 1) / 2) * u16.MaxValue),
            (u16)Math.Round(((rotation.Z + 1) / 2) * u16.MaxValue),
            (u16)Math.Round(((rotation.W + 1) / 2) * u16.MaxValue)
        );
    }

    public Quaternion Restore()
    {
        // Map back from [0, 65535] to [-1, 1]
        var x = (Qx / (f32)u16.MaxValue) * 2 - 1;
        var y = (Qy / (f32)u16.MaxValue) * 2 - 1;
        var z = (Qz / (f32)u16.MaxValue) * 2 - 1;
        var w = (Qw / (f32)u16.MaxValue) * 2 - 1;
        return Quaternion.Normalize(new Quaternion(x, y, z, w));
    }
}

[MemoryPackable]
public readonly partial record struct ScaleChunkData(Vector3 Value);

[MemoryPackable]
public partial record ChunkData<T>(List<T> Data, List<u8>? IndexOverride) : IChunkData;