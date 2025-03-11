using System.Drawing;
using Generators;
using GlmSharp;

namespace WgslGenerator.Sample;

using static WgslGenerator.BuildIn;
using static GlmSharp.glm;

[GenerateShader]
public partial class ComplexShader
{
    public partial record struct UbScene(
        mat4 ViewProjection
    );

    public partial record struct UbModel(
        mat4 Transform,
        vec4 Color
    );

    public partial record struct Attributes(
        [Location(0)] vec3 Position,
        [Location(1)] vec2 Tex0Coord
    );

    public partial record struct Varyings(
        [Builtin("position")] vec4 Position,
        [Location(0)] vec2 Tex0Coord
    );

    [Group(0), Binding(0), Access(ShaderAccess.Uniform)]
    private UbScene ubScene;

    [Group(1), Binding(0), Access(ShaderAccess.Uniform)]
    private UbModel ubModel;

    [Group(1)] [Binding(1)] private sampler textureSampler;

    [Group(1)] [Binding(2)] private texture_2d<float> tex0;


    [VertexShader]
    public Varyings vs_main(Attributes a)
    {
        var pos = new vec4(a.Position, 1.0f);
        return new Varyings(
            ubScene.ViewProjection * ubModel.Transform * pos,
            a.Tex0Coord
        );
    }

    [FragmentShader]
    // [Location(0)]
    vec4 fs_main(Varyings v)
    {
        return textureSample(tex0, textureSampler, v.Tex0Coord) * ubModel.Color;
    }
}
