using System.Collections.Concurrent;

namespace Common.Ecs
{
    public class World
    {
        private readonly ConcurrentDictionary<Type, IComponentStore> _componentStores = new();
        private readonly EntityManager _entityManager = new EntityManager();

        public Entity CreateEntity()
        {
            return _entityManager.Create();
        }

        public void DestroyEntity(Entity entity)
        {
            foreach (var store in _componentStores.Values)
            {
                store.Remove(entity);
            }
            _entityManager.Destroy(entity);
        }

        public void AddComponent<T>(Entity entity, T component) where T : struct, IComponentData
        {
            var store = GetStore<T>();
            store.Add(entity, component);
        }

        public ref T GetComponent<T>(Entity entity) where T : struct, IComponentData
        {
            var store = GetStore<T>();
            return ref store.Get(entity);
        }

        public ComponentStore<T> GetStore<T>() where T : struct, IComponentData
        {
            if (_componentStores.TryGetValue(typeof(T), out var store))
            {
                return (ComponentStore<T>)store;
            }

            var newStore = new ComponentStore<T>();
            _componentStores.TryAdd(typeof(T), newStore);
            return newStore;
        }
    }
}
