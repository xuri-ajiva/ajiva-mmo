using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Common.Ecs;
using Interception;
using MemoryPack;

namespace Common.SpacialPartitioning;

[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct SpacialGridInfo(ChunkCoord ChunkSize);

[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct ChunkCoord(i32 X, i32 Y, i32 Z, i32 Level)
{
    public i32 PosDistanceTo(ChunkCoord playerChunk) => Math.Abs(X - playerChunk.X) + Math.Abs(Y - playerChunk.Y) + Math.Abs(Z - playerChunk.Z);
}

public record SpatialChunk(SortedSet<EntityId> Entities)
{
    public bool IsDirty;

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

public class SpatialGrid(SpacialGridInfo spacialGridInfo)
{
    public SpacialGridInfo GridInfo { get; } = spacialGridInfo;
    private readonly ConcurrentDictionary<ChunkCoord, SpatialChunk> _chunks = new();
    private readonly ConcurrentDictionary<ChunkCoord, List<EntityId>> _deltaAdded = new();
    private readonly ConcurrentDictionary<ChunkCoord, List<EntityId>> _deltaRemoved = new();

    //   [ExcludeFromInterception]
    public ChunkCoord GetChunkCoord(Vector3 position, i32 level)
    {
        var x = (i32)Math.Floor(position.X / GridInfo.ChunkSize.X);
        var y = (i32)Math.Floor(position.Y / GridInfo.ChunkSize.Y);
        var z = (i32)Math.Floor(position.Z / GridInfo.ChunkSize.Z);
        return new ChunkCoord(x, y, z, level);
    }

    public SpatialChunk GetChunkData(ChunkCoord chunkCoord) =>
        _chunks.GetOrAdd(chunkCoord, _ => new SpatialChunk([]));

    public bool HasChunk(ChunkCoord chunkCoord) => _chunks.ContainsKey(chunkCoord);

    public bool TryGetChunk(ChunkCoord chunkCoord, [MaybeNullWhen(false)] out SpatialChunk chunk) =>
        _chunks.TryGetValue(chunkCoord, out chunk);

    public void MoveEntity(EntityId entityId, ChunkCoord oldCoord, ChunkCoord newCoord)
    {
        if (oldCoord.Equals(newCoord))
        {
            // Entity stayed in the same chunk - optionally mark dirty if needed
            if (_chunks.TryGetValue(newCoord, out var spatialChunk))
                spatialChunk.MarkDirty();
        }
        else
        {
            if (_chunks.TryGetValue(oldCoord, out var oldSpatialChunk))
            {
                oldSpatialChunk.Remove(entityId);
                oldSpatialChunk.MarkDirty();
                _deltaRemoved.GetOrAdd(oldCoord, _ => []).Add(entityId);
            }

            var newSpatialChunk = GetChunkData(newCoord);
            newSpatialChunk.MarkDirty();
            newSpatialChunk.Add(entityId);
            _deltaAdded.GetOrAdd(newCoord, []).Add(entityId);
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
        _deltaAdded.Clear();
        _deltaRemoved.Clear();
    }

    public bool TryGetDeltaInfo(ChunkCoord chunkCoord, out List<EntityId>? added, out List<EntityId>? removed)
    {
        var hasAdded = _deltaAdded.TryGetValue(chunkCoord, out added);
        var hasRemoved = _deltaRemoved.TryGetValue(chunkCoord, out removed);
        return hasAdded || hasRemoved;
    }
}