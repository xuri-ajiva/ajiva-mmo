using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Quic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Common.Helpers;
using Common.Networking.Rpc;
using Common.Rpc;
using Interception;
using Microsoft.Extensions.Logging;
using NetworkPackets;

namespace Common.Networking.AjivaQuic;

public class QConnectionStreamHandler(RpcSystemRegistry registry, ILogger logger)
    : IStreamHandler
{
    private readonly List<Task> _tasks = [];
    private readonly Dictionary<Type, QStream> _inboundStreams = [], _outBoundStreams = [];
    private Lock _streamLock = new();
    private CancellationToken _token;
    private QuicConnection _connection;
    private Instrumentation _instrumentation;
    private QuicStream _controlStream;
    private List<SystemMappings> _systemMapping;

    public void Poll(IRpcContext context)
    {
        using var sl = _streamLock.EnterScope();
        foreach (var qStream in _inboundStreams)
        {
            var system = registry.GetSystem(qStream.Key);
            try
            {
                PollStream(system, qStream.Value, context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public void PollContinuously(IRpcContext context)
    {
        //todo setup for new streams aswell
        using var sl = _streamLock.EnterScope();
        foreach (var keyValuePair in _inboundStreams)
        {
            var thread = AsyncHelper.Run(async () =>
            {
                while (!_token.IsCancellationRequested)
                {
                    try
                    {
                        await PollStreamAsync(registry.GetSystem(keyValuePair.Key), keyValuePair.Value, context);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }, logger, _token);
            _tasks.Add(thread);
        }
    }

    public static void PollStream<T>(T imple, QStream stream, IRpcContext context) where T : IRpcSystem
    {
        Span<u8> buffer = stackalloc u8[32768];

        while (stream.Reader.TryRead(out var result))
        {
            var readOnlySequence = result.Buffer;

            if (!MessageHeader.TryReadFrom(readOnlySequence, out var header, out var bytesRead))
            {
                stream.Reader.AdvanceTo(readOnlySequence.Start);
                return;
            }

            if (readOnlySequence.Length < bytesRead + header.PayloadLength)
            {
                stream.Reader.AdvanceTo(readOnlySequence.Start);
                return;
            }

            var reder = new SequenceReader<u8>(readOnlySequence.Slice(bytesRead, header.PayloadLength));
            if (header.PayloadLength <= buffer.Length)
            {
                var payload = buffer[..(i32)header.PayloadLength];
                reder.TryCopyTo(payload);
                imple.Dispatch(context, header.MethodId, payload);
            }
            else
            {
                var payload = ArrayPool<u8>.Shared.Rent((i32)header.PayloadLength);
                try
                {
                    reder.TryCopyTo(payload.AsSpan(0, (i32)header.PayloadLength));
                    imple.Dispatch(context, header.MethodId, payload);
                }
                finally
                {
                    ArrayPool<u8>.Shared.Return(payload);
                }
            }

            stream.Reader.AdvanceTo(readOnlySequence.GetPosition(bytesRead + header.PayloadLength));
        }
    }

    public static async Task PollStreamAsync<T>(T imple, QStream stream, IRpcContext context) where T : IRpcSystem
    {
        var headerBytes = new u8[MessageHeader.SizeOf];
        var streamReader = stream.Reader.AsStream();

        while (true)
        {
            await streamReader.ReadExactlyAsync(headerBytes);
            var header = MemoryMarshal.Read<MessageHeader>(headerBytes);

            var payload = ArrayPool<u8>.Shared.Rent((i32)header.PayloadLength);
            try
            {
                await streamReader.ReadExactlyAsync(payload.AsMemory(0, (i32)header.PayloadLength));
                imple.Dispatch(context, header.MethodId, payload);
            }
            finally
            {
                ArrayPool<u8>.Shared.Return(payload);
            }
        }
    }

    public async ValueTask Init(QuicConnection connection, Instrumentation instrumentation, bool isClient,
        CancellationToken token)
    {
        _connection = connection;
        _instrumentation = instrumentation;
        _token = token;
        if (isClient)
        {
            _controlStream = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, token);
            _systemMapping = await registry.OfferAwaitableSystems(_controlStream, token);
        }
        else
        {
            _controlStream = await connection.AcceptInboundStreamAsync(token);
            _systemMapping = await registry.DecideSystemsAndStreams(_controlStream, token);
        }

        AcceptStreams();

        await OpenMySystemStreams(isClient ? RpcSystemSideFlags.ServerHasImple : RpcSystemSideFlags.ClientHasImple);
    }

    private async ValueTask OpenMySystemStreams(RpcSystemSideFlags impleFlags)
    {
        logger.LogInformation("Opening Streams for as " + impleFlags);
        foreach (var mapping in _systemMapping)
        {
            if ((mapping.Flags & impleFlags) != impleFlags) continue;
            logger.LogInformation("Opening Stream for " + mapping.SystemHash);
            var stream = await _connection.OpenOutboundStreamAsync(
                mapping.Flags == RpcSystemSideFlags.Both ? QuicStreamType.Bidirectional : QuicStreamType.Unidirectional,
                _token);
            var streamSystemType = registry.BindSystemToStream(mapping, stream, _token);
            _outBoundStreams.Add(streamSystemType, new QStream(stream, _instrumentation, logger, _token));
        }
    }

    public QStream GetStreamFor<T>() where T : IRpcSystem
    {
        if (_outBoundStreams.TryGetValue(typeof(T), out var stream))
            return stream;
        throw new InvalidOperationException($"No stream found for system {typeof(T)}");
    }

    public IPEndPoint RemoteEndPoint => _connection.RemoteEndPoint;
    public IPEndPoint LocalEndPoint => _connection.LocalEndPoint;

    private void AcceptStreams()
    {
        _tasks.Add(AsyncHelper.Run(async () =>
        {
            while (!_token.IsCancellationRequested)
            {
                var stream = await _connection.AcceptInboundStreamAsync(_token);
                logger.LogInformation("New Stream accepted");
                HandleNewStreamAsync(stream);
            }
        }, logger, _token));
    }

    private void HandleNewStreamAsync(QuicStream stream)
    {
        try
        {
            var streamSystemType = registry.ResolveStreamSystem(stream, _systemMapping);
            using var sl = _streamLock.EnterScope();
            _inboundStreams.Add(streamSystemType, new QStream(stream, _instrumentation, logger, _token));
        }
        catch (Exception e)
        {
            Debug.WriteLine(e, "Error accepting new stream");
            registry.LogError(e, stream, _systemMapping);
            stream.Close();
        }
    }


    /*
    private void RunThreadLoop(Func<Task> action)
    {
        var thread = Task.Factory.StartNew(async () =>
        {
            //Instrumentation.Instance = instrumentation;
            var mater = _instrumentation.Meter?.CreateHistogram<f64>("QConnection_Loop_Out_Duration",
                "ms", "Duration of one loop in QConnection thread");
            while (!_token.IsCancellationRequested)
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
                            _token.Cancel();
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
                    if (_token.IsCancellationRequested)
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

    public async ValueTask DisposeAsync()
    {
        await _token.CancelAsync();
        foreach (var qStream in _streams)
        {
            qStream.Value.Close();
        }

        Task.WaitAll(_tasks);

        foreach (var qStream in _streams)
        {
            qStream.Value.Dispose();
        }

        await _connection.DisposeAsync();
        _token.Dispose();
        _tasks.Clear();
    }*/

    public override string ToString() => $"{_connection}: {_connection.RemoteEndPoint}";

    /*
    public void Close(SerializedMessage msg)
    {
        ctx.Cancel();

        Task.WaitAll(_tasks);

        foreach (var qStream in _streams)
        {
            qStream.Value.Close(msg);
            qStream.Value.Dispose();
        }

        var disposeAsync = _connection.DisposeAsync();
        ctx.Dispose();
        _tasks.Clear();
        disposeAsync.AsTask().GetAwaiter().GetResult();
    }*/
}