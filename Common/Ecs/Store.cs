using System.Collections;

namespace Common.Ecs;

public interface IEcsStore<T>
{
    i32 MaxStored { get; }
    Span<T> Data { get; }
    void Resize(i32 newSize);
    void Add(EntityId id, T item);
    void Remove(EntityId id);
    bool Has(EntityId id);
    T Get(EntityId id);
    bool TryGet(EntityId id, out T item);
    ref T Update(EntityId id);
}

public struct FreeList
{
    private Stack<i32> _freeIds;
    private i32 _nextId;

    public FreeList()
    {
        _freeIds = new Stack<i32>();
        _nextId = 0;
    }

    public i32 GetFreeId()
    {
        if (_freeIds.Count > 0)
        {
            return _freeIds.Pop();
        }

        return _nextId++;
    }

    public void ReleaseId(i32 id)
    {
        _freeIds.Push(id);
    }
}

public readonly ref struct PairKv<K, V>(K key, ref readonly V value)
{
    public readonly K Key = key;
    public readonly ref readonly V Value = ref value;

    public void Deconstruct(out K k,  out V v)
    {
        k = Key;
        v = Value;
    }
}

public class DenseStore<T>(i32 initialSize = 1024) : IEcsStore<T>
{
    private T[] _data = new T[initialSize];
    private i32 _maxStored = 0;
    private readonly BitArray _used = new BitArray(initialSize);

    public i32 MaxStored => _maxStored;
    public Span<T> Data => _data.AsSpan(0, _maxStored);

    public void Resize(i32 newSize)
    {
        if (newSize <= _data.Length)
            return;

        Array.Resize(ref _data, newSize);
        _used.Length = newSize;
    }

    public void Add(EntityId id, T item)
    {
        if (id.Value >= _data.Length)
        {
            Resize(id.Value * 2);
        }

        _data[id.Value] = item;
        _used.Set(id.Value, true);
        if (id.Value >= _maxStored)
        {
            _maxStored = id.Value + 1;
        }
    }

    public void Remove(EntityId id)
    {
        if (id.Value >= _data.Length)
            return;

        _used.Set(id.Value, false);
    }

    public bool Has(EntityId id)
    {
        return id.Value < _data.Length && _used.Get(id.Value);
    }

    public T Get(EntityId id)
    {
        return _data[id.Value];
    }

    public bool TryGet(EntityId id, out T item)
    {
        if (id.Value < _data.Length && _used.Get(id.Value))
        {
            item = _data[id.Value];
            return true;
        }

        item = default!;
        return false;
    }

    public ref T Update(EntityId id)
    {
        return ref _data[id.Value];
    }

    public Iterator GetEnumerator()
    {
        return new Iterator(this);
    }


    public ref struct Iterator(DenseStore<T> store)
    {
        public PairKv<EntityId, T> Current { get; set; }
        private i32 _index = -1;
        public bool MoveNext()
        {
            while (true)
            {
                _index++;
                if (_index >= store._maxStored) return false;
                if (store._used.Get(_index))
                {
                    Current = new PairKv<EntityId, T>(new EntityId(_index), ref store.Update(new EntityId(_index)));
                    return true;
                }
            }
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}

public class SparseStore<T>(i32 initialSize = 1024) : IEcsStore<T>
{
    private readonly DenseStore<T> _denseStore = new(initialSize);
    private readonly Dictionary<EntityId, i32> _sparseToDense = new();
    private FreeList _freeList = new();

    public i32 MaxStored => _denseStore.MaxStored;
    public Span<T> Data => _denseStore.Data;

    public void Resize(i32 newSize)
    {
        _denseStore.Resize(newSize);
    }

    public void Add(EntityId id, T item)
    {
        if (_sparseToDense.ContainsKey(id))
        {
            _denseStore.Update(id) = item;
            return;
        }

        var denseId = _freeList.GetFreeId();
        _sparseToDense[id] = denseId;
        _denseStore.Add(new EntityId(denseId), item);
    }

    public void Remove(EntityId id)
    {
        if (_sparseToDense.TryGetValue(id, out var denseId))
        {
            _denseStore.Remove(new EntityId(denseId));
            _sparseToDense.Remove(id);
            _freeList.ReleaseId(denseId);
        }
    }

    public bool Has(EntityId id) => _sparseToDense.ContainsKey(id);

    public T Get(EntityId id)
    {
        var denseId = _sparseToDense[id];
        return _denseStore.Get(new EntityId(denseId));
    }

    public bool TryGet(EntityId id, out T item)
    {
        if (_sparseToDense.TryGetValue(id, out var denseId))
        {
            item = _denseStore.Get(new EntityId(denseId));
            return true;
        }

        item = default!;
        return false;
    }

    public ref T Update(EntityId id)
    {
        var denseId = _sparseToDense[id];
        return ref _denseStore.Update(new EntityId(denseId));
    }

    public Iterator GetIterator()
    {
        return new Iterator(this);
    }

    public ref struct Iterator(SparseStore<T> store)
    {
        public PairKv<EntityId, T> Current;

        private i32 _index = -1;

        public bool MoveNext()
        {
            _index++;
            if (_index >= store._sparseToDense.Count) return false;

            var kvp = store._sparseToDense.ElementAt(_index);
            Current = new PairKv<EntityId, T>(kvp.Key, ref store._denseStore.Update(new EntityId(kvp.Value)));
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}

public class DirtyTrackedStore<T, U>(U underlying) : IEcsStore<T> where U : IEcsStore<T>
{
    private U Underlying { get; } = underlying;
    private readonly BitArray _dirtyFlags = new(underlying.MaxStored);

    public i32 MaxStored => Underlying.MaxStored;
    public Span<T> Data => Underlying.Data;

    public void Resize(i32 newSize)
    {
        Underlying.Resize(newSize);
        _dirtyFlags.Length = newSize;
    }

    public void Add(EntityId id, T item)
    {
        Underlying.Add(id, item);
        _dirtyFlags.Set(id.Value, true);
    }

    public void Remove(EntityId id)
    {
        Underlying.Remove(id);
        _dirtyFlags.Set(id.Value, false);
    }

    public bool Has(EntityId id) => Underlying.Has(id);

    public T Get(EntityId id) => Underlying.Get(id);

    public bool TryGet(EntityId id, out T item) => Underlying.TryGet(id, out item);

    public ref T Update(EntityId id) => ref Underlying.Update(id);

    public bool IsDirty(EntityId id)
    {
        return id.Value < _dirtyFlags.Length && _dirtyFlags.Get(id.Value);
    }

    public void ClearDirty(EntityId id)
    {
        if (id.Value < _dirtyFlags.Length)
        {
            _dirtyFlags.Set(id.Value, false);
        }
    }

    public void ClearAllDirty() => _dirtyFlags.SetAll(false);
    public void SetAllDirty() => _dirtyFlags.SetAll(true);

    public DirtyIterator GetDirtyIterator()
    {
        return new DirtyIterator(this);
    }

    public ref struct DirtyIterator(DirtyTrackedStore<T, U> store)
    {
        public PairKv<EntityId, T> Current;
        private i32 _index = -1;

        public bool MoveNext()
        {
            while (true)
            {
                _index++;
                if (_index >= store.MaxStored) return false;
                var entityId = new EntityId(_index);
                if (store._dirtyFlags.Get(_index) && store.Underlying.Has(entityId))
                {
                    Current = new PairKv<EntityId, T>(entityId, ref store.Update(entityId));
                    return true;
                }
            }
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}