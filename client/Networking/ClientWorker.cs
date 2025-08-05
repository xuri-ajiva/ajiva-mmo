using Common.Ecs;
using System.Numerics;
using Client.Renderer;
using Common.Networking;
using NetworkPackets;
using Riptide;
using ServiceDefaults;

namespace Client.Networking;

public class ClientWorker : BackgroundService, IUniversalMessageHandler
{
    private readonly ILogger<ClientWorker> _logger;
    private readonly Riptide.Client _client;
    private readonly GameLogic _gameLogic;
    private readonly Scene _scene;

    public ClientWorker(ILogger<ClientWorker> logger, Riptide.Client client, GameLogic gameLogic, Scene scene)
    {
        _logger = logger;
        _client = client;
        _gameLogic = gameLogic;
        _scene = scene;
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
            _scene.UnloadLevel();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in ClientWorker! Exiting...");
        }
    }

    public void Handle(EntityCreatedPacket message)
    {
        _gameLogic.HandleEntityCreated(message);
    }

    public void Handle(EntityUpdatePacket message)
    {
        _gameLogic.HandleEntityUpdate(message);
    }

    public void Handle(BeginWorldStatePacket message)
    {
        _logger.LogInformation("Beginning to receive world state for {entityCount} entities.", message.EntityCount);
        _gameLogic.HandleBeginWorldState(message);
    }

    public void Handle(LoadLevelPacket message)
    {
        _logger.LogInformation("Loading level: {levelName}", message.LevelName);
        _scene.LoadLevel(message.LevelName);
    }

    public void Handle(EntityAssignedPacket message)
    {
        _gameLogic.SetLocalPlayerEntityId(message.EntityId);
        _logger.LogInformation("Assigned local player entity ID: {EntityId}", message.EntityId);
    }

    public void MessageNotHandled(string messageType)
    {
        _logger.LogWarning("Message not handled: {MessageType}", messageType);
    }
    
}
