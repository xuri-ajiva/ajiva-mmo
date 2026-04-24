using System.Text;
using Client.Common;
using Client.Renderer.Resources;
using Client.UI.Desktop.RmlInterfaces;
using Client.UI.Desktop.wgpu;
using Client.Wgsl;
using Common;
using GlmSharp;
using Hexa.NET.SDL3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RmlUiNet;
using RmlUiNet.Input;
using Silk.NET.WebGPU.Safe;

namespace Client.UI.Desktop;

public class RmlUiRenderer(ILoggerFactory logger, IServiceProvider services, u32 width = 1920, u32 height = 1080)
    : IUiTextureRenderer
{
    private readonly ILogger<RmlUiRenderer> _logger = logger.CreateLogger<RmlUiRenderer>();
    private u32 _width = width;
    private u32 _height = height;
    private bool _isReady;

    private static bool _wasInitialized;

    private readonly FileInterface _fileInterface = new FileInterfaceImple();

    private readonly SystemInterface _systemInterface =
        new SystemInterfaceImple(logger.CreateLogger<SystemInterfaceImple>());

    private RmlWebGpuRenderer _renderInterface = null!;
    private Context? _context;
    private ElementDocument? _doc;

    public void Initialize()
    {
        _logger.LogInformation("Initializing RmlUi renderer {Width}x{Height}", _width, _height);


        _renderInterface = new RmlWebGpuRenderer(
            logger,
            services.GetRequiredService<ResourceManager>(),
            services.GetRequiredService<ShaderReloadManager>()
        );
        _renderInterface.Initialize();
        Rml.SetFileInterface(_fileInterface);
        Rml.SetRenderInterface(_renderInterface);
        Rml.SetSystemInterface(_systemInterface);

        if (_wasInitialized)
        {
            _logger.LogWarning("Rml already initialized, skipping");
        }
        else
        {
            if (Rml.Initialise())
            {
                _wasInitialized = true;
                _logger.LogInformation("Rml initialized successfully");
            }
            else
            {
                _logger.LogError("Failed to initialize Rml");
            }
        }

        _context = Rml.CreateContext("MainContext", new Vector2i((i32)_width, (i32)_height), _renderInterface)!;
        Debugger.Initialise(_context);
        Debugger.SetVisible(true);
        Rml.LoadFontFace("resources/Quicksand-SemiBold.ttf");
        Rml.LoadFontFace("resources/LatoLatin-Bold.ttf");
        Rml.LoadFontFace("resources/LatoLatin-BoldItalic.ttf");
        Rml.LoadFontFace("resources/LatoLatin-Italic.ttf");
        Rml.LoadFontFace("resources/LatoLatin-Regular.ttf");
        Rml.LoadFontFace("resources/NotoEmoji-Regular.ttf");

        _logger.LogInformation("RmlUi renderer initialized");
        _isReady = true;
    }


    public void LoadHTML(string rml)
    {
        _logger.LogDebug("Loading HTML content");
        _doc?.Close();
        _doc = _context?.LoadDocumentFromMemory(rml);

        if (_doc?.GetElementById("title") is { } titleEle)
            titleEle.SetInnerRml("Testing UI Renderer");
        _doc?.Show();
    }

    public void LoadURL(string url)
    {
        _logger.LogInformation("Loading URL: {Url}", url);
        _doc?.Close();
        _doc = _context?.LoadDocument(url);
        _doc?.Show();
    }

    public async Task<string> ExecuteJavaScript(string script)
    {
        _logger.LogDebug("Executing JavaScript");
        if (_doc == null)
        {
            _logger.LogWarning("No document loaded, cannot execute JavaScript");
            return string.Empty;
        }

        return "";
    }

    public void Update(float deltaTime, QueuePtr targetQueuePtr)
    {
        using var trace = Instrumentation.NamedTrace("start_frame");
        if (_doc?.GetElementById("fps") is not { } el) return;
        el.SetInnerRml($"FPS: {(i32)(1.0f / deltaTime)}");
        Instrumentation.RecordEvent("updated_document");

        using (Instrumentation.NamedTrace("Update"))
            _context?.Update();

        using (Instrumentation.NamedTrace("StartCapture"))
            _renderInterface.StartCapture();

        using (Instrumentation.NamedTrace("Render"))
            _context?.Render();

        using (Instrumentation.NamedTrace("ExecuteRender"))
            _renderInterface.ExecuteRender();
    }

    public void Resize(u32 width, u32 height)
    {
        if (_width == width && _height == height) return;

        _logger.LogInformation("Resizing UI from {OldW}x{OldH} to {NewW}x{NewH}", _width, _height, width, height);

        _width = width;
        _height = height;
    }

    public IUiTexture? GetTexture()
    {
        if (!_isReady) return null;
        if (_renderInterface.OutputFramebuffer is { Width: var width, Height: var height } fb &&
            fb.TryGetTargets(out var colorTarget))
        {
            return new RefTexture(colorTarget.View, new vec2(width, height));
        }

        return null;
    }

    public string GetDebugInfo()
    {
        if (_context is null) return "Context is null";
        return $"""
                IsMouseInteracting {_context.IsMouseInteracting}
                Document TagName {_doc?.TagName}
                Renderer {_renderInterface.DebugInfo()}
                """;
    }

    public void Interact()
    {
        StringBuilder rml = new();

        for (var i = 0; i < 50; i++)
        {
            var index = Random.Shared.Next() % 1000;
            var route = Random.Shared.Next() % 50;
            var max = Random.Shared.Next() % 40 + 10;
            var value = Random.Shared.Next() % max;
            rml.AppendLine($"""
                            <div class="row">
                                <div class="col col1"><button class="expand" index="{index}">+</button>&nbsp;<a>Route {route}</a></div>
                                    <div class="col col23"><input type="range" class="assign_range" min="0" max="{max}" value="{value}"/></div>
                                        <div class="col col4">Assigned</div>
                                    <select>
                                        <option>Red</option>
                                        <option>Blue</option>
                                        <option selected>Green</option>
                                        <option style="background-color: yellow;">Yellow</option>
                                    </select>
                                    <div class="inrow unmark_collapse">
                                    <div class="col col123 assign_text">Assign to route</div>
                                    <div class="col col4">
                                        <input type="submit" class="vehicle_depot_assign_confirm" quantity="0">Confirm</input>
                                    </div>
                                </div>
                            </div>
                            """);
        }

        if (_doc?.GetElementById("performance") is { } el)
            el.SetInnerRml(rml.ToString());
    }

    public bool IsReady => _isReady;
    public u32 Width => _width;
    public u32 Height => _height;

    public void Dispose()
    {
        if (!IsReady) return;
        _logger.LogInformation("Disposing RmlUi renderer");
        _isReady = false;
        _doc?.Close();
        _context?.Dispose();
        _renderInterface.Dispose();
        Rml.Shutdown();
    }

    public void OnEvent(ref AjivaEvent<AjivaKeyboardEvent> inputEvent)
    {
        if (_context is null) return;
        var data = inputEvent.EventData;
        switch (data.IsPressed)
        {
            case AjivaKeyboardEventType.KeyDown:
                if (!_context.ProcessKeyDown(ToRmlKeyEvent(data.KeyCode), KeyModifier.None))
                    inputEvent.Handled = true;
                break;
            case AjivaKeyboardEventType.KeyUp:
                if (!_context.ProcessKeyUp(ToRmlKeyEvent(data.KeyCode), KeyModifier.None))
                    inputEvent.Handled = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private KeyIdentifier ToRmlKeyEvent(SDLScancode dataKeyCode)
    {
        return dataKeyCode switch
        {
            SDLScancode.Unknown => KeyIdentifier.KI_UNKNOWN,
            >= SDLScancode.A and <= SDLScancode.Z => KeyIdentifier.KI_A + (u8)(dataKeyCode - SDLScancode.A),
            >= SDLScancode.Scancode1 and <= SDLScancode.Scancode9 => KeyIdentifier.KI_1 + (u8)(dataKeyCode - SDLScancode.Scancode1),
            SDLScancode.Scancode0 => KeyIdentifier.KI_0,
            SDLScancode.Space => KeyIdentifier.KI_SPACE,
            SDLScancode.Minus => KeyIdentifier.KI_OEM_MINUS,
            SDLScancode.Equals => KeyIdentifier.KI_OEM_PLUS,
            SDLScancode.Comma => KeyIdentifier.KI_OEM_COMMA,
            SDLScancode.Period => KeyIdentifier.KI_OEM_PERIOD,
            SDLScancode.Slash => KeyIdentifier.KI_OEM_2,
            SDLScancode.Backslash => KeyIdentifier.KI_OEM_5,
            SDLScancode.Semicolon => KeyIdentifier.KI_OEM_1,
            SDLScancode.Grave => KeyIdentifier.KI_OEM_3,
            SDLScancode.Leftbracket => KeyIdentifier.KI_OEM_4,
            SDLScancode.Rightbracket => KeyIdentifier.KI_OEM_6,
            SDLScancode.Apostrophe => KeyIdentifier.KI_OEM_7,
            SDLScancode.Capslock => KeyIdentifier.KI_OEM_8,
            SDLScancode.Numlockclear => KeyIdentifier.KI_OEM_102,
            >= SDLScancode.Kp1 and <= SDLScancode.Kp9 => KeyIdentifier.KI_NUMPAD1 + (u8)(dataKeyCode - SDLScancode.Kp1),
            SDLScancode.Kp0 => KeyIdentifier.KI_NUMPAD0,
                _ => KeyIdentifier.KI_UNKNOWN  
        };
    }


    public void OnEvent(ref AjivaEvent<AjivaMouseEvent> inputEvent)
    {
        if (_context is null) return;

        var data = inputEvent.EventData;
        switch (data.Type)
        {
            case AjivaMouseEventType.Move:
                if (!_context.ProcessMouseMove((i32)data.X, (i32)data.Y, KeyModifier.None))
                    inputEvent.Handled = true;
                break;
            case AjivaMouseEventType.ButtonDown:
                if (!_context.ProcessMouseButtonDown((i32)data.Button, KeyModifier.None))
                    inputEvent.Handled = true;
                break;
            case AjivaMouseEventType.ButtonUp:
                if (!_context.ProcessMouseButtonUp((i32)data.Button, KeyModifier.None))
                    inputEvent.Handled = true;
                break;
            case AjivaMouseEventType.Scroll:
                if (!_context.ProcessMouseWheel(new Vector2f(-data.DeltaX, data.DeltaY), KeyModifier.None))
                    inputEvent.Handled = true;
                break;
            case AjivaMouseEventType.DoubleClick:
                inputEvent.Handled = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}