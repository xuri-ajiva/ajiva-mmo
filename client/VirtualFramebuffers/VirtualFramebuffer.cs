using System.Runtime.CompilerServices;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace client.VirtualFramebuffers;

public class VirtualFramebuffer : IDisposable
{
    private struct ColorTarget(string name, TextureFormat format, TexturePtr? tex, TextureViewPtr? view)
    {
        public readonly string Name = name;
        public readonly TextureFormat Format = format;
        public TexturePtr? Tex = tex;
        public TextureViewPtr? View = view;
    }

    private struct DepthStencilTarget(
        TextureFormat format,
        TexturePtr? tex,
        TextureViewPtr? view,
        TextureViewPtr? depthOnlyView)
    {
        public readonly TextureFormat Format = format;
        public TexturePtr? Tex = tex;
        public TextureViewPtr? View = view;
        public TextureViewPtr? DepthOnlyView = depthOnlyView;
    }

    public record struct ColorTargetView(TexturePtr Tex, TextureViewPtr View);

    public record struct DepthStencilTargetView(TexturePtr Tex, TextureViewPtr View, TextureViewPtr DepthOnlyView);

    private readonly string? _label;
    private readonly DevicePtr _device;

    public TextureFormat? DepthStencilFormat => _depthStencil?.Format;
    public TextureFormat GetFormat(i32 index) => _colorTargets[index].Format;

    private ColorTarget[] _colorTargets;
    private DepthStencilTarget? _depthStencil;

    public u32 Width { get; private set; }
    public u32 Height { get; private set; }

    public VirtualFramebuffer(DevicePtr device,
        (string name, TextureFormat format)[] colorTargets, TextureFormat? depthStencilFormat,
        u32 width = 0, u32 height = 0, string? label = null)
    {
        _label = label;
        _device = device;

        _colorTargets = Array.ConvertAll(colorTargets,
            x => new ColorTarget(x.name, x.format, null, null)
        );

        if (depthStencilFormat is { } format)
            _depthStencil = new DepthStencilTarget(format, null, null, null);

        EnsureSize(width, height);

        Height = height;
        Width = width;
    }

    public bool HasViews => Width * Height != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetColorTarget(i32 index, out ColorTargetView colorTarget)
    {
        if (Width * Height == 0)
        {
            colorTarget = default;
            return false;
        }

        colorTarget = new ColorTargetView(
            _colorTargets[index].Tex.GetValueOrDefault(),
            _colorTargets[index].View.GetValueOrDefault()
        );

        return true;
    }

    public bool TryGetDepthTarget(out DepthStencilTargetView? depthTarget)
    {
        if (Width * Height == 0)
        {
            depthTarget = null;
            return false;
        }

        if (!_depthStencil.HasValue)
        {
            depthTarget = null;
            return true;
        }

        var ds = _depthStencil.GetValueOrDefault();
        depthTarget = new DepthStencilTargetView(
            ds.Tex.GetValueOrDefault(),
            ds.View.GetValueOrDefault(),
            ds.DepthOnlyView.GetValueOrDefault()
        );

        return true;
    }

    public void EnsureSize(u32 width, u32 height)
    {
        if (width == Width && height == Height)
            return;

        Width = width;
        Height = height;

        const TextureUsage usageFlags = TextureUsage.RenderAttachment | TextureUsage.TextureBinding;

        foreach (var t in _colorTargets)
            t.Tex?.Destroy();

        _depthStencil?.Tex?.Destroy();

        if (width * height == 0)
            return;

        for (var i = 0; i < _colorTargets.Length; i++)
        {
            ref var target = ref _colorTargets[i];
            target.Tex?.Release();
            
            target.Tex = _device.CreateTexture(usageFlags, TextureDimension.Dimension2D,
                new Extent3D(width, height, 1),
                target.Format, 1, 1, new ReadOnlySpan<TextureFormat>(in target.Format),
                label: _label == null ? null : $"{_label}:{target.Name}");

            target.View = target.Tex?.CreateView(target.Format, TextureViewDimension.Dimension2D,
                TextureAspect.All, 0, 1, 0, 1,
                label: _label == null ? null : $"{_label}:{target.Name}_View");
        }


        if (_depthStencil is null) return;

        var depthTarget = _depthStencil.Value;
        depthTarget.Tex?.Release();
        depthTarget.Tex = _device.CreateTexture(usageFlags, TextureDimension.Dimension2D,
            new Extent3D(width, height, 1),
            depthTarget.Format, 1, 1, new ReadOnlySpan<TextureFormat>(in depthTarget.Format),
            label: _label == null ? null : $"{_label}:DepthStencil");

        depthTarget.View = depthTarget.Tex?.CreateView(depthTarget.Format,
            TextureViewDimension.Dimension2D,
            TextureAspect.All, 0, 1, 0, 1,
            label: _label == null ? null : $"{_label}:DepthStencil_View");

        depthTarget.DepthOnlyView = depthTarget.Tex?.CreateView(depthTarget.Format,
            TextureViewDimension.Dimension2D,
            TextureAspect.DepthOnly, 0, 1, 0, 1,
            label: _label == null ? null : $"{_label}:DepthStencil_View(Depth)");

        _depthStencil = depthTarget;
    }

    public void Release()
    {
        foreach (var t in _colorTargets)
        {
            t.Tex?.Destroy();
            t.View?.Release();
        }

        _depthStencil?.Tex?.Destroy();
        _depthStencil?.View?.Release();
        _depthStencil?.DepthOnlyView?.Release();
    }

    public void Dispose()
    {
        Release();
    }
}
