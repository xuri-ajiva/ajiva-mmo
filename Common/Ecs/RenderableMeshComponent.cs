using Common.Ecs;

namespace Common.Networking
{
    public struct RenderableMeshComponent : IComponentData
    {
        public u32 MeshId { get; set; }
        public u32 EntityId { get; set; }
    }
}
