using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Client.Renderer.PbrRenderer;
using Client.Wgsl;
using Microsoft.CodeAnalysis.CSharp;
using Serilog;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.WGPU;
using Silk.NET.WebGPU.Safe;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Client.Renderer.Resources;

using System;

public class ResourceLoader(RendererApi api, ILogger<ResourceLoader> logger) : IHostedLifecycleService
{
    public readonly ILogger<ResourceLoader> Logger = logger;

    public AjivaTexture DefaultTexture = null!;

    private Task InitDefaultResources()
    {
        var extent3D = new Extent3D(64, 64, 1);
        var tex = CreateTexture(TextureUsage.TextureBinding, "DefaultTexture", 1, extent3D,
            TextureFormat.Rgba8Unorm, TextureAspect.All);

        Span<Rgba32> data = stackalloc Rgba32[(i32)(extent3D.Width * extent3D.Height)];

        //CoolTextureGenerator.GenerateCoolPattern(data, (i32)extent3D.Width, (i32)extent3D.Height);
        CoolTextureGenerator.GenerateMissingTexture(data, (i32)extent3D.Width, (i32)extent3D.Height);

        tex.WriteTexture<Rgba32>(data, extent3D);
        DefaultTexture = tex;

        return Task.CompletedTask;
    }

    public QueuePtr Queue => api.Device.GetQueue();

    public void AsyncTextureLoad(string path, AjivaTexture target, TextureUsage usage = TextureUsage.TextureBinding, string? label = null,
        u32 mipLevels = 1, TextureFormat format = TextureFormat.Rgba8Unorm, TextureAspect aspect = TextureAspect.All)
    {
        Task.Run(async () =>
        {
            try
            {
                await LoadTextureAndMips(path, target, usage, label ?? Path.GetFileNameWithoutExtension(path),
                    mipLevels, format, aspect);
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, "Failed to load texture {Path}", path);
                throw;
            }
        });
    }

    public AjivaTexture AsyncTextureLoad(string path, TextureUsage usage = TextureUsage.TextureBinding, string? label = null,
        u32 mipLevels = 1, TextureFormat format = TextureFormat.Rgba8Unorm, TextureAspect aspect = TextureAspect.All)
    {
        var result = DefaultTexture.Clone();
        AsyncTextureLoad(path, result, usage, label, mipLevels, format, aspect);
        return result;
    }

    private async Task LoadTextureAndMips(string path, AjivaTexture tex, TextureUsage usage, string? label,
        u32 requestedMipLevels, TextureFormat format, TextureAspect aspect)
    {
        await using var data = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var originalImage = Image.Load<Rgba32>(data);

        var extent3D = new Extent3D((u32)originalImage.Width, (u32)originalImage.Height, 1);

        var maxPossibleMipLevels = CalculateMaxMipLevels(extent3D);
        var effectiveMipLevels = Math.Max(1u, Math.Min(requestedMipLevels, maxPossibleMipLevels));

        var tmpTexture = CreateTexture(usage | TextureUsage.CopyDst, label, effectiveMipLevels, extent3D, format,
            aspect);
        tex.Swap(tmpTexture);

        var currentLevelImage = originalImage;

        try
        {
            for (u32 mipLevel = 0; mipLevel < effectiveMipLevels; ++mipLevel)
            {
                Image<Rgba32> imageToUpload;
                if (mipLevel != 0)
                {
                    var image = currentLevelImage;
                    var nextLevelImage = currentLevelImage.Clone(ctx => ctx.Resize(
                        Math.Max(1, image.Width / 2),
                        Math.Max(1, image.Height / 2),
                        KnownResamplers.Triangle));

                    if (mipLevel > 1)
                    {
                        currentLevelImage?.Dispose();
                    }

                    currentLevelImage = nextLevelImage;
                    imageToUpload = currentLevelImage;
                }
                else
                {
                    imageToUpload = originalImage;
                }

                var sizePixels = imageToUpload.Width * imageToUpload.Height; // Size in pixels

                var rent = ArrayPool<Rgba32>.Shared.Rent(sizePixels);
                try
                {
                    var pixelSpan = rent.AsSpan(0, sizePixels);
                    imageToUpload.CopyPixelDataTo(pixelSpan);
                    tex.WriteTexture<Rgba32>(pixelSpan,
                        new Extent3D((u32)imageToUpload.Width, (u32)imageToUpload.Height, 1), mipLevel: mipLevel);
                    Logger.LogTrace("Uploaded mip level {MipLevel} ({MipWidth}x{MipHeight}) for '{Label}'.", mipLevel,
                        imageToUpload.Width, imageToUpload.Height, label ?? path);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error processing/uploading mip level {MipLevel} for '{Label}'.", mipLevel,
                        label ?? path);
                    break;
                }
                finally
                {
                    ArrayPool<Rgba32>.Shared.Return(rent);
                }
            }
        }
        finally
        {
            if (effectiveMipLevels > 1)
            {
                currentLevelImage.Dispose();
            }
        }

        Logger.LogDebug("Successfully loaded texture '{Label}' with {EffectiveMipLevels} mip levels.", label ?? path,
            effectiveMipLevels);
        DisposeLater(tmpTexture);
    }

    private static u32 CalculateMaxMipLevels(Extent3D extent3D)
    {
        return (u32)MathF.Floor(MathF.Log2(Math.Max(extent3D.Width, extent3D.Height))) + 1;
    }

    private AjivaTexture CreateTexture(TextureUsage usage, string? label, u32 mipLevels, Extent3D extent3D,
        TextureFormat format, TextureAspect aspect)
    {
        var texture = api.Device.CreateTexture(
            usage | TextureUsage.CopyDst,
            TextureDimension.Dimension2D,
            extent3D,
            format,
            mipLevelCount: mipLevels,
            sampleCount: 1,
            viewFormats: new[] { format },
            label: label);

        var textureView = texture.CreateView(format, TextureViewDimension.Dimension2D,
            aspect, baseMipLevel: 0, mipLevelCount: 1, baseArrayLayer: 0, arrayLayerCount: 1,
            label: $"{label} - View");

        var tex = new AjivaTexture(this, texture, textureView, extent3D, aspect, mipLevels, true);
        return tex;
    }


    public AjivaBuffer<T> CreateBuffer<T>(BufferUsage usage, i32 count, string? label = null)
        where T : unmanaged
    {
        var bufferSize = (u64)Unsafe.SizeOf<T>() * (u64)count;
        var buffer = api.Device.CreateBuffer(usage, bufferSize, mappedAtCreation: false, label);
        var result = new AjivaBuffer<T>(this, buffer, 0, bufferSize, true);
        Logger.LogTrace("Created buffer {Label} with size {Size} bytes.", label, bufferSize);
        return result;
    }


    public AjivaBuffer<T> CreateBuffer<T>(BufferUsage usage, ReadOnlySpan<T> data, string? label = null)
        where T : unmanaged
    {
        var bufferSize = (u64)Unsafe.SizeOf<T>() * (u64)data.Length;

        var buffer = api.Device.CreateBuffer(usage, bufferSize, mappedAtCreation: true, label);
        var result = new AjivaBuffer<T>(this, buffer, 0, bufferSize, true);

        result.WriteBuffer(data);
        // Do not use queue here, as it is not guaranteed that the span is available after this method
        //result.QueuedWrite(data, Queue);
        Logger.LogTrace("Created buffer {Label} with size {Size} bytes and wrote {DataLength} elements.", label,
            bufferSize, data.Length);
        return result;
    }

    private readonly Channel<IDisposable> _disposables =
        Channel.CreateUnbounded<IDisposable>(new UnboundedChannelOptions { SingleReader = true });

    private void DisposeLater(IDisposable disposable)
    {
        // skip textures that are not supposed to be cleaned up (clone of default textures)
        if (disposable is AjivaTexture { CleanUp: false }) return;

        while (!_disposables.Writer.TryWrite(disposable))
        {
            Logger.LogWarning("Disposables Channel Full, waiting for space");
            Thread.Sleep(10);
        }
    }

    public Task StartingAsync(CancellationToken cancellationToken) => InitDefaultResources();
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
