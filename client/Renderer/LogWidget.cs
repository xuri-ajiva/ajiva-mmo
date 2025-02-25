using System.Numerics;
using System.Runtime.InteropServices;
using client.LayerSystem;
using ImGuiNET;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace client.Renderer;

public class LogWidget : ILogEventSink, IRenderLayer
{
    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        //Console.WriteLine(DateTimeOffset.Now.ToString() + " " + message);
        _messageList.Add(logEvent);
    }

    public IEnumerable<IVirtualView> ListVirtualViewTargets() => [];

    private bool _logOpen = true;
    private readonly bool[] _severityFilter = new bool[7];
    private readonly List<LogEvent> _messageList = new();

    private static readonly List<Vector4> Colors =
    [
        new(0.5f, 0.5f, 0.5f, 1.0f), //none = 0,
        new(1.0f, 0.0f, 0.0f, 1.0f), //fatal = 1,
        new(1.0f, 0.2f, 0.2f, 1.0f), //error = 2,
        new(1.0f, 1.0f, 0.0f, 1.0f), //warning = 3,
        new(1.0f, 1.0f, 1.0f, 1.0f), //info = 4,
        new(0.7f, 1.0f, 1.0f, 1.0f), //debug = 5,
        new(0.0f, 0.4f, 0.4f, 1.0f), //verbose = 6
    ];


    private readonly (LogEventLevel level, Vector4 color, bool filter)[] _levels;
    private readonly IFormatProvider? _formatProvider;

    public LogWidget(IFormatProvider? formatProvider = null)
    {
        _formatProvider = formatProvider;
        _levels =
        [
            (LogEventLevel.Verbose, new Vector4(0.7f, 1.0f, 1.0f, 1.0f), false),
            (LogEventLevel.Debug, new Vector4(0.7f, 1.0f, 1.0f, 1.0f), true),
            (LogEventLevel.Information, new Vector4(1.0f, 1.0f, 1.0f, 1.0f), true),
            (LogEventLevel.Warning, new Vector4(1.0f, 1.0f, 0.0f, 1.0f), true),
            (LogEventLevel.Error, new Vector4(1.0f, 0.2f, 0.2f, 1.0f), true),
            (LogEventLevel.Fatal, new Vector4(1.0f, 0.0f, 0.0f, 1.0f), true)
        ];
        if (Enum.GetValues<LogEventLevel>().Any(level => _levels[(i32)level].level != level))
        {
            throw new InvalidOperationException("Levels are not in order");
        }
    }

    public Task ExtraWindows(UpdateInfo info)
    {
        if (ImGui.Begin("Log", ref _logOpen))
        {
            RenderLog();
        }

        ImGui.End();

        return Task.CompletedTask;
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
        }

        for (var index = 0; index < _levels.Length; index++)
        {
            ref var keyValuePair = ref _levels[index];
            ImGui.Checkbox(keyValuePair.level.ToString(), ref keyValuePair.filter);
            ImGui.SameLine();
        }

        ImGui.Separator();

        if (ImGui.BeginChild("scrolling", new Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar))
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

            if (ImGui.BeginTable("log", 5,
                    ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable |
                    ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                    ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableSetupColumn("Severity", ImGuiTableColumnFlags.WidthFixed, 56);
                ImGui.TableSetupColumn("Time", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.DefaultSort,
                    91);
                ImGui.TableSetupColumn("Context", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.DefaultHide,
                    200);
                ImGui.TableSetupColumn("Message", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Exception", ImGuiTableColumnFlags.WidthFixed, 100);

                ImGui.TableHeadersRow();

                foreach (var record in _messageList)
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
                    ImGui.Text(record.Properties["SourceContext"]?.ToString() ?? "");
                    ImGui.TableNextColumn();
                    ImGui.TextWrapped(record.RenderMessage());
                    ImGui.TableNextColumn();
                    ImGui.TextWrapped(record.Exception?.ToString() ?? "");

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

    public Task Update(UpdateInfo info) => Task.CompletedTask;

    public Task Detach() => Task.CompletedTask;

    public bool Enabled { get; set; } = true;
}
