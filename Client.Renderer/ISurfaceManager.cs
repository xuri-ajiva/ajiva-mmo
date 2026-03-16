using Common;
using Hexa.NET.SDL3;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer;

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

public readonly ref struct SurfaceUsageInfo : IDisposable
{
    private readonly SurfaceInfo? _info;
    private readonly ISurfaceManager _manager;

    public SurfaceUsageInfo(SurfaceInfo? info, ISurfaceManager manager)
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
            _manager.LogError(e, "Error presenting surface");
            _info.RequestReConfigure = true;
        }
    }
}

public interface ISurfaceManager
{
    TextureFormat DefaultSurfaceFormat { get; }
    PresentMode DefaultPresentMode { get; }
    SurfaceUsageInfo GetWindowSurfaceTexture(SDLWindowPtr window);
    SurfaceUsageInfo GetSurfaceTexture(SDLWindowPtr window, Vector2D<u32> requestedSize);
    void LogError(Exception exception, string message);
}