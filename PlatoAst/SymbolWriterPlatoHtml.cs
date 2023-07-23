using Parakeet;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PlatoAst
{
    public class SymbolWriterPlatoHtml: CodeBuilder<SymbolWriterPlatoHtml>
    {
        public TypeGuesser TypeGuesser { get; }

        public SymbolWriterPlatoHtml(TypeGuesser tg)
            => TypeGuesser = tg;

        public string Span(string s, string cls)
            => $"<span class='{cls}'>{s}</span>";

        public string Operator(string s)
            => Span(s, "operator");

        public string Comment(string s)
            => Span(s, "comment");

        public string Delimiter(string s)
            => Span(s, "delimiter");

        public string Keyword(string s)
            => Span(s, "keyword");

        public string Type(string s)
            => Span(s, "type");

        public string Variable(string s)
            => Span(s, "variable");

        public string Literal(string s)
            => Span(s, "literal");

        public SymbolWriterPlatoHtml WriteBlock(params Symbol[] symbols)
            => WriteBlock((IEnumerable<Symbol>)symbols, false);

        public SymbolWriterPlatoHtml Write(IEnumerable<Symbol> symbols)
            => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

        public SymbolWriterPlatoHtml WriteBlock(IEnumerable<Symbol> symbols, bool semiColons)
        {
            var r = WriteLine(Delimiter("{")).Indent();
            foreach (var symbol in symbols)
            {
                r = r.Write(symbol);
                
                if (semiColons)
                    r = r.WriteLine(Delimiter(";"));
                else
                    r = r.WriteLine("");
            }
            r = r.Dedent().WriteLine(Delimiter("}"));
            return r;
        }

        public SymbolWriterPlatoHtml WriteRegion(IReadOnlyList<Symbol> symbols)
        {
            if (symbols.Count == 1 && symbols[0] is RegionSymbol rs)
                return Write(rs);
            var r = WriteLine(Delimiter("{")).Indent();
            for (var i=0; i < symbols.Count; ++i)
            {
                var symbol = symbols[i];
                if (i == symbols.Count - 1)
                    r = r.Write(Keyword("return") + " ");
                r = r.Write(symbol);
                r = r.WriteLine(Delimiter(";"));
            }
            r = r.Dedent().WriteLine(Delimiter("}"));
            return r;
        }

        public SymbolWriterPlatoHtml WriteTypeDecl(TypeRefSymbol typeRef, string defaultType = "var")
        {
            if (typeRef == null)
                return this;

            return Write(" " + Delimiter(":") + " " + Type(typeRef.Name));
        }

        public SymbolWriterPlatoHtml Write(FunctionSymbol function)
        {
            if (function.Name == "__lambda__")
            {
                return Write(Delimiter("("))
                    .WriteCommaList(function.Parameters.Select(p => p.Name))
                    .Write(Delimiter(")") + " " + Delimiter("=>") + " ")
                    .Write(function.Body);
            }

            // TODO: 
            // var node = function.Location;

            // function.T
            return Write(Variable(function.Name))
                .Write(Delimiter("("))
                .WriteCommaList(function.Parameters)
                .Write(Delimiter(")"))
                .WriteTypeDecl(function.Type, "void")
                .WriteLine()
                .Write(function.Body);
        }

        public SymbolWriterPlatoHtml WriteCommaList(IEnumerable<Symbol> symbols)
        {
            var r = this;
            var first = true;
            foreach (var s in symbols)
            {
                if (!first)
                    r = r.Write(Delimiter(",") + " ");
                first = false;
                r = r.Write(s);
            }

            return r;
        }

        public SymbolWriterPlatoHtml WriteCommaList(IEnumerable<string> symbols)
        {
            var r = this;
            var first = true;
            foreach (var s in symbols)
            {
                if (!first)
                    r = r.Write(Delimiter(",") + " ");
                first = false;
                r = r.Write(s);
            }

            return r;
        }

        public SymbolWriterPlatoHtml Write(TypeDefSymbol typeDef)
        {
            Write(Keyword(typeDef.Kind) + " ");
            WriteLine(Type(typeDef.Name));
            if (typeDef.Implements.Count > 0)
            {
                Indent();
                Write(Keyword("implements")).Write(" ");
                var types = string.Join(Delimiter(",") + " ", 
                    typeDef.Implements.Select(i => Type(i.Name)));
                WriteLine(types);
                Dedent();
            }

            if (typeDef.Inherits.Count > 0)
            {
                Indent();
                Write(Keyword("inherits")).Write(" ");
                var types = string.Join(Delimiter(",") + " ", 
                    typeDef.Inherits.Select(i => Type(i?.Name ?? "unknown")));
                WriteLine(types);
                Dedent();
            }

            return WriteBlock(typeDef.Members, false);
        }

        public SymbolWriterPlatoHtml Write(Symbol value)
        {
            switch (value)
            {
                case null:
                    return Write(Keyword("null"));

                case RefSymbol refSymbol:
                    return Write(Variable(refSymbol.Name));

                case ArgumentSymbol argument:
                    return Write(argument.Original);

                case AssignmentSymbol assignment:
                    return Write(assignment.LValue)
                        .Write(" " + Operator("=") + " ")
                        .Write(assignment.RValue);

                case ConditionalSymbol conditional:
                    return Write(conditional.Condition)
                        .Indent().WriteLine().Write(Operator("?") + " ")
                        .Write(conditional.IfTrue)
                        .WriteLine()
                        .Write(Operator(":") + " ")
                        .Write(conditional.IfFalse)
                        .Dedent().WriteLine();

                case FieldDefSymbol fieldDef:
                    return Write(Variable(fieldDef.Name)).WriteTypeDecl(fieldDef.Type)
                        .Write(Delimiter(";"));

                case FunctionSymbol function:
                    return Write(function);
                    
                case FunctionResultSymbol functionResult:
                    return Write(functionResult.Function).Write(Delimiter("("))
                        .WriteCommaList(functionResult.Args).Write(Delimiter(")"));

                case LiteralSymbol literal:
                    return Write(Literal(literal.Value.ToString()));

                case MethodDefSymbol methodDef:
                    return Write(methodDef.Function);

                case TypeParameterDefSymbol typeParameterDef:
                    throw new Exception("Not implemented");

                case MemberDefSymbol member:
                    throw new Exception("Not implemented");

                case NoValueSymbol noValue:
                    return Write("_");

                case ParameterSymbol parameter:
                    return Write(Variable(parameter.Name)).WriteTypeDecl(parameter.Type);

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

        public static string ToPlatoHtml(Operations ops)
        {
            var tg = new TypeGuesser(ops);
            var r = new SymbolWriterPlatoHtml(tg);
            r.WriteLine("<html><head><link rel=\"stylesheet\" href=\"style.css\"></head><body><pre>");
            r.Write(ops.Types);
            r.WriteLine("</pre></body></html>");
            return r.ToString();
        }
    }
}