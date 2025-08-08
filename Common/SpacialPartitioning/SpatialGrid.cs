using System.Numerics;
using Riptide;

namespace Common.SpacialPartitioning;

public struct ChunkCoord(i32 x, i32 y, i32 z, i32 level) : IMessageSerializable,  IEquatable<ChunkCoord>
{
    public i32 X = x;
    public i32 Y = y;
    public i32 Z = z;
    public i32 Level = level;

    // Override Equals and GetHashCode for dictionary keys
    public override bool Equals(object? obj) => obj is ChunkCoord other && Equals(other);

    public bool Equals(ChunkCoord other) => X == other.X && Y == other.Y && Z == other.Z && Level == other.Level;

    public static bool operator ==(ChunkCoord left, ChunkCoord right) => left.Equals(right);

    public static bool operator !=(ChunkCoord left, ChunkCoord right) => !(left == right);

    public override i32 GetHashCode()
    {
        // Combine the hash codes of all four fields efficiently
        unchecked
        {
            i32 hash = 17;
            hash = hash * 31 + X;
            hash = hash * 31 + Y;
            hash = hash * 31 + Z;
            hash = hash * 31 + Level;
            return hash;
        }
    }

    public void Serialize(Message message)
    {
        message.AddUShort((ushort)X);
        message.AddUShort((ushort)Y);
        message.AddUShort((ushort)Z);
        message.AddUShort((ushort)Level);
    }

    public void Deserialize(Message message)
    {
        X = message.GetUShort();
        Y = message.GetUShort();
        Z = message.GetUShort();
        Level = message.GetUShort();
    }
}

public class SpatialChunk
{
    public HashSet<i32> Entities = new();
    public bool IsDirty = false;

    public void Add(i32 entityId)
    {
        Entities.Add(entityId);
        MarkDirty();
    }

    public void Remove(i32 entityId)
    {
        Entities.Remove(entityId);
        MarkDirty();
    }

    public void MarkDirty() => IsDirty = true;

    public void ClearDirty() => IsDirty = false;
}

public class SpatialGrid(u32 chunkSize)
{
    private readonly Dictionary<ChunkCoord, SpatialChunk> chunks = new();
    private readonly u32 _chunkSize = chunkSize;

    public ChunkCoord GetChunkCoord(Vector3 position, i32 level)
    {
        i32 x = (i32)Math.Floor(position.X / _chunkSize);
        i32 y = (i32)Math.Floor(position.Y / _chunkSize);
        i32 z = (i32)Math.Floor(position.Z / _chunkSize);
        return new ChunkCoord(x, y, z, level);
    }

    public void MoveEntity(i32 entityId, Vector3 oldPos, i32 oldLevel, Vector3 newPos, i32 newLevel)
    {
        var oldChunk = GetChunkCoord(oldPos, oldLevel);
        var newChunk = GetChunkCoord(newPos, newLevel);

        if (!oldChunk.Equals(newChunk))
        {
            if (chunks.TryGetValue(oldChunk, out var oldSpatialChunk))
                oldSpatialChunk.Remove(entityId);

            if (!chunks.TryGetValue(newChunk, out var newSpatialChunk))
            {
                newSpatialChunk = new SpatialChunk();
                chunks[newChunk] = newSpatialChunk;
            }

            newSpatialChunk.Add(entityId);
        }
        else
        {
            // Entity stayed in the same chunk - optionally mark dirty if needed
            if (chunks.TryGetValue(newChunk, out var spatialChunk))
                spatialChunk.MarkDirty();
        }
    }

    public IEnumerable<(ChunkCoord chunkCoord, SpatialChunk chunk)> GetDirtyChunks()
    {
        return from kvp in chunks where kvp.Value.IsDirty select (kvp.Key, kvp.Value);
    }

    public void ClearDirtyFlags()
    {
        foreach (var chunk in chunks.Values)
            chunk.ClearDirty();
    }
}
