using System.Numerics;
using Generators;
using GlmSharp;

namespace Client.Wgsl;

using static BuiltIn;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

[GenerateShader]
public partial class WboitAccumShader
{
    public partial struct SceneUniform(
        mat4 viewProjection,
        vec3 cameraPosition,
        f32 time
    );

    public partial struct ModelUniform(
        mat4 transform,
        vec3 tex0Color,
        u32 modelId,
        vec3 tex1Color,
        u32 _padding
    );

    public partial struct Attributes(
        [Location(0)] vec3 position,
        [Location(1)] vec2 tex0Coord
    );

    public partial struct Instance(
        [Location(5)] vec4 transform0,
        [Location(6)] vec4 transform1,
        [Location(7)] vec4 transform2,
        [Location(8)] vec4 transform3,
        [Location(9)] vec4 highlightColor
    )
    {
        public void SetTransform(Matrix4x4 transform)
        {
            Transform0 = new vec4(transform.M11, transform.M12, transform.M13, transform.M14);
            Transform1 = new vec4(transform.M21, transform.M22, transform.M23, transform.M24);
            Transform2 = new vec4(transform.M31, transform.M32, transform.M33, transform.M34);
            Transform3 = new vec4(transform.M41, transform.M42, transform.M43, transform.M44);
        }
    }

    public partial struct AccumVaryings(
        [Builtin("position")] vec4 position,
        [Location(0)] vec2 tex0Coord,
        [Location(1)] u32 meshId,
        [Location(2)] u32 instanceId,
        [Location(3)] vec4 highlightColor
    );

    public partial struct AccumTargets(
        [Location(0)] vec4 accum, // Stores Sum(Color * Alpha * Weight), Sum(Alpha * Weight) 
        [Location(1)] vec4 reveal, // Stores Product(1 - Alpha)
        [Location(2)] uvec2 objectAndInstanceId
    );

    [Group(0), Binding(0), BindingMode(BindingType.Uniform)]
    private SceneUniform ubScene;

    [Group(1), Binding(0), BindingMode(BindingType.Uniform)]
    private ModelUniform ubModel;

    [Group(1)] [Binding(1), BindingMode(BindingType.Filtering)]
    private readonly sampler uSampler = null!;

    [Group(1)] [Binding(2)] private readonly texture_2d<f32> uAlbedoAlpha = null!;

    [VertexShader]
    public AccumVaryings vs_main(Attributes a, Instance i, [Builtin("instance_index")] u32 iId)
    {
        // Build the instance transform matrix from the instance attributes
        var instanceTransform = new mat4(i.Transform0, i.Transform1, i.Transform2, i.Transform3);

        // Combine the model transform with the instance transform
        var modelTransform = ubModel.Transform * instanceTransform;

        var pos = new vec4(a.Position, 1.0f);
        var worldPos = modelTransform * pos;
        var outPos = ubScene.ViewProjection * worldPos;

        return new AccumVaryings(
            outPos,
            a.Tex0Coord,
            ubModel.ModelId,
            iId,
            i.HighlightColor
        );
    }

    [FragmentShader]
    public AccumTargets fs_main(AccumVaryings v)
    {
        // Sample albedo and alpha
        var baseSample = textureSample(uAlbedoAlpha, uSampler, v.Tex0Coord);
        var texColor = baseSample.rgb * ubModel.Tex0Color + v.HighlightColor.rgb;
        var texAlpha = baseSample.a * v.HighlightColor.a;

        if (texAlpha < 0.01f)
        {
            discard();
        }

        // Calculate fragment depth (NDC Z is simplest here, 0.0 at near plane, 1.0 at far)
        // v.position is SV_Position / gl_FragCoord
        var fragDepthNDC = v.Position.z;

        // Calculate Weight (McGuire/Bavoil style, needs tuning)
        // k determines how strongly closer/more opaque fragments are weighted
        const f32 k = 100.0f;
        // Weight decreases with distance (1-fragDepthNDC) and lower alpha
        var weight = pow(max((1.0f - fragDepthNDC) * texAlpha, 1e-4f), k) + 1e-4f; // Add epsilon for stability

        // Alternate simpler weight:
        weight = max(1e-4f, texAlpha * (1.0f - fragDepthNDC) * 1.0f);

        // Calculate outputs
        // Accum: Weighted, PREMULTIPLIED color and weighted alpha
        var accumOut = vec4f(texColor * texAlpha * weight, texAlpha * weight);

        // Reveal: Pass alpha for the specific blend mode (Dst = Dst * (1 - Src))
        // We need to output texAlpha in the channel used by the blend state (usually R)
        var revealOut = vec4f(texAlpha, 0.0f, 0.0f, 1.0f); // Alpha in Red channel

        return new AccumTargets(accumOut, revealOut, vec2u(v.MeshId, v.InstanceId));
    }
}
