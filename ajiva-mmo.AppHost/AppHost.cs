using System.Net.Sockets;
using System.Reflection.Metadata;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddDockerComposeEnvironment("compose");


var clientConfig = builder.Configuration.GetSection("ClientConfiguration");
var desktopClients = clientConfig.GetValue("Desktop", 1);
var editorClients = clientConfig.GetValue("Editor", 1);
var webClients = clientConfig.GetValue("Web", 1);
var clusterConf = builder.Configuration.GetSection("ClusterConfiguration");

var entryPort = clusterConf.GetValue("EntryPort", 3300);
var managementPort = clusterConf.GetValue("ManagementPort", 3310);
var defaultServerInstances = clusterConf.GetValue("DefaultInstanceCount", 1);

var clusterManager = builder.AddProject<Projects.ClusterManager>("clusterManager")
    .WithArgs(
        "--ClusterConfiguration:EntryPort", entryPort.ToString(),
        "--ClusterConfiguration:ManagementPort", managementPort.ToString()
    )
    .WithEndpoint(port: managementPort - 1, targetPort: managementPort, protocol: ProtocolType.Udp, name: "management")
    .WithEndpoint(port: entryPort - 1, targetPort: entryPort, protocol: ProtocolType.Udp, name: "clients")
    .WithReplicas(1);


builder.AddProject<Projects.Server>("server")
    .WaitFor(clusterManager)
    .WithReference(clusterManager)
    .WithReplicas(defaultServerInstances)
    .WithArgs("--ServerConfiguration:ManagementPort", managementPort.ToString(),
        "--ServerConfiguration:ClusterManagerAddress", clusterManager.GetEndpoint("management"));

if (desktopClients > 0)
    builder.AddProject<Projects.Client_Desktop>("desktopClient")
        .WaitFor(clusterManager)
        .WithReplicas(desktopClients)
        .WithArgs("--ClientConfiguration:AutoConnect=true",
            "--ClientConfiguration:AutoConnectAddress", clusterManager.GetEndpoint("clients")
        );

if (editorClients > 0)
    builder.AddProject<Projects.Client_Editor>("client")
        .WaitFor(clusterManager)
        .WithReplicas(editorClients)
        .WithArgs("--ClientConfiguration:AutoConnect=true",
            "--ClientConfiguration:AutoConnectAddress", clusterManager.GetEndpoint("clients"),
            "--ClientConfiguration:ClusterManagerAddress", clusterManager.GetEndpoint("management")
        );

if (webClients > 0)
    builder.AddProject<Projects.Client_Web>("webClient")
        .WaitFor(clusterManager)
        .WithReplicas(webClients)
        .WithArgs("--ClientConfiguration:AutoConnect=true",
            "--ClientConfiguration:AutoConnectAddress", clusterManager.GetEndpoint("clients")
        );


builder.Build().Run();