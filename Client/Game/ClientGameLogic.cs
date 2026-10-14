using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Ajiva.Ecs;
using Client.Assets;
using Client.LayerSystem;
using Client.Renderer;
using Common.Ecs;
using Common.SpacialPartitioning;
using Common.Systems;

namespace Client.Game;

/// <summary>
/// Client-side game logic using EcsV2 with simple entity management.
/// No spatial partitioning needed since server pre-filters entities within range.
/// </summary>
public partial class ClientGameLogic : IDisposable
{
    private readonly Scene _scene;
    private readonly EntityAssetRegistry _assetRegistry;
    private readonly Dictionary<NetEntId, EntityId> _networkToEcs = new();
    private readonly Dictionary<EntityId, HashSet<SpatialChunk>> _chunkMembership = new();

    /*public NetEntId? LocalPlayerNetworkId { get; private set; }
    public EntityId? LocalPlayerEntityId { get; private set; }*/
    private readonly PeriodicUpdater.UpdateHandle _updateHandle;

    private readonly ILogger<ClientGameLogic> _logger;

    public ClientGameLogic(NClientEcsWorld world, Scene scene, EntityAssetRegistry assetRegistry,
        ILogger<ClientGameLogic> logger, PeriodicUpdater updater)
    {
        _logger = logger;
        _scene = scene;
        _assetRegistry = assetRegistry;
        World = world;
        _updateHandle = updater.RegisterUpdate(UpdateCycle.Update, Update);
    }

    private void Update(UpdateInfo updateInfo)
    {
        _lastTick = updateInfo.Ticks;
        if (Grid is null) return;
        var delCount = 0;
        foreach (var pair in World.NetState)
        {
            if (TryCleanUpEntity(pair.Key))
                delCount++;
        }

        if (delCount > 0)
            IncrementRenderVersion("Auto-delete final");
    }


    public NClientEcsWorld World { get; }

    public SpacialGrid? Grid => World.Grid;

    public i32 EntityCount => _networkToEcs.Count;
    private u64 _lastTick;

    private void IncrementRenderVersion([CallerMemberName] string caller = "")
    {
        // _logger.LogDebug("Incrementing RenderEntityVersion due to {Caller}", caller);
        World.RenderEntityVersion++;
    }

    private void ClearWorld()
    {
        _logger.LogWarning("Clearing entire world state");
        // Clear all mappings
        _networkToEcs.Clear();
        _chunkMembership.Clear();

        World.ClearAllEntities();

        // Note: EcsWorld doesn't have a clear method, so we'd need to implement that
        // For now, just create a new world
        // TODO: Add ClearAll method to EcsWorld generator
        IncrementRenderVersion();
    }

    private void SpecialEntity(NetEntId entId, string reason)
    {
        /*if (entId.Server == 4 && entId.Entity.Value == 1451)
        {
            _logger.LogInformation("Tracking special entity {NetEntId} for reason: {Reason}", entId, reason);
        }*/
    }

    public NetEntId? GetNetworkId(EntityId ecsEntity)
    {
        return World.NetState.TryGet(ecsEntity, out var netState) ? netState.Id : null;
    }

    private bool TryHandleLocalEntityDeletion(EntityId entity)
    {
        if (World.NetState.Has(entity))
        {
            World.NetState.Update(entity).DeletedTick = _lastTick;
            return true;
        }

        const i32 maxFind = 10;
        var i = 0;
        Span<NetEntId> keys = stackalloc NetEntId[maxFind];
        foreach (var keyValuePair in _networkToEcs)
        {
            if (keyValuePair.Value != entity) continue;

            keys[i++] = keyValuePair.Key;
            if (i >= maxFind)
            {
                _logger.LogWarning("Exceeded max find limit when deleting local entity {EntityId}", entity);
                break;
            }
        }

        var res = false;
        for (var j = 0; j < i; j++)
        {
            res |= TryDeleteEntity(entity, keys[j]);
        }

        return res;
    }

    private bool TryMarkEntityForDeletion(EntityId entity, NetEntId netEntId)
    {
        SpecialEntity(netEntId, nameof(TryMarkEntityForDeletion));
        if (World.NetState.Has(entity))
        {
            World.NetState.Update(entity).DeletedTick = _lastTick;
        }
        else
        {
            return TryDeleteEntity(entity, netEntId);
        }

        SpecialEntity(netEntId, "Deletion marked");
        return true;
    }

    private bool TryResurrectEntity(NetEntId netId, out EntityId id)
    {
        SpecialEntity(netId, nameof(TryResurrectEntity));
        if (!_networkToEcs.TryGetValue(netId, out id) || !World.NetState.Has(id))
        {
            SpecialEntity(netId, "No existing entity to resurrect");
            return false;
        }

        ref var state = ref World.NetState.Update(id);
        state.DeletedTick = null;
        state.CreatedTick = _lastTick;

        state.GhostChunk = null;
        return true;
    }

    private void AddChunkEntity(SpatialChunk chunk, EntityId id)
    {
        if (!_chunkMembership.TryGetValue(id, out var chunks))
            _chunkMembership[id] = chunks = new(ReferenceEqualityComparer.Instance);
        // Keep server ordering for positional updates, without duplicate delta additions.
        if (chunks.Add(chunk))
            chunk.Entities.Add(id);
    }

    private void RemoveChunkEntity(SpatialChunk chunk, EntityId id)
    {
        chunk.Entities.RemoveAll(entity => entity == id);
        if (!_chunkMembership.TryGetValue(id, out var chunks)) return;
        chunks.Remove(chunk);
        if (chunks.Count != 0) return;
        _chunkMembership.Remove(id);
        if (World.NetState.TryGet(id, out var state))
            TryMarkEntityForDeletion(id, state.Id);
    }


    private bool TryCleanUpEntity(EntityId entityId)
    {
        ref var netState = ref World.NetState.Update(entityId);
        if (!netState.DeletedTick.HasValue || netState.DeletedTick.Value + 2 >= _lastTick) return false;
    
        return TryDeleteEntity(entityId, netState.Id);
    }


    private bool TryDeleteEntity(EntityId id, NetEntId netId)
    {
        SpecialEntity(netId, nameof(TryDeleteEntity));

        if (_chunkMembership.Remove(id, out var chunks))
            foreach (var chunk in chunks)
            {
                using var scope = chunk.UseLock();
                chunk.Entities.RemoveAll(entity => entity == id);
                chunk.MarkDirty();
            }
        _networkToEcs.Remove(netId, out var entity);
        Debug.Assert(entity == id, "EntityId mismatch on deletion");
        World.DeleteEntity(id);
        return true;
    }

    private EntityId CreateEntity(NetEntId netEntId, u32 typeId)
    {
        SpecialEntity(netEntId, nameof(CreateEntity));

        if (_networkToEcs.TryGetValue(netEntId, out var id))
        {
            SpecialEntity(netEntId, "Entity already exists for this NetEntId, returning existing entity");
            return id;
        }

        Debug.Assert(!_networkToEcs.ContainsKey(netEntId), "Entity with the same NetEntId already exists.");
        // Store network -> ECS mapping
        var entity = World.CreateEntity();
        Debug.Assert(!_networkToEcs.ContainsValue(entity), "Entity with the same EntityId already exists.");
        _networkToEcs.Add(netEntId, entity);
        World.NetState.Add(entity, new NetState { Id = netEntId, CreatedTick = _lastTick });
        World.EntityType.Add(entity, new EntityType { TypeId = typeId });

        World.Transform.Add(entity, new Transform());

        // Add render components based on entity type using asset registry
        if (_assetRegistry.GetAssetDefinition(typeId) is { } assetDef)
        {
            World.Transform.Update(entity).Scale *= assetDef.Scale;
            // Apply position offset if defined
            World.Transform.Update(entity).Position += assetDef.PositionOffset;

            // Add RenderAsset component for asset streaming
            World.RenderAsset.Add(entity, new RenderAsset
            {
                AssetPath = assetDef.MeshPath,
                MaterialPath = assetDef.MaterialPath,
                IsLoaded = false
            });
        }

        var transform = World.Transform.Get(entity);
        World.Position.Add(entity, new Position { Value = transform.Position });
        World.Rotation.Add(entity, new Rotation { Value = transform.Rotation });
        World.Scale.Add(entity, new Scale { Value = transform.Scale });

        // Add RenderMesh component 
        World.RenderMesh.Add(entity, new RenderMesh
        {
            MeshId = typeId,
            EntityId = (u32)entity.Value
        });
        IncrementRenderVersion();
        return entity;
    }

    private void UpdateEntity(EntityId entity, Transform transform)
    {
        World.Transform.Update(entity) = transform;
    }

    [MemberNotNullWhen(true, nameof(Grid))]
    private bool GridCheck()
    {
        if (Grid is null)
        {
            _logger.LogWarning("Spatial grid not initialized. Cannot process chunk load.");
            return false;
        }

        return true;
    }

    public void InitializeSpacialGrid(SpacialGridInfo gridInfo)
    {
        if (Grid?.GridInfo == gridInfo) return;
        ClearWorld();
        World.InitGrid(gridInfo);
    }

    public void Dispose()
    {
        _updateHandle.Dispose();
    }
}
