using Common.Systems;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer;

public interface IRenderLayer : IAjivaLayer
{
    void Render(UpdateInfo info, RenderTarget target);
}

public readonly ref struct RenderTarget(in QueuePtr queuePtr)
{
    public readonly ref readonly QueuePtr QueuePtr = ref queuePtr;
}