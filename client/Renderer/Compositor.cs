using System.Runtime.InteropServices;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using Safe = Silk.NET.WebGPU.Safe;
using static Silk.NET.WebGPU.Safe.BindGroupLayoutEntries;
using static Silk.NET.WebGPU.Safe.BindGroupEntries;

namespace client.Renderer
{
    internal class Compositor
    {
        private struct Uniforms
        {
            public f32 Gamma;
            private u32 _padding0;
            private u32 _padding1;
            private u32 _padding2;
        }

        private static readonly BindGroupLayoutEntry[] _bindGroupLayoutEntries =
        [
            Buffer(0, ShaderStage.Fragment, BufferBindingType.Uniform, (u64)Marshal.SizeOf<Uniforms>()),
            Texture(1, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D, false),
            Texture(4, ShaderStage.Fragment, TextureSampleType.Depth, TextureViewDimension.Dimension2D, false)
        ];

        private BindGroupPtr CreateBindGroup(TextureViewPtr inColor, TextureViewPtr inDepth)
        {
            return _api.Device.CreateBindGroup(_textureBindGroupLayout, [
                Buffer(0, _uniformBuffer, 0, (u64)Marshal.SizeOf<Uniforms>()),
                Texture(1, inColor),
                Texture(4, inDepth),
            ]);
        }

        private readonly RendererApi _api;
        private readonly RenderPipelinePtr _pipeline;
        private readonly BindGroupLayoutPtr _textureBindGroupLayout;
        private readonly BufferPtr _uniformBuffer;

        private BindGroupPtr? _textureBindGroup;

        private Compositor(RendererApi api, RenderPipelinePtr pipeline, BindGroupLayoutPtr textureBindGroupLayout,
            BufferPtr uniformBuffer)
        {
            _api = api;
            _pipeline = pipeline;
            _textureBindGroupLayout = textureBindGroupLayout;
            _uniformBuffer = uniformBuffer;
        }

        public static Compositor Create(RendererApi api, TextureFormat outputTextureFormat)
        {
            var device = api.Device;
            var uniformBuffer = device.CreateBuffer(BufferUsage.CopyDst | BufferUsage.Uniform,
                (u64)Marshal.SizeOf<Uniforms>());

            var textureBindGroupLayout = device.CreateBindGroupLayout(_bindGroupLayoutEntries);

            var module = api.CreateShaderModuleFromFile("resources/builtin/compositor.wgsl", "compositor");

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
                        new Safe.ColorTargetState(outputTextureFormat,
                            blendState: null, ColorWriteMask.All)
                    ]
                }
            );

            return new Compositor(api, pipeline, textureBindGroupLayout, uniformBuffer);
        }

        public void Apply(QueuePtr queue, TextureViewPtr inColor, TextureViewPtr inDepth, TextureViewPtr output)
        {
            _textureBindGroup?.Release();

            var uniforms = new Uniforms
            {
                Gamma = 1
            };

            queue.WriteBuffer(_uniformBuffer, 0, new ReadOnlySpan<Uniforms>(ref uniforms));

            _textureBindGroup = CreateBindGroup(inColor, inDepth);

            var cmd = _api.Device.CreateCommandEncoder();
            var pass = cmd.BeginRenderPass([
                new Safe.RenderPassColorAttachment(view: output, null, loadOp: LoadOp.Load, storeOp: StoreOp.Store,
                    clearValue: default)
            ], timestampWrites: null, depthStencilAttachment: null, occlusionQuerySet: null);

            pass.SetPipeline(_pipeline);
            pass.SetBindGroup(0, _textureBindGroup.Value, dynamicOffsets: null);
            pass.Draw(6, 1, 0, 0);
            pass.End();

            queue.Submit(new ReadOnlySpan<CommandBufferPtr>(cmd.Finish()));
        }
    }
}
