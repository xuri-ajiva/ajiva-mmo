using Common.Systems;
using Server.Core;

namespace Server.Systems;

/// <summary>
/// Handles game logic updates and level management
/// </summary>
public class GameLogicSystem(ILogger<GameLogicSystem> logger) : IServerSystem
{
    public Task InitializeAsync(ISystemManager manager, CancellationToken token = default)
    {
        logger.LogInformation("Initializing game logic system");
        return Task.CompletedTask;
    }

    public void Tick(UpdateInfo info)
    {
    }
}