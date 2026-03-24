using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Common.Ecs;
using Interception;

namespace Common.SpacialPartitioning;

/// <summary>
/// Node in the 4D Hashed KD-Tree
/// Each node represents a region of 4D space at a specific tree level
/// Deep nodes (high level) contain actual entities and run simulation
/// Shallow nodes (low level) contain LOD proxies aggregated from children
/// </summary>
[ExcludeFromInterception]
public class KDTreeNode
{
    public MortonKey Key { get; }
    public List<EntityId> Entities { get; } = [];
    public bool IsDirty { get; private set; }
    public i64 LastUpdateTick { get; private set; }

    // LOD metadata for aggregator nodes
    public bool IsAggregator { get; }
    public i32 EntityCount { get; private set; }

    private readonly Lock _lock = new();

    public KDTreeNode(MortonKey key, bool isAggregator = false)
    {
        Key = key;
        IsAggregator = isAggregator;
    }

    public void Add(EntityId entityId)
    {
        using var _ = _lock.EnterScope();
        Entities.Add(entityId);
        EntityCount = Entities.Count;
        MarkDirty();
    }

    public bool Remove(EntityId entityId)
    {
        using var _ = _lock.EnterScope();
        bool removed = Entities.Remove(entityId);
        if (removed)
        {
            EntityCount = Entities.Count;
            MarkDirty();
        }
        return removed;
    }

    public void MarkDirty()
    {
        IsDirty = true;
        LastUpdateTick = DateTime.UtcNow.Ticks;
    }

    public void ClearDirty()
    {
        IsDirty = false;
    }

    public List<EntityId> GetEntities()
    {
        using var _ = _lock.EnterScope();
        return new List<EntityId>(Entities);
    }
}

/// <summary>
/// 4D Hashed KD-Tree for spatial partitioning
/// Maps MortonKey -> KDTreeNode using a hash map
/// Supports hierarchical LOD with aggregator and simulation nodes
/// </summary>
[ExcludeFromInterception]
public class HashedKDTree
{
    private readonly ConcurrentDictionary<MortonKey, KDTreeNode> _nodes = new();
    private readonly i32 _simulationLevel; // Deep level for physics (e.g., 40-50)
    private readonly i32 _aggregatorMinLevel; // Shallow level for LOD (e.g., 0-30)

    // Delta tracking for network optimization
    private readonly ConcurrentDictionary<MortonKey, List<EntityId>> _deltaAdded = new();
    private readonly ConcurrentDictionary<MortonKey, List<EntityId>> _deltaRemoved = new();

    public HashedKDTree(i32 simulationLevel = 45, i32 aggregatorMinLevel = 0)
    {
        _simulationLevel = simulationLevel;
        _aggregatorMinLevel = aggregatorMinLevel;
    }

    /// <summary>
    /// Get or create a node at the given Morton key
    /// </summary>
    public KDTreeNode GetOrCreateNode(MortonKey key)
    {
        bool isAggregator = key.Level < _simulationLevel;
        return _nodes.GetOrAdd(key, k => new KDTreeNode(k, isAggregator));
    }

    /// <summary>
    /// Try to get an existing node
    /// </summary>
    public bool TryGetNode(MortonKey key, [MaybeNullWhen(false)] out KDTreeNode node)
    {
        return _nodes.TryGetValue(key, out node);
    }

    /// <summary>
    /// Add an entity to the tree at the specified Morton key
    /// </summary>
    public void AddEntity(MortonKey key, EntityId entityId)
    {
        var node = GetOrCreateNode(key);
        node.Add(entityId);

        // Track delta for network sync
        _deltaAdded.GetOrAdd(key, _ => []).Add(entityId);
    }

    /// <summary>
    /// Remove an entity from the tree
    /// </summary>
    public bool RemoveEntity(MortonKey key, EntityId entityId)
    {
        if (!TryGetNode(key, out var node))
            return false;

        bool removed = node.Remove(entityId);
        if (removed)
        {
            // Track delta for network sync
            _deltaRemoved.GetOrAdd(key, _ => []).Add(entityId);

            // Clean up empty nodes
            if (node.EntityCount == 0)
            {
                _nodes.TryRemove(key, out _);
            }
        }

        return removed;
    }

    /// <summary>
    /// Move an entity from one Morton key to another
    /// </summary>
    public void MoveEntity(MortonKey oldKey, MortonKey newKey, EntityId entityId)
    {
        if (oldKey.Equals(newKey))
        {
            // Entity stayed in same region, just mark dirty
            if (TryGetNode(newKey, out var node))
                node.MarkDirty();
            return;
        }

        RemoveEntity(oldKey, entityId);
        AddEntity(newKey, entityId);
    }

    /// <summary>
    /// Get all dirty nodes (changed since last clear)
    /// </summary>
    public IEnumerable<(MortonKey key, KDTreeNode node)> GetDirtyNodes()
    {
        return _nodes.Where(kvp => kvp.Value.IsDirty).Select(kvp => (kvp.Key, kvp.Value));
    }

    /// <summary>
    /// Clear dirty flags and delta tracking
    /// </summary>
    public void ClearDirtyFlags()
    {
        foreach (var node in _nodes.Values)
            node.ClearDirty();

        _deltaAdded.Clear();
        _deltaRemoved.Clear();
    }

    /// <summary>
    /// Get delta information for a specific key
    /// </summary>
    public bool TryGetDeltaInfo(MortonKey key, out List<EntityId>? added, out List<EntityId>? removed)
    {
        var hasAdded = _deltaAdded.TryGetValue(key, out added);
        var hasRemoved = _deltaRemoved.TryGetValue(key, out removed);
        return hasAdded || hasRemoved;
    }

    /// <summary>
    /// Query nodes within a 4D frustum (defined by Morton key range)
    /// Used for client view frustum queries
    /// </summary>
    public IEnumerable<KDTreeNode> QueryFrustum(MortonKey centerKey, i32 radius)
    {
        // For simplicity, return all nodes at the same level within Morton code proximity
        // A more sophisticated implementation would use hierarchical culling

        var targetLevel = centerKey.Level;
        foreach (var (key, node) in _nodes)
        {
            if (key.Level != targetLevel)
                continue;

            // Simple proximity check - could be optimized with Morton range queries
            yield return node;
        }
    }

    /// <summary>
    /// Get all nodes at a specific level (for aggregation queries)
    /// </summary>
    public IEnumerable<(MortonKey key, KDTreeNode node)> GetNodesAtLevel(i32 level)
    {
        return _nodes.Where(kvp => kvp.Key.Level == level).Select(kvp => (kvp.Key, kvp.Value));
    }

    /// <summary>
    /// Get parent node key for hierarchical aggregation
    /// </summary>
    public MortonKey GetParentKey(MortonKey childKey)
    {
        return childKey.GetParent();
    }

    /// <summary>
    /// Get all child node keys for hierarchical subdivision
    /// </summary>
    public IEnumerable<MortonKey> GetChildKeys(MortonKey parentKey)
    {
        return parentKey.GetChildren();
    }

    public int GetNodeCount() => _nodes.Count;
}
