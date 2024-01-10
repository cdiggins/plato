using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ara3D.Utils;
using Plato.Compiler.Ast;
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

        public void StartNewFile(string fileName)
        {
            sb = new StringBuilder();
            Files.Add(fileName, sb);
        }

        public SymbolWriterCSharp Write(IEnumerable<Symbol> symbols)
            => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

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
                default:
                    throw new ArgumentOutOfRangeException(nameof(symbol));
            }
        }

        public SymbolWriterCSharp WriteAll()
        {
            WriteConceptInterfaces();
            //WriteConceptExtensions();
            WriteTypeImplementations();
            WriteLibraries();
            return this;
        }

        public SymbolWriterCSharp WriteLibraries()
        {
            StartNewFile(OutputFolder.RelativeFile("Library.cs"));

            foreach (var c in Compiler.AllTypeAndLibraryDefinitions)
            {
                if (c.IsLibrary())
                {
                    WriteLibraryMethods(c);
                }
            }

            return this;
        }

        public SymbolWriterCSharp WriteConceptInterfaces()
        {
            StartNewFile(OutputFolder.RelativeFile("Concepts.cs"));

            foreach (var c in Compiler.AllTypeAndLibraryDefinitions)
            {
                if (c.IsConcept())
                {
                    WriteConceptInterface(c);
                }
            }

            return this;
        }

        public static string JoinTypeParameters(IEnumerable<string> parameters)
        {
            var r = parameters.JoinStrings(", ");
            if (r.Length == 0)
                return r;
            return $"<{r}>";
        }

        public static string TypeAsInherited(TypeExpression type)
        {
            return type.Name + JoinTypeParameters(type.Definition.IsSelfConstrained() 
                ? type.TypeArgs.Select(t => ToString(t)).Prepend("Self") 
                : type.TypeArgs.Select(t => ToString(t)));
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
                if (tp.Constraint == null)
                    continue;
                // "Any" constraints aren't real constraints 
                if (tp.Constraint.Name == "Any")
                    continue;
                var constraintArgs = JoinTypeParameters(tp.Constraint.Definition.IsSelfConstrained()
                    ? tp.Constraint.TypeArgs.Select(t => ToString(t)).Prepend(tp.Name)
                    : tp.Constraint.TypeArgs.Select(t => ToString(t)));                
                WriteLine($"where {tp.Name} : {tp.Constraint.Name}{constraintArgs}");
            }

            WriteStartBlock();

            foreach (var m in type.Methods)
            {
                WriteSignature(m.Function, true, true);
            }

            WriteEndBlock();
            return this;
        }

        public SymbolWriterCSharp WriteConceptExtensions()
        {
            foreach (var c in Compiler.AllTypeAndLibraryDefinitions)
            {
                if (c.IsConcept())
                {
                    WriteConceptExtension(c);
                }
            }
            return this;
        }

        public SymbolWriterCSharp WriteConceptExtension(TypeDefinition type)
        {
            Debug.Assert(type.IsConcept());
            
            return WriteLine("public static partial class Extensions")
                .WriteStartBlock()
                .WriteConceptMembersAsExtensionMethods(type)
                .WriteEndBlock();
        }

        public SymbolWriterCSharp WriteTypeImplementations()
        {
            StartNewFile(OutputFolder.RelativeFile("Types.cs"));
            
            WriteLine("using System;");

            foreach (var c in Compiler.AllTypeAndLibraryDefinitions)
            {
                if (c.IsConcrete() && !IgnoreTypes.Contains(c.Name))
                {
                    WriteTypeImplementation(c);
                }
            }
            return this;
        }

        public static string ImplementedTypeString(TypeExpression te, string typeName)
        {
            var typeArgs = te.TypeArgs.Select(ta => ToString(ta));
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

            Write("public class ");
            Write(t.Name);
            WriteLine(implements);
            WriteStartBlock();

            var name = t.Name;
            var fieldTypes = t.Fields.Select(f => GetTypeName(f.Type, t)).ToList();
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
                {
                    fieldTypes.Add("string");
                }
                else if (name == "Number")
                {
                    fieldTypes.Add("double");
                }
                else if (name == "Boolean")
                {
                    fieldTypes.Add("bool");
                }
                else if (name == "Integer")
                {
                    fieldTypes.Add("int");
                }
                else if (name == "Character")
                {
                    fieldTypes.Add("char");
                }
                else if (name == "String")
                {
                    fieldTypes.Add("string");
                }
                else
                {
                    throw new Exception($"Not a recognized intrinsic {name}");
                }
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

            var funcs = new HashSet<string>();

            var concepts = GetAllImplementedConcepts(t).ToList();
            concepts = concepts.Distinct(sub => sub.Type.Definition).ToList();
            foreach (var sub in concepts)
            {
                var c = sub.Type.Definition;
                foreach (var m in c.Methods)
                {
                    var f = m.Function;
                    
                    var ret = GetTypeName(f.Type, sub);

                    var argList = f
                        .Parameters.Skip(1)
                        .Select(p => $"{p.Name}")
                        .JoinStringsWithComma();
                    
                    var firstName = f.Parameters.Count > 0 ? f.Parameters[0].Name : "";
                    
                    var paramList = f
                        .Parameters.Skip(1)
                        .Select(p => $"{GetTypeName(p.Type, sub)} {p.Name}")
                        .JoinStringsWithComma();

                    var staticParamList = f
                        .Parameters
                        .Select(p => $"{GetTypeName(p.Type, sub)} {p.Name}")
                        .JoinStringsWithComma();

                    var paramTypeList = f
                        .Parameters.Skip(1)
                        .Select(p => $"{GetTypeName(p.Type, sub)}")
                        .JoinStringsWithComma();

                    var sig = $"{f.Name}({paramTypeList}):{ret}";
                    if (funcs.Contains(sig))
                        continue;
                    funcs.Add(sig);

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

                                //.WriteLine()
                                //.WriteStartBlock()
                                //.Write("get")
                                //.WriteFunctionBody(f)
                                //.WriteEndBlock();
                            }
                        }
                    }

                    if (f.Parameters.Count == 1)
                    {
                        var op = OperatorNameLookup.NameToUnaryOperator(f.Name);
                        if (op != null)
                            WriteLine(
                                $"public static {ret} operator {op}({staticParamList}) => {firstName}.{f.Name};");
                    }
                }
            }

            WriteEndBlock();
            
            return this;
        }

        public static string ToString(TypeExpression type, string defaultType = "var")
        {
            if (type == null)
                return defaultType;
            if (type.TypeArgs.Count == 0)
                return type.Name;
            var argsJoined = type.TypeArgs.Select(t => ToString(t)).JoinStrings(", ");
            return $"{type.Name}<{argsJoined}>";
        }

        public SymbolWriterCSharp WriteTypeDecl(TypeExpression type, string defaultType = "var")
        {
            return Write(ToString(type, defaultType)).Write(" ");
        }

        public SymbolWriterCSharp WriteSignature(FunctionDefinition function, bool skipFirstParameter, bool eos)
        {   
            WriteTypeDecl(function.Type, "void")
                .Write(function.Name);

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

        // Used in library functions only.
        public IReadOnlyList<string> GetTypeParameterNames(TypeExpression expr)
        {
            Debug.Assert(expr.TypeArgs.Count == 0);
            var r = expr.Definition.TypeParameters.Select(p => p.Name);
            if (expr.Definition.IsSelfConstrained())
                r = r.Prepend("Self");
            return r.ToList();
        }

        public SymbolWriterCSharp WriteTypeParameters(TypeExpression expr)
        {
            var parameterNames = GetTypeParameterNames(expr);
            if (parameterNames.Count == 0) return this;
            Write("<");
            WriteCommaList(parameterNames);
            Write(">");
            return this;
        }

        public SymbolWriterCSharp WriteTypeForLibraryFunc(TypeExpression expr)
        {
            return Write(expr.Name).WriteTypeParameters(expr);
        }

        public SymbolWriterCSharp WriteLibraryMethods(TypeDefinition type)
        {
            Debug.Assert(type.IsLibrary());

            WriteLine($"public static partial class {type.Name}Library");
            WriteStartBlock();
            foreach (var f in type.Functions)
            {
                Write("public static ");
                WriteTypeForLibraryFunc(f.ReturnType);
                Write(" ");
                Write(f.Name);
                if (f.Parameters.Count > 0)
                {
                    WriteTypeParameters(f.Parameters[0].Type);
                    Write("(");
                    Write("this ");
                    for (var i = 0; i < f.Parameters.Count; ++i)
                    {
                        if (i > 0) Write(", ");
                        var p = f.Parameters[i];
                        WriteTypeForLibraryFunc(p.Type);
                        Write(" ");
                        Write(p.Name);
                    }
                    Write(")");
                }

                Write(" => ");
                WriteLine("throw new NotImplementedException();");
            }
            WriteEndBlock();
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

        public static IEnumerable<TypeSubstitutions> GetAllImplementedConcepts(TypeDefinition def)
        {
            var r = new HashSet<TypeSubstitutions>();
            foreach (var impl in def.Implements)
            {
                var sub = new TypeSubstitutions(def, impl);
                r.Add(sub);
                if (impl.Definition != null)
                {
                    foreach (var tmp2 in impl.Definition.GetAllImplementedConcepts())
                    {
                        r.Add(new TypeSubstitutions(def, tmp2, sub.Lookup));
                    }
                }
            }
            return r;
        }

        public static string GetTypeName(TypeExpression expr, TypeDefinition parentType)
            => GetTypeName(expr, new TypeSubstitutions(parentType, expr));

        public static string GetTypeName(TypeExpression expr, TypeSubstitutions subs)
        {
            var r = expr.Name;
            if (expr.Name == "Self") 
                r = subs.Self.Name;
            if (expr.Definition is TypeParameterDefinition tpd && subs.Lookup.ContainsKey(tpd))
                r = GetTypeName(subs.Lookup[tpd], subs);
            var args = expr.TypeArgs.Select(ta => GetTypeName(ta, subs)).ToList();
            if (args.Count == 0)
                return r;
            return $"{r}<{args.JoinStringsWithComma()}>";
        }

        public static string GetParameterNamesJoined(FunctionDefinition f)
            => string.Join(", ", f.Parameters.Select(p => p.Name));

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

        public SymbolWriterCSharp Write(DefinitionSymbol value)
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

        public SymbolWriterCSharp Write(TypeExpression typeExpression)
        {
            return WriteTypeDecl(typeExpression);
        }

        public SymbolWriterCSharp Write(Expression expr)
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