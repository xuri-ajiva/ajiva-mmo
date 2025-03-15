using Generators;
using GlmSharp;
using static WgslGenerator.BuiltIn;
using static GlmSharp.glm;

namespace WgslGenerator.Sample;

[GenerateShader]
public partial class DeferredShader
{
    public partial struct Varyings(
        [Builtin("position")] vec4 position,
        [Location(0)] vec2 texCoord
    );

    public partial struct Uniforms(
        mat4 inverseViewProjection,
        uint lightCount
    );

    public partial struct LightData(
        vec4 position,
        vec3 color,
        float radius
    );

    public partial struct LightsBuffer(
        LightData[] lights
    );


    [Group(0), Binding(0), Access(ShaderAccess.Uniform)]
    public Uniforms ub;

    [Group(0), Binding(1), Access(ShaderAccess.Storage | ShaderAccess.Read)]
    public LightsBuffer lightsBuffer;

    [Group(0), Binding(2)] public texture_2d<float> uAlbedo;
    [Group(0), Binding(3)] public texture_2d<float> uNormal;
    [Group(0), Binding(4)] public texture_depth_2d uDepth;


    [VertexShader]
    public Varyings vs_main([Builtin("vertex_index")] uint vertexIndex)
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
        var albedo = textureLoad(uAlbedo, new ivec2((int)floor(v.Position.x), (int)floor(v.Position.y))).rgb;
        var normal = textureLoad(uNormal, new ivec2((int)floor(v.Position.x), (int)floor(v.Position.y))).xyz;
        var depth = textureLoad(uDepth, new ivec2((int)floor(v.Position.x), (int)floor(v.Position.y)));

        var nrm = normal * 2.0f - new vec3(1.0f);
        var _position = ub.InverseViewProjection * new vec4(v.TexCoord * 2.0f - new vec2(1.0f), depth, 1.0f);
        var position = _position.xyz / _position.w;

        var result = new vec3(0.0f);
        for (var i = 0u; i < ub.LightCount; i++)
        {
            var L = lightsBuffer.Lights[i].Position.xyz - position;
            var distance = length<vec3, float>(L);
            if (distance > lightsBuffer.Lights[i].Radius)
            {
                continue;
            }

            var lambert = max(Dot(nrm, normalize(L)), 0.0f);
            result += lambert * pow(1.0f - distance / lightsBuffer.Lights[i].Radius, 2.0f) *
                      lightsBuffer.Lights[i].Color * albedo;
        }

        "//test"._raw();
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
