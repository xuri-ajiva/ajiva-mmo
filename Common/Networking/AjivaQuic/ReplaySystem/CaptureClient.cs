using System.Diagnostics;
using Interception;

namespace Common.Networking.AjivaQuic.ReplaySystem;

[ExcludeFromInterception]
public class CaptureClient(IQClient qClient, string fileName = "network_replay.aqrep") : IQClient
{
    private readonly string _file = fileName;
    private readonly IQClient _qClient = qClient;
    private NetworkPacketCapturer _capturer = new();

    public ConnectionState State => _qClient.State;


    public async ValueTask ConnectAsync(Instrumentation instrumentation, string? ipEndpoint = null, CancellationToken token = default)
    {
        await _qClient.ConnectAsync(instrumentation, ipEndpoint, token);
        _capturer.StartCapture();
    }

    public async Task DisconnectAsync()
    {
        await _capturer.SaveToFileAsync(_file);
        await _qClient.DisconnectAsync();
    }

    public void HandlePackets()
    {
        Debugger.Break();
        
        /*_qClient.HandlePackets((packetType, ref reader, connection) =>
        {
            _capturer.AddPacket(packetType, ref reader);
            handler(packetType, ref reader, connection);
        });*/
    }
}