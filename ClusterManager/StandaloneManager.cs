using Common;
using Common.Config;
using Serilog;
using ClusterManager;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.OpenTelemetry()
    .MinimumLevel.Verbose()
    .CreateLogger();

builder.AddServiceDefaults();

var serverConfig = builder.Configuration
                       .GetSection(nameof(ClusterConfiguration)).Get<ClusterConfiguration>()
                   ?? new ClusterConfiguration { ManagementPort = 9999, EntryPort = 7777 };

var services = builder.Services;

services.AddSingleton(serverConfig);

services.AddService<ClusterServer>();
var iHost = builder.Build();

await iHost.RunAsync();