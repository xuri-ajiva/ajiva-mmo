using System.Collections.Concurrent;
using System.Numerics;

namespace client;

public class Ecs
{
    private ConcurrentDictionary<u32, Player> _players = new();

    public class Player
    {
        public Vector3 Position;
        public u32 InstanceId;
    }

    private u32 _nextInstanceId;
    public Vector3 LocalPos { get; set; }

    public void UpdatePlayer(u32 id, Vector3 position)
    {
        _players.GetOrAdd(id, static (_, ecs) => new Player { InstanceId = ecs._nextInstanceId++ }, this).Position = position;
    }
    
    public void RemovePlayer(u32 id)
    {
        _players.TryRemove(id, out _);
    }
    
    public IEnumerable<Player> GetPlayers() => _players.Values;
}
