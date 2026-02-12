using System.Diagnostics;
using System.Runtime.CompilerServices;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.WGPU;
using Silk.NET.WebGPU.Safe;
using Silk.NET.WebGPU.Safe.Extensions;

namespace Client.Renderer;

public class RendererApi(ILogger<RendererApi> logger)
    : IHostedLifecycleService
{
    private bool _initialized;

    public DevicePtr Device;
    public AdapterPtr Adapter;
    private QueuePtr Queue;
    public WebGPU Wgpu = null!;
    public InstancePtr Instance;
    private static Wgpu _wgpuExtension = null!;
    private readonly SemaphoreSlim _frameQueueAccessWaitQueue = new(1, 1);
    private u32 _frameQueueHoldingThread = 0;
    internal readonly ILogger<RendererApi> _logger = logger;

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_waitCount")]
    public static extern i32 SemaphoreSlimWaitCount(SemaphoreSlim semaphoreSlim);

    private async Task Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        _logger.LogInformation("Initializing RendererApi");
        Wgpu = WebGPU.GetApi();

        Instance = Wgpu.CreateInstance();
        _logger.LogDebug("WGPU Instance: {Instance:X}", Instance.GetIntPtr());

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
                _logger.LogCritical("Device lost with reason: {Reason} and message: {Message}", r, message);
            },
            defaultQueueLabel: "DefaultQueue", label: "MainDevice");

        Device.SetUncapturedErrorCallback((type, m) =>
        {
            var message = m?.Replace(@"\r\n", "\n").Replace("\\n", "\n").Replace("\\t", "\t");
            switch (type)
            {
                case ErrorType.NoError:
                    _logger.LogInformation("No error with message: {Message}", message);
                    break;
                case ErrorType.Validation:
                    _logger.LogWarning("Validation error with message: {Message}", message);
                    return;
                case ErrorType.OutOfMemory:
                    _logger.LogCritical("Out of memory error with message: {Message}", message);
                    break;
                case ErrorType.Internal:
                    _logger.LogError("Internal error with message: {Message}", message);
                    break;
                case ErrorType.DeviceLost:
                    _logger.LogCritical("Device lost error with message: {Message}", message);
                    break;
                case ErrorType.Unknown:
                case ErrorType.Force32:
                default:
                    _logger.LogError("Unknown error type: {Type} with message: {Message}", type, message);
                    break;
            }

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        });

        if (!Device.TryGetExtension(out _wgpuExtension))
        {
            _logger.LogCritical("Wgpu (wgpu-rs) extension is not supported");
            throw new Exception("Wgpu (wgpu-rs) extension is not supported");
        }

        Queue = Device.GetQueue();
    }

    /*public ShaderModulePtr CreateShaderModuleFromFile(string fileName,
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
    }*/

    public void Poll()
    {
        Device.GetExtension(_wgpuExtension)
            .Poll(true, null);
    }

    public readonly ref struct QueueGuard : IDisposable
    {
        private readonly RendererApi _api;
        public QueuePtr Queue => _api.Queue;
        private readonly bool _skipped = false;

        public QueueGuard(RendererApi api)
        {
            _api = api;
            if (_api._frameQueueHoldingThread == (u32)Environment.CurrentManagedThreadId)
            {
                _skipped = true;
                return;
            }

            _api._frameQueueAccessWaitQueue.Wait();
            _api._frameQueueHoldingThread = (u32)Environment.CurrentManagedThreadId;
        }

        public void Dispose()
        {
            if (_skipped)
            {
                return;
            }

            _api._frameQueueAccessWaitQueue.Release();
            _api._frameQueueHoldingThread = 0;
        }
    }

    public QueueGuard EnterQueue() => new(this);

    public async Task StartingAsync(CancellationToken cancellationToken) => await Initialize();
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StoppedAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _frameQueueAccessWaitQueue.WaitAsync(cancellationToken);
        }
        finally
        {
            Device.Destroy();
            Adapter.Release();
            Instance.Release();
            Wgpu.Dispose();
        }
    }
}