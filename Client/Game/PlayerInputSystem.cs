using System.Numerics;
using Client.Game;
using Client.LayerSystem;
using Client.Networking;
using Client.Renderer.PbrRenderer;
using Common.Ecs;
using Interception;

namespace Client.Game;

/// <summary>
/// Handles player input and sends updates to server
/// Separated from UI layers for clean architecture
/// </summary>
[ExcludeFromInterception]
public class PlayerInputSystem(
    ClientGameLogic gameLogic,
    ClientNetworkManager networkManager,
    ICamera camera,
    ILogger<PlayerInputSystem> logger,
    PeriodicUpdater updater)
    : IHostedService
{
    private readonly ILogger<PlayerInputSystem> _logger = logger;
    private PeriodicUpdater.UpdateHandle? _updateHandle;

    public Vector3 CurrentPosition { get; set; }
    public Quaternion CurrentRotation { get; set; }

    public void Update(UpdateInfo info)
    {
        if (!networkManager.IsConnected) return;

        // Get camera transform
        var cameraPosition = camera.Position;
        var cameraRotation = Quaternion.CreateFromRotationMatrix(camera.ViewMatrix);

        if (Vector3.Distance(CurrentPosition, cameraPosition) > 0.01f
            || Quaternion.Dot(CurrentRotation, cameraRotation) < 0.9999f)
        {
            // Send to server
            networkManager.SendPlayerUpdate(cameraPosition, cameraRotation);
        }
        CurrentPosition = cameraPosition;
        CurrentRotation = cameraRotation;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _updateHandle = updater.RegisterUpdate(UpdateCycle.Update, Update);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _updateHandle?.Dispose();
        return Task.CompletedTask;
    }
}