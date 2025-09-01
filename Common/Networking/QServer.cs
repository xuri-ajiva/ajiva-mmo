using System.Collections.Concurrent;
using System.Net;
using System.Net.Quic;
using Interception;
using Microsoft.Extensions.Logging;
using NetworkPackets;


namespace Common.Networking;

public interface QServerEventListener
{
    void OnClientConnected(QConnection connection);
    void OnClientDisconnected(QConnection connection);
    void OnConnectionFailed(IPEndPoint? connection, Exception exception);
}

public class QServer(
    QuicListenerOptions options,
    ILogger<QServer> logger,
    Instrumentation instrumentation) : IAsyncDisposable, IQServer
{
    private QuicListener? _server;
    private readonly CancellationTokenSource _cts = new();
    private readonly ConcurrentBag<QConnection> _connections = [];
    private Task? _listenTask;

    public void SendToAll(SerializedMessage message)
    {
        foreach (var connection in _connections)
        {
            try
            {
                connection.Send(message);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error sending message to client {Connection}", connection);
            }
        }
    }

    [ExcludeFromInterception]
    public void Start(IUniversalMessageHandler<QConnection> handler, QServerEventListener eventListener)
    {
        _listenTask = RunListener(handler, eventListener);
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        if (_listenTask is not null)
            await _listenTask;
        if (_server is not null)
            await _server.DisposeAsync();
        _cts.Dispose();
    }

    private async Task RunListener(IUniversalMessageHandler<QConnection> handler, QServerEventListener? eventListener)
    {
        try
        {
            _server = await QuicListener.ListenAsync(options, cancellationToken: _cts.Token);
            logger.LogInformation("Server started and listening for connections in {EntryPoint}",
                options.ListenEndPoint);
            IPEndPoint? endPoint = null;
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var connection = await _server.AcceptConnectionAsync(_cts.Token);
                    endPoint = connection.RemoteEndPoint;
                    logger.LogInformation("Connection accepted {Connection}", connection);
                    var qConnection = await QConnectionEstablisher.Server(connection, instrumentation, _cts.Token);
                    qConnection.StartLoops(handler, instrumentation);
                    _connections.Add(qConnection);
                    eventListener?.OnClientConnected(qConnection);
                }
                catch (Exception e)
                {
                    eventListener?.OnConnectionFailed(endPoint, e);
                    logger.LogError(e, "Error accepting connection");
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in listener");
        }
    }
}