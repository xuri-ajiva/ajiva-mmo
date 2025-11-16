using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using Client.Platform;
using Client.Renderer;
using Common;
using Common.Systems;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.ImGui;
using Silk.NET.WebGPU.Safe;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;
using SurfaceConfiguration = Silk.NET.WebGPU.Safe.SurfaceConfiguration;

namespace Client.LayerSystem;

public class AjivaImGuiLayerSystem : IAjivaLayerSystem<AjivaWindow>, IDisposable
{
    private bool _initialized, _attatched;

    private readonly ILogger<AjivaImGuiLayerSystem> _logger;

    private readonly RendererApi _api;
    private readonly PeriodicUpdater _periodicUpdater;

    private ImFontPtr _quicksandFont;
    private ImGuiController? _imguiController;
    private AjivaWindow _window = null!;

    private List<IAjivaLayer> Layers { get; } = [];

    public AjivaImGuiLayerSystem(ILogger<AjivaImGuiLayerSystem> logger, RendererApi api,
        IServiceScopeFactory serviceProvider, PeriodicUpdater periodicUpdater)
    {
        _scope = serviceProvider.CreateScope();

        _logger = logger;
        _api = api;
        _periodicUpdater = periodicUpdater;
        // find alll types that implement IAjivaLayer
#if DEBUG
        _layerTypes =
            GetType().Assembly.DefinedTypes
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IAjivaLayer).IsAssignableFrom(t))
                .ToArray();
#else
        _layerTypes = [];
#endif
    }

    public void Attach(AjivaWindow parent)
    {
        if (_initialized) return;
        _initialized = true;
        _window = parent;
        var input = parent.Input;

        _imguiController = new ImGuiController(_api.Device, TextureFormat.Bgra8Unorm, parent.Window, input, SetupFonts);

        var io = ImGui.GetIO();

        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable; // Enable Docking
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable; // Enable Multi-Viewport / Platform Windows
        _renderUpdateHandle = _periodicUpdater.RegisterUpdate(UpdateCycle.Render, Render);
        _updateHandle = _periodicUpdater.RegisterUpdate(UpdateCycle.Update, Update);
        var style = ImGui.GetStyle();
        style.FrameRounding = 4;
        style.FrameBorderSize = 1;
        io.FontGlobalScale = 1.25f;

        foreach (var layer in Layers)
        {
            layer.Attach();
        }

        _attatched = true;
    }

    unsafe void SetupFonts()
    {
        //_imguiDefaultFont = ImGui.GetIO().Fonts.AddFontDefault();

        var fontPath = Path.GetFullPath("resources/Quicksand-SemiBold.ttf");

        var nativeConfig = ImGuiNative.ImFontConfig_ImFontConfig();
        //Add a higher horizontal/vertical sample rate for global scaling.
        nativeConfig->OversampleH = 8;
        nativeConfig->OversampleV = 8;
        nativeConfig->RasterizerMultiply = 1f;
        nativeConfig->GlyphOffset = new Vector2(0);

        _quicksandFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 18, nativeConfig);
    }

    public void Update(UpdateInfo info)
    {
        if (!_attatched) return;
        foreach (var layer in Layers)
        {
            if (layer.Enabled)
            {
                layer.Update(info);
            }
        }
    }

    public void PushLayer<T>(T? layer = default) where T : IAjivaLayer
    {
        layer ??= _scope.ServiceProvider.GetRequiredService<T>();
        Layers.Add(layer);
        if (_initialized)
            layer.Attach();
    }

    public void PopLayer<T>() where T : IAjivaLayer
    {
        var layer = Layers.OfType<T>().FirstOrDefault();
        if (layer is null) return;

        layer.Detach();
        Layers.Remove(layer);
    }

    public void Detach()
    {
        _attatched = false;
        foreach (var ajivaLayer in Layers)
        {
            ajivaLayer.Detach();
        }

        Layers.Clear();

        _imguiController?.Dispose();
        _imguiController = null;
    }

    private TextureViewPtr? _swapchainView;
    private Vector2D<i32> _lastFramebufferSize = new(0, 0);
    private bool _requestReConfigure = true;

    public void Render(UpdateInfo info)
    {
        if (!_attatched) return;
        using var tracer = Instrumentation.Trace(ActivityKind.Client);

        var window = _window.Window;
        var surface = _window.Surface;
        var framebufferSize = window.FramebufferSize;

        if (framebufferSize.X * framebufferSize.Y == 0)
            return;

        if (_requestReConfigure || _lastFramebufferSize != framebufferSize)
        {
            _lastFramebufferSize = framebufferSize;

            surface.Configure(new SurfaceConfiguration
            {
                AlphaMode = CompositeAlphaMode.Opaque,
                Device = _api.Device,
                Usage = TextureUsage.RenderAttachment,
                Format = TextureFormat.Bgra8Unorm,
                ViewFormats = [TextureFormat.Bgra8Unorm],
                Width = (u32)framebufferSize.X,
                Height = (u32)framebufferSize.Y,
                PresentMode = PresentMode.Fifo
            });
        }

        _imguiController!.Update(info.DeltaTime);
        _swapchainView?.Release();

        (var swapChainTexture, _requestReConfigure, var status) = _window.Surface.GetCurrentTexture();
        if (!CanContinueRender(status))
            return;

        try
        {
            ActualRender(info, swapChainTexture);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Exception during rendering");
            _requestReConfigure = true;
        }

        try
        {
            using var present = Instrumentation.NamedTrace("Presenting");
            surface.Present();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Exception during presenting");
            _requestReConfigure = true;
        }
    }

    private bool CanContinueRender(SurfaceGetCurrentTextureStatus status)
    {
        switch (status)
        {
            case SurfaceGetCurrentTextureStatus.Success:
                return true;
            case SurfaceGetCurrentTextureStatus.Outdated:
            case SurfaceGetCurrentTextureStatus.Timeout:
                _requestReConfigure = true;
                return true;
            case SurfaceGetCurrentTextureStatus.Lost:
                _logger.LogError("Surface lost, cannot continue rendering");
                _requestReConfigure = true;
                return false;
            case SurfaceGetCurrentTextureStatus.OutOfMemory:
                _logger.LogCritical("Out of memory, cannot continue rendering");
                _requestReConfigure = true;
                return false;
            case SurfaceGetCurrentTextureStatus.DeviceLost:
                _logger.LogCritical("Device lost, cannot continue rendering");
                _requestReConfigure = true;
                return false;
            default:
                _logger.LogWarning("Unknown surface status: {Status}, cannot continue rendering", status);
                _requestReConfigure = true;
                return true;
        }
    }

    private void ActualRender(UpdateInfo info, TexturePtr swapChainTexture)
    {
        _swapchainView = swapChainTexture.CreateView(
            TextureFormat.Bgra8Unorm,
            TextureViewDimension.Dimension2D, TextureAspect.All,
            baseMipLevel: 0, mipLevelCount: 1,
            baseArrayLayer: 0, arrayLayerCount: 1, label: "SwapchainView");

        ImGui.PushFont(_quicksandFont);
        ImGui.DockSpaceOverViewport(0, ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
        using (Instrumentation.Instance?.StartActivity(ActivityKind.Client, name: "Queue"))
        using (var guard = _api.EnterQueue())
        {
            foreach (var layer in Layers)
            {
                using var layers =
                    Instrumentation.Instance?.StartActivity(ActivityKind.Client, name: layer.GetType().ToString());
                if (!layer.Enabled) continue;

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (layer is IRenderLayer renderLayer)
                    renderLayer.Render(info, new RenderTarget(guard.Queue, _imguiController!));

                if (layer is IUiRenderLayer uiRenderLayer)
                    uiRenderLayer.RenderUi(info);
            }

            if (_showOverlay)
                RenderMetrics(info);
            if (_demoWindow)
                ImGui.ShowDemoWindow();

            RenderLayerControls();

            ImGui.PopFont();

            if (ImGui.GetFrameCount() == 4) //seems to be the magic number for loading dock layouts
                ImGui.LoadIniSettingsFromDisk("imgui.ini");

            var cmd = _api.Device.CreateCommandEncoder(label: "ImGui Composite");
            var pass = cmd.BeginRenderPass([
                    new RenderPassColorAttachment(_swapchainView.Value, resolveTarget: null,
                        LoadOp.Clear, StoreOp.Store, default)
                ], null, null,
                null);

            _imguiController!.Render(pass);
            ImGui.UpdatePlatformWindows();
            //ImGui.RenderPlatformWindowsDefault();

            pass.End();
            guard.Queue.Submit(cmd.Finish());
        }
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
            foreach (var layer in Layers)
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
                layerToRemove.Detach();
                Layers.Remove(layerToRemove);
            }

            ImGui.Separator();
            // combobox for adding or removing layers
            if (ImGui.BeginCombo("Add Layer", "Select Layer"))
            {
                foreach (var layerType in _layerTypes)
                {
                    if (Layers.Any(l => l.GetType() == layerType)) continue;

                    if (ImGui.Selectable(layerType.Name))
                    {
                        CreateLayer(layerType);
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

    private void CreateLayer(TypeInfo layerType)
    {
        var layer = (IAjivaLayer)_scope.ServiceProvider.GetRequiredService(layerType);
        PushLayer(layer);
    }

    private i32 _location = -1;
    private bool _showOverlay = true, _demoWindow;
    private f32[] _frameTimes = new f32[100];
    private readonly TypeInfo[] _layerTypes;
    private readonly IServiceScope _scope;
    private PeriodicUpdater.UpdateHandle? _renderUpdateHandle, _updateHandle;

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
            ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Always, new Vector2(0.5f, 0.5f));
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
            ImGui.PlotLines("", ref _frameTimes[0], _frameTimes.Length, 0, "Frame Times", 0, 50,
                new Vector2(300, 80));
        }

        ImGui.End();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _updateHandle?.Dispose();
        _renderUpdateHandle?.Dispose();
        GCStressTest = null;
    }
}