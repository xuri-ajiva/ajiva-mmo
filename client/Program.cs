using client;
using client.LayerSystem;
using client.Platform;
using client.Renderer;
using OpenTelemetry.Trace;
using Serilog;

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
    services.AddHostedService<Worker>();

    services.AddSingleton<AjivaImGuiLayerSystem>();
    services.AddSingleton<Simple3dScene>();
    
    services.AddSingleton<AjivaWindow>();
    services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<AjivaWindow>());

    services.AddSingleton<RendererApi>();
    services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<RendererApi>());
    // services.AddSingleton<RendererApi>(provider => provider.GetRequiredService<RendererApi>());

    services.AddHostedService<Application>();
}

var host = builder.Build();
{
    var services = host.Services;
    var platform = services.GetRequiredService<PlatformSystem>();

    platform.Init();

    host.Run();
    platform.Shotdown();
}
