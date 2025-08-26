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
    private readonly ClientConfiguration? _clientConfiguration;

    public ClientRunner(IHostApplicationBuilder builder, ClientConfiguration? clientConfiguration)
    {
        _clientConfiguration = clientConfiguration;
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
        _ = Task.Run(() => RunAutoConnect(app, instrumentation, logger));
        await app.RunAsync();

        platform.Shotdown();
    }

    private async Task RunAutoConnect(IHost app, Instrumentation tracer, ILogger<ClientRunner> logger)
    {
        if (_clientConfiguration is not { AutoConnect: true, AutoConnectAddress: not null })
            return;

        var client = app.Services.GetRequiredService<QClient>();
        var handler = app.Services.GetRequiredService<ClientNetworkManager>();

        while (!client.IsConnected)
        {
            using var span = tracer.ActivitySource.StartActivity(ActivityKind.Client);
            try
            {
                span?.AddEvent(new ActivityEvent("Connecting to server", DateTimeOffset.Now));
                await Task.Delay(1000);
                logger.LogInformation("Connecting client to server on {ServerAddress}",
                    _clientConfiguration.AutoConnectAddress);
                await client.ConnectAsync(handler,
                    _clientConfiguration.AutoConnectAddress.Replace("localhost", "127.0.0.1"));
                while (client.State == ConnectionState.Connecting)
                {
                    await Task.Delay(100);
                }
            }
            catch (Exception e)
            {
                span?.AddException(e);
                logger.LogError(e, "Failed to connect to server at {ServerAddress}",
                    _clientConfiguration.AutoConnectAddress);
                break;
            }

            if (client.IsConnected)
            {
                span?.AddEvent(new ActivityEvent(
                    "Connected to server",
                    DateTimeOffset.Now));
                span?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                span?.AddEvent(new ActivityEvent(
                    "Failed to connect to server",
                    DateTimeOffset.Now));
                span?.SetStatus(ActivityStatusCode.Error);
            }
        }
    }
}
