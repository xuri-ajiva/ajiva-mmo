using System.Diagnostics;
using System.Net.Quic;

namespace Common.Networking.AjivaQuic;

public static class QConnectionEstablisher
{
    public static async Task<QConnection> Client(QuicConnection connection, Instrumentation instrumentation,
        CancellationToken token)
    {
        var primary = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, token);
        await HandShakeSend(token, primary);
        HandShakeReceive(primary);

        /*var secondary = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, token);
        await HandShakeSend(token, secondary);
        HandShakeReceive(secondary);*/
        List<QStream> streams = [];
        var qConnection = new QConnection(connection, streams, instrumentation,
            CancellationTokenSource.CreateLinkedTokenSource(token));
        streams.Add(new QStream(primary, qConnection, instrumentation));
        //streams.Add(new QStream(secondary, qConnection, instrumentation));
        return qConnection;
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

            /*var secondary = await connection.AcceptInboundStreamAsync(token);
            Debug.Assert(secondary.Type == QuicStreamType.Bidirectional);
            HandShakeReceive(secondary);
            await HandShakeSend(token, secondary);*/

            List<QStream> streams = [];
            var qConnection = new QConnection(connection, streams, instrumentation,
                CancellationTokenSource.CreateLinkedTokenSource(token));
            streams.Add(new QStream(primary, qConnection, instrumentation));
            //streams.Add(new QStream(secondary, qConnection, instrumentation));
            return qConnection;
        }
        finally
        {
            Debug.Assert(true);
        }
    }
}