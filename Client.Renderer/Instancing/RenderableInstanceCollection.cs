using Client.Renderer.PbrRenderer;

namespace Client.Renderer.Instancing
{
    public class RenderableInstanceCollection : IDisposable
    {
        public List<PbrInstancedMeshBuffer> OpaqueMeshes { get; set; } = [];
        public List<OitInstancedMeshBuffer> TransparentMeshes { get; set; } = [];

        public void Dispose()
        {
            foreach (var mesh in OpaqueMeshes) mesh.Dispose();
            OpaqueMeshes.Clear();
            foreach (var mesh in TransparentMeshes) mesh.Dispose();
            TransparentMeshes.Clear();
        }
    }
}
