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
        public Compiler.Compiler Compiler { get; }
        
        public Dictionary<string, StringBuilder> Files { get; } = new Dictionary<string, StringBuilder>();

        public DirectoryPath OutputFolder { get; }

        public static HashSet<string> IgnoreTypes = new HashSet<string>()
        {
            "Array1",
            "Tuple2",
            "Tuple3",
            "Function0",
            "Function1",
            "Function2",
            "Function3",
        };

        public static HashSet<string> IntrinsicTypes = new HashSet<string>()
        {
            "Number",
            "Boolean",
            "Integer",
            "Character",
            "String",
        };

        public SymbolWriterCSharp(Compiler.Compiler compiler, DirectoryPath outputFolder)
        {
            Compiler = compiler;
            OutputFolder = outputFolder;
        }

        public SymbolWriterCSharp WriteAll()
        {
            WriteConceptInterfaces();
            WriteTypeImplementations();
            WriteLibraryMethodsOnTypes();
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
            foreach (var fa in ci.Functions)
                WriteAnalysis(fa, indent);
            foreach (var ci2 in ci.Inherited)
                WriteConceptImplementation(ci2, indent + "  ");
        }

        public void WriteAnalyses()
        {
            StartNewFile(OutputFolder.RelativeFile("analysis.txt"));
            foreach (var ta in Compiler.ConcreteTypes)
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

        public SymbolWriterCSharp WriteFunction(FunctionInstance f, ConcreteType t)
        {
            var ret = f.GetTypeString(f.ReturnType);

            var argList = f
                .Parameters.Skip(1)
                .Select(p => $"{p.Name}")
                .JoinStringsWithComma();

            var firstName = f.Parameters.Count > 0 ? f.Parameters[0].Name : "";

            var paramList = f
                .Parameters.Skip(1)
                .Select(p => $"{f.GetTypeString(p.Type)} {p.Name}")
                .JoinStringsWithComma();

            var staticParamList = f
                .Parameters
                .Select(p => $"{f.GetTypeString(p.Type)} {p.Name}")
                .JoinStringsWithComma();

            var paramTypeList = f
                .Parameters.Skip(1)
                .Select(p => $"{f.GetTypeString(p.Type)}")
                .JoinStringsWithComma();

            var sig = $"{f.Name}({paramTypeList}):{ret}";
            
            if (paramList.Length > 0)
            {
                WriteLine($"public {ret} {f.Name}({paramList}) => throw new NotImplementedException();");
            }
            else
            {
                WriteLine($"public {ret} {f.Name} => throw new NotImplementedException();");
            }

            if (f.Parameters.Count == 2)
            {
                var op = OperatorNameLookup.NameToBinaryOperator(f.Name);

                // NOTE: this is because in C#, the "||" and "&&" operators cannot be overridden.
                if (op == "||") op = "|";
                if (op == "&&") op = "&";

                if (op != null)
                {
                    if (op != "[]")
                    {
                        WriteLine(
                            $"public static {ret} operator {op}({staticParamList}) => {firstName}.{f.Name}({argList});");
                    }
                    else
                    {
                        var index = f.Parameters[1];
                        Write($"public ")
                            .Write(ret)
                            .Write(" this[")
                            .Write(index)
                            .Write($"] => {f.Name}({index.Name});")
                            .WriteLine();
                    }
                }
            }

            if (f.Parameters.Count == 1)
            {
                var op = OperatorNameLookup.NameToUnaryOperator(f.Name);
                if (op != null)
                    WriteLine($"public static {ret} operator {op}({staticParamList}) => {firstName}.{f.Name};");
            }

            return this;
        }

        public SymbolWriterCSharp WriteLibraryMethodsOnTypes()
        {
            StartNewFile(OutputFolder.RelativeFile("Library.cs"));
            WriteLine("using System;");

            foreach (var t in Compiler.ConcreteTypes)
            {
                WriteLine($"public partial class {t.Type.Name}");
                WriteStartBlock();

                foreach (var f in t.ConcreteFunctions)
                {
                    WriteFunction(f, t);
                }

                foreach (var f in t.GetConceptFunctions())
                {
                    WriteFunction(f, t);
                }

                WriteEndBlock();
            }

            return this;
        }

        public SymbolWriterCSharp WriteLibraries()
        {
            StartNewFile(OutputFolder.RelativeFile("Library.cs"));
            WriteLine("using System;");
            foreach (var c in Compiler.AllTypeAndLibraryDefinitions)
                if (c.IsLibrary())
                    WriteLibraryMethods(c);
            return this;
        }

        public SymbolWriterCSharp WriteConceptInterfaces()
        {
            StartNewFile(OutputFolder.RelativeFile("Concepts.cs"));
            foreach (var c in Compiler.AllTypeAndLibraryDefinitions)
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
            foreach (var c in Compiler.AllTypeAndLibraryDefinitions)
                if (c.IsConcrete() && !IgnoreTypes.Contains(c.Name))
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
        
        public SymbolWriterCSharp WriteTypeImplementation(TypeDefinition t)
        {
            Debug.Assert(t.IsConcrete());

            var implements = t.Implements.Count > 0
                ? ": " + string.Join(", ", t.Implements.Select(te => ImplementedTypeString(te, t.Name)))
                : "";

            Write("public partial class ");
            Write(t.Name);
            WriteLine(implements);
            WriteStartBlock();

            var name = t.Name;
            var fieldTypes = t.Fields.Select(f => TypeStr(f.Type)).ToList();
            var fieldNames = t.Fields.Select(f => f.Name).ToList();
            var parameterNames = fieldNames.Select(FieldNameToParameterName);
            Debug.Assert(fieldTypes.Count == fieldNames.Count);

            var isIntrinsic = IntrinsicTypes.Contains(name);
            if (fieldTypes.Count > 0)
                if (isIntrinsic)
                    throw new Exception($"Intrinsic {name} should not have fields");

            if (fieldTypes.Count == 0)
            {
                if (!isIntrinsic)
                    throw new Exception($"Only intrinsics can have no fields not {name}");

                fieldNames.Add("Value");
                if (name == "String")
                    fieldTypes.Add("string");
                else if (name == "Number")
                    fieldTypes.Add("double");
                else if (name == "Boolean")
                    fieldTypes.Add("bool");
                else if (name == "Integer")
                    fieldTypes.Add("int");
                else if (name == "Character")
                    fieldTypes.Add("char");
                else if (name == "String")
                    fieldTypes.Add("string");
                else
                    throw new Exception($"Not a recognized intrinsic {name}");
            }

            for (var i = 0; i < fieldTypes.Count; ++i)
            {
                var ft = fieldTypes[i];     
                var fn = fieldNames[i];
                WriteLine($"public {ft} {fn} {{ get; }}");
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
            WriteLine($"public {name}({parametersStr}) => ({fieldNamesStr}) = ({parameterNamesStr});");
            
            // Parameterless constructor
            WriteLine($"public {name}() {{ }}");

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

            /* TODO:
            var fieldNamesAsStrings = string.Join(", ", fieldNames.Select(f => $"\"{f}\""));
            WriteLine($"public string[] FieldNames() => new[] {{ {fieldNamesAsStrings} }};");
            WriteLine($"public object[] FieldValues() => new[] {{ {fieldNamesString} }};");
            */

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

            return WriteSignature(function, false, false)
                .WriteFunctionBody(function);
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

        public SymbolWriterCSharp WriteLibraryMethods(TypeDefinition type)
        {
            Debug.Assert(type.IsLibrary());

            // We put all methods 
            WriteLine($"public static partial class Library");
            WriteStartBlock();
            foreach (var f in type.Functions)
            {
                var typeParameters = GatherTypeParameters(f).Distinct().ToList();

                Write("public static ");
                Write(f.ReturnType);
                Write(f.Name);
                if (f.Parameters.Count > 0)
                {
                    if (typeParameters.Count > 0)
                    {
                        Write("<");
                        WriteCommaList(typeParameters);
                        Write(">");
                    }

                    Write("(");
                    Write("this ");
                    for (var i = 0; i < f.Parameters.Count; ++i)
                    {
                        if (i > 0) Write(", ");
                        var p = f.Parameters[i];
                        Write(p.Type);
                        Write(p.Name);
                    }
                    Write(")");
                }

                if (f.Body is BlockExpression expr && expr.Symbols.Count > 1)
                {
                    Write(expr).WriteLine();
                }
                else
                {
                    if (f.Body != null)
                    {
                        Write(" => ").Write(f.Body).WriteLine(";");
                    }
                    else
                    {
                        WriteLine("throw new NotImplementedException();");
                    }
                }
            }
            WriteEndBlock();
            return this;
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
                    return Write(function);

                case FunctionGroupDefinition memberGroup:
                    return Write(memberGroup.Name);

                case MethodDefinition methodDef:
                    return Write("public static ").Write(methodDef.Function);

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

        public SymbolWriterCSharp Write(Expression expr)
        {
            if (expr == null)
                return this;

            var t = Compiler.GetExpressionType(expr);
            
            // Don't put types in front of an argument (it is the same as the other)
            //if (!(expr is Argument)) WriteLine($"/* {t} */");

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

                case FunctionCall functionCall:
                    // Turn it into a "dot" call.
                    // TODO: If this is a function argument then we can't do the "." call transformation. 
                    if (functionCall.Args.Count == 0)
                        return Write(functionCall.Function);
                    Write(functionCall.Args[0]).Write(".").Write(functionCall.Function);
                    if (functionCall.Args.Count == 1)
                        return this;
                    return Write("(").WriteCommaList(functionCall.Args.Skip(1)).Write(")");

                case Literal literal:
                    return Write(literal.Value.ToString());

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