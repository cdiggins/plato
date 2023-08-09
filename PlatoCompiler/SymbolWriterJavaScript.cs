using Parakeet;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;

namespace Plato.Compiler
{
    public class SymbolWriterJavaScript : CodeBuilder<SymbolWriterJavaScript>
    {
        public TypeResolver TypeResolver { get; }
        public bool WriteTypes { get; } = true;

        public SymbolWriterJavaScript(TypeResolver tg)
            => TypeResolver = tg;

        public SymbolWriterJavaScript Write(IEnumerable<Symbol> symbols)
            => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

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

        public SymbolWriterJavaScript AnnotateType(Symbol s)
        {
            return AnnotateType(TypeResolver.GetType(s));
        }

        public SymbolWriterJavaScript AnnotateType(TypeRefSymbol trs)
        {
            return AnnotateType(trs?.Def);
        }

        public SymbolWriterJavaScript AnnotateType(TypeDefSymbol tds)
        {
            if (WriteTypes)
            {
                var name = tds?.UniqueName ?? "UnknownType";
                return Write($"/* : {name} */");
            }

            return this;
        }

        public SymbolWriterJavaScript Write(FunctionSymbol function)
        {
            return Write("function".PadRight())
                .Write("(")
                .WriteCommaList(function.Parameters)
                .Write(") ")
                .AnnotateType(function.Type)
                .Write("{ ")
                .Write("return ")
                .Write(function.Body)
                .Write(";".PadRight())
                .Write("}");
        }

        public SymbolWriterJavaScript WriteCommaList(IEnumerable<Symbol> symbols)
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

        public string GetName(RefSymbol rs)
            => GetName(rs.Def);

        public string GetName(DefSymbol ds)
        {
            if (ds is TypeDefSymbol ts) 
                return $"{ts.UniqueName}_{ts.Kind}";
            if (ds is FunctionGroupSymbol fgs)
            {
                if (fgs.Functions.Count > 1)
                    Debug.WriteLine($"Multiple functions found");
                return $"{fgs.Functions[0].UniqueName}";
            }

            return $"{ds.UniqueName}";
        }

        public SymbolWriterJavaScript WriteConcept(TypeDefSymbol typeDef)
        {
            if (typeDef?.Kind != TypeKind.Concept) throw new Exception("Expected concept");
            WriteLine($"class {GetName(typeDef)}");
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
                Write($"static {GetName(f)} = ");
                Write(f);
                WriteLine(";");
            }

            return this;
        }

        public SymbolWriterJavaScript WriteType(TypeDefSymbol typeDef)
        {
            if (typeDef?.Kind != TypeKind.Type) throw new Exception("Expected type");
            var name = GetName(typeDef);
            WriteLine($"class {name}");
            WriteStartBlock();

            // TODO: the default implementation of concepts will require this as well. 
            Write("constructor")
                .Write("(")
                .Write(string.Join(", ", typeDef.Fields.Select(GetName)))
                .Write(")")
                .WriteLine();
            WriteStartBlock();
            WriteLine("// field initialization ");
            foreach (var field in typeDef.Fields)
            {
                WriteLine($"this.{GetName(field)} = {GetName(field)};");
            }

            var concepts = typeDef.GetAllImplementedConcepts();
            foreach (var concept in concepts)
            {
                var cName = GetName(concept);
                foreach (var m in concept.Methods)
                {
                    var f = m.Function;
                    var fName = GetName(f);
                    WriteLine($"this.{fName} = {name}.{cName}.{fName};");
                }
            }

            WriteEndBlock();

            WriteLine("// field accessors");
            foreach (var field in typeDef.Fields)
            {
                Write($"static {GetName(field)} = function(self)");
                WriteLine($" {{ return self.{GetName(field)}; }}");
            }

            WriteLine("// implemented concepts ");
            var conceptNames = new List<string>();
            foreach (var concept in concepts)
            {
                if (concept.Kind != TypeKind.Concept) 
                    throw new Exception("Expected concept");
                var conceptName = GetName(concept);
                conceptNames.Add(conceptName);
                WriteLine($"static {conceptName} = new {conceptName}({name});");
            }
            WriteLine($"static Implements = [{string.Join(",", conceptNames)}];");

            WriteEndBlock();
            return this;
        }

        public SymbolWriterJavaScript WriteLibrary(TypeDefSymbol typeDef)
        {
            if (typeDef?.Kind != TypeKind.Library) throw new Exception("Expected library");
            var name = GetName(typeDef);
            WriteLine($"class {name}");
            WriteStartBlock();
            foreach (var method in typeDef.Methods)
            {
                var f = method.Function;
                Write($"static {GetName(f)} = ");
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
                    return Write(GetName(refSymbol))
                        .AnnotateType(refSymbol);

                case ArgumentSymbol argument:
                    return Write(argument.Original);

                case AssignmentSymbol assignment:
                    return Write(assignment.LValue)
                        .Write(" = ")
                        .Write(assignment.RValue)
                        .AnnotateType(assignment);

                case ConditionalExpressionSymbol conditional:
                    return Write(conditional.Condition)
                        .Indent().WriteLine()
                        .Write("? ")
                        .Write(conditional.IfTrue)
                        .WriteLine()
                        .Write(": ")
                        .Write(conditional.IfFalse)
                        .Dedent()
                        .WriteLine();

                case FunctionSymbol function:
                    return Write(function);

                case FunctionCallSymbol functionCall:
                    return Write(functionCall.Function).Write("(")
                        .WriteCommaList(functionCall.Args).Write(")")
                        .AnnotateType(functionCall);

                case LiteralSymbol literal:
                    // TODO: add a constructor 
                    return Write(literal.Value.ToLiteralString())
                        .AnnotateType(literal);

                case MethodDefSymbol methodDef:
                    throw new Exception("Not implemented");

                case TypeParameterDefSymbol typeParameterDef:
                    throw new Exception("Not implemented");

                case MemberDefSymbol member:
                    throw new Exception("Not implemented");

                case NoValueSymbol noValue:
                    return Write("_");

                case ParameterSymbol parameter:
                    return Write(GetName(parameter))
                        .AnnotateType(parameter);

                case TypeDefSymbol typeDef:
                    throw new NotImplementedException("Type definitions are supposed to be handled first ");

                case TypeRefSymbol typeRef:
                    throw new NotImplementedException("Type references are supposed to be handled in a function");

                case VariableSymbol variable:
                    return Write(GetName(variable));

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }

            return this;
        }
    }
}