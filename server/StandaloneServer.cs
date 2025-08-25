using Common;
using Common.Config;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Server;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.OpenTelemetry()
    .MinimumLevel.Verbose()
    .CreateLogger();

builder.AddServiceDefaults();
var serverConfig = builder.Configuration
                       .GetSection(nameof(ServerConfiguration)).Get<ServerConfiguration>()
                   ?? new ServerConfiguration { Port = 7777, MaxConnections = 100 };

var server = new ServerRunner(builder, serverConfig);
var iHost = builder.Build();
iHost.MapGet("/", () => "Hello World!");

await server.RunAsync(iHost);
