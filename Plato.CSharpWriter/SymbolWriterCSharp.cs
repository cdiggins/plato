using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ara3D.Utils;
using Plato.Compiler.Analysis;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;


namespace Plato.CSharpWriter
{
    public class SymbolWriterCSharp : CodeBuilder<SymbolWriterCSharp>
    {
        public const bool EmitInterfaces = true;
        public const bool EmitStructsInsteadOfClasses = true;
        public const bool EmitDataContracts = true;
        public const bool SkipGenericIArrayMethods = true;
        public string FloatType;
        public string Namespace;
        public string SelfType;

#if CHANGE_PRECISION
        public string OtherPrecisionFloatType;
        public string OtherPrecisionNamespace;
#endif

        public Compiler.Compilation Compilation { get; }
        public Dictionary<string, StringBuilder> Files { get; } = new Dictionary<string, StringBuilder>();

        public DirectoryPath OutputFolder { get; }

        public static HashSet<string> IgnoredTypes = new HashSet<string>()
        {
            "Dynamic",
            "Array",
            "Array2D",
            "Array3D",
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
            "Components",
            "FromComponents",
        };

        public static Dictionary<string, string> PrimitiveTypes = new Dictionary<string, string>()
        {
            { "Number", "{{float}}" },
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

        public SymbolWriterCSharp WriteAll(string fileName, string floatType)
        {
            FloatType = floatType;
            Namespace = floatType == "float"
                ? "Plato.SinglePrecision"
                : floatType == "double"
                    ? "Plato.DoublePrecision"
                    : throw new NotImplementedException("Only 'float' and 'double' are supported");
#if CHANGE_PRECISION
            OtherPrecisionFloatType = floatType == "float" ? "double" : "float";
            OtherPrecisionNamespace = floatType == "float" ? "Plato.DoublePrecision" : "Plato.SinglePrecision";
#endif 
            StartNewFile(fileName);
            WriteLine("using System.Runtime.Serialization;");
            WriteLine("using System.Runtime.InteropServices;");
            WriteLine("");
            WriteLine($"namespace {Namespace}");
            WriteStartBlock();
            WriteIntrinsics();
            WriteConceptInterfaces();
            WriteTypeImplementations();
            WriteConstantLibraryMethods();
            WriteInterfaceLibraryMethods();
            WriteEndBlock();
            sb.Replace("{{float}}", floatType);
            return this;
        }

        public SymbolWriterCSharp SetSelfType(string selfType, Func<SymbolWriterCSharp> f)
        {
            var oldSelfType = SelfType;
            try
            {
                SelfType = selfType;
                return f();
            }
            finally
            {
                SelfType = oldSelfType;
            }
        }

        public SymbolWriterCSharp WriteIntrinsics()
        {
            var intrinsicsFile = PathUtil.GetCallerSourceFolder().RelativeFile("Intrinsics.txt");
            var intrinsicsContent = intrinsicsFile.ReadAllText();
            return WriteLine(intrinsicsContent);
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

        public bool IsStaticOrLambda { get; set; }

        public SymbolWriterCSharp WriteStaticOrLambdaBody(Symbol sym)
        {
            var oldStaticOrLambda = IsStaticOrLambda;
            IsStaticOrLambda = true; 
            var r = Write(sym);
            r.IsStaticOrLambda = oldStaticOrLambda;
            return r;
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

        public SymbolWriterCSharp Write(Symbol symbol, string type = null)
        {
            switch (symbol)
            {
                case TypeDef typeDefinition:
                    return Write(typeDefinition);
                case DefSymbol definition:
                    return Write(definition);
                case Expression expression:
                    return Write(expression, type);
                case Statement statement:
                    return Write(statement);
                case TypeExpression typeExpression:
                    return Write(typeExpression);
                default:
                    throw new ArgumentOutOfRangeException(nameof(symbol));
            }
        }

        public SymbolWriterCSharp WriteBody(FunctionInfo fi, bool isStatic)
        {
            if (fi.Body == null)
            {
                return WriteLine(isStatic 
                    ? $" => Intrinsics.{fi.Name}{fi.StaticArgsString};" 
                    : $" => Intrinsics.{fi.Name}{fi.IntrinsicsArgsString};");
            }

            var body = fi.Body?.RewriteLambdasCapturingVars();
            if (body is Expression)
                Write(" => ");
            if (isStatic)
                WriteStaticOrLambdaBody(body);
            else
            {
                if (fi.NumParameters == 1 && !(body is Expression))
                    Write(" { get ").Write(body).Write(" } ");
                else
                    Write(body, fi.ReturnType);
            }
            if (body is Expression)
                WriteLine(";");
            return this;
        }

        public SymbolWriterCSharp GenerateFunc(FunctionInfo f, ConcreteType t)
        {
            var pns = f.ParameterNames;
            var fs = t.Type.Fields.Select(tf => tf.Name).ToList();
            Write(f.MethodSignature);

            if (f.Name == "At")
            {
                var p = pns[1];
                var s = "";
                for (var i=0; i < fs.Count; i++)
                    s += $"{p} == {i} ? {fs[i]} : ";
                s += $"throw new System.IndexOutOfRangeException()";
                WriteLine($" => {s};");

                WriteLine($"{f.IndexerSig} => {s};"); 
                return this;
            }

            if (f.Name == "Count")
            {
                return WriteLine($" => {fs.Count};");
            }

            throw new Exception("Only 'At' or 'Count' supported");
        }

        public string ToCSharp(TypeDef type)
            => ToCSharp(TypeInstance.Create(type), SelfType);

        public string ToCSharp(TypeExpression expr)
            => ToCSharp(TypeInstance.Create(expr), SelfType);

        public string ToCSharp(TypeInstance type)
            => ToCSharp(type, SelfType);

        public static string ToCSharp(TypeDef type, string selfType)
            => ToCSharp(TypeInstance.Create(type), selfType);

        public static string ToCSharp(TypeExpression expr, string selfType)
            => ToCSharp(TypeInstance.Create(expr), selfType);

        public static string ToCSharp(TypeInstance type, string selfType)
        {
            var sb = new StringBuilder();
            if (type.Name == "Self" && selfType != null)
                return selfType;

            if (type.Name.StartsWith("Function"))
                sb.Append("System.Func");
            else
                sb.Append(type.Name);

            if (type.Args.Count > 0)
                sb.Append("<")
                    .Append(string.Join(", ", type.Args.Select(a => ToCSharp(a, selfType))))
                    .Append(">");

            return sb.ToString();
        }

        public SymbolWriterCSharp WriteSimpleInterfaceFunction(ConceptImplementation c, FunctionInfo f)
        {
            Write(f.MethodSignature);
            
            if (f.IsIndexer)
                WriteLine(f.IndexerImpl);

            return this;
        }

        public SymbolWriterCSharp WriteMemberFunction(FunctionInfo f)
        {
            Write(f.MethodSignature);
            WriteBody(f, false);

            if (f.IsOperator)
                WriteLine(f.OperatorImpl);

            if (f.IsIndexer)
                WriteLine(f.IndexerImpl);

            if (f.IsImplicit)
            {
                if (f.ConcreteType.Name == f.Name)
                    Debug.WriteLine("Skipping implicit cast to self");
                else if (f.ConcreteType.Type.Fields.Count == 1 && f.ConcreteType.Type.Fields[0].Type.Def.Name == f.Name)
                    Debug.WriteLine("Skipping implicit cast to single field (already included)");
                else
                    WriteLine(f.ImplicitImpl);
            }

            return this;
        }
        
        public SymbolWriterCSharp WriteStaticFunction(FunctionInfo fi)
            => Write($"{fi.StaticSignature}").WriteBody(fi, true);

        public SymbolWriterCSharp WriteExtensionFunction(FunctionInfo fi)
            => Write($"{fi.ExtensionSignature}").WriteBody(fi, true);

        public SymbolWriterCSharp WriteConstantLibraryMethods()
        {
            WriteLine($"public static class Constants");
            WriteStartBlock();
            foreach (var f in Compilation.Libraries.AllConstants())
                WriteStaticFunction(ToFunctionInfo(f, null, FunctionInstanceKind.Constant));
            WriteEndBlock();
            return this;
        }

        public SymbolWriterCSharp WriteInterfaceLibraryMethods()
        {
            WriteLine($"public static class Extensions");
            WriteStartBlock();
            foreach (var f in Compilation.Libraries.AllFunctions())
            {
                if (f.NumParameters > 0)
                {
                    var pt = f.Parameters[0].Type;
                    if (!pt.Def.IsConcept())
                        continue;

                    // TODO: this is a HACK! we are temporarily only enabling this for IArray. 
                    // Ultimately it needs to be done with Self-constrained versions of the interfaces. 
                    // Writing those function signatures will be a lot of work. 
                    // Even then, there could be some problems (like 
                    if (!pt.Def.Name.StartsWith("IArray")) continue; 


                    // TODO: still a hack, we are skipping self-constrained interfaces temporarily
                    //if (pt.Def.IsSelfConstrained()) continue;
                    // NOTE: this causes problems because we have to implement property calls differently. 

                    var fi = ToFunctionInfo(f, null, FunctionInstanceKind.InterfaceExtension);
                    WriteExtensionFunction(fi);
                }
            }
            WriteEndBlock();
            return this;
        }
        
        public SymbolWriterCSharp WriteConceptInterfaces()
        {
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
            return type.Name + JoinTypeParameters(typeArgs.Select(TypeStr));
        }

        public string TypeAsInherited(TypeExpression type, bool useSelf = true)
        {
            var typeArgs = type.TypeArgs;
            return type.Name + JoinTypeParameters(type.Def.IsSelfConstrained() && useSelf 
                ? typeArgs.Select(TypeStr).Prepend("Self") 
                : typeArgs.Select(TypeStr));
        }

        public SymbolWriterCSharp WriteConceptInterfaceFunctions(TypeDef type)
        {
            var t = ToCSharp(type);
            WriteStartBlock();
            foreach (var m in type.Methods)
            {
                var fi = ToFunctionInfo(m.Function, null, FunctionInstanceKind.ConceptInterface);
                WriteLine(fi.MethodInterface);
                if (fi.IsIndexer)
                    WriteLine(fi.IndexerInterface);
            }

            WriteEndBlock();
            return this; 
        }

        public SymbolWriterCSharp WriteConceptInterface(TypeDef type)
        {
            // There may be one or two interfaces. 
            // If the type is self-constrained, then a version of the interface is created that has an explicit "Self" type as the first argument. 
            // And another version is created that does not: this is called the "simple interface".
            // The self constrained interface is more precise and efficient: it describes exactly the types that come in and out of the functions. 
            // The simple interface required dynamic casting to be made to work. 

            Debug.Assert(type.IsConcept());

            // We have a special implementation of IArray
            if (type.Name == "IArray")
                return this;

            var baseTypeParams = type.TypeParameters.Select(tp => tp.Name).ToList();

            var typeParams = type.IsSelfConstrained()
                ? baseTypeParams.Prepend("Self")
                : baseTypeParams;

            var baseFullName = $"{type.Name}{JoinTypeParameters(baseTypeParams)}";
            var fullName = $"{type.Name}{JoinTypeParameters(typeParams)}";

            var inherits = type.Inherits.Select(t => TypeAsInherited(t, true)).ToList();
            if (type.IsSelfConstrained())
                inherits.Add(baseFullName);
            
            var inherited = inherits.Count > 0 
                ? ": " + inherits.JoinStringsWithComma() : "";

            Write("public interface ").Write(fullName).WriteLine(inherited);

            foreach (var tp in type.TypeParameters)
            {
                foreach (var constraint in tp.Constraints)
                {
                    if (constraint.Name == "IAny")
                        continue;
                    var constraintArgs = JoinTypeParameters(constraint.Def.IsSelfConstrained()
                        ? constraint.TypeArgs.Select(TypeStr).Prepend(tp.Name)
                        : constraint.TypeArgs.Select(TypeStr));
                    WriteLine($"where {tp.Name} : {constraint.Name}{constraintArgs}");
                }
            }

            WriteConceptInterfaceFunctions(type);

            // We are going to write another version of the interface, without a "Self"
            if (type.IsSelfConstrained())
            {
                var simpleInherits = type.Inherits.Select(t => TypeAsInherited(t, false)).ToList();
                var simpleInherited = simpleInherits.Count > 0 ? ": " + simpleInherits.JoinStringsWithComma() : "";
                SetSelfType(baseFullName, () =>
                {
                    Write("public interface ").Write(baseFullName).WriteLine(simpleInherited);
                    WriteConceptInterfaceFunctions(type);
                    return this;
                });
            }

            return this;
        }

        public SymbolWriterCSharp WriteTypeImplementations()
        {
            foreach (var c in Compilation.ConcreteTypes)
                if (!IgnoredTypes.Contains(c.Type.Name))
                    WriteTypeImplementation(c);
            return this;
        }

        public string ImplementedTypeString(TypeExpression te, string typeName, bool useSelf)
        {
            var typeArgs = te.TypeArgs.Select(TypeStr);
            if (te.Def.IsSelfConstrained() && useSelf)
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
            
            return SetSelfType(t.Name, () =>
            {
                var implements = EmitInterfaces && t.Implements.Count > 0
                    ? ": " + string.Join(", ", t.Implements.Select(te => ImplementedTypeString(te, t.Name, true)))
                    : "";

                var typeParamsStr = JoinTypeParameters(t.TypeParameters.Select(tp => tp.Name));

                var kind = EmitStructsInsteadOfClasses ? "readonly partial struct" : "partial class";
                var attr = EmitDataContracts
                    ? "[DataContract, StructLayout(LayoutKind.Sequential, Pack=1)]"
                    : "[StructLayout(LayoutKind.Sequential, Pack=1)]";
                WriteLine($"{attr}");
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
                    var memberAttr = EmitDataContracts ? "[DataMember] " : "";
                    WriteLine($"{memberAttr}public readonly {ft} {fn};");
                }

                for (var i = 0; i < fieldTypes.Count; ++i)
                {
                    var ft = fieldTypes[i];
                    var fn = fieldNames[i];
                    var pn = FieldNameToParameterName(fn);
                    var args = fieldNames.Select((n, j) => j == i ? pn : n).JoinStringsWithComma();
                    WriteLine($"public {name} With{fn}({ft} {pn}) => new {name}({args});");
                }

                var parameters = fieldTypes.Zip(parameterNames, (pt, pn) => $"{pt} {pn}");
                var parameterNamesStr = parameterNames.JoinStringsWithComma();
                var parametersStr = parameters.JoinStringsWithComma();
                var deconstructorParametersStr =
                    fieldTypes.Zip(parameterNames, (pt, pn) => $"out {pt} {pn}").JoinStringsWithComma();

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

#if CHANGE_PRECISION
                // Precision changes
                if (t.TypeParameters.Count == 0 && fieldNames.Count != 0 && !fieldTypes.Any(ft => ft.Contains("<")))
                {
                    var changeFields = fieldNames.Select(f => $"{f}.ChangePrecision()").JoinStringsWithComma();
                    WriteLine($"public {OtherPrecisionNamespace}.{t.Name} ChangePrecision() => ({changeFields});");
                    WriteLine(
                        $"public static implicit operator {OtherPrecisionNamespace}.{t.Name}({t.Name} self) => self.ChangePrecision();");
                }
#endif

                // Implicit operators 
                if (fieldNames.Count > 1)
                {
                    // Implicit value-tuple operator 
                    var qualifiedFieldNames = fieldNames.Select(f => $"self.{f}").JoinStringsWithComma();
                    var tupleNames = string.Join(", ",
                        Enumerable.Range(1, fieldNames.Count).Select(i => $"value.Item{i}"));
                    WriteLine(
                        $"public static implicit operator ({fieldTypesStr})({name} self) => ({qualifiedFieldNames});");
                    WriteLine(
                        $"public static implicit operator {name}(({fieldTypesStr}) value) => new {name}({tupleNames});");

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

                    // Only implicit operators if we are not an 
                    if (isPrimitive || !t.Fields[0].Type.Def.IsConcept())
                    {
                        WriteLine($"public static implicit operator {fieldType}({name} self) => self.{fieldName};");
                        WriteLine($"public static implicit operator {name}({fieldType} value) => new {name}(value);");
                    }

                    // Any time that we are implicitly casting to/from Number (floating point)
                    // We can also cast from Plato.Integers and built-in integers, as well to/from built-in floating types 
                    if (fieldType == "Number")
                    {
                        WriteLine($"public static implicit operator {name}(Integer value) => new {name}(value);");
                        WriteLine($"public static implicit operator {name}(int value) => new Integer(value);");
                        WriteLine($"public static implicit operator {name}({FloatType} value) => new Number(value);");
                        WriteLine($"public static implicit operator {FloatType}({name} value) => value.{fieldName};");

                    }
                }

                // Object.Equals override
                if (fieldNames.Count > 0)
                {
                    Write(
                            $"public override bool Equals(object obj) {{ if (!(obj is {name})) return false; var other = ({name})obj; return ")
                        .Write(fieldNames.Select(f => $"{f}.Equals(other.{f})").JoinStrings(" && "))
                        .WriteLine("; }");
                }
                else
                {
                    WriteLine($"public override bool Equals(object obj) => true;");
                }

                // Object.GetHashCode override 
                WriteLine($"public override int GetHashCode() => Intrinsics.CombineHashCodes({fieldNamesStr});");

                // Object.ToString() override 
                if (isPrimitive)
                {
                    WriteLine($"public override string ToString() => Intrinsics.ToString(this);");
                }
                else
                {
                    var toStr = "$\"{{ " + fieldNames.Select(fn => $"\\\"{fn}\\\" = {{{fn}}}").JoinStringsWithComma() +
                                " }}\"";
                    WriteLine($"public override string ToString() => {toStr};");
                }

                // TODO: later
                /*
                WriteLine($"public static {name} FromString(String value) {{ int offset = 0; return ParseString(value, ref offset); }}");
                if (isPrimitive)
                {
                    WriteLine($"public static {name} ParseString(String value, ref int offset) => Intrinsics.ParseValue(value);");
                }
                else
                {
                    var fromStr = fieldTypes.Select(ft => $"{ft}.ParseString(value, offset)").JoinStringsWithComma();
                    WriteLine($"public static {name} ParseString(String value, ref int offset) => ({fromStr});");
                }
                */

                // Implicit casting operators to/from Dynamic
                WriteLine($"public static implicit operator Dynamic({name} self) => new Dynamic(self);");
                WriteLine($"public static implicit operator {name}(Dynamic value) => value.As<{name}>();");

                WriteLine($"public String TypeName => {name.Quote()};");

                var fieldNamesAsStringsStr = fieldNames.Select(n => $"(String){n.Quote()}").JoinStringsWithComma();
                WriteLine(
                    $"public IArray<String> FieldNames => Intrinsics.MakeArray<String>({fieldNamesAsStringsStr});");

                var fieldValuesAsDynamicsStr = fieldNames.Select(n => $"new Dynamic({n})").JoinStringsWithComma();
                WriteLine(
                    $"public IArray<Dynamic> FieldValues => Intrinsics.MakeArray<Dynamic>({fieldValuesAsDynamicsStr});");

                if (EmitInterfaces)
                {
                    // Explicit implementation of properties by forwarding to fields
                    foreach (var te in t.GetAllImplementedConcepts())
                    {
                        var its = ImplementedTypeString(te, t.Name, true);

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

                    foreach (var c in t.GetAllImplementedConcepts())
                    {
                        WriteSimpleInterface(concreteType, name, c);
                    }
                }

                // Check if the type is "IArray", so can add an enumerator and an implicit cast to/from system array. 
                var arrayConcept = concreteType.AllConcepts.FirstOrDefault(c => c.Name == "IArray");
                var isArray = arrayConcept != null;
                if (isArray)
                {
                    var argType = arrayConcept.Substitutions.Replace(arrayConcept.Expression.TypeArgs[0]);
                    var elem = ToCSharp(argType);
                    WriteLine($"// Array predefined functions");

                    // Check that there are mul
                    if (fieldNames.Count > 1 && fieldTypes.All(ft => ft == elem))
                    {
                        // Add a constructor from arrays 
                        var ctorArrayArgs = Enumerable.Range(0, fieldNames.Count).Select(i => $"xs[{i}]")
                            .JoinStringsWithComma();
                        WriteLine($"public {name}(IArray<{elem}> xs) : this({ctorArrayArgs}) {{ }}");
                        WriteLine($"public {name}({elem}[] xs) : this({ctorArrayArgs}) {{ }}");
                        WriteLine($"public static {name} New(IArray<{elem}> xs) => new {name}(xs);");
                        WriteLine($"public static {name} New({elem}[] xs) => new {name}(xs);");
                    }

                    // Allow implicit casting to System.Array
                    WriteLine($"public static implicit operator {elem}[]({name} self) => self.ToSystemArray();");

                    // Allow implicit casting to Array<T>
                    WriteLine(
                        $"public static implicit operator Array<{elem}>({name} self) => self.ToPrimitiveArray();");

                    // Implementation of IReadOnlyList
                    WriteLine(
                        $"public System.Collections.Generic.IEnumerator<{elem}> GetEnumerator() {{ for (var i=0; i < Count; i++) yield return At(i); }}");
                    WriteLine(
                        $"System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();");
                    WriteLine($"{elem} System.Collections.Generic.IReadOnlyList<{elem}>.this[int n] => At(n);");
                    WriteLine($"int System.Collections.Generic.IReadOnlyCollection<{elem}>.Count => this.Count;");
                }

                // Check if the type is "INumerical", if so provide implementations of the default types. 
                var isNumerical = t.GetAllImplementedConcepts().Any(te => te.Name == "INumerical");
                if (isNumerical)
                {
                    WriteLine($"// Numerical predefined functions");
                    var numDistinctFieldTypes = fieldTypes.Distinct().Count();
                    if (numDistinctFieldTypes != 1)
                        throw new Exception("INumerical types are assumed to have all of the fields of the same type");
                    WriteLine(
                        $"public IArray<Number> Components => Intrinsics.MakeArray<Number>({fieldNames.JoinStringsWithComma()});");

                    var tmp = Enumerable.Range(0, fieldNames.Count).Select(i => $"numbers[{i}]").JoinStringsWithComma();
                    var fromCompImpl = $"new {name}({tmp})";
                    WriteLine($"public {name} FromComponents(IArray<Number> numbers) => {fromCompImpl};");
                }

                // For the primitives, we have predefined implementations of the standard concepts .
                WriteLine("// Implemented concept functions and type functions");
                var funcGroups = concreteType
                    .ConcreteFunctions
                    .Concat(concreteType.GetConceptFunctions())
                    .GroupBy(f => f.SignatureId)
                    .Select(g => g.ToList());

                foreach (var g in funcGroups)
                {
                    var f = ChooseBestFunction(g);

                    // Note: we skip functions that are named after a field ... 
                    if (fieldNames.Contains(f.Name))
                        continue;

                    if (f.ConceptName == "IArray")
                    {
                        // TODO: We skip the generic IArray methods because there are too many of them.
                        // We can re-enable this for some experiments in the future, because they would be potentially more efficient. 
                        // Maybe what we want is an "ArrayLike" concept, which is a subset of IArray, if we want the programmer to be able to control some of the details
                        if (SkipGenericIArrayMethods && f.Concept.TypeParameters.Count == 1)
                        {
                            var p = f.Implementation.Parameters.FirstOrDefault();
                            var typeArg = p.Type.TypeArgs.FirstOrDefault();
                            if (typeArg == null || typeArg.Def.IsTypeVariable())
                                continue;
                            Debug.WriteLine($"Implementing IArray function: {f}");

                            // TEMP: we aren't bothering to implement any of them now.
                            continue;
                        }

                        // The problem here is that what we really care about is why the function is being implemented.
                        // Is it from a specific interface ?? 
                    }

                    // We have to be sure to not implement functions that cast to themselves
                    if (f.Name != name)
                        WriteMemberFunction(ToFunctionInfo(f, concreteType));
                }

                WriteLine("// Unimplemented concept functions");
                foreach (var f in concreteType.UnimplementedFunctions)
                {
                    if (IgnoredFunctions.Contains(f.Name))
                        continue;

                    if (f.Name == "At" || f.Name == "Count")
                    {
                        if (name == "String")
                            continue;

                        Debug.WriteLine($"IArray is being implemented for {name}");
                        var fi = ToFunctionInfo(f, concreteType);
                        GenerateFunc(fi, concreteType);
                    }
                    else
                    {
                        WriteMemberFunction(ToFunctionInfo(f, concreteType));
                    }
                }

                WriteEndBlock();
                return this;
            });
        }

        public SymbolWriterCSharp WriteSimpleInterface(ConcreteType concrete, string name, TypeExpression concept)
        {
            if (!concept.Def.IsSelfConstrained())
                return this; 

            var its = ImplementedTypeString(concept, concrete.Type.Name, false);

            return SetSelfType(its, () =>
            {
                foreach (var f in concept.Def.Functions)
                {
                    var ret = ToCSharp(concept.GetReplacement(f.ReturnType));
                    var pts = f.Parameters.Skip(1).Select(p => ToCSharp(concept.GetReplacement(p.Type))).ToList();
                    var pns = f.Parameters.Skip(1).Select(p => p.Name).ToList();
                    var ps = pts.Zip(pns, (pt, pn) => $"{pt} {pn}").JoinStringsWithComma();
                    if (ps.Length > 0) ps = $"({ps})";
                    var casts = pts.Select(p => p == its ? name : p).ToList();
                    var args = casts.Zip(pns, (cast, pn) => $"({cast}){pn}").JoinStringsWithComma();
                    if (args.Length > 0) args = $"({args})";
                    WriteLine($"{ret} {its}.{f.Name}{ps} => this.{f.Name}{args};");
                }

                return this;
            });
        }

        public int ScoreFunction(FunctionInstance f)
        {
            var a = f.ConcreteType;
            var b = f.Concept;

            // We assume that if there is no concept, then the function implementation originated as a concrete type.
            // Concrete types provide a better score than any concept. 
            if (b == null)
                return -100;

            var depth = a.Type.DepthTo(b);
            if (depth < 0)
                throw new Exception($"Expected {b} to be a base type of {a}");
            return depth;
        }

        public FunctionInstance ChooseBestFunction(IReadOnlyList<FunctionInstance> xs)
        {
            // We only want distinct implementations. 
            var first = xs[0];
            xs = xs.Distinct(x => x.Implementation.Id).ToList();

            if (xs.Count > 1)
            {
                xs = xs.GroupBy(ScoreFunction).OrderBy(g => g.Key).First().ToList();
                // By definition this should always be true, but keep it in case of refactoring. 
                Debug.Assert(xs.Count > 0);
            }

            if (xs.Count > 1)
            {
                WriteLine($"// Ambiguous: could not choose a best function implementation for {first}.");
            }

            if (xs.Count == 0)
                throw new Exception("No results: could not find a best function.");

            return xs[0];
        }

        public FunctionInfo ToFunctionInfo(FunctionDef fd, ConcreteType ct, FunctionInstanceKind kind)
            => ToFunctionInfo(new FunctionInstance(fd, ct, null, kind));

        public FunctionInfo ToFunctionInfo(FunctionInstance fi, ConcreteType ct = null)
        {
            var ret = ToCSharp(fi.ReturnType);
            var parameterTypes = fi.ParameterTypes.Select(ToCSharp).ToList();
            var funcTypeParams = ct == null 
                ? fi.UsedTypeParameters
                : fi.UsedTypeParameters.Where(tp => !ct.Type.TypeParameters.Contains(tp)).ToList();

            return new FunctionInfo(fi.Name,
                ct,
                ret,
                funcTypeParams.Select(ToCSharp),
                fi.ParameterNames,
                parameterTypes,
                fi.Implementation.Body,
                fi.IsImplicitCast);
        }

        public static string ParameterizedTypeName(string name, IEnumerable<string> args)
            => name + TypeArgsString(args);

        public static string TypeArgsString(IEnumerable<string> args)
            => args.Any() ? "<" + string.Join(", ", args) + ">" : "";

        public FilePath ToFileName(TypeDef type)
            => OutputFolder.RelativeFile($"{type.Kind}_{type.Name}.cs");
        
        public static void GatherTypeParameters(TypeExpression te, List<string> set)
        {
            if (te.Def.IsSelfConstrained())
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
            => Write(TypeStr(typeExpression)).Write(" ");

        public static string GetLiteralType(Literal literal) 
            => literal.TypeEnum.ToString();

        public static string GetLiteralValue(Literal literal) 
            => literal.Value.ToLiteralString();

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

                case IfStatement ifStatement:
                    Write("if (");
                    Write(ifStatement.Condition);
                    WriteLine(")");
                    Write(ifStatement.IfTrue);
                    if (ifStatement.IfFalse != null)
                    {
                        WriteLine("else");
                        Write(ifStatement.IfFalse);
                    }

                    return this;
                }

            return this;
        }

        public SymbolWriterCSharp WriteFunctionCall(FunctionCall functionCall)
        {
            // If there are no arguments, it is a constant.
            if (functionCall.Args.Count == 0)
                return Write("Constants.").Write(functionCall.Function);

            // Calling a parameter, or variable 
            if (functionCall.Function is ParameterRefSymbol 
                || functionCall.Function is VariableRefSymbol
                || functionCall.Function is FunctionCall)
            {
                return Write(functionCall.Function).Write(".Invoke(").WriteCommaList(functionCall.Args).Write(")");
            }

            Write(functionCall.Args[0]).Write(".").Write(functionCall.Function);
            
            if (functionCall.Args.Count == 1 && !functionCall.HasArgList) 
                return this;
            
            return Write("(").WriteCommaList(functionCall.Args.Skip(1)).Write(")");
        }

        public SymbolWriterCSharp Write(Expression expr, string type = null)
        {
            if (expr == null)
                return this;

            //var t = Compiler.GetExpressionType(expr);
            //if (!(expr is Argument)) WriteLine($"/* {t} */");

            switch (expr)
            {
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

                case ArrayLiteral arrayLiteral:
                {
                    var arg = "";
                    if (type != null && type.StartsWith("IArray"))
                        arg = type.Substring("IArray".Length);
                    return Write($"Intrinsics.MakeArray{arg}(")
                        .WriteCommaList(arrayLiteral.Expressions)
                        .Write(")");
                }
            }

            throw new ArgumentOutOfRangeException(nameof(expr));
        }
    }
}