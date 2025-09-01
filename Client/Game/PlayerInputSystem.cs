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
public class PlayerInputSystem : IHostedService
{
    private readonly ILogger<PlayerInputSystem> _logger;
    private readonly PeriodicUpdater _updater;
    private readonly ClientGameLogic _gameLogic;
    private readonly ClientNetworkManager _networkManager;
    private readonly ICamera _camera;
    private PeriodicUpdater.UpdateHandle _updateHandle;

    public PlayerInputSystem(ClientGameLogic gameLogic, ClientNetworkManager networkManager, ICamera camera,
        ILogger<PlayerInputSystem> logger, PeriodicUpdater updater)
    {
        _gameLogic = gameLogic;
        _networkManager = networkManager;
        _camera = camera;
        _logger = logger;
        _updater = updater;
    }

    public void Update(UpdateInfo info)
    {
        if (!_networkManager.IsConnected) return;
        if (!_gameLogic.LocalPlayerEntityId.HasValue) return;

        var localPlayerEntity = _gameLogic.LocalPlayerEntityId.Value;
        var networkId = _gameLogic.GetNetworkId(localPlayerEntity);

        if (!networkId.HasValue) return;

        // Get current transform from ECS
        if (!_gameLogic.World.HasPosition(localPlayerEntity)) return;

        var currentPosition = _gameLogic.World.GetPosition(localPlayerEntity);
        var currentRotation = _gameLogic.World.HasRotation(localPlayerEntity)
            ? _gameLogic.World.GetRotation(localPlayerEntity)
            : new Rotation { Value = Quaternion.Identity };
        var currentScale = _gameLogic.World.HasScale(localPlayerEntity)
            ? _gameLogic.World.GetScale(localPlayerEntity)
            : new Scale { Value = Vector3.One };

        // Get camera transform
        var cameraPosition = _camera.Position;
        var cameraRotation = Quaternion.CreateFromRotationMatrix(_camera.ViewMatrix);

        // Check if camera moved significantly (to avoid spamming network)
        var positionChanged = Vector3.Distance(currentPosition.Value, cameraPosition) > 0.01f;
        var rotationChanged = Quaternion.Dot(currentRotation.Value, cameraRotation) < 0.999f; // ~2.5 degree threshold

        if (positionChanged || rotationChanged)
        {
            // Update local ECS immediately for responsive feel
            ref var position = ref _gameLogic.World.GetPosition(localPlayerEntity);
            ref var rotation = ref _gameLogic.World.GetRotation(localPlayerEntity);

            position.Value = cameraPosition;
            rotation.Value = cameraRotation;

            // Send to server
            _networkManager.SendEntityUpdate(
                networkId!.Value,
                cameraPosition,
                cameraRotation,
                currentScale.Value
            );
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _updateHandle = _updater.RegisterUpdate(UpdateCycle.Update, Update);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _updateHandle?.Dispose();
        return Task.CompletedTask;
    }
}
