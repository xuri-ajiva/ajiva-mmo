using System.Numerics;
using Interception;
using MemoryPack;

namespace Common.SpacialPartitioning;

[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct ChunkExtent(u32 X, u32 Y, u32 Z, u32 Level)
{
    public Vector4 ToVector4() => new(X, Y, Z, Level);

    public string ReadableString()
    {
        // filter int.MinValue and int.MaxValue values for display
        return $"Chunk({Filter(X)}, {Filter(Y)}, {Filter(Z)}, {Filter(Level)})";

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