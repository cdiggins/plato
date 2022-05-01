using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using PlatoIR;

namespace PlatoRoslynSyntaxAnalyzer
{
    public class IRBuilder
    {
        public readonly Dictionary<(TextSpan, SyntaxKind), DeclarationIR> Declarations 
            = new Dictionary<(TextSpan, SyntaxKind), DeclarationIR>();

        public readonly Dictionary<(TextSpan, SyntaxKind), IR> NonDeclarations
            = new Dictionary<(TextSpan, SyntaxKind), IR>();

        public IRBuilder AddDeclaration(SyntaxNode node, DeclarationIR ir)
        {
            Debug.Assert(ir.Id == 0);
            Debug.Assert(!Declarations.ContainsKey(node.ToKey()));
            ir.Id = Declarations.Count;
            Declarations.Add(node.ToKey(), ir);
            return this;
        }

        public IR GetIR(SyntaxNode node)
            => Declarations[node.ToKey()];

        public T GetIR<T>(SyntaxNode node) where T : IR
            => (T)GetIR(node);

        public T GetDeclarationIR<T>(ISymbol symbol) where T : IR
            => (T)GetDeclarationIR(symbol);

        public IR GetDeclarationIR(ISymbol symbol)
        {
            if (symbol == null) return null;
            var refs = symbol.DeclaringSyntaxReferences;
            if (refs.Length > 0)
                throw new Exception("Multiple declarations found");
            if (refs.Length == 0)
                return null;
            return GetIR(refs[0].GetSyntax());
        }
    }

    public static class BuilderExtensions
    {
        public static (TextSpan, SyntaxKind) ToKey(this SyntaxNode node)
            => (node.Span, node.Kind());
    }
}
