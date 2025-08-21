using Ajiva.Ecs;
using Client.Game;
using Client.LayerSystem;
using Client.Networking;
using Client.Platform;
using Client.Renderer;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Wgsl;

namespace Client;

public class ClientRunner
{
    public ClientRunner(HostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Platform and core systems
        services.AddSingleton<PlatformSystem>();
        services.AddSingleton<ModelHelper>();
        services.AddSingleton<Scene>();
        services.AddSingleton<ICamera, FreeCamera>();

        // EcsV2 Game Logic
        services.AddSingleton<ClientGameLogic>();
        services.AddSingleton<ClientEcsWorld>();
        services.AddService<PlayerInputSystem>();

        // UI/Rendering
        services.AddSingleton<IAjivaLayerSystem, AjivaImGuiLayerSystem>();
        services.AddSingleton<Instanced3dDeferredRenderer>();
        services.AddSingleton<ClientConnectionLayer>();

        services.AddService<AjivaWindow>();

        services.AddService<RendererApi>();
        services.AddService<ResourceManager>();

        services.AddService<PeriodicUpdater>();
        services.AddService<ShaderReloadManager, IShaderManager>();

        // Riptide client
        services.AddSingleton<Riptide.Client>();

        // Background services
        services.AddHostedService<Application>();
        // Networking
        services.AddService<ClientNetworkManager>();
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
