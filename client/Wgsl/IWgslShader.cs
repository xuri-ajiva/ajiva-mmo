namespace Client.Wgsl;

using Silk.NET.WebGPU.Safe;

public interface IWgslShader
{
    public string ShaderSource { get; }

    /*
    public ReadOnlySpan<BindGroupLayoutPtr> CreateBindGroupLayouts(Silk.NET.WebGPU.Device device);

    public ReadOnlySpan<BindGroupPtr> CreateBindGroups(Silk.NET.WebGPU.Device device,
        ReadOnlySpan<TextureViewPtr> textureViews);

    public RenderPipelinePtr CreateRenderPipeline(Silk.NET.WebGPU.Device device,
        ReadOnlySpan<BindGroupLayoutPtr> bindGroupLayouts);
        */

    public string VertexEntryPoint { get; }
    public string FragmentEntryPoint { get; }
}
