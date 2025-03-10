using client;
using client.LayerSystem;
using client.Networking;
using client.Platform;
using client.Renderer;
using client.Renderer.PbrRenderer;
using Riptide;
using Serilog;
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
    .WriteTo.Sink(logWidget)
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .MinimumLevel.Verbose()
    .CreateLogger();

builder.AddServiceDefaults();
{
    var services = builder.Services;

    services.AddSingleton(logWidget);
    services.AddSingleton<PlatformSystem>();
    services.AddSingleton<Ecs>();

    services.AddSingleton<AjivaImGuiLayerSystem>();
    services.AddSingleton<Instanced3dDeferredRenderer>();
    services.AddSingleton<ClientConnectionLayer>();

    services.AddSingleton<AjivaWindow>();
    services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<AjivaWindow>());

    services.AddSingleton<RendererApi>();
    services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<RendererApi>());
    // services.AddSingleton<RendererApi>(provider => provider.GetRequiredService<RendererApi>());

    services.AddSingleton<Client>();
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
