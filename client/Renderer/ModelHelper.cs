using System.Numerics;
using Silk.NET.Assimp;

namespace client.Renderer;

internal static unsafe class ModelHelper
{
    public class Model
    {
        public u32[] Indices;
        public Vector3[] Positions;
        public Vector3[] Normals;
        public Vector3[] Tangents;
        public Vector2[] TexCoords0;
        public Vector2[] TexCoords1;
    }

    public static IReadOnlyList<Model> LoadModels(string file)
    {
        var assimp = Assimp.GetApi();

        if(!System.IO.File.Exists(file))
            throw new FileNotFoundException(file);

        Scene* scene = assimp.ImportFile(file, 0);

        if(scene == null)
            throw new Exception($"model could not be loaded");

        var models = LoadModels(scene);

        assimp.FreeScene(scene);
        return models;
    }

    public static IReadOnlyList<Model> LoadModels(u8[] data, string? formatHint)
    {
        var assimp = Assimp.GetApi();

        var scene = assimp.ImportFileFromMemory(data, (u32)data.Length, 0, formatHint);

        if (scene == null)
            throw new Exception($"model could not be loaded");

        var models = LoadModels(scene);

        assimp.FreeScene(scene);
        return models;
    }

    private static IReadOnlyList<Model> LoadModels(Scene* scene)
    {
        List<Model> models = new List<Model>();

        var jobs = new Stack<(nuint node, Matrix4x4 tranform)>();

        jobs.Push(((nuint)scene->MRootNode, scene->MRootNode->MTransformation));

        do
        {
            var (nodePtr, transform) = jobs.Pop();
            var node = (Node*)nodePtr;

            for (i32 iMesh = 0; iMesh < node->MNumMeshes; iMesh++)
            {
                var mesh = scene->MMeshes[node->MMeshes[iMesh]];

                var indexList = new List<u32>();
                var positions = new Vector3[mesh->MNumVertices];
                var normals = new Vector3[mesh->MNumVertices];
                var tangents = new Vector3[mesh->MNumVertices];
                var texCoords0 = new Vector2[mesh->MNumVertices];
                var texCoords1 = new Vector2[mesh->MNumVertices];

                for (i32 iFace = 0; iFace < mesh->MNumFaces; iFace++)
                {
                    var face = mesh->MFaces[iFace];

                    u32 first = face.MIndices[0];
                    u32 previous = face.MIndices[1];

                    for (i32 i = 2; i < face.MNumIndices; i++)
                    {
                        indexList.Add(first);
                        indexList.Add(previous);
                        indexList.Add((previous = face.MIndices[i]));
                    }
                }

                {
                    var sourceSpan = new ReadOnlySpan<Vector3>(mesh->MVertices, (i32)mesh->MNumVertices);
                    var destSpan = positions;
                    for (i32 i = 0; i < sourceSpan.Length; i++)
                        destSpan[i] = Vector3.Transform(sourceSpan[i], transform);
                }

                {
                    var sourceSpan = new ReadOnlySpan<Vector3>(mesh->MTextureCoords.Element0, (i32)mesh->MNumVertices);
                    var destSpan = texCoords0;
                    for (i32 i = 0; i < sourceSpan.Length; i++)
                        destSpan[i] = new(sourceSpan[i].X, sourceSpan[i].Y);
                }

                {
                    var sourceSpan = new ReadOnlySpan<Vector3>(mesh->MTextureCoords.Element1, (i32)mesh->MNumVertices);
                    var destSpan = texCoords1;
                    for (i32 i = 0; i < sourceSpan.Length; i++)
                        destSpan[i] = new(sourceSpan[i].X, sourceSpan[i].Y);
                }

                new ReadOnlySpan<Vector3>(mesh->MNormals, (i32)mesh->MNumVertices)
                    .CopyTo(normals);
                new ReadOnlySpan<Vector3>(mesh->MTangents, (i32)mesh->MNumVertices)
                    .CopyTo(tangents);
                models.Add(new Model
                {
                    Indices = indexList.ToArray(),
                    Positions = positions,
                    Normals = normals,
                    Tangents = tangents,
                    TexCoords0 = texCoords0,
                    TexCoords1 = texCoords1
                });
            }

            for (i32 iChild = 0; iChild < node->MNumChildren; iChild++)
            {
                var child = node->MChildren[iChild];
                jobs.Push(((nuint)child, Matrix4x4.Transpose(child->MTransformation) * transform));
            }
        }
        while (jobs.Count > 0);

        return models;
    }
}
