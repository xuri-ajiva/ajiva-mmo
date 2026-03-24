using System.Numerics;
using System.Runtime.CompilerServices;
using Interception;
using MemoryPack;

namespace Common.SpacialPartitioning;

/// <summary>
/// 128-bit Morton Code (Z-Order Curve) for 4D space-filling curve
/// Interleaves 32 bits from each of 4 dimensions (X, Y, Z, W)
/// Provides hierarchical spatial indexing with 2^32 discrete steps per dimension
/// </summary>
[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct MortonCode128(UInt128 Value) : IComparable<MortonCode128>
{
    public static readonly MortonCode128 Zero = new(0);
    public static readonly MortonCode128 Max = new(UInt128.MaxValue);

    /// <summary>
    /// Encode 4D coordinates (32 bits each) into a 128-bit Morton code
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MortonCode128 Encode(u32 x, u32 y, u32 z, u32 w)
    {
        UInt128 result = 0;

        // Interleave bits: W, Z, Y, X pattern (MSB to LSB)
        for (int i = 0; i < 32; i++)
        {
            UInt128 shift = (UInt128)(i * 4);
            result |= ((UInt128)((x >> i) & 1) << (int)shift);
            result |= ((UInt128)((y >> i) & 1) << (int)(shift + 1));
            result |= ((UInt128)((z >> i) & 1) << (int)(shift + 2));
            result |= ((UInt128)((w >> i) & 1) << (int)(shift + 3));
        }

        return new MortonCode128(result);
    }

    /// <summary>
    /// Encode normalized 4D coordinates [0.0, 1.0) into a Morton code
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MortonCode128 EncodeNormalized(f32 x, f32 y, f32 z, f32 w)
    {
        // Clamp to [0, 1) and convert to 32-bit fixed point
        u32 xi = (u32)(Math.Clamp(x, 0.0f, 0.9999999f) * uint.MaxValue);
        u32 yi = (u32)(Math.Clamp(y, 0.0f, 0.9999999f) * uint.MaxValue);
        u32 zi = (u32)(Math.Clamp(z, 0.0f, 0.9999999f) * uint.MaxValue);
        u32 wi = (u32)(Math.Clamp(w, 0.0f, 0.9999999f) * uint.MaxValue);

        return Encode(xi, yi, zi, wi);
    }

    /// <summary>
    /// Decode Morton code back to 4D integer coordinates
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (u32 x, u32 y, u32 z, u32 w) Decode()
    {
        u32 x = 0, y = 0, z = 0, w = 0;

        for (int i = 0; i < 32; i++)
        {
            UInt128 shift = (UInt128)(i * 4);
            x |= (u32)((Value >> (int)shift) & 1) << i;
            y |= (u32)((Value >> (int)(shift + 1)) & 1) << i;
            z |= (u32)((Value >> (int)(shift + 2)) & 1) << i;
            w |= (u32)((Value >> (int)(shift + 3)) & 1) << i;
        }

        return (x, y, z, w);
    }

    /// <summary>
    /// Get the Morton code prefix for a given tree level
    /// Level 0 = entire space (0 bits of precision)
    /// Level L = 2^L subdivisions per dimension (L bits of precision)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MortonCode128 GetPrefixAtLevel(i32 level)
    {
        if (level <= 0) return Zero;
        if (level >= 32) return this;

        // Keep only the top L*4 bits (L bits per dimension, 4 dimensions)
        int bitsToKeep = level * 4;
        int bitsToMask = 128 - bitsToKeep;

        UInt128 mask = UInt128.MaxValue << bitsToMask;
        return new MortonCode128(Value & mask);
    }

    /// <summary>
    /// Check if this Morton code is within the subtree defined by prefix at given level
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasPrefixAtLevel(MortonCode128 prefix, i32 level)
    {
        return GetPrefixAtLevel(level) == prefix.GetPrefixAtLevel(level);
    }

    /// <summary>
    /// Calculate the parent node (one level up in the tree)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MortonCode128 GetParent(i32 currentLevel)
    {
        if (currentLevel <= 0) return Zero;
        return GetPrefixAtLevel(currentLevel - 1);
    }

    /// <summary>
    /// Calculate the child nodes at the next level down
    /// Returns 16 children (2^4 for 4D space)
    /// </summary>
    public IEnumerable<MortonCode128> GetChildren(i32 currentLevel)
    {
        if (currentLevel >= 31) yield break;

        // Get the current prefix
        MortonCode128 prefix = GetPrefixAtLevel(currentLevel);

        // Calculate the bit position for the next level
        int bitPos = (31 - currentLevel) * 4;

        // Generate all 16 children (4 bits = 16 combinations)
        for (u32 child = 0; child < 16; child++)
        {
            UInt128 childBits = (UInt128)child << bitPos;
            yield return new MortonCode128(prefix.Value | childBits);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(MortonCode128 other) => Value.CompareTo(other.Value);

    public override string ToString() => $"Morton(0x{Value:X32})";
}

/// <summary>
/// Morton-based spatial key that combines a Morton code prefix with tree level
/// This represents a node in the 4D KD-Tree at a specific depth
/// </summary>
[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct MortonKey(MortonCode128 Code, i32 Level) : IComparable<MortonKey>
{
    /// <summary>
    /// Get the actual prefix for this key at its level
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MortonCode128 GetPrefix() => Code.GetPrefixAtLevel(Level);

    /// <summary>
    /// Check if a full-precision Morton code falls within this key's region
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(MortonCode128 code)
    {
        return code.HasPrefixAtLevel(Code, Level);
    }

    /// <summary>
    /// Get the parent key (one level up in the tree)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MortonKey GetParent()
    {
        if (Level <= 0) return this;
        return new MortonKey(Code.GetParent(Level), Level - 1);
    }

    /// <summary>
    /// Get all 16 child keys at the next level down
    /// </summary>
    public IEnumerable<MortonKey> GetChildren()
    {
        if (Level >= 31) yield break;

        foreach (var childCode in Code.GetChildren(Level))
        {
            yield return new MortonKey(childCode, Level + 1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(MortonKey other)
    {
        var levelCmp = Level.CompareTo(other.Level);
        return levelCmp != 0 ? levelCmp : Code.CompareTo(other.Code);
    }

    public override string ToString() => $"Key(L{Level}, {Code})";
}
