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

        public readonly Dictionary<(TextSpan, SyntaxKind), SyntaxNode> SyntaxNodes
            = new Dictionary<(TextSpan, SyntaxKind), SyntaxNode>();

        public T AddIR<T>(PlatoSyntax syntax, T ir) where T : IR
            => AddIR<T>(syntax.GetNode(), ir);

        public T AddIR<T>(SyntaxNode node, T ir) where T: IR
        {
            Debug.Assert(ir.Id == 0);
            if (ir is DeclarationIR declarationIr)
            {
                Debug.Assert(!Declarations.ContainsKey(node.ToKey()));
                ir.Id = Declarations.Count;
                var key = node.ToKey();
                Declarations.Add(key, declarationIr);
                SyntaxNodes.Add(key, node);
            }
            return ir;
        }

        public IR GetIR(SyntaxNode node)
            => node == null ? null : Declarations.TryGetValue(node.ToKey(), out var result) ? result : null;

        public T GetIR<T>(SyntaxNode node) where T : IR
            => GetIR(node) as T;

        public T GetDeclarationIR<T>(ISymbol symbol) where T : IR
            => GetDeclarationIR(symbol) as T;

        public IR GetDeclarationIR(ISymbol symbol)
        {
            if (symbol == null) return null;
            var refs = symbol.DeclaringSyntaxReferences;
            if (refs.Length > 1)
                throw new Exception("Multiple declarations found");
            return refs.Length == 0 
                ? null 
                : GetIR(refs[0].GetSyntax());
        }
    }

    public static class BuilderExtensions
    {
        public static (TextSpan, SyntaxKind) ToKey(this SyntaxNode node)
            => (node.Span, node.Kind());
    }
}
