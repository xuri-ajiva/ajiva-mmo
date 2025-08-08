using System.Diagnostics;
using System.Numerics;
using Client.Renderer;
using Common.EcsV2;
using Common.Networking;

namespace Client.Game;

/// <summary>
/// Client-side game logic using EcsV2 with simple entity management.
/// No spatial partitioning needed since server pre-filters entities within range.
/// </summary>
public class ClientGameLogic(ClientEcsWorld world, Scene scene, ILogger<ClientGameLogic> logger)
{
    private readonly Scene _scene = scene;
    private readonly Dictionary<u32, EntityId> _networkToEcs = new();
    private readonly Dictionary<EntityId, u32> _ecsToNetwork = new();
    private readonly Dictionary<EntityId, long> _staleEntities = new();
    private readonly ClientEcsWorld _world = world;
    public u64 RenderEntityVersion { get; private set; } = 0;

    public u32? LocalPlayerNetworkId { get; private set; }
    public EntityId? LocalPlayerEntityId { get; private set; }

    public ClientEcsWorld World => _world;

    public void SetLocalPlayerEntityId(u32 networkEntityId)
    {
        LocalPlayerNetworkId = networkEntityId;

        if (_networkToEcs.TryGetValue(networkEntityId, out var ecsEntityId))
        {
            LocalPlayerEntityId = ecsEntityId;
        }
    }

    public void HandleEntityCreated(EntityCreatedPacket packet)
    {
        logger.LogTrace("Entity created: {EntityId}", packet.EntityId);
        if (_networkToEcs.ContainsKey(packet.EntityId))
        {
            HandleEntityUpdate(new EntityUpdatePacket(packet.EntityId, packet.Transform));
            return;
        }

        var entity = World.CreateEntity();

        // Store network <-> ECS mapping
        _networkToEcs[packet.EntityId] = entity;
        _ecsToNetwork[entity] = packet.EntityId;

        World.AddRotation(entity, new Rotation { Value = packet.Transform.Rotation });
        World.AddPosition(entity, new Position { Value = packet.Transform.Position });
        World.AddScale(entity, new Scale { Value = packet.Transform.Scale });
        World.AddRenderMesh(entity, new RenderMesh { MeshId = packet.TypeId });

        // Update local player reference if this is our entity
        if (LocalPlayerNetworkId == packet.EntityId)
        {
            LocalPlayerEntityId = entity;
        }

        RenderEntityVersion++;
    }

    public void HandleEntityUpdate(EntityUpdatePacket packet)
    {
        if (!_networkToEcs.TryGetValue(packet.EntityId, out var entity))
            return; // Entity not found

        // Update components
        if (World.HasPosition(entity))
        {
            ref var position = ref World.GetPosition(entity);
            position.Value = packet.Transform.Position;
        }

        if (World.HasRotation(entity))
        {
            ref var rotation = ref World.GetRotation(entity);
            rotation.Value = packet.Transform.Rotation;
        }

        if (World.HasScale(entity))
        {
            ref var scale = ref World.GetScale(entity);
            scale.Value = packet.Transform.Scale;
        }
    }

    public void HandleEntityDestroyed(EntityDestroyedPacket packet)
    {
        logger.LogTrace("Entity destroyed: {EntityId}", packet.EntityId);
        if (!_networkToEcs.TryGetValue(packet.EntityId, out var entity))
            return;

        // Clean up mappings
        _networkToEcs.Remove(packet.EntityId);
        _ecsToNetwork.Remove(entity);

        // Clear local player reference if this was our entity
        if (LocalPlayerNetworkId == packet.EntityId)
        {
            LocalPlayerEntityId = null;
            LocalPlayerNetworkId = null;
        }

        World.DeleteEntity(entity);
        RenderEntityVersion++;
    }

    public void HandleBeginWorldState(BeginWorldStatePacket packet)
    {
        ClearWorld();
        RenderEntityVersion++;
    }

    private void ClearWorld()
    {
        // Clear all mappings
        _networkToEcs.Clear();
        _ecsToNetwork.Clear();

        // Reset local player
        LocalPlayerEntityId = null;

        // Note: EcsWorld doesn't have a clear method, so we'd need to implement that
        // For now, just create a new world
        // TODO: Add ClearAll method to EcsWorld generator
        RenderEntityVersion++;
    }

    /// <summary>
    /// Get the network ID for an ECS entity (for sending updates to server)
    /// </summary>
    public u32? GetNetworkId(EntityId ecsEntity)
    {
        return _ecsToNetwork.TryGetValue(ecsEntity, out var networkId) ? networkId : null;
    }

    /// <summary>
    /// Get the ECS entity for a network ID
    /// </summary>
    public EntityId? GetEcsEntity(u32 networkId)
    {
        return _networkToEcs.TryGetValue(networkId, out var ecsEntity) ? ecsEntity : null;
    }

    /// <summary>
    /// Get all entities for rendering/UI
    /// </summary>
    public IEnumerable<EntityId> GetAllEntities()
    {
        return World.GetEntitiesWithPosition();
    }

    /// <summary>
    /// Find nearest entity to a position (for selection, etc.)
    /// Simple linear search since client only has ~100 entities max
    /// </summary>
    public EntityId? FindNearestEntity(Vector3 position, float maxDistance = float.MaxValue)
    {
        EntityId? nearest = null;
        float nearestDist = maxDistance;

        foreach (var entityId in World.GetEntitiesWithPosition())
        {
            var entityPos = World.GetPosition(entityId);
            var dist = Vector3.Distance(position, entityPos.Value);

            if (dist < nearestDist)
            {
                nearest = entityId;
                nearestDist = dist;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Get entities within a radius (for area effects, etc.)
    /// </summary>
    public IEnumerable<EntityId> GetEntitiesInRadius(Vector3 center, float radius)
    {
        var radiusSquared = radius * radius;

        foreach (var entityId in World.GetEntitiesWithPosition())
        {
            var entityPos = World.GetPosition(entityId);
            var distSquared = Vector3.DistanceSquared(center, entityPos.Value);

            if (distSquared <= radiusSquared)
            {
                yield return entityId;
            }
        }
    }

    public void LoadLevel(string levelName)
    {
        _scene.LoadLevel(levelName);
    }
}
