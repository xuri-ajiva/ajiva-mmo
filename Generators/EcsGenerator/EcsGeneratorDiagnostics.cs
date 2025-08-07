using Microsoft.CodeAnalysis;

namespace EcsGenerator;

public static class EcsGeneratorRules
{
    private const string Category = "EcsGenerator";

    public static readonly DiagnosticDescriptor ConflictingFlagsRule = new(
        id: "ECS001",
        title: "Conflicting component flags",
        messageFormat: "Component '{0}' has conflicting flags: Dense and Sparse cannot be used together",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MustBeStructRule = new(
        id: "ECS002", 
        title: "Must be a struct",
        messageFormat: "ECS component '{0}' must be a struct",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MustImplementIComponentRule = new(
        id: "ECS003",
        title: "Must implement IComponent",
        messageFormat: "ECS component '{0}' must implement IComponent interface",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidFlagCombinationRule = new(
        id: "ECS004",
        title: "Invalid flag combination",
        messageFormat: "Component '{0}' has invalid flag combination: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoStorageFlagsRule = new(
        id: "ECS005",
        title: "No storage flags specified",
        messageFormat: "Component '{0}' must specify either Dense or Sparse storage flag",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConflictingSyncFlagsRule = new(
        id: "ECS006",
        title: "Conflicting sync flags",
        messageFormat: "Component '{0}' has conflicting sync flags: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MustBePartialRule = new(
        id: "ECS007",
        title: "Must be partial",
        messageFormat: "ECS component '{0}' must be declared as partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
