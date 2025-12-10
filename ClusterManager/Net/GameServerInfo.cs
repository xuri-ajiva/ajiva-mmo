using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;

namespace ClusterManager.Net;

public record GameServerInfo(QConnection Connection)
{
    public ServerId? ServerId { get; set; }
    public string? ServerName { get; set; }
    public bool IsReady { get; set; }
    public i32 ManagementPort { get; set; }
    public i32 GamePort { get; set; }
}