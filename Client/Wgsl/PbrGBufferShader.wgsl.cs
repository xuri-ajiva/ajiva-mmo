using System.Numerics;
using Generators;
using GlmSharp;
using Interception;

namespace Client.Wgsl;

using static BuiltIn;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToCompoundAssignment
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

[GenerateShader]
public partial class PbrGBufferShader
{
    public partial struct SceneUniform(
        mat4 view,
        mat4 projection,
        vec3 cameraPosition,
        f32 time
    );

    public partial struct PbrMaterialUniform(
        // Includes alpha
        vec4 baseColorFactor,
        f32 metallicFactor,
        f32 roughnessFactor,
        // For AO texture modulation
        f32 occlusionStrength,
        // Example if using SSBOs for materials
        u32 materialId,
        vec3 emissiveFactor,
        f32 normalScale,
        // Bitflags: 1=BaseColorTex, 2=NormalTex, 4=MRATex, 8=EmissiveTex, 16=OcclusionTex(if separate)
        u32 textureFlags,
        f32 alphaCutoff,
        // 0=Opaque, 1=Mask
        u32 alphaMode,
        u32 objectId
    );

    public partial struct Attributes(
        vec3 position,
        vec2 tex0Coord,
        vec2 tex1Coord,
        vec3 normal,
        vec4 tangent
    );

    public partial struct Instance(
        vec4 transform0,
        vec4 transform1,
        vec4 transform2,
        vec4 transform3,
        vec4 highlightColor
    )
    {
        public void SetTransform(Matrix4x4 transform)
        {
            Transform0 = new vec4(transform.M11, transform.M12, transform.M13, transform.M14);
            Transform1 = new vec4(transform.M21, transform.M22, transform.M23, transform.M24);
            Transform2 = new vec4(transform.M31, transform.M32, transform.M33, transform.M34);
            Transform3 = new vec4(transform.M41, transform.M42, transform.M43, transform.M44);
        }

        public Matrix4x4 GetTransform()
        {
            return new Matrix4x4(
                Transform0.x, Transform0.y, Transform0.z, Transform0.w,
                Transform1.x, Transform1.y, Transform1.z, Transform1.w,
                Transform2.x, Transform2.y, Transform2.z, Transform2.w,
                Transform3.x, Transform3.y, Transform3.z, Transform3.w
            );
        }
    }

    public partial struct Varyings(
        [Builtin("position")] vec4 position,
        vec4 worldPosition,
        vec2 texCoord0,
        vec2 texCoord1,
        vec3 worldNormal,
        vec3 worldTangent,
        vec3 worldBitangent,
        u32 materialId,
        u32 objectId,
        u32 instanceId,
        vec4 highlightColor
    );

    public partial struct GBufferTargets(
        vec4 baseColorAlpha,
        vec4 worldNormal,
        vec4 metallicRoughnessAO,
        vec4 emissive,
        uvec2 objectAndInstanceId
    );

    [Group(0), BindingMode(BindingType.Uniform)]
    private SceneUniform ubScene;

    [Group(1), BindingMode(BindingType.Uniform)]
    private PbrMaterialUniform ubMaterial;

    [Group(2), BindingMode(BindingType.Filtering)]
    private readonly sampler uSampler = null!;

    [Group(2)] private readonly texture_2d<f32> uBaseColorTexture = null!;
    [Group(2)] private readonly texture_2d<f32> uNormalTexture = null!;
    [Group(2)] private readonly texture_2d<f32> uMetallicRoughnessAOTexture = null!; // MRA packed
    [Group(2)] private readonly texture_2d<f32> uEmissiveTexture = null!;

    private static bool HasTexture(u32 flags, u32 textureBit)
    {
        return (flags & textureBit) != 0u;
    }

    [VertexShader]
    public Varyings vs_main(Attributes a, Instance i, [Builtin("instance_index")] u32 iId)
    {
        var instanceTransform = new mat4(i.Transform0, i.Transform1, i.Transform2, i.Transform3);

        var worldPos = instanceTransform * vec4(a.Position, 1.0f);
        var clipPos = ubScene.Projection * ubScene.View * worldPos;

        // Normal/Tangent transform (inverse transpose of upper 3x3 of model matrix)
        // Simplified: Assuming uniform scaling or no scaling for normal transform
        var normalMatrix = mat3x3f(vec4(instanceTransform[0]).xyz, vec4(instanceTransform[1]).xyz,
            vec4(instanceTransform[2]).xyz);

        var worldN = normalize(normalMatrix * a.Normal);
        var worldT = normalize(normalMatrix * a.Tangent.xyz);
        // Calculate bitangent using cross product and tangent handedness (a.Tangent.w)
        var worldB = normalize(cross(worldN, worldT) * a.Tangent.w);

        return new Varyings(
            clipPos,
            worldPos,
            a.Tex0Coord,
            a.Tex1Coord,
            worldN,
            worldT,
            worldB,
            ubMaterial.MaterialId,
            ubMaterial.ObjectId,
            iId,
            i.HighlightColor
        );
    }

    [FragmentShader]
    public GBufferTargets fs_main(Varyings v)
    {
        const u32 TEX_BASECOLOR = 1u;
        const u32 TEX_NORMAL = 2u;
        const u32 TEX_MRA = 4u; // MetallicRoughnessAO
        const u32 TEX_EMISSIVE = 8u;

        // --- Base Color & Alpha ---
        var baseColor = ubMaterial.BaseColorFactor;
        if (HasTexture(ubMaterial.TextureFlags, TEX_BASECOLOR))
        {
            baseColor = baseColor * textureSample(uBaseColorTexture, uSampler, v.TexCoord0);
        }

        // --- Alpha Testing ---
        if (ubMaterial.AlphaMode == 1u /* Mask */)
        {
            if (baseColor.a < ubMaterial.AlphaCutoff)
            {
                discard();
            }
        }

        // --- Normal Mapping ---
        var finalWorldNormal = v.WorldNormal; // Start with interpolated geometric normal
        if (HasTexture(ubMaterial.TextureFlags, TEX_NORMAL))
        {
            // Sample tangent-space normal map
            var tangentNormal = textureSample(uNormalTexture, uSampler, v.TexCoord0).xyz * 2.0f - 1.0f;
            tangentNormal =
                tangentNormal *
                vec3(ubMaterial.NormalScale, ubMaterial.NormalScale, 1.0f); // Apply scale, Z is often untouched

            // Construct TBN matrix (Tangent, Bitangent, Normal)
            var tbn = mat3x3f(normalize(v.WorldTangent), normalize(v.WorldBitangent), normalize(v.WorldNormal));
            // Transform tangent-space normal to world space
            finalWorldNormal = normalize(tbn * tangentNormal);
        }

        // Encode normal for G-Buffer storage (0 to 1 range)
        var encodedNormal = finalWorldNormal * 0.5f + 0.5f;

        // --- Metallic, Roughness, AO ---
        var metallic = ubMaterial.MetallicFactor;
        var roughness = ubMaterial.RoughnessFactor;
        var ao = ubMaterial.OcclusionStrength; // Default AO if no texture

        if (HasTexture(ubMaterial.TextureFlags, TEX_MRA))
        {
            var mraSample = textureSample(uMetallicRoughnessAOTexture, uSampler, v.TexCoord0);
            metallic = metallic * mraSample.r; // Metallic from Red channel
            roughness = roughness * mraSample.g; // Roughness from Green channel
            ao = ao * mraSample.b; // AO from Blue channel
        }

        // Clamp values to valid ranges
        roughness = clamp(roughness, 0.04f, 1.0f); // Avoid pure black roughness
        metallic = clamp(metallic, 0.0f, 1.0f);
        ao = clamp(ao, 0.0f, 1.0f);

        // --- Emissive ---
        var emissive = ubMaterial.EmissiveFactor;
        if (HasTexture(ubMaterial.TextureFlags, TEX_EMISSIVE))
        {
            emissive = emissive * textureSample(uEmissiveTexture, uSampler, v.TexCoord1).rgb;
        }

        // Add highlight color to emissive for debugging/selection visualization
        emissive = emissive + v.HighlightColor.rgb * v.HighlightColor.a; // Modulate by highlight alpha

        // --- Output ---
        return new GBufferTargets(
            baseColor, // Base Color (RGB), Alpha (A)
            vec4(encodedNormal, 1.0f), // Encoded World Normal (XYZ)
            vec4(metallic, roughness, ao, 1.0f), // MRA packed
            vec4(emissive, 1.0f), // Emissive (RGB)
            vec2u(v.ObjectId, v.InstanceId) // IDs
        );
    }
}
