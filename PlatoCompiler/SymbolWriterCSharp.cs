using Parakeet;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;

namespace Plato.Compiler
{
    public class SymbolWriterCSharp : CodeBuilder<SymbolWriterCSharp>
    {
        public TypeResolver TypeResolver { get; }

        public SymbolWriterCSharp(TypeResolver tg)
            => TypeResolver = tg;
        
        public SymbolWriterCSharp Write(IEnumerable<Symbol> symbols)
            => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

        public SymbolWriterCSharp WriteTypeDecl(TypeRefSymbol typeRef, string defaultType = "var")
        {
            if (typeRef == null)
                return Write($"{defaultType} ");
            return Write(typeRef.Name).Write(" ");
        }

        public SymbolWriterCSharp WriteConstraints(FunctionSymbol function)
        {
            var r = this;
            foreach (var p in function.Parameters)
            {
                // DEBUG: 

                if (!TypeResolver.ParameterConstraints.ContainsKey(p))
                    continue;

                var constraints = TypeResolver.ParameterConstraints[p];
                WriteLine($"// {p} {string.Join(", ", constraints)}");

                var candidates = TypeResolver.GetCandidateTypes(p);
                WriteLine($"// Candidates = {string.Join(",", candidates.Select(c => c.Name))}");
            }
            return r;
        }

        public SymbolWriterCSharp WriteSignature(FunctionSymbol function)
        {
            return WriteTypeDecl(function.Type, "void")
                .Write(function.Name)
                .Write("(")
                .WriteCommaList(function.Parameters)
                .Write(")");
        }

        public SymbolWriterCSharp Write(FunctionSymbol function)
        {
            if (function.Name == "__lambda__")
            {
                return Write("(")
                    .WriteCommaList(function.Parameters.Select(p => p.Name))
                    .WriteLine(") => ")
                    //.WriteConstraints(function)
                    .Write(function.Body);
            }

            // Functions with no bodies are intrinsics
            if (function.Body == null)
                return this;

            return WriteSignature(function)                //.WriteConstraints(function)
                .WriteStartBlock()
                .Write("return ")
                .Write(function.Body)
                .WriteLine(";")
                .WriteEndBlock();
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

        public static string ParameterizedTypeName(string name, IEnumerable<string> args)
            => name + GenericArgs(args);

        public static string GenericArgs(IEnumerable<string> args)
            => args.Any() ? "<" + string.Join(", ", args) + ">" : "";

        public SymbolWriterCSharp WriteMembersAsExtensionMethods(TypeDefSymbol typeDef)
        {
            var typeParameterNames = typeDef.TypeParameters.Select(tp => tp.Name).ToList();
            var typeArgs = GenericArgs(typeParameterNames.Prepend("Self"));

            var parameterizedTypeName = ParameterizedTypeName(typeDef.Name, typeParameterNames.Prepend("Self"));
            var constraint = $"where Self: {parameterizedTypeName}";

            foreach (var member in typeDef.Members)
            {
                if (!(member is MethodDefSymbol mds))
                    continue;

                if (mds.Function.Body == null)
                    continue;

                Write("public static ")
                    .WriteTypeDecl(mds.Type, "void")
                    .Write(mds.Name)
                    .Write(typeArgs)
                    .Write("(")
                    .Write(mds.Function.Parameters.Count > 0 ? "this " : "")
                    .WriteCommaList(mds.Function.Parameters)
                    .Write(") ")
                    .WriteLine(constraint)
                    .WriteStartBlock()
                    .Write("return ")
                    .Write(mds.Function.Body)
                    .WriteLine(";")
                    .WriteEndBlock();
            }

            return this;
        }

        public static string GetTypeName(TypeRefSymbol tr, TypeDefSymbol parent)
        {
            // TODO: slightly more complicated when actually passing arguments and stuff. 
            var r = tr.Name;
            if (r == "Self")
                return parent.Name;
            return r;
        }

        public static string GetParameterNamesJoined(FunctionSymbol f)
            => string.Join(", ", f.Parameters.Select(p => p.Name));

        public static string GetParameterNamesAndTypesJoined(FunctionSymbol f, TypeDefSymbol t)
            => string.Join(", ", f.Parameters.Select(p => $"{GetTypeName(p.Type, t)} {p.Name}"));

        public SymbolWriterCSharp WriteConstructor(TypeDefSymbol t)
        {
            if (!t.IsType()) throw new NotSupportedException();
            var name = t.Name;
            var fieldTypes = t.Fields.Select(f => GetTypeName(f.Type, t)).ToList();
            var fieldNames = t.Fields.Select(f => $"{f.Name}").ToList();
            var parameterList = string.Join(", ", t.Fields.Select(f => $"{GetTypeName(f.Type, t)} _{f.Name}"));
            var parameterNamesString = string.Join(", ", t.Fields.Select(f => $"_{f.Name}"));
            var fieldTypesString = string.Join(", ", fieldTypes);
            var fieldNamesString = string.Join(", ", fieldNames);

            WriteLine($"public {name}({parameterList}) => ({fieldNamesString}) = ({parameterNamesString});");
            WriteLine($"public static {name} New({parameterList}) => new({parameterNamesString});");
            if (t.Fields.Count > 1)
            {
                var qualifiedFieldNames = string.Join(", ", fieldNames.Select(f => $"self.{f}"));
                var tupleNames = string.Join(", ", Enumerable.Range(1, t.Fields.Count).Select(i => $"value.Item{i}"));
                WriteLine($"public static implicit operator ({fieldTypesString})({name} self) => ({qualifiedFieldNames});");
                WriteLine($"public static implicit operator {name}(({fieldTypesString}) value) => new {name}({tupleNames});");
            }
            else if (t.Fields.Count == 1)
            {
                var fieldName = t.Fields[0].Name;
                var fieldType = GetTypeName(t.Fields[0].Type, t);
                WriteLine($"public static implicit operator {fieldType}({name} self) => self.{fieldName};");
                WriteLine($"public static implicit operator {name}({fieldType} value) => new {fieldType}(value);");
            }

            var fieldNamesAsStrings = string.Join(", ", fieldNames.Select(f => $"\"{f}\""));
            WriteLine($"public string[] FieldNames() => new[] {{ {fieldNamesAsStrings} }};");
            WriteLine($"public object[] FieldValues() => new[] {{ {fieldNamesString} }};");

            // TODO: output the static methods. 
            // TODO: add all appropriate operators 
            // TODO: output all zero parameter methods as properties 
            // TODO: add all appropriate functions as extension methods

            foreach (var m in t.GetConceptMethods())
            {
                var f = m.Function;
                var ret = GetTypeName(f.Type, t);
                var paramList = GetParameterNamesAndTypesJoined(f, t);
                var argList = GetParameterNamesJoined(f);

                if (f.Body == null)
                {
                    WriteLine($"public static {ret} {f.Name}({paramList}) => Extensions.{f.Name}({argList});");
                }

                if (f.Parameters.Count == 2)
                {
                    var op = OperatorNameLookup.NameToBinaryOperator(f.Name);
                    if (op != null)
                        WriteLine($"public static {ret} operator {op}({paramList}) => Extensions.{f.Name}({argList});");
                }

                if (f.Parameters.Count == 1)
                {
                    var op = OperatorNameLookup.NameToUnaryOperator(f.Name);
                    if (op != null)
                        WriteLine($"public static {ret} operator {op}({paramList}) => Extensions.{f.Name}({argList});");
                }
            }

            return this;
        }

        public static bool IsSelfType(TypeRefSymbol rs)
            => rs.Def.Equals(PrimitiveTypes.Self);

        public static bool IsTypePreservingConcept(TypeDefSymbol typeDef)
        {
            if (!typeDef.IsConcept()) return false;
            foreach (var m in typeDef.Methods)
            {
                if (IsSelfType(m.Function.Type))
                    return true;
                for (var i=1; i < m.Function.Parameters.Count; ++i)
                    if (IsSelfType(m.Function.Parameters[i].Type))
                        return true;
            }

            return typeDef.Inherits.Any(tr => IsTypePreservingConcept(tr.Def));
        }

        public SymbolWriterCSharp WriteUnimplementedMethods(TypeDefSymbol typeDef)
        {
            foreach (var f in typeDef.Methods.Select(m => m.Function))
            {
                if (f.Body != null) 
                    continue;
                WriteSignature(f).WriteLine(";");
            }

            return this;
        }

        public SymbolWriterCSharp Write(TypeDefSymbol typeDef)
        {
            if (typeDef.IsConcept())
            {
                var fullName = $"{typeDef.Name}<Self>";
                var inherited = typeDef.Inherits.Count > 0
                    ? ": " + string.Join(", ", typeDef.Inherits.Select(td => $"{td.Name}<Self>"))
                    : "";

                Write("public interface ").Write(fullName)
                    .WriteLine(inherited)   
                    .Write(" where ")
                    .WriteLine($"Self : {fullName}")
                    .WriteStartBlock()
                    .WriteUnimplementedMethods(typeDef)
                    .WriteEndBlock();

                WriteLine("public static partial class Extensions")
                    .WriteStartBlock()
                    .WriteMembersAsExtensionMethods(typeDef)
                    .WriteEndBlock();

                return this;
            }
            
            if (typeDef.IsType())
            {
                var implements = typeDef.Implements.Count > 0
                    ? ": " + string.Join(", ", typeDef.Implements.Select(td => $"{td.Name}<{typeDef.Name}>"))
                    : "";
                return Write("public class ")
                    .Write(typeDef.Name)
                    .WriteLine(implements)
                    .WriteStartBlock()
                    .WriteConstructor(typeDef)
                    .Write(typeDef.Members)
                    .WriteEndBlock();

                // TODO: write properties
                // TODO: write "With" functions
                // TODO: write implicit casts 
                // TODO: write operator overloads 
            }

            if (typeDef.IsLibrary())
            {
                return WriteLine("public static partial class Extensions")
                    .WriteStartBlock()
                    .WriteMembersAsExtensionMethods(typeDef)
                    .WriteEndBlock();
            }

            throw new Exception($"Unrecognized kind of type {typeDef.Kind}");
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

                case ConditionalExpressionSymbol conditional:
                    return Write(conditional.Condition)
                        .Indent().WriteLine().Write("? ")
                        .Write(conditional.IfTrue)
                        .WriteLine()
                        .Write(": ")
                        .Write(conditional.IfFalse)
                        .Dedent().WriteLine();

                case FieldDefSymbol fieldDef:
                    return Write("public ").WriteTypeDecl(fieldDef.Type).Write(fieldDef.Name).Write(" { get; }").WriteLine();

                case FunctionSymbol function:
                    return Write(function);
                    
                case FunctionCallSymbol functionResult:
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

                case TypeDefSymbol typeDef:
                    return Write(typeDef);
                
                case TypeRefSymbol typeRef:
                    throw new NotImplementedException("Type references are supposed to be handled in a function");

                case VariableSymbol variable:
                    return Write(variable.Name);
            }

            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}