using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace PlatoGenerator
{
    public static class Helpers
    {
        public static IEnumerable<ISymbol> GetSymbolsOfSyntax<T>(this SemanticModel model)
            where T : SyntaxNode
            => model.SyntaxTree.GetRoot().DescendantNodes().OfType<T>()
                .Select(node => model.GetDeclaredSymbol(node));

        public static IEnumerable<ISymbol> GetInterfaceSymbols(this SemanticModel model)
            => model.GetSymbolsOfSyntax<InterfaceDeclarationSyntax>();
    }


    // TODO:
    // Get a list of all of the interfaces in the source project. 
    // Look for an attribute named "Implements" ...
    // Start creating some classes. 
    [Generator]
    public class PlatoGenerator : ISourceGenerator
    {
        public void ProcessSymbol(ISymbol symbol)
        {
            switch (symbol)
            {
                case IAliasSymbol aliasSymbol:
                    break;
                case IArrayTypeSymbol arrayTypeSymbol:
                    break;
                case ISourceAssemblySymbol sourceAssemblySymbol:
                    break;
                case IAssemblySymbol assemblySymbol:
                    break;
                case IDiscardSymbol discardSymbol:
                    break;
                case IDynamicTypeSymbol dynamicTypeSymbol:
                    break;
                case IErrorTypeSymbol errorTypeSymbol:
                    break;
                case IEventSymbol eventSymbol:
                    break;
                case IFieldSymbol fieldSymbol:
                    break;
                case IFunctionPointerTypeSymbol functionPointerTypeSymbol:
                    break;
                case ILabelSymbol labelSymbol:
                    break;
                case ILocalSymbol localSymbol:
                    break;
                case IMethodSymbol methodSymbol:
                    break;
                case IModuleSymbol moduleSymbol:
                    break;
                case INamedTypeSymbol namedTypeSymbol:
                    break;
                case INamespaceSymbol namespaceSymbol:
                    break;
                case IPointerTypeSymbol pointerTypeSymbol:
                    break;
                case ITypeParameterSymbol typeParameterSymbol:
                    break;
                case ITypeSymbol typeSymbol:
                    break;
                case INamespaceOrTypeSymbol namespaceOrTypeSymbol:
                    break;
                case IParameterSymbol parameterSymbol:
                    break;
                case IPreprocessingSymbol preprocessingSymbol:
                    break;
                case IPropertySymbol propertySymbol:
                    break;
                case IRangeVariableSymbol rangeVariableSymbol:
                    break;
            }
        }

        public string ProcessTypeSymbol(INamedTypeSymbol symbol)
        {
            var names = string.Join(", ", symbol.MemberNames);
            return $"{symbol.Name} {{ {names} }}";
        }

        public void Execute(GeneratorExecutionContext context)
        {
            Debug.WriteLine("Execute code generator"); 
            Debugger.Break();
            Console.WriteLine("I am executing!");

            
            // using the context, get a list of syntax trees in the users compilation
            var syntaxTrees = context.Compilation.SyntaxTrees;
            var models = syntaxTrees.Select(tree => context.Compilation.GetSemanticModel(tree));
            
            /*
            // add the filepath of each tree to the class we're building
            foreach (var tree in syntaxTrees)
            {
                sourceBuilder.AppendLine($@"Console.WriteLine(@"" - {tree.FilePath}"");");
            }
            */

            // begin creating the source we'll inject into the users compilation
            var sourceBuilder = new StringBuilder(@"
using System;
namespace HelloWorldGenerated
{
    public static class HelloWorld
    {
        public static void SayHello() 
        {
            Console.WriteLine(""Hello from generated code4!"");
            Console.WriteLine(""The following syntax trees existed in the compilation that created this program:"");
");

            foreach (var sym in models.SelectMany(model => model.GetInterfaceSymbols()))
            {
                sourceBuilder.AppendLine($"Console.WriteLine(\"{ProcessTypeSymbol(sym as INamedTypeSymbol)}\");");
            }


            // finish creating the source to inject
            sourceBuilder.Append(@"
        }
    }
}");

            // inject the created source into the users compilation
            context.AddSource("helloWorldGenerator", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            Debug.WriteLine("Initialization");
        }
    }
}
