
using Microsoft.CodeAnalysis;

namespace AjivaGenerator.Network;

/// <summary>
/// Diagnostic rules for the network packet generator
/// </summary>
public static class NetworkGeneratorRules
{

    public static readonly DiagnosticDescriptor MustBeRecordStructRule = new(
        "NET001",
        "Network packet must be a struct",
        "Type '{0}' marked with [NetworkPacket] must be a record struct",
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
    
    public static readonly DiagnosticDescriptor UnsupportedFieldTypeRule = new(
        "NET003", 
        "Unsupported field type for network serialization",
        "Field '{0}' in type '{1}' is of unsupported type '{2}' for network serialization",
        "NetworkPacket",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static readonly DiagnosticDescriptor InvalidAttributeRule = new(
        "NET004", 
        "Invalid NetworkPacket attribute usage",
        "The NetworkPacket attribute on type '{0}' is invalid: {1}",
        "NetworkPacket",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
}
