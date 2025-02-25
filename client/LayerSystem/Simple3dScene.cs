using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using client.LayerSystem;
using client.Renderer;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using static Silk.NET.WebGPU.Safe.BindGroupLayoutEntries;
using static Silk.NET.WebGPU.Safe.BindGroupEntries;
using ColorTargetState = Silk.NET.WebGPU.Safe.ColorTargetState;
using FragmentState = Silk.NET.WebGPU.Safe.FragmentState;
using PrimitiveState = Silk.NET.WebGPU.Safe.PrimitiveState;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;
using ShaderModuleCompilationHint = Silk.NET.WebGPU.Safe.ShaderModuleCompilationHint;
using VertexBufferLayout = Silk.NET.WebGPU.Safe.VertexBufferLayout;
using VertexState = Silk.NET.WebGPU.Safe.VertexState;

namespace client;

public class Simple3dScene : IRenderLayer
{
    private readonly ILogger<Simple3dScene> _logger;
    private readonly RendererApi _api;

    private BufferRange vertexBuffer;
    private SamplerPtr linearSampler;
    private BufferRange sceneUniformBuffer;
    private BindGroupPtr sceneBindGroup;
    private BufferRange modelUniformBuffer;
    private BindGroupLayoutPtr modelBindGroupLayout;
    private BindGroupPtr modelBindGroup;
    private RenderPipelinePtr pipeline;

    private (TexturePtr tex, TextureViewPtr view) cubeTexture;
    private Vector4 cubeColor = Vector4.One;

    private struct Vertex
    {
        public Vector3D<f32> Position;
        public Vector2D<f32> TexCoord;
    }

    private struct ModelUB
    {
        public Matrix4X4<f32> Transform;
        public Vector4D<f32> Color;
    }


    public Simple3dScene(ILogger<Simple3dScene> logger, RendererApi api)
    {
        _logger = logger;
        _api = api;
    }


    public async Task Attach()
    {
        var sceneBindGroupLayout = _api.Device.CreateBindGroupLayout(
            new BindGroupLayoutEntry[]
            {
                Buffer(0, ShaderStage.Vertex, BufferBindingType.Uniform, sizeof(f32) * 16)
            },
            label: "SceneBindGroupLayout"
        );

        modelBindGroupLayout = _api.Device.CreateBindGroupLayout(
            new BindGroupLayoutEntry[]
            {
                Buffer(0, ShaderStage.Vertex | ShaderStage.Fragment, BufferBindingType.Uniform,
                    (u64)Unsafe.SizeOf<ModelUB>()),
                Sampler(1, ShaderStage.Fragment, SamplerBindingType.Filtering),
                Texture(2, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D,
                    multisampled: false)
            },
            label: "ModelBindGroupLayout"
        );

        var pipelineLayout = _api.Device.CreatePipelineLayout(
            new[]
            {
                sceneBindGroupLayout,
                modelBindGroupLayout
            },
            label: "PipelineLayout"
        );

        _logger.LogDebug("Creating uniform resources");

        using (var fileStream = File.Open("resources/CubeTexture.png", FileMode.Open))
        {
            cubeTexture = _api.CreateTextureFromRGBAImage(TextureUsage.TextureBinding, fileStream,
                label: "CubeTexture.png");
        }

        linearSampler = _api.Device.CreateSampler(AddressMode.Repeat, AddressMode.Repeat, AddressMode.ClampToEdge,
            minFilter: FilterMode.Linear, magFilter: FilterMode.Linear, mipmapFilter: MipmapFilterMode.Linear,
            lodMinClamp: 0, lodMaxClamp: 1, compare: CompareFunction.Undefined, label: "LinearSampler");


        sceneUniformBuffer = new BufferRange(
            _api.Device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst,
                (u64)Unsafe.SizeOf<Matrix4X4<f32>>(),
                label: "SceneUniformBuffer"),
            0, (u64)Unsafe.SizeOf<Matrix4X4<f32>>());

        sceneBindGroup = _api.Device.CreateBindGroup(
            bindGroupLayout: sceneBindGroupLayout,
            new BindGroupEntry[]
            {
                Buffer(0, sceneUniformBuffer.Buffer, sceneUniformBuffer.Offset, sceneUniformBuffer.Size)
            },
            label: "SceneBindGroup"
        );

        modelUniformBuffer = new BufferRange(
            _api.Device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst, (u64)Unsafe.SizeOf<ModelUB>(),
                label: "ModelUniformBuffer"),
            0, (u64)Unsafe.SizeOf<ModelUB>());

        modelBindGroup = _api.Device.CreateBindGroup(
            bindGroupLayout: modelBindGroupLayout,
            new BindGroupEntry[]
            {
                Buffer(0, modelUniformBuffer.Buffer, modelUniformBuffer.Offset, modelUniformBuffer.Size),
                Sampler(1, linearSampler),
                Texture(2, cubeTexture.view)
            },
            label: "ModelBindGroup"
        );

        _logger.LogDebug("Compiling Shader");

        var shaderModule = _api.Device.CreateShaderModuleWGSL(await File.ReadAllBytesAsync("resources/shader.wgsl"),
            new ShaderModuleCompilationHint[]
            {
                new("vs_main", pipelineLayout),
                new("fs_main", pipelineLayout),
            });

        _logger.LogDebug("Creating Pipeline");

        // ReSharper disable once MethodHasAsyncOverload
        pipeline = _api.Device.CreateRenderPipeline(
            label: "DefaultPipeline",
            layout: pipelineLayout,
            vertex: new VertexState
            {
                Module = shaderModule,
                EntryPoint = "vs_main",
                Constants = [],
                Buffers =
                [
                    new VertexBufferLayout(
                        (u32)Marshal.SizeOf<Vertex>(), VertexStepMode.Vertex,
                        [
                            new VertexAttribute(VertexFormat.Float32x3, shaderLocation: 0, offset:
                                (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.Position))),
                            new VertexAttribute(VertexFormat.Float32x2, shaderLocation: 1, offset:
                                (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoord)))
                        ]
                    )
                ]
            },
            primitive: new PrimitiveState
            {
                Topology = PrimitiveTopology.TriangleList,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.Back
            },
            depthStencil: null,
            multisample: new MultisampleState
            {
                Count = 1,
                AlphaToCoverageEnabled = false,
                Mask = u32.MaxValue
            },
            fragment: new FragmentState
            {
                Module = shaderModule,
                EntryPoint = "fs_main",
                Constants = [],
                Targets =
                [
                    new ColorTargetState(
                        TextureFormat.Rgba8Unorm,
                        blendState: null,
                        ColorWriteMask.All
                    )
                ]
            }
        );

        _logger.LogDebug("Creating Vertexbuffer");


        var cubeVertices = new Vertex[6 * 6];
        Quaternion<f32>.CreateFromYawPitchRoll(0, 0, 0);

        var index = 0;
        foreach (var rot in new[]
                 {
                     Quaternion<f32>.CreateFromYawPitchRoll(0, 0, 0),
                     Quaternion<f32>.CreateFromYawPitchRoll(MathF.PI * 0.5f, 0, 0),
                     Quaternion<f32>.CreateFromYawPitchRoll(MathF.PI * 1.0f, 0, 0),
                     Quaternion<f32>.CreateFromYawPitchRoll(MathF.PI * 1.5f, 0, 0),
                     Quaternion<f32>.CreateFromYawPitchRoll(0, MathF.PI / 2, 0),
                     Quaternion<f32>.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0),
                 })
        {
            var a = new Vertex
            {
                Position = Vector3D.Transform(new Vector3D<f32>(-1, 1, 1), rot), TexCoord = new Vector2D<f32>(0, 0)
            };
            var b = new Vertex
            {
                Position = Vector3D.Transform(new Vector3D<f32>(1, 1, 1), rot), TexCoord = new Vector2D<f32>(1, 0)
            };
            var c = new Vertex
            {
                Position = Vector3D.Transform(new Vector3D<f32>(-1, -1, 1), rot), TexCoord = new Vector2D<f32>(0, 1)
            };
            var d = new Vertex
            {
                Position = Vector3D.Transform(new Vector3D<f32>(1, -1, 1), rot), TexCoord = new Vector2D<f32>(1, 1)
            };

            cubeVertices[index++] = b;
            cubeVertices[index++] = a;
            cubeVertices[index++] = c;

            cubeVertices[index++] = b;
            cubeVertices[index++] = c;
            cubeVertices[index++] = d;
        }

        ;

        vertexBuffer = _api.CreateBufferWithData(BufferUsage.Vertex | BufferUsage.CopyDst, cubeVertices,
            "TriangleVertexBuffer");

        _logger.LogDebug("Done");
    }

    public Task Update(UpdateInfo info)
    {
        return Task.CompletedTask;
    }

    public Task Detach()
    {
        throw new NotImplementedException();
    }

    public bool Enabled { get; set; } = true;

    private void RenderScene(CommandEncoderPtr cmd, QueuePtr queue, TextureViewPtr colorTarget0,
        f32 aspectRatio)
    {
    }

    private (TexturePtr tex, TextureViewPtr view)? _requestedCubeTexture = null;

    public Task Render(UpdateInfo info, RenderTarget target)
    {
        if (_requestedCubeTexture != null)
        {
            cubeTexture.tex.Destroy();
            cubeTexture.view.Release();

            cubeTexture = _requestedCubeTexture.Value;
            _requestedCubeTexture = null;

            modelBindGroup.Release();
            modelBindGroup = _api.Device.CreateBindGroup(
                bindGroupLayout: modelBindGroupLayout,
                new[]
                {
                    Buffer(0, modelUniformBuffer.Buffer, modelUniformBuffer.Offset, modelUniformBuffer.Size),
                    Sampler(1, linearSampler),
                    Texture(2, cubeTexture.view)
                },
                label: "ModelBindGroup"
            );
        }

        var (cmd, queue, sceneViewport) = target;
        var aspectRatio = sceneViewport.Width / (f32)sceneViewport.Height;
        var time = info.TotalTime;
        if (!sceneViewport.TryGetViews(out var colorTarget0, out _, out _))
            return Task.CompletedTask;

        queue.WriteBuffer(sceneUniformBuffer.Buffer, sceneUniformBuffer.Offset,
            new ReadOnlySpan<Matrix4X4<f32>>(
                Matrix4X4.CreateTranslation(0f, 0f, -10) *
                Matrix4X4.CreatePerspectiveFieldOfView((f32)((2 * Math.PI) / 5),
                    aspectRatio, 0.01f, 100f)
            ));

        queue.WriteBuffer(modelUniformBuffer.Buffer, modelUniformBuffer.Offset,
            new ReadOnlySpan<ModelUB>(new ModelUB
            {
                Transform =
                    Matrix4X4.CreateFromYawPitchRoll(time * 2, time, 0) *
                    Matrix4X4.CreateTranslation(0f, MathF.Sin(time * 2) * 0.5f + 0.5f, 0f),

                Color = cubeColor.ToGeneric()
            }));

        var pass = cmd.BeginRenderPass(new RenderPassColorAttachment[]
            {
                new(colorTarget0, resolveTarget: null,
                    LoadOp.Clear, StoreOp.Store, new Color(.06, .1, .1, 1))
            }, null, null,
            null);

        pass.SetPipeline(pipeline);

        pass.SetVertexBuffer(0, vertexBuffer.Buffer,
            vertexBuffer.Offset, vertexBuffer.Size);

        pass.SetBindGroup(0, sceneBindGroup, ReadOnlySpan<u32>.Empty);
        pass.SetBindGroup(1, modelBindGroup, ReadOnlySpan<u32>.Empty);

        pass.Draw(6 * 6, 1, 0, 0);

        pass.End();

        return Task.CompletedTask;
    }

    public IEnumerable<IVirtualView> ListVirtualViewTargets()
    {
        yield return new DelegateView("Simple3dScene", Render);
    }

    public Task ExtraWindows(UpdateInfo info)
    {
        if (ImGui.Begin("Controls"))
        {
            ImGui.Columns(2, null, false);
            f32 y = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(y + ImGui.GetStyle().FramePadding.Y);
            ImGui.Text("Texture");
            ImGui.NextColumn();
            ImGui.SetCursorPosY(y);
            var imageButtonSize = ImGui.GetContentRegionAvail().X;
            if (ImGui.ImageButton("ChooseTexture", cubeTexture.view.GetIntPtr(), new(imageButtonSize)))
            {
                var result = Nfd.FileOpen(new NfdFilter[]
                {
                    new NfdFilter { Description = "Image files", Specification = "png,jpg,jpeg,bmp" }
                });

                if (result.Status == NfdStatus.Ok)
                {
                    using var stream = new FileStream(result.Path, FileMode.Open);

                    _requestedCubeTexture = _api.CreateTextureFromRGBAImage(TextureUsage.TextureBinding, stream);
                }
            }

            ImGui.NextColumn();
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Text("Tint");
            ImGui.NextColumn();
            ImGui.ColorEdit4("##Tint", ref cubeColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha);
            ImGui.Spacing();
            ImGui.Separator();

            ImGui.End();
        }

        return Task.CompletedTask;
    }
}
