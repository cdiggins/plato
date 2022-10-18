using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace PlatoRoslynSyntaxAnalyzer
{
    public static class Extensions
    {
        public static List<PlatoTypeSyntax> GetPlatoTypes(this IEnumerable<SyntaxTree> trees)
            => trees.SelectMany(tree =>
                tree.GetRoot().DescendantNodes(node => !(node is TypeDeclarationSyntax))
                    .OfType<TypeDeclarationSyntax>().Select(PlatoTypeSyntax.Create)).ToList<PlatoTypeSyntax>();

    }
}
