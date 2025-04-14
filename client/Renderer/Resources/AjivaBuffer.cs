using System.Diagnostics;
using System.Runtime.CompilerServices;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.Resources;

public class AjivaBuffer<T>(
    ResourceLoader loader,
    string? label,
    BufferPtr buffer,
    BufferUsage usage,
    u64 count,
    bool cleanUp)
    : IAjivaVersionedObject where T : unmanaged
{
    private static readonly u32 SizeOfT = (u32)Unsafe.SizeOf<T>();
    public BufferPtr Buffer { get; private set; } = buffer;
    public u64 OffsetBytes { get; private set; } = 0;
    public u64 Count { get; private set; } = count;
    public u64 Size => Count * SizeOfT;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (!cleanUp) return;
        loader.Logger.LogTrace("Destroying Buffer {Buffer}", Buffer.GetIntPtr());
        Buffer.Destroy();
        Buffer.Release();
    }

    public u64 Version { get; private set; } = IAjivaVersionedObject.Sentinel;

    public void WriteBuffer(ReadOnlySpan<T> data)
    {
        loader.Logger.LogTrace("Writing Buffer {Buffer} with {Size} new values", Buffer.GetIntPtr(), data.Length);
        var mappedBuffer = Buffer.GetMappedRange<T>(UIntPtr.Zero, (nuint)Size);
        data.CopyTo(mappedBuffer);
        Buffer.Unmap();
    }

    public void QueuedWrite(ReadOnlySpan<T> data, QueuePtr queue)
    {
        if (loader.Logger.IsEnabled(LogLevel.Trace))
            loader.Logger.LogTrace("Queued Writing Buffer {Buffer} with {Size} new values", Buffer.GetIntPtr(),
                data.Length);
        if (queue == default)
            queue = loader.Queue;
        queue.WriteBuffer(Buffer, OffsetBytes, data);
    }

    public void ResizeWithoutDataPreserve(u64 newCount)
    {
        if (Count != 0 && Buffer != default)
        {
            Buffer.Release();
            Buffer.Destroy();
        }

        var range = loader.CreateBufferRaw(usage, newCount * SizeOfT, label);
        Debug.Assert(range.Size >= newCount * SizeOfT);
        Debug.Assert(range.Buffer != default);
        Buffer = range.Buffer;
        OffsetBytes = range.Offset;
        Count = newCount;
    }
}
