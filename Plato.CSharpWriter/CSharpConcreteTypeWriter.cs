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

            // Scalar erasure (--scalar=float): the five scalar wrapper types are not emitted as
            // partial structs at all; their per-type file becomes an extension-method class over
            // the native primitive (plus a minimal partial-struct shim for the members that
            // handwritten Plato.Intrinsics code requires with property syntax).
            if (Writer.ScalarErase && CSharpWriter.ScalarPrimitives.ContainsKey(Name))
            {
                FieldTypes = new List<string>();
                FieldNames = new List<string>();
                ExtensionPlan = Writer.GetExtensionPlan(t.TypeDef);
                WriteScalarErasedType();
                return;
            }

            // Scalar erasure: the implements-list keeps wrapper types (concept interfaces are
            // not erased - see CSharpTypeWriter.WriteConceptInterface), while FIELDS erase.
            var saveErase = TypeWriter.EraseScalars;
            TypeWriter.EraseScalars = false;
            var implements = ConcreteType.Interfaces.Count > 0
                ? $": " + ConcreteType.Interfaces.Select(TypeWriter.ToCSharpType).JoinStringsWithComma()
                : "";
            var unerasedFieldTypes = ConcreteType.TypeDef.Fields.Select(f => TypeWriter.ToCSharpType(f.Type)).ToList();
            TypeWriter.EraseScalars = saveErase;

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
                // Scalar erasure: the field erased to "float", so the primary pair above already
                // covers float<->{Name}; add the wrapper/int bridges the V1 block provided (the
                // handwritten intrinsics and mixed bodies still traffic in Number/Integer).
                else if (Writer.ScalarErase && unerasedFieldTypes[0] == "Number")
                {
                    TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}(Integer value) => new {Name}(value);");
                    TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}(int value) => new {Name}(value);");
                    TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}(Number value) => new {Name}(value);");
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
                    // Parenthesized: a bare `!a && b` would negate only the first field's comparison.
                    TypeWriter.WriteLine($"{Attr} public Boolean NotEquals({Name} other) => !({eqBody});");
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
            // Scalar erasure: explicit interface implementations must use the interface's
            // (unerased, wrapper-typed) member types; the erased field converts implicitly.
            TypeWriter.EraseScalars = false;
            foreach (var i in t.AllInterfaces)
            {
                var its = TypeWriter.ToCSharpType(i);
                foreach (var f in i.DeclaredFunctions)
                {
                    var fieldIndex = FieldNames.IndexOf(f.Name);
                    if (f.ParameterTypes.Count == 1 && fieldIndex >= 0)
                    {
                        var fieldType = IsPrimitive ? Name : unerasedFieldTypes[fieldIndex];
                        TypeWriter.WriteLine($"{fieldType} {its}.{f.Name} {{ {Attr} get => {f.Name}; }}");
                    }
                }
            }
            TypeWriter.EraseScalars = saveErase;
            TypeWriter.WriteLine();
           
            // Check if the type is "IArray", so can add an enumerator and an implicit cast to/from system array. 
            var arrayConcept = ConcreteType.AllInterfaces.FirstOrDefault(c => c.Name == "IArray");
            var isArray = arrayConcept != null;
            if (isArray)
            {
                TypeWriter.WriteLine("// Array predefined functions");

                // Scalar erasure: the IReadOnlyList<T> interface obligation comes from the
                // unerased IArray<T> concept, so the element type must stay unerased too.
                TypeWriter.EraseScalars = false;
                var argType = arrayConcept.Substitutions.Replace(arrayConcept.TypeExpression.TypeArgs[0]);
                var elem = TypeWriter.ToCSharpType(argType);
                TypeWriter.EraseScalars = saveErase;

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
                // Scalar erasure: the PUBLIC Components/CreateFromComponents use the erased
                // element type ("float-land" arrays); the unerased IArrayLike<Self, T>
                // interface obligation is satisfied by an explicit implementation below.
                var obligationFieldType = unerasedFieldTypes.Count > 0 ? unerasedFieldTypes[0] : null;

                var localFieldNames = FieldNames;

                if (IsPrimitive)
                {
                    // TEMP: this is a bit of a hack. In the future, we may want IArrayLike primitives that are not Number.
                    fieldType = "Number";
                    obligationFieldType = "Number";
                    if (!PrimitiveFieldNames.ContainsKey(Name))
                        throw new Exception($"Unrecognized primitive IArrayLike type {Name}");
                    localFieldNames = PrimitiveFieldNames[Name].ToList();
                }

                if (Writer.ScalarErase && CSharpWriter.ScalarPrimitives.TryGetValue(obligationFieldType ?? "", out var erasedComp))
                    fieldType = erasedComp;

                var nComps = localFieldNames.Count;

                TypeWriter.WriteLine($"// IArrayLike predefined functions");
                TypeWriter.WriteLine(Writer.ScalarErase
                    ? $"public int NumComponents {{ {Attr} get => {nComps}; }}"
                    : $"public Integer NumComponents {{ {Attr} get => {nComps}; }}");
                TypeWriter.WriteLine($"public IReadOnlyList<{fieldType}> Components {{ {Attr} get => Intrinsics.MakeArray<{fieldType}>({localFieldNames.JoinStringsWithComma()}); }}");
                if (Writer.ScalarErase && fieldType != obligationFieldType)
                {
                    // Explicit implementation of the unerased IArrayLike<Self, T> obligation
                    // (the public Components above is erased; wrappers convert element-wise).
                    var arrayLikeInterface = ConcreteType.AllInterfaces.FirstOrDefault(c => c.Name == "IArrayLike");
                    if (arrayLikeInterface != null)
                    {
                        TypeWriter.EraseScalars = false;
                        var its = TypeWriter.ToCSharpType(arrayLikeInterface);
                        TypeWriter.EraseScalars = saveErase;
                        TypeWriter.WriteLine($"IReadOnlyList<{obligationFieldType}> {its}.Components {{ {Attr} get => Intrinsics.MakeArray<{obligationFieldType}>({localFieldNames.JoinStringsWithComma()}); }}");
                    }
                }
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

            // Scalar erasure: struct-KEPT members (interface obligations, operators, stubs)
            // keep wrapper-typed signatures - they must exactly match the unerased concept
            // interfaces and coexist with the handwritten intrinsics. Their bodies are still
            // normalized to "float-land" by CSharpFunctionBodyWriter (wrappers convert
            // implicitly at the boundaries). The extension-method forwarders below the struct
            // ARE erased: they are API surface, not obligations.
            TypeWriter.EraseScalars = false;
            WriteImplementedInterfaceFunctions();

            WriteUnimplementedInterfaceFunctions();
            TypeWriter.EraseScalars = saveErase;

            TypeWriter.WriteEndBlock();

            WriteExtensionMethods();
        }

        // ============================================================================
        // Scalar erasure (--scalar=float): emission of the five scalar types' files.
        //
        // No partial struct is generated. The file contains:
        //   1. public static class {Name}Extensions - every library/interface function that
        //      applies to the scalar, as a classic extension method on the PRIMITIVE:
        //        - functions with Plato bodies      -> full extension methods (erased types);
        //        - intrinsics (Body == null)        -> forwarders into the handwritten
        //                                              wrapper members: ((Number)x).Sqrt;
        //        - operator-named intrinsics        -> native primitive operators: a + b;
        //      (members the extension plan MOVED to per-library classes are skipped here -
        //      they are emitted there, with erased signatures.)
        //   2. DROPPED with a report comment: implicit-conversion operators, indexers and
        //      C# interface implementations - primitives already have their operators, and
        //      Plato interface obligations are meaningless without a generated struct.
        //   3. public partial struct {Name} shim (only when needed): the pinned
        //      HandwrittenPropertySyntaxNames members as wrapper-typed PROPERTIES (handwritten
        //      Plato.Intrinsics code accesses them with property syntax on wrapper receivers,
        //      e.g. Number.Cubic uses a.Pow3), plus any static functions (nothing else can
        //      host a static under erasure).
        // ============================================================================
        public void WriteScalarErasedType()
        {
            var prim = CSharpWriter.ScalarPrimitives[SimpleName];
            var tw = TypeWriter;
            var plan = ExtensionPlan;

            // Candidate members, in the same order the struct writer would have visited them.
            var functions = new List<FunctionInstance>();
            foreach (var g in ConcreteType.InterfaceFunctionGroups)
                functions.Add(Analyzer.ChooseBestFunction(g, out _));
            functions.AddRange(ConcreteType.UnimplementedFunctions);

            var shimMembers = new List<FunctionInstance>();
            var dropped = new List<string>();
            var emittedSignatures = new HashSet<string>();
            // Wrapper-receiver bridges: generated call sites whose receiver the writer could
            // not prove scalar keep wrapper-typed intermediates (kept members of non-scalar
            // types return Number/Boolean/... unerased); every scalar extension method gets a
            // "this {Wrapper}" twin forwarding to the primitive one. Extension receivers do
            // not apply user-defined conversions, so the twins never conflict.
            var bridges = new List<string>();
            // Implicit conversions FROM the primitive (e.g. float -> Vector2 broadcast): the
            // wrapper's generated implicit operators are dropped with the struct, so the
            // TARGET types gain "implicit operator {T}({prim})" partials instead.
            var implicitOps = new List<(string RetType, string MethodName)>();

            string WrapperBridge(CSharpFunctionInfo bfi)
            {
                var parameterTypes = bfi.ParameterTypes.Skip(1).ToList().Prepend(SimpleName);
                var parameters = bfi.ParameterNames.Zip(parameterTypes, (n, t) => $"{t} {n}").JoinStringsWithComma();
                var sig = $"{CSharpFunctionInfo.Annotation}public static {bfi.ReturnType} {bfi.Name}{bfi.ExtensionGenericsString}(this {parameters}){bfi.Constraints}";
                var args = bfi.ParameterNames.Count <= 1 ? "()" : "(" + bfi.ParameterNames.Skip(1).JoinStringsWithComma() + ")";
                return $"{sig} => (({prim}){bfi.FirstParameterName}).{bfi.Name}{args};";
            }

            void AddBridge(CSharpFunctionInfo bfi)
            {
                var key = $"{bfi.Name}{bfi.ExtensionGenericsString}({SimpleName},{bfi.ParameterTypes.Skip(1).JoinStringsWithComma()})";
                if (emittedSignatures.Add(key))
                    bridges.Add(WrapperBridge(bfi));
            }

            tw.WriteLine($"// Scalar-erased emission (--scalar=float): {SimpleName} => {prim}");
            tw.WriteLine($"public static class {SimpleName}Extensions");
            tw.WriteStartBlock();

            // Bare names inside these bodies bound to the struct scope in wrapper mode; in a
            // static class they must be re-qualified, exactly like moved library members.
            tw.ExtensionStaticQualifier = $"{Writer.Namespace}.{SimpleName}";
            tw.ExtensionInstanceNames = plan.InstanceNames;
            tw.ExtensionStaticNames = plan.StaticNames;
            tw.ExtensionReceiverIsScalar = true;

            foreach (var f in functions)
            {
                if (SkipFunction(f, false))
                    continue;

                // The pinned handwritten-property-syntax names get a wrapper-typed property on
                // the partial-struct shim whether they moved or not (handwritten intrinsics
                // like Number.Cubic access them with property syntax on wrapper receivers).
                if (SimpleName == "Number"
                    && CSharpWriter.HandwrittenPropertySyntaxNames.Contains(f.Name)
                    && f.Implementation.Body != null)
                    shimMembers.Add(f);

                if (plan.ShouldMove(f))
                {
                    // Emitted (erased) in its per-library extension class. The scalar path
                    // replaces WriteImplementedInterfaceFunctions, so it must do the same
                    // moved-member routing. It still gets a wrapper-receiver bridge here.
                    Writer.MovedMembers.Add(new MovedExtensionMember(f, ConcreteType, f.Implementation.OwnerType.Name, plan));
                    AddBridge(tw.ToFunctionInfo(f, ConcreteType.TypeDef));
                    continue;
                }

                var fi = tw.ToFunctionInfo(f, ConcreteType.TypeDef);

                if (fi.IsStatic)
                {
                    shimMembers.Add(f); // a partial struct is the only host for statics
                    continue;
                }

                // HACK preserved from the wrapper-mode forwarders: generic scalar Multiply.
                if (fi.Name == "Multiply" && fi.ParameterTypes.Count > 1 && fi.ParameterTypes[1] == "_T0" && f.Implementation.Body == null)
                    continue;

                if (fi.IsIndexer)
                {
                    dropped.Add($"indexer {fi.Name} (handwritten on the intrinsic struct)");
                    continue;
                }

                var sigKey = $"{fi.Name}{fi.ExtensionGenericsString}({fi.ParameterTypes.JoinStringsWithComma()})";
                if (!emittedSignatures.Add(sigKey))
                    continue;

                tw.ExtensionReceiverName = f.ParameterNames[0];

                if (f.Implementation.Body != null)
                {
                    // Full extension method with the Plato body, all types erased.
                    if (fi.IsImplicit)
                    {
                        // The generated implicit conversion operator moves to the TARGET
                        // type's partial struct (float -> Vector2 broadcast etc.).
                        implicitOps.Add((fi.ReturnType, fi.Name));
                        dropped.Add($"implicit operator {fi.ReturnType}({prim}) => re-homed as a partial-struct operator on {fi.ReturnType} + method {fi.Name}()");
                    }
                    tw.Write(fi.ExtensionSignature);
                    var body = new CSharpFunctionBodyWriter(tw, fi, true, false, false);
                    tw.WriteWithLineStateSync(body.ToString());
                    AddBridge(fi);
                }
                else if (fi.IsOperator)
                {
                    // Operator-named intrinsic: forward through the WRAPPER's handwritten
                    // operator (bool/string lack native <, <= etc.; for float/int the wrapper
                    // operators compile down to the primitive ones anyway).
                    dropped.Add($"operator {fi.OperatorName} (via the {SimpleName} wrapper operator); kept as method {fi.Name}()");
                    var ps = fi.ParameterNames;
                    // Only SCALAR-typed operands are cast back to their wrapper; mixed
                    // operands (Multiply(Number, Matrix4x4)) pass through unchanged.
                    string Operand(int i)
                    {
                        var platoType = f.ParameterTypes[i]?.Name;
                        return platoType != null && CSharpWriter.ScalarPrimitives.ContainsKey(platoType)
                            ? $"(({platoType}){ps[i]})"
                            : ps[i];
                    }
                    var impl = ps.Count == 1
                        ? $"{fi.OperatorName}{Operand(0)}"
                        : $"{Operand(0)} {fi.OperatorName} {Operand(1)}";
                    tw.WriteLine($"{fi.ExtensionSignature} => {impl};");
                    AddBridge(fi);
                }
                else
                {
                    // Intrinsic: forward into the handwritten wrapper member (no-arg
                    // handwritten intrinsics are properties by convention). No wrapper-receiver
                    // bridge: the wrapper already has the real member, and a same-name
                    // extension would be shadowed by it anyway.
                    tw.WriteLine(GetPrimitiveForwardingExtensionMethod(fi, SimpleName, prim, forcePropertySyntax: true));
                }

                tw.ExtensionReceiverName = null;
            }

            // Equality helper the struct scaffolding used to provide (call sites use it).
            tw.WriteLine($"{CSharpTypeWriter.Annotation} public static bool NotEquals(this {prim} a, {prim} b) => !a.Equals(b);");
            tw.WriteLine($"{CSharpTypeWriter.Annotation} public static bool NotEquals(this {SimpleName} a, {prim} b) => !(({prim})a).Equals(b);");

            if (SimpleName == "Number")
            {
                // Handwritten members of the Number wrapper that the Plato compiler cannot see
                // (Plato.Intrinsics Number.cs "TODO: Figure out why these aren't being provided
                // by Plato"); generated call sites use them on scalar receivers.
                tw.WriteLine($"{CSharpTypeWriter.Annotation} public static float Cubic(this float x, float a, float b, float c, float d) => ((Number)x).Cubic(a, b, c, d);");
                tw.WriteLine($"{CSharpTypeWriter.Annotation} public static float Linear(this float x, float a, float b) => ((Number)x).Linear(a, b);");
                tw.WriteLine($"{CSharpTypeWriter.Annotation} public static float Quadratic(this float x, float a, float b, float c) => ((Number)x).Quadratic(a, b, c);");
                tw.WriteLine($"{CSharpTypeWriter.Annotation} public static float ReciprocalSquareRootEstimate(this float x) => ((Number)x).ReciprocalSquareRootEstimate;");
            }

            if (SimpleName == "Integer")
            {
                // Handwritten intrinsic taking a WRAPPER receiver the compiler cannot see
                // (Intrinsics.MakeArray2D(this Integer, ...)); erased call sites are int-typed.
                tw.WriteLine($"{CSharpTypeWriter.Annotation} public static Ara3D.Collections.ReadOnlyList2D<T> MakeArray2D<T>(this int columns, int rows, System.Func<Integer, Integer, T> f) => ((Integer)columns).MakeArray2D(rows, f);");
            }

            if (bridges.Count > 0)
            {
                tw.WriteLine($"// Wrapper-receiver bridges: call sites the writer could not prove scalar keep");
                tw.WriteLine($"// {SimpleName}-typed intermediates (unerased kept members, handwritten intrinsics).");
                foreach (var b in bridges)
                    tw.WriteLine(b);
            }

            tw.WriteEndBlock();

            foreach (var op in implicitOps.Distinct())
            {
                // Deliberately WRAPPER-sourced (not float-sourced): a float-sourced operator
                // would make member calls like v.Multiply(floatExpr) ambiguous between
                // Multiply(Number) and Multiply(Vector2) (two one-step user conversions from
                // float). This reproduces the V1 conversion graph; the body writer restores
                // wrapper-ness of scalar arguments at non-scalar member call sites.
                tw.WriteLine($"// Scalar erasure: re-homed implicit conversion (was 'implicit operator {op.RetType}({SimpleName})' on the dropped {SimpleName} struct).");
                tw.WriteLine($"public partial struct {op.RetType}");
                tw.WriteStartBlock();
                tw.WriteLine($"{CSharpTypeWriter.Annotation} public static implicit operator {op.RetType}({SimpleName} value) => (({prim})value).{op.MethodName}();");
                tw.WriteEndBlock();
            }

            foreach (var d in dropped.Distinct())
                tw.WriteLine($"// scalar-erasure drop ({SimpleName}): {d}");

            if (shimMembers.Count > 0)
            {
                tw.ExtensionStaticQualifier = null;
                tw.ExtensionInstanceNames = null;
                tw.ExtensionStaticNames = null;
                tw.ExtensionReceiverIsScalar = false;

                tw.WriteLine($"// Minimal shim: members handwritten Plato.Intrinsics code accesses with property");
                tw.WriteLine($"// syntax on the wrapper (plus statics, which need a type to live on).");
                tw.WriteLine($"public partial struct {SimpleName}");
                tw.WriteStartBlock();
                tw.EraseScalars = false; // wrapper-typed member signatures
                foreach (var f in shimMembers)
                {
                    var fi = tw.ToFunctionInfo(f, ConcreteType.TypeDef);
                    tw.Write(fi.MethodSignature);
                    tw.WriteBody(fi, fi.IsStatic);
                }
                tw.EraseScalars = true;
                tw.WriteEndBlock();
            }
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
                var f = Analyzer.ChooseBestFunction(g, out _);

                if (SkipFunction(f))
                    continue;

                // Extension style: this function is emitted later into a library extension
                // block (see ExtensionStyleWriter) instead of as an instance member.
                if (ExtensionPlan != null && ExtensionPlan.ShouldMove(f))
                {
                    Writer.MovedMembers.Add(new MovedExtensionMember(f, ConcreteType, f.Implementation.OwnerType.Name, ExtensionPlan));
                    continue;
                }

                // A same-name tie is resolved by ChooseBestFunction's specificity rules; it used
                // to be flagged with an "// AMBIGUOUS FUNCTIONS" debug comment in the SHIPPED
                // output (which also leaked process-global Symbol ids like "Geometry_15").
                // Ambiguity now surfaces through the checker's CHK202/CHK203 diagnostics and the
                // linter — generated code is not the reporting channel.

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

        public string GetPrimitiveForwardingExtensionMethod(CSharpFunctionInfo fi, string platoType, string primType, bool forcePropertySyntax = false)
        {
            var parameterTypes = fi.ParameterTypes.Skip(1).ToList().Prepend(primType);
            var parameters = fi.ParameterNames.Zip(parameterTypes, (n, t) => $"{t} {n}").JoinStringsWithComma();
            var sig = $"{CSharpFunctionInfo.Annotation}public static {fi.ReturnType} {fi.Name}{fi.ExtensionGenericsString}(this {parameters})";
            var args = fi.ParameterNames.Count <= 1 ? "" : "(" + fi.ParameterNames.Skip(1).JoinStringsWithComma() +")";
            // Extension style: forwarded no-arg members that moved out of the struct are now
            // classic extension METHODS, so the forwarding call site needs parentheses.
            // forcePropertySyntax (scalar erasure): the forwarding target is a handwritten
            // no-arg intrinsic, which is a PROPERTY on the wrapper by convention.
            if (args == "" && !forcePropertySyntax && Writer.ExtensionStyle && Writer.MovedNoArgNames.Contains(fi.Name))
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