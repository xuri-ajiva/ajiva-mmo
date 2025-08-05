using Client;
using Client.LayerSystem;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Expressions;
using Serilog.Settings.Configuration;
using server;
using ServiceDefaults;
using System.Diagnostics;
using Client.Config;
using Composer;
using Composer.Logging;
using Composer.ProcessOrchestrator;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using ServerConfiguration = Composer.ProcessOrchestrator.ServerConfiguration;

Console.WriteLine("Hello, World!");

var isSubprocess = args.Contains("--subprocess");
var inlineServerClient = args.Contains("--inline");
var debugPort = GetDebugPort(args);
var processName = GetProcessName(args) ?? "Composer";
var processType = GetProcessType(args);

//Debugger.Launch();

/*if (debugPort.HasValue && !Debugger.IsAttached)
{
    Console.WriteLine($"Process {processName} waiting for debugger on port {debugPort}");
}*/

if (inlineServerClient)
{
    var server = Task.Run(async () => await RunAsSubprocess(processName, "server", args));
    await RunAsSubprocess(processName, "client", args);
    await Waiter.CancellationTokenSource.CancelAsync();
    await server;
}
else if (isSubprocess)
{
    await RunAsSubprocess(processName, processType, args);
}
else
{
    await RunAsOrchestrator(args);
}

static async Task RunAsOrchestrator(string[] args)
{
    var config = LoadProcessConfiguration();
    using var processManager = new ProcessManager(config);

    await processManager.StartAllProcessesAsync();

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

static async Task RunAsSubprocess(string processName, string processType, string[] args)
{
    // Configure logging with named pipe sink
    var logger = new LoggerConfiguration()
        .WriteTo.Console(
            outputTemplate: "[{processName}] [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .Enrich.WithProperty("processName", processName)
        .Enrich.FromLogContext()
        .CreateLogger();

    Log.Logger = logger;

    logger.Information("Starting subprocess: {ProcessName} of type {ProcessType}", processName, processType);

    // Run specific process type
    switch (processType?.ToLower())
    {
        case "server":
            await RunServerSubprocess(processName, args);
            break;
        case "client":
            await RunClientSubprocess(processName, args);
            break;
        default:
            logger.Error("Unknown process type: {ProcessType}", processType);
            break;
    }
}

static async Task RunServerSubprocess(string processName, string[] args)
{
    var port = GetPort(args) ?? 7777;
    var maxConnections = GetMaxConnections(args) ?? 100;

    Log.Information("Starting server subprocess {ProcessName} on port {Port}", processName, port);

    Directory.SetCurrentDirectory("../client");

    var config = new ConfigurationBuilder()
        .AddJsonFile("../client/appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"../client/appsettings.{Environments.Development}.json", optional: true, reloadOnChange: true)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();

    var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
    {
        Args = args,
        DisableDefaults = true,
        ApplicationName = "Ajiva." + processName,
        ContentRootPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "client"),
    });

    builder.Configuration.AddConfiguration(config);
    builder.AddServiceDefaults();

    var server = new ServerRunner(builder,
        new server.ServerConfiguration { Port = (ushort)port, MaxConnections = (ushort)maxConnections });
    var app = builder.Build();
    await server.RunAsync(app, Waiter.CancellationTokenSource.Token);
}

static async Task RunClientSubprocess(string processName, string[] args)
{
    var windowConfig = GetWindowConfiguration(args);
    var port = GetPort(args) ?? 7777;

    Log.Information("Starting client subprocess {ProcessName}", processName);

    Directory.SetCurrentDirectory("../client");

    var config = new ConfigurationBuilder()
        .AddJsonFile("../client/appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"../client/appsettings.{Environments.Development}.json", optional: true, reloadOnChange: true)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();

    LogWidget logWidget = new();

    // Update logger to include LogWidget for client
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(config, new ConfigurationReaderOptions(
            typeof(ConsoleLoggerConfigurationExtensions).Assembly,
            typeof(SerilogExpression).Assembly))
        .Enrich.FromLogContext()
        .WriteTo.Sink(logWidget)
        .Enrich.WithProperty("processName", "client")
        .CreateLogger();

    var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
    {
        Args = args,
        DisableDefaults = true,
        ApplicationName = "Ajiva." + processName,
        ContentRootPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "client"),
    });

    builder.Configuration.AddConfiguration(config);
    builder.AddServiceDefaults();
    builder.Services.AddSingleton(logWidget);
    builder.Services.AddSingleton(windowConfig is not null
        ? ApplyWindowConfiguration(windowConfig, processName)
        : new WindowConfig
        {
            Title = "Ajiva MMO: " + processName,
            Width = 1920,
            Height = 1080,
            X = 100,
            Y = 100,
            Resizable = true,
            FullscreenBorderless = false
        });

    var client = new ClientRunner(builder);
    var app = builder.Build();
    _ = Task.Run(async () =>
    {
        await Task.Delay(1000);
        Log.Error("Connecting client {ProcessName} to server on port {Port}", processName, port);
        app.Services.GetRequiredService<Riptide.Client>()
            .Connect("127.0.0.1:" + port);
    });
    await client.RunAsync(app);
}

static WindowConfig ApplyWindowConfiguration(WindowConfiguration config, string processName)
{
    var (x, y, width, height) = CalculateWindowBounds(config);

    Log.Information("Window configuration for {ProcessName}: X={X}, Y={Y}, Width={Width}, Height={Height}",
        processName, x, y, width, height);

    return new WindowConfig
    {
        Title = "Ajiva MMO: " + processName,
        Width = width,
        Height = height,
        X = x,
        Y = y,
        Resizable = false,
        FullscreenBorderless = true
    };
}

static (int x, int y, int width, int height) CalculateWindowBounds(WindowConfiguration config)
{
    if (config.SplitMode == WindowSplitMode.None)
    {
        return (config.X, config.Y, config.Width, config.Height);
    }

    const int screenWidth = 2560;
    const int screenHeight = 1080;

    return config.SplitMode switch
    {
        WindowSplitMode.LeftHalf => (0, 0, screenWidth / 2, screenHeight),
        WindowSplitMode.RightHalf => (screenWidth / 2, 0, screenWidth / 2, screenHeight),
        WindowSplitMode.TopHalf => (0, 0, screenWidth, screenHeight / 2),
        WindowSplitMode.BottomHalf => (0, screenHeight / 2, screenWidth, screenHeight / 2),
        WindowSplitMode.TopLeft => (0, 0, screenWidth / 2, screenHeight / 2),
        WindowSplitMode.TopRight => (screenWidth / 2, 0, screenWidth / 2, screenHeight / 2),
        WindowSplitMode.BottomLeft => (0, screenHeight / 2, screenWidth / 2, screenHeight / 2),
        WindowSplitMode.BottomRight => (screenWidth / 2, screenHeight / 2, screenWidth / 2, screenHeight / 2),
        _ => (config.X, config.Y, config.Width, config.Height)
    };
}

static ProcessConfiguration LoadProcessConfiguration()
{
    return new ProcessConfiguration
    {
        BaseDebugPort = 5000,
        ClientConfigurations =
        [
            new ClientConfiguration
            {
                Id = 1,
                Name = "Player1",
                WindowConfiguration = new WindowConfiguration
                {
                    SplitMode = WindowSplitMode.LeftHalf,
                    Width = 2560,
                    Height = 1080
                }
            },

            new ClientConfiguration
            {
                Id = 2,
                Name = "Player2",
                WindowConfiguration = new WindowConfiguration
                {
                    SplitMode = WindowSplitMode.RightHalf,
                    Width = 2560,
                    Height = 1080
                }
            }
        ],
        ServerConfigurations = new List<ServerConfiguration>
        {
            new() { Id = 1, Name = "GameServer", Port = 7777 }
        }
    };
}

static int? GetDebugPort(string[] args)
{
    var debugPortIndex = Array.IndexOf(args, "--debug-port");
    if (debugPortIndex >= 0 && debugPortIndex + 1 < args.Length)
    {
        if (int.TryParse(args[debugPortIndex + 1], out var port))
            return port;
    }

    return null;
}

static string? GetProcessName(string[] args)
{
    var processNameIndex = Array.IndexOf(args, "--process-name");
    if (processNameIndex >= 0 && processNameIndex + 1 < args.Length)
    {
        return args[processNameIndex + 1];
    }

    return null;
}

static string? GetProcessType(string[] args)
{
    var processTypeIndex = Array.IndexOf(args, "--process-type");
    if (processTypeIndex >= 0 && processTypeIndex + 1 < args.Length)
    {
        return args[processTypeIndex + 1];
    }

    return null;
}

static int? GetPort(string[] args)
{
    var portIndex = Array.IndexOf(args, "--port");
    if (portIndex >= 0 && portIndex + 1 < args.Length)
    {
        if (int.TryParse(args[portIndex + 1], out var port))
            return port;
    }

    return null;
}

static int? GetMaxConnections(string[] args)
{
    var maxConnectionsIndex = Array.IndexOf(args, "--max-connections");
    if (maxConnectionsIndex >= 0 && maxConnectionsIndex + 1 < args.Length)
    {
        if (int.TryParse(args[maxConnectionsIndex + 1], out var maxConnections))
            return maxConnections;
    }

    return null;
}

static WindowConfiguration? GetWindowConfiguration(string[] args)
{
    var configIndex = Array.IndexOf(args, "--config");
    if (configIndex >= 0 && configIndex + 1 < args.Length)
    {
        try
        {
            var configJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[configIndex + 1]));
            return System.Text.Json.JsonSerializer.Deserialize<WindowConfiguration>(configJson);
        }
        catch
        {
            // ignored
        }
    }

    return null;
}

internal static class Waiter
{
    public static readonly CancellationTokenSource CancellationTokenSource = new();
}
