using Common.Ecs;
using System.Numerics;
using client;
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
    private readonly ObjectScene _objectScene;

    public ClientWorker(ILogger<ClientWorker> logger, Riptide.Client client, GameLogic gameLogic, ObjectScene objectScene)
    {
        _logger = logger;
        _client = client;
        _gameLogic = gameLogic;
        _objectScene = objectScene;
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
            _objectScene.UnloadLevel();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in ClientWorker! Exiting...");
        }
    }

    public void Handle(NewEntityPacket message)
    {
        _gameLogic.GetWorld().AddComponent(new Entity((int)message.Id), new TransformComponent { Position = new Vector3(message.X, message.Y, message.Z) });
    }

    public void Handle(UpdateEntityPacket message)
    {
        ref var transform = ref _gameLogic.GetWorld().GetComponent<TransformComponent>(new Entity((int)message.Id));
        transform = transform with { Position = new Vector3(message.X, message.Y, message.Z) };
    }

    public void Handle(BeginWorldStatePacket message)
    {
        _logger.LogInformation("Beginning to receive world state for {entityCount} entities.", message.EntityCount);
    }

    public void Handle(LoadLevelPacket message)
    {
        _logger.LogInformation("Loading level: {levelName}", message.LevelName);
        _objectScene.LoadLevel(message.LevelName);
    }
}
