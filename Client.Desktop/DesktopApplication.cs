using Client.Common;
using Client.Desktop.Platform;
using Client.Game;
using Client.LayerSystem;
using Client.Renderer;
using Client.UI.Desktop;

namespace Client.Desktop;

public class DesktopApplication(
    ILogger<DesktopApplication> logger,
    IServiceProvider serviceProvider,
    LayerStack layerSystem)
    : IApplication
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<DesktopApplication> _logger = logger;
    private readonly LayerStack _layerSystem = layerSystem;

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
        _layerSystem.PushLayer<EventSystem>();
        _layerSystem.PushLayer<UiRenderLayer>();
        _layerSystem.PushLayer<ExampleGameHudLayer>();
        _layerSystem.PushLayer<PlayerInputSystem>();
        _layerSystem.PushLayer<LogWidget>();
        _layerSystem.PushLayer<ViewPortRenderer>();
        _layerSystem.PushLayer<AjivaImGuiLayer>();
        _layerSystem.PushLayer<ClientConnectionLayer>();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}