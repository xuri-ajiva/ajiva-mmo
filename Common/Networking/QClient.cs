using System.Net;
using System.Net.Quic;
using Interception;
using Microsoft.Extensions.Logging;
using NetworkPackets;

namespace Common.Networking;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Disconnecting
}

public class QClient
{
    private readonly QuicClientConnectionOptions _options;
    private readonly ILogger<QClient> _logger;
    private QConnection? _connection;
    private readonly CancellationTokenSource _cts = new();

    public QClient(QuicClientConnectionOptions options, ILogger<QClient> logger)
    {
        _options = options;
        _logger = logger;
    }

    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
    public bool IsConnected => State == ConnectionState.Connected;
    public bool IsNotConnected => !IsConnected;
    public QConnection? Connection => _connection;

    [ExcludeFromInterception]
    public async ValueTask ConnectAsync(IUniversalMessageHandler<QConnection> handler, string? ipEndpoint = null,
        CancellationToken token = default)
    {
        if (ipEndpoint is not null)
            _options.RemoteEndPoint =
                IPEndPoint.Parse(ipEndpoint.TrimStart("http://").TrimStart("https://").TrimStart("udp://"));

        _logger.LogInformation("Connecting to server at {EndPoint}", _options.RemoteEndPoint);
        try
        {
            State = ConnectionState.Connecting;
            var connection = await QuicConnection.ConnectAsync(_options, _cts.Token);
            _connection = await QConnectionEstablisher.Client(connection, _cts.Token);
            _connection?.Update(handler);

            State = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            State = ConnectionState.Disconnected;
            _logger.LogError(e, "Failed to connect to server at {EndPoint}", _options.RemoteEndPoint);
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (State != ConnectionState.Connected || _connection is null)
            State = ConnectionState.Disconnecting;
        _logger.LogInformation("Disconnecting from server at {EndPoint}", _options.RemoteEndPoint);
        await _connection.Connection.DisposeAsync();
        State = ConnectionState.Disconnected;
    }
}
