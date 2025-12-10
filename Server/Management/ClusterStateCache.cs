using Common.SpacialPartitioning;

namespace Server.Management;

public class ClusterStateCache(ILogger<ClusterStateCache> logger)
{
    public ClusterMap? ClusterMap { get; set; } = new();
    public ServerMap? ServerMap { get; set; } = new();

    public void UpdateClusterMap(RegionAssignment[] assignments)
    {
        ClusterMap ??= new ClusterMap();
        foreach (var assignment in assignments)
        {
            ClusterMap.AssignRegion(assignment);
        }

        logger.LogInformation("Cluster map updated with {Count} assignments", assignments.Length);
    }

    public void UpdateServerMap(ServerInfo[] servers)
    {
        ServerMap ??= new ServerMap();
        foreach (var server in servers)
        {
            ServerMap.RegisterServer(server);
        }

        logger.LogInformation("Server map updated with {Count} servers", servers.Length);
    }
    
    public HashSet<ServerId> GetNeighboringServers(RegionAssignment query)
    {
        if (ClusterMap is null || query.ServerId == ServerId.Invalid) return[] ;
        return ClusterMap.GetNeighboringServersByRegion(query);
    }
}