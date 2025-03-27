using Generators;
using GlmSharp;

namespace Client.Wgsl;

using static BuiltIn;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable once NotAccessedField
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value


[GenerateShader]
public partial class ObjectSamplingShader
{
    public partial struct UB_Input(
        [Location(0)] uvec2 PixelPos
    );

    public partial struct SB_Result(
        f32 Depth,
        u32 ObjectId,
        u32 InstanceId
    );

    [Group(0), Binding(0), BindingMode(BindingType.Uniform)]
    private UB_Input ubInput;

    [Group(0), Binding(1), BindingMode(BindingType.ReadOnlyStorage)]
    private SB_Result sbResult;

    [Group(0), Binding(2)] private readonly texture_depth_2d uDepth = null!;
    [Group(0), Binding(3)] private readonly texture_2d<u32> objectAndInstanceId = null!;

    [ComputeShader]
    [WorkgroupSize(1)]
    public void main()
    {
        sbResult.Depth = textureLoad(uDepth, ubInput.PixelPos, 0);
        var ids = textureLoad(objectAndInstanceId, ubInput.PixelPos, 0);
        sbResult.InstanceId = ids.g;
        sbResult.ObjectId = ids.r;
    }
}
