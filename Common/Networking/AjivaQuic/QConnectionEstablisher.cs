namespace Common.Networking.AjivaQuic;

public static class QConnectionEstablisher
{
    /*public static async Task<QConnection> Client(QuicConnection connection, IRpcContext context,
        Instrumentation instrumentation, CancellationToken token)
    {
        var qConnection = new QConnection(connection, instrumentation,
            context.StreamHandler, CancellationTokenSource.CreateLinkedTokenSource(token));
        qConnection.AcceptStreams();
        await context.BindConnectionAsync(qConnection);
        return qConnection;
    }


    public static async Task<QConnection> Server(QuicConnection connection, IRpcContext context,
        Instrumentation instrumentation, CancellationToken token)
    {
        var qConnection = new QConnection(connection, instrumentation,
            context.StreamHandler, CancellationTokenSource.CreateLinkedTokenSource(token));
        qConnection.AcceptStreams();
        await context.BindConnectionAsync(qConnection);
        return qConnection;
    }*/
}