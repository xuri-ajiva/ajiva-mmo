using Common.Networking;
using client;
using Common.Ecs;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Client.LayerSystem;
using Client.Renderer.Pipelines;
using Client.Renderer.Resources;
using Client.Renderer.VirtualFramebuffers;
using Client.Wgsl;
using GlmSharp;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.PbrRenderer;

public class Instanced3dDeferredRenderer(
    ILogger<Instanced3dDeferredRenderer> logger,
    IShaderManager shaderManager,
    RendererApi api,
    GameLogic gameLogic,
    ResourceLoader loader,
    ICamera camera,
    ObjectScene scene)
    : IRenderLayer, IUiRenderLayer
{
    private readonly ILogger<Instanced3dDeferredRenderer> _logger = logger;
    private readonly IShaderManager _shaderManager = shaderManager;
    private readonly RendererApi _api = api;
    private readonly ResourceLoader _loader = loader;
    private readonly GameLogic _gameLogic = gameLogic;

    private readonly RenderableMeshCollection _dynamicMeshes = new();
    private u64 _transformVersion;
    private u64 _renderableMeshVersion;
    private u64 _sceneVersion;
    private ObjectScene _scene = scene;

    private GBuffer? _sceneGBuffer;
    private RenderTexture? _finalSceneTexture;
    private SceneLights _sceneLights = null!;
    private AjivaBuffer<PbrDeferredLightingShader.LightData> _lightsBuffer;
    private readonly ICamera _camera = camera;

    public ref bool Enabled => ref _enabled;

    private bool _isSceneViewHovered;
    private CompositorPipeline? _compositorPipeline;
    private PbrPipeline? _pbrPipeline;
    private PixelSamplingPipeline? _samplingPipeline;
    private WboitPipeline? _transparentPipeline;
    private bool _enabled = true;
    private ObjectSamplingShader.Result? _underCursor;
    private u32 _selectedObjectId;
    private u32 _selectedInstanceId;
    private Vector2 _mousePosition;

    public void Attach()
    {
        _camera.Attach();
        _logger.LogDebug("Creating View Buffers");

        _sceneGBuffer = GBuffer.Create(
            _api.Device,
            // PBR Formats
            baseColorAlphaFormat: TextureFormat.Bgra8Unorm, // Index 0
            worldNormalFormat: TextureFormat.Rgb10A2Unorm, // Index 1
            metallicRoughnessAoFormat: TextureFormat.Rgba8Unorm, // Index 2
            emissiveFormat: TextureFormat.Rgba16float, // Index 3
            // Other Formats
            objectAndInstanceIdFormat: TextureFormat.RG32Uint, // Index 4
            accumFormat: TextureFormat.Rgba32float, // Index 5
            revealFormat: TextureFormat.R8Unorm, // Index 7
            // Depth/Stencil
            depthStencilFormat: TextureFormat.Depth32float, // Or Depth24PlusStencil8
            label: "SceneGBuffer_PBR"
        );

        _finalSceneTexture =
            RenderTexture.Create(_api.Device, _sceneGBuffer.BaseColorAlphaFormat, label: "FinalSceneTexture");

        _logger.LogDebug("Creating Model Buffers");

        _logger.LogDebug("Creating uniform resources");
        {
            using var albedo = _loader.AsyncTextureLoad("resources/Temple/TempleStone_alb.png");
            using var normal = _loader.AsyncTextureLoad("resources/Temple/TempleStone_nrm.png");
            using var emission = _loader.AsyncTextureLoad("resources/Temple/TempleStone_emm.png");
        }

        _sceneLights = new SceneLights();
        _lightsBuffer = _loader.CreateBuffer(BufferUsage.Storage | BufferUsage.CopyDst, _sceneLights.LightData,
            "LightBuffer");

        CreatePipelines();

        _logger.LogDebug("Done");
    }

    [MemberNotNull(nameof(_compositorPipeline), nameof(_pbrPipeline), nameof(_samplingPipeline),
        nameof(_transparentPipeline))]
    private void CreatePipelines()
    {
        var wboitAccum = _shaderManager.GetCurrentShader<IWboitAccumShader>();
        var compositor = _shaderManager.GetCurrentShader<ICompositorShader>();
        var pbrDeferred = _shaderManager.GetCurrentShader<IPbrDeferredLightingShader>();
        var pbrGBuffer = _shaderManager.GetCurrentShader<IPbrGBufferShader>();
        var objectSamplingShader = _shaderManager.GetCurrentShader<IObjectSamplingShader>();

        if (_pbrPipeline?.IsUpToDate(pbrGBuffer, pbrDeferred) != true)
        {
            _pbrPipeline?.Dispose();
            _pbrPipeline = PbrPipeline.Create(_api, _loader, pbrGBuffer, pbrDeferred, _sceneGBuffer!);
        }

        if (_compositorPipeline?.IsUpToDate(compositor) != true)
        {
            _compositorPipeline?.Dispose();
            _compositorPipeline = CompositorPipeline.Create(_api, compositor, _sceneGBuffer!.BaseColorAlphaFormat);
        }

        if (_samplingPipeline?.IsUpToDate(objectSamplingShader) != true)
        {
            _samplingPipeline?.Dispose();
            _samplingPipeline = PixelSamplingPipeline.Create(_api, objectSamplingShader);
        }

        if (_transparentPipeline?.IsUpToDate(wboitAccum) != true)
        {
            _transparentPipeline?.Dispose();
            _transparentPipeline = WboitPipeline.Create(_api, _loader, wboitAccum,
                _sceneGBuffer!);
        }
    }

    public void Update(UpdateInfo info)
    {
        _sceneLights.Animate(info.DeltaTime);
    }

    public void Render(UpdateInfo info, RenderTarget target, Extent3D sceneViewport)
    {
        var queue = target.QueuePtr;
        _camera.AspectRatio = sceneViewport.Width / (f32)sceneViewport.Height;
        _pbrPipeline!.WriteGBufferSceneUniforms(queue, new PbrGBufferShader.SceneUniform
        {
            ViewRef = _camera.ViewMatrix,
            ProjectionRef = _camera.ProjectionMatrix,
            CameraPositionRef = _camera.Position,
            Time = info.TotalTime
        });
        _transparentPipeline!.WriteUniformBuffer(queue, new WboitAccumShader.SceneUniform
        {
            ViewProjectionRef = _camera.ViewProjectionMatrix,
            CameraPositionRef = _camera.Position,
            Time = info.TotalTime
        });

        if (_sceneVersion != _scene.Version)
        {
            _sceneVersion = _scene.Version;
            foreach (var buffer in _scene.RenderableMeshes.OpaqueMeshes)
            {
                WriteBuffer(queue, buffer);
                _pbrPipeline.RecreateMaterialBindGroup(buffer);
            }

            foreach (var buffer in _scene.RenderableMeshes.TransparentMeshes)
            {
                WriteBuffer(queue, buffer);
                _transparentPipeline.RecreateBindGroup(buffer);
            }
        }

        var transformStore = _gameLogic.GetWorld().GetStore<TransformComponent>();
        var renderableStore = _gameLogic.GetWorld().GetStore<RenderableMeshComponent>();

        if (_transformVersion != transformStore.Version || _renderableMeshVersion != renderableStore.Version)
        {
            _transformVersion = transformStore.Version;
            _renderableMeshVersion = renderableStore.Version;

            // Clear the old dynamic meshes
            foreach (var mesh in _dynamicMeshes.OpaqueMeshes) mesh.Dispose();
            _dynamicMeshes.OpaqueMeshes.Clear();

            // Group entities by mesh
            var entitiesByMesh = new Dictionary<u32, List<TransformComponent>>();
            var renderables = renderableStore.GetAll();
            for (var i = 0; i < renderables.Length; i++)
            {
                var renderable = renderables[i];
                if (!entitiesByMesh.TryGetValue(renderable.MeshId, out var list))
                {
                    list = new List<TransformComponent>();
                    entitiesByMesh[renderable.MeshId] = list;
                }

                list.Add(transformStore.Get(new Entity((int)renderable.MeshId)));
            }

            // Create a mesh buffer for each mesh
            foreach (var (meshId, transforms) in entitiesByMesh)
            {
                if (transforms.Count > 0)
                {
                    var dynamicMesh = new PbrInstancedMeshBuffer(_scene.RenderableMeshes.OpaqueMeshes[(int)meshId].Mesh,
                        ModelHelper.PbrMaterialExamples.WornLeather(),
                        _loader.CreateBuffer<PbrGBufferShader.PbrMaterialUniform>(
                            BufferUsage.Uniform | BufferUsage.CopyDst, 1, label: "ModelUniformBuffer"),
                        new PbrGBufferShader.Instance[transforms.Count]);
                    for (var i = 0; i < transforms.Count; i++)
                    {
                        dynamicMesh.InstanceInfos[i].SetTransform(Matrix4x4.CreateTranslation(transforms[i].Position));
                    }

                    _dynamicMeshes.OpaqueMeshes.Add(dynamicMesh);
                }
            }
        }

        //UpdateScene(queue, _scene.StaticMeshes);

        UpdateScene(queue, _dynamicMeshes);

        _lightsBuffer.QueuedWrite(_sceneLights.LightData, queue);

        if (_pbrPipeline.BeginGBufferPass() is { } pass)
        {
            foreach (var meshBuffer in _scene.RenderableMeshes.OpaqueMeshes)
            {
                _pbrPipeline.DrawMesh(pass, meshBuffer);
            }

            foreach (var meshBuffer1 in _dynamicMeshes.OpaqueMeshes)
            {
                _pbrPipeline.DrawMesh(pass, meshBuffer1);
            }

            queue.Submit(_pbrPipeline.EndGBufferPass(pass));
        }

        if (_transparentPipeline.BeginRenderPass(sceneViewport) is { } transparentPass)
        {
            foreach (var instances in _scene.RenderableMeshes.TransparentMeshes)
            {
                _transparentPipeline.DrawMesh(transparentPass, instances);
            }

            foreach (var instances in _dynamicMeshes.TransparentMeshes)
            {
                _transparentPipeline.DrawMesh(transparentPass, instances);
            }

            queue.Submit(_transparentPipeline.EndRenderPass(transparentPass));
        }
    }


    private void UpdateScene(QueuePtr queue, RenderableMeshCollection dynamicMeshes)
    {
        foreach (var buffer in dynamicMeshes.OpaqueMeshes)
        {
            WriteBuffer(queue, buffer);
            _pbrPipeline!.RecreateMaterialBindGroup(buffer);
        }

        foreach (var buffer in dynamicMeshes.TransparentMeshes)
        {
            WriteBuffer(queue, buffer);
            _transparentPipeline!.RecreateBindGroup(buffer);
        }
    }

    private void WriteBuffer<T1, T2, T3>(QueuePtr queue,
        IInstancedMeshBuffer<T1, T2, T3> buffer) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
    {
        buffer.UniformBuffer.QueuedWrite(new ReadOnlySpan<T3>(ref buffer.MaterialUniform),
            queue);

        ReadOnlySpan<T2> data = buffer.InstanceInfos;
        buffer.InstanceBuffer ??= _loader.CreateBuffer<T2>(BufferUsage.Vertex | BufferUsage.CopyDst, data.Length,
            "DynamicRecreated::IndexBuffer");

        if (buffer.InstanceBuffer.Size < (u64)data.Length * (u64)Unsafe.SizeOf<T2>())
        {
            buffer.InstanceBuffer.ResizeWithoutDataPreserve((u64)data.Length * (u64)Unsafe.SizeOf<T2>());
        }

        buffer.InstanceBuffer.QueuedWrite(data, queue);
    }

    public void Render(UpdateInfo info, RenderTarget target)
    {
        if (_isSceneViewHovered)
        {
            _camera.Update(info);
            if (_underCursor is { } pos && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                _selectedObjectId = pos.ObjectId;
                _selectedInstanceId = pos.InstanceId;
            }

            _mousePosition = ImGui.GetMousePos() -
                             (ImGui.GetCursorPos() + ImGui.GetWindowPos()) * ImGui.GetWindowDpiScale();
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        if (!ImGui.Begin("Scene")) return;
        if (_sceneGBuffer is null || _finalSceneTexture is null)
        {
            _logger.LogError("Scene Buffers not created");
            return;
        }

        var size = ImGui.GetContentRegionAvail();

        //var imDrawListPtr = ImGui.GetBackgroundDrawList();
        //var size = imDrawListPtr.GetClipRectMax();
        u32 width = (u32)size.X, height = (u32)Math.Max(0, size.Y);

        var prevWidth = _finalSceneTexture.Width;
        var prevHeight = _finalSceneTexture.Height;
        var hasPrev = _finalSceneTexture.TryGetTargets(out var prevView);
        _sceneGBuffer.EnsureSize(width, height);
        _finalSceneTexture.EnsureSize(width, height);

        CreatePipelines();
        var mappingTask = _samplingPipeline.RequestResultBufferMapping();
        _api.Poll();
        _underCursor = _samplingPipeline.ReadResult(mappingTask);


        if (_sceneGBuffer.TryGetCompositorOutputView(out var emissive, out var objectAndInstanceId, out var accum,
                out var reveal, out var depth) && _finalSceneTexture.TryGetTargets(out var colorOutput))
        {
            var queue = target.QueuePtr;
            Render(info, target, new Extent3D(width, height, 1));

            _samplingPipeline.DispatchPixelRead(queue, new uvec2((u32)Math.Clamp(_mousePosition.X, 0, width - 1),
                (u32)Math.Clamp(_mousePosition.Y, 0, height - 1)), depth.DepthOnlyView, objectAndInstanceId.View);

            if (!Matrix4x4.Invert(_camera.ProjectionMatrix, out var invViewProjection))
            {
                _logger.LogError("Failed to invert projection matrix");
                return;
            }

            if (!Matrix4x4.Invert(_camera.ViewMatrix, out var invView))
            {
                _logger.LogError("Failed to invert view matrix");
                return;
            }

            _pbrPipeline.WriteDeferredSceneUniforms(queue, new PbrDeferredLightingShader.SceneUniforms
            {
                InverseProjectionRef = invViewProjection,
                InverseViewRef = invView,
                CameraPositionWorldRef = camera.Position,
                LightCount = _sceneLights.LightCount,
                AmbientColorRef = new Vector3(0.1f, 0.1f, 0.1f),
                IblIntensity = 1f,
            });

            _pbrPipeline.ApplyDeferredLighting(queue, _lightsBuffer.Buffer, _sceneLights.LightCount);

            _compositorPipeline.Apply(queue, emissive.View, depth.DepthOnlyView, colorOutput.View, accum.View,
                reveal.View);

            if (prevWidth != width && prevHeight != height && hasPrev)
                target.ImguiController.RemoveTextureBindGroup(prevView.View);

            //imDrawListPtr.AddImage(color0.GetIntPtr(), new Vector2(0, 0), new Vector2(width, height));

            ImGui.Image(colorOutput.View.GetIntPtr(), new Vector2(width, height));
            _isSceneViewHovered = ImGui.IsItemHovered();
        }

        ImGui.End();
        ImGui.PopStyleVar();
    }

    public void RenderUi(UpdateInfo info)
    {
        if (ImGui.Begin("Controls"))
        {
            if (ImGui.Button("Collect Resources"))
            {
                _loader.DoCollection();
            }

            if (ImGui.Button("Load Sponza"))
            {
                _scene.LoadLevel("Sponza");
            }

            if (ImGui.Button("Unlaod Level"))
            {
                _scene.UnloadLevel();
            }

            ImGui.Text("Under Mouse Sample:");
            if (_underCursor is { } selected)
            {
                ImGui.Text($"Object ID: {selected.ObjectId}");
                ImGui.Text($"Instance ID: {selected.InstanceId}");
                ImGui.Text($"Depth: {selected.Depth}");
            }
            else
            {
                ImGui.Text("No object under mouse");
            }

            if (_selectedObjectId != 0 && _selectedObjectId <= _dynamicMeshes.OpaqueMeshes.Count)
            {
                var selectedBuffer = _dynamicMeshes.OpaqueMeshes[(int)_selectedObjectId - 1];

                var y = ImGui.GetCursorPosY();
                ImGui.Text("Textures");
                var imageButtonSize = ImGui.GetContentRegionAvail().X;
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
                            _loader.AsyncTextureLoad(pickResult.Path, clone);
                            _loader.DisposeLater(selectedBuffer.BaseColorTexture);
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
                            _loader.AsyncTextureLoad(pickResult.Path, clone);
                            _loader.DisposeLater(selectedBuffer.NormalTexture);
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
                            _loader.AsyncTextureLoad(pickResult.Path, clone);
                            _loader.DisposeLater(selectedBuffer.EmissiveTexture);
                            selectedBuffer.EmissiveTexture = clone;
                        });
                    }
                }

                ref var uniform = ref selectedBuffer.MaterialUniform;
                ImGui.Text(uniform.ToString());

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
            foreach (var buffer in _dynamicMeshes.OpaqueMeshes)
            {
                for (var i = 0; i < buffer.InstanceInfos.Length; i++)
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

        if (ImGui.Begin("DefTexture"))
        {
            ImGui.Image(_loader.DefaultTexture.View.GetIntPtr(), new Vector2(256, 256));
            ImGui.Image(_loader.DefaultNormal.View.GetIntPtr(), new Vector2(256, 256));
            ImGui.Image(_loader.DefaultRoughness.View.GetIntPtr(), new Vector2(256, 256));
        }

        ImGui.End();
    }

    public void Detach()
    {
        _sceneGBuffer?.Release();
        _finalSceneTexture?.Release();

        _dynamicMeshes.Dispose();

        _compositorPipeline?.Dispose();
        _pbrPipeline?.Dispose();

        _lightsBuffer.Buffer.Destroy();

        // set all null
        _sceneGBuffer = null;
        _finalSceneTexture = null;

        _dynamicMeshes.Dispose();
        _compositorPipeline = null;
        _pbrPipeline = null;
        _lightsBuffer = null!;
    }
}
