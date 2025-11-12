using System.Runtime.CompilerServices;
using Common.SpacialPartitioning;

namespace Common.Ecs;

public readonly record struct EntityId(i32 Value) : IComparable<EntityId>
{
    public static readonly EntityId Invalid = new(-1);
    public override string ToString() => $"Entity({Value})";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public i32 CompareTo(EntityId other) => Value.CompareTo(other.Value);
}

/// <summary>
/// Network entity ID - same structure as EntityId but explicitly for network serialization
/// </summary>
public readonly record struct NetEntId(ServerId Server, EntityId Entity)
{
    public override string ToString() => $"NetEnt({Server.Value}:{Entity.Value})";
}