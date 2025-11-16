using Silk.NET.WebGPU.Extensions.ImGui;
using Silk.NET.WebGPU.Safe;

namespace Client.LayerSystem;

public interface IRenderLayer : IAjivaLayer
{
    void Render(UpdateInfo info, RenderTarget target);
}

public ref struct RenderTarget(QueuePtr queuePtr, ImGuiController imguiController)
{
    public readonly QueuePtr QueuePtr = queuePtr;
    public readonly ImGuiController ImguiController = imguiController;
}