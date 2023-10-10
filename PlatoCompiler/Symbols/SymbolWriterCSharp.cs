using System.Collections.Generic;
using System;
using System.Linq;
using Plato.Compiler.Ast;
using Plato.Compiler.Types;

namespace Plato.Compiler.Symbols
{
    public class SymbolWriterCSharp : SymbolWriter<SymbolWriterCSharp>
    {
        public SymbolWriterCSharp(Compiler compiler)
            : base(compiler) 
        { }

        public SymbolWriterCSharp WriteTypeDecl(TypeExpression type, string defaultType = "var")
        {
            if (type == null)
                return Write($"{defaultType} ");
            return Write(type.Name).Write(" ");
        }

        public SymbolWriterCSharp WriteSignature(FunctionDefinition function)
        {
            return WriteTypeDecl(function.Type, "void")
                .Write(function.Name)
                .Write("(")
                .WriteCommaList(function.Parameters)
                .Write(")");
        }

        public SymbolWriterCSharp WriteFunctionBody(FunctionDefinition function)
        {
            return WriteStartBlock()
                .Write("return ")
                .Write(function.Body)
                .WriteLine(";")
                .WriteEndBlock();
        }

        public SymbolWriterCSharp Write(FunctionDefinition function)
        {
            // Functions with no bodies are probably intrinsics
            if (function.Body == null)
                return this;

            return WriteSignature(function)
                .WriteFunctionBody(function);
        }

        public static string ParameterizedTypeName(string name, IEnumerable<string> args)
            => name + TypeArgsString(args);

        public static string TypeArgsString(IEnumerable<string> args)
            => args.Any() ? "<" + string.Join(", ", args) + ">" : "";

        public SymbolWriterCSharp WriteLibraryMethods(TypeDefinition type)
        {
            foreach (var mds in type.Methods)
            {
                if (mds.Function.Body == null)
                    // Intrinsic 
                    continue;

                var f = mds.Function;

                if (f.Body == null)
                    // Intrinsic 
                    continue;

                var typeParameterNames =
                    f.Parameters.Count > 0
                        ? f.Parameters[0].Type.Definition.TypeParameters.Select(tp => tp.Name).ToList()
                        : new List<string>();
                var typeArgsString = TypeArgsString(typeParameterNames);

                // TODO: the type parameters are going to be a lot more complicated than this. 
                // We will need constraints.
                // We will need to look at all of the parameters.
                // By default every "Any", will actually be a type argument. 
                // I will also need to figure out how many parameters each thing has (e.g., function, tuple). 
                // NOTE: I should be able to infer the number of parameter of things, based on how they are called. 
                // 

                Write("public static ")
                    .WriteTypeDecl(f.Type, "void")
                    .Write(mds.Name)
                    .Write(typeArgsString)
                    .Write("(")
                    .WriteCommaList(mds.Function.Parameters)
                    .Write(") ")
                    .WriteFunctionBody(mds.Function);
            }

            return this;
        }

        public SymbolWriterCSharp WriteConceptMembersAsExtensionMethods(TypeDefinition type)
        {
            var typeParameterNames = type.TypeParameters.Select(tp => tp.Name).ToList();
            var typeArgsString = TypeArgsString(typeParameterNames.Prepend("Self"));

            var parameterizedTypeName = ParameterizedTypeName(type.Name, typeParameterNames.Prepend("Self"));
            var constraint = $"where Self: {parameterizedTypeName}";

            foreach (var member in type.Members)
            {
                if (!(member is MethodDefinition mds))
                    continue;

                if (mds.Function.Body == null)
                    // Interface method
                    continue;

                Write("public static ")
                    .WriteTypeDecl(mds.Type, "void")
                    .Write(mds.Name)
                    .Write(typeArgsString)
                    .Write("(")
                    .Write(mds.Function.Parameters.Count > 0 ? "this " : "")
                    .WriteCommaList(mds.Function.Parameters)
                    .Write(") ")
                    .WriteLine(constraint)
                    .WriteFunctionBody(mds.Function);
            }

            return this;
        }

        public static string GetTypeName(TypeExpression tr, TypeDefinition parent)
        {
            // TODO: slightly more complicated when actually passing arguments and stuff. 
            var r = tr.Name;
            if (r == "Self")
                return parent.Name;
            return r;
        }

        public static string GetParameterNamesJoined(FunctionDefinition f)
            => string.Join(", ", f.Parameters.Select(p => p.Name));

        public static string GetParameterNamesAndTypesJoined(FunctionDefinition f, TypeDefinition t)
            => string.Join(", ", f.Parameters.Select(p => $"{GetTypeName(p.Type, t)} {p.Name}"));

        public SymbolWriterCSharp WriteConstructor(TypeDefinition t)
        {
            if (!t.IsConcrete()) throw new NotSupportedException();
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
                    {
                        if (op != "[]")
                        {
                            WriteLine($"public static {ret} operator {op}({paramList}) => Extensions.{f.Name}({argList});");
                        }
                        else
                        {
                            var index = f.Parameters[1];
                            Write($"public ")
                                .Write(ret)
                                .Write(" this[").Write(index).Write("]")
                                .WriteLine()
                                .WriteStartBlock()
                                .Write("get")
                                .WriteFunctionBody(f)
                                .WriteEndBlock();
                        }
                    }
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

        public static bool IsSelfType(TypeExpression rs)
            => rs.Definition is SelfType;

        public static bool IsTypePreservingConcept(TypeDefinition type)
        {
            if (!type.IsConcept()) return false;
            foreach (var m in type.Methods)
            {
                if (IsSelfType(m.Function.Type))
                    return true;
                for (var i = 1; i < m.Function.Parameters.Count; ++i)
                    if (IsSelfType(m.Function.Parameters[i].Type))
                        return true;
            }

            return type.Inherits.Any(tr => IsTypePreservingConcept(tr.Definition));
        }

        public SymbolWriterCSharp WriteUnimplementedMethods(TypeDefinition type)
        {
            foreach (var f in type.Methods.Select(m => m.Function))
            {
                if (f.Body != null)
                    continue;
                WriteSignature(f).WriteLine(";");
            }

            return this;
        }

        public override SymbolWriterCSharp Write(TypeDefinition type)
        {
            if (type.IsConcept())
            {
                // TODO: get and append the type parameters 

                var fullName = $"{type.Name}<Self>";
                var inherited = type.Inherits.Count > 0
                    ? ": " + string.Join(", ", type.Inherits.Select(td => $"{td?.Name}<Self>"))
                    : "";

                Write("public interface ").Write(fullName)
                    .WriteLine(inherited)
                    .Indent()
                    .WriteLine($"where Self : {fullName}")
                    .Dedent()
                    .WriteStartBlock()
                    .WriteUnimplementedMethods(type)
                    .WriteEndBlock();

                WriteLine("public static partial class Extensions")
                    .WriteStartBlock()
                    .WriteConceptMembersAsExtensionMethods(type)
                    .WriteEndBlock();

                return this;
            }

            if (type.IsConcrete())
            {
                var implements = type.Implements.Count > 0
                    ? ": " + string.Join(", ", type.Implements.Select(td => $"{td?.Name}<{type.Name}>"))
                    : "";

                return Write("public class ")
                    .Write(type.Name)
                    .WriteLine(implements)
                    .WriteStartBlock()
                    .WriteConstructor(type)
                    .Write(type.Members)
                    .WriteEndBlock();

                // TODO: write properties
                // TODO: write "With" functions
                // TODO: write implicit casts 
                // TODO: write operator overloads 
            }

            if (type.IsLibrary())
            {
                return WriteLine("public static partial class Extensions")
                    .WriteStartBlock()
                    .WriteLibraryMethods(type)
                    .WriteEndBlock();
            }

            if (type.IsPrimitive())
            {
                // TODO: output the primitives 
                return this;
            }

            return this;
        }

        public override SymbolWriterCSharp Write(DefinitionSymbol value)
        {
            switch (value)
            {
                case null:
                    return Write("null");

                case FieldDefinition fieldDef:
                    return Write("public ").WriteTypeDecl(fieldDef.Type).Write(fieldDef.Name).Write(" { get; }")
                        .WriteLine();

                case FunctionDefinition function:
                    return Write(function);

                case FunctionGroupDefinition memberGroup:
                    return Write(memberGroup.Name);

                case MethodDefinition methodDef:
                    return Write("public static ").Write(methodDef.Function);

                case MemberDefinition member:
                    throw new Exception("Not implemented");

                case ParameterDefinition parameter:
                    return WriteTypeDecl(parameter.Type).Write(parameter.Name);

                case VariableDefinition variable:
                    return Write(variable.Name);

                case PredefinedDefinition predefined:
                    return Write(predefined.Name);
            }

            return this;
        }

        public override SymbolWriterCSharp Write(TypeExpression typeExpression)
        {
            return WriteTypeDecl(typeExpression);
        }

        public override SymbolWriterCSharp Write(Expression expr)
        {
            if (expr == null)
                return this;

            var t = Compiler.GetExpressionType(expr);

            // Don't put types in front of an argument (it is the same as the other)
            if (!(expr is Argument))
                WriteLine($"/* {t} */");

            switch (expr)
            {
                case Reference refSymbol:
                    return Write(refSymbol.Name);

                case Argument argument:
                    return Write(argument.Expression);

                case Assignment assignment:
                    return Write(assignment.LValue)
                        .Write(" = ")
                        .Write(assignment.RValue);

                case ConditionalExpression conditional:
                    return Write(conditional.Condition)
                        .Indent().WriteLine().Write("? ")
                        .Write(conditional.IfTrue)
                        .WriteLine()
                        .Write(": ")
                        .Write(conditional.IfFalse)
                        .Dedent().WriteLine();

                case FunctionCall functionResult:
                    return Write(functionResult.Function).Write("(")
                        .WriteCommaList(functionResult.Args).Write(")");

                case Literal literal:
                    return Write(literal.Value.ToString());

                case Lambda lambda:
                    return Write("(")
                        .WriteCommaList(lambda.Function.Parameters)
                        .WriteLine(") => ")
                        //.WriteConstraints(function)
                        .Write(lambda.Function.Body);

            }

            throw new ArgumentOutOfRangeException(nameof(expr));
        }
    }
}