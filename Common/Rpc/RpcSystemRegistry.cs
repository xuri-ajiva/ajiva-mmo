using System.Buffers.Binary;
using System.Net.Quic;
using System.Text;
using Common.Networking.Rpc;
using Microsoft.Extensions.Logging;

namespace Common.Rpc;

public class RpcSystemRegistry
{
    private readonly Dictionary<Type, IRpcProxyRegistration> _proxyTypes;

    private readonly Dictionary<string, IRpcSystem> _impleTypesByHash;
    private readonly Dictionary<Type, IRpcSystem> _impleTypes;
    private readonly ILogger<RpcSystemRegistry> _logger;

    public RpcSystemRegistry(ILogger<RpcSystemRegistry> logger,
        IEnumerable<IRpcSystem> systems,
        IEnumerable<IRpcProxyRegistration> proxyRegistrations)
    {
        _logger = logger;
        _proxyTypes = proxyRegistrations.ToDictionary(p => p.SystemType, p => p);
        _impleTypes = systems.ToDictionary(x => x.GetType(), x => x);
        _impleTypesByHash = _impleTypes.Values.ToDictionary(s => s.SystemHash, s => s);
    }

    public IRpcSystem GetSystem(Type systemType)
    {
        if (_impleTypes.TryGetValue(systemType, out var imple))
        {
            return imple;
        }

        throw new InvalidOperationException($"No implementation registered for {systemType}");
    }

    public IRpcSystem GetSystem(string systemHash)
    {
        if (_impleTypesByHash.TryGetValue(systemHash, out var imple))
        {
            return imple;
        }

        throw new InvalidOperationException($"No implementation registered for {systemHash}");
    }

    public void AddProxy(IRpcProxyRegistration registration)
    {
        _proxyTypes.Add(registration.SystemType, registration);
    }

    public TProxy CreateProxy<TProxy>(IRpcContext context) where TProxy : IRpcSystem
    {
        if (_proxyTypes.TryGetValue(typeof(TProxy), out var registration))
        {
            return (TProxy)Activator.CreateInstance(registration.ProxyType, context)!;
        }

        throw new InvalidOperationException($"No proxy registered for {typeof(TProxy).Name}");
    }

    public async ValueTask<List<SystemMappings>> OfferAwaitableSystems(QuicStream controlStream,
        CancellationToken token)
    {
        const i32 bufferSize = 4096;

        // Phase 1 offer all our systems (aka what proxies where registered)
        Span<u8> buffer = stackalloc u8[bufferSize];
        var proxyTypesCount = _proxyTypes.Values.Select(x => x.SystemHash).ToList();
        foreach (var keyValuePair in _impleTypesByHash)
        {
            if (proxyTypesCount.Any(x => x == keyValuePair.Key))
                continue;

            proxyTypesCount.Add(keyValuePair.Key);
        }

        BinaryPrimitives.WriteInt32LittleEndian(buffer, proxyTypesCount.Count);
        controlStream.Write(buffer[..4]);

        foreach (var systemHash in proxyTypesCount)
        {
            Encoding.UTF8.TryGetBytes(systemHash, buffer[4..], out var bytesWritten);
            BinaryPrimitives.WriteInt32LittleEndian(buffer, bytesWritten);
            controlStream.Write(buffer[..(4 + bytesWritten)]);
        }

        await controlStream.FlushAsync(token);

        // Phase 2 wait for the other side to decide which systems and streams to use and return that info
        Span<u8> readBuffer = stackalloc u8[bufferSize];
        controlStream.ReadExactly(readBuffer[..4]);
        var systemsCount = BinaryPrimitives.ReadInt32LittleEndian(readBuffer);
        var mappings = new List<SystemMappings>(systemsCount);
        for (var i = 0; i < systemsCount; i++)
        {
            controlStream.ReadExactly(readBuffer[..4]);
            var id = BinaryPrimitives.ReadUInt32LittleEndian(readBuffer);
            controlStream.ReadExactly(readBuffer[..4]);
            var hashLength = BinaryPrimitives.ReadInt32LittleEndian(readBuffer);
            controlStream.ReadExactly(readBuffer[..hashLength]);
            var systemHash = Encoding.UTF8.GetString(readBuffer[..hashLength]);
            var flags = (RpcSystemSideFlags)controlStream.ReadByte();
            mappings.Add(new SystemMappings(id, systemHash, flags));
        }

        return mappings;
    }

    public async Task<List<SystemMappings>> DecideSystemsAndStreams(QuicStream controlStream, CancellationToken token)
    {
        // Phase 1 read offered systems
        const i32 bufferSize = 4096;
        Span<u8> buffer = stackalloc u8[bufferSize];
        controlStream.ReadExactly(buffer[..4]);
        var offeredSystemsCount = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        var offeredSystems = new List<string>(offeredSystemsCount);
        for (var i = 0; i < offeredSystemsCount; i++)
        {
            controlStream.ReadExactly(buffer[..4]);
            var hashLength = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            controlStream.ReadExactly(buffer[..hashLength]);
            var systemHash = Encoding.UTF8.GetString(buffer[..hashLength]);
            offeredSystems.Add(systemHash);
        }

        // Phase 2 Build mapping for the systems we want to use and return that info
        var mappings = new List<SystemMappings>(offeredSystemsCount + _proxyTypes.Count);
        foreach (var systemHash in offeredSystems)
        {
            var flags = RpcSystemSideFlags.None;

            // do we have that system registered as callable
            if (_proxyTypes.Any(x => x.Value.SystemHash == systemHash))
            {
                flags |= RpcSystemSideFlags.ClientHasImple;
            }

            // do we have an implementation for that system
            if (_impleTypesByHash.ContainsKey(systemHash))
            {
                flags |= RpcSystemSideFlags.ServerHasImple;
            }

            if (flags != RpcSystemSideFlags.None)
            {
                mappings.Add(new SystemMappings((u32)mappings.Count, systemHash, flags));
            }
        }

        foreach (var registration in _proxyTypes.Values)
        {
            if (mappings.Any(x => x.SystemHash == registration.SystemHash))
                continue;

            mappings.Add(new SystemMappings((u32)mappings.Count, registration.SystemHash,
                RpcSystemSideFlags.ClientHasImple));
        }


        // Phase 3 send updated stuff
        Span<u8> writeBuffer = stackalloc u8[bufferSize];
        BinaryPrimitives.WriteInt32LittleEndian(writeBuffer, mappings.Count);
        controlStream.Write(writeBuffer[..4]);
        foreach (var mapping in mappings)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(writeBuffer, mapping.Id);
            controlStream.Write(writeBuffer[..4]);
            Encoding.UTF8.TryGetBytes(mapping.SystemHash, writeBuffer[4..], out var bytesWritten);
            BinaryPrimitives.WriteInt32LittleEndian(writeBuffer, bytesWritten);
            controlStream.Write(writeBuffer[..(4 + bytesWritten)]);
            controlStream.WriteByte((u8)mapping.Flags);
        }

        await controlStream.FlushAsync(token);

        return mappings;
    }

    public Type ResolveStreamSystem(QuicStream stream, List<SystemMappings> systemMapping)
    {
        Span<u8> buffer = stackalloc u8[4];
        stream.ReadExactly(buffer);
        var systemId = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        Console.WriteLine("Stream Request for: " + systemId);
        foreach (var mapping in systemMapping)
        {
            if (mapping.Id == systemId)
            {
                Console.WriteLine("Hash is: " + mapping.SystemHash);
                return _impleTypesByHash[mapping.SystemHash].GetType();
            }
        }

        throw new InvalidOperationException($"No system mapping found for stream with system id {systemId}");
    }


    public Type BindSystemToStream(SystemMappings mapping, QuicStream stream, CancellationToken token)
    {
        Span<u8> buffer = stackalloc u8[4];
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, mapping.Id);
        stream.Write(buffer);
        foreach (var iRpcProxyRegistration in _proxyTypes)
        {
            if (iRpcProxyRegistration.Value.SystemHash == mapping.SystemHash)
            {
                return iRpcProxyRegistration.Key;
            }
        }
        
        throw new InvalidOperationException($"No system mapping found for {mapping.SystemHash}");
    }

    public void LogError(Exception exception, QuicStream stream, List<SystemMappings> systemMapping)
    {
        try
        {
            var mapping = ResolveStreamSystem(stream, systemMapping);
            _logger.LogError(exception, "Error in stream for system {SystemType}", mapping);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving system for stream");
            _logger.LogError(exception, "Original error in unknown stream");
        }
    }
}

public record ProxyRegistrationForImple(string SystemHash, Type SystemType, Type ProxyType) : IRpcProxyRegistration;