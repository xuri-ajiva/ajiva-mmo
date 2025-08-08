
using Microsoft.CodeAnalysis;

namespace AjivaGenerator.Network;

/// <summary>
/// Diagnostic rules for the network packet generator
/// </summary>
public static class NetworkGeneratorRules
{
    public static readonly DiagnosticDescriptor MustBeStructRule = new(
        "NET001",
        "Network packet must be a struct",
        "Type '{0}' marked with [NetworkPacket] must be a struct",
        "NetworkPacket",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnknownTypeRule = new(
        "NET002", 
        "Unknown type for network serialization",
        "Type '{0}' is not supported for network serialization",
        "NetworkPacket",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
