using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Quic;
using System.Threading.Channels;
using Interception;
using NetworkPackets;

namespace Common.Networking.AjivaQuic;

public class QConnection(
    QuicConnection connection,
    List<QStream> streams,
    Instrumentation instrumentation,
    CancellationTokenSource ctx)
    : IQConnection, IAsyncDisposable
{
    private readonly List<Task> _tasks = [];

    private readonly Counter<i64>? _inPackets =
            instrumentation.Meter?.CreateCounter<i64>("QConnection_InPackets", "count",
                "Number of incoming packets"),
        _outPackets = instrumentation.Meter?.CreateCounter<i64>("QConnection_OutPackets", "count",
            "Number of outgoing packets"),
        _outDroppedPackets = instrumentation.Meter?.CreateCounter<i64>("QConnection_OutDropedPackets", "count",
            "Number of droped outgoing packets due to full buffer");



    [ExcludeFromInterception]
    public void StartLoops()
    {
        foreach (var qStream in streams)
        {
            RunThreadLoop(WriterTask);
            RunThreadLoop(ReaderTask);
            continue;

            [TraceInterception]
            async Task WriterTask() => await qStream.FlushAsync(ctx.Token);

            [TraceInterception]
            async Task ReaderTask()
            {
                await qStream.ReadMessages(ctx.Token);
            }
        }
    }

    public void HandlePackets(PacketHandleDelegate<QConnection> handler)
    {
        foreach (var qStream in streams)
        {
            qStream.DecodeMessages(handler);
        }
    }

    private void RunThreadLoop(Func<Task> action)
    {
        var thread = Task.Factory.StartNew(async () =>
        {
            //Instrumentation.Instance = instrumentation;
            var mater = instrumentation.Meter?.CreateHistogram<f64>("QConnection_Loop_Out_Duration",
                "ms", "Duration of one loop in QConnection thread");
            while (!ctx.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    await action();
                }
                catch (QuicException e)
                {
                    switch (e.QuicError)
                    {
                        case QuicError.InternalError:
                        case QuicError.ConnectionAborted:
                        case QuicError.StreamAborted:
                        case QuicError.ConnectionIdle:
                        case QuicError.OperationAborted:
                            ctx.Cancel();
                            return;
                        case QuicError.ConnectionTimeout:
                        case QuicError.ConnectionRefused:
                        case QuicError.TransportError:
                            // wtf happened here
                            Debug.WriteLine(e, "CONFUSION01");
                            return;
                        default:
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    if (ctx.IsCancellationRequested)
                        return;
                    throw;
                }
                catch (Exception e)
                {
                    // TODO Handle some how etc
                    Debug.WriteLine(e);
                }
                finally
                {
                    sw.Stop();
                    mater?.Record(sw.Elapsed.TotalMilliseconds);
                }
            }
        });
        _tasks.Add(thread);
    }

    public void Send(SerializedMessage message, u8 streamId = 0)
    {
        SendToChannel(message, streams[0]); //++chanel % 2 == 0 ? primary : secondary);
    }

    private void SendToChannel(SerializedMessage message, QStream qStream)
    {
        if (ctx.IsCancellationRequested) return;
        try
        {
            var spaceNeeded = (i32)message.Length + BitPackingHelper.MaxVarLength;
            if (qStream.FreeBytes() < spaceNeeded)
            {
                _outDroppedPackets?.Add(1);
                return;
            }

            using var @lock = qStream.EnterLock();
            if (qStream.FreeBytes() < spaceNeeded)
            {
                _outDroppedPackets?.Add(1);
                return;
            }

            qStream.WriteLength(message.Length);
            foreach (var memory in message)
            {
                qStream.WriteData(memory.Span);
            }

            _outPackets?.Add(1);
        }
        finally
        {
            qStream.SignalDataAvailable();
        }
    }

    public void Send(SerializedMessage message)
    {
        Send(message, 0);
    }

    public async ValueTask DisposeAsync()
    {
        await ctx.CancelAsync();
        foreach (var qStream in streams)
        {
            qStream.Close();
        }

        Task.WaitAll(_tasks);

        foreach (var qStream in streams)
        {
            qStream.Dispose();
        }

        await connection.DisposeAsync();
        ctx.Dispose();
        _tasks.Clear();
    }

    public override string ToString() => $"{connection}: {connection.RemoteEndPoint}";

    public void Close(SerializedMessage msg)
    {
        ctx.Cancel();

        Task.WaitAll(_tasks);

        foreach (var qStream in streams)
        {
            qStream.Close(msg);
            qStream.Dispose();
        }

        var disposeAsync = connection.DisposeAsync();
        ctx.Dispose();
        _tasks.Clear();
        disposeAsync.AsTask().GetAwaiter().GetResult();
    }
}