using Parakeet;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace Plato.Compiler
{
    public class SymbolWriterJavaScript : CodeBuilder<SymbolWriterJavaScript>
    {
        public string Rename(string name)
        {
            return "P_" + name;
        }

        public TypeResolver TypeResolver { get; }

        public SymbolWriterJavaScript(TypeResolver tg)
            => TypeResolver = tg;

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

        public SymbolWriterJavaScript WriteParameterConstraints(FunctionSymbol function)
        {
            foreach (var p in function.Parameters)
            {
                // DEBUG: 
                var constraints = TypeResolver.ParameterConstraints[p];
                WriteLine($"// {p} {string.Join(", ", constraints)}");

                var candidates = TypeResolver.GetCandidateTypes(p);
                WriteLine($"// Candidates = {string.Join(",", candidates.Select(c => c.Name))}");
            }
            return this;
        }

        public SymbolWriterJavaScript WriteFunctionCallCandidates(FunctionSymbol function)
        {
            var calls = function.Body.GetDescendantSymbols().OfType<FunctionCallSymbol>();
            foreach (var call in calls)
            {
                var f = call.Function;
                if (f is RefSymbol rs)
                {
                    var candidates = TypeResolver.Ops.GetMembers(rs.Name);
                    WriteLine($"// Called function: {call.Function} candidates = {string.Join(", ", candidates)}");
                }
                else
                {
                    throw new Exception("What else can be called?");
                }

            }
            return this;
        }

        public SymbolWriterJavaScript Write(FunctionSymbol function)
        {
            return Write(Keyword("function").PadRight())
                .Write(Delimiter("("))
                .WriteCommaList(function.Parameters)
                .Write(Delimiter(")").PadRight())
                
                // TEMP: for debugging
                .WriteLine()
                .WriteParameterConstraints(function)
                .WriteFunctionCallCandidates(function)

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

        public string TypeName(TypeDefSymbol typeDef)
        {
            return $"{typeDef.UniqueName}_{typeDef.Kind}";
        }

        public SymbolWriterJavaScript WriteConcept(TypeDefSymbol typeDef)
        {
            if (typeDef?.Kind != TypeKind.Concept) throw new Exception("Expected concept");
            var name = TypeName(typeDef);
            WriteLine($"class {name}");
            WriteStartBlock();
            WriteLine("constructor(self) { this.Self = self; };");
            WriteMethods(typeDef);
            WriteEndBlock();
            return this;
        }

        public SymbolWriterJavaScript WriteMethods(TypeDefSymbol typeDef)
        {
            foreach (var method in typeDef.Methods)
            {
                var f = method.Function;
                Write($"static {Rename(f.UniqueName)} = ");
                Write(f);
                WriteLine(";");
            }

            return this;
        }

        public SymbolWriterJavaScript WriteType(TypeDefSymbol typeDef)
        {
            if (typeDef?.Kind != TypeKind.Type) throw new Exception("Expected type");
            var name = TypeName(typeDef);
            WriteLine($"class {name}");
            WriteStartBlock();

            // TODO: the default implementation of concepts will require this as well. 
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
                WriteLine($"static {Rename(field.UniqueName)} = function(self) {{ return self.{FieldName(field)}; }}");
            }

            WriteLine(Comment("// implemented concepts "));
            
            foreach (var typeRef in typeDef.Implements)
            {
                if (typeRef.Def == null)
                    throw new Exception("Unexpected missing type def");
            }

            WriteEndBlock();
            return this;
        }

        public SymbolWriterJavaScript WriteLibrary(TypeDefSymbol typeDef)
        {
            if (typeDef?.Kind != TypeKind.Library) throw new Exception("Expected library");
            var name = TypeName(typeDef);
            WriteLine($"class {name}");
            WriteStartBlock();
            foreach (var method in typeDef.Methods)
            {
                var f = method.Function;
                Write($"static {Rename(f.UniqueName)} = ");
                Write(f);
                WriteLine(";");
            }
            WriteEndBlock();
            return this;
        }

        public SymbolWriterJavaScript WriteFile(IReadOnlyList<TypeDefSymbol> typeDefs)
        {
            var concepts = typeDefs.Where(s => s.Kind == TypeKind.Concept).ToList();
            var libraries = typeDefs.Where(s => s.Kind == TypeKind.Library).ToList();
            var types = typeDefs.Where(s => s.Kind == TypeKind.Type).ToList();

            foreach (var x in libraries)
                WriteLibrary(x);
            foreach (var x in concepts)
                WriteConcept(x);
            foreach (var x in types)
                WriteType(x);
            return this;
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
                    return Write(Literal(literal.Value.ToLiteralString()));

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
                    throw new NotImplementedException("Type definitions are supposed to be handled first ");

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