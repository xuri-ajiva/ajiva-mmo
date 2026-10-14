using Common;
using Common.Ecs;
using Common.Networking;

namespace Client.Game;

public partial class ClientGameLogic
{
    public void HandleEntityCreated(ToClientPackets.EntityCreatedPacket packet)
    {
        _logger.LogTrace("Entity created: {EntityId}", packet.Id);
        var id = CreateEntity(packet.Id, packet.TypeId);
        UpdateEntity(id, packet.Transform);

        IncrementRenderVersion();
    }

    public void HandleEntityUpdate(ToClientPackets.EntityUpdatePacket packet)
    {
        if (!_networkToEcs.TryGetValue(packet.Id, out var entity))
            return; // Entity not found

        ref var transform = ref World.Transform.Update(entity);
        transform.Position = packet.Transform.Position;
        transform.Rotation = packet.Transform.Rotation;
        transform.Scale = packet.Transform.Scale;
        World.Position.Update(entity).Value = packet.Transform.Position;
        World.Rotation.Update(entity).Value = packet.Transform.Rotation;
        World.Scale.Update(entity).Value = packet.Transform.Scale;
    }

    public void HandleEntityDestroyed(ToClientPackets.EntityDestroyedPacket packet)
    {
        _logger.LogTrace("Entity destroyed: {EntityId}", packet.Id);
        if (!_networkToEcs.TryGetValue(packet.Id, out var entity))
            return;
        if (TryMarkEntityForDeletion(entity, packet.Id))
            IncrementRenderVersion();
    }

    public void HandleBeginWorldState(ToClientPackets.BeginWorldStatePacket packet)
    {
        ClearWorld();
        IncrementRenderVersion();
    }

    public void LoadLevel(string levelName)
    {
        _scene.LoadLevel(levelName);
    }

    public void HandleChunkUpdate(ToClientPackets.ChunkUpdatePacket packet)
    {
       // _logger.LogTrace("Chunk update received: {ChunkCoord} with {Type}", packet.Coord, packet.Data.GetType());

        if (!GridCheck()) return;
        if (!Grid.TryGetChunk(packet.Coord, out var chunk)) return;
        switch (packet.Data)
        {
            case ChunkData<PositionChunkData> posUpdate:
                using (var _ = chunk.UseLock())
                {
                    using var a = Instrumentation.NamedTrace("PositionChunkData");
                    foreach (var (id, delta) in chunk.Entities.Zip(posUpdate.Data))
                    {
                        if (!World.Transform.Has(id)) continue;
                        var position = delta.Restore(packet.Coord, Grid.GridInfo);
                        World.Transform.Update(id).Position = position;
                        World.Position.Update(id).Value = position;
                    }

                    break;
                }

            case ChunkData<RotationChunkData> rotUpdate:
                using (var _ = chunk.UseLock())
                {
                    using var a = Instrumentation.NamedTrace("RotationChunkData");
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
                        if (!World.Transform.Has(id)) continue;
                        var rotation = rotUpdate.Data[i].Restore();
                        World.Transform.Update(id).Rotation = rotation;
                        World.Rotation.Update(id).Value = rotation;
                        i++;
                    }
                }

                break;

            case ChunkData<ScaleChunkData> rotUpdate:
                using (var _ = chunk.UseLock())
                {
                    using var a = Instrumentation.NamedTrace("ScaleChunkData");
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
                        if (!World.Transform.Has(id)) continue;
                        var scale = rotUpdate.Data[i].Value;
                        World.Transform.Update(id).Scale = scale;
                        World.Scale.Update(id).Value = scale;
                        i++;
                    }
                }

                break;

            default:
                return;
        }

        chunk.MarkDirty();
    }

    public void HandleChunkUnload(ToClientPackets.ChunkUnloadPacket packet)
    {
        _logger.LogTrace("Chunk unload received: {ChunkCoord}", packet.ChunkCoord);
        if (!GridCheck()) return;
        if (Grid.TryDeleteChunk(packet.ChunkCoord, out var chunk))
        {
            using var _ = chunk.UseLock();
            foreach (var id in chunk.Entities.ToArray())
                RemoveChunkEntity(chunk, id);
        }
    }

    public void HandleChunkDelta(ToClientPackets.ChunkDeltaPacket message)
    {
        if (!GridCheck()) return;
        if (!Grid.TryGetChunk(message.Coord, out var chunk)) return;
        using var _ = chunk.UseLock();

        // 1. Process REMOVALS FIRST
        // Fixes the issue where an entity bouncing between chunks in the same tick 
        // would get added, and then immediately marked for deletion.
        if (message.RemovedIds is { Count: > 0 } removedIds)
        {
            foreach (var entId in removedIds)
            {
                SpecialEntity(entId, "RemovedIds in ChunkDelta");
                if (!_networkToEcs.TryGetValue(entId, out var local)) continue;

                RemoveChunkEntity(chunk, local);
            }
        }

        // 2. Process ADDITIONS SECOND
        // Philosophy implementation: All added are immediately resurrected/created.
        if (message.NewIds is { Count: > 0 } newIds)
        {
            foreach (var netEntId in newIds)
            {
                SpecialEntity(netEntId, "NewIds in ChunkDelta");

                if (!TryResurrectEntity(netEntId, out var id))
                {
                    id = CreateEntity(netEntId, 1);
                }

                // Immediately register to the current chunk's bounds
                AddChunkEntity(chunk, id);
            }
        }

        chunk.MarkDirty();
    }

    public void HandleChunkLoad(ToClientPackets.ChunkLoadPacket message)
    {
        var entityCount = message.NetEntIds.Count;
        _logger.LogTrace("Chunk load received: {ChunkCoord} with {EntityCount} entities",
            message.Coord, entityCount);
        if (!GridCheck()) return;

        var chunk = Grid.GetChunkData(message.Coord);
        using var _ = chunk.UseLock();
        foreach (var id in chunk.Entities.ToArray())
            RemoveChunkEntity(chunk, id);

        foreach (var netEntId in message.NetEntIds)
        {
            if (!TryResurrectEntity(netEntId, out var id))
                id = CreateEntity(netEntId, 1);
            AddChunkEntity(chunk, id);
        }
        chunk.MarkDirty();
    }
}
