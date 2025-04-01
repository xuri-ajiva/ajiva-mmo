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
using ImageCopyTexture = Silk.NET.WebGPU.Safe.ImageCopyTexture;

namespace Client.Renderer.Resources;

using System;

public class ResourceLoader(RendererApi api, ILogger<ResourceLoader> logger) : IHostedLifecycleService
{
    public readonly ILogger<ResourceLoader> Logger = logger;

    private async Task InitDefaultResources()
    {
        // nothing atm
    }

    public QueuePtr Queue => api.Device.GetQueue();


    public AjivaTexture CreateTextureFromRgbaImage(
        TextureUsage usage, string path, string? label = null, u32 mipLevels = 1)
    {
        const TextureFormat format = TextureFormat.Rgba8Unorm;
        const TextureAspect aspect = TextureAspect.All;

        var tex = CreateTexture(usage, "async_placeholder:" + label, mipLevels, new Extent3D(1, 1, 1), format, aspect);

        Task.Run(async () => await LoadTexture(path, tex, label));

        return tex;
    }

    private async Task LoadTexture(string path, AjivaTexture tex, string? label)
    {
        await using var data = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var image = Image.Load<Rgba32>(data);

        var extent3D = new Extent3D((u32)image.Width, (u32)image.Height, 1);

        var rent = ArrayPool<Rgba32>.Shared.Rent(image.Width * image.Height);
        var pixelBuffer = rent.AsSpan(0, image.Width * image.Height);
        image.CopyPixelDataTo(pixelBuffer);

        var tmpTexture = CreateTexture(tex.Texture.GetUsage(), label, tex.MipLevels, extent3D, tex.Texture.GetFormat(),
            tex.Aspect);
        tmpTexture.WriteTexture<Rgba32>(pixelBuffer, extent3D);
        ArrayPool<Rgba32>.Shared.Return(rent);
        tex.Swap(tmpTexture);
        if (tex.MipLevels > 1)
        {
            await tex.GenerateMipmaps();
        }

        DisposeLater(tmpTexture);
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

    public AjivaTexture LoadTextureAsync(string path)
    {
        try
        {
            return CreateTextureFromRgbaImage(TextureUsage.TextureBinding, path, path);
        }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Failed to load texture {Path}", path);
            throw;
        }
    }

    public AjivaBuffer<T> CreateBuffer<T>(BufferUsage usage, i32 count, string? label = null)
        where T : unmanaged
    {
        var bufferSize = (u64)Unsafe.SizeOf<T>() * (u64)count;
        var buffer = api.Device.CreateBuffer(usage, bufferSize, mappedAtCreation: false, label);
        var result = new AjivaBuffer<T>(this, buffer, 0, bufferSize, true);
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

        return result;
    }

    private readonly Channel<IDisposable> _disposables =
        Channel.CreateUnbounded<IDisposable>(new UnboundedChannelOptions { SingleReader = true, });

    private void DisposeLater(IDisposable disposable)
    {
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
