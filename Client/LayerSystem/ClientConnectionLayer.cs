using System.Numerics;
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
public class ClientConnectionLayer(
    ClientNetworkManager networkManager,
    ManagementClient managementClient,
    ClientGameLogic gameLogic,
    PlayerInputSystem inputSystem)
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
                Task.Run(() => networkManager.Gateway.ConnectGateway(_address));
            }

            if (ImGui.Button("Disconnect Gateway"))
            {
                Task.Run(networkManager.Gateway.DisconnectGateway);
            }

            ImGui.TextWrapped($"Gateway Server: {networkManager.Gateway.Client}");
            ImGui.Separator();
            ImGui.InputText("Address Game", ref _address, 100);
            if (networkManager.Gateway.Client.IsNotConnected && ImGui.Button("Connect Game"))
            {
                Task.Run(() => networkManager.ConnectGame(_address));
            }

            if (networkManager.GameServerEndpoints.Length == 0)
            {
                ImGui.Text("Waiting for available game servers from gateway...");
            }
            else
            {
                ImGui.Text("Available Game Servers: ");
                ImGui.SameLine();
                ImGui.Text(networkManager.GameServerEndpoints.Length.ToString());
                if (ImGui.Button("Connect To All"))
                {
                    Task.Run(networkManager.ConnectToAllGameServers);
                }

                foreach (var serverEndpoint in networkManager.GameServerEndpoints)
                {
                    if (ImGui.Button($"Connect to {serverEndpoint}"))
                    {
                        Task.Run(() => networkManager.ConnectGame(serverEndpoint));
                    }
                }
            }

            if (networkManager.IsConnected && ImGui.Button("Disconnect Game"))
            {
                Task.Run(networkManager.DisconnectGame);
            }

            ImGui.TextWrapped($"Game Server: {networkManager}");
            ImGui.End();
        }


        if (ImGui.Begin("Network Entities"))
        {
            ImGui.Text($"RenderVersion: {gameLogic.RenderEntityVersion}");
            ImGui.SameLine();
            ImGui.Text($"EntityCount: {gameLogic.EntityCount}");

            ImGui.Text($"Player Position: {inputSystem.CurrentPosition}");
            ImGui.Text($"Player Rotation: {inputSystem.CurrentRotation}");

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

                foreach (var entityId in gameLogic.World.Take(200))
                {
                    var networkId = gameLogic.GetNetworkId(entityId);
                    var transform = gameLogic.World.Transform.Get(entityId);

                    var entityType = gameLogic.World.EntityType.TryGet(entityId, out var typeVal)
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
                    ImGui.Text($"{transform.Position.ToString()}");
                    ImGui.TableSetColumnIndex(4);
                    ImGui.Text($"{transform.Rotation.ToString()}");
                    ImGui.TableSetColumnIndex(5);
                    ImGui.Text($"{transform.Scale.ToString()}");
                }

                ImGui.EndTable();
            }

            ImGui.End();
        }

        if (ImGui.Begin("Management"))
        {
            ImGui.Text($"Cluster State: {managementClient.ClusterManager.State}");
            ImGui.Text($"Connected Servers: {managementClient.Servers.Count}:");
            foreach (var server in managementClient.Servers)
            {
                var region = server.Value.Region;
                var serverInfo = server.Value.Server;
                ImGui.Text($"ServerId: {server.Key}");
                ImGui.Text($"  {region.ReadableSpan()}");
                ImGui.Text($"  Ready: {serverInfo.IsReady}");
                ImGui.Text($"  Connected: {serverInfo.ConnectedAt}");
                ImGui.Separator();
            }
            
            if(ImGui.Button("Spawn Test Entities"))
            {
                managementClient.SpawnTestEntities();
            }
        }
    }
}