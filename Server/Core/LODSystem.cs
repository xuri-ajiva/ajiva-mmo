using System.Numerics;
using Common.Ecs;
using Common.SpacialPartitioning;
using MemoryPack;

namespace Server.Core;

/// <summary>
/// LOD (Level of Detail) proxy entity
/// Represents an aggregated view of many entities at a lower tree level
/// Used for efficient rendering of distant objects
/// </summary>
[MemoryPackable]
public partial struct LODProxy : IComponent
{
    public MortonKey SourceKey; // The tree node this proxy represents
    public i32 EntityCount; // Number of entities aggregated
    public Vector3 CenterOfMass; // Average position
    public f32 BoundingRadius; // Maximum distance from center
    public i64 LastUpdateTick; // When this LOD was last calculated
}

/// <summary>
/// LOD System - calculates and maintains Level-of-Detail proxies
/// for hierarchical rendering and network optimization
/// </summary>
public class LODSystem
{
    private readonly NServerEcs _world;
    private readonly ILogger<LODSystem> _logger;

    // LOD configuration
    private const i32 MinEntitiesForLOD = 10; // Don't create LOD for small clusters
    private const f32 LODUpdateInterval = 1.0f; // Update LOD proxies every second

    public LODSystem(ILogger<LODSystem> logger, NServerEcs world)
    {
        _logger = logger;
        _world = world;
    }

    /// <summary>
    /// Update LOD proxies for all aggregator nodes
    /// Called by aggregator threads
    /// </summary>
    public void UpdateLODProxies(MortonKey parentKey, IEnumerable<MortonKey> childKeys)
    {
        // Aggregate entity data from child nodes
        i32 totalEntities = 0;
        Vector3 centerOfMass = Vector3.Zero;
        f32 maxRadius = 0.0f;

        _world.DoLocked(() =>
        {
            foreach (var childKey in childKeys)
            {
                var childNodes = _world.Grid.GetNodesAtLevel(childKey.Level);

                foreach (var (key, node) in childNodes)
                {
                    if (!key.Equals(childKey)) continue;

                    var entities = node.GetEntities();
                    totalEntities += entities.Count;

                    // Calculate center of mass
                    foreach (var entityId in entities)
                    {
                        if (_world.Position.TryGet(entityId, out var pos))
                        {
                            centerOfMass += pos.Value;
                        }
                    }
                }
            }

            if (totalEntities < MinEntitiesForLOD)
                return;

            // Finalize center of mass
            centerOfMass /= totalEntities;

            // Calculate bounding radius
            foreach (var childKey in childKeys)
            {
                var childNodes = _world.Grid.GetNodesAtLevel(childKey.Level);

                foreach (var (key, node) in childNodes)
                {
                    if (!key.Equals(childKey)) continue;

                    var entities = node.GetEntities();
                    foreach (var entityId in entities)
                    {
                        if (_world.Position.TryGet(entityId, out var pos))
                        {
                            f32 distance = Vector3.Distance(centerOfMass, pos.Value);
                            maxRadius = Math.Max(maxRadius, distance);
                        }
                    }
                }
            }

            // Create or update LOD proxy entity
            // In a full implementation, we would store this as an actual entity
            // For now, we just log the aggregation
            _logger.LogDebug("LOD Proxy for {Key}: {Count} entities, Center: {Center}, Radius: {Radius}",
                parentKey, totalEntities, centerOfMass, maxRadius);
        });
    }

    /// <summary>
    /// Calculate which LOD level should be used for a given distance
    /// Closer objects use higher detail (higher tree level)
    /// Distant objects use lower detail (lower tree level)
    /// </summary>
    public i32 CalculateLODLevel(f32 distance, i32 maxLevel)
    {
        // Simple logarithmic LOD calculation
        // Distance 0-10: Level maxLevel (full detail)
        // Distance 10-100: Level maxLevel - 10
        // Distance 100-1000: Level maxLevel - 20
        // etc.

        if (distance < 10.0f)
            return maxLevel;

        f32 logDist = MathF.Log10(distance);
        i32 levelReduction = (i32)(logDist * 10);

        return Math.Max(0, maxLevel - levelReduction);
    }

    /// <summary>
    /// Check if a Morton key should be rendered based on LOD masking
    /// Higher detail nodes mask their parent LOD proxies
    /// </summary>
    public bool ShouldRenderNode(MortonKey key, HashSet<MortonKey> visibleHighDetailNodes)
    {
        // If any child node is visible at higher detail, suppress this LOD
        foreach (var highDetailKey in visibleHighDetailNodes)
        {
            if (highDetailKey.Level > key.Level &&
                highDetailKey.Code.HasPrefixAtLevel(key.Code, key.Level))
            {
                return false; // Masked by higher detail node
            }
        }

        return true;
    }
}
