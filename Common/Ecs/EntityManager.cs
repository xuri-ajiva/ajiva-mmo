using System.Collections.Generic;

namespace Common.Ecs
{
    public class EntityManager
    {
        private readonly Queue<u32> _freeIds = new();
        private u32 _nextId;

        public Entity Create()
        {
            return new Entity(_freeIds.Count > 0u ? _freeIds.Dequeue() : _nextId++);
        }

        public void Destroy(Entity entity)
        {
            _freeIds.Enqueue(entity.Id);
        }

        public void Clear()
        {
            _freeIds.Clear();
            _nextId = 0;
        }
    }
}
