using System.Numerics;
using Client.Renderer.PbrRenderer;
using Common.Ecs;
using Common.Networking;
using ImGuiNET;
using Riptide;
using Silk.NET.Maths;

namespace Client.LayerSystem;

public class ClientConnectionLayer(Riptide.Client client, World world, GameLogic gameLogic, ICamera camera)
    : IUiRenderLayer
{
    private readonly World _world = world;
    private readonly GameLogic _gameLogic = gameLogic;
    private readonly ICamera _camera = camera;

    public void Attach()
    {
    }


    public void Update(UpdateInfo info)
    {
        if (!client.IsConnected) return;

        if (_gameLogic.LocalPlayerEntityId.HasValue)
        {
            var localPlayer = world.GetEntity(_gameLogic.LocalPlayerEntityId.Value);
            if (world.HasComponent<TransformComponent>(localPlayer))
            {
                ref var currentTransform = ref world.GetComponent<TransformComponent>(localPlayer);
                var currentTransformRotation = Quaternion.CreateFromRotationMatrix(_camera.ViewMatrix);
                if (currentTransform.Position != _camera.Position ||
                    currentTransform.Rotation != currentTransformRotation)
                {
                    currentTransform.Position = _camera.Position;
                    currentTransform.Rotation = currentTransformRotation;
                    new EntityUpdatePacket(localPlayer.Id, currentTransform).Send(client.Connection!,
                        MessageSendMode.Unreliable);
                }
            }
        }
    }

    public void Detach()
    {
    }

    public ref bool Enabled => ref _enabled;

    private string _address = "127.0.0.1:7777";
    private bool _enabled = true;

    public void RenderUi(UpdateInfo info)
    {
        ImGui.Begin("Client Connection");
        ImGui.Text($"Status: {client.State?.ToString()}");
        if (client.IsNotConnected)
        {
            ImGui.InputText("Address", ref _address, 100);
            if (ImGui.Button("Connect"))
            {
                client.Connect(_address);
            }
        }
        else
        {
            if (ImGui.Button("Disconnect"))
            {
                client.Disconnect();
            }
        }

        var transformStore = world.GetStore<TransformComponent>();
        var transforms = transformStore.GetAll();
        for (u32 i = 0; i < transforms.Length; i++)
        {
            var entityId = transformStore.GetEntityIdFromDenseIndex(i);
            var transform = transforms[(i32)i];
            ImGui.Text($"Entity: {entityId}: P:{transform.Position} R:{transform.Rotation} S:{transform.Scale}");
        }

        ImGui.End();
    }
}
