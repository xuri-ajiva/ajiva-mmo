using System;
using System.Collections.Generic;

namespace Common.Ecs
{
    public class ComponentStore<T> : IComponentStore where T : struct, IComponentData
    {
        private T[] _dense = new T[1024];
        private int[] _sparse = new int[1024];
        private int[] _map = new int[1024];
        private int _count;

        public u64 Version { get; private set; }

        public void Add(Entity entity, T component)
        {
            if (entity.Id >= _sparse.Length)
            {
                var newSize = entity.Id * 2;
                Array.Resize(ref _sparse, newSize);
            }

            if (_count >= _dense.Length)
            {
                var newSize = _count * 2;
                Array.Resize(ref _dense, newSize);
                Array.Resize(ref _map, newSize);
            }

            _sparse[entity.Id] = _count;
            _map[_count] = entity.Id;
            _dense[_count] = component;
            _count++;
            Version++;
        }

        public ref T Get(Entity entity)
        {
            return ref _dense[_sparse[entity.Id]];
        }

        public bool Has(Entity entity)
        {
            return entity.Id < _sparse.Length && _sparse[entity.Id] < _count && _map[_sparse[entity.Id]] == entity.Id;
        }

        public void Remove(Entity entity)
        {
            if (!Has(entity)) return;

            var denseIndex = _sparse[entity.Id];
            var lastEntityId = _map[_count - 1];

            _dense[denseIndex] = _dense[_count - 1];
            _map[denseIndex] = lastEntityId;
            _sparse[lastEntityId] = denseIndex;

            _count--;
            Version++;
        }

        public ReadOnlySpan<T> GetAll() => new ReadOnlySpan<T>(_dense, 0, _count);

        public int GetEntityIdFromDenseIndex(int denseIndex)
        {
            return _map[denseIndex];
        }
    }
}
