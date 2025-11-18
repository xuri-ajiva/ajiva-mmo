using Common;
using Common.Config;
using Server.Core;
using Server.Management;
using Server.Systems;

namespace Server;

public class ServerRunner
{
    public ServerRunner(IHostApplicationBuilder builder, ServerConfiguration config)
    {
        var services = builder.Services;

        services.AddSingleton(config);

        // Core services
        services.AddService<ManagementServer>();
        services.AddService<ManagementClient>();
        services.AddSingleton<NServerEcs>();

        services.AddScoped<GameServerInstanceConfig>();
        services.AddScoped<GameServerInstance>();

        // Server systems
        services.AddScoped<NetworkSystem>();
        services.AddScoped<ChunkSystem>();
        services.AddScoped<GameLogicSystem>();
        services.AddScoped<SimplePhysicsSystem>();

        // System manager
        services.AddScoped<ISystemManager, ServerSystemManager>();
    }


    public async Task RunAsync(IHost app, CancellationToken token = default)
    {
        await app.RunAsync(token: token);
    }
}