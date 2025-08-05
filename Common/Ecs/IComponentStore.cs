namespace Common.Ecs
{
    public interface IComponentStore
    {
        u64 Version { get; }
        void Remove(Entity entity);
        bool Has(Entity entity);
        void Clear();
    }
}
