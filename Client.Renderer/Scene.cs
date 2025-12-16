using System.Numerics;
using Client.Renderer.Instancing;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Wgsl;
using GlmSharp;
using Silk.NET.WebGPU;

namespace Client.Renderer;

public class Scene(ResourceManager loader, ModelHelper modelHelper)
{
    private readonly ResourceManager _loader = loader;
    private readonly ModelHelper _modelHelper = modelHelper;

    public RenderableInstanceCollection RenderableInstances { get; set; } = new();
    public u64 Version { get; private set; }

    public void LoadLevel(string levelName)
    {
        // For now, we'll just load the Sponza model regardless of the level name.
        // In the future, we can use a dictionary or other mechanism to map level names to model paths.
        var oldCollection = RenderableInstances;
        var newCollection = new RenderableInstanceCollection();
        newCollection.OpaqueMeshes.AddRange(CreateModelBuffers("resources/Sponza/Meshes/Sponza_Modular.FBX",
            "Sponza_Modular"));
        newCollection.TransparentMeshes.AddRange(CreateTransparentCubes());
        RenderableInstances = newCollection;
        oldCollection.Dispose();
        Version++;
    }

    public void UnloadLevel()
    {
        RenderableInstances.Dispose();
        Version++;
    }

    public PbrInstancedMeshBuffer[] CreateModelBuffers(string path, string? label = null)
    {
        var (meshDatas, data, root, materials) = _modelHelper.LoadModels(path);

        var buffers = new PbrInstancedMeshBuffer[meshDatas.Count];

        for (var iModel = 0; iModel < meshDatas.Count; iModel++)
        {
            var mdl = meshDatas[iModel];
            var mat = materials.Count > mdl.MaterialIndex ? materials[mdl.MaterialIndex] : null;

            var mesh = _loader.AsyncMeshLoad(path, mdl, label);
            var pbrInstancedMeshBuffer = new PbrInstancedMeshBuffer(mesh,
                ModelHelper.MaterialConverter.ConvertToPbrUniform(mat, (u32)iModel + 1, (u32)mdl.MaterialIndex, false),
                _loader.CreateBuffer<PbrGBufferShader.PbrMaterialUniform>(BufferUsage.Uniform | BufferUsage.CopyDst, 1,
                    label: $"{label}::ModelUniformBuffer")
            );
            if (mat is { DiffuseTexture: { } diffuseTexturePath })
                pbrInstancedMeshBuffer.BaseColorTexture = _loader.AsyncTextureLoad(diffuseTexturePath);
            pbrInstancedMeshBuffer.NormalTexture = mat is { NormalTexture: { } normalTexturePath }
                ? _loader.AsyncTextureLoad(normalTexturePath)
                : _loader.DefaultNormal;
            pbrInstancedMeshBuffer.EmissiveTexture = mat is { EmissiveTexture: { } emissiveTexturePath }
                ? _loader.AsyncTextureLoad(emissiveTexturePath)
                : _loader.DefaultEmissive;

            if (mat is { EmissiveCoefficient: { X: > 0, Y: > 0, Z: > 0 } emissiveColor })
                pbrInstancedMeshBuffer.MaterialUniform.EmissiveFactorRef = emissiveColor;

            buffers[iModel] = pbrInstancedMeshBuffer;
        }

        /*PbrGBufferShader.Instance[] cubes = [new()];
        PbrGBufferShader.Instance[] floor = [new()];
        floor[0].SetTransform(Matrix4x4.CreateScale(new Vector3(100, 0.1f, 100)) *
                              Matrix4x4.CreateFromYawPitchRoll(0, 0, 0) *
                              Matrix4x4.CreateTranslation(new Vector3(0, -2, 0)));
        cubes[0].SetTransform(Matrix4x4.Identity);
        buffers[^2] =
            new PbrInstancedMeshBuffer(_loader.DefCube, ModelHelper.PbrMaterialExamples.PolishedChrome(), floor);
        buffers[^1] =
            new PbrInstancedMeshBuffer(generateCubeMesh, ModelHelper.PbrMaterialExamples.WornLeather(), cubes);*/

        // populate instances
        var jobs = new Stack<ModelHelper.SceneNode>();
        jobs.Push(root);

        while (jobs.TryPop(out var node))
        {
            foreach (var meshId in node.Meshes ?? Enumerable.Empty<i32>())
            {
                if (meshId < meshDatas.Count)
                {
                    var instance = new PbrGBufferShader.Instance();
                    instance.SetTransform(node.GlobalTransform * Matrix4x4.CreateScale(0.01f));
                    buffers[meshId]._instanceInfos.Add(instance);
                }
            }

            foreach (var child in node.Children ?? Enumerable.Empty<ModelHelper.SceneNode>())
                jobs.Push(child);
        }

        return buffers;
    }

    public OitInstancedMeshBuffer[] CreateTransparentCubes()
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
            new OitInstancedMeshBuffer(_loader.DefaultTransparentCubeMesh,
                new WboitAccumShader.ModelUniform
                {
                    Transform = mat4.Identity, Tex0Color = new vec3(0), ModelId = 0,
                    Tex1Color = new vec3(1, 1, 1)
                },
                _loader.CreateBuffer<WboitAccumShader.ModelUniform>(BufferUsage.Uniform | BufferUsage.CopyDst, 1,
                    label: $"CreateTransparentCubes::ModelUniformBuffer"), instances)
            {
                AlbedoTexture = transparentTexture,
            }
        ];
    }
}
