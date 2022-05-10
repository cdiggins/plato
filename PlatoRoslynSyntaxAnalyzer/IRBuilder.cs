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

        public T AddIR<T>(T ir) where T : IR
            => AddIR(null as SyntaxNode, ir);

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

            // TODO: handle shared namespace and partial classes
            //if (refs.Length > 1) throw new Exception("Multiple declarations found");

            return refs.Length == 0 
                ? null 
                : GetIR(refs[0].GetSyntax());
        }
    }

    public static class BuilderExtensions
    {
        public static (TextSpan, SyntaxKind) ToKey(this SyntaxNode node)
            => (node.Span, node.Kind());

        public static VariableDeclarationIR CreateNewVar(this IRBuilder builder, ExpressionIR value)
        {
            var r = builder.AddIR(new VariableDeclarationIR());
            r.Name = $"_var{r.Id}";
            r.InitialValue = value;
            return r;
        }

        public static T SetType<T>(this T self, TypeReferenceIR type) where T : ExpressionIR
        {
            self.ExpressionType = type;
            return self;
        }

        public static VariableReferenceIR CreateReference(this IRBuilder builder, VariableDeclarationIR var)
            => builder.AddIR(new VariableReferenceIR(var.Name, var)).SetType(var.Type);

        public static TupleIR CreateTuple(this IRBuilder builder, params ExpressionIR[] expressions)
            => builder.AddIR(new TupleIR(expressions));

        public static AssignmentIR CreateAssignment(this IRBuilder builder, ExpressionIR lvalue, ExpressionIR rvalue)
            => builder.AddIR(new AssignmentIR(lvalue, rvalue)).SetType(rvalue.ExpressionType);

        public static LetIR CreateLet(this IRBuilder builder, VariableDeclarationIR variable, ExpressionIR expression)
            => builder.AddIR(new LetIR(variable, expression)).SetType(expression.ExpressionType);

        public static LiteralIR ToLiteralIR(this int n)
            => new LiteralIR(n.ToString(), n);
    }
}
