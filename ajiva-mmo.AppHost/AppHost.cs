using System.Net.Sockets;
using System.Reflection.Metadata;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddDockerComposeEnvironment("compose");


var clientConfig = builder.Configuration.GetSection("ClientConfiguration");
var clients = clientConfig.GetValue("Count", 1);
var clusterConf = builder.Configuration.GetSection("ClusterConfiguration");

var entryPort = clusterConf.GetValue("EntryPort", 3300);
var managementPort = clusterConf.GetValue("ManagementPort", 3310);
var defaultServerInstances = clusterConf.GetValue("DefaultInstanceCount", 1);

var clusterManager = builder.AddProject<Projects.ClusterManager>("clusterManager")
    .WithArgs("--ClusterConfiguration:EntryPort", entryPort.ToString(),
        "--ClusterConfiguration:ManagementPort", managementPort.ToString())
    .WithEndpoint(port: managementPort - 1, targetPort: managementPort, protocol: ProtocolType.Udp, name: "management")
    .WithEndpoint(port: entryPort - 1, targetPort: entryPort, protocol: ProtocolType.Udp, name: "clients")
    .WithReplicas(1);


var server = builder.AddProject<Projects.Server>("server")
    .WaitFor(clusterManager)
    .WithReference(clusterManager)
    .WithReplicas(defaultServerInstances)
    .WithArgs("--ServerConfiguration:ManagementPort", managementPort.ToString(),
        "--ServerConfiguration:ClusterManagerAddress", clusterManager.GetEndpoint("management"));

builder.AddProject<Projects.Client>("client")
    .WaitFor(clusterManager)
    .WithReplicas(clients)
    .WithArgs("--ClientConfiguration:AutoConnect=true", "--ClientConfiguration:AutoConnectAddress",
        clusterManager.GetEndpoint("clients"));


builder.Build().Run();