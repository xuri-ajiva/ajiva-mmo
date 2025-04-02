using Client.Renderer.Resources;
using Client.Wgsl;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.PbrRenderer;

public class OITInstancedMeshBuffer(
    AjivaMesh<WboitAccumShader.Attributes> mesh,
    WboitAccumShader.ModelUniform modelUniform,
    WboitAccumShader.Instance[] instanceInfos)
    : IInstancedMeshBuffer<WboitAccumShader.Attributes, WboitAccumShader.Instance, WboitAccumShader.ModelUniform>
{
    public AjivaMesh<WboitAccumShader.Attributes> Mesh { get; set; } = mesh;

    // buffers
    public AjivaBuffer<WboitAccumShader.Instance>? InstanceBuffer { get; set; }
    public AjivaBuffer<WboitAccumShader.ModelUniform> ModelUniformBuffer { get; set; }

    // bind groups
    public BindGroupPtr? ModelBindGroup { get; set; }
    public i32 ModelBindGroupHash { get; set; }

    // values
    private WboitAccumShader.ModelUniform _modelUniform = modelUniform;
    public ref WboitAccumShader.ModelUniform ModelUniform => ref _modelUniform;


    private WboitAccumShader.Instance[] _instanceInfos = instanceInfos;
    public ref WboitAccumShader.Instance[] InstanceInfos => ref _instanceInfos;
    public AjivaTexture AlbedoTexture;
    public AjivaTexture EmissionTexture;

    public void Deconstruct(out AjivaBuffer<WboitAccumShader.Attributes> vertexBuffer, out AjivaBuffer<u32> indexBuffer,
        out AjivaBuffer<WboitAccumShader.Instance>? instanceBuffer, out BindGroupPtr? modelBindGroup,
        out WboitAccumShader.Instance[] instanceInfos)
    {
        vertexBuffer = Mesh.VertexBuffer;
        indexBuffer = Mesh.IndexBuffer;
        instanceBuffer = InstanceBuffer;
        modelBindGroup = ModelBindGroup;
        instanceInfos = InstanceInfos;
    }

    public void Dispose()
    {
        Mesh.Dispose();
        InstanceBuffer?.Dispose();
        ModelBindGroup?.Release();
        ModelUniformBuffer.Dispose();

        AlbedoTexture.Dispose();
        EmissionTexture.Dispose();
    }
}
