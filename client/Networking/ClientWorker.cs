using System.Numerics;
using NetworkPackets;
using Riptide;
using ServiceDefaults;

namespace client.Networking;

public class ClientWorker : BackgroundService, IUniversalMessageHandler
{
    private readonly ILogger<ClientWorker> _logger;
    private readonly Client _client;
    private readonly Ecs _ecs;

    public ClientWorker(ILogger<ClientWorker> logger, Client client, Ecs ecs)
    {
        _logger = logger;
        _client = client;
        _ecs = ecs;
        _client.MessageReceived += ClientOnMessageReceived;
    }

    private void ClientOnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        // _logger.LogInformation("Received message: {@Message}", e.Message);
        this.HandleMessage(e.Message!);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_client.IsNotConnected)
                {
                    try
                    {
                        _client.Update();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error in Client Update!");
                    }
                }

                try
                {
                    await Task.Delay(5, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _client.Disconnect();
            _client.Update();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in ClientWorker! Exiting...");
        }
    }

    public void Handle(NewEntityPacket message)
    {
        _ecs.UpdatePlayer(message.Id, new Vector3(message.X, message.Y, message.Z));
    }

    public void Handle(UpdateEntityPacket message)
    {
        _ecs.UpdatePlayer(message.Id, new Vector3(message.X, message.Y, message.Z));
    }
}
