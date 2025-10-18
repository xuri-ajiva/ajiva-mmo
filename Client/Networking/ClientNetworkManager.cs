using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Numerics;
using Client.Game;
using Client.LayerSystem;
using Common;
using Common.Config;
using Common.Ecs;
using Common.Networking;
using Common.Networking.AjivaQuic;
using MemoryPack;
using NetworkPackets;

namespace Client.Networking;

public class ClientConnectionContext(Packets.ServerType serverType)
{
    public QConnection Connection { get; set; } = null!;
    public Packets.ServerType ServerType { get; init; } = serverType;
}

/// <summary>
/// Clean network manager that handles all client-server communication
/// Separates networking concerns from UI and game logic
/// </summary>
public class ClientNetworkManager(
    ILogger<ClientNetworkManager> logger,
    QFactory factory,
    ClientGameLogic gameLogic,
    Instrumentation instrumentation,
    PeriodicUpdater updater,
    ClientConfiguration clientConfiguration)
    : IHostedLifecycleService, IClientPacketHandler<ClientConnectionContext>
{
    public ConnectionState State => client.State;

    public void Handle(Packets.HelloPacket message, QConnection userData)
    {
    }
    public QClient Client { get; } = factory.CreateClient();
    private readonly ClientConnectionContext _gameContext = new(Packets.ServerType.GameServer);

    public void Handle(Packets.DisconnectPacket message, QConnection userData)
    {
        logger.LogWarning("Disconnected from server: {Reason}", message.Reason);
        Disconnect();
    }

    public void Handle(Packets.PingPacket message, QConnection userData)
    {
        var now = Stopwatch.GetTimestamp();
        LastRtt = Stopwatch.GetElapsedTime(message.Timestamp, now);
        _rttMeter?.Record(LastRtt.TotalMilliseconds);
    }
    public bool IsConnected => Client.IsConnected;

    public TimeSpan LastRtt = TimeSpan.Zero;
    private Histogram<f64>? _rttMeter;

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        _rttMeter = instrumentation.Meter?.CreateHistogram<f64>("client_network_rtt", "ms",
            "Round-trip time to the server");
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        Disconnect();
        return Task.CompletedTask;
    }

    public void MessageNotHandled(string messageType, QConnection connection)
    {
        logger.LogWarning("Message not handled: {MessageType}", messageType);
    }


    public void Connect(string address)
    {
        if (client.IsNotConnected)
        {
            Task.Run(() => client.ConnectAsync(instrumentation, address));
        }
    }

    public void Disconnect()
    {
        if (!client.IsNotConnected)
        {
            Task.Run(client.DisconnectAsync);
        }
    }


    public bool IsConnected => client.IsConnected;
    public bool IsNotConnected => client.IsNotConnected;

    /// <summary>
    /// Send entity update to server (called by player input system)
    /// </summary>
    public void SendEntityUpdate(NetEntId networkEntityId, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (!IsConnected) return;

        var transform = new Transform
        {
            Position = position,
            Rotation = rotation,
            Scale = scale
        };

        new ToServerPackets.PlayerMovementPacket(networkEntityId, transform)
            .Send(client.Connection!);
    }


    private async Task RunAutoConnect(CancellationToken cancellationToken)
    {
        if (clientConfiguration is not { AutoConnect: true, AutoConnectAddress: not null })
            return;

        while (!client.IsConnected && !cancellationToken.IsCancellationRequested)
        {
            using var span = instrumentation.StartActivity(ActivityKind.Client);
            try
            {
                span?.AddEvent(new ActivityEvent("Connecting to server", DateTimeOffset.Now));
                await Task.Delay(1000, cancellationToken);
                logger.LogInformation("Connecting client to server on {ServerAddress}",
                    clientConfiguration.AutoConnectAddress);
                await client.ConnectAsync(instrumentation,
                    clientConfiguration.AutoConnectAddress.Replace("localhost", "127.0.0.1"), cancellationToken);
                while (client.State == ConnectionState.Connecting)
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (Exception e)
            {
                span?.AddException(e);
                logger.LogError(e, "Failed to connect to server at {ServerAddress}",
                    clientConfiguration.AutoConnectAddress);
                break;
            }

            if (client.IsConnected)
            {
                span?.AddEvent(new ActivityEvent(
                    "Connected to server",
                    DateTimeOffset.Now));
                span?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                span?.AddEvent(new ActivityEvent(
                    "Failed to connect to server",
                    DateTimeOffset.Now));
                span?.SetStatus(ActivityStatusCode.Error);
            }
        }
    }

    public void Handle(ToClientPackets.EntityCreatedPacket message, QConnection connection)
    {
        gameLogic.HandleEntityCreated(message);
    }

    public void Handle(ToClientPackets.EntityUpdatePacket message, QConnection connection)
    {
        gameLogic.HandleEntityUpdate(message);
    }

    public void Handle(ToClientPackets.EntityDestroyedPacket message, QConnection connection)
    {
        gameLogic.HandleEntityDestroyed(message);
    }

    public void Handle(ToClientPackets.BeginWorldStatePacket message, QConnection connection)
    {
        logger.LogInformation("Beginning to receive world state for {EntityCount} entities.", message.EntityCount);
        gameLogic.HandleBeginWorldState(message);
    }

    public void Handle(ToClientPackets.LoadLevelPacket message, QConnection connection)
    {
        logger.LogInformation("Loading level: {LevelName}", message.LevelName);
        //gameLogic.LoadLevel(message.LevelName);
        gameLogic.InitializeSpacialGrid(message.GridInfo);
    }

    public void Handle(ToClientPackets.EntityAssignedPacket message, QConnection connection)
    {
        gameLogic.SetLocalPlayerEntityId(message.Id);
        logger.LogInformation("Assigned local player entity ID: {EntityId}", message.Id);
    }

    public void Handle(ToClientPackets.ChunkUpdatePacket message, QConnection connection)
    {
        gameLogic.HandleChunkUpdate(message);
    }

    public void Handle(ToClientPackets.ChunkLoadPacket message, QConnection userData)
    {
        gameLogic.HandleChunkLoad(message);
    }

    public void Handle(ToClientPackets.ChunkDeltaPacket message, QConnection userData)
    {
        gameLogic.HandleChunkDelta(message);
    }

    public void Handle(ToClientPackets.ChunkUnloadPacket message, QConnection connection)
    {
        gameLogic.HandleChunkUnload(message);
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        updater.RegisterUpdate(UpdateCycle.Update, _ => client.HandlePackets(PacketDispatcher));
        return RunAutoConnect(cancellationToken);
    }

    private void PacketDispatcher(PacketType packetType, scoped ref MemoryPackReader reader, QConnection userData)
    {
        try
        {
            ((IClientPacketHandler<QConnection>)this).AutoHandle(packetType, ref reader, userData);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error handling packet of type {PacketType}", packetType);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}