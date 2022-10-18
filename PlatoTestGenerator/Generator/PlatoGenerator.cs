using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using PlatoAnalyzer;

namespace SourceGenerator
{
    public static class Extensions
    {
        public static string ToCommaDelimitedStrings<T>(this IEnumerable<T> self) => string.Join(", ", self);
    }

    [Generator]
    public class PlatoGenerator : ISourceGenerator
    {
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

        public static string BinaryMethodArg(string field, string name)
            => $"{field}.{name}(other.{field})";

        public static string AddBinaryMethod(string type, string name, IEnumerable<string> fieldNames)
            => $"public {type} {name}({type} other) => {fieldNames.Select(f => BinaryMethodArg(f, name)).ToCommaDelimitedStrings()}";


        public static string AddOperator(string type, string name)
        {
            switch (name)
            {
                case "Add": return $"public static {type} operator+(this T self, T other) => self.Add(other);";
                case "Subtract": return $"public static {type} operator-(this T self, T other) => self.Subtract(other);";
                case "Negate": return $"public static {type} operator-(this T self) => self.Negate();";
                case "Multiply": return $"public static {type} operator*(this T self, T other) => self.Multiply(other);";
                case "Divide": return $"public static {type} operator/(this T self, T other) => self.Divide(other);";
                case "Modulo": return $"public static {type} operator%(this T self, T other) => self.Modulo(other);";
                case "Equals": return $"public static {type} operator==(this T self, T other) => self.Equals(other);";
                case "NotEquals": return $"public static {type} operator!=(this T self, T other) => self.NotEquals(other);";
                case "GreaterThan": return $"public static {type} operator>(this T self, T other) => self.GreaterThan(other);";
                case "GreaterThanOrEquals": return $"public static {type} operator>=(this T self, T other) => self.GreaterThanOrEquals(other);";
                case "LessThan": return $"public static {type} operator<(this T self, T other) => self.LessThan(other);";
                case "LessThanOrEquals": return $"public static {type} operator<=(this T self, T other) => self.LessThanOrEquals(other);";
            }

            return null;
        }

        public StringBuilder AddImplementation(StringBuilder sb, PlatoAnalyzer.PlatoAnalyzer analyzer, PlatoClass classType)
        {
            var name = classType.Name;

            if (classType.IsValueType)
                throw new ArgumentException($"Expected {classType} to be class or struct", nameof(classType));


            var classOrStruct = classType.IsStruct ? "struct" : "class";
            sb.AppendLine($"public partial {classOrStruct} {classType.Name}");
            sb.AppendLine("{");

            var symbol = analyzer.Mapping.GetSymbol(classType);
            if (symbol != null)
                sb.AppendLine($"// Symbol = {symbol.Name} {symbol.GetType()} ");
            if (symbol is ITypeSymbol ts)
                sb.AppendLine($"// Interfaces = {ts.AllInterfaces.Select(i => i.Name).ToCommaDelimitedStrings()}");

            var props = classType.Properties;
            var propNames = props.Select(p => p.Name).ToList();
            var args = propNames.Select(p => "_" + p).ToList();
            var propTypes = props.Select(p => p.Type.Name).ToList();
            var argsWithTypes = string.Join(", ", args.Zip(propTypes, (arg, type) => $"{type} {arg}"));
            sb.AppendLine($"public {classType.Name}({argsWithTypes} => ({propNames.ToCommaDelimitedStrings()}) = ({args.ToCommaDelimitedStrings()});");

            sb.AppendLine($"public static implicit operator {name}(({argsWithTypes}) tuple) => new {name}(tuple);");
            sb.AppendLine($"public static implicit operator ({argsWithTypes})({name} self) => ({propNames.ToCommaDelimitedStrings()});");

            var stringFields = string.Join(", ", propNames.Select(f => $" \"{f}\" : {{ {f} }}"));
            sb.AppendLine($"public override string ToString() => {{ {stringFields} }};");

            // TODO: hash method
            // TODO: Parse method
            // TODO: equals 
            // TODO: ToBytes
            // TODO: FromBytes
            // TODO: deconstruct operator
            // TODO: implicit to value type / from value tuple
            // TODO: default value 
            // TODO: min value / max value

            foreach (var prop in props)
            {
                //sb.AppendLine($"public {prop.Type.Name} {prop.Name} {{ get; }}");

                var withArgs = props.Select(p => p.Name == prop.Name ? "_value" : p.Name);
                sb.AppendLine($"public {name} With{prop.Name}({prop.Type.Name} _value) => new({string.Join(", ", withArgs)});");
            }

            /*
            foreach (var method in ifaceType.GetMethods())
            {
                // TODO: add a method 
            }
            */

            sb.AppendLine("}");
            return sb;
        }


        public string AddImplementation(Type classType, Type ifaceType)
        {
            var name = classType.Name;

            if (!classType.IsClass)
                throw new ArgumentException($"Expected {classType} to be class", nameof(classType));
            if (!ifaceType.IsInterface)
                throw new ArgumentException($"Expected {ifaceType} to be interface", nameof(ifaceType));

            var sb = new StringBuilder();
            var classOrStruct = classType.IsValueType ? "struct" : "class";
            sb.AppendLine($"public partial {classOrStruct} {classType.Name}");
            sb.AppendLine("{");

            var fields = classType.GetFields();
            var args = fields.Select(f => "_" + f.Name.ToLowerInvariant()).ToList();
            var joinedArgs = string.Join(", ", args);
            var fieldNames = fields.Select(f => f.Name);
            var joinedFieldNames = string.Join(", ", fieldNames);
            var fieldTypes = fields.Select(f => f.FieldType.Name);
            var argsWithTypes = string.Join(", ", args.Zip(fieldTypes, (arg, type) => $"{type} {arg}"));
            sb.AppendLine($"public {classType.Name}({argsWithTypes} => ({joinedFieldNames}) = ({joinedArgs});");

            sb.AppendLine($"public static implicit operator {name}(({argsWithTypes}) tuple) => new {name}(tuple);");
            sb.AppendLine($"public static implicit operator ({argsWithTypes})({name} self) => ({joinedFieldNames});");

            var stringFields = string.Join(", ", fieldNames.Select(f => $" \"{f}\" : {{ {f} }}"));
            sb.AppendLine($"public override string ToString() => {{ {stringFields} }};");

            // TODO: hash method
            // TODO: Parse method
            // TODO: equals 
            // TODO: ToBytes
            // TODO: FromBytes
            // TODO: deconstruct operator
            // TODO: implicit to value type / from value tuple
            // TODO: default value 
            // TODO: min value / max value

            var fieldOffset = 0;
            foreach (var fieldInfo in fields )
            {
                if (!fieldInfo.IsInitOnly)
                    throw new Exception($"field {fieldInfo} should be readonly");

                sb.AppendLine($"[field: FieldOffset({fieldOffset})] public void {fieldInfo.FieldType} {fieldInfo.Name} {{ get; }}");

                var nSize = Marshal.SizeOf(fieldInfo.FieldType);
                fieldOffset += nSize;

                var withArgs = fields.Select(fi => fi.Name == fieldInfo.Name ? "_value" : fi.Name);
                sb.AppendLine($"public {name} With{fieldInfo.Name}({fieldInfo.FieldType.Name} _value) => new({string.Join(", ", withArgs)});");
            }

            foreach (var method in ifaceType.GetMethods())
            {
                // TODO: add a method 
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // TODO: get the classes 
            var analyzer = new PlatoAnalyzer.PlatoAnalyzer(context.Compilation);

            var sb = new StringBuilder();


            sb.AppendLine("/*");
            foreach (var c in analyzer.Mapping.GetClasses())
            {
                AddImplementation(sb, analyzer, c);
            }
            sb.AppendLine("*/");

            var source = sb.ToString();
            context.AddSource($"generated.g.cs", source);
            /*
                         *
                         */
            /*
            var types = GetAllTypes(context.Compilation);
            foreach (var namedTypeSymbol in types)
            {
                IR
            }
            */

            /*
            // Build up the source code
            var source = $@"// <auto-generated/>
using System;

namespace PlatoTestGenerator
{{
    public static class Test
    {{
        static void HelloFrom(string name) =>
            Console.WriteLine($""Generator says: Hi!"");
    }}
}}
";
            // Add the source code to the compilation
            context.AddSource($"generated.g.cs", source);
            */
        }

        // https://stackoverflow.com/questions/33420559/how-to-find-type-of-the-field-with-specific-name-in-roslyn

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}
/*

var fields = tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>();

//Get a particular variable in a field
var second = fields.SelectMany(n => n.Declaration.Variables).Where(n => n.Identifier.ValueText == "secondVariable").Single();
//Get the type of both of the first two fields.
var type = fields.First().Declaration.Type;
//Get the third variable
var third = fields.SelectMany(n => n.Declaration.Variables).Where(n => n.Identifier.ValueText == "thirdVariable").Single();


*/

/* https://stackoverflow.com/questions/37966106/how-to-see-if-a-class-has-implemented-the-interface-with-roslyn
 * var interfaceType = ...
var typeInQuestion = ...
foreach(var interfaceMember in interfaceType.GetMembers().OfType<IMethodSymbol>())
{
  var memberFound = false;
  foreach(var typeMember in typeInQuestion .GetMembers().OfType<IMethodSymbol>())
  {
    if (typeMember.Equals(typeInQuestion.FindImplementationForInterfaceMember(interfaceMember)))
    {
      // this member is found
      memberFound = true;
      break;
    }
  }
  if (!memberFound)
  {
    return false;
  }
}
return true;
 */