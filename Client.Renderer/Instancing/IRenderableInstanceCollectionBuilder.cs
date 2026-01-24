using System.Numerics;
using Common.Ecs;
using Common.Systems;
using Interception;

namespace Client.Renderer.Instancing;

public interface IRenderableInstanceCollectionBuilder
{
    public static readonly Vector4[] ColorPallet =
    [
        new Vector4(0.9f, 0.1f, 0.1f, 1f), // Red
        new Vector4(0.9f, 0.5f, 0.1f, 1f), // Orange
        new Vector4(0.9f, 0.9f, 0.1f, 1f), // Yellow
        new Vector4(0.5f, 0.9f, 0.1f, 1f), // Lime Green
        new Vector4(0.1f, 0.9f, 0.1f, 1f), // Green
        new Vector4(0.1f, 0.9f, 0.5f, 1f), // Spring Green
        new Vector4(0.1f, 0.9f, 0.9f, 1f), // Cyan
        new Vector4(0.1f, 0.5f, 0.9f, 1f), // Sky Blue
        new Vector4(0.1f, 0.1f, 0.9f, 1f), // Blue
        new Vector4(0.5f, 0.1f, 0.9f, 1f), // Violet
        new Vector4(0.9f, 0.1f, 0.9f, 1f), // Magenta
        new Vector4(0.9f, 0.1f, 0.5f, 1f), // Rose
        new Vector4(1f, 0.3f, 0.3f, 1f), // Light Red
        new Vector4(1f, 0.7f, 0.3f, 1f), // Light Orange
        new Vector4(1f, 1f, 0.3f, 1f), // Light Yellow
        new Vector4(0.7f, 1f, 0.3f, 1f), // Light Lime
        new Vector4(0.3f, 1f, 0.3f, 1f), // Light Green
        new Vector4(0.3f, 1f, 0.7f, 1f), // Light Spring Green
        new Vector4(0.3f, 1f, 1f, 1f), // Light Cyan
        new Vector4(0.3f, 0.7f, 1f, 1f), // Light Sky Blue
        new Vector4(0.3f, 0.3f, 1f, 1f), // Light Blue
        new Vector4(0.7f, 0.3f, 1f, 1f), // Light Violet
        new Vector4(1f, 0.3f, 1f, 1f), // Light Magenta
        new Vector4(1f, 0.3f, 0.7f, 1f), // Light Rose
        new Vector4(0.6f, 0f, 0f, 1f), // Dark Red
        new Vector4(0.6f, 0.3f, 0f, 1f), // Dark Orange
        new Vector4(0.6f, 0.6f, 0f, 1f), // Dark Yellow
        new Vector4(0.3f, 0.6f, 0f, 1f), // Dark Lime
        new Vector4(0f, 0.6f, 0f, 1f), // Dark Green
        new Vector4(0f, 0.6f, 0.6f, 1f) // Dark Cyan
    ];

    [TraceInterception]
    void PrepareDrawEntities(RenderableInstanceCollection target);

    [TraceInterception]
    void UpdateDrawEntities(UpdateInfo info, RenderableInstanceCollection target);

    ref f32 SmoothingFrames { get; }
    void RequestReBuild();

    string Stats();
    EntityId GetEntityId(u32 selectedObjectId, u32 selectedInstanceId);
}