using Client.Common;

namespace Client.UI;

/// <summary>
/// Extended interface for renderers that produce textures (Desktop only)
/// </summary>
public interface IUiTextureRenderer : IUiRenderer, IEventHandler<AjivaKeyboardEvent>, IEventHandler<AjivaMouseEvent>
{
    /// <summary>
    /// Get the current UI texture for uploading to GPU
    /// </summary>
    IUiTexture? GetTexture();

    string GetDebugInfo();
    void Interact();
}
