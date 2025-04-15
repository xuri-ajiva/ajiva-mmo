using Client.Renderer.Resources;

namespace Client.Renderer.PbrRenderer;

public interface IInstancedMeshBuffer<TAttributes, TInstance, TModelUniform> : IDisposable
    where TAttributes : unmanaged
    where TInstance : unmanaged
    where TModelUniform : unmanaged
{
    public AjivaMesh<TAttributes> Mesh { get; set; }
    public AjivaBuffer<TInstance>? InstanceBuffer { get; set; }

    public AjivaBuffer<TModelUniform> UniformBuffer { get; }

    //public BindGroupPtr? ModelBindGroup { get; set; }
    public ref TModelUniform MaterialUniform { get; }
    public ref TInstance[] InstanceInfos { get; }
}
