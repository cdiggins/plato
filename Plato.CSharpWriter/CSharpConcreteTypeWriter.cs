using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using Plato.Compiler;
using Plato.Compiler.Analysis;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    // TODO: this should probably be merged with CSharpTypeWriter. I don't see a clear advantage for it to be alone. 
    public class CSharpConcreteTypeWriter
    {
        public CSharpTypeWriter TypeWriter { get; }
        public CSharpWriter Writer => TypeWriter.Writer;
        public PlatoAnalyzer Analyzer => TypeWriter.Analyzer;
        public ConcreteType ConcreteType { get; }
        public string TypeParamsStr => ConcreteType.TypeDef.TypeParameters.Count > 0
            ? "<" + ConcreteType.TypeDef.TypeParameters.Select(tp => tp.Name).JoinStringsWithComma() + ">"
            : "";
        public string SimpleName => ConcreteType.Name;
        public string Name => SimpleName + TypeParamsStr;
        public bool IsIntrinsic => CSharpWriter.IntrinsicTypes.Contains(Name);
        public bool IsPrimitive => CSharpWriter.PrimitiveTypes.ContainsKey(Name) || IsIntrinsic;
        public string Attr => CSharpTypeWriter.Annotation;
        public bool IsArrayLike => ConcreteType.AllInterfaces.Any(te => te.Name == "IArrayLike");
        public Compilation Compilation => TypeWriter.Writer.Compilation;

        public List<List<FunctionInstance>> InterfaceFunctionGroups => ConcreteType
            .ConcreteFunctions
            .Concat(ConcreteType.GetInterfaceFunctions())
            .GroupBy(f => f.SignatureId)
            .Select(g => g.ToList()).ToList();

        public CSharpConcreteTypeWriter(CSharpTypeWriter typeWriter, ConcreteType t)
        {
            ConcreteType = t;
            TypeWriter = typeWriter;
            var floatType = Writer.FloatType;

            var implements = ConcreteType.Interfaces.Count > 0
                ? $": " + ConcreteType.Interfaces.Select(TypeWriter.ToCSharpType).JoinStringsWithComma()
                : "";
            var fieldTypes = ConcreteType.TypeDef.Fields.Select(f => TypeWriter.ToCSharpType(f.Type)).ToList();
            var fieldNames = ConcreteType.TypeDef.Fields.Select(f => f.Name).ToList();
            var parameterNames = fieldNames.Select(CSharpTypeWriter.FieldNameToParameterName).ToList();
            Debug.Assert(fieldTypes.Count == fieldNames.Count);
            var parameters = fieldTypes.Zip(parameterNames, (pt, pn) => $"{pt} {pn}");
            var parameterNamesStr = parameterNames.JoinStringsWithComma();
            var parametersStr = parameters.JoinStringsWithComma();
            var deconstructorParametersStr = fieldTypes.Zip(parameterNames, (pt, pn) => $"out {pt} {pn}").JoinStringsWithComma();
            var fieldTypesStr = string.Join(", ", fieldTypes);
            var fieldNamesStr = fieldNames.JoinStringsWithComma();
            var assignments = fieldNames.Zip(parameterNames, (fn, pn) => $"{fn} = {pn}; ").JoinStrings("");

            if (IsPrimitive)
                TypeWriter.WriteLine($"[StructLayout(LayoutKind.Sequential, Pack=1)]");
            else
                TypeWriter.WriteLine($"[DataContract, StructLayout(LayoutKind.Sequential, Pack=1)]");

            TypeWriter.Write($"public partial struct {Name}");
            TypeWriter.WriteLine(implements);
            TypeWriter.WriteStartBlock();

            if (!IsPrimitive)
            {
                TypeWriter.WriteLine("// Fields");
                for (var i = 0; i < fieldTypes.Count; ++i)
                    TypeWriter.WriteLine($"[DataMember] public readonly {fieldTypes[i]} {fieldNames[i]};");
                TypeWriter.WriteLine("");

                TypeWriter.WriteLine("// With functions ");
                for (var i = 0; i < fieldTypes.Count; ++i)
                {
                    var ft = fieldTypes[i];
                    var fn = fieldNames[i];
                    var pn = parameterNames[i];
                    var args = fieldNames.Select((n, j) => j == i ? pn : n).JoinStringsWithComma();
                    TypeWriter.WriteLine($"{Attr} public {Name} With{fn}({ft} {pn}) => new {Name}({args});");
                }

                TypeWriter.WriteLine();

                TypeWriter.WriteLine("// Regular Constructor");
                if (fieldNames.Count > 0)
                {
                    TypeWriter.WriteLine($"{Attr} public {SimpleName}({parametersStr}) {{ {assignments}}}");
                }

                TypeWriter.WriteLine();

                //sw.WriteLine($"public static {name} Default = new {name}();");
            }

            TypeWriter.WriteLine("// Static factory function");
            TypeWriter.WriteLine($"{Attr} public static {Name} Create({parametersStr}) => new {Name}({parameterNamesStr});");
            TypeWriter.WriteLine();

            TypeWriter.WriteLine("// Static default implementation");
            TypeWriter.WriteLine($"public static readonly {Name} Default = default;");
            TypeWriter.WriteLine();

            // Implicit operators 
            if (fieldNames.Count > 1)
            {
                TypeWriter.WriteLine("// Implicit converters to/from value-tuples and deconstructor");
                var qualifiedFieldNames = fieldNames.Select(f => $"self.{f}").JoinStringsWithComma();
                var tupleNames = string.Join(", ", Enumerable.Range(1, fieldNames.Count).Select(i => $"value.Item{i}"));
                TypeWriter.WriteLine($"{Attr} public static implicit operator ({fieldTypesStr})({Name} self) => ({qualifiedFieldNames});");
                TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}(({fieldTypesStr}) value) => new {Name}({tupleNames});");
                var outAssignments = fieldNames.Zip(parameterNames, (fn, pn) => $"{pn} = {fn}; ").JoinStrings("");
                TypeWriter.WriteLine($"{Attr} public void Deconstruct({deconstructorParametersStr}) {{ {outAssignments} }}");
                TypeWriter.WriteLine();
            }
            else if (fieldNames.Count == 1)
            {
                TypeWriter.WriteLine("// Implicit converters to/from single field");
                var fieldName = fieldNames[0];
                var fieldType = fieldTypes[0];

                // Only implicit operators if we are not an 
                if (IsPrimitive || !ConcreteType.TypeDef.Fields[0].Type.Def.IsInterface())
                {
                    TypeWriter.WriteLine($"{Attr} public static implicit operator {fieldType}({Name} self) => self.{fieldName};");
                    TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}({fieldType} value) => new {Name}(value);");
                }

                // Any time that we are implicitly casting to/from Number (floating point)
                // We can also cast from Plato.Integers and built-in integers, as well to/from built-in floating types 
                if (fieldType == "Number")
                {
                    TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}(Integer value) => new {Name}(value);");
                    TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}(int value) => new Integer(value);");
                    TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}({floatType} value) => new Number(value);");
                    TypeWriter.WriteLine($"{Attr} public static implicit operator {floatType}({Name} value) => value.{fieldName};");
                }
                TypeWriter.WriteLine();
            }

            TypeWriter.WriteLine("// Object virtual function overrides: Equals, GetHashCode, ToString");
            if (!IsPrimitive)
            {
                if (fieldNames.Count > 0)
                {
                    var eqBody = fieldNames.Select(f => $"{f}.Equals(other.{f})").JoinStrings(" && ");
                    TypeWriter.WriteLine($"{Attr} public Boolean Equals({Name} other) => {eqBody};");
                    TypeWriter.WriteLine($"{Attr} public Boolean NotEquals({Name} other) => !{eqBody};");
                    TypeWriter.WriteLine($"{Attr} public override bool Equals(object obj) => obj is {Name} other ? Equals(other).Value : false;");
                }
                else
                {
                    TypeWriter.WriteLine($"{Attr} public override bool Equals(object obj) => obj is {Name};");
                    TypeWriter.WriteLine($"{Attr} public Boolean Equals({Name} other) => true;");
                    TypeWriter.WriteLine($"{Attr} public Boolean NotEquals({Name} other) => false;");
                    TypeWriter.WriteLine($"{Attr} public static Boolean operator==({Name} a, {Name} b) => true;");
                    TypeWriter.WriteLine($"{Attr} public static Boolean operator!=({Name} a, {Name} b) => false;");
                }
                TypeWriter.WriteLine($"{Attr} public override int GetHashCode() => Intrinsics.CombineHashCodes({fieldNamesStr});");

                var toStr = "$\"{{ " + fieldNames.Select(fn => $"\\\"{fn}\\\" = {{{fn}}}").JoinStringsWithComma() + " }}\"";
                TypeWriter.WriteLine($"{Attr} public override string ToString() => {toStr};");
            }
            else
            {
                TypeWriter.WriteLine($"{Attr} public Boolean Equals({Name} other) => Value.Equals(other.Value);");
                TypeWriter.WriteLine($"{Attr} public Boolean NotEquals({Name} other) => !Value.Equals(other.Value);");
                TypeWriter.WriteLine($"{Attr} public override bool Equals(object obj) => obj is {Name} other ? Equals(other) : false;");
                TypeWriter.WriteLine($"{Attr} public static Boolean operator==({Name} a, {Name} b) => a.Equals(b);");
                TypeWriter.WriteLine($"{Attr} public static Boolean operator!=({Name} a, {Name} b) => !a.Equals(b);");
                TypeWriter.WriteLine($"{Attr} public override int GetHashCode() => Value.GetHashCode();");
                TypeWriter.WriteLine($"{Attr} public override string ToString() => Value.ToString();");
            }
            TypeWriter.WriteLine();

            // TODO: this might be a problem for primitives. 

            TypeWriter.WriteLine("// Explicit implementation of interfaces by forwarding properties to fields");
            foreach (var i in t.AllInterfaces)
            {
                var its = TypeWriter.ToCSharpType(i);
                foreach (var f in i.DeclaredFunctions)
                {
                    var fieldIndex = fieldNames.IndexOf(f.Name);
                    if (f.ParameterTypes.Count == 1 && fieldIndex >= 0)
                    {
                        var fieldType = IsPrimitive ? Name : fieldTypes[fieldIndex];
                        TypeWriter.WriteLine($"{fieldType} {its}.{f.Name} {{ {Attr} get => {f.Name}; }}");
                    }
                }
            }
            TypeWriter.WriteLine();
           
            // Check if the type is "IArray", so can add an enumerator and an implicit cast to/from system array. 
            var arrayConcept = ConcreteType.AllInterfaces.FirstOrDefault(c => c.Name == "IArray");
            var isArray = arrayConcept != null;
            if (isArray)
            {
                TypeWriter.WriteLine("// Array predefined functions");

                var argType = arrayConcept.Substitutions.Replace(arrayConcept.TypeExpression.TypeArgs[0]);
                var elem = TypeWriter.ToCSharpType(argType);

                // Check that there are mul
                if (fieldNames.Count > 1 && fieldTypes.All(ft => ft == elem))
                {
                    // Add a constructor from arrays 
                    var ctorArrayArgs = Enumerable.Range(0, fieldNames.Count).Select(i => $"xs[{i}]").JoinStringsWithComma();
                    TypeWriter.WriteLine($"{Attr} public {Name}(IReadOnlyList<{elem}> xs) : this({ctorArrayArgs}) {{ }}");
                    TypeWriter.WriteLine($"{Attr} public {Name}({elem}[] xs) : this({ctorArrayArgs}) {{ }}");
                    TypeWriter.WriteLine($"{Attr} public static {Name} Create(IReadOnlyList<{elem}> xs) => new {Name}(xs);");
                }

                // TODO: I think I am going to need to do some magic to make this work correctly. 
                // Allow implicit casting to System.Array
                //sw.WriteLine($"{Annotation} public static implicit operator {elem}[]({name} self) => self.ToSystemArray();");

                TypeWriter.WriteLine("// Implementation of IReadOnlyList");

                TypeWriter.WriteLine($"{Attr} public System.Collections.Generic.IEnumerator<{elem}> GetEnumerator() => new ArrayEnumerator<{elem}>(this);");
                TypeWriter.WriteLine($"{Attr} System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();");
                TypeWriter.WriteLine($"{elem} System.Collections.Generic.IReadOnlyList<{elem}>.this[int n] {{ {Attr} get => At(n); }}");
                TypeWriter.WriteLine($"int System.Collections.Generic.IReadOnlyCollection<{elem}>.Count {{ {Attr} get => this.Count; }}");
                TypeWriter.WriteLine();
            }

            if (IsArrayLike)
            {
                var numDistinctFieldTypes = fieldTypes.Distinct().Count();
                if (numDistinctFieldTypes > 1)
                    throw new Exception("IArrayLike types are assumed to have all of the fields of the same type");

                var fieldType = fieldTypes.Count > 0 ? fieldTypes[0] : null;
                
                var localFieldNames = fieldNames;

                if (IsPrimitive)
                {
                    // TEMP: this is a bit of a hack. In the future, we may want IArrayLike primitives that are not Number. 
                    fieldType = "Number";
                    if (!PrimitiveFieldNames.ContainsKey(Name))
                        throw new Exception($"Unrecognized primitive IArrayLike type {Name}");
                    localFieldNames = PrimitiveFieldNames[Name].ToList(); 
                }

                var nComps = localFieldNames.Count;

                TypeWriter.WriteLine($"// IArrayLike predefined functions");
                TypeWriter.WriteLine($"public Integer NumComponents {{ {Attr} get => {nComps}; }}");
                TypeWriter.WriteLine($"public IArray<{fieldType}> Components {{ {Attr} get => Intrinsics.MakeArray<{fieldType}>({localFieldNames.JoinStringsWithComma()}); }}");
                {
                    var tmp = Enumerable.Range(0, localFieldNames.Count).Select(i => $"numbers[{i}]").JoinStringsWithComma();
                    var impl = $"new {Name}({tmp})";
                    TypeWriter.WriteLine($"{Attr} public static {Name} CreateFromComponents(IArray<{fieldType}> numbers) => {impl};");
                    TypeWriter.WriteLine();
                }
                {
                    var tmp = Enumerable.Range(0, localFieldNames.Count).Select(i => $"x").JoinStringsWithComma();
                    var impl = $"new {Name}({tmp})";
                    TypeWriter.WriteLine($"{Attr} public static {Name} CreateFromComponent({fieldType} x) => {impl};");
                    TypeWriter.WriteLine();
                }
            }

            TypeWriter.WriteLine("// Implemented interface functions");
            foreach (var g in InterfaceFunctionGroups)
            {
                var f = Analyzer.ChooseBestFunction(g, out int cnt);
                if (cnt > 1)
                {
                    TypeWriter.WriteLine($"// AMBIGUOUS FUNCTIONS {cnt}");
                }

                // Note: we skip functions that are named after a field ... 
                if (fieldNames.Contains(f.Name))
                    continue;

                if (CSharpWriter.IgnoredFunctions.Contains(f.Name))
                    continue;

                // TEMP: currently I am inheriting all of the IArray functionality. 
                // I don't think it is that bad. 
                // We don't inherit all of the "IArray" functionality because it would be too much for vectors 
                // However, it may make sense for specific Array classes in the future. 
                if (f.InterfaceName == "IArray")
                  continue;

                // We have to be sure to not implement functions that cast to themselves
                if (f.Name == SimpleName)
                    continue;

                var fi = TypeWriter.ToFunctionInfo(f, ConcreteType);
                TypeWriter.WriteMemberFunction(fi, IsPrimitive);
            }
            TypeWriter.WriteLine();


            // Primitives don't have unimplemented functions (except operators). 
            //if (!IsPrimitive)
            // TEMP:
            {
                TypeWriter.WriteLine("// Unimplemented interface functions");
                foreach (var f in ConcreteType.UnimplementedFunctions)
                {
                    if (CSharpWriter.IgnoredFunctions.Contains(f.Name))
                        continue;

                    if (f.Name == "At" || f.Name == "Count")
                    {
                        if (Name == "String")
                            continue;

                        var fi = TypeWriter.ToFunctionInfo(f, ConcreteType);
                        TypeWriter.GenerateFunc(fi, ConcreteType);
                    }
                    else
                    {
                        // TODO: shouldn't this be a special function? 
                        TypeWriter.WriteMemberFunction(TypeWriter.ToFunctionInfo(f, ConcreteType), IsPrimitive);
                    }
                }
            }

            TypeWriter.WriteEndBlock();
        }

        public static Dictionary<string, string[]> PrimitiveFieldNames = new Dictionary<string, string[]>
        {
            { "Angle", ["Value"] },
            { "Number", ["Value"] },
            { "Vector2", ["X", "Y"] },
            { "Vector3", ["X", "Y", "Z"] },
            { "Vector4", ["X", "Y", "Z", "W"] },
            { "Vector8", [
                "X0", "X1", "X2", "X3", 
                "X4", "X5", "X6", "X7"] },
            { "Quaternion", ["X", "Y", "Z", "W"] },
            { "Matrix3x2", [
                "M11", "M12", 
                "M21", "M22", 
                "M31", "M32"] },
            { "Matrix4x4", [
                "M11", "M12", "M13", "M14", 
                "M21", "M22", "M23", "M24", 
                "M31", "M32", "M33", "M34", 
                "M41", "M42", "M43", "M44", ] },
        };
    }
}