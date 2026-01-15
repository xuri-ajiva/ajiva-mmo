using System.Diagnostics;
using Interception;
using NetworkPackets;

namespace Common.Networking.AjivaQuic.ReplaySystem;

[ExcludeFromInterception]
public class NetworkPacketCapturer
{
    private readonly List<CapturedPacket> _capturedPackets = [];
    private i64 _startTime;
    public IReadOnlyList<CapturedPacket> CapturedPackets => _capturedPackets;

    public void StartCapture()
    {
        _capturedPackets.Clear();
        _startTime = Stopwatch.GetTimestamp();
    }


    public void AddPacket(PacketType packetType, scoped ref MemoryPack.MemoryPackReader reader)
    {
        var delta = Stopwatch.GetElapsedTime(_startTime);

        var buffer = new u8[reader.Remaining];
        reader.GetRemainingSource(out var single, out var remaining);
        if (single.IsEmpty)
        {
            var offset = 0;
            foreach (var segment in remaining)
            {
                segment.CopyTo(new Memory<u8>(buffer, offset, segment.Length));
                offset += segment.Length;
            }
        }
        else
        {
            single.CopyTo(buffer);
        }

        _capturedPackets.Add(new CapturedPacket(delta, packetType, buffer));
    }

    public void StartReplay()
    {
        IsReplaying = true;
        _startTime = Stopwatch.GetTimestamp();
    }

    public bool IsReplaying { get; private set; } = false;
    public bool IsCapturing => !IsReplaying && _capturedPackets.Count > 0;

    public void StopReplay()
    {
        IsReplaying = false;
        ReplayIndex = 0;
    }

    public bool ReplayCompleted => ReplayIndex >= _capturedPackets.Count;

    public int ReplayIndex { get; private set; } = 0;

    public bool TryGetNextPacket(out CapturedPacket? packet)
    {
        if (!IsReplaying || ReplayIndex >= _capturedPackets.Count)
        {
            packet = null;
            return false;
        }

        var elapsed = Stopwatch.GetElapsedTime(_startTime);

        var nextPacket = _capturedPackets[ReplayIndex];
        if (elapsed >= nextPacket.Delta || nextPacket.Delta == TimeSpan.Zero || ReplayIndex == 0)
        {
            packet = nextPacket;
            ReplayIndex++;
            return true;
        }
        else
        {
            packet = null;
            return false;
        }
    }

    public async ValueTask SaveToFileAsync(string filePath, CancellationToken token = default)
    {
        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

        var headerCopy = CapturedPacket.Header;
        var header = new u8[headerCopy.Length + 4];
        headerCopy.CopyTo(header);
        BitConverter.TryWriteBytes(header.AsSpan(CapturedPacket.Header.Length), _capturedPackets.Count);
        await fs.WriteAsync(header, token);

        var metaBuffer = new u8[8 + 4 + 4];
        foreach (var capturedPacket in _capturedPackets)
        {
            var deltaTicks = capturedPacket.Delta.Ticks;
            var packetType = (i32)capturedPacket.PacketType;
            var dataLength = capturedPacket.Data.Length;

            BitConverter.TryWriteBytes(metaBuffer.AsSpan(0, 8), deltaTicks);
            BitConverter.TryWriteBytes(metaBuffer.AsSpan(8, 4), packetType);
            BitConverter.TryWriteBytes(metaBuffer.AsSpan(12, 4), dataLength);
            await fs.WriteAsync(metaBuffer, token);
            await fs.WriteAsync(capturedPacket.Data, token);
        }

        await fs.FlushAsync(token);
        fs.Close();
    }

    public async ValueTask LoadFromFileAsync(string filePath, CancellationToken token = default)
    {
        await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var headerBuffer = new u8[CapturedPacket.Header.Length + 4];
        await fs.ReadExactlyAsync(headerBuffer, token);
        var headerSpan = headerBuffer.AsSpan();
        if (!headerSpan.Slice(0, CapturedPacket.Header.Length).SequenceEqual(CapturedPacket.Header))
            throw new InvalidDataException("Invalid file header");

        var packetCount = BitConverter.ToInt32(headerSpan.Slice(CapturedPacket.Header.Length));

        var metaBuffer = new u8[8 + 4 + 4];
        for (var i = 0; i < packetCount; i++)
        {
            await fs.ReadExactlyAsync(metaBuffer, token);
            var deltaTicks = BitConverter.ToInt64(metaBuffer.AsSpan(0, 8));
            var packetType = (PacketType)BitConverter.ToInt32(metaBuffer.AsSpan(8, 4));
            var dataLength = BitConverter.ToInt32(metaBuffer.AsSpan(12, 4));

            var dataBuffer = new u8[dataLength];
            await fs.ReadExactlyAsync(dataBuffer, token);

            _capturedPackets.Add(new CapturedPacket(TimeSpan.FromTicks(deltaTicks), packetType, dataBuffer));
        }

        fs.Close();
    }

    public bool TryPeekNextPacket(out CapturedPacket nextPacket)
    {
        if (!IsReplaying || ReplayIndex >= _capturedPackets.Count)
        {
            nextPacket = null!;
            return false;
        }

        nextPacket = _capturedPackets[ReplayIndex];
        return true;
    }
}