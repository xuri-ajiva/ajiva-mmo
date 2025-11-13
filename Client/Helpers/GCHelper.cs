using System.Runtime.CompilerServices;

namespace Client.Helpers;

public static class GCHelper
{
    public static void RunGCFree(Action action, [CallerMemberName] string caller = null!)
    {
        action();
        return;
        if (!GC.TryStartNoGCRegion(10 * 1024 * 1024))
        {
            action();
            return;
        }

        try
        {
            action();
        }
        finally
        {
            try
            {
                GC.EndNoGCRegion();
            }
            catch (InvalidOperationException e)
            {
                Serilog.Log.Error(e, "Failed to end NoGCRegion for {Caller}", caller);
            }
        }
    }
}