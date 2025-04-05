using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Client.Platform;

namespace Client.LayerSystem;

public enum UpdateCycle
{
    /// <summary>
    /// What are you doing here?
    /// </summary>
    None,

    /// <summary>
    /// Render is called every frame
    /// </summary>
    Render,

    /// <summary>
    /// Update is called every frame
    /// </summary>
    Update,

    /// <summary>
    /// Fixed Update, Independent from Render
    /// </summary>
    FixedUpdate,

    /// <summary>
    /// For all Tasks that run in the background at reduced update rate
    /// </summary>
    Reduced,

    /// <summary>
    /// Executes Once per second
    /// </summary>
    EverySecond,

    /// <summary>
    /// For all Tasks that block for longer time
    /// </summary>
    Blocking,
}

public class PeriodicUpdater(IConfiguration configuration) : IHostedLifecycleService
{
    public delegate void UpdateRunnerDelegate(UpdateInfo info);

    private class UpdateRunner(UpdateCycle cycle) : IDisposable
    {
        private readonly List<UpdateRunnerDelegate> _tasks = [];
        private readonly Lock _locker = new();
        private Thread? _thread;
        private bool _running;
        private bool _disposed;
        private AjivaClock _clock;

        public void AddTask(UpdateRunnerDelegate update)
        {
            ArgumentNullException.ThrowIfNull(update);
            using var l = _locker.EnterScope();
            _tasks.Add(update);
            if (!_disposed)
                _running = true;
        }

        public void StartDynamicUpdater(TimeSpan interval)
        {
            if (interval == TimeSpan.MaxValue)
                return;

            _thread = new Thread(() =>
            {
                while (!_running)
                {
                    Thread.Sleep(interval == TimeSpan.Zero ? TimeSpan.FromMilliseconds(5) : interval);
                    if (_disposed) return;
                }

                _clock.Start();
                while (_running && !_disposed)
                {
                    RunOnce();
                    Thread.Sleep(interval);
                }
            })
            {
                IsBackground = true,
                Name = $"AjivaPeriodicUpdater-{cycle}"
            };
            _thread.Start();
        }

        public void Dispose()
        {
            _disposed = true;
            _running = false;
            _thread?.Join();
        }

        public void RunOnce()
        {
            _clock.Update();
            var info = new UpdateInfo(_clock);
            using var l = _locker.EnterScope();
            foreach (var task in _tasks)
            {
                task(info);
            }
        }
    }

    private readonly Dictionary<UpdateCycle, UpdateRunner> _updateData = new();
    public bool Running { get; private set; }
    private readonly Lock _locker = new();
    private Thread? _updateCycle;
    private Thread? _renderCycle;
    private readonly AutoResetEvent _resetEvent = new(false);

    private Task InitRunners()
    {
        if (Running)
            return Task.CompletedTask;
        Running = true;

        var updaterSection = configuration.GetRequiredSection("PeriodicUpdater");

        // populate the dictionary with empty lists
        foreach (var cycle in Enum.GetValues<UpdateCycle>())
        {
            var r = _updateData[cycle] = new UpdateRunner(cycle);
            switch (cycle)
            {
                case UpdateCycle.Render:
                    _renderCycle = new Thread(() => RenderUpdate(r));
                    break;
                case UpdateCycle.Update:
                    _updateCycle = new Thread(() => UpdateUpdate(r));
                    break;
                default:
                {
                    if (!TimeSpan.TryParse(updaterSection[cycle.ToString()], out var interval))
                    {
                        interval = GetDefaultInterval(cycle);
                    }

                    r.StartDynamicUpdater(interval);
                    break;
                }
            }
        }


        _updateCycle?.Start();
        _renderCycle?.Start();

        return Task.CompletedTask;
    }

    private void UpdateUpdate(UpdateRunner updateRunner)
    {
        while (Running)
        {
            _resetEvent.WaitOne();
            updateRunner.RunOnce();
        }
    }

    private void RenderUpdate(UpdateRunner updateRunner)
    {
        while (Running)
        {
            _resetEvent.Set();
            updateRunner.RunOnce();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TimeSpan GetDefaultInterval(UpdateCycle cycle)
    {
        return cycle switch
        {
            UpdateCycle.None => TimeSpan.MaxValue,
            UpdateCycle.Render => TimeSpan.FromMilliseconds(1),
            UpdateCycle.Update => TimeSpan.FromMilliseconds(1),
            UpdateCycle.FixedUpdate => TimeSpan.FromMilliseconds(30),
            UpdateCycle.Reduced => TimeSpan.FromMilliseconds(100),
            UpdateCycle.EverySecond => TimeSpan.FromSeconds(1),
            UpdateCycle.Blocking => TimeSpan.FromMilliseconds(1),
            _ => TimeSpan.FromMilliseconds(10),
        };
    }


    public void RegisterUpdate(UpdateCycle cycle, UpdateRunnerDelegate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        using var l = _locker.EnterScope();

        if (!_updateData.TryGetValue(cycle, out var r))
        {
            r = new UpdateRunner(cycle);
            _updateData[cycle] = r;
        }

        l.Dispose();
        r.AddTask(update);
    }

    private Task StopRunners()
    {
        if (!Running)
            return Task.CompletedTask;
        Running = false;
        _resetEvent.Set();

        foreach (var runner in _updateData.Values)
        {
            runner.Dispose();
        }

        _resetEvent.Set();
        _updateCycle?.Join();
        _renderCycle?.Join();

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartingAsync(CancellationToken cancellationToken) => InitRunners();
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => StopRunners();
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
