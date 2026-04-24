using System.Runtime.CompilerServices;
using Common.Systems;

namespace Common.Analytics;

public struct PerfDeltaHelper
{
    private const i32 Length = 1000;
    private i32 _sampleCount;

    [InlineArray(Length)]
    private struct Data
    {
        private f32 _element0;
    }
    private Data _updateDurations;
    

    public void RecordPerfData(UpdateInfo info)
    {
        _updateDurations[(i32)(info.Ticks % Length)] = info.DeltaTime;
        _sampleCount = Math.Min(_sampleCount + 1, Length);
    }

    public void TryGetPerformanceData(out f32 p50, out f32 p90, out f32 p99)
    {
        // Use only the portion of the buffer that has data
        var count = Math.Min(_sampleCount, Length);

        if (count == 0)
        {
            p50 = p90 = p99 = 0;
            return;
        }

        // Copy to stack for sorting
        Span<f32> data = stackalloc f32[count];
        ((ReadOnlySpan<f32>)_updateDurations)[..count].CopyTo(data);

        // Calculate Average (still useful to see alngside Median)
        // Sort to get percentiles
        data.Sort();

        // Calculate indices
        p50 = data[count / 2]; // Median
        p90 = data[(i32)(count * 0.90f)]; // 90th percentile
        p99 = data[(i32)(count * 0.99f)]; // 99th percentile (the "stutter" indicator)
    }
}