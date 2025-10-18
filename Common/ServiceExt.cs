using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common;

public static class ServiceExt
{
    public static IServiceCollection AddService<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TService>(
        this IServiceCollection services)
        where TService : class
    {
        services.AddSingleton<TService>();
        if (typeof(TService).IsAssignableTo(typeof(IHostedService)))
        {
            services.AddSingleton<IHostedService>(provider => (IHostedService)provider.GetRequiredService<TService>());
        }

        return services;
    }

    public static IServiceCollection AddService<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TService, TSecond>(
        this IServiceCollection services)
        where TService : class, TSecond
        where TSecond : class
    {
        services.AddSingleton<TService>();
        if (typeof(TService).IsAssignableTo(typeof(IHostedService)))
        {
            services.AddSingleton<IHostedService>(provider => (IHostedService)provider.GetRequiredService<TService>());
        }


        if (typeof(TService).IsAssignableTo(typeof(TSecond)))
        {
            services.AddSingleton<TSecond>(provider => provider.GetRequiredService<TService>());
        }

        return services;
    }
}
