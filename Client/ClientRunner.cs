using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Client.Assets;
using Client.Game;
using Client.LayerSystem;
using Client.Networking;
using Client.Renderer;
using Client.Renderer.Instancing;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Wgsl;
using Common;
using Common.Config;
using Common.Ecs;
using Common.Systems;

namespace Client;

public interface IApplication : IHostedLifecycleService;

public class ClientRunner<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApplication> where TApplication : class, IApplication
{
    public ClientRunner(IHostApplicationBuilder builder, ClientConfiguration? clientConfiguration)
    {
        var services = builder.Services;
        services.AddSingleton(clientConfiguration ?? new ClientConfiguration());

        // Platform and core systems
        services.AddSingleton<ModelHelper>();
        services.AddSingleton<Scene>();
        services.AddSingleton<IRenderableInstanceCollectionBuilder, UnifiedRenderableInstanceCollectionBuilder>();
        services.AddSingleton<ICamera, FreeCamera>();

        // Asset management
        services.AddSingleton<EntityAssetRegistry>();

        // Ecs Game Logic
        services.AddSingleton<ClientGameLogic>();
        services.AddSingleton<NClientEcsWorld>();
        services.AddService<PlayerInputSystem>();

        // UI/Rendering
        services.AddSingleton<LayerStack>();
        services.AddSingleton<Instanced3dDeferredRenderer>();
        services.AddSingleton<ClientConnectionLayer>();


        services.AddService<RendererApi>();
        services.AddService<ResourceManager>();

        services.AddService<PeriodicUpdater>();
        services.AddService<ShaderReloadManager, IShaderManager>();

        // Background services
        services.AddHostedService<TApplication>();
        // Networking
        services.AddService<ClientNetworkManager>();
    }

    public async Task RunAsync(IHost app)
    {
        using var scop = app.Services.CreateScope();
        var services = scop.ServiceProvider;

        var instrumentation = services.GetRequiredService<Instrumentation>();
        using var activity = instrumentation.StartActivity(ActivityKind.Client);

        await RunAsync(app, scop);
    }

    protected virtual Task RunAsync(IHost app, IServiceScope root)
    {
        return app.RunAsync();
    }
}