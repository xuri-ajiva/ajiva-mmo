using System.Numerics;
using Client.Editor.Management;
using Client.Game;
using Client.Networking;
using Client.Renderer;
using Client.Wgsl;
using Common.Ecs;
using ImGuiNET;
using Serilog;

namespace Client.Editor.EditorLayers.Widgets;

public class NetworkWidgets(ClientGameLogic gameLogic, NClientEcsWorld world, PlayerInputSystem inputSystem, ManagementClient managementClient, ClientNetworkManager clientNetwork) : IEditorWidget
{
    private bool _networkInfoEnabled = true, _managementInfoEnabled = true;
    public void Render()
    {
        if (_networkInfoEnabled && ImGui.Begin("Network Entities", ref _networkInfoEnabled))
        {
            ImGui.Text($"RenderVersion: {world.RenderEntityVersion}");
            ImGui.SameLine();
            ImGui.Text($"EntityCount: {gameLogic.EntityCount}");

            ImGui.Text($"Player Position: {inputSystem.CurrentPosition}");
            ImGui.SameLine();
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
        }

        ImGui.End();

        if (_managementInfoEnabled && ImGui.Begin("Network Management", ref _managementInfoEnabled))
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

            if (!clientNetwork.HasPacketCaptureOrReplay)
            {
                ImGui.Text("Not Capturing or Replaying");
                if (ImGui.Button("Capture Network"))
                {
                    clientNetwork.BeginCapture();
                }
                if (ImGui.Button("Start Replay"))
                {
                    Task.Run(async () => { try { await clientNetwork.BeginReplay("network_capture.aqrep"); } catch (Exception e) { Log.Error(e, "Failed to start network replay"); } });
                }
            }
            else
            {
                if (clientNetwork.Capturer!.IsCapturing)
                {
                    ImGui.Text($"Capturing Packets: {clientNetwork.Capturer.CapturedPackets.Count} packets captured");
                    if (ImGui.Button("Stop Capture"))
                    {
                        Task.Run(async () => { try { await clientNetwork.SaveCapture("network_capture.aqrep"); } catch (Exception e) { Log.Error(e, "Failed to save network capture"); } });
                    }
                }
                else
                {
                    if (clientNetwork.Capturer.ReplayCompleted)
                    {
                        ImGui.Text("Replay Completed");
                    }
                    else
                    {
                        ImGui.Text($"Replaying Packets: {clientNetwork.Capturer.ReplayIndex} / {clientNetwork.Capturer.CapturedPackets.Count} packets replayed");
                        ImGui.Text("Next Packet in  " +
                                   $"{(clientNetwork.Capturer.TryPeekNextPacket(out var nextPacket) ? nextPacket!.Delta.TotalMilliseconds : 0):0} ms");
                        if (ImGui.Button("Stop Replay"))
                        {
                            clientNetwork.StopReplay();
                        }
                    }
                }
            }

            if (ImGui.Button("Spawn Test Entities"))
            {
                managementClient.SpawnTestEntities();
            }
        }

        ImGui.End();
    }

    public void RenderMenu()
    {
        if (ImGui.BeginMenu("Network"))
        {
            ImGui.MenuItem("Network Info", null, ref _networkInfoEnabled);
            ImGui.MenuItem("Management Info", null, ref _managementInfoEnabled);
            ImGui.EndMenu();
        }
    }
}