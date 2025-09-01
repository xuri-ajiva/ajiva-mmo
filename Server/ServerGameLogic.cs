using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using Ajiva.Ecs;
using Common;
using Common.Ecs;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;
using NetworkPackets;

namespace Server;

public class ServerGameLogic
{
    private readonly ServerEcsWorld _world = new();
    private readonly SpatialGrid _spatialGrid = new(8);
    private readonly QServer _server;
    private readonly ConcurrentDictionary<QConnection, EntityId> _playerEntities = new();
    private readonly Dictionary<QConnection, HashSet<ChunkCoord>> _visibleChunks = new();
    private EntityId _rotatingEntity;
    private readonly List<EntityId> _bouncingEntities = [];
    private readonly List<EntityId> _slidingEntities = [];
    private string? _levelName;
    private const int MaxUpdateRangeChunks = 2; //  Range in chunks (4 chunks = 4*32 = 128 world units)
    private const int MinUpdateRangeChunks = 1; // Minimum range in chunks
    private int _currentUpdateRangeChunks = 8;
    private int _rangeStep;
    private const int MaxRangeSteps = 100;

    private void UpdateUpdateRange()
    {
        return;
        var t = (f32)_rangeStep / MaxRangeSteps; // 0..1
        _currentUpdateRangeChunks = (int)(MinUpdateRangeChunks + t * (MaxUpdateRangeChunks - MinUpdateRangeChunks));
        _rangeStep = (_rangeStep + 1) % (MaxRangeSteps + 1);
    }

    public ServerGameLogic(QServer server)
    {
        _server = server;
        LoadLevel("Sponza");
    }

    private void LoadLevel(string levelName)
    {
        _levelName = levelName;
        _playerEntities.Clear();
        _visibleChunks.Clear();

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
            CurrentChunk = chunk,
            PreviousChunk = chunk,
            HasMoved = false
        });
        _spatialGrid.MoveEntity(_rotatingEntity, Vector3.Zero, 0, pos.Value, pos.Level);

        // Create some static test entities
        for (var i = 0; i < 100; i++)
        {
            for (var j = 0; j < 100; j++)
            {
                var entity = _world.CreateEntity();
                var position = new Vector3(i, (f32.Cos(i * 0.1f) + f32.Sin(j * 0.1f) + 2) * 3, j);

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
                    CurrentChunk = chunk,
                    PreviousChunk = chunk,
                    HasMoved = false
                });
                _spatialGrid.MoveEntity(entity, Vector3.Zero, 0, position, 0);
                _bouncingEntities.Add(entity);
            }
        }

        for (var i = 0; i < 10; i++)
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
                CurrentChunk = chunk,
                PreviousChunk = chunk,
                HasMoved = false
            });
            _spatialGrid.MoveEntity(entity, Vector3.Zero, 0, position, 0);
            _slidingEntities.Add(entity);
        }
    }

    public void CreatePlayerEntity(QConnection connection)
    {
        var entity = _world.CreateEntity();
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
            CurrentChunk = chunk,
            PreviousChunk = chunk,
            HasMoved = false
        });
        _spatialGrid.MoveEntity(entity, Vector3.Zero, 0, spawnPosition, 0);

        // Track player entity
        _playerEntities[connection] = entity;
        _visibleChunks[connection] = [];

        // Send entity creation to all players within range
        var position = _world.GetPosition(entity);
        var entityType = _world.GetEntityType(entity);
        var rotation = _world.GetRotation(entity);
        var scale = _world.GetScale(entity);

        using (var msg = new EntityCreatedPacket(entity,
                   CreateTransformFromComponents(position, rotation, scale), entityType.TypeId).ToMessage())
        {
            connection.Send(msg); // client needs to know about its own entity immediately
            SendToPlayersInRange(position.Value, position.Level, msg);
        }

        // Send assignment to the player
        using (var msg = new EntityAssignedPacket(entity).ToMessage())
            connection.Send(msg);
    }

    public void Update(f32 deltaTime)
    {
        using var update = Instrumentation.Trace(ActivityKind.Server);

        UpdateUpdateRange();

        RotateEntity(deltaTime);

        // Update bouncing entities
        UpdateBouncing(deltaTime);

        UpdateSliding(deltaTime);

        // Update spatial partitioning for moved entities
        using (var act = Instrumentation.NamedTrace("SpatialPartitioning"))
            UpdateSpatialPartitioning();

        // Update player visibility based on spatial chunks
        using (var act = Instrumentation.NamedTrace("PlayerVisibility"))
            UpdateAllPlayerVisibility();

        // Send chunk updates for dirty chunks
        using (var act = Instrumentation.NamedTrace("ChunkUpdates"))
            SendChunkUpdates();
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

    private void UpdateSpatialPartitioning()
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
                // Entity moved to a different chunk
                _spatialGrid.MoveEntity(entityId,
                    Vector3.Zero, spatialData.CurrentChunk.Level, // We don't store old position, so use chunk center
                    position.Value, position.Level);

                spatialData.PreviousChunk = spatialData.CurrentChunk;
                spatialData.CurrentChunk = currentChunk;
                spatialData.HasMoved = true;
            }
        }
    }

    private void UpdateAllPlayerVisibility()
    {
        foreach (var kv in _playerEntities)
        {
            UpdatePlayerVisibility(kv.Key, kv.Value);

            new PingPacket(Stopwatch.GetTimestamp()).Send(kv.Key);
        }
    }

    private void UpdatePlayerVisibility(QConnection connection, EntityId playerEntity)
    {
        if (!_world.TryGetPosition(playerEntity, out var playerPosition)) return;

        var visibleChunks = _visibleChunks[connection];
        var newlyVisibleChunks = new HashSet<ChunkCoord>();

        // Calculate chunk range around player
        var playerChunk = _spatialGrid.GetChunkCoord(playerPosition.Value, playerPosition.Level);

        // Find all chunks within range (using chunk coordinates directly)
        for (var x = playerChunk.X - _currentUpdateRangeChunks; x <= playerChunk.X + _currentUpdateRangeChunks; x++)
        {
            for (var y = playerChunk.Y - _currentUpdateRangeChunks; y <= playerChunk.Y + _currentUpdateRangeChunks; y++)
            {
                for (var z = playerChunk.Z - _currentUpdateRangeChunks;
                     z <= playerChunk.Z + _currentUpdateRangeChunks;
                     z++)
                {
                    var chunkCoord = new ChunkCoord(x, y, z, playerPosition.Level);

                    // Calculate chunk distance (Manhattan distance for simplicity)
                    var chunkDistance = Math.Abs(x - playerChunk.X) + Math.Abs(y - playerChunk.Y) +
                                        Math.Abs(z - playerChunk.Z);

                    if (chunkDistance <= _currentUpdateRangeChunks)
                    {
                        newlyVisibleChunks.Add(chunkCoord);

                        // If newly visible, send full chunk data
                        if (!visibleChunks.Contains(chunkCoord))
                        {
                            //SendFullChunkToPlayer(connection, chunkCoord);
                        }
                    }
                }
            }
        }

        // Find chunks that are no longer visible
        var noLongerVisibleChunks = visibleChunks.Except(newlyVisibleChunks).ToList();
        foreach (var chunkCoord in noLongerVisibleChunks)
        {
            new ChunkUnloadPacket(chunkCoord, _spatialGrid.GetChunkData(chunkCoord).Entities.ToArray())
                .Send(connection);
        }

        // Update visibility set
        _visibleChunks[connection] = newlyVisibleChunks;
    }

    private void SendFullChunkToPlayer(QConnection connection, ChunkCoord chunkCoord)
    {
        // Create a temporary chunk with all entities in this coordinate
        var tempChunk = new SpatialChunk();

        // Find all entities in this chunk coordinate
        foreach (var entityId in _world.GetEntitiesWithPosition())
        {
            if (!_world.TryGetPosition(entityId, out var position)) continue;

            var entityChunk = _spatialGrid.GetChunkCoord(position.Value, position.Level);
            if (entityChunk.Equals(chunkCoord))
            {
                tempChunk.Add(entityId);
            }
        }

        if (tempChunk.Entities.Count > 0)
        {
            var chunkData = BuildChunkData(tempChunk);
            new ChunkUpdatePacket(chunkCoord, chunkData).Send(connection);
        }
    }

    private void SendToPlayersInRange(Vector3 position, i32 level, SerializedMessage packet)
    {
        // Calculate which chunk this position is in
        var chunkCoord = _spatialGrid.GetChunkCoord(position, level);

        foreach (var (connection, visibleChunks) in _visibleChunks)
        {
            // Send to players who can see this chunk
            if (visibleChunks.Contains(chunkCoord))
            {
                connection.Send(packet);
            }
        }
    }

    public void SendWorldState(QConnection connection)
    {
        using (var msg = new LoadLevelPacket(_levelName ?? "").ToMessage())
            connection.Send(msg);

        if (!_playerEntities.TryGetValue(connection, out var playerEntity)) return;
        if (!_world.HasPosition(playerEntity)) return;

        var playerPosition = _world.GetPosition(playerEntity);
        var playerChunk = _spatialGrid.GetChunkCoord(playerPosition.Value, playerPosition.Level);

        // Send chunks within max range to new player
        var chunksToSend = new HashSet<ChunkCoord>();
        for (var x = playerChunk.X - MaxUpdateRangeChunks; x <= playerChunk.X + MaxUpdateRangeChunks; x++)
        {
            for (var y = playerChunk.Y - MaxUpdateRangeChunks; y <= playerChunk.Y + MaxUpdateRangeChunks; y++)
            {
                for (var z = playerChunk.Z - MaxUpdateRangeChunks; z <= playerChunk.Z + MaxUpdateRangeChunks; z++)
                {
                    var chunkCoord = new ChunkCoord(x, y, z, playerPosition.Level);
                    var chunkDistance = Math.Abs(x - playerChunk.X) + Math.Abs(y - playerChunk.Y) +
                                        Math.Abs(z - playerChunk.Z);

                    if (chunkDistance <= MaxUpdateRangeChunks)
                    {
                        chunksToSend.Add(chunkCoord);
                    }
                }
            }
        }

        // Count total entities to send
        var totalEntities = 0;
        foreach (var chunkCoord in chunksToSend)
        {
            foreach (var entityId in _world.GetEntitiesWithPosition())
            {
                if (!_world.TryGetPosition(entityId, out var position)) continue;
                if (entityId == playerEntity) continue; // Don't include the player's own entity

                var entityChunk = _spatialGrid.GetChunkCoord(position.Value, position.Level);
                if (entityChunk.Equals(chunkCoord))
                {
                    totalEntities++;
                }
            }
        }

        new BeginWorldStatePacket((u32)totalEntities).Send(connection);

        // Send chunk data for each chunk
        foreach (var chunkCoord in chunksToSend)
        {
            SendFullChunkToPlayer(connection, chunkCoord);
        }
    }

    public void HandleEntityUpdate(EntityUpdatePacket message)
    {
        var entity = message.Id;
        if (!_world.HasPosition(entity)) return;

        // Update components from the transform
        ref var position = ref _world.GetPosition(entity);
        ref var rotation = ref _world.GetRotation(entity);
        ref var scale = ref _world.GetScale(entity);

        var oldPosition = position.Value;
        var oldLevel = position.Level;

        position.Value = message.Transform.Position;
        rotation.Value = message.Transform.Rotation;
        scale.Value = message.Transform.Scale;

        // Update spatial partitioning if position changed
        if (oldPosition != position.Value || _world.HasSpatialData(entity))
        {
            ref var spatialData = ref _world.GetSpatialData(entity);
            var newChunk = _spatialGrid.GetChunkCoord(position.Value, position.Level);

            if (!newChunk.Equals(spatialData.CurrentChunk))
            {
                _spatialGrid.MoveEntity(entity, oldPosition, oldLevel, position.Value, position.Level);
                spatialData.PreviousChunk = spatialData.CurrentChunk;
                spatialData.CurrentChunk = newChunk;
                spatialData.HasMoved = true;
            }
        }

        using var msg = new EntityUpdatePacket(entity, message.Transform).ToMessage();
        SendToPlayersInRange(position.Value, position.Level, msg);
    }

    public void RemovePlayer(QConnection connection)
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
            _playerEntities.TryRemove(connection, out var playerEntityId);
            _visibleChunks.Remove(connection);

            // Notify other players in range
            var position = _world.HasPosition(playerEntity) ? _world.GetPosition(playerEntity) : new Position();
            using var msg = new EntityDestroyedPacket(playerEntity).ToMessage();
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

        foreach (var (chunkCoord, chunk) in dirtyChunks)
        {
            // Build chunk data
            var chunkData = BuildChunkData(chunk);

            // Send to all players who can see this chunk
            foreach (var (connection, visibleChunks) in _visibleChunks)
            {
                if (visibleChunks.Contains(chunkCoord))
                {
                    new ChunkUpdatePacket(chunkCoord, chunkData).Send(connection);
                }
            }
        }

        // Clear dirty flags after sending updates
        _spatialGrid.ClearDirtyFlags();
    }

    private ChunkData BuildChunkData(SpatialChunk chunk)
    {
        var entityIds = new List<EntityId>();
        var transforms = new List<Transform>();
        var entityTypes = new List<u32>();
        var velocities = new List<Velocity?>();

        foreach (var entityId in chunk.Entities)
        {
            // Skip if entity doesn't exist anymore
            if (!_world.HasPosition(entityId)) continue;

            entityIds.Add(entityId);

            // Get transform components
            var position = _world.GetPosition(entityId);
            var rotation = _world.HasRotation(entityId)
                ? _world.GetRotation(entityId)
                : new Rotation { Value = Quaternion.Identity };
            var scale = _world.HasScale(entityId)
                ? _world.GetScale(entityId)
                : new Scale { Value = Vector3.One };

            transforms.Add(CreateTransformFromComponents(position, rotation, scale));

            // Get entity type
            var entityType = _world.HasEntityType(entityId)
                ? _world.GetEntityType(entityId)
                : new EntityType { TypeId = 0 };
            entityTypes.Add(entityType.TypeId);

            // Get velocity (sparse - null if entity doesn't have velocity)
            var velocity = _world.HasVelocity(entityId)
                ? _world.GetVelocity(entityId)
                : (Velocity?)null;
            velocities.Add(velocity);
        }

        return new ChunkData
        {
            EntityIds = entityIds.ToArray(),
            Transforms = transforms.ToArray(),
            EntityTypes = entityTypes.ToArray(),
            Velocities = velocities.Where(v => v.HasValue).Select(v => v!.Value).ToArray()
        };
    }
}