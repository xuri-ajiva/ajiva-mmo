using System.Diagnostics;
using System.Numerics;
using Ajiva.Ecs;
using Client.Assets;
using Client.Renderer;
using Common.Ecs;
using Common.Networking;
using Interception;

namespace Client.Game;

/// <summary>
/// Client-side game logic using EcsV2 with simple entity management.
/// No spatial partitioning needed since server pre-filters entities within range.
/// </summary>
public class ClientGameLogic(ClientEcsWorld world, Scene scene, EntityAssetRegistry assetRegistry, ILogger<ClientGameLogic> logger)
{
    private readonly Scene _scene = scene;
    private readonly EntityAssetRegistry _assetRegistry = assetRegistry;
    private readonly Dictionary<EntityId, EntityId> _networkToEcs = new();
    private readonly Dictionary<EntityId, EntityId> _ecsToNetwork = new();
    private readonly ClientEcsWorld _world = world;
    public u64 RenderEntityVersion { get; private set; } = 0;

    public EntityId? LocalPlayerNetworkId { get; private set; }
    public EntityId? LocalPlayerEntityId { get; private set; }

    public ClientEcsWorld World => _world;
    public int EntityCount => _networkToEcs.Count;

    public void SetLocalPlayerEntityId(EntityId networkEntityId)
    {
        LocalPlayerNetworkId = networkEntityId;

        if (_networkToEcs.TryGetValue(networkEntityId, out var ecsEntityId))
        {
            LocalPlayerEntityId = ecsEntityId;
        }
    }

    public void HandleEntityCreated(EntityCreatedPacket packet)
    {
        logger.LogTrace("Entity created: {EntityId}", packet.Id);
        if (_networkToEcs.ContainsKey(packet.Id))
        {
            HandleEntityUpdate(new EntityUpdatePacket(packet.Id, packet.Transform));
            return;
        }

        var entity = World.CreateEntity();

        // Store network <-> ECS mapping
        _networkToEcs[packet.Id] = entity;
        _ecsToNetwork[entity] = packet.Id;

        World.AddRotation(entity, new Rotation { Value = packet.Transform.Rotation });
        World.AddPosition(entity, new Position { Value = packet.Transform.Position });
        
        World.AddEntityType(entity, new EntityType { TypeId = packet.TypeId });

        // Add render components based on entity type using asset registry
        if (_assetRegistry.GetAssetDefinition(packet.TypeId) is { } assetDef)
        {
            // Apply asset-specific scale if defined, otherwise use transform scale
            var finalScale = assetDef.Scale == default ? packet.Transform.Scale : assetDef.Scale;
            World.AddScale(entity, new Scale { Value = finalScale });

            // Apply position offset if defined
            if (assetDef.PositionOffset != default)
            {
                ref var pos = ref World.GetPosition(entity);
                pos.Value += assetDef.PositionOffset;
            }

            // Add RenderAsset component for asset streaming
            World.AddRenderAsset(entity, new RenderAsset
            {
                AssetPath = assetDef.MeshPath,
                MaterialPath = assetDef.MaterialPath,
                IsLoaded = false
            });

            // Add RenderMesh component 
            World.AddRenderMesh(entity, new RenderMesh
            {
                MeshId = packet.TypeId,
                EntityId = (u32)entity.Value
            });
        }
        else
        {
            // Fallback: use transform scale and basic render mesh
            World.AddScale(entity, new Scale { Value = packet.Transform.Scale });
            World.AddRenderMesh(entity, new RenderMesh 
            { 
                MeshId = packet.TypeId,
                EntityId = (u32)entity.Value
            });
        }

        // Update local player reference if this is our entity
        if (LocalPlayerNetworkId == packet.Id)
        {
            LocalPlayerEntityId = entity;
        }

        RenderEntityVersion++;
    }

    public void HandleEntityUpdate(EntityUpdatePacket packet)
    {
        if (!_networkToEcs.TryGetValue(packet.Id, out var entity))
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
        logger.LogTrace("Entity destroyed: {EntityId}", packet.Id);
        if (!_networkToEcs.Remove(packet.Id, out var entity))
            return;

        // Clean up mappings
        _ecsToNetwork.Remove(entity);

        // Clear local player reference if this was our entity
        if (LocalPlayerNetworkId == packet.Id)
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

    public void HandleChunkUpdate(ChunkUpdatePacket packet)
    {
        logger.LogTrace("Chunk update received: {ChunkCoord} with {EntityCount} entities", 
            packet.Coord, packet.Data.EntityIds.Length);

        // Process all entities in the chunk
        for (int i = 0; i < packet.Data.EntityIds.Length; i++)
        {
            var networkId = packet.Data.EntityIds[i];
            var transform = packet.Data.Transforms[i];
            var typeId = packet.Data.EntityTypes[i];
            
            // Check if entity already exists
            if (_networkToEcs.TryGetValue(networkId, out var existingEntity))
            {
                // Update existing entity
                UpdateEntityTransform(existingEntity, transform);
            }
            else
            {
                // Create new entity
                CreateEntityFromChunkData(networkId, transform, typeId);
            }
        }

        RenderEntityVersion++;
    }

    public void HandleChunkUnload(ChunkUnloadPacket packet)
    {
        logger.LogTrace("Chunk unload received: {ChunkCoord}", packet.ChunkCoord);
        
        // Note: We don't have a direct way to find entities by chunk coordinate
        // For now, we'll rely on individual entity destroyed packets
        // In a more advanced implementation, we could track entity -> chunk mappings
        foreach (var entityId in packet.EntityIds)
        {
            if (!_networkToEcs.Remove(entityId, out var ecsEntity)) continue;
            
            // Clean up mappings
            _ecsToNetwork.Remove(ecsEntity);

            // Clear local player reference if this was our entity
            if (LocalPlayerNetworkId == entityId)
            {
                LocalPlayerEntityId = null;
                LocalPlayerNetworkId = null;
            }

            World.DeleteEntity(ecsEntity);
        }
        
        RenderEntityVersion++;
    }

    private void CreateEntityFromChunkData(EntityId networkId, Transform transform, u32 typeId)
    {
        var entity = World.CreateEntity();

        // Store network <-> ECS mapping
        _networkToEcs[networkId] = entity;
        _ecsToNetwork[entity] = networkId;

        // Add components
        World.AddPosition(entity, new Position
        {
            Value = transform.Position,
            Level = 0 // Assuming level 0 for now
        });

        World.AddRotation(entity, new Rotation
        {
            Value = transform.Rotation
        });

        World.AddEntityType(entity, new EntityType
        {
            TypeId = typeId
        });

        // Add render components based on entity type
        if (_assetRegistry.GetAssetDefinition(typeId) is { } assetDef)
        {
            // Apply asset-specific scale if defined, otherwise use transform scale
            var finalScale = assetDef.Scale == default ? transform.Scale : assetDef.Scale;
            World.AddScale(entity, new Scale { Value = finalScale });

            // Apply position offset if defined
            var finalPosition = transform.Position + assetDef.PositionOffset;
            if (assetDef.PositionOffset != default)
            {
                // Update position with offset
                ref var pos = ref World.GetPosition(entity);
                pos.Value = finalPosition;
            }

            // Add RenderAsset component for asset streaming
            World.AddRenderAsset(entity, new RenderAsset
            {
                AssetPath = assetDef.MeshPath,
                MaterialPath = assetDef.MaterialPath,
                IsLoaded = false // Will be set to true when ResourceManager loads it
            });

            // Add RenderMesh component 
            World.AddRenderMesh(entity, new RenderMesh
            {
                MeshId = typeId,
                EntityId = (u32)entity.Value // Store reference to entity for rendering
            });

            logger.LogTrace("Entity {NetworkId} assigned render assets: mesh={MeshPath}, material={MaterialPath}", 
                networkId, assetDef.MeshPath, assetDef.MaterialPath);
        }
        else
        {
            // Use transform scale as fallback
            World.AddScale(entity, new Scale { Value = transform.Scale });
            
            logger.LogWarning("No asset definition found for entity type {TypeId} (entity {NetworkId})", 
                typeId, networkId);
            
            // Fallback: create a basic render mesh with TypeId
            World.AddRenderMesh(entity, new RenderMesh
            {
                MeshId = typeId,
                EntityId = (u32)entity.Value
            });
        }

        // Check if this is our local player
        if (LocalPlayerNetworkId == networkId)
        {
            LocalPlayerEntityId = entity;
        }
    }

    private void UpdateEntityTransform(EntityId entity, Transform transform)
    {
        // Update transform components
        if (World.HasPosition(entity))
        {
            ref var position = ref World.GetPosition(entity);
            position.Value = transform.Position;
        }

        if (World.HasRotation(entity))
        {
            ref var rotation = ref World.GetRotation(entity);
            rotation.Value = transform.Rotation;
        }

        if (World.HasScale(entity))
        {
            ref var scale = ref World.GetScale(entity);
            scale.Value = transform.Scale;
        }
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
    public EntityId? GetNetworkId(EntityId ecsEntity)
    {
        return _ecsToNetwork.TryGetValue(ecsEntity, out var networkId) ? networkId : null;
    }

    /// <summary>
    /// Get the ECS entity for a network ID
    /// </summary>
    public EntityId? GetEcsEntity(EntityId networkId)
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
