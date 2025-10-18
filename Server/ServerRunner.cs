using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Common;
using Common.Config;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Server;

namespace Server;

public class ServerRunner
{
    public ServerRunner(IHostApplicationBuilder builder, ServerConfiguration config)
    {
        var services = builder.Services;

        services.AddSingleton(config);

        services.AddService<ServerWorker>();
        services.AddService<ManagementService>();
        services.AddSingleton<ServerGameLogic>();
    }


    public async Task RunAsync(IHost app, CancellationToken token = default)
    {
        await app.RunAsync(token: token);
    }
}
