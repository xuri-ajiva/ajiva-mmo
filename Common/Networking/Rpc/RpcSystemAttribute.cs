namespace Common.Networking.Rpc;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class RpcSystemAttribute(string name, i32 version) : Attribute
{
    public string Name { get; } = name;
    public i32 Version { get; } = version;
}