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

    public void Update(UpdateInfo info)
    {
        if (!networkManager.IsConnected) return;
        if (!gameLogic.LocalPlayerEntityId.HasValue) return;

        var localPlayerEntity = gameLogic.LocalPlayerEntityId.Value;
        var networkId = gameLogic.GetNetworkId(localPlayerEntity);

        if (!networkId.HasValue) return;

        // Get current transform from ECS
        if (!gameLogic.World.HasPosition(localPlayerEntity)) return;

        var currentPosition = gameLogic.World.GetPosition(localPlayerEntity);
        var currentRotation = gameLogic.World.HasRotation(localPlayerEntity)
            ? gameLogic.World.GetRotation(localPlayerEntity)
            : new Rotation { Value = Quaternion.Identity };
        var currentScale = gameLogic.World.HasScale(localPlayerEntity)
            ? gameLogic.World.GetScale(localPlayerEntity)
            : new Scale { Value = Vector3.One };

        // Get camera transform
        var cameraPosition = camera.Position;
        var cameraRotation = Quaternion.CreateFromRotationMatrix(camera.ViewMatrix);

        // Check if camera moved significantly (to avoid spamming network)
        var positionChanged = Vector3.Distance(currentPosition.Value, cameraPosition) > 0.01f;
        var rotationChanged = Quaternion.Dot(currentRotation.Value, cameraRotation) < 0.999f; // ~2.5 degree threshold

        if (positionChanged || rotationChanged)
        {
            // Update local ECS immediately for responsive feel

            gameLogic.World.UpdatePosition(localPlayerEntity).Value = cameraPosition;
            gameLogic.World.UpdateRotation(localPlayerEntity).Value = cameraRotation;

            // Send to server
            networkManager.SendEntityUpdate(
                networkId.Value,
                cameraPosition,
                cameraRotation,
                currentScale.Value
            );
        }
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
