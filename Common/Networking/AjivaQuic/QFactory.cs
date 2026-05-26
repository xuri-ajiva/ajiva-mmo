using System.Net;
using System.Net.Quic;
using System.Net.Security;
using Common.Networking.Rpc;
using Common.Rpc;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Networking.AjivaQuic;

public class QFactory(IServiceProvider provider)
{
    private const u16 DefaultServerPort = 0;
    private const string ApplicationProtocol = "ajiva-quic";
    private static readonly List<SslApplicationProtocol> ApplicationProtocols = [new(ApplicationProtocol)];


    private static TOptions DefaultQuicConnectionOptions<TOptions>()
        where TOptions : QuicConnectionOptions, new() => new()
    {
        DefaultCloseErrorCode = 0,
        DefaultStreamErrorCode = 0,
        IdleTimeout = TimeSpan.FromHours(1),
        HandshakeTimeout = TimeSpan.FromMinutes(1),
        MaxInboundBidirectionalStreams = 8,
        MaxInboundUnidirectionalStreams = 8,
        KeepAliveInterval = TimeSpan.FromMinutes(1)
    };

    public QServer CreateServer(i32 port) => CreateServer(new IPEndPoint(IPAddress.Any, port));

    public QServer CreateServer(IPEndPoint? iPEndPoint)
    {
        return new QServer(QuickServerFactory(provider, iPEndPoint), provider,
            provider.GetRequiredService<ILogger<QServer>>());
    }

    public QClient CreateClient(IPEndPoint? endPoint = null)
    {
        return new QClient(
            provider.GetService<QuicClientConnectionOptions>() ?? MakeClientOptions(endPoint),
            provider.GetRequiredService<RpcSystemRegistry>(),
            provider.GetRequiredService<ILogger<QClient>>()
        );
    }

    private static QuicServerConnectionOptions MakeServerOptions()
    {
        var serverOptions = DefaultQuicConnectionOptions<QuicServerConnectionOptions>();
        serverOptions.ServerAuthenticationOptions = new SslServerAuthenticationOptions
        {
            ApplicationProtocols = ApplicationProtocols,
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
            ServerCertificate = CertificateHelper.GenerateManualCertificate(),
            ClientCertificateRequired = false,
            AllowTlsResume = true,
            AllowRenegotiation = false,
        };
        return serverOptions;
    }

    private static QuicClientConnectionOptions MakeClientOptions(IPEndPoint? endPoint)
    {
        var clientOptions = DefaultQuicConnectionOptions<QuicClientConnectionOptions>();
        if (endPoint is not null)
            clientOptions.RemoteEndPoint = endPoint;
        clientOptions.ClientAuthenticationOptions = new SslClientAuthenticationOptions
        {
            ApplicationProtocols = ApplicationProtocols,
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
            TargetHost = endPoint?.Address.ToString(),
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls13,
        };
        return clientOptions;
    }


    static QuicListenerOptions QuickServerFactory(IServiceProvider provider, IPEndPoint? endPoint)
    {
        var options = provider.GetService<QuicServerConnectionOptions>() ?? MakeServerOptions();
        return new QuicListenerOptions
        {
            ListenEndPoint = endPoint ?? new IPEndPoint(IPAddress.IPv6Any, DefaultServerPort),
            ListenBacklog = 100,
            ConnectionOptionsCallback = (connection, info, token) => ValueTask.FromResult(options),
            ApplicationProtocols = ApplicationProtocols
        };
    }
}