using Generators;
using GlmSharp;
using static Client.Wgsl.BuiltIn;

// ReSharper disable UnusedMember.Global

namespace Client.Wgsl;

[GenerateShader]
public partial class CompositorShader
{
    public partial struct Varyings(
        [Builtin("position")] vec4 position,
        [Location(0)] vec2 texCoord
    );

    public partial struct Uniforms(
        f32 gamma,
        u32 padding0,
        u32 padding1,
        u32 padding2
    );

    [Group(0), Binding(0), BindingMode(BindingType.Uniform)]
    public Uniforms ub;

    [Group(0), Binding(1)] public texture_2d<f32> uColor;
    [Group(0), Binding(4)] public texture_depth_2d uDepth;

    [VertexShader]
    public Varyings vs_main([Builtin("vertex_index")] u32 vertexIndex)
    {
        var a = new vec2(-1.0f, 1.0f);
        var b = new vec2(1.0f, 1.0f);
        var c = new vec2(-1.0f, -1.0f);
        var d = new vec2(1.0f, -1.0f);

        var pos = array(
            b, a, c,
            b, c, d
        );

        return new Varyings(
            new vec4(pos[vertexIndex], 0.0f, 1.0f),
            pos[vertexIndex] * 0.5f + new vec2(0.5f)
        );
    }

    [FragmentShader]
    public vec4 fs_main(Varyings v)
    {
        //var uv = new ivec2((i32)floor(v.Position.x), (i32)floor(v.Position.y));
        var uv = "vec2<i32>(floor(v.Position.xy))"._raw<ivec2>();

        var color = textureLoad(uColor, uv, 0);
        var depth = textureLoad(uDepth, uv, 0);
        var checker = (fract(v.Position.x / 20.0f) < 0.5f) != (fract(v.Position.y / 20.0f) < 0.5f);
        var checkerValue = select(0.0f, 0.25f, checker);
        return color * (1.0f - checkerValue) + checkerValue;
    }
}
