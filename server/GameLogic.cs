using System.Numerics;
using Common.Ecs;
using Common.Networking;
using Riptide;

namespace server;

public class GameLogic
{
    private readonly World _world = new();
    private readonly Server _server;
    private Entity _rotatingEntity;
    private string _levelName = "Sponza";

    public GameLogic(Server server)
    {
        _server = server;
        LoadLevel();
    }

    private void LoadLevel()
    {
        _world.Clear();
        // Create a rotating box
        _rotatingEntity = _world.CreateEntity();
        _world.AddComponent(_rotatingEntity, new TransformComponent
        {
            Position = new Vector3(0, 1, 5),
        });
        _world.AddComponent(_rotatingEntity, new EntityTypeComponent { TypeId = 1 }); // Type 1 for a box

        // Create some static test entities
        for (var i = 0; i < 10; i++)
        {
            var entity = _world.CreateEntity();
            _world.AddComponent(entity, new TransformComponent
            {
                Position = new Vector3(i * 2, i, i * 3),
            });
            _world.AddComponent(entity, new EntityTypeComponent { TypeId = 1 }); // Type 0 for static
        }
    }

    public void CreatePlayerEntity(Connection connection)
    {
        var entity = _world.CreateEntity();
        _world.AddComponent(entity, new TransformComponent
        {
            Position = new Vector3(0, 1, 0)
        });
        _world.AddComponent(entity, new EntityTypeComponent { TypeId = 2 }); // Type 2 for player

        var transform = _world.GetComponent<TransformComponent>(entity);
        var entityType = _world.GetComponent<EntityTypeComponent>(entity);

        new EntityCreatedPacket(entity.Id, transform, entityType.TypeId)
            .SendToAll(_server, MessageSendMode.Reliable);
        new EntityAssignedPacket(entity.Id).Send(connection, MessageSendMode.Reliable);
    }

    public void Update(float deltaTime)
    {
        // Rotate the entity
        ref var transform = ref _world.GetComponent<TransformComponent>(_rotatingEntity);
        transform.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, deltaTime);
        transform.Rotation = Quaternion.Normalize(transform.Rotation);

        // Broadcast the update
        new EntityUpdatePacket(_rotatingEntity.Id, transform)
            .SendToAll(_server, MessageSendMode.Unreliable);
    }

    public void SendWorldState(Connection connection)
    {
        new LoadLevelPacket(_levelName).Send(connection, MessageSendMode.Reliable);

        var transformStore = _world.GetStore<TransformComponent>();
        var entityTypeStore = _world.GetStore<EntityTypeComponent>();
        var transforms = transformStore.GetAll();
        new BeginWorldStatePacket((u32)transforms.Length)
            .Send(connection, MessageSendMode.Reliable);

        for (u32 i = 0; i < transforms.Length; i++)
        {
            var entityId = transformStore.GetEntityIdFromDenseIndex(i);
            var transform = transforms[(i32)i];
            var entityType = entityTypeStore.Get(_world.GetEntity(entityId));
            new EntityCreatedPacket(entityId, transform, entityType.TypeId)
                .Send(connection, MessageSendMode.Reliable);
        }
    }

    public void HandleEntityUpdate(EntityUpdatePacket message)
    {
        var entity = _world.GetEntity(message.EntityId);
        if (!_world.HasComponent<TransformComponent>(entity)) return;

        _world.GetComponent<TransformComponent>(entity) = message.Transform;
        message.SendToAll(_server, MessageSendMode.Unreliable);
    }
}
