using Client.Renderer.Resources;

namespace Client.Renderer.PbrRenderer;

public class AjivaMesh<TAttributes>(
    AjivaBuffer<TAttributes> vertexBuffer,
    AjivaBuffer<u32> indexBuffer,
    bool cleanUp = false) : IDisposable where TAttributes : unmanaged
{
    public AjivaBuffer<TAttributes> VertexBuffer { get; set; } = vertexBuffer;
    public AjivaBuffer<u32> IndexBuffer { get; set; } = indexBuffer;
    private bool CleanUp { get; set; } = cleanUp;

    public void Dispose()
    {
        if (!CleanUp) return;
        VertexBuffer.Dispose();
        IndexBuffer.Dispose();
    }

    public AjivaMesh<TAttributes> Clone(bool cleanUp = false)
    {
        return new AjivaMesh<TAttributes>(VertexBuffer, IndexBuffer, cleanUp);
    }
}