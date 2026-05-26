using Microsoft.CodeAnalysis;

namespace AjivaGenerator.Network;

/// <summary>
/// Diagnostic rules for the RPC source generator
/// </summary>
public static class RpcGeneratorRules
{
    public static readonly DiagnosticDescriptor MustImplementIRpcSystemRule = new(
        "RPC001",
        "Interface must implement IRpcSystem",
        "Type '{0}' marked with [RpcSystem] must implement IRpcSystem interface",
        "RpcSystem",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedParameterTypeRule = new(
        "RPC002",
        "Unsupported parameter type for RPC method",
        "Method '{0}' has parameter '{1}' with unsupported type '{2}'",
        "RpcSystem",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidReturnTypeRule = new(
        "RPC003",
        "Invalid return type for RPC method",
        "Method '{0}' has invalid return type '{1}'",
        "RpcSystem",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor OptionalIRpcContextWarning = new(
        "RPC004",
        "IRpcContext parameter usage",
        "Method '{0}' includes IRpcContext parameter which will not be serialized over the network",
        "RpcSystem",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoMethodsWarning = new(
        "RPC005",
        "RPC system has no methods",
        "RPC system '{0}' has no methods defined",
        "RpcSystem",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnexpectedAttributeError = new(
        "RPC006",
        "Unexpected attribute format",
        "The [RpcSystem] attribute on type '{0}' is invalid: {1}",
        "RpcSystem",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

