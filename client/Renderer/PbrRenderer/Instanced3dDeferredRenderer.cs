using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Client.LayerSystem;
using Client.Renderer.Pipelines;
using Client.Renderer.Resources;
using Client.Renderer.VirtualFramebuffers;
using Client.Wgsl;
using GlmSharp;
using ImGuiNET;
using NativeFileDialogExtendedSharp;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using Color = Silk.NET.WebGPU.Color;
using ColorTargetState = Silk.NET.WebGPU.Safe.ColorTargetState;
using FragmentState = Silk.NET.WebGPU.Safe.FragmentState;
using PrimitiveState = Silk.NET.WebGPU.Safe.PrimitiveState;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;
using RenderPassDepthStencilAttachment = Silk.NET.WebGPU.Safe.RenderPassDepthStencilAttachment;
using VertexState = Silk.NET.WebGPU.Safe.VertexState;

namespace Client.Renderer.PbrRenderer;

using static TempleShader;

public class Instanced3dDeferredRenderer : IRenderLayer, IUiRenderLayer
{
    private readonly ILogger<Instanced3dDeferredRenderer> _logger;
    private readonly IShaderManager _shaderManager;
    private readonly RendererApi _api;
    private readonly ResourceLoader _loader;
    private readonly Ecs _ecs;


    private InstancedMeshBuffer<ModelUniform, Instance>[] _meshBuffers = [];
    private GBuffer? _sceneGBuffer;
    private RenderTexture? _finalSceneTexture;
    private SceneLights _sceneLights = null!;
    private AjivaBuffer<DeferredShader.LightData> _lightsBuffer;
    private readonly DragCamera _camera;

    public ref bool Enabled => ref _enabled;

    private TextureAndView? _requestedCubeTexture = null;

    private bool _isSceneViewHovered;
    private ITempleShader? _templeShader;
    private DeferredPipeline? _deferredPipeline;
    private CompositorPipeline? _compositorPipeline;
    private TemplePipeline? _templePipeline;
    private PixelSamplingPipeline? _samplingPipeline;
    private SceneUniform _sceneUniform;
    private bool _enabled = true;
    private ObjectSamplingShader.Result? _underCursor;
    private u32 _selectedObjectId;
    private u32 _selectedInstanceId;

    public Instanced3dDeferredRenderer(ILogger<Instanced3dDeferredRenderer> logger, IShaderManager shaderManager,
        RendererApi api, Ecs ecs, ResourceLoader loader)
    {
        _logger = logger;
        _shaderManager = shaderManager;
        _api = api;
        _ecs = ecs;
        _loader = loader;
        _camera = new DragCamera();
    }

    private InstancedMeshBuffer<ModelUniform, Instance>[] CreateModelBuffers(string path, string? label = null)
    {
        var models = ModelHelper.LoadModels(path);

        var buffers = new InstancedMeshBuffer<ModelUniform, Instance>[models.Count + 1];

        for (var iModel = 0; iModel < models.Count; iModel++)
        {
            var mdl = models[iModel];

            var modelVertices = new Attributes[mdl.Positions.Length];

            for (var iVertex = 0; iVertex < mdl.Positions.Length; iVertex++)
            {
                modelVertices[iVertex].PositionRef = mdl.Positions[iVertex];
                modelVertices[iVertex].Tex0CoordRef = mdl.TexCoords0[iVertex];
                modelVertices[iVertex].Tex1CoordRef = mdl.TexCoords1[iVertex];
                modelVertices[iVertex].NormalRef = mdl.Normals[iVertex];
                modelVertices[iVertex].TangentRef = mdl.Tangents[iVertex];
            }

            var vertexBuffer = _loader.CreateBuffer<Attributes>(BufferUsage.Vertex | BufferUsage.CopyDst,
                modelVertices,
                label == null ? null : $"{label}::VertexBuffer");

            var indexBuffer = _loader.CreateBuffer<u32>(BufferUsage.Index | BufferUsage.CopyDst, mdl.Indices,
                label == null ? null : $"{label}::IndexBuffer");

            Instance[] instances = [new()];
            instances[0].SetTransform(Matrix4x4.Identity);
            buffers[iModel] =
                new InstancedMeshBuffer<ModelUniform, Instance>(vertexBuffer, indexBuffer,
                    new ModelUniform
                    {
                        Transform = mat4.Identity, Tex0Color = new vec3(1, 1, 1), ModelId = (u32)iModel + 1,
                        Tex1Color = new vec3(1, 1, 1), NormalMapStrength = 1
                    }, instances
                );
        }

        {
            Span<Attributes> cubeVertices = stackalloc Attributes[6 * 4];
            Span<u32> indices = stackalloc u32[6 * 6];
            Quaternion<f32>.CreateFromYawPitchRoll(0, 0, 0);

            const f32 tex1Scale = 0.3f;
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
                var a = new Attributes
                {
                    PositionRef = Vector3D.Transform(new Vector3D<f32>(-1, 1, 1), rot).ToSystem(),
                    Tex0CoordRef = new Vector2(0, 0),
                    Tex1CoordRef = new Vector2(tex1Scale, tex1Scale),
                    NormalRef = normal,
                    TangentRef = tangent
                };
                var b = new Attributes
                {
                    PositionRef = Vector3D.Transform(new Vector3D<f32>(1, 1, 1), rot).ToSystem(),
                    Tex0CoordRef = new Vector2(1, 0),
                    Tex1CoordRef = new Vector2(1 - tex1Scale, tex1Scale),
                    NormalRef = normal,
                    TangentRef = tangent
                };
                var c = new Attributes
                {
                    PositionRef = Vector3D.Transform(new Vector3D<f32>(-1, -1, 1), rot).ToSystem(),
                    Tex0CoordRef = new Vector2(0, 1),
                    Tex1CoordRef = new Vector2(tex1Scale, 1 - tex1Scale),
                    NormalRef = normal,
                    TangentRef = tangent
                };
                var d = new Attributes
                {
                    PositionRef = Vector3D.Transform(new Vector3D<f32>(1, -1, 1), rot).ToSystem(),
                    Tex0CoordRef = new Vector2(1, 1),
                    Tex1CoordRef = new Vector2(1 - tex1Scale, 1 - tex1Scale),
                    NormalRef = normal,
                    TangentRef = tangent
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
                _loader.CreateBuffer<Attributes>(BufferUsage.Vertex | BufferUsage.CopyDst, cubeVertices,
                    label == null ? null : $"{label}::VertexBuffer"),
                _loader.CreateBuffer<u32>(BufferUsage.Index | BufferUsage.CopyDst, indices,
                    label == null ? null : $"{label}::IndexBuffer"),
                new ModelUniform
                {
                    Transform = mat4.Identity, Tex0Color = new vec3(1, 1, 1), ModelId = (u32)models.Count + 1,
                    Tex1Color = new vec3(1, 1, 1), NormalMapStrength = 1
                }, instances
            );
        }

        buffers[2].InstanceInfos[0].HighlightColor = new vec4(1, 1, 0.5f, 0.5f);
        buffers[1].InstanceInfos[0].HighlightColor = new vec4(0.5f, 1, 1, 0.5f);

        return buffers;
    }

    public async Task Attach()
    {
        // get current instance of shader
        _templeShader = _shaderManager.GetCurrentShader<ITempleShader>();

        _logger.LogDebug("Creating View Buffers");

        // TextureFormat.Rgba8Unorm, TextureFormat.Rgb10A2Unorm, TextureFormat.Rgba32float, TextureFormat.Depth24Plus,
        _sceneGBuffer = GBuffer.Create(_api.Device, TextureFormat.Rgba8Unorm, TextureFormat.Rgb10A2Unorm,
            TextureFormat.Rgba32float, TextureFormat.Depth24Plus, TextureFormat.RG32Uint, label: "SceneGBuffer");

        _finalSceneTexture = RenderTexture.Create(_api.Device, TextureFormat.Rgba8Unorm, label: "FinalSceneTexture");

        _logger.LogDebug("Creating Model Buffers");
        _meshBuffers = CreateModelBuffers("resources/Temple/TempleModel.glb", "Temple");

        _logger.LogDebug("Creating uniform resources");
        {
            var albedo = _loader.AsyncTextureLoad("resources/Temple/TempleStone_alb.png");
            var normal = _loader.AsyncTextureLoad("resources/Temple/TempleStone_nrm.png");
            var emission = _loader.AsyncTextureLoad("resources/Temple/TempleStone_emm.png");
            foreach (var instancedMeshBuffer in _meshBuffers)
            {
                instancedMeshBuffer.AlbedoTexture = albedo;
                instancedMeshBuffer.NormalTexture = normal;
                instancedMeshBuffer.EmissionTexture = emission;
            }
        }

        _sceneLights = new SceneLights();
        _lightsBuffer = _loader.CreateBuffer(BufferUsage.Storage | BufferUsage.CopyDst, _sceneLights.LightData,
            "LightBuffer");


        CreatePipelines();

        foreach (var instancedMeshBuffer in _meshBuffers)
        {
            instancedMeshBuffer.ModelUniformBuffer = _loader.CreateBuffer<ModelUniform>(
                BufferUsage.Uniform | BufferUsage.CopyDst, 1, label: "ModelUniformBuffer");

            _templePipeline.RecreateBindGroup(instancedMeshBuffer);
        }

        _logger.LogDebug("Done");
    }

    [MemberNotNull(nameof(_deferredPipeline), nameof(_compositorPipeline), nameof(_templePipeline),
        nameof(_samplingPipeline))]
    private void CreatePipelines()
    {
        _deferredPipeline?.Dispose();
        _deferredPipeline = DeferredPipeline.Create(_api, _shaderManager.GetCurrentShader<IDeferredShader>(),
            _sceneGBuffer!.LightChannelFormat);
        _compositorPipeline?.Dispose();
        _compositorPipeline =
            CompositorPipeline.Create(_api, _shaderManager.GetCurrentShader<ICompositorShader>(),
                _sceneGBuffer.AlbedoChannelFormat);
        _templePipeline?.Dispose();
        _templePipeline = TemplePipeline.Create(_api, _templeShader!, _sceneGBuffer);
        _samplingPipeline?.Dispose();
        _samplingPipeline =
            PixelSamplingPipeline.Create(_api, _shaderManager.GetCurrentShader<IObjectSamplingShader>());
    }

    public void Update(UpdateInfo info)
    {
        _sceneLights.Animate(info.DeltaTime);
        if (_isSceneViewHovered)
        {
            _camera.Update();
        }

        if (_samplingPipeline is not null)
        {
            var mappingTask = _samplingPipeline.RequestResultBufferMapping();
            _api.Poll();
            _underCursor = _samplingPipeline.ReadResult(mappingTask);
            if (_underCursor is { } pos && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                _selectedObjectId = pos.ObjectId;
                _selectedInstanceId = pos.InstanceId;
            }
        }
    }

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

        /*if (_requestedCubeTexture.HasValue)
        {
            ref var instancedMeshBuffer = ref _meshBuffers[_selectedObjectId - 1];
            // todo tell resource manager to release old texture or dec ref count
            instancedMeshBuffer.AlbedoTexture = _requestedCubeTexture.Value.View;
            _templePipeline!.RecreateBindGroup(instancedMeshBuffer);
            _requestedCubeTexture = null;
        }*/

        var queue = target.QueuePtr;
        var aspectRatio = sceneViewport.Width / (f32)sceneViewport.Height;

        _sceneUniform = new SceneUniform
        {
            ViewProjectionRef = _camera.GetViewMatrix(aspectRatio),
            CameraPositionRef = _camera.Position,
            Time = info.TotalTime
        };

        _templePipeline!.WriteUniformBuffer(queue, ref _sceneUniform);

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
            _templePipeline.RecreateBindGroup(buffer);
        }

        _lightsBuffer.QueuedWrite(_sceneLights.LightData, queue);

        if (_templePipeline.BeginRenderPass() is not { } pass) return;

        foreach (var (vertexBuffer, indexBuffer, instanceBuffer, modelBindGroup, instances) in _meshBuffers)
        {
            pass.SetVertexBuffer(0, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Size);
            pass.SetVertexBuffer(1, instanceBuffer!.Buffer, instanceBuffer.Offset,
                instanceBuffer.Size);

            pass.SetIndexBuffer(indexBuffer.Buffer, IndexFormat.Uint32, indexBuffer.Offset, indexBuffer.Size);

            pass.SetBindGroup(1, modelBindGroup!.Value, ReadOnlySpan<u32>.Empty);

            pass.DrawIndexed((u32)(indexBuffer.Size / sizeof(u32)), (u32)instances.Length, 0, 0, 0);
        }

        queue.Submit(_templePipeline.EndRenderPass(pass));
    }

    private void WriteBuffer(QueuePtr queue, InstancedMeshBuffer<ModelUniform, Instance> buffer)
    {
        buffer.ModelUniformBuffer.QueuedWrite(new ReadOnlySpan<ModelUniform>(ref buffer.ModelUniform), queue);

        var data = new ReadOnlySpan<Instance>(buffer.InstanceInfos);
        if (buffer.InstanceBuffer is null || buffer.InstanceBuffer.Size <
            (u64)data.Length * (u64)Unsafe.SizeOf<Instance>())
        {
            buffer.InstanceBuffer?.Dispose();
            buffer.InstanceBuffer = _loader.CreateBuffer(BufferUsage.Vertex | BufferUsage.CopyDst,
                data, "Temple::IndexBuffer");
        }

        buffer.InstanceBuffer.QueuedWrite(data, queue);
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
        CreatePipelines();

        if (_sceneGBuffer.TryGetTargets(out var albedo, out var normal, out var emission, out var objectAndInstanceId,
                out var depth) &&
            _finalSceneTexture.TryGetTargets(out var colorOutput))
        {
            var queue = target.QueuePtr;
            Render(info, target, new Extent3D(width, height, 1));

            var mousePosition = ImGui.GetMousePos() -
                                (ImGui.GetCursorPos() + ImGui.GetWindowPos()) * ImGui.GetWindowDpiScale();
            _samplingPipeline.DispatchPixelRead(queue, new uvec2((u32)mousePosition.X, (u32)mousePosition.Y),
                depth.DepthOnlyView, objectAndInstanceId.View);

            if (!Matrix4x4.Invert(_sceneUniform.ViewProjectionRef, out var invViewProjection))
                throw new InvalidOperationException("ViewProjection Matrix could not be inverted");

            _deferredPipeline.Apply(queue, albedo.View, normal.View, depth.DepthOnlyView, emission.View,
                invViewProjection, _lightsBuffer.Buffer, _sceneLights.LightCount);

            _compositorPipeline.Apply(queue, emission.View, depth.DepthOnlyView, colorOutput.View);

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
            ImGui.Text("Under Mouse Sample:");
            if (_underCursor is { } selected)
            {
                ImGui.Text($"Object ID: {selected.ObjectId}");
                ImGui.Text($"Instance ID: {selected.InstanceId}");
                ImGui.Text($"Depth: {selected.Depth}");
            }
            else
            {
                ImGui.Text("No object under mouse");
            }

            if (_selectedObjectId != 0 && _selectedObjectId <= _meshBuffers.Length)
            {
                ref var selectedBuffer = ref _meshBuffers[_selectedObjectId - 1];

                ImGui.Columns(2, null, false);
                var y = ImGui.GetCursorPosY();
                ImGui.SetCursorPosY(y + ImGui.GetStyle().FramePadding.Y);
                ImGui.Text("Texture");
                ImGui.NextColumn();
                ImGui.SetCursorPosY(y);
                var imageButtonSize = ImGui.GetContentRegionAvail().X;

                if (ImGui.ImageButton("ChooseTexture", selectedBuffer.AlbedoTexture.Texture.GetIntPtr(),
                        new Vector2(imageButtonSize)))
                {
                    Task.Run(() =>
                    {
                        var pickResult = Nfd.FileOpen([
                            new NfdFilter { Description = "Image files", Specification = "png,jpg,jpeg,bmp" }
                        ]);

                        if (pickResult.Status != NfdStatus.Ok) return;

                        _logger.LogInformation("Not Implemented currently");
                        // using var stream = File.OpenRead(pickResult.Path);
                        /*_requestedCubeTexture =
                            _api.CreateTextureFromRgbaImage(TextureUsage.TextureBinding, stream, "CustomTexture");*/
                    });
                }

                if (_selectedInstanceId < selectedBuffer.InstanceInfos.Length)
                {
                    ref var selectedInstance = ref selectedBuffer.InstanceInfos[_selectedInstanceId];
                    ImGui.Text($"Instance");
                    ImGui.DragFloat4($"##Position", ref selectedInstance.Transform0Ref, 0.1f);
                    ImGui.DragFloat4($"##Rotation", ref selectedInstance.Transform1Ref, 0.1f);
                    ImGui.DragFloat4($"##Scale", ref selectedInstance.Transform2Ref, 0.1f);
                    ImGui.DragFloat4($"##ScaleA", ref selectedInstance.Transform3Ref, 0.1f);
                    ImGui.Text("Highlight Color");
                    ImGui.ColorEdit4($"##HighlightColor", ref selectedInstance.HighlightColorRef);
                }
            }


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
                    ImGui.DragFloat4($"##Position{hi}", ref instance.Transform0Ref, 0.1f);
                    ImGui.DragFloat4($"##Rotation{hi}", ref instance.Transform1Ref, 0.1f);
                    ImGui.DragFloat4($"##Scale{hi}", ref instance.Transform2Ref, 0.1f);
                    ImGui.DragFloat4($"##ScaleA{hi}", ref instance.Transform3Ref, 0.1f);
                    ImGui.Text("Highlight Color");
                    ImGui.ColorEdit4($"##HighlightColor{hi}", ref instance.HighlightColorRef);
                    hi++;
                }
            }
        }

        ImGui.End();

        if (ImGui.Begin("DefTexture"))
        {
            ImGui.Image(_loader.DefaultTexture.View.GetIntPtr(), new Vector2(256, 256));
        }

        ImGui.End();
    }

    public Task Detach()
    {
        _sceneGBuffer?.Release();
        _finalSceneTexture?.Release();

        foreach (var buffer in _meshBuffers)
        {
            buffer.VertexBuffer.Dispose();
            buffer.IndexBuffer.Dispose();
            buffer.ModelUniformBuffer.Dispose();
            buffer.ModelBindGroup?.Release();
            buffer.InstanceBuffer?.Dispose();
        }

        _compositorPipeline?.Dispose();
        _deferredPipeline?.Dispose();
        _templePipeline?.Dispose();

        _lightsBuffer.Buffer.Destroy();

        // set all null
        _sceneGBuffer = null;
        _finalSceneTexture = null;

        _meshBuffers = [];
        _compositorPipeline = null;
        _deferredPipeline = null;
        _lightsBuffer = null!;
        _templeShader = null;

        return Task.CompletedTask;
    }
}
