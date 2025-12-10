using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Quic;
using System.Threading.Channels;
using Interception;
using NetworkPackets;
using Serilog;

namespace Common.Networking.AjivaQuic;

public delegate void PacketHandleDelegate<in TUserData>(PacketType packetType,
    scoped ref MemoryPack.MemoryPackReader reader, TUserData userData);

public class QStream(
    QuicStream stream,
    QConnection qConnection,
    Instrumentation instrumentation,
    i32 bufferSize = 64 * 1024 * 1024)
    : IDisposable
{
    private readonly Instrumentation _instrumentation = instrumentation;
    private readonly MessageDecoder _decoder = new(instrumentation);
    private readonly ResizableRingBuffer _writeBuffer = new(bufferSize);
    private readonly SemaphoreSlim _dataAvailableSignal = new(1, 1);

    private readonly Histogram<f64>? _inMeter = instrumentation.Meter?.CreateHistogram<f64>(
            "QStream_ReadMessages_Duration_" + stream.Id, "ms", "Duration of ReadMessages call"),
        _outMeter = instrumentation.Meter?.CreateHistogram<f64>("QStream_Flush_Duration_" + stream.Id, "ms",
            "Duration of Flush call");

    internal record struct RentedPartialBuffer(u8[] Buffer, i32 Length);

    private readonly Channel<RentedPartialBuffer> _channel = Channel.CreateBounded<RentedPartialBuffer>(
        new BoundedChannelOptions(20)
            { FullMode = BoundedChannelFullMode.Wait, SingleWriter = true });

    [ExcludeFromInterception]
    public async ValueTask ReadMessages(CancellationToken token = default)
    {
        var startingTimestamp = Stopwatch.GetTimestamp();
        var rent = ArrayPool<u8>.Shared.Rent(bufferSize);
        var partialRent = new RentedPartialBuffer(rent, await stream.ReadAsync(rent, token));
        await _channel.Writer.WriteAsync(partialRent, token);
        _inMeter?.Record(Stopwatch.GetElapsedTime(startingTimestamp).TotalMilliseconds);
    }


    public void DecodeMessages(PacketHandleDelegate<QConnection> handler)
    {
        while (_channel.Reader.TryRead(out var rentedBuffer))
        {
            try
            {
                _decoder.DecodeMessages(rentedBuffer.Buffer.AsSpan(0, rentedBuffer.Length), handler, qConnection);
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception during message decoding");
            }
            finally
            {
                ArrayPool<u8>.Shared.Return(rentedBuffer.Buffer);
            }
        }
    }

    [TraceInterception]
    public async Task FlushAsync(CancellationToken token)
    {
        var startingTimestamp = Stopwatch.GetTimestamp();
        while (_writeBuffer.DataAvailable() > 0)
        {
            var part = _writeBuffer.ConsumePart();
            stream.Write(part.Span);
            await stream.FlushAsync(token);
        }

        await _dataAvailableSignal.WaitAsync(token);
        _outMeter?.Record(Stopwatch.GetElapsedTime(startingTimestamp).TotalMilliseconds);
    }

    public i32 PendingWriteBytes() => _writeBuffer.DataAvailable();
    public i32 FreeBytes() => _writeBuffer.FreeSpace();

    public Lock.Scope EnterLock()
    {
        return _writeBuffer.EnterLock();
    }

    public void WriteLength(u64 nextMessageLength)
    {
        if (nextMessageLength == 0)
        {
            return;
        }

        Span<u8> lengthBytes = stackalloc u8[BitPackingHelper.MaxVarLength];
        var bytesWritten = lengthBytes.PackVarU64(nextMessageLength);
        _writeBuffer.Write(lengthBytes[..bytesWritten]);
    }

    public void WriteData(Span<u8> memorySpan)
    {
        _writeBuffer.Write(memorySpan);
    }

    public void SignalDataAvailable()
    {
        if (_dataAvailableSignal.CurrentCount != 0) return;

        try
        {
            _dataAvailableSignal.Release();
        }
        catch (SemaphoreFullException)
        {
            /* ignore race */
        }
    }

    public void Close()
    {
        stream.Close();
    }

    public void Close(SerializedMessage msg)
    {
        using var @lock = EnterLock();
        WriteLength(msg.Length);
        foreach (var memory in msg)
        {
            WriteData(memory.Span);
        }

        Task.WaitAll(FlushAsync(CancellationToken.None));
        stream.Close();
    }

    public void Dispose()
    {
        _dataAvailableSignal.Dispose();
        stream.Dispose();
    }
}