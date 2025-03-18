using System.Runtime.InteropServices;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using Client.Wgsl;
using Safe = Silk.NET.WebGPU.Safe;
using static Client.Wgsl.CompositorShader;

namespace Client.Renderer
{
    public class CompositorPipeline : IDisposable
    {
        private readonly RendererApi _api;
        private readonly RenderPipelinePtr _pipeline;
        private readonly BindGroupLayoutPtr _textureBindGroupLayout;
        private readonly BufferPtr _uniformBuffer;
        private readonly CompositorShader _shader;

        private BindGroupPtr? _textureBindGroup;

        private CompositorPipeline(RendererApi api, RenderPipelinePtr pipeline,
            BindGroupLayoutPtr textureBindGroupLayout,
            BufferPtr uniformBuffer, CompositorShader shader)
        {
            _api = api;
            _pipeline = pipeline;
            _textureBindGroupLayout = textureBindGroupLayout;
            _uniformBuffer = uniformBuffer;
            _shader = shader;
        }

        public static CompositorPipeline Create(RendererApi api, CompositorShader shader,
            TextureFormat outputTextureFormat)
        {
            var device = api.Device;
            var uniformBuffer = device.CreateBuffer(BufferUsage.CopyDst | BufferUsage.Uniform,
                (u64)Marshal.SizeOf<Uniforms>());

            var textureBindGroupLayout = shader.CreateBindGroupLayout0(device);

            var pipelineLayout = shader.CreatePipelineLayout(device, textureBindGroupLayout);

            var module = shader.CreateShaderModule(api.Device, pipelineLayout);

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
                        new Safe.ColorTargetState(outputTextureFormat,
                            blendState: null, ColorWriteMask.All)
                    ]
                }
            );

            module.Release();
            pipelineLayout.Release();
            return new CompositorPipeline(api, pipeline, textureBindGroupLayout, uniformBuffer, shader);
        }

        public void Apply(QueuePtr queue, TextureViewPtr inColor, TextureViewPtr inDepth, TextureViewPtr output)
        {
            _textureBindGroup?.Release();

            var uniforms = new Uniforms
            {
                Gamma = 1
            };

            queue.WriteBuffer(_uniformBuffer, 0, new ReadOnlySpan<Uniforms>(ref uniforms));
            _textureBindGroup = _shader.CreateBindGroup0(_api.Device, _textureBindGroupLayout, _uniformBuffer, inColor, inDepth);

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
            _textureBindGroup?.Release();
            _pipeline.Release();
            _textureBindGroupLayout.Release();
            _uniformBuffer.Destroy();
        }
    }
}
