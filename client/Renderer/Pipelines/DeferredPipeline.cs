using System.Numerics;
using System.Runtime.InteropServices;
using Client.Wgsl;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using Safe = Silk.NET.WebGPU.Safe;
using static Client.Wgsl.DeferredShader;

namespace Client.Renderer.Pipelines;

public class DeferredPipeline : IDisposable
{
    private readonly RendererApi _api;
    private readonly RenderPipelinePtr _pipeline;
    private readonly BindGroupLayoutPtr _textureBindGroupLayout;
    private readonly BufferPtr _uniformBuffer;
    private readonly IDeferredShader _shader;
    private SamplerPtr _sampler;

    private BindGroupPtr? _textureBindGroup;

    private DeferredPipeline(RendererApi api, RenderPipelinePtr pipeline, BindGroupLayoutPtr textureBindGroupLayout,
        SamplerPtr sampler, BufferPtr uniformBuffer, IDeferredShader shader)
    {
        _api = api;
        _pipeline = pipeline;
        _textureBindGroupLayout = textureBindGroupLayout;
        _sampler = sampler;
        _uniformBuffer = uniformBuffer;
        _shader = shader;
    }

    public static DeferredPipeline Create(RendererApi api, IDeferredShader shader, TextureFormat lightTextureFormat)
    {
        var device = api.Device;
        var uniformBuffer = device.CreateBuffer(BufferUsage.CopyDst | BufferUsage.Uniform,
            (u64)Marshal.SizeOf<Uniforms>());

        var sampler = device.CreateSampler(AddressMode.ClampToEdge, AddressMode.ClampToEdge,
            AddressMode.ClampToEdge,
            magFilter: FilterMode.Nearest, minFilter: FilterMode.Nearest, mipmapFilter: MipmapFilterMode.Nearest,
            lodMinClamp: 0, lodMaxClamp: 1, compare: CompareFunction.Undefined);

        var textureBindGroupLayout = shader.CreateBindGroupLayout0(device, lightsBufferMinSize: 0);

        var pipelineLayout = shader.CreatePipelineLayout(api.Device, textureBindGroupLayout);

        var module = shader.CreateShaderModule(device, pipelineLayout);

        var pipeline = device.CreateRenderPipeline(
            pipelineLayout,
            vertex: new Safe.VertexState
            {
                Buffers = [],
                EntryPoint = shader.VertexEntryPoint,
                Constants = [],
                Module = module
            },
            primitive: new Safe.PrimitiveState
            {
                CullMode = CullMode.Back,
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
            fragment: new Safe.FragmentState
            {
                Constants = [],
                EntryPoint = shader.FragmentEntryPoint,
                Module = module,
                Targets = 
                [
                    new Safe.ColorTargetState(lightTextureFormat,
                        (
                            color: new BlendComponent(BlendOperation.Add, BlendFactor.One, BlendFactor.One),
                            alpha: new BlendComponent(BlendOperation.Add, BlendFactor.One, BlendFactor.Zero)
                        ), ColorWriteMask.All)
                ]
            }
        );

        module.Release();
        pipelineLayout.Release();
        return new DeferredPipeline(api, pipeline, textureBindGroupLayout, sampler, uniformBuffer, shader);
    }

    public void Apply(QueuePtr queue, TextureViewPtr inAlbedo, TextureViewPtr inNormal,
        TextureViewPtr inDepth, TextureViewPtr output,
        Matrix4x4 inverseViewProjection, BufferPtr lightsBuf, u32 lightCount)
    {
        _textureBindGroup?.Release();

        var uniforms = new Uniforms
        {
            InverseViewProjectionRef = inverseViewProjection,
            LightCount = lightCount
        };

        queue.WriteBuffer(_uniformBuffer, 0, new ReadOnlySpan<Uniforms>(ref uniforms));

        _textureBindGroup = _shader.CreateBindGroup0(_api.Device, _textureBindGroupLayout,
            _uniformBuffer, lightsBuf,
            inAlbedo, inNormal, inDepth,
            lightsBufferSize: (u64)Marshal.SizeOf<LightData>() * lightCount);

        var cmd = _api.Device.CreateCommandEncoder();
        var pass = cmd.BeginRenderPass([
            new Safe.RenderPassColorAttachment(view: output, null, loadOp: LoadOp.Load, storeOp: StoreOp.Store,
                clearValue: default)
        ], timestampWrites: null, depthStencilAttachment: null, occlusionQuerySet: null);

        pass.SetPipeline(_pipeline);
        pass.SetBindGroup(0, _textureBindGroup.Value, dynamicOffsets: null);
        pass.Draw(6, 1, 0, 0);
        pass.End();

        queue.Submit(cmd.Finish());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _textureBindGroup?.Release();
        _pipeline.Release();
        _textureBindGroupLayout.Release();
        _uniformBuffer.Destroy();
        _sampler.Release();
    }

    public bool IsUpToDate(IDeferredShader deferred) => _shader == deferred;
}
