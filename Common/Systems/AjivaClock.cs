using System.Diagnostics;

namespace Common.Systems;

public struct AjivaClock
{
    private u64 _start;
    private u64 _last;

    public void Start()
    {
        _start = (u64)Stopwatch.GetTimestamp();
        _last = _start;
    }

    private void Reset()
    {
        _start = 0;
        _last = 0;
        Ticks = 0;
        DeltaTime = TimeSpan.Zero;
    }

    public void Update()
    {
        var now = (u64)Stopwatch.GetTimestamp();
        DeltaTime = TimeSpan.FromTicks((i64)(now - _last));
        Total = TimeSpan.FromTicks((i64)(now - _start));
        Ticks++;
        _last = now;
    }

    public TimeSpan DeltaTime { get; private set; }
    public TimeSpan Total { get; private set; }
    public u64 Ticks { get; private set; }
}
