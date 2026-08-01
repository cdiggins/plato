using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
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

        /// <summary>The `where` clauses this type's DECLARED bounds require (plato-382):
        /// `type Tween&lt;T&gt; where T: Interpolatable` emits
        /// `public partial struct Tween&lt;T&gt; ... where T : Interpolatable&lt;T&gt;`. Empty for
        /// an unbounded or non-generic type, which is every type in the shipping tiers today.
        /// The struct DECLARATION is the only place C# wants it: every other mention of the type
        /// inside its own file (`With`, `Create`, `Default`, the operators) is a USE of the already
        /// declared parameters, and generic types are excluded from the per-type extension class
        /// (see <see cref="WriteExtensionMethods"/>) and keep all their members in extension style
        /// (see ExtensionStylePlan's isGeneric), so no second declaration of `T` exists.</summary>
        public string WhereClauses
            => CSharpBoundWriter.WhereClauses(ConcreteType.TypeDef.TypeParameters, TypeWriter);
        public bool IsPrimitive => CSharpWriter.PrimitiveTypes.ContainsKey(Name);

        /// <summary>The handwritten runtime supplies this type's bodiless members and operators —
        /// true for primitives and for the shape-generated-but-behaviour-handwritten types
        /// (<see cref="CSharpWriter.IntrinsicBackedTypes"/>, plato-365).</summary>
        public bool IsIntrinsicBacked => CSharpWriter.IsIntrinsicBacked(Name);
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

            // Sum type (wave-2, plato-232): a tagged struct — one `int Kind` discriminant plus the
            // flattened per-case fields — emitted by a dedicated path (design doc §5). The flattened
            // field names serve as this writer's FieldNames so the shared library-function tail skips
            // any collision and the plan treats them as struct-surface.
            if (t.TypeDef.IsSum)
            {
                var flat = t.TypeDef.Cases.SelectMany(cs => cs.Fields).ToList();
                FieldNames = flat.Select(f => f.FlatName).ToList();
                FieldTypes = flat.Select(f => TypeWriter.ToCSharpType(f.Type)).ToList();
                if (Writer.ExtensionStyle)
                    ExtensionPlan = Writer.GetExtensionPlan(t.TypeDef);
                WriteSumType(flat);
                return;
            }

            // The concept interfaces erase exactly when the recipe erases (see
            // CSharpTypeWriter.WriteConceptInterface), so the implements-list follows them.
            // unerasedFieldTypes stays truly unerased (it feeds analyses, not emission).
            var saveErase = TypeWriter.EraseScalars;
            var implements = ConcreteType.Interfaces.Count > 0
                ? $": " + ConcreteType.Interfaces.Select(TypeWriter.ToCSharpType).JoinStringsWithComma()
                : "";
            TypeWriter.EraseScalars = false;
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
            TypeWriter.WriteLine(implements + WhereClauses);
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

                // Only implicit operators if the field type does not render as a C# interface
                // (user-defined conversions to/from an interface are illegal, CS0552). The
                // IsInterface check covers concept-typed fields; the IReadOnlyList check covers
                // the concrete Array/Array2D/Array3D types, which render as list interfaces.
                //
                // plato-308: a PRIMITIVE-backed type (a handwritten Plato.Intrinsics struct, e.g.
                // Angle) declares its own field-type and Number conversions, so generating those
                // here duplicates them (CS0557). Skip exactly that pair for primitives — the
                // Integer/int bridges below are NOT handwritten and stay generated. Legacy stdlib
                // declares its primitives with zero fields, so this branch never runs there.
                if (!IsPrimitive
                    && !ConcreteType.TypeDef.Fields[0].Type.Def.IsInterface()
                    && !fieldType.StartsWith("IReadOnlyList"))
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
                // plato-308: for a primitive-backed type the Number bridge is handwritten
                // (Angle declares implicit Angle(Number)); only the Integer/int bridges are ours.
                else if (Writer.ScalarErase && unerasedFieldTypes[0] == "Number")
                {
                    TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}(Integer value) => new {Name}(value);");
                    TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}(int value) => new {Name}(value);");
                    if (!IsPrimitive)
                        TypeWriter.WriteLine($"{Attr} public static implicit operator {Name}(Number value) => new {Name}(value);");
                }
                TypeWriter.WriteLine();
            }

            WriteIntrinsicVectorBridge();

            TypeWriter.WriteLine("// Object virtual function overrides: Equals, GetHashCode, ToString");
            // Under erasure the Boolean-returning scaffolding erases to bool (and Equals(other) is
            // then already a bool, so no .Value unwrap).
            var boolT = Writer.ScalarErase ? "bool" : "Boolean";
            var boolVal = Writer.ScalarErase ? "" : ".Value";
            if (!IsPrimitive)
            {
                if (FieldNames.Count > 0)
                {
                    var eqBody = FieldNames.Select(f => $"{f}.Equals(other.{f})").JoinStrings(" && ");
                    TypeWriter.WriteLine($"{Attr} public {boolT} Equals({Name} other) => {eqBody};");
                    // Parenthesized: a bare `!a && b` would negate only the first field's comparison.
                    TypeWriter.WriteLine($"{Attr} public {boolT} NotEquals({Name} other) => !({eqBody});");
                    TypeWriter.WriteLine($"{Attr} public override bool Equals(object obj) => obj is {Name} other ? Equals(other){boolVal} : false;");
                }
                else
                {
                    TypeWriter.WriteLine($"{Attr} public override bool Equals(object obj) => obj is {Name};");
                    TypeWriter.WriteLine($"{Attr} public {boolT} Equals({Name} other) => true;");
                    TypeWriter.WriteLine($"{Attr} public {boolT} NotEquals({Name} other) => false;");
                    TypeWriter.WriteLine($"{Attr} public static {boolT} operator==({Name} a, {Name} b) => true;");
                    TypeWriter.WriteLine($"{Attr} public static {boolT} operator!=({Name} a, {Name} b) => false;");
                }
                TypeWriter.WriteLine($"{Attr} public override int GetHashCode() => Intrinsics.CombineHashCodes({fieldNamesStr});");

                var toStr = "$\"{{ " + FieldNames.Select(fn => $"\\\"{fn}\\\" = {{{fn}}}").JoinStringsWithComma() + " }}\"";
                TypeWriter.WriteLine($"{Attr} public override string ToString() => {toStr};");
            }
            // A primitive whose handwritten counterpart is a WRAPPER (the five scalars) carries a
            // `Value` payload the scaffolding reads. `Type` and the Function arities do not: they
            // map straight onto System.Type / System.Func and no Plato.Intrinsics partial declares
            // a Value, so the same scaffolding would name a member that does not exist (measured:
            // `Value` binding to the concept of that name instead, CS0305 + CS1061). They get the
            // field-less shape — every value of such a type is interchangeable here.
            else if (!CSharpWriter.ScalarPrimitives.ContainsKey(SimpleName))
            {
                TypeWriter.WriteLine($"{Attr} public override bool Equals(object obj) => obj is {Name};");
                TypeWriter.WriteLine($"{Attr} public {boolT} Equals({Name} other) => true;");
                TypeWriter.WriteLine($"{Attr} public {boolT} NotEquals({Name} other) => false;");
                TypeWriter.WriteLine($"{Attr} public static {boolT} operator==({Name} a, {Name} b) => true;");
                TypeWriter.WriteLine($"{Attr} public static {boolT} operator!=({Name} a, {Name} b) => false;");
                TypeWriter.WriteLine($"{Attr} public override int GetHashCode() => 0;");
                TypeWriter.WriteLine($"{Attr} public override string ToString() => \"{SimpleName}\";");
            }
            else
            {
                TypeWriter.WriteLine($"{Attr} public {boolT} Equals({Name} other) => Value.Equals(other.Value);");
                TypeWriter.WriteLine($"{Attr} public {boolT} NotEquals({Name} other) => !Value.Equals(other.Value);");
                TypeWriter.WriteLine($"{Attr} public override bool Equals(object obj) => obj is {Name} other ? Equals(other) : false;");
                TypeWriter.WriteLine($"{Attr} public static {boolT} operator==({Name} a, {Name} b) => a.Equals(b);");
                TypeWriter.WriteLine($"{Attr} public static {boolT} operator!=({Name} a, {Name} b) => !a.Equals(b);");
                TypeWriter.WriteLine($"{Attr} public override int GetHashCode() => Value.GetHashCode();");
                TypeWriter.WriteLine($"{Attr} public override string ToString() => Value.ToString();");
            }
            TypeWriter.WriteLine();

            // TODO: this might be a problem for primitives. 

            TypeWriter.WriteLine("// Explicit implementation of interfaces by forwarding properties to fields");
            // The interfaces erase with the recipe and declare METHODS, so the explicit forwarders
            // match them member-for-member.
            var emittedExplicitImpls = new HashSet<string>();
            foreach (var i in t.AllInterfaces)
            {
                var its = TypeWriter.ToCSharpType(i);
                // plato-311: a property-form (or pinned-name) member satisfies the generic
                // interface only through its explicit implementation, so the concept's non-generic
                // existential view — reached transitively via `C<Self> : C` — needs its own
                // explicit implementation for the same member; the view spelling of the interface
                // name is the only difference. Members satisfied by ordinary public methods
                // satisfy both interfaces implicitly and never reach this loop.
                var viewIts = i.Interface.IsSelfConstrained() && i.Interface.HasObjectSafeSurface()
                    ? TypeWriter.ToCSharpViewType(i)
                    : null;
                foreach (var f in i.DeclaredFunctions)
                {
                    var viewTarget = viewIts != null && Compiler.Symbols.TypeDef.IsObjectSafeFunction(f.Implementation)
                        ? viewIts
                        : null;
                    var fieldIndex = FieldNames.IndexOf(f.Name);
                    if (f.ParameterTypes.Count == 1 && fieldIndex >= 0)
                    {
                        // The interface and the field erase together, so the field's own rendered
                        // type is what the obligation wants.
                        var fieldType = IsPrimitive ? Name : FieldTypes[fieldIndex];
                        if (emittedExplicitImpls.Add($"{its}.{f.Name}"))
                            TypeWriter.WriteLine($"{Attr} {fieldType} {its}.{f.Name}() => {f.Name};");
                        if (viewTarget != null && emittedExplicitImpls.Add($"{viewTarget}.{f.Name}"))
                            TypeWriter.WriteLine($"{Attr} {fieldType} {viewTarget}.{f.Name}() => {f.Name};");
                    }
                    // An obligation whose name is PINNED to property syntax (a handwritten
                    // Plato.Intrinsics property like Quaternion.Inverse) cannot become a method on
                    // the struct; an explicit interface implementation forwards the method
                    // obligation to the property.
                    else if (f.ParameterTypes.Count == 1
                        && Writer.IsStructSurfaceProperty(SimpleName, f.Name))
                    {
                        var retType = TypeWriter.ToCSharpType(f.ReturnType);
                        // Pinned-name struct members are uniformly PROPERTIES (handwritten on
                        // primitive types, generated elsewhere) — plain member access.
                        if (emittedExplicitImpls.Add($"{its}.{f.Name}"))
                            TypeWriter.WriteLine($"{Attr} {retType} {its}.{f.Name}() => {f.Name};");
                        if (viewTarget != null && emittedExplicitImpls.Add($"{viewTarget}.{f.Name}"))
                            TypeWriter.WriteLine($"{Attr} {retType} {viewTarget}.{f.Name}() => {f.Name};");
                    }
                    // A `_`-receiver FILL discharging an INSTANCE obligation: the fill emits as a
                    // type-level `public static` member (its receiver value is ignored), which C#
                    // cannot use to satisfy an instance interface member (CS0736). The pair
                    // static + explicit implementation gives both surfaces: `T.Zero()` on the
                    // type, and the interface/instance view forwarding to it. The static is never
                    // object-safe, so the non-generic view interface (viewTarget) is untouched.
                    else if (f.ParameterNames.Count >= 1 && f.ParameterNames[0] != "_")
                    {
                        var fill = ConcreteType.ImplementedFunctions.FirstOrDefault(m =>
                            m.Name == f.Name
                            && m.ParameterNames.Count == f.ParameterNames.Count
                            && m.ParameterNames[0] == "_"
                            && m.Implementation?.Body != null);
                        if (fill != null && emittedExplicitImpls.Add($"{its}.{f.Name}"))
                        {
                            var ifi = TypeWriter.ToFunctionInfo(f, ConcreteType.TypeDef);
                            var tailParams = ifi.MethodParameters.JoinStringsWithComma();
                            var tailArgs = ifi.ParameterNames.Skip(1).JoinStringsWithComma();
                            TypeWriter.WriteLine($"{Attr} {ifi.ReturnType} {its}.{f.Name}({tailParams}) => {f.Name}({tailArgs});");
                        }
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

                // The IArray<T> concept erases with the recipe, so the IReadOnlyList<T> element
                // type erases with it.
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
                // The IArrayLike obligation erases with the recipe, so these satisfy it directly —
                // no explicit unerased twin. NumComponents returns the concept's Integer, which is
                // the native int only when the recipe erases.
                var countT = Writer.ScalarErase ? "int" : "Integer";
                TypeWriter.WriteLine($"{Attr} public {countT} NumComponents() => {nComps};");
                TypeWriter.WriteLine($"{Attr} public IReadOnlyList<{fieldType}> Components() => Intrinsics.MakeArray<{fieldType}>({localFieldNames.JoinStringsWithComma()});");
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

            // Kept members (interface obligations, operators, stubs) erase exactly as the concept
            // interfaces they satisfy do, so their signatures line up member-for-member.
            WriteImplementedInterfaceFunctions();

            WriteUnimplementedInterfaceFunctions();

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
        /// <summary>Whether <paramref name="target"/>'s own single-field converter block already
        /// emits "implicit operator {target}({SimpleName})" — the Number bridge written for a
        /// non-primitive struct whose one field is a Number. Mirrors the condition in the
        /// constructor; a second copy from the scalar re-home is CS0557 (plato-365: Angle stopped
        /// being a primitive, so its bridge started being generated).</summary>
        private bool SingleFieldConverterCovers(TypeDef target)
            => SimpleName == "Number"
               && target != null
               && !CSharpWriter.PrimitiveTypes.ContainsKey(target.Name)
               && target.Fields.Count == 1
               && target.Fields[0].Type?.Def?.Name == "Number";

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
            var implicitOps = new List<(string RetType, string MethodName, TypeDef Target)>();

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
                    // A partial struct is the only host for statics — but only BODIED ones: a
                    // bodiless static declaration (Number.MinValue in intrinsics.plato) names a
                    // handwritten member the wrapper already defines.
                    if (f.Implementation.Body != null)
                        shimMembers.Add(f);
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
                        implicitOps.Add((fi.ReturnType, fi.Name, f.ReturnType?.Def));
                        dropped.Add($"implicit operator {fi.ReturnType}({prim}) => re-homed as a partial-struct operator on {fi.ReturnType} + method {fi.Name}()");
                    }
                    tw.Write(fi.ExtensionSignature);
                    // Scalar extension-method bodies over the primitives emit from the ground
                    // monomorphized TIR — the sole body writer since C4.
                    var tir = Writer.TryGetGroundTir(f.Implementation, ConcreteType.TypeDef);
                    if (tir == null)
                    {
                        // Degrade gracefully (see CSharpTypeWriter.WriteBody): a scalar-erased
                        // extension body with no ground TIR emits a throwing stub + burn-down
                        // count rather than aborting all output. Empty under stdlib-legacy.
                        var member = $"{SimpleName}.{fi.Name}";
                        Writer.DegradedBodies.Add(member);
                        tw.WriteWithLineStateSync(
                            $" => throw new System.NotImplementedException(" +
                            $"\"plato: no ground TIR for scalar-erased {member} (not monomorphized)\");" +
                            System.Environment.NewLine);
                        AddBridge(fi);
                        continue;
                    }
                    Writer.TirBodiesEmitted++;
                    tir = Writer.RunOptimizerPasses(tir, fi, true, out var lowered);
                    tw.WriteWithLineStateSync(new TirCSharpBodyWriter(tw, tir, isStatic: true, fi, lowered: lowered).ToString());
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
                    bool IsScalar(int i)
                    {
                        var platoType = f.ParameterTypes[i]?.Name;
                        return platoType != null && CSharpWriter.ScalarPrimitives.ContainsKey(platoType);
                    }

                    // plato-308: scalar-on-the-LEFT times a compound (Multiply(Number, Number4)).
                    // The compound declares only `operator *(T, {prim})`, so the wrapper-cast form
                    // `((Number)scalar) * right` has no candidate operator at all. Scalar
                    // multiplication commutes for every type in this vocabulary, so emit the
                    // operand order the compound actually declares.
                    var commuteScalar = ps.Count == 2 && fi.OperatorName == "*" && IsScalar(0) && !IsScalar(1);

                    var impl = ps.Count == 1
                        ? $"{fi.OperatorName}{Operand(0)}"
                        : commuteScalar
                            ? $"{ps[1]} {fi.OperatorName} {ps[0]}"
                            : $"{Operand(0)} {fi.OperatorName} {Operand(1)}";
                    tw.WriteLine($"{fi.ExtensionSignature} => {impl};");
                    AddBridge(fi);
                }
                else
                {
                    // Intrinsic: forward into the handwritten wrapper member. Whether that member
                    // is spelled as a property is decided per member by IsStructSurfaceProperty
                    // inside the forwarder builder. No wrapper-receiver bridge: the wrapper
                    // already has the real member, and a same-name extension would be shadowed
                    // by it anyway.
                    tw.WriteLine(GetPrimitiveForwardingExtensionMethod(fi, SimpleName, prim));
                }

                tw.ExtensionReceiverName = null;
            }

            // Equality helper the struct scaffolding used to provide (call sites use it).
            tw.WriteLine($"{CSharpTypeWriter.Annotation} public static bool NotEquals(this {prim} a, {prim} b) => !a.Equals(b);");
            tw.WriteLine($"{CSharpTypeWriter.Annotation} public static bool NotEquals(this {SimpleName} a, {prim} b) => !(({prim})a).Equals(b);");

            // (The old hardwired Cubic/Linear/Quadratic/ReciprocalSquareRootEstimate float
            // forwarders are gone: those members are now DECLARED in stdlib-legacy/intrinsics.plato,
            // so the regular intrinsic-forwarder path generates them.)

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
                // Skip the re-home when the TARGET type's own writer already emits this exact
                // conversion: a single-{prim}-field non-primitive struct gets
                // "implicit operator T({SimpleName})" from its single-field converter block, and a
                // second copy here is CS0557 (measured on Angle, whose one field is a Number).
                if (SingleFieldConverterCovers(op.Target))
                    continue;

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

        // ============================================================================
        // Sum-type (tagged-union) emission (wave-2, plato-232). One readonly partial struct:
        //   - int Kind discriminant (0-based, declaration order) + per-case Kind_<Case> tag consts;
        //   - the flattened per-case fields (Case_Field), [DataMember] public readonly;
        //   - a PRIVATE all-fields constructor (named args, so the 10-field tuple cap is irrelevant);
        //   - one public static factory per case, setting its own fields and defaulting the rest;
        //   - a bool Is<Case> predicate per case (the match lowering's branch condition);
        //   - structural Equals / NotEquals / GetHashCode / ToString over Kind + all flattened fields.
        // The ternary-chain ToString avoids the switch-expression CS8509 the design doc calls out.
        // Case-field types erase under --scalar exactly as ordinary struct fields do.
        // ============================================================================
        public void WriteSumType(IReadOnlyList<Ara3D.Geometry.Compiler.Symbols.SumCaseField> flat)
        {
            var tw = TypeWriter;
            var cases = ConcreteType.TypeDef.Cases;
            var boolT = Writer.ScalarErase ? "bool" : "Boolean";

            var implements = ConcreteType.Interfaces.Count > 0
                ? ": " + ConcreteType.Interfaces.Select(TypeWriter.ToCSharpType).JoinStringsWithComma()
                : "";

            // Parameter names for the flattened fields (the private ctor's named args).
            var flatParamNames = FieldNames.Select(CSharpTypeWriter.FieldNameToParameterName).ToList();

            tw.WriteLine("[DataContract, StructLayout(LayoutKind.Sequential, Pack=1)]");
            // A generic sum is CHK306 today, so WhereClauses is empty here; it is written the same
            // way as the product path deliberately — nothing about bounds is record-specific, so
            // lifting CHK306 (plato-079) needs no change on this line.
            tw.Write($"public partial struct {Name}");
            tw.WriteLine(implements + WhereClauses);
            tw.WriteStartBlock();

            tw.WriteLine("// Discriminant (0-based, declaration order)");
            tw.WriteLine("[DataMember] public readonly int Kind;");
            tw.WriteLine();

            tw.WriteLine("// Case tags");
            foreach (var c in cases)
                tw.WriteLine($"public const int {c.TagConstName} = {c.Tag};");
            tw.WriteLine();

            if (flat.Count > 0)
            {
                tw.WriteLine("// Flattened per-case fields (Case_Field); inactive cases hold default.");
                for (var i = 0; i < flat.Count; ++i)
                    tw.WriteLine($"[DataMember] public readonly {FieldTypes[i]} {FieldNames[i]};");
                tw.WriteLine();
            }

            // Private all-fields constructor.
            tw.WriteLine("// All-fields constructor (private: build via the per-case factories)");
            var ctorParams = new List<string> { "int kind" };
            for (var i = 0; i < flat.Count; ++i)
                ctorParams.Add($"{FieldTypes[i]} {flatParamNames[i]}");
            var assigns = new List<string> { "Kind = kind;" };
            for (var i = 0; i < flat.Count; ++i)
                assigns.Add($"{FieldNames[i]} = {flatParamNames[i]};");
            tw.WriteLine($"{Attr} private {SimpleName}({ctorParams.JoinStringsWithComma()}) {{ {string.Join(" ", assigns)} }}");
            tw.WriteLine();

            // Per-case static factories.
            tw.WriteLine("// Per-case static factories: set own fields, default the rest.");
            foreach (var c in cases)
            {
                // Factory param names come from the case's own field names (unique within the case);
                // the private ctor above uses the flat names, which are unique across all cases.
                var caseParamNames = c.Fields.Select(f => CSharpTypeWriter.FieldNameToParameterName(f.Name)).ToList();
                var caseParams = c.Fields.Select((f, j) => $"{TypeWriter.ToCSharpType(f.Type)} {caseParamNames[j]}");
                // Argument for each FLAT field: this case's own field value, else default.
                var ctorArgs = new List<string> { c.TagConstName };
                foreach (var f in flat)
                {
                    var idx = c.Fields.ToList().FindIndex(cf => ReferenceEquals(cf, f));
                    ctorArgs.Add(idx >= 0 ? caseParamNames[idx] : "default");
                }
                tw.WriteLine($"{Attr} public static {Name} {c.Name}({caseParams.JoinStringsWithComma()}) => new {SimpleName}({ctorArgs.JoinStringsWithComma()});");
            }
            tw.WriteLine();

            tw.WriteLine("// Static default implementation");
            tw.WriteLine($"public static readonly {Name} Default = default;");
            tw.WriteLine();

            // Case predicates — the match lowering's branch conditions.
            tw.WriteLine("// Case predicates (match lowering's branch conditions)");
            foreach (var c in cases)
                tw.WriteLine($"{Attr} public {boolT} {c.PredicateName}() => Kind == {c.TagConstName};");
            tw.WriteLine();

            // Structural equality / hashing / ToString over Kind + all flattened fields.
            tw.WriteLine("// Object virtual function overrides: Equals, GetHashCode, ToString");
            var eqTerms = new List<string> { "Kind == other.Kind" };
            foreach (var fn in FieldNames)
                eqTerms.Add($"{fn}.Equals(other.{fn})");
            var eqBody = string.Join(" && ", eqTerms);
            tw.WriteLine($"{Attr} public {boolT} Equals({Name} other) => {eqBody};");
            tw.WriteLine($"{Attr} public {boolT} NotEquals({Name} other) => !({eqBody});");
            tw.WriteLine($"{Attr} public override bool Equals(object obj) => obj is {Name} other ? Equals(other) : false;");
            tw.WriteLine($"{Attr} public static {boolT} operator==({Name} a, {Name} b) => a.Equals(b);");
            tw.WriteLine($"{Attr} public static {boolT} operator!=({Name} a, {Name} b) => !a.Equals(b);");
            var hashArgs = new List<string> { "Kind" };
            hashArgs.AddRange(FieldNames);
            tw.WriteLine($"{Attr} public override int GetHashCode() => Intrinsics.CombineHashCodes({hashArgs.JoinStringsWithComma()});");
            tw.WriteLine($"{Attr} public override string ToString() => {SumToString(cases, flat)};");
            tw.WriteLine();

            // Shared tail: the library functions over this type (EndPoint, ...) and any interface
            // obligations, exactly as for a product type.
            WriteImplementedInterfaceFunctions();
            WriteUnimplementedInterfaceFunctions();

            tw.WriteEndBlock();

            WriteExtensionMethods();
        }

        // A tag-aware ToString rendered as a ternary chain (last case is the unconditional else,
        // matching the exhaustive match lowering and dodging the switch-expression CS8509).
        private static string SumToString(IReadOnlyList<Ara3D.Geometry.Compiler.Symbols.SumCaseDef> cases,
            IReadOnlyList<Ara3D.Geometry.Compiler.Symbols.SumCaseField> flat)
        {
            string Body(Ara3D.Geometry.Compiler.Symbols.SumCaseDef c)
            {
                if (c.Fields.Count == 0)
                    return $"\"{c.Name}\"";
                var parts = string.Join(", ", c.Fields.Select(f => $"{{{f.FlatName}}}"));
                return $"$\"{c.Name}({parts})\"";
            }
            if (cases.Count == 1)
                return Body(cases[0]);
            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < cases.Count - 1; ++i)
                sb.Append($"Kind == {cases[i].TagConstName} ? {Body(cases[i])} : ");
            sb.Append(Body(cases[cases.Count - 1]));
            return sb.ToString();
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
            // Plato overloads on return type; C# does not. Two declarations that reduce to the
            // same C# signature on this type (Array2D.Map -> Array2D<T2> and -> Array<T2>) would
            // be CS0111, so the first one written wins.
            var emitted = new HashSet<string>();
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
                // Type-variable NAMES are per-declaration (_T0 vs _T1), so normalize them
                // positionally before comparing - otherwise the two Maps look distinct.
                var key = fi.ParameterTypes.Skip(1).JoinStringsWithComma();
                for (var gi = 0; gi < fi.Generics.Count; gi++)
                    key = System.Text.RegularExpressions.Regex.Replace(
                        key, $@"{System.Text.RegularExpressions.Regex.Escape(fi.Generics[gi])}", $"#{gi}");
                if (!emitted.Add($"{fi.Name}`{fi.Generics.Count}({key})"))
                    continue;
                TypeWriter.WriteMemberFunction(fi, IsIntrinsicBacked, HasFunctionNamed);
            }
            TypeWriter.WriteLine();
        }

        // All function names visible on this concrete type (implemented, unimplemented, and
        // concrete-library); used to detect an unpaired comparison operator (CS0216).
        private HashSet<string> _allFunctionNames;
        private bool HasFunctionNamed(string name)
        {
            if (_allFunctionNames == null)
                _allFunctionNames = new HashSet<string>(
                    ConcreteType.ImplementedFunctions
                        .Concat(ConcreteType.UnimplementedFunctions)
                        .Concat(ConcreteType.ConcreteFunctions)
                        .Select(f => f.Name));
            return _allFunctionNames.Contains(name);
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
                        TypeWriter.WriteMemberFunction(TypeWriter.ToFunctionInfo(f, ConcreteType.TypeDef), IsIntrinsicBacked, HasFunctionNamed);
                    }
                }
            }
        }

        public string GetExtensionMethod(CSharpFunctionInfo fi)
        {
            var firstArg = fi.ParameterNames[0];
            // The forwarding TARGET is a method unless its name is pinned to property syntax
            // (MethodArgsString yields "()" exactly then).
            var isProp = fi.ParameterNames.Count <= 1 && !fi.EmitAsMethod;
            var args = isProp ? "" : "(" + fi.ParameterNames.Skip(1).JoinStringsWithComma() + ")";
            if (!isProp && fi.ParameterNames.Count <= 1)
                args = "()";
            return $"{fi.ExtensionSignature} => {firstArg}.{fi.Name}{args};";
        }

        public string GetPrimitiveForwardingExtensionMethod(CSharpFunctionInfo fi, string platoType, string primType)
        {
            var parameterTypes = fi.ParameterTypes.Skip(1).ToList().Prepend(primType);
            var parameters = fi.ParameterNames.Zip(parameterTypes, (n, t) => $"{t} {n}").JoinStringsWithComma();
            var sig = $"{CSharpFunctionInfo.Annotation}public static {fi.ReturnType} {fi.Name}{fi.ExtensionGenericsString}(this {parameters})";
            var args = fi.ParameterNames.Count <= 1 ? "" : "(" + fi.ParameterNames.Skip(1).JoinStringsWithComma() +")";
            // Extension style: forwarded no-arg members that moved out of the struct are classic
            // extension METHODS, and so is every other no-arg member whose name is not pinned to
            // property syntax on this receiver — both need parentheses at the forwarding site.
            if (args == "" && Writer.ExtensionStyle
                && (Writer.MovedNoArgNames.Contains(fi.Name)
                    || !Writer.IsStructSurfaceProperty(platoType, fi.Name)))
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
            {
                // A handwritten intrinsic: normally we emit a forwarder extension
                // TExtensions.Foo(this T) => self.Foo(). For the primitive types that are NOT one
                // of the five scalars (Angle, Vector*, Matrix*, Quaternion, Plane) the runtime
                // supplies its intrinsics as handwritten EXTENSION methods (TIntrinsics.Foo(this
                // T)); emitting the forwarder too would be a CS0121-ambiguous second Foo(this T)
                // extension (and self-recurse). Skip it — the handwritten extension (or, until a
                // type is ported, its instance method) provides x.Foo(). (M5 / consolidation plan
                // C3. The scalars keep the forwarder: under erasure it lands on the primitive
                // receiver float/int/bool, distinct from the wrapper, so there is no collision.)
                var nonErasedPrimitive = CSharpWriter.IsIntrinsicBacked(Name)
                    && !CSharpWriter.ScalarPrimitives.ContainsKey(Name);
                if (!nonErasedPrimitive)
                    tw.WriteLine(GetExtensionMethod(fi));
            }

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

            WriteSumCaseConstructorExtensions();

            foreach (var f in ConcreteType.UnimplementedFunctions)
                WriteExtensionMethod(f);

            foreach (var g in ConcreteType.InterfaceFunctionGroups)
            {
                var f = Analyzer.ChooseBestFunction(g, out int _);
                WriteExtensionMethod(f);
            }
            
            
            
            tw.WriteEndBlock();
        }

        /// <summary>
        /// Receiver-style twins of the per-case factories on a sum type: `Line(seg)` in Plato
        /// lowers the same way every other one-argument call does — as `seg.Line()` — but the
        /// factory is a STATIC on the sum struct, so that call site has nothing to bind to
        /// (measured: `BrepCurve.Line`, `BrepSurface.Bilinear`). Emitting the twin here keeps the
        /// lowering uniform instead of teaching the body writer about case constructors.
        ///
        /// A field-less case gets no twin: there is no receiver to hang it on, and its call sites
        /// name the factory directly.
        /// </summary>
        private void WriteSumCaseConstructorExtensions()
        {
            if (!ConcreteType.TypeDef.IsSum)
                return;

            foreach (var c in ConcreteType.TypeDef.Cases)
            {
                if (c.Fields.Count == 0)
                    continue;
                var names = c.Fields.Select(f => CSharpTypeWriter.FieldNameToParameterName(f.Name)).ToList();
                var types = c.Fields.Select(f => TypeWriter.ToCSharpType(f.Type)).ToList();
                var ps = types.Zip(names, (t, n) => $"{t} {n}").ToList();
                ps[0] = "this " + ps[0];
                TypeWriter.WriteLine(
                    $"{Attr} public static {SimpleName} {c.Name}({ps.JoinStringsWithComma()})" +
                    $" => {SimpleName}.{c.Name}({names.JoinStringsWithComma()});");
            }
        }

        /// <summary>
        /// plato-308: the forward stdlib names its numeric tuples Number2/3/4/8 and its geometric
        /// vectors Vector2D/3D, but the handwritten Plato.Intrinsics surface those declarations
        /// bind to (Matrix4x4 rows, Vector4.Transform, Quaternion.Multiply, ...) traffics in
        /// Vector2/3/4/8. The two are the same bits with different names, so the generated struct
        /// declares the conversion pair. It cannot live on the intrinsic side: Plato.Intrinsics
        /// is a SHARED project and the legacy consumers do not generate Number4/Vector3D at all.
        /// </summary>
        public static Dictionary<string, string> IntrinsicVectorBridges = new Dictionary<string, string>
        {
            { "Number2", "Vector2" },
            { "Number3", "Vector3" },
            { "Number4", "Vector4" },
            { "Number8", "Vector8" },
            { "Vector2D", "Vector2" },
            { "Vector3D", "Vector3" },
            { "Point2D", "Vector2" },
            { "Point3D", "Vector3" },
            { "Direction2D", "Vector2" },
            { "Direction3D", "Vector3" },
        };

        /// <summary>
        /// Emits the implicit conversion pair between this type and its intrinsic twin (see
        /// <see cref="IntrinsicVectorBridges"/>). Component-wise when the fields are the erased
        /// scalars themselves; forwarded through the single field when the type merely wraps an
        /// already-bridged type (Direction3D wraps Vector3D).
        /// </summary>
        public void WriteIntrinsicVectorBridge()
        {
            if (IsPrimitive || !Writer.ScalarErase)
                return;
            if (!IntrinsicVectorBridges.TryGetValue(SimpleName, out var intrinsic))
                return;
            if (!PrimitiveFieldNames.TryGetValue(intrinsic, out var comps))
                return;

            var tw = TypeWriter;
            var floatType = Writer.FloatType;
            tw.WriteLine($"// Intrinsic bridge: {SimpleName} and {intrinsic} are the same components under different names.");

            if (FieldTypes.Count == comps.Length && FieldTypes.All(t => t == floatType))
            {
                var toIntrinsic = FieldNames.Select(f => $"self.{f}").JoinStringsWithComma();
                var fromIntrinsic = comps.Select(c => $"value.{c}").JoinStringsWithComma();
                tw.WriteLine($"{Attr} public static implicit operator {intrinsic}({Name} self) => new {intrinsic}({toIntrinsic});");
                tw.WriteLine($"{Attr} public static implicit operator {Name}({intrinsic} value) => new {Name}({fromIntrinsic});");
            }
            else if (FieldTypes.Count == 1 && IntrinsicVectorBridges.TryGetValue(FieldTypes[0], out var inner) && inner == intrinsic)
            {
                tw.WriteLine($"{Attr} public static implicit operator {intrinsic}({Name} self) => ({intrinsic})self.{FieldNames[0]};");
                tw.WriteLine($"{Attr} public static implicit operator {Name}({intrinsic} value) => new {Name}(({FieldTypes[0]})value);");
            }

            tw.WriteLine();
        }

        /// <summary>
        /// The component names of a HANDWRITTEN struct the writer never generates: the far side of
        /// <see cref="IntrinsicVectorBridges"/> (plus Number's payload). Read by the bridge writer
        /// and, for the remaining primitives, as pseudo-fields in the extension-style plan.
        ///
        /// The matrix entries used to live here too — sixteen M11..M44 names, the writer's private
        /// picture of System.Numerics' element naming. plato-365 deleted them: a matrix now
        /// generates from `stdlib/foundation/matrices.types.plato` (Row1..Row4 of Number4) and the
        /// M-names exist only inside Plato.Intrinsics, where the System.Numerics round-trip is
        /// written out row by row. Do not re-add them — that is the invisible-primitiveness
        /// mechanism the issue exists to delete.
        /// </summary>
        public static Dictionary<string, string[]> PrimitiveFieldNames = new Dictionary<string, string[]>
        {
            { "Number", ["Value"] },
            { "Vector2", ["X", "Y"] },
            { "Vector3", ["X", "Y", "Z"] },
            { "Vector4", ["X", "Y", "Z", "W"] },
            { "Vector8", [
                "X0", "X1", "X2", "X3",
                "X4", "X5", "X6", "X7"] },
        };
    }
}