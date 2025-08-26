using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Common.Config;
using Common.Networking;
using Server;

namespace Server;

public class ServerRunner
{
    public ServerRunner(IHostApplicationBuilder builder, ServerConfiguration config)
    {
        var services = builder.Services;


        var authenticationOptions = new SslServerAuthenticationOptions();
        authenticationOptions.ApplicationProtocols ??= [new SslApplicationProtocol("ice-rpc")];

        services.AddSingleton(config);
        services.AddSingleton(new QuicListenerOptions
        {
            ListenEndPoint = new IPEndPoint(IPAddress.IPv6Any, config.Port),
            ListenBacklog = config.MaxConnections,
            ConnectionOptionsCallback = ConnectionOptionsCallback,
            ApplicationProtocols = authenticationOptions.ApplicationProtocols
        });

        services.AddHostedService<ServerWorker>();
        services.AddSingleton<QServer>();
        services.AddSingleton<ServerGameLogic>();
    }

    private async ValueTask<QuicServerConnectionOptions> ConnectionOptionsCallback(QuicConnection arg1,
        SslClientHelloInfo arg2, CancellationToken arg3)
    {
        return new QuicServerConnectionOptions
        {
            ServerAuthenticationOptions = new SslServerAuthenticationOptions
            {
                ApplicationProtocols = [new SslApplicationProtocol("ice-rpc")],
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                ServerCertificate = GenerateManualCertificate(),
                ClientCertificateRequired = false,
            },
            DefaultCloseErrorCode = 233241,
            DefaultStreamErrorCode = 4332421,
            IdleTimeout = TimeSpan.FromHours(1),
            HandshakeTimeout = TimeSpan.FromMinutes(1),
            MaxInboundBidirectionalStreams = 99,
            MaxInboundUnidirectionalStreams = 99,
        };
    }

    X509Certificate2 GenerateManualCertificate()
    {
        X509Certificate2? cert = null;
        var store = new X509Store("KestrelWebTransportCertificates", StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        if (store.Certificates.Count > 0)
        {
            cert = store.Certificates[^1];

            // rotate key after it expires
            if (!cert.HasPrivateKey || DateTime.Parse(cert.GetExpirationDateString(), null) < DateTimeOffset.UtcNow)
            {
                cert = null;
            }
        }

        if (cert == null)
        {
            // generate a new cert
            var now = DateTimeOffset.UtcNow;
            SubjectAlternativeNameBuilder sanBuilder = new();
            sanBuilder.AddDnsName("localhost");
            using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            CertificateRequest req = new("CN=localhost", ec, HashAlgorithmName.SHA256);
            // Adds purpose
            req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
            {
                new Oid("1.3.6.1.5.5.7.3.1") // serverAuth
            }, false));
            // Adds usage
            req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
            // Adds subject alternate names
            req.CertificateExtensions.Add(sanBuilder.Build());
            // Sign
            using var
                crt = req.CreateSelfSigned(now,
                    now.AddDays(14)); // 14 days is the max duration of a certificate for this
            // Save
            cert = X509CertificateLoader.LoadPkcs12(crt.Export(X509ContentType.Pkcs12), null,
                X509KeyStorageFlags.Exportable);
            store.Add(cert);
        }

        store.Close();
        Debug.Assert(cert.HasPrivateKey);
        return cert;
    }

    public async Task RunAsync(IHost app, CancellationToken token = default)
    {
        await app.RunAsync(token: token);
    }
}
