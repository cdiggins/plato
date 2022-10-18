using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PlatoGenerator
{
    // https://www.meziantou.net/writing-a-roslyn-analyzer.htm
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PlatoAnalyzer : DiagnosticAnalyzer
    {
        // Metadata of the analyzer
        public const string DiagnosticId = "PlatoAnalyzer";

        // You could use LocalizedString but it's a little more complicated for this sample
        private static readonly string Title = "Nonplatonic code";
        private static readonly string MessageFormat = "Code is not platonic";
        private static readonly string Description = "Make sure code is referentially tranparent.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, 
            DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        // Register the list of rules this DiagnosticAnalizer supports
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationAction(Action);
            context.RegisterSemanticModelAction(Action);
            context.RegisterSyntaxNodeAction(Action, SyntaxKind.ReturnStatement);
            context.RegisterSymbolAction(Action, SymbolKind.Label);
        }

        private static void Action(SymbolAnalysisContext obj)
        {
        }

        private static void Action(SyntaxNodeAnalysisContext obj)
        {
        }

        private static void Action(SemanticModelAnalysisContext obj)
        {
        }

        private static void Action(CompilationAnalysisContext obj)
        {
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    }
}