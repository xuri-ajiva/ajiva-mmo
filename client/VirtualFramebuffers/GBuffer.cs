using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using static Client.VirtualFramebuffers.VirtualFramebuffer;

namespace Client.VirtualFramebuffers
{
    public class GBuffer
    {
        private readonly VirtualFramebuffer _framebuffer;

        public u32 Width => _framebuffer.Width;
        public u32 Height => _framebuffer.Height;

        public TextureFormat AlbedoChannelFormat => _framebuffer.GetFormat(0);
        public TextureFormat NormalChannelFormat => _framebuffer.GetFormat(1);
        public TextureFormat LightChannelFormat => _framebuffer.GetFormat(2);
        public TextureFormat ObjectAndInstanceIdChannelFormat => _framebuffer.GetFormat(3);
        public TextureFormat DepthStencilFormat => _framebuffer.DepthStencilFormat!.Value;

        private GBuffer(VirtualFramebuffer framebuffer)
        {
            _framebuffer = framebuffer;
        }

        public static GBuffer Create(DevicePtr device,
            TextureFormat albedoFormat, TextureFormat normalFormat, TextureFormat lightFormat,
            TextureFormat depthStencilFormat, TextureFormat objectAndInstanceId, u32 width = 0, u32 height = 0,
            string? label = null)
        {
            var framebuffer = new VirtualFramebuffer(device, [
                ("Albedo", albedoFormat),
                ("Normal", normalFormat),
                ("Light", lightFormat),
                ("ObjectAndInstanceId", objectAndInstanceId)
            ], depthStencilFormat, width, height, label);

            var gbuffer = new GBuffer(framebuffer);

            gbuffer.EnsureSize(width, height);

            return gbuffer;
        }

        public bool TryGetTargets(out ColorTargetView albedo, out ColorTargetView normal, out ColorTargetView light,
            out ColorTargetView objectAndInstanceId, out DepthStencilTargetView depthStencil)
        {
            var result = true;

            result &= _framebuffer.TryGetColorTarget(0, out albedo);
            result &= _framebuffer.TryGetColorTarget(1, out normal);
            result &= _framebuffer.TryGetColorTarget(2, out light);
            result &= _framebuffer.TryGetColorTarget(3, out objectAndInstanceId);
            result &= _framebuffer.TryGetDepthTarget(out var depthStencilTarget);
            depthStencil = depthStencilTarget.GetValueOrDefault();

            return result;
        }

        public void EnsureSize(u32 width, u32 height)
        {
            _framebuffer.EnsureSize(width, height);
        }

        public void Release()
        {
            _framebuffer.Release();
        }
    }
}
