using Generators;
using GlmSharp;

namespace Client.Wgsl;

using static BuiltIn;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToCompoundAssignment
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

[GenerateShader]
public partial class PbrDeferredLightingShader
{
    public partial struct Varyings(
        [Builtin("position")] vec4 position,
        vec2 screenCoord
    );

    public partial struct SceneUniforms(
        mat4 inverseProjection,
        mat4 inverseView,
        vec3 cameraPositionWorld,
        u32 lightCount,
        vec3 ambientColor,
        f32 iblIntensity
    );

    public partial struct LightData(
        vec3 position,
        // (0=directional, 1=point)
        u32 type,
        vec3 color,
        f32 radius
    );

    public partial struct LightsBuffer(
        LightData[] lights
    );


    [Group(0), BindingMode(BindingType.Uniform)]
    private SceneUniforms ubScene;

    [Group(0), BindingMode(BindingType.ReadOnlyStorage)]
    private LightsBuffer lightsBuffer;

    // G-Buffer Inputs
    [Group(1)] private readonly texture_2d<f32> uBaseColorAlpha = null!;
    [Group(1)] private readonly texture_2d<f32> uWorldNormal = null!;
    [Group(1)] private readonly texture_2d<f32> uMetallicRoughnessAO = null!; // MRA
    [Group(1)] private readonly texture_depth_2d uDepth = null!;


    // Reconstruct world position from depth and screen coordinates
    private vec3 WorldPositionFromDepth(f32 depth, vec2 screen_coord)
    {
        // Convert screen coord (0-1) to NDC (-1 to 1)
        var ndc_xy = screen_coord * 2.0f - 1.0f;
        // Depth is typically 0-1 in WebGPU/DX, already NDC z for projection
        var ndc_pos = vec4(ndc_xy.x, ndc_xy.y, depth, 1.0f);

        // Unproject back to view space
        var view_pos = ubScene.InverseProjection * ndc_pos;
        // Perspective divide
        view_pos = view_pos / view_pos.w;

        // Transform to world space
        var world_pos = ubScene.InverseView * view_pos;
        return world_pos.xyz;
    }

    // Normal Distribution Function (NDF) - GGX/Trowbridge-Reitz
    private f32 DistributionGGX(vec3 N, vec3 H, f32 roughness)
    {
        var a = roughness * roughness;
        var a2 = a * a;
        var NdotH = max(dot(N, H), 0.0f);
        var NdotH2 = NdotH * NdotH;

        var nom = a2;
        var denom = (NdotH2 * (a2 - 1.0f) + 1.0f);
        denom = 3.14159265359f * denom * denom;
        return nom / max(denom, 1e-5f); // Avoid divide by zero
    }

    // Geometry Function (Schlick-GGX for Smith)
    private f32 GeometrySchlickGGX(f32 NdotV, f32 roughness)
    {
        var r = roughness + 1.0f;
        var k = r * r / 8.0f;
        var nom = NdotV;
        var denom = NdotV * (1.0f - k) + k;
        return nom / max(denom, 1e-5f); // Avoid divide by zero
    }

    // Geometry Shadowing (Smith's method)
    private f32 GeometrySmith(vec3 N, vec3 V, vec3 L, f32 roughness)
    {
        var NdotV = max(dot(N, V), 0.0f);
        var NdotL = max(dot(N, L), 0.0f);
        var ggx2 = GeometrySchlickGGX(NdotV, roughness);
        var ggx1 = GeometrySchlickGGX(NdotL, roughness);
        return ggx1 * ggx2;
    }

    // Fresnel Function (Schlick approximation)
    private vec3 FresnelSchlick(f32 cosTheta, vec3 F0)
    {
        return F0 + (vec3(1.0f) - F0) * pow(clamp(1.0f - cosTheta, 0.0f, 1.0f), 5.0f);
    }


    [VertexShader]
    public Varyings vs_main([Builtin("vertex_index")] u32 vertex_index)
    {
        // Standard fullscreen triangle trick
        var uv = vec2f(f32((vertex_index << 1) & 2u), f32(vertex_index & 2u));
        var pos = vec4f(uv * 2.0f - 1.0f, 0.0f, 1.0f);
        
        return new Varyings(pos, uv);
    }


    [FragmentShader]
    public vec4 fs_main(Varyings v)
    {
        var texelCoord = vec2i(floor(v.Position.xy));

        // Sample G-Buffer
        var gbuf_BaseColorAlpha = textureLoad(uBaseColorAlpha, texelCoord, 0);
        var gbuf_WorldNormal = textureLoad(uWorldNormal, texelCoord, 0);
        var gbuf_MRA = textureLoad(uMetallicRoughnessAO, texelCoord, 0);
        //var gbuf_Emissive = textureLoad(uEmissive, texelCoord, 0);
        var depth = textureLoad(uDepth, texelCoord, 0);

        // Skip skybox/background pixels
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (depth == 1.0)
        {
            discard(); // Or return background
        }

        // Decode G-Buffer data
        var baseColor = gbuf_BaseColorAlpha.rgb;
        var alpha = gbuf_BaseColorAlpha.a; // May not be needed here unless using for masking lights
        var N = normalize(gbuf_WorldNormal.xyz * 2.0f - 1.0f); // Decode world normal
        var metallic = gbuf_MRA.r;
        var roughness = gbuf_MRA.g;
        var ao = gbuf_MRA.b;
        //var emissive = gbuf_Emissive.rgb;

        // Reconstruct World Position (pass ubScene by ref if needed)
        var P = WorldPositionFromDepth(depth, v.ScreenCoord);

        // Calculate View Vector
        var V = normalize(ubScene.CameraPositionWorld - P);

        // Calculate F0 (base reflectivity at normal incidence)
        var F0_dielectric = new vec3(0.04f);
        var F0 = mix(F0_dielectric, baseColor, metallic); // Assuming BuiltIn.mix

        // Initialize Outgoing Radiance (Lo)
        var Lo = vec3(0.0f); // Use vec3.Zero

        // Loop through lights
        for (var i = 0u; i < ubScene.LightCount; i++) // Use uint for loop to match WGSL
        {
            // Access light data (Generator needs to handle array access lightsBuffer.Lights[i])
            var light = lightsBuffer.Lights[i]; // Assuming direct access works

            // Calculate Light Vector (L) and Attenuation
            vec3 L;
            f32 attenuation;
            f32 distanceSqr; // Use squared distance to avoid sqrt initially

            if (light.Type == 0u)
            {
                // Directional Light
                L = normalize(-light.Position); // Direction is stored, negate for L vector
                attenuation = 1.0f; // No distance attenuation
            }
            else
            {
                // Point Light
                var L_unnormalized = light.Position - P;
                distanceSqr = dot(L_unnormalized, L_unnormalized); // Use dot for length squared
                var radiusSqr = light.Radius * light.Radius;

                if (distanceSqr > radiusSqr)
                {
                    continue; // Light doesn't reach this pixel
                }

                L = normalize(L_unnormalized);

                // Attenuation calculations
                var distance = sqrt(distanceSqr); // Calculate sqrt only when needed
                var falloff = pow(max(0.0f, 1.0f - pow(distance / light.Radius, 4.0f)), 2.0f);
                // Avoid division by zero for attenuation denominator
                attenuation = falloff / max(distanceSqr, 1e-5f);
            }

            // Calculate Half Vector
            var H = normalize(V + L);

            // Common factors
            var NdotL = max(dot(N, L), 0.0f);
            var NdotV = max(dot(N, V), 0.0f); // Needed for denominator and geometry term
            var VdotH = max(dot(V, H), 0.0f); // Used for Fresnel

            if (NdotL <= 0.0f) continue;
            
            // Only calculate lighting if surface faces the light
            // Calculate PBR terms
            var D = DistributionGGX(N, H, roughness);
            var G = GeometrySmith(N, V, L, roughness);
            var F = FresnelSchlick(VdotH, F0); // Use VdotH for Fresnel with Cook-Torrance

            // Specular BRDF (Cook-Torrance)
            var numerator = D * G * F; // Note: F is vec3
            var denominator = 4.0f * NdotV * NdotL + 1e-5f; // Avoid divide by zero
            var specular = numerator / denominator;

            // Diffuse BRDF (Lambertian + energy conservation)
            var kD = vec3(1.0f) - F;
            // Ensure we don't gain energy if metallic path reflects more
            kD *= 1.0f - metallic; // Only non-metals have diffuse reflection

            // Lambertian diffuse = baseColor / PI
            var diffuse = kD * baseColor / 3.14159265359f;

            // Combine and add light contribution
            // Radiance = (diffuse + specular) * NdotL * lightColor * attenuation
            var radiance = (diffuse + specular) * NdotL * light.Color * attenuation;
            Lo += radiance;
        } // End light loop

        // --- Ambient / Image-Based Lighting (IBL) ---
        // Simple constant ambient modulated by AO
        // Using a very low ambient value as placeholder
        var ambient = new vec3(0.03f) * baseColor * ao;
        // TODO: Replace with proper IBL sampling
        Lo += ambient;
        
        // --- Final Output ---
        // Apply tone mapping / gamma correction if needed (often done in a final post-process pass)
        // Lo = pow(Lo, new vec3(1.0f/2.2f)); // Simple gamma correction example

        return new vec4(Lo, 1.0f); // Output final color (XYZ) and Alpha=1.0
    }
}
