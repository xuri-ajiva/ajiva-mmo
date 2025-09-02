using System.Numerics;
using Common.Ecs;
using Common.SpacialPartitioning;
using MemoryPack;
using NetworkPackets;

namespace Common.Networking;

[NetworkPacket(10)]
[MemoryPackable]
public partial record struct PingPacket(i64 Timestamp);

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

[NetworkPacket(304)]
[MemoryPackable]
public partial record struct LoadLevelPacket(string LevelName, SpacialGridInfo GridInfo);

[NetworkPacket(305)]
[MemoryPackable]
public partial record struct EntityAssignedPacket(NetEntId Id);

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

[MemoryPackable]
//[MemoryPackUnion(0, typeof(PositionChunkData))]
[MemoryPackUnion(0, typeof(ChunkData<PositionChunkData>))]
[MemoryPackUnion(1, typeof(ChunkData<Vector3>))]
[MemoryPackUnion(2, typeof(ChunkData<Quaternion>))]
public partial interface IChunkData;

[MemoryPackable]
public partial record PositionChunkData(ushort Qx, ushort Qy, ushort Qz)
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
            (ushort)Math.Round((localX / info.ChunkSize.X) * ushort.MaxValue),
            (ushort)Math.Round((localY / info.ChunkSize.Y) * ushort.MaxValue),
            (ushort)Math.Round((localZ / info.ChunkSize.Z) * ushort.MaxValue)
        );
    }

    public Vector3 Restore(ChunkCoord packetCoord, SpacialGridInfo info)
    {
        var baseX = packetCoord.X * info.ChunkSize.X;
        var baseY = packetCoord.Y * info.ChunkSize.Y;
        var baseZ = packetCoord.Z * info.ChunkSize.Z;

        // Dequantize back to float in [0..size]
        var localX = (Qx / (f32)ushort.MaxValue) * info.ChunkSize.X;
        var localY = (Qy / (f32)ushort.MaxValue) * info.ChunkSize.Y;
        var localZ = (Qz / (f32)ushort.MaxValue) * info.ChunkSize.Z;
        return new Vector3(baseX + localX, baseY + localY, baseZ + localZ);
    }
}

[MemoryPackable]
public partial record ChunkData<T>(List<T> Data) : IChunkData;