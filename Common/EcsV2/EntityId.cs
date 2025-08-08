namespace Common.EcsV2;

public readonly struct EntityId(i32 value) : IEquatable<EntityId>
{
    public readonly i32 Value = value;

    public bool Equals(EntityId other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is EntityId other && Equals(other);
    public override i32 GetHashCode() => Value;
    public static bool operator ==(EntityId left, EntityId right) => left.Equals(right);
    public static bool operator !=(EntityId left, EntityId right) => !(left == right);
    
    public override string ToString() => $"Entity({Value})";
}
