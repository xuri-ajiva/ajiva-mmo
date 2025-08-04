namespace Client.Renderer.Resources;

public abstract class GpuResource<TSelf>(
    ResourceManager loader,
    string label
) : IAjivaVersionedObject where TSelf : GpuResource<TSelf>
{
    protected readonly ResourceManager Loader = loader;
    public string Label { get; set; } = label;
    public static readonly string SelfName = typeof(TSelf).Name;

    protected internal i32 RefCount;
    protected bool _clone;
    public bool IsClone => _clone;
    public u64 Version { get; protected set; } = IAjivaVersionedObject.Sentinel;

    public void AddRef()
    {
        Interlocked.Increment(ref RefCount);
    }

    public bool IsDisposed => RefCount <= 0;

    // Dispose / Release / Dereference
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (Interlocked.Decrement(ref RefCount) > 0) return;
        if (_clone) return;
        switch (RefCount)
        {
            case 0:
                Loader.Logger.LogInformation("Released {SelfName} {Label}", SelfName, Label);
                Destroy();
                break;
            case < 0:
                Loader.Logger.LogError("{SelfName} {Label} RefCount is negative, this should not happen", SelfName,
                    Label);
                break;
        }
    }

    protected abstract void Destroy();

    ~GpuResource()
    {
        Loader.Logger.LogWarning(
            "GpuResource {SelfName} {Resource} was not disposed properly, calling Dispose in finalizer",
            SelfName, Label);
        Dispose();
    }
}

public class GpuResourceHandle<T> : IAjivaVersionedObject, IDisposable where T : GpuResource<T>
{
    protected GpuResourceHandle(T data)
    {
        Interlocked.Increment(ref data.RefCount);
        Data = data;
    }

    protected internal T Data { get; private set; }

    ~GpuResourceHandle()
    {
        Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Data.Dispose();
    }

    private u64 _version = IAjivaVersionedObject.Sentinel;

    public u64 Version => _version + Data.Version;

    public string Name => Data.Label;

    internal void Assign(GpuResourceHandle<T> other) =>
        Assign(other.Data);

    internal void Assign(T tmp)
    {
        Interlocked.Increment(ref tmp.RefCount);
        _version += 1 + (Data.Version > tmp.Version ? 1ul : 0ul) * (Data.Version - tmp.Version);
        Data.Dispose();
        Data = tmp;
    }
}
