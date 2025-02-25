using OpenTelemetry.Trace;

namespace client;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Tracer _tracer;

    public Worker(ILogger<Worker> logger, Tracer tracer)
    {
        _logger = logger;
        _tracer = tracer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var span = _tracer.StartRootSpan("Worker", SpanKind.Client);
        int i = 0;
        while (!stoppingToken.IsCancellationRequested && i++ < 30)
        {
            using var inner = _tracer.StartActiveSpan("Worker", SpanKind.Internal, span);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogTrace("Worker running at: {Time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
