using System.Collections.Concurrent;
using System.Net;
using System.Net.Quic;
using Interception;
using Microsoft.Extensions.Logging;
using NetworkPackets;


namespace Common.Networking.AjivaQuic;

public interface QServerEventListener
{
    void OnClientConnected(QuicListener server, QConnection connection);
    void OnClientDisconnected(QConnection connection);
    void OnConnectionFailed(QuicListener server, IPEndPoint? connection, Exception exception);
}

public class QServer(QuicListenerOptions options, ILogger<QServer> logger) : IAsyncDisposable, IQServer
{
    private QuicListener? _server;
    private readonly CancellationTokenSource _cts = new();
    private readonly ConcurrentBag<QConnection> _connections = [];
    private Task? _listenTask;
    public IPEndPoint? LocalEndPoint => _server?.LocalEndPoint;
    private readonly TaskCompletionSource _readyHandle = new();

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
    public void Start(QServerEventListener eventListener, Instrumentation instrumentation)
    {
        _listenTask = RunListener(eventListener, instrumentation);
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

    public void HandlePackets(PacketHandleDelegate<QConnection> handler)
    {
        foreach (var connection in _connections)
        {
            connection.HandlePackets(handler);
        }
    }

    private async Task RunListener(QServerEventListener? eventListener, Instrumentation instrumentation)
    {
        try
        {
            _server = await QuicListener.ListenAsync(options, cancellationToken: _cts.Token);
            logger.LogInformation("Server started and listening for connections in {EntryPoint}",
                options.ListenEndPoint);
            _readyHandle.SetResult();
            IPEndPoint? endPoint = null;
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var connection = await _server.AcceptConnectionAsync(_cts.Token);
                    endPoint = connection.RemoteEndPoint;
                    logger.LogInformation("Connection accepted {Connection}", connection);
                    var qConnection = await QConnectionEstablisher.Server(connection, instrumentation, _cts.Token);
                    qConnection.StartLoops();
                    _connections.Add(qConnection);
                    eventListener?.OnClientConnected(_server, qConnection);
                }
                catch (Exception e)
                {
                    eventListener?.OnConnectionFailed(_server, endPoint, e);
                    logger.LogError(e, "Error accepting connection");
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in listener");
        }
    }

    public void SetEndpoint(IPEndPoint endPoint)
    {
        options.ListenEndPoint = endPoint;
    }

    public IEnumerable<QConnection> GetConnections() => _connections;

    public async Task WaitForReady()
    {
        await _readyHandle.Task;
    }
}