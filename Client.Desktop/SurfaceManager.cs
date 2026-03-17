using Client.Desktop.Platform;
using Client.Renderer;
using Common;
using Hexa.NET.SDL3;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using SurfaceConfiguration = Silk.NET.WebGPU.Safe.SurfaceConfiguration;

namespace Client.Desktop;

public class SurfaceManager(RendererApi api, ILogger<SurfaceManager> logger) : ISurfaceManager, IHostedService
{
    public TextureFormat DefaultSurfaceFormat => TextureFormat.Bgra8Unorm;
    public PresentMode DefaultPresentMode => PresentMode.Fifo;
    private readonly Dictionary<SDLWindowPtr, SurfaceInfo> _windowSurfaces = new();

    public SurfaceUsageInfoGuard GetWindowSurfaceTexture(SDLWindowPtr window)
    {
        var width = 0;
        var height = 0;
        SDL.GetWindowSizeInPixels(window, ref width, ref height);
        return GetSurfaceTexture(window, new Vector2D<u32>((u32)width, (u32)height));
    }

    public SurfaceUsageInfoGuard GetSurfaceTexture(SDLWindowPtr window, Vector2D<u32> requestedSize)
    {
        if (!_windowSurfaces.TryGetValue(window, out var surface))
        {
            surface = new SurfaceInfo(PlatformSystem.CreateWebGpuSurface(window, api.Instance))
            {
                Format = DefaultSurfaceFormat,
                PresentMode = DefaultPresentMode
            };
            _windowSurfaces[window] = surface;
        }

        surface.ReleaseView();

        if (surface.Size != requestedSize)
        {
            ReconfigureSurface(surface, requestedSize);
            //TODO implement dynamic surface format selection based on capabilities and preferences
            surface.Ptr.GetCapabilities(api.Adapter, out var capabilities);
            var textureFormat = surface.Ptr.GetPreferredFormat(api.Adapter);
        }

        unsafe
        {
            logger.LogTrace("Acquiring surface texture for window {Window} with size {Size}", (nint)window.Handle,
                requestedSize);
        }

        return new SurfaceUsageInfoGuard(CanContinueRender(surface) ? surface : null, this);
    }

    public void LogError(Exception exception, string message) => logger.LogError(exception, message);

    public void ReleaseWindowSurfaceTexture(SDLWindowPtr window)
    {
        if (_windowSurfaces.TryGetValue(window, out var surface))
        {
            surface.Ptr.Release();
            _windowSurfaces.Remove(window);
        }
    }

    private void ReconfigureSurface(SurfaceInfo surface, Vector2D<u32> requestedSize)
    {
        using var reconfigure = Instrumentation.NamedTrace("Reconfiguring Surface");
        surface.Size = requestedSize;
        surface.Ptr.Configure(new SurfaceConfiguration
        {
            AlphaMode = CompositeAlphaMode.Auto,
            Device = api.Device,
            Usage = TextureUsage.RenderAttachment,
            Format = surface.Format,
            ViewFormats = [surface.Format],
            Width = requestedSize.X,
            Height = requestedSize.Y,
            PresentMode = surface.PresentMode
        });
    }

    private bool CanContinueRender(SurfaceInfo info)
    {
        using var acquire = Instrumentation.NamedTrace("Acquiring Surface Texture");

        (info.Texture, info.RequestReConfigure, var status) = info.Ptr.GetCurrentTexture();
        switch (status)
        {
            case SurfaceGetCurrentTextureStatus.Success:
                return true;
            case SurfaceGetCurrentTextureStatus.Outdated:
            case SurfaceGetCurrentTextureStatus.Timeout:
                info.RequestReConfigure = true;
                return true;
            case SurfaceGetCurrentTextureStatus.Lost:
                logger.LogError("Surface lost, cannot continue rendering");
                info.RequestReConfigure = true;
                return false;
            case SurfaceGetCurrentTextureStatus.OutOfMemory:
                logger.LogCritical("Out of memory, cannot continue rendering");
                info.RequestReConfigure = true;
                return false;
            case SurfaceGetCurrentTextureStatus.DeviceLost:
                logger.LogCritical("Device lost, cannot continue rendering");
                info.RequestReConfigure = true;
                return false;
            case SurfaceGetCurrentTextureStatus.Force32:
            default:
                logger.LogWarning("Unknown surface status: {Status}, cannot continue rendering", status);
                info.RequestReConfigure = true;
                return true;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var surface in _windowSurfaces.Values)
        {
            surface.Ptr.Release();
            surface.Texture?.Release();
        }

        _windowSurfaces.Clear();
        return Task.CompletedTask;
    }
}