using Common.Ecs;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.SpacialPartitioning;

namespace ClusterManager;

public class ConnectionContext(QConnection connection)
{
    public HashSet<ChunkCoord> LoadedChunks { get; } = [];
    public EntityId PrimaryEntity { get; set; } = EntityId.Invalid;
    public QConnection Connection { get; } = connection;

    public void Kick(string reason)
    {
        Task.Run(() =>
        {
            try
            {
                using var msg = new Packets.DisconnectPacket("Kicked: " + reason).ToMessage();
                Connection.Close(msg);
            }
            catch
            {
                // ignored
            }
        });
    }
}