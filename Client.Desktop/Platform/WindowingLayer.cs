using Client.Common;
using Client.Config;
using Client.Renderer;
using Common.Systems;
using Hexa.NET.SDL3;

namespace Client.Desktop.Platform;

public class WindowingLayer(
    WindowConfig config,
    RendererApi api,
    LayerStack layerStack,
    IHostApplicationLifetime lifetime,
    ILogger<WindowingLayer> logger) : IRenderLayer
{
    public void Attach()
    {
        var flags = SDLWindowFlags.Transparent | SDLWindowFlags.Hidden | SDLWindowFlags.AllowHighdpi;

        if (config.FullscreenBorderless) flags |= SDLWindowFlags.Fullscreen;
        else if (config.Resizable) flags |= SDLWindowFlags.Resizable;

        if (config.VSync) flags |= SDLWindowFlags.SurfaceVsyncAdaptive;

        MainWindow = SDL.CreateWindow(config.Title ?? "Default", 10, 10, (u64)flags);
        _mainWindowId = SDL.GetWindowID(MainWindow);
        if (config is { X: { } x, Y: { } y })
            SDL.SetWindowPosition(MainWindow, x, y);
        if (config is { Width: { } w, Height: { } h })
            SDL.SetWindowSize(MainWindow, w, h);
        SDL.ShowWindow(MainWindow);
        var properties = api.Adapter.GetProperties();
        SDL.SetWindowTitle(MainWindow, $"Ajiva MMO {properties.BackendType} {properties.Name}");
    }

    public SDLWindowPtr MainWindow { get; set; }
    private uint _mainWindowId;

    private void ProcessEvent(SDLEvent sdlEvent)
    {
        //logger.LogInformation("SDL event {SdlEventType}", (SDLEventType)sdlEvent.Type);
        i32 x = 0, y = 0;
        var captured = false;
        switch ((SDLEventType)sdlEvent.Type)
        {
            case SDLEventType.WindowCloseRequested:
                var windowEvent = sdlEvent.Window;
                if (windowEvent.WindowID == _mainWindowId)
                {
                    RequestQuit();
                }

                break;
            case SDLEventType.Quit:
            case SDLEventType.Terminating:
                RequestQuit();
                break;
            case SDLEventType.WindowShown:
                //TODO Load();
                break;
            case SDLEventType.MouseMotion:
                SDL.GetWindowPosition(SDL.GetWindowFromID(sdlEvent.Motion.WindowID), ref x, ref y);
                captured = layerStack.DispatchEvent(new AjivaMouseEvent
                {
                    Type = AjivaMouseEventType.Move,
                    X = sdlEvent.Motion.X + x,
                    Y = sdlEvent.Motion.Y + y,
                    DeltaX = sdlEvent.Motion.Xrel,
                    DeltaY = sdlEvent.Motion.Yrel
                });
                break;
            case SDLEventType.MouseButtonUp or SDLEventType.MouseButtonDown:
                SDL.GetWindowPosition(SDL.GetWindowFromID(sdlEvent.Motion.WindowID), ref x, ref y);
                var ajivaMouseEvent = new AjivaMouseEvent
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
                    X = sdlEvent.Button.X + x,
                    Y = sdlEvent.Button.Y + y
                };
                captured = layerStack.DispatchEvent(ajivaMouseEvent);
                break;
            case SDLEventType.MouseWheel:
                captured = layerStack.DispatchEvent(new AjivaMouseEvent
                {
                    Type = AjivaMouseEventType.Scroll,
                    DeltaX = sdlEvent.Wheel.X,
                    DeltaY = sdlEvent.Wheel.Y,
                });
                break;
            case SDLEventType.KeyDown:
                if (sdlEvent.Key.Repeat == 0)
                    captured = layerStack.DispatchEvent(new AjivaKeyboardEvent(sdlEvent.Key.Scancode,
                        (SDLKeymod)sdlEvent.Key.Mod,
                        AjivaKeyboardEventType.KeyDown));
                break;
            case SDLEventType.KeyUp:
                if (sdlEvent.Key.Repeat == 0)
                    captured = layerStack.DispatchEvent(new AjivaKeyboardEvent(sdlEvent.Key.Scancode,
                        (SDLKeymod)sdlEvent.Key.Mod,
                        AjivaKeyboardEventType.KeyUp));
                break;

            case SDLEventType.WindowMouseEnter or SDLEventType.WindowMouseLeave or SDLEventType.WindowFocusGained
                or SDLEventType.WindowFocusLost or SDLEventType.WindowMoved or SDLEventType.WindowPixelSizeChanged or SDLEventType.WindowSafeAreaChanged or SDLEventType.WindowResized or SDLEventType.WindowExposed:
                break;

            default:
                logger.LogWarning("Unhandled SDL event {SdlEventType}", (SDLEventType)sdlEvent.Type);
                break;
        }

        if (!captured)
            layerStack.DispatchEvent(sdlEvent);
    }

    private void RequestQuit() => lifetime.StopApplication();

    public void Render(UpdateInfo info, RenderTarget target)
    {
        SDLEvent sdlEvent = default;

        SDL.PumpEvents();
        while (SDL.PollEvent(ref sdlEvent))
        {
            ProcessEvent(sdlEvent);
        }
    }

    public void Update(UpdateInfo info)
    {
    }

    public void Detach() => SDL.DestroyWindow(MainWindow);

    private bool _enabled = true;
    public ref bool Enabled => ref _enabled;
}