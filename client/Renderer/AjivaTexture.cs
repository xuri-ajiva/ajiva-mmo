using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace client.Renderer;

public record struct AjivaTexture(TextureFormat Format, TexturePtr? Tex, TextureViewPtr? View, TextureViewPtr? DepthOnlyView)
{
    private readonly string? label;

    
}
