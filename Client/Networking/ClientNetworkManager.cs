using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Numerics;
using Client.Game;
using Common;
using Common.Config;
using Common.Ecs;
using Common.Networking;
using Common.Networking.AjivaQuic;
using NetworkPackets;

namespace Client.Networking;

/// <summary>
/// Clean network manager that handles all client-server communication
/// Separates networking concerns from UI and game logic
/// </summary>
public class ClientNetworkManager(
    ILogger<ClientNetworkManager> logger,
    QClient client,
    ClientGameLogic gameLogic,
    Instrumentation instrumentation,
    ClientConfiguration clientConfiguration)
    : IHostedLifecycleService, IUniversalMessageHandler<QConnection>
{
    public ConnectionState State => client.State;

    public void Handle(PingPacket message, QConnection userData)
    {
        var now = Stopwatch.GetTimestamp();
        LastRtt = Stopwatch.GetElapsedTime(message.Timestamp, now);
        _rttMeter?.Record(LastRtt.TotalMilliseconds);
    }

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
            Task.Run(() => client.ConnectAsync(this, instrumentation, address));
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

        new EntityUpdatePacket(networkEntityId, transform)
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
                await client.ConnectAsync(this, instrumentation,
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

    public void Handle(EntityCreatedPacket message, QConnection connection)
    {
        gameLogic.HandleEntityCreated(message);
    }

    public void Handle(EntityUpdatePacket message, QConnection connection)
    {
        gameLogic.HandleEntityUpdate(message);
    }

    public void Handle(EntityDestroyedPacket message, QConnection connection)
    {
        gameLogic.HandleEntityDestroyed(message);
    }

    public void Handle(BeginWorldStatePacket message, QConnection connection)
    {
        logger.LogInformation("Beginning to receive world state for {EntityCount} entities.", message.EntityCount);
        gameLogic.HandleBeginWorldState(message);
    }

    public void Handle(LoadLevelPacket message, QConnection connection)
    {
        logger.LogInformation("Loading level: {LevelName}", message.LevelName);
        //gameLogic.LoadLevel(message.LevelName);
        gameLogic.InitializeSpacialGrid(message.GridInfo);
    }

    public void Handle(EntityAssignedPacket message, QConnection connection)
    {
        gameLogic.SetLocalPlayerEntityId(message.Id);
        logger.LogInformation("Assigned local player entity ID: {EntityId}", message.Id);
    }

    public void Handle(ChunkUpdatePacket message, QConnection connection)
    {
        gameLogic.HandleChunkUpdate(message);
    }
    
    public void Handle(ChunkLoadPacket message, QConnection userData)
    {
        gameLogic.HandleChunkLoad(message);
    }

    public void Handle(ChunkDeltaPacket message, QConnection userData)
    {
        gameLogic.HandleChunkDelta(message);
    }

    public void Handle(ChunkUnloadPacket message, QConnection connection)
    {
        gameLogic.HandleChunkUnload(message);
    }

    public Task StartedAsync(CancellationToken cancellationToken) => RunAutoConnect(cancellationToken);
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}