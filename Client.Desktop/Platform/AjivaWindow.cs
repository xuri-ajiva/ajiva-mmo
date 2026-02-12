using Client.Common.Accessors;
using Client.Config;
using Client.LayerSystem;
using Client.Renderer;
using Common.Systems;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.WebGPU.Safe;
using Silk.NET.Windowing;

namespace Client.Desktop.Platform;

public class AjivaWindow : IHostedLifecycleService, IInputAccessor
{
    private readonly ILogger<AjivaWindow> _logger;
    private readonly RendererApi _api;
    private readonly WindowConfig _config;
    private readonly PeriodicUpdater _updater;
    private readonly LayerStack _layerSystem;
    private readonly IHostApplicationLifetime _lifetime;

    internal IWindow Window = null!;
    private Thread _windowThread = null!;

    public SurfacePtr Surface;
    public IInputContext InputContext { get; private set; }
    private bool _isClosing;

    public AjivaWindow(ILogger<AjivaWindow> logger, RendererApi api, WindowConfig config, PeriodicUpdater updater,
        LayerStack layerSystem, IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _api = api;
        _config = config;
        _updater = updater;
        _layerSystem = layerSystem;
        _lifetime = lifetime;
    }


    private void Load()
    {
        try
        {
            InputContext = Window.CreateInput();
            Surface = Window.CreateWebGPUSurface(_api.Wgpu, _api.Instance);
            var properties = _api.Adapter.GetProperties();
            Window.Title = $"Ajiva MMO {properties.BackendType} {properties.Name}";

            // todo event system
            _layerSystem.Attach();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in Window Load");
            throw;
        }
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var options = WindowOptions.Default;
        options.API = GraphicsAPI.None;
        options.Size = new Vector2D<i32>(_config.Width, _config.Height);
        options.Position = new Vector2D<i32>(_config.X, _config.Y);
        options.Title = _config.Title;
        options.TransparentFramebuffer = true;
        options.VSync = _config.VSync;
        options.WindowBorder = _config.FullscreenBorderless
            ? WindowBorder.Hidden
            : _config.Resizable
                ? WindowBorder.Resizable
                : WindowBorder.Fixed;

        Window = Silk.NET.Windowing.Window.Create(options);
        Window.Load += Load;
        _windowThread = new Thread(MainWindow)
            { IsBackground = false, Name = "Ajiva Main Window Thread", Priority = ThreadPriority.AboveNormal };
        _windowThread.Start();

        return Task.CompletedTask;
    }

    private void MainWindow()
    {
        try
        {
            Window.Initialize();
            Window.Run((Action)(() =>
            {
                Window.DoEvents();
                _updater.SleepSignal();
                if (!Window.IsClosing || _isClosing) return;
                Window.IsClosing = false;
                _lifetime.StopApplication();
            }));
            Window.DoEvents();
            Window.Reset();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in Window Main");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _layerSystem.Detach();
        _isClosing = true;
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        /*Window.TopMost = true;
        Window.DoUpdate();
        Window.TopMost = false;*/
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        //Surface.Release();
        _isClosing = true;
        Window.Close();
        _windowThread.Join();
        return Task.CompletedTask;
    }
}