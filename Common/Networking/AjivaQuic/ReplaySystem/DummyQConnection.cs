using System.Net;
using Common.Helpers;
using Interception;
using NetworkPackets;

namespace Common.Networking.AjivaQuic.ReplaySystem;

[ExcludeFromInterception]
public class DummyQConnection : IQConnection
{
    private static readonly IPEndPoint IpEndPoint = new(IPAddress.Loopback, 0);
    public IPEndPoint RemoteEndPoint => IpEndPoint;

    public IPEndPoint LocalEndPoint => IpEndPoint;

    public void Close(SerializedMessage msg) => Ignore.Value(msg);

    public ValueTask DisposeAsync() => Ignore.ForEverAsync();

    public void HandlePackets(PacketHandleDelegate<IQConnection> handler) => Ignore.Value(handler);

    public void Send(SerializedMessage message, u8 streamId = 0) => Ignore.Value(message);

    public void Start() => Ignore.ForEver();
}