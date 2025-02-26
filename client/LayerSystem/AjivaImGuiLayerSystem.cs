using System.Diagnostics;
using System.Numerics;
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

    public AjivaImGuiLayerSystem(ILogger<AjivaImGuiLayerSystem> logger, RendererApi api)
    {
        _logger = logger;
        _api = api;
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

    public async Task Update(UpdateInfo info)
    {
        _imguiController!.Update(info.DeltaTime);
        foreach (var layer in Layers)
        {
            if (layer.Enabled)
            {
                await layer.Update(info);
            }
        }
    }

    public async Task PushLayer(IAjivaLayer layer)
    {
        await layer.Attach();
        Layers.Add(layer);
    }

    public Task Detach()
    {
        _imguiController!.Dispose();
        return Task.CompletedTask;
    }

    private TextureViewPtr? _swapchainView;


    public async Task Render(UpdateInfo info)
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

        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport());
        ImGui.UpdatePlatformWindows();
        ImGui.RenderPlatformWindowsDefault();

        // ImGui.ShowDemoWindow();

        foreach (var layer in Layers)
        {
            if (!layer.Enabled) continue;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (layer is IRenderLayer renderLayer)
                await renderLayer.Render(info, new RenderTarget(queue));

            if (layer is IUiRenderLayer uiRenderLayer)
                await uiRenderLayer.RenderUi(info);
        }

        if (_showOverlay)
            RenderMetrics(info);

        ImGui.PopFont();
        ImGui.PopStyleVar();

        if (ImGui.GetFrameCount() == 2) //seems to be the magic number for loading dock layouts
            ImGui.LoadIniSettingsFromDisk("imgui.ini");

        var cmd = _api.Device.CreateCommandEncoder();
        var pass = cmd.BeginRenderPass(new RenderPassColorAttachment[]
            {
                new(_swapchainView.Value, resolveTarget: null,
                    LoadOp.Clear, StoreOp.Store, new Color(.1, .1, .1, 1))
            }, null, null,
            null);

        _imguiController!.Render(pass);

        pass.End();

        queue.Submit(new[] { cmd.Finish() });
    }

    private i32 _location = -1;
    private bool _showOverlay = true;
    private f32[] _frameTimes = new f32[100];

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

public record struct UpdateInfo(u64 Ticks, f32 DeltaTime, f32 TotalTime)
{
    public UpdateInfo(AjivaClock updateClock) : this(
        updateClock.Ticks,
        (f32)updateClock.DeltaTime.TotalSeconds,
        (f32)updateClock.Total.TotalSeconds)
    {
    }
}

public record struct RenderTarget(QueuePtr QueuePtr);

public interface IAjivaLayer
{
    Task Attach();
    Task Update(UpdateInfo info);
    Task Detach();
    bool Enabled { get; set; }
}

public interface IRenderLayer : IAjivaLayer
{
    Task Render(UpdateInfo info, RenderTarget target);
}

public interface IUiRenderLayer : IAjivaLayer
{
    Task RenderUi(UpdateInfo info);
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
