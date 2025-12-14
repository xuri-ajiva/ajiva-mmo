using Interception;
using MemoryPack;

namespace Common.SpacialPartitioning;

[ExcludeFromInterception]
[MemoryPackable]
public readonly partial record struct HyperAreaCoord(ChunkCoord Pos, ChunkExtent Extent)
{
    public bool Contains(ChunkCoord coord)
    {
        return (i64)coord.X >= (i64)Pos.X && (i64)coord.X <= ((i64)Pos.X + (i64)Extent.X) &&
               (i64)coord.Y >= (i64)Pos.Y && (i64)coord.Y <= ((i64)Pos.Y + (i64)Extent.Y) &&
               (i64)coord.Z >= (i64)Pos.Z && (i64)coord.Z <= ((i64)Pos.Z + (i64)Extent.Z) &&
               (i64)coord.Level >= (i64)Pos.Level && (i64)coord.Level <= (i64)((i64)Pos.Level + (i64)Extent.Level);
    }

    public bool Contains(HyperAreaCoord other)
    {
        return ((i64)other.Pos.X >= (i64)Pos.X) &&
               ((i64)other.Pos.X + (i64)other.Extent.X <= (i64)Pos.X + (i64)Extent.X) &&
               ((i64)other.Pos.Y >= (i64)Pos.Y) &&
               ((i64)other.Pos.Y + (i64)other.Extent.Y <= (i64)Pos.Y + (i64)Extent.Y) &&
               ((i64)other.Pos.Z >= (i64)Pos.Z) &&
               ((i64)other.Pos.Z + (i64)other.Extent.Z <= (i64)Pos.Z + (i64)Extent.Z) &&
               ((i64)other.Pos.Level >= (i64)Pos.Level) && ((i64)other.Pos.Level + (i64)other.Extent.Level <=
                                                            (i64)Pos.Level + (i64)Extent.Level);
    }

    public HyperAreaCoord Min(HyperAreaCoord other)
    {
        return new HyperAreaCoord(
            new ChunkCoord(
                Math.Min(Pos.X, other.Pos.X),
                Math.Min(Pos.Y, other.Pos.Y),
                Math.Min(Pos.Z, other.Pos.Z),
                Math.Min(Pos.Level, other.Pos.Level)
            ),
            new ChunkExtent(
                Math.Min(Extent.X, other.Extent.X),
                Math.Min(Extent.Y, other.Extent.Y),
                Math.Min(Extent.Z, other.Extent.Z),
                Math.Min(Extent.Level, other.Extent.Level)
            )
        );
    }
    
    public HyperAreaCoord Union(HyperAreaCoord other)
    {
        return new HyperAreaCoord(
            new ChunkCoord(
                Math.Min(Pos.X, other.Pos.X),
                Math.Min(Pos.Y, other.Pos.Y),
                Math.Min(Pos.Z, other.Pos.Z),
                Math.Min(Pos.Level, other.Pos.Level)
            ),
            new ChunkExtent(
                (u32)(Math.Max(Pos.X + (i64)Extent.X, other.Pos.X + (i64)other.Extent.X) - Math.Min(Pos.X, other.Pos.X)),
                (u32)(Math.Max(Pos.Y + (i64)Extent.Y, other.Pos.Y + (i64)other.Extent.Y) - Math.Min(Pos.Y, other.Pos.Y)),
                (u32)(Math.Max(Pos.Z + (i64)Extent.Z, other.Pos.Z + (i64)other.Extent.Z) - Math.Min(Pos.Z, other.Pos.Z)),
                (u32)(Math.Max(Pos.Level + (i64)Extent.Level, other.Pos.Level + (i64)other.Extent.Level) - Math.Min(Pos.Level, other.Pos.Level))
            )
        );
    }

    public HyperAreaCoord Intersect(HyperAreaCoord other)
    {
        var newMinX = Math.Max(Pos.X, other.Pos.X);
        var newMinY = Math.Max(Pos.Y, other.Pos.Y);
        var newMinZ = Math.Max(Pos.Z, other.Pos.Z);
        var newMinLevel = Math.Max(Pos.Level, other.Pos.Level);

        var newMaxX = Math.Min(Pos.X + (i64)Extent.X, other.Pos.X + (i64)other.Extent.X);
        var newMaxY = Math.Min(Pos.Y + (i64)Extent.Y, other.Pos.Y + (i64)other.Extent.Y);
        var newMaxZ = Math.Min(Pos.Z + (i64)Extent.Z, other.Pos.Z + (i64)other.Extent.Z);
        var newMaxLevel = Math.Min(Pos.Level + (i64)Extent.Level, other.Pos.Level + (i64)other.Extent.Level);

        if (newMinX >= newMaxX || newMinY >= newMaxY || newMinZ >= newMaxZ || newMinLevel >= newMaxLevel)
        {
            // No intersection, return an empty area
            return new HyperAreaCoord(new ChunkCoord(0, 0, 0, 0), new ChunkExtent(0, 0, 0, 0));
        }

        return new HyperAreaCoord(
            new ChunkCoord(newMinX, newMinY, newMinZ, newMinLevel),
            new ChunkExtent(
                (u32)(newMaxX - newMinX),
                (u32)(newMaxY - newMinY),
                (u32)(newMaxZ - newMinZ),
                (u32)(newMaxLevel - newMinLevel)
            )
        );
    }


    public bool IsAdjacentTo(HyperAreaCoord other)
    {
        var adjacentX = IsAdjacentInDimension(Pos.X, Extent.X, other.Pos.X, other.Extent.X) &&
                        OverlapsInDimension(Pos.Y, Extent.Y, other.Pos.Y, other.Extent.Y) &&
                        OverlapsInDimension(Pos.Z, Extent.Z, other.Pos.Z, other.Extent.Z) &&
                        OverlapsInDimension(Pos.Level, Extent.Level, other.Pos.Level, other.Extent.Level);

        var adjacentY = IsAdjacentInDimension(Pos.Y, Extent.Y, other.Pos.Y, other.Extent.Y) &&
                        OverlapsInDimension(Pos.X, Extent.X, other.Pos.X, other.Extent.X) &&
                        OverlapsInDimension(Pos.Z, Extent.Z, other.Pos.Z, other.Extent.Z) &&
                        OverlapsInDimension(Pos.Level, Extent.Level, other.Pos.Level, other.Extent.Level);

        var adjacentZ = IsAdjacentInDimension(Pos.Z, Extent.Z, other.Pos.Z, other.Extent.Z) &&
                        OverlapsInDimension(Pos.X, Extent.X, other.Pos.X, other.Extent.X) &&
                        OverlapsInDimension(Pos.Y, Extent.Y, other.Pos.Y, other.Extent.Y) &&
                        OverlapsInDimension(Pos.Level, Extent.Level, other.Pos.Level, other.Extent.Level);

        var adjacentLevel = IsAdjacentInDimension(Pos.Level, Extent.Level, other.Pos.Level, other.Extent.Level) &&
                            OverlapsInDimension(Pos.X, Extent.X, other.Pos.X, other.Extent.X) &&
                            OverlapsInDimension(Pos.Y, Extent.Y, other.Pos.Y, other.Extent.Y) &&
                            OverlapsInDimension(Pos.Z, Extent.Z, other.Pos.Z, other.Extent.Z);

        return adjacentX || adjacentY || adjacentZ || adjacentLevel;

        bool IsAdjacentInDimension(i64 pos1, u32 extent1, i64 pos2, u32 extent2)
        {
            return (pos1 + extent1 == pos2) || (pos2 + extent2 == pos1);
        }

        bool OverlapsInDimension(i64 pos1, u32 extent1, i64 pos2, u32 extent2)
        {
            return (pos1 < pos2 + extent2) && (pos2 < pos1 + extent1);
        }
    }

    public ChunkCoord MinCoord => Pos;
    public ChunkCoord MaxCoord => new((i32)(Pos.X + Extent.X), (i32)(Pos.Y + Extent.Y), (i32)(Pos.Z + Extent.Z), (i32)(Pos.Level + Extent.Level));

    public string ToReadableSpan()
    {
        return $"Pos({Pos.ReadableString()}) Extent({Extent.ReadableString()})";
    }
}