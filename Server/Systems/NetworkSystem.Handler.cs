using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using System.Numerics;
using Common;
using Common.Ecs;
using Common.Networking;
using Common.Networking.AjivaQuic;
using MemoryPack;
using NetworkPackets;

namespace Server.Systems;

public partial class NetworkSystem : IServerPacketHandler<ConnectionContext>, QServerEventListener
{
    public void DispatchPacket(PacketType packetType, scoped ref MemoryPackReader reader, IQConnection connection)
    {
        try
        {
            if (!_connectionContexts.TryGetValue(connection, out var context)) return;
            ((IServerPacketHandler<ConnectionContext>)this).AutoHandle(packetType, ref reader, context);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling packet {PacketType} from {Client}", packetType, connection);
            //_serverGameLogic.RemovePlayer(_connectionContexts[connection]);
            //_connectionContexts[connection].Kick("Error handling packet: " + packetType);
        }
    }

    public void OnClientDisconnected(IQConnection connection)
    {
        if (!_connectionContexts.TryGetValue(connection, out var context)) return;
        logger.LogInformation("Client disconnected: {Client}", context);
        RemovePlayer(context);
    }

    public void OnConnectionFailed(QuicListener server, IPEndPoint? connection, Exception exception)
    {
        logger.LogCritical(exception, "Server connection failed: {@Client}", connection);
    }

    public void OnClientConnected(QuicListener server, IQConnection connection)
    {
        Instrumentation.Instance = instrumentation;
        using var activity = Instrumentation.NamedTrace("ClientConnected", ActivityKind.Server);
        _connectionContexts[connection] = new ConnectionContext(connection);
        // _serverGameLogic.SendWorldState(connection);
        logger.LogInformation("Client connected: {Client}", connection);
        //SetupPlayer();
    }

    public void Handle(Packets.Hello message, ConnectionContext context) => Ignore();

    public void Handle(Packets.Disconnect message, ConnectionContext context)
    {
        logger.LogError("Client disconnected: {Reason}", message.Reason);
        RemovePlayer(context);
    }

    public void Handle(Packets.Ping message, ConnectionContext context) => Ignore();

    public void Handle(ToServerPackets.PlayerMovementPacket message, ConnectionContext context)
    {
        if (true /*message.Id.Server == _serverId && message.Id.Entity == context.PrimaryEntity*/)
        {
            // todo validation / anti-cheat
            context.Position.Value = message.Position.Value;
            if (context.PrimaryEntity != EntityId.Invalid)
                HandleEntityUpdate(context.PrimaryEntity, new Transform
                    { Position = message.Position.Value, Rotation = message.Rotation.Value, Scale = Vector3.One }
                );
        }
        else
        {
            logger.LogWarning("Received PlayerMovementPacket from {Client}", context);
        }
    }

    public void HandleEntityUpdate(EntityId entity, Transform transform)
    {
        if (!world.Position.Has(entity)) return;

        world.Position.Update(entity).Value = transform.Position;
        world.Rotation.Update(entity).Value = transform.Rotation;
        world.Scale.Update(entity).Value = transform.Scale;
        if (world.Spacial.TryGet(entity, out var spacialData))
        {
            // Mark KD-Tree node as dirty
            var mortonKey = spacialData.CurrentChunk.ToMortonKey(world.Grid.GridInfo.SimulationLevel, world.Grid.GridInfo.ChunkSize.X);
            if (world.Grid.TryGetTreeNode(mortonKey, out var node))
            {
                node.MarkDirty();
            }
        }
    }

    private static void Ignore()
    {
    }
}