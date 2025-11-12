using System.Numerics;
using Ajiva.Ecs;
using Common;
using Common.Ecs;
using Common.Networking;
using Common.SpacialPartitioning;
using Serilog;

namespace Server;

public class DemoLevel(
    ServerEcsWorld world,
    SpatialGrid spatialGrid,
    ServerId localServerId,
    RegionAssignment assignedRegion)
{
    private readonly ServerEcsWorld _world = world;
    private readonly SpatialGrid _spatialGrid = spatialGrid;
    private readonly ServerId _localServerId = localServerId;
    private EntityId _rotatingEntity;
    private readonly List<EntityId> _bouncingEntities = [];
    private readonly List<EntityId> _slidingEntities = [];
    private readonly RegionAssignment _region = assignedRegion;

    private void ClearLevel()
    {
        _world.ClearAllEntities();
    }

    /// <summary>
    /// Load level within the assigned region
    /// </summary>
    public void LoadLevel()
    {
        ClearLevel();

        var chunkSize = _spatialGrid.GridInfo.ChunkSize;

        // Find the chunk coordinates closest to the world origin (0,0) that are within the region's bounds.
        var closestChunkX = Math.Clamp(0, _region.MinChunk.X, _region.MaxChunk.X);
        var closestChunkZ = Math.Clamp(0, _region.MinChunk.Z, _region.MaxChunk.Z);
        var closestChunk = new Vector3(closestChunkX, _region.MinChunk.Y, closestChunkZ);

        const i32 chunks = 16;
        var desiredMinChunk = new Vector3(closestChunk.X - chunks, _region.MinChunk.Y, closestChunk.Z - chunks);
        var desiredMaxChunk = new Vector3(closestChunk.X + chunks, _region.MaxChunk.Y, closestChunk.Z + chunks);

        // Clamp the desired 3x3 area to the actual region bounds. This handles regions smaller than 3x3.
        var placementMinChunk = new Vector3(
            Math.Max(_region.MinChunk.X, desiredMinChunk.X),
            _region.MinChunk.Y,
            Math.Max(_region.MinChunk.Z, desiredMinChunk.Z)
        );

        var placementMaxChunk = new Vector3(
            Math.Min(_region.MaxChunk.X, desiredMaxChunk.X),
            _region.MaxChunk.Y,
            Math.Min(_region.MaxChunk.Z, desiredMaxChunk.Z)
        );

        // --- Place the Rotating Box ---

        // Calculate the world center of the single chunk closest to the origin.
        var centerChunkWorld = new Vector3(
            (closestChunk.X + 0.5f) * chunkSize.X,
            (closestChunk.Y + 0.5f) * chunkSize.Y,
            (closestChunk.Z + 0.5f) * chunkSize.Z
        );

        // Create a rotating box at the center of that chunk.
        _rotatingEntity = CreateOwnedEntity(new Position(centerChunkWorld + new Vector3(0, 5, 0)), 1,
            new Velocity { Angular = Vector3.UnitZ });
        Log.Information("Placed rotating entity {EntityId} at {Position} in chunk {ChunkCoord}",
            _rotatingEntity, centerChunkWorld, closestChunk);


        // Calculate world coordinates from the determined placement chunk area.
        var minWorld = new Vector3(
            placementMinChunk.X * chunkSize.X,
            placementMinChunk.Y * chunkSize.Y,
            placementMinChunk.Z * chunkSize.Z
        );

        var maxWorld = new Vector3(
            (placementMaxChunk.X + 1) * chunkSize.X,
            (placementMaxChunk.Y + 1) * chunkSize.Y,
            (placementMaxChunk.Z + 1) * chunkSize.Z
        );

        var regionWidth = maxWorld.X - minWorld.X;
        var regionDepth = maxWorld.Z - minWorld.Z;
        var spacing = 2.0f;

        var entitiesX = (i32)(regionWidth / spacing);
        var entitiesZ = (i32)(regionDepth / spacing);

        for (var i = 0; i < entitiesX; i++)
        {
            for (var j = 0; j < entitiesZ; j++)
            {
                var entity = CreateOwnedEntity(new Position(new Vector3(
                    minWorld.X + i * spacing,
                    chunkSize.Y * 10,
                    minWorld.Z + j * spacing
                )), 1, new Velocity());
                _bouncingEntities.Add(entity);
            }
        }
    }

    private EntityId CreateOwnedEntity(Position position, u32 typeId, Velocity velocity)
    {
        var entity = _world.CreateEntity();

        _world.AddPosition(entity, position);
        _world.AddRotation(entity, new Rotation
        {
            Value = Quaternion.Identity
        });
        _world.AddScale(entity, new Scale
        {
            Value = Vector3.One
        });
        _world.AddVelocity(entity, velocity);
        _world.AddEntityType(entity, new EntityType { TypeId = typeId });

        // Add to spatial grid
        var chunk = _spatialGrid.GetChunkCoord(position.Value, 0);
        _world.AddSpatialData(entity, new SpatialData
        {
            CurrentChunk = chunk,
            PreviousChunk = chunk,
        });

        // Mark as owned by this server
        _world.AddEntityOwnership(entity, new EntityOwnership
        {
            OwnerServerId = _localServerId,
            IsProxy = false,
            IsInProxyRange = false,
            LastSyncTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        return entity;
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

            const f32 minY = 1f;
            // simple bounce logic
            var linearVel = _world.GetVelocity(entityId).Linear;
            if (position.Y < minY)
            {
                linearVel.Y = -linearVel.Y; // Reverse Y velocity on hit
                position.Y = minY; // Reset to ground level
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