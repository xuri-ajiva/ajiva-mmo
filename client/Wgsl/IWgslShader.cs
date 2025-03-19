namespace Client.Wgsl;

using Silk.NET.WebGPU.Safe;

public interface IWgslShader
{
    public string ShaderSource { get; }
    public string VertexEntryPoint { get; }
    public string FragmentEntryPoint { get; }
}
