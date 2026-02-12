using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Client.Renderer.Resources;
using Client.Renderer.VirtualFramebuffers;
using Client.Wgsl;
using GlmSharp;
using Microsoft.Extensions.Logging;
using RmlUiNet;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using PrimitiveState = Silk.NET.WebGPU.Safe.PrimitiveState;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;
using VertexState = Silk.NET.WebGPU.Safe.VertexState;

namespace Client.UI.Desktop.wgpu;

public unsafe class RmlWebGpuRenderer(
    ILoggerFactory loggerFactory,
    ResourceManager loader,
    ShaderReloadManager shaderReloadManager,
    u32 width = 1920,
    u32 height = 1080)
    : RenderInterface
{
    private class TextureAndBindGroup(AjivaTexture texture, BindGroupPtr? bindGroup)
    {
        public AjivaTexture Texture { get; set; } = texture;
        public BindGroupPtr? BindGroup { get; set; } = bindGroup;

        public void Deconstruct(out AjivaTexture texture, out BindGroupPtr? bindGroup)
        {
            texture = Texture;
            bindGroup = BindGroup;
        }
    }

    private readonly GeometryMemoryPool _memoryPool = new(loader, loggerFactory.CreateLogger<GeometryMemoryPool>());
    private readonly SimpleObjectToHandle<GeometryRent> _geometryBuffers = new();
    private readonly SimpleObjectToHandle<TextureAndBindGroup> _textureBindings = new();
    private readonly ILogger<RmlWebGpuRenderer> _logger = loggerFactory.CreateLogger<RmlWebGpuRenderer>();

    private IRmlShader? _currentShader;
    private BindGroupLayoutPtr _sceneBindGroupLayoutPtr, _textureBindGroupLayout;
    private RenderPipelinePtr _pipeline;
    private SamplerPtr _sampler;
    private AjivaBuffer<RmlShader.SceneUniform>? _uniformBuffer;
    private AjivaBuffer<RmlShader.TranslationUniform>? _translationBuffer;
    private BindGroupPtr _backupTextBind;
    public RenderTexture? OutputFramebuffer;

    private QueuePtr _queue;
    private BindGroupPtr? _sceneBind;
    private CommandEncoderPtr _cmd;
    private RenderPassEncoderPtr _pass;

    public void Initialize()
    {
        _memoryPool.Initialize();
    }

    [MemberNotNull(nameof(_pipeline), nameof(_currentShader), nameof(_uniformBuffer), nameof(_translationBuffer),
        nameof(OutputFramebuffer))]
    private void RecreatePipelineIfNeeded(IRmlShader shader)
    {
        if (_currentShader == shader) return;
        _currentShader = shader;
        var device = loader.Api.Device;
        _uniformBuffer ??= loader.CreateBuffer<RmlShader.SceneUniform>(BufferUsage.CopyDst | BufferUsage.Uniform, 1,
            "Rml_Scene_Uniform_Buffer");
        _translationBuffer ??= loader.CreateBuffer<RmlShader.TranslationUniform>(
            BufferUsage.CopyDst | BufferUsage.Uniform, 1,
            "Rml_Translation_Uniform_Buffer");
        OutputFramebuffer ??=
            RenderTexture.Create(device, TextureFormat.Rgba8Unorm, width, height, "Rml_Output_Framebuffer");

        _sceneBindGroupLayoutPtr = shader.CreateBindGroupLayout0(device, ubTranslationHasDynamicOffset: true);
        _textureBindGroupLayout = shader.CreateBindGroupLayout1(device);

        _backupTextBind =
            _currentShader.CreateBindGroup1(device, _textureBindGroupLayout, loader.DefaultTexture.View);

        var pipelineLayout = shader.CreatePipelineLayout(device, _sceneBindGroupLayoutPtr, _textureBindGroupLayout);

        var module = shader.CreateShaderModule(device, pipelineLayout);

        if (_pipeline != default)
            _pipeline.Release();

        _pipeline = device.CreateRenderPipeline(
            pipelineLayout,
            vertex: new VertexState
            {
                Buffers = //_currentShader.GetVertexBufferLayout(VertexStepMode.Vertex)
                [
                    new global::Silk.NET.WebGPU.Safe.VertexBufferLayout(20,
                        VertexStepMode.Vertex,
                        [
                            new VertexAttribute(VertexFormat.Float32x2,
                                offset: 0,
                                shaderLocation: 0),
                            new VertexAttribute(VertexFormat.Unorm8x4,
                                offset: 8,
                                shaderLocation: 1),
                            new VertexAttribute(VertexFormat.Float32x2,
                                offset: 12,
                                shaderLocation: 2)
                        ])
                ],
                EntryPoint = shader.VertexEntryPoint,
                Constants = [],
                Module = module,
            },
            primitive: new PrimitiveState
            {
                CullMode = CullMode.None,
                FrontFace = FrontFace.Ccw,
                StripIndexFormat = IndexFormat.Undefined,
                Topology = PrimitiveTopology.TriangleList
            },
            depthStencil: null,
            multisample: new MultisampleState
            {
                Count = 1,
                Mask = u32.MaxValue
            },
            fragment: new Silk.NET.WebGPU.Safe.FragmentState
            {
                Constants = [],
                EntryPoint = shader.FragmentEntryPoint,
                Module = module,
                Targets =
                [
                    new Silk.NET.WebGPU.Safe.ColorTargetState(OutputFramebuffer.ColorFormat,
                        // 0 True One 1 - Src Alpha Add One 1 - Src Alpha Subtract RGBA
                        blendState: (color: new BlendComponent(BlendOperation.Add, BlendFactor.Src, BlendFactor.OneMinusSrcAlpha),
                                     alpha: new BlendComponent(BlendOperation.Subtract, BlendFactor.One,
                                BlendFactor.OneMinusSrcAlpha)
                        ), ColorWriteMask.All)
                ]
            },
            label: "Rml_Render_Pipeline"
        );
        module.Release();
        pipelineLayout.Release();
        _sampler = device.CreateSampler(AddressMode.Repeat, AddressMode.Repeat,
            AddressMode.Repeat,
            magFilter: FilterMode.Linear, minFilter: FilterMode.Linear, mipmapFilter: MipmapFilterMode.Nearest,
            lodMinClamp: 0, lodMaxClamp: 0, compare: CompareFunction.Undefined);
    }


    public void StartCapture(QueuePtr targetQueuePtr)
    {
        _queue = targetQueuePtr;
        _pendingRenders.Clear();
        _translationUniforms.Clear();
    }

    private record struct GeometryRender(IntPtr Geometry, IntPtr Texture, i32 Translation);

    private readonly List<GeometryRender> _pendingRenders = [];
    private readonly List<RmlShader.TranslationUniform> _translationUniforms = [];

    public void ExecuteRender(QueuePtr queue)
    {
        var shader = shaderReloadManager.GetCurrentShader<IRmlShader>();
        RecreatePipelineIfNeeded(shader);

        _sceneBind?.Release();

        OutputFramebuffer.TryGetTargets(out var outputView);

        // write scene uniform
        var uniforms = new RmlShader.SceneUniform
        {
            //Transform = mat4.Identity,
            Size = new vec2(width, height)
        };
        _uniformBuffer!.QueuedWrite(new ReadOnlySpan<RmlShader.SceneUniform>(ref uniforms), queue);

        // write translation uniforms
        if (_translationBuffer.Count < (u64)_translationUniforms.Count)
            _translationBuffer.ResizeWithoutDataPreserve((u64)_translationUniforms.Count);
        _translationBuffer.QueuedWrite(CollectionsMarshal.AsSpan(_translationUniforms), queue);

        _sceneBind = _currentShader!.CreateBindGroup0(loader.Api.Device, _sceneBindGroupLayoutPtr,
            _uniformBuffer.Buffer, _sampler, _translationBuffer.Buffer);

        _cmd = loader.Api.Device.CreateCommandEncoder("Rml_UI_Command_Encoder");
        _pass = _cmd.BeginRenderPass([
            new RenderPassColorAttachment(view: outputView.View, null, loadOp: LoadOp.Clear, storeOp: StoreOp.Store,
                clearValue: default)
        ], timestampWrites: null, depthStencilAttachment: null, occlusionQuerySet: null, label: "Rml_UI_Pass");

        _pass.SetPipeline(_pipeline);
        _pass.SetViewport(0, 0, width, height, 0, 1);

        foreach (var pendingRender in _pendingRenders)
        {
            ResolveGeometryRender(pendingRender);
        }

        _pass.End();

        queue.Submit(_cmd.Finish());
    }

    private void ResolveGeometryRender(GeometryRender request)
    {
        var (geometry, texture, translation) = request;
        var handle = geometry.ToInt32();
        if (!_geometryBuffers.TryGet(handle, out var geomBuffers))
        {
            _logger.LogWarning("Invalid geometry handle {Handle}", handle);
            return;
        }

        BindGroupPtr? textureBindGroup = null;
        if (_textureBindings.TryGet(texture.ToInt32(), out var texHandle))
        {
            textureBindGroup = texHandle.BindGroup ??=
                _currentShader?.CreateBindGroup1(loader.Api.Device, _textureBindGroupLayout, texHandle.Texture.View);
        }

        _pass.SetBindGroup(0, _sceneBind!.Value, dynamicOffsets:
        [
            (u32)(translation * Unsafe.SizeOf<RmlShader.TranslationUniform>())
        ]);

        _pass.SetBindGroup(1, textureBindGroup ?? _backupTextBind, dynamicOffsets: null);

        _pass.SetVertexBuffer(0, geomBuffers.VertexBuffer.Buffer, geomBuffers.VertexBuffer.OffsetBytes,
            geomBuffers.VertexBuffer.Size);
        _pass.SetIndexBuffer(geomBuffers.IndexBuffer.Buffer, IndexFormat.Uint32, geomBuffers.IndexBuffer.OffsetBytes,
            geomBuffers.IndexBuffer.Size);
        _pass.DrawIndexed(geomBuffers.IndexCount, 1, geomBuffers.FirstIndex,
            (i32)geomBuffers.BaseVertex, 0);

        _logger.LogTrace("Rendered geometry with handle {Handle} using texture {TextureHandle}", handle, texHandle);
    }


    public override IntPtr CompileGeometry(Vertex* vertices, i32 vertexCount, i32* indices, i32 indexCount)
    {
        var geometryRent = _memoryPool.Rent(new ReadOnlySpan<Vertex>(vertices, vertexCount),
            new ReadOnlySpan<u32>(indices, indexCount), _queue);
        var handle = _geometryBuffers.Register(geometryRent);
        _logger.LogTrace("Compiled geometry with handle {Handle}: {VertexCount} vertices, {IndexCount} indices", handle,
            vertexCount, indexCount);
        return handle;
    }

    public override void RenderGeometry(IntPtr geometry, Vector2f translation, IntPtr texture)
    {
        _translationUniforms.Add(new RmlShader.TranslationUniform
        {
            Translation = new vec2(translation.X, translation.Y),
            HasTexture = texture.ToInt32()
        });
        _pendingRenders.Add(new GeometryRender(geometry, texture, _translationUniforms.Count - 1));
    }

    public override void ReleaseGeometry(IntPtr geometry)
    {
        var handle = geometry.ToInt32();
        if (_geometryBuffers.TryGet(handle, out var geomBuffers))
        {
            _memoryPool.Return(geomBuffers);
            _geometryBuffers.Unregister(handle);
            _logger.LogTrace("Released geometry with handle {Handle}", handle);
        }
        else
        {
            _logger.LogWarning("Attempted to release invalid geometry handle {Handle}", handle);
        }
    }

    public override IntPtr LoadTexture(ref Vector2i textureDimensions, string source)
    {
        var texture = loader.SimpleSyncTextureLoad(source, out var img)
            ? AjivaTexture.FromData(img)
            : loader.DefaultTexture;

        textureDimensions = new Vector2i((i32)texture.Size.Width, (i32)texture.Size.Height);
        return _textureBindings.Register(new TextureAndBindGroup(texture, null));
    }

    public override IntPtr GenerateTexture(u8* source, i32 numBytes, Vector2i dimensions)
    {
        var pixelData = new ReadOnlySpan<u8>(source, numBytes);
        var img = AjivaTexture.FromData(loader.CreateTexture(TextureUsage.CopyDst | TextureUsage.TextureBinding,
            "UI_Generated_Texture", 1, new Extent3D((u32)dimensions.X, (u32)dimensions.Y, 1),
            TextureFormat.Rgba8Unorm, TextureAspect.All));
        img.WriteTexture(MemoryMarshal.Cast<u8, u32>(pixelData), new Extent3D((u32)dimensions.X, (u32)dimensions.Y, 1));

        return _textureBindings.Register(new TextureAndBindGroup(img, null));
    }

    public override void ReleaseTexture(IntPtr textureHandle)
    {
        var handle = textureHandle.ToInt32();
        if (_textureBindings.TryGet(handle, out var texBinding))
        {
            texBinding.Texture.Dispose();
            _textureBindings.Unregister(handle);
            _logger.LogTrace("Released texture with handle {Handle}", handle);
        }
        else
        {
            _logger.LogWarning("Attempted to release invalid texture handle {Handle}", handle);
        }
    }

    public override void EnableScissorRegion(bool enable)
    {
        base.EnableScissorRegion(enable);
    }

    public override void SetScissorRegion(i32 x, i32 y, i32 width, i32 height)
    {
        base.SetScissorRegion(x, y, width, height);
    }

    public override void SetTransform(IntPtr transform)
    {
        base.SetTransform(transform);
    }

    public override void Test()
    {
        base.Test();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _memoryPool.Dispose();
    }

    public string DebugInfo()
    {
        var (vertexBufferCount, indexBufferCount, avgVertexFragmentation, avgIndexFragmentation, allocatedBytes,
            totalUsedBytes) = _memoryPool.GetStats();
        return $"""
                Current Shader: {_currentShader?.GetType().Name ?? "None"}
                Drawn Geometries: {_pendingRenders.Count}
                Geometry Buffers: {_geometryBuffers.Count}
                Texture Bindings: {_textureBindings.Count}
                Output Framebuffer: {(OutputFramebuffer != null ? $"{OutputFramebuffer.Width}x{OutputFramebuffer.Height}" : "None")}
                Pool Allocated: {allocatedBytes / 1024} KB
                Pool Used: {totalUsedBytes / 1024} KB
                Vertex Buffers: {vertexBufferCount}, Avg Fragmentation: {avgVertexFragmentation:P2}
                Index Buffers: {indexBufferCount}, Avg Fragmentation: {avgIndexFragmentation:P2}
                """;
    }
}