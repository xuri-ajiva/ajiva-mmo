namespace Common.Rpc;

[Flags]
public enum RpcSystemSideFlags : u8
{
    None = 0, // drop this
    ClientHasImple = 1,
    ServerHasImple = 2,
    Both = ClientHasImple | ServerHasImple,
}

public record struct SystemMappings(u32 Id, string SystemHash, RpcSystemSideFlags Flags);