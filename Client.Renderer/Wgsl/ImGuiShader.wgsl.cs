using Generators;
using GlmSharp;

namespace Client.Wgsl;

using static BuiltIn;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

[GenerateShader]
public partial class ImGuiShader
{
    public partial struct VertexInput(
        vec2 position,
        vec2 uv,
        vec4 color
    );

    public partial struct Varyings(
        [Builtin("position")] vec4 position,
        vec4 color,
        vec2 uv
    );

    public partial struct Uniforms(
        vec2 scale,
        vec2 translate
    );

    [Group(0), BindingMode(BindingType.Uniform)]
    public Uniforms uniforms;

    [Group(0), BindingMode(BindingType.Filtering)]
    private readonly sampler s = null!;

    [Group(1)] private readonly texture_2d<f32> t = null!;

    [VertexShader]
    public Varyings vs_main(VertexInput input)
    {
        return new Varyings(
            vec4f(input.Position * uniforms.Scale + uniforms.Translate, 0.0f, 1.0f),
            input.Color,
            input.Uv
        );
    }

    [FragmentShader]
    public vec4 fs_main(Varyings v)
    {
        var color = v.Color * textureSample(t, s, v.Uv);
        var corrected = pow(color.rgb, vec3(1.0f));
        return vec4f(corrected, color.a);
    }
}