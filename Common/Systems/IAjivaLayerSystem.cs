namespace Common.Systems;

public interface IAjivaLayerSystem<TParent>
{
    void Attach(TParent parent);
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


public interface IAjivaLayer
{
    void Attach();
    void Update(UpdateInfo info);
    void Detach();
    ref bool Enabled { get; }
}

public interface IUiRenderLayer : IAjivaLayer
{
    void RenderUi(UpdateInfo info);
}