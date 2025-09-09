using System.Diagnostics.Metrics;
using System.Net.Quic;
using Interception;
using NetworkPackets;

namespace Common.Networking.AjivaQuic;

public class QStream : IDisposable
{
    private readonly Instrumentation _instrumentation;
    private readonly MessageDecoder _decoder;
    private readonly ResizableRingBuffer _writeBuffer;
    private readonly u8[] _readBuffer;
    private readonly SemaphoreSlim _dataAvailableSignal = new(1, 1);
    private readonly QuicStream _stream;
    private readonly QConnection _qConnection;
    private readonly Histogram<f64> _inMeter, _outMeter;

    public QStream(QuicStream stream, QConnection qConnection, Instrumentation instrumentation,
        i32 bufferSize = 64 * 1024 * 1024)
    {
        _decoder = new MessageDecoder(instrumentation);
        _stream = stream;
        _qConnection = qConnection;
        _instrumentation = instrumentation;
        _writeBuffer = new ResizableRingBuffer(bufferSize);
        _readBuffer = new u8[bufferSize];
        _inMeter = instrumentation.Meter.CreateHistogram<f64>("QStream_ReadMessages_Duration_" + stream.Id, "ms",
            "Duration of ReadMessages call");
        _outMeter = instrumentation.Meter.CreateHistogram<f64>("QStream_Flush_Duration_" + stream.Id, "ms",
            "Duration of Flush call");
    }

    [ExcludeFromInterception]
    public void ReadMessages(IUniversalMessageHandler<QConnection> handler)
    {
        using var record = Instrumentation.Measure(_inMeter);
        var bytesRead = _stream.Read(_readBuffer);
        _decoder.DecodeMessages(_readBuffer.AsSpan(0, bytesRead), handler, _qConnection);
    }

    [TraceInterception]
    public void Flush(CancellationToken token)
    {
        using var record = Instrumentation.Measure(_outMeter);
        while (_writeBuffer.DataAvailable() > 0)
        {
            var part = _writeBuffer.ConsumePart();
            _stream.Write(part.Span);
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

    public void Close()
    {
        _stream.Close();
    }

    public void Dispose()
    {
        _dataAvailableSignal.Dispose();
        _stream.Dispose();
    }
}