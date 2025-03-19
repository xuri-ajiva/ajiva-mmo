using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Channels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WgslGenerator;

namespace Client.Wgsl;

public interface IShaderManager
{
    T GetCurrentShader<T>() where T : IWgslShader;
}

[RequiresUnreferencedCode("Calls System.Runtime.Loader.AssemblyLoadContext.LoadFromStream(Stream)")]
public class ShaderReloadManager : IHostedService, IShaderManager
{
    private readonly ILogger<ShaderReloadManager> _logger;
    private Dictionary<string, object> _shaders = [];
    private readonly FileSystemWatcher _watcher;
    private readonly CSharpGeneratorDriver _driver;
    private readonly IConfigurationSection _config;
    private readonly Channel<string> _shaderChannel;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _assemblyStream;
    private readonly AssemblyLoadContext _assemblyLoadContext;

    private Task _shaderTask = Task.CompletedTask;
    private List<SyntaxTree> _syntaxTrees = [];
    private List<PortableExecutableReference> _references = [];
    private u32 _counter = 0;

    public T GetCurrentShader<T>() where T : IWgslShader
    {
        var name = typeof(T).Name;
        if (typeof(T).IsInterface)
        {
            name = name[1..];
        }

        if (_shaders.TryGetValue(name, out var shader))
        {
            return (T)shader;
        }

        // find deriving type
        var type = typeof(T).Assembly.DefinedTypes
            .FirstOrDefault(t => typeof(T).IsAssignableFrom(t));
        if (type is null)
        {
            _logger.LogError("Failed to find shader type: {Name}", name);
            return default!;
        }

        var ctor = type.DeclaredConstructors?.FirstOrDefault();
        if (ctor is not null)
        {
            _shaders[name] = ctor.Invoke(null);
            return (T)_shaders[name];
        }

        _logger.LogError("Failed to find constructor in shader type: {Name}", name);
        return default!;
    }

    public ShaderReloadManager(IConfiguration config, ILogger<ShaderReloadManager> logger)
    {
        _config = config.GetSection(nameof(ShaderReloadManager));
        _logger = logger;
        _watcher = new FileSystemWatcher(_config["Path"] ?? "Shaders")
        {
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = "*.wgsl.cs",
        };
        _driver = CSharpGeneratorDriver.Create(new WgslShaderGenerator());
        _shaderChannel = Channel.CreateUnbounded<string>();
        _cancellationTokenSource = new CancellationTokenSource();
        _assemblyStream = new MemoryStream();
        _assemblyLoadContext = new AssemblyLoadContext(nameof(ShaderReloadManager), true);
    }


    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Shader file changed: {FullPath}", e.FullPath);
        _shaderChannel.Writer.TryWrite(e.FullPath);
    }


    private void OnError(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), "Error in file watcher");
    }

    [RequiresAssemblyFiles]
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _watcher.Changed += OnFileChanged;
        _watcher.Error += OnError;
        _watcher.EnableRaisingEvents = true;
        _shaderTask = ShaderThread();

        var configurationSection = _config.GetRequiredSection("RequiredFiles");
        var files = configurationSection
            .GetChildren().Select(x => x.Value).ToArray();
        foreach (var t in files)
        {
            _syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(t!),
                cancellationToken: cancellationToken));
        }

        _logger.LogInformation("Loaded {Count} syntax trees", _syntaxTrees.Count);

        _references.AddRange(AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location)));

        _references.AddRange(MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(GlmSharp.glm).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Numerics.Complex).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Silk.NET.WebGPU.Adapter).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Silk.NET.WebGPU.Safe.AdapterInfo).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Silk.NET.Core.RawImage).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DynamicAttribute).Assembly.Location));

        _logger.LogInformation("Loaded {Count} assemblies", _references.Count);

        Task.Run(() => _shaderChannel.Writer.WriteAsync("./Wgsl/DeferredShader.wgsl.cs", cancellationToken),
            cancellationToken);

        return _shaderTask.IsCompleted ? _shaderTask : Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher.EnableRaisingEvents = false;
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private async Task ShaderThread()
    {
        var cancellationToken = _cancellationTokenSource.Token;
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            if (!await _shaderChannel.Reader.WaitToReadAsync(cancellationToken)) continue;

            while (_shaderChannel.Reader.TryRead(out var fullPath))
            {
                try
                {
                    CompileShader(fullPath);
                    _counter++;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to compile shader");
                }
            }
        }
    }

    [RequiresAssemblyFiles("Calls System.Reflection.Assembly.Location")]
    private void CompileShader(string fullPath)
    {
        var start = Stopwatch.GetTimestamp();
        var shaderTree = CSharpSyntaxTree.ParseText(File.ReadAllText(fullPath));
        var compilation = CSharpCompilation.Create(nameof(ShaderReloadManager) + _counter,
            [shaderTree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);

        var run = _driver.RunGenerators(compilation);
        var result = run.GetRunResult();
        _logger.LogInformation("Generated {Count} trees in {ElapsedTime}", result.GeneratedTrees.Length,
            run.GetTimingInfo().ElapsedTime);
        foreach (var diagnostic in result.Diagnostics)
        {
            _logger.LogInformation("{Diagnostic}", diagnostic);
        }

        // we only need the last tree as this is the one that was generated
        var compilationWithReferences = CSharpCompilation.Create(nameof(ShaderReloadManager) + _counter,
                _syntaxTrees, _references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true))
            //.AddReferences(compilation.ToMetadataReference())
            .AddSyntaxTrees(shaderTree, result.GeneratedTrees[^1]);

        _assemblyStream.SetLength(0);
        var emitResult = compilationWithReferences.Emit(_assemblyStream);

        if (!emitResult.Success)
        {
            _logger.LogError("Failed to compile shader: {FullPath}", fullPath);
            foreach (var diagnostic in emitResult.Diagnostics)
            {
                switch (diagnostic.Severity)
                {
                    case DiagnosticSeverity.Info:
                        _logger.LogInformation("{Diagnostic}", diagnostic);
                        break;
                    case DiagnosticSeverity.Warning:
                        _logger.LogWarning("{Diagnostic}", diagnostic);
                        break;
                    case DiagnosticSeverity.Error:
                        _logger.LogError("{Diagnostic}", diagnostic);
                        break;
                    case DiagnosticSeverity.Hidden:
                    default:
                        break;
                }
            }

            return;
        }

        _assemblyStream.Position = 0;
        var asm = _assemblyLoadContext.LoadFromStream(_assemblyStream);

        var shaderType = asm.DefinedTypes
            .FirstOrDefault(t => typeof(IWgslShader).IsAssignableFrom(t));
        if (shaderType is null)
        {
            _logger.LogError("Failed to find shader type in assembly: {FullPath}", fullPath);
            return;
        }

        var ctor = shaderType.DeclaredConstructors?.FirstOrDefault();
        if (ctor is not null)
        {
            _shaders[shaderType.Name] = ctor.Invoke(null);
            _logger.LogInformation("Loaded shader: {FullPath} in {ElapsedTime}", fullPath,
                Stopwatch.GetElapsedTime(start));
        }
        else
        {
            _logger.LogError("Failed to find constructor in shader type: {FullPath}", fullPath);
        }
    }
}
