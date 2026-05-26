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