using System.Diagnostics;
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

public class ServerGameLogic
{
    private readonly ServerEcsWorld _world = new();
    private readonly SpatialGrid _spatialGrid = new(new SpacialGridInfo(new ChunkCoord(16, 16, 16, 1)));
    private readonly QServer _server;
    private readonly ILogger<ServerGameLogic> _logger;
    private readonly SemaphoreSlim _semaphorePlayer = new(1);

    private readonly Dictionary<QConnection, EntityId> _playerEntities = new();
    private readonly Dictionary<QConnection, HashSet<ChunkCoord>> _loadedChunks = new();
    private const int MaxSendRangeChunks = 10; //  Range in chunks (4 chunks = 4*32 = 128 world units)
    private const int MinSendRangeChunks = 4; // Minimum range in chunks
    private int _currentSendRangeChunks = MinSendRangeChunks;
    private int _rangeStep;
    private const int MaxRangeSteps = 100;

    private DemoLevel _level;

    private void UpdateSendRange()
    {
        var t = (f32)_rangeStep / MaxRangeSteps; // 0..1
        _currentSendRangeChunks = (int)(MinSendRangeChunks + t * (MaxSendRangeChunks - MinSendRangeChunks));
        _rangeStep = (_rangeStep + 1) % (MaxRangeSteps + 1);
    }

    public ServerGameLogic(QServer server, ILogger<ServerGameLogic> logger)
    {
        _server = server;
        _logger = logger;
        _level = new DemoLevel(_world, _spatialGrid);
        _level.LoadLevel("Sponza");
    }


    public void SetupPlayer(QConnection connection)
    {
        EntityId entity;
        try
        {
            _semaphorePlayer.Wait();
            // Send level load info

            entity = _world.CreateEntity();
            _level.SetupPlayer(connection, entity);
            var spawnPosition = new Vector3(0, 1, 0);

            _world.AddPosition(entity, new Position
            {
                Value = spawnPosition,
                Level = 0
            });
            _world.AddRotation(entity, new Rotation
            {
                Value = Quaternion.Identity
            });
            _world.AddScale(entity, new Scale
            {
                Value = Vector3.One
            });
            _world.AddEntityType(entity, new EntityType { TypeId = 2 }); // Type 2 for player

            // Add to spatial grid
            var chunk = _spatialGrid.GetChunkCoord(spawnPosition, 0);
            _world.AddSpatialData(entity, new SpatialData
            {
                CurrentChunk = chunk with { Z = chunk.Z + 1 },
                PreviousChunk = chunk,
            });

            // Track player entity
            _playerEntities[connection] = entity;
            _loadedChunks[connection] = [];
        }
        finally
        {
            _semaphorePlayer.Release();
        }

        _logger.LogInformation("Player entity {Entity} created for connection {Connection}", entity, connection);

        // Send entity creation to all players within range
        var position = _world.GetPosition(entity);
        var entityType = _world.GetEntityType(entity);
        var rotation = _world.GetRotation(entity);
        var scale = _world.GetScale(entity);
        using (var msg = new EntityCreatedPacket((NetEntId)entity,
                   CreateTransformFromComponents(position, rotation, scale), entityType.TypeId).ToMessage())
        {
            connection.Send(msg); // client needs to know about its own entity immediately
            SendToPlayersInRange(position.Value, position.Level, msg);
        }

        // Send assignment to the player
        using (var msg = new EntityAssignedPacket((NetEntId)entity).ToMessage())
            connection.Send(msg);
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
        foreach (var kv in _playerEntities)
        {
            UnloadPlayerChunks(kv.Key, kv.Value);

            new PingPacket(Stopwatch.GetTimestamp()).Send(kv.Key);
        }
    }

    [TraceInterception]
    public void UnloadPlayerChunks(QConnection connection, EntityId playerEntity)
    {
        if (!_world.TryGetPosition(playerEntity, out var playerPosition)) return;
        var playerChunk = _spatialGrid.GetChunkCoord(playerPosition.Value, playerPosition.Level);
        var visibleChunks = _loadedChunks[connection];
        foreach (var coord in
                 visibleChunks.Where(chunkCoord => chunkCoord.PosDistanceTo(playerChunk) > MaxSendRangeChunks))
        {
            SendUnloadChunk(connection, coord);
            visibleChunks.Remove(coord);
        }
    }

    [TraceInterception]
    public void SendUnloadChunk(QConnection connection, ChunkCoord chunkCoord)
    {
        new ChunkUnloadPacket(chunkCoord, _spatialGrid.GetChunkData(chunkCoord).Entities
            .Select(x => (NetEntId)x).ToList()).Send(connection);
    }

    public void SendLoadChunk(QConnection connection, ChunkCoord chunkCoord)
    {
        var chunk = _spatialGrid.GetChunkData(chunkCoord);
        using var _ = chunk.UseLock();
        new ChunkLoadPacket(chunkCoord, chunk.Entities
            .Select(x => (NetEntId)x).ToList()).Send(connection);
    }

    private void SendToPlayersInRange(Vector3 position, i32 level, SerializedMessage packet)
    {
        // Calculate which chunk this position is in
        var chunkCoord = _spatialGrid.GetChunkCoord(position, level);

        foreach (var (connection, entity) in _playerEntities)
        {
            if (!_world.TryGetPosition(entity, out var playerPosition)) continue;
            var playerChunk = _spatialGrid.GetChunkCoord(playerPosition.Value, playerPosition.Level);
            if (playerChunk.PosDistanceTo(chunkCoord) > MaxSendRangeChunks) continue;
            connection.Send(packet);
        }
    }

    public void HandleEntityUpdate(EntityUpdatePacket message)
    {
        var entity = (EntityId)message.Id;
        if (!_world.HasPosition(entity)) return;

        _world.UpdatePosition(entity).Value = message.Transform.Position;
        _world.UpdateRotation(entity).Value = message.Transform.Rotation;
        _world.UpdateScale(entity).Value = message.Transform.Scale;
        if (_world.HasSpatialData(entity))
        {
            _spatialGrid.GetChunkData(_world.GetSpatialData(entity).CurrentChunk).MarkDirty();
        }
    }

    public void RemovePlayer(QConnection connection) => DoLocked(() => InternalRemovePlayer(connection));

    private void InternalRemovePlayer(QConnection connection)
    {
        if (_playerEntities.TryGetValue(connection, out var playerEntity))
        {
            // Remove from spatial grid
            if (_world.HasSpatialData(playerEntity))
            {
                var spatialData = _world.GetSpatialData(playerEntity);
                // Note: Could clean up empty chunks here if needed
            }

            // Remove from ECS world
            _world.DeleteEntity(playerEntity);

            // Clean up tracking
            _playerEntities.Remove(connection, out _);
            _loadedChunks.Remove(connection, out _);

            // Notify other players in range
            var position = _world.HasPosition(playerEntity) ? _world.GetPosition(playerEntity) : new Position();
            using var msg = new EntityDestroyedPacket((NetEntId)playerEntity).ToMessage();
            SendToPlayersInRange(position.Value, position.Level, msg);
        }
    }

    private static Transform CreateTransformFromComponents(Position position, Rotation rotation, Scale scale)
    {
        return new Transform
        {
            Position = position.Value,
            Rotation = rotation.Value,
            Scale = scale.Value
        };
    }

    private void SendChunkUpdates()
    {
        // Get all dirty chunks from the spatial grid
        var dirtyChunks = _spatialGrid.GetDirtyChunks().ToList();

        if (dirtyChunks.Count == 0) return;
        List<QConnection> visibleChunksSnapshot = [];
        foreach (var (chunkCoord, chunk) in dirtyChunks)
        {
            if (chunk.Entities.Count == 0) continue;
            visibleChunksSnapshot.Clear();
            foreach (var (connection, visibleChunks) in _playerEntities)
            {
                var pos = _world.GetPosition(visibleChunks);
                var playerChunk = _spatialGrid.GetChunkCoord(pos.Value, pos.Level);
                var chunkDistance = playerChunk.PosDistanceTo(chunkCoord);
                if (chunkDistance <= _currentSendRangeChunks)
                    visibleChunksSnapshot.Add(connection);
            }

            if (visibleChunksSnapshot.Count == 0) continue;

            using var chunkData = BuildChunkData(chunkCoord, chunk);

            // Send to all players who can see this chunk
            foreach (var connection in visibleChunksSnapshot)
            {
                if (_loadedChunks[connection].Add(chunkCoord))
                {
                    SendLoadChunk(connection, chunkCoord);
                }

                connection.Send(chunkData);
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
            var addedIds = (added ?? []).Select(x => (NetEntId)x).ToList();
            var removedIds = (removed ?? []).Select(x => (NetEntId)x).ToList();
            new ChunkDeltaPacket(chunkCoord, addedIds, removedIds).Serialize(ref writer, false);
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

        new ChunkUpdatePacket(chunkCoord, positions).Serialize(ref writer);
        if (rotations.Data.Count > 0)
        {
            new ChunkUpdatePacket(chunkCoord, rotations).Serialize(ref writer);
        }

        if (scales.Data.Count > 0)
        {
            new ChunkUpdatePacket(chunkCoord, scales).Serialize(ref writer);
        }

        return serializer;
    }
}