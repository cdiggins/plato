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
            CSharp,
            CPlusPlus,
            Python,
            Pail,
        }

        public Language Lang { get; }

        public AstCodeWriter(Language lang)
            => Lang = lang;

        public AstCodeWriter WriteEol()
        {
            return Lang != Language.Python 
                ? WriteLine(";") 
                : WriteLine();
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
                case Language.Pail:
                case Language.JavaScript:
                    return Write("let ").WriteTypedName(def.Name, def.Type).Write(" = ").Write(def.Value).WriteEol();
                case Language.CSharp:
                    return Write(def.Type).Write(" ").Write(def.Name).Write(" = ").Write(def.Value).WriteEol();
                case Language.CPlusPlus:
                    return Write("auto ").Write(def.Name).Write(" = ").Write(def.Value).WriteEol();
                case Language.Python:
                    return Write(def.Name).Write(" =").Write(def.Value).WriteEol();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public AstCodeWriter Write(AstConditional astConditional)
        {
            if (Lang == Language.Pail)
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
            else if (Lang == Language.Python)
            {
                return Write(astConditional.IfTrue)
                    .Write(" if ")
                    .Write(astConditional.Condition)
                    .Write(" else ")
                    .Write(astConditional.IfFalse);
            }
            else
            {
                return Write(astConditional.Condition).Indent().WriteLine()
                    .Write("? ").Write(astConditional.IfTrue).WriteLine()
                    .Write(": ").Write(astConditional.IfFalse).Dedent().WriteLine();
            }
        }

        public AstCodeWriter Write(AstLoop astLoop)
        {
            if (Lang == Language.Pail)
            {
                return Write("while ")
                    .Write(astLoop.Condition)
                    .WriteLine(" do ")
                    .Write(astLoop.Body)
                    .WriteLine();
            }
            else if (Lang == Language.Python)
            {
                return Write("while ")
                    .Write(astLoop.Condition)
                    .Write(":").Indent().WriteLine()
                    .Write(astLoop.Body)
                    .Dedent()
                    .WriteLine();
            }
            else
            {
                return Write("while ")
                    .Write("(")
                    .Write(astLoop.Condition)
                    .WriteLine(")")
                    .Write(astLoop.Body)
                    .WriteLine();
            }
        }

        public AstCodeWriter WriteTypedName(AstIdentifier ident, AstTypeNode type)
        {
            return Write(ident).Write(" : ").Write(type);
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
                {
                    if (Lang == Language.Python)
                    {
                        return Indent()
                            .WriteLine()
                            .Write(astBlock.Statements)
                            .Dedent()
                            .WriteLine();
                    }
                    else
                    {
                        return Brace(cb => 
                            cb.WriteStatements(astBlock.Statements));
                    }
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
                    return Lang == Language.Pail ? Write("_") : this;

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
                    return Lang == Language.Python
                            ? WriteLine($"raise ErrorE(\"{astError.Text}\")") 
                            : WriteLine($"throw new Exception(\"{astError.Text}\")");

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
                    return Write("field ")
                        .WriteTypedName(fieldDeclaration.Name, fieldDeclaration.Type)
                        .WriteEol();

                case AstPropertyDeclaration propertyDeclaration:
                    return Write("property ")
                        .Write(propertyDeclaration.Type)
                        .Write(" ")
                        .Write(propertyDeclaration.Name).WriteEol();

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

                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }
        }
    }

    public static class CodeWriterExtensions
    {
        public static string ToJavaScript(this AstNode node)
            => new AstCodeWriter(AstCodeWriter.Language.JavaScript).Write(node).ToString();

        public static string ToPython(this AstNode node)
            => new AstCodeWriter(AstCodeWriter.Language.Python).Write(node).ToString();

        public static string ToPail(this AstNode node)
            => new AstCodeWriter(AstCodeWriter.Language.Pail).Write(node).ToString();

        public static string ToCSharp(this AstNode node)
            => new AstCodeWriter(AstCodeWriter.Language.CSharp).Write(node).ToString();

        public static string ToCPlusPlus(this AstNode node)
            => new AstCodeWriter(AstCodeWriter.Language.CPlusPlus).Write(node).ToString();

        public static string ToXml(this AstNode node)
            => new AstXmlBuilder().Write(node).ToString();
    }
}