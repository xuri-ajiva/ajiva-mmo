using Silk.NET.WebGPU.Safe;

namespace Client.UI;

/// <summary>
/// Platform-agnostic UI rendering interface.
/// Desktop: RmlUi renders to texture
/// Web: DOM manipulation via JavaScript
/// </summary>
public interface IUiRenderer : IDisposable
{
    /// <summary>
    /// Initialize the UI renderer
    /// </summary>
    void Initialize();

    /// <summary>
    /// Load HTML content
    /// </summary>
    void LoadHTML(string rml);

    /// <summary>
    /// Load a URL
    /// </summary>
    void LoadURL(string url);

    /// <summary>
    /// Execute JavaScript code
    /// </summary>
    Task<string?> ExecuteJavaScript(string script);

    /// <summary>
    /// Update the UI renderer (call every frame)
    /// </summary>
    void Update(float deltaTime, QueuePtr targetQueuePtr);

    /// <summary>
    /// Resize the UI surface
    /// </summary>
    void Resize(uint width, uint height);

    /// <summary>
    /// Check if the UI is ready
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Get the UI width
    /// </summary>
    uint Width { get; }

    /// <summary>
    /// Get the UI height
    /// </summary>
    uint Height { get; }
}
