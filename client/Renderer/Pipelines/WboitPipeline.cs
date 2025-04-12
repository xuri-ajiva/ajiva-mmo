using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Renderer.VirtualFramebuffers;
using Client.Wgsl;
using Silk.NET.Core;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using ColorTargetState = Silk.NET.WebGPU.Safe.ColorTargetState;
using FragmentState = Silk.NET.WebGPU.Safe.FragmentState;
using PrimitiveState = Silk.NET.WebGPU.Safe.PrimitiveState;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;
using RenderPassDepthStencilAttachment = Silk.NET.WebGPU.Safe.RenderPassDepthStencilAttachment;
using VertexState = Silk.NET.WebGPU.Safe.VertexState;

namespace Client.Renderer.Pipelines;

using static WboitAccumShader;

public class WboitPipeline(
    RendererApi api,
    ResourceLoader loader,
    RenderPipelinePtr pipeline,
    SamplerPtr defaultSampler,
    AjivaBuffer<SceneUniform> sceneUniformBuffer,
    BindGroupLayoutPtr modelBindGroupLayout,
    BindGroupPtr sceneBindGroup,
    IWboitAccumShader shader,
    GBuffer gbuffer) : IDisposable
{
    private readonly RendererApi _api = api;
    private readonly ResourceLoader _loader = loader;


    public static WboitPipeline Create(RendererApi api, ResourceLoader loader, IWboitAccumShader shader,
        GBuffer gbuffer)
    {
        var device = api.Device;
        var accumScene = shader.CreateBindGroupLayout0(device);
        var accumModel = shader.CreateBindGroupLayout1(device);
        var pipelineLayout = shader.CreatePipelineLayout(device, accumScene, accumModel);

        var defaultSampler = device.CreateSampler(AddressMode.ClampToEdge, AddressMode.ClampToEdge,
            AddressMode.ClampToEdge, FilterMode.Linear, FilterMode.Linear, MipmapFilterMode.Nearest,
            0, 1, CompareFunction.Undefined, label: "WBOIT Default Sampler");

        var sceneUniformBuffer =
            loader.CreateBuffer<SceneUniform>(BufferUsage.Uniform | BufferUsage.CopyDst, 1, "WBOIT SceneUniformBuffer");

        var sceneBindGroup = shader.CreateBindGroup0(device, accumScene, sceneUniformBuffer.Buffer);
        accumScene.Release();
        var module = shader.CreateShaderModule(device, pipelineLayout);

        var pipeline = device.CreateRenderPipeline(
            label: "WBOIT_AccumulationPipeline",
            layout: pipelineLayout,
            vertex: new VertexState
            {
                Module = module,
                EntryPoint = shader.VertexEntryPoint,
                Constants = [],
                Buffers = shader.GetVertexBufferLayout(VertexStepMode.Vertex, VertexStepMode.Instance)
            },
            primitive: new PrimitiveState
            {
                Topology = PrimitiveTopology.TriangleList,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.None,
            },
            depthStencil: new DepthStencilState
            {
                Format = gbuffer.DepthStencilFormat,
                DepthCompare = CompareFunction.LessEqual,
                DepthWriteEnabled = false,
                StencilFront = new StencilFaceState { Compare = CompareFunction.Always },
                StencilBack = new StencilFaceState { Compare = CompareFunction.Always },
                DepthBias = 0,
                DepthBiasClamp = 0,
                DepthBiasSlopeScale = 0,
            },
            multisample: new MultisampleState { Count = 1, Mask = u32.MaxValue, AlphaToCoverageEnabled = false },
            fragment: new FragmentState
            {
                Module = module,
                EntryPoint = shader.FragmentEntryPoint, // "fs_main"
                Constants = [],
                Targets =
                    shader.ColorTarget(
                        // Target 0: Accumulation Buffer (Additive Blending)
                        new ColorTargetState(
                            gbuffer.AccumChannelFormat,
                            blendState: (
                                color: new BlendComponent
                                {
                                    SrcFactor = BlendFactor.One, DstFactor = BlendFactor.One,
                                    Operation = BlendOperation.Add
                                },
                                alpha: new BlendComponent
                                {
                                    SrcFactor = BlendFactor.One, DstFactor = BlendFactor.One,
                                    Operation = BlendOperation.Add
                                }
                            ),
                            writeMask: ColorWriteMask.All
                        ),
                        // Target 1: Revealage Buffer (Multiplicative Blending Dst = Dst * (1 - Src))
                        new ColorTargetState(
                            gbuffer.RevealChannelFormat,
                            blendState: (
                                color: new BlendComponent
                                {
                                    SrcFactor = BlendFactor.Zero, DstFactor = BlendFactor.OneMinusSrc,
                                    Operation = BlendOperation.Add
                                },
                                alpha: new BlendComponent
                                {
                                    SrcFactor = BlendFactor.Zero, DstFactor = BlendFactor.One,
                                    Operation = BlendOperation.Add
                                }
                            ),
                            writeMask: ColorWriteMask.Red // Only need to write to Red channel (as per shader)
                            //writeMask: ColorWriteMask.All // Or All if format is single channel like R8Unorm
                        ),
                        // Target 2: Object and Instance ID Buffer (read only)
                        new ColorTargetState(
                            gbuffer.ObjectAndInstanceIdChannelFormat,
                            blendState: null,
                            writeMask: ColorWriteMask.None
                        )
                    )
            }
        );
        module.Release();
        pipelineLayout.Release();


        return new WboitPipeline(api, loader, pipeline, defaultSampler, sceneUniformBuffer, accumModel, sceneBindGroup,
            shader, gbuffer);
    }

    public void RecreateBindGroup(OITInstancedMeshBuffer buffer)
    {
        var targetHash = new HashCode();
        targetHash.Add(modelBindGroupLayout);
        targetHash.Add(buffer.ModelUniformBuffer.Version);
        targetHash.Add(buffer.AlbedoTexture.Version);
        targetHash.Add(buffer.ModelUniformBuffer.Version);

        if (buffer.ModelBindGroupHash == targetHash.ToHashCode())
            return;
        buffer.ModelBindGroupHash = targetHash.ToHashCode();

        buffer.ModelBindGroup?.Release();
        buffer.ModelBindGroup = shader.CreateBindGroup1(_api.Device, modelBindGroupLayout,
            buffer.ModelUniformBuffer.Buffer, defaultSampler, buffer.AlbedoTexture.View,
            buffer.ModelUniformBuffer.Offset, buffer.ModelUniformBuffer.Size);
    }

    public void WriteUniformBuffer(QueuePtr queue, SceneUniform sceneUniform)
    {
        queue.WriteBuffer(sceneUniformBuffer.Buffer, sceneUniformBuffer.Offset,
            new ReadOnlySpan<SceneUniform>(ref sceneUniform));
    }

    private CommandEncoderPtr _cmd;

    public RenderPassEncoderPtr? BeginRenderPass(Extent3D sceneViewport)
    {
        if (!gbuffer.TryGetAlphaBlend(out var objectAndInstanceId, out var accum, out var reveal, out var depth))
            return null;

        _cmd = _api.Device.CreateCommandEncoder();

        var pass = _cmd.BeginRenderPass([
                new RenderPassColorAttachment(accum.View, null, LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0, 0)),
                new RenderPassColorAttachment(reveal.View, null, LoadOp.Clear, StoreOp.Store, new Color(1, 0, 0, 0)),
                new RenderPassColorAttachment(objectAndInstanceId.View, null, LoadOp.Load, StoreOp.Store, default),
            ],
            null,
            new RenderPassDepthStencilAttachment(depth.View, LoadOp.Load, StoreOp.Store, 1, true,
                LoadOp.Undefined, StoreOp.Discard, 0, true), null, label: "WBOIT Accum Pass");

        pass.SetViewport(0, 0, sceneViewport.Width, sceneViewport.Height, 0, 1);

        pass.SetPipeline(pipeline);
        pass.SetBindGroup(0, sceneBindGroup, ReadOnlySpan<u32>.Empty);

        return pass;
    }

    public CommandBufferPtr EndRenderPass(RenderPassEncoderPtr pass)
    {
        pass.End();
        return _cmd.Finish("TemplePipeline");
    }

    public void Dispose()
    {
        sceneUniformBuffer.Dispose();
        defaultSampler.Release();
        sceneBindGroup.Release();
        modelBindGroupLayout.Release();
        pipeline.Release();
    }

    public bool IsUpToDate(IWboitAccumShader other)
    {
        return shader == other;
    }
}
