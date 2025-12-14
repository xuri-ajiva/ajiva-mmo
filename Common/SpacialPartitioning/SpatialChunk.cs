using Common.Ecs;

namespace Common.SpacialPartitioning;

public record SpatialChunk(List<EntityId> Entities)
{
    private readonly Lock _lock = new();
    public bool IsDirty;

    public void Add(EntityId entityId)
    {
        using var _ = UseLock();
        Entities.Add(entityId);
        MarkDirty();
    }

    public void Remove(EntityId entityId)
    {
        using var _ = UseLock();
        Entities.Remove(entityId);
        MarkDirty();
    }

    public void MarkDirty() => IsDirty = true;

    public void ClearDirty() => IsDirty = false;

    public Lock.Scope UseLock()
    {
        return _lock.EnterScope();
    }
}