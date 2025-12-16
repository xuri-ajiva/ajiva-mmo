using Client.Editor.EditorLayers.Widgets;
using Client.Editor.Management;
using Client.Game;
using Client.LayerSystem;
using Client.Networking;
using ImGuiNET;

namespace Client.Editor.EditorLayers;

public class EditorLayer(IEnumerable<IEditorWidget> widgets)
    : IUiRenderLayer
{
    private readonly List<IEditorWidget> _widgets = widgets.ToList();
    private bool _enabled = true;

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

    public void RenderUi(UpdateInfo info)
    {
        ImGui.BeginMainMenuBar();
        foreach (var iEditorWidget in widgets)
        {
            iEditorWidget.RenderMenu();
        }
        ImGui.EndMainMenuBar();

        foreach (var iEditorWidget in widgets)
        {
            iEditorWidget.Render();
        }
    }
}