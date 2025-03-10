using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using client.LayerSystem;
using client.VirtualFramebuffers;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using static Silk.NET.WebGPU.Safe.BindGroupLayoutEntries;
using static Silk.NET.WebGPU.Safe.BindGroupEntries;
using Color = Silk.NET.WebGPU.Color;
using ColorTargetState = Silk.NET.WebGPU.Safe.ColorTargetState;
using FragmentState = Silk.NET.WebGPU.Safe.FragmentState;
using PrimitiveState = Silk.NET.WebGPU.Safe.PrimitiveState;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;
using RenderPassDepthStencilAttachment = Silk.NET.WebGPU.Safe.RenderPassDepthStencilAttachment;
using ShaderModuleCompilationHint = Silk.NET.WebGPU.Safe.ShaderModuleCompilationHint;
using VertexBufferLayout = Silk.NET.WebGPU.Safe.VertexBufferLayout;
using VertexState = Silk.NET.WebGPU.Safe.VertexState;

namespace client.Renderer.PbrRenderer;

public class Instanced3dDeferredRenderer : IRenderLayer, IUiRenderLayer
{
    private readonly ILogger<Instanced3dDeferredRenderer> _logger;
    private readonly RendererApi _api;
    private readonly Ecs _ecs;

    private SamplerPtr _linearSampler;
    private BufferRange _sceneUniformBuffer;
    private BindGroupPtr _sceneBindGroup;
    private BindGroupLayoutPtr _modelBindGroupLayout;
    private RenderPipelinePtr _modelPipeline;

    private InstancedMeshBuffer<ModelUniform, Instance>[] _meshBuffers = [];
    private GBuffer? _sceneGBuffer;
    private RenderTexture? _finalSceneTexture;
    private SceneLights _sceneLights;
    private BufferRange _lightsBuffer;

    public Instanced3dDeferredRenderer(ILogger<Instanced3dDeferredRenderer> logger, RendererApi api, Ecs ecs)
    {
        _logger = logger;
        _api = api;
        _ecs = ecs;
        _camera = new DragCamera();
    }

    private InstancedMeshBuffer<ModelUniform, Instance>[] CreateModelBuffers(string path, string? label = null)
    {
        var models = ModelHelper.LoadModels(path);

        var buffers = new InstancedMeshBuffer<ModelUniform, Instance>[models.Count + 1];

        for (var iModel = 0; iModel < models.Count; iModel++)
        {
            var mdl = models[iModel];

            var modelVertices = new Vertex[mdl.Positions.Length];

            for (var iVertex = 0; iVertex < mdl.Positions.Length; iVertex++)
            {
                modelVertices[iVertex].Position = mdl.Positions[iVertex];
                modelVertices[iVertex].TexCoord0 = mdl.TexCoords0[iVertex];
                modelVertices[iVertex].TexCoord1 = mdl.TexCoords1[iVertex];
                modelVertices[iVertex].Normal = mdl.Normals[iVertex];
                modelVertices[iVertex].Tangent = mdl.Tangents[iVertex];
            }

            var vertexBuffer = _api.CreateBufferWithData<Vertex>(BufferUsage.Vertex | BufferUsage.CopyDst,
                modelVertices,
                label == null ? null : $"{label}::VertexBuffer");

            var indexBuffer = _api.CreateBufferWithData<u32>(BufferUsage.Index | BufferUsage.CopyDst, mdl.Indices,
                label == null ? null : $"{label}::IndexBuffer");

            Instance[] instances = [new()];
            instances[0].SetTransform(Matrix4x4.Identity);
            buffers[iModel] =
                new InstancedMeshBuffer<ModelUniform, Instance>(vertexBuffer, indexBuffer,
                    new ModelUniform
                    {
                        Transform = Matrix4x4.Identity, Tex0Color = new Vector3(1, 1, 1), ModelId = (u32)iModel,
                        Tex1Color = new Vector3(1, 1, 1), NormalMapStrength = 1
                    }, instances
                );
        }

        {
            Span<Vertex> cubeVertices = stackalloc Vertex[6 * 4];
            Span<u32> indices = stackalloc u32[6 * 6];
            Quaternion<f32>.CreateFromYawPitchRoll(0, 0, 0);

            var vIndex = 0;
            var iIndex = 0;
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
                var normal = Vector3D.Transform(new Vector3D<f32>(0, 0, 1), rot).ToSystem();
                var tangent = Vector3D.Transform(new Vector3D<f32>(1, 0, 0), rot).ToSystem();
                var a = new Vertex
                {
                    Position = Vector3D.Transform(new Vector3D<f32>(-1, 1, 1), rot).ToSystem(),
                    TexCoord0 = new Vector2(0, 0),
                    TexCoord1 = new Vector2(0, 0),
                    Normal = normal,
                    Tangent = tangent
                };
                var b = new Vertex
                {
                    Position = Vector3D.Transform(new Vector3D<f32>(1, 1, 1), rot).ToSystem(),
                    TexCoord0 = new Vector2(1, 0),
                    TexCoord1 = new Vector2(1, 0),
                    Normal = normal,
                    Tangent = tangent
                };
                var c = new Vertex
                {
                    Position = Vector3D.Transform(new Vector3D<f32>(-1, -1, 1), rot).ToSystem(),
                    TexCoord0 = new Vector2(0, 1),
                    TexCoord1 = new Vector2(0, 1),
                    Normal = normal,
                    Tangent = tangent
                };
                var d = new Vertex
                {
                    Position = Vector3D.Transform(new Vector3D<f32>(1, -1, 1), rot).ToSystem(),
                    TexCoord0 = new Vector2(1, 1),
                    TexCoord1 = new Vector2(1, 1),
                    Normal = normal,
                    Tangent = tangent
                };

                /*cubeVertices[index++] = b;
                cubeVertices[index++] = a;
                cubeVertices[index++] = c;

                cubeVertices[index++] = b;
                cubeVertices[index++] = c;
                cubeVertices[index++] = d;*/
                cubeVertices[vIndex++] = a;
                cubeVertices[vIndex++] = b;
                cubeVertices[vIndex++] = c;
                cubeVertices[vIndex++] = d;

                indices[iIndex++] = (u32)(vIndex - 4);
                indices[iIndex++] = (u32)(vIndex - 2);
                indices[iIndex++] = (u32)(vIndex - 3);
                indices[iIndex++] = (u32)(vIndex - 2);
                indices[iIndex++] = (u32)(vIndex - 1);
                indices[iIndex++] = (u32)(vIndex - 3);
            }

            Instance[] instances = [new()];
            instances[0].SetTransform(Matrix4x4.Identity);

            buffers[^1] = new InstancedMeshBuffer<ModelUniform, Instance>(
                _api.CreateBufferWithData<Vertex>(BufferUsage.Vertex | BufferUsage.CopyDst, cubeVertices,
                    label == null ? null : $"{label}::VertexBuffer"),
                _api.CreateBufferWithData<u32>(BufferUsage.Index | BufferUsage.CopyDst, indices,
                    label == null ? null : $"{label}::IndexBuffer"),
                new ModelUniform
                {
                    Transform = Matrix4x4.Identity, Tex0Color = new Vector3(1, 1, 1), ModelId = (u32)models.Count,
                    Tex1Color = new Vector3(1, 1, 1), NormalMapStrength = 1
                }, instances
            );
        }

        buffers[2].InstanceInfos[0].HighlightColor = new Vector4(1, 1, 0.5f, 0.5f);
        buffers[1].InstanceInfos[0].HighlightColor = new Vector4(0.5f, 1, 1, 0.5f);

        return buffers;
    }

    public async Task Attach()
    {
        _logger.LogDebug("Creating View Buffers");

        // TextureFormat.Rgba8Unorm, TextureFormat.Rgb10A2Unorm, TextureFormat.Rgba32float, TextureFormat.Depth24Plus,
        _sceneGBuffer = GBuffer.Create(_api.Device, TextureFormat.Rgba8Unorm, TextureFormat.Rgb10A2Unorm,
            TextureFormat.Rgba32float, TextureFormat.Depth24Plus,
            label: "SceneGBuffer");

        _finalSceneTexture = RenderTexture.Create(_api.Device, TextureFormat.Rgba8Unorm, label: "FinalSceneTexture");

        _logger.LogDebug("Creating BindGroupLayouts");
        var sceneBindGroupLayout = _api.Device.CreateBindGroupLayout(
            [Buffer(0, ShaderStage.Vertex, BufferBindingType.Uniform, (u32)Marshal.SizeOf<SceneUniform>())],
            label: "SceneBindGroupLayout"
        );

        _modelBindGroupLayout = _api.Device.CreateBindGroupLayout(
            [
                // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
                Buffer(0, ShaderStage.Vertex | ShaderStage.Fragment, BufferBindingType.Uniform,
                    (u64)Unsafe.SizeOf<ModelUniform>()),
                Sampler(1, ShaderStage.Fragment, SamplerBindingType.Filtering),
                Texture(2, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D,
                    multisampled: false),
                Texture(3, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D,
                    multisampled: false),
                Texture(4, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D,
                    multisampled: false)
            ],
            label: "ModelBindGroupLayout"
        );

        var pipelineLayout = _api.Device.CreatePipelineLayout(
            new[]
            {
                sceneBindGroupLayout,
                _modelBindGroupLayout
            },
            label: "PipelineLayout"
        );

        _logger.LogDebug("Creating Model Buffers");
        _meshBuffers = CreateModelBuffers("resources/Temple/TempleModel.glb", "Temple");

        _logger.LogDebug("Creating uniform resources");
        {
            var albedo = await _api.LoadTextureAsync("resources/Temple/TempleStone_alb.png");
            var normal = await _api.LoadTextureAsync("resources/Temple/TempleStone_nrm.png");
            var emission = await _api.LoadTextureAsync("resources/Temple/TempleStone_emm.png");
            foreach (var instancedMeshBuffer in _meshBuffers)
            {
                instancedMeshBuffer.AlbedoTexture = albedo;
                instancedMeshBuffer.NormalTexture = normal;
                instancedMeshBuffer.EmissionTexture = emission;
            }
        }

        _linearSampler = _api.Device.CreateSampler(AddressMode.Repeat, AddressMode.Repeat, AddressMode.ClampToEdge,
            minFilter: FilterMode.Linear, magFilter: FilterMode.Linear, mipmapFilter: MipmapFilterMode.Linear,
            lodMinClamp: 0, lodMaxClamp: 1, compare: CompareFunction.Undefined, label: "LinearSampler");

        _sceneUniformBuffer = new BufferRange(
            _api.Device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst,
                (u64)Unsafe.SizeOf<SceneUniform>(), label: "SceneUniformBuffer"),
            0, (u64)Unsafe.SizeOf<SceneUniform>());

        _sceneLights = new SceneLights();
        _lightsBuffer = _api.CreateBufferWithData(BufferUsage.Storage | BufferUsage.CopyDst, _sceneLights.LightData,
            "LightBuffer");

        _sceneBindGroup = _api.Device.CreateBindGroup(
            bindGroupLayout: sceneBindGroupLayout,
            [Buffer(0, _sceneUniformBuffer.Buffer, _sceneUniformBuffer.Offset, _sceneUniformBuffer.Size)],
            label: "SceneBindGroup"
        );

        foreach (var instancedMeshBuffer in _meshBuffers)
        {
            instancedMeshBuffer.ModelUniformBuffer = new BufferRange(
                _api.Device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst,
                    (u64)Unsafe.SizeOf<ModelUniform>(), label: "ModelUniformBuffer"),
                0, (u64)Unsafe.SizeOf<ModelUniform>());

            RecreateBindGroup(instancedMeshBuffer);
        }

        _logger.LogDebug("Compiling Shader");


        var shaderModule = _api.CreateShaderModuleFromFile("resources/Temple/TempleShader.wgsl", "TempleShaderModule",
            new ShaderModuleCompilationHint("vs_main", pipelineLayout),
            new ShaderModuleCompilationHint("fs_main", pipelineLayout));

        // ReSharper disable once MethodHasAsyncOverload
        _modelPipeline = _api.Device.CreateRenderPipeline(
            label: "DefaultPipeline",
            layout: pipelineLayout,
            vertex: new VertexState
            {
                Module = shaderModule,
                EntryPoint = "vs_main",
                Constants = [],
                Buffers =
                [
                    // vertex buffer
                    new VertexBufferLayout(
                        (u32)Marshal.SizeOf<Vertex>(), VertexStepMode.Vertex,
                        [
                            new VertexAttribute(VertexFormat.Float32x3, shaderLocation: 0, offset:
                                (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.Position))),
                            new VertexAttribute(VertexFormat.Float32x2, shaderLocation: 1, offset:
                                (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoord0))),
                            new VertexAttribute(VertexFormat.Float32x2, shaderLocation: 2, offset:
                                (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoord1))),
                            new VertexAttribute(VertexFormat.Float32x3, shaderLocation: 3, offset:
                                (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.Normal))),
                            new VertexAttribute(VertexFormat.Float32x3, shaderLocation: 4, offset:
                                (u32)Marshal.OffsetOf<Vertex>(nameof(Vertex.Tangent))),
                        ]
                    ),
                    // instance buffer
                    new VertexBufferLayout(
                        (u32)Marshal.SizeOf<Instance>(), VertexStepMode.Instance,
                        [
                            new VertexAttribute(VertexFormat.Float32x4, shaderLocation: 5, offset:
                                (u32)Marshal.OffsetOf<Instance>(nameof(Instance.Transform0))),
                            new VertexAttribute(VertexFormat.Float32x4, shaderLocation: 6, offset:
                                (u32)Marshal.OffsetOf<Instance>(nameof(Instance.Transform1))),
                            new VertexAttribute(VertexFormat.Float32x4, shaderLocation: 7, offset:
                                (u32)Marshal.OffsetOf<Instance>(nameof(Instance.Transform2))),
                            new VertexAttribute(VertexFormat.Float32x4, shaderLocation: 8, offset:
                                (u32)Marshal.OffsetOf<Instance>(nameof(Instance.Transform3))),
                            new VertexAttribute(VertexFormat.Float32x4, shaderLocation: 9, offset:
                                (u32)Marshal.OffsetOf<Instance>(nameof(Instance.HighlightColor))),
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
            depthStencil: new DepthStencilState
            {
                Format = _sceneGBuffer.DepthStencilFormat,
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
                Module = shaderModule,
                EntryPoint = "fs_main",
                Constants = [],
                Targets =
                [
                    new ColorTargetState(
                        _sceneGBuffer.AlbedoChannelFormat,
                        blendState: null,
                        ColorWriteMask.All
                    ),
                    new ColorTargetState(
                        _sceneGBuffer.NormalChannelFormat,
                        blendState: null,
                        ColorWriteMask.All
                    ),
                    new ColorTargetState(
                        _sceneGBuffer.LightChannelFormat,
                        blendState: null,
                        ColorWriteMask.All
                    )
                ]
            }
        );

        _logger.LogDebug("Creating DeferredShader and Compositor");

        _deferredShader = DeferredShader.Create(_api, _sceneGBuffer.LightChannelFormat);

        _compositor = Compositor.Create(_api, _sceneGBuffer.AlbedoChannelFormat);

        _logger.LogDebug("Done");
    }

    private void RecreateBindGroup(InstancedMeshBuffer<ModelUniform, Instance> instancedMeshBuffer)
    {
        instancedMeshBuffer.ModelBindGroup?.Release();
        instancedMeshBuffer.ModelBindGroup = _api.Device.CreateBindGroup(
            bindGroupLayout: _modelBindGroupLayout,
            [
                Buffer(0, instancedMeshBuffer.ModelUniformBuffer.Buffer, instancedMeshBuffer.ModelUniformBuffer.Offset,
                    instancedMeshBuffer.ModelUniformBuffer.Size),
                Sampler(1, _linearSampler),
                Texture(2, instancedMeshBuffer.AlbedoTexture.View),
                Texture(3, instancedMeshBuffer.NormalTexture.View),
                Texture(4, instancedMeshBuffer.EmissionTexture.View),
            ],
            label: "ModelBindGroup"
        );
    }

    public void Update(UpdateInfo info)
    {
        _sceneLights.Animate(info.DeltaTime);
        if (_isSceneViewHovered)
        {
            _camera.Update();
        }
    }

    public ref bool Enabled => ref _enabled;

    private (TexturePtr tex, TextureViewPtr view)? _requestedCubeTexture = null;

    private bool _isSceneViewHovered;
    private DeferredShader _deferredShader;
    private Compositor _compositor;
    private DragCamera _camera;
    private SceneUniform _sceneUniform;
    private bool _enabled = true;

    public void Render(UpdateInfo info, RenderTarget target, Extent3D sceneViewport)
    {
        /*bool rebuildBindGroup = false;
        if (_requestedCubeTexture != null)
        {
            _cubeTexture.tex.Destroy();
            _cubeTexture.view.Release();

            _cubeTexture = _requestedCubeTexture.Value;
            _requestedCubeTexture = null;

            rebuildBindGroup = true;
        }

        if (rebuildBindGroup)
        {
            _modelBindGroup.Release();
            _modelBindGroup = _api.Device.CreateBindGroup(
                bindGroupLayout: _modelBindGroupLayout,
                new[]
                {
                    Buffer(0, modelUniformBuffer.Buffer, modelUniformBuffer.Offset, modelUniformBuffer.Size),
                    Sampler(1, _linearSampler),
                    Texture(2, _cubeTexture.view)
                },
                label: "ModelBindGroup"
            );
        }*/

        var queue = target.QueuePtr;
        var aspectRatio = sceneViewport.Width / (f32)sceneViewport.Height;

        _sceneUniform = new SceneUniform
        {
            ViewProjection = _camera.GetViewMatrix(aspectRatio),
            CameraPosition = _camera.Position,
            Time = info.TotalTime
        };

        queue.WriteBuffer(_sceneUniformBuffer.Buffer, _sceneUniformBuffer.Offset,
            new ReadOnlySpan<SceneUniform>(ref _sceneUniform));

        var players = _ecs.GetPlayers().ToArray();
        if (_meshBuffers[^1].InstanceInfos.Length != players.Length + 1)
            _meshBuffers[^1].InstanceInfos = new Instance[players.Length + 1];

        ref var instanceInfo = ref _meshBuffers[^1].InstanceInfos;
        instanceInfo[0].SetTransform(Matrix4x4.CreateTranslation(_camera.Position));
        _ecs.LocalPos = _camera.Position;
        for (var i = 0; i < players.Length; i++)
        {
            instanceInfo[i + 1].SetTransform(Matrix4x4.CreateTranslation(players[i].Position));
        }

        foreach (var buffer in _meshBuffers)
        {
            WriteBuffer(queue, buffer);
        }

        queue.WriteBuffer(_lightsBuffer.Buffer, _lightsBuffer.Offset, _sceneLights.LightData);

        if (!_sceneGBuffer!.TryGetTargets(out var albedo, out var normal, out var emission, out var depthStencil))
            return;

        var cmd = _api.Device.CreateCommandEncoder();

        var pass = cmd.BeginRenderPass([
                new RenderPassColorAttachment(albedo.View, resolveTarget: null,
                    LoadOp.Clear, StoreOp.Store, new Color(.06, .1, .1, 1)),
                new RenderPassColorAttachment(normal.View, resolveTarget: null,
                    LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0, 1)),
                new RenderPassColorAttachment(emission.View, resolveTarget: null,
                    LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0.1, 1))
            ],
            null,
            new RenderPassDepthStencilAttachment(depthStencil.View, LoadOp.Clear, StoreOp.Store, 1f, false,
                /*stencil*/ LoadOp.Clear, StoreOp.Discard, 0, true),
            null);

        pass.SetPipeline(_modelPipeline);


        foreach (var (vertexBuffer, indexBuffer, instanceBuffer, modelBindGroup, instances) in _meshBuffers)
        {
            pass.SetVertexBuffer(0, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Size);
            pass.SetVertexBuffer(1, instanceBuffer!.Value.Buffer, instanceBuffer.Value.Offset,
                instanceBuffer.Value.Size);

            pass.SetIndexBuffer(indexBuffer.Buffer, IndexFormat.Uint32, indexBuffer.Offset, indexBuffer.Size);

            pass.SetBindGroup(0, _sceneBindGroup, ReadOnlySpan<u32>.Empty);
            pass.SetBindGroup(1, modelBindGroup!.Value, ReadOnlySpan<u32>.Empty);

            pass.DrawIndexed((u32)(indexBuffer.Size / sizeof(u32)), (u32)instances.Length, 0, 0, 0);
        }

        pass.End();
        queue.Submit(new[] { cmd.Finish() });
    }
    

    private void WriteBuffer(QueuePtr queue, InstancedMeshBuffer<ModelUniform, Instance> buffer)
    {
        queue.WriteBuffer(buffer.ModelUniformBuffer.Buffer, buffer.ModelUniformBuffer.Offset,
            new ReadOnlySpan<ModelUniform>(ref buffer.ModelUniform));

        var data = new ReadOnlySpan<Instance>(buffer.InstanceInfos);
        if (buffer.InstanceBuffer is null || buffer.InstanceBuffer.Value.Size <
            (u64)data.Length * (u64)Unsafe.SizeOf<Instance>())
        {
            buffer.InstanceBuffer?.Buffer.Release();
            buffer.InstanceBuffer = _api.CreateBufferWithData(BufferUsage.Vertex | BufferUsage.CopyDst,
                data, "Temple::IndexBuffer");
        }

        queue.WriteBuffer(buffer.InstanceBuffer.Value.Buffer, buffer.InstanceBuffer.Value.Offset, data);
    }

    public void Render(UpdateInfo info, RenderTarget target)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        if (!ImGui.Begin("Scene") || _sceneGBuffer is null || _finalSceneTexture is null) return;
        var size = ImGui.GetContentRegionAvail();

        //var imDrawListPtr = ImGui.GetBackgroundDrawList();
        //var size = imDrawListPtr.GetClipRectMax();
        u32 width = (u32)size.X, height = (u32)Math.Max(0, size.Y);

        var prevWidth = _sceneGBuffer.Width;
        var prevHeight = _sceneGBuffer.Height;
        var hasPrev = _finalSceneTexture.TryGetTargets(out var prevView);
        _sceneGBuffer.EnsureSize(width, height);
        _finalSceneTexture.EnsureSize(width, height);

        if (_sceneGBuffer.TryGetTargets(out var albedo, out var normal, out var emission, out var depth) &&
            _finalSceneTexture.TryGetTargets(out var colorOutput))
        {
            var queue = target.QueuePtr;
            Render(info, target, new Extent3D(width, height, 1));

            if (!Matrix4x4.Invert(_sceneUniform.ViewProjection, out var invViewProjection))
                throw new InvalidOperationException("ViewProjection Matrix could not be inverted");

            _deferredShader.Apply(queue, albedo.View, normal.View, depth.DepthOnlyView, emission.View,
                invViewProjection, _lightsBuffer.Buffer, _sceneLights.LightCount);

            _compositor.Apply(queue, emission.View, depth.DepthOnlyView, colorOutput.View);


            if (prevWidth != width && prevHeight != height && hasPrev)
                target.ImguiController.RemoveTextureBindGroup(prevView.View);

            //imDrawListPtr.AddImage(color0.GetIntPtr(), new Vector2(0, 0), new Vector2(width, height));

            ImGui.Image(colorOutput.View.GetIntPtr(), new Vector2(width, height));
            _isSceneViewHovered = ImGui.IsItemHovered();
        }

        ImGui.End();

        ImGui.PopStyleVar();
    }

    public void RenderUi(UpdateInfo info)
    {
        if (ImGui.Begin("Controls"))
        {
            ImGui.Columns(2, null, false);
            var y = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(y + ImGui.GetStyle().FramePadding.Y);
            ImGui.Text("Texture");
            ImGui.NextColumn();
            ImGui.SetCursorPosY(y);
            var imageButtonSize = ImGui.GetContentRegionAvail().X;
            /*
            if (ImGui.ImageButton("ChooseTexture", _cubeTexture.view.GetIntPtr(), new(imageButtonSize)))
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
            */

            ImGui.NextColumn();
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Text("Tint");
            ImGui.NextColumn();
            //ImGui.ColorEdit4("##Tint", ref _cubeColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha);
            ImGui.Spacing();
            ImGui.Separator();

            ImGui.Columns(1);
            var hi = 0;
            foreach (var buffer in _meshBuffers)
            {
                for (var i = 0; i < buffer.InstanceInfos.Length; i++)
                {
                    ref var instance = ref buffer.InstanceInfos[i];
                    ImGui.Text($"Instance {hi}");
                    ImGui.DragFloat4($"##Position{hi}", ref instance.Transform0, 0.1f);
                    ImGui.DragFloat4($"##Rotation{hi}", ref instance.Transform1, 0.1f);
                    ImGui.DragFloat4($"##Scale{hi}", ref instance.Transform2, 0.1f);
                    ImGui.DragFloat4($"##ScaleA{hi}", ref instance.Transform3, 0.1f);
                    ImGui.Text("Highlight Color");
                    ImGui.ColorEdit4($"##HighlightColor{hi}", ref instance.HighlightColor);
                    hi++;
                }
            }
        }

        ImGui.End();
    }


    public Task Detach()
    {
        _sceneGBuffer?.Release();
        _finalSceneTexture?.Release();
        _linearSampler.Release();
        _sceneUniformBuffer.Buffer.Release();
        _sceneBindGroup.Release();
        _modelBindGroupLayout.Release();
        _modelPipeline.Release();
        foreach (var buffer in _meshBuffers)
        {
            buffer.VertexBuffer.Buffer.Release();
            buffer.IndexBuffer.Buffer.Release();
            buffer.ModelUniformBuffer.Buffer.Release();
            buffer.ModelBindGroup?.Release();
            buffer.InstanceBuffer?.Buffer.Release();
        }

        _lightsBuffer.Buffer.Release();
        return Task.CompletedTask;
    }
}
