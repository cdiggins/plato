using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter;

// ============================================================================================
// "Extension style" C# emitter mode (docs/plato-roadmap.md Phase 2 + Phase 2 revision,
// --csharp-style=extensions).
//
// In the default mode every library function applicable to a concrete type is emitted as an
// instance member of the generated partial struct (~hundreds of members per type). In extension
// style, the library-function fanout is moved OUT of the partial struct into CLASSIC extension
// methods (`public static R Name(this T recv, ...)`), organized as one plain static class per
// Plato `library` (one file per library, e.g. Vectors.g.cs), so the generated C# mirrors
// plato-src 1:1. Per the Phase 2 revision, C# 14 `extension` blocks / extension properties are
// NOT used: the output compiles with the default LangVersion on net8.0, and no-arg moved
// functions become extension METHODS (`v.Length()`). Consequently the emitter's own expression
// writer must add `()` at every call site of a moved no-arg member; the global name partition
// that drives this lives in CSharpWriter.BuildExtensionPlans (a no-arg name is only moved if it
// is not a property anywhere - conflicted names are demoted back into their structs, so a name
// is uniformly a method or uniformly a property across the whole generated output).
//
// What MUST stay in the partial struct (extension methods cannot satisfy C# interfaces, and
// operators/conversions/statics are simpler in place):
//   - fields, constructors, With*, Create, Default, tuple conversions, Equals/GetHashCode/etc.
//     (all of the fixed scaffolding written by CSharpConcreteTypeWriter: unchanged);
//   - every member whose signature matches a function *declared* by any implemented interface
//     (the C# interface obligation);
//   - all unimplemented (declared-only) functions - they are obligations by definition;
//   - operators, implicit conversions, indexers (At), static functions (so no static member
//     ever moves: "constructor-like" statics remain callable as Type.Name(...));
//   - intrinsic functions (Body == null): for primitive types these are handwritten in
//     Plato.Intrinsics; for other types they are NotImplementedException stubs;
//   - all members of generic concrete types (kept whole for simplicity/safety);
//   - Law_* functions: the Phase-1 conformance harness (conformance\Ara3D.SDK.ConformanceTests\
//     LawTests.cs) discovers laws by reflection over *instance* members of the generated
//     structs. Moving them would silently remove test cases, breaking the differential gate;
//   - any function sharing a NAME with a kept member or field of the same type: C# instance
//     members hide same-name extension methods (no warning), so a name is moved either
//     completely or not at all ("when in doubt, keep it in the struct");
//   - any no-arg function whose name is a kept no-arg PROPERTY on any other generated type or
//     is declared no-arg by any interface (global demotion, see CSharpWriter.BuildExtensionPlans):
//     the writer decides call-site syntax (`x.N` vs `x.N()`) purely by name, so a name may not
//     be a property on one receiver type and a method on another.
// ============================================================================================

/// <summary>A library function that extension style moves out of a concrete type's partial struct.</summary>
public class MovedExtensionMember
{
    public MovedExtensionMember(FunctionInstance function, ConcreteType concreteType, string libraryName, ExtensionStylePlan plan)
    {
        Function = function;
        ConcreteType = concreteType;
        LibraryName = libraryName;
        Plan = plan;
    }

    public FunctionInstance Function { get; }
    public ConcreteType ConcreteType { get; }
    public string LibraryName { get; }
    public ExtensionStylePlan Plan { get; }
    public string ReceiverName => Function.ParameterNames[0];
}

/// <summary>
/// Per-concrete-type partition of member functions into "stays in the struct" vs
/// "moves to a library extension class". Computed for ALL types before any member is
/// written (CSharpWriter.BuildExtensionPlans), because call-site syntax for no-arg
/// members is decided globally by name.
/// </summary>
public class ExtensionStylePlan
{
    private readonly HashSet<FunctionInstance> _moved = new HashSet<FunctionInstance>();

    // All member names of the type as they exist in the default (V1) output, split by kind.
    // Used when writing moved bodies: a bare name that bound implicitly inside the partial
    // struct must be re-qualified (receiver parameter for instance members, type name for
    // static members) because a static library class does not import the receiver type's scope.
    public HashSet<string> InstanceNames { get; } = new HashSet<string>();
    public HashSet<string> StaticNames { get; } = new HashSet<string>();

    // Names of instance members that remain in the struct as no-arg PROPERTIES (fields,
    // primitive pseudo-fields, kept no-arg functions, no-arg stubs). Feeds the global
    // moved/kept partition: a name emitted with `()` at call sites must not be a property
    // on any generated type.
    public HashSet<string> KeptNoArgPropertyNames { get; } = new HashSet<string>();

    public IEnumerable<FunctionInstance> MovedFunctions => _moved;

    public ExtensionStylePlan(CSharpWriter writer, ConcreteType ct)
    {
        var typeDef = ct.TypeDef;
        var typeWriter = new CSharpTypeWriter(writer, typeDef);
        var fieldNames = typeDef.Fields.Select(f => f.Name).ToList();
        var typeParamsStr = typeDef.TypeParameters.Count > 0
            ? "<" + typeDef.TypeParameters.Select(tp => tp.Name).JoinStringsWithComma() + ">"
            : "";
        var name = ct.Name + typeParamsStr;
        var isPrimitive = CSharpWriter.PrimitiveTypes.ContainsKey(name);

        // Generic concrete types keep all their members (mirrors the V1 decision to not
        // generate per-type extension classes for generic types).
        var isGeneric = typeDef.TypeParameters.Count > 0;

        var declaredSigs = new HashSet<string>(ct.DeclaredFunctions.Select(f => f.SignatureId));

        // The same candidate set (and choice) as CSharpConcreteTypeWriter.WriteImplementedInterfaceFunctions.
        var candidates = new List<(FunctionInstance F, CSharpFunctionInfo Fi)>();
        foreach (var g in ct.InterfaceFunctionGroups)
        {
            var f = writer.Analyzer.ChooseBestFunction(g, out _);
            if (CSharpConcreteTypeWriter.SkipFunction(f, fieldNames, ct.Name))
                continue;
            candidates.Add((f, typeWriter.ToFunctionInfo(f, typeDef)));
        }

        // Primitive pseudo-fields (Vector3.X, Matrix4x4.M11, ...): the struct's own IArrayLike
        // scaffolding (the Components getter) references these names unqualified, so members
        // named after them must remain instance members.
        string[] pseudoFields = null;
        if (isPrimitive)
            CSharpConcreteTypeWriter.PrimitiveFieldNames.TryGetValue(name, out pseudoFields);

        // Names that force same-name functions to stay (shadowing safety, see header comment).
        var keepNames = new HashSet<string>(fieldNames);
        if (pseudoFields != null)
            keepNames.UnionWith(pseudoFields);
        foreach (var f in ct.UnimplementedFunctions)
            keepNames.Add(f.Name);

        // No-arg properties that unconditionally remain in the struct.
        KeptNoArgPropertyNames.UnionWith(fieldNames);
        if (pseudoFields != null)
            KeptNoArgPropertyNames.UnionWith(pseudoFields);
        foreach (var f in ct.UnimplementedFunctions)
            if (f.ParameterNames.Count == 1)
                KeptNoArgPropertyNames.Add(f.Name);

        // Record the V1 member-name universe for bare-name re-qualification in moved bodies.
        InstanceNames.UnionWith(keepNames);
        foreach (var (f, fi) in candidates)
            (fi.IsStatic ? StaticNames : InstanceNames).Add(f.Name);

        var movable = new List<FunctionInstance>();
        foreach (var (f, fi) in candidates)
        {
            var keep =
                isGeneric                                           // generic types keep everything
                || declaredSigs.Contains(f.SignatureId)             // C# interface obligation
                || fi.IsOperator                                    // emits operator overloads
                || fi.IsImplicit                                    // emits implicit conversions
                || fi.IsIndexer                                     // emits this[] indexer
                || fi.IsStatic                                      // static members stay in place
                || f.Implementation.Body == null                    // intrinsic (handwritten or stub)
                || f.Implementation.OwnerType == null               // unknown provenance: play safe
                || !f.Implementation.OwnerType.IsLibrary()          // not a library function
                || f.Name.StartsWith("Law_");                       // conformance-harness reflection contract

            if (keep)
            {
                keepNames.Add(f.Name);
                if (!fi.IsStatic && f.ParameterNames.Count == 1)
                    KeptNoArgPropertyNames.Add(f.Name);
            }
            else
                movable.Add(f);
        }

        foreach (var f in movable)
        {
            if (!keepNames.Contains(f.Name))
                _moved.Add(f);
            else if (f.ParameterNames.Count == 1)
                KeptNoArgPropertyNames.Add(f.Name); // stays in the struct as a property (name-shadow keep)
        }
    }

    public bool ShouldMove(FunctionInstance f)
        => _moved.Contains(f);

    /// <summary>
    /// Global conflict resolution (CSharpWriter.BuildExtensionPlans): a name that is a no-arg
    /// property anywhere may not be a moved method anywhere, so ALL functions with a conflicted
    /// name return to the struct (all arities: a kept property would hide same-name extension
    /// methods of any arity).
    /// </summary>
    public void DemoteMovedNames(ICollection<string> names)
    {
        var demoted = _moved.Where(f => names.Contains(f.Name)).ToList();
        foreach (var f in demoted)
        {
            _moved.Remove(f);
            if (f.ParameterNames.Count == 1)
                KeptNoArgPropertyNames.Add(f.Name);
        }
    }
}

/// <summary>Writes the per-library extension-method files ({Library}.g.cs) from the moved members.</summary>
public static class ExtensionStyleWriter
{
    public static void WriteLibraryFiles(CSharpWriter writer)
    {
        // Names already claimed in the generated namespace: a static library class may not
        // collide with a generated type. (Generic types/interfaces differ by arity and would
        // not collide, but renaming on any name match is harmless and simpler.)
        var takenNames = new HashSet<string> { "Constants", "Extensions", "Intrinsics" };
        foreach (var t in writer.Compilation.AllTypeAndLibraryDefinitions)
            if (t != null && (t.IsConcrete() || t.IsInterface()))
                takenNames.Add(t.Name);

        foreach (var libGroup in writer.MovedMembers.GroupBy(m => m.LibraryName))
        {
            var className = libGroup.Key;
            while (takenNames.Contains(className))
                className += "Library";
            takenNames.Add(className);

            var members = libGroup.ToList();
            writer.WriteFile($"{className}.g.cs", () => WriteLibraryClass(writer, className, members));
        }
    }

    private static CSharpWriter WriteLibraryClass(CSharpWriter writer, string className, List<MovedExtensionMember> members)
    {
        writer.WriteLine($"public static class {className}");
        writer.WriteStartBlock();

        // Classic extension methods: `public static R Name(this T recv, ...)`. Function bodies
        // are written in "static" mode and refer to the first Plato parameter by name, which is
        // exactly the `this` parameter's name, so no rewriting of the body's receiver is needed.
        var emittedSignatures = new HashSet<string>();
        foreach (var m in members)
        {
            var tw = NewMemberWriter(writer, m);
            var fi = tw.ToFunctionInfo(m.Function, m.ConcreteType.TypeDef);

            // Defensive: never emit two extension methods with the same C# signature.
            var sigKey = $"{fi.Name}{fi.GenericsString}({fi.ParameterTypes.JoinStringsWithComma()})";
            if (!emittedSignatures.Add(sigKey))
                continue;

            // ExtensionSignature: [MethodImpl(AggressiveInlining)] public static R Name<..>(this T recv, ...) where ...
            // No-arg moved functions are deliberately METHODS (v.Length()), never properties;
            // the body writer is told so via propOverride: false.
            tw.Write(fi.ExtensionSignature);
            var body = new CSharpFunctionBodyWriter(tw, fi, true, false, false);
            tw.Write(body.ToString());
            writer.WriteWithLineStateSync(tw.ToString());
        }

        return writer.WriteEndBlock();
    }

    private static CSharpTypeWriter NewMemberWriter(CSharpWriter writer, MovedExtensionMember m)
        => new CSharpTypeWriter(writer, m.ConcreteType.TypeDef)
        {
            // Inside the static library class the receiver type's member scope is not implicit:
            // bare member references must be re-qualified (see CSharpTypeWriter fields).
            ExtensionReceiverName = m.ReceiverName,
            ExtensionStaticQualifier = $"{writer.Namespace}.{m.ConcreteType.TypeDef.Name}",
            ExtensionInstanceNames = m.Plan.InstanceNames,
            ExtensionStaticNames = m.Plan.StaticNames
        };
}
