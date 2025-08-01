/*
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace WgslGenerator
{
    //[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class WgslAnalyzer : DiagnosticAnalyzer
    {
        private const string AttributeName = "GenerateShader";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => WgslGeneratorRules.AllRules;

        public override void Initialize(AnalysisContext context)
        {
            //Debugger.Launch();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            if (classDeclaration.AttributeLists.SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains(AttributeName)))
            {
                var visitor = new WgslVisitor(context.Compilation, classDeclaration, VisitorMode.Analyze);
                visitor.Visit(classDeclaration);

                foreach (var diagnostic in visitor.GetDiagnostics())
                {
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
*/
