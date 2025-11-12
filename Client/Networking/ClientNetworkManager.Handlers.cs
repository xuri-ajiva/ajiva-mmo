using System.Diagnostics;
using Common.Networking;
using MemoryPack;
using NetworkPackets;

namespace Client.Networking;

public partial class ClientNetworkManager
{
    public void Handle(Packets.Hello message, ClientConnectionContext context)
    {
        // using var a = Instrumentation.NamedTrace("public void Handle(Packets.Hello message, ClientConnectionContext context)");
        if (message.ServerType != context.ServerType)
        {
            logger.LogError("Server type mismatch: expected {Expected}, got {Actual}", context.ServerType,
                message.ServerType);
            var dc = new Packets.Disconnect("Server type mismatch").ToMessage();
            context.Connection?.Close(dc);
            return;
        }

        logger.LogInformation("Connected to {ServerType} server version {ServerVersion}", message.ServerType,
            message.Version);
    }

    public void Handle(Packets.Disconnect message, ClientConnectionContext context)
    {
        // using var a = Instrumentation.NamedTrace("public void Handle(Packets.Disconnect message, ClientConnectionContext context)");
        logger.LogWarning("Disconnected from server {Server}: {Reason}", context.Connection?.RemoteEndPoint,
            message.Reason);
        switch (context.ServerType)
        {
            case Packets.ServerType.EntryPoint:
                Task.Run(Gateway.DisconnectGateway);
                break;
            case Packets.ServerType.GameServer:
                Task.Run(DisconnectGame);
                break;
            case Packets.ServerType.Management:
            case Packets.ServerType.Unknown:
                break;
            default:
                logger.LogError("Unknown server type: {ServerType}", context.ServerType);
                break;
        }
    }

    public void Handle(Packets.Ping message, ClientConnectionContext context)
    {
        // using var a = Instrumentation.NamedTrace("public void Handle(Packets.Ping message, ClientConnectionContext context)");
        var now = Stopwatch.GetTimestamp();
        LastRtt = Stopwatch.GetElapsedTime(message.Timestamp, now);
        _rttMeter?.Record(LastRtt.TotalMilliseconds);
    }

    public void Handle(ToClientPackets.EntityCreatedPacket message, ClientConnectionContext context)
    {
        // using var a = Instrumentation.NamedTrace("public void Handle(ToClientPackets.EntityCreatedPacket message, ClientConnectionContext context)");
        GameLogic.HandleEntityCreated(message);
    }

    public void Handle(ToClientPackets.EntityUpdatePacket message, ClientConnectionContext context)
    {
        // using var a = Instrumentation.NamedTrace("public void Handle(ToClientPackets.EntityUpdatePacket message, ClientConnectionContext context)");
        GameLogic.HandleEntityUpdate(message);
    }

    public void Handle(ToClientPackets.EntityDestroyedPacket message, ClientConnectionContext context)
    {
        // using var a = Instrumentation.NamedTrace("public void Handle(ToClientPackets.EntityDestroyedPacket message, ClientConnectionContext context)");
        GameLogic.HandleEntityDestroyed(message);
    }

    public void Handle(ToClientPackets.BeginWorldStatePacket message, ClientConnectionContext context)
    {
        // using var a = Instrumentation.NamedTrace("public void Handle(ToClientPackets.BeginWorldStatePacket message, ClientConnectionContext context)");
        logger.LogInformation("Beginning to receive world state for {EntityCount} entities.", message.EntityCount);
        GameLogic.HandleBeginWorldState(message);
    }

    public void Handle(ToClientPackets.ChunkUpdatePacket message, ClientConnectionContext context)
    {
        // using var a = Instrumentation.NamedTrace("public void Handle(ToClientPackets.ChunkUpdatePacket message, ClientConnectionContext context)");
        GameLogic.HandleChunkUpdate(message);
    }

    public void Handle(ToClientPackets.ChunkLoadPacket message, ClientConnectionContext context)
    {
        // using var a = Instrumentation.NamedTrace("public void Handle(ToClientPackets.ChunkLoadPacket message, ClientConnectionContext context)");
        GameLogic.HandleChunkLoad(message);
    }

    public void Handle(ToClientPackets.ChunkDeltaPacket message, ClientConnectionContext context)
    {
        // using var a = Instrumentation.NamedTrace("public void Handle(ToClientPackets.ChunkDeltaPacket message, ClientConnectionContext context)");
        GameLogic.HandleChunkDelta(message);
    }

    public void Handle(ToClientPackets.ChunkUnloadPacket message, ClientConnectionContext context)
    {
        // using var a = Instrumentation.NamedTrace("public void Handle(ToClientPackets.ChunkUnloadPacket message, ClientConnectionContext context)");
        GameLogic.HandleChunkUnload(message);
    }

    private void PacketDispatcher(PacketType packetType, scoped ref MemoryPackReader reader,
        ClientConnectionContext context)
    {
        
        // using var a = Instrumentation.NamedTrace("PacketDispatcher");
        try
        {
            switch (context.ServerType)
            {
                case Packets.ServerType.Unknown:
                case Packets.ServerType.EntryPoint:
                    ((IGatewayClientPacketHandler<ClientConnectionContext>)Gateway).AutoHandle(packetType,
                        ref reader,
                        context);
                    break;
                case Packets.ServerType.GameServer:
                    ((IClientPacketHandler<ClientConnectionContext>)this).AutoHandle(packetType, ref reader, context);
                    break;
                case Packets.ServerType.Management:
                    logger.LogWarning("Server type {ServerType} not supported", context.ServerType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling packet of type {PacketType}", packetType);
            if (context.ErrorCount++ > ClientConnectionContext.MaxErrorCount)
            {
                throw;
            }
        }
    }
}