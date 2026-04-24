using System.Diagnostics;
using System.Numerics;
using Common;
using Common.Ecs;
using Common.Networking;
using Common.SpacialPartitioning;
using Common.Systems;
using Interception;
using MemoryPack;
using MemoryPack.Internal;
using NetworkPackets;
using Server.Core;

namespace Server.Systems;

/// <summary>
/// Handles chunk loading/unloading and visibility updates for players
/// </summary>
public class ChunkSystem(
    ILogger<ChunkSystem> logger,
    NServerEcs world,
    GameServerInstanceConfig config)
    : IServerSystem
{
    private ServerId _serverId = ServerId.Invalid;

    private NetworkSystem _networkSystem = null!;

    // Chunk visibility settings
    private const i32 MaxSendRangeChunks = 16;
    private const i32 MinSendRangeChunks = 4;
    private i32 _currentSendRangeChunks = MinSendRangeChunks;
    private i32 _rangeStep;
    private const i32 MaxRangeSteps = 100;

    public Task InitializeAsync(ISystemManager manager, CancellationToken token = default)
    {
        _networkSystem = manager.GetSystem<NetworkSystem>()!;
        logger.LogInformation("Chunk system initialized");
        return Task.CompletedTask;
    }

    public void Tick(UpdateInfo info)
    {
        _serverId = config.Id;
        using var update = Instrumentation.Trace(ActivityKind.Server);

        UpdateSendRange();

        // Update spatial partitioning for moved entities
        using (Instrumentation.NamedTrace("SpatialPartitioning"))
            world.DoLocked(UpdateSpatialPartitioning);

        // Update player visibility based on spatial chunks
        using (Instrumentation.NamedTrace("PlayerVisibility"))
            world.DoLocked(UpdateAllPlayerVisibility);

        // Send chunk updates for dirty chunks
        using (Instrumentation.NamedTrace("ChunkUpdates"))
            world.DoLocked(SendChunkUpdates);
    }

    [TraceInterception]
    public void UpdateSpatialPartitioning()
    {
        // Check all entities with position and spatial data for movement
        foreach (var kv in world.Position)
        {
            if (!world.Spacial.TryGet(kv.Key, out var spacialData)) continue;

            var currentChunk = world.Grid.GetChunkCoord(kv.Value.Value, kv.Value.Level);

            if (currentChunk.Equals(spacialData.CurrentChunk)) continue;

            spacialData.PreviousChunk = spacialData.CurrentChunk;
            spacialData.CurrentChunk = currentChunk;
            world.Grid.MoveEntity(kv.Key, spacialData.PreviousChunk, spacialData.CurrentChunk);
            world.Spacial.Update(kv.Key) = spacialData;
        }
    }

    private void UpdateSendRange()
    {
        var t = (f32)_rangeStep / MaxRangeSteps; // 0..1
        _currentSendRangeChunks = (i32)(MinSendRangeChunks + t * (MaxSendRangeChunks - MinSendRangeChunks));
        _rangeStep = (_rangeStep + 1) % (MaxRangeSteps + 1);
    }


    [TraceInterception]
    public void UpdateAllPlayerVisibility()
    {
        foreach (var kv in _networkSystem._connectionContexts)
        {
            UnloadPlayerChunks(kv.Value);

            new Packets.Ping(Stopwatch.GetTimestamp()).Send(kv.Key);
        }
    }

    [TraceInterception]
    public void UnloadPlayerChunks(ConnectionContext context)
    {
        var playerPosition = context.Position;
        var playerChunk = world.Grid.GetChunkCoord(playerPosition.Value, playerPosition.Level);
        var visibleChunks = context.LoadedChunks;
        foreach (var coord in
                 visibleChunks.Where(chunkCoord => chunkCoord.PosDistanceTo(playerChunk) > MaxSendRangeChunks))
        {
            SendUnloadChunk(context, coord);
            visibleChunks.Remove(coord);
        }
    }

    [TraceInterception]
    public void SendUnloadChunk(ConnectionContext context, ChunkCoord chunkCoord)
    {
        new ToClientPackets.ChunkUnloadPacket(chunkCoord, world.Grid.GetChunkData(chunkCoord).Entities
            .Select(x => new NetEntId(_serverId, x)).ToList()).Send(context.Connection);
    }

    public void SendLoadChunk(ConnectionContext context, ChunkCoord chunkCoord)
    {
        var chunk = world.Grid.GetChunkData(chunkCoord);
        using var _ = chunk.UseLock();
        new ToClientPackets.ChunkLoadPacket(chunkCoord, chunk.Entities
            .Select(x => new NetEntId(_serverId, x)).ToList()).Send(context.Connection);
    }


    private void SendChunkUpdates()
    {
        // Get all dirty chunks from the spatial grid
        var dirtyChunks = world.Grid.GetDirtyChunks().ToList();

        if (dirtyChunks.Count == 0) return;
        List<ConnectionContext> visibleChunksSnapshot = [];
        foreach (var (chunkCoord, chunk) in dirtyChunks)
        {
            //if (chunk.Entities.Count == 0) continue; we still need to delta removed entities
            if (chunk.Entities.Count == 0 &&
                !world.Grid.TryGetDeltaInfo(chunkCoord, out var added, out var removed)) continue;
            visibleChunksSnapshot.Clear();
            foreach (var context in _networkSystem._connectionContexts.Values)
            {
                var playerChunk = world.Grid.GetChunkCoord(context.Position.Value, context.Position.Level);
                var chunkDistance = playerChunk.PosDistanceTo(chunkCoord);
                if (chunkDistance <= _currentSendRangeChunks)
                    visibleChunksSnapshot.Add(context);
            }

            if (visibleChunksSnapshot.Count == 0) continue;

            using var chunkData = BuildChunkData(chunkCoord, chunk);

            // Send to all players who can see this chunk
            foreach (var context in visibleChunksSnapshot)
            {
                if (context.LoadedChunks.Add(chunkCoord))
                {
                    SendLoadChunk(context, chunkCoord);
                }

                context.Connection.Send(chunkData);
            }
        }

        // Clear dirty flags after sending updates
        world.Grid.ClearDirtyFlags();
    }

    private SerializedMessage BuildChunkData(ChunkCoord chunkCoord, SpatialChunk chunk)
    {
        var serializer = new SerializedMessage();
        var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref serializer.Writer, serializer.State);

        if (world.Grid.TryGetDeltaInfo(chunkCoord, out var added, out var removed))
        {
            var addedIds = (added ?? []).Select(x => new NetEntId(_serverId, x)).ToList();
            var removedIds = (removed ?? []).Select(x => new NetEntId(_serverId, x)).ToList();
            new ToClientPackets.ChunkDeltaPacket(chunkCoord, addedIds, removedIds).Serialize(ref writer, false);
        }

        ChunkData<PositionChunkData> positions = new([], null);
        ChunkData<RotationChunkData> rotations = new([], []);
        ChunkData<ScaleChunkData> scales = new([], []);

        using var _ = chunk.UseLock();
        u8 i = 0;
        foreach (var entityId in chunk.Entities)
        {
            if (!world.Position.TryGet(entityId, out var position))
                position = default;

            positions.Data.Add(PositionChunkData.QuantizeFor(chunkCoord, world.Grid.GridInfo, position.Value));


            if (world.Rotation.IsDirty(entityId))
            {
                var rotation = world.Rotation.Get(entityId);
                rotations.Data.Add(RotationChunkData.Quantize(rotation.Value));
                rotations.IndexOverride!.Add(i);
                world.Rotation.ClearDirty(entityId);
            }

            if (world.Scale.IsDirty(entityId))
            {
                var scale = world.Scale.Get(entityId);
                scales.Data.Add(new ScaleChunkData(scale.Value));
                scales.IndexOverride!.Add(i);
                world.Scale.ClearDirty(entityId);
            }

            i++;
        }

        new ToClientPackets.ChunkUpdatePacket(chunkCoord, positions).Serialize(ref writer);
        if (rotations.Data.Count > 0)
        {
            new ToClientPackets.ChunkUpdatePacket(chunkCoord, rotations).Serialize(ref writer);
        }

        if (scales.Data.Count > 0)
        {
            new ToClientPackets.ChunkUpdatePacket(chunkCoord, scales).Serialize(ref writer);
        }

        return serializer;
    }

    public void RemovePlayerFromChunks(ConnectionContext context)
    {
        // TODO rework this

        var playerEntity = context.PrimaryEntity;

        // Remove from spatial grid
        if (world.Spacial.Has(playerEntity))
        {
            var spatialData = world.Spacial.Get(playerEntity);
            // Note: Could clean up empty chunks here if needed
        }

        var position = world.Position.Has(playerEntity) ? world.Position.Get(playerEntity) : new Position();

        // Notify other players in range
        using var msg = new ToClientPackets.EntityDestroyedPacket(new NetEntId(_serverId, playerEntity)).ToMessage();
        SendToPlayersInRange(position.Value, position.Level, msg);

        context.PrimaryEntity = EntityId.Invalid;
        context.LoadedChunks.Clear();
        world.DeleteEntity(playerEntity);
    }

    private void SendToPlayersInRange(Vector3 position, i32 level, SerializedMessage packet)
    {
        // Calculate which chunk this position is in
        var chunkCoord = world.Grid.GetChunkCoord(position, level);

        foreach (var context in _networkSystem._connectionContexts.Values)
        {
            var playerPosition = context.Position;
            var playerChunk = world.Grid.GetChunkCoord(playerPosition.Value, playerPosition.Level);
            if (playerChunk.PosDistanceTo(chunkCoord) > MaxSendRangeChunks) continue;
            context.Connection.Send(packet);
        }
    }
}