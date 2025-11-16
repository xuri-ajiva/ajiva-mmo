using System.Diagnostics;
using System.Runtime.CompilerServices;
using Client.LayerSystem;
using Common.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Systems;

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

public class PeriodicUpdater : IHostedLifecycleService
{
    public delegate void UpdateRunnerDelegate(UpdateInfo info);

    public record class UpdateHandle(UpdateCycle Cycle, UpdateRunnerDelegate Action, PeriodicUpdater Runner)
        : IDisposable
    {
        public void Dispose()
        {
            Runner.UnregisterUpdate(Cycle, Action);
        }
    }

    internal class UpdateRunner(UpdateCycle cycle) : IDisposable
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
            if (!_disposed) _running = true;
        }

        public void RemoveTask(UpdateRunnerDelegate action)
        {
            ArgumentNullException.ThrowIfNull(action);
            using var l = _locker.EnterScope();
            if (!_tasks.Remove(action)) return;
            if (_tasks.Count != 0) return;
            _running = false;
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
                task.Invoke(info);
            }
        }
    }

    private readonly Dictionary<UpdateCycle, UpdateRunner> _updateData = new();
    public bool Running { get; private set; }
    public bool TraceRenderer { get; set; }
    public bool TraceUpdate { get; set; }

    private readonly Lock _locker = new();
    private Thread? _updateCycle;
    private Thread? _renderCycle;
    private readonly AutoResetEvent _resetEvent = new(false);
    private readonly SemaphoreSlim _globalHandle = new(0, 9999);
    private readonly IConfiguration _configuration;
    private readonly ILogger<PeriodicUpdater> _logger;
    private readonly Instrumentation _instrumentation;

    public PeriodicUpdater(IConfiguration configuration,
        ILogger<PeriodicUpdater> logger,
        Instrumentation instrumentation)
    {
        _configuration = configuration;
        _logger = logger;
        _instrumentation = instrumentation;
        InitRunners();
    }

    private Task InitRunners()
    {
        if (Running)
            return Task.CompletedTask;
        Running = true;

        var updaterSection = _configuration.GetRequiredSection("PeriodicUpdater");

        // populate the dictionary with empty lists
        foreach (var cycle in Enum.GetValues<UpdateCycle>())
        {
            var r = _updateData[cycle] = new UpdateRunner(cycle);
            switch (cycle)
            {
                case UpdateCycle.Render:
                    _renderCycle = new Thread(() => RenderUpdate(r))
                        { Name = "Render Loop", Priority = ThreadPriority.Highest };
                    break;
                case UpdateCycle.Update:
                    _updateCycle = new Thread(() => UpdateUpdate(r))
                        { Name = "Update Loop", Priority = ThreadPriority.AboveNormal };
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
            ReleaseAllWaiting();
            if (TraceUpdate)
            {
                TraceUpdate = false;
                using var activityRenderer = _instrumentation.StartActivity(ActivityKind.Client);
                Instrumentation.Instance = _instrumentation;
                updateRunner.RunOnce();
                Instrumentation.Instance = null!;
            }
            else
            {
                updateRunner.RunOnce();
            }
        }
    }


    private void RenderUpdate(UpdateRunner updateRunner)
    {
        while (Running)
        {
            _resetEvent.Set();
            ReleaseAllWaiting();
            if (TraceRenderer)
            {
                TraceRenderer = false;
                using var activityRenderer = _instrumentation.StartActivity(ActivityKind.Client);
                Instrumentation.Instance = _instrumentation;
                updateRunner.RunOnce();
                Instrumentation.Instance = null!;
            }
            else
            {
                GCHelper.RunGCFree(updateRunner.RunOnce);
            }
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

    public UpdateHandle RegisterUpdate(UpdateCycle cycle, UpdateRunnerDelegate update)
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
        // todo restart updater if it was stopped
        // r.StartDynamicUpdater();
        return new UpdateHandle(cycle, update, this);
    }

    private void UnregisterUpdate(UpdateCycle cycle, UpdateRunnerDelegate action)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var l = _locker.EnterScope();
        if (!_updateData.TryGetValue(cycle, out var r)) return;
        r.RemoveTask(action);
    }

    private Task StopRunners()
    {
        if (!Running)
            return Task.CompletedTask;
        Running = false;
        _globalHandle.Release(999);
        _globalHandle.Dispose();
        _resetEvent.Set();

        foreach (var runner in _updateData.Values)
        {
            runner.Dispose();
        }

        _resetEvent.Set();
        DisposeFor(_updateCycle);
        DisposeFor(_renderCycle);
        return Task.CompletedTask;
    }

    private void DisposeFor(Thread? thread)
    {
        if (thread is null) return;
        thread.Join(100);
        thread.Interrupt();
        thread.Join(100);
        thread.Interrupt();
        if (thread.IsAlive)
        {
            _logger.LogCritical("PeriodicUpdater thread {ThreadName} did not stop gracefully, handle will be lost!",
                thread.Name);
        }
    }

    /// <summary>
    /// Sleeps until any next update cycle is triggered.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void SleepSignal()
    {
        if (!Running) return;
        try
        {
            _globalHandle.Wait();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private void ReleaseAllWaiting()
    {
        if (_globalHandle.CurrentCount == 0)
            _globalHandle.Release(10);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartingAsync(CancellationToken cancellationToken) => InitRunners();
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => StopRunners();
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}