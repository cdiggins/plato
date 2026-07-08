using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter
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
        public bool IsPrimitive => CSharpWriter.PrimitiveTypes.ContainsKey(Name);
        public string Attr => CSharpTypeWriter.Annotation;
        public bool IsArrayLike => ConcreteType.AllInterfaces.Any(te => te.Name == "IArrayLike");
        public Compilation Compilation => TypeWriter.Writer.Compilation;
        public List<string> FieldNames { get; }
        public List<string> FieldTypes { get; }

        // Non-null only in extension style (Writer.ExtensionStyle): decides which member
        // functions move out of the struct into library extension blocks (roadmap P2.2).
        public ExtensionStylePlan ExtensionPlan { get; }

        public CSharpConcreteTypeWriter(CSharpTypeWriter typeWriter, ConcreteType t)
        {
            ConcreteType = t;
            TypeWriter = typeWriter;
            var floatType = Writer.FloatType;

            var implements = ConcreteType.Interfaces.Count > 0
                ? $": " + ConcreteType.Interfaces.Select(TypeWriter.ToCSharpType).JoinStringsWithComma()
                : "";
            FieldTypes = ConcreteType.TypeDef.Fields.Select(f => TypeWriter.ToCSharpType(f.Type)).ToList();
            FieldNames = ConcreteType.TypeDef.Fields.Select(f => f.Name).ToList();
            var parameterNames = FieldNames.Select(CSharpTypeWriter.FieldNameToParameterName).ToList();
            Debug.Assert(FieldTypes.Count == FieldNames.Count);

            if (Writer.ExtensionStyle)
                ExtensionPlan = Writer.GetExtensionPlan(t.TypeDef);
            var parameters = FieldTypes.Zip(parameterNames, (pt, pn) => $"{pt} {pn}");
            var parameterNamesStr = parameterNames.JoinStringsWithComma();
            var parametersStr = parameters.JoinStringsWithComma();
            var deconstructorParametersStr = FieldTypes.Zip(parameterNames, (pt, pn) => $"out {pt} {pn}").JoinStringsWithComma();
            var fieldTypesStr = string.Join(", ", FieldTypes);
            var fieldNamesStr = FieldNames.JoinStringsWithComma();
            var assignments = FieldNames.Zip(parameterNames, (fn, pn) => $"{fn} = {pn}; ").JoinStrings("");

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
                for (var i = 0; i < FieldTypes.Count; ++i)
                    TypeWriter.WriteLine($"[DataMember] public readonly {FieldTypes[i]} {FieldNames[i]};");
                TypeWriter.WriteLine("");

                TypeWriter.WriteLine("// With functions ");
                for (var i = 0; i < FieldTypes.Count; ++i)
                {
                    var ft = FieldTypes[i];
                    var fn = FieldNames[i];
                    var pn = parameterNames[i];
                    var args = FieldNames.Select((n, j) => j == i ? pn : n).JoinStringsWithComma();
                    TypeWriter.WriteLine($"{Attr} public {Name} With{fn}({ft} {pn}) => new {Name}({args});");
                }

                TypeWriter.WriteLine();

                TypeWriter.WriteLine("// Regular Constructor");
                if (FieldNames.Count > 0)
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
            if (FieldNames.Count > 1)
            {
                TypeWriter.WriteLine("// Implicit converters to/from value-tuples and deconstructor");
                var qualifiedFieldNames = FieldNames.Select(f => $"self.{f}").JoinStringsWithComma();
                var tupleNames = string.Join(", ", Enumerable.Range(1, FieldNames.Count).Select(i => $"value.Item{i}"));
                TypeWriter.WriteLine($"{Attr} public static implicit operator ({fieldTypesStr})({Name} self) => ({qualifiedFieldNames});");
                TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}(({fieldTypesStr}) value) => new {Name}({tupleNames});");
                var outAssignments = FieldNames.Zip(parameterNames, (fn, pn) => $"{pn} = {fn}; ").JoinStrings("");
                TypeWriter.WriteLine($"{Attr} public void Deconstruct({deconstructorParametersStr}) {{ {outAssignments} }}");
                TypeWriter.WriteLine();
            }
            else if (FieldNames.Count == 1)
            {
                TypeWriter.WriteLine("// Implicit converters to/from single field");
                var fieldName = FieldNames[0];
                var fieldType = FieldTypes[0];

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
                if (FieldNames.Count > 0)
                {
                    var eqBody = FieldNames.Select(f => $"{f}.Equals(other.{f})").JoinStrings(" && ");
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

                var toStr = "$\"{{ " + FieldNames.Select(fn => $"\\\"{fn}\\\" = {{{fn}}}").JoinStringsWithComma() + " }}\"";
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
                    var fieldIndex = FieldNames.IndexOf(f.Name);
                    if (f.ParameterTypes.Count == 1 && fieldIndex >= 0)
                    {
                        var fieldType = IsPrimitive ? Name : FieldTypes[fieldIndex];
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
                if (FieldNames.Count > 1 && FieldTypes.All(ft => ft == elem))
                {
                    // Add a constructor from arrays 
                    var ctorArrayArgs = Enumerable.Range(0, FieldNames.Count).Select(i => $"xs[{i}]").JoinStringsWithComma();
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
                var numDistinctFieldTypes = FieldTypes.Distinct().Count();
                if (numDistinctFieldTypes > 1)
                    throw new Exception("IArrayLike types are assumed to have all of the fields of the same type");

                var fieldType = FieldTypes.Count > 0 ? FieldTypes[0] : null;
                
                var localFieldNames = FieldNames;

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
                TypeWriter.WriteLine($"public IReadOnlyList<{fieldType}> Components {{ {Attr} get => Intrinsics.MakeArray<{fieldType}>({localFieldNames.JoinStringsWithComma()}); }}");
                {
                    var tmp = Enumerable.Range(0, localFieldNames.Count).Select(i => $"numbers[{i}]").JoinStringsWithComma();
                    var impl = $"new {Name}({tmp})";
                    TypeWriter.WriteLine($"{Attr} public static {Name} CreateFromComponents(IReadOnlyList<{fieldType}> numbers) => {impl};");
                    TypeWriter.WriteLine();
                }
                {
                    var tmp = Enumerable.Range(0, localFieldNames.Count).Select(i => $"x").JoinStringsWithComma();
                    var impl = $"new {Name}({tmp})";
                    TypeWriter.WriteLine($"{Attr} public static {Name} CreateFromComponent({fieldType} x) => {impl};");
                    TypeWriter.WriteLine();
                }
            }

            WriteImplementedInterfaceFunctions();

            WriteUnimplementedInterfaceFunctions();

            TypeWriter.WriteEndBlock();

            WriteExtensionMethods();
        }

        public bool SkipFunction(FunctionInstance f, bool skipFields = true)
            => SkipFunction(f, FieldNames, SimpleName, skipFields);

        // Static so ExtensionStylePlan can apply the identical filter before any
        // CSharpConcreteTypeWriter exists (plans are computed in a pre-pass).
        public static bool SkipFunction(FunctionInstance f, IReadOnlyList<string> fieldNames, string simpleName, bool skipFields = true)
        {
            // Note: we skip functions that are named after a field ...
            if (skipFields && fieldNames.Contains(f.Name))
                return true;

            if (CSharpWriter.IgnoredFunctions.Contains(f.Name))
                return true;

            if (f.InterfaceName == "IArray")
                return true;

            // We have to be sure to not implement functions that cast to themselves
            if (f.Name == simpleName)
                return true;

            return false;
        }

        public void WriteImplementedInterfaceFunctions()
        {

            TypeWriter.WriteLine("// Implemented interface functions");
            foreach (var g in ConcreteType.InterfaceFunctionGroups)
            {
                var f = Analyzer.ChooseBestFunction(g, out int cnt);

                if (SkipFunction(f))
                    continue;

                // Extension style: this function is emitted later into a library extension
                // block (see ExtensionStyleWriter) instead of as an instance member.
                if (ExtensionPlan != null && ExtensionPlan.ShouldMove(f))
                {
                    Writer.MovedMembers.Add(new MovedExtensionMember(f, ConcreteType, f.Implementation.OwnerType.Name, ExtensionPlan));
                    continue;
                }

                if (cnt > 1)
                {
                    //throw new Exception($"{cnt} ambiguous function found for {f.Name}");
                    TypeWriter.WriteLine($"// AMBIGUOUS FUNCTIONS {cnt}");
                    foreach (var tmp in g)
                    {
                        TypeWriter.WriteLine($"/* {tmp.Implementation} */");
                    }
                }

                var fi = TypeWriter.ToFunctionInfo(f, ConcreteType.TypeDef);
                TypeWriter.WriteMemberFunction(fi, IsPrimitive);
            }
            TypeWriter.WriteLine();
        }

        public void WriteUnimplementedInterfaceFunctions()
        {
            // Primitives don't have unimplemented functions (except operators). 
            //if (!IsPrimitive)
            // TEMP:
            {
                TypeWriter.WriteLine("// Unimplemented interface functions");
                foreach (var f in ConcreteType.UnimplementedFunctions)
                {
                    if (SkipFunction(f))
                        continue;

                    if (f.Name == "At" || f.Name == "Count")
                    {
                        if (Name == "String")
                            continue;

                        var fi = TypeWriter.ToFunctionInfo(f, ConcreteType.TypeDef);
                        TypeWriter.GenerateFunc(fi, ConcreteType);
                    }
                    else
                    {
                        // TODO: shouldn't this be a special function? 
                        TypeWriter.WriteMemberFunction(TypeWriter.ToFunctionInfo(f, ConcreteType.TypeDef), IsPrimitive);
                    }
                }
            }
        }

        public string GetExtensionMethod(CSharpFunctionInfo fi)
        {
            var firstArg = fi.ParameterNames[0];
            var isProp = fi.ParameterNames.Count <= 1;
            var args = isProp ?  "" : "(" + fi.ParameterNames.Skip(1).JoinStringsWithComma() + ")";
            return $"{fi.ExtensionSignature} => {firstArg}.{fi.Name}{args};";
        }

        public string GetPrimitiveForwardingExtensionMethod(CSharpFunctionInfo fi, string platoType, string primType)
        {
            var parameterTypes = fi.ParameterTypes.Skip(1).ToList().Prepend(primType);
            var parameters = fi.ParameterNames.Zip(parameterTypes, (n, t) => $"{t} {n}").JoinStringsWithComma();
            var sig = $"{CSharpFunctionInfo.Annotation}public static {fi.ReturnType} {fi.Name}{fi.ExtensionGenericsString}(this {parameters})";
            var args = fi.ParameterNames.Count <= 1 ? "" : "(" + fi.ParameterNames.Skip(1).JoinStringsWithComma() +")";
            // Extension style: forwarded no-arg members that moved out of the struct are now
            // classic extension METHODS, so the forwarding call site needs parentheses.
            if (args == "" && Writer.ExtensionStyle && Writer.MovedNoArgNames.Contains(fi.Name))
                args = "()";
            var firstParamName = fi.ParameterNames[0];
            return $"{sig} => (({platoType}){firstParamName}).{fi.Name}{args};";
        }

        public void WriteExtensionMethod(FunctionInstance f)
        {
            var tw = TypeWriter;    
            var fi = TypeWriter.ToFunctionInfo(f, ConcreteType.TypeDef);
            if (SkipFunction(f, false))
                return;
            if (fi.IsStatic) return; // We don't want to generate extension methods for static functions.

            if (f.Implementation.Body == null)
                // This is an intrinsic function , so we don't want to generate an extension method for it.
                tw.WriteLine(GetExtensionMethod(fi));

            if (CSharpWriter.PrimitiveTypes.TryGetValue(Name, out var primType))
            {
                // HACK:
                if (fi.Name == "Multiply" && fi.ParameterTypes[1] == "_T0")
                {
                    return;
                }

                // "Angle" is a special function with some space for confusion (it is not the replacement for "float"
                if (Name == "Angle") return;
                var extMethod2 = GetPrimitiveForwardingExtensionMethod(fi, Name, primType);
                tw.WriteLine(extMethod2);
            }
        }

        public void WriteExtensionMethods()
        {
            var tw = TypeWriter;
            // We don't want to generate extension methods for generic types .
            if (ConcreteType.TypeDef.TypeParameters.Count > 0)
                return; 
            tw.WriteLine("// Extension methods for the type");
            tw.WriteLine($"public static class {SimpleName}Extensions");
            tw.WriteStartBlock();

            foreach (var f in ConcreteType.UnimplementedFunctions)
                WriteExtensionMethod(f);

            foreach (var g in ConcreteType.InterfaceFunctionGroups)
            {
                var f = Analyzer.ChooseBestFunction(g, out int _);
                WriteExtensionMethod(f);
            }
            
            
            
            tw.WriteEndBlock();
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