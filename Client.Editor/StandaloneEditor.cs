using Client;
using Client.Config;
using Client.Desktop;
using Client.Editor;
using Client.Editor.EditorLayers;
using Client.Editor.EditorLayers.Widgets;
using Client.Editor.Management;
using Client.LayerSystem;
using Common;
using Common.Config;
using Serilog;
using Serilog.Expressions;
using Serilog.Settings.Configuration;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    DisableDefaults = true,
    ApplicationName = "Ajiva.MMO.Client",
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
    .WriteTo.OpenTelemetry()
    .WriteTo.Sink(logWidget)
//    .MinimumLevel.Verbose()
    .CreateLogger();

builder.AddServiceDefaults();
builder.Services.AddSingleton(logWidget);
builder.Services.AddSingleton(builder.Configuration.GetSection("Window").Get<WindowConfig>() ?? new WindowConfig());

var clientConfiguration = builder.Configuration.GetSection(nameof(ClientConfiguration)).Get<ClientConfiguration>();
var client = new DesktopClientRunner<DesktopApplication>(builder, clientConfiguration);
builder.Services.AddScoped<EditorLayer>();
builder.Services.AddScoped<IEditorWidget, NetworkWidgets>();
builder.Services.AddScoped<IEditorWidget, EntityWidgets>();
builder.Services.AddService<ManagementClient>();
var host = builder.Build();
var stack = host.Services.GetRequiredService<LayerStack>();
stack.PushLayer<EditorLayer>();
await client.RunAsync(host);