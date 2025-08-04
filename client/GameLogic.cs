using Common.Ecs;

namespace Client;

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
