using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Common;
using Common.Ecs;
using Common.SpacialPartitioning;

namespace Server;

public class NServerEcs
{
    public DenseStore<NetState> NetState;
    public DenseStore<Position> Position;
    public DirtyTrackedStore<Rotation, DenseStore<Rotation>, DenseStore<Rotation>.Iterator> Rotation;
    public DirtyTrackedStore<Scale, DenseStore<Scale>, DenseStore<Scale>.Iterator> Scale;
    public DenseStore<Velocity> Velocity;
    public DenseStore<SpacialData> Spacial;
    public SparseSetStore<EntityType> EntityType;
    public DenseStore<EntityOwnership> EntityOwnership;
    public SpacialGrid Grid = null!;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    [MemberNotNull(nameof(NetState), nameof(Position), nameof(Rotation), nameof(Scale), nameof(Velocity),
        nameof(EntityType), nameof(Spacial), nameof(EntityOwnership))]
    public void ClearAllEntities()
    {
        NetState = new();
        Position = new();
        Rotation = new(new());
        Scale = new(new());
        Velocity = new();
        EntityType = new();
        Spacial = new();
        EntityOwnership = new();
        _entityIdFreeList = new();
    }

    public NServerEcs()
    {
        ClearAllEntities();
    }

    private FreeList _entityIdFreeList;

    public EntityId CreateEntity() => new(_entityIdFreeList.GetFreeId());

    public Iterator GetEnumerator() => new Iterator(Position.GetEnumerator());

    public ref struct Iterator(DenseStore<Position>.Iterator iterator)
    {
        private DenseStore<Position>.Iterator _iterator = iterator;

        public bool MoveNext()
        {
            return _iterator.MoveNext();
        }

        public EntityId Current => _iterator.Current.Key;

        public Iterator GetEnumerator() => this;
    }

    public ref struct TakeIterator(DenseStore<Position>.Iterator iterator, i32 limit = i32.MaxValue)
    {
        private DenseStore<Position>.Iterator _iterator = iterator;
        private i32 _remaining = limit;

        public bool MoveNext()
        {
            return _remaining-- > 0 && _iterator.MoveNext();
        }

        public EntityId Current => _iterator.Current.Key;

        public TakeIterator GetEnumerator() => this;
    }

    public TakeIterator Take(i32 limit)
    {
        return new TakeIterator(Position.GetEnumerator(), limit);
    }

    public void DeleteEntity(EntityId id)
    {
        _entityIdFreeList.ReleaseId(id.Value);
        NetState.Remove(id);
        Position.Remove(id);
        Rotation.Remove(id);
        Scale.Remove(id);
        Velocity.Remove(id);
        EntityType.Remove(id);
        Spacial.Remove(id);
    }


    public void DoLocked(Action action)
    {
        using var trace = Instrumentation.NamedTrace("DoLocked");
        try
        {
            _semaphore.Wait();
            trace?.AddEvent(new ActivityEvent("Acquired Lock"));
            action();
            trace?.AddEvent(new ActivityEvent("Finished Action"));
        }
        finally
        {
            _semaphore.Release();
        }
    }
}