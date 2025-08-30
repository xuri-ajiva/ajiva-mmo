using System.Diagnostics;
using System.Net.Quic;
using Interception;
using NetworkPackets;

namespace Common.Networking;

public class QConnection(QuicConnection connection, QStream primary, QStream secondary, CancellationTokenSource ctx)
    : IQConnection, IAsyncDisposable
{
    private readonly List<Thread> _threads = [];

    [ExcludeFromInterception]
    public void Update(IUniversalMessageHandler<QConnection> handler)
    {
        RunThreadLoop(() => primary.Flush(ctx.Token));
        RunThreadLoop(() => secondary.Flush(ctx.Token));
        RunThreadLoop(() => primary.ReadMessages(Wrap(handler)));
        RunThreadLoop(() => secondary.ReadMessages(Wrap(handler)));
    }

    private void RunThreadLoop(Action action)
    {
        var thread = new Thread(() =>
        {
            while (!ctx.IsCancellationRequested)
            {
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
            }
        })
        {
            Name = "QConnectionThread"
        };
        thread.Start();
        _threads.Add(thread);
    }

    private Action<ReadOnlySpan<u8>> Wrap(IUniversalMessageHandler<QConnection> handler) =>
        span => handler.AutoHandle(span, this);

    private int chanel = 0;

    public void Send(SerializedMessage message, u8 streamId = 0)
    {
        SendToChannel(message, ++chanel % 2 == 0 ? primary : secondary);
    }

    private static void SendToChannel(SerializedMessage message, QStream qStream)
    {
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