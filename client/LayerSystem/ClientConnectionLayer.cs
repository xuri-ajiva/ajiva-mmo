using ImGuiNET;
using Riptide;

namespace client.LayerSystem;

public class ClientConnectionLayer(Client client) : IUiRenderLayer
{
    public Task Attach() => Task.CompletedTask;

    public Task Update(UpdateInfo info) => Task.CompletedTask;

    public Task Detach() => Task.CompletedTask;

    public bool Enabled { get; set; } = true;

    private string _address = "127.0.0.1:7777";

    public Task RenderUi(UpdateInfo info)
    {
        ImGui.Begin("Client Connection");
        ImGui.Text($"Status: {client.State?.ToString()}");
        if (client.IsNotConnected)
        {
            ImGui.InputText("Address", ref _address, 100);
            if (ImGui.Button("Connect"))
            {
                client.Connect(_address);
            }
        }
        else
        {
            if (ImGui.Button("Disconnect"))
            {
                client.Disconnect();
            }
        }

        ImGui.End();
        return Task.CompletedTask;
    }
}
