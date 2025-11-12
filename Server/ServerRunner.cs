using Common;
using Common.Config;

namespace Server;

public class ServerRunner
{
    public ServerRunner(IHostApplicationBuilder builder, ServerConfiguration config)
    {
        var services = builder.Services;

        services.AddSingleton(config);

        services.AddService<ManagementServer>();
        services.AddService<ManagementClient>();
        services.AddService<ServerGameNetHandler>();
    }


    public async Task RunAsync(IHost app, CancellationToken token = default)
    {
        await app.RunAsync(token: token);
    }
}