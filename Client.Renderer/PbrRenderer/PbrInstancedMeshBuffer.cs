using Client.Renderer.Resources;
using Client.Wgsl;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.PbrRenderer;

public class PbrInstancedMeshBuffer(
    AjivaMesh<PbrGBufferShader.Attributes> mesh,
    PbrGBufferShader.PbrMaterialUniform materialUniform,
    AjivaBuffer<PbrGBufferShader.PbrMaterialUniform> uniformBuffer,
    AjivaBuffer<PbrGBufferShader.Instance> instanceBuffer,
    params IEnumerable<PbrGBufferShader.Instance> instanceInfos)
    : AdvancedInstancedMeshBuffer<PbrGBufferShader.Attributes, PbrGBufferShader.Instance,
            PbrGBufferShader.PbrMaterialUniform>
        (mesh, materialUniform, uniformBuffer, instanceBuffer, instanceInfos)
{
    // bind groups
    public BindGroupPtr? TextureBindGroup { get; set; }
    public i32 TextureBindGroupHash { get; set; }
    public BindGroupPtr? MaterialBindGroup { get; set; }
    public i32 MaterialBindGroupHash { get; set; }

    // values
    public AjivaTexture? BaseColorTexture;
    public AjivaTexture? NormalTexture;
    public AjivaTexture? MetallicRoughnessAoTexture;
    public AjivaTexture? EmissiveTexture;

    public void Deconstruct(out AjivaBuffer<PbrGBufferShader.Attributes> vertexBuffer, out AjivaBuffer<u32> indexBuffer,
        out AjivaBuffer<PbrGBufferShader.Instance>? instanceBuffer, out BindGroupPtr? textureBindGroup,
        out BindGroupPtr? materialBindGroup, out List<PbrGBufferShader.Instance> instanceInfos)
    {
        vertexBuffer = Mesh.VertexBuffer;
        indexBuffer = Mesh.IndexBuffer;
        instanceBuffer = InstanceBuffer;
        textureBindGroup = TextureBindGroup;
        materialBindGroup = MaterialBindGroup;
        instanceInfos = _instanceInfos;
    }

    public override void Dispose()
    {
        base.Dispose();
        TextureBindGroup?.Release();

        BaseColorTexture?.Dispose();
        NormalTexture?.Dispose();
        MetallicRoughnessAoTexture?.Dispose();
        EmissiveTexture?.Dispose();
    }
}