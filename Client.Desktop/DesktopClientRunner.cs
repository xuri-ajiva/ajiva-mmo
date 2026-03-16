using System.Diagnostics.CodeAnalysis;
using Client.Common;
using Common.Config;
using Client.Desktop.Platform;
using Client.Renderer;
using Client.Renderer.Renderers;
using Client.UI;
using Client.UI.Desktop;
using Common;

namespace Client.Desktop;

public class
    DesktopClientRunner<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        TApp> : ClientRunner<TApp>
    where TApp : class, IApplication
{
    public DesktopClientRunner(IHostApplicationBuilder builder, ClientConfiguration? clientConfiguration) : base(
        builder, clientConfiguration)
    {
        var services = builder.Services;
        services.AddSingleton<PlatformSystem>();
        services.AddService<AjivaImGuiLayer>();
        services.AddService<AjivaWindow>();
        services.AddService<ImGuiPlatform>();
        services.AddService<UiRenderLayer>();
        services.AddService<SurfaceManager, ISurfaceManager>();
        services.AddService<ExampleGameHudLayer>();
        services.AddService<EventSystem>();
        services.AddService<ViewPortRenderer>();
        services.AddService<RmlUiRenderer, IUiTextureRenderer>();
    }

    protected override async Task RunAsync(IHost app, IServiceScope root)
    {
        var services = root.ServiceProvider;

        var platform = services.GetRequiredService<PlatformSystem>();
        platform.Init();
        await base.RunAsync(app, root);
        platform.Shutdown();
    }
}