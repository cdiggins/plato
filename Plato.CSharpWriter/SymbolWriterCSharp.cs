using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Ara3D.Parakeet.Cst.CSharpGrammarNameSpace;
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
        public const bool UseSelfConstraints = true;

        public Compiler.Compilation Compilation { get; }
        public string CurrentType { get; set; }
        public Dictionary<string, StringBuilder> Files { get; } = new Dictionary<string, StringBuilder>();

        public DirectoryPath OutputFolder { get; }

        public bool IsStaticOrLambda { get; set; }

        public SymbolWriterCSharp WriteStaticOrLambdaBody(Symbol sym)
        {
            var oldStaticOrLambda = IsStaticOrLambda;
            IsStaticOrLambda = true;
            var r = Write(sym);
            r.IsStaticOrLambda = oldStaticOrLambda;
            return r;
        }

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
            
            // The core vector primitives 
            "Map",
            "Filter",
            "Reduce",
            "MapReduce",
            "FilterReduce",
            "MapFilterReduce",
        };

        public static Dictionary<string, string> PrimitiveTypes = new Dictionary<string, string>()
        {
            { "Number", "double" },
            { "Boolean", "bool" },
            { "Integer", "int" },
            { "Character", "char" },
            { "String", "string" },
            { "Dynamic", "object" },
            { "Type", "System.Type" },
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
                case TypeDef typeDefinition:
                    return Write(typeDefinition);
                case DefSymbol definition:
                    return Write(definition);
                case Expression expression:
                    return Write(expression);
                case Statement statement:
                    return Write(statement);
                case TypeExpression typeExpression:
                    return Write(typeExpression);
                default:
                    throw new ArgumentOutOfRangeException(nameof(symbol));
            }
        }

        public SymbolWriterCSharp WriteBody(Symbol body, bool isStatic)
        {
            var r = this;
            if (body is Expression)
                r = r.Write(" => ");
            r = isStatic ? r.WriteStaticOrLambdaBody(body) : r.Write(body);
            if (body is Expression)
                r = r.WriteLine(";");
            return r;
        }

        public SymbolWriterCSharp WriteFunctionBody(Symbol body0, bool isProperty, bool isStatic)
        {
            var body = body0.RewriteLambdasCapturingVars();

            return body is BlockStatement bs
                ? isProperty
                    ? WriteLine().WriteStartBlock().WriteLine("get").WriteBody(bs, isStatic).WriteEndBlock()
                    : WriteLine().WriteBody(bs, isStatic)
                : WriteBody(body, isStatic);
        }

        public string ToCSharp(TypeDef type)
            => ToCSharp(TypeInstance.Create(type));

        public string ToCSharp(TypeExpression expr)
            => ToCSharp(TypeInstance.Create(expr));

        public string ToCSharp(TypeInstance type)
        {
            var sb = new StringBuilder();
            if (type.Name.StartsWith("Function"))
                sb.Append("System.Func");
            else
            {
                if (!UseSelfConstraints && type.Name == "Self")
                {
                    return CurrentType;
                }
                else {
                    sb.Append(type.Name);
                }
            }

            if (type.Args.Count > 0)
            {
                sb.Append("<").Append(string.Join(", ", type.Args.Select(ToCSharp))).Append(">");
            }

            return sb.ToString();
        }
        
        public SymbolWriterCSharp WriteFunction(FunctionInstance f, ConcreteType t, bool generateImpl = false)
        {
            var ret = ToCSharp(f.ReturnType);
            
            var funcTypeParams = f.UsedTypeParameters
                .Where(tp => !t.Type.TypeParameters.Contains(tp)).ToList();

            /*
            if (funcTypeParams.Count > 0)
            {
                // TODO: the type parameter in this case should come from the parent type.
                // It doesn't so that is a problem. I need to be more sophisticated in FunctionInstance
                throw new Exception("Whoops!!");
            }
            */

            var funcTypeParamsStr = funcTypeParams.Count == 0
                ? ""
                : $"<{funcTypeParams.Select(tp => tp.Name).JoinStringsWithComma()}>";

            var argList = f
                .ParameterNames.Skip(1)
                .JoinStringsWithComma();

            var firstName = f.ParameterNames.FirstOrDefault() ?? "";

            var parameterTypes = f.ParameterTypes.Select(ToCSharp).ToList();

            var paramList = parameterTypes.Skip(1)
                .Zip(f.ParameterNames.Skip(1), (pt, pn) => $"{pt} {pn}")
                .JoinStringsWithComma();

            if (paramList.Length > 0)
                paramList = $"({paramList})";

            var staticParamList = parameterTypes
                .Zip(f.ParameterNames, (pt, pn) => $"{pt} {pn}")
                .JoinStringsWithComma();

            var isProperty = paramList.IsNullOrWhiteSpace();

            if (f.Name == Operators.ImplicitCast)
            {
                if (f.ParameterTypes.Count != 1)
                    throw new Exception("Implicit cast can have no parameters");

                return Write($"public static implicit operator {ret}({staticParamList}) ")
                    .WriteFunctionBody(f.Implementation.Body, isProperty, true);
            }
            
            if (f.Name == Operators.ExplicitCast)
            {
                if (f.ParameterTypes.Count != 1)
                    throw new Exception("Explicit cast can have no parameters");

                return Write($"public static explicit operator {ret}({staticParamList}) ")
                    .WriteFunctionBody(f.Implementation.Body, isProperty, true);
            }

            var isVectorOp = f.Name == "Map" || f.Name == "Reduce";

            if (f.Implementation.Body != null && !isVectorOp)
            {
                Write($"public {ret} {f.Name}{funcTypeParamsStr}{paramList}");
                WriteFunctionBody(f.Implementation.Body, isProperty, false);
            }
            else
            {
                // Create a default implementation the best we can looking at each field. 

                var fields = t.Type.Fields.Select(field => field.Name).ToList();
                var sep = ret == "Boolean" ? " & " : ", ";

                var impl = "throw new NotImplementedException()";

                // Only generate implementations when the the return type is a Boolean or the same as this type, or we have only one field.
                var canGenerateImpl = ret == t.Name || ret == "Boolean" || fields.Count == 1;

                var fieldTypes = t.DistinctFieldTypes;
                var elementType = fieldTypes.Count == 1 ? fieldTypes[0] : null;
                var isVectorLike = elementType != null;


                // Special case skip "map" and "reduce" for array-like types. 

                if (f.Name == "Map")
                {
                    if (!isVectorLike) return this; // throw new Exception("Not vector like");
                    impl = "(" + string.Join(", ", fields.Select((fieldName) => $"f({fieldName})")) + ")";
                    return WriteLine($"public {t.Type.Name} Map(System.Func<{elementType.Name}, {elementType.Name}> f) => {impl};");
                }

                if (f.Name == "Reduce")
                {
                    if (!isVectorLike) return this; // throw new Exception("Not vector like");
                    impl = "init";
                    foreach (var fieldName in fields)
                        impl = $"f({impl}, {fieldName})";
                    return WriteLine($"public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, {elementType.Name}, TAcc> f) => {impl};");
                }

                if (generateImpl && !canGenerateImpl && isVectorLike)
                {
                    switch (f.Name)
                    {
                        case "At":
                            impl = string.Join(" : ", fields.Select((fieldName, i) => $"n == {i} ? {fieldName}")) 
                                   + " : throw new System.IndexOutOfRangeException()";
                            break;

                        case "Count":
                            impl = fields.Count.ToString();
                            break;

                     
                    }
                }
                else if (generateImpl && canGenerateImpl)
                {
                    if (f.ParameterNames.Count <= 1)
                    {
                        var args = fields.Select(field => $"{field}.{f.Name}");
                        impl = $"({args.JoinStrings(sep)})";
                    }
                    else if (f.ParameterNames.Count == 2)
                    {
                        var p0 = f.ParameterNames[1];
                        var pt0 = f.ParameterTypes[1];
                        if (pt0.Name == t.Name)
                        {
                            var args = fields.Select(field => $"{field}.{f.Name}({p0}.{field})");
                            impl = $"({args.JoinStrings(sep)})";
                        }
                        else 
                        {
                            var args = fields.Select(field => $"{field}.{f.Name}({p0})");
                            impl = $"({args.JoinStrings(sep)})";
                        }
                    }
                    else if (f.ParameterNames.Count == 3)
                    {
                        var p0 = f.ParameterNames[1];
                        var pt0 = f.ParameterTypes[1];
                        var p1 = f.ParameterNames[2];
                        var pt1 = f.ParameterTypes[2];
                        if (pt0.Name == t.Name && pt1.Name == t.Name)
                        {
                            var args = fields.Select(field => $"{field}.{f.Name}({p0}.{field}, {p1}.{field})");
                            impl = $"({args.JoinStrings(sep)})";
                        }
                        else if (pt0.Name == t.Name)
                        {
                            var args = fields.Select(field => $"{field}.{f.Name}({p0}.{field}, {p1})");
                            impl = $"({args.JoinStrings(sep)})";
                        }
                        else if (pt1.Name == t.Name)
                        {
                            var args = fields.Select(field => $"{field}.{f.Name}({p0}, {p1}.{field})");
                            impl = $"({args.JoinStrings(sep)})";
                        }
                        else 
                        {
                            var args = fields.Select(field => $"{field}.{f.Name}({p0}, {p1})");
                            impl = $"({args.JoinStrings(sep)})";
                        }
                    }
                    else
                    {
                        throw new Exception(
                            "Cannot generate default implementations for functions with more than 3 parameters");
                    }
                }
                else if (f.Implementation.OwnerType.Name == "Intrinsics")
                {
                    var args = f.ParameterNames.Skip(1).Prepend("this").JoinStringsWithComma();
                    impl = $"Intrinsics.{f.Name}({args})";
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
                CurrentType = $"{t.Type.Name}{typeParamsStr}";
                WriteLine($"public {kind} {CurrentType}");
                WriteStartBlock();

                var funcGroups = t
                    .ConcreteFunctions
                    .Concat(t.GetConceptFunctions())
                    .GroupBy(f => f.SignatureId);

                foreach (var g in funcGroups)
                {
                    var f = g.First();
                    WriteFunction(f, t);
                }

                WriteEndBlock();
            }

            return this;
        }
        
        public SymbolWriterCSharp WriteConceptInterfaces()
        {
            StartNewFile(OutputFolder.RelativeFile("Concepts.cs"));
            if (!EmitInterfaces)
                WriteLine("/*");
            foreach (var c in Compilation.AllTypeAndLibraryDefinitions)
                if (c.IsConcept())
                    WriteConceptInterface(c);
            if (!EmitInterfaces)
                WriteLine("*/");
            return this;
        }

        public static string JoinTypeParameters(IEnumerable<string> parameters)
        {
            var r = parameters.JoinStrings(", ");
            return r.Length == 0 ? r : $"<{r}>";
        }

        public string TypeStr(TypeExpression type)
        {
            var typeArgs = type.TypeArgs;
            if (!UseSelfConstraints)
                typeArgs = typeArgs.Where(ta => !ta.IsSelfType() && ta.Name != "Self").ToList();
            return type.Name + JoinTypeParameters(typeArgs.Select(TypeStr));
        }

        public string TypeAsInherited(TypeExpression type)
        {
            var typeArgs = type.TypeArgs;
            if (!UseSelfConstraints)
                typeArgs = typeArgs.Where(ta => !ta.IsSelfType() && ta.Name != "Self").ToList();
            return type.Name + JoinTypeParameters((UseSelfConstraints && type.Def.IsSelfConstrained()) 
                ? typeArgs.Select(TypeStr).Prepend("Self") 
                : typeArgs.Select(TypeStr));
        }

        public SymbolWriterCSharp WriteConceptInterface(TypeDef type)
        {
            Debug.Assert(type.IsConcept());

            var typeParams = JoinTypeParameters((UseSelfConstraints && type.IsSelfConstrained())
                ? type.TypeParameters.Select(tp => tp.Name).Prepend("Self")
                : type.TypeParameters.Select(tp => tp.Name));

            var fullName = $"{type.Name}{typeParams}";
            var inherited = type.Inherits.Count > 0
                ? ": " + type.Inherits.Select(TypeAsInherited).JoinStrings(", ")
                : "";

            CurrentType = fullName;
            Write("public interface ").Write(fullName).WriteLine(inherited);

            foreach (var tp in type.TypeParameters)
            {
                foreach (var constraint in tp.Constraints)
                {
                    if (constraint.Name == "Any")
                        continue;
                    var constraintArgs = JoinTypeParameters(constraint.Def.IsSelfConstrained()
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
            if (UseSelfConstraints && te.Def.IsSelfConstrained())
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
            CurrentType = t.Name;
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
            if (isPrimitive && fieldTypes.Count != 0)
                throw new Exception($"Primitive {name} should not have fields");

            if (isPrimitive)
            {
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
            if (fieldNames.Count > 0)
            {
                WriteLine($"public {t.Name}({parametersStr}) => ({fieldNamesStr}) = ({parameterNamesStr});");
            }
            else
            {
                if (!EmitStructsInsteadOfClasses)
                    WriteLine($"public {t.Name}({parametersStr}) {{ }}");
            }

            // Parameterless constructor, but not if writing structs
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

            // Object.Equals override
            if (fieldNames.Count > 0)
            {
                Write($"public override bool Equals(object obj) {{ if (!(obj is {name})) return false; var other = ({name})obj; return ")
                    .Write(fieldNames.Select(f => $"{f}.Equals(other.{f})").JoinStrings(" && "))
                    .WriteLine("; }");
            }
            else
            {
                Write($"public override bool Equals(object obj) => true;");
            }

            // Object.GetHashCode override 
            WriteLine($"public override int GetHashCode() => Intrinsics.CombineHashCodes({fieldNamesStr});");
            
            // Object.ToString() override 
            WriteLine($"public override string ToString() => Intrinsics.MakeString(TypeName, FieldNames, FieldValues);");

            // Implicit casting operators to/from Dynamic
            WriteLine($"public static implicit operator Dynamic({name} self) => new Dynamic(self);");
            WriteLine($"public static implicit operator {name}(Dynamic value) => value.As<{name}>();");

            WriteLine($"public String TypeName => {name.Quote()};");

            var fieldNamesAsStringsStr = fieldNames.Select(n => $"(String){n.Quote()}").JoinStringsWithComma();
            WriteLine($"public Array<String> FieldNames => Intrinsics.MakeArray<String>({fieldNamesAsStringsStr});");
            var fieldValuesAsDynamicsStr = fieldNames.Select(n => $"new Dynamic({n})").JoinStringsWithComma();
            WriteLine($"public Array<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>({fieldValuesAsDynamicsStr});");

            if (EmitInterfaces)
            {
                foreach (var te in t.Implements)
                {
                    var its = ImplementedTypeString(te, t.Name);
                    
                    foreach (var f in te.Def.Functions)
                    {
                        var fieldIndex = fieldNames.IndexOf(f.Name);
                        if (f.Parameters.Count == 1 && fieldIndex >= 0)
                        {
                            var fieldType = isPrimitive ? name : fieldTypes[fieldIndex]; 
                            WriteLine($"{fieldType} {its}.{f.Name} => {f.Name};");
                        }
                    }
                }

            }

            // For the primitives, we have predefined implementations of the standard concepts .
            if (!isPrimitive)
            {
                WriteLine("// Unimplemented concept functions");
                foreach (var f in concreteType.UnimplementedFunctions)
                {
                    if (IgnoredFunctions.Contains(f.Name))
                        continue;

                    WriteFunction(f, concreteType, true);
                }
            }

            // Check if the type is an "Array", if so provide implementations of the default types. 
            var arrayConcept = t.GetAllImplementedConcepts().FirstOrDefault(te => te.Name == "Array");
            if (arrayConcept != null)
            {
                if (arrayConcept.TypeArgs.Count != 1)
                    throw new Exception("The Array concept must have exactly one type argument");
               

                // NOTE: we may eventually want implicit cast operators to/from the native array type.
            }

            WriteEndBlock();
            return this;
        }
        
        public SymbolWriterCSharp WriteSignature(FunctionDef function, bool skipFirstParameter, bool eos)
        {
            
            Write(ToCSharp(function.Type));
            Write(" ");
            Write(function.Name);

            var parameters = function.Parameters.Select(p => $"{ToCSharp(p.Type)} {p.Name}").ToList();
            if (parameters.Count > 1)
            {
                var r = Write("(")
                    .WriteCommaList(skipFirstParameter ? parameters.Skip(1) : parameters)
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

        public FilePath ToFileName(TypeDef type)
            => OutputFolder.RelativeFile($"{type.Kind}_{type.Name}.cs");
        
        public static void GatherTypeParameters(TypeExpression te, List<string> set)
        {
            if (UseSelfConstraints && te.Def.IsSelfConstrained())
                set.Add("Self");
            if (te.Name.StartsWith("$"))
                set.Add("T" + te.Name.Substring(1));
            foreach (var arg in te.TypeArgs)
                GatherTypeParameters(arg, set);
        }

        public static IEnumerable<string> GatherTypeParameters(FunctionDef fd)
        {
            var r = new List<string>();
            foreach (var param in fd.Parameters)
                GatherTypeParameters(param.Type, r);
            return r;
        }

        public SymbolWriterCSharp Write(DefSymbol value)
        {
            switch (value)
            {
                case null:
                    return Write("null");

                case FieldDef fieldDef:
                    return Write("public ").Write(fieldDef.Type).Write(fieldDef.Name).Write(" { get; }")
                        .WriteLine();

                case FunctionDef function:
                    throw new Exception("Not implemented");

                case FunctionGroupDef memberGroup:
                    return Write(memberGroup.Name);

                case MethodDef methodDef:
                    throw new Exception("Not implemented");

                case MemberDef member:
                    throw new Exception("Not implemented");

                case ParameterDef parameter:
                    return Write(parameter.Type).Write(parameter.Name);

                case VariableDef variable:
                    return Write("var ").Write(variable.Name).Write(" = ").Write(variable.Value).WriteLine(";");
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

        public SymbolWriterCSharp Write(Statement st)
        {
            switch (st)
            {
                case ReturnStatement returnSymbol:
                    Write("return ");
                    if (returnSymbol.Expression != null)
                        Write(returnSymbol.Expression);
                    WriteLine(";");
                    return this;
                
                case LoopStatement loopSymbol:
                    Write("while (").Write(loopSymbol.Condition).WriteLine(")");
                    WriteStartBlock();
                    Write(loopSymbol.Body);
                    WriteEndBlock();
                    WriteLine();
                    return this;

                case MultiStatement multiStatement:
                    foreach (var child in multiStatement.Symbols)
                    {
                        Write(child);
                        if (child is Expression)
                            WriteLine(";");
                    }
                    return this;

                case BlockStatement block:
                {
                    WriteStartBlock();
                    foreach (var x in block.Symbols)
                    {
                        Write(x);
                        if (x is Expression)
                            WriteLine(";");
                    }

                    return WriteEndBlock();
                }

                case CommentStatement commentStatement:
                    return Write($"/* {commentStatement.Comment} */");
            }

            return this;
        }

        public SymbolWriterCSharp WriteFunctionCall(FunctionCall functionCall)
        {
            // If there are no arguments, it is a constant.
            if (functionCall.Args.Count == 0)
                return Write("Constants.").Write(functionCall.Function);

            if (functionCall.Function is ParameterRefSymbol pr)
            {
                return Write(functionCall.Function).Write("(").WriteCommaList(functionCall.Args).Write(")");
            }

            Write(functionCall.Args[0]).Write(".").Write(functionCall.Function);
            if (functionCall.Args.Count == 1)
                return this;
            return Write("(").WriteCommaList(functionCall.Args.Skip(1)).Write(")");
        }

        public SymbolWriterCSharp Write(Expression expr)
        {
            if (expr == null)
                return this;

            //var t = Compiler.GetExpressionType(expr);
            //if (!(expr is Argument)) WriteLine($"/* {t} */");

            switch (expr)
            {
                case ThrowExpression te:
                    return Write("throw new Exception()");

                case ParameterRefSymbol pr:
                    return pr.Def.Index == 0 && !IsStaticOrLambda
                        ? Write("this") 
                        : Write(pr.Name);

                case FunctionGroupRefSymbol fgr:
                    // HACK: check if it is a constant.
                    // TODO: I need to have all function calls properly resolved to generate better quality code. 
                    if (fgr.Def.Functions.Count == 1 &&
                        fgr.Def.Functions[0].NumParameters == 0)
                        return Write($"Constants.{fgr.Name}");
                    return Write(fgr.Name);

                case RefSymbol refSymbol:
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
                    return WriteFunctionCall(functionCall);

                case Literal literal:
                    return Write($"(({GetLiteralType(literal)}){GetLiteralValue(literal)})");

                case Lambda lambda:
                    return Write("(")
                        .WriteCommaList(lambda.Function.Parameters.Select(p => p.Name))
                        .Write(") => ")
                        .WriteStaticOrLambdaBody(lambda.Function.Body);
            }

            throw new ArgumentOutOfRangeException(nameof(expr));
        }

        // TODO: I want to rewrite lambdas. 

        // If a Lambda shows up, that captures the "this", we have to create a block to capture it. 
        // That block has to surround the statement that references the Lambda. 

        public static IEnumerable<Lambda> GetLambdas(Statement statement)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Expression> GetStatementExpressionsNoChildren(Statement statement)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Statement> TransformStatement(Statement statement)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Statement> ExpressionBodyToStatement(Expression expression)
        {
            throw new NotImplementedException();
        }

    }
}