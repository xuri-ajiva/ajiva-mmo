using Generators;
using GlmSharp;

namespace Client.Wgsl;

using static BuiltIn;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

[GenerateShader]
public partial class RmlShader
{
    public partial struct SceneUniform(
        vec2 size
    );

    public partial struct TranslationUniform(
        vec2 translation,
        i32 hasTexture,
        u32 padding,
        vec4 padding0,
        vec4 padding1,
        vec4 padding2
    );

    public partial struct Attributes(
        vec2 position,
        //u32 packedColor,
        vec4 color,
        vec2 tex0Coord
    );


    public partial struct Varyings(
        [Builtin("position")] vec4 position,
        vec4 color,
        vec2 tex0Coord
    );

    public partial struct RmlOutputs(
        vec4 color
    );


    [Group(0), BindingMode(BindingType.Uniform)]
    private SceneUniform ubScene;


    [Group(0), BindingMode(BindingType.Filtering)]
    private readonly sampler uSampler = null!;

    [Group(0)] private TranslationUniform ubTranslation;

    [Group(1)] private readonly texture_2d<f32> uiTexture = null!;


    [VertexShader]
    public Varyings vs_main(Attributes a)
    {
        /*var colorVec = new vec4(
            f32((a.PackedColor >> 8) & 0xFF) / 255.0f,
            f32((a.PackedColor >> 16) & 0xFF) / 255.0f,
            f32((a.PackedColor >> 24) & 0xFF) / 255.0f,
            f32(a.PackedColor & 0xFF) / 255.0f
        );*/

        var relX = (a.Position.x + ubTranslation.Translation.x) / ubScene.Size.x;
        var relY = (a.Position.y + ubTranslation.Translation.y) / ubScene.Size.y;

        // Transform to NDC 
        relX = relX * 2.0f - 1.0f;
        relY = 1.0f - relY * 2.0f;

        return new Varyings(
            position: vec4(relX, relY, 0.0f, 1.0f),
            color: a.Color,
            tex0Coord: a.Tex0Coord
        );
    }

    [FragmentShader]
    public RmlOutputs fs_main(Varyings v)
    {
        var color = v.Color;

        //var texColor = textureLoad(uiTexture, uv, 0);
        if (ubTranslation.HasTexture > 0)
        {
            var uv = vec2i(floor(v.Position.xy));
            var texColor = textureSample(uiTexture, uSampler, v.Tex0Coord);
            color = texColor * v.Color;
        }

        return new RmlOutputs(
            color: color
            //new vec4(1.0f, 1.0f, 1.0f, 1.0f)
        );
    }
}