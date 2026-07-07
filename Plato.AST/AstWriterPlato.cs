using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;

namespace Ara3D.Geometry.AST
{
    public class AstWriterPlato : CodeBuilder<AstWriterPlato>
    {
        public AstWriterPlato WriteEol()
            => WriteLine(";");

        public AstWriterPlato WriteStatements(IEnumerable<AstNode> nodes)
            => WriteList(nodes, (w, n) => w.Write(n));

        public AstWriterPlato Write(IEnumerable<AstNode> nodes, string sep = "")
        {
            var first = true;
            var r = this;
            foreach (var n in nodes)
            {
                if (!first)
                    r = r.Write(sep);
                else
                    first = false;
                r = r.Write(n);
            }
            return r;
        }

        public AstWriterPlato ToCSharp(AstConstant c)
        {
            switch (c.Value)
            {
                case string _:
                    return Write('"').Write(c.Value.ToString()).Write('"');
                case char _:
                    return Write('\'').Write(c.Value.ToString()).Write('\'');
                case Delegate d:
                    return Write($"{d.Method.Name}");
                case int n:
                    return Write($"Integer.Create({n})");
                case double d:
                    return Write($"Number.Create({d})");
                case bool b:
                    return Write($"Boolean.Create({b})");
                default:
                    return Write(c.Value.ToString());
            }
        }

        public AstWriterPlato Write(AstVarDef def)
        {
            return Write("var").Write(" ").Write(def.Name).Write(" = ").Write(def.Value).WriteEol();
        }

        public AstWriterPlato Write(AstConditional astConditional)
        {
            return Write("if ")
                .Write(astConditional.Condition)
                .Write("then ")
                .Write(astConditional.IfTrue)
                .WriteLine()
                .Write("else ")
                .Write(astConditional.IfFalse)
                .WriteLine();
        }

        public AstWriterPlato Write(AstLoop astLoop)
        {
            return Write("while ")
                .Write(astLoop.Condition)
                .WriteLine(" do ")
                .Write(astLoop.Body)
                .WriteLine();
        }

        public AstWriterPlato WriteTypedName(string ident, AstTypeNode type)
        {
            return Write(ident).Write(": ").Write(type);
        }

        static (string, string)[] BinaryIntrinsics = new[]
        {
            ("+", "Add"),
            ("-", "Sub"),
            ("*", "Mul"),
            ("/", "Div"),
            ("%", "Mod"),
            ("==", "Eq"),
            ("!=", "NEq"),
            (">", "Gt"),
            ("<", "Lt"),
            (">=", "GtEq"),
            ("<=", "LtEq"),
            ("&&", "And"),
            ("||", "Or"),
            ("&", "And"),
            ("|", "Or"),
            ("^", "XOr"),
            ("[]", "At")
        };

        public static Dictionary<string, string> BinaryIntrinsicLookup =
            BinaryIntrinsics.ToDictionary(bi => bi.Item1, bi => bi.Item2);

        public static string ConvertIntrinsicName(string name)
        {
            name = name.Trim();
            return BinaryIntrinsicLookup.ContainsKey(name)
                ? BinaryIntrinsicLookup[name]
                : name;
        }

        public static AstWriterPlato Write<T>(AstWriterPlato w, T x)
            where T : AstNode
        {
            return w.Write(x);
        }

        public AstWriterPlato Write(AstNode node)
        {
            if (node == null)
                return this;

            switch (node)
            {
                case AstAssign astAssign:
                    return Write(astAssign.Var)
                        .Write(" = ")
                        .Write(astAssign.Value);

                case AstBlock astBlock:
                    {
                        return Brace(cb =>
                            cb.WriteStatements(astBlock.Statements));
                    }

                case AstBreak _:
                    return WriteLine("break");

                case AstConditional astConditional:
                    return Write(astConditional);

                case AstConstant astConstant:
                    return ToCSharp(astConstant);

                case AstContinue astContinue:
                    return WriteLine("continue");

                case AstInvoke astInvoke:
                    return Write(astInvoke.Function)
                        .Write("( ")
                        .WriteCommaList(astInvoke.Arguments, Write)
                        .Write(")");

                case AstLambda astLambda:
                    return Write("(")
                        .WriteCommaList(astLambda.Parameters, Write)
                        .Write(") => ")
                        .Write(astLambda.Body)
                        .WriteLine();

                case AstLoop astLoop:
                    return Write(astLoop);

                case AstNoop astNoop:
                    return this;

                case AstReturn astReturn:
                    return Write("return ")
                        .Write(astReturn.Value)
                        .WriteEol();

                case AstVarDef astVarDef:
                    return Write(astVarDef);

                case AstMulti astMulti:
                    return astMulti.Nodes.Aggregate(this, (w, n) => w.Write(n));

                case AstIdentifier astIdent:
                    return Write(ConvertIntrinsicName(astIdent.Text));

                case AstTypeDeclaration typeDeclaration:
                    {
                        var r = Write($"{typeDeclaration.Kind} ").WriteLine(typeDeclaration.Name.Text);

                        // TODO: this needs to convert from attributes to implements list 
                        if (typeDeclaration.Implements.Count > 0)
                        {
                            r = r.Write("  implements ").WriteCommaList(typeDeclaration.Implements,
                                Write).WriteLine();
                        }
                        else if (typeDeclaration.Inherits.Count > 0)
                        {
                            r = r.Write("  inherits ").WriteCommaList(typeDeclaration.Inherits,
                                Write).WriteLine();
                        }

                        r = r.WriteLine("{").Indent();
                        r = typeDeclaration.Members.Aggregate(r, (w, n) => w.Write(n));
                        r = r.Dedent().WriteLine("}");
                        r = r.WriteLine();
                        return r;
                    }

                case AstTypeNode typeNode:
                    {
                        var r = Write(typeNode.Name);
                        if (typeNode.TypeArguments.Count > 0)
                        {
                            r = r.Write("<");
                            r = r.Write(typeNode.TypeArguments, ",");
                            r = r.Write(">");
                        }
                        return r;
                    }

                case AstFieldDeclaration fieldDeclaration:
                    return Write("field ")
                        .WriteTypedName(fieldDeclaration.Name.Text, fieldDeclaration.Type)
                        .WriteEol();

                case AstParameterDeclaration parameterDeclaration:
                    return WriteTypedName(parameterDeclaration.Name.Text, parameterDeclaration.Type);

                case AstParenthesized parenthesized:
                    return Write("(").Write(parenthesized.Inner).Write(")");

                case AstMethodDeclaration methodDeclaration:
                    return Write("function")
                        .Write(" ")
                        .Write(methodDeclaration.Name)
                        .Write("(")
                        .WriteCommaList(methodDeclaration.Parameters, Write)
                        .Write(")")
                        .Write(" : ")
                        .Write(methodDeclaration.Type)
                        .WriteLine()
                        .Write(methodDeclaration.Body)
                        .WriteLine();

                case AstFile astNamespace:
                    return astNamespace.Children.Aggregate(this, (w, n) => w.Write(n));

                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }
        }
    }
}