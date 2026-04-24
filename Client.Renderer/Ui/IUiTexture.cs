using Silk.NET.WebGPU.Safe;

namespace Client.UI;

/// <summary>
/// Represents a UI texture that can be uploaded to the GPU
/// Desktop: RmlUi pixel buffer
/// Web: Not used (direct DOM rendering)
/// </summary>
public interface IUiTexture
{
    /// <summary>
    /// Get the raw pixel data (BGRA8 format)
    /// </summary>
    ReadOnlySpan<byte> GetPixelData();
    
    TextureViewPtr GetTextureView();

    /// <summary>
    /// Width of the texture
    /// </summary>
    uint Width { get; }

    /// <summary>
    /// Height of the texture
    /// </summary>
    uint Height { get; }

    /// <summary>
    /// Row stride in bytes
    /// </summary>
    uint RowBytes { get; }

    /// <summary>
    /// Check if the texture has been updated this frame
    /// </summary>
    bool IsDirty { get; }

    /// <summary>
    /// Mark the texture as clean
    /// </summary>
    void MarkClean();
}
