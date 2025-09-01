using System.Diagnostics;
using System.Net.Quic;

namespace Common.Networking;

public static class QConnectionEstablisher
{
    public static async Task<QConnection> Client(QuicConnection connection, Instrumentation instrumentation,
        CancellationToken token)
    {
        var primary = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, token);
        await HandShakeSend(token, primary);
        HandShakeReceive(primary);

        var secondary = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, token);
        await HandShakeSend(token, secondary);
        HandShakeReceive(secondary);

        return new QConnection(connection, new QStream(primary, instrumentation),
            new QStream(secondary, instrumentation), instrumentation,
            CancellationTokenSource.CreateLinkedTokenSource(token));
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

    public static async Task<QConnection> Server(QuicConnection connection, Instrumentation instrumentation,
        CancellationToken token)
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

            return new QConnection(connection, new QStream(primary, instrumentation),
                new QStream(secondary, instrumentation), instrumentation,
                CancellationTokenSource.CreateLinkedTokenSource(token));
        }
        finally
        {
            Debug.Assert(true);
        }
    }
}