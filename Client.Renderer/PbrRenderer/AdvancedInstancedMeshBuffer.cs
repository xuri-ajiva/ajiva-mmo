using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Client.Renderer.Resources;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.PbrRenderer;

public class AdvancedInstancedMeshBuffer<TAttributes, TInstance, TModelUniform>(
    AjivaMesh<TAttributes> mesh,
    TModelUniform materialUniform,
    AjivaBuffer<TModelUniform> uniformBuffer,
    AjivaBuffer<TInstance> instanceBuffer,
    params IEnumerable<TInstance> instanceInfos) : IDisposable
    where TAttributes : unmanaged
    where TInstance : unmanaged
    where TModelUniform : unmanaged
{
    public AjivaMesh<TAttributes> Mesh { get; set; } = mesh;

    // buffers
    public AjivaBuffer<TInstance> InstanceBuffer { get; set; } = instanceBuffer;
    public AjivaBuffer<TModelUniform> UniformBuffer { get; set; } = uniformBuffer;

    // values
    private TModelUniform _materialUniform = materialUniform;
    public ref TModelUniform MaterialUniform => ref _materialUniform;

    public readonly List<TInstance> _instanceInfos = [..instanceInfos];
    public Span<TInstance> InstanceInfos => CollectionsMarshal.AsSpan(_instanceInfos);

    public void CreateAndWriteBuffers(QueuePtr queue, ResourceManager loader)
    {
        UniformBuffer.QueuedWrite(new ReadOnlySpan<TModelUniform>(ref MaterialUniform), queue);

        if (InstanceBuffer.Size < (u64)InstanceInfos.Length * (u64)Unsafe.SizeOf<TInstance>())
        {
            InstanceBuffer.ResizeWithoutDataPreserve((u64)InstanceInfos.Length * (u64)Unsafe.SizeOf<TInstance>());
        }

        InstanceBuffer.QueuedWrite(InstanceInfos, queue);
    }

    public virtual void Dispose()
    {
        Mesh.Dispose();
        InstanceBuffer.Dispose();
        UniformBuffer.Dispose();
    }
}