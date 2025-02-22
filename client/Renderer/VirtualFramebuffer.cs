using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace client.Renderer;

public class VirtualFramebuffer
{
    public u32 Width { get; private set; }
    public u32 Height { get; private set; }
    private readonly string? _label;

    private readonly DevicePtr _device;


    private (TextureFormat format, TexturePtr? tex, TextureViewPtr? view) _color0;
    private (TextureFormat format, TexturePtr? tex, TextureViewPtr? view, TextureViewPtr? depthOnlyView)? _depthStencil;

    public VirtualFramebuffer(DevicePtr device, TextureFormat color0Format, TextureFormat? depthStencilFormat = null,
        u32 width = 0, u32 height = 0, string? label = null)
    {
        _label = label;

        _device = device;
        _color0 = (color0Format, null, null);

        if (depthStencilFormat is not null)
            _depthStencil = (depthStencilFormat.Value, null, null, null);
        else
            _depthStencil = null;

        EnsureSize(width, height);

        Height = height;
        Width = width;
    }

    public bool HasViews => Width * Height != 0;
    
    public bool TryGetViews(out TextureViewPtr color0View, out TextureViewPtr? depthStencilView, out TextureViewPtr? depthOnlyView)
    {
        if (Width * Height == 0)
        {
            color0View = default;
            depthStencilView = default;
            depthOnlyView = default;
            return false;
        }

        color0View = _color0.view!.Value;
        depthStencilView = _depthStencil?.view;
        depthOnlyView = _depthStencil?.depthOnlyView;

        return true;
    }
    
    public void EnsureSize(u32 width, u32 height)
    {
        if (width == Width && height == Height)
            return;

        Width = width;
        Height = height;

        const TextureUsage usageFlags = TextureUsage.RenderAttachment | TextureUsage.TextureBinding;

        _color0.tex?.Destroy();
        _depthStencil?.tex?.Destroy();

        if (width * height == 0) return;

        _color0.tex = _device.CreateTexture(usageFlags, TextureDimension.Dimension2D,
            new Extent3D(width, height, 1),
            _color0.format, 1, 1, new ReadOnlySpan<TextureFormat>(ref _color0.format),
            label: _label == null ? null : $"{_label}:Color0");

        _color0.view = _color0.tex?.CreateView(_color0.format, TextureViewDimension.Dimension2D,
            TextureAspect.All, 0, 1, 0, 1,
            label: _label == null ? null : $"{_label}:Color0 - View");

        if (_depthStencil is null) return;

        var depthStencil = _depthStencil.Value;
        depthStencil.tex = _device.CreateTexture(usageFlags, TextureDimension.Dimension2D,
            new Extent3D(width, height, 1),
            depthStencil.format, 1, 1, new ReadOnlySpan<TextureFormat>(ref depthStencil.format),
            label: _label == null ? null : $"{_label}:DepthStencil");

        depthStencil.view = depthStencil.tex?.CreateView(depthStencil.format,
            TextureViewDimension.Dimension2D,
            TextureAspect.All, 0, 1, 0, 1,
            label: _label == null ? null : $"{_label}:DepthStencil - View");

        depthStencil.depthOnlyView = depthStencil.tex?.CreateView(depthStencil.format,
            TextureViewDimension.Dimension2D,
            TextureAspect.DepthOnly, 0, 1, 0, 1,
            label: _label == null ? null : $"{_label}:DepthStencil - View(Depth)");

        _depthStencil = depthStencil;
    }
}
