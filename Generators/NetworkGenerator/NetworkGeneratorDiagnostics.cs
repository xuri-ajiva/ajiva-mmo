using Microsoft.CodeAnalysis;

namespace NetworkGenerator;

public static class NetworkGeneratorRules
{
    private const string Category = "NetworkGenerator";

    public static readonly DiagnosticDescriptor UnknownTypeRule = new(
        id: "NET001",
        title: "Unknown Type",
        messageFormat: "The type '{0}' is not a known type and does not implement IMessageSerializable. The generator cannot serialize or deserialize this type.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    public static readonly DiagnosticDescriptor MustBeStructRule = new(
        id: "NET002",
        title: "Must be a struct",
        messageFormat: "The type '{0}' must be a struct",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

}
