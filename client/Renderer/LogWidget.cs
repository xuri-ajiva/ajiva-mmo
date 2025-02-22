using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace client.Renderer;

public class LogWidget(IFormatProvider formatProvider) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(formatProvider);
        Console.WriteLine(DateTimeOffset.Now.ToString() + " " + message);
    }
}

public static class MySinkExtensions
{
    public static LoggerConfiguration MySink(
        this LoggerSinkConfiguration loggerConfiguration,
        IFormatProvider formatProvider = null)
    {
        return loggerConfiguration.Sink(new LogWidget(formatProvider));
    }
}
