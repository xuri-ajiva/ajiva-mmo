using Client.Common;
using Client.Config;
using Client.Renderer;
using Common.Systems;
using Hexa.NET.SDL3;

namespace Client.Desktop.Platform;

public class AjivaWindow(
    ILogger<AjivaWindow> logger,
    RendererApi api,
    WindowConfig config,
    PeriodicUpdater updater,
    LayerStack layerSystem,
    IHostApplicationLifetime lifetime,
    EventSystem eventSystem)
    : IHostedLifecycleService
{
    internal SDLWindowPtr Window = null!;
    private Thread _windowThread = null!;

    private bool _isClosing;


    bool firstLoad = true;

    private void Load()
    {
        if (!firstLoad) return;
        firstLoad = false;
        try
        {
            var properties = api.Adapter.GetProperties();
            SDL.SetWindowTitle(Window, $"Ajiva MMO {properties.BackendType} {properties.Name}");

            // todo event system
            layerSystem.Attach();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in Window Load");
            throw;
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        /*var options = WindowOptions.Default;
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
                : WindowBorder.Fixed;*/


        _windowThread = new Thread(MainWindow)
            { IsBackground = false, Name = "Ajiva Main Window Thread", Priority = ThreadPriority.AboveNormal };
        _windowThread.Start();

        return Task.CompletedTask;
    }

    private void MainWindow()
    {
        Window = SDL.CreateWindow("Default", config.Width, config.Height,
            (u64)(SDLWindowFlags.Resizable | SDLWindowFlags.Transparent));
        var mainWindowId = SDL.GetWindowID(Window);
        try
        {
            SDLEvent sdlEvent = default;

            while (!_isClosing)
            {
                SDL.PumpEvents();
                // updater.SleepSignal();
                while (SDL.WaitEvent(ref sdlEvent) && !_isClosing)
                {
                    ProcessEvent(sdlEvent, mainWindowId);
                }
            }

            lifetime.StopApplication();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in Window Main");
            throw;
        }
    }

    private void ProcessEvent(SDLEvent sdlEvent, u32 mainWindowId)
    {
        eventSystem.Dispatch(sdlEvent);
        switch ((SDLEventType)sdlEvent.Type)
        {
            case SDLEventType.Quit:
            case SDLEventType.Terminating:
                _isClosing = true;
                break;

            case SDLEventType.WindowCloseRequested:
                var windowEvent = sdlEvent.Window;
                if (windowEvent.WindowID == mainWindowId)
                {
                    _isClosing = true;
                }

                break;
            case SDLEventType.WindowShown:
                Load();
                break;
            case SDLEventType.MouseMotion:
                eventSystem.Dispatch(new AjivaMouseEvent
                {
                    Type = AjivaMouseEventType.Move,
                    X = sdlEvent.Motion.X,
                    Y = sdlEvent.Motion.Y,
                    DeltaX = sdlEvent.Motion.Xrel,
                    DeltaY = sdlEvent.Motion.Yrel
                });
                break;
            case SDLEventType.MouseButtonUp or SDLEventType.MouseButtonDown:
                eventSystem.Dispatch(new AjivaMouseEvent
                {
                    Type = sdlEvent.Button.Down == 0 ? AjivaMouseEventType.ButtonUp : AjivaMouseEventType.ButtonDown,
                    Button = sdlEvent.Button.Button switch
                    {
                        SDL.SDL_BUTTON_LEFT => AjivaMouseButton.Left,
                        SDL.SDL_BUTTON_RIGHT => AjivaMouseButton.Right,
                        SDL.SDL_BUTTON_MIDDLE => AjivaMouseButton.Middle,
                        SDL.SDL_BUTTON_X1 => AjivaMouseButton.Button4,
                        SDL.SDL_BUTTON_X2 => AjivaMouseButton.Button5,
                        var a and > SDL.SDL_BUTTON_X2 => (AjivaMouseButton)
                            (a - SDL.SDL_BUTTON_X2 + (i32)AjivaMouseButton.Button5 + 1),
                        _ => AjivaMouseButton.Left
                    },
                    X = sdlEvent.Button.X,
                    Y = sdlEvent.Button.Y
                });
                break;
            case SDLEventType.MouseWheel:
                eventSystem.Dispatch(new AjivaMouseEvent
                {
                    Type = AjivaMouseEventType.Scroll,
                    DeltaX = sdlEvent.Wheel.X,
                    DeltaY = sdlEvent.Wheel.Y,
                });
                break;
            case SDLEventType.KeyDown:
                if (sdlEvent.Key.Repeat == 0)
                    eventSystem.Dispatch(new AjivaKeyboardEvent(sdlEvent.Key.Scancode, (SDLKeymod)sdlEvent.Key.Mod,
                        AjivaKeyboardEventType.KeyDown));
                break;
            case SDLEventType.KeyUp:
                if (sdlEvent.Key.Repeat == 0)
                    eventSystem.Dispatch(new AjivaKeyboardEvent(sdlEvent.Key.Scancode, (SDLKeymod)sdlEvent.Key.Mod,
                        AjivaKeyboardEventType.KeyUp));
                break;

            default:
                logger.LogInformation("Unhandled SDL event {SdlEventType}", (SDLEventType)sdlEvent.Type);
                break;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        layerSystem.Detach();
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
        var closeEvent = new SDLEvent
        {
            Type = (u32)SDLEventType.WindowCloseRequested,
            Window = new SDLWindowEvent
            {
                WindowID = SDL.GetWindowID(Window)
            }
        };
        SDL.PushEvent(ref closeEvent);
        _windowThread.Join();
        return Task.CompletedTask;
    }
}