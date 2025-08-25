using System.Diagnostics;

namespace Common;

/// <summary>
/// It is recommended to use a custom type to hold references for ActivitySource.
/// This avoids possible type collisions with other components in the DI container.
/// </summary>
public class Instrumentation : IDisposable
{
    internal readonly string ActivitySourceName;
    internal readonly string ActivitySourceVersion;

    public Instrumentation(string activitySourceName, string activitySourceVersion)
    {
        ActivitySourceVersion = activitySourceVersion;
        ActivitySourceName = activitySourceName;
        ActivitySource = new ActivitySource(ActivitySourceName, ActivitySourceVersion);
    }

    public ActivitySource ActivitySource { get; }

    public void Dispose()
    {
        this.ActivitySource.Dispose();
    }
}
