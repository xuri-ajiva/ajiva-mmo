using Serilog;

namespace Common.Networking.AjivaQuic;

public class ResizableRingBuffer
{
    private u8[] _buffer;
    private i32 _writePos = 0;
    private i32 _readPos = 0;
    private readonly Lock _lock = new();

    public ResizableRingBuffer(i32 initialSize = 64 * 1024)
    {
        _buffer = new u8[initialSize];
    }

    public i32 DataAvailable()
    {
        if (_writePos >= _readPos)
            return _writePos - _readPos;
        return _buffer.Length - _readPos + _writePos;
    }

    public i32 FreeSpace()
    {
        if (_writePos >= _readPos)
            return _buffer.Length - (_writePos - _readPos) - 1;
        return _readPos - _writePos - 1;
    }

    public void Write(ReadOnlySpan<u8> data)
    {
        if (data.Length == 0) return;

        if (!EnsureBufferSize(data.Length))
        {
            throw new OverflowException("Buffer exceeded maximum size");
        }

        lock (_lock)
        {
            if (_writePos + data.Length <= _buffer.Length)
            {
                data.CopyTo(_buffer.AsSpan(_writePos));
                _writePos += data.Length;
            }
            else
            {
                i32 firstPart = _buffer.Length - _writePos;
                data[..firstPart].CopyTo(_buffer.AsSpan(_writePos));
                data[firstPart..].CopyTo(_buffer.AsSpan(0));
                _writePos = data.Length - firstPart;
            }
        }
    }

    public Memory<u8> ConsumePart()
    {
        lock (_lock)
        {
            i32 available = DataAvailable();
            if (available == 0) return Memory<u8>.Empty;

            if (_readPos < _writePos)
            {
                var span = _buffer.AsMemory(_readPos, _writePos - _readPos);
                _readPos = _writePos;
                return span;
            }
            else
            {
                var span = _buffer.AsMemory(_readPos, _buffer.Length - _readPos);
                _readPos = 0;
                return span;
            }
        }
    }

    private bool EnsureBufferSize(i32 additionalRequired)
    {
        i32 available = FreeSpace();
        if (available >= additionalRequired)
            return true;
        
        // check size fo max of 20mb
        if (_buffer.Length > 20 * 1024 * 1024)
            Log.Warning("ResizableRingBuffer({buffer}) exceeded 20MB, current size: {Size} bytes", this, _buffer.Length);

        i32 used = DataAvailable();
        i32 newSize = _buffer.Length;
        while (newSize - used - 1 < additionalRequired)
            newSize *= 2;

        lock (_lock)
        {
            var newBuffer = new u8[newSize];

            if (_writePos >= _readPos)
            {
                Array.Copy(_buffer, _readPos, newBuffer, 0, used);
            }
            else
            {
                i32 firstPart = _buffer.Length - _readPos;
                Array.Copy(_buffer, _readPos, newBuffer, 0, firstPart);
                Array.Copy(_buffer, 0, newBuffer, firstPart, _writePos);
            }

            _buffer = newBuffer;
            _readPos = 0;
            _writePos = used;
        }

        return true;
    }

    public Lock.Scope EnterLock()
    {
        return _lock.EnterScope();
    }
}
