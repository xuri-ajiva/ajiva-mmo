using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AjivaGenerator.Ecs;

[Generator]
public class EcsSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate the ECS attributes first
        context.RegisterPostInitializationOutput(EcsAttributesGenerator.GenerateAttributes);

        // Find all structs with EcsComponent attribute
        var componentProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is StructDeclarationSyntax,
                transform: (ctx, _) => GetComponentInfo(ctx))
            .Where(info => info.componentInfo != null || !info.diagnostics.IsEmpty);

        // Generate all ECS artifacts when components are found
        context.RegisterSourceOutput(context.CompilationProvider.Combine(componentProvider.Collect()),
            (ctx, t) => GenerateAllEcsArtifacts(ctx, t.Left, t.Right));
    }

    private static (ComponentInfo? componentInfo, ImmutableArray<Diagnostic> diagnostics) GetComponentInfo(
        GeneratorSyntaxContext context)
    {
        var structSyntax = (StructDeclarationSyntax)context.Node;
        var structSymbol = context.SemanticModel.GetDeclaredSymbol(structSyntax);

        if (structSymbol == null)
            return (null, ImmutableArray<Diagnostic>.Empty);

        // Look for EcsComponent attribute
        var ecsAttribute = structSymbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == "EcsComponentAttribute" &&
                                    attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "Ajiva.Ecs");

        if (ecsAttribute == null)
            return (null, ImmutableArray<Diagnostic>.Empty);

        // Validate the component
        var validation = ComponentValidator.ValidateComponent(structSymbol, structSyntax);
        if (!validation.isValid)
        {
            return (null, validation.diagnostics);
        }

        // Extract attribute parameters
        var scope = GetEnumValue<ComponentScope>(ecsAttribute, 0);
        var sync = GetEnumValue<ComponentSync>(ecsAttribute, 1);
        var flags = GetEnumValue<ComponentFlags>(ecsAttribute, 2);

        // Get field information
        var fields = structSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => !f.IsStatic)
            .Select(f => new FieldInfo(f.Name, f.Type.ToDisplayString()))
            .ToImmutableArray();

        var componentInfo = new ComponentInfo(
            structSymbol.Name,
            structSymbol.ContainingNamespace?.ToDisplayString() ?? "Global",
            scope,
            sync,
            flags,
            fields,
            structSyntax,
            structSymbol
        );

        return (componentInfo, validation.diagnostics);
    }

    private static T GetEnumValue<T>(AttributeData attribute, int parameterIndex) where T : struct, System.Enum
    {
        if (attribute.ConstructorArguments.Length > parameterIndex)
        {
            var value = attribute.ConstructorArguments[parameterIndex].Value;
            if (value != null)
            {
                return (T)System.Enum.ToObject(typeof(T), value);
            }
        }

        return default;
    }

    private static void GenerateAllEcsArtifacts(
        SourceProductionContext context, Compilation compilation,
        ImmutableArray<(ComponentInfo? componentInfo, ImmutableArray<Diagnostic> diagnostics)> results)
    {
        // Report all diagnostics
        foreach (var (_, diagnostics) in results)
        {
            foreach (var diagnostic in diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }
        }

        // Collect valid components
        var components = results
            .Where(r => r.componentInfo != null)
            .Select(r => r.componentInfo!)
            .ToImmutableArray();

        if (components.IsEmpty)
            return;

        // Filter components for server and client worlds
        var serverComponents = components
            .Where(c => c.Scope == ComponentScope.Shared || c.Scope == ComponentScope.Server)
            .ToImmutableArray();
        var clientComponents = components
            .Where(c => c.Scope == ComponentScope.Shared || c.Scope == ComponentScope.Client)
            .ToImmutableArray();

        // Generate ServerEcsWorld and ClientEcsWorld
        EcsWorldGenerator.GenerateEcsWorld(context, serverComponents, "ServerEcsWorld");
        EcsWorldGenerator.GenerateEcsWorld(context, clientComponents, "ClientEcsWorld");

        // Optionally, keep the original EcsWorld for shared/testing purposes
        // EcsWorldGenerator.GenerateEcsWorld(context, components, "EcsWorld");

        // Generate EntityBuilder class
        //EntityBuilderGenerator.GenerateEntityBuilder(context, components);

        /*// Generate network packets for components with sync flags
        foreach (var componentInfo in components)
        {
            context.AddSource(componentInfo.Name + "NetworkPacket.g.cs",
                SourceText.From(NetworkPacketsSourceGenerator.GenerateIMessageSerializableInterface(
                    context, compilation.GetSemanticModel(componentInfo.StructSyntax.SyntaxTree),
                    componentInfo.StructSymbol,
                    componentInfo.StructSyntax,
                    componentInfo.StructSymbol.ContainingNamespace?.ToDisplayString()!,
                    componentInfo.Name,
                    out var fields), Encoding.UTF8));
        }*/
    }
}
