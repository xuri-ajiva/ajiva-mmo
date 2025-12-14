using System.Diagnostics;
using System.Numerics;
using Common.Assets;
using Common.Ecs;
using Common.Networking;

namespace Server.Management.Transactions;

public class SpawnTransaction(ILogger<SpawnTransaction> logger, NServerEcs ecs) : IServerTransaction
{
    public TransactionId Id { get; init; }
    public u64 ReadyTick { get; private set; } = u64.MaxValue;
    public u64 ExecuteTick { get; set; } = u64.MaxValue;

    public void Execute(u64 currentTick)
    {
        if (!AssetRegistry.Prefabs.TryGetValue(Prefab, out var prefab))
        {
            logger.LogWarning("Prefab {PrefabId} not found for transaction {TransactionId}", Prefab, Id);
            // how did we arrive here?
            throw new UnreachableException(
                "Prefab not found for spawn transaction, it should be present once we said ready");
        }

        var worldWrapper = new GameWorldWrapper(ecs);

        logger.LogInformation("Spawn transaction {TransactionId}", Id);

        // TODO use a proper generator system
        var count = 0;
        ecs.DoLocked(() =>
        {
            // Simple grid spawn logic
            // todo optimize with spatial partitioning and actual region we own
            for (float x = MinBounds.X; x <= MaxBounds.X; x += Spacing)
            {
                for (float z = MinBounds.Z; z <= MaxBounds.Z; z += Spacing)
                {
                    // Check if point is in our region
                    //if (AssignedRegion.Contains(new Vector2(x, z)))
                    {
                        // Use Y from bounds or raycast? For now use MinBounds.Y
                        var pos = new Vector3(x, MinBounds.Y, z);
                        var entity = prefab.Generator(worldWrapper, pos, Quaternion.Identity);
                        var chunkCoord = ecs.Grid.GetChunkCoord(pos, 0);
                        var spatialChunk = ecs.Grid.GetChunkData(chunkCoord);
                        ecs.Spacial.Add(entity,
                            new SpacialData { CurrentChunk = chunkCoord, PreviousChunk = chunkCoord });
                        spatialChunk.Add(entity);
                        spatialChunk.MarkDirty();
                        count++;
                    }
                }
            }
        });

        logger.LogInformation("Spawned {EntityCount} instances of prefab {PrefabId}", count, Prefab);
        logger.LogInformation("Executed spawn transaction {TransactionId} at tick {Tick}", Id, currentTick);
    }

    public async Task Prepare(Func<u64> tickAccessor)
    {
        // TODO Load asset to ensure it's available
        // assetSystem.LoadPrefabsRecursively(root: prefabId);
        await Task.Delay(Random.Shared.Next(200, 1000)); // Simulate some loading time
        logger.LogInformation("Prepared spawn transaction {TransactionId} for prefab {PrefabId}", Id, Prefab);
        ReadyTick = tickAccessor() + 10;
    }

    public required PrefabId Prefab;
    public required Vector3 MinBounds;
    public required Vector3 MaxBounds;
    public required f32 Spacing;
}