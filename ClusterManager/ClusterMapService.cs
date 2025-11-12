using Common;
using Common.SpacialPartitioning;
using Common.Networking;
using Common.Networking.AjivaQuic;

namespace ClusterManager;

/// <summary>
/// Manages the cluster map and assigns regions to game servers
/// Uses quadrant-based assignment for world regions
/// </summary>
public class ClusterMapService
{
    private readonly ILogger<ClusterMapService> _logger;
    private readonly ClusterMap _clusterMap = new();
    private readonly ServerMap _serverMap = new();
    private readonly Lock _lock = new();

    private u16 _nextServerId = 1; // ServerId 0 is reserved for Invalid
    private i32 _nextQuadrant = 0; // 0=+X+Z, 1=-X+Z, 2=-X-Z, 3=+X-Z, then repeat with different Y ranges

    public ClusterMapService(ILogger<ClusterMapService> logger)
    {
        _logger = logger;
    }

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
        _logger.LogInformation("Registered server {ServerId} ({Name}) at {Address}:{Port}",
            serverId, name, managementAddress, managementPort);

        // Assign a region (quadrant-based)
        var assignment = AllocateNextQuadrant(serverId);
        _clusterMap.AssignRegion(assignment);

        _logger.LogInformation("Assigned region to server {ServerId}: {MinChunk} to {MaxChunk}",
            serverId, assignment.MinChunk, assignment.MaxChunk);

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
            _logger.LogInformation("Server {ServerId} is now ready", serverId);
        }
        else
        {
            _logger.LogWarning("Attempted to mark unknown server {ServerId} as ready", serverId);
        }
    }

    /// <summary>
    /// Unregister a server and free its regions
    /// </summary>
    public void UnregisterServer(ServerId serverId)
    {
        using var _ = _lock.EnterScope();

        var regions = _clusterMap.GetServerRegions(serverId).ToList();
        _clusterMap.UnassignServer(serverId);
        _serverMap.UnregisterServer(serverId);

        _logger.LogWarning("Unregistered server {ServerId}, freed {Count} regions",
            serverId, regions.Count);

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

    /// <summary>
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
    }

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
        const i32 min = i32.MinValue, max = i32.MaxValue;
        ChunkCoord minChunk, maxChunk;

        switch (_nextQuadrant % 4)
        {
            case 0: // +X, +Z quadrant
                minChunk = new ChunkCoord(0, min, 0, 0);
                maxChunk = new ChunkCoord(max, max, max, 0);
                break;
            case 1: // -X, +Z quadrant
                minChunk = new ChunkCoord(min, min, 0, 0);
                maxChunk = new ChunkCoord(-1, max, max, 0);
                break;
            case 2: // -X, -Z quadrant
                minChunk = new ChunkCoord(min, min, min, 0);
                maxChunk = new ChunkCoord(-1, max, -1, 0);
                break;
            case 3: // +X, -Z quadrant
                minChunk = new ChunkCoord(0, min, min, 0);
                maxChunk = new ChunkCoord(max, max, -1, 0);
                break;
            default:
                throw new InvalidOperationException("Invalid quadrant");
        }

        _nextQuadrant++;

        return new RegionAssignment(
            serverId,
            minChunk,
            maxChunk,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        );
    }
}