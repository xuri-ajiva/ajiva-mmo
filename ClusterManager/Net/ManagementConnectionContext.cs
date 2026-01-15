using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;

namespace ClusterManager.Net;

public record ManagementConnectionContext(IQConnection Connection)
{
    public ServerId? ServerId { get; set; }
    public string? ServerName { get; set; }
    public bool IsReady { get; set; }
    public bool IsObserver { get; set; }
    public i32 ManagementPort { get; set; }
    public i32 GamePort { get; set; }
}