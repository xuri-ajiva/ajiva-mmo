using Common.Config;
using Server;

namespace Server;

public class ServerRunner
{
    public ServerRunner(IHostApplicationBuilder builder, ServerConfiguration config)
    {
        var services = builder.Services;

        services.AddSingleton(config);

        services.AddHostedService<ServerWorker>();
        services.AddSingleton<Riptide.Server>();
        services.AddSingleton<ServerGameLogic>();
    }

    public async Task RunAsync(IHost app, CancellationToken token = default)
    {
        await app.RunAsync(token: token);
    }
}
