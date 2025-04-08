using System.Numerics;
using Generators;
using GlmSharp;

namespace Client.Wgsl;

using static BuiltIn;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

[GenerateShader]
public partial class TempleShader
{
    public partial struct SceneUniform(
        mat4 view,
        mat4 projection,
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
        vec3 position,
        vec2 tex0Coord,
        vec2 tex1Coord,
        vec3 normal,
        vec3 tangent
    );

    public partial struct Instance(
        vec4 transform0,
        vec4 transform1,
        vec4 transform2,
        vec4 transform3,
        vec4 highlightColor
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
        vec4 worldPosition,
        vec4 viewPosition,
        vec2 tex0Coord,
        vec2 tex1Coord,
        vec3 normal,
        vec3 tangent,
        vec3 bitangent,
        u32 meshId,
        u32 instanceId,
        vec4 highlightColor
    );

    public partial struct ColorTargets(
        vec4 position,
        vec4 albedo,
        vec4 normal,
        vec4 emission,
        uvec2 objectAndInstanceId
    );

    [Group(0), BindingMode(BindingType.Uniform)]
    private SceneUniform ubScene;

    [Group(1), BindingMode(BindingType.Uniform)]
    private ModelUniform ubModel;

    [Group(1), BindingMode(BindingType.Filtering)]
    private readonly sampler uSampler = null!;

    [Group(1)] private readonly texture_2d<f32> uAlbedo = null!;
    [Group(1)] private readonly texture_2d<f32> uNormal = null!;
    [Group(1)] private readonly texture_2d<f32> uEmission = null!;

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
        var viewPos = ubScene.View * worldPos;
        var clipPos = ubScene.Projection * viewPos;

        // For normal and tangent, extract the rotation part from the combined transform.
        var nrmTransform = "mat3x3(modelTransform[0].xyz, modelTransform[1].xyz, modelTransform[2].xyz)"._raw<mat3>();

        var nrm = normalize(nrmTransform * a.Normal);
        var tan = normalize(nrmTransform * a.Tangent);
        var bitan = cross(tan, nrm);

        return new Varyings(
            clipPos,
            worldPos,
            viewPos,
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

        if (oAlbedo.a < 0.01f)
        {
            discard();
        }

        var normalMap = textureSample(uNormal, uSampler, v.Tex0Coord).xyz;
        var nrm =
            fma(normalMap.r, 2.0f, -1.0f) * v.Tangent +
            fma(normalMap.g, 2.0f, -1.0f) * v.Bitangent +
            fma(normalMap.b, 2.0f, -1.0f) * v.Normal;

        var oPosition = v.ViewPosition;

        var ambient = new vec3(0.0f, 0.0f, 0.1f);

        var emission = textureSample(uEmission, uSampler, v.Tex1Coord).xyz * ubModel.Tex1Color;

        var oNormal = new vec4(mix(v.Normal, nrm, ubModel.NormalMapStrength) * 0.5f + new vec3(0.5f), 0.0f);

        return new ColorTargets(oPosition, oAlbedo, oNormal,
            new vec4(ambient + emission + v.HighlightColor.xyz * 0.5f, 1.0f),
            new uvec2(v.MeshId, v.InstanceId));
    }
}
