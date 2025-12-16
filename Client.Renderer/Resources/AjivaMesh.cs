namespace Client.Renderer.Resources;

public class MeshData<TAttributes>(
    ResourceManager loader,
    string label,
    AjivaBuffer<TAttributes> vertexBuffer,
    AjivaBuffer<u32> indexBuffer)
    : GpuResource<MeshData<TAttributes>>(loader, label) where TAttributes : unmanaged
{
    public AjivaBuffer<TAttributes> VertexBuffer { get; set; } = vertexBuffer;
    public AjivaBuffer<u32> IndexBuffer { get; set; } = indexBuffer;

    protected override void Destroy()
    {
        VertexBuffer.Dispose();
        IndexBuffer.Dispose();
    }

    public MeshData<TAttributes> Clone(string? label)
    {
        return new MeshData<TAttributes>(Loader, $"CopyOf::{Label}::as::{label}",
            VertexBuffer, IndexBuffer) { _clone = true };
    }
}

public class AjivaMesh<TAttributes>(MeshData<TAttributes> data)
    : GpuResourceHandle<MeshData<TAttributes>>(data) where TAttributes : unmanaged
{
    public bool IsDisposed => Data.IsDisposed;
    public AjivaBuffer<TAttributes> VertexBuffer => Data.VertexBuffer;
    public AjivaBuffer<u32> IndexBuffer => Data.IndexBuffer;

    public AjivaMesh<TAttributes> Clone(string? label = null)
    {
        var clone = Data.Clone(label);
        return new AjivaMesh<TAttributes>(clone);
    }
}
