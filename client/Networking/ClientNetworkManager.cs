using System.Numerics;
using Client.Game;
using Client.Renderer;
using Common.Ecs;
using Common.Networking;
using NetworkPackets;
using Riptide;

namespace Client.Networking;

/// <summary>
/// Clean network manager that handles all client-server communication
/// Separates networking concerns from UI and game logic
/// </summary>
public class ClientNetworkManager : BackgroundService, IUniversalMessageHandler, IDisposable
{
    private readonly ILogger<ClientNetworkManager> _logger;
    private readonly Riptide.Client _client;
    private readonly ClientGameLogic _gameLogic;

    public ClientNetworkManager(ILogger<ClientNetworkManager> logger, Riptide.Client client, ClientGameLogic gameLogic)
    {
        _logger = logger;
        _client = client;
        _gameLogic = gameLogic;

        _client.MessageReceived += OnMessageReceived;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Update();
            try
            {
                await Task.Delay(5, cancellationToken: stoppingToken); // ~200 FPS update rate
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        this.HandleMessage(e.Message!);
    }

    public void Handle(EntityCreatedPacket message)
    {
        _gameLogic.HandleEntityCreated(message);
    }

    public void Handle(EntityUpdatePacket message)
    {
        _gameLogic.HandleEntityUpdate(message);
    }

    public void Handle(EntityDestroyedPacket message)
    {
        _gameLogic.HandleEntityDestroyed(message);
    }

    public void Handle(BeginWorldStatePacket message)
    {
        _logger.LogInformation("Beginning to receive world state for {entityCount} entities.", message.EntityCount);
        _gameLogic.HandleBeginWorldState(message);
    }

    public void Handle(LoadLevelPacket message)
    {
        _logger.LogInformation("Loading level: {levelName}", message.LevelName);
        _gameLogic.LoadLevel(message.LevelName);
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


    public void Connect(string address)
    {
        if (_client.IsNotConnected)
        {
            _client.Connect(address);
        }
    }

    public void Disconnect()
    {
        if (!_client.IsNotConnected)
        {
            _client.Disconnect();
        }
    }

    public void Update()
    {
        if (_client.IsNotConnected) return;

        try
        {
            _client.Update();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in Client Update!");
        }
    }

    public bool IsConnected => _client.IsConnected;
    public bool IsNotConnected => _client.IsNotConnected;
    public ConnectionState? State => _client.State;
    public Connection? Connection => _client.Connection;

    /// <summary>
    /// Send entity update to server (called by player input system)
    /// </summary>
    public void SendEntityUpdate(u32 networkEntityId, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (!IsConnected) return;

        var transform = new Transform
        {
            Position = position,
            Rotation = rotation,
            Scale = scale
        };

        new EntityUpdatePacket(networkEntityId, transform)
            .Send(_client.Connection!, MessageSendMode.Unreliable);
    }

    public override void Dispose()
    {
        _client.MessageReceived -= OnMessageReceived;
        Disconnect();
        base.Dispose();
    }
}
