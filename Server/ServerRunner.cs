using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Common;
using Common.Config;
using Common.Networking;
using Server;

namespace Server;

public class ServerRunner
{
    public ServerRunner(IHostApplicationBuilder builder, ServerConfiguration config)
    {
        var services = builder.Services;


        var authenticationOptions = new SslServerAuthenticationOptions();
        authenticationOptions.ApplicationProtocols ??= [new SslApplicationProtocol("ice-rpc")];

        services.AddSingleton(config);

        // networking
        builder.AddQuickServerOptions(new IPEndPoint(IPAddress.Any, config.Port));
        services.AddSingleton<QServer>();

        services.AddHostedService<ServerWorker>();
        services.AddSingleton<ServerGameLogic>();
    }


    public async Task RunAsync(IHost app, CancellationToken token = default)
    {
        await app.RunAsync(token: token);
    }
}
