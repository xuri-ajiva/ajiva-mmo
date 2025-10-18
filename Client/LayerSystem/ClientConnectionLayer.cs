using Client.Game;
using Client.Networking;
using ImGuiNET;
using Interception;

namespace Client.LayerSystem;

/// <summary>
/// UI layer for connection status and entity debugging
/// Now purely focused on UI - no network logic
/// </summary>
[ExcludeFromInterception]
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

    [ExcludeFromInterception]
    public void RenderUi(UpdateInfo info)
    {
        if (ImGui.Begin("Client Connection"))
        {
            // Connection status and controls
            ImGui.Text($"RoundTrip: {networkManager.LastRtt}, RemoteEndPoint: {networkManager.RemoteEndPoint}");
            ImGui.InputText("Address Gateway", ref _address, 100);
            if (ImGui.Button("Connect Gateway"))
            {
                networkManager.Gateway.ConnectGateway(_address);
            }

            if (ImGui.Button("Disconnect Gateway"))
            {
                networkManager.Gateway.DisconnectGateway();
            }

            ImGui.TextWrapped($"Gateway Server: {networkManager.Gateway.Client}");
            ImGui.Separator();
            ImGui.InputText("Address Game", ref _address, 100);
            if (networkManager.Gateway.Client.IsNotConnected && ImGui.Button("Connect Game"))
            {
                networkManager.ConnectGame(_address);
            }
            
            ImGui.Text("Available Game Servers:");
            foreach (var serverEndpoint in networkManager.GameServerEndpoints)
            {
                if (ImGui.Button($"Connect to {serverEndpoint}"))
                {
                    networkManager.ConnectGame(serverEndpoint);
                }
            }

            if (networkManager.IsConnected && ImGui.Button("Disconnect Game"))
            {
                networkManager.DisconnectGame();
            }
            ImGui.TextWrapped($"Game Server: {networkManager.Client}");
            ImGui.End();
        }


        if (ImGui.Begin("Network Entities"))
        {
            ImGui.Text($"RenderVersion: {gameLogic.RenderEntityVersion}");
            ImGui.SameLine();
            ImGui.Text($"EntityCount: {gameLogic.EntityCount}");
            
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
                else
                {
                    ImGui.Text("Local Player ECS ID: <none>");
                }
            }

            ImGui.Separator();

            // Entity list
            ImGui.Text("Entities (first 200):");
            if (ImGui.BeginTable("entityTable", 6,
                    ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("ECS", ImGuiTableColumnFlags.WidthFixed, 50);
                ImGui.TableSetupColumn("Net", ImGuiTableColumnFlags.WidthFixed, 50);
                ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, 50);
                ImGui.TableSetupColumn("Position");
                ImGui.TableSetupColumn("Rotation");
                ImGui.TableSetupColumn("Scale");
                ImGui.TableHeadersRow();

                foreach (var entityId in gameLogic.GetAllEntities().Take(200))
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
}