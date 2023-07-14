using System;
using System.Collections.Generic;
using System.Linq;
using Parakeet;

namespace PlatoAst
{
    public class AstCodeWriter : CodeBuilder<AstCodeWriter>
    {
        public enum Language
        {
            JavaScript,
            TypeScript,
            CSharp,
        }

        public Language Lang { get; }

        public AstCodeWriter(Language lang)
            => Lang = lang;

        public AstCodeWriter WriteEol()
        {
            return WriteLine(";");
        }

        public AstCodeWriter WriteStatements(IEnumerable<AstNode> nodes)
        {
            return WriteList(nodes, (w, n) => w.Write(n));
        }

        public AstCodeWriter Write(IEnumerable<AstNode> nodes, string sep = "")
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

        public AstCodeWriter ToCSharp(AstConstant c)
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

        public AstCodeWriter Write(AstVarDef def)
        {
            switch (Lang)
            {
                case Language.JavaScript:
                    return Write("let ").WriteTypedName(def.Name, def.Type).Write(" = ").Write(def.Value).WriteEol();
                case Language.CSharp:
                    return Write(def.Type).Write(" ").Write(def.Name).Write(" = ").Write(def.Value).WriteEol();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public AstCodeWriter Write(AstConditional astConditional)
        {
            return Write(astConditional.Condition).Indent().WriteLine()
                .Write("? ").Write(astConditional.IfTrue).WriteLine()
                .Write(": ").Write(astConditional.IfFalse).Dedent().WriteLine();
        }

        public AstCodeWriter Write(AstLoop astLoop)
        {
            return Write("while ")
                .Write("(")
                .Write(astLoop.Condition)
                .WriteLine(")")
                .Write(astLoop.Body)
                .WriteLine();
        }

        public AstCodeWriter WriteTypedName(string ident, AstTypeNode type)
        {
            if (Lang == Language.JavaScript)
            {
                return Write(ident);
            }
            else if (Lang == Language.CSharp)
            {
                if (type == null)
                {
                    return Write("object").Write(" ").Write(ident);
                }
                else
                {
                    return Write(type).Write(" ").Write(ident);
                }
            }
            else
            {
               return Write(ident).Write(" : ").Write(type);
            }
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

        public static AstCodeWriter Write<T>(AstCodeWriter w, T x)
            where T : AstNode
        {
            return w.Write(x);
        }

        public AstCodeWriter WriteTypeOrObject(AstTypeNode type)
        {
            if (type == null)
                return Write("object");
            else
                return Write(type);
        }

        public AstCodeWriter Write(AstNode node)
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
                    return Brace(cb => 
                        cb.WriteStatements(astBlock.Statements));

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

                case AstIntrinsic astIntrinsic:
                    return Write(ConvertIntrinsicName(astIntrinsic.Name));

                case AstTypeDeclaration typeDeclaration:
                {
                    var r = Write("class ").WriteLine(typeDeclaration.Name);
                    r = r.WriteLine("{").Indent();
                    r = typeDeclaration.Members.Aggregate(r, (w, n) => w.Write(n));
                    r = r.Dedent().WriteLine("}");
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
                    return WriteTypedName(fieldDeclaration.Name, fieldDeclaration.Type).WriteEol();

                case AstParameterDeclaration parameterDeclaration:
                    return WriteTypedName(parameterDeclaration.Name, parameterDeclaration.Type);

                case AstParenthesized parenthesized:
                    return Write("(").Write(parenthesized.Inner).Write(")");

                case AstMethodDeclaration methodDeclaration:
                {
                    var r = Lang == Language.CSharp
                        ? WriteTypeOrObject(methodDeclaration.Type).Write(" ")
                        : this;
                    r = r.Write(methodDeclaration.Name);
                    r = r.Write("(");
                    r = r.WriteCommaList(methodDeclaration.Parameters, Write);
                    r = r.Write(")");
                    if (Lang == Language.TypeScript)
                    {
                        if (methodDeclaration.Type != null)
                        {
                            r = r.Write(" : ");
                            r = r.Write(methodDeclaration.Type);
                        }
                    }

                    r = r.WriteLine();
                    r = r.Write(methodDeclaration.Body);
                    r = r.WriteLine();
                    return r;
                }

                case AstNamespace astNamespace:
                    return astNamespace.Children.Aggregate(this, (w, n) => w.Write(n));

                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }
        }
    }

    public static class CodeWriterExtensions
    {
        public static string ToJavaScript(this AstNode node)
            => new AstCodeWriter(AstCodeWriter.Language.JavaScript).Write(node).ToString();

        public static string ToPlato(this AstNode node)
            => new PlatoWriter().Write(node).ToString();

        public static string ToCSharp(this AstNode node)
            => new AstCodeWriter(AstCodeWriter.Language.CSharp).Write(node).ToString();

        public static string ToXml(this AstNode node)
            => new AstXmlBuilder().Write(node).ToString();
    }
}