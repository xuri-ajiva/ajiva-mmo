using Common;
using Hexa.NET.SDL3;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer;

public class SurfaceInfo(SurfacePtr ptr)
{
    public SurfacePtr Ptr { get; } = ptr;
    public Vector2D<u32> Size { get; set; }
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

public readonly ref struct SurfaceUsageInfoGuard(SurfaceInfo? info, ISurfaceManager manager) : IDisposable
{
    public bool IsValid => info != null;
    public TextureViewPtr TextureView => info?.CreateOrGetView() ?? default;
    public Vector2D<u32> Size => info?.Size ?? default;
    public TexturePtr Texture => info?.Texture ?? default;

    public void Dispose()
    {
        if (info is null) return;
        try
        {
            using var present = Instrumentation.NamedTrace("Presenting");
            info.Ptr.Present();
        }
        catch (Exception e)
        {
            manager.LogError(e, "Error presenting surface");
            info.RequestReConfigure = true;
        }
    }
}

public interface ISurfaceManager
{
    TextureFormat DefaultSurfaceFormat { get; }
    PresentMode DefaultPresentMode { get; }
    SurfaceUsageInfoGuard GetWindowSurfaceTexture(SDLWindowPtr window);
    SurfaceUsageInfoGuard GetSurfaceTexture(SDLWindowPtr window, Vector2D<u32> requestedSize);
    void LogError(Exception exception, string message);
    void ReleaseWindowSurfaceTexture(SDLWindowPtr window);
}