using Parakeet;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Plato.Compiler
{
    public class SymbolWriterPlatoHtml: CodeBuilder<SymbolWriterPlatoHtml>
    {
        public const bool UseOperators = true;

        public TypeResolver TypeResolver { get; }

        public SymbolWriterPlatoHtml(TypeResolver tg)
            => TypeResolver = tg;

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

        public string DeclarationName(string s)
            => Span(s, "declname");

        public SymbolWriterPlatoHtml WriteBlock(params Symbol[] symbols)
            => WriteBlock(symbols, false);

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

        public SymbolWriterPlatoHtml WriteTypeDecl(TypeRefSymbol typeRef, string defaultType = "var")
        {
            if (typeRef == null)
                return this;

            return Write(" " + Delimiter(":") + " " + Type(typeRef.Name));
        }

        public SymbolWriterPlatoHtml Write(FunctionCallSymbol functionCall)
        {
            if (UseOperators && functionCall.Function is RefSymbol rs)
            {
                if (OperatorNameLookup.NamesToBinaryOperators.ContainsKey(rs.Name)
                    && functionCall.Args.Count > 1)
                {
                    var op = OperatorNameLookup.NamesToBinaryOperators[rs.Name];
                    return Write(Delimiter("("))
                        .Write(functionCall.Args[0]).Write(op.Pad()).Write(functionCall.Args[1])
                        .Write(Delimiter(")"));
                }
                if (OperatorNameLookup.NamesToUnaryOperators.ContainsKey(rs.Name))
                {
                    var op = OperatorNameLookup.NamesToUnaryOperators[rs.Name];
                    return Write(op).Write(functionCall.Args[0]);
                }
            }

            return functionCall.Args.Count > 0
                ? Write(functionCall.Args[0]).Write(Operator("."))
                    .Write(functionCall.Function).WriteFunctionArgs(functionCall.Args.Skip(1))
                : Write(functionCall.Function);
        }

        public SymbolWriterPlatoHtml Write(FunctionSymbol function)
        {
            if (function.Name == "__lambda__")
            {
                return Write(Delimiter("("))
                    .WriteCommaList(function.Parameters.Select(p => p.Name))
                    .Write(Delimiter(")") + " " + Delimiter("=>") + " ")
                    .Indent().WriteLine().Write(function.Body).Dedent();
            }

            // TODO: 
            // var node = function.Location;

            // function.T
            return Write(DeclarationName(function.Name))
                .Write(Delimiter("("))
                .WriteCommaList(function.Parameters)
                .Write(Delimiter(")"))
                .WriteTypeDecl(function.Type, "void")
                .Write(" " + Delimiter("=>") + " ")
                .Indent().WriteLine()
                .Write(function.Body)
                .WriteLine(Delimiter(";"))
                .Dedent();
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
            Write(Keyword(typeDef.Kind.ToString()) + " ");
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

        public SymbolWriterPlatoHtml WriteFunctionArgs(IEnumerable<Symbol> symbols)
        {
            if (!symbols.Any())
                return this;
            return Write(Delimiter("("))
                .WriteCommaList(symbols)
                .Write(Delimiter(")"));
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

                case ConditionalExpressionSymbol conditional:
                    return Write(conditional.Condition)
                        .Indent().WriteLine().Write(Operator("?") + " ")
                        .Write(conditional.IfTrue)
                        .WriteLine()
                        .Write(Operator(":") + " ")
                        .Write(conditional.IfFalse)
                        .Dedent() ;

                case FieldDefSymbol fieldDef:
                    return Write(DeclarationName(fieldDef.Name)).WriteTypeDecl(fieldDef.Type)
                        .Write(Delimiter(";"));

                case FunctionSymbol function:
                    return Write(function);
                    
                case FunctionCallSymbol functionResult:
                    return Write(functionResult);

                case LiteralSymbol literal:
                    return Write(Literal(literal.Value.ToString()));

                case MethodDefSymbol methodDef:
                    return Write(methodDef.Function);

                case TypeParameterDefSymbol typeParameterDef:
                    throw new Exception("Not implemented");

                case MemberDefSymbol member:
                    throw new Exception("Not implemented");

                case ParameterSymbol parameter:
                    return Write(Variable(parameter.Name)).WriteTypeDecl(parameter.Type);

                case TypeDefSymbol typeDef:
                    return Write(typeDef);
                
                case TypeRefSymbol typeRef:
                    throw new NotImplementedException("Type references are supposed to be handled in a function");

                case VariableSymbol variable:
                    return Write(variable.Name);

                case PredefinedSymbol predefined:
                    return Write(predefined.Name);

                case MemberGroupSymbol memberGroup:
                    return Write(memberGroup.Name);
            }

            throw new ArgumentOutOfRangeException(nameof(value));
        }

    }
}