using System.Runtime.InteropServices;
using Client.Config;
using Hexa.NET.SDL3;
using Silk.NET.WebGPU.Safe;

namespace Client.Desktop.Platform;

public class PlatformSystem(ILogger<PlatformSystem> logger)
{
    public void Init()
    {
        SDL.SetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");
        SDL.SetHint(SDL.SDL_HINT_FRAMEBUFFER_ACCELERATION, "1");
        SDL.Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_EVENTS);

        logger.LogInformation("PlatformSystem Init");
    }

    public void Shutdown()
    {
        logger.LogInformation("PlatformSystem Shutdown");
    }

    public static unsafe SurfacePtr CreateWebGpuSurface(SDLWindowPtr window, InstancePtr instance)
    {
        var props = SDL.GetWindowProperties(window);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var hwnd = (IntPtr)SDL.GetPointerProperty(props, SDL.SDL_PROP_WINDOW_WIN32_HWND_POINTER, null);
            var hInstance = (IntPtr)SDL.GetPointerProperty(props, SDL.SDL_PROP_WINDOW_WIN32_INSTANCE_POINTER, null);
            return instance.CreateSurfaceFromWindowsHWND(hwnd, hInstance, "canvas");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
        {
            return instance.CreateSurfaceFromHTMLCanvas(selector: "canvas", label: "canvas surface");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var driverVersion = SDL.GetCurrentVideoDriverS();
            return driverVersion switch
            {
                "x11" => instance.CreateSurfaceFromXlibWindow(
                    (IntPtr)SDL.GetPointerProperty(props, SDL.SDL_PROP_WINDOW_X11_DISPLAY_POINTER, null),
                    (u32)SDL.GetPointerProperty(props, SDL.SDL_PROP_WINDOW_X11_WINDOW_NUMBER, null), "x11 surface"),
                "wayland" => instance.CreateSurfaceFromWaylandSurface(
                    (IntPtr)SDL.GetPointerProperty(props, SDL.SDL_PROP_WINDOW_WAYLAND_DISPLAY_POINTER, null),
                    (IntPtr)SDL.GetPointerProperty(props, SDL.SDL_PROP_WINDOW_WAYLAND_SURFACE_POINTER, null),
                    "wayland surface"),
                _ => throw new PlatformNotSupportedException(
                    $"Your platform is not supported! {SDL.GetPlatformS()}, with video driver {driverVersion}")
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return instance.CreateSurfaceFromMetalLayer(
                (IntPtr)SDL.GetPointerProperty(props, SDL.SDL_PROP_WINDOW_UIKIT_WINDOW_POINTER, null), "metal surface");
        }


        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID")))
        {
            return instance.CreateSurfaceFromAndroidNativeWindow(
                (IntPtr)SDL.GetPointerProperty(props, SDL.SDL_PROP_WINDOW_ANDROID_WINDOW_POINTER, null),
                "android surface");
        }

        throw new PlatformNotSupportedException($"Your platform is not supported! {SDL.GetPlatformS()}");
    }
}