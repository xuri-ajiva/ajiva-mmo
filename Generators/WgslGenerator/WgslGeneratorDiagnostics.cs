using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace WgslGenerator
{
    public static class WgslGeneratorRules
    {
        public const string MismatchedShadersId = "WGSL001";
        public const string MultipleFragmentOutputsId = "WGSL002";

        private const string Category = "WgslGenerator";

        public static readonly DiagnosticDescriptor MismatchedShadersRule = new(
            id: MismatchedShadersId,
            title: "Mismatched Shaders",
            messageFormat: "Fragment shader does not use vertex shader output",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MultipleFragmentOutputsRule = new(
            id: MultipleFragmentOutputsId,
            title: "Multiple Fragment Outputs",
            messageFormat: "Fragment shader has multiple output structs, using '{0}' as output",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static ImmutableArray<DiagnosticDescriptor> AllRules { get; } =
        [
            MismatchedShadersRule,
            MultipleFragmentOutputsRule
        ];
    }
}
