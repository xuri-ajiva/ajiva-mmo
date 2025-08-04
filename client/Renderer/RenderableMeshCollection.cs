using Client.Renderer.PbrRenderer;

namespace Client.Renderer
{
    public class RenderableMeshCollection : IDisposable
    {
        public List<PbrInstancedMeshBuffer> OpaqueMeshes { get; set; } = [];
        public List<OitInstancedMeshBuffer> TransparentMeshes { get; set; } = [];

        public void Clear()
        {
            var oldOpaque = OpaqueMeshes;
            var oldTransparent = TransparentMeshes;
            OpaqueMeshes = [];
            TransparentMeshes = [];
            
            foreach (var mesh in oldOpaque) mesh.Dispose();
            oldOpaque.Clear();
            foreach (var mesh in oldTransparent) mesh.Dispose();
            oldTransparent.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
