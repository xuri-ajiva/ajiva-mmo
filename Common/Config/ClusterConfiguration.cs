using Common.SpacialPartitioning;

namespace Common.Config;

public class ClusterConfiguration
{
    public u16 EntryPort { get; set; }
    public u16 ManagementPort { get; set; }
    public ChunkCoord GridSize { get; set; } = new(16, 16, 16, 1);
    public TimeSpan TickInterval { get; set; } = TimeSpan.FromMilliseconds(50);

    // 4D Multi-Verse KD-Tree configuration
    public i32 SimulationLevel { get; set; } = 45; // Deep nodes: high-frequency physics (Level 40-50)
    public i32 AggregatorMinLevel { get; set; } = 0; // Shallow nodes: LOD aggregation (Level 0-30)
}