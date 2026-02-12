using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Client.Renderer;
using Client.Renderer.Instancing;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Common;
using Common.Ecs;
using Common.Systems;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using Silk.NET.WebGPU.Safe;

namespace Client.Editor.EditorLayers.Widgets;

public class EntityWidgets(
    ILogger<EntityWidgets> _logger,
    NClientEcsWorld world,
    IRenderableInstanceCollectionBuilder renderBuilder,
    Instanced3dDeferredRenderer renderer,
    ResourceManager loader,
    Scene scene,
    PeriodicUpdater periodicUpdater) : IEditorWidget
{
    private bool _entityInspectorEnabled = true, _controlsEnabled = true, _resourceLoaderEnabled = true;
    private EntityId _selectedEntityId = EntityId.Invalid;
    private u32 _selectedObjectId = 0;
    private u32 _selectedInstanceId = 0;
    private f32 _textureSize = 128f;
    private i32 _maxConcurrentLoads = 4;

    public void Render()
    {
        using var tracer = Instrumentation.Instance?.StartActivity(ActivityKind.Internal);
        if (_entityInspectorEnabled && ImGui.Begin("Entity Inspector", ref _entityInspectorEnabled))
        {
            ImGui.DragInt("##EntityId", ref Unsafe.As<EntityId, i32>(ref _selectedEntityId));
            if (_selectedEntityId != EntityId.Invalid &&
                world.Transform.Has(_selectedEntityId))
            {
                ref var transform = ref world.Transform.Update(_selectedEntityId);
                ImGui.DragFloat3("##EntityPosition", ref transform.Position);

                ImGui.DragFloat4("##EntityRotation", ref Unsafe.As<Quaternion, Vector4>(ref transform.Rotation));

                ImGui.DragFloat3("##EntityScale", ref transform.Scale);

                if (world.NetState.Has(_selectedEntityId))
                {
                    var netState = world.NetState.Get(_selectedEntityId);
                    ImGui.Text($"Network ID: {netState.Id}");
                    ImGui.Text($"CreatedTick: {netState.CreatedTick}");
                    ImGui.Text($"DeletedTick: {netState.DeletedTick}");
                    ImGui.Text($"GhostChunk: {netState.GhostChunk}");
                }
                else
                {
                    ImGui.Text("No Network State");
                }

                if (world.Grid?.GetChunkCoord(transform.Position, transform.Level) is { } chunk)
                {
                    ImGui.Text($"Chunk Coord: {chunk}");
                    var spatialChunk = world.Grid.GetChunkData(chunk);
                    ImGui.Text($"Chunk Entities: {spatialChunk.Entities.Count}");
                    ImGui.Text($"Chunk Dirty: {spatialChunk.IsDirty}");
                    ImGui.Text($"Contains Entity: {spatialChunk.Entities.Contains(_selectedEntityId)}");
                }
                else
                {
                    ImGui.Text("No Spatial Chunk");
                }
            }
            else
            {
                ImGui.Text("Entity has no Position component");
            }
        }

        ImGui.End();

        if (_controlsEnabled && ImGui.Begin("Controls", ref _controlsEnabled))
        {
            ImGui.InputFloat("SmoothingFrames", ref renderBuilder.SmoothingFrames, 0.1f, 1f);
            //ImGui.Text($"SFrame: {1f - MathF.Pow(0.01f, info.DeltaTime * renderBuilder.SmoothingFrames)}");
            var dynamicInstances = renderer.DynamicInstances;
            ImGui.Text("Rendering Stats:");
            ImGui.Text($"Opaque Instancing: {dynamicInstances.OpaqueMeshes.Count}");
            ImGui.Text($"Opaque Total Instances: {dynamicInstances.OpaqueMeshes.Sum(x => x._instanceInfos.Count)}");
            ImGui.TextWrapped($"Instance Builder: {renderBuilder.Stats()}");
            if (ImGui.Button("ReInitialize Renderable Meshes"))
            {
                renderBuilder.RequestReBuild();
            }

            if (ImGui.Button("Trace Renderer"))
            {
                periodicUpdater.TraceRenderer = true;
            }

            ImGui.SameLine();
            if (ImGui.Button("Trace Update"))
            {
                periodicUpdater.TraceUpdate = true;
            }

            ImGui.Separator();

            if (ImGui.Button("Collect Resources"))
            {
                loader.DoCollection();
            }

            if (ImGui.Button("Load Sponza"))
            {
                scene.LoadLevel("Sponza");
            }

            if (ImGui.Button("Unlaod Level"))
            {
                scene.UnloadLevel();
            }

            ImGui.Text("Under Mouse Sample:");
            ImGui.Text($"Mouse Position: {ImGui.GetMousePos()}");
            if (renderer.PixelReadResult is { } pos && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                _selectedObjectId = pos.ObjectId;
                _selectedInstanceId = pos.InstanceId;
                _selectedEntityId = renderBuilder.GetEntityId(_selectedObjectId, _selectedInstanceId);
            }

            if (renderer.PixelReadResult is { } selected)
            {
                ImGui.Text($"Object ID: {selected.ObjectId}");
                ImGui.Text($"Instance ID: {selected.InstanceId}");
                ImGui.Text($"Depth: {selected.Depth}");
                var selRenderEnt = renderBuilder.GetEntityId(_selectedObjectId, _selectedInstanceId);
                ImGui.Text($"Entity ID: {selRenderEnt}");
            }
            else
            {
                ImGui.Text("No object under mouse");
            }

            if (_selectedObjectId != 0 && _selectedObjectId <= dynamicInstances.OpaqueMeshes.Count)
            {
                var y = ImGui.GetCursorPosY();
                ImGui.Text("Textures");
                var imageButtonSize = ImGui.GetContentRegionAvail().X;
                var selectedBuffer = dynamicInstances.OpaqueMeshes[(i32)_selectedObjectId - 1];
                if (selectedBuffer.BaseColorTexture is { } texture)
                {
                    if (ImGui.ImageButton("ChooseTextureA", texture.View.GetIntPtr(),
                            new Vector2(imageButtonSize)))
                    {
                        Task.Run(() =>
                        {
                            var pickResult = Nfd.FileOpen([
                                new NfdFilter { Description = "Image files", Specification = "png,jpg,jpeg,bmp" }
                            ]);

                            if (pickResult.Status != NfdStatus.Ok) return;

                            using var clone = texture.Clone("TMP_CLONE_FOR_UI");
                            loader.AsyncTextureLoad(pickResult.Path, clone);
                            loader.DisposeLater(selectedBuffer.BaseColorTexture);
                            selectedBuffer.BaseColorTexture = clone;
                        });
                    }
                }

                if (selectedBuffer.NormalTexture is { } normalTexture)
                {
                    if (ImGui.ImageButton("ChooseTextureN", normalTexture.View.GetIntPtr(),
                            new Vector2(imageButtonSize)))
                    {
                        Task.Run(() =>
                        {
                            var pickResult = Nfd.FileOpen([
                                new NfdFilter { Description = "Image files", Specification = "png,jpg,jpeg,bmp" }
                            ]);

                            if (pickResult.Status != NfdStatus.Ok) return;

                            using var clone = normalTexture.Clone("TMP_CLONE_FOR_UI");
                            loader.AsyncTextureLoad(pickResult.Path, clone);
                            loader.DisposeLater(selectedBuffer.NormalTexture);
                            selectedBuffer.NormalTexture = clone;
                        });
                    }
                }

                if (selectedBuffer.EmissiveTexture is { } emission)
                {
                    if (ImGui.ImageButton("ChooseTextureE", emission.View.GetIntPtr(),
                            new Vector2(imageButtonSize)))
                    {
                        Task.Run(() =>
                        {
                            var pickResult = Nfd.FileOpen([
                                new NfdFilter { Description = "Image files", Specification = "png,jpg,jpeg,bmp" }
                            ]);

                            if (pickResult.Status != NfdStatus.Ok) return;

                            using var clone = emission.Clone("TMP_CLONE_FOR_UI");
                            loader.AsyncTextureLoad(pickResult.Path, clone);
                            loader.DisposeLater(selectedBuffer.EmissiveTexture);
                            selectedBuffer.EmissiveTexture = clone;
                        });
                    }
                }

                //ImGui.Text(selectedBuffer.MaterialUniform.ToString());

                if (_selectedInstanceId < selectedBuffer.InstanceInfos.Length)
                {
                    ref var selectedInstance = ref selectedBuffer.InstanceInfos[(i32)_selectedInstanceId];
                    ImGui.Text($"Instance");
                    ImGui.DragFloat4($"##Position", ref selectedInstance.Transform0Ref, 0.1f);
                    ImGui.DragFloat4($"##Rotation", ref selectedInstance.Transform1Ref, 0.1f);
                    ImGui.DragFloat4($"##Scale", ref selectedInstance.Transform2Ref, 0.1f);
                    ImGui.DragFloat4($"##ScaleA", ref selectedInstance.Transform3Ref, 0.1f);
                    ImGui.Text("Highlight Color");
                    ImGui.ColorEdit4($"##HighlightColor", ref selectedInstance.HighlightColorRef);
                }
            }


            ImGui.NextColumn();
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Text("Tint");
            ImGui.NextColumn();
            //ImGui.ColorEdit4("##Tint", ref _cubeColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha);
            ImGui.Spacing();
            ImGui.Separator();

            ImGui.Columns(1);
            var hi = 0;
            foreach (var buffer in dynamicInstances.OpaqueMeshes)
            {
                if (hi > 50) break;
                for (var i = 0; i < buffer.InstanceInfos.Length && i < 100; i++)
                {
                    ref var instance = ref buffer.InstanceInfos[i];
                    ImGui.Text($"Instance {hi}");
                    ImGui.DragFloat4($"##Position{hi}", ref instance.Transform0Ref, 0.1f);
                    ImGui.DragFloat4($"##Rotation{hi}", ref instance.Transform1Ref, 0.1f);
                    ImGui.DragFloat4($"##Scale{hi}", ref instance.Transform2Ref, 0.1f);
                    ImGui.DragFloat4($"##ScaleA{hi}", ref instance.Transform3Ref, 0.1f);
                    ImGui.Text("Highlight Color");
                    ImGui.ColorEdit4($"##HighlightColor{hi}", ref instance.HighlightColorRef);
                    hi++;
                }
            }
        }

        ImGui.End();

        if (_resourceLoaderEnabled && ImGui.Begin("ResourceLoader", ref _resourceLoaderEnabled))
        {
            var current = _maxConcurrentLoads;
            ImGui.SliderInt("Max Concurrent Loads", ref _maxConcurrentLoads, 1, 64);
            if (current != _maxConcurrentLoads)
                loader.UpdateMaxConcurrentLoads(_maxConcurrentLoads);

            if (ImGui.CollapsingHeader("Textures"))
            {
                ImGui.SliderFloat("Size", ref _textureSize, 16, 1024);
                const f32 padding = 10f;
                var avail = ImGui.GetContentRegionAvail();
                var imagesPerRow = Math.Max(1, (i32)(avail.X / (_textureSize + padding)));
                var imageCount = 0;

                ImGui.Text("Default Textures");

                void RenderTexture(IntPtr texture)
                {
                    ImGui.Image(texture, new Vector2(_textureSize, _textureSize));
                    imageCount++;
                    if (imageCount % imagesPerRow != 0)
                    {
                        ImGui.SameLine();
                    }
                }

                // Show default textures
                RenderTexture(loader.DefaultTexture.View.GetIntPtr());
                RenderTexture(loader.DefaultNormal.View.GetIntPtr());
                RenderTexture(loader.DefaultRoughness.View.GetIntPtr());

                ImGui.Separator();
                imageCount = 0;

                // Show other textures
                foreach (var ajivaTexture in loader.GetAllTextures())
                {
                    RenderTexture(ajivaTexture.View.GetIntPtr());
                }
            }

            ImGui.NewLine();

            if (ImGui.CollapsingHeader("Meshes"))
            {
                // meshes
                ImGui.Separator();
                ImGui.Text("Default Meshes");
                ImGui.Text(
                    $"Cube: VB: {loader.DefaultPbrCubeMesh.VertexBuffer.Size} IB: {loader.DefaultPbrCubeMesh.IndexBuffer.Size} Refs: {loader.DefaultPbrCubeMesh.Data.RefCount}");
                ImGui.Text(
                    $"TransparentCube: VB: {loader.DefaultTransparentCubeMesh.VertexBuffer.Size} IB: {loader.DefaultTransparentCubeMesh.IndexBuffer.Size}  Refs: {loader.DefaultPbrCubeMesh.Data.RefCount}");

                ImGui.Separator();
                foreach (var mesh in loader.GetAllMeshes())
                {
                    ImGui.Text($"{mesh.Name}");
                    ImGui.Text($"VB: {mesh.VertexBuffer.Size} IB: {mesh.IndexBuffer.Size}  Refs: {mesh.Data.RefCount}");
                }
            }
        }

        ImGui.End();
    }

    public void RenderMenu()
    {
        if (ImGui.BeginMenu("Entities"))
        {
            ImGui.MenuItem("Entity Inspector", null, ref _entityInspectorEnabled);
            ImGui.MenuItem("Controls", null, ref _controlsEnabled);
            ImGui.MenuItem("Resource Loader", null, ref _resourceLoaderEnabled);
            ImGui.EndMenu();
        }
    }
}