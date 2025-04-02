using Client.Renderer.Resources;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.PbrRenderer;

public interface IInstancedMeshBuffer<TAttributes, TInstance, TModelUniform> : IDisposable
    where TAttributes : unmanaged
    where TInstance : unmanaged
    where TModelUniform : unmanaged
{
    public AjivaMesh<TAttributes> Mesh { get; set; }
    public AjivaBuffer<TInstance>? InstanceBuffer { get; set; }
    public AjivaBuffer<TModelUniform> ModelUniformBuffer { get; set; }
    public BindGroupPtr? ModelBindGroup { get; set; }
    public i32 ModelBindGroupHash { get; set; }
    public ref TModelUniform ModelUniform { get;}
    public ref TInstance[] InstanceInfos { get; }
}
