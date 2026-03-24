using System.Collections.Concurrent;
using Common;
using Common.Ecs;
using Common.SpacialPartitioning;
using Common.Systems;

namespace Server.Core;

/// <summary>
/// Thread pool configuration for the multi-threaded server
/// </summary>
public class ThreadPoolConfig
{
    public i32 SimulationThreadCount { get; set; } = 4; // High-frequency physics threads
    public i32 AggregatorThreadCount { get; set; } = 2; // LOD aggregation threads
    public TimeSpan SimulationTickInterval { get; set; } = TimeSpan.FromMilliseconds(16.67); // ~60Hz
    public TimeSpan AggregatorTickInterval { get; set; } = TimeSpan.FromMilliseconds(100); // ~10Hz
}

/// <summary>
/// Multi-threaded server architecture with separate simulation and aggregation threads
/// Simulation threads: Run high-frequency physics on deep tree nodes (Level 40-50)
/// Aggregator threads: Calculate LOD proxies on shallow nodes (Level 0-30)
/// </summary>
public class MultiThreadedSimulationManager : IDisposable
{
    private readonly ILogger<MultiThreadedSimulationManager> _logger;
    private readonly NServerEcs _world;
    private readonly ThreadPoolConfig _config;
    private readonly Instrumentation _instrumentation;

    // Thread management
    private readonly List<Thread> _simulationThreads = [];
    private readonly List<Thread> _aggregatorThreads = [];
    private readonly CancellationTokenSource _cts = new();

    // Work queues
    private readonly BlockingCollection<SimulationWorkItem> _simulationQueue = new();
    private readonly BlockingCollection<AggregationWorkItem> _aggregationQueue = new();

    // Tick coordination
    private readonly HighResolutionTicker _simulationTicker;
    private readonly HighResolutionTicker _aggregatorTicker;

    public MultiThreadedSimulationManager(
        ILogger<MultiThreadedSimulationManager> logger,
        NServerEcs world,
        ThreadPoolConfig config,
        Instrumentation instrumentation)
    {
        _logger = logger;
        _world = world;
        _config = config;
        _instrumentation = instrumentation;

        _simulationTicker = new HighResolutionTicker(
            config.SimulationTickInterval, 0, 0, instrumentation: instrumentation);

        _aggregatorTicker = new HighResolutionTicker(
            config.AggregatorTickInterval, 0, 0, instrumentation: instrumentation);
    }

    public void Start()
    {
        _logger.LogInformation("Starting multi-threaded simulation manager with {SimThreads} simulation threads and {AggThreads} aggregator threads",
            _config.SimulationThreadCount, _config.AggregatorThreadCount);

        // Start simulation threads
        for (int i = 0; i < _config.SimulationThreadCount; i++)
        {
            var threadIndex = i;
            var thread = new Thread(() => SimulationWorker(threadIndex))
            {
                IsBackground = true,
                Name = $"Simulation-{threadIndex}"
            };
            thread.Start();
            _simulationThreads.Add(thread);
        }

        // Start aggregator threads
        for (int i = 0; i < _config.AggregatorThreadCount; i++)
        {
            var threadIndex = i;
            var thread = new Thread(() => AggregatorWorker(threadIndex))
            {
                IsBackground = true,
                Name = $"Aggregator-{threadIndex}"
            };
            thread.Start();
            _aggregatorThreads.Add(thread);
        }

        // Start tickers
        _simulationTicker.OnTick = ScheduleSimulationWork;
        _aggregatorTicker.OnTick = ScheduleAggregationWork;

        _simulationTicker.Start();
        _aggregatorTicker.Start();

        _logger.LogInformation("Multi-threaded simulation manager started");
    }

    public void Stop()
    {
        _logger.LogInformation("Stopping multi-threaded simulation manager");

        _simulationTicker.Stop();
        _aggregatorTicker.Stop();

        _cts.Cancel();
        _simulationQueue.CompleteAdding();
        _aggregationQueue.CompleteAdding();

        foreach (var thread in _simulationThreads)
            thread.Join();

        foreach (var thread in _aggregatorThreads)
            thread.Join();

        _logger.LogInformation("Multi-threaded simulation manager stopped");
    }

    private void ScheduleSimulationWork(UpdateInfo info)
    {
        // Get all deep tree nodes that need simulation
        var simulationNodes = _world.Grid.GetNodesAtLevel(_world.Grid.GridInfo.SimulationLevel).ToList();

        // Distribute work across simulation threads
        foreach (var (key, node) in simulationNodes)
        {
            if (node.EntityCount == 0) continue;

            _simulationQueue.Add(new SimulationWorkItem
            {
                Key = key,
                Node = node,
                DeltaTime = (f32)info.DeltaTime.TotalSeconds
            });
        }
    }

    private void ScheduleAggregationWork(UpdateInfo info)
    {
        // Get all shallow nodes that need aggregation (LOD calculation)
        for (i32 level = _world.Grid.GridInfo.AggregatorMinLevel; level < _world.Grid.GridInfo.SimulationLevel; level++)
        {
            var aggregatorNodes = _world.Grid.GetNodesAtLevel(level).ToList();

            foreach (var (key, node) in aggregatorNodes)
            {
                _aggregationQueue.Add(new AggregationWorkItem
                {
                    Key = key,
                    Node = node,
                    Level = level
                });
            }
        }
    }

    private void SimulationWorker(i32 threadIndex)
    {
        _logger.LogInformation("Simulation worker {Index} started", threadIndex);

        try
        {
            foreach (var workItem in _simulationQueue.GetConsumingEnumerable(_cts.Token))
            {
                ProcessSimulationWork(workItem);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in simulation worker {Index}", threadIndex);
        }

        _logger.LogInformation("Simulation worker {Index} stopped", threadIndex);
    }

    private void AggregatorWorker(i32 threadIndex)
    {
        _logger.LogInformation("Aggregator worker {Index} started", threadIndex);

        try
        {
            foreach (var workItem in _aggregationQueue.GetConsumingEnumerable(_cts.Token))
            {
                ProcessAggregationWork(workItem);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in aggregator worker {Index}", threadIndex);
        }

        _logger.LogInformation("Aggregator worker {Index} stopped", threadIndex);
    }

    private void ProcessSimulationWork(SimulationWorkItem workItem)
    {
        using var trace = _instrumentation.Trace("SimulationWork");

        // Get all entities in this node
        var entities = workItem.Node.GetEntities();

        // Run physics simulation on each entity
        _world.DoLocked(() =>
        {
            foreach (var entityId in entities)
            {
                if (!_world.Position.TryGet(entityId, out var position)) continue;
                if (!_world.Velocity.TryGet(entityId, out var velocity)) continue;

                // Simple velocity integration
                position.Value += velocity.Linear * workItem.DeltaTime;
                _world.Position.Update(entityId) = position;

                // Apply rotation
                if (_world.Rotation.TryGet(entityId, out var rotation))
                {
                    // Simple angular velocity integration (simplified)
                    var angularVel = velocity.Angular;
                    if (angularVel.LengthSquared() > 0.0001f)
                    {
                        var angle = angularVel.Length() * workItem.DeltaTime;
                        var axis = System.Numerics.Vector3.Normalize(angularVel);
                        var deltaRotation = System.Numerics.Quaternion.CreateFromAxisAngle(axis, angle);
                        rotation.Value = System.Numerics.Quaternion.Multiply(rotation.Value, deltaRotation);
                        _world.Rotation.Update(entityId) = rotation;
                    }
                }
            }
        });
    }

    private void ProcessAggregationWork(AggregationWorkItem workItem)
    {
        using var trace = _instrumentation.Trace("AggregationWork");

        // Get all child nodes and aggregate their data into an LOD proxy
        var childKeys = workItem.Key.GetChildren().ToList();

        i32 totalEntities = 0;
        System.Numerics.Vector3 avgPosition = System.Numerics.Vector3.Zero;

        _world.DoLocked(() =>
        {
            foreach (var childKey in childKeys)
            {
                var childNodes = _world.Grid.GetNodesAtLevel(childKey.Level);

                foreach (var (key, node) in childNodes)
                {
                    if (!key.Equals(childKey)) continue;

                    totalEntities += node.EntityCount;

                    // Calculate average position of entities in child node
                    var entities = node.GetEntities();
                    foreach (var entityId in entities)
                    {
                        if (_world.Position.TryGet(entityId, out var pos))
                        {
                            avgPosition += pos.Value;
                        }
                    }
                }
            }

            // Store aggregated LOD data in the parent node
            // (This is simplified - in production, create actual LOD proxy entities)
            if (totalEntities > 0)
            {
                avgPosition /= totalEntities;
                // Mark node as having aggregated data
                workItem.Node.MarkDirty();
            }
        });
    }

    public void Dispose()
    {
        _cts.Dispose();
        _simulationQueue.Dispose();
        _aggregationQueue.Dispose();
    }
}

/// <summary>
/// Work item for simulation threads
/// </summary>
internal record struct SimulationWorkItem
{
    public MortonKey Key;
    public KDTreeNode Node;
    public f32 DeltaTime;
}

/// <summary>
/// Work item for aggregation threads
/// </summary>
internal record struct AggregationWorkItem
{
    public MortonKey Key;
    public KDTreeNode Node;
    public i32 Level;
}
