using System.Numerics;
using Ajiva.Ecs;
using Common;
using Common.Ecs;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;

namespace Server;

public class DemoLevel(ServerEcsWorld world, SpatialGrid spatialGrid)
{
    private readonly ServerEcsWorld _world = world;
    private readonly SpatialGrid _spatialGrid = spatialGrid;
    private EntityId _rotatingEntity;
    private readonly List<EntityId> _bouncingEntities = [];
    private readonly List<EntityId> _slidingEntities = [];
    private string? _levelName;

    private void ClearLevel()
    {
        //TODO
        _levelName = null;
        //_world.ClearAllEntities();
    }

    public void SetupPlayer(QConnection connection, EntityId entity)
    {
        new LoadLevelPacket(_levelName ?? "", _spatialGrid.GridInfo).Send(connection);
    }

    public void LoadLevel(string levelName)
    {
        ClearLevel();
        _levelName = levelName;

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
        for (var i = 0; i < 10; i++)
        {
            for (var j = 0; j < 10; j++)
            {
                var entity = _world.CreateEntity();
                var position = new Vector3(i * 10, MathF.Floor(( /*f32.Cos(i * 0.1f) + */f32.Sin(j * 0.05f) + 2) * 3),
                    j * 10);

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

        for (var i = 0; i < 8; i++)
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
        }
    }

    public void Update(f32 deltaTime)
    {
        RotateEntity(deltaTime);

        // Update bouncing entities
        UpdateBouncing(deltaTime);

        UpdateSliding(deltaTime);
    }

    private f32 _progress;

    private static T Lerp<T>(T a, T b, T t) where T : INumber<T> => a + (b - a) * t;

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
            _world.UpdatePosition(entityId).Value = pos;

            _world.UpdateScale(entityId).Value = new Vector3(scaleFactor);

            //_world.UpdateRotation(entityId).Value = Quaternion.Identity; // keep upright

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

            var position = _world.GetPosition(entityId).Value;

            // simple bounce logic
            var linearVel = _world.GetVelocity(entityId).Linear;
            if (position.Y <= 0.001)
            {
                linearVel.Y = -linearVel.Y; // Reverse Y velocity on hit
                position.Y = 0; // Reset to ground level
            }
            else
            {
                linearVel.Y -= 9.81f * deltaTime; // Apply gravity
            }

            linearVel.Y = Math.Clamp(linearVel.Y, -20f, 20f);

            // Update position based on velocity
            position += linearVel * deltaTime;

            _world.UpdatePosition(entityId).Value = position;
            _world.UpdateVelocity(entityId).Linear = linearVel;

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
        var angularVel = _world.GetVelocity(_rotatingEntity).Angular;

        // Apply angular velocity
        var angularRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angularVel.Y * deltaTime);
        _world.UpdateRotation(_rotatingEntity).Value *= angularRotation;
        //rotation.Value = Quaternion.Normalize(rotation.Value);
        if (_world.HasSpatialData(_rotatingEntity))
        {
            _spatialGrid.GetChunkData(_world.GetSpatialData(_rotatingEntity).CurrentChunk).MarkDirty();
        }
    }
}