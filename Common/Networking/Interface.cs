using System;
using System.Net;
using Common.Networking.AjivaQuic;
using Interception;
using NetworkPackets;

namespace Common.Networking;

[ExcludeFromInterception]
public delegate void PacketHandleDelegate<in TUserData>(i32 packetType,
    scoped ref MemoryPack.MemoryPackReader reader, TUserData userData);


public interface IQServer
{
    void SendToAll(SerializedMessage message);
}
public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Disconnecting
}

public interface IQClient
{
    ConnectionState State { get; }
    bool IsConnected => State == ConnectionState.Connected;
    bool IsNotConnected => State != ConnectionState.Connected;
    ValueTask ConnectAsync(Instrumentation instrumentation, string? ipEndpoint = null,
        CancellationToken token = default);
    Task DisconnectAsync();
}
