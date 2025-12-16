using Common;
using Common.SpacialPartitioning;
using Common.Networking;
using Common.Networking.AjivaQuic;

namespace ClusterManager;

/// <summary>
/// Manages the cluster map and assigns regions to game servers
/// Uses quadrant-based assignment for world regions
/// </summary>
public class ClusterMapService(ILogger<ClusterMapService> logger)
{
    private readonly ClusterMap _clusterMap = new();
    private readonly ServerMap _serverMap = new();
    private readonly Lock _lock = new();

    private u16 _nextServerId = 1; // ServerId 0 is reserved for Invalid
    private i32 _nextQuadrant = 0; // 0=+X+Z, 1=-X+Z, 2=-X-Z, 3=+X-Z, then repeat with different Y ranges

    public ClusterMap ClusterMap => _clusterMap;
    public ServerMap ServerMap => _serverMap;

    /// <summary>
    /// Register a new server and assign it a region
    /// </summary>
    public (ServerId serverId, RegionAssignment assignment) RegisterServerAndAssignRegion(string name,
        string managementAddress,
        i32 managementPort)
    {
        using var _ = _lock.EnterScope();

        // Allocate new server ID
        var serverId = new ServerId(_nextServerId++);

        // Register in server map
        var serverInfo = new ServerInfo(
            serverId,
            name,
            managementAddress,
            managementPort,
            0,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            false
        );

        _serverMap.RegisterServer(serverInfo);
        logger.LogInformation("Registered server {ServerId} ({Name}) at {Address}:{Port}",
            serverId, name, managementAddress, managementPort);

        // Assign a region (quadrant-based)
        var assignment = AllocateNextQuadrant(serverId);
        _clusterMap.AssignRegion(assignment);

        logger.LogInformation("Assigned region to server {ServerId}: {Area}",
            serverId, assignment.Area.ToReadableSpan());

        return (serverId, assignment);
    }

    /// <summary>
    /// Mark a server as ready to accept connections
    /// </summary>
    public void MarkServerReady(ServerId serverId, i32 messageGamePort)
    {
        if (_serverMap.TryGetServer(serverId, out var serverInfo))
        {
            serverInfo = serverInfo with
            {
                IsReady = true,
                GamePort = messageGamePort
            };
            _serverMap.RegisterServer(serverInfo);
            logger.LogInformation("Server {ServerId} is now ready", serverId);
        }
        else
        {
            logger.LogWarning("Attempted to mark unknown server {ServerId} as ready", serverId);
        }
    }

    /// <summary>
    /// Unregister a server and free its regions
    /// </summary>
    public void UnregisterServer(ServerId serverId)
    {
        using var _ = _lock.EnterScope();

        if (_clusterMap.GetServerRegion(serverId, out var region))
        {
            _clusterMap.UnassignServer(serverId);
            _serverMap.UnregisterServer(serverId);

            logger.LogWarning("Unregistered server {ServerId}, freed {Region}", serverId, region);
        }

        // TODO: Implement region reassignment to other servers
    }

    /// <summary>
    /// Get the full cluster state for a newly connected server
    /// </summary>
    public (RegionAssignment[] assignments, ServerInfo[] servers) GetClusterState()
    {
        using var _ = _lock.EnterScope();

        return (
            _clusterMap.ExportAssignments().ToArray(),
            _serverMap.ExportServers().ToArray()
        );
    }

    /*/// <summary>
    /// Find neighboring servers for a given server (based on region borders)
    /// Uses the optimized ClusterMap method that works on region boundaries
    /// </summary>
    public HashSet<ServerId> GetNeighboringServers(ServerId serverId)
    {
        using var _ = _lock.EnterScope();

        var neighbors = new HashSet<ServerId>();
        var regions = _clusterMap.GetServerRegions(serverId).ToList();

        foreach (var region in regions)
        {
            // Use the optimized region-based neighbor detection
            var regionNeighbors = _clusterMap.GetNeighboringServersByRegion(region);
            neighbors.UnionWith(regionNeighbors);
        }

        return neighbors;
    }*/

    /// <summary>
    /// Allocate regions in quadrants for world partitioning
    /// Quadrant 0: X:[0, +∞), Z:[0, +∞)
    /// Quadrant 1: X:(-∞, 0), Z:[0, +∞)
    /// Quadrant 2: X:(-∞, 0), Z:(-∞, 0)
    /// Quadrant 3: X:[0, +∞), Z:(-∞, 0)
    /// All quadrants span full Y range and world levels (Level 0)
    /// </summary>
    private RegionAssignment AllocateNextQuadrant(ServerId serverId)
    {
        const u32 length = 100000;
        const i32 minusLength = (i32)(-length);
        const u32 length2 = length * 2;
        
        /*return new RegionAssignment(
            serverId,
            new HyperAreaCoord(new ChunkCoord(minusLength, minusLength, minusLength, minusLength),
                new ChunkExtent(length2, length2, length2, length2)),
            Stopwatch.GetTimestamp()
        );*/

        var pos = (_nextQuadrant % 4) switch
        {
            0 => // +X, +Z quadrant
                new ChunkCoord(0, minusLength, 0, 0),
            1 => // -X, +Z quadrant
                new ChunkCoord(minusLength, minusLength, 0, 0),
            2 => // -X, -Z quadrant
                new ChunkCoord(minusLength, minusLength, minusLength, 0),
            3 => // +X, -Z quadrant
                new ChunkCoord(0, minusLength, minusLength, 0),
            _ => throw new InvalidOperationException("Invalid quadrant")
        };

        var size = new ChunkExtent(length, length2, length, 1);

        _nextQuadrant++;

        return new RegionAssignment(
            serverId,
            new HyperAreaCoord(pos, size), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        );
    }
}