using System.Collections.Concurrent;
using MemoryPack;

namespace Common.SpacialPartitioning;

/// <summary>
/// Identifies a server in the cluster
/// </summary>
[MemoryPackable]
public readonly partial record struct ServerId(u16 Value)
{
    public static readonly ServerId Invalid = new(0);
    public override string ToString() => $"Server({Value})";
    
    public static implicit operator u16(ServerId serverId) => serverId.Value;
    public static implicit operator ServerId(u16 value) => new(value);
}

/// <summary>
/// Represents a region assignment in the cluster - which server owns which chunks
/// A region is defined by min/max bounds in 4D space (x, y, z, level)
/// For level-specific regions: MinChunk.Level == MaxChunk.Level (e.g., dungeon instances)
/// For world regions: MinChunk.Level can be different or use special sentinel values
/// </summary>
[MemoryPackable]
public readonly partial record struct RegionAssignment(
    ServerId ServerId,
    ChunkCoord MinChunk,
    ChunkCoord MaxChunk,
    i64 AssignedAt
);

/// <summary>
/// Maps chunk coordinates to the responsible server
/// Used by both ClusterManager (authoritative) and servers/clients (cached)
/// </summary>
public class ClusterMap
{
    private readonly ConcurrentDictionary<ServerId, List<RegionAssignment>> _serverRegions = new();
    private readonly Lock _lock = new();

    /// <summary>
    /// Assign a region to a server (stores the region bounds, not individual chunks)
    /// </summary>
    public void AssignRegion(RegionAssignment assignment)
    {
        using var _ = _lock.EnterScope();
        
        var regions = _serverRegions.GetOrAdd(assignment.ServerId, _ => new List<RegionAssignment>());
        regions.Add(assignment);
    }

    /// <summary>
    /// Get the server responsible for a chunk by checking region bounds
    /// </summary>
    public bool TryGetChunkOwner(ChunkCoord chunk, out ServerId serverId)
    {
        using var _ = _lock.EnterScope();
        
        foreach (var (sid, regions) in _serverRegions)
        {
            foreach (var region in regions)
            {
                if (IsChunkInRegion(chunk, region))
                {
                    serverId = sid;
                    return true;
                }
            }
        }
        
        serverId = ServerId.Invalid;
        return false;
    }

    /// <summary>
    /// Check if a chunk is within a region's bounds (4D check including level)
    /// </summary>
    private bool IsChunkInRegion(ChunkCoord chunk, RegionAssignment region)
    {
        // Level check - handle special cases
        // If region spans all levels (MinChunk.Level == i32.MinValue, MaxChunk.Level == i32.MaxValue)
        // Or if chunk level is within region level bounds
        var levelMatches = (region.MinChunk.Level == i32.MinValue && region.MaxChunk.Level == i32.MaxValue) ||
                          (chunk.Level >= region.MinChunk.Level && chunk.Level <= region.MaxChunk.Level);
        
        if (!levelMatches)
            return false;

        // 3D spatial check
        return chunk.X >= region.MinChunk.X && chunk.X <= region.MaxChunk.X &&
               chunk.Y >= region.MinChunk.Y && chunk.Y <= region.MaxChunk.Y &&
               chunk.Z >= region.MinChunk.Z && chunk.Z <= region.MaxChunk.Z;
    }

    /// <summary>
    /// Get all regions assigned to a server
    /// </summary>
    public IEnumerable<RegionAssignment> GetServerRegions(ServerId serverId)
    {
        return _serverRegions.TryGetValue(serverId, out var regions) 
            ? regions.ToList() 
            : Enumerable.Empty<RegionAssignment>();
    }

    /// <summary>
    /// Remove all assignments for a server (when it disconnects)
    /// </summary>
    public void UnassignServer(ServerId serverId)
    {
        using var _ = _lock.EnterScope();
        _serverRegions.TryRemove(serverId, out var _);
    }

    /// <summary>
    /// Get all neighboring chunks for a given chunk (26 neighbors in 3D)
    /// </summary>
    public IEnumerable<ChunkCoord> GetNeighboringChunks(ChunkCoord chunk, i32 range = 1)
    {
        for (var dx = -range; dx <= range; dx++)
        {
            for (var dy = -range; dy <= range; dy++)
            {
                for (var dz = -range; dz <= range; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0)
                        continue;

                    yield return new ChunkCoord(
                        chunk.X + dx,
                        chunk.Y + dy,
                        chunk.Z + dz,
                        chunk.Level
                    );
                }
            }
        }
    }

    /// <summary>
    /// Get all servers that own neighboring chunks (for proxy entity streaming)
    /// </summary>
    public HashSet<ServerId> GetNeighboringServers(ChunkCoord chunk, i32 range = 1)
    {
        var servers = new HashSet<ServerId>();
        
        foreach (var neighbor in GetNeighboringChunks(chunk, range))
        {
            if (TryGetChunkOwner(neighbor, out var serverId))
                servers.Add(serverId);
        }
        
        return servers;
    }

    /// <summary>
    /// Get all servers that have regions adjacent to or overlapping with the given region.
    /// This is optimized to work on region boundaries rather than iterating through every chunk.
    /// </summary>
    public HashSet<ServerId> GetNeighboringServersByRegion(RegionAssignment myRegion)
    {
        var neighbors = new HashSet<ServerId>();

        using var _ = _lock.EnterScope();
        
        foreach (var (serverId, regions) in _serverRegions)
        {
            if (serverId == myRegion.ServerId)
                continue;

            // Check if any of this server's regions are adjacent to our region
            foreach (var region in regions)
            {
                if (AreRegionsAdjacent(myRegion, region))
                {
                    neighbors.Add(serverId);
                    break; // No need to check other regions for this server
                }
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Check if two regions are adjacent (within 1 chunk distance) or overlapping
    /// </summary>
    private bool AreRegionsAdjacent(RegionAssignment region1, RegionAssignment region2)
    {
        // Check if levels overlap or are adjacent
        var levelOverlaps = !(region1.MaxChunk.Level < region2.MinChunk.Level - 1 || 
                             region1.MinChunk.Level > region2.MaxChunk.Level + 1);
        if (!levelOverlaps)
            return false;

        // Check if regions are within 1 chunk distance in 3D space
        // Regions are adjacent if they overlap or their boundaries are within 1 chunk
        // Special handling for int.MaxValue/int.MinValue to avoid overflow
        var xAdjacent = AreRangesAdjacent(region1.MinChunk.X, region1.MaxChunk.X, 
                                          region2.MinChunk.X, region2.MaxChunk.X);
        var yAdjacent = AreRangesAdjacent(region1.MinChunk.Y, region1.MaxChunk.Y, 
                                          region2.MinChunk.Y, region2.MaxChunk.Y);
        var zAdjacent = AreRangesAdjacent(region1.MinChunk.Z, region1.MaxChunk.Z, 
                                          region2.MinChunk.Z, region2.MaxChunk.Z);

        return xAdjacent && yAdjacent && zAdjacent;
    }

    /// <summary>
    /// Check if two 1D ranges are within 1 unit of each other (adjacent or overlapping)
    /// Handles int.MaxValue/int.MinValue without overflow
    /// </summary>
    private bool AreRangesAdjacent(i32 min1, i32 max1, i32 min2, i32 max2)
    {
        // Check if ranges are separated by more than 1 unit
        // Range1 is too far to the left: max1 < min2 - 1
        // But we need to avoid overflow when min2 == int.MinValue
        if (min2 > i32.MinValue && max1 < min2 - 1)
            return false;
        
        // Range1 is too far to the right: min1 > max2 + 1
        // But we need to avoid overflow when max2 == int.MaxValue
        if (max2 < i32.MaxValue && min1 > max2 + 1)
            return false;

        return true;
    }

    /// <summary>
    /// Export the current cluster map state for synchronization
    /// </summary>
    public List<RegionAssignment> ExportAssignments()
    {
        using var _ = _lock.EnterScope();
        return _serverRegions.Values.SelectMany(x => x).ToList();
    }
}
