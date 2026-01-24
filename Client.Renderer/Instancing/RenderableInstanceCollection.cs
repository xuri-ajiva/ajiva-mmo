using Client.Renderer.PbrRenderer;
using Client.Wgsl;

namespace Client.Renderer.Instancing
{
    public class RenderableInstanceCollection : IDisposable
    {
        public List<PbrInstancedMeshBuffer> OpaqueMeshes { get; set; } = [];
        public List<OitInstancedMeshBuffer> TransparentMeshes { get; set; } = [];

        public List<SimpleInstancedMeshBBuffer<GizmoShader.Attributes, GizmoShader.Instance>>
            GizmoMeshes { get; set; } = [];

        public void Dispose()
        {
            foreach (var mesh in OpaqueMeshes) mesh.Dispose();
            OpaqueMeshes.Clear();
            foreach (var mesh in TransparentMeshes) mesh.Dispose();
            TransparentMeshes.Clear();
            foreach (var mesh in GizmoMeshes) mesh.Dispose();
            GizmoMeshes.Clear();
        }
    }
}