using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace WgslGenerator
{
    public static class WgslGeneratorRules
    {
        private const string Category = "WgslGenerator";

        public static readonly DiagnosticDescriptor MismatchedShadersRule = new(
            id: "WGSL001",
            title: "Mismatched Shaders",
            messageFormat: "Fragment shader does not use vertex shader output",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MultipleFragmentOutputsRule = new(
            id: "WGSL002",
            title: "Multiple Fragment Outputs",
            messageFormat: "Fragment shader has multiple output structs, using '{0}' as output",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnsupportedSyntaxNodeRule = new(
            id: "WGSL003",
            title: "Unsupported Syntax Node",
            messageFormat: "The C# syntax node '{0}' is not supported for WGSL generation",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NestedClassesNotAllowedRule = new(
            id: "WGSL004",
            title: "Nested Classes Not Allowed",
            messageFormat: "Nested classes are not supported in WGSL. Please use structs instead.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnsupportedTextureFormatRule = new(
            id: "WGSL005",
            title: "Unsupported Texture Format",
            messageFormat: "Unsupported texture type '{0}'. Cannot determine WGSL texture format.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingAttributeArgumentRule = new(
            id: "WGSL006",
            title: "Missing Attribute Argument",
            messageFormat: "The attribute '{0}' requires an argument",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnknownBindingTypeRule = new(
            id: "WGSL007",
            title: "Unknown Binding Type",
            messageFormat: "Unknown binding type '{0}'",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidSamplerBindingModeRule = new(
            id: "WGSL008",
            title: "Invalid Sampler Binding Mode",
            messageFormat: "Sampler must have filtering, non-filtering, or comparison access",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnexpectedAttributeArgumentSyntaxRule = new(
            id: "WGSL009",
            title: "Unexpected Attribute Argument Syntax",
            messageFormat: "Unexpected syntax '{0}' for attribute argument",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UndefinedBindingTypeRule = new(
            id: "WGSL010",
            title: "Undefined Binding Type",
            messageFormat: "Binding type must be defined for field '{0}'",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidBindingTypeEnumValueRule = new(
            id: "WGSL011",
            title: "Invalid Binding Type Enum Value",
            messageFormat: "Invalid BindingType enum value encountered for field '{0}'",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnsupportedAttributeRule = new(
            id: "WGSL012",
            title: "Unsupported Attribute",
            messageFormat: "The attribute '{0}' is not implemented",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnknownAttributeNameRule = new(
            id: "WGSL013",
            title: "Unknown Attribute Name",
            messageFormat: "The attribute name '{0}' is not recognized",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnsupportedInvocationExpressionRule = new(
            id: "WGSL014",
            title: "Unsupported Invocation Expression",
            messageFormat: "Only generic and identifier names are supported for invocation expressions. Found '{0}'.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MethodNameCasingChangedRule = new(
            id: "WGSL015",
            title: "Method Name Casing Changed",
            messageFormat: "Method name '{0}' was changed to '{1}' to match WGSL conventions",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnknownMethodCallRule = new(
            id: "WGSL016",
            title: "Unknown Method Call",
            messageFormat: "Unknown method '{0}'",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor IncompleteVariableDeclarationHandlingRule = new(
            id: "WGSL017",
            title: "Incomplete Variable Declaration Handling",
            messageFormat: "Variable declaration handling is incomplete and may produce incorrect WGSL",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
        
        public static readonly DiagnosticDescriptor MethodCallNotValidatedRule = new(
            id: "WGSL018",
            title: "Math Method Call",
            messageFormat: "Method call '{0}' was not validated as it is on known type '{1}', but still may not be supported in WGSL",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor EmptyReturnStatementRule = new(
            id: "WGSL019",
            title: "Empty Return Statement",
            messageFormat: "Empty return did you intend discard;? use discard(); instead",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
