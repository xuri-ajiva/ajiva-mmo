using System.Diagnostics;
using System.Runtime.CompilerServices;
using Serilog;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ImageCopyTexture = Silk.NET.WebGPU.Safe.ImageCopyTexture;

namespace client.Renderer;

public record struct BufferRange(BufferPtr Buffer, ulong Offset, ulong Size);

public class RendererApi(ILogger<RendererApi> logger)
    : IHostedLifecycleService
{
    private bool _initialized;
    
    public DevicePtr Device;
    public AdapterPtr Adapter;
    public WebGPU Wgpu = null!;
    public InstancePtr Instance;

    public BufferRange CreateBufferWithData<T>(BufferUsage usage, T[] data, string? label = null)
        where T : unmanaged
    {
        var bufferSize = (ulong)(Unsafe.SizeOf<T>() * data.LongLength);

        var buffer = Device.CreateBuffer(usage,
            bufferSize,
            mappedAtCreation: true, label);

        var mappedBuffer = buffer.GetMappedRange<T>(0, (nuint)bufferSize);
        data.CopyTo(mappedBuffer);

        buffer.Unmap();
        return new(buffer, 0, bufferSize);
    }

    public (TexturePtr texture, TextureViewPtr view) CreateTextureFromRGBAImage(
        TextureUsage usage, Stream data, string? label = null)
    {
        using var image = Image.Load<Rgba32>(data);

        var pixelBuffer = new Rgba32[image.Width * image.Height];
        image.CopyPixelDataTo(pixelBuffer);

        var texture = Device.CreateTexture(
            usage | TextureUsage.CopyDst,
            TextureDimension.Dimension2D,
            new Extent3D
            {
                Width = (uint)image.Width,
                Height = (uint)image.Height,
                DepthOrArrayLayers = 1,
            },
            TextureFormat.Rgba8Unorm,
            mipLevelCount: 1,
            sampleCount: 1,
            viewFormats: new[]
            {
                TextureFormat.Rgba8Unorm
            },
            label: label);

        Device.GetQueue().WriteTexture<Rgba32>(
            new ImageCopyTexture
            {
                Aspect = TextureAspect.All,
                MipLevel = 0,
                Origin = new Origin3D(0, 0, 0),
                Texture = texture
            }, pixelBuffer,
            new TextureDataLayout
            {
                Offset = 0,
                BytesPerRow = 4 * (uint)image.Width,
                RowsPerImage = (uint)image.Height
            },
            new Extent3D((uint)image.Width, (uint)image.Height, 1));

        var textureView = texture.CreateView(TextureFormat.Rgba8Unorm, TextureViewDimension.Dimension2D,
            TextureAspect.All,
            baseMipLevel: 0, mipLevelCount: 1, baseArrayLayer: 0, arrayLayerCount: 1,
            label: $"{label} - View");

        return (texture, textureView);
    }
    
    async Task Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        logger.LogInformation("Initializing RendererApi");
        Wgpu = WebGPU.GetApi();

        Instance = Wgpu.CreateInstance();
        logger.LogDebug("WGPU Instance: {Instance:X}", Instance.GetIntPtr());

        Adapter = await Instance.RequestAdapter(
            backendType: default,
            powerPreference: PowerPreference.Undefined
        );

        var features = Adapter.EnumerateFeatures();
        var limits = Adapter.GetLimits();
        var properties = Adapter.GetProperties();

        Device = await Adapter.RequestDevice(requiredLimits: limits, requiredFeatures: features,
            deviceLostCallback: (r, m) =>
            {
                var message = m?.Replace("\\r\\n", "\n").Replace("\\n", "\n").Replace("\\t", "\t");
                logger.LogCritical("Device lost with reason: {Reason} and message: {Message}", r, message);
            },
            defaultQueueLabel: "DefaultQueue", label: "MainDevice");

        Device.SetUncapturedErrorCallback((type, m) =>
        {
            var message = m?.Replace("\\r\\n", "\n").Replace("\\n", "\n").Replace("\\t", "\t");
            switch (type)
            {
                case ErrorType.NoError:
                    logger.LogInformation("No error with message: {Message}", message);
                    break;
                case ErrorType.Validation:
                    logger.LogWarning("Validation error with message: {Message}", message);
                    break;
                case ErrorType.OutOfMemory:
                    logger.LogCritical("Out of memory error with message: {Message}", message);
                    break;
                case ErrorType.Internal:
                    logger.LogError("Internal error with message: {Message}", message);
                    break;
                case ErrorType.DeviceLost:
                    logger.LogCritical("Device lost error with message: {Message}", message);
                    break;
                case ErrorType.Unknown:
                case ErrorType.Force32:
                default:
                    logger.LogError("Unknown error type: {Type} with message: {Message}", type, message);
                    break;
            }
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken) => await Initialize();
    public async Task StartingAsync(CancellationToken cancellationToken) => await Initialize();

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
