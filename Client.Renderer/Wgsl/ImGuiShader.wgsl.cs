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
        mat4 mvp,
        f32 gamma,
        f32 padding0,
        f32 padding1,
        f32 padding2
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
            uniforms.Mvp * vec4f(input.Position, 0.0f, 1.0f),
            input.Color,
            input.Uv
        );
    }

    [FragmentShader]
    public vec4 fs_main(Varyings v)
    {
        var color = v.Color * textureSample(t, s, v.Uv);
        var corrected = pow(color.rgb, vec3f(uniforms.Gamma));
        return vec4f(corrected, color.a);
    }
}