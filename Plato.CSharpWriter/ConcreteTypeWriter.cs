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
    public class ConcreteTypeWriter
    {
        public ConcreteType ConcreteType { get; }
        public TypeDef TypeDef => ConcreteType.Type;
        public string SimpleName => TypeDef.Name;
        public string Name => SimpleName + TypeParamsStr;
        public List<TypeParameterDef> TypeParameters => TypeDef.TypeParameters.ToList();
        public string TypeParamsStr => SymbolWriterCSharp.JoinTypeParameters(TypeParameters.Select(tp => tp.Name));
        public bool IsIntrinsic => SymbolWriterCSharp.IntrinsicTypes.Contains(Name);
        public bool IsPrimitive => SymbolWriterCSharp.PrimitiveTypes.ContainsKey(Name) || IsIntrinsic;
        public string Attr => SymbolWriterCSharp.Annotation;
        public bool IsNumerical => TypeDef.GetAllImplementedConcepts().Any(te => te.Name == "INumerical");
        public bool IsVectorSpace => TypeDef.GetAllImplementedConcepts().Any(te => te.Name == "IVectorSpace");
        public Compilation Compilation { get; }
        public IReadOnlyList<FunctionDef> NumberConstants { get; }

        public List<List<FunctionInstance>> ConceptFunctionGroups => ConcreteType
            .ConcreteFunctions
            .Concat(ConcreteType.GetConceptFunctions())
            .GroupBy(f => f.SignatureId)
            .Select(g => g.ToList()).ToList();

        public ConcreteTypeWriter(Compilation compilation, ConcreteType t)
        {
            Compilation = compilation;
            ConcreteType = t;
        }

        private SymbolWriterCSharp WriteImpl(SymbolWriterCSharp sw)
        {
            var floatType = sw.FloatType;
            var implements = TypeDef.Implements.Count > 0
                ? ": " + string.Join(", ",
                    TypeDef.Implements.Select(te => sw.ImplementedTypeString(te, TypeDef.Name, true)))
                : "";
            var fieldTypes = TypeDef.Fields.Select(f => sw.TypeStr(f.Type)).ToList();
            var fieldNames = TypeDef.Fields.Select(f => f.Name).ToList();
            var parameterNames = fieldNames.Select(SymbolWriterCSharp.FieldNameToParameterName).ToList();
            Debug.Assert(fieldTypes.Count == fieldNames.Count);
            var parameters = fieldTypes.Zip(parameterNames, (pt, pn) => $"{pt} {pn}");
            var parameterNamesStr = parameterNames.JoinStringsWithComma();
            var parametersStr = parameters.JoinStringsWithComma();
            var deconstructorParametersStr = fieldTypes.Zip(parameterNames, (pt, pn) => $"out {pt} {pn}").JoinStringsWithComma();
            var fieldTypesStr = string.Join(", ", fieldTypes);
            var fieldNamesStr = fieldNames.JoinStringsWithComma();
            var assignments = fieldNames.Zip(parameterNames, (fn, pn) => $"{fn} = {pn}; ").JoinStrings("");

            if (IsPrimitive)
                sw.WriteLine($"[StructLayout(LayoutKind.Sequential, Pack=1)]");
            else
                sw.WriteLine($"[DataContract, StructLayout(LayoutKind.Sequential, Pack=1)]");
            
            sw.Write($"public partial struct {Name}");
            sw.WriteLine(implements);
            sw.WriteStartBlock();

            if (!IsPrimitive)
            {
                sw.WriteLine("// Fields");
                for (var i = 0; i < fieldTypes.Count; ++i)
                    sw.WriteLine($"[DataMember] public readonly {fieldTypes[i]} {fieldNames[i]};");
                sw.WriteLine("");

                sw.WriteLine("// With functions ");
                for (var i = 0; i < fieldTypes.Count; ++i)
                {
                    var ft = fieldTypes[i];
                    var fn = fieldNames[i];
                    var pn = parameterNames[i];
                    var args = fieldNames.Select((n, j) => j == i ? pn : n).JoinStringsWithComma();
                    sw.WriteLine($"{Attr} public {Name} With{fn}({ft} {pn}) => new {Name}({args});");
                }

                sw.WriteLine();

                sw.WriteLine("// Regular Constructor");
                if (fieldNames.Count > 0)
                {
                    sw.WriteLine($"{Attr} public {SimpleName}({parametersStr}) {{ {assignments}}}");
                }

                sw.WriteLine();

                //sw.WriteLine($"public static {name} Default = new {name}();");
            }

            sw.WriteLine("// Static factory function");
            sw.WriteLine($"{Attr} public static {Name} Create({parametersStr}) => new {Name}({parameterNamesStr});");
            sw.WriteLine();

            // Implicit operators 
            if (fieldNames.Count > 1)
            {
                sw.WriteLine("// Implicit converters to/from value-tuples and deconstructor");
                var qualifiedFieldNames = fieldNames.Select(f => $"self.{f}").JoinStringsWithComma();
                var tupleNames = string.Join(", ", Enumerable.Range(1, fieldNames.Count).Select(i => $"value.Item{i}"));
                sw.WriteLine($"{Attr} public static implicit operator ({fieldTypesStr})({Name} self) => ({qualifiedFieldNames});");
                sw.WriteLine($"{Attr} public static implicit operator {Name}(({fieldTypesStr}) value) => new({tupleNames});");
                var outAssignments = fieldNames.Zip(parameterNames, (fn, pn) => $"{pn} = {fn}; ").JoinStrings("");
                sw.WriteLine($"{Attr} public void Deconstruct({deconstructorParametersStr}) {{ {outAssignments} }}");
                sw.WriteLine();
            }
            else if (fieldNames.Count == 1)
            {
                sw.WriteLine("// Implicit converters to/from single field");
                var fieldName = fieldNames[0];
                var fieldType = fieldTypes[0];

                // Only implicit operators if we are not an 
                if (IsPrimitive || !TypeDef.Fields[0].Type.Def.IsConcept())
                {
                    sw.WriteLine($"{Attr} public static implicit operator {fieldType}({Name} self) => self.{fieldName};");
                    sw.WriteLine($"{Attr} public static implicit operator {Name}({fieldType} value) => new {Name}(value);");
                }

                // Any time that we are implicitly casting to/from Number (floating point)
                // We can also cast from Plato.Integers and built-in integers, as well to/from built-in floating types 
                if (fieldType == "Number")
                {
                    sw.WriteLine($"{Attr} public static implicit operator {Name}(Integer value) => new {Name}(value);");
                    sw.WriteLine($"{Attr} public static implicit operator {Name}(int value) => new Integer(value);");
                    sw.WriteLine($"{Attr} public static implicit operator {Name}({floatType} value) => new Number(value);");
                    sw.WriteLine($"{Attr} public static implicit operator {floatType}({Name} value) => value.{fieldName};");
                }
                sw.WriteLine();
            }

            sw.WriteLine("// Object virtual function overrides: Equals, GetHashCode, ToString");
            if (!IsPrimitive)
            {
                if (fieldNames.Count > 0)
                {
                    var eqBody = fieldNames.Select(f => $"{f}.Equals(other.{f})").JoinStrings(" && ");
                    sw.WriteLine($"{Attr} public Boolean Equals({Name} other) => {eqBody};");
                    sw.WriteLine($"{Attr} public Boolean NotEquals({Name} other) => !{eqBody};");
                    sw.WriteLine($"{Attr} public override bool Equals(object obj) => obj is {Name} other ? Equals(other) : false;");
                }
                else
                {
                    sw.WriteLine($"{Attr} public override bool Equals(object obj) => obj is {Name};");
                    sw.WriteLine($"{Attr} public Boolean Equals({Name} other) => true;");
                    sw.WriteLine($"{Attr} public Boolean NotEquals({Name} other) => false;");
                    sw.WriteLine($"{Attr} public static Boolean operator==({Name} a, {Name} b) => true;");
                    sw.WriteLine($"{Attr} public static Boolean operator!=({Name} a, {Name} b) => false;");
                }
                sw.WriteLine($"{Attr} public override int GetHashCode() => Intrinsics.CombineHashCodes({fieldNamesStr});");

                var toStr = "$\"{{ " + fieldNames.Select(fn => $"\\\"{fn}\\\" = {{{fn}}}").JoinStringsWithComma() + " }}\"";
                sw.WriteLine($"{Attr} public override string ToString() => {toStr};");
            }
            else
            {
                sw.WriteLine($"{Attr} public Boolean Equals({Name} other) => Value.Equals(other.Value);");
                sw.WriteLine($"{Attr} public Boolean NotEquals({Name} other) => !Value.Equals(other.Value);");
                sw.WriteLine($"{Attr} public override bool Equals(object obj) => obj is {Name} other ? Equals(other) : false;");
                sw.WriteLine($"{Attr} public static Boolean operator==({Name} a, {Name} b) => a.Equals(b);");
                sw.WriteLine($"{Attr} public static Boolean operator!=({Name} a, {Name} b) => !a.Equals(b);");
                sw.WriteLine($"{Attr} public override int GetHashCode() => Value.GetHashCode();");
                sw.WriteLine($"{Attr} public override string ToString() => Value.ToString();");
            }
            sw.WriteLine();

            // TODO: this might be a problem for primitives. 

            sw.WriteLine("// Explicit implementation of interfaces by forwarding properties to fields");
            foreach (var te in TypeDef.GetAllImplementedConcepts())
            {
                var its = sw.ImplementedTypeString(te, Name, true);

                foreach (var f in te.Def.Functions)
                {
                    var fieldIndex = fieldNames.IndexOf(f.Name);
                    if (f.Parameters.Count == 1 && fieldIndex >= 0)
                    {
                        var fieldType = IsPrimitive ? Name : fieldTypes[fieldIndex];
                        sw.WriteLine($"{fieldType} {its}.{f.Name} {{ {Attr} get => {f.Name}; }}");
                    }
                }
            }
            sw.WriteLine();
           
            // Check if the type is "IArray", so can add an enumerator and an implicit cast to/from system array. 
            var arrayConcept = ConcreteType.AllConcepts.FirstOrDefault(c => c.Name == "IArray");
            var isArray = arrayConcept != null;
            if (isArray)
            {
                sw.WriteLine("// Array predefined functions");

                var argType = arrayConcept.Substitutions.Replace(arrayConcept.Expression.TypeArgs[0]);
                var elem = sw.ToCSharp(argType);

                // Check that there are mul
                if (fieldNames.Count > 1 && fieldTypes.All(ft => ft == elem))
                {
                    // Add a constructor from arrays 
                    var ctorArrayArgs = Enumerable.Range(0, fieldNames.Count).Select(i => $"xs[{i}]").JoinStringsWithComma();
                    sw.WriteLine($"{Attr} public {Name}(IReadOnlyList<{elem}> xs) : this({ctorArrayArgs}) {{ }}");
                    sw.WriteLine($"{Attr} public {Name}({elem}[] xs) : this({ctorArrayArgs}) {{ }}");
                    sw.WriteLine($"{Attr} public static {Name} Create(IReadOnlyList<{elem}> xs) => new {Name}(xs);");
                }

                // TODO: I think I am going to need to do some magic to make this work correctly. 
                // Allow implicit casting to System.Array
                //sw.WriteLine($"{Annotation} public static implicit operator {elem}[]({name} self) => self.ToSystemArray();");

                sw.WriteLine("// Implementation of IReadOnlyList");

                sw.WriteLine($"{Attr} public System.Collections.Generic.IEnumerator<{elem}> GetEnumerator() => new ArrayEnumerator<{elem}>(this);");
                sw.WriteLine($"{Attr} System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();");
                sw.WriteLine($"{elem} System.Collections.Generic.IReadOnlyList<{elem}>.this[int n] {{ {Attr} get => At(n); }}");
                sw.WriteLine($"int System.Collections.Generic.IReadOnlyCollection<{elem}>.Count {{ {Attr} get => this.Count; }}");
                sw.WriteLine();
            }

            if (IsVectorSpace)
            {
                sw.WriteLine($"// Vectorspace predefined functions");
             
                var numDistinctFieldTypes = fieldTypes.Distinct().Count();
                if (numDistinctFieldTypes > 1)
                    throw new Exception("IVectorSpace types are assumed to have all of the fields of the same type");

                var nComps = Math.Max(fieldTypes.Count, 1);
                sw.WriteLine($"public static readonly int NumComponents = {nComps};");
                sw.WriteLine($"public IArray<Number> Components {{ {Attr} get => Intrinsics.MakeArray<Number>({fieldNames.JoinStringsWithComma()}); }}");

                var tmp = Enumerable.Range(0, fieldNames.Count).Select(i => $"numbers[{i}]").JoinStringsWithComma();
                var fromCompImpl = $"new {Name}({tmp})";
                sw.WriteLine($"{Attr} public static {Name} CreateFromComponents(IArray<Number> numbers) => {fromCompImpl};");
                sw.WriteLine();
            }

            // For the primitives, we have predefined implementations of the standard concepts .
            sw.WriteLine("// Implemented concept functions and type functions");

            foreach (var g in ConceptFunctionGroups)
            { 
                // TODO: the best function is always one defined on intrinsic 
                // If a function is an operator function ... we will forward it. 

                var f = sw.ChooseBestFunction(g);

                // Note: we skip functions that are named after a field ... 
                if (fieldNames.Contains(f.Name))
                    continue;

                if (SymbolWriterCSharp.IgnoredFunctions.Contains(f.Name))
                    continue;

                // TEMP: currently I am inheriting all of the IArray functionality. 
                // I don't think it is that bad. 
                // We don't inherit all of the "IArray" functionality because it would be too much for vectors 
                // However, it may make sense for specific Array classes in the future. 
                if (f.ConceptName == "IArray")
                  continue;

                // We have to be sure to not implement functions that cast to themselves
                if (f.Name == SimpleName)
                    continue;

                var nextInterfacePos = f.NextInterfacePosition();
                
                // NOTE: we don't count IArray types as different.
                if (nextInterfacePos > 0 
                    && !f.ParameterTypes[nextInterfacePos].Name.StartsWith("IArray"))
                {

                    // This means multiple versions of the function will need to get created. 
                    // One for each type that implements the interface. 
                    var iface = f.ParameterTypes[nextInterfacePos];
                    var replacements = Compilation.GetImplementers(iface.Expr);

                    foreach (var r in replacements)
                    {
                        var fi = sw.ToFunctionInfoReplaceInterface(f, ConcreteType, iface.Expr, r);
                        sw.WriteMemberFunction(fi, IsPrimitive);
                    }
                }
                else
                {
                    var fi = sw.ToFunctionInfo(f, ConcreteType);
                    sw.WriteMemberFunction(fi, IsPrimitive);
                }
            }
            sw.WriteLine();


            // Primitives don't have unimplemented functions (except operators). 
            //if (!IsPrimitive)
            // TEMP:
            {
                sw.WriteLine("// Unimplemented concept functions");
                foreach (var f in ConcreteType.UnimplementedFunctions)
                {
                    if (SymbolWriterCSharp.IgnoredFunctions.Contains(f.Name))
                        continue;

                    if (f.Name == "At" || f.Name == "Count")
                    {
                        if (Name == "String")
                            continue;

                        var fi = sw.ToFunctionInfo(f, ConcreteType);
                        sw.GenerateFunc(fi, ConcreteType);
                    }
                    else
                    {
                        sw.WriteMemberFunction(sw.ToFunctionInfo(f, ConcreteType), IsPrimitive);
                    }
                }
            }

            sw.WriteEndBlock();
            return sw;

        }

        public SymbolWriterCSharp Write(SymbolWriterCSharp sw)
        {
            return sw.SetSelfType(Name, () => WriteImpl(sw));
        }
    }
}