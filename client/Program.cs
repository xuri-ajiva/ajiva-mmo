using System.Diagnostics.CodeAnalysis;
using Client;
using Client.LayerSystem;
using Client.Networking;
using Client.Platform;
using Client.Renderer;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Wgsl;
using Serilog;
using Serilog.Expressions;
using Serilog.Settings.Configuration;
using ServiceDefaults;

var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    DisableDefaults = true,
    ApplicationName = "Ajiva",
    ContentRootPath = Directory.GetCurrentDirectory(),
});
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

LogWidget logWidget = new();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration,
        new ConfigurationReaderOptions(typeof(ConsoleLoggerConfigurationExtensions).Assembly,
            typeof(SerilogExpression).Assembly))
    .Enrich.FromLogContext()
    .WriteTo.Sink(logWidget)
    .MinimumLevel.Verbose()
    .CreateLogger();

builder.AddServiceDefaults();
{
    var services = builder.Services;

    services.AddSingleton(logWidget);
    services.AddSingleton<PlatformSystem>();
    services.AddSingleton<Ecs>();
    services.AddSingleton<ModelHelper>();
    services.AddSingleton<ObjectScene>();
    services.AddSingleton<ICamera, FreeCamera>();


    services.AddSingleton<IAjivaLayerSystem, AjivaImGuiLayerSystem>();
    services.AddSingleton<Instanced3dDeferredRenderer>();
    services.AddSingleton<ClientConnectionLayer>();

    services.AddService<AjivaWindow>();

    services.AddService<RendererApi>();
    services.AddService<ResourceLoader>();
    services.AddService<PeriodicUpdater>();

    services.AddService<ShaderReloadManager, IShaderManager>();

    services.AddSingleton<Riptide.Client>();
    services.AddHostedService<ClientWorker>();
    services.AddHostedService<Application>();
}

var host = builder.Build();
{
    using var scop = host.Services.CreateScope();
    var services = scop.ServiceProvider;

    var platform = services.GetRequiredService<PlatformSystem>();
    platform.Init();

    await host.RunAsync();

    platform.Shotdown();
}

public static class ServiceExt
{
    public static IServiceCollection AddService<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TService>(
        this IServiceCollection services)
        where TService : class
    {
        services.AddSingleton<TService>();
        if (typeof(TService).IsAssignableTo(typeof(IHostedService)))
        {
            services.AddSingleton<IHostedService>(provider => (IHostedService)provider.GetRequiredService<TService>());
        }

        return services;
    }

    public static IServiceCollection AddService<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TService, TSecond>(
        this IServiceCollection services)
        where TService : class, TSecond
        where TSecond : class
    {
        services.AddSingleton<TService>();
        if (typeof(TService).IsAssignableTo(typeof(IHostedService)))
        {
            services.AddSingleton<IHostedService>(provider => (IHostedService)provider.GetRequiredService<TService>());
        }


        if (typeof(TService).IsAssignableTo(typeof(TSecond)))
        {
            services.AddSingleton<TSecond>(provider => provider.GetRequiredService<TService>());
        }

        return services;
    }
}
