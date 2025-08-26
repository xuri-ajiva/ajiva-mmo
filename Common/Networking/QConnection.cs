using System.Diagnostics;
using System.Net.Quic;
using Interception;
using NetworkPackets;

namespace Common.Networking;

public class QConnection(QuicConnection connection, QStream primary, QStream secondary, CancellationToken token)
    : IQConnection, IAsyncDisposable
{
    public QuicConnection Connection { get; } = connection;
    private Thread? _primary, _secondary;

    [ExcludeFromInterception]
    public void Update(IUniversalMessageHandler<QConnection> handler)
    {
        _primary = RunThreadLoop(() => primary.Flush(token));
        _secondary = RunThreadLoop(() => secondary.Flush(token));
        RunTaskLoop(() => primary.ReadMessages(Wrap(handler)));
        RunTaskLoop(() => secondary.ReadMessages(Wrap(handler)));
    }

    private Thread RunThreadLoop(Action action)
    {
        var result = new Thread(() =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    // TODO Handle some how etc
                    Debug.WriteLine(e);
                }
            }
        });
        result.Start();
        return result;
    }

    private void RunTaskLoop(Func<ValueTask> loopFunc)
    {
        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await loopFunc();
                }
                catch (Exception e)
                {
                    // TODO Handle some how etc
                    Debug.WriteLine(e);
                }
            }
        }, token);
    }

    private Action<ReadOnlySpan<u8>> Wrap(IUniversalMessageHandler<QConnection> handler) =>
        span => handler.AutoHandle(span, this);

    public void Send(SerializedMessage message, u8 streamId = 0)
    {
        using var @lock = primary.EnterLock();
        primary.WriteLength(message.Length);
        foreach (var memory in message)
        {
            primary.WriteData(memory.Span);
        }

        primary.SignalDataAvailable();
    }

    public void Send(SerializedMessage message)
    {
        Send(message, 0);
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
        _primary?.Join();
        _secondary?.Join();
    }
}
