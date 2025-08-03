using System.Numerics;
using Riptide;

namespace Common.Networking;

public static class MessageExtensions
{
    public static void AddVector3(this Message message, Vector3 value)
    {
        message.AddFloat(value.X);
        message.AddFloat(value.Y);
        message.AddFloat(value.Z);
    }

    public static Vector3 GetVector3(this Message message)
    {
        return new Vector3(message.GetFloat(), message.GetFloat(), message.GetFloat());
    }

    public static void AddQuaternion(this Message message, Quaternion value)
    {
        message.AddFloat(value.X);
        message.AddFloat(value.Y);
        message.AddFloat(value.Z);
        message.AddFloat(value.W);
    }

    public static Quaternion GetQuaternion(this Message message)
    {
        return new Quaternion(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
    }
}