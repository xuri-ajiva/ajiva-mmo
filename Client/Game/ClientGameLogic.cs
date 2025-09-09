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
    private readonly EntityAssetRegistry _assetRegistry = assetRegistry;
    private readonly Dictionary<NetEntId, EntityId> _networkToEcs = new();
    private readonly Dictionary<EntityId, NetEntId> _ecsToNetwork = new();
    public u64 RenderEntityVersion { get; private set; } = 0;

    public NetEntId? LocalPlayerNetworkId { get; private set; }
    public EntityId? LocalPlayerEntityId { get; private set; }

    public ClientEcsWorld World { get; } = world;

    public SpatialGrid? Grid { get; private set; }

    public i32 EntityCount => _networkToEcs.Count;

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
                World.UpdatePosition(entity).Value += assetDef.PositionOffset;
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
            World.UpdatePosition(entity).Value = packet.Transform.Position;
        }

        if (World.HasRotation(entity))
        {
            World.UpdateRotation(entity).Value = packet.Transform.Rotation;
        }

        if (World.HasScale(entity))
        {
            World.UpdateScale(entity).Value = packet.Transform.Scale;
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
        Grid = null;
        World.ClearAllEntities();

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
        if (!Grid.TryGetChunk(packet.Coord, out var chunk)) return;
        switch (packet.Data)
        {
            // todo make a universal handler that checks if IndexOverride is used
            case ChunkData<PositionChunkData> posUpdate:
                using (var _ = chunk.UseLock())
                {
                    foreach (var (id, delta) in chunk.Entities.Zip(posUpdate.Data))
                    {
                        if (!World.HasPosition(id)) continue;
                        World.UpdatePosition(id).Value = delta.Restore(packet.Coord, Grid.GridInfo);
                    }

                    break;
                }

            case ChunkData<RotationChunkData> rotUpdate:
                using (var _ = chunk.UseLock())
                {
                    var entEnum = chunk.Entities.GetEnumerator();
                    var entEnumIndex = 0;
                    var i = 0;
                    foreach (var index in rotUpdate.IndexOverride!)
                    {
                        while (entEnumIndex < index)
                        {
                            entEnum.MoveNext();
                            entEnumIndex++;
                        }

                        if (!entEnum.MoveNext()) break;
                        entEnumIndex++;
                        var id = entEnum.Current;
                        if (!World.HasRotation(id)) continue;
                        World.UpdateRotation(id).Value = rotUpdate.Data[i].Restore();
                        i++;
                    }
                }

                break;
            case ChunkData<ScaleChunkData> rotUpdate:
                using (var _ = chunk.UseLock())
                {
                    var entEnum = chunk.Entities.GetEnumerator();
                    var entEnumIndex = 0;
                    var i = 0;
                    foreach (var index in rotUpdate.IndexOverride!)
                    {
                        while (entEnumIndex < index)
                        {
                            entEnum.MoveNext();
                            entEnumIndex++;
                        }

                        if (!entEnum.MoveNext()) break;
                        entEnumIndex++;
                        var id = entEnum.Current;
                        if (!World.HasScale(id)) continue;
                        World.UpdateScale(id).Value = rotUpdate.Data[i].Value;
                        i++;
                    }
                }

                break;
            default:
                return;
        }

        chunk.MarkDirty();
    }

    public void HandleChunkUnload(ChunkUnloadPacket packet)
    {
        logger.LogTrace("Chunk unload received: {ChunkCoord}", packet.ChunkCoord);
        if (!GridCheck()) return;
        var anyRemoved = false;
        if (Grid.TryDeleteChunk(packet.ChunkCoord, out var chunk))
        {
            anyRemoved = true;
            using var _ = chunk.UseLock();
            foreach (var entity in chunk.Entities)
            {
                anyRemoved |= TryDelEntity(entity);
            }

            chunk.Entities.Clear();
        }

        foreach (var entityId in packet.NetEntIds)
        {
            if (!_networkToEcs.Remove(entityId, out var ecsEntity)) continue;
            anyRemoved |= TryDelEntity(ecsEntity);
        }

        if (anyRemoved)
            IncrementRenderVersion();
    }

    private bool TryDelEntity(EntityId entity)
    {
        if (!_ecsToNetwork.Remove(entity, out var netEntId)) return false;
        _networkToEcs.Remove(netEntId);
        World.DeleteEntity(entity);
        // Clear local player reference if this was our entity
        if (LocalPlayerEntityId == entity) LocalPlayerEntityId = null;
        if (LocalPlayerNetworkId == netEntId) LocalPlayerNetworkId = null;
        return true;
    }

    public void HandleChunkDelta(ChunkDeltaPacket message)
    {
        logger.LogTrace("Chunk delta received: {ChunkCoord} with {NewCount} new and {RemovedCount} removed entities",
            message.Coord, message.NewIds.Count, message.RemovedIds.Count);

        if (!GridCheck()) return;
        if (!Grid.TryGetChunk(message.Coord, out var chunk)) return;
        using var _ = chunk.UseLock();
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

        chunk.MarkDirty();
        //IncrementRenderVersion();
    }

    private Transform TryGetTransform(NetEntId entId)
    {
        if (!_networkToEcs.TryGetValue(entId, out var entity)) return new Transform();

        if (!World.TryGetPosition(entity, out var pos))
            pos = default;
        if (!World.TryGetRotation(entity, out var rot))
            rot = default;
        if (!World.TryGetScale(entity, out var scl))
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
            if (!Grid.TryGetChunk(message.Coord, out var chunk) || chunk is not { Entities.Count: > 0 }) return;
            using var _ = chunk.UseLock();
            chunk.Entities.Clear();
            IncrementRenderVersion();
            chunk.MarkDirty();
        }
        else
        {
            var chunk = Grid.GetChunkData(message.Coord);
            using var _ = chunk.UseLock();
            var beforeCount = chunk.Entities.Count;
            if (beforeCount > 0)
                chunk.Entities.Clear();

            foreach (var entId in message.NetEntIds)
            {
                HandleEntityCreated(new EntityCreatedPacket(entId, new Transform(), 1));
                chunk.Entities.Add(_networkToEcs[entId]);
            }

            if (beforeCount > 0)
                IncrementRenderVersion();
            chunk.MarkDirty();
        }
    }

    [MemberNotNullWhen(true, nameof(Grid))]
    private bool GridCheck()
    {
        if (Grid is null)
        {
            logger.LogWarning("Spatial grid not initialized. Cannot process chunk load.");
            return false;
        }

        return true;
    }

    public void InitializeSpacialGrid(SpacialGridInfo gridInfo)
    {
        ClearWorld();
        Grid = new SpatialGrid(gridInfo);
    }
}