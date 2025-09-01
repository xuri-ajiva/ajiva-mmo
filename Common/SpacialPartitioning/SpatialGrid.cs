using System.Collections.Concurrent;
using System.Numerics;
using Common.Ecs;
using Interception;
using MemoryPack;

namespace Common.SpacialPartitioning;

[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct ChunkCoord(i32 X, i32 Y, i32 Z, i32 Level);

public class SpatialChunk
{
    public readonly HashSet<EntityId> Entities = new();
    public bool IsDirty = false;

    public void Add(EntityId entityId)
    {
        Entities.Add(entityId);
        MarkDirty();
    }

    public void Remove(EntityId entityId)
    {
        Entities.Remove(entityId);
        MarkDirty();
    }

    public void MarkDirty() => IsDirty = true;

    public void ClearDirty() => IsDirty = false;
}

public class SpatialGrid(u32 chunkSize)
{
    private readonly ConcurrentDictionary<ChunkCoord, SpatialChunk> _chunks = new();
    private readonly u32 _chunkSize = chunkSize;

//   [ExcludeFromInterception]
    public ChunkCoord GetChunkCoord(Vector3 position, i32 level)
    {
        var x = (i32)Math.Floor(position.X / _chunkSize);
        var y = (i32)Math.Floor(position.Y / _chunkSize);
        var z = (i32)Math.Floor(position.Z / _chunkSize);
        return new ChunkCoord(x, y, z, level);
    }

    public SpatialChunk GetChunkData(ChunkCoord chunkCoord) =>
        _chunks.TryGetValue(chunkCoord, out var chunk) ? chunk : new SpatialChunk();

    public void MoveEntity(EntityId entityId, Vector3 oldPos, i32 oldLevel, Vector3 newPos, i32 newLevel)
    {
        var oldChunk = GetChunkCoord(oldPos, oldLevel);
        var newChunk = GetChunkCoord(newPos, newLevel);

        if (!oldChunk.Equals(newChunk))
        {
            if (_chunks.TryGetValue(oldChunk, out var oldSpatialChunk))
                oldSpatialChunk.Remove(entityId);

            if (!_chunks.TryGetValue(newChunk, out var newSpatialChunk))
            {
                newSpatialChunk = new SpatialChunk();
                _chunks[newChunk] = newSpatialChunk;
            }

            newSpatialChunk.Add(entityId);
        }
        else
        {
            // Entity stayed in the same chunk - optionally mark dirty if needed
            if (_chunks.TryGetValue(newChunk, out var spatialChunk))
                spatialChunk.MarkDirty();
        }
    }

    public IEnumerable<(ChunkCoord chunkCoord, SpatialChunk chunk)> GetDirtyChunks()
    {
        return from kvp in _chunks where kvp.Value.IsDirty select (kvp.Key, kvp.Value);
    }

    public void ClearDirtyFlags()
    {
        foreach (var chunk in _chunks.Values)
            chunk.ClearDirty();
    }
}