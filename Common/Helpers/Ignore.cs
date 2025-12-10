using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Client.LayerSystem;

namespace Common.Helpers;

public static class Ignore
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RequiresImplementation() =>
        throw new NotImplementedException("This code path requires implementation.");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //[Obsolete("Replace with Ignore.ForEver() or Implement.")]
    public static void ForNow()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ForEver()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //[Obsolete("Replace with Ignore.ForEverAsync() or Implement.")]
    public static ValueTask ForNowAsync() => ValueTask.CompletedTask;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask ForEverAsync() => ValueTask.CompletedTask;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // ReSharper disable once UnusedParameter.Global
    public static void Value<T>(T info) where T : allows ref struct => ForEver();
}