using Client.LayerSystem;

namespace Server.Core;

/// <summary>
/// Simple system manager that handles initialization and disposal of server systems
/// </summary>
public class ServerSystemManager(ILogger<IAsyncDisposable> logger) : ISystemManager
{
    private readonly List<IServerSystem> _systems = [];
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private bool _isInitialized;
    private bool _disposed;


    /// <inheritdoc />
    public void RegisterSystem<T>(T system) where T : class, IServerSystem
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _systems.Add(system);
        logger.LogInformation("Registered system: {System}", system);
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        if (_isInitialized || _disposed) return;

        logger.LogInformation("Initializing {Count} systems", _systems.Count);

        foreach (var system in _systems)
        {
            try
            {
                await system.InitializeAsync(this, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize system: {System}", system);
                throw;
            }
        }

        _isInitialized = true;
        logger.LogInformation("All systems initialized successfully");
    }

    /// <inheritdoc />
    public async Task StartAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        foreach (var system in _systems)
        {
            try
            {
                await system.StartAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to start system: {System}", system);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public Task StopAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var stopTasks = _systems.Select(system =>
        {
            try
            {
                return system.StopAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to stop system: {System}", system);
                throw;
            }
        });

        return Task.WhenAll(stopTasks);
    }

    /// <inheritdoc />
    public T? GetSystem<T>() where T : class, IServerSystem
    {
        return _systems.OfType<T>().FirstOrDefault();
    }

    /// <inheritdoc />
    public IEnumerable<IServerSystem> GetAllSystems()
    {
        return _systems.AsReadOnly();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);

        logger.LogInformation("Disposing system manager");

        await _cancellationTokenSource.CancelAsync();

        var disposeTasks = _systems.Select(async system =>
        {
            try
            {
                switch (system)
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync();
                        break;
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error disposing system: {System}", system);
            }
        });

        await Task.WhenAll(disposeTasks);

        _cancellationTokenSource.Dispose();
        logger.LogInformation("System manager disposed");
    }

    public void Tick(UpdateInfo info)
    {
        foreach (var system in _systems)
        {
            try
            {
                system.Tick(info);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error ticking system: {System}", system);
            }
        }
    }
}