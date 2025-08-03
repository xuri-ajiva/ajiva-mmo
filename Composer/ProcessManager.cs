using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;
using Composer.ProcessOrchestrator;
using Serilog;

namespace Composer;

public partial class ProcessManager : IDisposable
{
    private readonly ILogger _logger;
    private readonly List<ManagedProcess> _processes = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _logListenerTask;
    private readonly ProcessConfiguration _config;

    public ProcessManager(ProcessConfiguration config)
    {
        _config = config;
        _logger = ConfigureLogger();
    }

    private ILogger ConfigureLogger()
    {
        return new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/orchestrator-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public async Task StartAllProcessesAsync()
    {
        _logger.Information("Starting Ajiva Compositor Process Orchestrator");

        // Start client/server configurations
        foreach (var clientConfig in _config.ClientConfigurations)
        {
            await StartClientProcessAsync(clientConfig);
        }

        foreach (var serverConfig in _config.ServerConfigurations)
        {
            await StartServerProcessAsync(serverConfig);
        }

        // Monitor processes
        _ = Task.Run(MonitorProcesses);
    }

    private async Task StartClientProcessAsync(ClientConfiguration config)
    {
        var debugPort = _config.BaseDebugPort + 10 + config.Id;
        var windowConfig = JsonSerializer.Serialize(config.WindowConfiguration);

        var processInfo = new ProcessStartInfo
        {
            FileName = _config.ExecutablePath,
            Arguments =
                $"--subprocess --process-type client --config \"{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(windowConfig))}\" --debug-port {debugPort} --process-name Client-{config.Name}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true
        };

        var process = new ManagedProcess($"Client-{config.Name}", processInfo, debugPort);
        await StartManagedProcessAsync(process);
    }

    private async Task StartServerProcessAsync(ServerConfiguration config)
    {
        var debugPort = _config.BaseDebugPort + 20 + config.Id;

        var processInfo = new ProcessStartInfo
        {
            FileName = _config.ExecutablePath,
            Arguments =
                $"--subprocess --process-type server --port {config.Port} --debug-port {debugPort} --process-name Server-{config.Name}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true
        };

        var process = new ManagedProcess($"Server-{config.Name}", processInfo, debugPort);
        await StartManagedProcessAsync(process);
    }

    private async Task StartManagedProcessAsync(ManagedProcess managedProcess)
    {
        _logger.Information("Starting process: {ProcessName} (Debug port: {DebugPort})",
            managedProcess.Name, managedProcess.DebugPort);

        managedProcess.Process = Process.Start(managedProcess.StartInfo);
        if (managedProcess.Process == null)
        {
            _logger.Error("Failed to start process: {ProcessName}", managedProcess.Name);
            return;
        }

        _processes.Add(managedProcess);

        // Redirect output to centralized logger
        _ = Task.Run(() => RedirectOutput(managedProcess));
    }

    private async Task RedirectOutput(ManagedProcess managedProcess)
    {
        var process = managedProcess.Process;
        var name = managedProcess.Name;

        _ = Task.Run(async () =>
        {
            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                if (line != null)
                    _logger.Information("[{ProcessName}] {Message}", name, line);
            }
        });

        _ = Task.Run(async () =>
        {
            while (!process.StandardError.EndOfStream)
            {
                var line = await process.StandardError.ReadLineAsync();
                if (line != null)
                    _logger.Error("[{ProcessName}] {Message}", name, line);
            }
        });
    }

    private async Task MonitorProcesses()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            foreach (var managedProcess in _processes.ToList())
            {
                if (managedProcess.Process.HasExited)
                {
                    _logger.Warning("Process {ProcessName} has exited with code {ExitCode}. Restarting...",
                        managedProcess.Name, managedProcess.Process.ExitCode);

                    _processes.Remove(managedProcess);
                    await StartManagedProcessAsync(managedProcess);
                }
            }

            await Task.Delay(5000, _cancellationTokenSource.Token);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();

        foreach (var managedProcess in _processes)
        {
            if (!managedProcess.Process.HasExited)
            {
                managedProcess.Process.Kill();
            }

            managedProcess.Process.Dispose();
        }

        _cancellationTokenSource?.Dispose();
    }
}

public class ManagedProcess(string name, ProcessStartInfo startInfo, int debugPort)
{
    public string Name { get; } = name;
    public ProcessStartInfo StartInfo { get; } = startInfo;
    public int DebugPort { get; } = debugPort;
    public Process? Process { get; set; }
}
