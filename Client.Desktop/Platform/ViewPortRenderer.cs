using System.Numerics;
using Client.Renderer;
using Client.Renderer.Renderers;
using Common.Systems;
using GlmSharp;
using Hexa.NET.ImGui;
using Silk.NET.WebGPU.Safe;

namespace Client.Desktop.Platform;

public class ViewPortRenderer(Instanced3dDeferredRenderer renderer) : IRenderLayer
{
    public void Attach() => renderer.Initiallize();
    public void Detach() => renderer.Teardown();

    public void Update(UpdateInfo info)
    {
        renderer.Update(info);
    }


    public void Render(UpdateInfo info, RenderTarget target)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

        if (!ImGui.Begin("Scene"))
        {
            ImGui.End();
            ImGui.PopStyleVar();
            return;
        }

        var size = ImGui.GetContentRegionAvail();

        //var imDrawListPtr = ImGui.GetBackgroundDrawList();
        //var size = imDrawListPtr.GetClipRectMax();
        u32 width = (u32)size.X, height = (u32)Math.Max(0, size.Y);

        var mousePosition = ImGui.GetMousePos() -
                            (ImGui.GetCursorPos() + ImGui.GetWindowPos()) * ImGui.GetWindowDpiScale();
        //ImGui.Text($"Mouse: {ImGui.GetMousePos()}, Cursor: {ImGui.GetCursorPos()}, WindowPos: {ImGui.GetWindowPos()}, DpiScale: {ImGui.GetWindowDpiScale()}, result : {_mousePosition}");

        renderer.PixelReadRequest = new uvec2(Math.Clamp((u32)mousePosition.X, 0, width - 1),
            Math.Clamp((u32)mousePosition.Y, 0, height - 1));

        if (renderer.Render(info, target, width, height, _isSceneViewHovered) is { } colorOutput)
        {
            unsafe
            {
                //imDrawListPtr.AddImage(color0.GetIntPtr(), new Vector2(0, 0), new Vector2(width, height));
                ImGui.Image(new ImTextureRef(texId: colorOutput.View.GetIntPtr()), new Vector2(width, height));
                _isSceneViewHovered = ImGui.IsItemHovered();
            }
        }

        ImGui.End();
        ImGui.PopStyleVar();
    }

    public ref bool Enabled => ref _enabled;
    private bool _enabled = true;
    private bool _isSceneViewHovered;
}