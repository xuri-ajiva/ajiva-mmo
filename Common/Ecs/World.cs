using System.Collections.Concurrent;
using Microsoft.Win32.SafeHandles;

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
            if (!store.Has(entity))
            {
                store.Add(entity, component);
            }
            else
            {
                store.Get(entity) = component;
            }
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

        public Entity GetEntity(u32 entityId)
        {
            return new Entity(entityId);
        }

        public void Clear()
        {
            foreach (var store in _componentStores.Values)
            {
                store.Clear();
            }

            _entityManager.Clear();
        }

        public bool HasComponent<T>(Entity entity) where T : struct, IComponentData
        {
            if (_componentStores.TryGetValue(typeof(T), out var store))
            {
                return store.Has(entity);
            }

            return false;
        }
    }
}
