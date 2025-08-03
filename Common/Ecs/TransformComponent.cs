using System.Numerics;
using Common.Networking;
using Riptide;

namespace Common.Ecs;

public readonly record struct TransformComponent : IComponentData, IMessageSerializable
{
    public Vector3 Position { get; init; }
    public Quaternion Rotation { get; init; }
    public Vector3 Scale { get; init; }

    public void Serialize(Message message)
    {
        message.AddVector3(Position);
        message.AddQuaternion(Rotation);
        message.AddVector3(Scale);
    }

    public void Deserialize(Message message)
    {
        // This is intentionally left empty. We will use the static method below.
    }

    public static TransformComponent FromMessage(Message message)
    {
        return new TransformComponent
        {
            Position = message.GetVector3(),
            Rotation = message.GetQuaternion(),
            Scale = message.GetVector3()
        };
    }
}