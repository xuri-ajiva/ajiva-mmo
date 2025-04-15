using Client.Renderer.Resources;
using Client.Wgsl;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.PbrRenderer;

public class PbrInstancedMeshBuffer(
    AjivaMesh<PbrGBufferShader.Attributes> mesh,
    PbrGBufferShader.PbrMaterialUniform materialUniform,
    PbrGBufferShader.Instance[] instanceInfos)
: IInstancedMeshBuffer<PbrGBufferShader.Attributes, PbrGBufferShader.Instance, PbrGBufferShader.PbrMaterialUniform>
{
    public AjivaMesh<PbrGBufferShader.Attributes> Mesh { get; set; } = mesh;

    // buffers
    public AjivaBuffer<PbrGBufferShader.Instance>? InstanceBuffer { get; set; }
    public AjivaBuffer<PbrGBufferShader.PbrMaterialUniform> MaterialUniformBuffer { get; set; }
    public AjivaBuffer<PbrGBufferShader.PbrMaterialUniform> UniformBuffer => MaterialUniformBuffer;

    // bind groups
    public BindGroupPtr? TextureBindGroup { get; set; }
    public i32  TextureBindGroupHash { get; set; }
    public BindGroupPtr? MaterialBindGroup { get; set; }
    public i32  MaterialBindGroupHash { get; set; }

    // values
    private PbrGBufferShader.PbrMaterialUniform _materialUniform = materialUniform;
    public ref PbrGBufferShader.PbrMaterialUniform MaterialUniform => ref _materialUniform;

    private PbrGBufferShader.Instance[] _instanceInfos = instanceInfos;
    public ref PbrGBufferShader.Instance[] InstanceInfos => ref _instanceInfos;
    public AjivaTexture? BaseColorTexture;
    public AjivaTexture? NormalTexture;
    public AjivaTexture? MetallicRoughnessAoTexture;
    public AjivaTexture? EmissiveTexture;

    public void Deconstruct(out AjivaBuffer<PbrGBufferShader.Attributes> vertexBuffer, out AjivaBuffer<u32> indexBuffer,
        out AjivaBuffer<PbrGBufferShader.Instance>? instanceBuffer, out BindGroupPtr? textureBindGroup,
        out BindGroupPtr? materialBindGroup, out PbrGBufferShader.Instance[] instanceInfos)
    {
        vertexBuffer = Mesh.VertexBuffer;
        indexBuffer = Mesh.IndexBuffer;
        instanceBuffer = InstanceBuffer;
        textureBindGroup = TextureBindGroup;
        materialBindGroup = MaterialBindGroup;
        instanceInfos = InstanceInfos;
    }

    public void Dispose()
    {
        Mesh.Dispose();
        InstanceBuffer?.Dispose();
        TextureBindGroup?.Release();
        MaterialUniformBuffer.Dispose();

        BaseColorTexture?.Dispose();
        NormalTexture?.Dispose();
        MetallicRoughnessAoTexture?.Dispose();
        EmissiveTexture?.Dispose();
    }
}
