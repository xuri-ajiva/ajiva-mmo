using System.Text;
using Client.Common;
using Client.Renderer.Resources;
using Client.UI.Desktop.RmlInterfaces;
using Client.UI.Desktop.wgpu;
using Client.Wgsl;
using Common;
using GlmSharp;
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
    private readonly SystemInterface _systemInterface = new SystemInterfaceImple(logger.CreateLogger<SystemInterfaceImple>());
    private RmlWebGpuRenderer _renderInterface = null!;
    private Context? _context;
    private ElementDocument? _doc;

    public void Initialize()
    {
        _logger.LogInformation("Initializing Ultralight renderer {Width}x{Height}", _width, _height);


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

        _context = Rml.CreateContext("MainContext", new Vector2i((i32)_width, (i32)_height), _renderInterface);
        RmlUiNet.Debugger.Initialise(_context);
        RmlUiNet.Debugger.SetVisible(true);
        Rml.LoadFontFace("resources/Quicksand-SemiBold.ttf");
        Rml.LoadFontFace("resources/LatoLatin-Bold.ttf");
        Rml.LoadFontFace("resources/LatoLatin-BoldItalic.ttf");
        Rml.LoadFontFace("resources/LatoLatin-Italic.ttf");
        Rml.LoadFontFace("resources/LatoLatin-Regular.ttf");
        Rml.LoadFontFace("resources/NotoEmoji-Regular.ttf");

        _logger.LogInformation("Ultralight renderer initialized");
        _isReady = true;
    }


    public void LoadHTML(string html, string style)
    {
        _logger.LogDebug("Loading HTML content");
        _doc?.Close();
        _doc = _context?.LoadDocumentFromMemory(
            """
            <rml>
            <head>
            	<link type="text/template" href="../resources/rml/window.rml"/>
            	<title>Benchmark Sample</title>
            	<style>
            		body.window
            		{
            			left: 80dp;
            			right: 80dp;
            			top: 50dp;
            			bottom: 50dp;
            			max-width: 2000dp;
            			max-height: 2000dp;
            		}
            		#fps
            		{
            			position: absolute;
            			top: 55dp;
            			left: 20dp;
            			font-size: 0.85em;
            			text-align: left;
            		}
            		#performance
            		{
            			width: 800dp;
            			height: 300dp;
            		}
            	</style>
            </head>

            <body template="window">
            <div id="fps"/>
            <div id="click_test"/>
            <div id="performance"/>
            </body>
            </rml>
            """
            /*
            $$"""
              <rml>
                  <head>
                      <title>Main Menu</title>
                      <style>
                          body {
                              font-family: Quicksand;
                              font-size: 24px;
                              width: 100%;
                              height: 100%;
                              background-color: #000000aa; /* Semi-transparent background #1#
                          }

                          .button {
                              display: block;
                              background-color: #af0000;
                              color: white;
                              text-align: center;
                              margin: auto;
                          }

                          .button:hover {
                              background-color: #ff5555;
                          }

                          {{style}}
                      </style>
                  </head>
                  <body>
                      {{html}}
                  </body>
              </rml>
              """*/ /**/);

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

        for (i32 i = 0; i < 50; i++)
        {
            i32 index = Random.Shared.Next() % 1000;
            i32 route = Random.Shared.Next() % 50;
            i32 max = Random.Shared.Next() % 40 + 10;
            i32 value = Random.Shared.Next() % max;
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
        _logger.LogInformation("Disposing Ultralight renderer");
        _isReady = false;
    }

    public void OnEvent(ref AjivaEvent<AjivaKeyboardEvent> inputEvent)
    {
        if (_context is null) return;
        var data = inputEvent.EventData;
        /*switch (data.IsPressed)
        {
            case AjivaKeyboardEventType.KeyDown:
                if (!_context.ProcessKeyDown(data.KeyCode.ToRmlKeyEvent(), KeyModifier.None))
                    inputEvent.Handled = true;
                break;
            case AjivaKeyboardEventType.KeyUp:
                if (!_context.ProcessKeyUp(data.KeyCode.ToRmlKeyEvent(), KeyModifier.None))
                    inputEvent.Handled = true;
                break;
            case AjivaKeyboardEventType.Key:
                if (!_context.ProcessTextInput(char.ConvertFromUtf32((i32)data.KeyCode))) ;
                inputEvent.Handled = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }*/
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