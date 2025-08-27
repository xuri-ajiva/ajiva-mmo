using System.Net.Quic;
using NetworkPackets;

namespace Common.Networking;

public class QStream(QuicStream stream, i32 bufferSize = 64 * 1024)
{
    private readonly MessageDecoder _decoder = new();
    private readonly ResizableRingBuffer _writeBuffer = new(bufferSize);
    private readonly u8[] _readBuffer = new u8[bufferSize];
    private readonly SemaphoreSlim _dataAvailableSignal = new(1, 1);

    public void ReadMessages(Action<ReadOnlySpan<u8>> handler)
    {
        var bytesRead = stream.Read(_readBuffer);
        _decoder.DecodeMessages(_readBuffer.AsSpan(0, bytesRead), handler);
    }

    public void Flush(CancellationToken token)
    {
        while (_writeBuffer.DataAvailable() > 0)
        {
            stream.Write(_writeBuffer.ConsumePart().Span);
            stream.Flush();
        }

        _dataAvailableSignal.Wait(token);
    }

    public i32 PendingWriteBytes() => _writeBuffer.DataAvailable();

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
