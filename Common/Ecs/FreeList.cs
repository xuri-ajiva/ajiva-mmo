namespace Common.Ecs;

public struct FreeList
{
    private Stack<i32> _freeIds;
    private i32 _nextId;
    public i32 Capacity => _nextId;
    public i32 Count => _nextId - _freeIds.Count;

    public FreeList(int initialId = 0)
    {
        _freeIds = new Stack<i32>();
        _nextId = initialId;
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
        if (id == _nextId - 1)
        {
            _nextId--;
            return;
        }

        _freeIds.Push(id);
    }
}