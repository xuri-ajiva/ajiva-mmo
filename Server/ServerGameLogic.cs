using System.Diagnostics;
using System.Numerics;
using Ajiva.Ecs;
using Common;
using Common.Ecs;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;
using Interception;
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
    private EntityId _rotatingEntity;
    private readonly List<EntityId> _bouncingEntities = [];
    private readonly List<EntityId> _slidingEntities = [];
    private string? _levelName;
    private const int MaxSendRangeChunks = 10; //  Range in chunks (4 chunks = 4*32 = 128 world units)
    private const int MinSendRangeChunks = 4; // Minimum range in chunks
    private int _currentSendRangeChunks = MinSendRangeChunks;
    private int _rangeStep;
    private const int MaxRangeSteps = 100;

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
        LoadLevel("Sponza");
    }

    private void LoadLevel(string levelName)
    {
        _levelName = levelName;
        _playerEntities.Clear();
        _loadedChunks.Clear();
        _world.ClearAllEntities();

        // Create a rotating box
        _rotatingEntity = _world.CreateEntity();
        _world.AddPosition(_rotatingEntity, new Position
        {
            Value = new Vector3(0, 1, 5),
            Level = 0
        });
        _world.AddRotation(_rotatingEntity, new Rotation
        {
            Value = Quaternion.Identity
        });
        _world.AddScale(_rotatingEntity, new Scale
        {
            Value = Vector3.One
        });
        _world.AddVelocity(_rotatingEntity, new Velocity
        {
            Linear = Vector3.Zero,
            Angular = new Vector3(0, 1, 0) // Rotate around Y axis
        });
        _world.AddEntityType(_rotatingEntity, new EntityType { TypeId = 1 }); // Type 1 for rotating box

        // Add to spatial grid
        var pos = _world.GetPosition(_rotatingEntity);
        var chunk = _spatialGrid.GetChunkCoord(pos.Value, pos.Level);
        _world.AddSpatialData(_rotatingEntity, new SpatialData
        {
            CurrentChunk = chunk with { Z = chunk.Z + 1 },
            PreviousChunk = chunk,
        });

        // Create some static test entities
        for (var i = 0; i < 200; i++)
        {
            for (var j = 0; j < 200; j++)
            {
                var entity = _world.CreateEntity();
                var position = new Vector3(i * 2, MathF.Floor(( /*f32.Cos(i * 0.1f) + */f32.Sin(j * 0.05f) + 2) * 3),
                    j * 2);

                _world.AddPosition(entity, new Position
                {
                    Value = position,
                    Level = 0
                });
                _world.AddRotation(entity, new Rotation
                {
                    Value = Quaternion.Identity
                });
                _world.AddScale(entity, new Scale
                {
                    Value = new Vector3(0.5f)
                });
                _world.AddEntityType(entity, new EntityType { TypeId = 1 });
                _world.AddVelocity(entity, new Velocity
                {
                    Linear = Vector3.Zero,
                    Angular = Vector3.Zero
                });

                // Add to spatial grid
                chunk = _spatialGrid.GetChunkCoord(position, 0);
                _world.AddSpatialData(entity, new SpatialData
                {
                    CurrentChunk = chunk with { Z = chunk.Z + 1 },
                    PreviousChunk = chunk,
                });
                _bouncingEntities.Add(entity);
            }
        }

        /*for (var i = 0; i < 0; i++)
        {
            var entity = _world.CreateEntity();
            var position = new Vector3(-i * 2, 5, -i * 2 + 10);

            _world.AddPosition(entity, new Position
            {
                Value = position,
                Level = 0
            });
            _world.AddRotation(entity, new Rotation
            {
                Value = Quaternion.Identity
            });
            _world.AddScale(entity, new Scale
            {
                Value = new Vector3(0.5f)
            });
            _world.AddEntityType(entity, new EntityType { TypeId = 1 });

            // Add to spatial grid
            chunk = _spatialGrid.GetChunkCoord(position, 0);
            _world.AddSpatialData(entity, new SpatialData
            {
                CurrentChunk = chunk with { Z = chunk.Z + 1 },
                PreviousChunk = chunk,
            });
            _slidingEntities.Add(entity);
        }*/
    }

    public void SetupPlayer(QConnection connection)
    {
        EntityId entity;
        try
        {
            _semaphorePlayer.Wait();
            // Send level load info
            new LoadLevelPacket(_levelName ?? "", _spatialGrid.GridInfo).Send(connection);

            entity = _world.CreateEntity();
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

        RotateEntity(deltaTime);

        // Update bouncing entities
        UpdateBouncing(deltaTime);

        UpdateSliding(deltaTime);

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

    private f32 _progress;

    private void UpdateSliding(f32 deltaTime)
    {
        var speed = 0.1f; // cycles per second (1 = full square per second)
        var squareSize = 10f; // size of square
        // Advance progress
        _progress = (_progress + speed * deltaTime) % 1f;
        using var act = Instrumentation.NamedTrace("Sliding");
        foreach (var entityId in _slidingEntities)
        {
            if (!_world.HasPosition(entityId) || !_world.HasScale(entityId) || !_world.HasRotation(entityId))
                continue;

            var progress = (_progress + (entityId.Value % 8) * 0.25f) % 1.0f;

            // Determine position along square perimeter
            Vector3 pos;
            if (progress < 0.25f) // bottom edge → right
            {
                var t = progress / 0.25f;
                pos = new Vector3(Lerp(-squareSize, squareSize, t), 0, -squareSize);
            }
            else if (progress < 0.5f) // right edge → up
            {
                var t = (progress - 0.25f) / 0.25f;
                pos = new Vector3(squareSize, 0, Lerp(-squareSize, squareSize, t));
            }
            else if (progress < 0.75f) // top edge → left
            {
                var t = (progress - 0.5f) / 0.25f;
                pos = new Vector3(Lerp(squareSize, -squareSize, t), 0, squareSize);
            }
            else // left edge → down
            {
                var t = (progress - 0.75f) / 0.25f;
                pos = new Vector3(-squareSize, 0, Lerp(squareSize, -squareSize, t));
            }

            // Example: scale changes from 0.5x at left to 2x at right
            var scaleFactor = Lerp(0.5f, 2f, (pos.X + squareSize) / (squareSize * 2));

            // Update ECS components
            ref var position = ref _world.GetPosition(entityId);
            position.Value = pos;

            ref var scale = ref _world.GetScale(entityId);
            scale.Value = new Vector3(scaleFactor);

            ref var rotation = ref _world.GetRotation(entityId);
            rotation.Value = Quaternion.Identity; // keep upright

            if (_world.HasSpatialData(entityId))
            {
                _spatialGrid.GetChunkData(_world.GetSpatialData(entityId).CurrentChunk).MarkDirty();
            }
        }
    }

    private void UpdateBouncing(f32 deltaTime)
    {
        using var act = Instrumentation.NamedTrace("Bouncing");
        foreach (var entityId in _bouncingEntities)
        {
            if (!_world.HasPosition(entityId) || !_world.HasVelocity(entityId)) continue;

            ref var position = ref _world.GetPosition(entityId);
            ref var velocity = ref _world.GetVelocity(entityId);

            // simple bounce logic
            if (position.Value.Y <= 0)
            {
                velocity.Linear.Y = -velocity.Linear.Y; // Reverse Y velocity on hit
                position.Value.Y = 0; // Reset to ground level
            }
            else
            {
                velocity.Linear.Y -= 9.81f * deltaTime; // Apply gravity
            }

            velocity.Linear.Y = Math.Clamp(velocity.Linear.Y, -20f, 20f);

            // Update position based on velocity
            position.Value += velocity.Linear * deltaTime;

            if (_world.HasSpatialData(entityId))
            {
                _spatialGrid.GetChunkData(_world.GetSpatialData(entityId).CurrentChunk).MarkDirty();
            }
        }
    }

    private void RotateEntity(f32 deltaTime)
    {
        if (!_world.HasVelocity(_rotatingEntity)) return;

        using var act = Instrumentation.NamedTrace("Velocity");
        ref var rotation = ref _world.GetRotation(_rotatingEntity);
        var velocity = _world.GetVelocity(_rotatingEntity);

        // Apply angular velocity
        var angularRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, velocity.Angular.Y * deltaTime);
        rotation.Value *= angularRotation;
        //rotation.Value = Quaternion.Normalize(rotation.Value);
        if (_world.HasSpatialData(_rotatingEntity))
        {
            _spatialGrid.GetChunkData(_world.GetSpatialData(_rotatingEntity).CurrentChunk).MarkDirty();
        }
    }

    private static T Lerp<T>(T a, T b, T t) where T : INumber<T> => a + (b - a) * t;

    [TraceInterception]
    public void UpdateSpatialPartitioning()
    {
        // Check all entities with position and spatial data for movement
        foreach (var entityId in _world.GetEntitiesWithPosition())
        {
            if (!_world.HasSpatialData(entityId)) continue;

            var position = _world.GetPosition(entityId);
            ref var spatialData = ref _world.GetSpatialData(entityId);

            var currentChunk = _spatialGrid.GetChunkCoord(position.Value, position.Level);

            if (!currentChunk.Equals(spatialData.CurrentChunk))
            {
                spatialData.PreviousChunk = spatialData.CurrentChunk;
                spatialData.CurrentChunk = currentChunk;
                _spatialGrid.MoveEntity(entityId, spatialData.PreviousChunk, spatialData.CurrentChunk);
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

        _world.GetPosition(entity).Value = message.Transform.Position;
        _world.GetRotation(entity).Value = message.Transform.Rotation;
        _world.GetScale(entity).Value = message.Transform.Scale;
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
        var writer = ReusableLinkedArrayBufferWriterPool.Rent();
        if (_spatialGrid.TryGetDeltaInfo(chunkCoord, out var added, out var removed))
        {
            var addedIds = (added ?? []).Select(x => (NetEntId)x).ToList();
            var removedIds = (removed ?? []).Select(x => (NetEntId)x).ToList();
            new ChunkDeltaPacket(chunkCoord, addedIds, removedIds).Serialize(writer);
        }

        ChunkData<PositionChunkData> positions = new([]);
        using var _ = chunk.UseLock();
        foreach (var entityId in chunk.Entities)
        {
            if (!_world.TryGetPosition(entityId, out var position))
                position = default;

            positions.Data.Add(PositionChunkData.QuantizeFor(chunkCoord, _spatialGrid.GridInfo, position.Value));

            if (!_world.HasPosition(entityId) || !_world.HasRotation(entityId) || !_world.HasScale(entityId))
                continue;
        }

        new ChunkUpdatePacket(chunkCoord, positions).Serialize(writer);

        return new SerializedMessage(writer);
    }
}