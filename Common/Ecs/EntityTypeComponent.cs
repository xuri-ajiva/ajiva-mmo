using Common.Ecs;

namespace Common.Networking
{
    public struct EntityTypeComponent : IComponentData
    {
        public u32 TypeId { get; set; }
    }
}
