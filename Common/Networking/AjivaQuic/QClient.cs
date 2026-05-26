using System.Net;
using System.Net.Quic;
using Common.Networking.Rpc;
using Common.Rpc;
using Interception;
using Microsoft.Extensions.Logging;

namespace Common.Networking.AjivaQuic;

public abstract class QClientBase(RpcSystemRegistry registry, ILogger logger, QuicConnection connection)
    : IRpcContext
{
    protected QuicConnection Connection = connection;
    private readonly Dictionary<Type, object> _proxies = [];
    public ConnectionState State { get; protected set; } = ConnectionState.Disconnected;
    public bool IsConnected => State == ConnectionState.Connected;
    public bool IsNotConnected => !IsConnected;
    public abstract IStreamHandler StreamHandler { get; }
    public ILogger Logger => logger;

    public TProxy GetProxy<TProxy>() where TProxy : IRpcSystem
    {
        if (_proxies.TryGetValue(typeof(TProxy), out var proxy))
        {
            return (TProxy)proxy;
        }

        var proxyT = registry.CreateProxy<TProxy>(this);
        _proxies[typeof(TProxy)] = proxyT;
        return proxyT;
    }
}

public class QClient(QuicClientConnectionOptions options, RpcSystemRegistry registry, ILogger<QClient> logger)
    : QClientBase(registry, logger, null!), IQClient
{
    private CancellationTokenSource? _cts;
    private readonly QConnectionStreamHandler _streamHandler = new(registry, logger);
    public override IStreamHandler StreamHandler => _streamHandler;

    [ExcludeFromInterception]
    public async ValueTask ConnectAsync(Instrumentation instrumentation, string? ipEndpoint = null,
        CancellationToken token = default)
    {
        if (State != ConnectionState.Disconnected) return;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        if (ipEndpoint is not null)
            options.RemoteEndPoint =
                IPEndPoint.Parse(ipEndpoint.Replace("localhost", "127.0.0.1")
                    .TrimStart("http://").TrimStart("https://").TrimStart("udp://"));

        Logger.LogInformation("Connecting to server at {EndPoint}", options.RemoteEndPoint);
        try
        {
            State = ConnectionState.Connecting;
            Connection = await QuicConnection.ConnectAsync(options, _cts.Token);
            await _streamHandler.Init(Connection, instrumentation, true, _cts.Token);
            State = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            State = ConnectionState.Disconnected;
            Logger.LogError(e, "Failed to connect to server at {EndPoint}", options.RemoteEndPoint);
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (State != ConnectionState.Connected || Connection is null)
            State = ConnectionState.Disconnecting;
        Logger.LogInformation("Disconnecting from server at {EndPoint}", options.RemoteEndPoint);
        if (_cts is not null)
            await _cts.CancelAsync();
        if (Connection is not null)
            await Connection.DisposeAsync();
        State = ConnectionState.Disconnected;
    }

    public override string ToString()
    {
        return $"{nameof(State)}: {State}, {nameof(IsConnected)}: {IsConnected}, {nameof(Connection)}: {Connection}";
    }

    public void HandlePacketsAsync() => _streamHandler.PollContinuously(this);
}