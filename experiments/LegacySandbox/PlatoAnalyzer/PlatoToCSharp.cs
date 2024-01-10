using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace PlatoAnalyzer
{
    // TODO: convert to IR, and use that, so that I can use the JavaScript version.
    public static class PlatoToCSharp
    {
        public static (string, string)[] AdditiveBinaryOps = { ("Add", "+"), ("Subtract", "-") };

        public static (string, string)[] MultiplicativeBinaryOps =
            { ("Multiply", "*"), ("Divide", "/"), ("Modulo", "%") };

        public static (string, string)[] VectorScalarBinaryOps =
            { ("Multiply", "*"), ("Divide", "/"), ("Modulo", "%"), ("Add", "+"), ("Subtract", "-"), };

        public static (string, string)[] UnaryOps = { ("Negate", "-") };
        public static (string, string)[] BinaryBooleanOps = { ("Equals", "=="), ("NotEquals", "!=") };

        public static (string, string)[] BinaryComparisonOps =
            { ("LessThan", "<"), ("LessThanOrEquals", "<="), ("GreaterThan", ">"), ("GreaterThanOrEquals", ">=") };

        public static string[] NumericConstants = { "Zero", "One", "MinValue", "MaxValue" };

        public static HashSet<string> PrimitiveNumberTypes = new HashSet<string>
        {
            "bool", "Boolean",
            "byte", "Byte",
            "sbyte", "SByte",
            "short", "Int16",
            "ushort", "UInt16",
            "int", "Int32",
            "uint", "UInt32",
            "long", "Int64",
            "ulong", "UInt64",
            "decimal", "Decimal",
            "float", "Single",
            "double", "Double"
        };

        public static bool IsPrimitiveNumber(string type)
        {
            return PrimitiveNumberTypes.Contains(type);
        }

        public static StringBuilder GenerateIntrinsics(StringBuilder sb, string type, bool generateOps,
            bool multiplicative, bool comparable, List<string> propTypes)
        {
            var t = type;
            sb.AppendLine("public static partial class Intrinsics {");

            if (generateOps)
            {
                foreach (var (m, op) in AdditiveBinaryOps)
                {
                    sb.AppendLine($"public static {t} {m}(this {t} a, {t} b) => a {op} b;");
                    sb.AppendLine(
                        $"public static IArray<{t}> {m}(this IArray<{t}> self, IArray<{t}> other) => self.Zip(other, (a,b) => a {op} b);");
                }

                sb.AppendLine($"public static {t} Sum(this IArray<{t}> self) => self.Aggregate((a, b) => a + b);");

                if (multiplicative)
                {
                    foreach (var (m, op) in MultiplicativeBinaryOps)
                    {
                        sb.AppendLine($"public static {t} {m}(this {t} a, {t} b) => a {op} b;");
                        sb.AppendLine(
                            $"public static IArray<{t}> {m}(this IArray<{t}> self, IArray<{t}> other) => self.Zip(other, (a,b) => a {op} b);");
                    }

                    sb.AppendLine(
                        $"public static {t} Product(this IArray<{t}> self) => self.Aggregate((a, b) => a * b);");
                }

                foreach (var (m, op) in UnaryOps)
                {
                    sb.AppendLine($"public static {t} {m}(this {t} a) => {op} a;");
                    sb.AppendLine($"public static IArray<{t}> {m}(this IArray<{t}> self) => self.Select(a => {op} a);");
                }


                foreach (var (m, op) in BinaryBooleanOps)
                    sb.AppendLine($"public static bool {m}(this {t} a, {t} b) => a {op} b;");
            }

            if (comparable)
            {
                sb.AppendLine($"public static int CompareTo(this {type} self, {type} other) => self < other ? -1 : self > other ? 1 : 0;");
                sb.AppendLine($"public static IArray<int> CompareTo(this IArray<{t}> self, IArray<{t}> other) => self.Zip(other, (a,b) => a.CompareTo(b));");

                foreach (var (m, op) in BinaryComparisonOps)
                {
                    sb.AppendLine($"public static bool {m}(this {t} a, {t} b) => a {op} b;");
                    sb.AppendLine(
                        $"public static IArray<bool> {m}(this IArray<{t}> self, IArray<{t}> other) => self.Zip(other, (a,b) => a {op} b);");
                }
            }

            sb.AppendLine($"public static {t} Default(this {t} _) => default({t});");
            if (IsPrimitiveNumber(t))
            {
                sb.AppendLine($"public static {t} Zero(this {t} _) => ({t})0;");
                sb.AppendLine($"public static {t} One(this {t} _) => ({t})1;");
            }
            else
            {
                sb.AppendLine($"public static {t} Zero(this {t} _) => {t}.Zero;");
                sb.AppendLine($"public static {t} One(this {t} _) => {t}.One;");

                if (propTypes?.Count == 1)
                {
                    var propType = propTypes[0];
                    var casedPropType = propType.Substring(0, 1).ToUpperInvariant() + propType.Substring(1);
                    sb.AppendLine($"public static {propType} To{casedPropType}(this {t} self) => self;");
                }
            }

            sb.AppendLine($"public static {t} MinValue(this {t} _) => {t}.MinValue;");
            sb.AppendLine($"public static {t} MaxValue(this {t} _) => {t}.MaxValue;");

            return sb.AppendLine("}");
        }

        public static StringBuilder GenerateVectorOps(StringBuilder sb, string type, string scalarType,
            IReadOnlyList<string> members)
        {
            var t = type;

            /*
            foreach (var (m, op) in VectorScalarBinaryOps)
            {
                // Vector <op> Scalar
                {
                    var args = members.Select(x => $"self.{x} {op} scalar").ToCommaDelimitedStrings();
                    Builder.AppendLine(
                        $"public static {t} operator {op}({t} self, {scalarType} scalar) => new {t}({args});");
                }
                // Scalar <op> Vector
                {
                    var args = members.Select(x => $"scalar {op} self.{x}").ToCommaDelimitedStrings();
                    Builder.AppendLine(
                        $"public static {t} operator {op}({scalarType} scalar, {t} self) => new {t}({args});");
                }
                // This would give us a member function named "Multiply" or "Divide" that takes a scalar. Not sure it is necessary.  
                //Builder.AppendLine($"public {t} {m}({t} self, {scalarType} scalar) => self {op} scalar;");
            }
            */

            var toArrayBody = members.Select(m => $"value.{m}").ToCommaDelimitedStrings();
            sb.AppendLine($"public static implicit operator {scalarType}[]({t} value) => new[] {{ {toArrayBody} }};");

            // Common vector functions 
            sb.AppendLine($"public double Dot({t} other) => this.Multiply(other).Sum();");
            sb.AppendLine($"public double LengthSquared() => this.Dot(this);");
            sb.AppendLine($"public double Length() => System.Math.Sqrt(LengthSquared());");

            // IArray implementation 
            sb.AppendLine($"public IIterator<{scalarType}> Iterator => new ArrayIterator<{scalarType}>(this);");
            sb.AppendLine($"public int Count => {members.Count};");
            sb.AppendLine($"public {scalarType} this[int index] {{ get {{ switch (index) {{");
            for (var i = 0; i < members.Count; ++i)
            {
                sb.AppendLine($"case {i}: return {members[i]};");
            }

            sb.AppendLine("default: throw new System.ArgumentOutOfRangeException(nameof(index));");
            sb.AppendLine("} } }");

            return sb;
        }

        public static StringBuilder GenerateOps(StringBuilder sb, string type, string scalar, bool multiplicative,
            bool supportsScalar, bool comparable, IReadOnlyList<string> members)
        {
            var t = type;

            foreach (var (m, op) in AdditiveBinaryOps)
            {
                var args = members.Select(x => $"a.{x} {op} b.{x}").ToCommaDelimitedStrings();
                sb.AppendLine($"public static {t} operator {op}({t} a, {t} b) => new {t}({args});");
                args = members.Select(x => $"a {op} b.{x}").ToCommaDelimitedStrings();
                sb.AppendLine($"public static {t} operator {op}({scalar} a, {t} b) => new {t}({args});");
                args = members.Select(x => $"a.{x} {op} b").ToCommaDelimitedStrings();
                sb.AppendLine($"public static {t} operator {op}({t} a, {scalar} b) => new {t}({args});");
            }

            foreach (var (m, op) in MultiplicativeBinaryOps)
            {
                if (multiplicative)
                {
                    var args = members.Select(x => $"a.{x} {op} b.{x}").ToCommaDelimitedStrings();
                    sb.AppendLine($"public static {t} operator {op}({t} a, {t} b) => new {t}({args});");
                }

                if (supportsScalar)
                {
                    var args = members.Select(x => $"a.{x} {op} b").ToCommaDelimitedStrings();
                    sb.AppendLine($"public static {t} operator {op}({t} a, {scalar}  b) => new {t}({args});");
                    args = members.Select(x => $"a {op} b.{x}").ToCommaDelimitedStrings();
                    sb.AppendLine($"public static {t} operator {op}({scalar}  a, {t} b) => new {t}({args});");
                }
            }

            foreach (var (m, op) in UnaryOps)
            {
                var args = members.Select(x => $"{op} a.{x}").ToCommaDelimitedStrings();
                sb.AppendLine($"public static {t} operator {op}({t} a) => new {t}({args});");
            }

            if (comparable)
            {
                foreach (var (m, op) in BinaryComparisonOps)
                {
                    var args = members.Select(x => $"a.{x} {op} b.{x}").ToCommaDelimitedStrings();
                    sb.AppendLine($"public static bool operator {op}({t} a, {t} b) => {args};");
                }
            }

            return sb;
        }

        public static DiagnosticDescriptor PlatoError { get; } = new DiagnosticDescriptor("PLATO001", "Error",
            "Exception occurred: {0}", "Plato", DiagnosticSeverity.Error, true);

        public static IEnumerable<INamedTypeSymbol> GetAllTypes(Compilation compilation) =>
            GetAllTypes(compilation.GlobalNamespace);

        public static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol @namespace)
        {
            foreach (var type in @namespace.GetTypeMembers())
            foreach (var nestedType in GetNestedTypes(type))
                yield return nestedType;

            foreach (var nestedNamespace in @namespace.GetNamespaceMembers())
            foreach (var type in GetAllTypes(nestedNamespace))
                yield return type;
        }

        public static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
        {
            yield return type;
            foreach (var nestedType in type.GetTypeMembers()
                         .SelectMany(nestedType => GetNestedTypes(nestedType)))
                yield return nestedType;
        }

        public static PlatoClass GetPlatoClass(PlatoTypeExpr typeExpr, PlatoSemanticMapping mapping)
        {
            var id = typeExpr.Id;
            var symbol = mapping.IdsToSymbols[id];
            var syntaxNode = PlatoSemanticMapping.GetSymbolNode(symbol);
            var decl = mapping.GetPlatoSyntaxNode(syntaxNode);
            return decl as PlatoClass;
        }

        public static IEnumerable<PlatoClass> GetSelfAndBaseTypes(PlatoClass classType, PlatoSemanticMapping mapping)
            => GetBaseTypes(classType, mapping).SelectMany(t => GetSelfAndBaseTypes(t, mapping)).Prepend(classType);

        public static IEnumerable<PlatoClass> GetBaseTypes(PlatoClass classType, PlatoSemanticMapping mapping)
            => classType.BaseTypes?.Select(bt => GetPlatoClass(bt, mapping)).Where(c => c != null) ??
               Enumerable.Empty<PlatoClass>();

        public static IEnumerable<PlatoClass> GetInterfaces(PlatoClass classType, PlatoSemanticMapping mapping)
            => GetSelfAndBaseTypes(classType, mapping).Where(t => t?.IsInterface == true);

        public static bool IsNumeric(PlatoClass classType)
            => classType.Attributes.Any(attr =>
                attr.Name == "Vector" || attr.Name == "Number");

        public static bool IsComparable(PlatoClass classType)
            => classType.Attributes.Any(attr =>
                attr.Name == "Number" || attr.Name == "Measure");

        public static bool IsMeasure(PlatoClass classType)
            => classType.Attributes.Any(attr =>
                attr.Name == "Measure");

        public static bool IsInterval(PlatoClass classType)
            => classType.Attributes.Any(attr =>
                attr.Name == "Interval");

        // TODO: concepts derived from concepts. In the future we will look at the inheritance chain. 
        public static bool IsValue(PlatoClass classType)
            => classType.Attributes.Any(attr =>
                attr.Name == "Vector" || attr.Name == "Interval" || attr.Name == "Value" || attr.Name == "Number" ||
                attr.Name == "Measure");

        public static bool IsOperations(PlatoClass classType)
            => classType.Attributes.Any(attr => attr.Name == "Operations");

        public static bool IsVectorized(PlatoClass classType)
            => classType.Attributes.Any(attr => attr.Name == "Vectorized");

        public static bool IsVector(PlatoClass classType)
            => IsVector(classType, out _);

        public static bool IsVector(PlatoClass classType, out string scalarType)
        {
            scalarType = null;

            // Make sure each field has the same type.
            foreach (var f in classType.Fields)
            {
                if (scalarType == null)
                    scalarType = f.Type.Name;

                if (scalarType != f.Type.Name)
                    return false;
            }

            return classType.Attributes.Any(attr => attr.Name == "Vector");
        }

        public static StringBuilder AddImplementation(StringBuilder sb, PlatoAnalyzer analyzer, PlatoClass classType)
        {
            var name = classType.Name;
            var isNumeric = IsNumeric(classType);
            var isVector = IsVector(classType, out var scalarType);
            var isComparable = IsComparable(classType);
            var isMeasure = IsMeasure(classType);
            var isInterval = IsInterval(classType);
            var isMultiplicative = isNumeric || isVector;

            if (IsValue(classType))
            {
                sb.AppendLine($"[StructLayout(LayoutKind.Sequential, Pack = 1)]");
                sb.AppendLine($"public partial struct {classType.Name}");
            }
            else
            {
                sb.AppendLine($"public partial class {classType.Name}");
            }

            if (isVector)
            {
                sb.AppendLine($": IArray<{scalarType}>");
            }

            sb.AppendLine("{");

            // TODO: we would expect the list of interfaces to be included in the class type.
            var symbol = analyzer.Mapping.GetSymbol(classType);
            if (symbol != null)
                sb.AppendLine($"// Symbol = {symbol.Name} {symbol.GetType()} ");
            if (symbol is ITypeSymbol ts)
                sb.AppendLine($"// Interfaces = {ts.AllInterfaces.Select(i => i.Name).ToCommaDelimitedStrings()}");

            //var props = classType.Properties.Where(p => p.Arrow == null).ToList();
            var props = classType.Fields.ToList();
            var propNames = props.Select(p => p.Name).ToList();
            var args = propNames.Select(p => p.ToLowerInvariant()).ToList();
            var propTypes = props.Select(p => p.Type.Name).ToList();
            var propsWithTypes = string.Join(", ", propNames.Zip(propTypes, (p, type) => $"{type} {p}"));
            var argsWithTypes = string.Join(", ", args.Zip(propTypes, (arg, type) => $"{type} {arg}"));
            sb.AppendLine($"public {classType.Name}({argsWithTypes}) => ({propNames.ToCommaDelimitedStrings()}) = ({args.ToCommaDelimitedStrings()});");

            sb.AppendLine($"public static {classType.Name} Create({argsWithTypes}) => new {classType.Name}({args.ToCommaDelimitedStrings()});");

            if (IsMeasure(classType))
            {
                sb.AppendLine($"public const string Units = nameof({propNames[0]});");
            }

            foreach (var p in props)
            {
                sb.AppendLine($"public {p.Type.Name} {p.Name} {{ get; }}");
            }

            if (propNames.Count > 1)
            {
                sb.AppendLine($"public static implicit operator {name}(({propsWithTypes}) tuple) => new {name}({string.Join(", ", propNames.Select(p => $"tuple.{p}"))});");
                sb.AppendLine($"public static implicit operator ({propsWithTypes})({name} self) => ({string.Join(", ", propNames.Select(p => $"self.{p}"))});");
                var outArgsWithTypes = string.Join(", ", args.Zip(propTypes, (arg, type) => $"out {type} {arg}"));
                sb.AppendLine($"public void Deconstruct({outArgsWithTypes}) => ({string.Join(", ", args)}) = ({string.Join(", ", propNames)});");
            }
            else
            {
                var propName = propNames[0];
                var propType = propTypes[0];

                // Single field structs are automatically convertible to/from 
                sb.AppendLine($"public static implicit operator {name}({propType} value) => new {name}(value);");
                sb.AppendLine($"public static implicit operator {propType}({name} value) => value.{propName};");
            }

            if (isInterval)
            {
                var propType = propTypes[0];
                sb.AppendLine(
                    $"public static implicit operator {name}({propType} value) => new {name}(default, value);");
            }

            var stringFields = string.Join(", ", propNames.Select(f => $"\\\"{f}\\\" : {{ {f} }}"));
            sb.AppendLine($"public override string ToString() => $\"{{{{ {stringFields} }}}}\";");
            sb.AppendLine(
                $"public override bool Equals(object other) => other is {name} typedOther && this == typedOther;");
            sb.AppendLine(
                $"public override int GetHashCode() => ({propNames.ToCommaDelimitedStrings()}).GetHashCode();");

            sb.AppendLine($"public static readonly {name} Default = default;");

            foreach (var constant in NumericConstants)
            {
                var fields = string.Join(",", propNames.Select(n => $"Default.{n}.{constant}()"));
                sb.AppendLine($"public static {name} {constant} = new {name}({fields});");
            }

            {
                var impl = string.Join(" && ", propNames.Select(x => $"(a.{x} == b.{x})"));
                sb.AppendLine($"public static bool operator ==({name} a, {name} b) => {impl};");
            }

            {
                var impl = string.Join(" || ", propNames.Select(x => $"(a.{x} != b.{x})"));
                sb.AppendLine($"public static bool operator !=({name} a, {name} b) => {impl};");
            }

            foreach (var prop in props)
            {
                var withArgs = props.Select(p => p.Name == prop.Name ? "value" : p.Name);
                sb.AppendLine(
                    $"public {name} With{prop.Name}({prop.Type.Name} value) => new {name}({string.Join(", ", withArgs)});");
            }

            if (isNumeric || isComparable)
                GenerateOps(sb, name, scalarType, isMultiplicative, isNumeric || isMeasure, isComparable, propNames);

            if (isVector)
                GenerateVectorOps(sb, name, scalarType, propNames);

            if (isComparable)
                sb.AppendLine($"public int CompareTo({name} other) => this < other ? -1 : this > other ? 1 : 0;");

            sb.AppendLine("}");

            GenerateIntrinsics(sb, name, isNumeric, isMultiplicative, isComparable, propTypes);

            return sb;
        }

        public static void OutputMethodBody(PlatoFunction method, StringBuilder sb)
        {
            sb.AppendLine("{");
            // Body
            sb.AppendLine(method.Body.ToString());
            sb.AppendLine("}");
        }

        public static string DefaultExpression(PlatoParameter p)
        {
            if (p.DefaultValue == null) return null;
            return $" = {p.DefaultValue.ToFormattedString()}";
        }

        public static void OutputVectorizedMethod(PlatoFunction f, PlatoClass vectorType, StringBuilder sb)
        {
            var rt = f.ReturnType.ToString();
            var parameters = f.Parameters.Parameters;

            if (rt != "double" || parameters.Count < 1)
                return;

            var firstParam = parameters[0];
            if (firstParam.Type.ToString() != "double")
                return;

            sb.Append($"public static {vectorType.Name} {f.Name}(this {vectorType.Name} {firstParam.Name}");
            for (var i = 1; i < parameters.Count; ++i)
            {
                var p = parameters[i];
                if (p.Type.ToString() == "double")
                {
                    sb.Append($", {vectorType.Name} {p.Name}");
                }
                else
                {
                    sb.Append($", {p.Type} {p.Name}");
                }
            }

            sb.Append(") => (");

            var elements = vectorType.Fields.Select(field => field.Name).ToList();
            var scalarType = vectorType.Fields[0].Type.ToString();

            for (var i = 0; i < elements.Count; ++i)
            {
                var member = elements[i];
                if (i > 0) sb.Append(", ");
                sb.Append($"({scalarType})");
                sb.Append(f.Name);
                sb.Append("(");
                for (var j = 0; j < parameters.Count; ++j)
                {
                    if (j > 0) sb.Append(", ");
                    var p = parameters[j];
                    if (p.Type.ToString() == "double")
                    {
                        sb.Append($"(double){p.Name}.{member}");
                    }
                    else
                    {
                        sb.Append(p.Name);
                    }
                }

                sb.Append($")");
            }

            sb.AppendLine(");");
        }

        public static bool IsPrimitive(string type)
        {
            return PrimitiveNumberTypes.Contains(type);
        }
    
        public static void OutputOperations(PlatoClass cls, List<PlatoClass> vectorTypes, StringBuilder sb)
        {
            // TODO: walk through the class's functions.
            // make sure there are only functions for now.
            // create a public static class with the same name
            // Create a whole bunch of functions: each is a public static function with a This
            // Make all of those functions work for Arrays as well. 
            // Arrays of 1, 2, or 3 arguments. 
            // NOTE: where is "IArray" defined?
            // NOTE: should those functions exist on the class? Nah. 

            // TODO: would be nice ... optimize the "functional" versions!
            // TODO: make it an optimization, and have benchmarks and stuff 

            if (cls.Fields.Count > 0)
                throw new System.Exception("Fields not supported in an operations class");
            
            if (cls.Properties.Count > 0)
                throw new System.Exception("Properties not supported in an operations class");

            sb.AppendLine($"public static class {cls.Name} {{");
            for (var i = 0; i < cls.Methods.Count; ++i)
            {
                var m = cls.Methods[i];
                var rt = m.ReturnType.ToString();
                var parameters = m.Parameters.Parameters.Select(p => $"{p.Type} {p.Name}{DefaultExpression(p)}").ToCommaDelimitedStrings();
                var args = m.Parameters.Parameters.Select(p => p.Name).ToCommaDelimitedStrings();

                if (m.Parameters.Parameters.Count > 0)
                {
                    sb.AppendLine($"public static {rt} {m.Name}(this {parameters})");
                }
                else
                {
                    sb.AppendLine($"public static {rt} {m.Name}()");
                }

                OutputMethodBody(m, sb);

                // Extension methods on doubles are extended to ints and floats. 
                if (parameters.StartsWith("double "))
                {
                    sb.AppendLine($"public static {rt} {m.Name}(this int {parameters.Substring(7)}) => {m.Name}((double){args});");
                    sb.AppendLine($"public static {rt} {m.Name}(this float {parameters.Substring(7)}) => {m.Name}((double){args});");
                }

                if (vectorTypes != null)
                {
                    foreach (var vt in vectorTypes)
                    {
                        OutputVectorizedMethod(m, vt, sb);
                    }
                }
            }

            sb.AppendLine("}");

            // Any new operators? 
            // Any new properties?

            for (var i = 0; i < cls.Methods.Count; ++i)
            {
                var m = cls.Methods[i];
                var rType = m.ReturnType.ToString();
                if (m.Parameters.Parameters.Count > 0)
                {
                    if (Operators.ContainsKey(m.Name))
                    {
                        var p0 = m.Parameters.Parameters[0];
                        var pType = p0.Type.ToString();
                        var op = Operators[m.Name];
                        sb.AppendLine($"public partial struct {pType}");
                        sb.AppendLine("{");
                        var parameters = m.Parameters.Parameters.Select(p => $"{p.Type} {p.Name}").ToCommaDelimitedStrings();
                        var args = m.Parameters.Parameters.Select(p => p.Name).ToCommaDelimitedStrings();
                        sb.AppendLine($"public static {rType} operator {op}({parameters}) => {cls.Name}.{m.Name}({args});");
                        sb.AppendLine("}");
                    }
                }
                else
                {
                    if (!IsPrimitive(rType))
                    {
                        sb.AppendLine($"public partial struct {rType}");
                        sb.AppendLine("{");
                        sb.AppendLine($"public static {rType} {m.Name} => {cls.Name}.{m.Name}();");
                        sb.AppendLine("}");
                    }
                }

                // "ToBla(x)" becomes a property "x.Bla"
                if (m.Name.StartsWith("To") && m.Parameters.Parameters.Count == 1)
                {
                    var p0 = m.Parameters.Parameters[0];
                    var pType = p0.Type.ToString();
                    if (!IsPrimitive(pType))
                    {
                        sb.AppendLine($"public partial struct {pType}");
                        sb.AppendLine("{");
                        var parameters = m.Parameters.Parameters.Select(p => $"{p.Type} {p.Name}")
                            .ToCommaDelimitedStrings();
                        var args = m.Parameters.Parameters.Select(p => p.Name).ToCommaDelimitedStrings();
                        sb.AppendLine($"public {rType} {m.Name.Substring(2)} => {cls.Name}.{m.Name}(this);");
                        sb.AppendLine("}");
                    }
                }
            }
        }

        public static Dictionary<string, string> Operators = new Dictionary<string, string>()
        {
            { "Multiply", "*" },
            { "Divide", "/" },
            { "Add", "+" },
            { "Subtract", "-" },
            { "Modulo", "%" },
            { "And", "&&" },
            { "Or", "||" },
            { "Not", "!" },
        };

        public static void OutputOperations(Compilation compilation, string outputFile)
        {
            var analyzer = new PlatoAnalyzer(compilation);

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using System.Runtime.InteropServices;");
            sb.AppendLine("using System.Runtime.Serialization;");

            // TODO: this should be replaced with the Plato collection library 
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");

            sb.AppendLine("namespace Plato {");

            var vectorTypes = analyzer.Mapping.GetClasses().Where(IsVector).ToList();

            foreach (var c in analyzer.Mapping.GetClasses())
            {
                if (IsOperations(c))
                {
                    OutputOperations(c, null, sb);
                }
                if (IsVectorized(c))
                {
                    OutputOperations(c, vectorTypes, sb);
                }
            }

            sb.AppendLine("} // End namespace");
            var source = sb.ToString();
            File.WriteAllText(outputFile, source);
        }

        public static void OutputTypes(Compilation compilation, string outputFile)
        {
            var analyzer = new PlatoAnalyzer(compilation);

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using System.Runtime.InteropServices;");
            sb.AppendLine("using System.Runtime.Serialization;");

            // TODO: this should be replaced with the Plato collection library 
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");

            sb.AppendLine("namespace Plato {");

            var numericPrimitives = new[] { "float", "double", "int", "long", "decimal", "byte" };
            foreach (var p in numericPrimitives)
            {
                var notByte = p != "byte";
                GenerateIntrinsics(sb, p, notByte, true, notByte, null);
            }

            foreach (var c in analyzer.Mapping.GetClasses())
            {
                if (IsValue(c))
                {
                    AddImplementation(sb, analyzer, c);
                }
            }

            sb.AppendLine("} // End namespace");

            var source = sb.ToString();
            File.WriteAllText(outputFile, source);
        }
    }
}