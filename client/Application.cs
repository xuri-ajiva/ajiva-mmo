using client.LayerSystem;
using client.Platform;
using client.Renderer;

namespace client;

public class Application : IHostedLifecycleService
{
    private readonly ILogger<Application> _logger;
    private readonly RendererApi _rendererApi;
    private readonly AjivaWindow _window;
    private readonly IServiceProvider _serviceProvider;
    private readonly AjivaImGuiLayerSystem _layerSystem;

    public Application(ILogger<Application> logger, RendererApi rendererApi, AjivaWindow window, IServiceProvider serviceProvider, AjivaImGuiLayerSystem layerSystem)
    {
        _logger = logger;
        _rendererApi = rendererApi;
        _window = window;
        _serviceProvider = serviceProvider;
        _layerSystem = layerSystem;
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application starting");
    }


    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application started");
    }


    public async Task StoppedAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application stopped");
    }

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Application stopping");
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var appScope = _serviceProvider.CreateScope();
        var scene = appScope.ServiceProvider.GetRequiredService<Simple3dScene>();

        await _layerSystem.PushLayer(scene);
        scene.Enabled = true;
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
