using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.Pipelines;
using System.Net.Quic;
using Common.Helpers;
using Interception;
using Microsoft.Extensions.Logging;
using NetworkPackets;

namespace Common.Networking.AjivaQuic;

public class QStream : IDisposable
{
    private readonly Instrumentation _instrumentation;
    private readonly Pipe _outboundPipe, _inboundPipe;
    private readonly SemaphoreSlim _dataAvailableSignal = new(1, 1);

    private readonly Histogram<f64>? _inMeter, _outMeter;
    private readonly QuicStream _stream;
    private readonly Task _readerTask, _writerTask;
    public i64 Id => _stream.Id;

    public QStream(QuicStream stream,
        Instrumentation instrumentation,
        ILogger logger,
        CancellationToken token = default)
    {
        _stream = stream;
        _instrumentation = instrumentation;
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
            _writerTask = AsyncHelper.Run(
                () => _outboundPipe.Reader.CopyToAsync(_stream, cancellationToken: token),
                logger, token);
        }

        if (stream.CanRead)
        {
            _readerTask = AsyncHelper.Run(
                () => _stream.CopyToAsync(_inboundPipe.Writer, cancellationToken: token),
                logger, token);
        }
    }

    private async Task WriteInPipeAsync(CancellationToken token)
    {
        await _stream.CopyToAsync(_inboundPipe.Writer, cancellationToken: token);
    }

    public PipeWriter Writer => _outboundPipe.Writer;
    public PipeReader Reader => _inboundPipe.Reader;

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