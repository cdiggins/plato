// Coordinates C# output files, extension plans, runtime profiles, and optimizer passes.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter
{
    public class CSharpWriter : CodeBuilder<CSharpWriter>, ITirInlineHost
    {
        public CSharpWriter(Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            Analyzer = new PlatoAnalyzer(compilation);
            OutputFolder = outputFolder;
        }

        // The Number profile this compilation is written under (--profile=, default cpu64). The
        // emitted code names the machine number through NumberRuntimeType, which the runtime binds
        // from the same profile; the profile itself decides how literals are spelled and is stated
        // in every file header and checked by NumberProfile.g.cs.
        public NumberProfile Profile = NumberProfile.Default;
        public string Namespace = "Ara3D.Geometry";

        // The C# name the emitted code uses for the machine type under Number: an alias that
        // src/Plato.Intrinsics/NumberProfile.cs binds to double (cpu64, cpu64i32) or float (gpu32).
        // Emitting the alias rather than a keyword keeps the type in one place.
        public const string NumberRuntimeType = "NumberValue";

        // When true, emits the "extension style" output (--csharp-style=extensions, roadmap P2.2
        // as amended by the Phase 2 revision): the library-function fanout is written as CLASSIC
        // extension methods (one plain static class per Plato library) instead of instance
        // members on the partial structs. Compiles with the default LangVersion on net8.0.
        // Default (false) output is byte-identical to the original writer.
        public bool ExtensionStyle;

        // Extension style only (null otherwise): per-concrete-type move/keep plans, computed
        // for ALL types before anything is written (see BuildExtensionPlans).
        public Dictionary<TypeDef, ExtensionStylePlan> ExtensionPlans { get; private set; }

        // Extension style only (null otherwise): names of no-arg library functions that moved
        // out of their structs and became classic extension METHODS. The function-body writer
        // appends "()" at every call site of such a name (v.Length -> v.Length()). Globally
        // consistent by construction: a name that is a no-arg property on ANY generated type
        // (or declared no-arg by any interface) is never in this set - such functions are
        // demoted back into their structs instead.
        public HashSet<string> MovedNoArgNames { get; private set; }

        public ExtensionStylePlan GetExtensionPlan(TypeDef typeDef)
            => ExtensionPlans[typeDef];

        // Scalar erasure only: plan lookup by concrete type NAME, for the body writer's
        // receiver-type-aware analysis.
        private Dictionary<string, ExtensionStylePlan> _plansByName;
        public ExtensionStylePlan GetExtensionPlanByTypeName(string typeName)
        {
            if (ExtensionPlans == null || typeName == null)
                return null;
            if (_plansByName == null)
            {
                _plansByName = new Dictionary<string, ExtensionStylePlan>();
                foreach (var kv in ExtensionPlans)
                    _plansByName[kv.Key.Name] = kv.Value;
            }
            return _plansByName.TryGetValue(typeName, out var p) ? p : null;
        }

        // Computes the extension-style plans and the global no-arg name partition. Must run
        // before ANY file is written: Constants.g.cs / Extensions.g.cs / struct-kept bodies all
        // contain call sites of moved members that need the "()" injection.
        private void BuildExtensionPlans()
        {
            ExtensionPlans = new Dictionary<TypeDef, ExtensionStylePlan>();
            foreach (var c in Compilation.ConcreteTypes)
                if (!IgnoredTypes.Contains(c.TypeDef.Name) && !c.TypeDef.IsUnique && !ExtensionPlans.ContainsKey(c.TypeDef))
                    ExtensionPlans[c.TypeDef] = new ExtensionStylePlan(this, c);

            var collisionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Constants", "Extensions", "Intrinsics", "Interfaces", "NumberProfile",
            };
            foreach (var type in Compilation.AllTypeAndLibraryDefinitions.Where(t => t != null))
            {
                collisionNames.Add(type.Name);
                collisionNames.Add("_" + type.Name); // Concrete-type output file stem.
                collisionNames.Add(type.Name + "Extensions");
            }
            foreach (var plan in ExtensionPlans.Values)
                foreach (var member in plan.FieldCollisionMembers)
                    collisionNames.Add(member.Function.Name);
            var collisionIndex = 0;
            foreach (var plan in ExtensionPlans.OrderBy(p => p.Key.Name, StringComparer.Ordinal).Select(p => p.Value))
                if (plan.FieldCollisionMembers.Count > 0)
                {
                    plan.FieldCollisionClassName = ExtensionStyleWriter.AllocateClassName(
                        "__PlatoFieldCalls" + collisionIndex++, collisionNames);
                }

            // The struct-surface property-name set (the uniform rendering rule): every name that
            // renders with member/property syntax at a call site — exactly the per-type field +
            // pseudo-field names (unioned below) plus the BCL Count/NumColumns/NumRows
            // obligations. Interface interfaces declare no-arg obligations as METHODS, so they
            // contribute nothing here.
            var keptNoArg = new HashSet<string>();
            foreach (var p in ExtensionPlans.Values)
                keptNoArg.UnionWith(p.KeptNoArgPropertyNames);

            // The subset that is NOT owned by any per-type plan and therefore stays keyed by NAME
            // even under the receiver-aware rule (see IsStructSurfaceProperty).
            var globalNames = new HashSet<string>();

            // BCL/collection parity: Count (IReadOnlyCollection) and NumColumns/NumRows
            // (the handwritten Ara3D.Collections IReadOnlyList2D) are PROPERTIES on
            // receivers the emitter does not generate; generated code calls them on both
            // handwritten and generated receivers, and call-site syntax is decided by name.
            // GLOBAL: no ExtensionStylePlan owns them, so the per-receiver rule below cannot
            // find them on any plan and they must stay name-keyed.
            globalNames.Add("Count");
            globalNames.Add("NumColumns");
            globalNames.Add("NumRows");

            // Sum-type flattened fields (wave-2, plato-232) are genuine struct fields, so the
            // match lowering's projections (seg.Move_EndPoint) must read with field/property
            // syntax, never a "()" method call. The Kind tag field likewise. (Per-case Is<Case>
            // predicates are deliberately NOT added — they are methods and keep their "()".)
            // GLOBAL for the same reason: the flattened fields live on the sum's generated
            // struct, whose per-case projections are read through receivers the plans do not
            // describe.
            foreach (var t in Compilation.AllTypeAndLibraryDefinitions)
                if (t != null && t.IsSum)
                {
                    globalNames.Add("Kind");
                    foreach (var c in t.Cases)
                        foreach (var f in c.Fields)
                            globalNames.Add(f.FlatName);
                }

            keptNoArg.UnionWith(globalNames);
            StructSurfacePropertyNames = keptNoArg;
            GlobalStructSurfacePropertyNames = globalNames;

            var movedNoArg = new HashSet<string>();
            foreach (var p in ExtensionPlans.Values)
                foreach (var f in p.MovedFunctions)
                    if (f.ParameterNames.Count == 1)
                        movedNoArg.Add(f.Name);

            // A name may not be a property on one receiver type and a method on another
            // (call-site syntax is decided by name): demote the conflicted names everywhere.
            var conflicted = new HashSet<string>(System.Linq.Enumerable.Where(movedNoArg, keptNoArg.Contains));
            if (conflicted.Count > 0)
                foreach (var p in ExtensionPlans.Values)
                    p.DemoteMovedNames(conflicted);

            movedNoArg.ExceptWith(conflicted);
            MovedNoArgNames = movedNoArg;
        }

        // The five scalar wrapper types and the native C# primitive each one WRAPS. Scalars are
        // never erased to those primitives — that recipe (--scalar=float) was retired 2026-08-01,
        // see tracker/decisions/2026-08-01-wrapper-scalars-are-the-only-representation.md. The map
        // survives because TIR nodes still carry primitive types at the handwritten-intrinsics
        // boundary, and name-recognition (IsScalarTypeName, IsConcreteTypeName) must accept both
        // spellings. Deliberately NOT CSharpWriter.PrimitiveTypes: Angle (and every other struct)
        // is a real type — these five are the only wrappers over a BCL primitive.
        public static readonly Dictionary<string, string> ScalarPrimitives = new Dictionary<string, string>
        {
            { "Number", NumberRuntimeType },
            { "Integer", "int" },
            { "Boolean", "bool" },
            { "Character", "char" },
            { "String", "string" },
        };

        // Count of member/static bodies emitted from the monomorphized TIR (Elaborate →
        // Monomorphize → Emit). The TIR is the SOLE C# body writer since the legacy
        // CSharpFunctionBodyWriter was retired (consolidation plan C4). Diagnostic only.
        public int TirBodiesEmitted;

        // Bodied concrete members whose monomorphized TIR is not fully ground (residual
        // diagnostics, never-instantiated generic types, or an unresolved body). The writer
        // degrades gracefully — it emits a NotImplementedException-throwing stub WITH a comment
        // naming the member, rather than aborting all output. The count is a burn-down number:
        // it should shrink as the forward stdlib bodies mature and the monomorphizer coverage
        // grows. Under the shipping (stdlib-legacy) recipes this list is EMPTY (asserted by the
        // byte-identity gate); a non-empty list only ever appears for the forward stdlib.
        public readonly List<string> DegradedBodies = new List<string>();

        // --dump-tir=<dir> (null = off): write the per-phase TIR of every emitted body to
        // <dir>, one file per owner type. Records the elaborated/monomorphized INPUT and then
        // each optimizer pass (inline -> component-unroll -> array-materialize -> loop-lower)
        // that CHANGED the tree, so the dump shows exactly what each phase did. A development
        // aid for the optimizer passes; has no effect on the emitted C#.
        public string TirDumpDir;
        private readonly Dictionary<string, StringBuilder> _tirDumps = new Dictionary<string, StringBuilder>();

        // --inline-report (null = off): per-generation aggregation of inliner refusals + success
        // totals, printed after WriteAll. A diagnostic; no effect on emitted C#. See InlineReport.
        public InlineReport InlineReport { get; set; }

        /// <summary>The ONE place the pass order is written. Applies the optimizer TIR passes in order (inline → fusion/literal-unroll →
        /// component-unroll → consumer-fusion → array-materialize → loop-lower; see docs/csharp-optimizer-pipeline.md) and returns the transformed function. Shared by every
        /// body-emit site so the pass pipeline is defined once. With <see cref="TirDumpDir"/> set,
        /// records each phase that changed the tree (see the field comment).</summary>
        public TirFunction RunOptimizerPasses(TirFunction tir, CSharpFunctionInfo fi)
        {
            StringBuilder sb = null;
            string prev = null;
            if (TirDumpDir != null)
            {
                var key = SanitizeFileName(fi?.OwnerType?.Name ?? "Static");
                if (!_tirDumps.TryGetValue(key, out sb))
                    _tirDumps[key] = sb = new StringBuilder();
                var ps = fi?.ParameterTypes ?? (IReadOnlyList<string>)System.Array.Empty<string>();
                sb.AppendLine($"======== {fi?.ReturnType} {fi?.Name}({string.Join(", ", ps)}) ========");
                prev = tir?.Body?.ToString() ?? "<null>";
                sb.AppendLine("-- input (elaborated / monomorphized) --");
                sb.AppendLine(prev);
            }
            void Phase(string name)
            {
                if (sb == null)
                    return;
                var value = tir?.Body?.ToString() ?? "<null>";
                if (value != prev)
                {
                    sb.AppendLine($"-- after {name} --");
                    sb.AppendLine(value);
                    prev = value;
                }
            }
            if (InlineStatements)
            {
                tir = TirInliner.InlineStatements(tir, this, fi?.OwnerType?.Name, EliminateKnownLambdas, SimplifyInlineStatements, fi);
                Phase("experimental statement / callable expansion");
            }
            tir = TirInliner.Inline(tir, this, fi?.OwnerType?.Name, out _);
            Phase("inline (layer 7)");
            tir = TirFusion.Rewrite(tir, this);
            Phase("fuse / combinator fusion (layer 7b)");
            tir = TirComponentUnroller.UnrollFunction(tir, fi, this);
            Phase("optimize / component-unroll (layer 8)");
            tir = TirMultiConsumerFusion.Rewrite(tir, this);
            Phase("fuse-consumers / independent reductions");
            tir = TirArrayMaterializer.Rewrite(tir, this);
            Phase("optimize-arrays / materialize (layer 9)");
            tir = TirLoopLowerer.Rewrite(tir, this);
            Phase("loops / lower (layer 10)");
            tir = TirInPlaceUpdates.Rewrite(tir, this);
            Phase("in-place / last-use array updates (TKT-1132)");
            sb?.AppendLine();
            return tir;
        }

        private static string SanitizeFileName(string s)
        {
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s;
        }

        /// <summary>Writes the collected --dump-tir buffers to disk (one file per owner type).
        /// Called at the end of WriteAll; a no-op when dumping is off.</summary>
        public void FlushTirDumps()
        {
            if (TirDumpDir == null)
                return;
            System.IO.Directory.CreateDirectory(TirDumpDir);
            foreach (var kv in _tirDumps)
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(TirDumpDir, $"{kv.Key}.tir.txt"), kv.Value.ToString());
        }

        // Lazily built the first time a TIR is requested (UseTir only): the shared checker-run
        // lookups (see TirEmitSource). Never built on the default-off path.
        private TirEmitSource _tirSource;
        private TirEmitSource TirSource => _tirSource ?? (_tirSource = new TirEmitSource(Compilation));

        /// <summary>The fully-ground monomorphized TIR body for a (source function, concrete type),
        /// or null when none exists (non-ground / unresolved / not bodied).</summary>
        public TirFunction TryGetGroundTir(FunctionDef original, TypeDef concreteType)
            => TirSource.TryGetGroundTir(original, concreteType);

        /// <summary>The open-generic monomorphized TIR body for a (source function, generic concrete
        /// type) — the fallback when no ground instantiation exists because the compiled tier holds
        /// no call site at a concrete type argument. Non-null only for bodies that are type-agnostic
        /// in the residual variables, which are exactly the type parameters the emitted C# signature
        /// already declares (see <see cref="TirEmitSource.TryGetOpenGenericTir(FunctionDef, TypeDef)"/>).</summary>
        public TirFunction TryGetOpenGenericTir(FunctionDef original, TypeDef concreteType)
            => TirSource.TryGetOpenGenericTir(original, concreteType);

        /// <summary>Why a bodied member has no TIR to emit (see <see cref="TirEmitSource.ExplainMissing"/>).</summary>
        public string ExplainMissingTir(FunctionDef original, TypeDef concreteType)
            => TirSource.ExplainMissing(original, concreteType?.Name);

        /// <summary>The generic (unspecialized) TIR for a STATIC body — a constant or an IArray
        /// library function — or null when the function's elaboration has unresolved nodes.</summary>
        public TirFunction TryGetStaticTir(FunctionDef original)
            => TirSource.TryGetStaticTir(original);

        /// <summary>Name-keyed ground-TIR lookup for the inliner (--inline): the callee's body
        /// specialized for the receiver's solved concrete type name.</summary>
        public TirFunction TryGetGroundTirByTypeName(FunctionDef original, string concreteTypeName)
            => TirSource.TryGetGroundTir(original, concreteTypeName);

        public bool IsConcreteTypeName(string name)
            => name != null
               && (ScalarPrimitives.ContainsKey(name) || GetExtensionPlanByTypeName(name) != null);

        public bool IsScalarPrimitiveName(string name)
            => name != null && ScalarPrimitives.ContainsKey(name);

        public ISet<string> TryGetKnownMemberNames(string typeName)
        {
            var plan = GetExtensionPlanByTypeName(typeName);
            if (plan == null)
                return null;
            var set = new HashSet<string>(plan.InstanceNames);
            set.UnionWith(plan.StaticNames);
            return set;
        }

        /// <summary>Test helper: the ground input TIR for a (concrete type name, function name),
        /// for in-proc inliner shape tests (M0).</summary>
        public TirFunction TestGetGroundTir(string concreteTypeName, string functionName)
            => TirSource.TryGetGroundTirByNames(concreteTypeName, functionName);

        // When true (--static-abstract), an interface member whose receiver is `_` — Plato's
        // constructor-shaped idiom for a TYPE-level operation (`Zero(_: Self): Self`,
        // `FromAmount(_: Self, x: Number): Self`) — emits as a C# `static abstract` interface
        // member instead of being omitted from the interface entirely. The implementing type
        // already emits a plain `static` method for it (CSharpFunctionInfo.IsStatic), so the two
        // halves finally agree; previously the interface either skipped the member (losing the
        // contract) or declared it as an instance member that a static method cannot satisfy
        // (CS0736). See the 2026-07-29 static-interface-members ADR and plato-312.
        //
        // OPT-IN because it is not free for stdlib-legacy: `IArrayLike<T>` there declares
        // NumComponents / CreateFromComponents / CreateFromComponent with `_` receivers, so
        // enabling this would add three static abstract obligations to every IArrayLike
        // implementor and move the diff-gated goldens. Default (false) output is byte-identical,
        // which is what keeps hard rule 2 intact; the forward recipe opts in.
        public bool StaticAbstract;

        // PROPERTY-FREE EMISSION (unconditional; see the 2026-08-01 ADR). The generated output
        // declares no C# properties and no indexers:
        //   - interface interfaces (Interfaces.g.cs) declare no-arg obligations as METHODS;
        //   - struct-KEPT members (obligations, conversions, statics, stubs) emit as methods;
        //     struct indexers are dropped (At() remains);
        //   - the IArrayLike scaffolding (NumComponents/Components) and nullary constants
        //     (Constants.Pi()) are methods; call sites get "()" accordingly.
        // The UNIFORM RENDERING RULE decides call-site syntax: a member renders with member/property
        // syntax iff its name is on the struct surface (a genuine field, a primitive pseudo-field
        // X/Y/Z/M11..., or a BCL Count/NumRows/NumColumns obligation — see below); every other
        // member is a method call. This is independent of ScalarErase: a wrapper-scalar recipe is
        // just as property-free as an erased one.
        //
        // The UNION over all types of the names that keep PROPERTY/field access syntax — struct
        // fields, the primitive pseudo-fields (X/Y/Z, M11...), the BCL Count/NumRows/NumColumns
        // obligations, and the sum-type flattened fields. Built in BuildExtensionPlans.
        //
        // This set drives the MOVED/KEPT partition (a name that is a property on ANY generated type
        // is demoted back into the struct on ALL types, because a C# instance member silently hides
        // a same-name extension method) and is the FALLBACK for receivers no plan describes. It is
        // NOT the call-site rendering rule any more: rendering is decided per receiver by
        // IsStructSurfaceProperty (plato-323), which every consumer routes through.
        public HashSet<string> StructSurfacePropertyNames { get; private set; }

        // The struct-surface names that no ExtensionStylePlan owns, so they must be decided by NAME
        // on every receiver: the BCL/collection obligations (Count/NumColumns/NumRows, properties on
        // handwritten receivers the emitter never generates) and the sum-type flattened fields +
        // Kind tag (read through match-lowering projections on receivers the plans do not describe).
        // A subset of StructSurfacePropertyNames.
        public HashSet<string> GlobalStructSurfacePropertyNames { get; private set; }

        // The RECEIVER-AWARE rendering rule (plato-323): does `name` render with property/field
        // syntax when read off a receiver of type `ownerTypeName`?
        //
        // The global name set alone is wrong: a receiver whose own struct surface does not carry
        // `name` reads a MOVED
        //     library function, i.e. an extension METHOD. `Amount` (a field on an image filter, a
        //     dynamic quantity and an uncertainty value) otherwise forced property syntax onto
        //     `Angle.Radians()` / `Amount(x: Angle)` at every call site: 110 x CS0030.
        //
        // So: resolve `name` against the receiver's OWN plan when the receiver has one, and fall
        // back to the global union only for receivers no plan describes (handwritten runtime types,
        // IgnoredTypes, interface interfaces, generic parameters). GlobalStructSurfacePropertyNames
        // short-circuits first because those names are properties on every receiver by construction.
        //
        // Both halves of the rule stay consistent with the DECLARATIONS because every consumer —
        // CSharpFunctionInfo.EmitAsMethod (which decides property-vs-method when EMITTING the
        // member), the two forwarding-shim writers in CSharpConcreteTypeWriter, and the body
        // writer's call sites — routes through this one predicate with the owner type in hand.
        public bool IsStructSurfaceProperty(string ownerTypeName, string name)
        {
            if (name == null)
                return false;
            // The handwritten members the declaration mis-describes. Checked before the scalar
            // short-circuit because some of them are read off the WRAPPER, in the
            // `((Number)x).Sqrt` forwarders an erased per-type file emits.
            var ov = PrimitiveSurfaceOverride(ownerTypeName, name);
            if (ov.HasValue)
                return ov.Value;
            if (GlobalStructSurfacePropertyNames?.Contains(name) ?? false)
                return true;
            var plan = GetExtensionPlanByTypeName(ownerTypeName);
            if (plan != null)
                return plan.KeptNoArgPropertyNames.Contains(name);
            // No plan describes this receiver: an interface, a generic type variable bound to
            // one, an IgnoredTypes collection/delegate, or an unknown handwritten type. Under
            // an interface declares every no-arg obligation as a METHOD, and the
            // IgnoredTypes receivers are handwritten BCL collections whose only property-shaped
            // members are already in the global set. So there is NO struct surface here — falling
            // back to the union let a field on some unrelated struct pin property syntax onto
            // `mesh.VertexCount()` / `edges.UndirectedEdgeCount()` (CS0428 method-group reads).
            return false;
        }

        // A Plato scalar WRAPPER name (Number/Integer/...) or the primitive it wraps
        // (NumberValue/int/...): a TIR node's type may carry either at the intrinsics boundary.
        public static bool IsScalarTypeName(string name)
            => name != null
               && (ScalarPrimitives.ContainsKey(name) || ScalarPrimitives.ContainsValue(name));

        // When true (--loops), recognized array-combinator call sites (Map/Zip/Reduce/All/Any/
        // Reverse/WithNext/MapPairs/... on one-dimensional list receivers) are lowered to
        // for-loop statements filling materialized arrays (see TirLoopLowerer). Runs after the
        // other optimizer passes. Default (false) output is unchanged.
        public bool LowerLoops;

        // When true (--fuse), array-combinator chains are fused before lowering (n.Range.Map(f)
        // to n.MapRange(f), xs.Map(g).Map(f) to one Map, xs.Take(k).Map(f) to k.MapRange) and
        // literal-count ranges of at most TirFusion.UnrollLimit are unrolled to straight-line
        // code (see TirFusion, TirLiteralUnroller). Pure rewrites: the output computes the same
        // values with fewer arrays. Usually combined with --loops.
        public bool FuseCombinators;
        public bool FuseConsumers { get; set; }
        public bool ExactOutputAllocation { get; set; }

        // When true (--lean-inlining, TKT-1091), [MethodImpl(AggressiveInlining)] stays only on
        // one-line forwarding members (see LeanInliningPass); the handwritten intrinsics keep theirs.
        // Default (false) output is unchanged.
        public bool LeanInlining { get; set; }

        // When true (--in-place, TKT-1132), a SetAt on an array that nothing can observe afterwards is
        // emitted as a write into the array's own storage (see TirInPlaceUpdates). Default (false)
        // output is unchanged.
        public bool InPlaceUpdates { get; set; }

        // When true (--inline, roadmap P3.2 "beta reduction"), resolved calls to small library
        // functions are INLINED at emission: the callee's fully-ground TIR body is substituted at
        // the call site (parameters replaced by argument nodes), iteratively, before the
        // component unroller and array materializer run — so HOF call sites hidden inside callees
        // become visible to those transforms. See TirInliner for the eligibility rules.
        // Default (false) output is unchanged.
        public bool InlineCalls { get; set; }
        public bool InlineStatements { get; set; }
        public bool EliminateKnownLambdas { get; set; }
        public bool SimplifyInlineStatements { get; set; }
        public int StatementNodeBudget { get; set; } = 4096;
        public int StatementModuleNodeBudget { get; set; } = 1000000;
        internal int RemainingStatementGrowth = 1000000;

        // When true (--optimize-arrays, optimizer stage 2 increment 1), Map/MapRange call sites in
        // MATERIALIZATION positions (constructor / tuple arguments — results stored into structs —
        // and multi-referenced local bindings) are rewritten to the eager MapEager/MapRangeEager
        // intrinsics, so multi-consumed pipelines evaluate each element once instead of per access
        // (see TirArrayMaterializer + docs/design/optimizer-stage2-plan.md). TIR-path only; sound for the
        // pure language, gated on profitability shape. Default (false) output is unchanged.
        public bool OptimizeArrays;

        // When true, applies the component-op unrolling optimization (--optimize, roadmap P3.1):
        // recognized MapComponents/ZipComponents/Reduce/All*/Any* call sites on statically-known
        // IArrayLike types are rewritten to direct field expressions at emission time (see
        // ComponentUnroller). Default (false) output is byte-identical to the original writer.
        public bool Optimize;

        // Lazily-built table for ComponentUnroller: concrete IArrayLike type name -> field names.
        private Dictionary<string, IReadOnlyList<string>> _componentFields;
        public IReadOnlyList<string> GetComponentFields(string typeName)
        {
            if (_componentFields == null)
                _componentFields = ComponentUnroller.BuildComponentFieldTable(Compilation);
            return typeName != null && _componentFields.TryGetValue(typeName, out var fields) ? fields : null;
        }

        // Lazily-built table for ComponentUnroller under scalar erasure: concrete IArrayLike
        // type name -> the PLATO type of its components ("Number" for the primitive
        // pseudo-field types, the shared field type otherwise).
        private Dictionary<string, string> _componentPlatoTypes;
        public string GetComponentPlatoType(string typeName)
        {
            if (_componentPlatoTypes == null)
            {
                _componentPlatoTypes = new Dictionary<string, string>();
                foreach (var ct in Compilation.ConcreteTypes)
                {
                    if (ct.TypeDef.TypeParameters.Count > 0)
                        continue;
                    if (!System.Linq.Enumerable.Any(ct.AllInterfaces, i => i.Name == "IArrayLike"))
                        continue;
                    if (PrimitiveTypes.ContainsKey(ct.Name))
                        _componentPlatoTypes[ct.Name] = "Number"; // matches the Components scaffolding hack
                    else if (ct.TypeDef.Fields.Count > 0)
                        _componentPlatoTypes[ct.Name] = ct.TypeDef.Fields[0].Type.Name;
                }
            }
            return typeName != null && _componentPlatoTypes.TryGetValue(typeName, out var t) ? t : null;
        }

        // Collected by CSharpConcreteTypeWriter while writing each type in extension style;
        // written out by ExtensionStyleWriter.WriteLibraryFiles at the end of WriteAll.
        public List<MovedExtensionMember> MovedMembers { get; } = new List<MovedExtensionMember>();

        // All Plato type/library/interface names; used by the extension-style body writer to
        // distinguish bare type references from bare static-member references.
        private HashSet<string> _allTypeNames;
        public HashSet<string> AllTypeNames => _allTypeNames ?? (_allTypeNames = new HashSet<string>(
            System.Linq.Enumerable.Select(Compilation.AllTypeAndLibraryDefinitions, t => t?.Name ?? "")));


        public Compiler.Compilation Compilation => Analyzer.Compilation;
        public PlatoAnalyzer Analyzer { get; }
        public Dictionary<string, StringBuilder> Files { get; } = new Dictionary<string, StringBuilder>();

        public DirectoryPath OutputFolder { get; }

        // Unique (affine) builder types (roadmap Phase 6): the Plato names map to prefixed
        // C# type names because the handwritten intrinsics live in namespace Ara3D.Geometry,
        // where a type literally named "List<T>" would shadow System.Collections.Generic.List
        // for every handwritten file in that namespace (and "Buffer" would collide with
        // System.Buffer). The generated code (which has no using for S.C.G.List) simply uses
        // the prefixed names.
        public static Dictionary<string, string> UniqueTypeCSharpNames = new Dictionary<string, string>()
        {
            { "List", "PlatoList" },
            { "Buffer", "PlatoBuffer" },
        };

        // The handwritten struct surface of a PRIMITIVE type, where the Plato.Intrinsics runtime
        // disagrees with what the Plato declaration implies. The writer generates
        // NO struct for a PrimitiveTypes entry (CSharpConcreteTypeWriter's "// Fields" block is
        // `!IsPrimitive`), so it cannot see the runtime's real shape and reads the declaration as a
        // proxy: a declared field is a property/field, everything else is a method. That proxy is
        // right for the whole V2 surface except these members, hence a short exception list
        // rather than a second full surface record. Keyed "{Type}.{Member}"; the value is whether
        // the runtime spells the member as a PROPERTY.
        //
        //   Matrix4x4.Translation / .Rotation / Number.Angle — NOT declared as fields (they are
        //     library functions in Plato), yet V2 keeps them as properties. Before the
        //     receiver-aware rule they rode on the global name union by accident, each name being a
        //     field on some unrelated struct.
        //
        // The V2-side inconsistency itself (a method-form runtime with ten property leftovers) is
        // tracked as plato-331; when it is resolved this table shrinks to nothing. Public so
        // PlatoTests\IntrinsicsSurfaceTests.cs can police it against the actual V2 surface.
        public static readonly IReadOnlyDictionary<string, bool> PrimitiveSurfaceOverrides
            = new Dictionary<string, bool>
            {
                // Angle.Radians and Plane.Distance left this table with plato-365 increment (a):
                // both are now GENERATED fields, so the declaration proxy is simply right and
                // there is nothing to pin.
                // Matrix4x4.Translation / .Rotation left this table with plato-308: the forward
                // stdlib has reference bodies for both, so they are GENERATED members now and the
                // handwritten properties are gone — nothing left to pin.
                // Number.Angle left 2026-08-01: the property left the runtime with the kernel
                // reduction (the conversion is generated now), so the declaration proxy is right.

                // 2026-08-01 kernel reduction: the irreducible kernel members live on the wrapper
                // structs as PROPERTIES (wrapper-mode generated call sites use property syntax),
                // while the declarations are bodiless library functions — so each needs a pin for
                // every recipe.
                { "Integer.Hash", true },
                { "Integer.BitwiseNot", true },
                { "Integer.ToNumber", true },
                { "Number.Hash", true },
                { "Number.Floor", true },
                { "Number.Sqrt", true },
                { "Number.Exp", true },
                { "Number.NaturalLog", true },
                { "Number.Sin", true },
                { "Number.Cos", true },
                { "Number.IsNaN", true },
                { "Number.IsInfinite", true },
                { "Number.ToInteger", true },
                { "String.Count", true },

                // plato-383: String wraps a REFERENCE, so its storage is a private field behind a
                // null-normalizing property (`default(String)` reads as the empty string). Value is
                // the one wrapper accessor V2 does not spell as a field.
                { "String.Value", true },
            };

        /// <summary>Whether V2 spells {typeName}.{name} as a property, or null when the declaration
        /// is a reliable proxy (the usual case). See <see cref="PrimitiveSurfaceOverrides"/>.</summary>
        public static bool? PrimitiveSurfaceOverride(string typeName, string name)
        {
            if (typeName == null || name == null)
                return null;
            return PrimitiveSurfaceOverrides.TryGetValue($"{typeName}.{name}", out var b)
                ? b
                : (bool?)null;
        }

        public static HashSet<string> IgnoredTypes = new HashSet<string>()
        {
            "Array",
            "Function0",
            "Function1",
            "Function2",
            "Function3",
        };

        public static HashSet<string> IgnoredFunctions = new HashSet<string>()
        {
            "FieldNames",
            "FieldValues",
            "TypeName",
            "Equals",
            "NotEquals",
            "GetHashCode",
            "ToString",
            "GetType",
            // The serialization surface the writer synthesizes on every struct (see
            // CSharpConcreteTypeWriter.WriteSerializationSurface). A library function of the same
            // name would be a duplicate member, exactly as ToString/Equals would.
            "ToJson",
            "FromJson",
            "AppendJson",
            "TryFormat",
            "Parse",
            "TryParse",
            // These are functions of IArrayLike
            "Components",
            "CreateFromComponents",
            "CreateFromComponent",
            "NumComponents",
            
            // Implemented elswehere.
            // "Range" left this list 2026-08-01: the forward stdlib gives it a Plato body
            // (primitives.library.plato, `Range(self) => MapRange(self, i => i)`), and
            // while it was ignored no ExtensionStylePlan owned it, so every call site rendered
            // with PROPERTY syntax (`n.Range`) against a handwritten extension METHOD.
            "MapRange",
        };

        // Types the writer does NOT generate a shape for: the declaration is a name only, and the
        // C# type on the right is the real one. As of plato-365 that is the SCALAR MAP and nothing
        // else — every type with fields now generates an ordinary struct from its declaration.
        //
        // What used to be here, and where it went (all 2026-07-30):
        //   Vector2/3/4/8 — never declared in the forward stdlib at all (Number2/3/4/8 and
        //     Vector2D/3D carry that vocabulary); the names survive only as the far side of
        //     CSharpConcreteTypeWriter.IntrinsicVectorBridges.
        //   Angle, Plane, Matrix3x2, Matrix4x4, Quaternion — moved to IntrinsicBackedTypes: the
        //     declaration owns the shape, the handwritten runtime still owns the behaviour.
        //
        // The NAMES come from the compiler-side WriterPrimitiveNames.All, which is also the
        // linter's LINT015 authority; this dictionary only attaches the runtime mapping, so there
        // is exactly one place to edit when the set changes. A name with no mapping here is a
        // startup failure rather than a silently-unmapped primitive.
        //
        // NOTE FOR A NEW BACKEND (C++, GLSL, Rust): this is the whole primitive story. A backend
        // needs a scalar map — number / integer / boolean / character / string / type / function —
        // and nothing else. No System.Numerics analogue, no per-type name list.
        public static Dictionary<string, string> PrimitiveTypes
            = System.Linq.Enumerable.ToDictionary(WriterPrimitiveNames.All, n => n, PrimitiveRuntimeType);

        private static string PrimitiveRuntimeType(string name)
        {
            switch (name)
            {
                case "Number": return NumberRuntimeType;
                case "Boolean": return "bool";
                case "Integer": return "int";
                case "Character": return "char";
                case "String": return "string";
                case "Type": return "System.Type";
            }
            if (Compiler.Analysis.Linter.IsFunctionTypeName(name))
                return "System.Func";
            throw new Exception(
                $"WriterPrimitiveNames.All contains '{name}' with no C# runtime mapping. Add one "
                + "here, or (plato-365) remove the name so the declaration generates a struct.");
        }

        /// <summary>
        /// Types whose SHAPE comes from the stdlib declaration (an ordinary generated struct, fields
        /// and all) but whose BEHAVIOUR is still handwritten: their bodiless intrinsic members and
        /// operator overloads are supplied by the Plato.Intrinsics partial struct, exactly as they
        /// were while the name was a <see cref="PrimitiveTypes"/> entry, so the writer must not emit
        /// a NotImplementedException stub over the top of them.
        ///
        /// This is the half of "primitive" plato-365 KEEPS. The half it deletes is the shape
        /// override: the declaration is now the authority on the fields, and the handwritten side
        /// reaches System.Numerics from those fields rather than being System.Numerics. Every entry
        /// here graduated out of <see cref="PrimitiveTypes"/>.
        ///
        /// Unlike the primitive list this one is NOT a language-level fact: it changes which C#
        /// members the writer emits, never what a declaration means. A name here is still an
        /// ordinary Plato type.
        /// </summary>
        public static readonly HashSet<string> IntrinsicBackedTypes = new HashSet<string>
        {
            "Angle",
            "Plane",
            "Matrix3x2",
            "Matrix4x4",
            "Quaternion",
        };

        /// <summary>Whether the handwritten runtime — not the writer — supplies this type's
        /// bodiless members and operators. True for every primitive and for
        /// <see cref="IntrinsicBackedTypes"/>.</summary>
        public static bool IsIntrinsicBacked(string name)
            => name != null && (PrimitiveTypes.ContainsKey(name) || IntrinsicBackedTypes.Contains(name));

        /// <summary>The version stamped into every emitted <c>[GeneratedCode]</c> attribute. Read
        /// from the writer assembly rather than hard-coded, so it cannot drift.</summary>
        public static readonly string ToolVersion
            = typeof(CSharpWriter).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";

        public const string ToolName = "Plato";

        /// <summary>The attributes every emitted type carries: `[GeneratedCode]` is what analyzers,
        /// StyleCop and `dotnet format` key off per-type, and `[ExcludeFromCodeCoverage]` keeps
        /// hundreds of thousands of emitted lines out of the coverage denominator. Fully qualified
        /// on purpose — a `using` for either namespace could collide with a Plato type name.</summary>
        public static string GeneratedCodeAttributes
            => $"[System.CodeDom.Compiler.GeneratedCode(\"{ToolName}\", \"{ToolVersion}\")"
               + ", System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

        /// <summary>The interface-level form. `ExcludeFromCodeCoverage` is class/struct-only
        /// (CS0592), and an interface has no bodies to keep out of a coverage report anyway.</summary>
        public static string GeneratedCodeAttribute
            => $"[System.CodeDom.Compiler.GeneratedCode(\"{ToolName}\", \"{ToolVersion}\")]";

        public CSharpWriter WriteFile(FilePath fileName, Func<CSharpWriter> f)
        {
            StartNewFile(fileName);
            // `<auto-generated/>` must be the FIRST line: that marker is what StyleCop, the .NET
            // analyzers, `dotnet format` and most coverage tools read to skip a file wholesale.
            // No timestamp — regenerating unchanged sources must produce an unchanged file, so a
            // stale checkout shows a real diff rather than a clock difference.
            WriteLine("//------------------------------------------------------------------------------");
            WriteLine("// <auto-generated>");
            WriteLine("//     This code was generated by the Plato compiler. DO NOT EDIT.");
            WriteLine("//     Changes to this file will be lost when the code is regenerated.");
            WriteLine($"//     Number profile: {Profile.Name} (Number is IEEE 754 binary{Profile.NumberBits}).");
            WriteLine("// </auto-generated>");
            WriteLine("//------------------------------------------------------------------------------");
            WriteLine();
            WriteLine("using System.Runtime.CompilerServices;");
            WriteLine("using System.Runtime.Serialization;");
            WriteLine("using System.Runtime.InteropServices;");
            WriteLine("using System.Text.Json.Serialization;");
            WriteLine("using static System.Runtime.CompilerServices.MethodImplOptions;");
            WriteLine("using Ara3D.Collections;");
            WriteLine("");
            WriteLine($"namespace {Namespace}");
            WriteStartBlock();
            f();
            WriteEndBlock();
            return this;
        }

        public CSharpWriter WriteAll()
        {
            // The lambda-capture hoist names (`_var{N}`) and the inliner's collision-rename
            // names (`{p}_{N}`) draw from process-global counters; reset them per generation so
            // two WriteAll runs in one process produce identical output (a fresh CLI process
            // always started at 0, so file output is unchanged).
            SymbolRewriter.NextId = 0;
            TirInliner.NextRenameId = 0;
            RemainingStatementGrowth = StatementModuleNodeBudget;
            TirFusion.NextId = 0;
            TirLoopLowerer.NextId = 0;

            WriteFile("NumberProfile.g.cs", WriteNumberProfileCheck);
            if (ExtensionStyle)
                BuildExtensionPlans();

            WriteFile("Interfaces.g.cs", WriteConceptInterfaces);
            WriteFile("Constants.g.cs", WriteConstantLibraryMethods);
            WriteFile("Extensions.g.cs", WriteInterfaceLibraryMethods);

            foreach (var c in Compilation.ConcreteTypes)
            {
                var name = c.TypeDef.Name;
                // Unique (affine) builder types are backed entirely by the handwritten
                // Plato.Intrinsics implementations (PlatoList<T> / PlatoBuffer<T>); no
                // struct is generated for them (roadmap Phase 6).
                if (!IgnoredTypes.Contains(name) && !c.TypeDef.IsUnique)
                    WriteFile($"_{name}.g.cs", () => WriteTypeImplementation(c));
            }

            if (ExtensionStyle)
                ExtensionStyleWriter.WriteLibraryFiles(this);

            if (LeanInlining)
                LeanInliningPass.Apply(Files);

            FlushTirDumps();
            return this;
        }

        // A literal's C# spelling. A Number literal is spelled for the profile, exactly and in the
        // invariant culture: under cpu64 the shortest round-trip binary64 digits with a `D` suffix,
        // under gpu32 the value rounded once to binary32 with an `F` suffix, so C# never rounds it
        // a second time. (The front end parses Number literals to binary64, so a gpu32 literal is
        // rounded from that, not from its decimal text; a decimal halfway between two binary32
        // values after binary64 rounding is the only case that differs.)
        public string LiteralString(object value)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            switch (value)
            {
                case double d:
                {
                    var rounded = Profile.NumberBits == 32 ? (float)d : d;
                    if (!double.IsFinite(rounded))
                        return $"{NumberRuntimeType}." + (double.IsNaN(rounded) ? "NaN"
                            : rounded > 0 ? "PositiveInfinity" : "NegativeInfinity");
                    return Profile.NumberBits == 32
                        ? ((float)d).ToString("R", inv) + "F"
                        : d.ToString("R", inv) + "D";
                }
                case float f:
                    return LiteralString((double)f);
                case string s:
                    return StringLiteral(s);
                case long l:
                    return IntegerLiteral(l, Profile.IntegerBits);
                case int i:
                    return IntegerLiteral(i, Profile.IntegerBits);
                default:
                    return value.ToLiteralString();
            }
        }

        /// <summary>A Plato string literal as C# source. The value is the characters between the quotes,
        /// verbatim (the front end does no escape processing), so this escapes the backslash and the quote,
        /// writes control characters as n, r, t or uXXXX escapes, and writes every non-ASCII character as a
        /// uXXXX escape: the generated file is then pure ASCII and means the same in any encoding.</summary>
        public static string StringLiteral(string value)
        {
            var sb = new System.Text.StringBuilder(value.Length + 2);
            sb.Append('"');
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20 || c >= 0x7F)
                            sb.Append("\\u").Append(((int)c).ToString("X4"));
                        else
                            sb.Append(c);
                        break;
                }
            }
            return sb.Append('"').ToString();
        }

        // The host contract: what the handwritten Plato.Intrinsics runtime implements for a
        // bodiless declaration. That is every declaration of `library Intrinsics`
        // (stdlib/foundation/intrinsics.library.plato), and every interface member, which a
        // primitive's runtime struct implements as an instance member (`Boolean.And`). Every
        // other bodiless function, a library function declared without a body, is a stub
        // (BodilessStub, plato-459).
        public static bool IsHostIntrinsic(FunctionDef f)
            => f?.OwnerType?.Name == "Intrinsics"
               || f?.FunctionType == FunctionType.Interface
               || f?.OwnerType?.IsInterface() == true;

        // The one body every bodiless non-intrinsic function gets, whatever its receiver
        // (plato-459): it throws and names the function, so a call fails where it is made.
        public static string BodilessStub(string functionName)
            => $" => throw new System.NotImplementedException(\"plato: {functionName} is declared without a body\");";

        /// <summary>
        /// A C# integer literal for <paramref name="value"/> at the profile's integer width.
        /// A 32-bit profile spells a value C# cannot write as an <c>int</c> literal — outside
        /// the <c>int</c> range, or <c>int.MinValue</c>, whose decimal form is a <c>long</c> —
        /// as an unchecked narrowing cast, which is the wrap the runtime's <c>int</c> performs.
        /// </summary>
        public static string IntegerLiteral(long value, int integerBits)
        {
            var text = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (integerBits == 32 && (value <= int.MinValue || value > int.MaxValue))
                return $"unchecked((int)({text}L))";
            return text;
        }

        // Stops the C# build when it is not under the profile this code was written for: the
        // runtime binds NumberValue and IntegerValue from the MSBuild property PlatoNumberProfile,
        // and literals here are spelled for this writer's profile. PLATO_NUMBER_GPU32 means
        // binary32 Number; PLATO_INTEGER_32 means a 32-bit Integer. Either symbol in the wrong
        // state is a different profile.
        public CSharpWriter WriteNumberProfileCheck()
        {
            var numberWrong = Profile.NumberBits == 32 ? "!PLATO_NUMBER_GPU32" : "PLATO_NUMBER_GPU32";
            var integerWrong = Profile.IntegerBits == 32 ? "!PLATO_INTEGER_32" : "PLATO_INTEGER_32";
            WriteLine($"#if {numberWrong} || {integerWrong}");
            WriteLine($"#error This code was generated under Number profile {Profile.Name}; build it with -p:PlatoNumberProfile={Profile.Name}, or regenerate it with the profile you build under.");
            WriteLine("#endif");
            return this;
        }

        public void StartNewFile(string fileName)
        {
            sb = new StringBuilder();
            Files.Add(fileName, sb);
        }

        public CSharpTypeWriter NewDefaultTypeWriter() 
            => new CSharpTypeWriter(this, null);

        public CSharpWriter WriteConstantFunction(FunctionDef f)
        {
            var tmp = NewDefaultTypeWriter();
            var fi = tmp.ToFunctionInfo(f, null, FunctionInstanceKind.Constant);
            tmp.WriteStaticFunction(fi);
            // Extension style fixes the V1 indentation quirk (see WriteWithLineStateSync);
            // the default mode must stay byte-identical, misindentation included.
            return ExtensionStyle ? this.WriteWithLineStateSync(tmp.ToString()) : Write(tmp.ToString());
        }

        public CSharpWriter WriteConstantLibraryMethods()
        {
            WriteLine($"public static class Constants");
            WriteStartBlock();
            foreach (var f in Compilation.Libraries.AllConstants())
                WriteConstantFunction(f);
            WriteEndBlock();
            return this;
        }

        // The CONCRETE array types. A library function whose receiver is one of these renders
        // exactly like a legacy `IArray*`-interface receiver — ToCSharpTypeName maps both to the
        // runtime list interfaces (Array<T> -> IReadOnlyList<T>, Array2D -> IReadOnlyList2D, ...)
        // — so it belongs on the same classic-extension-method path (see
        // IsListExtensionReceiver / WriteInterfaceLibraryMethods).
        public static readonly HashSet<string> ConcreteArrayTypeNames = new HashSet<string>()
        {
            "Array",
        };

        // Is this the receiver of a library function that emits as a classic extension method on a
        // runtime list interface in Extensions.g.cs? Two spellings of the same shape:
        //   - an `IArray*` interface (but not IArrayLike, the fixed-arity component scaffolding) —
        //     stdlib-legacy's spelling;
        //   - the CONCRETE Array/Array2D/Array3D types — the forward stdlib's spelling
        //     (`BoundsOfPoints(points: Array<Point2D>)`).
        // The concrete arm is plato-323: `Array` is in IgnoredTypes, so those types get no
        // ExtensionStylePlan, and moved members are only discovered while writing a concrete type's
        // own file — so before this a library function with an array receiver was emitted by NO
        // writer at all and every call site of it was a CS1061.
        private const string IndexableInterfaceName = "IIndexable";

        // A receiver typed by the one-dimensional IIndexable<T> interface itself.
        private static bool IsIndexableReceiver(TypeExpression pt)
            => pt?.Def != null && pt.Def.IsInterface() && pt.Def.Name == IndexableInterfaceName;

        private static bool IsListExtensionReceiver(TypeExpression pt)
            => pt?.Def != null
               && (pt.Def.IsInterface()
                   ? pt.Def.Name.StartsWith("IArray") && !pt.Def.Name.StartsWith("IArrayLike")
                   : ConcreteArrayTypeNames.Contains(pt.Def.Name));

        // A receiver whose C# type is a runtime class with no generated struct and so no plan of
        // its own: a builder (List<T>, Buffer<T>, a unique type: PlatoList, PlatoBuffer) or a
        // function (Function0..Function9: System.Func). A library function over one is written
        // here as an extension method on that class, like an array receiver; before this it was
        // dropped without a word (builders TKT-347 and compiler-416; functions TKT-353,
        // MinimizeOverDomain in the STEP mesher's B-spline inverse).
        private static bool IsRuntimeClassExtensionReceiver(TypeExpression pt)
            => pt?.Def != null
               && ((pt.Def.IsUnique && (pt.Def.Name == "List" || pt.Def.Name == "Buffer"))
                   || Compiler.Analysis.Linter.IsFunctionTypeName(pt.Def.Name));

        public CSharpWriter WriteInterfaceLibraryMethods()
        {
            WriteLine($"public static partial class Extensions");
            WriteStartBlock();

            // One emission per distinct C# signature. The forward stdlib overloads array-receiver
            // functions on the ELEMENT type (BoundsOfPoints on Array<Point2D> and Array<Point3D>),
            // which are distinct C# signatures — but two Plato libraries may also declare the same
            // one, and a duplicate would be CS0111 rather than a fixed error.
            var emittedSignatures = new HashSet<string>();

            // A name the Array library itself declares (Map, Reduce, Zip, ...: bodied in the stdlib
            // or supplied by the runtime) belongs to Array. The IIndexable version of it is written
            // in terms of the Array one (`MapRange(xs.Count, i => xs[i]).Map(f)`), so emitting it
            // on IReadOnlyList would either take the Array function's C# signature (and recurse
            // into itself) or add an ambiguous overload.
            var arrayFunctionNames = new HashSet<string>(System.Linq.Enumerable.Select(
                System.Linq.Enumerable.Where(Compilation.Libraries.AllFunctions(),
                    g => g.NumParameters > 0 && IsListExtensionReceiver(g.Parameters[0].Type)),
                g => g.Name));

            foreach (var f in Compilation.Libraries.AllFunctions())
            {
                if (f.NumParameters == 0)
                    continue;

                // Array<T> implements IIndexable<T>, but Array is a runtime IReadOnlyList with no
                // generated struct, so the IIndexable library (IsValidIndex, At(IIndex),
                // LastIndex, First, Last, ...) reaches arrays only if it is written here as
                // extensions on IReadOnlyList (plato-456).
                var indexable = IsIndexableReceiver(f.Parameters[0].Type);
                if (indexable && arrayFunctionNames.Contains(f.Name))
                    continue;
                if (!indexable && !IsListExtensionReceiver(f.Parameters[0].Type)
                    && !IsRuntimeClassExtensionReceiver(f.Parameters[0].Type))
                    continue;

                // The array and builder INTRINSICS (Count/At/Reduce/FlatMap/..., Add/Set/Freeze/...)
                // are provided by the handwritten runtime; emitting a stub would shadow it. Any other bodiless
                // function on an array receiver gets the common throwing stub (plato-459).
                if (f.Body == null && IsHostIntrinsic(f))
                    continue;

                if (IgnoredFunctions.Contains(f.Name))
                    continue;

                // We need to fix this, we should be creating functions instances.
                var interfaceWriter = NewDefaultTypeWriter();
                var fi = new FunctionInstance(f, null, null, FunctionInstanceKind.InterfaceExtension);
                var cfi = new CSharpFunctionInfo(fi, null, interfaceWriter);
                if (indexable)
                {
                    cfi.RetargetReceiver(IndexableInterfaceName, "IReadOnlyList");
                    cfi.GenericizeInterfaceParameters("IIndex");
                }
                if (!emittedSignatures.Add(
                        $"{cfi.Name}{cfi.ExtensionGenericsString}({cfi.ParameterTypes.JoinStringsWithComma()})"))
                    continue;
                interfaceWriter.WriteExtensionFunction(cfi);
                if (ExtensionStyle)
                    this.WriteWithLineStateSync(interfaceWriter.ToString());
                else
                    Write(interfaceWriter.ToString());
            }
            WriteEndBlock();
            return this;
        }
        
        public CSharpWriter WriteConceptInterface(TypeDef type)
        {
            var tmp = new CSharpTypeWriter(this, type);
            tmp.WriteConceptInterface();
            return ExtensionStyle ? this.WriteWithLineStateSync(tmp.ToString()) : Write(tmp.ToString());
        }

        public CSharpWriter WriteConceptInterfaces()
        {
            foreach (var c in Compilation.AllTypeAndLibraryDefinitions)
                if (c.IsInterface())
                    WriteConceptInterface(c);
            return this;
        }

        public CSharpWriter WriteTypeImplementation(ConcreteType concreteType)
        {
            var tmp = new CSharpTypeWriter(this, concreteType.TypeDef);
            tmp.WriteConcreteType(concreteType);
            return ExtensionStyle ? this.WriteWithLineStateSync(tmp.ToString()) : Write(tmp.ToString());
        }
    }
}
