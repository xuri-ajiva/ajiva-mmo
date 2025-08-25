using System.Net.Sockets;
using System.Reflection.Metadata;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var clientConfig = builder.Configuration.GetSection("ClientConfiguration");
var clients = clientConfig.GetValue("Count", 1);
var serverConfig = builder.Configuration.GetSection("ServerConfiguration");
if (!int.TryParse(serverConfig["port"], out var serverPort))
{
    serverPort = 7777;
}

var server = builder.AddProject<Projects.Server>("server")
        .WithArgs("--ServerConfiguration:Port", serverPort.ToString(), "--ServerConfiguration:MaxConnections",
            clients * 2)
        .WithReplicas(1)
        .WithEndpoint(port: serverPort - 1, targetPort: serverPort, protocol: ProtocolType.Udp, name: "server")
    ;

builder.AddProject<Projects.Client>("client")
    .WaitFor(server)
    .WithReplicas(clients)
    .WithArgs("--ClientConfiguration:AutoConnect=true", "--ClientConfiguration:AutoConnectAddress", server.GetEndpoint("server"));


builder.Build().Run();
