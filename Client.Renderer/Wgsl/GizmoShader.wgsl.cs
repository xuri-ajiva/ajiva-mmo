using System.Numerics;
using Generators;
using GlmSharp;

namespace Client.Wgsl;

using static BuiltIn;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

[GenerateShader]
public partial class GizmoShader
{
    public partial struct SceneUniform(
        mat4 view,
        mat4 projection,
        vec3 cameraPosition,
        f32 time,
        vec2 viewportSize,
        f32 gizmoPixelSize,
        u32 objectId
    );

    public partial struct Attributes(
        vec3 position,
        vec2 tex0Coord
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
        vec4 highlightColor,
        u32 objectId,
        u32 instanceId,
        vec2 uv,
        vec2 screenSpaceSize
    );

    public partial struct GizmoOutputs(
        vec4 color,
        uvec2 objectAndInstanceId
    );

    [Group(0), BindingMode(BindingType.Uniform)]
    private SceneUniform ubScene;

    [VertexShader]
    public Varyings vs_main(Attributes a, Instance i, [Builtin("instance_index")] u32 iId)
    {
        var instanceTransform = new mat4(i.Transform0, i.Transform1, i.Transform2, i.Transform3);

        // Standard model-view-projection transform
        var worldPos = instanceTransform * vec4(a.Position, 1.0f);
        var viewPos = ubScene.View * worldPos;
        var clipPos = ubScene.Projection * viewPos;

        return new Varyings(
            clipPos,
            i.HighlightColor,
            ubScene.ObjectId,
            iId,
            a.Tex0Coord,
            vec2(ubScene.GizmoPixelSize, ubScene.GizmoPixelSize)
        );
    }

    [FragmentShader]
    public GizmoOutputs fs_main(Varyings v)
    {
        // Calculate derivatives to determine pixel size in UV space
        var uvDx = dpdx(v.Uv);
        var uvDy = dpdy(v.Uv);

        // Estimate UV distance per pixel
        var uvPerPixel = max(length(uvDx), length(uvDy));

        // Calculate UV threshold for the wireframe
        var uvThickness = uvPerPixel * ubScene.GizmoPixelSize;

        // Calculate distance from edges in UV space
        var distFromEdgeX = min(v.Uv.x, 1.0f - v.Uv.x);
        var distFromEdgeY = min(v.Uv.y, 1.0f - v.Uv.y);
        var distFromEdge = min(distFromEdgeX, distFromEdgeY);

        // Discard if we're too far from the edge
        if (distFromEdge > uvThickness)
        {
            // discard();
            return new GizmoOutputs(
                v.HighlightColor * 0.05f,
                vec2u(v.ObjectId, v.InstanceId)
            );
        }

        return new GizmoOutputs(
            v.HighlightColor,
            vec2u(v.ObjectId, v.InstanceId)
        );
    }
}