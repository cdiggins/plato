using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace PlatoAnalyzer
{
    /*
    public static string RemapType(string s)
    {
        switch (s)
        {
            case "Single": return "float";
            case "Int32": return "int";
            case "Boolean": return "bool";
        }
    }*/

    /*
     * TODO:
     * - I need to generate intrinsics for the built-in types.
     * - I don't want to have to explicitly implement each interface: some of them have an obvious implementation.
     *  - A whole bunch of them have automatic implementations. So what do I do?
     *  - What is an example of an automatic implementation:
     *  - To "self.Fubar(other) => new selfType(self.Component.Fubar(other.Component));"
     *  - This applies to a lot of binary operators ... what about Min/Max/Add/Subtract/ etc.
     *  - There are boolean operators (equals etc.), some are "OR" (not equals) some are "AND" (equals).
     *  - How is that specified?
     *  - I could just hard-code the logic.
     * - Intrinsics. 
     */

    [Generator]
    public class PlatoGenerator : ISourceGenerator
    {
        public static (string, string)[] BinaryOps = { ("Add", "+"), ("Subtract", "-"), ("Multiply", "*"), ("Divide", "/") };
        public static (string, string)[] VectorScalarBinaryOps = { ("Multiply", "*"), ("Divide", "/") };
        public static (string, string)[] UnaryOps = { ("Negate", "-") };
        public static (string, string)[] BinaryBooleanOps = { ("Equals", "=="), ("NotEquals", "!=") };
        public static (string, string)[] BinaryComparisonOps = { ("LessThan", "<"), ("LessThanOrEquals", "<="), ("GreaterThan", ">"), ("GreaterThanOrEquals", ">=") };
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

        public static StringBuilder GenerateIntrinsics(StringBuilder sb, string type, bool ops = true)
        {
            var t = type;
            sb.AppendLine("public static partial class Intrinsics {");

            if (ops)
            {
                foreach (var (m, op) in BinaryOps)
                    sb.AppendLine($"public static {t} {m}(this {t} a, {t} b) => a {op} b;");
                foreach (var (m, op) in UnaryOps)
                    sb.AppendLine($"public static {t} {m}(this {t} a) => {op} a;");
                foreach (var (m, op) in BinaryBooleanOps)
                    sb.AppendLine($"public static bool {m}(this {t} a, {t} b) => a {op} b;");
            }

            /*
            foreach (var (m, op) in BinaryComparisonOps)
                sb.AppendLine($"public static bool {m}(this {t} a, {t} b) => a {op} b;");
            */
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
            }
            sb.AppendLine($"public static {t} MinValue(this {t} _) => {t}.MinValue;");
            sb.AppendLine($"public static {t} MaxValue(this {t} _) => {t}.MaxValue;");
            return sb.AppendLine("}");
        }

        public static StringBuilder GenerateVectorOps(StringBuilder sb, string type, string scalarType, IReadOnlyList<string> members)
        {
            var t = type;

            foreach (var (m, op) in VectorScalarBinaryOps)
            {
                var args = members.Select(x => $"self.{x} {op} scalar").ToCommaDelimitedStrings();
                sb.AppendLine($"public static {t} operator {op}({t} self, {scalarType} scalar) => new {t}({args});");

                // This would give us a member function named "Multiply" or "Divide" that takes a scalar. Not sure it is necessary.  
                //sb.AppendLine($"public {t} {m}({t} self, {scalarType} scalar) => self {op} scalar;");
            }

            // IArray implementation 
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

        public static StringBuilder GenerateOps(StringBuilder sb, string type, IReadOnlyList<string> members)
        {
            var t = type;

            foreach (var (m, op) in BinaryOps)
            {
                var args = members.Select(x => $"a.{x} {op} b.{x}").ToCommaDelimitedStrings();
                sb.AppendLine($"public static {t} operator {op}({t} a, {t} b) => new {t}({args});");
            }
            foreach (var (m, op) in UnaryOps)
            {
                var args = members.Select(x => $"{op} a.{x}").ToCommaDelimitedStrings();
                sb.AppendLine($"public static {t} operator {op}({t} a) => new {t}({args});");
            }

            /*
            foreach (var (m, op) in BinaryComparisonOps)
            {
                var args = members.Select(x => $"a.{x} {op} b.{x}").ToCommaDelimitedStrings();
                sb.AppendLine($"public static bool operator {op}({t} a, {t} b) => new {t}({args});");
            }
            */
            return sb;
        }

        public DiagnosticDescriptor PlatoError { get; } = new DiagnosticDescriptor("PLATO001", "Error", "Exception occurred: {0}", "Plato", DiagnosticSeverity.Error, true);

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
            => classType.BaseTypes?.Select(bt => GetPlatoClass(bt, mapping)).Where(c => c != null) ?? Enumerable.Empty<PlatoClass>();

        public static IEnumerable<PlatoClass> GetInterfaces(PlatoClass classType, PlatoSemanticMapping mapping)
            => GetSelfAndBaseTypes(classType, mapping).Where(t => t?.IsInterface == true);

        public static bool IsNumeric(PlatoClass classType, PlatoSemanticMapping mapping)
            => GetInterfaces(classType, mapping).Any(iface => iface.Name == "INumber");

        public static bool IsVector(PlatoClass classType, PlatoSemanticMapping mapping, out string scalarType)
        {
            foreach (var baseType in classType.BaseTypes)
            {
                if (baseType.Name == "IVector")
                {
                    if (baseType.TypeArguments.Count > 0)
                    {
                        scalarType = baseType.TypeArguments[0].Name;
                        return true;
                    }
                }
                if (IsVector(GetPlatoClass(baseType, mapping), mapping, out scalarType))
                    return true;
            }

            scalarType = null;
            return false;
        }

        public StringBuilder AddImplementation(StringBuilder sb, PlatoAnalyzer analyzer, PlatoClass classType)
        {
            var name = classType.Name;

            var classOrStruct = classType.IsStruct ? "struct" : "class";
            sb.AppendLine($"public partial {classOrStruct} {classType.Name}");
            sb.AppendLine("{");

            // TODO: we would expect the list of interfaces to be included in the class type.
            var symbol = analyzer.Mapping.GetSymbol(classType);
            if (symbol != null)
                sb.AppendLine($"// Symbol = {symbol.Name} {symbol.GetType()} ");
            if (symbol is ITypeSymbol ts)
                sb.AppendLine($"// Interfaces = {ts.AllInterfaces.Select(i => i.Name).ToCommaDelimitedStrings()}");

            var props = classType.Properties.Where(p => p.Arrow == null).ToList();
            var propNames = props.Select(p => p.Name).ToList();
            var args = propNames.Select(p => p.ToLowerInvariant()).ToList();
            var propTypes = props.Select(p => p.Type.Name).ToList();
            var propsWithTypes = string.Join(", ", propNames.Zip(propTypes, (p, type) => $"{type} {p}"));
            var argsWithTypes = string.Join(", ", args.Zip(propTypes, (arg, type) => $"{type} {arg}"));
            sb.AppendLine($"public {classType.Name}({argsWithTypes}) => ({propNames.ToCommaDelimitedStrings()}) = ({args.ToCommaDelimitedStrings()});");

            if (propNames.Count > 1)
            {
                sb.AppendLine(
                    $"public static implicit operator {name}(({propsWithTypes}) tuple) => new {name}({string.Join(", ", propNames.Select(p => $"tuple.{p}"))});");
                sb.AppendLine(
                    $"public static implicit operator ({propsWithTypes})({name} self) => ({string.Join(", ", propNames.Select(p => $"self.{p}"))});");
                var outArgsWithTypes = string.Join(", ", args.Zip(propTypes, (arg, type) => $"out {type} {arg}"));
                sb.AppendLine(
                    $"public void Deconstruct({outArgsWithTypes}) => ({string.Join(", ", args)}) = ({string.Join(", ", propNames)});");
            }

            var stringFields = string.Join(", ", propNames.Select(f => $"\\\"{f}\\\" : {{ {f} }}"));
            sb.AppendLine($"public override string ToString() => $\"{{{{ {stringFields} }}}}\";");
            sb.AppendLine($"public override bool Equals(object other) => other is {name} typedOther && this == typedOther;");
            sb.AppendLine($"public override int GetHashCode() => ({propNames.ToCommaDelimitedStrings()}).GetHashCode();");

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

            // TODO: Parse method
            // TODO: ToBytes
            // TODO: FromBytes
            // TODO: math functions when appropriate ... like all of that scaled stuff. 
            // - How am I going to deal with the math functions 
            // TODO: implicit to array operator 
            // TODO: explicit from array function 


            foreach (var prop in props)
            {
                //sb.AppendLine($"public {prop.Type.Name} {prop.Name} {{ get; }}");

                var withArgs = props.Select(p => p.Name == prop.Name ? "value" : p.Name);
                sb.AppendLine($"public {name} With{prop.Name}({prop.Type.Name} value) => new {name}({string.Join(", ", withArgs)});");
            }

            /*
            foreach (var method in ifaceType.GetMethods())
            {
                // TODO: add a method 
            }
            */

            var isNumeric = IsNumeric(classType, analyzer.Mapping);
            var isVector = IsVector(classType, analyzer.Mapping, out var scalarType);

            if (isNumeric)
                GenerateOps(sb, name, propNames);

            if (isVector)
                GenerateVectorOps(sb, name, scalarType, propNames);

            sb.AppendLine("}");

            if (isNumeric)
                GenerateIntrinsics(sb, name, scalarType != "Byte");
            else
                GenerateIntrinsics(sb, name, false);

            //sb.AppendLine()
            return sb;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // TODO: get the classes 
            var analyzer = new PlatoAnalyzer(context.Compilation);


            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("namespace Plato {");

            var numericPrimitives = new[] { "float", "double", "int", "long", "decimal", "byte" };
            foreach (var p in numericPrimitives)
            {
                GenerateIntrinsics(sb, p, p != "byte");
            }

            //sb.AppendLine("/*");
            foreach (var c in analyzer.Mapping.GetClasses())
            {
                if (c.IsStruct)
                {
                    AddImplementation(sb, analyzer, c);
                }
            }
            //sb.AppendLine("*/");

            sb.AppendLine("} // End namespace");
            var source = sb.ToString();

            // For debugging. 

            File.WriteAllText(@"C:\GitHub\plato\Plato.Core\generated.cs", source);
            //File.WriteAllText(@"C:\GitHub\plato\test-generator\PlatoTestGenerator\PlatoTestGenerator\generated.cs", source);

            //context.AddSource($"generated.g.cs", "/* no content right now */");
        }

        // https://stackoverflow.com/questions/33420559/how-to-find-type-of-the-field-with-specific-name-in-roslyn

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}
