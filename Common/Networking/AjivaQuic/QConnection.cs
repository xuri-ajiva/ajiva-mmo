using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Quic;
using Interception;
using NetworkPackets;
using Serilog;

namespace Common.Networking.AjivaQuic;

public class QConnection(
    QuicConnection connection,
    QStream primary,
    QStream secondary,
    Instrumentation instrumentation,
    CancellationTokenSource ctx)
    : IQConnection, IAsyncDisposable
{
    private readonly List<Thread> _threads = [];
    private Counter<i64> _inPackets, _outPackets, _outDropedPackets;

    [ExcludeFromInterception]
    public void StartLoops(IUniversalMessageHandler<QConnection> handler, Instrumentation instrumentation)
    {
        _inPackets = instrumentation.Meter.CreateCounter<i64>("QConnection_InPackets", "count",
            "Number of incoming packets");
        _outPackets = instrumentation.Meter.CreateCounter<i64>("QConnection_OutPackets", "count",
            "Number of outgoing packets");
        _outDropedPackets = instrumentation.Meter.CreateCounter<i64>("QConnection_OutDropedPackets", "count",
            "Number of droped outgoing packets due to full buffer");


        RunThreadLoop([TraceInterception]() => primary.Flush(ctx.Token), instrumentation);
        RunThreadLoop(() => secondary.Flush(ctx.Token), instrumentation);
        RunThreadLoop([TraceInterception]() => primary.ReadMessages(Wrap(handler)), instrumentation);
        RunThreadLoop(() => secondary.ReadMessages(Wrap(handler)), instrumentation);
    }

    private void RunThreadLoop(Action action, Instrumentation instrumentation)
    {
        var thread = new Thread(() =>
        {
            //Instrumentation.Instance = instrumentation;
            var mater = instrumentation.Meter.CreateHistogram<double>("QConnection_Loop_Duration",
                "ms", "Duration of one loop in QConnection thread");
            while (!ctx.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    action();
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
                    mater.Record(sw.Elapsed.TotalMilliseconds);
                }
            }
        })
        {
            Name = "QConnectionThread"
        };
        thread.Start();
        _threads.Add(thread);
    }

    private Action<ReadOnlySpan<u8>> Wrap(IUniversalMessageHandler<QConnection> handler)
    {
        return span =>
        {
            _inPackets.Add(1);
            handler.AutoHandle(span, this);
        };
    }

    private int chanel = 0;

    public void Send(SerializedMessage message, u8 streamId = 0)
    {
        SendToChannel(message, primary); //++chanel % 2 == 0 ? primary : secondary);
    }

    private void SendToChannel(SerializedMessage message, QStream qStream)
    {
        if (qStream.FreeBytes() < (i32)message.Length)
        {
            _outDropedPackets.Add(1);
            return;
        }

        _outPackets.Add(1);


        using var @lock = qStream.EnterLock();
        qStream.WriteLength(message.Length);
        foreach (var memory in message)
        {
            qStream.WriteData(memory.Span);
        }

        qStream.SignalDataAvailable();
    }

    public void Send(SerializedMessage message)
    {
        Send(message, 0);
    }

    public async ValueTask DisposeAsync()
    {
        await ctx.CancelAsync();
        await connection.DisposeAsync();
        foreach (var thread in _threads)
        {
            thread.Join();
        }

        ctx.Dispose();
        _threads.Clear();
    }

    public override string ToString() => $"{connection}: {connection.RemoteEndPoint}";
}