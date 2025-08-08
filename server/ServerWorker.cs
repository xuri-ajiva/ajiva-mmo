using Common.Networking;
using NetworkPackets;
using Riptide;

namespace Server;

public class ServerWorker : IHostedService, IUniversalMessageHandler
{

    private Task? _serverUpdateTask;
    private CancellationTokenSource? _stoppingCts;

    private readonly ILogger<ServerWorker> _logger;
    private readonly Riptide.Server _server;
    private readonly ServerConfiguration _configuration;
    private readonly ServerGameLogic _serverGameLogic;

    public ServerWorker(ILogger<ServerWorker> logger, Riptide.Server server, ServerConfiguration configuration)
    {
        _logger = logger;
        _server = server;
        _configuration = configuration;
        _serverGameLogic = new ServerGameLogic(_server);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _server.MessageReceived += ServerOnMessageReceived;
        _server.ClientConnected += ServerOnClientConnected;
        _server.ClientDisconnected += ServerOnClientDisconnected;
        _server.ConnectionFailed += ServerOnConnectionFailed;
        _server.Start(_configuration.Port, _configuration.MaxConnections);
        _serverUpdateTask = UpdateAsync(_stoppingCts.Token);
        return _serverUpdateTask.IsCompleted ? _serverUpdateTask : Task.CompletedTask;
    }

    private async Task UpdateAsync(CancellationToken cancellationToken)
    {
        var lastUpdate = DateTime.Now;
        while (!cancellationToken.IsCancellationRequested)
        {
            _server.Update();
            var now = DateTime.Now;
            var delta = (float)(now - lastUpdate).TotalSeconds;
            _serverGameLogic.Update(delta);
            lastUpdate = now;
            await Task.Delay(5, cancellationToken);
        }
    }

    private void ServerOnConnectionFailed(object? sender, ServerConnectionFailedEventArgs e)
    {
        _logger.LogCritical("Server connection failed: {@Client}", e.Client);
    }

    private void ServerOnClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
    {
        _logger.LogInformation("Client disconnected: {@Client}", e.Client);
        _serverGameLogic.RemovePlayer(e.Client);
    }

    private void ServerOnClientConnected(object? sender, ServerConnectedEventArgs e)
    {
        _logger.LogInformation("Client connected: {@Client}", e.Client);
        _serverGameLogic.SendWorldState(e.Client);
        _serverGameLogic.CreatePlayerEntity(e.Client);
    }

    private void ServerOnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        this.HandleMessage(e.Message!);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _server.Stop();

        if (_serverUpdateTask == null)
            return;

        try
        {
            // Signal cancellation to the executing method
            await _stoppingCts!.CancelAsync();
        }
        finally
        {
            await _serverUpdateTask.WaitAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    public void Handle(EntityUpdatePacket message)
    {
        _serverGameLogic.HandleEntityUpdate(message);
    }

    public void MessageNotHandled(string messageType)
    {
        _logger.LogWarning("Message not handled: {MessageType}", messageType);
    }
}

public class ServerConfiguration
{
    public ushort Port { get; set; }
    public ushort MaxConnections { get; set; }
}
