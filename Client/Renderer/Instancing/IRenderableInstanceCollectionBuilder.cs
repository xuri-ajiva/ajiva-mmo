using Client.LayerSystem;
using Interception;

namespace Client.Renderer.Instancing;

public interface IRenderableInstanceCollectionBuilder
{
    [TraceInterception]
    void PrepareDrawEntities(RenderableInstanceCollection target);

    [TraceInterception]
    void UpdateDrawEntities(UpdateInfo info, RenderableInstanceCollection target);

    ref f32 SmoothingFrames { get; }
    void RequestReBuild();

    string Stats();
}