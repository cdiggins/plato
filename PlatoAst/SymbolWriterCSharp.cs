using Parakeet;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PlatoAst
{
    public class SymbolWriterCSharp : CodeBuilder<SymbolWriterCSharp>
    {
        public SymbolWriterCSharp WriteBlock(params Symbol[] symbols)
            => WriteBlock((IEnumerable<Symbol>)symbols, false);

        public SymbolWriterCSharp Write(IEnumerable<Symbol> symbols)
            => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

        public SymbolWriterCSharp WriteBlock(IEnumerable<Symbol> symbols, bool semiColons)
        {
            var r = WriteLine("{").Indent();
            foreach (var symbol in symbols)
            {
                r = r.Write(symbol);
                
                if (semiColons)
                    r = r.WriteLine(";");
                else
                    r = r.WriteLine("");
            }
            r = r.Dedent().WriteLine("}");
            return r;
        }

        public SymbolWriterCSharp WriteTypeDecl(TypeRefSymbol typeRef, string defaultType = "var")
        {
            if (typeRef == null)
                return Write($"{defaultType} ");
            else
                return Write(typeRef.Name).Write(" ");
        }

        public SymbolWriterCSharp WriteCommaList(IEnumerable<Symbol> symbols)
        {
            var r = this;
            var first = true;
            foreach (var s in symbols)
            {
                if (!first)
                    r = r.Write(", ");
                first = false;
                r = r.Write(s);
            }

            return r;
        }

        public SymbolWriterCSharp Write(Symbol value)
        {
            switch (value)
            {
                case null:
                    return Write("null");

                case RefSymbol refSymbol:
                    return Write(refSymbol.Name);

                case ArgumentSymbol argument:
                    return Write(argument.Original);

                case AssignmentSymbol assignment:
                    return Write(assignment.LValue)
                        .Write(" = ")
                        .Write(assignment.RValue);

                case ConditionalSymbol conditional:
                    return Write(conditional.Condition)
                        .Indent().WriteLine().Write("? ")
                        .Write(conditional.IfTrue)
                        .WriteLine()
                        .Write(": ")
                        .Write(conditional.IfFalse)
                        .Dedent().WriteLine();

                case FieldDefSymbol fieldDef:
                    return WriteTypeDecl(fieldDef.Type).Write(fieldDef.Name);

                case FunctionSymbol function:
                    return WriteTypeDecl(function.Type, "void").Write(function.Name)
                        .Write("(").WriteCommaList(function.Parameters).WriteLine(")")
                        .WriteBlock(function.Body);

                case FunctionResultSymbol functionResult:
                    return Write(functionResult.Function).Write("(")
                        .WriteCommaList(functionResult.Args).Write(")");

                case IntrinsicSymbol intrinsic:
                    return Write(intrinsic.Name);//.Write("/* intrinsic */");

                case LiteralSymbol literal:
                    return Write(literal.Value.ToString());

                case MethodDefSymbol methodDef:
                    return Write("public static ").Write(methodDef.Function);

                case TypeParameterDefSymbol typeParameterDef:
                    throw new Exception("Not implemented");

                case MemberDefSymbol member:
                    throw new Exception("Not implemented");

                case NoValueSymbol noValue:
                    return Write("_");

                case ParameterSymbol parameter:
                    return WriteTypeDecl(parameter.Type).Write(parameter.Name);

                case RegionSymbol region:
                    return WriteBlock(region.Children, true);

                case TypeDefSymbol typeDef:
                    return Write("class ").WriteLine(typeDef.Name).WriteBlock(typeDef.Members, false);
                   
                case TypeRefSymbol typeRef:
                    throw new NotImplementedException("Type references are supposed to be handled in a function");

                case VariableSymbol variable:
                    return Write(variable.Name);    

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }

            return this;
        }

        public static string ToCSharp(IEnumerable<Symbol> symbols)
        {
            var writer = new SymbolWriterCSharp();
            var r = writer.Write(symbols);
            return r.ToString();
        }
    }
}