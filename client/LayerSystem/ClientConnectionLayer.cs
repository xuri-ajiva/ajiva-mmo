using System.Numerics;
using Client.Game;
using Client.Networking;
using Client.Renderer.PbrRenderer;
using ImGuiNET;
using Silk.NET.Maths;

namespace Client.LayerSystem;

/// <summary>
/// UI layer for connection status and entity debugging
/// Now purely focused on UI - no network logic
/// </summary>
public class ClientConnectionLayer(ClientNetworkManager networkManager, ClientGameLogic gameLogic)
    : IUiRenderLayer
{
    public void Attach()
    {
    }

    public void Update(UpdateInfo info)
    {
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

        // Connection status and controls
        ImGui.Text($"Status: {networkManager.State?.ToString()}");
        if (networkManager.IsNotConnected)
        {
            ImGui.InputText("Address", ref _address, 100);
            if (ImGui.Button("Connect"))
            {
                networkManager.Connect(_address);
            }
        }
        else
        {
            if (ImGui.Button("Disconnect"))
            {
                networkManager.Disconnect();
            }
        }

        ImGui.Separator();

        // Player info
        if (gameLogic.LocalPlayerNetworkId.HasValue)
        {
            ImGui.Text($"Local Player Network ID: {gameLogic.LocalPlayerNetworkId.Value}");

            if (gameLogic.LocalPlayerEntityId.HasValue)
            {
                var playerEntity = gameLogic.LocalPlayerEntityId.Value;
                ImGui.Text($"Local Player ECS ID: {playerEntity.Value}");

                if (gameLogic.World.HasPosition(playerEntity))
                {
                    var pos = gameLogic.World.GetPosition(playerEntity);
                    ImGui.Text($"Player Position: {pos.Value}");
                }
            }
        }

        ImGui.Separator();

        // Entity list
        ImGui.Text("Entities:");
        if (ImGui.BeginTable("entityTable", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("ECS", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Net", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Position");
            ImGui.TableSetupColumn("Rotation");
            ImGui.TableSetupColumn("Scale");
            ImGui.TableHeadersRow();

            foreach (var entityId in gameLogic.GetAllEntities())
            {
                var networkId = gameLogic.GetNetworkId(entityId);

                var position = gameLogic.World.TryGetPosition(entityId, out var posVal)
                    ? posVal.Value.ToString()
                    : "<none>";
                var rotation = gameLogic.World.TryGetRotation(entityId, out var rotVal)
                    ? rotVal.Value.ToString()
                    : "<none>";
                var scale = gameLogic.World.TryGetScale(entityId, out var scaleVal)
                    ? scaleVal.Value.ToString()
                    : "<none>";
                var entityType = gameLogic.World.TryGetEntityType(entityId, out var typeVal)
                    ? $"{typeVal.TypeId}"
                    : "<none>";

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text($"{entityId.Value}");
                ImGui.TableSetColumnIndex(1);
                ImGui.Text($"{networkId}");
                ImGui.TableSetColumnIndex(2);
                ImGui.Text($"{entityType}");
                ImGui.TableSetColumnIndex(3);
                ImGui.Text($"{position}");
                ImGui.TableSetColumnIndex(4);
                ImGui.Text($"{rotation}");
                ImGui.TableSetColumnIndex(5);
                ImGui.Text($"{scale}");
            }

            ImGui.EndTable();
        }

        ImGui.End();
    }
}
