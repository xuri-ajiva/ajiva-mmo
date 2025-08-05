using System.Numerics;
using Common.Networking;
using Riptide;

namespace Common.Ecs;

public record struct TransformComponent() : IComponentData, IMessageSerializable
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3 Scale { get; set; } = Vector3.One;

    public void Serialize(Message message)
    {
        message.AddVector3(Position);
        message.AddQuaternion(Rotation);
        message.AddVector3(Scale);
    }

    public void Deserialize(Message message)
    {
        Position = message.GetVector3();
        Rotation = message.GetQuaternion();
        Scale = message.GetVector3();
    }
}
