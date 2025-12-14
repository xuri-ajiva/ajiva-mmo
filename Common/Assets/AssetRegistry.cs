using System.Numerics;
using Common.Ecs;
using Common.Networking;

namespace Common.Assets;

public interface IGameWorld
{
    EntityId CreateEntity();
    void SetComponent<T>(EntityId id, T component) where T : struct, IComponent;
}

public delegate EntityId EntityGenerator(IGameWorld world, Vector3 position, Quaternion rotation);

public class Prefab
{
    public required string Name { get; set; }
    public EntityGenerator Generator { get; set; }
}

public static class AssetRegistry
{
    public static readonly Dictionary<PrefabId, Prefab> Prefabs = [];

    static AssetRegistry()
    {
        Register("RotatingBox", (world, position, rotation) =>
        {
            var entity = world.CreateEntity();
            world.SetComponent(entity, new Position(position));
            world.SetComponent(entity, new Rotation { Value = rotation });
            world.SetComponent(entity, new Scale { Value = Vector3.One });
            world.SetComponent(entity, new Velocity { Angular = Vector3.UnitZ });
            world.SetComponent(entity, new EntityType { TypeId = 1 });
            return entity;
        });
    }
    
    public static PrefabId Register(string name, EntityGenerator generator)
    {
        var prefabId = new PrefabId((u32)Prefabs.Count);
        Prefabs[prefabId] = new Prefab { Name = name, Generator = generator };
        return prefabId;
    }
}