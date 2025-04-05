using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

namespace Client.Renderer.PbrRenderer;

public class Instanced3dDeferredRenderer : IRenderLayer, IUiRenderLayer
{
    private readonly ILogger<Instanced3dDeferredRenderer> _logger;
    private readonly IShaderManager _shaderManager;
    private readonly RendererApi _api;
    private readonly ResourceLoader _loader;
    private readonly Ecs _ecs;


    private PBRInstancedMeshBuffer[] _meshBuffers = [];
    private OITInstancedMeshBuffer[] _transparentMeshBuffers = [];

    private GBuffer? _sceneGBuffer;
    private RenderTexture? _finalSceneTexture;
    private SceneLights _sceneLights = null!;
    private AjivaBuffer<DeferredShader.LightData> _lightsBuffer;
    private readonly DragCamera _camera;

    public ref bool Enabled => ref _enabled;

    private bool _isSceneViewHovered;
    private DeferredPipeline? _deferredPipeline;
    private CompositorPipeline? _compositorPipeline;
    private TemplePipeline? _templePipeline;
    private PixelSamplingPipeline? _samplingPipeline;
    private WboitPipeline? _transparentPipeline;
    private TempleShader.SceneUniform _sceneUniform;
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

    private AjivaMesh<T> GenerateCubeMesh<T>(Func<i32, Quaternion<f32>, Vector3, Vector3, T> creator)
        where T : unmanaged
    {
        Span<T> cubeVertices = stackalloc T[6 * 4];
        Span<u32> indices = stackalloc u32[6 * 6];
        Quaternion<f32>.CreateFromYawPitchRoll(0, 0, 0);

        var vIndex = 0;
        var iIndex = 0;
        ReadOnlySpan<Quaternion<f32>> rotations =
        [
            Quaternion<f32>.CreateFromYawPitchRoll(0, 0, 0),
            Quaternion<f32>.CreateFromYawPitchRoll(MathF.PI * 0.5f, 0, 0),
            Quaternion<f32>.CreateFromYawPitchRoll(MathF.PI * 1.0f, 0, 0),
            Quaternion<f32>.CreateFromYawPitchRoll(MathF.PI * 1.5f, 0, 0),
            Quaternion<f32>.CreateFromYawPitchRoll(0, MathF.PI / 2, 0),
            Quaternion<f32>.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0),
        ];
        foreach (var rot in rotations)
        {
            var normal = Vector3D.Transform(new Vector3D<f32>(0, 0, 1), rot).ToSystem();
            var tangent = Vector3D.Transform(new Vector3D<f32>(1, 0, 0), rot).ToSystem();

            cubeVertices[vIndex++] = creator(0, rot, normal, tangent);
            cubeVertices[vIndex++] = creator(1, rot, normal, tangent);
            cubeVertices[vIndex++] = creator(2, rot, normal, tangent);
            cubeVertices[vIndex++] = creator(3, rot, normal, tangent);

            indices[iIndex++] = (u32)(vIndex - 4);
            indices[iIndex++] = (u32)(vIndex - 2);
            indices[iIndex++] = (u32)(vIndex - 3);
            indices[iIndex++] = (u32)(vIndex - 2);
            indices[iIndex++] = (u32)(vIndex - 1);
            indices[iIndex++] = (u32)(vIndex - 3);
        }

        return new AjivaMesh<T>(
            _loader.CreateBuffer<T>(BufferUsage.Vertex | BufferUsage.CopyDst, cubeVertices,
                $"CubeMesh::VertexBuffer"),
            _loader.CreateBuffer<u32>(BufferUsage.Index | BufferUsage.CopyDst, indices,
                $"CubeMesh::IndexBuffer"));
    }

    private PBRInstancedMeshBuffer[] CreateModelBuffers(string path,
        string? label = null)
    {
        var models = ModelHelper.LoadModels(path);

        var buffers = new PBRInstancedMeshBuffer[models.Count + 2];

        for (var iModel = 0; iModel < models.Count; iModel++)
        {
            var mdl = models[iModel];

            var modelVertices = new TempleShader.Attributes[mdl.Positions.Length];

            for (var iVertex = 0; iVertex < mdl.Positions.Length; iVertex++)
            {
                modelVertices[iVertex].PositionRef = mdl.Positions[iVertex];
                modelVertices[iVertex].Tex0CoordRef = mdl.TexCoords0[iVertex];
                modelVertices[iVertex].Tex1CoordRef = mdl.TexCoords1[iVertex];
                modelVertices[iVertex].NormalRef = mdl.Normals[iVertex];
                modelVertices[iVertex].TangentRef = mdl.Tangents[iVertex];
            }

            var vertexBuffer = _loader.CreateBuffer<TempleShader.Attributes>(BufferUsage.Vertex | BufferUsage.CopyDst,
                modelVertices,
                label == null ? null : $"{label}::VertexBuffer");

            var indexBuffer = _loader.CreateBuffer<u32>(BufferUsage.Index | BufferUsage.CopyDst, mdl.Indices,
                label == null ? null : $"{label}::IndexBuffer");

            TempleShader.Instance[] instances = [new()];
            instances[0].SetTransform(Matrix4x4.Identity);
            buffers[iModel] =
                new PBRInstancedMeshBuffer(
                    new AjivaMesh<TempleShader.Attributes>(vertexBuffer, indexBuffer),
                    new TempleShader.ModelUniform
                    {
                        Transform = mat4.Identity, Tex0Color = new vec3(1, 1, 1), ModelId = (u32)iModel + 1,
                        Tex1Color = new vec3(1, 1, 1), NormalMapStrength = 1
                    }, instances
                );
        }

        TempleShader.Instance[] cubes = [new()];
        TempleShader.Instance[] floor = [new()];
        floor[0].SetTransform(Matrix4x4.CreateScale(new Vector3(100, 0.1f, 100)) *
                              Matrix4x4.CreateFromYawPitchRoll(0, 0, 0) *
                              Matrix4x4.CreateTranslation(new Vector3(0, -2, 0)));
        cubes[0].SetTransform(Matrix4x4.Identity);
        const f32 tex1Scale = 0;
        var generateCubeMesh = GenerateCubeMesh((u, rot, normal, tangent) =>
        {
            ReadOnlySpan<Vector3D<f32>> positions = [new(-1, 1, 1), new(1, 1, 1), new(-1, -1, 1), new(1, -1, 1)];
            ReadOnlySpan<Vector2> tex0Coords = [new(0, 0), new(1, 0), new(0, 1), new(1, 1)];
            ReadOnlySpan<Vector2> tex1Coords =
            [
                new(tex1Scale, tex1Scale), new(1 - tex1Scale, tex1Scale), new(tex1Scale, 1 - tex1Scale),
                new(1 - tex1Scale, 1 - tex1Scale)
            ];
            return new TempleShader.Attributes
            {
                PositionRef = Vector3D.Transform(positions[u], rot).ToSystem(),
                Tex0CoordRef = tex0Coords[u],
                Tex1CoordRef = tex1Coords[u],
                NormalRef = normal,
                TangentRef = tangent
            };
        });
        buffers[^2] = new PBRInstancedMeshBuffer(generateCubeMesh, new TempleShader.ModelUniform
            {
                Transform = mat4.Identity, Tex0Color = new vec3(1, 1, 1), ModelId = (u32)models.Count + 1,
                Tex1Color = new vec3(1, 1, 1), NormalMapStrength = 1
            }, floor
        );
        buffers[^1] = new PBRInstancedMeshBuffer(generateCubeMesh, new TempleShader.ModelUniform
            {
                Transform = mat4.Identity, Tex0Color = new vec3(1, 1, 1), ModelId = (u32)models.Count + 1,
                Tex1Color = new vec3(1, 1, 1), NormalMapStrength = 1
            }, cubes
        );

        buffers[2].InstanceInfos[0].HighlightColor = new vec4(1, 1, 0.5f, 0.5f);
        buffers[1].InstanceInfos[0].HighlightColor = new vec4(0.5f, 1, 1, 0.5f);

        return buffers;
    }

    private OITInstancedMeshBuffer[] CreateTransparentCubes()
    {
        var instances = new WboitAccumShader.Instance[10];
        for (var i = 0; i < 9; i++)
        {
            // make a grid
            var x = i % 3;
            var y = i / 3;
            var position = new Vector3(x * 4, 8, y * 4);
            var rotation = new Vector3(0, 0, 0);
            var scale = new Vector3(2.0f);

            instances[i].SetTransform(Matrix4x4.CreateScale(scale) *
                                      Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z) *
                                      Matrix4x4.CreateTranslation(position));
        }

        instances[0].HighlightColor = new vec4(1, 0, 0, 0.5f);
        instances[1].HighlightColor = new vec4(0, 1, 0, 0.5f);
        instances[2].HighlightColor = new vec4(0, 0, 1, 0.5f);
        instances[3].HighlightColor = new vec4(1, 1, 0, 0.5f);
        instances[4].HighlightColor = new vec4(1, 0, 1, 0.5f);
        instances[5].HighlightColor = new vec4(0, 1, 1, 0.5f);
        instances[6].HighlightColor = new vec4(1, 0.5f, 0, 0.5f);
        instances[7].HighlightColor = new vec4(0.5f, 1, 0, 0.5f);
        instances[8].HighlightColor = new vec4(0, 0.5f, 1, 0.5f);


        // big cube that is like a floor
        instances[^1].SetTransform(Matrix4x4.CreateScale(new Vector3(100, 100f, 100)) *
                                   Matrix4x4.CreateFromYawPitchRoll(0, 0, 0) *
                                   Matrix4x4.CreateTranslation(new Vector3(0, 0, 0)));

        var transparentTexture = _loader.AsyncTextureLoad("resources/Glass/Red.png");

        return
        [
            new OITInstancedMeshBuffer(GenerateCubeMesh((u, rot, _, _) =>
                {
                    ReadOnlySpan<Vector3D<f32>> positions =
                        [new(-1, 1, 1), new(1, 1, 1), new(-1, -1, 1), new(1, -1, 1)];
                    ReadOnlySpan<Vector2> tex0Coords = [new(0, 0), new(1, 0), new(0, 1), new(1, 1)];
                    return new WboitAccumShader.Attributes()
                    {
                        PositionRef = Vector3D.Transform(positions[u], rot).ToSystem(),
                        Tex0CoordRef = tex0Coords[u],
                    };
                }),
                new WboitAccumShader.ModelUniform
                {
                    Transform = mat4.Identity, Tex0Color = new vec3(0), ModelId = 0,
                    Tex1Color = new vec3(1, 1, 1)
                }, instances)
            {
                AlbedoTexture = transparentTexture,
            }
        ];
    }

    public async Task Attach()
    {
        _logger.LogDebug("Creating View Buffers");

        // TextureFormat.Rgba8Unorm, TextureFormat.Rgb10A2Unorm, TextureFormat.Rgba32float, TextureFormat.Depth24Plus,
        _sceneGBuffer = GBuffer.Create(_api.Device, TextureFormat.Rgba8Unorm, TextureFormat.Rgb10A2Unorm,
            TextureFormat.Rgba32float, TextureFormat.RG32Uint,
            TextureFormat.Rgba32float, TextureFormat.R32float,
            TextureFormat.Depth24Plus, label: "SceneGBuffer");

        _finalSceneTexture =
            RenderTexture.Create(_api.Device, TextureFormat.Rgba8Unorm, label: "FinalSceneTexture");

        _logger.LogDebug("Creating Model Buffers");
        _meshBuffers = CreateModelBuffers("resources/Temple/TempleModel.glb", "Temple");
        _transparentMeshBuffers = CreateTransparentCubes();

        _logger.LogDebug("Creating uniform resources");
        {
            using var albedo = _loader.AsyncTextureLoad("resources/Temple/TempleStone_alb.png");
            using var normal = _loader.AsyncTextureLoad("resources/Temple/TempleStone_nrm.png");
            using var emission = _loader.AsyncTextureLoad("resources/Temple/TempleStone_emm.png");
            foreach (var instancedMeshBuffer in _meshBuffers)
            {
                instancedMeshBuffer.AlbedoTexture = albedo.Reference();
                instancedMeshBuffer.NormalTexture = normal.Reference();
                instancedMeshBuffer.EmissionTexture = emission.Reference();
            }

            // for testing
            foreach (var instancedMeshBuffer in _transparentMeshBuffers)
            {
                instancedMeshBuffer.EmissionTexture = emission;
            }
        }

        _sceneLights = new SceneLights();
        _lightsBuffer = _loader.CreateBuffer(BufferUsage.Storage | BufferUsage.CopyDst, _sceneLights.LightData,
            "LightBuffer");


        CreatePipelines();

        foreach (var instancedMeshBuffer in _meshBuffers)
        {
            instancedMeshBuffer.ModelUniformBuffer = _loader.CreateBuffer<TempleShader.ModelUniform>(
                BufferUsage.Uniform | BufferUsage.CopyDst, 1, label: "ModelUniformBuffer");

            _templePipeline.RecreateBindGroup(instancedMeshBuffer);
        }

        foreach (var instancedMeshBuffer in _transparentMeshBuffers)
        {
            instancedMeshBuffer.ModelUniformBuffer = _loader.CreateBuffer<WboitAccumShader.ModelUniform>(
                BufferUsage.Uniform | BufferUsage.CopyDst, 1, label: "ModelUniformBuffer");

            _transparentPipeline.RecreateBindGroup(instancedMeshBuffer);
        }

        _logger.LogDebug("Done");
    }

    [MemberNotNull(nameof(_deferredPipeline), nameof(_compositorPipeline), nameof(_templePipeline),
        nameof(_samplingPipeline), nameof(_transparentPipeline))]
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
        _templePipeline = TemplePipeline.Create(_api, _shaderManager.GetCurrentShader<ITempleShader>(), _sceneGBuffer);
        _samplingPipeline?.Dispose();
        _samplingPipeline =
            PixelSamplingPipeline.Create(_api, _shaderManager.GetCurrentShader<IObjectSamplingShader>());
        _transparentPipeline?.Dispose();
        _transparentPipeline = WboitPipeline.Create(_api, _loader, _shaderManager.GetCurrentShader<IWboitAccumShader>(),
            _sceneGBuffer!);
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
        var queue = target.QueuePtr;
        _camera.AspectRatio = sceneViewport.Width / (f32)sceneViewport.Height;
        _sceneUniform = new TempleShader.SceneUniform
        {
            ViewProjectionRef = _camera.ViewProjectionMatrix,
            CameraPositionRef = _camera.Position,
            Time = info.TotalTime
        };
        //Debug.Assert(Unsafe.SizeOf<WboitAccumShader.SceneUniform>() == Unsafe.SizeOf<TempleShader.SceneUniform>(), "Size of SceneUniform is not equal");
        Debug.Assert(
            Marshal.OffsetOf<WboitAccumShader.SceneUniform>(nameof(WboitAccumShader.SceneUniform.ViewProjection)) ==
            Marshal.OffsetOf<TempleShader.SceneUniform>(nameof(TempleShader.SceneUniform.ViewProjection)),
            "Offset of ViewProjection is not equal");
        Debug.Assert(
            Marshal.OffsetOf<WboitAccumShader.SceneUniform>(nameof(WboitAccumShader.SceneUniform.CameraPosition)) ==
            Marshal.OffsetOf<TempleShader.SceneUniform>(nameof(TempleShader.SceneUniform.CameraPosition)),
            "Offset of CameraPosition is not equal");

        _templePipeline!.WriteUniformBuffer(queue, ref _sceneUniform);
        _transparentPipeline!.WriteUniformBuffer(queue,
            ref Unsafe.As<TempleShader.SceneUniform, WboitAccumShader.SceneUniform>(ref _sceneUniform));

        var players = _ecs.GetPlayers().ToArray();
        if (_meshBuffers[^1].InstanceInfos.Length != players.Length + 1)
            _meshBuffers[^1].InstanceInfos = new TempleShader.Instance[players.Length + 1];

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

        foreach (var buffer in _transparentMeshBuffers)
        {
            WriteBuffer(queue, buffer);
            _transparentPipeline.RecreateBindGroup(buffer);
        }

        _lightsBuffer.QueuedWrite(_sceneLights.LightData, queue);

        if (_templePipeline.BeginRenderPass() is { } pass)
        {
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

        if (_transparentPipeline.BeginRenderPass(sceneViewport) is { } transparentPass)
        {
            foreach (var (vertexBuffer, indexBuffer, instanceBuffer, modelBindGroup, instances) in
                     _transparentMeshBuffers)
            {
                transparentPass.SetVertexBuffer(0, vertexBuffer.Buffer, vertexBuffer.Offset, vertexBuffer.Size);
                transparentPass.SetVertexBuffer(1, instanceBuffer!.Buffer, instanceBuffer.Offset,
                    instanceBuffer.Size);

                transparentPass.SetIndexBuffer(indexBuffer.Buffer, IndexFormat.Uint32, indexBuffer.Offset,
                    indexBuffer.Size);

                transparentPass.SetBindGroup(1, modelBindGroup!.Value, ReadOnlySpan<u32>.Empty);

                transparentPass.DrawIndexed((u32)(indexBuffer.Size / sizeof(u32)), (u32)instances.Length, 0, 0, 0);
            }

            queue.Submit(_transparentPipeline.EndRenderPass(transparentPass));
        }
    }

    private void WriteBuffer<T1, T2, T3>(QueuePtr queue,
        IInstancedMeshBuffer<T1, T2, T3> buffer) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged
    {
        buffer.ModelUniformBuffer.QueuedWrite(new ReadOnlySpan<T3>(ref buffer.ModelUniform),
            queue);

        var data = new ReadOnlySpan<T2>(buffer.InstanceInfos);
        if (buffer.InstanceBuffer is null || buffer.InstanceBuffer.Size <
            (u64)data.Length * (u64)Unsafe.SizeOf<T2>())
        {
            buffer.InstanceBuffer?.Dispose();
            buffer.InstanceBuffer = _loader.CreateBuffer(BufferUsage.Vertex | BufferUsage.CopyDst,
                data, "DynamicRecreated::IndexBuffer");
        }

        buffer.InstanceBuffer.QueuedWrite(data, queue);
    }

    public void Render(UpdateInfo info, RenderTarget target)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        if (!ImGui.Begin("Scene")) return;
        if (_sceneGBuffer is null || _finalSceneTexture is null)
        {
            _logger.LogError("Scene Buffers not created");
            return;
        }

        var size = ImGui.GetContentRegionAvail();

        //var imDrawListPtr = ImGui.GetBackgroundDrawList();
        //var size = imDrawListPtr.GetClipRectMax();
        u32 width = (u32)size.X, height = (u32)Math.Max(0, size.Y);

        var prevWidth = _finalSceneTexture.Width;
        var prevHeight = _finalSceneTexture.Height;
        var hasPrev = _finalSceneTexture.TryGetTargets(out var prevView);
        _sceneGBuffer.EnsureSize(width, height);
        _finalSceneTexture.EnsureSize(width, height);
        CreatePipelines();

        if (_sceneGBuffer.TryGetTargets(out var albedo, out var normal, out var emission,
                out var objectAndInstanceId, out var accum, out var reveal,
                out var depth) && _finalSceneTexture.TryGetTargets(out var colorOutput))
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

            _compositorPipeline.Apply(queue, emission.View, depth.DepthOnlyView, colorOutput.View, accum.View,
                reveal.View);

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
            if(ImGui.Button("Collect Resources"))
            {
                _loader.DoCollection();
            }
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
                var selectedBuffer = _meshBuffers[_selectedObjectId - 1];

                ImGui.Columns(2, null, false);
                var y = ImGui.GetCursorPosY();
                ImGui.SetCursorPosY(y + ImGui.GetStyle().FramePadding.Y);
                ImGui.Text("Texture");
                ImGui.NextColumn();
                ImGui.SetCursorPosY(y);
                var imageButtonSize = ImGui.GetContentRegionAvail().X;
                if (ImGui.ImageButton("ChooseTexture", selectedBuffer.AlbedoTexture.View.GetIntPtr(), new Vector2(imageButtonSize)))
                {
                    Task.Run(() =>
                    {
                        var pickResult = Nfd.FileOpen([
                            new NfdFilter { Description = "Image files", Specification = "png,jpg,jpeg,bmp" }
                        ]);

                        if (pickResult.Status != NfdStatus.Ok) return;

                        var clone = selectedBuffer.AlbedoTexture.Clone();
                        _loader.AsyncTextureLoad(pickResult.Path, clone);
                        selectedBuffer.AlbedoTexture.Dispose();
                        selectedBuffer.AlbedoTexture = clone;
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
            buffer.Dispose();
        }

        foreach (var buffer in _transparentMeshBuffers)
        {
            buffer.Dispose();
        }

        _compositorPipeline?.Dispose();
        _deferredPipeline?.Dispose();
        _templePipeline?.Dispose();

        _lightsBuffer.Buffer.Destroy();

        // set all null
        _sceneGBuffer = null;
        _finalSceneTexture = null;

        _meshBuffers = [];
        _transparentMeshBuffers = [];
        _compositorPipeline = null;
        _deferredPipeline = null;
        _lightsBuffer = null!;

        return Task.CompletedTask;
    }
}
