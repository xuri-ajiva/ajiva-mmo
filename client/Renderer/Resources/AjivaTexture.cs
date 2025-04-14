using System.Runtime.CompilerServices;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using ImageCopyTexture = Silk.NET.WebGPU.Safe.ImageCopyTexture;

namespace Client.Renderer.Resources;

public class TextureData(
    ResourceLoader loader,
    string name,
    TexturePtr texture,
    TextureViewPtr view,
    Extent3D size,
    TextureAspect aspect,
    u32 mipLevels) : IAjivaVersionedObject
{
    public string Name { get; set; } = name;
    private readonly ResourceLoader _loader = loader;
    public TexturePtr Texture { get; private set; } = texture;
    public TextureViewPtr View { get; private set; } = view;
    public Extent3D Size { get; private set; } = size;
    public TextureAspect Aspect { get; private set; } = aspect;
    public u32 MipLevels { get; private set; } = mipLevels;
    private bool _clone;
    internal i32 RefCount;
    public bool IsClone => _clone;

    public void WriteTexture<T>(ReadOnlySpan<T> data, Extent3D writeSize, Origin3D origin = default,
        u32 mipLevel = 0)
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
                $"{nameof(writeSize)}.Width > {nameof(Size)}.Width >> mipLevel ({{WriteSizeWidth}} > {{U}}) correcting...",
                writeSize.Width, Size.Width >> iMipLevel);
            writeSize.Width = Size.Width >> iMipLevel;
        }

        if (writeSize.Height > Size.Height >> iMipLevel)
        {
            _loader.Logger.LogInformation(
                $"{nameof(writeSize)}.Height > {nameof(Size)}.Height >> mipLevel ({{WriteSizeHeight}} > {{U}}) correcting...",
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

    // Dispose / Release / Dereference
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (Interlocked.Decrement(ref RefCount) > 0) return;
        if (_clone) return;
        switch (RefCount)
        {
            case 0:
                _loader.Logger.LogInformation("Released texture {Texture}", Name);
                View.Release();
                Texture.Destroy();
                break;
            case < 0:
                _loader.Logger.LogError("Texture {Texture} RefCount is negative, this should not happen", Name);
                break;
        }
    }

    public bool IsDisposed => RefCount <= 0;

    public u64 Version { get; private set; } = IAjivaVersionedObject.Sentinel;

    public void Swap(TextureData other)
    {
        var temp = (Texture, Name, View, Size, Aspect, MipLevels, clone: _clone);
        Texture = other.Texture;
        Name = other.Name;
        View = other.View;
        Size = other.Size;
        Aspect = other.Aspect;
        MipLevels = other.MipLevels;
        _clone = other._clone;
        Version++;
        other.Texture = temp.Texture;
        other.Name = temp.Name;
        other.View = temp.View;
        other.Size = temp.Size;
        other.Aspect = temp.Aspect;
        other.MipLevels = temp.MipLevels;
        other._clone = temp.clone;
        other.Version++;
    }

    public TextureData Clone(string? label)
    {
        return new TextureData(_loader, $"CopyOf::{Name}::as::{label}", Texture, View, Size, Aspect,
            MipLevels) { _clone = true };
    }
}

public class AjivaTexture : IAjivaVersionedObject
{
    public TextureData Data { get; private set; }

    private AjivaTexture(TextureData data)
    {
        Interlocked.Increment(ref data.RefCount);
        Data = data;
    }

    public AjivaTexture(AjivaTexture texture) : this(texture.Data)
    {
    }

    public static AjivaTexture FromData(TextureData data) => new(data);


    ~AjivaTexture()
    {
        Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Data.Dispose();
    }

    private u64 _version = IAjivaVersionedObject.Sentinel;


    public u64 Version => _version + Data.Version;
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

    internal void Assign(TextureData tmpTexture)
    {
        Interlocked.Increment(ref tmpTexture.RefCount);
        _version += 1 + (Data.Version > tmpTexture.Version ? 1ul : 0ul) * (Data.Version - tmpTexture.Version);
        Data.Dispose();
        Data = tmpTexture;
    }

    //public static implicit operator AjivaTexture(TextureData texture) => texture.Reference();
    //public static implicit operator TexturePtr(AjivaTexture texture) => texture.Texture;
    public AjivaTexture Clone(string? label)
    {
        var clone = Data.Clone(label);
        return new AjivaTexture(clone);
    }
}
