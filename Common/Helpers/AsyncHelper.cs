using Microsoft.Extensions.Logging;

namespace Common.Helpers;

public class AsyncHelper
{
    public static Task Run(Func<Task?> function, ILogger logger, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            try
            {
                var task = function();
                if (task is not null)
                {
                    await task;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception in async task");
            }
        }, cancellationToken);
    }
}