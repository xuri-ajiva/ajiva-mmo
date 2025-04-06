using Generators;
using GlmSharp;

namespace Client.Wgsl;

using static BuiltIn;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

[GenerateShader]
public partial class CompositorShader
{
    public partial struct CompositeVaryings(
        [Builtin("position")] vec4 position,
        [Location(0)] vec2 texCoord
    );

    public partial struct Uniforms(
        f32 gamma,
        u32 padding0,
        u32 padding1,
        u32 padding2
    );

    [Group(0), BindingMode(BindingType.Uniform)]
    public Uniforms ub;

    [Group(0)] private readonly texture_2d<f32> uColor = null!;
    [Group(0)] private readonly texture_depth_2d uDepth = null!;
    [Group(0)] private readonly texture_2d<f32> uAccum = null!;
    [Group(0)] private readonly texture_2d<f32> uRevea = null!;

    [VertexShader]
    public CompositeVaryings vs_main([Builtin("vertex_index")] u32 vertexIndex)
    {
        // Generate a fullscreen triangle (outputs vertices (-1,-1), (3,-1), (-1, 3))
        // UVs will be (0,0), (2,0), (0,2) respectively.
        var uv = vec2f(f32((vertexIndex << 1) & 2u), f32(vertexIndex & 2u));
        var pos = vec4f(uv * 2.0f - 1.0f, 0.0f, 1.0f);

        return new CompositeVaryings(pos, uv);
    }

    [FragmentShader]
    public vec4 fs_main(CompositeVaryings v)
    {
        var checker = (fract(v.Position.x / 20.0f) < 0.5f) != (fract(v.Position.y / 20.0f) < 0.5f);
        var checkerValue = select(0.0f, 0.25f, checker);

        var uv = vec2i(floor(v.Position.xy));
        var color = textureLoad(uColor, uv, 0);
        var depth = textureLoad(uDepth, uv, 0);
        var accum = textureLoad(uAccum, uv, 0);
        var reveal = textureLoad(uRevea, uv, 0);

        //if (isApproximatelyEqual(revealage, 1.0f))

        // Extract components
        var sumColorWeight = accum.rgb;
        var sumAlphaWeight = accum.a;
        var revealage = reveal.r; // Product(1 - Alpha) stored in Red channel

        // Calculate average color: Sum(C*A*W) / Sum(A*W)
        var avgTransparentColor = sumColorWeight / max(sumAlphaWeight, 1e-5f);

        // Calculate total opacity: 1 - Product(1 - Alpha)
        var totalOpacity = 1.0f - revealage;

        // Alternative formulation (often clearer): Under operator
        // Out = Src * SrcAlpha + Dst * (1 - SrcAlpha)
        var finalColor = avgTransparentColor * totalOpacity + color.rgb * (1.0f - totalOpacity);

        // Clamp color if needed (optional, helps prevent issues from slight inaccuracies)
        //finalColor = clamp(finalColor, vec3f(0.0f), vec3f(1.0f));

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (depth == 1.0f && accum.a == 0.0f)
        {
            return vec4(0.1f, 0.1f, 0.3f, 1.0f) * (1.0f - checkerValue) + checkerValue;
        }
        else
        {
            return vec4(finalColor.xyz, 1.0f);
        }
    }
}
