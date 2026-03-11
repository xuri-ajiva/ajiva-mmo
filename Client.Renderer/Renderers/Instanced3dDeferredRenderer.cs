using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Client.Renderer.Instancing;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Pipelines;
using Client.Renderer.Resources;
using Client.Renderer.VirtualFramebuffers;
using Client.Wgsl;
using Common.Ecs;
using Common.Systems;
using GlmSharp;
using Interception;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.Renderers;

public class Instanced3dDeferredRenderer(
    ILogger<Instanced3dDeferredRenderer> logger,
    IShaderManager shaderManager,
    RendererApi api,
    ResourceManager loader,
    ICamera camera,
    Scene scene,
    IRenderableInstanceCollectionBuilder renderBuilder,
    NClientEcsWorld world)
{
    private readonly ILogger<Instanced3dDeferredRenderer> _logger = logger;
    private readonly IShaderManager _shaderManager = shaderManager;
    private readonly RendererApi _api = api;
    private readonly ResourceManager _loader = loader;
    private readonly Scene _scene = scene;
    private readonly IRenderableInstanceCollectionBuilder _renderBuilder = renderBuilder;
    private readonly NClientEcsWorld _world = world;
    private readonly ICamera _camera = camera;

    internal readonly RenderableInstanceCollection DynamicInstances = new();
    private RenderableInstanceCollection? _sceneRenderCollection;

    private GBuffer? _sceneGBuffer;
    private RenderTexture? _finalSceneTexture;
    private SceneLights _sceneLights = null!;
    private AjivaBuffer<PbrDeferredLightingShader.LightData> _lightsBuffer = null!;

    public ref bool Enabled => ref _enabled;
    public uvec2 PixelReadRequest { get; set; }
    public ObjectSamplingShader.Result? PixelReadResult;

    private CompositorPipeline? _compositorPipeline;
    private PbrPipeline? _pbrPipeline;
    private PixelSamplingPipeline? _samplingPipeline;
    private WboitPipeline? _transparentPipeline;
    private GizmoPipeline? _gizmoPipeline;
    private bool _enabled = true;

    public void Initiallize()
    {
        _camera.Attach();
        _logger.LogDebug("Creating View Buffers");

        _sceneGBuffer = GBuffer.Create(
            _api.Device,
            // PBR Formats
            baseColorAlphaFormat: TextureFormat.Bgra8Unorm, // Index 0
            worldNormalFormat: TextureFormat.Rgb10A2Unorm, // Index 1
            metallicRoughnessAoFormat: TextureFormat.Rgba8Unorm, // Index 2
            emissiveFormat: TextureFormat.Rgba16float, // Index 3 // R11G11B10Float
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

    [TraceInterception]
    [MemberNotNull(nameof(_compositorPipeline), nameof(_pbrPipeline), nameof(_samplingPipeline),
        nameof(_transparentPipeline), nameof(_gizmoPipeline))]
    private void CreatePipelines()
    {
        var wboitAccum = _shaderManager.GetCurrentShader<IWboitAccumShader>();
        var compositor = _shaderManager.GetCurrentShader<ICompositorShader>();
        var pbrDeferred = _shaderManager.GetCurrentShader<IPbrDeferredLightingShader>();
        var pbrGBuffer = _shaderManager.GetCurrentShader<IPbrGBufferShader>();
        var objectSamplingShader = _shaderManager.GetCurrentShader<IObjectSamplingShader>();
        var gizmoShader = _shaderManager.GetCurrentShader<IGizmoShader>();

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

        if (_gizmoPipeline?.IsUpToDate(gizmoShader) != true)
        {
            _gizmoPipeline?.Dispose();
            _gizmoPipeline = GizmoPipeline.Create(api, _loader, gizmoShader, _sceneGBuffer!);
        }
    }

    public void Update(UpdateInfo info)
    {
        _sceneLights.Animate(info.DeltaTime);
    }

    [TraceInterception]
    public void Render(UpdateInfo info, RenderTarget target, Extent3D sceneViewport)
    {
        UniformUpdate(info, target, sceneViewport);

        _renderBuilder.PrepareDrawEntities(DynamicInstances);

        _renderBuilder.UpdateDrawEntities(info, DynamicInstances);

        UpdateScene(target.QueuePtr, DynamicInstances);

        DeferredMeshDraw(target, sceneViewport);
    }


    [TraceInterception]
    internal void UniformUpdate(UpdateInfo info, RenderTarget target, Extent3D sceneViewport)
    {
        _camera.AspectRatio = sceneViewport.Width / (f32)sceneViewport.Height;
        _pbrPipeline!.WriteGBufferSceneUniforms(target.QueuePtr, new PbrGBufferShader.SceneUniform
        {
            ViewRef = _camera.ViewMatrix,
            ProjectionRef = _camera.ProjectionMatrix,
            CameraPositionRef = _camera.Position,
            Time = info.TotalTime
        });
        _transparentPipeline!.WriteUniformBuffer(target.QueuePtr, new WboitAccumShader.SceneUniform
        {
            ViewProjectionRef = _camera.ViewProjectionMatrix,
            CameraPositionRef = _camera.Position,
            Time = info.TotalTime
        });

        _gizmoPipeline!.WriteSceneUniforms(target.QueuePtr, new GizmoShader.SceneUniform
        {
            ViewRef = _camera.ViewMatrix,
            ProjectionRef = _camera.ProjectionMatrix,
            CameraPositionRef = _camera.Position,
            Time = info.TotalTime,
            ObjectId = u32.MaxValue,
            GizmoPixelSize = 5,
            ViewportSize = new vec2(sceneViewport.Width, sceneViewport.Height)
        });

        if (_sceneRenderCollection != _scene.RenderableInstances || _sceneRenderCollection == null)
        {
            _sceneRenderCollection = _scene.RenderableInstances;
            foreach (var buffer in _scene.RenderableInstances.OpaqueMeshes)
            {
                buffer.CreateAndWriteBuffers(target.QueuePtr, _loader);
                _pbrPipeline.RecreateMaterialBindGroup(buffer);
            }

            foreach (var buffer in _scene.RenderableInstances.TransparentMeshes)
            {
                buffer.CreateAndWriteBuffers(target.QueuePtr, _loader);
                _transparentPipeline.RecreateBindGroup(buffer);
            }

            foreach (var buffer in _scene.RenderableInstances.GizmoMeshes)
            {
                buffer.CreateAndWriteBuffers(target.QueuePtr, _loader);
            }
        }
    }


    [TraceInterception]
    private void UpdateScene(QueuePtr queue, RenderableInstanceCollection dynamicInstances)
    {
        foreach (var buffer in dynamicInstances.OpaqueMeshes)
        {
            buffer.CreateAndWriteBuffers(queue, _loader);
            _pbrPipeline!.RecreateMaterialBindGroup(buffer);
        }

        foreach (var buffer in dynamicInstances.TransparentMeshes)
        {
            buffer.CreateAndWriteBuffers(queue, _loader);
            _transparentPipeline!.RecreateBindGroup(buffer);
        }

        foreach (var buffer in _scene.RenderableInstances.GizmoMeshes)
        {
            buffer.CreateAndWriteBuffers(queue, _loader);
        }

        _lightsBuffer.QueuedWrite(_sceneLights.LightData, queue);
    }

    [TraceInterception]
    internal void DeferredMeshDraw(RenderTarget target, Extent3D sceneViewport)
    {
        if (_pbrPipeline!.BeginGBufferPass() is { } pass)
        {
            foreach (var meshBuffer in _sceneRenderCollection!.OpaqueMeshes)
            {
                _pbrPipeline.DrawMesh(pass, meshBuffer);
            }

            foreach (var meshBuffer1 in DynamicInstances.OpaqueMeshes)
            {
                _pbrPipeline.DrawMesh(pass, meshBuffer1);
            }

            target.QueuePtr.Submit(_pbrPipeline.EndGBufferPass(pass));
        }

        if (_transparentPipeline!.BeginRenderPass(sceneViewport) is { } transparentPass)
        {
            foreach (var instances in _sceneRenderCollection!.TransparentMeshes)
            {
                _transparentPipeline.DrawMesh(transparentPass, instances);
            }

            foreach (var instances in DynamicInstances.TransparentMeshes)
            {
                _transparentPipeline.DrawMesh(transparentPass, instances);
            }

            target.QueuePtr.Submit(_transparentPipeline.EndRenderPass(transparentPass));
        }

        if (_gizmoPipeline!.BeginGizmoPass(sceneViewport) is { } gizmoPass)
        {
            foreach (var instances in _sceneRenderCollection!.GizmoMeshes)
            {
                _gizmoPipeline.DrawGizmos(gizmoPass, instances);
            }

            foreach (var instances in DynamicInstances.GizmoMeshes)
            {
                _gizmoPipeline.DrawGizmos(gizmoPass, instances);
            }

            target.QueuePtr.Submit(_gizmoPipeline.EndGizmoPass(gizmoPass));
        }
    }

    [TraceInterception]
    public VirtualFramebuffer.ColorTargetView? Render(UpdateInfo info, RenderTarget target, u32 width, u32 height,
        bool isSceneViewHovered)
    {
        if (isSceneViewHovered)
        {
            _camera.Update(info);
        }


        if (_sceneGBuffer is null || _finalSceneTexture is null)
        {
            _logger.LogError("Scene Buffers not created");
            return null;
        }


        var prevWidth = _finalSceneTexture.Width;
        var prevHeight = _finalSceneTexture.Height;
        var hasPrev = _finalSceneTexture.TryGetTargets(out var prevView);
        _sceneGBuffer.EnsureSize(width, height);
        _finalSceneTexture.EnsureSize(width, height);

        CreatePipelines();
        var mappingTask = _samplingPipeline.RequestResultBufferMapping();
        _api.Poll();
        PixelReadResult = _samplingPipeline.ReadResult(mappingTask);

        if (!_sceneGBuffer.TryGetCompositorOutputView(out var emissive, out var objectAndInstanceId, out var accum,
                out var reveal, out var overlay, out var depth) ||
            !_finalSceneTexture.TryGetTargets(out var colorOutput))
        {
            _logger.LogError("Failed to get scene buffer views");
            return null;
        }

        var queue = target.QueuePtr;
        Render(info, target, new Extent3D(width, height, 1));

        if (PixelReadRequest is { x: > 0, y: >= 0 } request && request.x < width && request.y < height)
            _samplingPipeline.DispatchPixelRead(queue, request, depth.DepthOnlyView, objectAndInstanceId.View);

        if (!Matrix4x4.Invert(_camera.ProjectionMatrix, out var invViewProjection))
        {
            _logger.LogError("Failed to invert projection matrix");
            return null;
        }

        if (!Matrix4x4.Invert(_camera.ViewMatrix, out var invView))
        {
            _logger.LogError("Failed to invert view matrix");
            return null;
        }

        _pbrPipeline.WriteDeferredSceneUniforms(queue, new PbrDeferredLightingShader.SceneUniforms
        {
            InverseProjectionRef = invViewProjection,
            InverseViewRef = invView,
            CameraPositionWorldRef = _camera.Position,
            LightCount = _sceneLights.LightCount,
            AmbientColorRef = new Vector3(0.1f, 0.1f, 0.1f),
            IblIntensity = 1f,
        });

        _pbrPipeline.ApplyDeferredLighting(queue, _lightsBuffer.Buffer, _sceneLights.LightCount);

        _compositorPipeline.Apply(queue, emissive.View, depth.DepthOnlyView, colorOutput.View, accum.View,
            reveal.View, overlay.View);

        /*
         TODO check if this is auto done
         if (prevWidth != width && prevHeight != height && hasPrev)
            target.ImguiController.RemoveTextureBindGroup(prevView.View);
        */

        return colorOutput;
    }

    public void Teardown()
    {
        _sceneGBuffer?.Release();
        _finalSceneTexture?.Release();

        DynamicInstances.Dispose();

        _compositorPipeline?.Dispose();
        _pbrPipeline?.Dispose();

        _lightsBuffer.Buffer.Destroy();

        // set all null
        _sceneGBuffer = null;
        _finalSceneTexture = null;

        DynamicInstances.Dispose();
        _compositorPipeline = null;
        _pbrPipeline = null;
        _lightsBuffer = null!;
    }
}