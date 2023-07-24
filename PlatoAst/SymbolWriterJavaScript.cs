﻿using Parakeet;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PlatoAst
{
    public class SymbolWriterJavaScript : CodeBuilder<SymbolWriterJavaScript>
    {
        public string Rename(string name)
        {
            return "P_" + name;
        }

        public TypeGuesser TypeGuesser { get; }

        public SymbolWriterJavaScript(TypeGuesser tg)
            => TypeGuesser = tg;

        public SymbolWriterJavaScript Write(IEnumerable<Symbol> symbols)
            => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

        public string Span(string s, string cls)
            => s;

        public string Operator(string s)
            => Span(s, "operator");

        public string Comment(string s)
            => Span(s, "comment");

        public string Delimiter(string s)
            => Span(s, "delimiter");

        public string Keyword(string s)
            => Span(s, "keyword");

        public string Type(string s)
            => Span(Rename(s), "type");

        public string Variable(string s)
            => Span(Rename(s), "variable");

        public string Literal(string s)
            => Span(s, "literal");

        public string DeclarationName(string s)
            => Span(Rename(s), "declname");

        public SymbolWriterJavaScript WriteStartBlock()
            => WriteLine(Delimiter("{")).Indent();

        public SymbolWriterJavaScript WriteEndBlock()
            => Dedent().WriteLine(Delimiter("}"));

        public SymbolWriterJavaScript WriteBlock(IEnumerable<Symbol> symbols)
            => symbols.Aggregate(WriteStartBlock(),
                (w, s) => w.Write(s)).WriteEndBlock();

        public SymbolWriterJavaScript Write(FunctionSymbol function)
        {
            return Write(Keyword("function").PadRight())
                .Write(Delimiter("("))
                .WriteCommaList(function.Parameters)
                .Write(Delimiter(")").PadRight())
                .Write(Delimiter("{").PadRight())
                .Write(Keyword("return").PadRight())
                .Write(function.Body)
                .Write(Delimiter(";").PadRight())
                .Write(Delimiter("}"));
        }

        public SymbolWriterJavaScript WriteCommaList(IEnumerable<Symbol> symbols)
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

        public static string FieldName(FieldDefSymbol fd)
            => FieldName(fd.Name);

        public static string FieldName(string name)
            => "_field_" + name; 
        

        public SymbolWriterJavaScript Write(TypeDefSymbol typeDef)
        {
            Write(Keyword("class").PadRight())
                .Write(DeclarationName(typeDef.Name))
                .WriteLine()
                .WriteStartBlock();
            
            // So much generated code! 

            Write(Keyword("constructor"))
                .Write(Delimiter("("))
                .Write(string.Join(", ", typeDef.Fields.Select(f => Rename(f.Name))))
                .Write(Delimiter(")"))
                .WriteLine();

            WriteStartBlock();
            foreach (var field in typeDef.Fields)
            {
                WriteLine($"this.{FieldName(field)} = {Rename(field.Name)};");
            }
            WriteEndBlock();

            WriteLine(Comment("// field accessors"));
            foreach (var field in typeDef.Fields)
            {
                WriteLine($"static {Rename(field.Name)} = function(self) {{ return self.{FieldName(field)}; }}");
            }

            WriteLine(Comment("// functions "));
            foreach (var method in typeDef.Methods)
            {
                var f = method.Function;
                Write($"static {Rename(f.Name)} = ");
                Write(f);
                WriteLine(";");
            }

            return WriteEndBlock();
        }

        public SymbolWriterJavaScript Write(Symbol value)
        {
            switch (value)
            {
                case null:
                    return Write("null");

                case RefSymbol refSymbol:
                    return Write(Variable(refSymbol.Name));

                case ArgumentSymbol argument:
                    return Write(argument.Original);

                case AssignmentSymbol assignment:
                    return Write(assignment.LValue)
                        .Write(Operator("=").Pad())
                        .Write(assignment.RValue);

                case ConditionalExpressionSymbol conditional:
                    return Write(conditional.Condition)
                        .Indent().WriteLine()
                        .Write(Operator("?").PadRight())
                        .Write(conditional.IfTrue)
                        .WriteLine()
                        .Write(Operator(":").PadRight())
                        .Write(conditional.IfFalse)
                        .Dedent().WriteLine();

                case FunctionSymbol function:
                    return Write(function);

                case FunctionCallSymbol functionCall:
                    return Write(functionCall.Function).Write(Delimiter("("))
                        .WriteCommaList(functionCall.Args).Write(Delimiter(")"));

                case LiteralSymbol literal:
                    return Write(Literal(literal.Value.ToString()));

                case MethodDefSymbol methodDef:
                    throw new Exception("Not implemented");

                case TypeParameterDefSymbol typeParameterDef:
                    throw new Exception("Not implemented");

                case MemberDefSymbol member:
                    throw new Exception("Not implemented");

                case NoValueSymbol noValue:
                    return Write("_");

                case ParameterSymbol parameter:
                    return Write(DeclarationName(parameter.Name));

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
    }
}