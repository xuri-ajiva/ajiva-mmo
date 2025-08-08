using Client;
using Client.Config;
using Client.LayerSystem;
using Common;
using Serilog;
using Serilog.Expressions;
using Serilog.Settings.Configuration;

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
//    .MinimumLevel.Verbose()
    .CreateLogger();

builder.AddServiceDefaults();
builder.Services.AddSingleton(logWidget);
builder.Services.AddSingleton(builder.Configuration.GetSection("Window").Get<WindowConfig>() ?? new WindowConfig());

var client = new ClientRunner(builder);
await client.RunAsync(builder.Build());
