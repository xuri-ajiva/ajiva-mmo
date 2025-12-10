using System.Collections.Concurrent;
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
using Common.Systems;
using NetworkPackets;
using Polly;

namespace Client.Networking;

public class ClientConnectionContext(Packets.ServerType serverType)
{
    public QConnection? Connection => Client?.Connection;
    public Packets.ServerType ServerType { get; init; } = serverType;
    public string? Endpoint { get; set; }
    public QClient Client { get; set; } = null!;
    public i32 ErrorCount { get; set; }
    public static int MaxErrorCount { get; set; } = 10;
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
    public Guid ClientId { get; } = Guid.NewGuid();

    private ClientConnectionContext _primaryContext = new(Packets.ServerType.GameServer);
    private readonly ClientConnectionContext _gatewayContext = new(Packets.ServerType.EntryPoint);
    private readonly ConcurrentBag<ClientConnectionContext> _clients = [];
    public string[] GameServerEndpoints { get; private set; } = [];

    public bool IsConnected => _primaryContext.Client.IsConnected;
    public string RemoteEndPoint { get; private set; } = "Not connected";

    public TimeSpan LastRtt = TimeSpan.Zero;
    private Histogram<f64>? _rttMeter;
    public readonly ClientGameLogic GameLogic = gameLogic;

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        Gateway = new GatewayHandler(this, logger, factory.CreateClient(), instrumentation);
        _rttMeter = instrumentation.Meter?.CreateHistogram<f64>("client_network_rtt", "ms",
            "Round-trip time to the server");
        _primaryContext.Client = factory.CreateClient();
        _gatewayContext.Client = Gateway.Client;
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        updater.RegisterUpdate(UpdateCycle.Update, cycle =>
        {
            if (cycle.Ticks % 1000 == 0)
            {
                //Instrumentation.Instance = instrumentation;
            }

            using var activity = Instrumentation.NamedTrace("Client Packet Handling", ActivityKind.Client);
            foreach (var client in _clients)
            {
                client.Client.HandlePackets((packetType, ref reader, _) =>
                    PacketDispatcher(packetType, ref reader, _primaryContext));
            }

            if (cycle.Ticks % 1000 == 0)
            {
                Instrumentation.Instance = null;
            }
        });
        updater.RegisterUpdate(UpdateCycle.Update, _ => Gateway.Client.HandlePackets((packetType, ref reader, _) =>
            PacketDispatcher(packetType, ref reader, _gatewayContext)));
        return RunAutoConnect(cancellationToken);
    }

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        foreach (var client in _clients.Select(context => context.Client).Where(client => client.IsConnected))
        {
            await Task.Run(client.DisconnectAsync, cancellationToken);
        }

        if (Gateway.Client.IsConnected)
        {
            await Task.Run(Gateway.Client.DisconnectAsync, cancellationToken);
        }
    }

    public async Task ConnectGame(string address)
    {
        var existingConnection = _clients.FirstOrDefault(context => context.Endpoint == address);

        if (existingConnection is { Client.IsConnected: true })
        {
            logger.LogInformation(
                "Game Client is already connected to a server at {RemoteEndPoint}, Setting as primary",
                existingConnection.Connection?.RemoteEndPoint);
            //await existingConnection.Client.DisconnectAsync();
            _primaryContext = existingConnection;
            RemoteEndPoint = existingConnection.Connection?.RemoteEndPoint.ToString() ?? "Unknown";
            return;
        }

        existingConnection = new ClientConnectionContext(Packets.ServerType.GameServer)
        {
            Client = factory.CreateClient(),
            Endpoint = address,
        };

        RemoteEndPoint = address;
        await Policy.Handle<Exception>()
            .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(5))
            .ExecuteAsync(async ct => await existingConnection.Client.ConnectAsync(instrumentation, address, ct),
                CancellationToken.None);
        if (existingConnection.Client.IsConnected)
        {
            _clients.Add(existingConnection);
            _primaryContext = existingConnection;
            RemoteEndPoint = existingConnection.Connection?.RemoteEndPoint.ToString() ?? "Unknown";
        }
        else
        {
            logger.LogError("Failed to connect to game server at {ServerAddress}", address);
        }
    }

    public async Task DisconnectGame()
    {
        foreach (var client in _clients.Select(context => context.Client).Where(client => client.IsConnected))
        {
            await client.DisconnectAsync();
        }

        _clients.Clear();
        RemoteEndPoint = "Not connected";
    }

    public async Task ConnectToAllGameServers()
    {
        foreach (var serverEndpoint in GameServerEndpoints)
        {
            await ConnectGame(serverEndpoint);
            await Task.Delay(100);
        }
    }

    public void SendPlayerUpdate(Vector3 position, Quaternion rotation)
    {
        if (!IsConnected) return;

        foreach (var context in _clients)
        {
            new ToServerPackets.PlayerMovementPacket(new Position(position), new Rotation(rotation))
                .Send(context.Connection!);
        }
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