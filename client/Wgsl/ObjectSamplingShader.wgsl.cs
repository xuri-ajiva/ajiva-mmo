using Generators;
using GlmSharp;

namespace Client.Wgsl;

using static BuiltIn;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable NotAccessedField.Local
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

[GenerateShader]
public partial class ObjectSamplingShader
{
    public partial struct Input(
        [Location(0)] uvec2 PixelPos
    );

    public partial struct Result(
        f32 Depth,
        u32 ObjectId,
        u32 InstanceId
    );

    [Group(0), Binding(0), BindingMode(BindingType.Uniform)]
    private Input _input;

    [Group(0), Binding(1), BindingMode(BindingType.Storage)]
    private Result _result;

    [Group(0), Binding(2)] private readonly texture_depth_2d uDepth = null!;
    [Group(0), Binding(3)] private readonly texture_2d<u32> objectAndInstanceId = null!;

    [ComputeShader]
    [WorkgroupSize(1)]
    public void main()
    {
        _result.Depth = textureLoad(uDepth, _input.PixelPos, 0);
        var ids = textureLoad(objectAndInstanceId, _input.PixelPos, 0);
        _result.InstanceId = ids.g;
        _result.ObjectId = ids.r;
    }
}
