using System.Runtime.CompilerServices;
using Client.Renderer.PbrRenderer;
using Client.Renderer.VirtualFramebuffers;
using Client.Wgsl;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using ColorTargetState = Silk.NET.WebGPU.Safe.ColorTargetState;
using FragmentState = Silk.NET.WebGPU.Safe.FragmentState;
using PrimitiveState = Silk.NET.WebGPU.Safe.PrimitiveState;
using VertexState = Silk.NET.WebGPU.Safe.VertexState;
using static Client.Wgsl.TempleShader;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;
using RenderPassDepthStencilAttachment = Silk.NET.WebGPU.Safe.RenderPassDepthStencilAttachment;

namespace Client.Renderer.Pipelines;

public class TemplePipeline : IDisposable
{
    private readonly RendererApi _api;
    private readonly RenderPipelinePtr _modelPipeline;
    private readonly BufferRange _sceneUniformBuffer;
    private readonly BindGroupLayoutPtr _modelBindGroupLayout;
    private readonly SamplerPtr _linearSampler;
    private readonly ITempleShader _shader;
    private readonly GBuffer _gBuffer;
    private BindGroupPtr _sceneBindGroup;

    private TemplePipeline(RendererApi api, RenderPipelinePtr modelPipeline, BindGroupLayoutPtr modelBindGroupLayout,
        SamplerPtr linearSampler, BindGroupPtr sceneBindGroup, BufferRange sceneUniformBuffer, ITempleShader shader,
        GBuffer gBuffer)
    {
        _api = api;
        _modelPipeline = modelPipeline;
        _modelBindGroupLayout = modelBindGroupLayout;
        _linearSampler = linearSampler;
        _sceneBindGroup = sceneBindGroup;
        _sceneUniformBuffer = sceneUniformBuffer;
        _shader = shader;
        _gBuffer = gBuffer;
    }

    public static TemplePipeline Create(RendererApi api, ITempleShader shader, GBuffer gBuffer)
    {
        var device = api.Device;
        var sceneBindGroupLayout = shader.CreateBindGroupLayout0(device);
        var modelBindGroupLayout = shader.CreateBindGroupLayout1(device);
        var pipelineLayout =
            shader.CreatePipelineLayout(device, sceneBindGroupLayout, modelBindGroupLayout);

        var linearSampler = device.CreateSampler(AddressMode.Repeat, AddressMode.Repeat, AddressMode.ClampToEdge,
            minFilter: FilterMode.Linear, magFilter: FilterMode.Linear, mipmapFilter: MipmapFilterMode.Linear,
            lodMinClamp: 0, lodMaxClamp: 1, compare: CompareFunction.Undefined, label: "LinearSampler");

        var sceneUniformBuffer = new BufferRange(
            device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst,
                (u64)Unsafe.SizeOf<SceneUniform>(), label: "SceneUniformBuffer"),
            0, (u64)Unsafe.SizeOf<SceneUniform>());

        var sceneBindGroup = shader.CreateBindGroup0(device, sceneBindGroupLayout, sceneUniformBuffer.Buffer,
            sceneUniformBuffer.Offset, sceneUniformBuffer.Size);
        sceneBindGroupLayout.Release();
        var module = shader.CreateShaderModule(device, pipelineLayout);

        // ReSharper disable once MethodHasAsyncOverload
        var modelPipeline = device.CreateRenderPipeline(
            label: "DefaultPipeline",
            layout: pipelineLayout,
            vertex: new VertexState
            {
                Module = module,
                EntryPoint = shader.VertexEntryPoint,
                Constants = [],
                Buffers = shader.GetVertexBufferLayout(VertexStepMode.Vertex, VertexStepMode.Instance),
            },
            primitive: new PrimitiveState
            {
                Topology = PrimitiveTopology.TriangleList,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.Back
            },
            depthStencil: new DepthStencilState
            {
                Format = gBuffer.DepthStencilFormat,
                DepthCompare = CompareFunction.LessEqual,
                DepthWriteEnabled = true,
                StencilFront = new StencilFaceState { Compare = CompareFunction.Always },
                StencilBack = new StencilFaceState { Compare = CompareFunction.Always },
            },
            multisample: new MultisampleState
            {
                Count = 1,
                AlphaToCoverageEnabled = false,
                Mask = u32.MaxValue
            },
            fragment: new FragmentState
            {
                Module = module,
                EntryPoint = shader.FragmentEntryPoint,
                Constants = [],
                Targets = shader.ColorTarget(
                    albedo: new ColorTargetState(
                        gBuffer.AlbedoChannelFormat,
                        blendState: null,
                        ColorWriteMask.All
                    ),
                    normal: new ColorTargetState(
                        gBuffer.NormalChannelFormat,
                        blendState: null,
                        ColorWriteMask.All
                    ),
                    emission: new ColorTargetState(
                        gBuffer.LightChannelFormat,
                        blendState: null,
                        ColorWriteMask.All
                    ),
                    objectAndInstanceId: new ColorTargetState(
                        gBuffer.ObjectAndInstanceIdChannelFormat,
                        blendState: null,
                        ColorWriteMask.All
                    )
                )
            }
        );


        module.Release();
        pipelineLayout.Release();

        return new TemplePipeline(api, modelPipeline, modelBindGroupLayout, linearSampler,
            sceneBindGroup, sceneUniformBuffer, shader, gBuffer);
    }

    public void RecreateBindGroup(PBRInstancedMeshBuffer pbrInstancedMeshBuffer)
    {
        var targetHash = new HashCode();
        targetHash.Add(_modelBindGroupLayout);
        targetHash.Add(pbrInstancedMeshBuffer.ModelUniformBuffer.Version);
        targetHash.Add(pbrInstancedMeshBuffer.AlbedoTexture.Version);
        targetHash.Add(pbrInstancedMeshBuffer.NormalTexture.Version);
        targetHash.Add(pbrInstancedMeshBuffer.EmissionTexture.Version);

        if (pbrInstancedMeshBuffer.ModelBindGroupHash == targetHash.ToHashCode())
            return;

        pbrInstancedMeshBuffer.ModelBindGroupHash = targetHash.ToHashCode();

        pbrInstancedMeshBuffer.ModelBindGroup?.Release();
        pbrInstancedMeshBuffer.ModelBindGroup = _shader.CreateBindGroup1(_api.Device,
            _modelBindGroupLayout,
            pbrInstancedMeshBuffer.ModelUniformBuffer.Buffer,
            _linearSampler,
            pbrInstancedMeshBuffer.AlbedoTexture.View,
            pbrInstancedMeshBuffer.NormalTexture.View,
            pbrInstancedMeshBuffer.EmissionTexture.View,
            ubModelOffset: pbrInstancedMeshBuffer.ModelUniformBuffer.Offset,
            ubModelSize: pbrInstancedMeshBuffer.ModelUniformBuffer.Size
        );
    }

    public void WriteUniformBuffer(QueuePtr queue, ref SceneUniform sceneUniform)
    {
        queue.WriteBuffer(_sceneUniformBuffer.Buffer, _sceneUniformBuffer.Offset,
            new ReadOnlySpan<SceneUniform>(ref sceneUniform));
    }

    private CommandEncoderPtr _cmd;

    public RenderPassEncoderPtr? BeginRenderPass()
    {
        if (!_gBuffer.TryGetTargets(out var albedo, out var normal, out var emission, out var objectAndInstanceId,
                out _, out _, out var depthStencil))
            return null;

        _cmd = _api.Device.CreateCommandEncoder();

        var pass = _cmd.BeginRenderPass([
                new RenderPassColorAttachment(albedo.View, resolveTarget: null,
                    LoadOp.Clear, StoreOp.Store, new Color(.06, .1, .1, 1)),
                new RenderPassColorAttachment(normal.View, resolveTarget: null,
                    LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0, 1)),
                new RenderPassColorAttachment(emission.View, resolveTarget: null,
                    LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0.1, 1)),
                new RenderPassColorAttachment(objectAndInstanceId.View, resolveTarget: null,
                    LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0, 0))
            ],
            null,
            new RenderPassDepthStencilAttachment(depthStencil.View, LoadOp.Clear, StoreOp.Store, 1f, false,
                /*stencil*/ LoadOp.Clear, StoreOp.Discard, 0, true), null);

        pass.SetPipeline(_modelPipeline);
        pass.SetBindGroup(0, _sceneBindGroup, ReadOnlySpan<u32>.Empty);

        return pass;
    }

    public CommandBufferPtr EndRenderPass(RenderPassEncoderPtr pass)
    {
        pass.End();
        return _cmd.Finish("TemplePipeline");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _linearSampler.Release();
        _sceneBindGroup.Release();
        _modelBindGroupLayout.Release();
        _modelPipeline.Release();
        _sceneUniformBuffer.Buffer.Destroy();
    }
}
