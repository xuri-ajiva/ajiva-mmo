using Client.Platform;
using Client.Platform.ImGuiPlatform;
using Silk.NET.WebGPU.Extensions.ImGui;
using Silk.NET.WebGPU.Safe;

namespace Client.LayerSystem;

public interface IAjivaLayerSystem
{
    void Attach(AjivaWindow parent);
    void Update(UpdateInfo info);
    void PushLayer<T>(T? layer = default) where T : IAjivaLayer;
    void PopLayer<T>() where T : IAjivaLayer;
    void Detach();
    void Render(UpdateInfo info);
}

public ref struct UpdateInfo(u64 ticks, f32 deltaTime, f32 totalTime)
{
    public UpdateInfo(AjivaClock updateClock) : this(
        updateClock.Ticks,
        (f32)updateClock.DeltaTime.TotalSeconds,
        (f32)updateClock.Total.TotalSeconds)
    {
    }

    public readonly u64 Ticks = ticks;
    public readonly f32 DeltaTime = deltaTime;
    public readonly f32 TotalTime = totalTime;
}

public ref struct RenderTarget(QueuePtr queuePtr, ImGuiController imguiController)
{
    public readonly QueuePtr QueuePtr = queuePtr;
    public readonly ImGuiController ImguiController = imguiController;
}

public interface IAjivaLayer
{
    void Attach();
    void Update(UpdateInfo info);
    void Detach();
    ref bool Enabled { get; }
}

public interface IRenderLayer : IAjivaLayer
{
    void Render(UpdateInfo info, RenderTarget target);
}

public interface IUiRenderLayer : IAjivaLayer
{
    void RenderUi(UpdateInfo info);
}
