using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ajiva.Ecs;
using Client.Assets;
using Client.Renderer;
using Common.Ecs;
using Common.Networking;
using Common.SpacialPartitioning;

namespace Client.Game;

/// <summary>
/// Client-side game logic using EcsV2 with simple entity management.
/// No spatial partitioning needed since server pre-filters entities within range.
/// </summary>
public class ClientGameLogic(
    ClientEcsWorld world,
    Scene scene,
    EntityAssetRegistry assetRegistry,
    ILogger<ClientGameLogic> logger)
{
    private readonly Scene _scene = scene;
    private SpatialGrid? _grid;
    private readonly EntityAssetRegistry _assetRegistry = assetRegistry;
    private readonly Dictionary<NetEntId, EntityId> _networkToEcs = new();
    private readonly Dictionary<EntityId, NetEntId> _ecsToNetwork = new();
    private readonly ClientEcsWorld _world = world;
    public u64 RenderEntityVersion { get; private set; } = 0;

    public NetEntId? LocalPlayerNetworkId { get; private set; }
    public EntityId? LocalPlayerEntityId { get; private set; }

    public ClientEcsWorld World => _world;
    public int EntityCount => _networkToEcs.Count;

    private void IncrementRenderVersion([CallerMemberName] string caller = "")
    {
        logger.LogDebug("Incrementing RenderEntityVersion due to {Caller}", caller);
        RenderEntityVersion++;
    }

    public void SetLocalPlayerEntityId(NetEntId networkEntityId)
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

        IncrementRenderVersion();
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
        IncrementRenderVersion();
    }

    private void ClearWorld()
    {
        // Clear all mappings
        _networkToEcs.Clear();
        _ecsToNetwork.Clear();

        // Reset local player
        LocalPlayerEntityId = null;
        LocalPlayerNetworkId = null;
        _grid = null;
        _world.ClearAllEntities();

        // Note: EcsWorld doesn't have a clear method, so we'd need to implement that
        // For now, just create a new world
        // TODO: Add ClearAll method to EcsWorld generator
        IncrementRenderVersion();
    }

    public NetEntId? GetNetworkId(EntityId ecsEntity)
    {
        return _ecsToNetwork.TryGetValue(ecsEntity, out var networkId) ? networkId : null;
    }

    public IEnumerable<EntityId> GetAllEntities()
    {
        return World.GetEntitiesWithPosition();
    }


    public void LoadLevel(string levelName)
    {
        _scene.LoadLevel(levelName);
    }

    public void HandleChunkUpdate(ChunkUpdatePacket packet)
    {
        logger.LogTrace("Chunk update received: {ChunkCoord} with {Type}", packet.Coord, packet.Data.GetType());

        if (!GridCheck()) return;
        if (!_grid.TryGetChunk(packet.Coord, out var chunk)) return;
        switch (packet.Data)
        {
            case ChunkData<PositionChunkData> posUpdate:
                foreach (var (id, delta) in chunk.Entities.Zip(posUpdate.Data))
                {
                    if (World.HasPosition(id))
                    {
                        ref var position = ref World.GetPosition(id);
                        position.Value = delta.Restore(packet.Coord, _grid.GridInfo);
                    }
                }

                break;
        }
    }

    public void HandleChunkUnload(ChunkUnloadPacket packet)
    {
        logger.LogTrace("Chunk unload received: {ChunkCoord}", packet.ChunkCoord);
        var anyRemoved = false;
        foreach (var entityId in packet.NetEntIds)
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
            anyRemoved = true;
        }

        if (anyRemoved)
            IncrementRenderVersion();
    }

    public void HandleChunkDelta(ChunkDeltaPacket message)
    {
        logger.LogTrace("Chunk delta received: {ChunkCoord} with {NewCount} new and {RemovedCount} removed entities",
            message.Coord, message.NewIds.Count, message.RemovedIds.Count);

        if (!GridCheck()) return;
        if (!_grid.TryGetChunk(message.Coord, out var chunk)) return;

        if (message.NewIds is { Count: > 0 } newIds)
        {
            foreach (var netEntId in newIds)
            {
                if (_networkToEcs.TryGetValue(netEntId, out var id))
                {
                    chunk.Entities.Add(id);
                }
                else
                {
                    HandleEntityCreated(new EntityCreatedPacket(netEntId, TryGetTransform(netEntId), 1));
                }
            }
        }

        if (message.RemovedIds is { Count: > 0 } removedIds)
        {
            foreach (var entId in removedIds)
            {
                if (!_networkToEcs.TryGetValue(entId, out var local)) continue;
                //HandleEntityDestroyed(new EntityDestroyedPacket(entId));
                chunk.Entities.Remove(local);
            }
        }

        //IncrementRenderVersion();
    }

    private Transform TryGetTransform(NetEntId entId)
    {
        if (!_networkToEcs.TryGetValue(entId, out var entity)) return new Transform();

        if (!_world.TryGetPosition(entity, out var pos))
            pos = default;
        if (!_world.TryGetRotation(entity, out var rot))
            rot = default;
        if (!_world.TryGetScale(entity, out var scl))
            scl = default;

        return new Transform
        {
            Position = pos.Value,
            Rotation = rot.Value,
            Scale = scl.Value == Vector3.Zero ? Vector3.One : scl.Value
        };
    }

    public void HandleChunkLoad(ChunkLoadPacket message)
    {
        var entityCount = message.NetEntIds.Count;
        logger.LogTrace("Chunk load received: {ChunkCoord} with {EntityCount} entities",
            message.Coord, entityCount);
        if (!GridCheck()) return;
        if (entityCount == 0)
        {
            if (!_grid.TryGetChunk(message.Coord, out var data) || data is not { Entities.Count: > 0 }) return;
            data.Entities.Clear();
            IncrementRenderVersion();
        }
        else
        {
            var data = _grid.GetChunkData(message.Coord);
            var beforeCount = data.Entities.Count;
            if (beforeCount > 0)
                data.Entities.Clear();

            foreach (var entId in message.NetEntIds)
            {
                HandleEntityCreated(new EntityCreatedPacket(entId, new Transform(), 1));
                data.Entities.Add(_networkToEcs[entId]);
            }

            if (beforeCount > 0)
                IncrementRenderVersion();
        }
    }

    [MemberNotNullWhen(true, nameof(_grid))]
    private bool GridCheck()
    {
        if (_grid is null)
        {
            logger.LogWarning("Spatial grid not initialized. Cannot process chunk load.");
            return false;
        }

        return true;
    }

    public void InitializeSpacialGrid(SpacialGridInfo gridInfo)
    {
        ClearWorld();
        _grid = new SpatialGrid(gridInfo);
    }
}