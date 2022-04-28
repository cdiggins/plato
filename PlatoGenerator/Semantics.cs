using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using PlatoRoslynSyntaxAnalyzer;

namespace PlatoGenerator
{
    public static class Semantics
    {
        public static SyntaxNode GetDeclaringSyntax(this ISymbol symbol)
        {
            if (symbol == null) return null;
            var refs = symbol.DeclaringSyntaxReferences.Select(sr => sr.GetSyntax()).ToList();
            if (refs.Count == 0) return null;
            if (refs.Count > 1) throw new Exception("Partial declaration not currently supported");
            return refs[0];
        }

        public static PlatoSyntax GetDeclaringPlatoSyntax(this ISymbol symbol)
            => PlatoSyntax.GetSyntax(GetDeclaringSyntax(symbol));
    }
}