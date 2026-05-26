namespace Common.Networking.Rpc.Models;

public readonly record struct RpcSystemHash(string Value);
public readonly record struct RpcSystemRequirement(RpcSystemHash SystemHash, bool Required);
