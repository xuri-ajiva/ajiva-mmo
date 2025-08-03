using System.Collections.Generic;

namespace Common.Ecs
{
    public class EntityManager
    {
        private readonly Queue<int> _freeIds = new Queue<int>();
        private int _nextId;

        public Entity Create()
        {
            return new Entity(_freeIds.Count > 0 ? _freeIds.Dequeue() : _nextId++);
        }

        public void Destroy(Entity entity)
        {
            _freeIds.Enqueue(entity.Id);
        }
    }
}
