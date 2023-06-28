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
            return WriteList(nodes, (w, n) => w.Write(n).WriteLine(";"));
        }

        public AstCodeWriter Write(IEnumerable<AstNode> nodes)
        {
            return nodes.Aggregate(this, (cb, n) => cb.Write(n));
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

                default:
                    return Write(c.Value.ToString());
            }
        }

        public AstCodeWriter Write(AstVarDef def)
        {
            switch (Lang)
            {
                case Language.Pail:
                    return Write("let ").Write(def.Name).Write(" =").Write(def.Value).WriteLine();
                case Language.JavaScript:
                    return Write("let ").Write(def.Name).Write(" =").Write(def.Value).WriteLine();
                case Language.CSharp:
                    return Write("var ").Write(def.Name).Write(" =").Write(def.Value).WriteLine();
                case Language.CPlusPlus:
                    return Write("auto ").Write(def.Name).Write(" =").Write(def.Value).WriteLine();
                case Language.Python:
                    return Write(def.Name).Write(" =").Write(def.Value).WriteLine();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public AstCodeWriter WriteParameters(IEnumerable<AstVarDef> parameters)
        {
            return Lang == Language.CPlusPlus 
                ? WriteCommaList(parameters, (w, p) => w.Write("PlatoObject ").Write(p.Name)) 
                : WriteCommaList(parameters, (w, p) => Write(p.Name));
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
                return Write(astConditional.Condition)
                    .WriteLine("?")
                    .Write(astConditional.IfTrue)
                    .WriteLine()
                    .Write(":")
                    .Write(astConditional.IfFalse)
                    .WriteLine();
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

        public AstCodeWriter Write(AstNode node)
        {
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

                case AstInvoke astInvoke:
                    return Write(astInvoke.Function)
                        .Write("( ")
                        .WriteCommaList(astInvoke.AstArguments, (w, x) => w.Write(x))
                        .Write(")");
                        
                case AstLambda astLambda:
                    return Write("(")
                        .WriteParameters(astLambda.Parameters)
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
                        .WriteLine(";");

                case AstVarDef astVarDef:
                    return Write(astVarDef);
                
                case AstVarRef astVarRef:
                    return Write(astVarRef.Name);

                case AstMulti astMulti:
                    return astMulti.Nodes.Aggregate(this, (w, n) => w.Write(n));

                case AstIdentifier astIdent:
                    return Write(astIdent.Text);

                case AstError astError:
                    return Lang == Language.Python
                            ? WriteLine($"raise ErrorE(\"{astError.Text}\")") 
                            : WriteLine($"throw new Exception(\"{astError.Text}\")");

                case AstIntrinsic astIntrinsic:
                    return Write(astIntrinsic.Name);

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