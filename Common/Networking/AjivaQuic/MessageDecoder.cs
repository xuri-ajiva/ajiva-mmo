using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Interception;
using MemoryPack;
using NetworkPackets;
using Serilog;

namespace Common.Networking.AjivaQuic;

public class MessageDecoder(Instrumentation instrumentation)
{
    private enum BufferState
    {
        EmptyBuffer,
        PartialLength,
        PartialMessage
    }

    private u8[] _partialBuffer = ArrayPool<u8>.Shared.Rent(1024);
    private i32 _partialLength;
    private i32 _nextMessageLength;
    private BufferState _bufferState = BufferState.EmptyBuffer;

    private readonly Counter<i64> _batchPacketsExceptionCounter =
        instrumentation.Meter.CreateCounter<i64>("MessageDecoder_BatchPacketsException", "count",
            "Number of exceptions during batch packet processing");

    private readonly Counter<i64> _totalPacketsCounter =
        instrumentation.Meter.CreateCounter<i64>("MessageDecoder_TotalPackets", "count",
            "Total number of packets processed");

    public void DecodeMessages(ReadOnlySpan<u8> newData, IUniversalMessageHandler<QConnection> handler,
        QConnection connection)
    {
        switch (_bufferState)
        {
            case BufferState.PartialLength:
                TryReadMessageLength(newData, out var lengthBytesRead);
                newData = newData[lengthBytesRead..];
                break;
            case BufferState.PartialMessage:
                ContinuePartialMessageRead(newData, handler, connection);
                return;
            case BufferState.EmptyBuffer:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        NormalRead(newData, handler, connection);
    }

    private void ContinuePartialMessageRead(ReadOnlySpan<u8> newData, IUniversalMessageHandler<QConnection> handler,
        QConnection connection)
    {
        var needed = _nextMessageLength - _partialLength;
        if (newData.Length >= needed)
        {
            // We have enough to complete the message
            ResizeBufferIfNeeded(_nextMessageLength, _partialLength);
            newData[..needed].CopyTo(_partialBuffer.AsSpan(_partialLength));
            ProcessData(_partialBuffer.AsSpan(0, _nextMessageLength), _nextMessageLength, handler, connection);
            _partialLength = 0;
            _bufferState = BufferState.EmptyBuffer;
            NormalRead(newData[needed..], handler, connection);
        }
        else
        {
            // Still not enough, just save what we have
            SavePartialData(newData);
        }
    }

    private void NormalRead(ReadOnlySpan<u8> newData, IUniversalMessageHandler<QConnection> handler,
        QConnection connection)
    {
        while (newData.Length > 0)
        {
            if (!TryReadMessageLength(newData, out var lengthBytesRead))
            {
                _bufferState = BufferState.PartialLength;
                return; // Need more data for length
            }

            if (newData.Length - lengthBytesRead >= _nextMessageLength)
            {
                ProcessData(newData.Slice(lengthBytesRead, _nextMessageLength), _nextMessageLength, handler,
                    connection);
                newData = newData[(lengthBytesRead + _nextMessageLength)..];
            }
            else
            {
                SavePartialData(newData[lengthBytesRead..]);
                _bufferState = BufferState.PartialMessage;
                return; // Need more data for the full message
            }
        }
    }

    private void ProcessData(ReadOnlySpan<u8> slice, i32 messageLength, IUniversalMessageHandler<QConnection> handler,
        QConnection connection)
    {
        if (messageLength == 0)
        {
            return;
        }
        
        Debug.Assert(slice.Length == messageLength);
        using var state = MemoryPackReaderOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);
        var reader = new MemoryPackReader(slice, state);
        while (reader.Remaining > 0)
        {
            var pos = reader.Consumed;
            try
            {
                var type = reader.ReadUnmanaged<PacketType>();
                pos = reader.Consumed;

                handler.AutoHandle(type, ref reader, connection);
                _totalPacketsCounter.Add(1);
            }
            catch (MemoryPackSerializationException)
            {
                _batchPacketsExceptionCounter.Add(1);
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing packet of length {Length} at position {Position}", messageLength, pos);
                return;
            }
        }
    }

    private bool TryReadMessageLength(ReadOnlySpan<u8> data, out i32 lengthBytesRead)
    {
        switch (_bufferState)
        {
            case BufferState.EmptyBuffer:
                if (data.TryReadVarU64(out var tmp, out lengthBytesRead))
                {
                    _nextMessageLength = (i32)tmp;
                    return true;
                }

                SavePartialData(data);
                lengthBytesRead = data.Length;
                _nextMessageLength = 0;
                _bufferState = BufferState.PartialLength;
                return false;
            case BufferState.PartialLength:
                var originalPartialLength = _partialLength;
                var toCopy = Math.Min(BitPackingHelper.MaxVarLength - _partialLength, data.Length);
                data[..toCopy].CopyTo(_partialBuffer.AsSpan(_partialLength));
                _partialLength += toCopy;
                if (_partialBuffer.AsSpan(0, _partialLength)
                    .TryReadVarU64(out var tmp2, out lengthBytesRead))
                {
                    _nextMessageLength = (i32)tmp2;
                    lengthBytesRead -= originalPartialLength;
                    _partialLength = 0;
                    _bufferState = BufferState.EmptyBuffer;
                    return true;
                }

                lengthBytesRead = toCopy;
                return false;
            case BufferState.PartialMessage:
            default:
                throw new Exception("State");
        }
    }

    private void SavePartialData(ReadOnlySpan<u8> data)
    {
        if (data.Length == 0) return;

        // Ensure buffer is large enough
        var newLength = _partialLength + data.Length;
        ResizeBufferIfNeeded(newLength, _partialLength);

        data.CopyTo(_partialBuffer.AsSpan(_partialLength));

        _partialLength += data.Length;
    }

    private void ResizeBufferIfNeeded(i32 newLength, i32 preserveLength)
    {
        if (_partialBuffer.Length >= newLength) return;

        var newSize = Math.Max(_partialBuffer.Length * 2, newLength);
        var newBuffer = ArrayPool<u8>.Shared.Rent(newSize);
        if (preserveLength > 0)
        {
            Array.Copy(_partialBuffer, newBuffer, preserveLength);
        }
        ArrayPool<u8>.Shared.Return(_partialBuffer);
        _partialBuffer = newBuffer;
    }
}