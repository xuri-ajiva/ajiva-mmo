using System.IO.Pipelines;
using Common.Networking.AjivaQuic;
using Common.Rpc;
using Microsoft.Extensions.Logging;

namespace Common.Networking.Rpc;

public interface IStreamHandler
{
    QStream GetStreamFor<T>() where T : IRpcSystem;
    void Poll(IRpcContext context);
    void PollContinuously(IRpcContext context);
}

public interface IRpcContext
{
    IStreamHandler StreamHandler { get; }
    ILogger Logger { get; }
    T GetProxy<T>() where T : IRpcSystem;
}

public interface IRpcSystem
{
    void Dispatch(IRpcContext context, u16 methodId, ReadOnlySpan<u8> payload);
    string DispatchDebug(u16 methodId, ReadOnlySpan<u8> payload);
    string SystemHash { get; }
}

public interface IRpcProxyRegistration
{
    public string SystemHash { get; }
    public Type SystemType { get; }
    public Type ProxyType { get; }
}

public static class RpcContext
{
    private static readonly AsyncLocal<IRpcContext?> AsyncLocalCurrent = new();

    public static IRpcContext Current => AsyncLocalCurrent.Value ?? throw new InvalidOperationException("No IRpcContext is active on this thread/async context.");

    public static IRpcContext? CurrentInternal
    {
        get => AsyncLocalCurrent.Value;
        set => AsyncLocalCurrent.Value = value;
    }
}

public static class RpcSystemExtensions
{
    public static IRpcContext GetContext(this IRpcSystem system) => RpcContext.Current;
    public static T GetProxy<T>(this IRpcSystem system) where T : class, IRpcSystem => RpcContext.Current.GetProxy<T>();
}