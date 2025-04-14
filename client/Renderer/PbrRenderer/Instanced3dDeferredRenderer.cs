using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Client.LayerSystem;
using Client.Renderer.Pipelines;
using Client.Renderer.Resources;
using Client.Renderer.VirtualFramebuffers;
using Client.Wgsl;
using GlmSharp;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.PbrRenderer;

public class Instanced3dDeferredRenderer(
    ILogger<Instanced3dDeferredRenderer> logger,
    IShaderManager shaderManager,
    RendererApi api,
    Ecs ecs,
    ResourceLoader loader,
    ICamera camera,
    ObjectScene scene)
    : IRenderLayer, IUiRenderLayer
{
    private readonly ILogger<Instanced3dDeferredRenderer> _logger = logger;
    private readonly IShaderManager _shaderManager = shaderManager;
    private readonly RendererApi _api = api;
    private readonly ResourceLoader _loader = loader;
    private readonly Ecs _ecs = ecs;


    private PBRInstancedMeshBuffer[] _meshBuffers = [];
    private OITInstancedMeshBuffer[] _transparentMeshBuffers = [];
    private ObjectScene _scene = scene;

    private GBuffer? _sceneGBuffer;
    private RenderTexture? _finalSceneTexture;
    private SceneLights _sceneLights = null!;
    private AjivaBuffer<DeferredShader.LightData> _lightsBuffer;
    private readonly ICamera _camera = camera;

    public ref bool Enabled => ref _enabled;

    private bool _isSceneViewHovered;
    private DeferredPipeline? _deferredPipeline;
    private CompositorPipeline? _compositorPipeline;
    private TemplePipeline? _templePipeline;
    private PixelSamplingPipeline? _samplingPipeline;
    private WboitPipeline? _transparentPipeline;
    private TempleShader.SceneUniform _sceneUniform;
    private bool _enabled = true;
    private ObjectSamplingShader.Result? _underCursor;
    private u32 _selectedObjectId;
    private u32 _selectedInstanceId;

    public void Attach()
    {
        _camera.Attach();
        _logger.LogDebug("Creating View Buffers");

        // TextureFormat.Rgba8Unorm, TextureFormat.Rgb10A2Unorm, TextureFormat.Rgba32float, TextureFormat.Depth24Plus,
        _sceneGBuffer = GBuffer.Create(_api.Device, TextureFormat.Rgba16float, TextureFormat.Rgba8Unorm,
            TextureFormat.Rgb10A2Unorm, TextureFormat.Rgba32float, TextureFormat.RG32Uint, TextureFormat.Rgba32float,
            TextureFormat.R32float, TextureFormat.Depth24Plus, label: "SceneGBuffer");

        _finalSceneTexture =
            RenderTexture.Create(_api.Device, TextureFormat.Rgba8Unorm, label: "FinalSceneTexture");

        _logger.LogDebug("Creating Model Buffers");
        //_meshBuffers = CreateModelBuffers("resources/Temple/TempleModel.glb", "Temple");
        _meshBuffers = _scene.CreateModelBuffers("resources/SponzaAtrium/sponza.obj", "SponzaAtrium");
        _transparentMeshBuffers = _scene.CreateTransparentCubes();

        _logger.LogDebug("Creating uniform resources");
        {
            using var albedo = _loader.AsyncTextureLoad("resources/Temple/TempleStone_alb.png");
            using var normal = _loader.AsyncTextureLoad("resources/Temple/TempleStone_nrm.png");
            using var emission = _loader.AsyncTextureLoad("resources/Temple/TempleStone_emm.png");
            foreach (var instancedMeshBuffer in _meshBuffers)
            {
                instancedMeshBuffer.AlbedoTexture ??= albedo.Reference();
                instancedMeshBuffer.NormalTexture ??= normal.Reference();
                instancedMeshBuffer.EmissionTexture ??= emission.Reference();
            }

            // for testing
            foreach (var instancedMeshBuffer in _transparentMeshBuffers)
            {
                instancedMeshBuffer.EmissionTexture = emission.Reference();
            }
        }

        _sceneLights = new SceneLights();
        _lightsBuffer = _loader.CreateBuffer(BufferUsage.Storage | BufferUsage.CopyDst, _sceneLights.LightData,
            "LightBuffer");


        CreatePipelines();

        foreach (var instancedMeshBuffer in _meshBuffers)
        {
            instancedMeshBuffer.ModelUniformBuffer = _loader.CreateBuffer<TempleShader.ModelUniform>(
                BufferUsage.Uniform | BufferUsage.CopyDst, 1, label: "ModelUniformBuffer");

            _templePipeline.RecreateBindGroup(instancedMeshBuffer);
        }

        foreach (var instancedMeshBuffer in _transparentMeshBuffers)
        {
            instancedMeshBuffer.ModelUniformBuffer = _loader.CreateBuffer<WboitAccumShader.ModelUniform>(
                BufferUsage.Uniform | BufferUsage.CopyDst, 1, label: "ModelUniformBuffer");

            _transparentPipeline.RecreateBindGroup(instancedMeshBuffer);
        }

        _logger.LogDebug("Done");
    }

    [MemberNotNull(nameof(_deferredPipeline), nameof(_compositorPipeline), nameof(_templePipeline),
        nameof(_samplingPipeline), nameof(_transparentPipeline))]
    private void CreatePipelines()
    {
        var wboitAccum = _shaderManager.GetCurrentShader<IWboitAccumShader>();
        var deferred = _shaderManager.GetCurrentShader<IDeferredShader>();
        var compositor = _shaderManager.GetCurrentShader<ICompositorShader>();
        var temple = _shaderManager.GetCurrentShader<ITempleShader>();
        var objectSamplingShader = _shaderManager.GetCurrentShader<IObjectSamplingShader>();

        if (_deferredPipeline?.IsUpToDate(deferred) != true)
        {
            _deferredPipeline?.Dispose();
            _deferredPipeline = DeferredPipeline.Create(_api, deferred, _sceneGBuffer!.LightChannelFormat);
        }

        if (_compositorPipeline?.IsUpToDate(compositor) != true)
        {
            _compositorPipeline?.Dispose();
            _compositorPipeline = CompositorPipeline.Create(_api, compositor, _sceneGBuffer!.AlbedoChannelFormat);
        }

        if (_templePipeline?.IsUpToDate(temple) != true)
        {
            _templePipeline?.Dispose();
            _templePipeline = TemplePipeline.Create(_api, temple, _sceneGBuffer!);
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
        if (_samplingPipeline is not null)
        {
            if (_underCursor is { } pos && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                _selectedObjectId = pos.ObjectId;
                _selectedInstanceId = pos.InstanceId;
            }
        }
    }

    public void Render(UpdateInfo info, RenderTarget target, Extent3D sceneViewport)
    {
        var queue = target.QueuePtr;
        _camera.AspectRatio = sceneViewport.Width / (f32)sceneViewport.Height;
        _sceneUniform = new TempleShader.SceneUniform
        {
            ViewRef = _camera.ViewMatrix,
            ProjectionRef = _camera.ProjectionMatrix,
            CameraPositionRef = _camera.Position,
            Time = info.TotalTime
        };
        _templePipeline!.WriteUniformBuffer(queue, _sceneUniform);
        _transparentPipeline!.WriteUniformBuffer(queue, new WboitAccumShader.SceneUniform
        {
            ViewProjectionRef = _sceneUniform.ViewRef * _sceneUniform.ProjectionRef,
            CameraPositionRef = _camera.Position,
            Time = info.TotalTime
        });

        var players = _ecs.GetPlayers().ToArray();
        if (_meshBuffers[^1].InstanceInfos.Length != players.Length + 1)
            _meshBuffers[^1].InstanceInfos = new TempleShader.Instance[players.Length + 1];

        ref var instanceInfo = ref _meshBuffers[^1].InstanceInfos;
        instanceInfo[0].SetTransform(Matrix4x4.CreateTranslation(_camera.Position));
        _ecs.LocalPos = _camera.Position;
        for (var i = 0; i < players.Length; i++)
        {
            instanceInfo[i + 1].SetTransform(Matrix4x4.CreateTranslation(players[i].Position));
        }

        foreach (var buffer in _meshBuffers)
        {
            WriteBuffer(queue, buffer);
            _templePipeline.RecreateBindGroup(buffer);
        }

        foreach (var buffer in _transparentMeshBuffers)
        {
            WriteBuffer(queue, buffer);
            _transparentPipeline.RecreateBindGroup(buffer);
        }

        _lightsBuffer.QueuedWrite(_sceneLights.LightData, queue);

        if (_templePipeline.BeginRenderPass() is { } pass)
        {
            foreach (var (vertexBuffer, indexBuffer, instanceBuffer, modelBindGroup, instances) in _meshBuffers)
            {
                pass.SetVertexBuffer(0, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Size);
                pass.SetVertexBuffer(1, instanceBuffer!.Buffer, instanceBuffer.Offset,
                    instanceBuffer.Size);

                pass.SetIndexBuffer(indexBuffer.Buffer, IndexFormat.Uint32, indexBuffer.Offset, indexBuffer.Size);

                pass.SetBindGroup(1, modelBindGroup!.Value, ReadOnlySpan<u32>.Empty);

                pass.DrawIndexed((u32)(indexBuffer.Size / sizeof(u32)), (u32)instances.Length, 0, 0, 0);
            }

            queue.Submit(_templePipeline.EndRenderPass(pass));
        }

        if (_transparentPipeline.BeginRenderPass(sceneViewport) is { } transparentPass)
        {
            foreach (var (vertexBuffer, indexBuffer, instanceBuffer, modelBindGroup, instances) in
                     _transparentMeshBuffers)
            {
                transparentPass.SetVertexBuffer(0, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Size);
                transparentPass.SetVertexBuffer(1, instanceBuffer!.Buffer, instanceBuffer.Offset,
                    instanceBuffer.Size);

                transparentPass.SetIndexBuffer(indexBuffer.Buffer, IndexFormat.Uint32, indexBuffer.Offset,
                    indexBuffer.Size);

                transparentPass.SetBindGroup(1, modelBindGroup!.Value, ReadOnlySpan<u32>.Empty);

                transparentPass.DrawIndexed((u32)(indexBuffer.Size / sizeof(u32)), (u32)instances.Length, 0, 0, 0);
            }

            queue.Submit(_transparentPipeline.EndRenderPass(transparentPass));
        }
    }

    private void WriteBuffer<T1, T2, T3>(QueuePtr queue,
        IInstancedMeshBuffer<T1, T2, T3> buffer) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
    {
        buffer.ModelUniformBuffer.QueuedWrite(new ReadOnlySpan<T3>(ref buffer.ModelUniform),
            queue);

        var data = new ReadOnlySpan<T2>(buffer.InstanceInfos);
        if (buffer.InstanceBuffer is null || buffer.InstanceBuffer.Size <
            (u64)data.Length * (u64)Unsafe.SizeOf<T2>())
        {
            buffer.InstanceBuffer?.Dispose();
            buffer.InstanceBuffer = _loader.CreateBuffer(BufferUsage.Vertex | BufferUsage.CopyDst,
                data, "DynamicRecreated::IndexBuffer");
        }
        else
        {
            buffer.InstanceBuffer.QueuedWrite(data, queue);
        }
    }

    public void Render(UpdateInfo info, RenderTarget target)
    {
        if (_isSceneViewHovered)
        {
            _camera.Update(info);
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

        if (_sceneGBuffer.TryGetTargets(out var positions, out var albedo, out var normal, out var emission,
                out var objectAndInstanceId, out var accum, out var reveal, out var depth) &&
            _finalSceneTexture.TryGetTargets(out var colorOutput))
        {
            var queue = target.QueuePtr;
            Render(info, target, new Extent3D(width, height, 1));

            var mousePosition = ImGui.GetMousePos() -
                                (ImGui.GetCursorPos() + ImGui.GetWindowPos()) * ImGui.GetWindowDpiScale();
            _samplingPipeline.DispatchPixelRead(queue, new uvec2((u32)mousePosition.X, (u32)mousePosition.Y),
                depth.DepthOnlyView, objectAndInstanceId.View);

            if (!Matrix4x4.Invert(_camera.ViewProjectionMatrix, out var invViewProjection))
                throw new InvalidOperationException("ViewProjection Matrix could not be inverted");

            _deferredPipeline.Apply(queue, albedo.View, normal.View, depth.DepthOnlyView, emission.View,
                invViewProjection, _lightsBuffer.Buffer, _sceneLights.LightCount);

            _compositorPipeline.Apply(queue, emission.View, depth.DepthOnlyView, colorOutput.View, accum.View,
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

            if (_selectedObjectId != 0 && _selectedObjectId <= _meshBuffers.Length)
            {
                var selectedBuffer = _meshBuffers[_selectedObjectId - 1];

                ImGui.Columns(2, null, false);
                var y = ImGui.GetCursorPosY();
                ImGui.SetCursorPosY(y + ImGui.GetStyle().FramePadding.Y);
                ImGui.Text("Texture");
                ImGui.NextColumn();
                ImGui.SetCursorPosY(y);
                var imageButtonSize = ImGui.GetContentRegionAvail().X;
                if (selectedBuffer.AlbedoTexture is { } texture)
                {
                    if (ImGui.ImageButton("ChooseTexture", texture.View.GetIntPtr(),
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
                            _loader.DisposeLater(selectedBuffer.AlbedoTexture);
                            selectedBuffer.AlbedoTexture = clone;
                        });
                    }
                }

                if (_selectedInstanceId < selectedBuffer.InstanceInfos.Length)
                {
                    ref var selectedInstance = ref selectedBuffer.InstanceInfos[_selectedInstanceId];
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
            foreach (var buffer in _meshBuffers)
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
        }

        ImGui.End();
    }

    public void Detach()
    {
        _sceneGBuffer?.Release();
        _finalSceneTexture?.Release();

        foreach (var buffer in _meshBuffers)
        {
            buffer.Dispose();
        }

        foreach (var buffer in _transparentMeshBuffers)
        {
            buffer.Dispose();
        }

        _compositorPipeline?.Dispose();
        _deferredPipeline?.Dispose();
        _templePipeline?.Dispose();

        _lightsBuffer.Buffer.Destroy();

        // set all null
        _sceneGBuffer = null;
        _finalSceneTexture = null;

        _meshBuffers = [];
        _transparentMeshBuffers = [];
        _compositorPipeline = null;
        _deferredPipeline = null;
        _lightsBuffer = null!;
    }
}
