using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace client.VirtualFramebuffers
{
    internal class RenderTexture
    {
        private readonly VirtualFramebuffer _framebuffer;

        public u32 Width => _framebuffer.Width;
        public u32 Height => _framebuffer.Height;

        public TextureFormat ColorFormat => _framebuffer.GetFormat(0);

        private RenderTexture(VirtualFramebuffer framebuffer)
        {
            _framebuffer = framebuffer;
        }

        public static RenderTexture Create(DevicePtr device,
            TextureFormat colorFormat, u32 width = 0, u32 height = 0, string? label = null)
        {
            var framebuffer = new VirtualFramebuffer(device, [
                ("Color", colorFormat)
            ], null, width, height, label: label);

            var renderTexture = new RenderTexture(framebuffer);

            renderTexture.EnsureSize(width, height);

            return renderTexture;
        }

        public bool TryGetTargets(out VirtualFramebuffer.ColorTargetView color)
        {
            var result = true;

            result &= _framebuffer.TryGetColorTarget(0, out color);

            return result;
        }

        public void EnsureSize(u32 width, u32 height)
        {
            _framebuffer.EnsureSize(width, height);
        }
    }
}
