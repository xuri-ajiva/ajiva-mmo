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
            _world.AddComponent(entity, new EntityTypeComponent { TypeId = 0 });
        }
    }

    public void SendWorldState(Connection connection)
    {
        new LoadLevelPacket("Sponza").Send(connection, MessageSendMode.Reliable);

        var transformStore = _world.GetStore<TransformComponent>();
        var entityTypeStore = _world.GetStore<EntityTypeComponent>();
        var transforms = transformStore.GetAll();
        new BeginWorldStatePacket(transforms.Length)
            .Send(connection, MessageSendMode.Reliable);

        for (var i = 0; i < transforms.Length; i++)
        {
            var entityId = transformStore.GetEntityIdFromDenseIndex(i);
            var transform = transforms[i];
            var entityType = entityTypeStore.Get(_world.GetEntity(entityId));
            new EntityCreatedPacket(entityId, transform, entityType.TypeId)
                .Send(connection, MessageSendMode.Reliable);
        }
    }
}
