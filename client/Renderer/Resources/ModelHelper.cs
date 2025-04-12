using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Silk.NET.Assimp;

namespace Client.Renderer.Resources;

public unsafe class ModelHelper(ILogger<ModelHelper> logger)
{
    public class Model
    {
        public required u32[] Indices;
        public required Vector3[] Positions;
        public required Vector3[] Normals;
        public required Vector3[] Tangents;
        public required Vector2[] TexCoords0;
        public required Vector2[] TexCoords1;
        public required i32 MaterialIndex;
    }

    public class Material
    {
        public string? Name;
        public Vector3 AmbientReflectivity;
        public Vector3 DiffuseReflectivity;
        public Vector3 SpecularReflectivity;
        public Vector3 EmissiveCoefficient;
        public Vector3 TransmissionFilter;
        public i32 IlluminationModel;
        public f32 Opacity;
        public f32 Transparency => 1 - Opacity;
        public f32 SpecularExponent;
        public f32 Sharpness;
        public f32 OpticalDensity;
        public string? AmbientTexture;
        public string? DiffuseTexture;
        public string? SpecularTexture;
        public string? EmissiveTexture;
        public string? NormalTexture;
        public string? DisplacementMapTexture;
        public string? BumpMapTexture;
        public string? StencilTexture;
    }

    public (IReadOnlyList<Model> models, IReadOnlyList<Material> materials) LoadModels(string file)
    {
        var assimp = Assimp.GetApi();

        if (!System.IO.File.Exists(file))
            throw new FileNotFoundException(file);

        var scene = assimp.ImportFile(file, 0);

        if (scene == null)
            throw new Exception($"model could not be loaded");

        var result = LoadModels(scene);

        assimp.FreeScene(scene);
        return result;
    }

    public (IReadOnlyList<Model> models, IReadOnlyList<Material> materials) LoadModels(u8[] data,
        string? formatHint)
    {
        var assimp = Assimp.GetApi();

        var scene = assimp.ImportFileFromMemory(data, (u32)data.Length, 0, formatHint);

        if (scene == null)
            throw new Exception($"model could not be loaded");

        var result = LoadModels(scene);

        assimp.FreeScene(scene);
        return result;
    }

    private (IReadOnlyList<Model> models, IReadOnlyList<Material> materials) LoadModels(Scene* scene)
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
                    var next = mesh->MTextureCoords.Element1;
                    if (next != null)
                    {
                        var sourceSpan =
                            new ReadOnlySpan<Vector3>(mesh->MTextureCoords.Element1, (i32)mesh->MNumVertices);
                        var destSpan = texCoords1;
                        for (i32 i = 0; i < sourceSpan.Length; i++)
                            destSpan[i] = new(sourceSpan[i].X, sourceSpan[i].Y);
                    }
                    else
                    {
                        texCoords0.CopyTo(texCoords1.AsSpan());
                    }
                }

                new ReadOnlySpan<Vector3>(mesh->MNormals, (i32)mesh->MNumVertices)
                    .CopyTo(normals);
                if (mesh->MTangents == null)
                {
                    for (i32 i = 0; i < mesh->MNumVertices; i++)
                        tangents[i] = Vector3.UnitX;
                }
                else
                {
                    new ReadOnlySpan<Vector3>(mesh->MTangents, (i32)mesh->MNumVertices)
                        .CopyTo(tangents);
                }

                models.Add(new Model
                {
                    Indices = indexList.ToArray(),
                    Positions = positions,
                    Normals = normals,
                    Tangents = tangents,
                    TexCoords0 = texCoords0,
                    TexCoords1 = texCoords1,
                    MaterialIndex = (i32)mesh->MMaterialIndex,
                });
            }

            for (i32 iChild = 0; iChild < node->MNumChildren; iChild++)
            {
                var child = node->MChildren[iChild];
                jobs.Push(((nuint)child, Matrix4x4.Transpose(child->MTransformation) * transform));
            }
        } while (jobs.Count > 0);

        List<Material> materials = new List<Material>();

        for (int i = 0; i < scene->MNumMaterials; i++)
        {
            var material = scene->MMaterials[i];
            var result = ParseMaterial(material);
            materials.Add(result);
        }


        return (models, materials);
    }

    private Material ParseMaterial(Silk.NET.Assimp.Material* material)
    {
        var result = new Material();
        string? lastFile = null;
        for (int j = 0; j < material->MNumProperties; j++)
        {
            var property = material->MProperties[j];
            var span = new ReadOnlySpan<u8>(property->MData, (i32)property->MDataLength);

            var str = property->MKey.AsString;
            if ("?mat.name" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.String);
                result.Name = ParseString(span);
            }
            else if ("$mat.shadingm" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.Integer);
                result.IlluminationModel = ParseI32(span);
            }
            else if ("$mat.illum" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.Integer);
                result.IlluminationModel = ParseI32(span);
            }
            else if ("$clr.ambient" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.Float);
                result.AmbientReflectivity = ParseVector3F32(span);
            }
            else if ("$clr.diffuse" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.Float);
                result.DiffuseReflectivity = ParseVector3F32(span);
            }
            else if ("$clr.specular" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.Float);
                result.SpecularReflectivity = ParseVector3F32(span);
            }
            else if ("$clr.emissive" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.Float);
                result.EmissiveCoefficient = ParseVector3F32(span);
            }
            else if ("$mat.shininess" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.Float);
                result.SpecularExponent = ParseF32(span);
            }
            else if ("$mat.opacity" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.Float);
                result.Opacity = ParseF32(span);
            }
            else if ("$clr.transparent" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.Float);
                result.Opacity = 1 - ParseF32(span);
            }
            else if ("$mat.anisotropyFactor" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.Float);
                result.Opacity = ParseF32(span);
            }
            else if ("$mat.refracti" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.Float);
                result.OpticalDensity = ParseF32(span);
            }
            else if ("$tex.file" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.String);
                lastFile = ParseString(span);
            }
            else if ("$tex.uvwsrc" == str)
            {
                switch (property->MSemantic)
                {
                    case 1:
                        result.AmbientTexture = lastFile;
                        break;
                    case 3:
                        result.DiffuseTexture = lastFile;
                        break;
                    case 5:
                        result.DisplacementMapTexture = lastFile;
                        break;
                    case 8:
                        result.BumpMapTexture = lastFile;
                        break;
                    default:
                        logger.LogWarning("Unknown texture semantic: {MSemantic}", property->MSemantic);
                        break;
                }
            }
            else

                logger.LogWarning(
                    "Unknown property: {str}\n   - lastFile: {lastFile}\n   - MSemantic: {MSemantic}\n   - MIndex: {MIndex}\n   - MType: {MType}\n   - MDataLength: {MDataLength}\n   - MData: [{Data}]",
                    str, lastFile ?? "", property->MSemantic, property->MIndex, property->MType, property->MDataLength,
                    string.Join(", ", span.ToArray()));
        }

        return result;

        static void AssertType(PropertyTypeInfo type, PropertyTypeInfo expected)
        {
            if (type != expected)
                throw new Exception($"Invalid property type: {type} != {expected}");
        }

        static string ParseString(ReadOnlySpan<u8> span)
        {
            // get length as int from start
            var length = BitConverter.ToInt32(span[..4]);
            // get string from span
            return Encoding.UTF8.GetString(span[4..(4 + length)]);
        }

        static i32 ParseI32(ReadOnlySpan<u8> span) => BitConverter.ToInt32(span[..4]);
        static f32 ParseF32(ReadOnlySpan<u8> span) => BitConverter.ToSingle(span[..4]);
        static f64 ParseF64(ReadOnlySpan<u8> span) => BitConverter.ToDouble(span[..8]);

        static Vector3 ParseVector3F32(ReadOnlySpan<u8> span)
        {
            var x = BitConverter.ToSingle(span[..4]);
            var y = BitConverter.ToSingle(span[4..8]);
            var z = BitConverter.ToSingle(span[8..12]);
            return new Vector3(x, y, z);
        }
    }
}
