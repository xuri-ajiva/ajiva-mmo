using System.Reflection;
using Common.Systems;

namespace Client.LayerSystem;

public class LayerStack : IDisposable
{
    private readonly List<IAjivaLayer> _layers = [];
    public bool Attached { get; private set; }

    private readonly IServiceScope _scope;
    private readonly PeriodicUpdater.UpdateHandle _updateHandle;

    public IReadOnlyList<IAjivaLayer> Layers => _layers;

    public LayerStack(PeriodicUpdater periodicUpdater, IServiceProvider serviceProvider)
    {
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
        if (!Attached) return;
        foreach (var layer in _layers)
        {
            if (layer.Enabled)
            {
                layer.Update(info);
            }
        }
    }

    public void PushLayer<T>(T? layer = default) where T : IAjivaLayer
    {
        layer ??= _scope.ServiceProvider.GetRequiredService<T>();
        _layers.Add(layer);
        if (Attached)
            layer.Attach();
    }

    public void CreateLayer(TypeInfo layerType)
    {
        var layer = (IAjivaLayer)_scope.ServiceProvider.GetRequiredService(layerType);
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
}