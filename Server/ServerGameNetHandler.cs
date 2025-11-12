using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Numerics;
using Ajiva.Ecs;
using Common;
using Common.Ecs;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;
using Interception;
using MemoryPack;
using MemoryPack.Internal;
using NetworkPackets;

namespace Server;

public partial class ServerGameNetHandler(
    ILogger<ServerGameNetHandler> logger,
    Instrumentation instrumentation,
    QFactory factory) : IServerPacketHandler<ConnectionContext>, QServerEventListener,
    IAsyncDisposable
{
    private readonly ServerEcsWorld _world = new();
    private readonly SemaphoreSlim _semaphorePlayer = new(1);

    private readonly Dictionary<QConnection, ConnectionContext> _connectionContexts = new();
    private const i32 MaxSendRangeChunks = 16; //  Range in chunks (4 chunks = 4*32 = 128 world units)
    private const i32 MinSendRangeChunks = 4; // Minimum range in chunks
    private i32 _currentSendRangeChunks = MinSendRangeChunks;
    private i32 _rangeStep;
    private const i32 MaxRangeSteps = 100;

    private DemoLevel _level = null!;
    private QServer _server = null!;
    private ManagementServer _managementServer = null!;
    private SpatialGrid _spatialGrid = null!;
    private ServerId _serverId;
    private Thread? _serverNetworkTask, _serverUpdateTask;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly CancellationTokenSource _stoppingCts = new();

    private void UpdateSendRange()
    {
        //_currentSendRangeChunks = MaxSendRangeChunks;
        var t = (f32)_rangeStep / MaxRangeSteps; // 0..1
        _currentSendRangeChunks = (i32)(MinSendRangeChunks + t * (MaxSendRangeChunks - MinSendRangeChunks));
        _rangeStep = (_rangeStep + 1) % (MaxRangeSteps + 1);
    }

    public async Task<IPEndPoint> InitAsync(ServerId serverId, ManagementServer managementServer,
        RegionAssignment assignedRegion, SpacialGridInfo spatialGridInfo)
    {
        _serverId = serverId;
        _managementServer = managementServer;
        _spatialGrid = new SpatialGrid(spatialGridInfo);
        _level = new DemoLevel(_world, _spatialGrid, serverId, assignedRegion);
        _level.LoadLevel();
        _server = factory.CreateServer(0);
        _server.Start(this, instrumentation);
        _serverUpdateTask = new Thread(() => RunLoop(_stoppingCts.Token, loopName: "GameTick",
            histogram: instrumentation.Meter?.CreateHistogram<f64>("server.tick.duration",
                unit: "ms", description: "Duration of server tick"),
            lockedAction: Update, tps: 50f)
        );
        _serverNetworkTask = new Thread(() => RunLoop(_stoppingCts.Token, loopName: "NetworkTick",
            histogram: instrumentation.Meter?.CreateHistogram<f64>("server.network.duration",
                unit: "ms", description: "Duration of server net tick"),
            lockedAction: Poll, tps: 100f)
        );
        _serverUpdateTask?.Start();
        _serverNetworkTask?.Start();
        await _server.WaitForReady();
        return _server.LocalEndPoint!;
    }

    private void RunLoop(CancellationToken token, string loopName, Histogram<f64>? histogram, Action<f32> lockedAction,
        f32 tps, u32 traceIntervalSeconds = 10)
    {
        f32 traceIntervalElapsed = 0;
        var lastUpdate = Stopwatch.GetTimestamp();
        while (!token.IsCancellationRequested)
        {
            if (traceIntervalElapsed > traceIntervalSeconds) Instrumentation.Instance = instrumentation;

            using var activity = Instrumentation.Instance?.StartActivity(ActivityKind.Server);

            var now = Stopwatch.GetTimestamp();
            var elapsed = Stopwatch.GetElapsedTime(lastUpdate, now);
            var delta = (f32)elapsed.TotalSeconds;

            try
            {
                using (Instrumentation.Instance?.StartNamedActivity($"Entering Locked Section ({loopName})"))
                    _semaphore.Wait(token);
                using var exec = Instrumentation.Instance?.StartNamedActivity($"Executing ({loopName})");
                lockedAction.Invoke(delta);
            }
            finally
            {
                _semaphore.Release();
            }

            histogram?.Record(elapsed.TotalMilliseconds);
            lastUpdate = now;

            var sleepTime = 1f / tps - delta;
            using var wait = Instrumentation.Instance?.StartNamedActivity("Waiting",
                tags: [new KeyValuePair<string, object?>("sleepTime", sleepTime)]);
            Thread.Sleep(Math.Max(1, (i32)(sleepTime * 1000)));
            if (traceIntervalElapsed > traceIntervalSeconds)
            {
                Instrumentation.Instance = null;
                traceIntervalElapsed = 0;
            }

            traceIntervalElapsed += delta;
        }
    }

    public void Poll(f32 f)
    {
        using var poll = Instrumentation.NamedTrace("PollPackets");
        _server.HandlePackets(DispatchPacket);
    }

    public void Update(f32 deltaTime)
    {
        using var update = Instrumentation.Trace(ActivityKind.Server);

        UpdateSendRange();

        _level.Update(deltaTime);

        // Update spatial partitioning for moved entities
        using (var act = Instrumentation.NamedTrace("SpatialPartitioning"))
            DoLocked(UpdateSpatialPartitioning);

        // Update player visibility based on spatial chunks
        using (var act = Instrumentation.NamedTrace("PlayerVisibility"))
            DoLocked(UpdateAllPlayerVisibility);

        // Send chunk updates for dirty chunks
        using (var act = Instrumentation.NamedTrace("ChunkUpdates"))
            DoLocked(SendChunkUpdates);
    }

    private void DoLocked(Action action)
    {
        using var trace = Instrumentation.NamedTrace("DoLocked");
        try
        {
            _semaphorePlayer.Wait();
            trace?.AddEvent(new ActivityEvent("Acquired Lock"));
            action();
            trace?.AddEvent(new ActivityEvent("Finished Action"));
        }
        finally
        {
            _semaphorePlayer.Release();
        }
    }


    [TraceInterception]
    public void UpdateSpatialPartitioning()
    {
        // Check all entities with position and spatial data for movement
        foreach (var entityId in _world.GetEntitiesWithPosition())
        {
            if (!_world.HasSpatialData(entityId)) continue;

            var position = _world.GetPosition(entityId);
            var spatialData = _world.GetSpatialData(entityId);

            var currentChunk = _spatialGrid.GetChunkCoord(position.Value, position.Level);

            if (!currentChunk.Equals(spatialData.CurrentChunk))
            {
                spatialData.PreviousChunk = spatialData.CurrentChunk;
                spatialData.CurrentChunk = currentChunk;
                _spatialGrid.MoveEntity(entityId, spatialData.PreviousChunk, spatialData.CurrentChunk);
                _world.UpdateSpatialData(entityId) = spatialData;
            }
        }
    }

    [TraceInterception]
    public void UpdateAllPlayerVisibility()
    {
        foreach (var kv in _connectionContexts)
        {
            UnloadPlayerChunks(kv.Value);

            new Packets.Ping(Stopwatch.GetTimestamp()).Send(kv.Key);
        }
    }

    [TraceInterception]
    public void UnloadPlayerChunks(ConnectionContext context)
    {
        var playerPosition = context.Position;
        var playerChunk = _spatialGrid.GetChunkCoord(playerPosition.Value, playerPosition.Level);
        var visibleChunks = context.LoadedChunks;
        foreach (var coord in
                 visibleChunks.Where(chunkCoord => chunkCoord.PosDistanceTo(playerChunk) > MaxSendRangeChunks))
        {
            SendUnloadChunk(context, coord);
            visibleChunks.Remove(coord);
        }
    }

    [TraceInterception]
    public void SendUnloadChunk(ConnectionContext context, ChunkCoord chunkCoord)
    {
        new ToClientPackets.ChunkUnloadPacket(chunkCoord, _spatialGrid.GetChunkData(chunkCoord).Entities
            .Select(x => new NetEntId(_serverId, x)).ToList()).Send(context.Connection);
    }

    public void SendLoadChunk(ConnectionContext context, ChunkCoord chunkCoord)
    {
        var chunk = _spatialGrid.GetChunkData(chunkCoord);
        using var _ = chunk.UseLock();
        new ToClientPackets.ChunkLoadPacket(chunkCoord, chunk.Entities
            .Select(x => new NetEntId(_serverId, x)).ToList()).Send(context.Connection);
    }

    private void SendToPlayersInRange(Vector3 position, i32 level, SerializedMessage packet)
    {
        // Calculate which chunk this position is in
        var chunkCoord = _spatialGrid.GetChunkCoord(position, level);

        foreach (var context in _connectionContexts.Values)
        {
            var playerPosition = context.Position;
            var playerChunk = _spatialGrid.GetChunkCoord(playerPosition.Value, playerPosition.Level);
            if (playerChunk.PosDistanceTo(chunkCoord) > MaxSendRangeChunks) continue;
            context.Connection.Send(packet);
        }
    }

    public void HandleEntityUpdate(EntityId entity, Transform transform)
    {
        if (!_world.HasPosition(entity)) return;

        _world.UpdatePosition(entity).Value = transform.Position;
        _world.UpdateRotation(entity).Value = transform.Rotation;
        _world.UpdateScale(entity).Value = transform.Scale;
        if (_world.HasSpatialData(entity))
        {
            _spatialGrid.GetChunkData(_world.GetSpatialData(entity).CurrentChunk).MarkDirty();
        }
    }

    public void RemovePlayer(ConnectionContext context) => DoLocked(() => InternalRemovePlayer(context));

    private void InternalRemovePlayer(ConnectionContext context)
    {
        var playerEntity = context.PrimaryEntity;
        // Remove from spatial grid
        if (_world.HasSpatialData(playerEntity))
        {
            var spatialData = _world.GetSpatialData(playerEntity);
            // Note: Could clean up empty chunks here if needed
        }

        // Remove from ECS world
        _world.DeleteEntity(playerEntity);
        context.PrimaryEntity = EntityId.Invalid;
        context.LoadedChunks.Clear();
        _connectionContexts.Remove(context.Connection);

        // Notify other players in range
        var position = _world.HasPosition(playerEntity) ? _world.GetPosition(playerEntity) : new Position();
        using var msg = new ToClientPackets.EntityDestroyedPacket(new NetEntId(_serverId, playerEntity)).ToMessage();
        SendToPlayersInRange(position.Value, position.Level, msg);
    }
    
    private void SendChunkUpdates()
    {
        // Get all dirty chunks from the spatial grid
        var dirtyChunks = _spatialGrid.GetDirtyChunks().ToList();

        if (dirtyChunks.Count == 0) return;
        List<ConnectionContext> visibleChunksSnapshot = [];
        foreach (var (chunkCoord, chunk) in dirtyChunks)
        {
            //if (chunk.Entities.Count == 0) continue; we still need to delta removed entities
            if(chunk.Entities.Count == 0 && !_spatialGrid.TryGetDeltaInfo(chunkCoord, out var added, out var removed)) continue;
            visibleChunksSnapshot.Clear();
            foreach (var context in _connectionContexts.Values)
            {
                var playerChunk = _spatialGrid.GetChunkCoord(context.Position.Value, context.Position.Level);
                var chunkDistance = playerChunk.PosDistanceTo(chunkCoord);
                if (chunkDistance <= _currentSendRangeChunks)
                    visibleChunksSnapshot.Add(context);
            }

            if (visibleChunksSnapshot.Count == 0) continue;

            using var chunkData = BuildChunkData(chunkCoord, chunk);

            // Send to all players who can see this chunk
            foreach (var context in visibleChunksSnapshot)
            {
                if (context.LoadedChunks.Add(chunkCoord))
                {
                    SendLoadChunk(context, chunkCoord);
                }

                context.Connection.Send(chunkData);
            }
        }

        // Clear dirty flags after sending updates
        _spatialGrid.ClearDirtyFlags();
    }

    private SerializedMessage BuildChunkData(ChunkCoord chunkCoord, SpatialChunk chunk)
    {
        var serializer = new SerializedMessage();
        var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref serializer.Writer, serializer.State);
        if (_spatialGrid.TryGetDeltaInfo(chunkCoord, out var added, out var removed))
        {
            var addedIds = (added ?? []).Select(x => new NetEntId(_serverId, x)).ToList();
            var removedIds = (removed ?? []).Select(x => new NetEntId(_serverId, x)).ToList();
            new ToClientPackets.ChunkDeltaPacket(chunkCoord, addedIds, removedIds).Serialize(ref writer, false);
        }

        ChunkData<PositionChunkData> positions = new([], null);
        ChunkData<RotationChunkData> rotations = new([], []);
        ChunkData<ScaleChunkData> scales = new([], []);
        using var _ = chunk.UseLock();
        u8 i = 0;
        foreach (var entityId in chunk.Entities)
        {
            if (!_world.TryGetPosition(entityId, out var position))
                position = default;

            positions.Data.Add(PositionChunkData.QuantizeFor(chunkCoord, _spatialGrid.GridInfo, position.Value));

            //if (_world.HasDirtyRotation(entityId))
            {
                var rotation = _world.GetRotation(entityId);
                rotations.Data.Add(RotationChunkData.Quantize(rotation.Value));
                rotations.IndexOverride!.Add(i);
                _world.ClearRotationDirty(entityId);
            }

            //if (_world.HasDirtyScale(entityId))
            {
                var scale = _world.GetScale(entityId);
                scales.Data.Add(new ScaleChunkData(scale.Value));
                scales.IndexOverride!.Add(i);
                _world.ClearScaleDirty(entityId);
            }

            i++;
        }

        new ToClientPackets.ChunkUpdatePacket(chunkCoord, positions).Serialize(ref writer);
        if (rotations.Data.Count > 0)
        {
            new ToClientPackets.ChunkUpdatePacket(chunkCoord, rotations).Serialize(ref writer);
        }

        if (scales.Data.Count > 0)
        {
            new ToClientPackets.ChunkUpdatePacket(chunkCoord, scales).Serialize(ref writer);
        }

        return serializer;
    }

    public async ValueTask DisposeAsync()
    {
        await _server.DisposeAsync();
        try
        {
            await _stoppingCts.CancelAsync();
        }
        finally
        {
            _serverUpdateTask?.Join();
            _serverNetworkTask?.Join();
            _stoppingCts.Dispose();
        }

        _semaphorePlayer.Dispose();
        _semaphore.Dispose();
    }
}