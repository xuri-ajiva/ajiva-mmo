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
        u32 instanceId,
        vec2 uv,
        vec3 worldPos
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
        var worldPos = instanceTransform * vec4(a.Position, 1.0f);

        return new Varyings(
            ubScene.Projection * (ubScene.View * worldPos),
            i.HighlightColor,
            iId,
            a.Tex0Coord,
            worldPos.xyz
        );
    }

    [FragmentShader]
    public GizmoOutputs fs_main(Varyings v)
    {
        // --- Grid Constants ---
        const f32 baseGridSpacing = 1.00f;
        const f32 lodBias = -0.20f;
        const f32 lodBlendStart = 0.40f;
        const f32 lodBlendEnd = 0.60f;
        const f32 lineWidthFine = .00f;
        const f32 lineWidthCoarse = 1.25f;
        const f32 intensityFine = 1.00f;
        const f32 intensityCoarse = 0.90f;
        const f32 minDistance = 0.01f;

        // 1. Screen-space Edge Logic
        // Using fwidth is slightly faster than manual dpdx/dpdy for thickness
        var uvThickness = fwidth(v.Uv) * ubScene.GizmoPixelSize;
        var distFromEdge = min(v.Uv, 1.0f - v.Uv);

        // 2. Camera-distance-based Continuous LOD
        var cameraDist = max(length(v.WorldPos - ubScene.CameraPosition), minDistance);
        var lodCont = max(log2(cameraDist / baseGridSpacing) + lodBias, 0.0f);
        var lodFloor = floor(lodCont);
        var lodFrac = fract(lodCont);

        // 3. Grid Spacing
        var spacingFine = baseGridSpacing * exp2(lodFloor);
        var spacingCoarse = spacingFine * 2.0f;

        // 4. Vectorized Fine Grid
        var pFine = v.WorldPos / spacingFine;
        var dFine = abs(fract(pFine) - 0.5f);
        var fwFine = max(fwidth(pFine), vec3(1e-4f));
        var maskFineVec = 1.0f - smoothstep(vec3(0.0f), fwFine * lineWidthFine, dFine);
        var fineMask = max(max(maskFineVec.x, maskFineVec.y), maskFineVec.z) * intensityFine;

        // 5. Vectorized Coarse Grid
        var pCoarse = v.WorldPos / spacingCoarse;
        var dCoarse = abs(fract(pCoarse) - 0.5f);
        var fwCoarse = max(fwidth(pCoarse), vec3(1e-4f));
        var maskCoarseVec = 1.0f - smoothstep(vec3(0.0f), fwCoarse * lineWidthCoarse, dCoarse);
        var coarseMask = max(max(maskCoarseVec.x, maskCoarseVec.y), maskCoarseVec.z) * intensityCoarse;

        // 6. Final Mask and Color Calculation
        var lodBlend = smoothstep(lodBlendStart, lodBlendEnd, lodFrac) + lodFrac * 0.5f;
        var worldGridMask = clamp(mix(fineMask, coarseMask, lodBlend), 0.0f, 1.0f);

        // Branchless edge vs fill selection
        var isInsideVec = step(uvThickness, distFromEdge);
        var isInside = isInsideVec.x * isInsideVec.y;

        var baseColor = mix(v.HighlightColor, v.HighlightColor * 0.05f, isInside);
        var finalColor = baseColor + (v.HighlightColor * worldGridMask);

        return new GizmoOutputs(
            finalColor,
            vec2u(ubScene.ObjectId, v.InstanceId)
        );
    }
}