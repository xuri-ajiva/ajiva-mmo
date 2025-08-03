using Common.Ecs;
using Common.Networking;
using Riptide;

namespace server;

public class GameLogic
{
    private readonly World _world = new();
    private readonly Server _server;

    public GameLogic(Server server)
    {
        _server = server;

        // Create some test entities
        for (var i = 0; i < 10; i++)
        {
            var entity = _world.CreateEntity();
            _world.AddComponent(entity, new TransformComponent
            {
                Position = new System.Numerics.Vector3(i * 2, 0, 0)
            });
        }
    }

    public void SendWorldState(Connection connection)
    {
        new LoadLevelPacket("Sponza").Send(connection, MessageSendMode.Reliable);

        var store = _world.GetStore<TransformComponent>();
        var transforms = store.GetAll();
        new BeginWorldStatePacket(transforms.Length)
            .Send(connection, MessageSendMode.Reliable);

        for (var i = 0; i < transforms.Length; i++)
        {
            // This is not quite right, we need a way to get the entity ID from the dense index.
            // We will fix this in a later step.
            var entityId = i; // Placeholder
            var transform = transforms[i];
            new EntityCreatedPacket(entityId, transform)
                .Send(connection, MessageSendMode.Reliable);
        }
    }
}
