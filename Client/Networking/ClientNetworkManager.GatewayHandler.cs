using Common;
using Common.Networking;
using Common.Networking.AjivaQuic;
using NetworkPackets;
using Polly;

namespace Client.Networking;

public partial class ClientNetworkManager
{
    public class GatewayHandler(ClientNetworkManager handler, ILogger<ClientNetworkManager> logger, QClient client, Instrumentation instrumentation)
        : IEntryPointClientPacketHandler<ClientConnectionContext>
    {
        protected internal readonly QClient Client = client;

        public void ConnectGateway(string address)
        {
            Task.Run(async () =>
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
            });
        }

        public void DisconnectGateway()
        {
            if (Client.IsConnected)
            {
                Task.Run(Client.DisconnectAsync);
            }
        }

        
        public void Handle(Management.ServerList message, ClientConnectionContext userData)
        {
            handler.GameServerEndpoints = message.IpEndpoints;
            //if (handler.GameServerEndpoints is [var first, ..])
            //handler.ConnectGame(first);
            logger.LogInformation("Received server list with {ServerCount} servers",
                handler.GameServerEndpoints.Length);
        }

        public void Handle(Packets.HelloPacket message, ClientConnectionContext userData) =>
            handler.Handle(message, userData);

        public void Handle(Packets.DisconnectPacket message, ClientConnectionContext userData) =>
            handler.Handle(message, userData);

        public void Handle(Packets.PingPacket message, ClientConnectionContext userData) =>
            handler.Handle(message, userData);
    }
}