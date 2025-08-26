using System.Diagnostics;
using System.Net.Quic;
using System.Net.Security;
using Ajiva.Ecs;
using Client.Game;
using Client.LayerSystem;
using Client.Networking;
using Client.Platform;
using Client.Renderer;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Wgsl;
using Common;
using Common.Config;
using Common.Networking;

namespace Client;

public class ClientRunner
{
    public ClientRunner(IHostApplicationBuilder builder, ClientConfiguration? clientConfiguration)
    {
        var services = builder.Services;
        services.AddSingleton(clientConfiguration ?? new ClientConfiguration());

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
        services.AddSingleton<QClient>();
        services.AddSingleton(new QuicClientConnectionOptions
        {
            DefaultCloseErrorCode = 12332, DefaultStreamErrorCode = 14323,
            ClientAuthenticationOptions = new SslClientAuthenticationOptions
            {
                ApplicationProtocols = [new SslApplicationProtocol("ice-rpc")],
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
            },
            IdleTimeout = TimeSpan.FromHours(1),
            HandshakeTimeout = TimeSpan.FromMinutes(1),
            MaxInboundBidirectionalStreams = 99,
            MaxInboundUnidirectionalStreams = 99,
        });

        // Background services
        services.AddHostedService<Application>();
        // Networking
        services.AddService<ClientNetworkManager>();
    }

    public async Task RunAsync(IHost app)
    {
        using var scop = app.Services.CreateScope();
        var services = scop.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<ClientRunner>>();
        var instrumentation = services.GetRequiredService<Instrumentation>();
        using var activity = instrumentation.StartActivity(ActivityKind.Client);

        var platform = services.GetRequiredService<PlatformSystem>();
        platform.Init();
        await app.RunAsync();
        platform.Shotdown();
    }
}
