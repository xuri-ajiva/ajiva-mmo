using System.Numerics;
using Client.Common;
using Client.Game;
using Client.LayerSystem;
using Client.Networking;
using Client.Renderer.PbrRenderer;
using Common.Ecs;
using Common.Systems;
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
    ILogger<PlayerInputSystem> logger)
    : IAjivaLayer, IEventHandler<AjivaMouseEvent>, IEventHandler<AjivaKeyboardEvent>
{
    public Vector3 CurrentPosition { get; set; }
    public Quaternion CurrentRotation { get; set; }

    public void Attach()
    {
    }

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

    public void Detach()
    {
    }

    private bool _updateEnabled = true;
    public ref bool Enabled => ref _updateEnabled;

    public void OnEvent(ref AjivaEvent<AjivaMouseEvent> inputEvent)
    {
        if (camera is IEventHandler<AjivaMouseEvent> handler)
        {
            handler.OnEvent(ref inputEvent);
        }
    }

    public void OnEvent(ref AjivaEvent<AjivaKeyboardEvent> inputEvent)
    {
        if (camera is IEventHandler<AjivaKeyboardEvent> handler)
        {
            handler.OnEvent(ref inputEvent);
        }
    }
}