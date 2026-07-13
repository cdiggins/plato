using System;
using System.Collections.Generic;
using System.Text;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter
{
    public class CSharpWriter : CodeBuilder<CSharpWriter>
    {
        public CSharpWriter(Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            Analyzer = new PlatoAnalyzer(compilation);
            OutputFolder = outputFolder;
        }

        public string FloatType;
        public string Namespace;

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

        // Number members that handwritten Plato.Intrinsics code accesses on a Number receiver
        // (Number.Cubic/Linear/Quadratic use a.Pow2/a.Pow3; AnyPerpendicular reads v.AlmostZero).
        // Under --scalar=float the Number struct is not generated, so these bodied members are
        // emitted into the minimal `partial struct Number` SHIM (see WriteScalarErasedType) so the
        // handwritten call sites still bind. NOT a property-syntax pin (the V2 runtime exposes them
        // as methods and the shim emits them as such) — purely the shim-membership selector.
        public static HashSet<string> HandwrittenPropertySyntaxNames = new HashSet<string>
        {
            "AlmostZero",
            "Pow2",
            "Pow3",
            "ReciprocalSquareRootEstimate",
        };

        // Computes the extension-style plans and the global no-arg name partition. Must run
        // before ANY file is written: Constants.g.cs / Extensions.g.cs / struct-kept bodies all
        // contain call sites of moved members that need the "()" injection.
        private void BuildExtensionPlans()
        {
            ExtensionPlans = new Dictionary<TypeDef, ExtensionStylePlan>();
            foreach (var c in Compilation.ConcreteTypes)
                if (!IgnoredTypes.Contains(c.TypeDef.Name) && !c.TypeDef.IsUnique && !ExtensionPlans.ContainsKey(c.TypeDef))
                    ExtensionPlans[c.TypeDef] = new ExtensionStylePlan(this, c);

            // The struct-surface property-name set (the uniform rendering rule): every name that
            // renders with member/property syntax at a call site. Under --no-properties this is
            // exactly the per-type field + pseudo-field names (unioned below) plus the BCL
            // Count/NumColumns/NumRows obligations — no handwritten "pins" (the V2 runtime exposes
            // those members as methods).
            var keptNoArg = new HashSet<string>();
            foreach (var p in ExtensionPlans.Values)
                keptNoArg.UnionWith(p.KeptNoArgPropertyNames);

            // Property-ful (non-NoProperties) extension style only: interface-declared no-arg
            // members are C# interface properties, so their names keep property syntax everywhere.
            // Under --no-properties interfaces declare METHODS, so nothing is seeded here.
            foreach (var t in Compilation.AllTypeAndLibraryDefinitions)
                if (t != null && t.IsInterface())
                    foreach (var m in t.Methods)
                        if (m.Function.NumParameters == 1 && !NoProperties)
                            keptNoArg.Add(m.Function.Name);

            if (NoProperties)
            {
                // BCL/collection parity: Count (IReadOnlyCollection) and NumColumns/NumRows
                // (the handwritten Ara3D.Collections IReadOnlyList2D) are PROPERTIES on
                // receivers the emitter does not generate; generated code calls them on both
                // handwritten and generated receivers, and call-site syntax is decided by name.
                keptNoArg.Add("Count");
                keptNoArg.Add("NumColumns");
                keptNoArg.Add("NumRows");
                StructSurfacePropertyNames = keptNoArg;
            }

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

        // When true (--scalar=float, roadmap "Phase 2 revision" item 3), the five scalar wrapper
        // types (Number/Integer/Boolean/Character/String) are ERASED from the generated output in
        // favor of the native C# primitives (float/int/bool/char/string):
        //   - every generated signature, field, property, local and generic argument uses the
        //     primitive (IArray<Number> -> IReadOnlyList<float>);
        //   - the per-type files (_Number.g.cs, ...) no longer emit partial structs; they emit
        //     extension-method classes over the primitives (see CSharpConcreteTypeWriter);
        //   - generated bodies are normalized to "float-land": scalar-typed parameter references
        //     are written as ((float)x), calls to scalar-returning function groups are wrapped in
        //     ((float)...) casts, and no-arg member accesses on scalar receivers get "()" (all
        //     scalar members are classic extension methods on the primitives);
        //   - literals lose their wrapper casts: ((Number)0.5) becomes 0.5f.
        // Deliberate NON-erasure (the handwritten Plato.Intrinsics boundary, which cannot fork):
        //   - generated concept interfaces (Interfaces.g.cs) keep wrapper types, because
        //     handwritten intrinsic members (e.g. Vector3.Magnitude returning Number) satisfy
        //     interface obligations and their signatures cannot change;
        //   - struct-kept members of NON-scalar types (interface obligations, operators,
        //     scaffolding) keep wrapper signatures for the same reason; their bodies still
        //     compile because the wrappers convert implicitly both ways;
        //   - a tiny "public partial struct Number" shim keeps the pinned
        //     HandwrittenPropertySyntaxNames members as wrapper properties (handwritten
        //     intrinsics access them with property syntax on Number receivers).
        // Requires ExtensionStyle. Default (false) output is byte-identical to the original.
        public bool ScalarErase;

        // The scalar wrapper types --scalar=float erases, and their native primitives. This is
        // deliberately NOT CSharpWriter.PrimitiveTypes: Angle (and every other struct) remains a
        // real type - only the five scalar wrappers erase.
        public static readonly Dictionary<string, string> ScalarPrimitives = new Dictionary<string, string>
        {
            { "Number", "float" },
            { "Integer", "int" },
            { "Boolean", "bool" },
            { "Character", "char" },
            { "String", "string" },
        };

        // Count of member/static bodies emitted from the monomorphized TIR (Elaborate →
        // Monomorphize → Emit). The TIR is the SOLE C# body writer since the legacy
        // CSharpFunctionBodyWriter was retired (consolidation plan C4). Diagnostic only.
        public int TirBodiesEmitted;

        // --dump-tir=<dir> (null = off): write the per-phase TIR of every emitted body to
        // <dir>, one file per owner type. Records the elaborated/monomorphized INPUT and then
        // each optimizer pass (inline -> component-unroll -> array-materialize -> loop-lower)
        // that CHANGED the tree, so the dump shows exactly what each phase did. A development
        // aid for the optimizer passes; has no effect on the emitted C#.
        public string TirDumpDir;
        private readonly Dictionary<string, StringBuilder> _tirDumps = new Dictionary<string, StringBuilder>();

        // --inline-report (null = off): per-generation aggregation of inliner refusals + success
        // totals, printed after WriteAll. A diagnostic; no effect on emitted C#. See InlineReport.
        public InlineReport InlineReport;

        /// <summary>Applies the four optimizer TIR passes in order (inline → component-unroll →
        /// array-materialize → loop-lower) and returns the transformed function. Shared by every
        /// body-emit site so the pass pipeline is defined once. With <see cref="TirDumpDir"/> set,
        /// records each phase that changed the tree (see the field comment).</summary>
        public TirFunction RunOptimizerPasses(TirFunction tir, CSharpFunctionInfo fi, bool lowerScalars = true)
            => RunOptimizerPasses(tir, fi, lowerScalars, out _);

        /// <summary>As <see cref="RunOptimizerPasses(TirFunction, CSharpFunctionInfo, bool)"/>, also
        /// reporting via <paramref name="lowered"/> whether <see cref="TirScalarLowerer"/> actually RAN
        /// on this body. That real per-body signal is what tells <see cref="TirCSharpBodyWriter"/> to
        /// render type-directed — including a SCALAR-FREE body, which lowers to an (unchanged) tree with
        /// no erased primitive yet still renders type-directed (there being no legacy fallback).</summary>
        public TirFunction RunOptimizerPasses(TirFunction tir, CSharpFunctionInfo fi, bool lowerScalars, out bool lowered)
        {
            // Scalar lowering runs on every scalar-erased body IsGroundBody accepts — with static-body
            // lowering (lowerScalars is true at all emit sites now) that is every emitted body. The
            // `lowered` flag it reports is the printer's type-directed signal (no legacy fallback).
            bool ShouldLower(TirFunction t)
                => ScalarErase && lowerScalars && TirScalarLowerer.IsGroundBody(t);

            lowered = false;
            if (TirDumpDir == null)
            {
                tir = TirInliner.Inline(tir, this, fi?.OwnerType?.Name, out _);
                tir = TirComponentUnroller.UnrollFunction(tir, fi, this);
                tir = TirArrayMaterializer.Rewrite(tir, this);
                tir = TirLoopLowerer.Rewrite(tir, this);
                if (ShouldLower(tir)) { tir = TirScalarLowerer.LowerWithCoercions(tir); lowered = true; }
                return tir;
            }

            var key = SanitizeFileName(fi?.OwnerType?.Name ?? "Static");
            if (!_tirDumps.TryGetValue(key, out var sb))
                _tirDumps[key] = sb = new StringBuilder();
            var ps = fi?.ParameterTypes ?? (IReadOnlyList<string>)System.Array.Empty<string>();
            sb.AppendLine($"======== {fi?.ReturnType} {fi?.Name}({string.Join(", ", ps)}) ========");
            var prev = tir?.Body?.ToString() ?? "<null>";
            sb.AppendLine("-- input (elaborated / monomorphized) --");
            sb.AppendLine(prev);
            void Phase(string name, TirFunction t)
            {
                var s = t?.Body?.ToString() ?? "<null>";
                if (s != prev) { sb.AppendLine($"-- after {name} --"); sb.AppendLine(s); prev = s; }
            }
            tir = TirInliner.Inline(tir, this, fi?.OwnerType?.Name, out _); Phase("inline (layer 7)", tir);
            tir = TirComponentUnroller.UnrollFunction(tir, fi, this);  Phase("optimize / component-unroll (layer 8)", tir);
            tir = TirArrayMaterializer.Rewrite(tir, this);            Phase("optimize-arrays / materialize (layer 9)", tir);
            tir = TirLoopLowerer.Rewrite(tir, this);                  Phase("loops / lower (layer 10)", tir);
            if (ShouldLower(tir)) { tir = TirScalarLowerer.LowerWithCoercions(tir); lowered = true; Phase("scalar-lower (layer 10.5)", tir); }
            sb.AppendLine();
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

        /// <summary>The generic (unspecialized) TIR for a STATIC body — a constant or an IArray
        /// library function — or null when the function's elaboration has unresolved nodes.</summary>
        public TirFunction TryGetStaticTir(FunctionDef original)
            => TirSource.TryGetStaticTir(original);

        /// <summary>Name-keyed ground-TIR lookup for the inliner (--inline): the callee's body
        /// specialized for the receiver's solved concrete type name.</summary>
        public TirFunction TryGetGroundTirByTypeName(FunctionDef original, string concreteTypeName)
            => TirSource.TryGetGroundTir(original, concreteTypeName);

        /// <summary>Test helper: the ground input TIR for a (concrete type name, function name),
        /// for in-proc inliner shape tests (M0).</summary>
        public TirFunction TestGetGroundTir(string concreteTypeName, string functionName)
            => TirSource.TryGetGroundTirByNames(concreteTypeName, functionName);

        // The single property/method flag (--no-properties / --methods, unified at C4). When true,
        // the generated output declares NO C# properties or indexers — the shipping V2 recipe:
        //   - concept interfaces (Interfaces.g.cs) declare no-arg obligations as METHODS with
        //     scalar-ERASED signatures (bool Closed(); float Eval(float x); ...);
        //   - struct-KEPT members (obligations, conversions, statics, stubs) emit as methods with
        //     erased signatures; struct indexers are dropped (At() remains);
        //   - the IArrayLike scaffolding (NumComponents/Components) and nullary constants
        //     (Constants.Pi()) become methods; call sites get "()" accordingly.
        // The UNIFORM RENDERING RULE decides call-site syntax: a member renders as member/property
        // syntax iff its name is on the struct surface (a genuine field, a primitive pseudo-field
        // X/Y/Z/M11..., or a BCL Count/NumRows/NumColumns obligation — see StructSurfacePropertyNames);
        // every other member is a method call. Requires ExtensionStyle + ScalarErase.
        // (Historically two flags — MethodsOnly kept the primitive handwritten members as
        // properties, NoProperties erased them too; the weaker MethodsOnly variant was retired
        // with the default style at C4, leaving this one flag.)
        public bool NoProperties;

        // The UNIFORM RENDERING RULE, realized as a global name set: names that keep PROPERTY/field
        // access syntax at call sites — struct fields, the primitive pseudo-fields (X/Y/Z, M11...),
        // and the BCL Count/NumRows/NumColumns obligations. Every other no-arg member gets "()".
        // Decided globally by name (never per-receiver): the moved/kept partition demotes any name
        // that is a property on ANY generated type back into the struct on ALL types, so a name is
        // uniformly a property or uniformly a method across the whole output. Built in
        // BuildExtensionPlans; consulted by the body writer and CSharpFunctionInfo.EmitAsMethod.
        public HashSet<string> StructSurfacePropertyNames { get; private set; }

        // When true (--loops), recognized array-combinator call sites (Map/Zip/Reduce/All/Any/
        // Reverse/WithNext/MapPairs/... on one-dimensional list receivers) are lowered to
        // for-loop statements filling materialized arrays (see TirLoopLowerer). Runs after the
        // other optimizer passes. Default (false) output is unchanged.
        public bool LowerLoops;

        // When true (--inline, roadmap P3.2 "beta reduction"), resolved calls to small library
        // functions are INLINED at emission: the callee's fully-ground TIR body is substituted at
        // the call site (parameters replaced by argument nodes), iteratively, before the
        // component unroller and array materializer run — so HOF call sites hidden inside callees
        // become visible to those transforms. See TirInliner for the eligibility rules.
        // Default (false) output is unchanged.
        public bool InlineCalls;

        // When true (--optimize-arrays, optimizer stage 2 increment 1), Map/MapRange call sites in
        // MATERIALIZATION positions (constructor / tuple arguments — results stored into structs —
        // and multi-referenced local bindings) are rewritten to the eager MapEager/MapRangeEager
        // intrinsics, so multi-consumed pipelines evaluate each element once instead of per access
        // (see TirArrayMaterializer + docs/optimizer-stage2-plan.md). TIR-path only; sound for the
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

        public static HashSet<string> IgnoredTypes = new HashSet<string>()
        {
            "Dynamic",
            "Array",
            "Array2D",
            "Array3D",
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
            // These are functions of IArrayLike
            "Components",
            "CreateFromComponents",
            "CreateFromComponent",
            "NumComponents",
            
            // Implemented elswehere
            "Range",
            "MakeArray2D",
            "MapRange",
        };

        public static Dictionary<string, string> PrimitiveTypes = new Dictionary<string, string>()
        {
            { "Number", "float" },
            { "Boolean", "bool" },
            { "Integer", "int" },
            { "Character", "char" },
            { "String", "string" },
            { "Dynamic", "object" },
            { "Type", "System.Type" },
            { "Function0", "System.Func" },
            { "Function1", "System.Func" },
            { "Function2", "System.Func" },
            { "Function3", "System.Func" },
            { "Function4", "System.Func" },
            { "Function5", "System.Func" },
            { "Function6", "System.Func" },
            { "Function7", "System.Func" },
            { "Function8", "System.Func" },
            { "Function9", "System.Func" },
            { "Angle", "float" },
            { "Matrix3x2", "System.Numerics.Matrix3x2" },
            { "Matrix4x4", "System.Numerics.Matrix4x4" },
            { "Quaternion", "System.Numerics.Quaternion" },
            { "Plane", "System.Numerics.Plane" },
            { "Vector2", "System.Numerics.Vector2" },
            { "Vector3", "System.Numerics.Vector3" },
            { "Vector4", "System.Numerics.Vector4" },
            { "Vector8", "System.Runtime.Intrinsics.Vector256<float>" }
        };

        public CSharpWriter WriteFile(FilePath fileName, Func<CSharpWriter> f)
        {
            StartNewFile(fileName);
            WriteLine($"// Autogenerated file: DO NOT EDIT");
            WriteLine($"// Created on {DateTime.Now}");
            WriteLine();
            WriteLine("using System.Runtime.CompilerServices;");
            WriteLine("using System.Runtime.Serialization;");
            WriteLine("using System.Runtime.InteropServices;");
            WriteLine("using static System.Runtime.CompilerServices.MethodImplOptions;");
            WriteLine("using Ara3D.Collections;");
            WriteLine("");
            WriteLine($"namespace {Namespace}");
            WriteStartBlock();
            f();
            WriteEndBlock();
            return this;
        }

        public CSharpWriter WriteAll(string floatType)
        {
            // The lambda-capture hoist names (`_var{N}`) and the inliner's collision-rename
            // names (`{p}_{N}`) draw from process-global counters; reset them per generation so
            // two WriteAll runs in one process produce identical output (a fresh CLI process
            // always started at 0, so file output is unchanged).
            SymbolRewriter.NextId = 0;
            TirInliner.NextRenameId = 0;
            TirLoopLowerer.NextId = 0;

            FloatType = floatType;
            Namespace = floatType == "float"
                ? "Ara3D.Geometry"
                : floatType == "double"
                    ? "Ara3D.Geometry.DoublePrecision"
                    : throw new NotImplementedException("Only 'float' and 'double' are supported");

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

            FlushTirDumps();
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

        public CSharpWriter WriteInterfaceLibraryMethods()
        {
            WriteLine($"public static partial class Extensions");
            WriteStartBlock();
            foreach (var f in Compilation.Libraries.AllFunctions())
            {
                if (f.NumParameters > 0)
                {
                    var pt = f.Parameters[0].Type;
                    if (!pt.Def.IsInterface())
                        continue;

                    // We are going to skip functions that do not have a body
                    if (f.Body == null)
                        continue;

                    if (!pt.Def.Name.StartsWith("IArray") || pt.Def.Name.StartsWith("IArrayLike"))
                        continue;

                    // We need to fix this, we should be creating functions instances.
                    var interfaceWriter = NewDefaultTypeWriter();
                    var fi = new FunctionInstance(f, null, null, FunctionInstanceKind.InterfaceExtension);
                    var cfi = new CSharpFunctionInfo(fi, null, interfaceWriter);
                    interfaceWriter.WriteExtensionFunction(cfi);
                    if (ExtensionStyle)
                        this.WriteWithLineStateSync(interfaceWriter.ToString());
                    else
                        Write(interfaceWriter.ToString());
                }
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