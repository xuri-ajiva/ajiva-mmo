using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Common.Ecs;
using Interception;
using MemoryPack;

namespace Common.SpacialPartitioning;

[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct SpacialGridInfo(ChunkCoord ChunkSize, i32 SimulationLevel, i32 AggregatorMinLevel);

/// <summary>
/// Spatial grid using Morton-encoded 4D Hashed KD-Tree
/// Manages both simulation nodes (deep levels) and aggregator nodes (shallow levels)
/// </summary>
public class SpacialGrid
{
    public SpacialGridInfo GridInfo { get; }

    // Legacy chunk-based storage (for backward compatibility during migration)
    private readonly ConcurrentDictionary<ChunkCoord, SpatialChunk> _chunks = new();
    private readonly ConcurrentDictionary<ChunkCoord, List<EntityId>> _deltaAdded = new();
    private readonly ConcurrentDictionary<ChunkCoord, List<EntityId>> _deltaRemoved = new();

    // New Morton-based KD-Tree
    private readonly HashedKDTree _kdTree;

    public SpacialGrid(SpacialGridInfo spacialGridInfo)
    {
        GridInfo = spacialGridInfo;
        _kdTree = new HashedKDTree(spacialGridInfo.SimulationLevel, spacialGridInfo.AggregatorMinLevel);
    }

    [ExcludeFromInterception]
    public ChunkCoord GetChunkCoord(Vector3 position, i32 w)
    {
        var x = (i32)Math.Floor(position.X / GridInfo.ChunkSize.X);
        var y = (i32)Math.Floor(position.Y / GridInfo.ChunkSize.Y);
        var z = (i32)Math.Floor(position.Z / GridInfo.ChunkSize.Z);
        return new ChunkCoord(x, y, z, w);
    }

    /// <summary>
    /// Calculate Morton key for a given 4D position and tree level
    /// </summary>
    public MortonKey GetMortonKey(Vector3 position, i32 w, i32 level)
    {
        var chunkCoord = GetChunkCoord(position, w);
        return chunkCoord.ToMortonKey(level, GridInfo.ChunkSize.X);
    }

    // Legacy chunk-based API (maintained for backward compatibility)
    public ConcurrentDictionary<ChunkCoord, SpatialChunk> Chunks => _chunks;

    public SpatialChunk GetChunkData(ChunkCoord chunkCoord) =>
        _chunks.GetOrAdd(chunkCoord, _ => new SpatialChunk([]));

    public bool HasChunk(ChunkCoord chunkCoord) => _chunks.ContainsKey(chunkCoord);

    public bool TryDeleteChunk(ChunkCoord chunkCoord, [MaybeNullWhen(false)] out SpatialChunk value) =>
        _chunks.TryRemove(chunkCoord, out value);

    public bool TryGetChunk(ChunkCoord chunkCoord, [MaybeNullWhen(false)] out SpatialChunk chunk) =>
        _chunks.TryGetValue(chunkCoord, out chunk);

    public void MoveEntity(EntityId entityId, ChunkCoord oldCoord, ChunkCoord newCoord)
    {
        // Legacy chunk-based movement
        if (oldCoord.Equals(newCoord))
        {
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

    // Morton-based API (new architecture)

    /// <summary>
    /// Add entity to KD-Tree at given Morton key
    /// </summary>
    public void AddEntityToTree(MortonKey key, EntityId entityId)
    {
        _kdTree.AddEntity(key, entityId);
    }

    /// <summary>
    /// Remove entity from KD-Tree
    /// </summary>
    public bool RemoveEntityFromTree(MortonKey key, EntityId entityId)
    {
        return _kdTree.RemoveEntity(key, entityId);
    }

    /// <summary>
    /// Move entity between Morton keys
    /// </summary>
    public void MoveEntityInTree(MortonKey oldKey, MortonKey newKey, EntityId entityId)
    {
        _kdTree.MoveEntity(oldKey, newKey, entityId);
    }

    /// <summary>
    /// Query entities within a frustum (by Morton key range)
    /// </summary>
    public IEnumerable<KDTreeNode> QueryFrustum(MortonKey centerKey, i32 radius)
    {
        return _kdTree.QueryFrustum(centerKey, radius);
    }

    /// <summary>
    /// Get all nodes at a specific tree level (for LOD queries)
    /// </summary>
    public IEnumerable<(MortonKey key, KDTreeNode node)> GetNodesAtLevel(i32 level)
    {
        return _kdTree.GetNodesAtLevel(level);
    }

    /// <summary>
    /// Get all dirty nodes in the KD-Tree
    /// </summary>
    public IEnumerable<(MortonKey key, KDTreeNode node)> GetDirtyTreeNodes()
    {
        return _kdTree.GetDirtyNodes();
    }

    // Legacy API continued
    public IEnumerable<(ChunkCoord chunkCoord, SpatialChunk chunk)> GetDirtyChunks()
    {
        return _chunks.Where(kvp => kvp.Value.IsDirty).Select(kvp => (kvp.Key, kvp.Value));
    }

    public void ClearDirtyFlags()
    {
        foreach (var chunk in _chunks.Values)
            chunk.ClearDirty();
        _deltaAdded.Clear();
        _deltaRemoved.Clear();

        // Also clear KD-Tree dirty flags
        _kdTree.ClearDirtyFlags();
    }

    public bool TryGetDeltaInfo(ChunkCoord chunkCoord, out List<EntityId>? added, out List<EntityId>? removed)
    {
        var hasAdded = _deltaAdded.TryGetValue(chunkCoord, out added);
        var hasRemoved = _deltaRemoved.TryGetValue(chunkCoord, out removed);
        return hasAdded || hasRemoved;
    }

    /// <summary>
    /// Get delta info for Morton-based tree
    /// </summary>
    public bool TryGetTreeDeltaInfo(MortonKey key, out List<EntityId>? added, out List<EntityId>? removed)
    {
        return _kdTree.TryGetDeltaInfo(key, out added, out removed);
    }

    public int GetTreeNodeCount() => _kdTree.GetNodeCount();
}
