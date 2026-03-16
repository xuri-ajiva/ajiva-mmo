using System.Numerics;
using System.Runtime.InteropServices;
using Client.Common;
using Client.Renderer.Resources;
using Client.UI;
using Common.Systems;
using Hexa.NET.ImGui;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer;

/// <summary>
/// Desktop UI rendering layer that composites UI texture over 3D content
/// Uses Ultralight texture rendered to WebGPU
/// </summary>
public class UiRenderLayer : IUiRenderLayer, IRenderLayer, IEventHandler<AjivaKeyboardEvent>,
    IEventHandler<AjivaMouseEvent>
{
    private readonly ILogger<UiRenderLayer> _logger;
    private readonly IUiTextureRenderer _uiRenderer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ResourceManager _loader;

    private TextureData? _gpuTexture;
    private bool _enabled = true;
    Vector2 TextureOffset { get; set; }
    Vector2 TextureVisibleSize { get; set; }
    Vector2 MousePositionOnTexture { get; set; }

    public void OnEvent(ref AjivaEvent<AjivaKeyboardEvent> inputEvent)
    {
        if (!HoveringUi()) return;
        _uiRenderer.OnEvent(ref inputEvent);
    }

    public void OnEvent(ref AjivaEvent<AjivaMouseEvent> inputEvent)
    {
        MousePositionOnTexture = new Vector2(inputEvent.EventData.X, inputEvent.EventData.Y) - TextureOffset;
        if (!HoveringUi()) return;
        var offsetEvent = inputEvent.EventData with
        {
            X = MousePositionOnTexture.X,
            Y = MousePositionOnTexture.Y
        };
        var uiEvent = new AjivaEvent<AjivaMouseEvent>(ref offsetEvent, inputEvent.Handled);
        _uiRenderer.OnEvent(ref uiEvent);
        if (uiEvent.Handled)
            inputEvent.Handled = true;
    }

    private bool HoveringUi()
    {
        return MousePositionOnTexture is { X: >= 0, Y: >= 0 }
               && MousePositionOnTexture.X < TextureVisibleSize.X &&
               MousePositionOnTexture.Y < TextureVisibleSize.Y;
    }

    public UiRenderLayer(
        ILogger<UiRenderLayer> logger,
        IUiTextureRenderer uiRenderer,
        IServiceProvider serviceProvider,
        ResourceManager loader)
    {
        _logger = logger;
        _uiRenderer = uiRenderer;
        _serviceProvider = serviceProvider;
        _loader = loader;
    }

    public void Attach()
    {
        _logger.LogInformation("Attaching UI render layer");
        _uiRenderer.Initialize();

        CreateGpuResources();
    }

    public void Update(UpdateInfo info)
    {
        if (!_enabled) return;
    }

    public void Render(UpdateInfo info, RenderTarget target)
    {
        if (!_enabled || !_uiRenderer.IsReady) return;

        _uiRenderer.Update(info.DeltaTime, target.QueuePtr);
        /*var texture = _uiRenderer.GetTexture();
        if (texture == null) return;

        // Upload texture to GPU if dirty
        if (texture.IsDirty)
        {
            UploadTextureToGpu(texture, target.QueuePtr);
            texture.MarkClean();
        }*/

        // TODO: Render UI quad with texture
        // This would use a simple fullscreen quad with alpha blending
        // RenderUiQuad(target);
    }

    public void RenderUi(UpdateInfo info)
    {
        if (ImGui.Begin("UI Layer"))
        {
            //ImGui.Image(_gpuTexture.View.GetIntPtr(), new Vector2(_gpuTexture.Size.Width, _gpuTexture.Size.Height));
            if (_uiRenderer.GetTexture() is { } text)
            {
                unsafe
                {
                    TextureOffset = ImGui.GetCursorScreenPos();
                    var contentRegionAvail = ImGui.GetContentRegionAvail();
                    ImGui.Image(new ImTextureRef(texId: text.GetTextureView().GetIntPtr()),
                        new Vector2(text.Width, text.Height));
                    TextureVisibleSize = ImGui.GetItemRectSize();
                    //clip visible size to content region available
                    TextureVisibleSize = new Vector2(Math.Min(TextureVisibleSize.X, contentRegionAvail.X),
                        Math.Min(TextureVisibleSize.Y, contentRegionAvail.Y));
                }
            }
            else
                ImGui.Text("UI Texture not available");
        }

        ImGui.End();

        if (ImGui.Begin("UI Debug Info"))
        {
            ImGui.Text("This is the UI Render Layer");
            ImGui.Text($"Delta Time: {info.DeltaTime:F4}s");
            ImGui.Text($"Total Time: {info.TotalTime:F2}s");
            ImGui.Text($"Ticks: {info.Ticks}");
            ImGui.Text(
                $"Mouse on UI Texture: {MousePositionOnTexture.X}, {MousePositionOnTexture.Y} (Hovering: {HoveringUi()})");
            ImGui.Text(
                $"Ui Bounds: [{TextureOffset.X}, {TextureOffset.Y}] .. [{TextureVisibleSize.X}, {TextureVisibleSize.Y}]");
            ImGui.Text($"Debug from UI Layer: {_uiRenderer.GetDebugInfo()}");
            if (ImGui.Button("Interact"))
            {
                _uiRenderer.Interact();
            }

            if (ImGui.Button("Load Example HTML"))
            {
                _uiRenderer.LoadHTML("""
                                     <h1 style='color: red;'>Hello from UI Layer!</h1>
                                     <h1>My WebGPU Game</h1>
                                     <!-- ID used for C# binding -->
                                     <div id="start_button" class="button">Start Game</div>
                                     """);
            }

            if (ImGui.Button("Load Benchmark HTML"))
            {
                _uiRenderer.LoadHTML("""
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
                                     """);
            }
        }

        ImGui.End();
    }

    private void CreateGpuResources()
    {
        _logger.LogInformation("Creating GPU resources for UI rendering");

        // TODO: Create texture, bind group, and pipeline for UI rendering
        // This would be similar to your existing texture rendering code
        // but optimized for fullscreen quad with alpha blending
    }

    private unsafe void UploadTextureToGpu(IUiTexture texture, QueuePtr queue)
    {
        _gpuTexture ??= _loader.CreateTexture(
            TextureUsage.TextureBinding | TextureUsage.CopyDst,
            "UI Texture",
            1,
            new Extent3D(texture.Width, texture.Height, 1),
            TextureFormat.Bgra8UnormSrgb, // BGRA8_UNORM_SRGB
            TextureAspect.All
        );

        // Write texture data
        var pixelData = texture.GetPixelData();
        _gpuTexture.WriteTexture<u32>(MemoryMarshal.Cast<u8, u32>(pixelData),
            new Extent3D(texture.Width, texture.Height, 1));
    }

    public void Detach()
    {
        _logger.LogInformation("Detaching UI render layer");

        _gpuTexture?.Dispose();
        _gpuTexture = null;

        _uiRenderer.Dispose();
    }

    public ref bool Enabled => ref _enabled;
}