using Interception;
using MemoryPack;

namespace Common.Networking.AjivaQuic.ReplaySystem;

[ExcludeFromInterception]
public class ReplayClient(NetworkPacketCapturer capturer) : IQClient
{
    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

    public IQConnection Connection => new DummyQConnection();

    public ValueTask ConnectAsync(Instrumentation instrumentation, string? ipEndpoint = null,
        CancellationToken token = default)
    {
        State = ConnectionState.Connected;
        capturer.StartReplay();
        return ValueTask.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        State = ConnectionState.Disconnected;
        capturer.StopReplay();
        return Task.CompletedTask;
    }

    public void HandlePackets(PacketHandleDelegate<IQConnection> handler)
    {
        MemoryPackReaderOptionalState? state = null;
        try
        {
            while (!capturer.ReplayCompleted && State == ConnectionState.Connected && capturer.IsReplaying)
            {
                state ??= MemoryPackReaderOptionalStatePool.Rent(null);
                if (capturer.TryGetNextPacket(out var packet))
                {
                    if (packet is null) continue;

                    var reader = new MemoryPackReader(packet.Data, state);
                    try
                    {
                        handler(packet.PacketType, ref reader, Connection);
                    }
                    finally
                    {
                        reader.Dispose();
                    }
                }
                else
                {
                    if (capturer.ReplayCompleted)
                        State = ConnectionState.Disconnected;
                    break;
                }
            }
        }
        finally
        {
            if (state is IDisposable disposable)
                disposable.Dispose();
        }
    }
}