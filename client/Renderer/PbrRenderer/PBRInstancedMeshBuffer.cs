using Client.Renderer.Resources;
using Client.Wgsl;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.PbrRenderer;

public class PBRInstancedMeshBuffer(
    AjivaMesh<TempleShader.Attributes> mesh,
    TempleShader.ModelUniform modelUniform,
    TempleShader.Instance[] instanceInfos)
    : IInstancedMeshBuffer<TempleShader.Attributes, TempleShader.Instance, TempleShader.ModelUniform>
{
    public AjivaMesh<TempleShader.Attributes> Mesh { get; set; } = mesh;

    // buffers
    public AjivaBuffer<TempleShader.Instance>? InstanceBuffer { get; set; }
    public AjivaBuffer<TempleShader.ModelUniform> ModelUniformBuffer { get; set; }

    // bind groups
    public BindGroupPtr? ModelBindGroup { get; set; }
    public i32 ModelBindGroupHash { get; set; }

    // values
    private TempleShader.ModelUniform _modelUniform = modelUniform;
    public ref TempleShader.ModelUniform ModelUniform => ref _modelUniform;

    public TempleShader.Instance[] _instanceInfos = instanceInfos;
    public ref TempleShader.Instance[] InstanceInfos => ref _instanceInfos;
    public AjivaTexture? AlbedoTexture;
    public AjivaTexture? NormalTexture;
    public AjivaTexture? EmissionTexture;

    public void Deconstruct(out AjivaBuffer<TempleShader.Attributes> vertexBuffer, out AjivaBuffer<u32> indexBuffer,
        out AjivaBuffer<TempleShader.Instance>? instanceBuffer, out BindGroupPtr? modelBindGroup,
        out TempleShader.Instance[] instanceInfos)
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
        NormalTexture.Dispose();
        EmissionTexture.Dispose();
    }
}
