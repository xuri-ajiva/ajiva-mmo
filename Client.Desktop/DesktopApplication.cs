using Client.Common;
using Client.Desktop.Platform;
using Client.Game;
using Client.LayerSystem;
using Client.Renderer;
using Client.UI.Desktop;

namespace Client.Desktop;

public class DesktopApplication(ILogger<DesktopApplication> logger, LayerStack layerSystem) : IApplication
{
    public Task StartingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Application starting");
        return Task.CompletedTask;
    }


    public Task StartedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Application started");
        return Task.CompletedTask;
    }


    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Application stopped");
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Application stopping");
        return Task.CompletedTask;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        layerSystem.PushLayer<WindowingLayer>();
        layerSystem.PushLayer<UiRenderLayer>();
        layerSystem.PushLayer<ExampleGameHudLayer>();
        layerSystem.PushLayer<PlayerInputSystem>();
        layerSystem.PushLayer<LogWidget>();
        layerSystem.PushLayer<ViewPortRenderer>();
        layerSystem.PushLayer<AjivaImGuiLayer>();
        layerSystem.PushLayer<ClientConnectionLayer>();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}