using System.Diagnostics;
using NetworkPackets;

namespace Common.Networking;

public class MessageDecoder
{
    private enum BufferState
    {
        EmptyBuffer,
        PartialLength,
        PartialMessage
    }

    private u8[] _partialBuffer = new u8[1024];
    private i32 _partialLength;
    private i32 _nextMessageLength;
    private BufferState _bufferState = BufferState.EmptyBuffer;

    public void DecodeMessages(ReadOnlySpan<u8> newData, Action<ReadOnlySpan<u8>> handler)
    {
        switch (_bufferState)
        {
            case BufferState.EmptyBuffer:
                NormalRead(newData, handler);
                break;
            case BufferState.PartialLength:
                TryReadMessageLength(newData, out var lengthBytesRead);
                NormalRead(newData[lengthBytesRead..], handler);
                break;
            case BufferState.PartialMessage:
                ContinuePartialMessageRead(newData, handler);
                break;
        }
    }

    private void ContinuePartialMessageRead(ReadOnlySpan<u8> newData, Action<ReadOnlySpan<u8>> handler)
    {
        var needed = _nextMessageLength - _partialLength;
        if (newData.Length >= needed)
        {
            // We have enough to complete the message
            ResizeBufferIfNeeded(_nextMessageLength, _partialLength);
            newData[..needed].CopyTo(_partialBuffer.AsSpan(_partialLength));
            ProcessData(_partialBuffer.AsSpan(0, _nextMessageLength), _nextMessageLength, handler);
            _partialLength = 0;
            _bufferState = BufferState.EmptyBuffer;
            NormalRead(newData[needed..], handler);
        }
        else
        {
            // Still not enough, just save what we have
            SavePartialData(newData);
        }
    }

    private void NormalRead(ReadOnlySpan<u8> newData, Action<ReadOnlySpan<u8>> handler)
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
                ProcessData(newData.Slice(lengthBytesRead, _nextMessageLength), _nextMessageLength, handler);
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

    private void ProcessData(ReadOnlySpan<u8> slice, i32 messageLength, Action<ReadOnlySpan<u8>> handler)
    {
        if (messageLength == 0)
        {
            return;
        }

        Debug.Assert(slice.Length == messageLength);
        // Handle the complete message in 'slice'
        handler.Invoke(slice);
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
                var needed = BitPackingHelper.MaxVarLength - _partialLength;
                var toCopy = Math.Min(needed, data.Length);
                data[..toCopy].CopyTo(_partialBuffer.AsSpan(_partialLength));
                _partialLength += toCopy;
                if (_partialLength < BitPackingHelper.MaxVarLength)
                {
                    lengthBytesRead = toCopy;
                    return false;
                }

                _nextMessageLength = (i32)_partialBuffer.AsSpan(0, _partialLength)
                    .ReadVarU64(out lengthBytesRead);
                lengthBytesRead -= toCopy;
                if (lengthBytesRead < 0)
                {
                    throw new UnreachableException(
                        "how did we end up here?, we where suppose to read length value if it is fully in the buffer");
                }

                _partialLength = 0;
                _bufferState = BufferState.EmptyBuffer;
                return true;
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
        var newBuffer = new u8[newSize];
        if (preserveLength > 0)
        {
            Array.Copy(_partialBuffer, newBuffer, preserveLength);
        }

        _partialBuffer = newBuffer;
    }
}