using System.Diagnostics;
using System.Net.Quic;
using System.Numerics;
using Client.Game;
using Client.Renderer;
using Common;
using Common.Ecs;
using Common.Networking;
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
    Instrumentation instrumentation)
    : BackgroundService, IUniversalMessageHandler<QConnection>, IDisposable
{
    public ConnectionState State => client.State;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Update();
                await Task.Delay(5, cancellationToken: stoppingToken); // ~200 FPS update rate
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in ClientNetworkManager ExecuteAsync!");
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
        logger.LogInformation("Beginning to receive world state for {entityCount} entities.", message.EntityCount);
        gameLogic.HandleBeginWorldState(message);
    }

    public void Handle(LoadLevelPacket message, QConnection connection)
    {
        logger.LogInformation("Loading level: {levelName}", message.LevelName);
        gameLogic.LoadLevel(message.LevelName);
    }

    public void Handle(EntityAssignedPacket message, QConnection connection)
    {
        gameLogic.SetLocalPlayerEntityId(message.EntityId);
        logger.LogInformation("Assigned local player entity ID: {EntityId}", message.EntityId);
    }

    public TimeSpan LastRtt = TimeSpan.Zero;

    public void Handle(PingPacket message, QConnection userData)
    {
        var now = Stopwatch.GetTimestamp();
        LastRtt = Stopwatch.GetElapsedTime(message.Timestamp, now);
    }

    public void MessageNotHandled(string messageType, QConnection connection)
    {
        logger.LogWarning("Message not handled: {MessageType}", messageType);
    }


    public void Connect(string address)
    {
        if (client.IsNotConnected)
        {
            Task.Run(() => client.ConnectAsync(this, address));
        }
    }

    public void Disconnect()
    {
        if (!client.IsNotConnected)
        {
            Task.Run(client.DisconnectAsync);
        }
    }

    public async Task Update()
    {
        if (client.IsNotConnected) return;

        try
        {
            //using var activity = instrumentation.StartActivity(ActivityKind.Client);
            //Instrumentation.Instance = instrumentation;
            //await client.Update(this);
            //Instrumentation.Instance = null;
        }
        catch (OperationCanceledException)
        {
            await client.DisconnectAsync();
        }
        catch (QuicException e)
        {
            logger.LogError(e, "QuicException in Client Update, disconnecting.");
            await client.DisconnectAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in Client Update!");
        }
    }

    public bool IsConnected => client.IsConnected;
    public bool IsNotConnected => client.IsNotConnected;

    /// <summary>
    /// Send entity update to server (called by player input system)
    /// </summary>
    public void SendEntityUpdate(u32 networkEntityId, Vector3 position, Quaternion rotation, Vector3 scale)
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

    public override void Dispose()
    {
        Disconnect();
        base.Dispose();
    }
}
