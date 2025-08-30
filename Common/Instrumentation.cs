using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace Common;

/// <summary>
/// It is recommended to use a custom type to hold references for ActivitySource.
/// This avoids possible type collisions with other components in the DI container.
/// </summary>
public class Instrumentation : IDisposable
{
    internal readonly string SourceName;
    internal readonly string SourceVersion;
    [field: ThreadStatic] public static Instrumentation? Instance { get; set; }

    public Instrumentation(string sourceName, string sourceVersion)
    {
        SourceVersion = sourceVersion;
        SourceName = sourceName;
        ActivitySource = new ActivitySource(SourceName, SourceVersion);
        Meter = new Meter(SourceName, SourceVersion);
        //Instance = this;
    }

    public ActivitySource ActivitySource { get; }
    public Meter Meter { get; }

    [DebuggerStepThrough, DebuggerHidden, DebuggerNonUserCode]
    public Activity? StartActivity(ActivityKind? kind = null, IEnumerable<KeyValuePair<string, object?>>? tags = null,
        DateTimeOffset startTime = default, [CallerMemberName] string name = "")
        => ActivitySource.StartActivity(name, kind ?? Activity.Current?.Kind ?? ActivityKind.Internal,
            parentContext: Activity.Current?.Context ?? default, tags: tags,
            startTime: startTime);

    [DebuggerStepThrough, DebuggerHidden, DebuggerNonUserCode]
    public Activity? StartNamedActivity(string name, ActivityKind? kind = null,
        IEnumerable<KeyValuePair<string, object?>>? tags = null, DateTimeOffset startTime = default)
        => ActivitySource.StartActivity(name, kind ?? Activity.Current?.Kind ?? ActivityKind.Internal,
            parentContext: Activity.Current?.Context ?? default, tags: tags,
            startTime: startTime);

    [DebuggerStepThrough, DebuggerHidden, DebuggerNonUserCode]
    public static Activity? Trace(ActivityKind? kind = null,
        IEnumerable<KeyValuePair<string, object?>>? tags = null,
        DateTimeOffset startTime = default, [CallerMemberName] string name = "")
        => Instance?.StartActivity(name: name, kind: kind, tags: tags, startTime: startTime);

    [DebuggerStepThrough, DebuggerHidden, DebuggerNonUserCode]
    public static Activity? NamedTrace(string name, ActivityKind? kind = null,
        IEnumerable<KeyValuePair<string, object?>>? tags = null, DateTimeOffset startTime = default)
        => Instance?.StartActivity(name: name, kind: kind, tags: tags, startTime: startTime);

    [DebuggerStepThrough, DebuggerHidden, DebuggerNonUserCode]
    public static void RecordEvent(string name, IEnumerable<KeyValuePair<string, object?>>? tags = null,
        DateTimeOffset timestamp = default)
        => Activity.Current?.AddEvent(new ActivityEvent(name, timestamp: timestamp,
            tags: tags is not null ? new ActivityTagsCollection(tags) : null));

    [DebuggerStepThrough, DebuggerHidden, DebuggerNonUserCode]
    public static void RecordFunction([CallerMemberName] string name = "", [CallerFilePath] string file = "", [CallerLineNumber] i32 line = 0)
        => Activity.Current?.AddEvent(new ActivityEvent(name/*, tags: [new KeyValuePair<string, object?>("location", $"{file}:{line}")]*/));

    public void Dispose()
    {
        this.ActivitySource.Dispose();
    }
}