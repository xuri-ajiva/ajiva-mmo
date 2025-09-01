using System.Net;
using System.Net.Quic;
using System.Net.Security;
using Common.Networking.AjivaQuic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace Common;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const ushort DefaultServerPort = 31313;
    private const string ApplicationProtocol = "ajiva-quic";
    private static List<SslApplicationProtocol> _applicationProtocols = [new(ApplicationProtocol)];

    public static IHostApplicationBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddServiceDiscovery();
        builder.Services.AddSerilog();

        builder.ConfigureOpenTelemetry();
        return builder;
    }

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

    public static IHostApplicationBuilder AddQuickServerOptions<TBuilder>(this TBuilder builder,
        IPEndPoint? endPoint = null)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddSingleton(provider => QuickServerFactory(provider, endPoint));
        builder.Services.AddSingleton(provider =>
        {
            var serverOptions = DefaultQuicConnectionOptions<QuicServerConnectionOptions>();
            serverOptions.ServerAuthenticationOptions = new SslServerAuthenticationOptions
            {
                ApplicationProtocols = _applicationProtocols,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                ServerCertificate = CertificateHelper.GenerateManualCertificate(),
                ClientCertificateRequired = false,
                AllowTlsResume = true,
                AllowRenegotiation = false,
            };
            return serverOptions;
        });

        return builder;
    }

    public static IHostApplicationBuilder AddQuickClientOptions<TBuilder>(this TBuilder builder,
        IPEndPoint? endPoint = null)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddSingleton(provider =>
        {
            var clientOptions = DefaultQuicConnectionOptions<QuicClientConnectionOptions>();
            if (endPoint is not null)
                clientOptions.RemoteEndPoint = endPoint;
            clientOptions.ClientAuthenticationOptions = new SslClientAuthenticationOptions
            {
                ApplicationProtocols = _applicationProtocols,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                TargetHost = endPoint?.Address.ToString(),
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls13,
            };
            return clientOptions;
        });
        return builder;
    }


    static QuicListenerOptions QuickServerFactory(IServiceProvider provider, IPEndPoint? endPoint)
    {
        var options = provider.GetRequiredService<QuicServerConnectionOptions>();
        return new QuicListenerOptions
        {
            ListenEndPoint = endPoint ?? new IPEndPoint(IPAddress.IPv6Any, DefaultServerPort),
            ListenBacklog = 100,
            ConnectionOptionsCallback = (connection, info, token) => ValueTask.FromResult(options),
            ApplicationProtocols = _applicationProtocols
        };
    }


    public static IHostApplicationBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var serviceName = builder.Environment.ApplicationName + ".Service";
        var serviceVersion = "1.0.0";

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion));
        });

        var instrumentation = new Instrumentation(serviceName, serviceVersion);
        builder.Services.AddSingleton(instrumentation);
        builder.Services.AddSingleton(_ => instrumentation.ActivitySource);
        builder.Services.AddSingleton(_ => instrumentation.Meter);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(serviceName)
                    .AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                    {
                        // Exclude health check requests from tracing
                        // tracing.Filter = context => !context.Request.Path.StartsWithSegments(HealthEndpointPath) && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    })
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            })
            .WithLogging();

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }
}
