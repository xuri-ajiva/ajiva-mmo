using System.Diagnostics;
using System.Net.Quic;
using Interception;
using NetworkPackets;

namespace Common.Networking;

public class QStream(QuicStream stream, i32 bufferSize = 64 * 1024)
{
    private readonly MessageDecoder _decoder = new();
    private readonly ResizableRingBuffer _writeBuffer = new(bufferSize);
    private readonly u8[] _readBuffer = new u8[bufferSize];

    public async ValueTask ReadMessages(Action<ReadOnlySpan<u8>> handler)
    {
        //handler.HandleMessage();
        var bytesRead = await stream.ReadAsync(_readBuffer);
        _decoder.DecodeMessages(_readBuffer.AsSpan(0, bytesRead), handler);
    }

    public async ValueTask FlushAsync(CancellationToken token)
    {
        if (_writeBuffer.DataAvailable() == 0) return;

        await stream.WriteAsync(_writeBuffer.ConsumePart(), token);
        if (_writeBuffer.DataAvailable() > 0)
        {
            await stream.WriteAsync(_writeBuffer.ConsumePart(), token);
        }

        await stream.FlushAsync(token);
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

    public void WriteData(Span<byte> memorySpan)
    {
        _writeBuffer.Write(memorySpan);
    }
}

public record QConnection(
    QuicConnection Connection,
    QStream Primary,
    QStream Secondary,
    CancellationToken Token) : IQConnection
{
    [ExcludeFromInterception]
    public void Update(IUniversalMessageHandler<QConnection> handler)
    {
        RunLoop(() => Primary.FlushAsync(Token));
        RunLoop(() => Secondary.FlushAsync(Token));
        RunLoop(() => Primary.ReadMessages(Wrap(handler)));
        RunLoop(() => Secondary.ReadMessages(Wrap(handler)));
    }

    private void RunLoop(Func<ValueTask> loopFunc)
    {
        _ = Task.Run(async () =>
        {
            while (!Token.IsCancellationRequested)
            {
                try
                {
                    await loopFunc();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                await Task.Delay(1, Token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }, Token);
    }

    private Action<ReadOnlySpan<u8>> Wrap(IUniversalMessageHandler<QConnection> handler) =>
        span => handler.AutoHandle(span, this);

    public void Send(SerializedMessage message, u8 streamId = 0)
    {
        using var @lock = Primary.EnterLock();
        Primary.WriteLength(message.Length);
        foreach (var memory in message)
        {
            Primary.WriteData(memory.Span);
        }
    }

    public void Send(SerializedMessage message)
    {
        Send(message, 0);
    }
}

public static class QConnectionEstablisher
{
    public static async Task<QConnection> Client(QuicConnection connection, CancellationToken token)
    {
        var primary = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, token);
        await HandShakeSend(token, primary);
        HandShakeReceive(primary);

        var secondary = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, token);
        await HandShakeSend(token, secondary);
        HandShakeReceive(secondary);

        return new QConnection(connection, new QStream(primary), new QStream(secondary), token);
    }


    private static async Task HandShakeSend(CancellationToken token, QuicStream primary)
    {
        primary.WriteByte(69);
        await primary.FlushAsync(token);
    }

    private static void HandShakeReceive(QuicStream primary)
    {
        var a = primary.ReadByte();
        Debug.Assert(a == 69);
    }

    public static async Task<QConnection> Server(QuicConnection connection, CancellationToken token)
    {
        try
        {
            var primary = await connection.AcceptInboundStreamAsync(token);
            Debug.Assert(primary.Type == QuicStreamType.Bidirectional);
            HandShakeReceive(primary);
            await HandShakeSend(token, primary);

            var secondary = await connection.AcceptInboundStreamAsync(token);
            Debug.Assert(secondary.Type == QuicStreamType.Bidirectional);
            HandShakeReceive(secondary);
            await HandShakeSend(token, secondary);

            return new QConnection(connection, new QStream(primary), new QStream(secondary), token);
        }
        finally
        {
            Debug.Assert(true);
        }
    }
}
