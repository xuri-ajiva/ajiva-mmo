using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Client.Renderer.Resources;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.PbrRenderer;

public class SimpleInstancedMeshBBuffer<TAttributes, TInstance>(
    AjivaMesh<TAttributes> mesh,
    AjivaBuffer<TInstance> instanceBuffer,
    params IEnumerable<TInstance> instanceInfos) : IDisposable
    where TAttributes : unmanaged
    where TInstance : unmanaged
{
    public AjivaMesh<TAttributes> Mesh { get; set; } = mesh;
    public AjivaBuffer<TInstance> InstanceBuffer { get; set; } = instanceBuffer;

    internal readonly List<TInstance> _instanceInfos = [..instanceInfos];
    public Span<TInstance> InstanceInfos => CollectionsMarshal.AsSpan(_instanceInfos);

    public void CreateAndWriteBuffers(QueuePtr queue, ResourceManager loader)
    {
        ReadOnlySpan<TInstance> data = InstanceInfos;
        if (InstanceBuffer.Size < (u64)data.Length * (u64)Unsafe.SizeOf<TInstance>())
        {
            InstanceBuffer.ResizeWithoutDataPreserve((u64)data.Length * (u64)Unsafe.SizeOf<TInstance>());
        }

        InstanceBuffer.QueuedWrite(data, queue);
    }

    public virtual void Dispose()
    {
        Mesh.Dispose();
        InstanceBuffer.Dispose();
    }
}