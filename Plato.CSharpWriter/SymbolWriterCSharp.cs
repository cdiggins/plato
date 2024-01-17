using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ara3D.Utils;
using Plato.AST;
using Plato.Compiler.Analysis;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    public class SymbolWriterCSharp : CodeBuilder<SymbolWriterCSharp>
    {
        public const bool EmitInterfaces = true;
        public const bool EmitStructsInsteadOfClasses = true;

        public Compiler.Compilation Compilation { get; }
        
        public Dictionary<string, StringBuilder> Files { get; } = new Dictionary<string, StringBuilder>();

        public DirectoryPath OutputFolder { get; }

        public static HashSet<string> IgnoredTypes = new HashSet<string>()
        {
            "Dynamic",
            "Array1",
            "Function0",
            "Function1",
            "Function2",
            "Function3",
        };

        public static HashSet<string> IgnoredFunctions = new HashSet<string>()
        {
            "FieldNames",
            "FieldValues",
            "TypeName",
        };

        public static Dictionary<string, string> PrimitiveTypes = new Dictionary<string, string>()
        {
            { "Number", "double" },
            { "Boolean", "bool" },
            { "Integer", "int" },
            { "Character", "char" },
            { "String", "string" },
            { "Dynamic", "object" },
        };

        public SymbolWriterCSharp(Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            Compilation = compilation;
            OutputFolder = outputFolder;
        }

        public SymbolWriterCSharp WriteAll()
        {
            WriteConceptInterfaces();
            WriteTypeImplementations();
            WriteLibraryMethods();
            WriteAnalyses();
            return this;
        }

        public void WriteAnalysis(FunctionInstance fa, string indent = "  ")
        {
            var parameters = fa.Implementation.Parameters.Select(p => $"{p.Type} {p.Name}").JoinStringsWithComma();
            WriteLine($"{indent}Function: {fa.Implementation.Name}({parameters})");
        }

        public void WriteConceptImplementation(ConceptImplementation ci, string indent = "  ")
        {
            WriteLine($"{indent}Concept={ci.Concept.Name} Expr={ci.Expression} Subs={ci.Substitutions}");
            foreach (var fa in ci.ImplementedFunctions)
                WriteAnalysis(fa, indent);
            foreach (var ci2 in ci.Children)
                WriteConceptImplementation(ci2, indent + "  ");
        }

        public void WriteAnalyses()
        {
            StartNewFile(OutputFolder.RelativeFile("analysis.txt"));
            foreach (var ta in Compilation.ConcreteTypes)
            {
                WriteLine($"Type analysis for {ta.Type.Name}");
                foreach (var ci in ta.Concepts)
                {
                    WriteConceptImplementation(ci);
                }
                foreach (var fa in ta.ConcreteFunctions)
                {
                    WriteAnalysis(fa);
                }
            }
        }

        public void StartNewFile(string fileName)
        {
            sb = new StringBuilder();
            Files.Add(fileName, sb);
        }

        public SymbolWriterCSharp Write(IEnumerable<Symbol> symbols)
            => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

        public SymbolWriterCSharp WriteCommaList(IEnumerable<Symbol> symbols) 
            => WriteCommaList(symbols, (w, s) => w.Write(s));

        public SymbolWriterCSharp WriteCommaList(IEnumerable<string> symbols)
            => WriteCommaList(symbols, (w, s) => w.Write(s));

        public SymbolWriterCSharp Write(Symbol symbol)
        {
            switch (symbol)
            {
                case DefinitionSymbol definition:
                    return Write(definition);
                case Expression expression:
                    return Write(expression);
                case TypeDefinition typeDefinition:
                    return Write(typeDefinition);
                case TypeExpression typeExpression:
                    return Write(typeExpression);
                case LoopSymbol loopSymbol:
                    return Write("throw new NotImplementedException(\"Loops not implemented yet!\");");
                default:
                    throw new ArgumentOutOfRangeException(nameof(symbol));
            }
        }

        public SymbolWriterCSharp WriteFunction(FunctionInstance f, ConcreteType t, bool generateImpl = false)
        {
            var ret = f.ReturnType;

            var funcTypeParams = f.UsedTypeParameters
                .Where(tp => !t.Type.TypeParameters.Contains(tp)).ToList();

            var funcTypeParamsStr = funcTypeParams.Count == 0
                ? ""
                : $"<{funcTypeParams.Select(tp => tp.Name).JoinStringsWithComma()}>";

            var argList = f
                .ParameterNames.Skip(1)
                .JoinStringsWithComma();

            var firstName = f.ParameterNames.FirstOrDefault() ?? "";

            var paramList = f
                .ParameterTypes.Skip(1)
                .Zip(f.ParameterNames.Skip(1), (pt, pn) => $"{pt} {pn}")
                .JoinStringsWithComma();

            if (paramList.Length > 0)
                paramList = $"({paramList})";

            var staticParamList = f
                .ParameterTypes
                .Zip(f.ParameterNames, (pt, pn) => $"{pt} {pn}")
                .JoinStringsWithComma();

            if (f.Implementation.Body != null && f.Name != "Aggregate")
            {
                WriteLine($"public {ret} {f.Name}{funcTypeParamsStr}{paramList} =>")
                    .Indent()
                    .Write(f.Implementation.Body)
                    .WriteLine(";")
                    .Dedent();
            }
            else
            {
                var fields = t.Type.Fields.Select(field => field.Name).ToList();
                var sep = ret.Name == "Boolean" ? " & " : ", ";

                var impl = "throw new NotImplementedException()";
                if (generateImpl)
                {
                    if (f.ParameterNames.Count <= 1)
                    {
                        var args = fields.Select(field => $"{field}.{f.Name}");
                        impl = $"({args.JoinStrings(sep)})";
                    }
                    else if (f.ParameterNames.Count == 2)
                    {
                        var p0 = f.ParameterNames[1];
                        var args = fields.Select(field => $"{field}.{f.Name}({p0}.{field})");
                        impl = $"({args.JoinStrings(sep)})";
                    }
                    else if (f.ParameterNames.Count == 3)
                    {
                        var p0 = f.ParameterNames[1];
                        var p1 = f.ParameterNames[2];
                        var args = fields.Select(field => $"{field}.{f.Name}({p0}.{field}, {p1}.{field})");
                        impl = $"({args.JoinStrings(sep)})";
                    }
                    else if (f.ParameterNames.Count == 4)
                    {
                        var p0 = f.ParameterNames[1];
                        var p1 = f.ParameterNames[2];
                        var p2 = f.ParameterNames[3];
                        var args = fields.Select(field =>
                            $"{field}.{f.Name}({p0}.{field}, {p1}.{field}, {p2}.{field})");
                        impl = $"({args.JoinStrings(sep)})";
                    }
                    else
                    {
                        throw new Exception(
                            "Cannot generate default implementations for functions with more than 2 parameters");
                    }
                }

                WriteLine($"public {ret} {f.Name}{funcTypeParamsStr}{paramList} => {impl};");
            }

            if (f.ParameterNames.Count == 2)
            {
                var op = Operators.NameToBinaryOperator(f.Name);

                // NOTE: this is because in C#, the "||" and "&&" operators cannot be overridden.
                if (op == "||") op = "|";
                if (op == "&&") op = "&";

                if (op != null)
                {
                    if (op != "[]")
                    {
                        WriteLine(
                            $"public static {ret} operator {op}{funcTypeParamsStr}({staticParamList}) => {firstName}.{f.Name}({argList});");
                    }
                    else
                    {
                        var paramList2 = paramList.Replace('(', '[').Replace(')', ']');
                        WriteLine($"public {ret} this{funcTypeParamsStr}{paramList2} => {f.Name}({argList});");
                    }
                }
            }

            if (f.ParameterNames.Count == 1)
            {
                var op = Operators.NameToUnaryOperator(f.Name);
                if (op != null)
                    WriteLine($"public static {ret} operator {op}({staticParamList}) => {firstName}.{f.Name};");
            }

            return this;
        }

        public SymbolWriterCSharp WriteLibraryMethods()
        {
            StartNewFile(OutputFolder.RelativeFile("Library.cs"));
            WriteLine("using System;");

            WriteLine($"public static class Constants");
            WriteStartBlock();
            foreach (var f in Compilation.Libraries.AllConstants())
            {
                var retType = f.ReturnType;
                Write($"public static {retType.Name} {f.Name} => ")
                    .Write(f.Body)
                    .WriteLine(";");
            }
            WriteEndBlock();

            foreach (var t in Compilation.ConcreteTypes)
            {
                var typeParamsStr = JoinTypeParameters(t.Type.TypeParameters.Select(tp => tp.Name));

                var kind = EmitStructsInsteadOfClasses ? "readonly partial struct" : "partial class";
                WriteLine($"public {kind} {t.Type.Name}{typeParamsStr}");
                WriteStartBlock();

                var funcGroups = t.ConcreteFunctions.Concat(t.GetConceptFunctions()).GroupBy(f => f.SignatureId);

                foreach (var g in funcGroups)
                {
                    WriteFunction(g.First(), t);
                }

                WriteEndBlock();
            }

            return this;
        }
        
        public SymbolWriterCSharp WriteConceptInterfaces()
        {
            StartNewFile(OutputFolder.RelativeFile("Concepts.cs"));
            foreach (var c in Compilation.AllTypeAndLibraryDefinitions)
                if (c.IsConcept())
                    WriteConceptInterface(c);
            return this;
        }

        public static string JoinTypeParameters(IEnumerable<string> parameters)
        {
            var r = parameters.JoinStrings(", ");
            return r.Length == 0 ? r : $"<{r}>";
        }

        public string TypeStr(TypeExpression type)
        {
            return type.Name + JoinTypeParameters(type.TypeArgs.Select(TypeStr));
        }

        public string TypeAsInherited(TypeExpression type)
        {
            return type.Name + JoinTypeParameters(type.Definition.IsSelfConstrained() 
                ? type.TypeArgs.Select(TypeStr).Prepend("Self") 
                : type.TypeArgs.Select(TypeStr));
        }

        public SymbolWriterCSharp WriteConceptInterface(TypeDefinition type)
        {
            Debug.Assert(type.IsConcept());

            var typeParams = JoinTypeParameters(type.IsSelfConstrained()
                ? type.TypeParameters.Select(tp => tp.Name).Prepend("Self")
                : type.TypeParameters.Select(tp => tp.Name));

            var fullName = $"{type.Name}{typeParams}";
            var inherited = type.Inherits.Count > 0
                ? ": " + type.Inherits.Select(TypeAsInherited).JoinStrings(", ")
                : "";

            Write("public interface ").Write(fullName).WriteLine(inherited);

            foreach (var tp in type.TypeParameters)
            {
                foreach (var constraint in tp.Constraints)
                {
                    if (constraint.Name == "Any")
                        continue;
                    var constraintArgs = JoinTypeParameters(constraint.Definition.IsSelfConstrained()
                        ? constraint.TypeArgs.Select(TypeStr).Prepend(tp.Name)
                        : constraint.TypeArgs.Select(TypeStr));
                    WriteLine($"where {tp.Name} : {constraint.Name}{constraintArgs}");
                }
            }

            WriteStartBlock();
            foreach (var m in type.Methods)
                WriteSignature(m.Function, true, true);

            WriteEndBlock();
            return this;
        }
        
        public SymbolWriterCSharp WriteTypeImplementations()
        {
            StartNewFile(OutputFolder.RelativeFile("Types.cs"));
            WriteLine("using System;");
            foreach (var c in Compilation.ConcreteTypes)
                if (!IgnoredTypes.Contains(c.Type.Name))
                    WriteTypeImplementation(c);
            return this;
        }

        public string ImplementedTypeString(TypeExpression te, string typeName)
        {
            var typeArgs = te.TypeArgs.Select(TypeStr);
            if (te.Definition.IsSelfConstrained())
                typeArgs = typeArgs.Prepend(typeName);
            return $"{te.Name}{JoinTypeParameters(typeArgs)}";
        }

        public static string FieldNameToParameterName(string fieldName)
            => fieldName.Length == 0 || fieldName[0].IsLower() 
                ? $"_{fieldName}" 
                : fieldName.DecapitalizeFirst();
        
        public SymbolWriterCSharp WriteTypeImplementation(ConcreteType concreteType)
        {
            var t = concreteType.Type;
            var implements = EmitInterfaces && t.Implements.Count > 0
                ? ": " + string.Join(", ", t.Implements.Select(te => ImplementedTypeString(te, t.Name)))
                : "";

            var typeParamsStr = JoinTypeParameters(t.TypeParameters.Select(tp => tp.Name));

            var kind = EmitStructsInsteadOfClasses ? "readonly partial struct" : "partial class";
            Write($"public {kind} ");
            
            Write(t.Name);
            Write(typeParamsStr);
            WriteLine(implements);
            WriteStartBlock();

            var name = t.Name + typeParamsStr;
            var fieldTypes = t.Fields.Select(f => TypeStr(f.Type)).ToList();
            var fieldNames = t.Fields.Select(f => f.Name).ToList();
            var parameterNames = fieldNames.Select(FieldNameToParameterName);
            Debug.Assert(fieldTypes.Count == fieldNames.Count);

            var isPrimitive = PrimitiveTypes.ContainsKey(name);
            if (fieldTypes.Count == 0 ^ isPrimitive)
                throw new Exception($"Primitive {name} should not have fields");

            if (fieldTypes.Count == 0)
            {
                if (!isPrimitive)
                    throw new Exception($"Only primitives can have no fields not {name}");
                fieldNames.Add("Value");
                fieldTypes.Add(PrimitiveTypes[name]);
            }

            for (var i = 0; i < fieldTypes.Count; ++i)
            {
                var ft = fieldTypes[i];     
                var fn = fieldNames[i];
                WriteLine($"public readonly {ft} {fn};");
            }

            for (var i = 0; i < fieldTypes.Count; ++i)
            {
                var ft = fieldTypes[i];
                var fn = fieldNames[i];
                var pn = FieldNameToParameterName(fn);
                var args = fieldNames.Select((n, j) => j == i ? pn : n).JoinStringsWithComma();
                WriteLine($"public {name} With{fn}({ft} {pn}) => ({args});");
            }

            var parameters = fieldTypes.Zip(parameterNames, (pt, pn) => $"{pt} {pn}");
            var parameterNamesStr = parameterNames.JoinStringsWithComma();
            var parametersStr = parameters.JoinStringsWithComma();
            var deconstructorParametersStr = fieldTypes.Zip(parameterNames, (pt, pn) => $"out {pt} {pn}").JoinStringsWithComma();

            var fieldTypesStr = string.Join(", ", fieldTypes);
            var fieldNamesStr = fieldNames.JoinStringsWithComma();

            // Regular Constructor 
            WriteLine($"public {t.Name}({parametersStr}) => ({fieldNamesStr}) = ({parameterNamesStr});");
            
            // Parameterless constructor
            if (!EmitStructsInsteadOfClasses)
                WriteLine($"public {t.Name}() {{ }}");

            WriteLine($"public static {name} Default = new {name}();");

            // Static factory function (New)
            WriteLine($"public static {name} New({parametersStr}) => new {name}({parameterNamesStr});");
            
            // Implicit operators 
            if (fieldNames.Count > 1)
            {
                // Implicit value-tuple operator 
                var qualifiedFieldNames = fieldNames.Select(f => $"self.{f}").JoinStringsWithComma();
                var tupleNames = string.Join(", ", Enumerable.Range(1, fieldNames.Count).Select(i => $"value.Item{i}"));
                WriteLine($"public static implicit operator ({fieldTypesStr})({name} self) => ({qualifiedFieldNames});");
                WriteLine($"public static implicit operator {name}(({fieldTypesStr}) value) => new {name}({tupleNames});");

                // Deconstructor function 
                Write($"public void Deconstruct({deconstructorParametersStr}) {{ ");
                foreach (var fn in fieldNames)
                    Write($"{FieldNameToParameterName(fn)} = {fn}; ");
                WriteLine("}");
            }
            else if (fieldNames.Count == 1)
            {
                // Implicit operator to/from the field 
                var fieldName = fieldNames[0];
                var fieldType = fieldTypes[0];

                WriteLine($"public static implicit operator {fieldType}({name} self) => self.{fieldName};");
                WriteLine($"public static implicit operator {name}({fieldType} value) => new {name}(value);");
            }

            WriteLine($"public String TypeName => {name.Quote()};");

            var fieldNamesAsStringsStr = fieldNames.Select(n => $"(String){n.Quote()}").JoinStringsWithComma();
            WriteLine($"public Array<String> FieldNames => (Array1<String>)new[] {{ {fieldNamesAsStringsStr} }};");
            var fieldValuesAsDynamicsStr = fieldNames.Select(n => $"new Dynamic({n})").JoinStringsWithComma();
            WriteLine($"public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] {{ {fieldValuesAsDynamicsStr} }};");

            /* TODO:    
            var fieldNamesAsStrings = string.Join(", ", fieldNames.Select(f => $"\"{f}\""));
            WriteLine($"public string[] FieldNames() => new[] {{ {fieldNamesAsStrings} }};");
            WriteLine($"public object[] FieldValues() => new[] {{ {fieldNamesString} }};");
            */

            // For the primitives, we have predefined implementations of the standard concepts .
            if (!isPrimitive)
            {
                WriteLine("// Unimplemented concept functions");
                foreach (var f in concreteType.UnimplementedFunctions)
                {
                    if (IgnoredFunctions.Contains(f.Name))
                        continue;

                    // Only generate implementations when the parameters and the return type are the same as this type
                    var genImpl = f.ParameterTypes.All(pt => pt.Name == concreteType.Name)
                                  && (f.ReturnType.Name == concreteType.Name || f.ReturnType.Name == "Boolean");
                    WriteFunction(f, concreteType, genImpl);
                }
            }

            WriteEndBlock();
            return this;
        }
        
        public SymbolWriterCSharp WriteSignature(FunctionDefinition function, bool skipFirstParameter, bool eos)
        {
            Write(function.Type);
            Write(" ");
            Write(function.Name);

            if (function.Parameters.Count > 1)
            {
                var r = Write("(")
                    .WriteCommaList(skipFirstParameter
                        ? function.Parameters.Skip(1)
                        : function.Parameters)
                    .Write(")");
                if (eos)
                     r = r.WriteLine(";");
                return r;
            }
            else
            {
                var r = Write(" { get; }");
                if (eos) r = r.WriteLine();
                return r;
            }
        }

        public static string ParameterizedTypeName(string name, IEnumerable<string> args)
            => name + TypeArgsString(args);

        public static string TypeArgsString(IEnumerable<string> args)
            => args.Any() ? "<" + string.Join(", ", args) + ">" : "";

        public FilePath ToFileName(TypeDefinition type)
            => OutputFolder.RelativeFile($"{type.Kind}_{type.Name}.cs");
        
        public static void GatherTypeParameters(TypeExpression te, List<string> set)
        {
            if (te.Definition.IsSelfConstrained())
                set.Add("Self");
            if (te.Name.StartsWith("$"))
                set.Add("T" + te.Name.Substring(1));
            foreach (var arg in te.TypeArgs)
                GatherTypeParameters(arg, set);
        }

        public static IEnumerable<string> GatherTypeParameters(FunctionDefinition fd)
        {
            var r = new List<string>();
            foreach (var param in fd.Parameters)
                GatherTypeParameters(param.Type, r);
            return r;
        }

        public SymbolWriterCSharp Write(DefinitionSymbol value)
        {
            switch (value)
            {
                case null:
                    return Write("null");

                case FieldDefinition fieldDef:
                    return Write("public ").Write(fieldDef.Type).Write(fieldDef.Name).Write(" { get; }")
                        .WriteLine();

                case FunctionDefinition function:
                    throw new Exception("Not implemented");

                case FunctionGroupDefinition memberGroup:
                    return Write(memberGroup.Name);

                case MethodDefinition methodDef:
                    throw new Exception("Not implemented");

                case MemberDefinition member:
                    throw new Exception("Not implemented");

                case ParameterDefinition parameter:
                    return Write(parameter.Type).Write(parameter.Name);

                case VariableDefinition variable:
                    return Write(variable.Name);

                case PredefinedDefinition predefined:
                    return Write(predefined.Name);
            }

            return this;
        }

        public SymbolWriterCSharp Write(TypeExpression typeExpression)
        {
            return Write(TypeStr(typeExpression)).Write(" ");
        }

        public static string GetLiteralType(Literal literal)
        {
            return literal.TypeEnum.ToString();
        }

        public static string GetLiteralValue(Literal literal)
        {
            return literal.Value.ToLiteralString();
        }

        public SymbolWriterCSharp Write(Expression expr)
        {
            if (expr == null)
                return this;

            //var t = Compiler.GetExpressionType(expr);
            //if (!(expr is Argument)) WriteLine($"/* {t} */");

            switch (expr)
            {
                case ParameterReference pr:
                    return pr.Definition.Index == 0 
                        ? Write("this") 
                        : Write(pr.Name);

                case FunctionGroupReference fgr:
                    // HACK: check if it is a constant.
                    // TODO: I need to have all function calls properly resolved to generate better quality code. 
                    if (fgr.Definition.Functions.Count == 1 &&
                        fgr.Definition.Functions[0].NumParameters == 0)
                        return Write($"Constants.{fgr.Name}");
                    return Write(fgr.Name);

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
                        .Write(" ? ")
                        .Write(conditional.IfTrue)
                        .Write(" : ")
                        .Write(conditional.IfFalse);

                case FunctionCall functionCall:
                    // Turn it into a "dot" call.
                    // TODO: If this is a lambda then we can't do the "." call transformation. 
                    
                    // If there are no arguments, it is a constant.
                    if (functionCall.Args.Count == 0)
                        return Write("Constants.").Write(functionCall.Function);

                    Write(functionCall.Args[0]).Write(".").Write(functionCall.Function);
                    if (functionCall.Args.Count == 1)
                        return this;
                    return Write("(").WriteCommaList(functionCall.Args.Skip(1)).Write(")");

                case Literal literal:
                    return Write($"(({GetLiteralType(literal)}){GetLiteralValue(literal)})");

                case Lambda lambda:
                    return Write("(")
                        .WriteCommaList(lambda.Function.Parameters)
                        .WriteLine(") => ")
                        //.WriteConstraints(function)
                        .Write(lambda.Function.Body);

                case BlockExpression block:
                {
                    if (block.Symbols.Count == 1)
                    {
                        return Write(block.Symbols[0]);
                    }
                    else
                    {
                        WriteStartBlock();
                        foreach (var x in block.Symbols)
                        {
                            Write(x);
                            WriteLine(";");
                        }
                        return WriteEndBlock();
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(expr));
        }
    }
}