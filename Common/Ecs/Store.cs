using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Common.Ecs;

public interface RefIEnumerator<T, out TSelf> where TSelf : struct, RefIEnumerator<T, TSelf>, allows ref struct
{
    PairKv<EntityId, T> Current { get; }
    bool MoveNext();
    void Reset();
    TSelf GetEnumerator();
}

public interface IEcsStore
{
    const i32 InitialSize = 1024;
    i32 MaxStored { get; }
    void Resize(i32 newSize);
    bool Has(EntityId id);
}

public interface IEcsStore<T, out TEnumerator> : IEcsStore
    where TEnumerator : struct, RefIEnumerator<T, TEnumerator>, allows ref struct
{
    Span<T> Data { get; }
    void Add(EntityId id, T item);
    void Remove(EntityId id);
    T Get(EntityId id);
    bool TryGet(EntityId id, out T item);
    ref T Update(EntityId id);
    public TEnumerator GetEnumerator();
    public ref T this[EntityId id] => ref Update(id);
}

public readonly ref struct PairKv<K, V>(K key, ref V value)
{
    public readonly K Key = key;
    public readonly ref V Value = ref value;

    public void Deconstruct(out K k, out V v)
    {
        k = Key;
        v = Value;
    }
}

/// <summary>
/// A simple dense store that assumes all slots are valid or default.
/// Optimized for high-frequency components where existence check is implicit or handled externally.
/// </summary>
public class SimpleDenseStore<T> : IEcsStore<T, SimpleDenseStore<T>.Iterator>
{
    protected T[] _data = new T[IEcsStore.InitialSize];
    protected i32 _maxStored;

    public i32 MaxStored => _maxStored;
    public Span<T> Data => _data.AsSpan(0, _maxStored);

    public virtual void Resize(i32 newSize)
    {
        if (newSize <= _data.Length) return;
        Array.Resize(ref _data, newSize);
    }

    public virtual void Add(EntityId id, T item)
    {
        EnsureCapacity(id.Value + 1);
        _data[id.Value] = item;
        if (id.Value >= _maxStored) _maxStored = id.Value + 1;
    }

    public virtual void Remove(EntityId id)
    {
        if (id.Value < _data.Length)
            _data[id.Value] = default!;
    }

    public virtual bool Has(EntityId id) => id.Value < _data.Length;

    public virtual T Get(EntityId id) => _data[id.Value];

    public virtual bool TryGet(EntityId id, out T item)
    {
        if (id.Value < _data.Length)
        {
            item = _data[id.Value];
            return true;
        }

        item = default!;
        return false;
    }

    public virtual ref T Update(EntityId id)
    {
        EnsureCapacity(id.Value + 1);
        return ref _data[id.Value];
    }

    public Iterator GetEnumerator() => new(this);

    public ref struct Iterator(SimpleDenseStore<T> store) : RefIEnumerator<T, Iterator>
    {
        public PairKv<EntityId, T> Current { get; private set; }
        private i32 _index = -1;

        public bool MoveNext()
        {
            while (true)
            {
                _index++;
                if (_index >= store._maxStored) return false;
                Current = new PairKv<EntityId, T>(new EntityId(_index), ref store.Update(new EntityId(_index)));
                return true;
            }
        }

        public void Reset() => _index = -1;
        public Iterator GetEnumerator() => this;
    }

    protected void EnsureCapacity(i32 size)
    {
        if (size > _data.Length)
        {
            Resize(Math.Max(size, _data.Length * 2));
        }
    }
}

/// <summary>
/// A dense store that tracks validity of slots using a BitArray.
/// </summary>
public class DenseStore<T> : IEcsStore<T, DenseStore<T>.Iterator>
{
    private T[] _data = new T[IEcsStore.InitialSize];
    private i32 _maxStored;
    private readonly BitArray _used = new(IEcsStore.InitialSize);

    public i32 MaxStored => _maxStored;
    public Span<T> Data => _data.AsSpan(0, _maxStored);

    public void Resize(i32 newSize)
    {
        if (newSize <= _data.Length) return;
        Array.Resize(ref _data, newSize);
        _used.Length = newSize;
    }

    public void Add(EntityId id, T item)
    {
        if (id.Value >= _data.Length) Resize(Math.Max(id.Value * 2, IEcsStore.InitialSize));
        _data[id.Value] = item;
        _used.Set(id.Value, true);
        if (id.Value >= _maxStored) _maxStored = id.Value + 1;
    }

    public void Remove(EntityId id)
    {
        if (id.Value < _data.Length) _used.Set(id.Value, false);
    }

    public bool Has(EntityId id) => id.Value >= 0 && id.Value < _data.Length && _used.Get(id.Value);

    public T Get(EntityId id) => _data[id.Value];

    public bool TryGet(EntityId id, out T item)
    {
        if (Has(id))
        {
            item = _data[id.Value];
            return true;
        }

        item = default!;
        return false;
    }

    public ref T Update(EntityId id)
    {
        if (id.Value >= _data.Length) Resize(Math.Max(id.Value * 2, IEcsStore.InitialSize));
        _used.Set(id.Value, true);
        return ref _data[id.Value];
    }

    public Iterator GetEnumerator() => new(this);

    public ref struct Iterator(DenseStore<T> store) : RefIEnumerator<T, Iterator>
    {
        public PairKv<EntityId, T> Current { get; private set; }
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

        public void Reset() => _index = -1;
        public Iterator GetEnumerator() => this;
    }

    public ref T this[EntityId id] => ref Update(id);
}

/// <summary>
/// A sparse set store that maintains a packed dense array for efficient iteration.
/// Uses "Swap-and-Pop" for removal.
/// </summary>
public class SparseSetStore<T> : IEcsStore<T, SparseSetStore<T>.Iterator>
{
    protected T[] _dense = new T[IEcsStore.InitialSize];
    protected EntityId[] _denseToEntity = new EntityId[IEcsStore.InitialSize];
    protected readonly Dictionary<EntityId, i32> _entityToDense = new();
    protected i32 _count;

    public i32 MaxStored => _count;
    public Span<T> Data => _dense.AsSpan(0, _count);

    public void Resize(i32 newSize)
    {
        if (newSize <= _dense.Length) return;
        Array.Resize(ref _dense, newSize);
        Array.Resize(ref _denseToEntity, newSize);
    }

    public void Add(EntityId id, T item)
    {
        if (_entityToDense.TryGetValue(id, out var value))
        {
            _dense[value] = item;
            return;
        }

        if (_count >= _dense.Length) Resize(Math.Max(_count * 2, IEcsStore.InitialSize));

        _dense[_count] = item;
        _denseToEntity[_count] = id;
        _entityToDense[id] = _count;
        _count++;
    }

    public void Remove(EntityId id)
    {
        if (!_entityToDense.TryGetValue(id, out var denseIndex)) return;

        var lastIndex = _count - 1;
        var lastEntity = _denseToEntity[lastIndex];

        // Swap
        _dense[denseIndex] = _dense[lastIndex];
        _denseToEntity[denseIndex] = lastEntity;
        _entityToDense[lastEntity] = denseIndex;

        // Pop
        _entityToDense.Remove(id);
        _count--;
    }

    public bool Has(EntityId id) => _entityToDense.ContainsKey(id);

    public T Get(EntityId id) => _dense[_entityToDense[id]];

    public bool TryGet(EntityId id, out T item)
    {
        if (_entityToDense.TryGetValue(id, out var denseIndex))
        {
            item = _dense[denseIndex];
            return true;
        }

        item = default!;
        return false;
    }

    public ref T Update(EntityId id)
    {
        if (!_entityToDense.TryGetValue(id, out var denseIndex))
        {
            Add(id, default!);
        }

        return ref _dense[_entityToDense[id]];
    }

    public Iterator GetEnumerator() => new(this);
    public SparseIndexIterator GetSparseIndexEnumerator() => new(this);

    public ref struct Iterator(SparseSetStore<T> store) : RefIEnumerator<T, Iterator>
    {
        public PairKv<EntityId, T> Current { get; private set; }
        private i32 _index = -1;

        public bool MoveNext()
        {
            _index++;
            if (_index >= store._count) return false;
            Current = new PairKv<EntityId, T>(store._denseToEntity[_index], ref store._dense[_index]);
            return true;
        }

        public void Reset() => _index = -1;
        public Iterator GetEnumerator() => this;
    }

    public ref struct SparseIndexIterator(SparseSetStore<T> store)
    {
        public PairKv<u32, T> Current { get; private set; }
        private u32 _index = 0;

        public bool MoveNext()
        {
            if (_index >= store._count) return false;
            Current = new PairKv<u32, T>(_index, ref store._dense[_index]);
            _index++;
            return true;
        }

        public void Reset() => _index = 0;
        public SparseIndexIterator GetEnumerator() => this;
    }

    public ref T this[EntityId id] => ref Update(id);
}

/// <summary>
/// A dense store for boolean tags.
/// </summary>
public class DenseTagStore : SimpleDenseStore<bool>;

/// <summary>
/// A sparse store for boolean tags.
/// </summary>
public class SparseTagStore : SparseSetStore<bool>;

public class DirtyTrackedStore<T, U, I>(U underlying)
    : IEcsStore<T, DirtyTrackedStore<T, U, I>.DirtyIterator> where U : IEcsStore<T, I>, new()
    where I : struct, RefIEnumerator<T, I>, allows ref struct
{
    private U Underlying { get; } = underlying;
    private readonly BitArray _dirtyFlags = new(IEcsStore.InitialSize);

    public i32 MaxStored => Underlying.MaxStored;
    public Span<T> Data => Underlying.Data;

    public void Resize(i32 newSize)
    {
        Underlying.Resize(newSize);
        _dirtyFlags.Length = newSize;
    }

    public void Add(EntityId id, T item)
    {
        if (id.Value >= _dirtyFlags.Length)
        {
            Resize(Math.Max(id.Value * 2, IEcsStore.InitialSize));
        }

        Underlying.Add(id, item);
        _dirtyFlags.Set(id.Value, true);
    }

    public void Remove(EntityId id)
    {
        if (id.Value >= _dirtyFlags.Length) return;
        Underlying.Remove(id);
        _dirtyFlags.Set(id.Value, false);
    }

    public bool Has(EntityId id) => Underlying.Has(id);

    public T Get(EntityId id) => Underlying.Get(id);

    public bool TryGet(EntityId id, out T item) => Underlying.TryGet(id, out item);

    public ref T Update(EntityId id)
    {
        if (id.Value >= _dirtyFlags.Length)
        {
            Resize(Math.Max(id.Value * 2, IEcsStore.InitialSize));
        }

        _dirtyFlags.Set(id.Value, true);
        return ref Underlying.Update(id);
    }

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

    public DirtyIterator GetEnumerator() => new(this);

    public ref struct DirtyIterator(DirtyTrackedStore<T, U, I> store) : RefIEnumerator<T, DirtyIterator>
    {
        public PairKv<EntityId, T> Current { get; private set; }
        private I _underlyingIterator = store.Underlying.GetEnumerator();

        public bool MoveNext()
        {
            while (_underlyingIterator.MoveNext())
            {
                var entityId = _underlyingIterator.Current.Key;
                if (store._dirtyFlags.Get(entityId.Value))
                {
                    Current = new PairKv<EntityId, T>(entityId, ref _underlyingIterator.Current.Value);
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            _underlyingIterator.Reset();
        }

        public DirtyIterator GetEnumerator() => this;
    }

    public ref T this[EntityId id] => ref Update(id);
}