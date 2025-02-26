using NetworkPackets;
using Riptide;
using ServiceDefaults;

namespace client.Networking;

public class ClientWorker : BackgroundService, IUniversalMessageHandler
{
    private readonly ILogger<ClientWorker> _logger;
    private readonly Client _client;

    public ClientWorker(ILogger<ClientWorker> logger, Client client)
    {
        _logger = logger;
        _client = client;
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
                    _client.Update();
                }

                try
                {
                    await Task.Delay(1, stoppingToken);
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
        _logger.LogInformation("Received new entity: {@Message}", message);
    }

    public void Handle(UpdateEntityPacket message)
    {
        _logger.LogInformation("Received update entity: {@Message}", message);
    }
}
