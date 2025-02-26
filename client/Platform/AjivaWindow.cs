using client.LayerSystem;
using client.Renderer;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using Silk.NET.Windowing;
using SurfaceConfiguration = Silk.NET.WebGPU.Safe.SurfaceConfiguration;


namespace client.Platform;

public class AjivaWindow : IHostedLifecycleService
{
    private readonly ILogger<AjivaWindow> _logger;
    private readonly RendererApi _api;
    private readonly AjivaImGuiLayerSystem _layerSystem;
    private readonly IHostApplicationLifetime _lifetime;

    public IWindow Window = null!;
    private Thread windowThread = null!;

    public SurfacePtr surface;
    private AjivaClock _updateClock;
    private AjivaClock _renderClock;

    public AjivaWindow(ILogger<AjivaWindow> logger, RendererApi api,
        AjivaImGuiLayerSystem layerSystem, IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _api = api;
        _layerSystem = layerSystem;
        _lifetime = lifetime;
    }


    private void W_Load()
    {
        try
        {
            _layerSystem.Attach(this);
            surface = Window.CreateWebGPUSurface(_api.Wgpu, _api.Instance);
            var properties = _api.Adapter.GetProperties();
            Window.Title = $"Ajiva MMO {properties.BackendType} {properties.Name}";
            _updateClock.Start();
            _renderClock.Start();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in Window Load");
        }
    }

    private void W_Update(f64 deltaTime)
    {
        _updateClock.Update();
        _layerSystem.Update(new UpdateInfo(_updateClock)).ConfigureAwait(false).GetAwaiter();
    }

    private Vector2D<i32> _lastFramebufferSize = new(0, 0);


    private void W_Render(f64 deltaTime)
    {
        _renderClock.Update();
        var framebufferSize = Window.FramebufferSize;

        if (framebufferSize.X * framebufferSize.Y == 0)
            return;

        if (_lastFramebufferSize != framebufferSize)
        {
            _lastFramebufferSize = framebufferSize;

            surface.Configure(new SurfaceConfiguration
            {
                AlphaMode = CompositeAlphaMode.Opaque,
                Device = _api.Device,
                Usage = TextureUsage.RenderAttachment,
                Format = TextureFormat.Bgra8Unorm,
                ViewFormats = [TextureFormat.Bgra8Unorm],
                Width = (u32)framebufferSize.X,
                Height = (u32)framebufferSize.Y,
                PresentMode = PresentMode.Fifo
            });
        }

        _layerSystem.Render(new UpdateInfo(_renderClock)).ConfigureAwait(false).GetAwaiter();

        surface.Present();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var options = WindowOptions.Default;
        options.API = GraphicsAPI.None;
        options.Size = new Vector2D<i32>(1920, 1080);
        options.TransparentFramebuffer = true;
        options.WindowBorder = WindowBorder.Resizable;

        Window = Silk.NET.Windowing.Window.Create(options);

        Window.Load += W_Load;
        Window.Render += W_Render;
        Window.Move += _ =>
        {
            Window.DoUpdate();
            Window.DoRender();
        };
        Window.Resize += _ =>
        {
            Window.DoUpdate();
            Window.DoRender();
        };
        Window.Update += W_Update;

        windowThread = new Thread(MainWindow);
        windowThread.Start();

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
                if (!Window.IsClosing)
                    Window.DoUpdate();
                if (Window.IsClosing)
                {
                    _lifetime.StopApplication();
                    return;
                }

                Window.DoRender();
            }));
            Window.DoEvents();
            Window.Reset();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in Window Main");
        }
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        Window.Close();
        windowThread.Join();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        Window.TopMost = true;
        Window.DoUpdate();
        Window.TopMost = false;
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
