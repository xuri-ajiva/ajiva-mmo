using System.Diagnostics;
using System.Diagnostics.Metrics;
using Client.LayerSystem;
using Interception;

namespace Common.Systems;

/// <summary>
/// Produces periodic, high-resolution ticks with minimal drift.
/// This class is designed for server-side game loops and synchronization tasks.
/// It is not thread-safe for configuration changes after starting; configure before calling Start().
/// </summary>
[ExcludeFromInterception]
public sealed class HighResolutionTicker : IDisposable
{
    // The frequency of the Stopwatch is the number of ticks per second.
    private static readonly f64 StopwatchFrequency = Stopwatch.Frequency;

    /// <summary>
    /// Fired for each tick, providing the tick count and the exact timestamp it was scheduled for.
    /// </summary>
    public Action<UpdateInfo> OnTick = null!;

    // Core timing state
    private readonly i64 _baseTimestamp;
    private readonly i64 _tickIntervalStopwatchTicks;
    private u64 _currentTick;

    // Control state
    private Thread _tickLoopTask;
    private volatile bool _isRunning;
    private volatile f32 _speedMultiplier = 1.0f;

    // Dynamic scheduling state for targeting a specific tick/timestamp
    private u64 _targetTick;
    private i64 _targetTimestamp;
    private volatile bool _isTargeting;
    private i64 _lastTickScheduledTimestamp;
    private readonly Histogram<f32>? _meterDelta, _meterTickDuration;

    /// <summary>
    /// Initializes a new HighResolutionTicker.
    /// </summary>
    /// <param name="tickInterval">The desired interval between ticks (e.g., TimeSpan.FromSeconds(1.0 / 60.0)).</param>
    /// <param name="initialTick">The starting tick count.</param>
    /// <param name="baseTimestamp">The reference Stopwatch timestamp for tick 0. If null, uses the current time.</param>
    /// <param name="instrumentation">Optional instrumentation for tracing.</param>
    /// <param name="start">If true, starts the ticker immediately upon creation.</param>
    public HighResolutionTicker(TimeSpan tickInterval, u64 initialTick = 0, i64? baseTimestamp = null,
        Instrumentation? instrumentation = null, bool start = false)
    {
        _tickIntervalStopwatchTicks = (i64)(tickInterval.TotalSeconds * StopwatchFrequency);
        _currentTick = initialTick;
        _baseTimestamp = baseTimestamp ?? Stopwatch.GetTimestamp();
        _lastTickScheduledTimestamp = GetScheduledTimestamp(initialTick > 0 ? initialTick - 1 : 0);
        _tickLoopTask = new Thread(TickLoop)
            { IsBackground = true, Name = "HighResolutionTicker Thread", Priority = ThreadPriority.Highest };
        // meter delta time and time spend in OnTick
        _meterDelta = instrumentation?.Meter?.CreateHistogram<f32>("HighResolutionTicker.OnTick.DeltaTime", "ms",
            "Delta time of each tick");
        _meterTickDuration = instrumentation?.Meter?.CreateHistogram<f32>("HighResolutionTicker.OnTick.Duration",
            "ms", "Duration of OnTick handler");
        if (start)
        {
            Start();
        }
    }

    /// <summary>
    /// Starts the ticker loop in a background task.
    /// </summary>
    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        _tickLoopTask.Start();
    }

    /// <summary>
    /// Signals the ticker loop to stop and waits for the task to complete.
    /// </summary>
    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _tickLoopTask?.Join();
        _tickLoopTask = null!;
    }

    /// <summary>
    /// Sets a simulation speed multiplier. 1.0 is realtime, 10.0 is 10x speed.
    /// </summary>
    public void SetSpeed(f32 multiplier)
    {
        if (multiplier <= 0) throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be positive.");
        _isTargeting = false; // Setting speed cancels any active target scheduling.
        _speedMultiplier = multiplier;
    }

    /// <summary>
    /// Dynamically adjusts the tick rate to ensure a specific tick count is reached by a specific timestamp.
    /// </summary>
    /// <param name="targetTick">The tick count to reach.</param>
    /// <param name="targetTimestamp">The Stopwatch timestamp by which to reach the target tick.</param>
    public void ScheduleTarget(u64 targetTick, i64 targetTimestamp)
    {
        if (targetTick <= _currentTick)
        {
            _isTargeting = false;
            return;
        }

        Volatile.Write(ref _targetTick, targetTick);
        Volatile.Write(ref _targetTimestamp, targetTimestamp);
        _isTargeting = true;
    }

    /// <summary>
    /// Calculates the exact Stopwatch timestamp at which a future tick is scheduled to occur in realtime (Speed = 1.0).
    /// </summary>
    /// <param name="tick">The tick number to query.</param>
    /// <returns>The calculated Stopwatch timestamp for that tick.</returns>
    public i64 GetScheduledTimestamp(u64 tick)
    {
        return _baseTimestamp + (i64)(tick * (f64)_tickIntervalStopwatchTicks);
    }

    public (u64 NextTick, i64 NextTickTimestamp) GetNextTickInfo()
    {
        var nextTick = _currentTick + 1;
        var nextTickTimestamp = CalculateNextTickTimestamp();
        return (nextTick, nextTickTimestamp);
    }
    
    private void TickLoop()
    {
        while (_isRunning)
        {
            var currentTickScheduledTimestamp = CalculateNextTickTimestamp();

            // High-resolution spin-wait until the scheduled time is reached.
            while (Stopwatch.GetTimestamp() < currentTickScheduledTimestamp)
            {
                var remainingTime = Stopwatch.GetElapsedTime(Stopwatch.GetTimestamp(), currentTickScheduledTimestamp);
                if (remainingTime.TotalMilliseconds > 5)
                {
                    Thread.Sleep(5);
                }
                else
                {
                    // Yield the thread to prevent 100% CPU usage while waiting,
                    // but wake up frequently to check the time.
                    Thread.Yield();
                }
            }

            var actualElapsed =
                Stopwatch.GetElapsedTime(_lastTickScheduledTimestamp, currentTickScheduledTimestamp);
            var deltaTime = (f32)actualElapsed.TotalSeconds;

            // totalTime based on the CURRENT tick's scheduled time.
            var totalElapsed = Stopwatch.GetElapsedTime(_baseTimestamp, currentTickScheduledTimestamp);
            var totalTime = (f32)totalElapsed.TotalSeconds;

            _meterDelta?.Record((f32)actualElapsed.TotalMilliseconds);
            var tickStart = Stopwatch.GetTimestamp();
            OnTick?.Invoke(new UpdateInfo(_currentTick, deltaTime, totalTime));
            _meterTickDuration?.Record((f32)Stopwatch.GetElapsedTime(tickStart, Stopwatch.GetTimestamp())
                .TotalMilliseconds);

            _lastTickScheduledTimestamp = currentTickScheduledTimestamp;
            _currentTick++;
        }
    }

    private i64 CalculateNextTickTimestamp()
    {
        var nextTick = _currentTick + 1;

        if (!_isTargeting)
            return _baseTimestamp + (i64)(nextTick * (_tickIntervalStopwatchTicks / _speedMultiplier));

        var now = Stopwatch.GetTimestamp();
        if (nextTick >= Volatile.Read(ref _targetTick) || now >= Volatile.Read(ref _targetTimestamp))
        {
            // Target met or passed, revert to normal ticking.
            _isTargeting = false;
            return GetScheduledTimestamp(nextTick);
        }

        // Dynamically calculate the interval to hit the target.
        var remainingTime = Stopwatch.GetElapsedTime(Volatile.Read(ref _targetTimestamp), now);
        var remainingTicks = Volatile.Read(ref _targetTick) - nextTick;

        if (remainingTicks > 0 && remainingTime > TimeSpan.Zero)
        {
            var dynamicInterval = remainingTime.TotalSeconds / remainingTicks;
            var dynamicIntervalTicks = (i64)(dynamicInterval * StopwatchFrequency);
            return now + dynamicIntervalTicks;
        }

        // Standard calculation based on speed multiplier.
        return _baseTimestamp + (i64)(nextTick * (_tickIntervalStopwatchTicks / _speedMultiplier));
    }

    public void Dispose()
    {
        Stop();
    }
}