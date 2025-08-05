using Client.LayerSystem;
using Client.Networking;
using Client.Platform;
using Client.Renderer;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Wgsl;
using Common.Ecs;

namespace Client;

public class ClientRunner
{
    public ClientRunner(HostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddSingleton<PlatformSystem>();
        services.AddSingleton<GameLogic>();
        services.AddSingleton<World>();
        services.AddSingleton<ModelHelper>();
        services.AddSingleton<Scene>();
        services.AddSingleton<ICamera, FreeCamera>();


        services.AddSingleton<IAjivaLayerSystem, AjivaImGuiLayerSystem>();
        services.AddSingleton<Instanced3dDeferredRenderer>();
        services.AddSingleton<ClientConnectionLayer>();

        services.AddService<AjivaWindow>();

        services.AddService<RendererApi>();
        services.AddService<ResourceManager>();
        services.AddService<PeriodicUpdater>();

        services.AddService<ShaderReloadManager, IShaderManager>();

        services.AddSingleton<Riptide.Client>();
        services.AddHostedService<ClientWorker>();
        services.AddHostedService<Application>();
    }

    public async Task RunAsync(IHost app)
    {
        using var scop = app.Services.CreateScope();
        var services = scop.ServiceProvider;

        var platform = services.GetRequiredService<PlatformSystem>();
        platform.Init();

        await app.RunAsync();

        platform.Shotdown();
    }
}
