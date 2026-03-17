using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Client.Common;
using Client.Renderer.Resources;
using Client.Wgsl;
using Common.Systems;
using Hexa.NET.ImGui;
using Hexa.NET.SDL3;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using ImGuiImplSDL3 = Hexa.NET.ImGui.Backends.SDL3.ImGuiImplSDL3;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;

namespace Client.Renderer.Renderers;

public partial class ImGuiPlatform(
    ResourceManager loader,
    ILogger<ImGuiPlatform> logger,
    ShaderReloadManager shaderReloadManager,
    ISurfaceManager surfaceManager)
    : IEventHandler<SDLEvent>
{
    private static readonly Memory<u8> PlatformName = new("Ajiva ImGui Renderer\0"u8.ToArray());
    private ImGuiContextPtr _context;
    private Delegate _createDele = null!, _renderDele = null!, _updateDele = null!;
    private u64 _currentTick;

    public unsafe bool Initialize(SDLWindowPtr windowWindow)
    {
        _context = ImGui.CreateContext();
        ImGui.SetCurrentContext(_context);

        var io = ImGui.GetIO();
        io.BackendPlatformName = (u8*)_platformNameHandle.Pointer;
        io.BackendRendererName = (u8*)_platformNameHandle.Pointer;

        io.BackendFlags |= ImGuiBackendFlags.RendererHasTextures | ImGuiBackendFlags.RendererHasVtxOffset |
                           ImGuiBackendFlags.RendererHasViewports;

        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard
                          | ImGuiConfigFlags.NavEnableGamepad
                          | ImGuiConfigFlags.DockingEnable
                          | ImGuiConfigFlags.ViewportsEnable;

        ImGuiImplSDL3.SetCurrentContext(_context);
        ImGuiImplSDL3.InitForOther((Hexa.NET.ImGui.Backends.SDL3.SDLWindow*)windowWindow.Handle);

        var platformIo = ImGui.GetPlatformIO();

        platformIo.RendererCreateWindow =
            Marshal.GetFunctionPointerForDelegate(_createDele = OnCreateWindow).ToPointer();
        platformIo.RendererRenderWindow =
            Marshal.GetFunctionPointerForDelegate(_renderDele = OnRenderWindow).ToPointer();
        platformIo.RendererDestroyWindow =
            Marshal.GetFunctionPointerForDelegate(_updateDele = OnDestroyWindow).ToPointer();

        return true;
    }


    private unsafe void OnRenderWindow(ImGuiViewport* viewport, void* _)
    {
        var window = SDL.GetWindowFromID((u32)viewport->PlatformHandle);
        RenderWindow(window, viewport->DrawData);
    }

    private unsafe void RenderWindow(SDLWindowPtr window, ImDrawDataPtr drawData)
    {
        using var surfaceGuard = surfaceManager.GetWindowSurfaceTexture(window);
        if (!surfaceGuard.IsValid) return;
        using var queueGuard = loader.Api.EnterQueue();

        var surfaceTexture = surfaceGuard.TextureView;
        var cmd = loader.Api.Device.CreateCommandEncoder(label: "ImGui Composite");
        var pass = cmd.BeginRenderPass([
                new RenderPassColorAttachment(surfaceTexture, resolveTarget: null,
                    LoadOp.Clear, StoreOp.Store, default)
            ], null, null,
            null);
        RenderImDrawData(drawData, pass);

        pass.End();
        queueGuard.Queue.Submit(cmd.Finish());
    }

    private unsafe void OnCreateWindow(ImGuiViewport* viewport)
    {
        var window = SDL.GetWindowFromID((u32)viewport->PlatformHandle);
        surfaceManager.GetWindowSurfaceTexture(window).Dispose();
    }

    private unsafe void OnDestroyWindow(ImGuiViewport* viewport)
    {
        var window = SDL.GetWindowFromID((u32)viewport->PlatformHandle);
        surfaceManager.ReleaseWindowSurfaceTexture(window);
    }

    public void Teardown()
    {
        InvalidateDeviceObjects();
        ImGuiImplSDL3.Shutdown();
        ImGui.DestroyContext(_context);
    }


    public void NewFrame(UpdateInfo info, Vector2D<i32> framebufferSize)
    {
        _currentTick = info.Ticks;
        ImGui.SetCurrentContext(_context);
        var currentShader = shaderReloadManager.GetCurrentShader<IImGuiShader>();
        if ((_pipeline is null || !_pipeline.IsUpToDate(currentShader)) && !CreateDeviceObjects(currentShader))
        {
            logger.LogError("Failed to create ImGui device objects");
            throw new Exception("Failed to create ImGui device");
        }

        var io = ImGui.GetIO();
        if (framebufferSize is { X: > 0, Y: > 0 })
            io.DisplaySize = new Vector2(framebufferSize.X, framebufferSize.Y);
        io.DisplayFramebufferScale = Vector2.One;

        io.DeltaTime = info.DeltaTime;
        ImGuiImplSDL3.NewFrame();
        ImGui.NewFrame();

        UpdateTexture(io.Fonts.TexData);
    }

    public void EndFrame(SDLWindowPtr mainWindow)
    {
        ImGui.Render();
        ImGui.EndFrame();

        if (ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
        {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
        }

        RenderWindow(mainWindow, ImGui.GetDrawData());
    }

    public void OnEvent(ref AjivaEvent<SDLEvent> inputEvent)
    {
        if (ImGuiImplSDL3.ProcessEvent(
                ref Unsafe.As<SDLEvent, Hexa.NET.ImGui.Backends.SDL3.SDLEvent>(ref inputEvent.EventData)))
        {
        }
    }
}