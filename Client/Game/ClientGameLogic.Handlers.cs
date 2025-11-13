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
        _logger.LogTrace("Chunk update received: {ChunkCoord} with {Type}", packet.Coord, packet.Data.GetType());

        if (!GridCheck()) return;
        if (!Grid.TryGetChunk(packet.Coord, out var chunk)) return;
        switch (packet.Data)
        {
            // todo make a universal handler that checks if IndexOverride is used
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
        var anyRemoved = false;
        if (Grid.TryDeleteChunk(packet.ChunkCoord, out var chunk))
        {
            anyRemoved = true;
            using var _ = chunk.UseLock();
            /*foreach (var entity in chunk.Entities)
            {
                anyRemoved |= TryHandleLocalEntityDeletion(entity);
            }*/
            chunk.Entities.Clear();
        }

        foreach (var netEntId in packet.NetEntIds)
        {
            if (!_networkToEcs.TryGetValue(netEntId, out var ecsEntity)) continue;
            anyRemoved |= TryMarkEntityForDeletion(ecsEntity, netEntId);
        }

        if (anyRemoved)
            IncrementRenderVersion();
    }

    public void HandleChunkDelta(ToClientPackets.ChunkDeltaPacket message)
    {
        _logger.LogTrace("Chunk delta received: {ChunkCoord} with {NewCount} new and {RemovedCount} removed entities",
            message.Coord, message.NewIds.Count, message.RemovedIds.Count);

        if (!GridCheck()) return;
        if (!Grid.TryGetChunk(message.Coord, out var chunk)) return;
        using var _ = chunk.UseLock();
        if (message.NewIds is { Count: > 0 } newIds)
        {
            foreach (var netEntId in newIds)
            {
                if (!TryResurrectEntity(netEntId, out var id))
                {
                    id = CreateEntity(netEntId, 1);
                }

                chunk.Entities.Add(id);
            }
        }

        // todo bug: this dont seam to work properly, entities dont seam to get marked for deletion
        // maybe even the server dont do delta properly?
        // the main problem is the the delete has to be handled before the add in a new chunk
        // bbut we cannot guarantee this as chunk order isn't guaranteed
        if (message.RemovedIds is { Count: > 0 } removedIds)
        {
            foreach (var entId in removedIds)
            {
                if (!_networkToEcs.TryGetValue(entId, out var local)) continue;
                if (TryMarkEntityForDeletion(local, entId))
                {
                    World.NetState.Update(local).GhostChunk = chunk;
                }

                chunk.Entities.Remove(local);
            }
        }

        chunk.MarkDirty();
        //IncrementRenderVersion();
    }

    public void HandleChunkLoad(ToClientPackets.ChunkLoadPacket message)
    {
        var entityCount = message.NetEntIds.Count;
        _logger.LogTrace("Chunk load received: {ChunkCoord} with {EntityCount} entities",
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

            foreach (var netEntId in message.NetEntIds)
            {
                if (!TryResurrectEntity(netEntId, out var id))
                {
                    id = CreateEntity(netEntId, 1);
                }

                chunk.Entities.Add(id);
            }

            if (beforeCount > 0)
                IncrementRenderVersion();
            chunk.MarkDirty();
        }
    }
}