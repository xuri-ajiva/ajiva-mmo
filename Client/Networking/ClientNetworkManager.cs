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
using Polly;

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
public partial class ClientNetworkManager(
    ILogger<ClientNetworkManager> logger,
    QFactory factory,
    ClientGameLogic gameLogic,
    Instrumentation instrumentation,
    PeriodicUpdater updater,
    ClientConfiguration clientConfiguration)
    : IHostedLifecycleService, IClientPacketHandler<ClientConnectionContext>
{
    public GatewayHandler Gateway { get; private set; } = null!;
    public QClient Client { get; } = factory.CreateClient();
    private readonly ClientConnectionContext _gameContext = new(Packets.ServerType.GameServer);
    private readonly ClientConnectionContext _gatewayContext = new(Packets.ServerType.EntryPoint);
    public string[] GameServerEndpoints { get; private set; } = [];

    public bool IsConnected => Client.IsConnected;
    public string RemoteEndPoint { get; private set; } = "Not connected";

    public TimeSpan LastRtt = TimeSpan.Zero;
    private Histogram<f64>? _rttMeter;

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        Gateway = new GatewayHandler(this, logger, factory.CreateClient(), instrumentation);
        _rttMeter = instrumentation.Meter?.CreateHistogram<f64>("client_network_rtt", "ms",
            "Round-trip time to the server");
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        _gameContext.Connection = Client.Connection!;
        _gatewayContext.Connection = Gateway.Client.Connection!;
        updater.RegisterUpdate(UpdateCycle.Update, _ => Client.HandlePackets((packetType, ref reader, _) =>
            PacketDispatcher(packetType, ref reader, _gameContext)));
        updater.RegisterUpdate(UpdateCycle.Update, _ => Gateway.Client.HandlePackets((packetType, ref reader, _) =>
            PacketDispatcher(packetType, ref reader, _gatewayContext)));
        return RunAutoConnect(cancellationToken);
    }

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        if (Client.IsConnected)
        {
            await Task.Run(Client.DisconnectAsync, cancellationToken);
        }

        if (Gateway.Client.IsConnected)
        {
            await Task.Run(Gateway.Client.DisconnectAsync, cancellationToken);
        }
    }


    public void ConnectGame(string address)
    {
        Task.Run(async () =>
        {
            if (Client.IsConnected)
            {
                logger.LogWarning("Game Client is already connected to a server at {RemoteEndPoint}, Disconnecting...",
                    Client.Connection?.RemoteEndPoint);
                await Client.DisconnectAsync();
            }

            RemoteEndPoint = address;
            await Policy.Handle<Exception>()
                .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(5))
                .ExecuteAsync(async ct => await Client.ConnectAsync(instrumentation, address, ct),
                    CancellationToken.None);
            RemoteEndPoint = Client.Connection?.RemoteEndPoint.ToString() ?? "Unknown";
        });
    }


    public void DisconnectGame()
    {
        if (Client.IsConnected)
        {
            Task.Run(Client.DisconnectAsync);
        }
    }

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
            .Send(Client.Connection!);
    }


    private async Task RunAutoConnect(CancellationToken cancellationToken)
    {
        if (clientConfiguration is not { AutoConnect: true, AutoConnectAddress: not null })
            return;

        while (!Gateway.Client.IsConnected && !cancellationToken.IsCancellationRequested)
        {
            using var span = instrumentation.StartActivity(ActivityKind.Client);
            try
            {
                span?.AddEvent(new ActivityEvent("Connecting to server", DateTimeOffset.Now));
                await Task.Delay(1000, cancellationToken);
                logger.LogInformation("Connecting client to server on {ServerAddress}",
                    clientConfiguration.AutoConnectAddress);
                await Gateway.Client.ConnectAsync(instrumentation, clientConfiguration.AutoConnectAddress,
                    cancellationToken);
                while (Gateway.Client.State == ConnectionState.Connecting)
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

            if (Gateway.Client.IsConnected)
            {
                span?.AddEvent(new ActivityEvent("Connected to server", DateTimeOffset.Now));
                span?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                span?.AddEvent(new ActivityEvent("Failed to connect to server", DateTimeOffset.Now));
                span?.SetStatus(ActivityStatusCode.Error);
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}