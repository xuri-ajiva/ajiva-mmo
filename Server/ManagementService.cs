using System.Net;
using System.Net.Quic;
using Common;
using Common.Config;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Polly;

namespace Server;

public class ManagementService : IHostedLifecycleService, QServerEventListener
{
    private readonly QClient _client;
    private readonly ILogger<ManagementService> _logger;
    private readonly Instrumentation _instrumentation;
    private readonly ServerConfiguration _configuration;
    private readonly QServer _server;

    public ManagementService(ILogger<ManagementService> logger, QFactory factory, Instrumentation instrumentation,
        ServerConfiguration configuration)
    {
        _client = factory.CreateClient();
        _server = factory.CreateServer(0);
        _logger = logger;
        _instrumentation = instrumentation;
        _configuration = configuration;
    }

    public async Task RegisterServer(IPEndPoint? serverLocalEndPoint, CancellationToken token)
    {
        if (serverLocalEndPoint is null)
            throw new ArgumentNullException(nameof(serverLocalEndPoint));

        while (!_client.IsConnected)
        {
            await Task.Delay(100, token);
        }

        _logger.LogInformation("Registering server with ClusterManager");
        new Management.RegisterServerPacket(Guid.NewGuid().ToString("D"), serverLocalEndPoint.Port,
            _server.LocalEndPoint!.Port).Send(
            _client.Connection!);
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        _server.Start(this, _instrumentation);
        await _server.WaitForReady();
        await Policy.Handle<Exception>()
            .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(5),
                (exception, timeSpan) => _logger.LogWarning(exception,
                    "Failed to connect to ClusterManager, retrying in {TimeSpan}", timeSpan))
            .ExecuteAsync(
                async ct => await _client.ConnectAsync(_instrumentation, _configuration.ClusterManagerAddress, ct),
                cancellationToken);
    }

    public void OnClientConnected(QuicListener server, QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void OnClientDisconnected(QConnection connection)
    {
        throw new NotImplementedException();
    }

    public void OnConnectionFailed(QuicListener server, IPEndPoint? connection, Exception exception)
    {
        throw new NotImplementedException();
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}