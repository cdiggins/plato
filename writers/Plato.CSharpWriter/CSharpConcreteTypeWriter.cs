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
    // The JSON / IFormattable / IParsable surface lives in the other half of this partial class
    // (CSharpSerializationWriter.cs).
    public partial class CSharpConcreteTypeWriter
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

        /// <summary>The handwritten runtime supplies this type's bodiless members and operators â€”
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

            // Sum type (wave-2, plato-232): a tagged struct â€” one `int Kind` discriminant plus the
            // flattened per-case fields â€” emitted by a dedicated path (design doc Â§5). The flattened
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

            FieldTypes = ConcreteType.TypeDef.Fields.Select(f => TypeWriter.ToCSharpType(f.Type)).ToList();
            FieldNames = ConcreteType.TypeDef.Fields.Select(f => f.Name).ToList();

            var implements = BaseListString();
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

            WriteTypeHeader(implements);

            if (!IsPrimitive)
            {
                TypeWriter.WriteLine("// Fields");
                for (var i = 0; i < FieldTypes.Count; ++i)
                {
                    TypeWriter.WriteDoc(ConcreteType.TypeDef.Fields[i].Doc);
                    TypeWriter.WriteLine($"{DataMemberAttribute(i, FieldNames[i])} public readonly {FieldTypes[i]} {FieldNames[i]};");
                }
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
                    TypeWriter.WriteLine(
                        $"{Attr}{JsonConstructorAttribute} public {SimpleName}({parametersStr}) {{ {assignments}}}");
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
                // IsInterface check covers interface-typed fields; the IReadOnlyList check covers
                // the concrete Array/Array2D/Array3D types, which render as list interfaces.
                //
                // plato-308: a PRIMITIVE-backed type (a handwritten Plato.Intrinsics struct, e.g.
                // Angle) declares its own field-type and Number conversions, so generating those
                // here duplicates them (CS0557). Skip exactly that pair for primitives â€” the
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
                TypeWriter.WriteLine();
            }

            TypeWriter.WriteLine("// Object virtual function overrides: Equals, GetHashCode, ToString");
            const string boolT = "Boolean";
            const string boolVal = ".Value";
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
            }
            // A primitive whose handwritten counterpart is a WRAPPER (the five scalars) carries a
            // `Value` payload the scaffolding reads. `Type` and the Function arities do not: they
            // map straight onto System.Type / System.Func and no Plato.Intrinsics partial declares
            // a Value, so the same scaffolding would name a member that does not exist (measured:
            // `Value` binding to the concept of that name instead, CS0305 + CS1061). They get the
            // field-less shape â€” every value of such a type is interchangeable here.
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
            }
            TypeWriter.WriteLine();

            WriteSerializationSurface();

            // TODO: this might be a problem for primitives. 

            TypeWriter.WriteLine("// Explicit implementation of interfaces by forwarding properties to fields");
            // The interfaces erase with the recipe and declare METHODS, so the explicit forwarders
            // match them member-for-member.
            var emittedExplicitImpls = new HashSet<string>();
            foreach (var i in t.AllInterfaces)
            {
                var its = TypeWriter.ToCSharpType(i);
                // plato-311: a property-form (or pinned-name) member satisfies the generic
                // interface only through its explicit implementation, so the interface's non-generic
                // existential view â€” reached transitively via `C<Self> : C` â€” needs its own
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
                    // A one-argument obligation NAMED FOR THIS TYPE is the identity conversion —
                    // IRigid3D declares `Pose3D()`, and Pose3D itself has to answer it. SkipFunction
                    // deliberately keeps it off the public surface (a public `Pose3D Pose3D()` on
                    // Pose3D is a cast to self), but the interface still needs discharging, so it
                    // lands here as an explicit implementation returning `this`.
                    if (f.ParameterTypes.Count == 1 && f.Name == SimpleName)
                    {
                        if (emittedExplicitImpls.Add($"{its}.{f.Name}"))
                            TypeWriter.WriteLine($"{Attr} {Name} {its}.{f.Name}() => this;");
                        if (viewTarget != null && emittedExplicitImpls.Add($"{viewTarget}.{f.Name}"))
                            TypeWriter.WriteLine($"{Attr} {Name} {viewTarget}.{f.Name}() => this;");
                    }
                    else if (f.ParameterTypes.Count == 1 && fieldIndex >= 0)
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
                        // primitive types, generated elsewhere) â€” plain member access.
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

                // The IArray<T> interface erases with the recipe, so the IReadOnlyList<T> element
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
                // These satisfy the IArrayLike<Self, T> obligation directly. NumComponents returns
                // the interface's Integer wrapper.
                TypeWriter.WriteLine($"{Attr} public Integer NumComponents() => {nComps};");
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

            // Kept members (interface obligations, operators, stubs) erase exactly as the interface
            // interfaces they satisfy do, so their signatures line up member-for-member.
            WriteImplementedInterfaceFunctions();

            WriteUnimplementedInterfaceFunctions();

            TypeWriter.WriteEndBlock();

            WriteExtensionMethods();
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
        // ============================================================================
        public void WriteSumType(IReadOnlyList<Ara3D.Geometry.Compiler.Symbols.SumCaseField> flat)
        {
            var tw = TypeWriter;
            var cases = ConcreteType.TypeDef.Cases;
            const string boolT = "Boolean";

            var implements = BaseListString();

            // Parameter names for the flattened fields (the private ctor's named args).
            var flatParamNames = FieldNames.Select(CSharpTypeWriter.FieldNameToParameterName).ToList();

            // A generic sum is CHK306 today, so WhereClauses is empty here; it is written the same
            // way as the product path deliberately â€” nothing about bounds is record-specific, so
            // lifting CHK306 (plato-079) needs no change on this line.
            WriteTypeHeader(implements);

            tw.WriteLine("// Discriminant (0-based, declaration order)");
            tw.WriteLine(DataMemberAttribute(0, "Kind") + " public readonly int Kind;");
            tw.WriteLine();

            tw.WriteLine("// Case tags");
            foreach (var c in cases)
                tw.WriteLine($"public const int {c.TagConstName} = {c.Tag};");
            tw.WriteLine();

            if (flat.Count > 0)
            {
                tw.WriteLine("// Flattened per-case fields (Case_Field); inactive cases hold default.");
                for (var i = 0; i < flat.Count; ++i)
                    tw.WriteLine($"{DataMemberAttribute(i + 1, FieldNames[i])} public readonly {FieldTypes[i]} {FieldNames[i]};");
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
            tw.WriteLine($"{Attr}{JsonConstructorAttribute} private {SimpleName}({ctorParams.JoinStringsWithComma()}) {{ {string.Join(" ", assigns)} }}");
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

            // Case predicates â€” the match lowering's branch conditions.
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
            tw.WriteLine();

            // The sum serializes as its honest layout — the Kind discriminant plus every flattened
            // field — which is exactly what DataContract writes, so the two round-trip alike. The
            // old tag-aware ternary chain rendered `Move(1, 2)`, which no parser reads back.
            WriteSerializationSurface();

            // Shared tail: the library functions over this type (EndPoint, ...) and any interface
            // obligations, exactly as for a product type.
            WriteImplementedInterfaceFunctions();
            WriteUnimplementedInterfaceFunctions();

            tw.WriteEndBlock();

            WriteExtensionMethods();
        }

        /// <summary>The struct's base list: the interface interfaces it implements, plus the four
        /// System interfaces the serialization surface discharges.</summary>
        private string BaseListString()
        {
            var bases = ConcreteType.Interfaces.Select(TypeWriter.ToCSharpType).ToList();
            if (HasSerializationSurface)
                bases.Add(SerializationInterfaces);
            return bases.Count > 0 ? ": " + bases.JoinStringsWithComma() : "";
        }

        /// <summary>The doc comment, attributes and declaration line, shared by the product and sum
        /// paths.
        ///
        /// `readonly` is on every generated struct: the fields already are, and the modifier is
        /// what stops the compiler from defensively copying the struct at every readonly-context
        /// call site. The handwritten halves of the five scalar wrappers carry it too - a partial
        /// struct's declarations must agree.</summary>
        private void WriteTypeHeader(string implements)
        {
            TypeWriter.WriteDoc(ConcreteType.TypeDef.Doc);
            // A primitive's [DataContract] is on the handwritten half of the partial.
            //
            // No `Pack`. Sequential layout at the default packing is already deterministic - the
            // C rule (each field at the next multiple of its natural alignment, size rounded up to
            // the struct's alignment) is what every C and C++ compiler applies, so it is the
            // packing that agrees with the other side of an interop boundary. `Pack=1` bought no
            // determinism we did not already have and cost alignment: no aligned SIMD loads,
            // `Vector128`/`Vector256` unusable over the fields, and faulting unaligned access on
            // some ARM64 paths. See docs/csharp-type-generation-design.md.
            TypeWriter.WriteLine(IsPrimitive
                ? "[StructLayout(LayoutKind.Sequential)]"
                : "[DataContract, StructLayout(LayoutKind.Sequential)]");
            TypeWriter.WriteLine(CSharpWriter.GeneratedCodeAttributes);
            TypeWriter.Write($"public readonly partial struct {Name}");
            TypeWriter.WriteLine(implements + WhereClauses);
            TypeWriter.WriteStartBlock();
        }

        /// <summary>
        /// The three attributes that make a field's wire representation a property of the TYPE
        /// rather than of whoever happens to be serializing it:
        ///
        ///   `[DataMember(Order = n)]` — DataContractSerializer orders members ALPHABETICALLY
        ///     without it, so renaming or reordering a Plato field silently changes the wire format
        ///     of every already-serialized document.
        ///   `[JsonInclude]` — what makes System.Text.Json see the member at all: it ignores fields
        ///     unless the field opts in or the serializer is configured with IncludeFields.
        ///   `[JsonPropertyName]` — pins the JSON name against the caller's
        ///     `JsonSerializerOptions.PropertyNamingPolicy`, which otherwise rewrites it. Without
        ///     it, `JsonSerializer.Serialize(p, camelCase)` wrote `{"x":...}` while the emitted
        ///     ToJson wrote `{"X":...}` — two spellings of the same value, from the same type, in
        ///     one process. The attribute wins over the policy, so the two agree again.
        /// </summary>
        private static string DataMemberAttribute(int order, string name)
            => $"[DataMember(Order = {order}), JsonInclude, JsonPropertyName(\"{name}\")]";

        /// <summary>`[JsonConstructor]` — readonly fields cannot be assigned by the deserializer, so
        /// System.Text.Json needs the parameterized constructor to build the value; a struct
        /// otherwise defaults to its parameterless constructor and yields all-zero results. It is
        /// also how the generated Parse/TryParse work at all, since those hand the input to
        /// System.Text.Json.
        ///
        /// Unconditional: <see cref="CSharpTypeWriter.FieldNameToParameterName"/> guarantees every
        /// parameter name matches its field case-insensitively, which is what the binding needs.
        /// A private constructor is fine — the attribute is honoured on non-public constructors,
        /// which is what lets a sum type (whose all-fields constructor is deliberately private)
        /// round-trip.</summary>
        public const string JsonConstructorAttribute = " [JsonConstructor]";

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
                // linter â€” generated code is not the reporting channel.

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
            // property syntax on this receiver â€” both need parentheses at the forwarding site.
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
                // extension (and self-recurse). Skip it â€” the handwritten extension (or, until a
                // type is ported, its instance method) provides x.Foo(). (M5 / consolidation plan
                // C3. The five scalar wrappers keep the forwarder: their handwritten members are
                // instance methods on the wrapper struct, which wins overload resolution over an
                // extension, so there is no CS0121.)
                var intrinsicBackedNonScalar = CSharpWriter.IsIntrinsicBacked(Name)
                    && !CSharpWriter.ScalarPrimitives.ContainsKey(Name);
                if (!intrinsicBackedNonScalar)
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
        /// lowers the same way every other one-argument call does â€” as `seg.Line()` â€” but the
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
        /// The component names of a HANDWRITTEN struct the writer never generates: the far side of
        /// <see cref="IntrinsicVectorBridges"/> (plus Number's payload). Read by the bridge writer
        /// and, for the remaining primitives, as pseudo-fields in the extension-style plan.
        ///
        /// The matrix entries used to live here too â€” sixteen M11..M44 names, the writer's private
        /// picture of System.Numerics' element naming. plato-365 deleted them: a matrix now
        /// generates from `stdlib/foundation/matrices.types.plato` (Row1..Row4 of Number4) and the
        /// M-names exist only inside Plato.Intrinsics, where the System.Numerics round-trip is
        /// written out row by row. Do not re-add them â€” that is the invisible-primitiveness
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
