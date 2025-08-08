using System.Numerics;
using Common.EcsV2;
using Common.Networking;
using Common.SpacialPartitioning;
using NetworkPackets;
using Riptide;

namespace Server;

public class ServerGameLogic
{
    private readonly ServerEcsWorld _world = new();
    private readonly SpatialGrid _spatialGrid = new(32);
    private readonly Riptide.Server _server;
    private readonly Dictionary<Connection, EntityId> _playerEntities = new();
    private readonly Dictionary<Connection, HashSet<EntityId>> _visibleEntities = new();
    private EntityId _rotatingEntity;
    private List<EntityId> _bouncingEntities = new();
    private string? _levelName;
    private const float UpdateRange = 64.0f; // Only send updates to clients within 32 units

    public ServerGameLogic(Riptide.Server server)
    {
        _server = server;
        LoadLevel("Sponza");
    }

    private void LoadLevel(string levelName)
    {
        _levelName = levelName;
        _playerEntities.Clear();
        _visibleEntities.Clear();

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
        _spatialGrid.MoveEntity(_rotatingEntity.Value, Vector3.Zero, 0, pos.Value, pos.Level);

        // Create some static test entities
        for (var i = 0; i < 10; i++)
        {
            for (var j = 0; j < 10; j++)
            {
                var entity = _world.CreateEntity();
                var position = new Vector3(i * 3, (f32.Cos(i) + f32.Sin(j) + 2) * 10, j * 3f);

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
                    Value = Vector3.One
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
                _spatialGrid.MoveEntity(entity.Value, Vector3.Zero, 0, position, 0);
                _bouncingEntities.Add(entity);
            }
        }
    }

    public void CreatePlayerEntity(Connection connection)
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
        _spatialGrid.MoveEntity(entity.Value, Vector3.Zero, 0, spawnPosition, 0);

        // Track player entity
        _playerEntities[connection] = entity;
        _visibleEntities[connection] = [];

        // Send entity creation to all players within range
        var position = _world.GetPosition(entity);
        var entityType = _world.GetEntityType(entity);
        var rotation = _world.GetRotation(entity);
        var scale = _world.GetScale(entity);

        SendToPlayersInRange(position.Value, position.Level,
            new EntityCreatedPacket((u32)entity.Value, CreateTransformFromComponents(position, rotation, scale),
                entityType.TypeId),
            MessageSendMode.Reliable);

        // Send assignment to the player
        new EntityAssignedPacket((u32)entity.Value).Send(connection, MessageSendMode.Reliable);

        // Update visibility for this player
        UpdatePlayerVisibility(connection, entity);
    }

    public void Update(float deltaTime)
    {
        // Update rotating entity
        if (_world.HasVelocity(_rotatingEntity))
        {
            ref var rotation = ref _world.GetRotation(_rotatingEntity);
            var velocity = _world.GetVelocity(_rotatingEntity);

            // Apply angular velocity
            var angularRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, velocity.Angular.Y * deltaTime);
            rotation.Value *= angularRotation;
            //rotation.Value = Quaternion.Normalize(rotation.Value);

            // Send updates to players in range
            var position = _world.GetPosition(_rotatingEntity);
            var scale = _world.GetScale(_rotatingEntity);
            var transform = CreateTransformFromComponents(position, rotation, scale);

            SendToPlayersInRange(position.Value, position.Level,
                new EntityUpdatePacket((u32)_rotatingEntity.Value, transform),
                MessageSendMode.Unreliable);
        }

        // Update bouncing entities
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

            // Send updates to players in range
            var rotation = _world.GetRotation(entityId);
            var scale = _world.GetScale(entityId);
            var transform = CreateTransformFromComponents(position, rotation, scale);

            SendToPlayersInRange(position.Value, position.Level,
                new EntityUpdatePacket((u32)entityId.Value, transform),
                MessageSendMode.Unreliable);
        }

        // Update spatial partitioning for moved entities
        UpdateSpatialPartitioning();

        // Update player visibility based on spatial chunks
        UpdateAllPlayerVisibility();
    }

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
                _spatialGrid.MoveEntity(entityId.Value,
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
        }
    }

    private void UpdatePlayerVisibility(Connection connection, EntityId playerEntity)
    {
        //if (!_playerEntities.TryGetValue(connection, out var playerEntity)) return;
        if (!_world.TryGetPosition(playerEntity, out var playerPosition)) return;

        var visibleEntities = _visibleEntities[connection];
        var newlyVisible = new HashSet<EntityId>();

        // Find all entities within range
        // todo query spatial grid for chunks in range instead of iterating all entities
        foreach (var entityId in _world.GetEntitiesWithPosition())
        {
            if (entityId == playerEntity) continue; // Don't include self

            var entityPosition = _world.GetPosition(entityId);

            // Check if in same level and within range
            if (entityPosition.Level == playerPosition.Level)
            {
                var distance = Vector3.Distance(playerPosition.Value, entityPosition.Value);
                if (distance <= UpdateRange)
                {
                    newlyVisible.Add(entityId);

                    // If newly visible, send creation packet
                    if (!visibleEntities.Contains(entityId))
                    {
                        var entityType = _world.GetEntityType(entityId);
                        var rotation = _world.HasRotation(entityId)
                            ? _world.GetRotation(entityId)
                            : new Rotation { Value = Quaternion.Identity };
                        var scale = _world.HasScale(entityId)
                            ? _world.GetScale(entityId)
                            : new Scale { Value = Vector3.One };
                        var transform = CreateTransformFromComponents(entityPosition, rotation, scale);

                        new EntityCreatedPacket((u32)entityId.Value, transform, entityType.TypeId)
                            .Send(connection, MessageSendMode.Reliable);
                    }
                }
            }
        }

        // Find entities that are no longer visible
        var noLongerVisible = visibleEntities.Except(newlyVisible).ToList();
        foreach (var entityId in noLongerVisible)
        {
            new EntityDestroyedPacket((u32)entityId.Value).Send(connection, MessageSendMode.Reliable);
        }

        // Update visibility set
        _visibleEntities[connection] = newlyVisible;
    }

    private void SendToPlayersInRange<T>(Vector3 position, i32 level, T packet, MessageSendMode sendMode)
        where T : IPacket
    {
        foreach (var (connection, playerEntity) in _playerEntities)
        {
            if (!_world.HasPosition(playerEntity)) continue;

            var playerPosition = _world.GetPosition(playerEntity);

            // Check if in same level and within range
            if (playerPosition.Level == level)
            {
                var distance = Vector3.Distance(playerPosition.Value, position);
                if (distance <= UpdateRange)
                {
                    packet.Send(connection, sendMode);
                }
            }
        }
    }

    public void SendWorldState(Connection connection)
    {
        new LoadLevelPacket(_levelName ?? "").Send(connection, MessageSendMode.Reliable);

        if (!_playerEntities.TryGetValue(connection, out var playerEntity)) return;
        if (!_world.HasPosition(playerEntity)) return;

        var playerPosition = _world.GetPosition(playerEntity);
        var entitiesInRange = new List<(EntityId entityId, Transform transform, u32 typeId)>();

        // Collect all entities within range
        foreach (var entityId in _world.GetEntitiesWithPosition())
        {
            if (entityId == playerEntity) continue; // Don't include the player's own entity

            var entityPosition = _world.GetPosition(entityId);

            // Check if in same level and within range
            if (entityPosition.Level == playerPosition.Level)
            {
                var distance = Vector3.Distance(playerPosition.Value, entityPosition.Value);
                if (distance <= UpdateRange)
                {
                    var entityType = _world.GetEntityType(entityId);
                    var rotation = _world.HasRotation(entityId)
                        ? _world.GetRotation(entityId)
                        : new Rotation { Value = Quaternion.Identity };
                    var scale = _world.HasScale(entityId)
                        ? _world.GetScale(entityId)
                        : new Scale { Value = Vector3.One };
                    var transform = CreateTransformFromComponents(entityPosition, rotation, scale);

                    entitiesInRange.Add((entityId, transform, entityType.TypeId));
                }
            }
        }

        new BeginWorldStatePacket((u32)entitiesInRange.Count)
            .Send(connection, MessageSendMode.Reliable);

        foreach (var (entityId, transform, typeId) in entitiesInRange)
        {
            new EntityCreatedPacket((u32)entityId.Value, transform, typeId)
                .Send(connection, MessageSendMode.Reliable);
        }
    }

    public void HandleEntityUpdate(EntityUpdatePacket message)
    {
        var entity = new EntityId((i32)message.EntityId);
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
                _spatialGrid.MoveEntity(entity.Value, oldPosition, oldLevel, position.Value, position.Level);
                spatialData.PreviousChunk = spatialData.CurrentChunk;
                spatialData.CurrentChunk = newChunk;
                spatialData.HasMoved = true;
            }
        }

        SendToPlayersInRange(position.Value, position.Level, message, MessageSendMode.Unreliable);
    }

    public void RemovePlayer(Connection connection)
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
            _playerEntities.Remove(connection);
            _visibleEntities.Remove(connection);

            // Notify other players in range
            var position = _world.HasPosition(playerEntity) ? _world.GetPosition(playerEntity) : new Position();
            SendToPlayersInRange(position.Value, position.Level,
                new EntityDestroyedPacket((u32)playerEntity.Value),
                MessageSendMode.Reliable);
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
}
