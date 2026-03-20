using System.Reflection;
using System.Runtime.InteropServices;
using Common;
using Common.Systems;

namespace Client.Common;

public class LayerStack : IDisposable, IHostedLifecycleService
{
    private readonly PeriodicUpdater _periodicUpdater;
    internal readonly List<IAjivaLayer> _layers = [];
    public bool Attached { get; private set; }

    private readonly IServiceScope _scope;
    private readonly PeriodicUpdater.UpdateHandle _updateHandle;

    public ReadOnlySpan<IAjivaLayer> LayersSpan => CollectionsMarshal.AsSpan(_layers);
    public IReadOnlyList<IAjivaLayer> Layers => _layers;

    public LayerStack(PeriodicUpdater periodicUpdater, IServiceProvider serviceProvider)
    {
        _periodicUpdater = periodicUpdater;
        _scope = serviceProvider.CreateScope();
        _updateHandle = periodicUpdater.RegisterUpdate(UpdateCycle.Update, Update);
    }

    public void Attach()
    {
        if (Attached) return;
        foreach (var layer in _layers)
        {
            layer.Attach();
        }

        Attached = true;
    }

    public void Detach()
    {
        Attached = false;
        foreach (var ajivaLayer in _layers)
        {
            ajivaLayer.Detach();
        }

        _layers.Clear();
    }

    public void Update(UpdateInfo info)
    {
        using var trace = Instrumentation.Trace();

        if (!Attached) return;
        foreach (var layer in _layers)
        {
            using (Instrumentation.NamedTrace(layer.GetType().FullName ?? "UnknownLayer"))
                if (layer.Enabled)
                {
                    layer.Update(info);
                }
        }
    }

    public bool DispatchEvent<T>(T data) where T : struct
    {
        using var trace = Instrumentation.NamedTrace($"EventSystem.Dispatch<{typeof(T).Name}>");
        var e = new AjivaEvent<T>(ref data, false);
        foreach (var layer in Layers.OfType<IEventHandler<T>>())
        {
            layer.OnEvent(ref e);
            if (e.Handled)
            {
                return true;
            }
        }

        return false;
    }

    public void PushLayer<T>(T? layer = default) where T : IAjivaLayer
    {
        IAjivaLayer service;
        if (layer is null)
        {
            if (_scope.ServiceProvider.GetService<T>() is IAjivaLayer layerFromService)
            {
                service = layerFromService;
            }
            else
            {
                return;
            }
        }
        else
        {
            service = layer;
        }

        _layers.Add(service);
        if (Attached)
            service.Attach();
    }

    public void CreateLayer(Type layerType)
    {
        if (_scope.ServiceProvider.GetService(layerType) is IAjivaLayer layer)
            PushLayer(layer);
    }

    public void PopLayer<T>() where T : IAjivaLayer
    {
        var layer = _layers.OfType<T>().FirstOrDefault();
        if (layer is null) return;

        layer.Detach();
        _layers.Remove(layer);
    }

    public void Remove(IAjivaLayer layerToRemove)
    {
        layerToRemove.Detach();
        _layers.Remove(layerToRemove);
    }

    public void Dispose()
    {
        _scope.Dispose();
        _updateHandle.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        _periodicUpdater.DispatchNowOn(UpdateCycle.Render, Attach);
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        _periodicUpdater.DispatchNowOn(UpdateCycle.Render, Detach);
        return Task.CompletedTask;
    }
}