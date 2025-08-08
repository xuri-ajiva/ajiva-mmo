using Common;
using Serilog;
using Server;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .MinimumLevel.Verbose()
    .CreateLogger();

builder.AddServiceDefaults();

var server = new ServerRunner(builder, new ServerConfiguration { Port = 7777, MaxConnections = 100 });
await server.RunAsync(builder.Build());
