using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Client.Common;
using Client.Renderer.Pipelines;
using Client.Renderer.Resources;
using Client.Wgsl;
using Common.Systems;
using Hexa.NET.ImGui;
using Hexa.NET.SDL3;
using Microsoft.AspNetCore.Mvc;
using Silk.NET.Core.Native;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using SixLabors.ImageSharp.PixelFormats;

namespace Client.Renderer.Renderers;

public partial class ImGuiPlatform(
    ResourceManager loader,
    ILogger<ImGuiPlatform> logger,
    ShaderReloadManager shaderReloadManager) : IEventHandler<AjivaMouseEvent>, IEventHandler<AjivaKeyboardEvent>
{
    private static readonly Memory<u8> PlatformName = new("Ajiva ImGui Renderer\0"u8.ToArray());
    private ImGuiContextPtr _context;

    public unsafe bool Initialize()
    {
        _context = ImGui.CreateContext();
        ImGui.SetCurrentContext(_context);

        var io = ImGui.GetIO();
        io.BackendPlatformName = (u8*)_platformNameHandle.Pointer;
        io.BackendRendererName = (u8*)_platformNameHandle.Pointer;

        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.HasSetMousePos |
                           ImGuiBackendFlags.PlatformHasViewports | ImGuiBackendFlags.HasMouseHoveredViewport |
                           ImGuiBackendFlags.RendererHasTextures | ImGuiBackendFlags.RendererHasVtxOffset;
        return true;
    }

    public void Teardown()
    {
        InvalidateDeviceObjects();
        ImGui.DestroyContext(_context);
    }


    public void NewFrame(UpdateInfo info, ImVec2I framebufferSize)
    {
        ImGui.SetCurrentContext(_context);
        var currentShader = shaderReloadManager.GetCurrentShader<IImGuiShader>();
        if ((_pipeline is null || !_pipeline.IsUpToDate(currentShader)) && !CreateDeviceObjects(currentShader))
        {
            logger.LogError("Failed to create ImGui device objects");
            throw new Exception("Failed to create ImGui device");
        }

        var io = ImGui.GetIO();
        if (framebufferSize is { X: > 0, Y: > 0 })
            io.DisplaySize = new Vector2(framebufferSize.X, framebufferSize.Y);
        io.DisplayFramebufferScale = Vector2.One;

        io.DeltaTime = info.DeltaTime;
        ImGui.NewFrame();

        UpdateTexture(io.Fonts.TexData);

        UpdateInput();
    }

    private void UpdateInput()
    {
        var io = ImGui.GetIO();
        // TODO implement input handling and update io.MousePos, io.MouseDown, io.KeysDown, etc. based on the input system
    }


    public void OnEvent(ref AjivaEvent<AjivaMouseEvent> inputEvent)
    {
        var mouseEvent = inputEvent.EventData;
        var io = ImGui.GetIO();
        var buttonIndex = (i32)mouseEvent.Button;
        switch (mouseEvent.Type)
        {
            case AjivaMouseEventType.DoubleClick:
                break;
            case AjivaMouseEventType.ButtonUp:
                if (io.MouseDown.Length > buttonIndex)
                    io.MouseDown[buttonIndex] = false;
                break;
            case AjivaMouseEventType.ButtonDown:
                if (io.MouseDown.Length > buttonIndex)
                    io.MouseDown[buttonIndex] = true;
                break;
            case AjivaMouseEventType.Move:
                io.MousePos = new Vector2(mouseEvent.X, mouseEvent.Y);
                break;
            case AjivaMouseEventType.Scroll:
                io.MouseWheelH = mouseEvent.DeltaX;
                io.MouseWheel = mouseEvent.DeltaY;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnEvent(ref AjivaEvent<AjivaKeyboardEvent> inputEvent)
    {
        var keyboardEvent = inputEvent.EventData;
        var io = ImGui.GetIO();
        var keyCore = TranslateKeyCodeToImGui(keyboardEvent.KeyCode);
        AddKeyModifiersToImGui(io, keyboardEvent.Modifiers);
        io.AddKeyEvent(keyCore, keyboardEvent.IsPressed == AjivaKeyboardEventType.KeyDown);
    }

    private static void AddKeyModifiersToImGui(ImGuiIOPtr io, SDLKeymod modifiers)
    {
        io.AddKeyEvent(ImGuiKey.ModCtrl, (modifiers & SDLKeymod.Ctrl) != 0);
        io.AddKeyEvent(ImGuiKey.ModShift, (modifiers & SDLKeymod.Shift) != 0);
        io.AddKeyEvent(ImGuiKey.ModAlt, (modifiers & SDLKeymod.Alt) != 0);
        io.AddKeyEvent(ImGuiKey.ModSuper, (modifiers & SDLKeymod.Gui) != 0);
    }

    private ImGuiKey TranslateKeyCodeToImGui(SDLScancode keyCode)
    {
        var result = keyCode switch
        {
            >= SDLScancode.Scancode1 and <= SDLScancode.Scancode9 => (ImGuiKey)((i32)ImGuiKey.Key1 +
                (keyCode - SDLScancode.Scancode1)),
            SDLScancode.Scancode0 => ImGuiKey.Key0,
            >= SDLScancode.A and <= SDLScancode.Z => (ImGuiKey)((i32)ImGuiKey.A + (keyCode - SDLScancode.A)),
            SDLScancode.Return => ImGuiKey.Enter,
            SDLScancode.Escape => ImGuiKey.Escape,
            SDLScancode.Backspace => ImGuiKey.Backspace,
            SDLScancode.Tab => ImGuiKey.Tab,
            SDLScancode.Space => ImGuiKey.Space,
            SDLScancode.Minus => ImGuiKey.Minus,
            SDLScancode.Equals => ImGuiKey.Equal,
            SDLScancode.Leftbracket => ImGuiKey.LeftBracket,
            SDLScancode.Rightbracket => ImGuiKey.RightBracket,
            SDLScancode.Backslash => ImGuiKey.Backslash,
            SDLScancode.Semicolon => ImGuiKey.Semicolon,
            SDLScancode.Apostrophe => ImGuiKey.Apostrophe,
            SDLScancode.Grave => ImGuiKey.GraveAccent,
            SDLScancode.Comma => ImGuiKey.Comma,
            SDLScancode.Period => ImGuiKey.Period,
            SDLScancode.Slash => ImGuiKey.Slash,
            SDLScancode.Capslock => ImGuiKey.CapsLock,
            >= SDLScancode.F1 and <= SDLScancode.F12 => (ImGuiKey)((i32)ImGuiKey.F1 + (keyCode - SDLScancode.F1)),
            >= SDLScancode.F13 and <= SDLScancode.F24 => (ImGuiKey)((i32)ImGuiKey.F13 + (keyCode - SDLScancode.F13)),
            SDLScancode.Lctrl => ImGuiKey.LeftCtrl,
            SDLScancode.Lshift => ImGuiKey.LeftShift,
            SDLScancode.Lalt => ImGuiKey.LeftAlt,
            SDLScancode.Lgui => ImGuiKey.LeftSuper,
            SDLScancode.Rctrl => ImGuiKey.RightCtrl,
            SDLScancode.Rshift => ImGuiKey.RightShift,
            SDLScancode.Ralt => ImGuiKey.RightAlt,
            SDLScancode.Rgui => ImGuiKey.RightSuper,
            SDLScancode.Menu => ImGuiKey.Menu,
            SDLScancode.Unknown => ImGuiKey.None,
            _ => ImGuiKey.None
        };
        if (result == ImGuiKey.None)
            logger.LogWarning("Unmapped SDL scancode {Scancode} in ImGuiPlatform", keyCode);
        return result;
    }
}