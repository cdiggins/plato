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
        public readonly Dictionary<(TextSpan, SyntaxKind), int> DeclarationLookup
            = new Dictionary<(TextSpan, SyntaxKind), int>();

        public readonly List<(SyntaxNode, DeclarationIR)> Declarations = new List<(SyntaxNode, DeclarationIR)>();
        
        public T AddIR<T>(PlatoSyntax syntax, T ir) where T : IR
            => AddIR<T>(syntax.GetNode(), ir);

        public T AddIR<T>(SyntaxNode node, T ir) where T: IR
        {
            Debug.Assert(ir.Id == 0);
            if (ir is DeclarationIR declarationIr)
            {
                ir.Id = Declarations.Count;
                Declarations.Add((node, declarationIr));
                if (node != null)
                {
                    var key = node.ToKey();
                    if (!DeclarationLookup.ContainsKey(key))
                    {
                        DeclarationLookup.Add(key, ir.Id);
                    }
                }
            }
            return ir;
        }

        public IR GetIR(SyntaxNode node)
            => node == null ? null 
                : DeclarationLookup.TryGetValue(node.ToKey(), out var result) ? Declarations[result].Item2 : null;

        public IR GetIR(int n)
            => Declarations[n].Item2;

        public SyntaxNode GetSyntax(IR ir)
            => Declarations[ir.Id].Item1;

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
