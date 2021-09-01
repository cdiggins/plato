using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlatoAnalyzer
{
    public class PlatoAnalyzer
    {
        public PlatoSemanticMapping Mapping = new PlatoSemanticMapping();

        public void Analyze(SyntaxTree tree, SemanticModel model)
        {
            Mapping.Model = model;
            foreach (var classDecl in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                Mapping.Add(() => classDecl.ToPlato(Mapping), classDecl);
        }

        public void Analyze(Compilation compilation)
        {
            foreach (var tree in compilation.SyntaxTrees)
                Analyze(tree, compilation.GetSemanticModel(tree));
        }
    }
}
