using Common;
using Common.Config;
using Serilog;
using Server;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.OpenTelemetry()
    .MinimumLevel.Verbose()
    .CreateLogger();

builder.AddServiceDefaults();
var serverConfig = builder.Configuration
                       .GetSection(nameof(ServerConfiguration)).Get<ServerConfiguration>()
                   ?? new ServerConfiguration();

serverConfig.ClusterManagerAddress ??= builder.Configuration["services:clusterManager:management:0"];

var server = new ServerRunner(builder, serverConfig);
var iHost = builder.Build();

await server.RunAsync(iHost);