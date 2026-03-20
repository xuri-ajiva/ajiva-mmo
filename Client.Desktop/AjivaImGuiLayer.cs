using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Client.Common;
using Client.Desktop.Platform;
using Client.Renderer;
using Client.Renderer.Renderers;
using Common;
using Common.Systems;
using Hexa.NET.ImGui;
using Hexa.NET.SDL3;
using Silk.NET.Maths;

namespace Client.Desktop;

public class AjivaImGuiLayer : IAjivaLayer, IDisposable, IEventHandler<AjivaKeyboardEvent>,
    IEventHandler<AjivaMouseEvent>, IEventHandler<SDLEvent>
{
    private readonly ILogger<AjivaImGuiLayer> _logger;
    private readonly LayerStack _layerStack;

    private readonly RendererApi _api;
    private readonly PeriodicUpdater _periodicUpdater;

    private ImFontPtr _quicksandFont;
    private readonly IServiceProvider _serviceProvider;
    private readonly WindowingLayer _windowingLayer;
    private readonly ImGuiPlatform _imGuiPlatform;
    private readonly ISurfaceManager _surfaceManager;

    public AjivaImGuiLayer(ILogger<AjivaImGuiLayer> logger, LayerStack layerStack, RendererApi api,
        PeriodicUpdater periodicUpdater, IServiceProvider serviceProvider,
        WindowingLayer windowingLayer, ImGuiPlatform imGuiPlatform, ISurfaceManager surfaceManager)
    {
        _logger = logger;
        _layerStack = layerStack;
        _api = api;
        _periodicUpdater = periodicUpdater;
        _serviceProvider = serviceProvider;
        _windowingLayer = windowingLayer;
        _imGuiPlatform = imGuiPlatform;
        _surfaceManager = surfaceManager;
        // find all types that implement IAjivaLayer
#if DEBUG
        _layerTypes = [];
        /*serviceProvider.GetRequiredService<IServiceCollection>()
            .Select(x=> x.ServiceType)
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IAjivaLayer).IsAssignableFrom(t))
            .ToArray();*/
#else
        _layerTypes = [];
#endif
    }

    public unsafe void Attach()
    {
        _imGuiPlatform.Initialize(_windowingLayer.MainWindow);
        //_imguiDefaultFont = ImGui.GetIO().Fonts.AddFontDefault();
        var io = ImGui.GetIO();

        var fontPath = Path.GetFullPath("resources/Quicksand-SemiBold.ttf");

        //TODO IMG MIGRATION: var nativeConfig = ImGuiNative.ImFontConfig_ImFontConfig();
        //Add a higher horizontal/vertical sample rate for global scaling.
        //TODO IMG MIGRATION: nativeConfig->OversampleH = 8;
        //TODO IMG MIGRATION: nativeConfig->OversampleV = 8;
        //TODO IMG MIGRATION: nativeConfig->RasterizerMultiply = 1f;
        //TODO IMG MIGRATION: nativeConfig->GlyphOffset = new Vector2(0);
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
        _quicksandFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 18);
        // _imguiController = new ImGuiController(_api.Device, TextureFormat.Bgra8Unorm, _window.Window, SetupFonts);

        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
        /*io.ConfigFlags |= ImGuiConfigFlags.DockingEnable; // Enable Docking
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable; // Enable Multi-Viewport / Platform Windows*/
        _renderUpdateHandle = _periodicUpdater.RegisterUpdate(UpdateCycle.Render, Render);
        var style = ImGui.GetStyle();
        style.FrameRounding = 4;
        style.FrameBorderSize = 1;
        //io.ConfigDpiScaleFonts = true;
        //io.ConfigDpiScaleViewports = true;
        //TODO IMG MIGRATION: io.FontGlobalScale = 1.25f;

        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            style.WindowRounding = 0.0f;
            style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
        }
    }

    public void Update(UpdateInfo info)
    {
    }

    public void Detach()
    {
        _imGuiPlatform.Teardown();
        _renderUpdateHandle?.Dispose();
        _renderUpdateHandle = null;
    }

    private bool _enabled = true;
    public ref bool Enabled => ref _enabled;

    public void Render(UpdateInfo info)
    {
        if (!_layerStack.Attached) return;
        using var tracer = Instrumentation.Trace(ActivityKind.Client);

        var window = _windowingLayer.MainWindow;
        i32 width = 0, height = 0;
        SDL.GetWindowSize(window, ref width, ref height);
        using var guard = _api.EnterQueue();

        _imGuiPlatform.NewFrame(info, new Vector2D<i32>(width, height));

        try
        {
            ActualRender(info, guard.Queue);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Exception during rendering");
        }

        _imGuiPlatform.EndFrame(_windowingLayer.MainWindow);
    }


    private void ActualRender(UpdateInfo info, QueuePtr queue)
    {
        ImGui.PushFont(_quicksandFont, 0);
        ImGui.DockSpaceOverViewport(0, ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
        using var activity = Instrumentation.Instance?.StartActivity(ActivityKind.Client, name: "Queue");

        foreach (var layer in _layerStack.Layers)
        {
            using var layers =
                Instrumentation.Instance?.StartActivity(ActivityKind.Client, name: layer.GetType().ToString());
            if (!layer.Enabled) continue;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            using (Instrumentation.NamedTrace("rendering"))
                if (layer is IRenderLayer renderLayer)
                    renderLayer.Render(info, new RenderTarget(queue));

            using (Instrumentation.NamedTrace("ui rendering"))
                if (layer is IUiRenderLayer uiRenderLayer)
                    uiRenderLayer.RenderUi(info);
        }

        if (_showOverlay)
            RenderMetrics(info);
        if (_demoWindow)
            ImGui.ShowDemoWindow();

        RenderLayerControls();

        ImGui.PopFont();

        if (ImGui.GetFrameCount() == 2) //seems to be the magic number for loading dock layouts
            ImGui.LoadIniSettingsFromDisk("imgui.ini");
    }

    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH",
        MessageId = "type: System.Object")]
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH",
        MessageId = "type: System.Object; size: 54687MB")]
    private void RenderLayerControls()
    {
        if (ImGui.Begin("Layer Controls"))
        {
            ImGui.Checkbox("Show Overlay", ref _showOverlay);
            ImGui.Checkbox("Show Demo Window", ref _demoWindow);

            ImGui.Separator();
            ImGui.Columns(2);
            IAjivaLayer? layerToRemove = null;
            foreach (var layer in _layerStack.Layers)
            {
                ImGui.Checkbox(layer.GetType().Name, ref layer.Enabled);
                ImGui.NextColumn();
                if (ImGui.Button("X"))
                {
                    layerToRemove = layer;
                }

                ImGui.NextColumn();
            }

            ImGui.Columns(1);

            if (layerToRemove is not null)
            {
                _layerStack.Remove(layerToRemove);
            }

            ImGui.Separator();
            // combobox for adding or removing layers
            if (ImGui.BeginCombo("Add Layer", "Select Layer"))
            {
                foreach (var layerType in _layerTypes)
                {
                    if (_layerStack.Layers.Any(l => l.GetType() == layerType)) continue;

                    if (ImGui.Selectable(layerType.Name))
                    {
                        _layerStack.CreateLayer(layerType);
                    }
                }

                ImGui.EndCombo();
            }

            if (ImGui.Button("GC"))
            {
                GC.Collect();
            }

            if (GCStressTest is null)
            {
                if (ImGui.Button("GC Stress Test"))
                {
                    allocations = 0;
                    GCStressTest = new Thread(() =>
                    {
                        while (GCStressTest is not null)
                        {
                            Parallel.For(0, 10000,
                                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
                                {
                                    var array = new object[1000];
                                    for (var index = 0; index < array.Length; index++)
                                    {
                                        array[index] = (object[])[i, index];
                                    }

                                    Interlocked.Add(ref allocations, (u64)array.LongLength * 4);
                                });
                        }
                    }) { Name = "GC Testing Thread", IsBackground = true };
                    GCStressTest.Start();
                }
            }
            else
            {
                if (ImGui.Button("Stop GC Stress Test"))
                {
                    GCStressTest = null;
                }
            }

            ImGui.Text($"Allocations: {allocations:000 000 000 000}");
            // Allocation per sec:
            var allocDiff = allocations - allocationsLast;
            ImGui.Text($"Delta {allocDiff:000 000 000 000}");
            allocationsLast = allocations;
        }

        ImGui.End();
    }

    private u64 allocationsLast;
    private u64 allocations;
    private Thread? GCStressTest;


    private i32 _location = -1;
    private bool _showOverlay = true, _demoWindow;
    private readonly f32[] _frameTimes = new f32[100];
    private readonly TypeInfo[] _layerTypes;
    private PeriodicUpdater.UpdateHandle? _renderUpdateHandle;

    private void RenderMetrics(UpdateInfo info)
    {
        ImGuiWindowFlags windowFlags =
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav;
        const f32 pad = 10.0f;
        _frameTimes[info.Ticks % (u64)_frameTimes.Length] = Math.Max(0, info.DeltaTime * 1000);

        if (_location >= 0)
        {
            var viewport = ImGui.GetMainViewport();
            var workPos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
            var workSize = viewport.WorkSize;
            Vector2 windowPos, windowPosPivot;
            windowPos.X = (_location & 1) > 0 ? (workPos.X + workSize.X - pad) : (workPos.X + pad);
            windowPos.Y = (_location & 2) > 0 ? (workPos.Y + workSize.Y - pad) : (workPos.Y + pad);
            windowPosPivot.X = (_location & 1) > 0 ? 1.0f : 0.0f;
            windowPosPivot.Y = (_location & 2) > 0 ? 1.0f : 0.0f;
            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always, windowPosPivot);
            windowFlags |= ImGuiWindowFlags.NoMove;
        }
        else if (_location == -2)
        {
            // Center window
            //TODO IMG MIGRATION: ImGui.SetNextWindowPos(ImGui.GetMainViewport()., ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            windowFlags |= ImGuiWindowFlags.NoMove;
        }

        var io = ImGui.GetIO();
        ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background
        if (ImGui.Begin("Metrics overlay", ref _showOverlay, windowFlags))
        {
            ImGui.Text("Simple overlay\n(right-click to change position)");
            ImGui.Separator();
            if (ImGui.IsMousePosValid())
                ImGui.Text($"Mouse Position: ({io.MousePos.X:N0}, {io.MousePos.Y:N0})");
            else
                ImGui.Text("Mouse Position: <invalid>");
            ImGui.Text($"Application average {1000.0f / io.Framerate:N3} ms/frame ({io.Framerate:N1} FPS)");
            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.MenuItem("Custom", string.Empty, _location == -1)) _location = -1;
                if (ImGui.MenuItem("Center", string.Empty, _location == -2)) _location = -2;
                if (ImGui.MenuItem("Top-left", string.Empty, _location == 0)) _location = 0;
                if (ImGui.MenuItem("Top-right", string.Empty, _location == 1)) _location = 1;
                if (ImGui.MenuItem("Bottom-left", string.Empty, _location == 2)) _location = 2;
                if (ImGui.MenuItem("Bottom-right", string.Empty, _location == 3)) _location = 3;
                if (_showOverlay && ImGui.MenuItem("Close")) _showOverlay = false;
                ImGui.EndPopup();
            }

            // render small graphs with fps
            ImGui.PlotLines("##", ref _frameTimes[0], _frameTimes.Length, 0, "Frame Times", 0, 50,
                new Vector2(300, 80));
        }

        ImGui.End();
    }

    public void Dispose()
    {
        _renderUpdateHandle?.Dispose();
        GCStressTest = null;
    }

    public void OnEvent(ref AjivaEvent<AjivaKeyboardEvent> inputEvent)
    {
        //_imGuiPlatform.OnEvent(ref inputEvent);
    }

    public void OnEvent(ref AjivaEvent<AjivaMouseEvent> inputEvent)
    {
        //_imGuiPlatform.OnEvent(ref inputEvent);
    }

    public void OnEvent(ref AjivaEvent<SDLEvent> inputEvent)
    {
        _imGuiPlatform.OnEvent(ref inputEvent);
        if (ImGui.GetIO().WantCaptureKeyboard)
        {
            inputEvent.Handled = true;
        }

        if (ImGui.GetIO().WantCaptureMouse)
        {
            inputEvent.Handled = true;
        }
    }
}