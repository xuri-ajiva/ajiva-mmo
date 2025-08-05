using Common.Ecs;
using Common.Networking;

namespace Client;

public class GameLogic(Riptide.Client client, World world)
{
    private readonly Riptide.Client _client = client;
    public u32? LocalPlayerEntityId { get; private set; }

    public World GetWorld() => world;

    public void HandleEntityCreated(EntityCreatedPacket message)
    {
        var entity = world.GetEntity(message.EntityId);
        world.AddComponent(entity, message.Transform);
        world.AddComponent(entity, new EntityTypeComponent { TypeId = message.TypeId });

        // We will render the rotating box and the player with the first mesh.
        if (message.TypeId is 1 or 2)
        {
            world.AddComponent(entity, new RenderableMeshComponent { MeshId = 0, EntityId = (u32)entity.Id });
        }
    }

    public void HandleEntityUpdate(EntityUpdatePacket message)
    {
        var entity = world.GetEntity(message.EntityId);
        world.GetComponent<TransformComponent>(entity) = message.Transform;
    }

    public void HandleBeginWorldState(BeginWorldStatePacket message)
    {
        world.Clear();
    }

    public void SetLocalPlayerEntityId(u32 messageEntityId)
    {
        LocalPlayerEntityId = messageEntityId;
        var entity = world.GetEntity(messageEntityId);
        world.AddComponent(entity, new TransformComponent
        {
            Position = new System.Numerics.Vector3(0, 1, 0)
        });
        world.AddComponent(entity, new EntityTypeComponent { TypeId = 2 }); // Type 2 for player
        world.AddComponent(entity, new RenderableMeshComponent { MeshId = 0, EntityId = (u32)entity.Id });
    }
}
