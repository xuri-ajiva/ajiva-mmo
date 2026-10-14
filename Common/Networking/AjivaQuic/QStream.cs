using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.Pipelines;
using System.Net.Quic;
using Interception;
using Microsoft.Extensions.Logging;

namespace Common.Networking.AjivaQuic;

public class QStream : IDisposable
{
    private readonly Instrumentation _instrumentation;
    private readonly ILogger _logger;
    private readonly CancellationToken _token;
    private readonly Pipe _outboundPipe, _inboundPipe;
    private readonly SemaphoreSlim _dataAvailableSignal = new(1, 1);
    private readonly Lock _writeLock = new();

    private readonly Histogram<f64>? _inMeter, _outMeter;
    private readonly QuicStream _stream;
    private readonly QConnectionStreamHandler _streamHandler;
    private readonly Task _readerTask, _writerTask;
    public i64 Id => _stream.Id;

    public QStream(QuicStream stream,
        QConnectionStreamHandler streamHandler,
        Instrumentation instrumentation,
        ILogger logger,
        CancellationToken token = default)
    {
        _stream = stream;
        _streamHandler = streamHandler;
        _instrumentation = instrumentation;
        _logger = logger;
        _token = token;
        /*
        _inMeter = instrumentation.Meter?.CreateHistogram<f64>(
            "QStream_ReadMessages_Duration_" + stream.Id, "ms", "Duration of ReadMessages call");
        _outMeter = instrumentation.Meter?.CreateHistogram<f64>("QStream_Flush_Duration_" + stream.Id, "ms",
            "Duration of Flush call");
            */

        const i32 pauseThreshold = 1024 * 1024;
        var options = new PipeOptions(pauseWriterThreshold: pauseThreshold, resumeWriterThreshold: pauseThreshold / 2);
        _outboundPipe = new Pipe(options);
        _inboundPipe = new Pipe(options);
        /*_readerTask = ReadOutPipeAsync(token);
        _writerTask = WriteInPipeAsync(token);*/

        if (stream.CanWrite)
        {
            _writerTask = Task.Run(() => WrapAsync(_outboundPipe.Reader.CopyToAsync(_stream, cancellationToken: token)),
                token);
        }

        if (stream.CanRead)
        {
            _readerTask = Task.Run(() => WrapAsync(_stream.CopyToAsync(_inboundPipe.Writer, cancellationToken: token)),
                token);
        }
    }

    private async Task? WrapAsync(Task task)
    {
        try
        {
            await task;
        }
        catch (QuicException e) when (_streamHandler.IsQuicErrAbort(e))
        {
            //
        }
        catch (OperationCanceledException)
        {
            if (!_token.IsCancellationRequested)
                _streamHandler.CloseIfOpen();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in QStream {StreamId}", Id);
        }
    }

    private async Task WriteInPipeAsync(CancellationToken token)
    {
        await _stream.CopyToAsync(_inboundPipe.Writer, cancellationToken: token);
    }

    public PipeWriter Writer => _outboundPipe.Writer;
    public PipeReader Reader => _inboundPipe.Reader;

    public QStreamWriterGuard AcquireWriter()
    {
        _writeLock.Enter();
        return new QStreamWriterGuard(this, _outboundPipe.Writer);
    }

    internal void ReleaseWriter()
    {
        _writeLock.Exit();
    }

    [TraceInterception]
    private async Task ReadOutPipeAsync(CancellationToken token)
    {
        await _outboundPipe.Reader.CopyToAsync(_stream, token);
        return;

        while (true)
        {
            var startingTimestamp = Stopwatch.GetTimestamp();

            var readResult = await _outboundPipe.Reader.ReadAsync(token);

            foreach (var memory in readResult.Buffer)
            {
                await _stream.WriteAsync(memory, token);
                _outboundPipe.Writer.Advance(memory.Length);
            }

            await _stream.FlushAsync(token);
            _outMeter?.Record(Stopwatch.GetElapsedTime(startingTimestamp).TotalMilliseconds);

            if (readResult.IsCompleted || readResult.IsCanceled) break;
        }
    }

    public void Close()
    {
        _stream.Close();
    }

    public void Close(SerializedMessage msg)
    {
        //todo remove
        Debugger.Break();
        _stream.Close();
    }

    public void Dispose()
    {
        _readerTask.Dispose();
        _writerTask.Dispose();
        _dataAvailableSignal.Dispose();
        _stream.Dispose();
    }
}

public readonly struct QStreamWriterGuard(QStream stream, PipeWriter writer) : IDisposable
{
    public PipeWriter Writer { get; } = writer;

    public void Dispose()
    {
        try
        {
            Writer.FlushAsync().AsTask().Wait();
        }
        finally
        {
            stream.ReleaseWriter();
        }
    }
}