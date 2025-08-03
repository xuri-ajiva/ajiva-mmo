using System.IO.Pipes;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Composer.Logging;

public class NamedPipeSink : ILogEventSink, IDisposable
{
    private readonly NamedPipeClientStream _pipeClient;
    private readonly StreamWriter _writer;
    private readonly ITextFormatter _formatter;
    private readonly string _processName;

    public NamedPipeSink(string processName, string pipeName = "AjivaCompositorLogs")
    {
        _processName = processName;
        _pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
        _formatter =
            new MessageTemplateTextFormatter(
                "[{ProcessName}] [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

        try
        {
            _pipeClient.Connect(10000); // 1 second timeout
            _writer = new StreamWriter(_pipeClient, Encoding.UTF8);
        }
        catch
        {
            // If pipe connection fails, we'll just not send logs to pipe
            _writer = null;
        }
    }

    public void Emit(LogEvent logEvent)
    {
        if (_writer == null) return;

        try
        {
            // Add process name to log event properties
            var enrichedEvent = new LogEvent(
                logEvent.Timestamp,
                logEvent.Level,
                logEvent.Exception,
                logEvent.MessageTemplate,
                logEvent.Properties.Select(kv => new LogEventProperty(kv.Key, kv.Value))
                    .Append(new LogEventProperty("ProcessName", new ScalarValue(_processName))
                    ));

            var stringWriter = new StringWriter();
            _formatter.Format(enrichedEvent, stringWriter);
            _writer.WriteLine(stringWriter.ToString().Trim());
            _writer.Flush();
        }
        catch
        {
            // Ignore pipe write errors
        }
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _pipeClient?.Dispose();
    }
}
