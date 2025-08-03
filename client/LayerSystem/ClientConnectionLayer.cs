using ImGuiNET;
using Riptide;
using ServiceDefaults;

namespace Client.LayerSystem;

public class ClientConnectionLayer(Riptide.Client client) : IUiRenderLayer
{
    public void Attach() {}


    public void Update(UpdateInfo info)
    {
        if (!client.IsConnected) return;

        /*new UpdateEntityPacket(client.Id, ecs.LocalPos.X, ecs.LocalPos.Y, ecs.LocalPos.Z).Send(
            client.Connection!, MessageSendMode.Unreliable);*/
    }

    public void Detach() {}

    public ref bool Enabled => ref _enabled;

    private string _address = "127.0.0.1:7777";
    private bool _enabled = true;

    public void RenderUi(UpdateInfo info)
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

        /*foreach (var player in ecs.GetPlayers())
        {
            ImGui.Text($"Player {player.InstanceId}: {player.Position}");
        }*/

        ImGui.End();
    }
}
