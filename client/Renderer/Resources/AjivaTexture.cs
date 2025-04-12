using System.Runtime.CompilerServices;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ImageCopyTexture = Silk.NET.WebGPU.Safe.ImageCopyTexture;

namespace Client.Renderer.Resources;

public class AjivaTexture(
    ResourceLoader loader,
    TexturePtr texture,
    TextureViewPtr view,
    Extent3D size,
    TextureAspect aspect,
    u32 mipLevels) : IAjivaVersionedObject
{
    private readonly ResourceLoader _loader = loader;
    public TexturePtr Texture { get; private set; } = texture;
    public TextureViewPtr View { get; private set; } = view;
    public Extent3D Size { get; private set; } = size;
    public TextureAspect Aspect { get; private set; } = aspect;
    public u32 MipLevels { get; private set; } = mipLevels;
    private bool _clone;
    private i32 _refCount = 1;
    public bool IsClone => _clone;

    public void WriteTexture<T>(ReadOnlySpan<T> data, Extent3D writeSize, Origin3D origin = default, u32 mipLevel = 0)
        where T : unmanaged
    {
        var iMipLevel = (i32)mipLevel;
        if (writeSize.Width == 0 || writeSize.Height == 0 || writeSize.DepthOrArrayLayers == 0)
        {
            _loader.Logger.LogInformation("Texture Write Size not set, using default size");
            writeSize.DepthOrArrayLayers = Size.DepthOrArrayLayers;
            writeSize.Height = Size.Height >> iMipLevel;
            writeSize.Width = Size.Width >> iMipLevel;
        }

        if (writeSize.Width > Size.Width >> iMipLevel)
        {
            _loader.Logger.LogInformation(
                $"{nameof(writeSize)}.Width > {nameof(size)}.Width >> mipLevel ({{WriteSizeWidth}} > {{U}}) correcting...",
                writeSize.Width, Size.Width >> iMipLevel);
            writeSize.Width = Size.Width >> iMipLevel;
        }

        if (writeSize.Height > Size.Height >> iMipLevel)
        {
            _loader.Logger.LogInformation(
                $"{nameof(writeSize)}.Height > {nameof(size)}.Height >> mipLevel ({{WriteSizeHeight}} > {{U}}) correcting...",
                writeSize.Height, Size.Height >> iMipLevel);
            writeSize.Height = Size.Height >> iMipLevel;
        }

        _loader.Queue.WriteTexture(
            new ImageCopyTexture
            {
                Texture = Texture,
                MipLevel = mipLevel,
                Origin = origin,
                Aspect = Aspect
            },
            data,
            new TextureDataLayout
            {
                Offset = 0,
                BytesPerRow = (u32)(writeSize.Width * Unsafe.SizeOf<T>()),
                RowsPerImage = writeSize.Height
            },
            writeSize);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (--_refCount > 0) return;
        if (_clone) return;
        if (_refCount < 0)
        {
            _loader.Logger.LogInformation("Released texture {Texture}", Texture.GetIntPtr());
            View.Release();
            Texture.Destroy();
        }
        else
        {
            _loader.DisposeLater(this);
        }
    }

    public bool IsDisposed => _refCount < 0;

    public u64 Version { get; private set; } = IAjivaVersionedObject.Sentinel;

    public void Swap(AjivaTexture other)
    {
        var temp = (Texture, View, Size, Aspect, MipLevels, clone: _clone);
        Texture = other.Texture;
        View = other.View;
        Size = other.Size;
        Aspect = other.Aspect;
        MipLevels = other.MipLevels;
        _clone = other._clone;
        Version++;
        other.Texture = temp.Texture;
        other.View = temp.View;
        other.Size = temp.Size;
        other.Aspect = temp.Aspect;
        other.MipLevels = temp.MipLevels;
        other._clone = temp.clone;
        other.Version++;
    }

    public AjivaTexture Clone()
    {
        return new AjivaTexture(_loader, Texture, View, Size, Aspect, MipLevels) { _clone = true };
    }

    public AjivaTexture Reference()
    {
        _refCount++;
        return this;
    }
}
