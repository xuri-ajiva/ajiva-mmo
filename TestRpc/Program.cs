using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Common;
using Common.Ecs;
using Common.Helpers;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.Networking.Rpc;
using Common.Rpc;
using Common.SpacialPartitioning;
using MemoryPack;
using MemoryPack.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestRpc;

[RpcSystem("chat.client", 1)]
public partial interface IChatClientSystem : IRpcSystem
{
    void ReceiveMessage(i64 timestampSend, string from, string text);
}

[RpcSystem("chat.server", 1)]
public partial interface IChatServerSystem : IRpcSystem
{
    void SendMessage(i64 timestamp, string from, string text);
}

[RpcSystem("ping.pong", 1)]
public partial interface IPingPongSystem : IRpcSystem
{
    void Ping(i64 startTime, i64 total, i64 count);
    void BigData(i64 sendTime, u8[] data);

    void Test(string data, String a, int x, long y, float f, double d, bool b, decimal dec, List<int> list,
        Dictionary<string, int> dict, HashSet<string> set,
        Span<byte> span, ReadOnlySpan<byte> readOnlySpan,
        Memory<EntityId> memory, ReadOnlyMemory<Vector3?> readOnlyMemory,
        ActivityTraceFlags traceFlags,
        Guid guid,
        Packets.ServerType type,
        NetEntId EntityId,
        Vector3? Position,
        Quaternion? Rotation,
        Vector3? Velocity,
        PositionChunkData chunkData,
        string[] IpEndpoints
    );
}

[MemoryPackable]
public partial record struct TestData(
    string data,
    String a,
    int x,
    long y,
    float f,
    double d,
    bool b,
    decimal dec,
    List<int> list,
    Dictionary<string, int> dict,
    HashSet<string> set,
    ActivityTraceFlags traceFlags,
    Guid guid,
    Packets.ServerType type,
    NetEntId EntityId,
    Vector3? Position,
    Quaternion? Rotation,
    Vector3? Velocity,
    PositionChunkData chunkData,
    string[] IpEndpoints);

public class PingPongImple(ILogger<PingPongImple> logger) : IPingPongSystem
{
    public void Ping(i64 startTime, i64 total, i64 count)
    {
        if (count >= total)
        {
            var totalTime = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
            logger.LogInformation(
                "Pong received, test complete! Took: {Latency:F2}ms, Messages: {Total}, RPS: {RPS:F2}", totalTime,
                total, total / (totalTime / 1000.0));
            return;
        }

        if (count % 10000 == 0)
            logger.LogInformation("Pong received, remaining: {Remaining}", total - count);

        this.GetProxy<IPingPongSystem>().Ping(startTime, total, count + 1);
    }

    public void BigData(i64 sendTime, u8[] data)
    {
        var latencyMS = Stopwatch.GetElapsedTime(sendTime).TotalMilliseconds;
        logger.LogInformation(
            "BigData received, size: {Size} bytes, latency: {Latency:F2}ms, throughput: {Throughput:F2} MB/s",
            data.Length, latencyMS, data.Length / (1024.0 * 1024.0) / (latencyMS / 1000.0));
    }

    public void Test(string data, string a, int x, long y, float f, double d, bool b, decimal dec, List<int> list,
        Dictionary<string, int> dict,
        HashSet<string> set, Span<byte> span, ReadOnlySpan<byte> readOnlySpan, Memory<EntityId> memory,
        ReadOnlyMemory<Vector3?> readOnlyMemory,
        ActivityTraceFlags traceFlags, Guid guid, Packets.ServerType type, NetEntId EntityId, Vector3? Position,
        Quaternion? Rotation, Vector3? Velocity, PositionChunkData chunkData, string[] IpEndpoints)
    {
        /*
                     pingPong.Test(
                "TestData",
                "String value",
                42,
                1234567890123456789,
                3.14f,
                2.71828,
                true,
                12345.6789m,
                [1, 2, 3, 4, 5],
                new Dictionary<string, int> { { "one", 1 }, { "two", 2 } },
                ["apple", "banana", "cherry"],
                [1, 2, 3, 4, 5],
                [5, 4, 3, 2, 1],
                new Memory<EntityId>([new EntityId(1), new EntityId(2)]),
                new ReadOnlyMemory<Vector3?>([new Vector3(1, 2, 3), null]),
                ActivityTraceFlags.Recorded,
                Guid.NewGuid(),
                Packets.ServerType.EntryPoint,
                new NetEntId(123, new EntityId(23)),
                new Vector3(10, 20, 30),
                Quaternion.Identity,
                new Vector3(0.1f, 0.2f, 0.3f),
                PositionChunkData.QuantizeFor(new ChunkCoord(0, 0, 0, 0),
                    new SpacialGridInfo(new ChunkCoord(16, 16, 16, 1)), new Vector3(5, 10, 15)),
                ["asd"]);
         */

        Debug.Assert(data == "TestData");
        Debug.Assert(a == "String value");
        Debug.Assert(x == 42);
        Debug.Assert(y == 1234567890123456789);
        Debug.Assert(Math.Abs(f - 3.14f) < 0.001);
        Debug.Assert(Math.Abs(d - 2.71828) < 0.00001);
        Debug.Assert(b == true);
        Debug.Assert(dec == 12345.6789m);
        Debug.Assert(list.SequenceEqual([1, 2, 3, 4, 5]));
        Debug.Assert(dict.Count == 2 && dict["one"] == 1 && dict["two"] == 2);
        Debug.Assert(set.SetEquals(["apple", "banana", "cherry"]));
        Debug.Assert(span.SequenceEqual(new u8[] { 1, 2, 3, 4, 5 }));
        Debug.Assert(readOnlySpan.SequenceEqual(new u8[] { 5, 4, 3, 2, 1 }));
        Debug.Assert(memory.Span.SequenceEqual(new[] { new EntityId(1), new EntityId(2) }));
        Debug.Assert(readOnlyMemory.Span[0] == new Vector3(1, 2, 3));
        Debug.Assert(readOnlyMemory.Span[1] == null);
        Debug.Assert(traceFlags == ActivityTraceFlags.Recorded);
        Debug.Assert(guid != Guid.Empty);
        Debug.Assert(type == Packets.ServerType.EntryPoint);
        Debug.Assert(EntityId.Server == 123 && EntityId.Entity == new EntityId(23));
        Debug.Assert(Position == new Vector3(10, 20, 30));
        Debug.Assert(Rotation == Quaternion.Identity);
        Debug.Assert(Velocity == new Vector3(0.1f, 0.2f, 0.3f));
        Debug.Assert(chunkData == PositionChunkData.QuantizeFor(new ChunkCoord(0, 0, 0, 0),
            new SpacialGridInfo(new ChunkCoord(16, 16, 16, 1)), new Vector3(5, 10, 15)));
        Debug.Assert(IpEndpoints.SequenceEqual(new[] { "asd" }));
    }
}

// Chat Server Implementation
public class ChatServer : IChatServerSystem
{
    private readonly List<(string from, i64 timestamp, string text)> _messages = new();
    private readonly ILogger _logger;
    private QServer? _server;
    public ChatServer(ILogger<ChatServer> logger) => _logger = logger;

    public void SendMessage(i64 timestamp, string from, string text)
    {
        _messages.Add((from, timestamp, text));
        var now = Stopwatch.GetTimestamp();
        var latencyMs = (now - timestamp) / (double)Stopwatch.Frequency * 1000;
        _logger.LogInformation("[SERVER] {From}: {Text} (latency: {Latency:F2}ms)", from, text, latencyMs);

        if (_server is { } server)
        {
            foreach (var connection in server.GetConnections())
            {
                connection.GetProxy<IChatClientSystem>()
                    .ReceiveMessage(timestamp, from, text);
            }
        }
    }

    public void SetServer(QServer server)
    {
        _server = server;
    }
}

// Chat Client Implementation
public class ChatClient : IChatClientSystem
{
    private readonly ILogger _logger;

    public ChatClient(ILogger<ChatClient> logger) => _logger = logger;

    public void ReceiveMessage(i64 timestampSend, string from, string text)
    {
        var now = Stopwatch.GetTimestamp();
        var latencyMs = (now - timestampSend) / (double)Stopwatch.Frequency * 1000;
        _logger.LogInformation("[CLIENT] {From}: {Text} (latency: {Latency:F2}ms)", from, text, latencyMs);
    }
}

class Program
{
    static async Task Main()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║    RPC Chat using QFactory Infrastructure      ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝\n");

        Console.Write("Are you running as [S]erver or [C]lient or [M]any Clients? ");
        var mode = Console.ReadLine()?.ToUpper();

        if (mode == "S")
            await RunServerAsync();
        else if (mode == "C")
            await RunClientAsync();
        else if (mode == "M")
        {
            Console.Write("How many clients to run? ");
            var countStr = Console.ReadLine();
            var count = int.TryParse(countStr, out var c) ? c : 10;
            await RunAutoClientAsync(count);
        }
        else
            Console.WriteLine("Invalid choice");
    }

    static async Task RunServerAsync()
    {
        Console.Write("Enter port to listen on (default 5000): ");
        var portStr = Console.ReadLine();
        var port = int.TryParse(portStr, out var p) ? p : 5000;

        // Build services with RPC infrastructure
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Register RPC systems
        services.AddAsRpcSystem<IChatServerSystem, ChatServer>();
        services.AddAsRpcSystem<IPingPongSystem, PingPongImple>();
        services.AddIPingPongSystem();
        services.AddIChatClientSystem();
        services.AddSingleton<RpcSystemRegistry>();

        // Add QFactory
        services.AddSingleton<QFactory>();

        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<Program>>();
        var factory = provider.GetRequiredService<QFactory>();
        var server = factory.CreateServer(port);
        var chatServer = provider.GetRequiredService<IChatServerSystem>() as ChatServer;
        chatServer.SetServer(server);

        server.Start(new SevlListener());
        logger.LogInformation("Server created, listening on port {Port}", port);
        logger.LogInformation("Type messages to broadcast (type 'quit' to exit):\n");

        server.HandlePacketsAsync();

        /*_ = AsyncHelper.Run(async () =>
        {
            while (true)
            {
                var connections = server.GetConnections().ToArray();
                logger.LogInformation("Connected clients: {Count}, State: {Status}", connections.Count(),
                    string.Join(',', connections.Select(x => x.State)));
                await Task.Delay(1000);
            }
        }, logger);*/

        var serverImpl = provider.GetRequiredService<IChatServerSystem>();

        try
        {
            while (true)
            {
                var input = Console.ReadLine();
                if (input?.ToLower() == "quit")
                    break;
                if (input?.ToLower() == "clients")
                {
                    var connections = server.GetConnections();
                    logger.LogInformation("Connected clients: {Count}", connections.Count());
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(input))
                {
                    var timestamp = Stopwatch.GetTimestamp();
                    serverImpl.SendMessage(timestamp, "SERVER", input);
                }
            }
        }
        finally
        {
            await server.DisposeAsync();
            await provider.DisposeAsync();
        }
    }

    static async Task RunClientAsync()
    {
        Console.Write("Enter your name: ");
        var clientName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(clientName))
            clientName = "Client";
        Console.Write("Enter server host (default localhost): ");
        var host = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(host))
            host = "127.0.0.1";

        Console.Write("Enter server port (default 5000): ");
        var portStr = Console.ReadLine();
        var port = int.TryParse(portStr, out var p) ? p : 5000;

        // Build services with RPC infrastructure
        var services = new ServiceCollection();
        services.AddIChatServerSystem();
        services.AddIPingPongSystem();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Register RPC systems and proxies
        services.AddAsRpcSystem<IPingPongSystem, PingPongImple>();
        services.AddAsRpcSystem<IChatClientSystem, ChatClient>();
        services.AddSingleton<RpcSystemRegistry>();

        // Add QFactory
        services.AddSingleton<QFactory>();

        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<Program>>();
        var factory = provider.GetRequiredService<QFactory>();

        logger.LogInformation("Connecting to {Host}:{Port} as '{ClientName}'...\n", host, port, clientName);

        try
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(host), port);
            var client = factory.CreateClient(endpoint);
            await client.ConnectAsync(new Instrumentation("", ""));

            logger.LogInformation("Connected! Type messages (type 'quit' to exit):\n");

            var clientImpl = client.GetProxy<IChatServerSystem>();
            var pingPong = client.GetProxy<IPingPongSystem>();

            pingPong.Test(
                "TestData",
                "String value",
                42,
                1234567890123456789,
                3.14f,
                2.71828,
                true,
                12345.6789m,
                [1, 2, 3, 4, 5],
                new Dictionary<string, int> { { "one", 1 }, { "two", 2 } },
                ["apple", "banana", "cherry"],
                [1, 2, 3, 4, 5],
                [5, 4, 3, 2, 1],
                new Memory<EntityId>([new EntityId(1), new EntityId(2)]),
                new ReadOnlyMemory<Vector3?>([new Vector3(1, 2, 3), null]),
                ActivityTraceFlags.Recorded,
                Guid.NewGuid(),
                Packets.ServerType.EntryPoint,
                new NetEntId(123, new EntityId(23)),
                new Vector3(10, 20, 30),
                Quaternion.Identity,
                new Vector3(0.1f, 0.2f, 0.3f),
                PositionChunkData.QuantizeFor(new ChunkCoord(0, 0, 0, 0),
                    new SpacialGridInfo(new ChunkCoord(16, 16, 16, 1)), new Vector3(5, 10, 15)),
                ["asd"]);

            client.HandlePacketsAsync();

            while (true)
            {
                var input = Console.ReadLine();
                if (input?.ToLower() == "quit")
                    break;

                if (input?.ToLower() == "ping")
                {
                    logger.LogInformation("Ping Pong RPS Test");
                    pingPong.Ping(Stopwatch.GetTimestamp(), 1000000, 0);
                    continue;
                }

                if (input?.ToLower().StartsWith("big") ?? false)
                {
                    logger.LogInformation("Sending big data...");
                    var amount = input[3..];
                    var data = new u8[(i32.TryParse(amount, out var mb) ? mb : 10) * 1024 * 1024];
                    new Random().NextBytes(data);
                    pingPong.BigData(Stopwatch.GetTimestamp(), data);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(input))
                {
                    var timestamp = Stopwatch.GetTimestamp();
                    clientImpl.SendMessage(timestamp, clientName, input);
                }
            }

            await client.DisconnectAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Connection error");
        }
        finally
        {
            provider.Dispose();
        }
    }

    static async Task RunAutoClientAsync(i32 clients)
    {
        Console.Write("Enter your name: ");
        var clientName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(clientName))
            clientName = "Client";
        Console.Write("Enter server host (default localhost): ");
        var host = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(host))
            host = "127.0.0.1";

        Console.Write("Enter server port (default 5000): ");
        var portStr = Console.ReadLine();
        var port = int.TryParse(portStr, out var p) ? p : 5000;

        // Build services with RPC infrastructure
        var services = new ServiceCollection();
        services.AddIChatServerSystem();
        services.AddIPingPongSystem();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Register RPC systems and proxies
        services.AddAsRpcSystem<IPingPongSystem, PingPongImple>();
        services.AddAsRpcSystem<IChatClientSystem, ChatClient>();
        services.AddSingleton<RpcSystemRegistry>();

        // Add QFactory
        services.AddSingleton<QFactory>();

        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<Program>>();
        var factory = provider.GetRequiredService<QFactory>();

        logger.LogInformation("Spawning {Count} clients", clients);

        try
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(host), port);
            List<Task> clientTasks = new(clients);
            for (int i = 0; i < clients; i++)
            {
                var client = factory.CreateClient(endpoint);
                await client.ConnectAsync(new Instrumentation("", ""));

                var clientImpl = client.GetProxy<IChatServerSystem>();

                client.HandlePacketsAsync();
                var i1 = i;
                clientTasks.Add(AsyncHelper.Run(async () =>
                {
                    var name = $"{clientName}_{i1 + 1}";
                    var count = 0;
                    while (true)
                    {
                        var message = $"Hello from {name} - {++count}";
                        var timestamp = Stopwatch.GetTimestamp();
                        clientImpl.SendMessage(timestamp, name, message);
                        await Task.Delay(1000);
                    }
                }, logger));

                await Task.Delay(100);
            }

            await Task.WhenAll(clientTasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Connection error");
        }
        finally
        {
            provider.Dispose();
        }
    }
}

internal class SevlListener : QServerEventListener
{
    public void OnClientConnected(QuicListener server, QServerConnection connection)
    {
        Console.WriteLine("Client connected: {0}", connection);
    }

    public void OnClientDisconnected(QServerConnection connection)
    {
        Console.WriteLine("Client disconnected: {0}", connection);
    }

    public void OnConnectionFailed(QuicListener server, IPEndPoint? connection, Exception exception)
    {
        Console.WriteLine("Connection failed: {0}, Exception: {1}", connection, exception);
    }
}