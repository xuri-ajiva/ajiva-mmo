using Client.Editor.EditorLayers.Widgets;
using Common.Analytics;
using Common.Systems;
using Hexa.NET.ImGui;

namespace Client.Editor.EditorLayers;

public class EditorLayer(IEnumerable<IEditorWidget> widgets)
    : IUiRenderLayer
{
    private readonly List<IEditorWidget> _widgets = widgets.ToList();
    private bool _enabled = true;
    private PerfDeltaHelper _updatePerfDelta;
    private PerfDeltaHelper _renderPerfDelta;

    public void Attach()
    {
    }

    public void Update(UpdateInfo info)
    {
        _updatePerfDelta.RecordPerfData(info);
    }

    public void Detach()
    {
    }

    public ref bool Enabled => ref _enabled;

    public void RenderUi(UpdateInfo info)
    {
        _renderPerfDelta.RecordPerfData(info);

        ImGui.BeginMainMenuBar();
        foreach (var iEditorWidget in _widgets)
        {
            iEditorWidget.RenderMenu();
        }

        ImGui.EndMainMenuBar();

        foreach (var iEditorWidget in _widgets)
        {
            iEditorWidget.Render();
        }


        if (ImGuiP.BeginViewportSideBar("##Stat", ImGui.GetMainViewport(), ImGuiDir.Down, ImGui.GetFrameHeight(),
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.MenuBar))
        {
            DrawStatusBar();

            ImGui.End();
        }
    }

    private f32 _widthCenter;
    private f32 _widthRight;


    private void LeftStatus()
    {
        ImGui.Text("Left Info");
        foreach (var widget in _widgets.OfType<IEditorWidgetStatusLeft>())
        {
            widget.RenderStatusLeft();
        }
    }

    private void CenterStatus()
    {
        ImGui.Text("Centered Title or Status");
        foreach (var widget in _widgets.OfType<IEditorWidgetStatusCenter>())
        {
            widget.RenderStatusCenter();
        }
    }

    private void RightStatus()
    {
        foreach (var widget in _widgets.OfType<IEditorWidgetStatusRight>())
        {
            widget.RenderStatusRight();
        }

        _updatePerfDelta.TryGetPerformanceData(out var p50, out var p90, out var p99);
        ImGui.Text($"TPS: {1.0f / p50:F1}");
        if (ImGui.IsItemHovered())
        {
            ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);
            ImGui.SetItemTooltip($"Update: P50={p50 * 1000:F2}ms, P90={p90 * 1000:F2}ms, P99={p99 * 1000:F2}ms");
        }

        _renderPerfDelta.TryGetPerformanceData(out p50, out p90, out p99);
        ImGui.Text($"FPS: {1.0f / p50:F1}");
        if (ImGui.IsItemHovered())
        {
            ImGui.SetNextWindowViewport(ImGui.GetMainViewport().ID);
            ImGui.SetItemTooltip($"Render: P50={p50 * 1000:F2}ms, P90={p90 * 1000:F2}ms, P99={p99 * 1000:F2}ms");
        }
    }

    private void DrawStatusBar()
    {
        if (!ImGui.BeginMenuBar()) return;
        
        // Total width available in the bar
        var totalWidth = ImGui.GetContentRegionAvail().X;
        var spacing = ImGui.GetStyle().ItemSpacing.X;

        ImGui.BeginGroup();
        LeftStatus();
        ImGui.EndGroup();

        var leftSectionEnd = ImGui.GetItemRectMax().X;

        // Formula: (Total / 2) - (Half of Center Width)
        var centerTargetX = (totalWidth / 2.0f) - (_widthCenter / 2.0f);

        // Safety: Don't let Center overlap Left
        if (centerTargetX < (leftSectionEnd + spacing))
            centerTargetX = leftSectionEnd + spacing;

        ImGui.SetCursorPosX(centerTargetX);

        ImGui.BeginGroup();
        CenterStatus();
        ImGui.EndGroup();

        _widthCenter = ImGui.GetItemRectSize().X;
        var centerSectionEnd = ImGui.GetItemRectMax().X;

        // Formula: Total - Right Width
        var rightTargetX = totalWidth - _widthRight;

        // Safety: Don't let Right overlap Center
        if (rightTargetX < (centerSectionEnd + spacing))
            rightTargetX = centerSectionEnd + spacing;

        ImGui.SetCursorPosX(rightTargetX);

        ImGui.BeginGroup();
        RightStatus();
        ImGui.EndGroup();

        _widthRight = ImGui.GetItemRectSize().X;

        ImGui.EndMenuBar();
    }
}