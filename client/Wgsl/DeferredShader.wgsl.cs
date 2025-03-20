using Generators;
using GlmSharp;
using static Client.Wgsl.BuiltIn;
using static GlmSharp.glm;

// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace Client.Wgsl;

[GenerateShader]
public partial class DeferredShader
{
    public partial struct Varyings(
        [Builtin("position")] vec4 position,
        [Location(0)] vec2 texCoord
    );

    public partial struct Uniforms(
        mat4 inverseViewProjection,
        u32 lightCount,
        u32 padding0,
        u32 padding1,
        u32 padding2
    );

    public partial struct LightData(
        vec4 position,
        vec3 color,
        f32 radius
    );

    public partial struct LightsBuffer(
        LightData[] lights
    );


    [Group(0), Binding(0), BindingMode(BindingType.Uniform)]
    private Uniforms ub;

    [Group(0), Binding(1), BindingMode(BindingType.ReadOnlyStorage)]
    private LightsBuffer lightsBuffer;

    [Group(0), Binding(2)] private readonly texture_2d<f32> uAlbedo = null!;
    [Group(0), Binding(3)] private readonly texture_2d<f32> uNormal = null!;
    [Group(0), Binding(4)] private readonly texture_depth_2d uDepth = null!;


    [VertexShader]
    public Varyings vs_main([Builtin("vertex_index")] u32 vertex_index)
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
            vec4(pos[vertex_index], 0.0f, 1.0f),
            pos[vertex_index] * 0.5f + new vec2(0.5f)
        );
    }

    private static vec3 AlbedoColor(vec3 nrm, vec3 L, f32 distance, LightData light, vec3 albedo)
    {
        var lambert = max(Dot(nrm, normalize(L)), 0.0f);
        var color = lambert * pow(1.0f - distance / light.Radius, 2.0f) * light.Color * albedo;
        return color;
    }

    [FragmentShader]
    public vec4 fs_main(Varyings v)
    {
        var uv = vec2i(floor(v.Position.xy));
        var albedo = textureLoad(uAlbedo, uv, 0).rgb;
        var normal = textureLoad(uNormal, uv, 0).xyz;
        var depth = textureLoad(uDepth, uv, 0);

        var nrm = normal * 2.0f - new vec3(1.0f);
        var _position = ub.InverseViewProjection * new vec4(v.TexCoord * 2.0f - new vec2(1.0f), depth, 1.0f);
        var position = _position.xyz / _position.w;

        var result = new vec3(0.0f);
        for (var i = 0u; i < ub.LightCount; i++)
        {
            var light = lightsBuffer.Lights[i];
            var L = light.Position.xyz - position;
            var distance = length(L);
            if (distance > light.Radius)
            {
                continue;
            }

            var color = AlbedoColor(nrm, L, distance, light, albedo);
            result += color;
        }

        result += new vec3(0.1f);

        if (depth == 1.0f)
        {
            return new vec4(albedo, 1.0f);
        }
        else
        {
            return new vec4(result, 1.0f);
        }
    }
}
