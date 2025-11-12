using Common.SpacialPartitioning;

namespace Common.Config;

public class ClusterConfiguration
{
    public u16 EntryPort { get; set; }
    public u16 ManagementPort { get; set; }
    public ChunkCoord GridSize { get; set; } = new(16, 16, 16, 1);
}