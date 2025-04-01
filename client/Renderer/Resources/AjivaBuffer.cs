using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.Resources;

public class AjivaBuffer<T>(ResourceLoader loader, BufferPtr buffer, u64 offset, u64 size, bool cleanUp)
    : IAjivaVersionedObject where T : unmanaged
{
    private readonly ResourceLoader _loader = loader;
    private readonly bool _cleanUp = cleanUp;
    public BufferPtr Buffer { get; private set; } = buffer;
    public u64 Offset { get; private set; } = offset;
    public u64 Size { get; private set; } = size;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (!_cleanUp) return;
        Buffer.Destroy();
        Buffer.Release();
    }

    public u64 Version { get; private set; } = IAjivaVersionedObject.Sentinel;

    public void WriteBuffer(ReadOnlySpan<T> data)
    {
        _loader.Logger.LogTrace("Writing Buffer {Buffer} with {Size} new values", Buffer.GetIntPtr(), data.Length);
        var mappedBuffer = Buffer.GetMappedRange<T>(UIntPtr.Zero, (nuint)Size);
        data.CopyTo(mappedBuffer);
        Buffer.Unmap();
        Version++;
    }

    public void QueuedWrite(ReadOnlySpan<T> data, QueuePtr queue)
    {
        _loader.Logger.LogTrace("Queued Writing Buffer {Buffer} with {Size} new values", Buffer.GetIntPtr(),
            data.Length);
        if (queue == default)
            queue = _loader.Queue;
        queue.WriteBuffer(Buffer, 0, data);
        Version++; //todo check if this is correct, as the buffer is not written directly
    }
}
