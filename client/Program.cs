using client;
using client.LayerSystem;
using client.Platform;
using client.Renderer;
using OpenTelemetry.Trace;
using Serilog;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings()
{
    Args = args,
    DisableDefaults = true,
    ApplicationName = "Ajiva",
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.MySink()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .MinimumLevel.Verbose()
    .CreateLogger();

builder.AddServiceDefaults();
{
    var services = builder.Services;

    services.AddSerilog();
    services.AddSingleton(TracerProvider.Default.GetTracer(""));

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
