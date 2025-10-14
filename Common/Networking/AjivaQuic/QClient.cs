using System.Net;
using System.Net.Quic;
using Interception;
using Microsoft.Extensions.Logging;
using NetworkPackets;

namespace Common.Networking.AjivaQuic;

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
    private CancellationTokenSource? _cts;

    public QClient(QuicClientConnectionOptions options, ILogger<QClient> logger)
    {
        _options = options;
        _logger = logger;
    }

    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
    public bool IsConnected => State == ConnectionState.Connected;
    public bool IsNotConnected => !IsConnected;
    public QConnection? Connection { get; private set; }

    [ExcludeFromInterception]
    public async ValueTask ConnectAsync(Instrumentation instrumentation, string? ipEndpoint = null,
        CancellationToken token = default)
    {
        if (State != ConnectionState.Disconnected) return;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        if (ipEndpoint is not null)
            _options.RemoteEndPoint =
                IPEndPoint.Parse(ipEndpoint.TrimStart("http://").TrimStart("https://").TrimStart("udp://"));

        _logger.LogInformation("Connecting to server at {EndPoint}", _options.RemoteEndPoint);
        try
        {
            State = ConnectionState.Connecting;
            var connection = await QuicConnection.ConnectAsync(_options, _cts.Token);
            Connection = await QConnectionEstablisher.Client(connection, instrumentation, _cts.Token);
            Connection?.StartLoops();

            State = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            State = ConnectionState.Disconnected;
            _logger.LogError(e, "Failed to connect to server at {EndPoint}", _options.RemoteEndPoint);
            throw;
        }
    }

    public void HandlePackets(PacketHandleDelegate<QConnection> handler)
    {
        if (!IsConnected) return;
        Connection?.HandlePackets(handler);
    }

    public async Task DisconnectAsync()
    {
        if (State != ConnectionState.Connected || Connection is null)
            State = ConnectionState.Disconnecting;
        _logger.LogInformation("Disconnecting from server at {EndPoint}", _options.RemoteEndPoint);
        if (_cts is not null)
            await _cts.CancelAsync();
        if (Connection is not null)
            await Connection.DisposeAsync();
        State = ConnectionState.Disconnected;
    }
}