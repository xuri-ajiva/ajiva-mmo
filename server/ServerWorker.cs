using System.Diagnostics;
using Common;
using Common.Config;
using Common.Networking;
using NetworkPackets;
using OpenTelemetry.Trace;
using Riptide;

namespace Server;

public class ServerWorker : IHostedService, IUniversalMessageHandler
{
    private Task? _serverTickTask;
    private Task? _serverUpdateTask;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private CancellationTokenSource? _stoppingCts;

    private readonly ILogger<ServerWorker> _logger;
    private readonly Riptide.Server _server;
    private readonly Instrumentation _instrumentation;
    private readonly ServerConfiguration _configuration;
    private readonly ServerGameLogic _serverGameLogic;

    public ServerWorker(ILogger<ServerWorker> logger, Riptide.Server server, Instrumentation instrumentation,
        ServerConfiguration configuration, ServerGameLogic serverGameLogic)
    {
        _logger = logger;
        _server = server;
        _instrumentation = instrumentation;
        _configuration = configuration;
        _serverGameLogic = serverGameLogic;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _server.MessageReceived += ServerOnMessageReceived;
        _server.ClientConnected += ServerOnClientConnected;
        _server.ClientDisconnected += ServerOnClientDisconnected;
        _server.ConnectionFailed += ServerOnConnectionFailed;
        _server.Start(_configuration.Port, _configuration.MaxConnections);
        _serverTickTask = TickAsync(_stoppingCts.Token);
        _serverUpdateTask = UpdateAsync(_stoppingCts.Token);
        return _serverUpdateTask.IsCompleted ? _serverUpdateTask : Task.CompletedTask;
    }

    private async Task UpdateAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _semaphore.WaitAsync(token);
                _server.Update();
            }
            finally
            {
                _semaphore.Release();
            }

            await Task.Delay(10, token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        const f32 tps = 10;
        var lastUpdate = DateTime.Now;
        while (!cancellationToken.IsCancellationRequested)
        {
            using var activity = _instrumentation.ActivitySource.StartActivity(ActivityKind.Server);
            var now = DateTime.Now;
            var delta = (f32)(now - lastUpdate).TotalSeconds;
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                _serverGameLogic.Update(delta);
            }
            finally
            {
                _semaphore.Release();
            }

            lastUpdate = now;
            var sleepTime = 1f / tps - delta;
            await Task.Delay(Math.Max(1, (i32)(sleepTime * 1000)), cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
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

        if (_serverUpdateTask is null && _serverTickTask is null)
            return;

        try
        {
            // Signal cancellation to the executing method
            await _stoppingCts!.CancelAsync();
        }
        finally
        {
            if (_serverUpdateTask is not null)
                await _serverUpdateTask.WaitAsync(cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            if (_serverTickTask is not null)
                await _serverTickTask.WaitAsync(cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
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
