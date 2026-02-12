using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using Client.Desktop.Platform;
using Client.LayerSystem;
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

namespace Client.Desktop;

public class AjivaImGuiLayer : IAjivaLayer, IDisposable
{
    private readonly ILogger<AjivaImGuiLayer> _logger;
    private readonly LayerStack _layerStack;

    private readonly RendererApi _api;
    private readonly PeriodicUpdater _periodicUpdater;

    private ImFontPtr _quicksandFont;
    private ImGuiController? _imguiController;
    private readonly AjivaWindow _window;

    public AjivaImGuiLayer(ILogger<AjivaImGuiLayer> logger, LayerStack layerStack, RendererApi api,
        PeriodicUpdater periodicUpdater, AjivaWindow window, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _layerStack = layerStack;
        _api = api;
        _periodicUpdater = periodicUpdater;
        _window = window;
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

    public void Attach()
    {
        _imguiController = new ImGuiController(_api.Device, TextureFormat.Bgra8Unorm, _window.Window,
            _window.InputContext, SetupFonts);

        var io = ImGui.GetIO();

        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable; // Enable Docking
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable; // Enable Multi-Viewport / Platform Windows
        _renderUpdateHandle = _periodicUpdater.RegisterUpdate(UpdateCycle.Render, Render);
        var style = ImGui.GetStyle();
        style.FrameRounding = 4;
        style.FrameBorderSize = 1;
        io.FontGlobalScale = 1.25f;
    }

    public void Update(UpdateInfo info)
    {
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

    public void Detach()
    {
        _imguiController?.Dispose();
        _imguiController = null;
    }

    private bool _enabled = true;
    public ref bool Enabled => ref _enabled;

    private TextureViewPtr? _swapchainView;
    private Vector2D<i32> _lastFramebufferSize = new(0, 0);
    private bool _requestReConfigure = true;

    public void Render(UpdateInfo info)
    {
        if (!_layerStack.Attached) return;
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
            foreach (var layer in _layerStack.Layers)
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
    private f32[] _frameTimes = new f32[100];
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
}