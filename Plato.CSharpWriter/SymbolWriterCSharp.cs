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
    public class FunctionToWrite
    {
        public FunctionToWrite(FunctionInstance f, string currentType)
        {
            Func = f;
            CurrentType = currentType;
        }

        public string Name => Func.Name;
        public readonly string CurrentType; 
        public readonly FunctionInstance Func;
        public string ParamListStr => ParameterTypes.Zip(ParameterNames, (t, n) => $"{t} {n}").JoinStringsWithComma();
        public IReadOnlyList<string> ParameterNames => Func.ParameterNames;
        public IReadOnlyList<string> ParameterTypes => Func.ParameterTypes.Select(t => SymbolWriterCSharp.ToCSharp(t, CurrentType)).ToList();
        public string ReturnType => SymbolWriterCSharp.ToCSharp(Func.ReturnType, CurrentType);
    }

    public class SymbolWriterCSharp : CodeBuilder<SymbolWriterCSharp>
    {
        public const bool EmitInterfaces = true;
        public const bool EmitStructsInsteadOfClasses = true;
        public string FloatType;
        public string Namespace;

        // Used during the writing process to track of additional types that need to be exposed as extension methods 
        public List<FunctionToWrite> ExtensionList = new List<FunctionToWrite>();

#if CHANGE_PRECISION
        public string OtherPrecisionFloatType;
        public string OtherPrecisionNamespace;
#endif

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
            WriteLine($"namespace {Namespace}");
            WriteStartBlock();
            WriteIntrinsics();
            WriteConceptInterfaces();
            WriteTypeImplementations();
            WriteLibraryMethods();
            WriteEndBlock();
            sb.Replace("{{float}}", floatType);
            return this;
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

        public SymbolWriterCSharp GenerateBody(FunctionInstance f, ConcreteType t, bool isStatic, string forwardTo = null)
        {
            var pns = f.ParameterNames;
            if (forwardTo != null)
            {
                if (pns.Count == 1)
                    // We are forwarding to a property
                    return WriteLine($" => {pns[0]}.{forwardTo};");
                else
                    // We are forwarding to a method 
                    return WriteLine($" => {pns[0]}.{forwardTo}({pns.Skip(1).JoinStringsWithComma()});");
            }

            var impl = "throw new System.NotImplementedException()";
            if (f.Implementation.OwnerType.Name == "Intrinsics")
            {
                var args = pns.Skip(1).Prepend("this").JoinStringsWithComma();
                return WriteLine($" => Intrinsics.{f.Name}({args});");
            }

            var fs = t.Type.Fields.Select(tf => tf.Name).ToList();
            if (f.Name == "At")
            {
                var p = pns[1];
                var s = "";
                for (var i=0; i < fs.Count; i++)
                    s += $"{p} == {i} ? {fs[i]} : ";
                s += $"throw new System.IndexOutOfRangeException()";
                return WriteLine($" => {s};");
            }

            if (f.Name == "Count")
            {
                return WriteLine($" => {fs.Count};");
            }

            /*
            NOTE: these are super efficient versions of the ZipComponents and MapComponents functions.
            In theory, we can reduce down to this in the compiler by evaluating constant expressions 
            at compile-time.
            
            var fields = t.Type.Fields.Select(field => field.Name).ToList();
               var isPrimitive = PrimitiveTypes.ContainsKey(t.Name);
               if (isPrimitive)
               {
                   fields.Add("Value");
               }

                        // Create a default implementation the best we can looking at each field. 
               var ret = ToCSharp(f.ReturnType);    
            var sep = ret == "Boolean" ? " & " : ", ";
            var thisDot = isStatic ? $"{pns[0]}." : "this.";
            
            else if (pns.Count <= 1)
            {
                var args = fields.Select(field => $"{thisDot}{field}.{f.Name}");
                impl = $"({args.JoinStrings(sep)})";
            }
            else if (pns.Count == 2)
            {
                var p0 = pns[1];
                var pt0 = f.ParameterTypes[1];
                if (pt0.Name == t.Name)
                {
                    var args = fields.Select(field => $"{thisDot}{field}.{f.Name}({p0}.{field})");
                    impl = $"({args.JoinStrings(sep)})";
                }
                else
                {
                    var args = fields.Select(field => $"{thisDot}{field}.{f.Name}({p0})");
                    impl = $"({args.JoinStrings(sep)})";
                }
            }
            else if (pns.Count == 3)
            {
                var p0 = pns[1];
                var pt0 = f.ParameterTypes[1];
                var p1 = pns[2];
                var pt1 = f.ParameterTypes[2];
                if (pt0.Name == t.Name && pt1.Name == t.Name)
                {
                    var args = fields.Select(field => $"{thisDot}{field}.{f.Name}({p0}.{field}, {p1}.{field})");
                    impl = $"({args.JoinStrings(sep)})";
                }
                else if (pt0.Name == t.Name)
                {
                    var args = fields.Select(field => $"{thisDot}{field}.{f.Name}({p0}.{field}, {p1})");
                    impl = $"({args.JoinStrings(sep)})";
                }
                else if (pt1.Name == t.Name)
                {
                    var args = fields.Select(field => $"{thisDot}{field}.{f.Name}({p0}, {p1}.{field})");
                    impl = $"({args.JoinStrings(sep)})";
                }
                else
                {
                    var args = fields.Select(field => $"{thisDot}{field}.{f.Name}({p0}, {p1})");
                    impl = $"({args.JoinStrings(sep)})";
                }
            }
            else
            {
                throw new Exception("Cannot generate default implementations for functions with more than 3 parameters");
            }
            */

            return WriteLine($" => {impl};");
        }

        public SymbolWriterCSharp WriteFunctionBody(FunctionInstance f, ConcreteType t, bool isProperty, bool isStatic, string forwardTo)
        {
            var body0 = f.Implementation.Body;
            if (body0 == null)
            {
                if (forwardTo != null)
                    Debug.Assert(isStatic);
                return GenerateBody(f, t, isStatic, forwardTo);
            }

            var body = body0.RewriteLambdasCapturingVars();

            return body is BlockStatement bs
                ? isProperty
                    ? WriteLine().WriteStartBlock().WriteLine("get").WriteBody(bs, isStatic).WriteEndBlock()
                    : WriteLine().WriteBody(bs, isStatic)
                : WriteBody(body, isStatic);
        }

        public string ToCSharp(TypeDef type)
            => ToCSharp(TypeInstance.Create(type), CurrentType);

        public string ToCSharp(TypeExpression expr)
            => ToCSharp(TypeInstance.Create(expr), CurrentType);

        public string ToCSharp(TypeInstance type)
            => ToCSharp(type, CurrentType);

        public static string ToCSharp(TypeDef type, string currentType)
            => ToCSharp(TypeInstance.Create(type), currentType);

        public static string ToCSharp(TypeExpression expr, string currentType)
            => ToCSharp(TypeInstance.Create(expr), currentType);

        public static string ToCSharp(TypeInstance type, string currentType)
        {
            var sb = new StringBuilder();
            if (type.Name.StartsWith("Function"))
                sb.Append("System.Func");
            else
                sb.Append(type.Name);

            if (type.Args.Count > 0)
                sb.Append("<")
                    .Append(string.Join(", ", type.Args.Select(a => ToCSharp(a, currentType))))
                    .Append(">");

            return sb.ToString();
        }

        public SymbolWriterCSharp WriteMemberFunction(FunctionInstance f, ConcreteType t)
        {
            var ret = ToCSharp(f.ReturnType);

            var parameterTypes = f.ParameterTypes.Select(ToCSharp).ToList();
            var staticParamList = parameterTypes
                .Zip(f.ParameterNames, (pt, pn) => $"{pt} {pn}")
                .JoinStringsWithComma();

            var paramList = parameterTypes.Skip(1)
                .Zip(f.ParameterNames.Skip(1), (pt, pn) => $"{pt} {pn}")
                .JoinStringsWithComma();

            var funcTypeParams = f.UsedTypeParameters
                .Where(tp => !t.Type.TypeParameters.Contains(tp)).ToList();

            var funcTypeParamsStr = funcTypeParams.Count == 0
                ? ""
                : $"<{funcTypeParams.Select(tp => tp.Name).JoinStringsWithComma()}>";


            var pns = f.ParameterNames;
            var pts = parameterTypes;
            Debug.Assert(pns.Count == pts.Count);

            if (pts.Count > 0 && pts[0] != ToCSharp(t.Type))
            {
                // HACK: Scalar multiply hack

                // NOTE: this is a member function where the first type is NOT the same as the enclosing member. 
                // e.g., Multiply(Number n, Vector2D v)) 

                // Right now this only happens for scalar multiplication, but we may be able to extend thsi to other types 
                
                Debug.Assert(f.Name == "Multiply" && pts.Count == 2 && pts[0] == "Number");

                Write($"public static {ret} {f.Name}{funcTypeParamsStr}({staticParamList})");
                WriteLine($" => {pns[1]}.{f.Name}({pns[0]});");

                var op = Operators.NameToBinaryOperator(f.Name);

                // NOTE: this is because in C#, the "||" and "&&" operators cannot be overridden.
                if (op == "||") op = "|";
                if (op == "&&") op = "&";

                if (op != null)
                {
                    Write($"public static {ret} operator {op}{funcTypeParamsStr}({staticParamList})");
                    WriteLine($" => {f.Name}({pns.JoinStringsWithComma()});");
                }
                return this;
            }
            if (f.ParameterNames.Count == 1)
            {
                var op = Operators.NameToUnaryOperator(f.Name);
                if (op != null)
                {
                    Write($"public static {ret} operator {op}({staticParamList})");
                    WriteFunctionBody(f, t, false, true, f.Name);
                }

                if (f.IsImplicitCast)
                {
                    Write($"public static implicit operator {ret}({staticParamList})")
                        .WriteFunctionBody(f, t, false, true, f.Name);
                }
                
                Write($"public {ret} {f.Name}");
                WriteFunctionBody(f, t, true, false, null);
            }
            else
            if (f.ParameterTypes.Count > 1)
            {
                if (f.ParameterNames.Count == 2)
                {
                    var op = Operators.NameToBinaryOperator(f.Name);

                    // NOTE: this is because in C#, the "||" and "&&" operators cannot be overridden.
                    if (op == "||") op = "|";
                    if (op == "&&") op = "&";

                    if (op != null)
                    {
                        Write($"public static {ret} operator {op}{funcTypeParamsStr}({staticParamList})");
                        WriteFunctionBody(f, t, false, true, f.Name);
                    }
                }

                if (f.Name == "At")
                {
                    var paramList2 = paramList.Replace('(', '[').Replace(')', ']');
                    Write($"public {ret} this{funcTypeParamsStr}[{paramList2}]");
                    WriteFunctionBody(f, t, true, false, null);
                }

                Write($"public {ret} {f.Name}{funcTypeParamsStr}({paramList})");
                WriteFunctionBody(f, t, false, false, null);
            }

            return this;
        }

        public void AddToExtensionList(FunctionInstance f)
        {
            ExtensionList.Add(new FunctionToWrite(f, CurrentType));
        }

        public SymbolWriterCSharp WriteLibraryMethods()
        {
            WriteLine($"public static class Constants");
            WriteStartBlock();
            foreach (var f in Compilation.Libraries.AllConstants())
            {
                var retType = f.ReturnType;
                Write($"public static {retType.Name} {f.Name} => ");
                if (f.Body == null) Write($"Intrinsics.{f.Name}");
                else Write(f.Body);
                WriteLine(";");
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

        public string TypeAsInherited(TypeExpression type)
        {
            var typeArgs = type.TypeArgs;
            return type.Name + JoinTypeParameters(type.Def.IsSelfConstrained() 
                ? typeArgs.Select(TypeStr).Prepend("Self") 
                : typeArgs.Select(TypeStr));
        }

        public SymbolWriterCSharp WriteConceptInterface(TypeDef type)
        {
            Debug.Assert(type.IsConcept());

            var typeParams = JoinTypeParameters(type.IsSelfConstrained()
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
            {
                var f = m.Function;
                if (f.NumParameters == 0)
                    continue;
                var firstParamType = f.GetParameterType(0).Name;
                
                // A concept can have functions that are not defined on "itself" as the first parameter.
                // In this case, it does not make sense for that function to be in an interface (in fact it can't be).
                if (firstParamType != "Self")
                    continue;

                WriteSignature(m.Function);
            }

            WriteEndBlock();
            return this;
        }
        
        public SymbolWriterCSharp WriteTypeImplementations()
        {
            foreach (var c in Compilation.ConcreteTypes)
                if (!IgnoredTypes.Contains(c.Type.Name))
                    WriteTypeImplementation(c);
            return this;
        }

        public string ImplementedTypeString(TypeExpression te, string typeName)
        {
            var typeArgs = te.TypeArgs.Select(TypeStr);
            if (te.Def.IsSelfConstrained())
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

#if CHANGE_PRECISION
            // Precision changes
            if (t.TypeParameters.Count == 0 && fieldNames.Count != 0 && !fieldTypes.Any(ft => ft.Contains("<")))
            {
                var changeFields = fieldNames.Select(f => $"{f}.ChangePrecision()").JoinStringsWithComma();
                WriteLine($"public {OtherPrecisionNamespace}.{t.Name} ChangePrecision() => ({changeFields});");
                WriteLine($"public static implicit operator {OtherPrecisionNamespace}.{t.Name}({t.Name} self) => self.ChangePrecision();");
            }
#endif

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

                // Any time that we are implicitly casting to/from Number (floating point)
                // We can also cast from Integers. 
                if (fieldType == "Number")
                {
                    WriteLine($"public static implicit operator {name}(Integer value) => new {name}(value);");
                }
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

            // Check if the type is "Numerical", if so provide implementations of the default types. 
            var isNumerical = t.GetAllImplementedConcepts().Any(te => te.Name == "Numerical");
            if (isNumerical)
            {
                WriteLine($"// Numerical predefined functions");
                var numDistinctFieldTypes = fieldTypes.Distinct().Count();
                if (numDistinctFieldTypes != 1)
                    throw new Exception("Numerical types are assumed to have all of the fields of the same type");
                WriteLine($"public Array<Number> Components => Intrinsics.MakeArray<Number>({fieldNames.JoinStringsWithComma()});");

                var tmp = Enumerable.Range(0, fieldNames.Count).Select(i => $"numbers[{i}]").JoinStringsWithComma();
                var fromCompImpl = $"new {name}({tmp})";
                WriteLine($"public {name} FromComponents(Array<Number> numbers) => {fromCompImpl};");
            }

            // For the primitives, we have predefined implementations of the standard concepts .
            WriteLine("// Implemented concept functions and type functions");
            var funcGroups = concreteType
                .ConcreteFunctions
                .Concat(concreteType.GetConceptFunctions())
                .GroupBy(f => f.SignatureId);

            foreach (var g in funcGroups)
            {
                var f = g.First();
                if (f.ConceptName == "Array")
                {
                    continue;
                }

                WriteMemberFunction(f, concreteType);
            }

            WriteLine("// Unimplemented concept functions");
            foreach (var f in concreteType.UnimplementedFunctions)
            {
                if (IgnoredFunctions.Contains(f.Name))
                    continue;
                WriteMemberFunction(f, concreteType);
            }

            WriteEndBlock();

            WriteExtensions();

            return this;
        }

        public void WriteExtensions()
        {
            if (ExtensionList.Count == 0)
                return;

            WriteLine("public static partial class Extensions");
            WriteStartBlock();

            foreach (var f in ExtensionList)
            {
                Write($"public static {f.ReturnType} {f.Name}(this {f.ParamListStr})");
                //WriteFunctionBody(f.Func, null, false, false, null);
                WriteLine($" => throw new System.NotImplementedException();");
            }

            WriteEndBlock();

            ExtensionList.Clear();
        }

        public SymbolWriterCSharp WriteSignature(FunctionDef function)
        {
            Write(ToCSharp(function.Type));
            Write(" ");
            Write(function.Name);

            var parameters = function.Parameters.Select(p => $"{ToCSharp(p.Type)} {p.Name}").ToList();
            if (parameters.Count > 1)
            {
                Write("(");
                WriteCommaList(parameters.Skip(1));
                Write(")");
                WriteLine(";");
            }
            else
            {
                Write(" { get; }");
                WriteLine();
            }

            // Add an indexing property if the function is named "At"
            if (function.Name == "At")
            {
                Write(ToCSharp(function.Type));
                Write(" this[");
                WriteCommaList(parameters.Skip(1));
                WriteLine("] { get; }");
            }

            return this;
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

        public SymbolWriterCSharp Write(TypeExpression typeExpression) => Write(TypeStr(typeExpression)).Write(" ");

        public static string GetLiteralType(Literal literal) => literal.TypeEnum.ToString();

        public static string GetLiteralValue(Literal literal) => literal.Value.ToLiteralString();

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

            // Calling a parameter, or variable 
            if (functionCall.Function is ParameterRefSymbol 
                || functionCall.Function is VariableRefSymbol
                || functionCall.Function is FunctionCall)
            {
                return Write(functionCall.Function).Write(".Invoke(").WriteCommaList(functionCall.Args).Write(")");
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

                case ArrayLiteral arrayLiteral:
                    return Write("Intrinsics.MakeArray(")
                        .WriteCommaList(arrayLiteral.Expressions)
                        .Write(")");
            }

            throw new ArgumentOutOfRangeException(nameof(expr));
        }
    }
}