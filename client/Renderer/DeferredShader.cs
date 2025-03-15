using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using Safe = Silk.NET.WebGPU.Safe;
using static Silk.NET.WebGPU.Safe.BindGroupLayoutEntries;
using static Silk.NET.WebGPU.Safe.BindGroupEntries;

namespace Client.Renderer
{
    internal class DeferredShader
    {
        public struct LightData
        {
            public Vector4 Position;
            public Vector3 Color;
            public f32 Radius;
        }

        private struct Uniforms
        {
            public Matrix4x4 InverseViewProjection;
            public u32 LightCount;
            public u32 _padding0;
            public u32 _padding1;
            public u32 _padding2;
        }

        private readonly RendererApi _api;
        private RenderPipelinePtr _pipeline;
        private BindGroupLayoutPtr _textureBindGroupLayout;
        private SamplerPtr _sampler;
        private BufferPtr _uniformBuffer;

        private BindGroupPtr? textureBindGroup;

        private DeferredShader(RendererApi api, RenderPipelinePtr pipeline, BindGroupLayoutPtr textureBindGroupLayout,
            SamplerPtr sampler, BufferPtr uniformBuffer)
        {
            _api = api;
            _pipeline = pipeline;
            _textureBindGroupLayout = textureBindGroupLayout;
            _sampler = sampler;
            _uniformBuffer = uniformBuffer;
        }

        public static DeferredShader Create(RendererApi api, TextureFormat lightTextureFormat)
        {
            var device = api.Device;
            var uniformBuffer = device.CreateBuffer(BufferUsage.CopyDst | BufferUsage.Uniform,
                (u64)Marshal.SizeOf<Uniforms>());

            var sampler = device.CreateSampler(AddressMode.ClampToEdge, AddressMode.ClampToEdge,
                AddressMode.ClampToEdge,
                magFilter: FilterMode.Nearest, minFilter: FilterMode.Nearest, mipmapFilter: MipmapFilterMode.Nearest,
                lodMinClamp: 0, lodMaxClamp: 1, compare: CompareFunction.Undefined);

            var textureBindGroupLayout = device.CreateBindGroupLayout([
                    Buffer(0, ShaderStage.Fragment, BufferBindingType.Uniform, (u64)Marshal.SizeOf<Uniforms>()),
                    Buffer(1, ShaderStage.Fragment, BufferBindingType.ReadOnlyStorage, 0),
                    Texture(2, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D, false),
                    Texture(3, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D, false),
                    Texture(4, ShaderStage.Fragment, TextureSampleType.Depth, TextureViewDimension.Dimension2D, false)
                ]
            );

            var module = api.CreateShaderModuleFromFile("resources/builtin/deferred.wgsl", "deferred");

            var pipelineLayout = device.CreatePipelineLayout(
                new ReadOnlySpan<BindGroupLayoutPtr>(ref textureBindGroupLayout)
            );

            var pipeline = device.CreateRenderPipeline(
                pipelineLayout,
                vertex: new Safe.VertexState
                {
                    Buffers = [],
                    EntryPoint = "vs_main",
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
                    EntryPoint = "fs_main",
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

            return new DeferredShader(api, pipeline, textureBindGroupLayout, sampler, uniformBuffer);
        }

        public void Apply(QueuePtr queue, TextureViewPtr inAlbedo, TextureViewPtr inNormal,
            TextureViewPtr inDepth, TextureViewPtr output,
            Matrix4x4 inverseViewProjection, BufferPtr lightsBuffer, u32 lightCount)
        {
            textureBindGroup?.Release();

            var uniforms = new Uniforms
            {
                InverseViewProjection = inverseViewProjection,
                LightCount = lightCount
            };

            queue.WriteBuffer(_uniformBuffer, 0, new ReadOnlySpan<Uniforms>(ref uniforms));

            textureBindGroup = _api.Device.CreateBindGroup(_textureBindGroupLayout, [
                Buffer(0, _uniformBuffer, 0, (u64)Marshal.SizeOf<Uniforms>()),
                Buffer(1, lightsBuffer, 0, (u64)Marshal.SizeOf<LightData>() * lightCount),
                Texture(2, inAlbedo),
                Texture(3, inNormal),
                Texture(4, inDepth),
            ]);

            var cmd = _api.Device.CreateCommandEncoder();
            var pass = cmd.BeginRenderPass([
                new Safe.RenderPassColorAttachment(view: output, null, loadOp: LoadOp.Load, storeOp: StoreOp.Store,
                    clearValue: default)
            ], timestampWrites: null, depthStencilAttachment: null, occlusionQuerySet: null);

            pass.SetPipeline(_pipeline);
            pass.SetBindGroup(0, textureBindGroup.Value, dynamicOffsets: null);
            pass.Draw(6, 1, 0, 0);
            pass.End();

            queue.Submit(new ReadOnlySpan<CommandBufferPtr>(cmd.Finish()));
        }
    }
}
