using System.Runtime.CompilerServices;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using ImageCopyTexture = Silk.NET.WebGPU.Safe.ImageCopyTexture;

namespace Client.Renderer.Resources;

public class TextureData(
    ResourceManager loader,
    string label,
    TexturePtr texture,
    TextureViewPtr view,
    Extent3D size,
    TextureAspect aspect,
    u32 mipLevels) : GpuResource<TextureData>(loader, label)
{
    public TexturePtr Texture { get; private set; } = texture;
    public TextureViewPtr View { get; private set; } = view;
    public Extent3D Size { get; private set; } = size;
    public TextureAspect Aspect { get; private set; } = aspect;
    public u32 MipLevels { get; private set; } = mipLevels;

    public void WriteTexture<T>(ReadOnlySpan<T> data, Extent3D writeSize, Origin3D origin = default,
        u32 mipLevel = 0)
        where T : unmanaged
    {
        var iMipLevel = (i32)mipLevel;
        if (writeSize.Width == 0 || writeSize.Height == 0 || writeSize.DepthOrArrayLayers == 0)
        {
            Loader.Logger.LogInformation("Texture Write Size not set, using default size");
            writeSize.DepthOrArrayLayers = Size.DepthOrArrayLayers;
            writeSize.Height = Size.Height >> iMipLevel;
            writeSize.Width = Size.Width >> iMipLevel;
        }

        if (writeSize.Width > Size.Width >> iMipLevel)
        {
            Loader.Logger.LogInformation(
                $"{nameof(writeSize)}.Width > {nameof(Size)}.Width >> mipLevel ({{WriteSizeWidth}} > {{U}}) correcting...",
                writeSize.Width, Size.Width >> iMipLevel);
            writeSize.Width = Size.Width >> iMipLevel;
        }

        if (writeSize.Height > Size.Height >> iMipLevel)
        {
            Loader.Logger.LogInformation(
                $"{nameof(writeSize)}.Height > {nameof(Size)}.Height >> mipLevel ({{WriteSizeHeight}} > {{U}}) correcting...",
                writeSize.Height, Size.Height >> iMipLevel);
            writeSize.Height = Size.Height >> iMipLevel;
        }

        Loader.Queue.WriteTexture(
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


    protected override void Destroy()
    {
        View.Release();
        Texture.Destroy();
    }


    public void Swap(TextureData other)
    {
        var temp = (Texture, Label, View, Size, Aspect, MipLevels, clone: _clone);
        Texture = other.Texture;
        Label = other.Label;
        View = other.View;
        Size = other.Size;
        Aspect = other.Aspect;
        MipLevels = other.MipLevels;
        _clone = other._clone;
        Version++;
        other.Texture = temp.Texture;
        other.Label = temp.Label;
        other.View = temp.View;
        other.Size = temp.Size;
        other.Aspect = temp.Aspect;
        other.MipLevels = temp.MipLevels;
        other._clone = temp.clone;
        other.Version++;
    }

    public TextureData Clone(string? label)
    {
        return new TextureData(Loader, $"CopyOf::{Label}::as::{label}", Texture, View, Size, Aspect,
            MipLevels) { _clone = true };
    }
}

public class AjivaTexture : GpuResourceHandle<TextureData>
{
    private AjivaTexture(TextureData data) : base(data)
    {
    }

    public static AjivaTexture FromData(TextureData data) => new(data);
    public TexturePtr Texture => Data.Texture;
    public TextureViewPtr View => Data.View;
    public Extent3D Size => Data.Size;
    public TextureAspect Aspect => Data.Aspect;
    public u32 MipLevels => Data.MipLevels;
    public bool IsDisposed => Data.IsDisposed;

    public void WriteTexture<T>(ReadOnlySpan<T> data, Extent3D writeSize, Origin3D origin = default, u32 mipLevel = 0)
        where T : unmanaged => Data.WriteTexture(data, writeSize, origin, mipLevel);

    public AjivaTexture Reference()
    {
        return new AjivaTexture(Data);
    }

    //public static implicit operator AjivaTexture(TextureData texture) => texture.Reference();
    //public static implicit operator TexturePtr(AjivaTexture texture) => texture.Texture;
    public AjivaTexture Clone(string? label)
    {
        var clone = Data.Clone(label);
        return new AjivaTexture(clone);
    }
}
