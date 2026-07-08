using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter;

// ============================================================================================
// "Extension style" C# emitter mode (docs/plato-roadmap.md Phase 2.2, --csharp-style=extensions).
//
// In the default mode every library function applicable to a concrete type is emitted as an
// instance member of the generated partial struct (~hundreds of members per type). In extension
// style, the library-function fanout is moved OUT of the partial struct into C# 14 `extension`
// blocks, organized as one static class per Plato `library` (one file per library, e.g.
// Vectors.g.cs), so the generated C# mirrors plato-src 1:1. Call syntax is preserved:
// no-arg functions become extension *properties* (v.Length), others extension methods.
//
// What MUST stay in the partial struct (extension members cannot satisfy C# interfaces, and
// operators/conversions/statics are simpler in place):
//   - fields, constructors, With*, Create, Default, tuple conversions, Equals/GetHashCode/etc.
//     (all of the fixed scaffolding written by CSharpConcreteTypeWriter: unchanged);
//   - every member whose signature matches a function *declared* by any implemented interface
//     (the C# interface obligation);
//   - all unimplemented (declared-only) functions - they are obligations by definition;
//   - operators, implicit conversions, indexers (At), static functions;
//   - intrinsic functions (Body == null): for primitive types these are handwritten in
//     Plato.Intrinsics; for other types they are NotImplementedException stubs;
//   - all members of generic concrete types (kept whole for simplicity/safety);
//   - Law_* functions: the Phase-1 conformance harness (Ara3D.SDK.ConformanceTests\LawTests.cs)
//     discovers laws by reflection over *instance* members of the generated structs. Moving them
//     would silently remove test cases, breaking the differential gate (roadmap P2.4);
//   - any function sharing a NAME with a kept member or field of the same type: C# 14 instance
//     members silently shadow same-name extension members (no warning), so a name is moved
//     either completely or not at all ("when in doubt, keep it in the struct").
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
/// "moves to a library extension block". Computed before any member is written.
/// </summary>
public class ExtensionStylePlan
{
    private readonly HashSet<FunctionInstance> _moved = new HashSet<FunctionInstance>();

    // All member names of the type as they exist in the default (V1) output, split by kind.
    // Used when writing moved bodies: a bare name that bound implicitly inside the partial
    // struct must be re-qualified (receiver parameter for instance members, type name for
    // static members) because extension blocks do not import the receiver type's scope.
    public HashSet<string> InstanceNames { get; } = new HashSet<string>();
    public HashSet<string> StaticNames { get; } = new HashSet<string>();

    public ExtensionStylePlan(CSharpConcreteTypeWriter ctw)
    {
        var ct = ctw.ConcreteType;

        // Generic concrete types keep all their members (mirrors the V1 decision to not
        // generate per-type extension classes for generic types).
        if (ct.TypeDef.TypeParameters.Count > 0)
            return;

        var declaredSigs = new HashSet<string>(ct.DeclaredFunctions.Select(f => f.SignatureId));

        // The same candidate set (and choice) as CSharpConcreteTypeWriter.WriteImplementedInterfaceFunctions.
        var candidates = new List<(FunctionInstance F, CSharpFunctionInfo Fi)>();
        foreach (var g in ct.InterfaceFunctionGroups)
        {
            var f = ctw.Analyzer.ChooseBestFunction(g, out _);
            if (ctw.SkipFunction(f))
                continue;
            candidates.Add((f, ctw.TypeWriter.ToFunctionInfo(f, ct.TypeDef)));
        }

        // Primitive pseudo-fields (Vector3.X, Matrix4x4.M11, ...): the struct's own IArrayLike
        // scaffolding (the Components getter) references these names unqualified, so members
        // named after them must remain instance members.
        string[] pseudoFields = null;
        if (ctw.IsPrimitive)
            CSharpConcreteTypeWriter.PrimitiveFieldNames.TryGetValue(ctw.Name, out pseudoFields);

        // Names that force same-name functions to stay (shadowing safety, see header comment).
        var keepNames = new HashSet<string>(ctw.FieldNames);
        if (pseudoFields != null)
            keepNames.UnionWith(pseudoFields);
        foreach (var f in ct.UnimplementedFunctions)
            keepNames.Add(f.Name);

        // Record the V1 member-name universe for bare-name re-qualification in moved bodies.
        InstanceNames.UnionWith(keepNames);
        foreach (var (f, fi) in candidates)
            (fi.IsStatic ? StaticNames : InstanceNames).Add(f.Name);

        var movable = new List<(FunctionInstance F, CSharpFunctionInfo Fi)>();
        foreach (var (f, fi) in candidates)
        {
            var keep =
                declaredSigs.Contains(f.SignatureId)                // C# interface obligation
                || fi.IsOperator                                    // emits operator overloads
                || fi.IsImplicit                                    // emits implicit conversions
                || fi.IsIndexer                                     // emits this[] indexer
                || fi.IsStatic                                      // static members stay in place
                || f.Implementation.Body == null                    // intrinsic (handwritten or stub)
                || f.Implementation.OwnerType == null               // unknown provenance: play safe
                || !f.Implementation.OwnerType.IsLibrary()          // not a library function
                || f.Name.StartsWith("Law_");                       // conformance-harness reflection contract

            if (keep)
                keepNames.Add(f.Name);
            else
                movable.Add((f, fi));
        }

        foreach (var (f, _) in movable)
            if (!keepNames.Contains(f.Name))
                _moved.Add(f);
    }

    public bool ShouldMove(FunctionInstance f)
        => _moved.Contains(f);
}

/// <summary>Writes the per-library extension-block files ({Library}.g.cs) from the moved members.</summary>
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

        // One extension block per (receiver type, receiver parameter name), in encounter order:
        // function bodies are written in "static" mode and refer to the first Plato parameter
        // by name, so the receiver parameter must carry that name.
        var emittedSignatures = new HashSet<string>();
        foreach (var recvGroup in members.GroupBy(m => (m.ConcreteType.TypeDef, m.ReceiverName)))
        {
            var opened = false;
            foreach (var m in recvGroup)
            {
                var tw = NewMemberWriter(writer, m);
                var fi = tw.ToFunctionInfo(m.Function, m.ConcreteType.TypeDef);
                var receiverType = fi.ParameterTypes[0];

                // Defensive: never emit two extension members with the same C# signature.
                var sigKey = $"{receiverType}|{fi.Name}{fi.GenericsString}({fi.MethodParameterTypes.JoinStringsWithComma()})";
                if (!emittedSignatures.Add(sigKey))
                    continue;

                if (!opened)
                {
                    writer.WriteLine($"extension({receiverType} {m.ReceiverName})");
                    writer.WriteStartBlock();
                    opened = true;
                    // Recreate the member writer at the (now deeper) indentation level.
                    tw = NewMemberWriter(writer, m);
                    fi = tw.ToFunctionInfo(m.Function, m.ConcreteType.TypeDef);
                }

                // No-arg functions (single Plato parameter = the receiver) become extension
                // properties, preserving v.Length call syntax; the rest extension methods.
                // MethodSignature has no "static" and no receiver parameter: exactly the
                // instance-member form, which is what extension blocks expect. The body is
                // written in static mode so the receiver is referenced by name, not "this".
                tw.Write(fi.MethodSignature);
                var body = new CSharpFunctionBodyWriter(tw, fi, true, false, fi.NumParameters == 1);
                tw.Write(body.ToString());
                writer.Write(tw.ToString());
            }
            if (opened)
                writer.WriteEndBlock();
        }

        return writer.WriteEndBlock();
    }

    private static CSharpTypeWriter NewMemberWriter(CSharpWriter writer, MovedExtensionMember m)
        => new CSharpTypeWriter(writer, m.ConcreteType.TypeDef)
        {
            // Inside an extension block the receiver type's member scope is not implicit:
            // bare member references must be re-qualified (see CSharpTypeWriter fields).
            ExtensionReceiverName = m.ReceiverName,
            ExtensionStaticQualifier = $"{writer.Namespace}.{m.ConcreteType.TypeDef.Name}",
            ExtensionInstanceNames = m.Plan.InstanceNames,
            ExtensionStaticNames = m.Plan.StaticNames
        };
}
