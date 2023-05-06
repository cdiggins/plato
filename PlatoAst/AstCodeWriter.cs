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
            Pail,
        }

        public Language Lang { get; }

        public AstCodeWriter(Language lang)
            => Lang = lang;

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
                    return WriteToken("let").Write(def.Name).WriteToken(" =").Write(def.Value);
                case Language.JavaScript:
                    return WriteToken("let").Write(def.Name).WriteToken(" =").Write(def.Value);
                case Language.CSharp:
                    return WriteToken("var").Write(def.Name).WriteToken(" =").Write(def.Value);
                case Language.CPlusPlus:
                    return WriteToken("auto").Write(def.Name).WriteToken(" =").Write(def.Value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public AstCodeWriter WriteParameters(IEnumerable<AstVarDef> parameters)
        {
            return Lang == Language.CPlusPlus 
                ? WriteCommaList(parameters, (w, p) => w.WriteToken("PlatoObject").Write(p.Name)) 
                : WriteCommaList(parameters, (w, p) => Write(p.Name));
        }

        public AstCodeWriter Write(AstConditional astConditional)
        {
            if (Lang == Language.Pail)
            {
                return WriteToken("if")
                    .Write(astConditional.Condition)
                    .WriteToken(" then")
                    .Write(astConditional.IfTrue)
                    .WriteToken(" else")
                    .Write(astConditional.IfFalse);
            }
            else 
            {
                return Write(astConditional.Condition)
                    .WriteToken(" ?")
                    .Write(astConditional.IfTrue)
                    .WriteToken(" :")
                    .Write(astConditional.IfFalse);
            }
        }

        public AstCodeWriter Write(AstLoop astLoop)
        {
            if (Lang == Language.Pail)
            {
                return WriteToken("while")
                    .Write(astLoop.Condition)
                    .WriteToken(" do")
                    .Write(astLoop.Body);
            }
            else
            {
                return WriteToken("while")
                    .Write("(")
                    .Write(astLoop.Condition)
                    .WriteLine(")")
                    .Write(astLoop.Body);
            }
        }

        public AstCodeWriter Write(AstNode node)
        {
            switch (node)
            {
                case AstAssign astAssign:
                    return Write(astAssign.Var)
                        .WriteToken(" =")   
                        .Write(astAssign.Value);
                    
                case AstBlock astBlock:
                    return Brace(cb => cb.WriteStatements(astBlock.Statements));

                case AstBreak astBreak:
                    return Write("break");

                case AstConditional astConditional:
                    return Write(astConditional);

                case AstConstant astConstant:
                    return ToCSharp(astConstant);

                case AstContinue astContinue:
                    return Write("continue");

                case AstInvoke astInvoke:
                    return Write(astInvoke.Function)
                        .WriteToken("(")
                        .WriteCommaList(astInvoke.AstArguments, (w, x) => w.Write(x))
                        .WriteToken(")");
                        
                case AstLambda astLambda:
                    return Write("(")
                        .WriteParameters(astLambda.Parameters)
                        .WriteToken(")")
                        .WriteToken("=>")
                        .Write(astLambda.Body);
                        
                case AstLoop astLoop:
                    return Write(astLoop);

                case AstNoop astNoop:
                    return Lang == Language.Pail ? Write("_") : this;

                case AstReturn astReturn:
                    return WriteToken("return")
                        .Write(astReturn.Value);

                case AstVarDef astVarDef:
                    return Write(astVarDef);
                
                case AstVarRef astVarRef:
                    return Write(astVarRef.Name);

                case AstMulti astMulti:
                    return astMulti.Nodes.Aggregate(this, (w, n) => w.Write(n));

                case AstIdentifier astIdent:
                    return Write(astIdent.Text);

                case AstError astError:
                    return Write($"throw new Exception({astError.Text})");

                case AstIntrinsic astIntrinsic:
                    return Write(astIntrinsic.Name);

                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }
        }
    }
}