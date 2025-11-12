using System.Collections.Concurrent;
using System.Net;
using MemoryPack;

namespace Common.SpacialPartitioning;

/// <summary>
/// Information about a game server in the cluster
/// </summary>
[MemoryPackable]
public partial record struct ServerInfo(
    ServerId ServerId,
    string Name,
    string Address,
    i32 ManagementPort,
    i32 GamePort,
    i64 ConnectedAt,
    bool IsReady);

/// <summary>
/// Maps server IDs to their connection information
/// Used by servers to connect to each other and by ClusterManager for routing
/// </summary>
public class ServerMap
{
    private readonly ConcurrentDictionary<ServerId, ServerInfo> _servers = new();
    private readonly Lock _lock = new();

    /// <summary>
    /// Register a server in the map
    /// </summary>
    public void RegisterServer(ServerInfo info)
    {
        _servers[info.ServerId] = info;
    }

    public bool UnregisterServer(ServerId serverId)
    {
        return _servers.TryRemove(serverId, out _);
    }
    
    public bool TryGetServer(ServerId serverId, out ServerInfo info)
    {
        return _servers.TryGetValue(serverId, out info);
    }

    /// <summary>
    /// Get all registered servers
    /// </summary>
    public IEnumerable<ServerInfo> GetAllServers()
    {
        return _servers.Values.ToList();
    }

    /// <summary>
    /// Get all ready servers
    /// </summary>
    public IEnumerable<ServerInfo> GetReadyServers()
    {
        return _servers.Values.Where(s => s.IsReady).ToList();
    }

    /// <summary>
    /// Export the current server map for synchronization
    /// </summary>
    public List<ServerInfo> ExportServers()
    {
        using var _ = _lock.EnterScope();
        return _servers.Values.ToList();
    }

    /// <summary>
    /// Get management endpoint for a server
    /// </summary>
    public IPEndPoint? GetManagementEndPoint(ServerId serverId)
    {
        if (TryGetServer(serverId, out var info))
        {
            if (IPAddress.TryParse(info.Address, out var addr))
                return new IPEndPoint(addr, info.ManagementPort);
        }

        return null;
    }
}