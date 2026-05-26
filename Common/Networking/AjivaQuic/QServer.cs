using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using Common.Networking.Rpc;
using Common.Rpc;
using Interception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkPackets;


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
    private readonly ConcurrentBag<(IServiceScope scope, QServerConnection qConnection)> _connections = [];
    private Task? _listenTask;
    public IPEndPoint? LocalEndPoint => _server?.LocalEndPoint;
    private readonly TaskCompletionSource _readyHandle = new(TaskCreationOptions.RunContinuationsAsynchronously);

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
        _listenTask = RunListener(eventListener);
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
            connection.qConnection.StreamHandler.PollContinuously(connection.qConnection);
        }
    }

    public void HandlePackets()
    {
        foreach (var connection in _connections)
        {
            connection.qConnection.StreamHandler.Poll(connection.qConnection);
            //       connection.HandlePackets(handler);
        }
    }

    private async Task RunListener(QServerEventListener? eventListener)
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
                    var qConnection = new QServerConnection(connection, registry, logger);
                    logger.LogInformation("Initializing connection {Connection}", connection);
                    await qConnection.Init(_cts.Token);
                    if (_asyncPacketHandling)
                    {
                        qConnection.StreamHandler.PollContinuously(qConnection);
                        logger.LogInformation("Setup packet handling for connection {Connection}", connection);
                    }

                    _connections.Add((scope, qConnection));
                    logger.LogInformation("Connection {Connection} started", connection);
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

    public void Stop()
    {
        _cts.Cancel();
    }

    public void SetEndpoint(IPEndPoint endPoint)
    {
        options.ListenEndPoint = endPoint;
    }

    public IEnumerable<QServerConnection> GetConnections() => _connections.Select(c => c.qConnection);

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

public class QServerConnection(QuicConnection connection, RpcSystemRegistry registry, ILogger logger)
    : QClientBase(registry, logger, connection)
{
    private readonly QConnectionStreamHandler _streamHandler = new(registry, logger);
    public override IStreamHandler StreamHandler => _streamHandler;

    public async ValueTask Init(CancellationToken token)
    {
        State = ConnectionState.Connecting;
        await _streamHandler.Init(Connection, null, false, token);
        State = ConnectionState.Connected;
    }
}