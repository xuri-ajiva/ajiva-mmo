using Client.Renderer.Resources;
using Client.Wgsl;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.PbrRenderer;

public class OitInstancedMeshBuffer(
    AjivaMesh<WboitAccumShader.Attributes> mesh,
    WboitAccumShader.ModelUniform modelUniform,
    AjivaBuffer<WboitAccumShader.ModelUniform> uniformBuffer,
    AjivaBuffer<WboitAccumShader.Instance> instanceBuffer,
    params WboitAccumShader.Instance[] instanceInfos)
    : AdvancedInstancedMeshBuffer<WboitAccumShader.Attributes, WboitAccumShader.Instance,
        WboitAccumShader.ModelUniform>(mesh, modelUniform, uniformBuffer, instanceBuffer, instanceInfos)
{
    // bind groups
    public BindGroupPtr? ModelBindGroup { get; set; }
    public i32 ModelBindGroupHash { get; set; }

    public required AjivaTexture AlbedoTexture;
    public AjivaTexture? EmissionTexture;

    public void Deconstruct(out AjivaBuffer<WboitAccumShader.Attributes> vertexBuffer, out AjivaBuffer<u32> indexBuffer,
        out AjivaBuffer<WboitAccumShader.Instance>? instanceBuffer, out BindGroupPtr? modelBindGroup,
        out List<WboitAccumShader.Instance> instanceInfos)
    {
        vertexBuffer = Mesh.VertexBuffer;
        indexBuffer = Mesh.IndexBuffer;
        instanceBuffer = InstanceBuffer;
        modelBindGroup = ModelBindGroup;
        instanceInfos = _instanceInfos;
    }

    public override void Dispose()
    {
        base.Dispose();
        
        ModelBindGroup?.Release();

        AlbedoTexture?.Dispose();
        EmissionTexture?.Dispose();
    }
}