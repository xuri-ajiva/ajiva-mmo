using Common.Ecs;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;

namespace Server;

public class ConnectionContext(IQConnection connection)
{
    public HashSet<ChunkCoord> LoadedChunks { get; } = [];
    public EntityId PrimaryEntity { get; set; } = EntityId.Invalid;

    /// <summary>
    /// The Position for the client, dictates which chunks are loaded around this position.
    /// </summary>
    public Position Position;

    public IQConnection Connection { get; } = connection;

    public void Kick(string reason)
    {
        Task.Run(() =>
        {
            try
            {
                using var msg = new Packets.Disconnect("Kicked: " + reason).ToMessage();
                Connection.Close(msg);
            }
            catch
            {
                // ignored
            }
        });
    }
}