using System.Diagnostics;
using System.Numerics;
using Client.LayerSystem;
using Common.Assets;
using Common.Ecs;
using Common.Networking;
using Common.SpacialPartitioning;
using Server.Core;
using Server.Management;
using Server.Management.Transactions;

namespace Server.Systems;

public class TransactionSystem(NServerEcs world, IServiceProvider provider) : IServerSystem
{
    private u64 _currentTick;
    private readonly Dictionary<TransactionId, IServerTransaction> _pendingTransactions = new();


    private void ProcessPendingSpawns(u64 currentTick)
    {
        List<TransactionId> toRemove = [];
        foreach (var kvp in _pendingTransactions)
        {
            if (kvp.Value.ExecuteTick > currentTick || kvp.Value.ReadyTick < currentTick) continue;
            
            kvp.Value.Execute(currentTick);
            toRemove.Add(kvp.Key);
        }

        foreach (var id in toRemove)
        {
            _pendingTransactions.Remove(id);
        }
    }


    public void PrepareSpawn(TransactionId tid, PrefabId prefabId, Vector3 min, Vector3 max, float spacing,
        Action<u64> onReady)
    {
        var transaction = _pendingTransactions[tid] =
            new SpawnTransaction(provider.GetRequiredService<ILogger<SpawnTransaction>>(), world)
            {
                Id = tid,
                Prefab = prefabId,
                MinBounds = min,
                MaxBounds = max,
                Spacing = spacing,
            };
        Task.Run(async () =>
        {
            await transaction.Prepare(() => _currentTick);
            onReady(transaction.ReadyTick);
        });
    }

    public void SetSpawnExecutionTick(TransactionId tid, u64 tick)
    {
        if (_pendingTransactions.TryGetValue(tid, out var transaction))
        {
            transaction.ExecuteTick = tick;
        }
    }

    public void TransferEntities(ServerId originalServer, CoreEntityData[] entities)
    {
        world.DoLocked(() =>
        {
            foreach (var entityData in entities)
            {
                var newId = world.CreateEntity();
                world.Position[newId] = new Position { Value = entityData.Transform.Position };
                world.Rotation[newId] = new Rotation { Value = entityData.Transform.Rotation };
                world.Scale[newId] = new Scale { Value = entityData.Transform.Scale };
                world.Velocity[newId] = entityData.Velocity;
                world.EntityType[newId] = new EntityType { TypeId = entityData.EntityTypeId };
                world.NetState[newId] = new NetState
                    { Id = new NetEntId(originalServer, entityData.EntityId), CreatedTick = _currentTick };
            }
        });
    }

    public void UpdateEntity(NetEntId netId, Vector3? pos, Quaternion? rot, Vector3? vel)
    {
        world.DoLocked(() =>
        {
            foreach (var (id, state) in world.NetState)
            {
                if (state.Id == netId)
                {
                    if (pos.HasValue) world.Position[id] = new Position { Value = pos.Value };
                    if (rot.HasValue) world.Rotation[id] = new Rotation { Value = rot.Value };
                    if (vel.HasValue) world.Velocity[id] = new Velocity { };
                    break;
                }
            }
        });
    }

    public void DeleteEntities(NetEntId[] entityIds)
    {
        var idsToDelete = new HashSet<NetEntId>(entityIds);
        world.DoLocked(() =>
        {
            // Collect local IDs to delete
            var toDelete = new List<EntityId>();
            foreach (var (id, state) in world.NetState)
            {
                if (idsToDelete.Contains(state.Id))
                {
                    toDelete.Add(id);
                }
            }

            foreach (var id in toDelete)
            {
                world.DeleteEntity(id);
            }
        });
    }

    public void Tick(UpdateInfo info)
    {
        _currentTick = info.Ticks;
        ProcessPendingSpawns(info.Ticks);
    }
}