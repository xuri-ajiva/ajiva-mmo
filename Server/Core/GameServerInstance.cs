using System.Net;
using Client.LayerSystem;
using Common;
using Common.SpacialPartitioning;
using Server.Management;
using Server.Systems;

namespace Server.Core;

/// <summary>
/// Configuration for a game server instance
/// </summary>
public class GameServerInstanceConfig
{
    public ServerId Id { get; set; } = ServerId.Invalid;
    public SpacialGridInfo GridInfo { get; set; } = new();
    public RegionAssignment InitialRegion { get; set; } = new();
    public WorkerManagementServer? ManagementServer { get; set; }
    public TimeSpan TickInterval { get; set; } = TimeSpan.FromMilliseconds(50);

    public void Validate()
    {
        if (Id == ServerId.Invalid)
            throw new InvalidOperationException("ServerId is invalid.");
        if (GridInfo.ChunkSize is { X: <= 0 } or { Y: <= 0 } or { Z: <= 0 })
            throw new InvalidOperationException("ChunkSize must be greater than zero.");
        if (ManagementServer == null)
            throw new InvalidOperationException("ManagementServer is not set.");
    }
}

/// <summary>
/// A complete game server instance that manages all server systems
/// </summary>
public class GameServerInstance(
    ILogger<GameServerInstance> logger,
    IServiceProvider serviceProvider,
    ISystemManager systemManager,
    GameServerInstanceConfig config,
    Instrumentation instrumentation) : IAsyncDisposable
{
    // Systems
    private NetworkSystem _networkSystem = null!;
    internal TransactionSystem TransactionSystem = null!;

    private bool _isInitialized;
    private bool _isRunning;

    private readonly HighResolutionTicker _highResolutionTicker =
        new(config.TickInterval, instrumentation: instrumentation);

    public RegionAssignment AssignedRegion => config.InitialRegion;
    public IPEndPoint? ClientEndPoint => _networkSystem?.LocalEndPoint;

    /// <summary>
    /// Initialize the server instance with all systems
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        logger.LogInformation("Initializing game server instance {ServerId} for region {Region}",
            config.Id, config.InitialRegion);

        config.Validate();

        var ecs = serviceProvider.GetRequiredService<NServerEcs>();
        ecs.Grid = new SpacialGrid(config.GridInfo);

        systemManager.RegisterSystem(_networkSystem = serviceProvider.GetRequiredService<NetworkSystem>());
        systemManager.RegisterSystem(serviceProvider.GetRequiredService<ChunkSystem>());
        systemManager.RegisterSystem(serviceProvider.GetRequiredService<GameLogicSystem>());
        systemManager.RegisterSystem(serviceProvider.GetRequiredService<SimplePhysicsSystem>());
        systemManager.RegisterSystem(TransactionSystem = serviceProvider.GetRequiredService<TransactionSystem>());

        await systemManager.InitializeAsync();
        _highResolutionTicker.OnTick = RunTick;

        _isInitialized = true;
        logger.LogInformation("Game server instance {ServerId} initialized", config.Id);
    }

    private void RunTick(UpdateInfo info)
    {
        systemManager.Tick(info);
    }

    /// <summary>
    /// Start the server instance
    /// </summary>
    public async Task StartAsync()
    {
        if (!_isInitialized)
            await InitializeAsync();

        if (_isRunning) return;

        logger.LogInformation("Starting game server instance {ServerId}", config.Id);

        // Create and demo entities
        DemoLevel.LoadLevel(serviceProvider.GetRequiredService<NServerEcs>(), AssignedRegion, config.Id);

        await systemManager.StartAsync();
        _highResolutionTicker.Start();

        _isRunning = true;
        logger.LogInformation("Game server instance {ServerId} started. Client endpoint: {ClientEndPoint}",
            config.Id, ClientEndPoint);
    }

    /// <summary>
    /// Stop the server instance
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning) return;
        _highResolutionTicker.Stop();
        await systemManager.StopAsync();
        _isRunning = false;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isRunning)
        {
            await StopAsync();
        }

        await systemManager.DisposeAsync();
        logger.LogInformation("Game server instance {ServerId} disposed", config.Id);
    }
}