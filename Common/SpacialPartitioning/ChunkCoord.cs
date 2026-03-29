using System.Numerics;
using System.Runtime.CompilerServices;
using Interception;
using MemoryPack;

namespace Common.SpacialPartitioning;

/// <summary>
/// 4D Coordinate in the Multi-Verse simulation
/// X, Y, Z: Spatial dimensions (normalized [0, 1) at root level)
/// W: Multiverse ID dimension (normalized [0, 1) at root level)
/// Note: Level previously used for "dungeon instances" is now replaced by W dimension
/// Tree Level/Depth is now stored separately as part of MortonKey
/// </summary>
[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct ChunkCoord(i32 X, i32 Y, i32 Z, i32 W)
{
    /// <summary>
    /// Manhattan distance in 3D space (ignoring W dimension)
    /// </summary>
    public i32 PosDistanceTo(ChunkCoord playerChunk) =>
        Math.Abs(X - playerChunk.X) + Math.Abs(Y - playerChunk.Y) + Math.Abs(Z - playerChunk.Z);

    /// <summary>
    /// Full 4D distance including multiverse dimension
    /// </summary>
    public i32 HyperDistanceTo(ChunkCoord other) =>
        Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z) + Math.Abs(W - other.W);

    /// <summary>
    /// Convert to normalized 4D vector [0.0, 1.0)
    /// Used for Morton encoding
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (f32 x, f32 y, f32 z, f32 w) ToNormalized(i32 chunkSize)
    {
        // Normalize chunk coordinates to [0, 1) space
        // Assumes world wraps at chunk boundaries defined by chunkSize
        f32 x = (f32)(X % chunkSize) / chunkSize;
        f32 y = (f32)(Y % chunkSize) / chunkSize;
        f32 z = (f32)(Z % chunkSize) / chunkSize;
        f32 w = (f32)(W % chunkSize) / chunkSize;

        // Ensure positive values
        if (x < 0) x += 1.0f;
        if (y < 0) y += 1.0f;
        if (z < 0) z += 1.0f;
        if (w < 0) w += 1.0f;

        return (x, y, z, w);
    }

    /// <summary>
    /// Calculate Morton code for this chunk coordinate at a given tree level
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MortonKey ToMortonKey(i32 level, i32 chunkSize)
    {
        var (x, y, z, w) = ToNormalized(chunkSize);
        var code = MortonCode128.EncodeNormalized(x, y, z, w);
        return new MortonKey(code, level);
    }

    /// <summary>
    /// Convert from Morton key back to ChunkCoord
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ChunkCoord FromMortonKey(MortonKey key, i32 chunkSize)
    {
        // Get the prefix for this level
        var prefix = key.GetPrefix();

        // Decode to normalized coordinates [0, 1)
        var (xu32, yu32, zu32, wu32) = prefix.Decode();

        // Convert from 32-bit fixed point back to normalized float
        f32 x = xu32 / (f32)uint.MaxValue;
        f32 y = yu32 / (f32)uint.MaxValue;
        f32 z = zu32 / (f32)uint.MaxValue;
        f32 w = wu32 / (f32)uint.MaxValue;

        // Convert normalized [0, 1) to chunk coordinates
        i32 cx = (i32)Math.Floor(x * chunkSize);
        i32 cy = (i32)Math.Floor(y * chunkSize);
        i32 cz = (i32)Math.Floor(z * chunkSize);
        i32 cw = (i32)Math.Floor(w * chunkSize);

        return new ChunkCoord(cx, cy, cz, cw);
    }

    public Vector4 ToVector4() => new(X, Y, Z, W);

    public string ReadableString()
    {
        // filter int.MinValue and int.MaxValue values for display
        return $"Chunk({Filter(X)}, {Filter(Y)}, {Filter(Z)}, W:{Filter(W)})";

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