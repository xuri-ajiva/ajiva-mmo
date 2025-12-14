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
    HyperAreaCoord Area,
    i64 AssignedAt
)
{
    public string ReadableSpan() => $"Region[{Area.ToReadableSpan()}]";
}

/// <summary>
/// Maps chunk coordinates to the responsible server
/// Used by both ClusterManager (authoritative) and servers/clients (cached)
/// </summary>
public class ClusterMap
{
    private readonly ConcurrentDictionary<ServerId, RegionAssignment> _serverRegions = new();
    private readonly Lock _lock = new();

    /// <summary>
    /// Assign a region to a server (stores the region bounds, not individual chunks)
    /// </summary>
    public void AssignRegion(RegionAssignment assignment)
    {
        using var _ = _lock.EnterScope();

        _serverRegions.AddOrUpdate(assignment.ServerId,
            assignment, (_, _) => assignment);
    }

    /// <summary>
    /// Get the server responsible for a chunk by checking region bounds
    /// </summary>
    public bool TryGetChunkOwner(ChunkCoord chunk, out ServerId serverId)
    {
        using var _ = _lock.EnterScope();

        foreach (var (sid, region) in _serverRegions)
        {
            if (!region.Area.Contains(chunk)) continue;

            serverId = sid;
            return true;
        }

        serverId = ServerId.Invalid;
        return false;
    }

    /// <summary>
    /// Get all regions assigned to a server
    /// </summary>
    public bool GetServerRegion(ServerId serverId, out RegionAssignment regions)
    {
        using var _ = _lock.EnterScope();
        return _serverRegions.TryGetValue(serverId, out regions);
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
        using var _ = _lock.EnterScope();
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

        foreach (var (serverId, region) in _serverRegions)
        {
            if (serverId == myRegion.ServerId)
                continue;

            if (!myRegion.Area.IsAdjacentTo(region.Area)) continue;
            neighbors.Add(serverId);
            break; // No need to check other regions for this server
        }

        return neighbors;
    }

    /// <summary>
    /// Export the current cluster map state for synchronization
    /// </summary>
    public List<RegionAssignment> ExportAssignments()
    {
        using var _ = _lock.EnterScope();
        return _serverRegions.Values.ToList();
    }
}