using Parakeet;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace PlatoAst
{
    public class SymbolWriterCSharp : CodeBuilder<SymbolWriterCSharp>
    {
        public TypeGuesser TypeGuesser { get; }

        public SymbolWriterCSharp(TypeGuesser tg)
            => TypeGuesser = tg;

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

        public SymbolWriterCSharp WriteRegion(IReadOnlyList<Symbol> symbols)
        {
            if (symbols.Count == 1 && symbols[0] is RegionSymbol rs)
                return Write(rs);
            var r = WriteLine("{").Indent();
            for (var i=0; i < symbols.Count; ++i)
            {
                var symbol = symbols[i];
                if (i == symbols.Count - 1)
                    r = r.Write("return ");
                r = r.Write(symbol);
                r = r.WriteLine(";");
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

        public SymbolWriterCSharp WriteConstraints(FunctionSymbol function)
        {
            var r = this;
            foreach (var p in function.Parameters)
            {
                var constraints = TypeGuesser.ParameterConstraints[p];
                WriteLine($"// {p} {string.Join(", ", constraints)}");
            }
            // TODO: use the type guesser 
            return r;
        }

        public SymbolWriterCSharp Write(FunctionSymbol function)
        {
            if (function.Name == "__lambda__")
            {
                return Write("(")
                    .WriteCommaList(function.Parameters.Select(p => p.Name))
                    .WriteLine(") => ")
                    .WriteConstraints(function)
                    .Write(function.Body);
            }

            // TODO: 
            // var node = function.Location;

            // function.T
            return WriteTypeDecl(function.Type, "void")
                .Write(function.Name)
                .Write("(")
                .WriteCommaList(function.Parameters)
                .WriteLine(")")
                .WriteConstraints(function)
                .Write(function.Body);
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

        public SymbolWriterCSharp WriteCommaList(IEnumerable<string> symbols)
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

        public SymbolWriterCSharp Write(TypeDefSymbol typeDef)
        {
            if (typeDef.Kind == "concept")
            {
                var fullName = $"{typeDef.Name}<Self>";
                return Write("interface ").Write(fullName).Write(" where ")
                    .WriteLine($"Self : {fullName}")
                    .WriteBlock(typeDef.Members, false);
            }
            else 
            //if (typeDef.Kind == "type")
            {
                return Write("class ").WriteLine(typeDef.Name).WriteBlock(typeDef.Members, false);
            }
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
                    return Write(function);
                    
                case FunctionResultSymbol functionResult:
                    return Write(functionResult.Function).Write("(")
                        .WriteCommaList(functionResult.Args).Write(")");

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
                    return WriteRegion(region.Children);

                case TypeDefSymbol typeDef:
                    return Write(typeDef);
                
                case TypeRefSymbol typeRef:
                    throw new NotImplementedException("Type references are supposed to be handled in a function");

                case VariableSymbol variable:
                    return Write(variable.Name);    

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }

            return this;
        }

        public static string ToCSharp(Operations ops)
        {
            var tg = new TypeGuesser(ops);
            var writer = new SymbolWriterCSharp(tg);
            var r = writer.Write(ops.Types);
            return r.ToString();
        }
    }
}