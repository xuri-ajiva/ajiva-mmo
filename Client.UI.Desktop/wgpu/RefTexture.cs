using GlmSharp;
using Silk.NET.WebGPU.Safe;

namespace Client.UI.Desktop.wgpu;

public class RefTexture(TextureViewPtr view, vec2 size) : IUiTexture
{
    public ReadOnlySpan<byte> GetPixelData()
    {
        throw new NotImplementedException();
    }

    public TextureViewPtr GetTextureView()
    {
        return view;
    }

    public u32 Width => (u32)size.x;
    public u32 Height => (u32)size.y;
    public u32 RowBytes { get; }
    public bool IsDirty { get; }

    public void MarkClean()
    {
    }
}