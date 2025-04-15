using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using static Client.Renderer.VirtualFramebuffers.VirtualFramebuffer;

namespace Client.Renderer.VirtualFramebuffers;

public class GBuffer : IDisposable
{
    private readonly VirtualFramebuffer _framebuffer;

    public u32 Width => _framebuffer.Width;
    public u32 Height => _framebuffer.Height;

    public TextureFormat BaseColorAlphaFormat => _framebuffer.GetFormat(0);
    public TextureFormat WorldNormalFormat => _framebuffer.GetFormat(1);
    public TextureFormat MetallicRoughnessAoFormat => _framebuffer.GetFormat(2);
    public TextureFormat EmissiveFormat => _framebuffer.GetFormat(3);

    public TextureFormat ObjectAndInstanceIdFormat => _framebuffer.GetFormat(4);
    public TextureFormat AccumChannelFormat => _framebuffer.GetFormat(5);
    public TextureFormat RevealChannelFormat => _framebuffer.GetFormat(6);
    public TextureFormat DepthStencilFormat => _framebuffer.DepthStencilFormat!.Value;

    private GBuffer(VirtualFramebuffer framebuffer)
    {
        _framebuffer = framebuffer;
    }

    public static GBuffer Create(DevicePtr device, TextureFormat baseColorAlphaFormat, TextureFormat worldNormalFormat,
        TextureFormat metallicRoughnessAoFormat, TextureFormat emissiveFormat, TextureFormat objectAndInstanceIdFormat,
        TextureFormat accumFormat, TextureFormat revealFormat, TextureFormat depthStencilFormat,
        u32 width = 0, u32 height = 0,
        string? label = "GBuffer") // Default label
    {
        // Define the attachments for the VirtualFramebuffer according to the new layout
        var attachments = new (string Name, TextureFormat Format)[]
        {
            ("PBR_BaseColorAlpha", baseColorAlphaFormat), // Index 0
            ("PBR_WorldNormal", worldNormalFormat), // Index 1
            ("PBR_MetallicRoughnessAO", metallicRoughnessAoFormat), // Index 2
            ("PBR_Emissive", emissiveFormat), // Index 3
            ("Shared_ObjectAndInstanceId", objectAndInstanceIdFormat), // Index 4
            ("WBOIT_Accum", accumFormat), // Index 5
            ("WBOIT_Reveal", revealFormat), // Index 6
        };

        var framebuffer = new VirtualFramebuffer(device, attachments, depthStencilFormat, width, height, label);

        var gBuffer = new GBuffer(framebuffer);
        gBuffer.EnsureSize(width, height); // Initial size ensure
        return gBuffer;
    }

    /// <summary>
    /// Gets the Texture Views needed for the PBR G-Buffer PASS (writing).
    /// Also includes ObjectAndInstanceId buffer.
    /// </summary>
    public bool TryGetPbrGBufferPassViews(
        out ColorTargetView baseColorView,
        out ColorTargetView normalView,
        out ColorTargetView mraoView,
        out ColorTargetView emissiveView,
        out ColorTargetView objectIdView,
        out DepthStencilTargetView depthStencilView)
    {
        var result = true;
        result &= _framebuffer.TryGetColorTarget(0, out baseColorView);
        result &= _framebuffer.TryGetColorTarget(1, out normalView);
        result &= _framebuffer.TryGetColorTarget(2, out mraoView);
        result &= _framebuffer.TryGetColorTarget(3, out emissiveView);
        result &= _framebuffer.TryGetColorTarget(4, out objectIdView);
        result &= _framebuffer.TryGetDepthTarget(out var depthTarget);
        depthStencilView = depthTarget.GetValueOrDefault();
        result &= depthTarget.HasValue;

        return result;
    }

    /// <summary>
    /// Gets the Texture Views needed as INPUT for the PBR Deferred Lighting PASS (reading).
    /// </summary>
    public bool TryGetPbrDeferredInputViews(
        out ColorTargetView baseColorView,
        out ColorTargetView normalView,
        out ColorTargetView mraoView,
        out ColorTargetView emissiveView, // Emissive needed for final composition
        out DepthStencilTargetView depthView) // Only need depth texture view
    {
        var result = true;
        result &= _framebuffer.TryGetColorTarget(0, out baseColorView);
        result &= _framebuffer.TryGetColorTarget(1, out normalView);
        result &= _framebuffer.TryGetColorTarget(2, out mraoView);
        result &= _framebuffer.TryGetColorTarget(3, out emissiveView); // Get emissive view
        result &= _framebuffer.TryGetDepthTarget(out var depthTarget); // Get depth view
        depthView = depthTarget.GetValueOrDefault();
        result &= depthTarget.HasValue;

        return result;
    }


    // Keep TryGetAlphaBlend for WBOIT pass as it uses preserved indices
    /// <summary>
    /// Gets the Texture Views needed for the WBOIT Alpha Blending PASS (writing).
    /// </summary>
    public bool TryGetAlphaBlendViews(
        out ColorTargetView objectAndInstanceIdView, // Read-only input / LoadOp.Load
        out ColorTargetView accumView,
        out ColorTargetView revealView,
        out DepthStencilTargetView depthStencilView) // Depth read-only
    {
        var result = true;
        result &= _framebuffer.TryGetColorTarget(4, out objectAndInstanceIdView); // Input/Load
        result &= _framebuffer.TryGetColorTarget(5, out accumView); // Output/Clear
        result &= _framebuffer.TryGetColorTarget(6, out revealView); // Output/Clear
        result &= _framebuffer.TryGetDepthTarget(out var depthTarget); // Input/Load
        depthStencilView = depthTarget.GetValueOrDefault();
        result &= depthTarget.HasValue;

        return result;
    }


    public bool TryGetCompositorOutputView(
        out ColorTargetView emissiveView,
        out ColorTargetView objectAndInstanceIdView,
        out ColorTargetView accumView,
        out ColorTargetView revealView,
        out DepthStencilTargetView depthStencilView) // Depth read-only
    {
        var result = true;
        result &= _framebuffer.TryGetColorTarget(3, out emissiveView); // Emissive
        result &= _framebuffer.TryGetColorTarget(4, out objectAndInstanceIdView); // Object and Instance ID
        result &= _framebuffer.TryGetColorTarget(5, out accumView); // Accumulation
        result &= _framebuffer.TryGetColorTarget(6, out revealView); // Reveal
        result &= _framebuffer.TryGetDepthTarget(out var depthTarget); // Depth read-only
        depthStencilView = depthTarget.GetValueOrDefault();
        result &= depthTarget.HasValue;
        
        return result;
    }

    public void EnsureSize(u32 width, u32 height)
    {
        _framebuffer.EnsureSize(width, height);
    }

    public void Release()
    {
        _framebuffer.Release(); // Release the underlying framebuffer
    }

    public void Dispose() // Implement IDisposable pattern
    {
        Release();
        GC.SuppressFinalize(this);
    }
}
