using server;

namespace server;

public class ServerRunner
{
    public ServerRunner(HostApplicationBuilder builder, ServerConfiguration config)
    {
        var services = builder.Services;

        services.AddSingleton(config);

        services.AddHostedService<Worker>();
        services.AddSingleton<Riptide.Server>();
    }

    public async Task RunAsync(IHost app)
    {
        await app.RunAsync();
    }
}
