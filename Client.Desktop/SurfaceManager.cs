using Client.Desktop.Platform;
using Client.Renderer;
using Common;
using Hexa.NET.SDL3;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using SurfaceConfiguration = Silk.NET.WebGPU.Safe.SurfaceConfiguration;

namespace Client.Desktop;

public class SurfaceManager(RendererApi api, ILogger<SurfaceManager> logger)
{
    public TextureFormat DefaultSurfaceFormat => TextureFormat.Bgra8Unorm;
    public PresentMode DefaultPresentMode => PresentMode.Fifo;

    public class SurfaceInfo(SurfacePtr ptr)
    {
        public SurfacePtr Ptr { get; } = ptr;
        public Vector2D<u32> LastSize { get; set; }
        public bool RequestReConfigure { get; set; }
        public TexturePtr? Texture { get; set; }
        public TextureViewPtr? View { get; private set; }
        public TextureFormat Format { get; set; }
        public PresentMode PresentMode { get; set; }

        public TextureViewPtr? CreateOrGetView()
        {
            if (Texture is null) return null;
            if (View is not null) return View;
            return View = Texture.Value.CreateView(Format, TextureViewDimension.Dimension2D, TextureAspect.All,
                baseMipLevel: 0, mipLevelCount: 1, baseArrayLayer: 0, arrayLayerCount: 1, label: "SwapchainView");
        }

        public void ReleaseView()
        {
            if (View is null) return;
            View.Value.Release();
            View = null;
        }
    }

    private readonly Dictionary<SDLWindowPtr, SurfaceInfo> _windowSurfaces = new();
    private readonly ILogger<SurfaceManager> _logger = logger;

    public readonly ref struct SurfaceUsageInfo : IDisposable
    {
        private readonly SurfaceInfo? _info;
        private readonly SurfaceManager _manager;

        public SurfaceUsageInfo(SurfaceInfo? info, SurfaceManager manager)
        {
            _info = info;
            _manager = manager;
        }

        public bool IsValid => _info != null;
        public TextureViewPtr TextureView => _info?.CreateOrGetView() ?? default;
        public Vector2D<u32> Size => _info?.LastSize ?? default;
        public TexturePtr Texture => _info?.Texture ?? default;

        public void Dispose()
        {
            if (_info is null) return;
            try
            {
                using var present = Instrumentation.NamedTrace("Presenting");
                _info.Ptr.Present();
            }
            catch (Exception e)
            {
                _manager._logger.LogError(e, "Error presenting surface");
                _info.RequestReConfigure = true;
            }
        }
    }

    public SurfaceUsageInfo GetWindowSurfaceTexture(SDLWindowPtr window)
    {
        var width = 0;
        var height = 0;
        SDL.GetWindowSizeInPixels(window, ref width, ref height);
        return GetSurfaceTexture(window, new Vector2D<u32>((u32)width, (u32)height));
    }

    public SurfaceUsageInfo GetSurfaceTexture(SDLWindowPtr window, Vector2D<u32> requestedSize)
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

        if (surface.LastSize != requestedSize)
        {
            ReconfigureSurface(surface, requestedSize);
        }

        return new SurfaceUsageInfo(CanContinueRender(surface) ? surface : null, this);
    }

    private void ReconfigureSurface(SurfaceInfo surface, Vector2D<u32> requestedSize)
    {
        using var reconfigure = Instrumentation.NamedTrace("Reconfiguring Surface");
        surface.LastSize = requestedSize;
        surface.Ptr.Configure(new SurfaceConfiguration
        {
            AlphaMode = CompositeAlphaMode.Opaque,
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
                _logger.LogError("Surface lost, cannot continue rendering");
                info.RequestReConfigure = true;
                return false;
            case SurfaceGetCurrentTextureStatus.OutOfMemory:
                _logger.LogCritical("Out of memory, cannot continue rendering");
                info.RequestReConfigure = true;
                return false;
            case SurfaceGetCurrentTextureStatus.DeviceLost:
                _logger.LogCritical("Device lost, cannot continue rendering");
                info.RequestReConfigure = true;
                return false;
            case SurfaceGetCurrentTextureStatus.Force32:
            default:
                _logger.LogWarning("Unknown surface status: {Status}, cannot continue rendering", status);
                info.RequestReConfigure = true;
                return true;
        }
    }
}