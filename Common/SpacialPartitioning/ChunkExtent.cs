using System.Numerics;
using Interception;
using MemoryPack;

namespace Common.SpacialPartitioning;

/// <summary>
/// 4D extent/size for HyperAreaCoord
/// X, Y, Z: Spatial extent
/// W: Multiverse extent
/// </summary>
[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct ChunkExtent(u32 X, u32 Y, u32 Z, u32 W)
{
    public Vector4 ToVector4() => new(X, Y, Z, W);

    public string ReadableString()
    {
        // filter int.MinValue and int.MaxValue values for display
        return $"Extent({Filter(X)}, {Filter(Y)}, {Filter(Z)}, W:{Filter(W)})";

        string Filter(u32 p0)
        {
            return p0 switch
            {
                i32.MaxValue => "+inf",
                _ => p0.ToString()
            };
        }
    }
}