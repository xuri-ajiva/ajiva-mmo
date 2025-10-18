using System.Net;
using Common.Networking.AjivaQuic;

namespace ClusterManager;

public record GameServerInfo(QConnection Connection)
{
    public string? ServerName { get; set; }
    public bool IsReady { get; set; }
    public i32 ManagementPort { get; set; }
    public i32 GamePort { get; set; }
}