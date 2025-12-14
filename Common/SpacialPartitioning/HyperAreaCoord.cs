using Interception;
using MemoryPack;

namespace Common.SpacialPartitioning;

[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct HyperAreaCoord(ChunkCoord Pos, ChunkExtent Extent)
{
    // --------------------------------------------------------------------------
    // 1. Helpers to convert to "Long" Min/Max for safe math
    //    Using inclusive Min, EXCLUSIVE Max (Standard for grids/arrays)
    // --------------------------------------------------------------------------

    // Min is just Pos
    private i64 MinX => Pos.X;
    private i64 MinY => Pos.Y;
    private i64 MinZ => Pos.Z;
    private i64 MinL => Pos.Level;
    public ChunkCoord Min => Pos;

    // Max is Pos + Extent (Exclusive)
    // Example: Pos 0, Extent 10 -> Max 10. Valid range: [0, 9]
    private i64 MaxX => Pos.X + Extent.X;
    private i64 MaxY => Pos.Y + Extent.Y;
    private i64 MaxZ => Pos.Z + Extent.Z;
    private i64 MaxL => Pos.Level + Extent.Level;
    public ChunkCoord Max => new ChunkCoord((i32)MaxX, (i32)MaxY, (i32)MaxZ, (i32)MaxL);

    // A standard "Empty" value to return when math fails
    public static HyperAreaCoord Empty => new HyperAreaCoord(new ChunkCoord(0, 0, 0, 0), new ChunkExtent(0, 0, 0, 0));

    public bool IsEmpty => Extent.X == 0 || Extent.Y == 0 || Extent.Z == 0 || Extent.Level == 0;

    // --------------------------------------------------------------------------
    // 2. Geometric Operations
    // --------------------------------------------------------------------------

    public bool Contains(ChunkCoord coord)
    {
        return coord.X >= MinX && coord.X < MaxX &&
               coord.Y >= MinY && coord.Y < MaxY &&
               coord.Z >= MinZ && coord.Z < MaxZ &&
               coord.Level >= MinL && coord.Level < MaxL;
    }

    public bool Contains(HyperAreaCoord other)
    {
        return MinX <= other.MinX && MaxX >= other.MaxX &&
               MinY <= other.MinY && MaxY >= other.MaxY &&
               MinZ <= other.MinZ && MaxZ >= other.MaxZ &&
               MinL <= other.MinL && MaxL >= other.MaxL;
    }

    /// <summary>
    /// Returns the Bounding Box that covers both areas (Union).
    /// </summary>
    public HyperAreaCoord Union(HyperAreaCoord other)
    {
        if (IsEmpty) return other;
        if (other.IsEmpty) return this;

        var unionMinX = Math.Min(MinX, other.MinX);
        var unionMinY = Math.Min(MinY, other.MinY);
        var unionMinZ = Math.Min(MinZ, other.MinZ);
        var unionMinL = Math.Min(MinL, other.MinL);

        var unionMaxX = Math.Max(MaxX, other.MaxX);
        var unionMaxY = Math.Max(MaxY, other.MaxY);
        var unionMaxZ = Math.Max(MaxZ, other.MaxZ);
        var unionMaxL = Math.Max(MaxL, other.MaxL);

        return FromMinMax(unionMinX, unionMinY, unionMinZ, unionMinL,
            unionMaxX, unionMaxY, unionMaxZ, unionMaxL);
    }

    /// <summary>
    /// Returns the Area where the two boxes overlap. Returns Empty if none.
    /// </summary>
    public HyperAreaCoord Intersect(HyperAreaCoord other)
    {
        var intMinX = Math.Max(MinX, other.MinX);
        var intMinY = Math.Max(MinY, other.MinY);
        var intMinZ = Math.Max(MinZ, other.MinZ);
        var intMinL = Math.Max(MinL, other.MinL);

        var intMaxX = Math.Min(MaxX, other.MaxX);
        var intMaxY = Math.Min(MaxY, other.MaxY);
        var intMaxZ = Math.Min(MaxZ, other.MaxZ);
        var intMaxL = Math.Min(MaxL, other.MaxL);

        // If any dimension has a negative or zero size, there is no intersection
        if (intMinX >= intMaxX || intMinY >= intMaxY || intMinZ >= intMaxZ || intMinL >= intMaxL)
        {
            return Empty;
        }

        return FromMinMax(intMinX, intMinY, intMinZ, intMinL,
            intMaxX, intMaxY, intMaxZ, intMaxL);
    }

    public bool IsAdjacentTo(HyperAreaCoord other)
    {
        // Adjacent means:
        // 1. Touching in exactly ONE dimension (gap == 0)
        // 2. Overlapping in ALL other dimensions

        var touchX = (MaxX == other.MinX) || (MinX == other.MaxX);
        var touchY = (MaxY == other.MinY) || (MinY == other.MaxY);
        var touchZ = (MaxZ == other.MinZ) || (MinZ == other.MaxZ);
        var touchL = (MaxL == other.MinL) || (MinL == other.MaxL);

        var overlapX = (MinX < other.MaxX) && (other.MinX < MaxX);
        var overlapY = (MinY < other.MaxY) && (other.MinY < MaxY);
        var overlapZ = (MinZ < other.MaxZ) && (other.MinZ < MaxZ);
        var overlapL = (MinL < other.MaxL) && (other.MinL < MaxL);

        return (touchX && overlapY && overlapZ && overlapL) ||
               (touchY && overlapX && overlapZ && overlapL) ||
               (touchZ && overlapX && overlapY && overlapL) ||
               (touchL && overlapX && overlapY && overlapZ);
    }

    // --------------------------------------------------------------------------
    // 3. Private Construction Helper
    // --------------------------------------------------------------------------
    private static HyperAreaCoord FromMinMax(i64 minX, i64 minY, i64 minZ, i64 minL,
        i64 maxX, i64 maxY, i64 maxZ, i64 maxL)
    {
        return new HyperAreaCoord(
            new ChunkCoord((i32)minX, (i32)minY, (i32)minZ, (i32)minL),
            new ChunkExtent((u32)(maxX - minX), (u32)(maxY - minY), (u32)(maxZ - minZ), (u32)(maxL - minL))
        );
    }

    public string ToReadableSpan()
    {
        return $"Pos({Pos.ReadableString()}) Extent({Extent.ReadableString()})";
    }
}