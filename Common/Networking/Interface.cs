using System;
using System.Net;
using Common.Networking.AjivaQuic;
using Interception;
using NetworkPackets;

namespace Common.Networking;

[ExcludeFromInterception]
public delegate void PacketHandleDelegate<in TUserData>(PacketType packetType,
    scoped ref MemoryPack.MemoryPackReader reader, TUserData userData);

public interface IQConnection : IAsyncDisposable
{
    void Send(SerializedMessage message, u8 streamId = 0);
    void Start();
    void HandlePackets(PacketHandleDelegate<IQConnection> handler);
    void Close(SerializedMessage msg);
    IPEndPoint RemoteEndPoint { get; }
    IPEndPoint LocalEndPoint { get; }
}

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
    IQConnection? Connection { get; }
    ValueTask ConnectAsync(Instrumentation instrumentation, string? ipEndpoint = null,
        CancellationToken token = default);
    void HandlePackets(PacketHandleDelegate<IQConnection> handler);
    Task DisconnectAsync();
}
