using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Generators;
using GlmSharp;

namespace WgslGenerator.Sample;

using static WgslGenerator.BuildIn;
using static GlmSharp.glm;

[GenerateShader]
public partial class TempleShader
{
    public partial record struct UbScene(
        mat4 ViewProjection,
        vec3 CameraPosition,
        float Time
    );

    public partial record struct UbModel(
        mat4 Transform,
        vec3 Tex0Color,
        uint ModelId,
        vec3 Tex1Color,
        float NormalMapStrength
    );

    public partial record struct Attributes(
        [Location(0)] vec3 Position,
        [Location(1)] vec2 Tex0Coord,
        [Location(2)] vec2 Tex1Coord,
        [Location(3)] vec3 Normal,
        [Location(4)] vec3 Tangent
    );

    public partial record struct Instance(
        [Location(5)] vec4 Transform0,
        [Location(6)] vec4 Transform1,
        [Location(7)] vec4 Transform2,
        [Location(8)] vec4 Transform3,
        [Location(9)] vec4 HighlightColor
    );

    public partial record struct Varyings(
        [Builtin("position")] vec4 Position,
        [Location(0)] vec2 Tex0Coord,
        [Location(1)] vec2 Tex1Coord,
        [Location(2)] vec3 Normal,
        [Location(3)] vec3 Tangent,
        [Location(4)] vec3 Bitangent,
        [Location(5)] uint MeshId,
        [Location(6)] uint InstanceId,
        [Location(7)] vec4 HighlightColor
    );

    public partial record struct ColorTargets(
        [Location(0)] vec4 Albedo,
        [Location(1)] vec4 Normal,
        [Location(2)] vec4 Emission
    );

    [Group(0), Binding(0), Access(ShaderAccess.Uniform)]
    private UbScene ubScene;

    [Group(1), Binding(0), Access(ShaderAccess.Uniform)]
    private UbModel ubModel;

    [Group(1)] [Binding(1)] private sampler uSampler;
    [Group(1)] [Binding(2)] private texture_2d<float> uAlbedo;
    [Group(1)] [Binding(3)] private texture_2d<float> uNormal;
    [Group(1)] [Binding(4)] private texture_2d<float> uEmission;

    [VertexShader]
    public Varyings vs_main(Attributes a, Instance i, [Builtin("instance_index")] uint iId)
    {
        // Build the instance transform matrix from the instance attributes
        var instanceTransform = new mat4(
            i.Transform0,
            i.Transform1,
            i.Transform2,
            i.Transform3
        );

        // Combine the model transform with the instance transform
        var modelTransform = ubModel.Transform * instanceTransform;

        var pos = new vec4(a.Position, 1.0f);
        var worldPos = modelTransform * pos;
        var outPos = ubScene.ViewProjection * worldPos;

        // For normal and tangent, extract the rotation part from the combined transform.
        var nrmTransform = "mat3x3(modelTransform[0].xyz, modelTransform[1].xyz, modelTransform[2].xyz);"._raw<mat3>();

        var nrm = normalize(nrmTransform * a.Normal);
        var tan = normalize(nrmTransform * a.Tangent);
        var bitan = cross(tan, nrm);

        return new Varyings(
            outPos,
            a.Tex0Coord,
            a.Tex1Coord,
            nrm,
            tan,
            bitan,
            ubModel.ModelId,
            iId,
            i.HighlightColor
        );
    }
    
    [FragmentShader]
    public ColorTargets fs_main(Varyings v)
    {
        var oAlbedo = textureSample(uAlbedo, uSampler, v.Tex0Coord) * new vec4(ubModel.Tex0Color, 1.0f);

        var normalMap = textureSample(uNormal, uSampler, v.Tex0Coord).xyz;
        var nrm =
            fma(normalMap.r, 2.0f, -1.0f) * v.Tangent +
            fma(normalMap.g, 2.0f, -1.0f) * v.Bitangent +
            fma(normalMap.b, 2.0f, -1.0f) * v.Normal;

        var ambient = new vec3(0.0f, 0.0f, 0.1f);

        var emission = textureSample(uEmission, uSampler, v.Tex1Coord).xyz * ubModel.Tex1Color;

        var oNormal = new vec4(mix(v.Normal, nrm, ubModel.NormalMapStrength) * 0.5f + new vec3(0.5f), 0.0f);

        return new ColorTargets(oAlbedo, oNormal, new vec4(ambient + emission + v.HighlightColor.xyz, 1.0f));
    }
}
