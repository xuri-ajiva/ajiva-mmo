using Common;
using Common.Networking;
using Common.Networking.AjivaQuic;
using NetworkPackets;
using Polly;

namespace Client.Networking;

public partial class ClientNetworkManager
{
    public class GatewayHandler(
        ClientNetworkManager handler,
        ILogger<ClientNetworkManager> logger,
        QClient client,
        Instrumentation instrumentation)
        : IGatewayClientPacketHandler<ClientConnectionContext>
    {
        protected internal readonly QClient Client = client;

        public async Task ConnectGateway(string address)
        {
            if (Client.IsConnected)
            {
                logger.LogWarning(
                    "Gateway client is already connected to a server at {RemoteEndPoint}, Disconnecting...",
                    Client.Connection?.RemoteEndPoint);
                await Client.DisconnectAsync();
            }

            await Policy.Handle<Exception>()
                .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(5))
                .ExecuteAsync(async ct => await Client.ConnectAsync(instrumentation, address, ct),
                    CancellationToken.None);
        }

        public async Task DisconnectGateway()
        {
            if (Client.IsConnected)
            {
                await Client.DisconnectAsync();
            }
        }

        public void Handle(GatewayPackets.ServerList message, ClientConnectionContext userData)
        {
            handler.GameServerEndpoints = message.IpEndpoints;
            //if (handler.GameServerEndpoints is [var first, ..])
            //handler.ConnectGame(first);
            logger.LogInformation("Received server list with {ServerCount} servers",
                handler.GameServerEndpoints.Length);
        }

        public void Handle(GatewayPackets.LoadLevelPacket message, ClientConnectionContext userData)
        {
            logger.LogInformation("Loading level: {LevelName}", message.LevelName);
            handler.GameLogic.LoadLevel(message.LevelName);
            handler.GameLogic.InitializeSpacialGrid(message.GridInfo);
        }

        public void Handle(Packets.Hello message, ClientConnectionContext userData) =>
            handler.Handle(message, userData);

        public void Handle(Packets.Disconnect message, ClientConnectionContext userData) =>
            handler.Handle(message, userData);

        public void Handle(Packets.Ping message, ClientConnectionContext userData) =>
            handler.Handle(message, userData);
    }
}