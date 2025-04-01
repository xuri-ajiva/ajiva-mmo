using System.Runtime.CompilerServices;
using Client.Renderer.PbrRenderer;
using Serilog;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.WGPU;
using Silk.NET.WebGPU.Safe;
using Silk.NET.WebGPU.Safe.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ImageCopyTexture = Silk.NET.WebGPU.Safe.ImageCopyTexture;
using ShaderModuleCompilationHint = Silk.NET.WebGPU.Safe.ShaderModuleCompilationHint;

namespace Client.Renderer;

public class RendererApi(ILogger<RendererApi> logger)
    : IHostedLifecycleService
{
    private bool _initialized;

    public DevicePtr Device;
    public AdapterPtr Adapter;
    public WebGPU Wgpu = null!;
    public InstancePtr Instance;
    private static Wgpu _wgpuExtension = null!;

    private async Task Initialize()
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

        if (!Device.TryGetExtension(out _wgpuExtension))
        {
            logger.LogCritical("Wgpu (wgpu-rs) extension is not supported");
            throw new Exception("Wgpu (wgpu-rs) extension is not supported");
        }

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

    public ShaderModulePtr CreateShaderModuleFromFile(string fileName,
        string? label = null, params Span<ShaderModuleCompilationHint> compilationHints)
    {
        try
        {
            using var fileStream = File.Open(fileName, FileMode.Open);

            var length = fileStream.Length;
            if (length < 2048)
            {
                Span<u8> code = stackalloc u8[(i32)length];
                fileStream.ReadExactly(code);
                return Device.CreateShaderModuleWGSL(code, compilationHints, label: label);
            }

            var buffer = new u8[length];
            fileStream.ReadExactly(buffer);
            return Device.CreateShaderModuleWGSL(buffer, compilationHints, label: label);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed to load shader module {FileName}", fileName);
            throw;
        }
    }

    public void Poll()
    {
        Device.GetExtension(_wgpuExtension)
            .Poll(true, null);
    }

    public async Task StartingAsync(CancellationToken cancellationToken) => await Initialize();
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        Device.Destroy();
        Adapter.Release();
        Instance.Release();
        Wgpu.Dispose();
        return Task.CompletedTask;
    }
}
