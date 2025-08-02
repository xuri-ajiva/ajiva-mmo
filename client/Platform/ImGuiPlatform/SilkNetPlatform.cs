using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using Silk.NET.Core.Contexts;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using Silk.NET.Maths;
using Silk.NET.SDL;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using Color = Silk.NET.WebGPU.Color;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;
using RenderPassTimestampWrites = Silk.NET.WebGPU.Safe.RenderPassTimestampWrites;
using SurfaceConfiguration = Silk.NET.WebGPU.Safe.SurfaceConfiguration;
using Window = Silk.NET.Windowing.Window;

namespace Client.Platform.ImGuiPlatform
{
    public class SilkNetPlatform : IImGuiPlatform
    {
        private readonly IWindow _view;
        private readonly IInputContext _input;
        private readonly List<char> _pressedChars = new List<char>();
        private IKeyboard? _keyboard;
        private List<(ImGuiKey key, bool isDown)> _keyEvents = new();

        private ImGuiController _controller = null!;

        private readonly ConcurrentDictionary<IntPtr, IWindow> _windows = new();
        private readonly ConcurrentDictionary<IntPtr, (SurfacePtr, TextureFormat)> _surfaces = new();
        private readonly ImGuiViewport[] _viewports = new ImGuiViewport[99];

        public SilkNetPlatform(IWindow view, IInputContext input)
        {
            _view = view;
            _input = input;
        }

        public unsafe void Init(ImGuiContextPtr context)
        {
            ImGui.SetCurrentContext(context);
            var io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
          //  io.BackendFlags |= ImGuiBackendFlags.PlatformHasViewports;
           // io.BackendFlags |= ImGuiBackendFlags.RendererHasViewports;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasTextures;


            var monitors = Silk.NET.Windowing.Monitor.GetMonitors(null).ToList();
            var platformIo = ImGui.GetPlatformIO();

            var mainMonitor = monitors[0];
            platformIo.Monitors.Resize(monitors.Count);
            for (int i = 0; i < monitors.Count; i++)
            {
                var monitor = monitors[i];
                var area = monitor.Bounds;
                var workArea = monitor.Bounds;
                platformIo.Monitors[i] = new ImGuiPlatformMonitor
                {
                    MainPos = new Vector2(area.Origin.X, area.Origin.Y),
                    MainSize = new Vector2(area.Size.X, area.Size.Y),
                    WorkPos = new Vector2(workArea.Origin.X, workArea.Origin.Y),
                    WorkSize = new Vector2(workArea.Size.X, workArea.Size.Y),
                    DpiScale = 1
                };
            }

            platformIo.Viewports.Resize(1);
            fixed (void* th = &this._controller)
            {
                var viewPort = new ImGuiViewport()
                {
                    DpiScale = 1,
                    PlatformHandle = th,
                    PlatformUserData = th,
                    RendererUserData = th,
                    PlatformHandleRaw = th,
                    Flags = ImGuiViewportFlags.OwnedByApp,
                    ID = 0,
                    Size = new Vector2(100, 100),
                    WorkSize = new Vector2(100, 100),
                };
                _viewports[0] = viewPort;
                fixed (ImGuiViewport* viewPortPtr = &_viewports[0])
                {
                    ref var a = ref platformIo.Viewports;
                    a.Data[0].Handle = viewPortPtr;
                }
            }


            platformIo.PlatformCreateWindow =
                Marshal.GetFunctionPointerForDelegate<CreateWindowDelegate>(CreateWindow).ToPointer();
            platformIo.PlatformDestroyWindow =
                Marshal.GetFunctionPointerForDelegate<DestroyWindowDelegate>(DestroyWindow).ToPointer();
            platformIo.PlatformShowWindow =
                Marshal.GetFunctionPointerForDelegate<ShowWindowDelegate>(ShowWindow).ToPointer();
            platformIo.PlatformSetWindowPos =
                Marshal.GetFunctionPointerForDelegate<SetWindowPosDelegate>(SetWindowPos).ToPointer();
            platformIo.PlatformGetWindowPos =
                Marshal.GetFunctionPointerForDelegate<GetWindowPosDelegate>(GetWindowPos).ToPointer();
            platformIo.PlatformSetWindowSize =
                Marshal.GetFunctionPointerForDelegate<SetWindowSizeDelegate>(SetWindowSize).ToPointer();
            platformIo.PlatformGetWindowSize =
                Marshal.GetFunctionPointerForDelegate<GetWindowSizeDelegate>(GetWindowSize).ToPointer();
            platformIo.PlatformSetWindowFocus =
                Marshal.GetFunctionPointerForDelegate<SetWindowFocusDelegate>(SetWindowFocus).ToPointer();
            platformIo.PlatformGetWindowFocus =
                Marshal.GetFunctionPointerForDelegate<GetWindowFocusDelegate>(GetWindowFocus).ToPointer();
            platformIo.PlatformGetWindowMinimized =
                Marshal.GetFunctionPointerForDelegate<GetWindowMinimizedDelegate>(GetWindowMinimized).ToPointer();
            platformIo.PlatformSetWindowTitle =
                Marshal.GetFunctionPointerForDelegate<SetWindowTitleDelegate>(SetWindowTitle).ToPointer();

            platformIo.RendererRenderWindow =
                Marshal.GetFunctionPointerForDelegate<RenderWindowDelegate>(RenderWindow).ToPointer();
        }

        private unsafe void RenderWindow(ImGuiViewportPtr viewport, IntPtr userdata)
        {
            if (!_windows.TryGetValue(new IntPtr(viewport.PlatformHandle), out var window) ||
                !_surfaces.TryGetValue(new IntPtr(viewport.PlatformHandle), out var surfaceInfo))
                return;

            var (surface, format) = surfaceInfo;

            var (swapChainTexture, _, status) = surface.GetCurrentTexture();
            if (status != SurfaceGetCurrentTextureStatus.Success)
                return;

            var swapchainView = swapChainTexture.CreateView(format, TextureViewDimension.Dimension2D, TextureAspect.All,
                baseMipLevel: 0, mipLevelCount: 1,
                baseArrayLayer: 0, arrayLayerCount: 1, label: "SwapchainView");

            var commandEncoder = _controller._api.Device.CreateCommandEncoder();
            var renderPass = commandEncoder.BeginRenderPass(colorAttachments:
            [
                new RenderPassColorAttachment(swapchainView, null, LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0, 0))
            ], timestampWrites: null, depthStencilAttachment: null, occlusionQuerySet: null);

            _controller.RenderImDrawData(viewport.DrawData, renderPass);

            renderPass.End();
            _controller._api.Device.GetQueue().Submit(commandEncoder.Finish());

            surface.Present();
        }

        private unsafe void SetWindowTitle(ImGuiViewportPtr viewport, IntPtr title)
        {
            if (_windows.TryGetValue(new IntPtr(viewport.PlatformHandle), out var window))
                window.Title = Marshal.PtrToStringUTF8(title) ?? "";
        }

        private unsafe bool GetWindowMinimized(ImGuiViewportPtr viewport)
        {
            return _windows.TryGetValue(new IntPtr(viewport.PlatformHandle), out var window) &&
                   window.WindowState == WindowState.Minimized;
        }

        private unsafe bool GetWindowFocus(ImGuiViewportPtr viewport)
        {
            return _windows.TryGetValue(new IntPtr(viewport.PlatformHandle), out var window); // && window.IsFocused;
        }

        private unsafe void SetWindowFocus(ImGuiViewportPtr viewport)
        {
            if (_windows.TryGetValue(new IntPtr(viewport.PlatformHandle), out var window))
                window.Focus();
        }

        private unsafe Vector2 GetWindowSize(ImGuiViewportPtr viewport)
        {
            return _windows.TryGetValue(new IntPtr(viewport.PlatformHandle), out var window)
                ? new Vector2(window.Size.X, window.Size.Y)
                : Vector2.Zero;
        }

        private unsafe void SetWindowSize(ImGuiViewportPtr viewport, Vector2 size)
        {
            if (_windows.TryGetValue(new IntPtr(viewport.PlatformHandle), out var window))
                window.Size = new Vector2D<int>((int)size.X, (int)size.Y);
        }

        private unsafe Vector2 GetWindowPos(ImGuiViewportPtr viewport)
        {
            return _windows.TryGetValue(new IntPtr(viewport.PlatformHandle), out var window)
                ? new Vector2(window.Position.X, window.Position.Y)
                : Vector2.Zero;
        }

        private unsafe void SetWindowPos(ImGuiViewportPtr viewport, Vector2 pos)
        {
            if (_windows.TryGetValue(new IntPtr(viewport.PlatformHandle), out var window))
                window.Position = new Vector2D<int>((int)pos.X, (int)pos.Y);
        }

        private unsafe void ShowWindow(ImGuiViewportPtr viewport)
        {
            if (_windows.TryGetValue(new IntPtr(viewport.PlatformHandle), out var window))
                window.IsVisible = true;
        }

        private unsafe void DestroyWindow(ImGuiViewportPtr viewport)
        {
            if (_windows.TryRemove(new IntPtr(viewport.PlatformHandle), out var window))
            {
                window.Close();
                window.Dispose();
            }

            if (_surfaces.TryRemove(new IntPtr(viewport.PlatformHandle), out var surfaceInfo))
            {
                surfaceInfo.Item1.Release();
            }
        }

        private unsafe void CreateWindow(ImGuiViewportPtr viewport)
        {
            var options = new WindowOptions
            {
                API = GraphicsAPI.None,
                Size = new Vector2D<int>((int)viewport.Size.X, (int)viewport.Size.Y),
                Position = new Vector2D<int>((int)viewport.Pos.X, (int)viewport.Pos.Y),
                Title = "ImGui Viewport",
                IsVisible = false,
                WindowState = WindowState.Normal,
                WindowBorder = WindowBorder.Resizable,
                TransparentFramebuffer = true
            };

            var newWindow = Window.Create(options);
            newWindow.Initialize();

            var surface = newWindow.CreateWebGPUSurface(_controller._api.Device.GetAPI(), _controller._api.Instance);
            var format = surface.GetPreferredFormat(_controller._api.Adapter);
            surface.Configure(new SurfaceConfiguration
            {
                Device = _controller._api.Device,
                Format = format,
                Usage = TextureUsage.RenderAttachment,
                Width = (uint)newWindow.Size.X,
                Height = (uint)newWindow.Size.Y,
                PresentMode = PresentMode.Fifo
            });

            viewport.PlatformHandle = newWindow.Handle.ToPointer();
            _windows[newWindow.Handle] = newWindow;
            _surfaces[newWindow.Handle] = (surface, format);
        }

        public void Update(float deltaSeconds)
        {
            var io = ImGui.GetIO();
            io.DisplaySize = new Vector2(_view.Size.X, _view.Size.Y);

            if (_view.Size.X * _view.Size.Y > 0)
            {
                io.DisplayFramebufferScale = new Vector2(_view.FramebufferSize.X / (float)_view.Size.X,
                    _view.FramebufferSize.Y / (float)_view.Size.Y);
            }

            io.DeltaTime = deltaSeconds;

            UpdateImGuiInput();
        }

        public void BeginFrame()
        {
            ImGui.NewFrame();

            if (_keyboard == _input.Keyboards[0])
                return;

            if (_keyboard is not null)
            {
                _keyboard.KeyChar -= OnKeyChar;
                _keyboard.KeyDown -= OnKeyDown;
                _keyboard.KeyUp -= OnKeyUp;
            }

            _keyboard = _input.Keyboards[0];
            _keyboard.KeyChar += OnKeyChar;
            _keyboard.KeyDown += OnKeyDown;
            _keyboard.KeyUp += OnKeyUp;
        }

        public void EndFrame()
        {
        }

        private void UpdateImGuiInput()
        {
            var io = ImGui.GetIO();

            var mouseState = _input.Mice[0].CaptureState();
            var keyboardState = _input.Keyboards[0];

            io.MouseDown[0] = mouseState.IsButtonPressed(MouseButton.Left);
            io.MouseDown[1] = mouseState.IsButtonPressed(MouseButton.Right);
            io.MouseDown[2] = mouseState.IsButtonPressed(MouseButton.Middle);

            var point = new Vector2D<int>((int)mouseState.Position.X, (int)mouseState.Position.Y);
            io.MousePos = new Vector2(point.X, point.Y);

            var wheel = mouseState.GetScrollWheels()[0];
            if (wheel.X != 0 || wheel.Y != 0)
            {
                io.MouseWheelH = wheel.X;
                io.MouseWheel = wheel.Y;
            }

            foreach (var entry in _keyEvents)
            {
                //io.AddKeyEvent(entry.key, entry.isDown);
                //io.AddInputCharacter(entry.key, entry.isDown);
            }

            _keyEvents.Clear();

            foreach (var c in _pressedChars)
            {
                io.AddInputCharacter(c);
            }

            _pressedChars.Clear();

            io.KeyCtrl = keyboardState.IsKeyPressed(Key.ControlLeft) || keyboardState.IsKeyPressed(Key.ControlRight);
            io.KeyAlt = keyboardState.IsKeyPressed(Key.AltLeft) || keyboardState.IsKeyPressed(Key.AltRight);
            io.KeyShift = keyboardState.IsKeyPressed(Key.ShiftLeft) || keyboardState.IsKeyPressed(Key.ShiftRight);
            io.KeySuper = keyboardState.IsKeyPressed(Key.SuperLeft) || keyboardState.IsKeyPressed(Key.SuperRight);
        }

        private void OnKeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (TryDecodeKey(arg2, arg3, out var decodedKey))
                _keyEvents.Add((decodedKey, true));
        }

        private void OnKeyUp(IKeyboard arg1, Key arg2, int arg3)
        {
            if (TryDecodeKey(arg2, arg3, out var decodedKey))
                _keyEvents.Add((decodedKey, false));
        }

        private void OnKeyChar(IKeyboard arg1, char arg2)
        {
            _pressedChars.Add(arg2);
        }

        private bool TryDecodeKey(Key key, int scancode, out ImGuiKey decodedKey)
        {
            decodedKey = key switch
            {
                Key.Tab => ImGuiKey.Tab,
                Key.Left => ImGuiKey.LeftArrow,
                Key.Right => ImGuiKey.RightArrow,
                Key.Up => ImGuiKey.UpArrow,
                Key.Down => ImGuiKey.DownArrow,
                Key.PageUp => ImGuiKey.PageUp,
                Key.PageDown => ImGuiKey.PageDown,
                Key.Home => ImGuiKey.Home,
                Key.End => ImGuiKey.End,
                Key.Delete => ImGuiKey.Delete,
                Key.Backspace => ImGuiKey.Backspace,
                Key.Enter => ImGuiKey.Enter,
                Key.Escape => ImGuiKey.Escape,
                Key.A => ImGuiKey.A,
                Key.C => ImGuiKey.C,
                Key.V => ImGuiKey.V,
                Key.X => ImGuiKey.X,
                Key.Y => ImGuiKey.Y,
                Key.Z => ImGuiKey.Z,
                _ => ImGuiKey.None
            };
            return decodedKey != ImGuiKey.None;
        }

        public void Dispose()
        {
            if (_keyboard is not null)
            {
                _keyboard.KeyChar -= OnKeyChar;
                _keyboard.KeyDown -= OnKeyDown;
                _keyboard.KeyUp -= OnKeyUp;
            }

            foreach (var window in _windows.Values)
            {
                window.Close();
                window.Dispose();
            }

            foreach (var surfaceInfo in _surfaces.Values)
            {
                surfaceInfo.Item1.Release();
            }
        }

        public void SetImGuiController(ImGuiController controller)
        {
            _controller = controller;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void CreateWindowDelegate(ImGuiViewportPtr viewport);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void DestroyWindowDelegate(ImGuiViewportPtr viewport);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void ShowWindowDelegate(ImGuiViewportPtr viewport);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void SetWindowPosDelegate(ImGuiViewportPtr viewport, Vector2 pos);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate Vector2 GetWindowPosDelegate(ImGuiViewportPtr viewport);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void SetWindowSizeDelegate(ImGuiViewportPtr viewport, Vector2 size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate Vector2 GetWindowSizeDelegate(ImGuiViewportPtr viewport);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void SetWindowFocusDelegate(ImGuiViewportPtr viewport);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate bool GetWindowFocusDelegate(ImGuiViewportPtr viewport);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate bool GetWindowMinimizedDelegate(ImGuiViewportPtr viewport);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void SetWindowTitleDelegate(ImGuiViewportPtr viewport, IntPtr title);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void RenderWindowDelegate(ImGuiViewportPtr viewport, IntPtr userdata);
    }
}
