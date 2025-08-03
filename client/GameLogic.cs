using Common.Ecs;
using Common.Networking;
using Riptide;
using Riptide.Transports;

namespace client;

public class GameLogic
{
    private readonly World _world = new();
    private readonly Riptide.Client _client;

    public GameLogic(Riptide.Client client)
    {
        _client = client;
    }

    public World GetWorld() => _world;
}
