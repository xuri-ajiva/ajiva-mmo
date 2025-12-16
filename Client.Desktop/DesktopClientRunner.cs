using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Client.LayerSystem;
using Common.Config;
using Client.Common.Accessors;
using Client.Desktop.Platform;
using Common;


namespace Client.Desktop;

public class DesktopClientRunner<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp> : ClientRunner<TApp> where TApp : class, IApplication
{
    public DesktopClientRunner(IHostApplicationBuilder builder, ClientConfiguration? clientConfiguration) : base(
        builder, clientConfiguration)
    {
        var services = builder.Services;
        services.AddSingleton<PlatformSystem>();
        services.AddService<AjivaImGuiLayer>();
        services.AddService<AjivaWindow, IInputAccessor>();
    }

    protected override async Task RunAsync(IHost app, IServiceScope root)
    {
        var services = root.ServiceProvider;
        
        var platform = services.GetRequiredService<PlatformSystem>();
        platform.Init();
        await base.RunAsync(app, root);
        platform.Shotdown();
    }
}