using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Common.Ecs;

public class NClientEcsWorld
{
    public DenseStore<NetState> NetState;
    public DenseStore<Position> Position;
    public DenseStore<Rotation> Rotation;
    public DenseStore<TransformData> Transform;
    public DenseStore<Scale> Scale;
    public DenseStore<Velocity> Velocity;
    public SparseStore<EntityType> EntityType;
    public DenseStore<RenderAsset> RenderAsset;
    public DenseStore<RenderMesh> RenderMesh;

    [MemberNotNull(nameof(NetState), nameof(Position), nameof(Rotation), nameof(Scale), nameof(Velocity),
        nameof(EntityType), nameof(RenderAsset), nameof(RenderMesh))]
    public void ClearAllEntities()
    {
        NetState = new();
        Position = new();
        Rotation = new();
        Scale = new();
        Velocity = new();
        EntityType = new();
        RenderAsset = new();
        RenderMesh = new();
        _entityIdFreeList = new();
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

    public ref struct TakeIterator(DenseStore<Position>.Iterator iterator, i32 limit = int.MaxValue)
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
        RenderAsset.Remove(id);
        RenderMesh.Remove(id);
    }
}