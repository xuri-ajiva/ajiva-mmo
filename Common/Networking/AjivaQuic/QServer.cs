using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using Common.Networking.Rpc;
using Common.Rpc;
using Interception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Common.Networking.AjivaQuic;

public interface QServerEventListener
{
    void OnClientConnected(QuicListener server, QServerConnection connection);
    void OnClientDisconnected(QServerConnection connection);
    void OnConnectionFailed(QuicListener server, IPEndPoint? connection, Exception exception);
}

public class QServer(QuicListenerOptions options, IServiceProvider provider, ILogger<QServer> logger)
    : IAsyncDisposable, IQServer
{
    private QuicListener? _server;
    private readonly CancellationTokenSource _cts = new();
    private readonly List<QServerConnection> _connections = [];
    private Task? _listenTask;
    public IPEndPoint? LocalEndPoint => _server?.LocalEndPoint;
    private readonly TaskCompletionSource _readyHandle = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private QServerEventListener? _eventListener;

    private bool _asyncPacketHandling = true;

    public void SendToAll(SerializedMessage message)
    {
        foreach (var connection in _connections)
        {
            try
            {
                Debugger.Break();

                //connection.Send(message);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error sending message to client {Connection}", connection);
            }
        }
    }

    [ExcludeFromInterception]
    public void Start(QServerEventListener eventListener)
    {
        _eventListener = eventListener;
        _listenTask = RunListener();
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

    public void HandlePacketsAsync()
    {
        _asyncPacketHandling = true;
        foreach (var connection in _connections)
        {
            connection.StreamHandler.PollContinuously(connection);
        }
    }

    public void HandlePackets()
    {
        foreach (var connection in _connections)
        {
            connection.StreamHandler.Poll(connection);
            //       connection.HandlePackets(handler);
        }
    }

    private async Task RunListener()
    {
        try
        {
            _server = await QuicListener.ListenAsync(options, cancellationToken: _cts.Token);
            logger.LogInformation("Server started and listening for connections in {EntryPoint}",
                options.ListenEndPoint);
            _readyHandle.SetResult();
            IPEndPoint? endPoint = null;
            var registry = provider.GetRequiredService<RpcSystemRegistry>();
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var scope = provider.CreateScope();
                    var connection = await _server.AcceptConnectionAsync(_cts.Token);
                    endPoint = connection.RemoteEndPoint;
                    logger.LogInformation("Connection accepted {Connection}", connection);
                    var qConnection = new QServerConnection(connection, registry, logger, scope);
                    logger.LogInformation("Initializing connection {Connection}", connection);
                    await qConnection.Init(this, _cts.Token);
                    if (_asyncPacketHandling)
                    {
                        qConnection.StreamHandler.PollContinuously(qConnection);
                        logger.LogInformation("Setup packet handling for connection {Connection}", connection);
                    }

                    _connections.Add(qConnection);
                    logger.LogInformation("Connection {Connection} started", connection);
                    _eventListener?.OnClientConnected(_server, qConnection);
                }
                catch (Exception e)
                {
                    _eventListener?.OnConnectionFailed(_server, endPoint, e);
                    logger.LogError(e, "Error accepting connection");
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in listener");
        }
    }

    public void RemoveConnection(QServerConnection connection)
    {
        _eventListener?.OnClientDisconnected(connection);
        if (_connections.Remove(connection))
        {
            logger.LogInformation("Connection {Connection} removed", connection);
        }
        else
        {
            logger.LogInformation("Connection {Connection} not found", connection);
        }

        connection.Dispose();
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    public void SetEndpoint(IPEndPoint endPoint)
    {
        options.ListenEndPoint = endPoint;
    }

    public IEnumerable<QServerConnection> GetConnections() => _connections;

    public async Task WaitForReady()
    {
        await _readyHandle.Task;
    }

    public async Task WaitForShutdown()
    {
        while (!_cts.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken: _cts.Token)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }

        if (_listenTask is not null)
            await _listenTask;
    }
}

public class QServerConnection(
    QuicConnection connection,
    RpcSystemRegistry registry,
    ILogger logger,
    IServiceScope scope)
    : QClientBase(registry, logger, connection), IDisposable
{
    private readonly QConnectionStreamHandler _streamHandler = new(registry, logger);
    public override IStreamHandler StreamHandler => _streamHandler;
    private CancellationTokenSource? _cts;
    private CancellationTokenRegistration _registration;

    public async ValueTask Init(QServer qServer, CancellationToken token)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _registration = _cts.Token.Register(() => qServer.RemoveConnection(this));
        State = ConnectionState.Connecting;
        await _streamHandler.Init(Connection!, null, false, _cts);
        State = ConnectionState.Connected;
    }

    public void Dispose()
    {
        _registration.Unregister();
        _registration.Dispose();
        _cts?.Dispose();
        scope.Dispose();
    }
}