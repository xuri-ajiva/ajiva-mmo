using System.Numerics;
using Interception;
using MemoryPack;

namespace Common.SpacialPartitioning;

[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct ChunkCoord(i32 X, i32 Y, i32 Z, i32 Level)
{
    public i32 PosDistanceTo(ChunkCoord playerChunk) =>
        Math.Abs(X - playerChunk.X) + Math.Abs(Y - playerChunk.Y) + Math.Abs(Z - playerChunk.Z);

    public Vector4 ToVector4() => new(X, Y, Z, Level);

    public string ReadableString()
    {
        // filter int.MinValue and int.MaxValue values for display
        return $"Chunk({Filter(X)}, {Filter(Y)}, {Filter(Z)}, {Filter(Level)})";

        string Filter(i32 p0)
        {
            return p0 switch
            {
                i32.MinValue => "-inf",
                i32.MaxValue => "+inf",
                _ => p0.ToString()
            };
        }
    }
}