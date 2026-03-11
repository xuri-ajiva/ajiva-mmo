using Common.Systems;
using Silk.NET.WebGPU.Extensions.ImGui;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer;

public interface IRenderLayer : IAjivaLayer
{
    void Render(UpdateInfo info, RenderTarget target);
}

public ref struct RenderTarget(QueuePtr queuePtr)
{
    public readonly QueuePtr QueuePtr = queuePtr;
    //public readonly ImGuiController ImguiController = imguiController;
}