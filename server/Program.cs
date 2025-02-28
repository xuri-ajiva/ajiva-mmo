using Riptide;
using Serilog;
using server;
using ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .MinimumLevel.Verbose()
    .CreateLogger();

builder.AddServiceDefaults();
{
    var services = builder.Services;

    services.AddSingleton(new ServerConfiguration { Port = 7777, MaxConnections = 100 });

    services.AddHostedService<Worker>();
    services.AddSingleton<Server>();
}

var app = builder.Build();

app.Run();
