using Common.Assets;
using Common.Ecs;
using Server;
using System.Numerics;
using Polly;

namespace Server.Management;

public class GameWorldWrapper(NServerEcs ecs) : IGameWorld
{
    public EntityId CreateEntity() => ecs.CreateEntity();

    public void SetComponent<T>(EntityId id, T component) where T : struct, IComponent
    {
        switch (component)
        {
            case Position p: ecs.Position.Add(id, p); break;
            case Rotation r: ecs.Rotation.Add(id, r); break;
            case Scale s: ecs.Scale.Add(id, s); break;
            case Velocity v: ecs.Velocity.Add(id, v); break;
            case EntityType et: ecs.EntityType.Add(id, et); break;
            case NetState ns: ecs.NetState.Add(id, ns); break;
            case SpacialData sd: ecs.Spacial.Add(id, sd); break;
            case EntityOwnership eo: ecs.EntityOwnership.Add(id, eo); break;
            default:
            {
                throw new NotImplementedException($"Component of type {typeof(T)} is not supported in GameWorldWrapper.");
            }
        }
    }
}