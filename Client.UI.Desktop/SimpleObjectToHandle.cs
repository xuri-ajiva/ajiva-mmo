using Common.Ecs;

namespace Client.UI.Desktop;

public class SimpleObjectToHandle<T>
{
    private T[] _values = [];
    private FreeList _freeList = new(1);
    public int Count => _values.Length;

    public i32 Register(T value)
    {
        var index = _freeList.GetFreeId();
        EnsureCapacity();
        _values[index] = value;
        return index;
    }

    public T Unregister(i32 handle)
    {
        var value = _values[handle];
        _freeList.ReleaseId(handle);
        _values[handle] = default!;
        return value;
    }

    public T Get(i32 handle)
    {
        return _values[handle];
    }

    public bool Has(i32 handle)
    {
        return handle > 0 && handle < _freeList.Capacity;
    }

    public bool TryGet(i32 handle, out T value)
    {
        if (!Has(handle))
        {
            value = default!;
            return false;
        }

        value = _values[handle];
        return true;
    }

    private void EnsureCapacity()
    {
        if (_values.Length > _freeList.Capacity) return;
        Array.Resize(ref _values, (i32)(_freeList.Capacity * 1.5) + 1);
    }
}