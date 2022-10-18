using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoAnalyzer
{
    //[Generator]
    public class PlatoSourceGenerator : ISourceGenerator, ISyntaxContextReceiver
    {
        public PlatoSemanticMapping Mapping = new PlatoSemanticMapping();
        
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register the attribute source
            context.RegisterForPostInitialization(PostInitialization);

            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => this);
        }

        public GeneratorExecutionContext ExecutionContext { get; private set; }

        // TODO: I need probably to have a bunch of these 
        public DiagnosticDescriptor PlatoError { get; } = new DiagnosticDescriptor("PLATO001", "Error", "Exception occurred: {0}", "Plato", DiagnosticSeverity.Error, true);

        public void Execute(GeneratorExecutionContext context)
        {
            Console.WriteLine("Executing");
            ExecutionContext = context;
            //OutputSyntaxNodes();
        }

        public void PostInitialization(GeneratorPostInitializationContext context)
        {
            Console.WriteLine("Post initialization");
            //OutputSyntaxNodes();
        }

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            Mapping.Model = context.SemanticModel;

            if (context.Node is ClassDeclarationSyntax classDecl)
            {
                try
                {
                    Mapping.Add(() => classDecl.ToPlato(Mapping), classDecl);
                }
                catch (Exception e)
                {
                    ExecutionContext.ReportDiagnostic(Diagnostic.Create(PlatoError, context.Node.GetLocation(), e.Message));
                }
            }
        }
    }
}