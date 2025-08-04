using Client.LayerSystem;
using Client.Renderer.PbrRenderer;

namespace Client;

public class Application : IHostedLifecycleService
{
    private readonly ILogger<Application> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAjivaLayerSystem _layerSystem;

    public Application(ILogger<Application> logger,
        IServiceProvider serviceProvider, IAjivaLayerSystem layerSystem)
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


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _layerSystem.PushLayer<LogWidget>();
        _layerSystem.PushLayer<Instanced3dDeferredRenderer>();
        _layerSystem.PushLayer<ClientConnectionLayer>();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
