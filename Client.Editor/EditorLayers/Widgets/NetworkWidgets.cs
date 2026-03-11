using System.Numerics;
using Client.Editor.Management;
using Client.Game;
using Client.Networking;
using Client.Renderer;
using Client.Wgsl;
using Common.Ecs;
using Hexa.NET.ImGui;
using Serilog;

namespace Client.Editor.EditorLayers.Widgets;

public class NetworkWidgets(
    ClientGameLogic gameLogic,
    NClientEcsWorld world,
    PlayerInputSystem inputSystem,
    ManagementClient managementClient,
    ClientNetworkManager clientNetwork,
    Scene scene) : IEditorWidget
{
    private bool _networkInfoEnabled = true, _managementInfoEnabled = true;
    private bool serverRegionGizmosEnabled = false;

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
                    Task.Run(async () =>
                    {
                        try
                        {
                            await clientNetwork.BeginReplay("network_capture.aqrep");
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Failed to start network replay");
                        }
                    });
                }
            }
            else
            {
                if (clientNetwork.Capturer!.IsCapturing)
                {
                    ImGui.Text($"Capturing Packets: {clientNetwork.Capturer.CapturedPackets.Count} packets captured");
                    if (ImGui.Button("Stop Capture"))
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                await clientNetwork.SaveCapture("network_capture.aqrep");
                            }
                            catch (Exception e)
                            {
                                Log.Error(e, "Failed to save network capture");
                            }
                        });
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
                        ImGui.Text(
                            $"Replaying Packets: {clientNetwork.Capturer.ReplayIndex} / {clientNetwork.Capturer.CapturedPackets.Count} packets replayed");
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

    private readonly Vector4[] _colors =
    [
        new(1, 0, 0, 1),
        new(0, 1, 0, 1),
        new(0, 0, 1, 1),
        new(1, 1, 0, 1),
        new(1, 0, 1, 1),
        new(0, 1, 1, 1)
    ];

    public void RenderMenu()
    {
        if (ImGui.BeginMenu("Network"))
        {
            ImGui.MenuItem("Network Info", string.Empty, ref _networkInfoEnabled);
            ImGui.MenuItem("Management Info", string.Empty, ref _managementInfoEnabled);
            if (ImGui.MenuItem("Server Region Rendering", string.Empty, ref serverRegionGizmosEnabled))
            {
                if (scene.RenderableInstances.GizmoMeshes.FirstOrDefault() is { } gizmos)
                {
                    foreach (var perServerManagementWorker in managementClient.Servers)
                    {
                        if (serverRegionGizmosEnabled)
                        {
                            var index = (u32)gizmos.InstanceInfos.Length;
                            if (perServerManagementWorker.Value.RegionGizmoEntityId is null)
                            {
                                perServerManagementWorker.Value.RegionGizmoEntityId = index;
                                gizmos._instanceInfos.Add(new GizmoShader.Instance());
                            }
                            else
                            {
                                index = perServerManagementWorker.Value.RegionGizmoEntityId.Value;
                            }

                            var instance = new GizmoShader.Instance();
                            var area = perServerManagementWorker.Value.Region.Area;
                            // Convert region min/max (in chunk coords) to world-space using grid chunk size
                            var chunkSize = gameLogic.Grid?.GridInfo.ChunkSize.ToVector4().AsVector3() ??
                                            new Vector3(16, 16, 16);
                            var minChunks = area.Min.ToVector4().AsVector3();
                            var maxChunks = area.Max.ToVector4().AsVector3();
                            var minWorld = minChunks * chunkSize;
                            var maxWorld = maxChunks * chunkSize;
                            var sizeWorld = maxWorld - minWorld; // exclusive max -> correct full size in world units
                            var centerWorld = minWorld + maxWorld;
                            sizeWorld.Y = 0.0001f;
                            centerWorld.Y = -5.0f;
                            instance.SetTransform(Matrix4x4.CreateScale(sizeWorld) *
                                                  Matrix4x4.CreateTranslation(centerWorld));
                            instance.HighlightColorRef =
                                _colors[(perServerManagementWorker.Key.Value - 1) % _colors.Length];
                            instance.HighlightColorRef.W *= 0.5f;
                            gizmos.InstanceInfos[(i32)index] = instance;
                        }
                        else
                        {
                            if (perServerManagementWorker.Value.RegionGizmoEntityId is { } entityId)
                            {
                                gizmos.InstanceInfos[(i32)entityId] = new GizmoShader.Instance();
                            }
                        }
                    }
                }
            }

            ImGui.EndMenu();
        }
    }
}