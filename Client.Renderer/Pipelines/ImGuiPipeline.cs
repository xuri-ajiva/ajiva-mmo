using Client.Renderer.Resources;
using Client.Wgsl;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using ColorTargetState = Silk.NET.WebGPU.Safe.ColorTargetState;
using FragmentState = Silk.NET.WebGPU.Safe.FragmentState;
using PrimitiveState = Silk.NET.WebGPU.Safe.PrimitiveState;
using VertexBufferLayout = Silk.NET.WebGPU.Safe.VertexBufferLayout;
using VertexState = Silk.NET.WebGPU.Safe.VertexState;

namespace Client.Renderer.Pipelines;

public class ImGuiPipeline(
    ResourceManager loader,
    IImGuiShader shader,
    RenderPipelinePtr pipeline,
    BindGroupLayoutPtr layout0,
    BindGroupLayoutPtr layout1,
    BindGroupPtr bind0,
    SamplerPtr pointSampler,
    AjivaBuffer<ImGuiShader.Uniforms> uniformBuffer) : IDisposable
{
    public bool IsUpToDate(IImGuiShader currentShader) => shader == currentShader;

    public static ImGuiPipeline Create(ResourceManager loader, IImGuiShader shader, TextureFormat colorOutputFormat)
    {
        var api = loader.Api;
        var device = api.Device;

        var layout0 = shader.CreateBindGroupLayout0(device);
        var layout1 = shader.CreateBindGroupLayout1(device);

        var pipelineLayout = shader.CreatePipelineLayout(device, layout0, layout1);

        var shaderModulePtr = shader.CreateShaderModule(device, pipelineLayout);
        var pipeline = device.CreateRenderPipeline(pipelineLayout,
            vertex: new VertexState
            {
                Module = shaderModulePtr,
                EntryPoint = shader.VertexEntryPoint,
                Buffers =
                [
                    new VertexBufferLayout()
                    {
                        ArrayStride = 5 * sizeof(f32),
                        StepMode = VertexStepMode.Vertex,
                        Attributes =
                        [
                            //in_position
                            new VertexAttribute(VertexFormat.Float32x2, offset: 0, shaderLocation: 0),
                            //in_texCoord
                            new VertexAttribute(VertexFormat.Float32x2, offset: 2 * sizeof(f32), shaderLocation: 1),
                            //in_color
                            new VertexAttribute(VertexFormat.Unorm8x4, offset: 4 * sizeof(f32), shaderLocation: 2)
                        ]
                    }
                ],
                Constants = []
            },
            primitive: new PrimitiveState
            {
                Topology = PrimitiveTopology.TriangleList,
                StripIndexFormat = IndexFormat.Undefined,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.None
            },
            depthStencil: null,
            multisample: new MultisampleState
            {
                Count = 1,
                Mask = u32.MaxValue,
                AlphaToCoverageEnabled = false
            },
            fragment: new FragmentState
            {
                Module = shaderModulePtr,
                EntryPoint = shader.FragmentEntryPoint,
                Targets =
                [
                    new ColorTargetState(colorOutputFormat, blendState: (color:
                            new BlendComponent
                            {
                                Operation = BlendOperation.Add,
                                SrcFactor = BlendFactor.SrcAlpha,
                                DstFactor = BlendFactor.OneMinusSrcAlpha
                            }, alpha: new BlendComponent
                            {
                                Operation = BlendOperation.Add,
                                SrcFactor = BlendFactor.One,
                                DstFactor = BlendFactor.OneMinusSrcAlpha
                            })
                        , ColorWriteMask.All)
                ],
                Constants = []
            }, label: "ImGui Pipeline");

        var pointSampler = device.CreateSampler(
            AddressMode.Repeat, AddressMode.Repeat, AddressMode.Repeat,
            FilterMode.Nearest, FilterMode.Nearest, MipmapFilterMode.Nearest,
            0, 0, default, 1,
            "ImGui.NET PointSampler");

        var uniformBuffer = loader.CreateBuffer<ImGuiShader.Uniforms>(BufferUsage.Uniform | BufferUsage.CopyDst, 1,
            label: "ImGui Uniform Buffer");

        var bind0 = shader.CreateBindGroup0(device, layout0, uniformBuffer.Buffer, pointSampler);
        shaderModulePtr.Release();
        pipelineLayout.Release();

        return new ImGuiPipeline(loader, shader, pipeline, layout0, layout1, bind0, pointSampler, uniformBuffer);
    }

    public void Dispose()
    {
        pipeline.Release();
        bind0.Release();
        layout0.Release();
        layout1.Release();
        pointSampler.Release();
        uniformBuffer.Dispose();
    }

    public BindGroupPtr CreateTextureBindGroup(TextureViewPtr textureViewPtr) =>
        shader.CreateBindGroup1(loader.Api.Device, layout1, textureViewPtr, "ImGui Texture BindGroup");

    public RenderPipelinePtr Pipeline => pipeline;
    public BindGroupPtr BindGroup0 => bind0;

    public void UpdateUniform(ImGuiShader.Uniforms uniforms, QueuePtr queue)
    {
        uniformBuffer.QueuedWrite([uniforms], queue);
    }
}