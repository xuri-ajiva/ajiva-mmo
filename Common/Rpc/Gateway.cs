using System.Buffers;
using System.Runtime.CompilerServices;
using Common.Networking;
using Common.Networking.AjivaQuic;
using Common.Networking.Rpc;
using Common.SpacialPartitioning;
using MemoryPack;
using MemoryPack.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkPackets;

namespace Common.Rpc;

[RpcSystem("IGatewayServerForClients", 1)]
public partial interface IGatewayServerForClients : IRpcSystem
{
    void Connect(IRpcContext context);
    void Disconnect(IRpcContext context);
}

[RpcSystem("IGatewayServerForWorkers", 1)]
public partial interface IGatewayServerForWorkers : IRpcSystem
{
    void RegisterServer(string name, i32 managementPort, i64 clientTimestamp);
    void ServerReady(ServerId serverId, i32 gamePort);
    void RegisterObserver();
}

[RpcSystem("IClusterWorkerForGateway", 1)]
public partial interface IClusterWorkerForGateway : IRpcSystem
{
    void InitServer(IRpcContext context, ServerId serverId, SpacialGridInfo gridInfo, RegionAssignment assignment,
        GlobalTickInfo tickInfo);

    void ClusterMapUpdate(IRpcContext context, RegionAssignment[] assignments);

    void ServerMapUpdate(IRpcContext context, ServerInfo[] servers);
}

/*[CompilerGenerated]
public partial interface IClusterWorkerForGateway : IRpcSystem
{
    string IRpcSystem.SystemHash =>
        nameof(IClusterWorkerForGateway); // todo actual hash (for now just exact methods and parameters

    void IRpcSystem.Dispatch(IRpcContext context, u16 methodId, ReadOnlySpan<u8> payload)
    {
        var reader = new MemoryPackReader(payload, null!);
        switch (methodId)
        {
            case 1:
            {
                var arg_serverId = reader.ReadUnmanaged<global::Common.SpacialPartitioning.ServerId>();
                var arg_gridInfo = reader.ReadUnmanaged<global::Common.SpacialPartitioning.SpacialGridInfo>();
                var arg_assignment = reader.ReadUnmanaged<global::Common.SpacialPartitioning.RegionAssignment>();
                var arg_tickInfo = reader.ReadUnmanaged<global::Common.Networking.GlobalTickInfo>();
                InitServer(context, arg_serverId, arg_gridInfo, arg_assignment, arg_tickInfo);
                break;
            }
            case 2:
            {
                var arg_assignments = reader.ReadArray<global::Common.SpacialPartitioning.RegionAssignment>();
                ClusterMapUpdate(context, arg_assignments);
                break;
            }
            case 3:
            {
                var arg_servers = reader.ReadArray<global::Common.SpacialPartitioning.ServerInfo>();
                ServerMapUpdate(context, arg_servers);
                break;
            }
            default:
                context.Logger.LogWarning("Received call to unknown method id {MethodId} for system {SystemHash}",
                    methodId,
                    SystemHash);
                break;
        }
    }
}

[CompilerGenerated]
file class ClusterWorkerForGatewayProxyRegistration : IRpcProxyRegistration
{
    public string SystemHash { get; } = nameof(IClusterWorkerForGateway);
    public Type SystemType { get; } = typeof(IClusterWorkerForGateway);
    public Type ProxyType { get; } = typeof(ClusterWorkerForGatewayProxy);
}

[CompilerGenerated]
public static class ClusterWorkerForGatewayProxyRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection Add()
        {
            return services.AddSingleton<IRpcProxyRegistration, ClusterWorkerForGatewayProxyRegistration>();
        }
    }
}

[CompilerGenerated]
file class ClusterWorkerForGatewayProxy(IRpcContext context) : IClusterWorkerForGateway
{
    // ... other methods


    public async ValueTask ServerMapUpdate(global::Common.SpacialPartitioning.ServerInfo[] servers)
    {
        var stream = context.StreamHandler.GetStreamFor<IClusterWorkerForGateway>();
        var bufferWriter = stream.Writer;
        var serializer = new SerializedMessage();
        try
        {
            var writer = new MemoryPackWriter<ReusableLinkedArrayBufferWriter>(ref serializer.Writer, serializer.State);

            writer.WriteArray(servers);
            // write stuff here

            writer.Flush();

            new MessageHeader
            {
                MethodId = 3,
                PayloadLength = (u32)serializer.Length
            }.WriteTo(bufferWriter);

            foreach (var memory in serializer)
            {
                bufferWriter.Write(memory.Span);
            }
        }
        finally
        {
            serializer.Dispose();
        }

        await bufferWriter.FlushAsync();
    }
}*/