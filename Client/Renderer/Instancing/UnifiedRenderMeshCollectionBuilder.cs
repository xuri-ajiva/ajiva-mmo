using Client.LayerSystem;

namespace Client.Renderer.Instancing;

public interface IRenderableInstanceCollectionBuilder
{
    void PrepareDrawEntities(RenderableInstanceCollection target);
    void UpdateDrawEntities(UpdateInfo info, RenderableInstanceCollection target);
    ref f32 SmoothingFrames { get; }
    void RequestReBuild();
}