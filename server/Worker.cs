using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using NetTopologySuite.Geometries;
using NetworkPackets;
using Riptide;
using ServiceDefaults;

namespace server;

public class Worker : IHostedService, IUniversalMessageHandler
{
    private Task? _serverUpdateTask;
    private CancellationTokenSource? _stoppingCts;

    private readonly ILogger<Worker> _logger;
    private readonly Server _server;
    private readonly ServerConfiguration _configuration;

    ConcurrentBag<Entitys> _entitys = new();

    public Worker(ILogger<Worker> logger, Server server, ServerConfiguration configuration)
    {
        _logger = logger;
        _server = server;
        _configuration = configuration;
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
        while (!cancellationToken.IsCancellationRequested)
        {
            _server.Update();
            await Task.Delay(5, cancellationToken);
            foreach (var entity in _entitys.ToArray())
            {
                _server.SendToAll(
                    new UpdateEntityPacket(entity.Id, entity.Position.X, entity.Position.Y, entity.Position.Z)
                        .ToMessage(MessageSendMode.Unreliable));
            }
        }
    }

    private void ServerOnConnectionFailed(object? sender, ServerConnectionFailedEventArgs e)
    {
        _logger.LogCritical("Server connection failed: {@Client}", e.Client);
    }

    private void ServerOnClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
    {
        _logger.LogInformation("Client disconnected: {@Client}", e.Client);
    }

    private void ServerOnClientConnected(object? sender, ServerConnectedEventArgs e)
    {
        _logger.LogInformation("Client connected: {@Client}", e.Client);
        _entitys.Add(new Entitys
        {
            Id = e.Client.Id, // todo use a better id system
            Position = new Vector3(0, 0, 0),
        });
        foreach (var entity in _entitys.ToArray())
        {
            new NewEntityPacket(entity.Id, 0u, entity.Position.X, entity.Position.Y, entity.Position.Z)
                .Send(e.Client, MessageSendMode.Reliable);
        }
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

    public void Handle(UpdateEntityPacket message)
    {
        var entity = _entitys.FirstOrDefault(e => e.Id == message.Id);
        if (entity == null)
            return;

        entity.Position = new Vector3(message.X, message.Y, message.Z);
    }
}

internal class Entitys
{
    public Vector3 Position { get; set; }
    public u32 Id { get; set; }
}

public class ServerConfiguration
{
    public ushort Port { get; set; }
    public ushort MaxConnections { get; set; }
}
