using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Common.SpacialPartitioning;

namespace Common.Ecs;

public class NClientEcsWorld
{
    public DenseStore<NetState> NetState;
    public DenseStore<Position> Position;
    public DenseStore<Rotation> Rotation;
    public DenseStore<Scale> Scale;
    public DenseStore<Transform> Transform;
    public DenseStore<Velocity> Velocity;
    public SparseSetStore<EntityType> EntityType;
    public DenseStore<RenderAsset> RenderAsset;
    public DenseStore<RenderMesh> RenderMesh;
    public SpacialGrid? Grid;
    public u64 RenderEntityVersion { get; set; }

    [MemberNotNull(nameof(NetState), nameof(Position), nameof(Rotation), nameof(Scale), nameof(Velocity),
        nameof(EntityType), nameof(RenderAsset), nameof(RenderMesh), nameof(Transform))]
    public void ClearAllEntities()
    {
        Transform = new();
        NetState = new();
        Position = new();
        Rotation = new();
        Scale = new();
        Velocity = new();
        EntityType = new();
        RenderAsset = new();
        RenderMesh = new();
        _entityIdFreeList = new();
        Grid = null;
    }

    public NClientEcsWorld()
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
        Transform.Remove(id);
        NetState.Remove(id);
        Position.Remove(id);
        Rotation.Remove(id);
        Scale.Remove(id);
        Velocity.Remove(id);
        EntityType.Remove(id);
        RenderAsset.Remove(id);
        RenderMesh.Remove(id);
    }

    public void InitGrid(SpacialGridInfo gridInfo)
    {
        Grid = new SpacialGrid(gridInfo);
    }
}