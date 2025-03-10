using client.LayerSystem;
using client.Renderer;
using client.Renderer.PbrRenderer;

namespace client;

public class Application : IHostedLifecycleService
{
    private readonly ILogger<Application> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly AjivaImGuiLayerSystem _layerSystem;

    public Application(ILogger<Application> logger,
        IServiceProvider serviceProvider, AjivaImGuiLayerSystem layerSystem)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _layerSystem = layerSystem;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application starting");
        return Task.CompletedTask;
    }


    public Task StartedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application started");
        return Task.CompletedTask;
    }


    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application stopped");
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application stopping");
        return Task.CompletedTask;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        /*await _layerSystem.PushLayer<LogWidget>();
        await _layerSystem.PushLayer<Instanced3dDeferredRenderer>();
        await _layerSystem.PushLayer<ClientConnectionLayer>();*/
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
