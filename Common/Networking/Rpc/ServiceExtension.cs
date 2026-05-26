using Microsoft.Extensions.DependencyInjection;

namespace Common.Networking.Rpc;

public static class ServiceExtension
{
    extension(IServiceCollection servies)
    {
        public void AddAsRpcSystem<TSystem, TImple>() where TSystem : class, IRpcSystem where TImple : class, TSystem
        {
            servies.AddSingleton<TImple>();
            servies.AddSingleton<TSystem>(provider => provider.GetRequiredService<TImple>());
            servies.AddSingleton<IRpcSystem>(provider => provider.GetRequiredService<TImple>());
        }
    }
}