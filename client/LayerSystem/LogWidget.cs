using System.Collections.Concurrent;
using System.Numerics;
using ImGuiNET;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Client.LayerSystem;

public class LogWidget : ILogEventSink, IUiRenderLayer
{
    public void Emit(LogEvent logEvent)
    {
        logEvent.AddPropertyIfAbsent(new LogEventProperty("SourceContext", new ScalarValue("unknown")));
        _messageList.Add(logEvent);
    }

    private bool _logOpen = true;
    private readonly ConcurrentBag<LogEvent> _messageList = [];

    private readonly (LogEventLevel level, Vector4 color, bool filter)[] _levels;
    private readonly IFormatProvider? _formatProvider;
    private bool _enabled = true;

    public LogWidget(IFormatProvider? formatProvider = null)
    {
        _formatProvider = formatProvider;
        _levels = new (LogEventLevel level, Vector4 color, bool filter)[Enum.GetValues<LogEventLevel>().Length];
        _levels[(i32)LogEventLevel.Verbose] = (LogEventLevel.Verbose, new Vector4(0.7f, 1.0f, 1.0f, 1.0f), false);
        _levels[(i32)LogEventLevel.Debug] = (LogEventLevel.Debug, new Vector4(0.7f, 1.0f, 1.0f, 1.0f), true);
        _levels[(i32)LogEventLevel.Information] =
            (LogEventLevel.Information, new Vector4(1.0f, 1.0f, 1.0f, 1.0f), true);
        _levels[(i32)LogEventLevel.Warning] = (LogEventLevel.Warning, new Vector4(1.0f, 1.0f, 0.0f, 1.0f), true);
        _levels[(i32)LogEventLevel.Error] = (LogEventLevel.Error, new Vector4(1.0f, 0.2f, 0.2f, 1.0f), true);
        _levels[(i32)LogEventLevel.Fatal] = (LogEventLevel.Fatal, new Vector4(1.0f, 0.2f, 0.2f, 1.0f), true);
    }

    public void RenderUi(UpdateInfo info)
    {
        if (ImGui.Begin("Log", ref _logOpen))
        {
            RenderLog();
        }

        ImGui.End();
    }


    private void RenderLog()
    {
        var clear = ImGui.Button("Clear");
        ImGui.SameLine();
        var copy = ImGui.Button("Copy");
        ImGui.SameLine();
        if (ImGui.Button("Example"))
        {
            Log.Verbose("This is a VERBOSE message");
            Log.Debug("This is a DEBUG message");
            Log.Information("This is an INFO message");
            Log.Warning("This is a WARNING message");
            Log.Error("This is an ERROR message");
            Log.Fatal("This is a FATAL message");
            Log.Error(new Exception("This is an exception"), "This is an exception message");
        }

        for (var index = 0; index < _levels.Length; index++)
        {
            ref var keyValuePair = ref _levels[index];
            ImGui.Checkbox(keyValuePair.level.ToString(), ref keyValuePair.filter);
            ImGui.SameLine();
        }

        ImGui.Separator();

        if (ImGui.BeginChild("scrolling", new Vector2(0, 0), ImGuiChildFlags.None,
                ImGuiWindowFlags.HorizontalScrollbar))
        {
            if (clear)
            {
                _messageList.Clear();
            }

            if (copy)
            {
                ImGui.LogToClipboard();
            }

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

            if (ImGui.BeginTable("log", 4,
                    ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable |
                    ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                    ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableSetupColumn("Severity", ImGuiTableColumnFlags.WidthFixed, 56);
                ImGui.TableSetupColumn("Time", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.DefaultSort,
                    91);
                ImGui.TableSetupColumn("Context", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.DefaultHide,
                    200);
                ImGui.TableSetupColumn("Message or Exception", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                foreach (var record in _messageList.OrderBy(x => x.Timestamp))
                {
                    var severity = record.Level;
                    ref var level = ref _levels[(i32)severity];
                    if (level.filter == false) // _levels[(int)severity] is guaranteed by the constructor to be in order
                    {
                        continue;
                    }

                    ImGui.TableNextRow();

                    if (severity == LogEventLevel.Fatal)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.GetColorU32(level.color));
                    }
                    else
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, level.color);
                    }

                    ImGui.TableNextColumn();
                    ImGui.Text(severity.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(record.Timestamp.ToString("HH:mm:ss.fff"));
                    ImGui.TableNextColumn();
                    ImGui.Text(record.Properties["SourceContext"].ToString());
                    ImGui.TableNextColumn();
                    ImGui.TextWrapped(record.Exception is not null
                        ? (record.RenderMessage() + " : " + record.Exception)
                        : record.RenderMessage());

                    ImGui.PopStyleColor();
                    ImGui.TableNextColumn();
                }

                ImGui.EndTable();
            }

            ImGui.PopStyleVar();

            if (copy)
            {
                ImGui.LogFinish();
            }

            ImGui.EndChild();
        }
    }

    public Task Attach() => Task.CompletedTask;

    public void Update(UpdateInfo info)
    {
    }

    public Task Detach() => Task.CompletedTask;

    public ref bool Enabled => ref _enabled;
}
