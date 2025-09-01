using System.Diagnostics.Metrics;
using System.Net.Quic;
using Common.Networking.AjivaQuic;
using Interception;
using NetworkPackets;

namespace Common.Networking.AjivaQuic;

public class QStream
{
    private readonly Instrumentation _instrumentation;
    private readonly MessageDecoder _decoder = new();
    private readonly ResizableRingBuffer _writeBuffer;
    private readonly u8[] _readBuffer;
    private readonly SemaphoreSlim _dataAvailableSignal = new(1, 1);
    private readonly QuicStream _stream;
    private readonly Histogram<f64> _inMeter, _outMeter;

    public QStream(QuicStream stream, Instrumentation instrumentation, i32 bufferSize = 128 * 1024 * 1024)
    {
        _stream = stream;
        _instrumentation = instrumentation;
        _writeBuffer = new ResizableRingBuffer(bufferSize);
        _readBuffer = new u8[bufferSize];
        _inMeter = instrumentation.Meter.CreateHistogram<f64>("QStream_ReadMessages_Duration_" + stream.Id, "ms",
            "Duration of ReadMessages call");
        _outMeter = instrumentation.Meter.CreateHistogram<f64>("QStream_Flush_Duration_" + stream.Id, "ms",
            "Duration of Flush call");
    }

    [TraceInterception]
    public void ReadMessages(Action<ReadOnlySpan<u8>> handler)
    {
        using var record = Instrumentation.Measure(_inMeter);
        var bytesRead = _stream.Read(_readBuffer);
        _decoder.DecodeMessages(_readBuffer.AsSpan(0, bytesRead), handler);
    }

    [TraceInterception]
    public void Flush(CancellationToken token)
    {
        using var record = Instrumentation.Measure(_outMeter);
        while (_writeBuffer.DataAvailable() > 0)
        {
            _stream.Write(_writeBuffer.ConsumePart().Span);
            _stream.Flush();
        }

        _dataAvailableSignal.Wait(token);
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
}