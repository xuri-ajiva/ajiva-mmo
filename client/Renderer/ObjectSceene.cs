using System.Numerics;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Wgsl;
using GlmSharp;
using Silk.NET.Maths;
using Silk.NET.WebGPU;

namespace Client.Renderer;

public class ObjectScene(ResourceLoader loader, ModelHelper modelHelper)
{
    private readonly ResourceLoader _loader = loader;
    private readonly ModelHelper _modelHelper = modelHelper;

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

    public PbrInstancedMeshBuffer[] CreateModelBuffers(string path, string? label = null)
    {
        var (meshDatas, data, root, materials) = _modelHelper.LoadModels(path);

        var buffers = new PbrInstancedMeshBuffer[meshDatas.Count + 2];

        for (var iModel = 0; iModel < meshDatas.Count; iModel++)
        {
            var mdl = meshDatas[iModel];
            var mat = materials.Count > mdl.MaterialIndex ? materials[mdl.MaterialIndex] : null;

            var modelVertices = new PbrGBufferShader.Attributes[mdl.Vertices.Length];

            for (var iVertex = 0u; iVertex < mdl.Vertices.Length; iVertex++)
            {
                modelVertices[iVertex].PositionRef = mdl.Vertices[iVertex];
                modelVertices[iVertex].Tex0CoordRef = mdl.TexCoords0[iVertex];
                modelVertices[iVertex].Tex1CoordRef = mdl.TexCoords1[iVertex];
                modelVertices[iVertex].NormalRef = mdl.Normals[iVertex];
                modelVertices[iVertex].TangentRef = new Vector4(mdl.Tangents[iVertex], 1.0f);
            }

            var vertexBuffer = _loader.CreateBuffer<PbrGBufferShader.Attributes>(
                BufferUsage.Vertex | BufferUsage.CopyDst,
                modelVertices,
                label == null ? null : $"{label}::VertexBuffer");

            var indexBuffer = _loader.CreateBuffer<u32>(BufferUsage.Index | BufferUsage.CopyDst, mdl.Indices,
                label == null ? null : $"{label}::IndexBuffer");

            var pbrInstancedMeshBuffer = new PbrInstancedMeshBuffer(
                new AjivaMesh<PbrGBufferShader.Attributes>(vertexBuffer, indexBuffer),
                ModelHelper.MaterialConverter.ConvertToPbrUniform(mat, (u32)iModel + 1, (u32)mdl.MaterialIndex, false)
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

        PbrGBufferShader.Instance[] cubes = [new()];
        PbrGBufferShader.Instance[] floor = [new()];
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
            return new PbrGBufferShader.Attributes
            {
                PositionRef = Vector3D.Transform(positions[u], rot).ToSystem(),
                Tex0CoordRef = tex0Coords[u],
                Tex1CoordRef = tex1Coords[u],
                NormalRef = normal,
                TangentRef = new Vector4(tangent, 1)
            };
        });
        buffers[^2] =
            new PbrInstancedMeshBuffer(generateCubeMesh, ModelHelper.PbrMaterialExamples.PolishedChrome(), floor);
        buffers[^1] =
            new PbrInstancedMeshBuffer(generateCubeMesh, ModelHelper.PbrMaterialExamples.WornLeather(), cubes);

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
            new OitInstancedMeshBuffer(GenerateCubeMesh((u, rot, _, _) =>
                {
                    ReadOnlySpan<Vector3D<f32>> positions =
                        [new(-1, 1, 1), new(1, 1, 1), new(-1, -1, 1), new(1, -1, 1)];
                    ReadOnlySpan<Vector2> tex0Coords = [new(0, 0), new(1, 0), new(0, 1), new(1, 1)];
                    return new WboitAccumShader.Attributes
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
}
