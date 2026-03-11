using Client.Networking;
using Common.Systems;
using Hexa.NET.ImGui;
using Interception;

namespace Client.LayerSystem;

/// <summary>
/// UI layer for connection status and entity debugging
/// Now purely focused on UI - no network logic
/// </summary>
[ExcludeFromInterception]
public class ClientConnectionLayer(ClientNetworkManager networkManager) : IUiRenderLayer
{
    public void Attach()
    {
    }

    public void Update(UpdateInfo info)
    {
    }

    public void Detach()
    {
    }

    public ref bool Enabled => ref _enabled;

    private string _address = "127.0.0.1:7777";
    private bool _enabled = true;

    [ExcludeFromInterception]
    public void RenderUi(UpdateInfo info)
    {
        if (ImGui.Begin("Client Connection"))
        {
            // Connection status and controls
            ImGui.Text($"RoundTrip: {networkManager.LastRtt}, RemoteEndPoint: {networkManager.RemoteEndPoint}");
            ImGui.InputText("Address Gateway", ref _address, 100);
            if (ImGui.Button("Connect Gateway"))
            {
                Task.Run(() => networkManager.Gateway.ConnectGateway(_address));
            }

            if (ImGui.Button("Disconnect Gateway"))
            {
                Task.Run(networkManager.Gateway.DisconnectGateway);
            }

            ImGui.TextWrapped($"Gateway Server: {networkManager.Gateway.Client}");
            ImGui.Separator();
            ImGui.InputText("Address Game", ref _address, 100);
            if (networkManager.Gateway.Client.IsNotConnected && ImGui.Button("Connect Game"))
            {
                Task.Run(() => networkManager.ConnectGame(_address));
            }

            if (networkManager.GameServerEndpoints.Length == 0)
            {
                ImGui.Text("Waiting for available game servers from gateway...");
            }
            else
            {
                ImGui.Text("Available Game Servers: ");
                ImGui.SameLine();
                ImGui.Text(networkManager.GameServerEndpoints.Length.ToString());
                if (ImGui.Button("Connect To All"))
                {
                    Task.Run(networkManager.ConnectToAllGameServers);
                }

                foreach (var serverEndpoint in networkManager.GameServerEndpoints)
                {
                    if (ImGui.Button($"Connect to {serverEndpoint}"))
                    {
                        Task.Run(() => networkManager.ConnectGame(serverEndpoint));
                    }
                }
            }

            if (networkManager.IsConnected && ImGui.Button("Disconnect Game"))
            {
                Task.Run(networkManager.DisconnectGame);
            }

            ImGui.TextWrapped($"Game Server: {networkManager}");
        }

        ImGui.End();
    }
}