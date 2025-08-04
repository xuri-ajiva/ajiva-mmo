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

    public void Handle(NewEntityPacket message)
    {
        var entity = new Entity((int)message.Id);
        _gameLogic.GetWorld().AddComponent(entity, new TransformComponent { Position = new Vector3(message.X, message.Y, message.Z) });
        // For now, we'll hardcode TypeId 0 to MeshId 0 (first mesh in Sponza)
        _gameLogic.GetWorld().AddComponent(entity, new RenderableMeshComponent { MeshId = message.TypeId });
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
        _scene.LoadLevel(message.LevelName);
    }
}
