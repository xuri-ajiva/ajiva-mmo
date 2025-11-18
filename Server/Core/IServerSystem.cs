using Client.LayerSystem;
using Common;

namespace Server.Core;

public interface ISystemManager : IAsyncDisposable
{
    /// <summary>
    /// Register a system with the manager
    /// </summary>
    void RegisterSystem<T>(T system) where T : class, IServerSystem;

    /// <summary>
    /// Initialize all registered systems
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Start all registered systems
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stop all registered systems
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Get a system by its Type
    /// </summary>
    T? GetSystem<T>() where T : class, IServerSystem;

    /// <summary>
    /// Get all systems
    /// </summary>
    IEnumerable<IServerSystem> GetAllSystems();

    /// <summary>
    /// Tick all registered systems
    /// </summary>
    void Tick(UpdateInfo info);
}

/// <summary>
/// Base interface for all server systems
/// </summary>
public interface IServerSystem
{
    /// <summary>
    /// Initialize the system
    /// </summary>
    Task InitializeAsync(ISystemManager manager, CancellationToken token = default) => Task.CompletedTask;

    /// <summary>
    /// Start the system
    /// </summary>
    Task StartAsync(CancellationToken token = default) => Task.CompletedTask;

    /// <summary>
    /// Stop the system
    /// </summary>
    Task StopAsync(CancellationToken token = default) => Task.CompletedTask;

    /// <summary>
    /// Tick the system
    /// </summary>
    /// <param name="info"></param>
    void Tick(UpdateInfo info);
}