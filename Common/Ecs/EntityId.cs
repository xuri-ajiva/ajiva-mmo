using System.Runtime.CompilerServices;

namespace Common.Ecs;

public readonly record struct EntityId(i32 Value) : IComparable<EntityId>
{
    public static readonly EntityId Invalid = new(-1);
    public override string ToString() => $"Entity({Value})";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public i32 CompareTo(EntityId other) => Value.CompareTo(other.Value);
}

public readonly record struct NetEntId(i32 Value)
{
    public override string ToString() => $"NetEnt({Value})";

    public static explicit operator EntityId(NetEntId netEntId) => new(netEntId.Value);
    public static explicit operator NetEntId(EntityId entityId) => new(entityId.Value);
}