using System.Diagnostics;
using System.Numerics;
using System.Text;
using Client.Renderer.PbrRenderer;
using Client.Wgsl;
using GlmSharp;
using Silk.NET.Assimp;

namespace Client.Renderer.Resources;

internal static class ReadOnlyRangeExtensions
{
    public static ReadWriteRange<T> AsRoR<T>(this T[] @this, u64 start, u64 count) =>
        new(@this, start, count);
}

public readonly struct ReadWriteRange<T>(T[] backing, u64 start, u64 count)
{
    private readonly T[] _backing = backing;
    private readonly u64 _start = start;
    private readonly u64 _count = count;

    public static implicit operator ReadOnlySpan<T>(ReadWriteRange<T> range) =>
        new(range._backing, (i32)range._start, (i32)range._count);

    public static implicit operator Span<T>(ReadWriteRange<T> range) =>
        new(range._backing, (i32)range._start, (i32)range._count);

    public static implicit operator ReadOnlyMemory<T>(ReadWriteRange<T> range) =>
        new(range._backing, (i32)range._start, (i32)range._count);

    public Span<T> RwSpan => new(_backing, (i32)_start, (i32)_count);
    public ReadOnlySpan<T> Span => new(_backing, (i32)_start, (i32)_count);

    public ref T this[u64 index] => ref _backing[_start + index];
    public u64 Length => _count;
}

public unsafe class ModelHelper(ILogger<ModelHelper> logger)
{
    public class MeshData
    {
        public ReadWriteRange<u32> Indices;
        public ReadWriteRange<Vector3> Vertices;
        public ReadWriteRange<Vector3> Normals;
        public ReadWriteRange<Vector3> Tangents;
        public ReadWriteRange<Vector2> TexCoords0;
        public ReadWriteRange<Vector2> TexCoords1;
        public required i32 MaterialIndex;
    }

    public class SceneNode
    {
        public required string Name;
        public required Matrix4x4 NodeTransform;
        public required Matrix4x4 GlobalTransform;
        public List<SceneNode>? Children;
        public List<i32>? Meshes;
    }

    public record PackedMeshes(
        u32[] Indices,
        Vector3[] Vertices,
        Vector3[] Normals,
        Vector3[] Tangents,
        Vector2[] TexCoords0,
        Vector2[] TexCoords1)
    {
        public PackedMeshes(u64 totalIndices, u64 totalVertices) : this(new u32[totalIndices],
            new Vector3[totalVertices], new Vector3[totalVertices], new Vector3[totalVertices],
            new Vector2[totalVertices], new Vector2[totalVertices])
        {
        }

        private u32 _refCount = 0;
        public void IncrementRefCount() => Interlocked.Increment(ref _refCount);
        public void DecrementRefCount() => Interlocked.Decrement(ref _refCount);
    }

    public record class ResolvedScene(
        IReadOnlyList<MeshData> Models,
        PackedMeshes Data,
        SceneNode Root,
        IReadOnlyList<Material> Materials);

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


    private ResolvedScene LoadModels(Scene* scene)
    {
        u64 gIndices = 0;
        u64 gVertices = 0;

        // prepass
        for (var i = 0; i < scene->MNumMeshes; i++)
        {
            var mesh = scene->MMeshes[i];
            if (mesh == null)
                continue;

            u64 indices = 0;
            for (var iFace = 0; iFace < mesh->MNumFaces; iFace++)
            {
                //indices += mesh->MFaces[iFace].MNumIndices;
                indices += 3 * (mesh->MFaces[iFace].MNumIndices - 2);
            }

            gIndices += indices;
            gVertices += mesh->MNumVertices;
            logger.LogInformation("Mesh {i}: {Vertices} vertices, {Indices} indices", i,
                mesh->MNumVertices, indices);
        }

        // allocate
        var meshes = new List<MeshData>();
        var data = new PackedMeshes(gIndices, gVertices);

        // copy pass
        gIndices = 0;
        gVertices = 0;
        for (var i = 0; i < scene->MNumMeshes; i++)
        {
            var mesh = scene->MMeshes[i];
            if (mesh == null)
                continue;

            var iVertices = mesh->MNumVertices;

            var verticesRef = data.Vertices.AsRoR(gVertices, iVertices);
            var normalsRef = data.Normals.AsRoR(gVertices, iVertices);
            var tangentsRef = data.Tangents.AsRoR(gVertices, iVertices);
            var texCoords0Ref = data.TexCoords0.AsRoR(gVertices, iVertices);
            var texCoords1Ref = data.TexCoords1.AsRoR(gVertices, iVertices);

            new Span<Vector3>(mesh->MVertices, (i32)mesh->MNumVertices).CopyTo(verticesRef);
            new Span<Vector3>(mesh->MNormals, (i32)mesh->MNumVertices).CopyTo(normalsRef);
            if (mesh->MTangents != null)
            {
                new Span<Vector3>(mesh->MTangents, (i32)mesh->MNumVertices).CopyTo(tangentsRef);
            }
            else
            {
                tangentsRef.RwSpan.Fill(Vector3.UnitX);
            }

            for (var j = 0u; j < mesh->MNumVertices; j++)
            {
                var src = mesh->MTextureCoords.Element0[j];
                texCoords0Ref[j] = new Vector2(src.X, src.Y);
            }

            if (mesh->MTextureCoords.Element1 != null)
            {
                for (var j = 0u; j < mesh->MNumVertices; j++)
                {
                    var src = mesh->MTextureCoords.Element1[j];
                    texCoords1Ref[j] = new Vector2(src.X, src.Y);
                }
            }
            else
            {
                texCoords0Ref.Span.CopyTo(texCoords1Ref);
            }

            var start = gIndices;
            for (var iFace = 0; iFace < mesh->MNumFaces; iFace++)
            {
                var face = mesh->MFaces[iFace];
                var first = face.MIndices[0];
                var previous = face.MIndices[1];

                for (var j = 2; j < face.MNumIndices; j++)
                {
                    data.Indices[gIndices++] = first;
                    data.Indices[gIndices++] = previous;
                    data.Indices[gIndices++] = previous = face.MIndices[j];
                }
            }

            gVertices += iVertices;

            meshes.Add(new MeshData
            {
                MaterialIndex = (i32)mesh->MMaterialIndex,
                Indices = data.Indices.AsRoR(start, gIndices - start),
                Vertices = verticesRef,
                Normals = normalsRef,
                Tangents = tangentsRef,
                TexCoords0 = texCoords0Ref,
                TexCoords1 = texCoords1Ref,
            });
        }

        Debug.Assert(gIndices == (u64)data.Indices.LongLength, "gIndices != data.Indices.Length");
        Debug.Assert(gVertices == (u64)data.Vertices.LongLength, "gVertices != data.Vertices.Length");

        var totalNodes = 0;
        var root = Convert(scene->MRootNode, Matrix4x4.Identity);

        var materials = new List<Material>();

        for (var i = 0; i < scene->MNumMaterials; i++)
        {
            var material = scene->MMaterials[i];
            var result = ParseMaterial(material);
            materials.Add(result);
        }

        return new ResolvedScene(meshes, data, root, materials);

        SceneNode Convert(Node* node, Matrix4x4 transform)
        {
            totalNodes++;
            var result = new SceneNode
            {
                Name = node->MName,
                GlobalTransform = node->MTransformation * transform,
                NodeTransform = node->MTransformation,
            };
            if (node->MNumChildren > 0)
            {
                result.Children = new List<SceneNode>((i32)node->MNumChildren);
                for (var iChild = 0; iChild < node->MNumChildren; iChild++)
                {
                    var child = node->MChildren[iChild];
                    result.Children.Add(Convert(child, Matrix4x4.Transpose(child->MTransformation) * transform));
                }
            }

            if (node->MNumMeshes <= 0) return result;

            result.Meshes = new List<i32>((i32)node->MNumMeshes);
            for (var iMesh = 0; iMesh < node->MNumMeshes; iMesh++)
            {
                var meshIndex = node->MMeshes[iMesh];
                if (meshIndex < meshes.Count)
                {
                    result.Meshes.Add((i32)meshIndex);
                }
                else
                {
                    logger.LogWarning("Mesh index out of bounds: {meshIndex}", meshIndex);
                }
            }

            return result;
        }
    }

    private static u64 ComputeKey(Mesh* mesh)
    {
        HashCode code = new();
        code.Add(mesh->MMaterialIndex);
        code.Add(mesh->MNumVertices);
        code.Add(mesh->MNumFaces);

        // add first 10 vertices and faces
        var h1 = code.ToHashCode();
        //return (u64)h1;
        code = new HashCode();
        for (var i = 0; i < Math.Min(mesh->MNumVertices, 10); i++)
        {
            code.Add(mesh->MVertices[i]);
            code.Add(mesh->MNormals[i]);
            if (mesh->MTangents != null)
                code.Add(mesh->MTangents[i]);
            code.Add(mesh->MTextureCoords.Element0[i]);
            if (mesh->MTextureCoords.Element1 != null)
                code.Add(mesh->MTextureCoords.Element1[i]);
        }

        for (var i = 0; i < Math.Min(mesh->MNumFaces, 10); i++)
        {
            code.Add(mesh->MFaces[i].MNumIndices);
            for (var j = 0; j < mesh->MFaces[i].MNumIndices; j++)
            {
                code.Add(mesh->MFaces[i].MIndices[j]);
            }
        }

        return ((u64)code.ToHashCode() << 32) + (u64)h1;
    }

    private Material ParseMaterial(Silk.NET.Assimp.Material* material)
    {
        var result = new Material();
        string? lastFile = null;
        for (var j = 0; j < material->MNumProperties; j++)
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
                if (lastFile.StartsWith(@"..\..\..\Unity\Projects\SponzaHDRP\Assets\Art\"))
                {
                    lastFile = lastFile[@"..\..\..\Unity\Projects\SponzaHDRP\Assets\Art\".Length..];
                }
            }
            else if ("$raw.DiffuseColor|file" == str)
            {
                AssertType(property->MType, PropertyTypeInfo.String);
                result.DiffuseTexture = ParseString(span);
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
            {
                logger.LogWarning(
                    "Unknown property: {str}\n   - lastFile: {lastFile}\n   - MSemantic: {MSemantic}\n   - MIndex: {MIndex}\n   - MType: {MType}\n   - MDataLength: {MDataLength}\n   - MData: [{Data}]",
                    str, lastFile ?? "", property->MSemantic, property->MIndex, property->MType,
                    property->MDataLength,
                    string.Join(", ", span.ToArray()));
            }
        }

        return result;

        static void AssertType(PropertyTypeInfo type, PropertyTypeInfo expected)
        {
            if (type != expected)
                Console.WriteLine("Invalid property type: {0} != {1}", type, expected);
            //throw new Exception($"Invalid property type: {type} != {expected}");
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

    public ResolvedScene LoadModels(string file)
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

    public ResolvedScene LoadModels(u8[] data, string? formatHint)
    {
        var assimp = Assimp.GetApi();

        var scene = assimp.ImportFileFromMemory(data, (u32)data.Length, 0, formatHint);

        if (scene == null)
            throw new Exception($"model could not be loaded");

        var result = LoadModels(scene);

        assimp.FreeScene(scene);
        return result;
    }

    public static class MaterialConverter
    {
        // Texture Flags Constants (match shader)
        private const u32 TEX_BASECOLOR = 1u;
        private const u32 TEX_NORMAL = 2u;

        private const u32
            TEX_MRA = 4u; // Assuming AmbientTexture (map_Ka) OR a potential combined MRA texture maps here

        private const u32 TEX_EMISSIVE = 8u;
        private const u32 TEX_OCCLUSION = 16u; // Use this if AO (map_Ka) is treated separately from MRA

        // Alpha Mode Constants (match shader)
        private const u32 ALPHA_OPAQUE = 0u;
        private const u32 ALPHA_MASK = 1u;
        // Note: Alpha Blend (2u) would require a different pipeline/pass

        // Roughness clamping defaults
        private const f32 MIN_ROUGHNESS = 0.04f; // Min roughness to avoid pure mirrors/division issues
        private const f32 MAX_ROUGHNESS = 1.0f;

        public static PbrGBufferShader.PbrMaterialUniform ConvertToPbrUniform(
            Material? sourceMaterial,
            u32 objectId = 0u, // Pass object ID if needed
            u32 materialId = 0u, // Pass material ID if needed
            bool assumeMetallicRoughnessTexture = false, // Set true if map_Ks/map_Ns hint at a packed MRA texture
            string? metallicRoughnessAOTexturePath = null // Explicit path if known
        )
        {
            if (sourceMaterial == null)
            {
                // Return a default fallback material uniform
                Console.WriteLine(
                    "Warning: Null source material provided to converter. Returning default PBR uniform.");
                return new PbrGBufferShader.PbrMaterialUniform(
                    baseColorFactor: new vec4(0.8f, 0.8f, 0.8f, 1.0f), // Default grey
                    metallicFactor: 0.0f,
                    roughnessFactor: 0.5f,
                    occlusionStrength: 1.0f,
                    objectId: objectId,
                    materialId: materialId,
                    emissiveFactor: vec3.Zero,
                    normalScale: 1.0f,
                    textureFlags: 0u,
                    alphaCutoff: 0.5f,
                    alphaMode: ALPHA_OPAQUE
                );
            }

            var pbr = new PbrGBufferShader.PbrMaterialUniform();

            // --- Assign IDs ---
            pbr.ObjectId = objectId;
            pbr.MaterialId = materialId;

            // --- Setup Defaults ---
            pbr.MetallicFactor = 0.0f; // Default to dielectric
            pbr.RoughnessFactor = 0.5f; // Default to moderately rough
            pbr.OcclusionStrength = 1.0f; // Default to no occlusion modification
            pbr.NormalScale = 1.0f; // Default normal map strength
            pbr.AlphaCutoff = 0.01f; // Default cutoff for masking
            pbr.TextureFlags = 0u; // Start with no texture flags

            // --- Base Color & Alpha ---
            // Start with diffuse color and material opacity
            pbr.BaseColorFactorRef = new Vector4(sourceMaterial.DiffuseReflectivity, sourceMaterial.Transparency);

            // --- Metallic Heuristic ---
            // Simple heuristic: Illum model 3 suggests metallic reflection workflow
            // A more complex one could check Ks brightness vs Kd darkness, but it's unreliable.
            var likelyMetal = sourceMaterial.IlluminationModel == 3;

            if (likelyMetal &&
                sourceMaterial.SpecularReflectivity.LengthSquared() > 0.01f) // Check if Ks is non-trivial
            {
                pbr.MetallicFactor = 1.0f;
                // For metals, baseColor *is* the specular color (F0)
                pbr.BaseColorFactorRef = new Vector4(sourceMaterial.SpecularReflectivity, sourceMaterial.Transparency);
                // We generally ignore map_Ks in simple metallic conversions, assuming baseColor texture is used instead.
                // If map_Ks is meant to be the BaseColor map for metals, override here:
                // if (!string.IsNullOrEmpty(sourceMaterial.SpecularTexture)) {
                //     pbr.BaseColorTexturePath = sourceMaterial.SpecularTexture; // Requires PbrMaterialUniform to hold paths or texture handles
                // }
            }
            else
            {
                pbr.MetallicFactor = 0.0f;
                // BaseColorFactor remains based on Kd (already set)
            }

            // --- Roughness Conversion ---
            // Convert Ns (Shininess) to Roughness. High Ns = low roughness.
            if (sourceMaterial.SpecularExponent > 0.0f)
            {
                pbr.RoughnessFactor = MathF.Sqrt(2.0f / (sourceMaterial.SpecularExponent + 2.0f));
            }
            else
            {
                pbr.RoughnessFactor = MAX_ROUGHNESS; // Default to max roughness if Ns is invalid
            }

            // Clamp roughness to a sensible range
            pbr.RoughnessFactor = glm.Clamp(pbr.RoughnessFactor, MIN_ROUGHNESS, MAX_ROUGHNESS);

            // --- Emissive ---
            pbr.EmissiveFactorRef = sourceMaterial.EmissiveCoefficient;

            // --- Texture Flags & Paths ---
            // Note: PbrMaterialUniform only stores flags. The actual texture loading/binding
            // happens elsewhere using the sourceMaterial's texture path strings.

            if (!string.IsNullOrEmpty(sourceMaterial.DiffuseTexture))
            {
                pbr.TextureFlags |= TEX_BASECOLOR;
                // If this texture contains alpha, it might influence alpha mode, but we base mode on Opacity/map_d first.
            }

            if (!string.IsNullOrEmpty(sourceMaterial.NormalTexture))
            {
                pbr.TextureFlags |= TEX_NORMAL;
            }
            else if (!string.IsNullOrEmpty(sourceMaterial.BumpMapTexture))
            {
                // Has a bump map, but needs conversion to normal map offline.
                // We don't set the TEX_NORMAL flag here, but could set a custom flag
                // or log a warning. The pipeline needs to know not to bind a normal map yet.
                Console.WriteLine(
                    $"Warning: Material '{sourceMaterial.Name}' uses bump map '{sourceMaterial.BumpMapTexture}'. Convert to Normal Map.");
                // Option: Provide the path if PbrMaterialUniform stored it, assuming offline conversion
                // pbr.NormalTexturePath = "CONVERTED_" + sourceMaterial.BumpMapTexture;
            }

            if (!string.IsNullOrEmpty(sourceMaterial.EmissiveTexture))
            {
                pbr.TextureFlags |= TEX_EMISSIVE;
            }

            // Handle AO / MRA Texture(s)
            var hasExplicitMRATexture = !string.IsNullOrEmpty(metallicRoughnessAOTexturePath);
            if (hasExplicitMRATexture)
            {
                pbr.TextureFlags |= TEX_MRA;
                // If MRA includes AO, potentially don't set TEX_OCCLUSION separately
                // pbr.TextureFlags |= TEX_OCCLUSION; // Or maybe MRA implies AO? Depends on shader.
                // Here we might assume MRA map provides Metallic, Roughness, AND AO.
            }
            else if (assumeMetallicRoughnessTexture &&
                     !string.IsNullOrEmpty(sourceMaterial.SpecularTexture) /* or SpecularExponent map */)
            {
                // Heuristic: If we're told to assume packing, and map_Ks/map_Ns exist, treat them as one MRA map.
                // This is a big assumption! The actual packing order (MRA/ARM/etc.) isn't known.
                Console.WriteLine(
                    $"Warning: Assuming packed Metallic/Roughness(/AO?) texture for Material '{sourceMaterial.Name}' based on map_Ks/map_Ns presence.");
                pbr.TextureFlags |= TEX_MRA;
                // If map_Ka also exists, decide if it's separate or part of the pack. Let's assume separate for now if MRA is just inferred.
                if (!string.IsNullOrEmpty(sourceMaterial.AmbientTexture))
                {
                    pbr.TextureFlags |= TEX_OCCLUSION; // Treat map_Ka as separate AO
                    pbr.OcclusionStrength = 1.0f; // Enable usage of the AO texture
                }
            }
            else
            {
                // No explicit or assumed MRA map. Check for separate AO map (map_Ka).
                if (!string.IsNullOrEmpty(sourceMaterial.AmbientTexture))
                {
                    Console.WriteLine(
                        $"Info: Using AmbientTexture (map_Ka) as Occlusion texture for Material '{sourceMaterial.Name}'.");
                    pbr.TextureFlags |= TEX_OCCLUSION; // Use separate AO flag
                    pbr.OcclusionStrength = 1.0f; // Enable usage of the AO texture
                    // Our shader setup uses MRA texture slot, need to ensure AO sampling works correctly
                    // Might require binding map_Ka to the MRA slot and shader logic to sample only B channel?
                }
            }

            // --- Alpha Mode ---
            // Determine alpha mode based on Opacity and map_d hint.
            // Opaque GBuffer pass handles Opaque and Alpha Mask. Blend requires separate pass.
            var hasAlphaMaskTexture = !string.IsNullOrEmpty(sourceMaterial.StencilTexture);
            if (sourceMaterial.Opacity < (1.0f - 1e-5f) || hasAlphaMaskTexture)
            {
                pbr.AlphaMode = ALPHA_MASK;
                // BaseColorFactor alpha component already set from Opacity
            }
            else
            {
                pbr.AlphaMode = ALPHA_OPAQUE;
                // Ensure alpha is 1.0 for fully opaque materials if needed by downstream compositing
                // pbr.BaseColorFactor.a = 1.0f; // Optional override
            }

            // Ignored source properties:
            // sourceMaterial.AmbientReflectivity (Ka color) - AO texture used instead
            // sourceMaterial.TransmissionFilter (Tf)
            // sourceMaterial.Sharpness
            // sourceMaterial.OpticalDensity (Ni)
            // sourceMaterial.SpecularTexture (map_Ks) - Usually ignored
            // sourceMaterial.DisplacementMapTexture

            return pbr;
        }
    }

    public class PbrMaterialExamples
    {
        // Texture Flags Constants (match shader)
        private const u32 TEX_BASECOLOR = 1u;
        private const u32 TEX_NORMAL = 2u;
        private const u32 TEX_MRA = 4u; // MetallicRoughnessAO
        private const u32 TEX_EMISSIVE = 8u;
        private const u32 TEX_OCCLUSION = 16u; // Use this if AO is separate

        // Alpha Mode Constants (match shader)
        private const u32 ALPHA_OPAQUE = 0u;
        private const u32 ALPHA_MASK = 1u;

        public static PbrGBufferShader.PbrMaterialUniform PolishedChrome()
        {
            return new PbrGBufferShader.PbrMaterialUniform(
                // BaseColorFactor: For metals, this is the specular reflectance F0.
                // Chrome is highly reflective white/grey. Alpha is 1.0 (opaque).
                baseColorFactor: new vec4(0.95f, 0.95f, 0.95f, 1.0f),

                // MetallicFactor: Chrome is a metal.
                metallicFactor: 1.0f,

                // RoughnessFactor: "Polished" implies very low roughness for sharp reflections.
                // Avoid 0.0 exactly, use a small value.
                roughnessFactor: 0.05f,

                // OcclusionStrength: Assume maybe slight occlusion from a texture (e.g., dust).
                occlusionStrength: 1.0f,

                // IDs: Example identifiers.
                objectId: 10u,
                materialId: 1u,

                // EmissiveFactor: Not emissive.
                emissiveFactor: new vec3(0.0f),

                // NormalScale: Assume no normal map, or a very subtle one.
                normalScale: 1.0f,

                // TextureFlags: Assume textures for MRA (Metallic/Roughness/AO variation).
                // No basecolor texture needed if uniform color. No normal map assumed here.
                textureFlags: TEX_MRA, // 4u

                // AlphaCutoff: Not relevant for opaque mode.
                alphaCutoff: 0.5f,

                // AlphaMode: Fully opaque.
                alphaMode: ALPHA_OPAQUE // 0u
            );
        }

        public static PbrGBufferShader.PbrMaterialUniform WornLeather()
        {
            return new PbrGBufferShader.PbrMaterialUniform(
                // BaseColorFactor: For dielectrics, this is diffuse color.
                // A typical brown leather color. Alpha 1.0 (opaque).
                // This factor can tint the base color texture.
                baseColorFactor: new vec4(0.4f, 0.2f, 0.1f, 1.0f),

                // MetallicFactor: Leather is non-metallic (dielectric).
                metallicFactor: 0.0f,

                // RoughnessFactor: Leather is quite rough, especially if worn.
                roughnessFactor: 0.75f,

                // OcclusionStrength: Enable occlusion from texture.
                occlusionStrength: 1.0f,

                // IDs: Example identifiers.
                objectId: 11u,
                materialId: 2u,

                // EmissiveFactor: Not emissive.
                emissiveFactor: new vec3(0.0f),

                // NormalScale: Leather has significant surface detail, use normal map.
                normalScale: 1.0f,

                // TextureFlags: Assume textures for BaseColor (pattern/color variation),
                // Normal Map (surface bumps/grain), and MRA map (roughness/AO variation).
                textureFlags: TEX_BASECOLOR | TEX_NORMAL | TEX_MRA, // 1u | 2u | 4u = 7u

                // AlphaCutoff: Not relevant for opaque mode.
                alphaCutoff: 0.5f,

                // AlphaMode: Fully opaque.
                alphaMode: ALPHA_OPAQUE // 0u
            );
        }
    }
}
