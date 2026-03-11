using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.Resources;

public class AjivaBuffer<T>(
    ResourceManager loader,
    string? label,
    BufferPtr buffer,
    BufferUsage usage,
    u64 count,
    bool cleanUp)
    : IAjivaVersionedObject where T : unmanaged
{
    public BufferUsage Usage { get; } = usage;
    public static readonly u32 SizeOfT = (u32)Unsafe.SizeOf<T>();
    public BufferPtr Buffer { get; private set; } = buffer;
    public u64 OffsetBytes { get; private set; } = 0;
    public u64 Count { get; private set; } = count;
    public u64 Size => Count * SizeOfT;
    private static u64 NextValidBufferSize(u64 size) => (size + 15) / 16 * 16;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (!cleanUp) return;
        loader.Logger.LogTrace("Destroying Buffer {Buffer}", Buffer.GetIntPtr());
        Buffer.Destroy();
        Buffer.Release();
    }

    ~AjivaBuffer()
    {
        loader.Logger.LogTrace("Finalizing AjivaBuffer {Buffer}", Buffer.GetIntPtr());
        Dispose();
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
        => QueuedWrite(data, 0, queue);

    public unsafe void QueuedWrite(ReadOnlySpan<T> data, u32 startElement, QueuePtr queue)
    {
        if (loader.Logger.IsEnabled(LogLevel.Trace))
            loader.Logger.LogTrace("Queued Writing Buffer {Buffer} with {Size} new values", Buffer.GetIntPtr(),
                data.Length);
        var source = new Span<byte>(
            Unsafe.AsPointer(ref MemoryMarshal.GetReference(data)),
            (i32)((data.Length * SizeOfT + 15) / 16 * 16));
        if (queue == default)
        {
            using var guard = loader.Api.EnterQueue();
            guard.Queue.WriteBuffer(Buffer, OffsetBytes + startElement * SizeOfT, source);
        }
        else
        {
            queue.WriteBuffer(Buffer, OffsetBytes + startElement * SizeOfT, source);
        }
    }

    public void ResizeWithoutDataPreserve(u64 newCount, bool forceNewBuffer = false)
    {
        if (!forceNewBuffer && newCount <= Count)
        {
            return;
        }

        if (Count != 0 && Buffer != default)
        {
            Buffer.Release();
        }

        var range = loader.CreateBufferRaw(Usage, NextValidBufferSize(newCount * SizeOfT), label);
        Debug.Assert(range.Size >= newCount * SizeOfT);
        Debug.Assert(range.Buffer != default);
        Buffer = range.Buffer;
        OffsetBytes = range.Offset;
        Count = newCount;
    }
}