using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using client.Platform;
using client.Renderer;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.ImGui;
using Silk.NET.WebGPU.Safe;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;

namespace client.LayerSystem;

public class AjivaImGuiLayerSystem
{
    private bool _initialized;

    private readonly ILogger<AjivaImGuiLayerSystem> _logger;

    private readonly RendererApi _api;

    private ImFontPtr _imguiDefaultFont;
    private ImFontPtr _quicksandFont;
    private ImGuiController? _imguiController;
    private AjivaWindow _window = null!;

    private List<IAjivaLayer> Layers { get; } = [];

    public AjivaImGuiLayerSystem(ILogger<AjivaImGuiLayerSystem> logger, RendererApi api,
        IServiceScopeFactory serviceProvider)
    {
        _scope = serviceProvider.CreateScope();

        _logger = logger;
        _api = api;
        // find alll types that implement IAjivaLayer
        _layerTypes = typeof(AjivaImGuiLayerSystem).Assembly.DefinedTypes
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IAjivaLayer).IsAssignableFrom(t))
            .ToArray();
    }

    public void Attach(AjivaWindow parent)
    {
        if (_initialized) return;
        _initialized = true;
        _window = parent;

        _imguiController = new ImGuiController(_api.Device,
            TextureFormat.Bgra8Unorm, parent.Window, parent.Window.CreateInput(), SetupFonts);

        var io = ImGui.GetIO();

        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable; // Enable Docking
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable; // Enable Multi-Viewport / Platform Windows
        return;

        unsafe void SetupFonts()
        {
            _imguiDefaultFont = ImGui.GetIO().Fonts.AddFontDefault();

            var fontPath = Path.GetFullPath("resources/Quicksand-SemiBold.ttf");

            var nativeConfig = ImGuiNative.ImFontConfig_ImFontConfig();
            //Add a higher horizontal/vertical sample rate for global scaling.
            nativeConfig->OversampleH = 8;
            nativeConfig->OversampleV = 8;
            nativeConfig->RasterizerMultiply = 1f;
            nativeConfig->GlyphOffset = new Vector2(0);

            _quicksandFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 18, nativeConfig);
        }
    }

    public void Update(UpdateInfo info)
    {
        _imguiController!.Update(info.DeltaTime);
        foreach (var layer in Layers)
        {
            if (layer.Enabled)
            {
                layer.Update(info);
            }
        }
    }

    public async Task PushLayer<T>(T? layer = default) where T : IAjivaLayer
    {
        layer ??= _scope.ServiceProvider.GetRequiredService<T>();
        await layer.Attach();
        Layers.Add(layer);
    }

    public async Task PopLayer<T>() where T : IAjivaLayer
    {
        var layer = Layers.OfType<T>().FirstOrDefault();
        if (layer is null) return;

        await layer.Detach();
        Layers.Remove(layer);
    }

    public async Task Detach()
    {
        foreach (var ajivaLayer in Layers)
        {
            await ajivaLayer.Detach();
        }

        Layers.Clear();

        _imguiController!.Dispose();
    }

    private TextureViewPtr? _swapchainView;


    public void Render(UpdateInfo info)
    {
        _swapchainView?.Release();

        var (swapChainTexture, _, status) = _window.surface.GetCurrentTexture();
        Debug.Assert(status == SurfaceGetCurrentTextureStatus.Success);

        _swapchainView = swapChainTexture.CreateView(
            TextureFormat.Bgra8Unorm,
            TextureViewDimension.Dimension2D, TextureAspect.All,
            baseMipLevel: 0, mipLevelCount: 1,
            baseArrayLayer: 0, arrayLayerCount: 1, label: "SwapchainView");

        var queue = _api.Device.GetQueue();

        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4);
        ImGui.PushFont(_quicksandFont);

        ImGui.DockSpaceOverViewport(0, ImGui.GetMainViewport());
        ImGui.UpdatePlatformWindows();
        //ImGui.RenderPlatformWindowsDefault();

        // ImGui.ShowDemoWindow();

        foreach (var layer in Layers)
        {
            if (!layer.Enabled) continue;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (layer is IRenderLayer renderLayer)
                renderLayer.Render(info, new RenderTarget(queue, _imguiController!));

            if (layer is IUiRenderLayer uiRenderLayer)
                uiRenderLayer.RenderUi(info);
        }

        /*if (_showOverlay)
            RenderMetrics(info);*/
        if (_demoWindow)
            ImGui.ShowDemoWindow();

        //RenderLayerControls();

        ImGui.PopFont();
        ImGui.PopStyleVar();

        if (ImGui.GetFrameCount() == 4) //seems to be the magic number for loading dock layouts
            ImGui.LoadIniSettingsFromDisk("imgui.ini");

        var cmd = _api.Device.CreateCommandEncoder();
        var pass = cmd.BeginRenderPass([
                new RenderPassColorAttachment(_swapchainView.Value, resolveTarget: null,
                    LoadOp.Clear, StoreOp.Store, new Color(.1, .1, .1, 1))
            ], null, null,
            null);

        _imguiController!.Render(pass);

        pass.End();
        queue.Submit(cmd.Finish());
    }

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
                Task.WaitAll(layerToRemove.Detach());
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
                GC.Collect();
        }

        ImGui.End();
    }

    private void CreateLayer(TypeInfo layerType)
    {
        var layer = (IAjivaLayer)_scope.ServiceProvider.GetRequiredService(layerType);
        Task.WaitAll(PushLayer(layer));
    }

    private i32 _location = -1;
    private bool _showOverlay = true, _demoWindow = false;
    private f32[] _frameTimes = new f32[100];
    private readonly TypeInfo[] _layerTypes;
    private readonly IServiceScope _scope;

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
}

public ref struct UpdateInfo(u64 ticks, f32 deltaTime, f32 totalTime)
{
    public UpdateInfo(AjivaClock updateClock) : this(
        updateClock.Ticks,
        (f32)updateClock.DeltaTime.TotalSeconds,
        (f32)updateClock.Total.TotalSeconds)
    {
    }

    public readonly u64 Ticks = ticks;
    public readonly f32 DeltaTime = deltaTime;
    public readonly f32 TotalTime = totalTime;
}

public ref struct RenderTarget(QueuePtr queuePtr, ImGuiController imguiController)
{
    public readonly QueuePtr QueuePtr = queuePtr;
    public readonly ImGuiController ImguiController = imguiController;
}

public interface IAjivaLayer
{
    Task Attach();
    void Update(UpdateInfo info);
    Task Detach();
    ref bool Enabled { get; }
}

public interface IRenderLayer : IAjivaLayer
{
    void Render(UpdateInfo info, RenderTarget target);
}

public interface IUiRenderLayer : IAjivaLayer
{
    void RenderUi(UpdateInfo info);
}

public interface IVirtualView
{
    string Name { get; }
    Task Render(UpdateInfo info, RenderTarget target);
}

public class DelegateView(string name, Func<UpdateInfo, RenderTarget, Task> renderFunc)
    : IVirtualView
{
    public string Name { get; } = name;

    public Task Render(UpdateInfo info, RenderTarget target) => renderFunc(info, target);
}
