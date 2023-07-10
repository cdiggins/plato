using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet;

namespace PlatoAst
{
    public class PlatoWriter : CodeBuilder<PlatoWriter>
    {
        public PlatoWriter WriteEol()
            => WriteLine(";");

        public PlatoWriter WriteStatements(IEnumerable<AstNode> nodes)
            => WriteList(nodes, (w, n) => w.Write(n));

        public PlatoWriter Write(IEnumerable<AstNode> nodes, string sep = "")
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

        public PlatoWriter ToCSharp(AstConstant c)
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

        public PlatoWriter Write(AstVarDef def)
        {
            return Write("var").Write(" ").Write(def.Name).Write(" = ").Write(def.Value).WriteEol();
        }

        public PlatoWriter Write(AstConditional astConditional)
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

        public PlatoWriter Write(AstLoop astLoop)
        {
            return Write("while ")
                .Write(astLoop.Condition)
                .WriteLine(" do ")
                .Write(astLoop.Body)
                .WriteLine();
        }

        public PlatoWriter WriteTypedName(AstIdentifier ident, AstTypeNode type)
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

        public static PlatoWriter Write<T>(PlatoWriter w, T x)
            where T : AstNode
        {
            return w.Write(x);
        }

        public PlatoWriter Write(AstNode node)
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

                case AstBreak astBreak:
                    return WriteLine("break");

                case AstConditional astConditional:
                    return Write(astConditional);

                case AstConstant astConstant:
                    return ToCSharp(astConstant);

                case AstContinue astContinue:
                    return WriteLine("continue");

                case AstMemberAccess astMember:
                    return Write(astMember.Receiver).Write(".").Write(
                        ConvertIntrinsicName(astMember.Name));

                case AstInvoke astInvoke:
                    return Write(astInvoke.Function)
                        .Write("( ")
                        .WriteCommaList(astInvoke.AstArguments, Write)
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
                    return Write(astIdent.Text);

                case AstError astError:
                    return WriteLine($"throw \"{astError.Text}\"");

                case AstIntrinsic astIntrinsic:
                    return Write(ConvertIntrinsicName(astIntrinsic.Name));

                case AstTypeDeclaration typeDeclaration:
                {
                    var r = Write("type ").WriteLine(typeDeclaration.Name);

                    // TODO: this needs to convert from attributes to implements list 
                    if (typeDeclaration.Attributes.Count > 0)
                    {
                        r = r.Write("  implements ").WriteCommaList(typeDeclaration.Attributes, Write).WriteLine();
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

                case AstDirective directive:
                    return WriteLine($"#directive {directive.Argument}");

                case AstFieldDeclaration fieldDeclaration:
                    return Write("field ")
                        .WriteTypedName(fieldDeclaration.Name, fieldDeclaration.Type)
                        .WriteEol();

                // TODO: remove properties
                case AstPropertyDeclaration propertyDeclaration:
                    throw new Exception("Properties are not supported");

                case AstParameterDeclaration parameterDeclaration:
                    return WriteTypedName(parameterDeclaration.Name, parameterDeclaration.Type);

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

                case AstNamespace astNamespace:
                    return astNamespace.Children.Aggregate(this, (w, n) => w.Write(n));

                case AstAttribute astAttribute:
                    return Write(astAttribute.Name);

                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }
        }
    }
}