using System.Numerics;
using Generators;
using GlmSharp;
using static Client.Wgsl.BuiltIn;

namespace Client.Wgsl;

[GenerateShader]
public partial class TempleShader
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
        f32 normalMapStrength
    );

    public partial struct Attributes(
        [Location(0)] vec3 position,
        [Location(1)] vec2 tex0Coord,
        [Location(2)] vec2 tex1Coord,
        [Location(3)] vec3 normal,
        [Location(4)] vec3 tangent
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

    public partial struct Varyings(
        [Builtin("position")] vec4 position,
        [Location(0)] vec2 tex0Coord,
        [Location(1)] vec2 tex1Coord,
        [Location(2)] vec3 normal,
        [Location(3)] vec3 tangent,
        [Location(4)] vec3 bitangent,
        [Location(5)] u32 meshId,
        [Location(6)] u32 instanceId,
        [Location(7)] vec4 highlightColor
    );

    public partial struct ColorTargets(
        [Location(0)] vec4 albedo,
        [Location(1)] vec4 normal,
        [Location(2)] vec4 emission
    );

    [Group(0), Binding(0), BindingMode(BindingType.Uniform)]
    private SceneUniform ubScene;

    [Group(1), Binding(0), BindingMode(BindingType.Uniform)]
    private ModelUniform ubModel;

    [Group(1)] [Binding(1), BindingMode(BindingType.Filtering)]
    private sampler uSampler;

    [Group(1)] [Binding(2)] private texture_2d<f32> uAlbedo;
    [Group(1)] [Binding(3)] private texture_2d<f32> uNormal;
    [Group(1)] [Binding(4)] private texture_2d<f32> uEmission;

    [VertexShader]
    public Varyings vs_main(Attributes a, Instance i, [Builtin("instance_index")] u32 iId)
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
        var oAlbedo = textureSample<vec2>(uAlbedo, uSampler, v.Tex0Coord) * new vec4(ubModel.Tex0Color, 1.0f);

        var normalMap = textureSample<vec2>(uNormal, uSampler, v.Tex0Coord).xyz;
        var nrm =
            fma(normalMap.r, 2.0f, -1.0f) * v.Tangent +
            fma(normalMap.g, 2.0f, -1.0f) * v.Bitangent +
            fma(normalMap.b, 2.0f, -1.0f) * v.Normal;

        var ambient = new vec3(0.0f, 0.0f, 0.1f);

        var emission = textureSample<vec2>(uEmission, uSampler, v.Tex1Coord).xyz * ubModel.Tex1Color;

        var oNormal = new vec4(mix<vec3, f32>(v.Normal, nrm, ubModel.NormalMapStrength) * 0.5f + new vec3(0.5f), 0.0f);

        return new ColorTargets(oAlbedo, oNormal, new vec4(ambient + emission + v.HighlightColor.xyz, 1.0f));
    }
}
