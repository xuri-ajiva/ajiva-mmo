namespace Client.Wgsl;

using Silk.NET.WebGPU.Safe;

public interface IWgslShader
{
    public string ShaderSource { get; }
    public string VertexEntryPoint => "null";
    public string FragmentEntryPoint => "null";
    public string ComputeEntryPoint => "null";
}
