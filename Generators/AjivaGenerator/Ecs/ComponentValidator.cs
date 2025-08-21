using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AjivaGenerator.Ecs;

/// <summary>
/// Validates ECS components and reports diagnostics
/// </summary>
internal static class ComponentValidator
{
    public static (bool isValid, ImmutableArray<Diagnostic> diagnostics) ValidateComponent(
        INamedTypeSymbol structSymbol,
        StructDeclarationSyntax structSyntax)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        bool isValid = true;

        // Must be a struct
        if (structSymbol.TypeKind != TypeKind.Struct)
        {
            diagnostics.Add(Diagnostic.Create(
                EcsGeneratorRules.MustBeStructRule,
                structSyntax.GetLocation(),
                structSymbol.Name));
            isValid = false;
        }

        // Must be partial
        if (!structSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            diagnostics.Add(Diagnostic.Create(
                EcsGeneratorRules.MustBePartialRule,
                structSyntax.GetLocation(),
                structSymbol.Name));
            isValid = false;
        }

        // Must implement IComponent
        var implementsIComponent = structSymbol.AllInterfaces.Any(i => i.Name == "IComponent");

        if (!implementsIComponent)
        {
            diagnostics.Add(Diagnostic.Create(
                EcsGeneratorRules.MustImplementIComponentRule,
                structSyntax.GetLocation(),
                structSymbol.Name));
            isValid = false;
        }

        return (isValid, diagnostics.ToImmutable());
    }

    public static (bool isValid, ImmutableArray<Diagnostic> diagnostics) ValidateComponentFlags(
        ComponentInfo component,
        Location location)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        bool isValid = true;

        // Must have either Dense or Sparse
        bool hasDense = component.Flags.HasFlag(ComponentFlags.Dense);
        bool hasSparse = component.Flags.HasFlag(ComponentFlags.Sparse);

        if (hasDense && hasSparse)
        {
            diagnostics.Add(Diagnostic.Create(
                EcsGeneratorRules.ConflictingFlagsRule,
                location,
                component.Name));
            isValid = false;
        }

        if (!hasDense && !hasSparse)
        {
            diagnostics.Add(Diagnostic.Create(
                EcsGeneratorRules.NoStorageFlagsRule,
                location,
                component.Name));
            isValid = false;
        }

        // Validate sync combinations
        if (component.Sync != ComponentSync.None)
        {
            bool hasReliable = component.Flags.HasFlag(ComponentFlags.ReliableSync);
            bool hasUnreliable = component.Flags.HasFlag(ComponentFlags.UnreliableSync);

            if (hasReliable && hasUnreliable)
            {
                diagnostics.Add(Diagnostic.Create(
                    EcsGeneratorRules.ConflictingSyncFlagsRule,
                    location,
                    component.Name,
                    "ReliableSync and UnreliableSync"));
            }
        }

        return (isValid, diagnostics.ToImmutable());
    }
}
