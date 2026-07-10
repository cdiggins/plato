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

        // Names the emitter must keep as no-arg struct PROPERTIES because handwritten code in
        // Plato.Intrinsics either accesses them with property syntax (AlmostZero in
        // Vector3_Extensions.AnyPerpendicular; Pow2/Pow3 in Number.Cubic/Linear/Quadratic) or
        // hand-implements them as a property the compiler cannot see
        // (Number.ReciprocalSquareRootEstimate). The compiler has no view into handwritten
        // usages, so these are pinned; moving any of them would break the Plato.Intrinsics
        // build (which is shared byte-for-byte with the default-mode SDK and cannot fork).
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

            // Every name that is (or may be accessed as) a no-arg instance PROPERTY somewhere.
            // The pinned handwritten-property-syntax names stay seeded under scalar erasure too:
            // handwritten intrinsics use property syntax on NON-scalar receivers as well
            // (Vector3_Extensions.AnyPerpendicular reads v.AlmostZero on a Vector3), so those
            // members must remain struct properties on every non-scalar type. On the erased
            // scalar types they become extension methods + the Number partial-struct shim.
            var keptNoArg = new HashSet<string>(HandwrittenPropertySyntaxNames);
            foreach (var p in ExtensionPlans.Values)
                keptNoArg.UnionWith(p.KeptNoArgPropertyNames);

            // Scalar erasure: record every member-function name of the five scalar types (all of
            // them become extension methods on the primitives; used for receiver-aware "()"),
            // and the subset whose scalar overloads all return scalars (receiver-chain analysis).
            if (ScalarErase)
            {
                ScalarMemberNames.UnionWith(HandwrittenScalarExtensionMethodNames);
                foreach (var kv in ExtensionPlans)
                {
                    if (!ScalarPrimitives.ContainsKey(kv.Key.Name))
                        continue;
                    ScalarMemberNames.UnionWith(kv.Value.NoArgMemberFunctionNames);
                    foreach (var sig in kv.Value.MemberFunctionSignatures)
                    {
                        if (!ScalarOverloads.TryGetValue(sig.Key, out var list))
                            ScalarOverloads[sig.Key] = list = new List<(IReadOnlyList<string>, string)>();
                        list.AddRange(sig.Value);
                    }
                }
            }

            // Interface-declared no-arg members are C# interface properties; call sites whose
            // receiver is interface-typed (or a constrained type variable) use property syntax,
            // so these names must keep property syntax everywhere.
            foreach (var t in Compilation.AllTypeAndLibraryDefinitions)
                if (t != null && t.IsInterface())
                    foreach (var m in t.Methods)
                        if (m.Function.NumParameters == 1)
                            keptNoArg.Add(m.Function.Name);

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

        // Scalar erasure only (empty otherwise): every member-function name of the five scalar
        // types. Under erasure these all exist as classic extension methods on the primitives,
        // so a no-arg access on a receiver the writer can prove scalar-valued must be written
        // with "()" even if the same name is a kept property on some non-scalar type.
        public HashSet<string> ScalarMemberNames { get; } = new HashSet<string>();

        // Scalar erasure only (empty otherwise): declared Plato signatures of the five scalar
        // types' member functions, by name. Drives the body writer's receiver/argument
        // analysis: a member call whose receiver and arguments are provably scalar binds an
        // all-scalar-parameter overload (exact match beats any conversion), so its result type
        // is that overload's (scalar) return - this lets the analysis chase chains like
        // "1f.Subtract(t).Sqr()".
        public Dictionary<string, List<(IReadOnlyList<string> Params, string Return)>> ScalarOverloads { get; }
            = new Dictionary<string, List<(IReadOnlyList<string>, string)>>();

        // Handwritten no-arg extension methods over the scalar primitives in Plato.Intrinsics
        // (compiler-invisible); their call sites need "()" on scalar receivers like everything
        // else, so they are folded into ScalarMemberNames under erasure.
        public static HashSet<string> HandwrittenScalarExtensionMethodNames = new HashSet<string>
        {
            "Range", // ArrayExtensions.Range(this int count)
            // Handwritten Number property invisible to the compiler; the scalar path emits a
            // hardwired float forwarder for it, so scalar call sites use method syntax.
            "ReciprocalSquareRootEstimate",
        };

        // The Elaborate → Monomorphize → Emit retarget, ON BY DEFAULT since increment 3: in the
        // pure DEFAULT C# style (no extension/scalar/optimize), every member-instance BODY with
        // Plato source is emitted from the monomorphized TIR through TirCSharpBodyWriter instead
        // of from the symbol graph through CSharpFunctionBodyWriter. The TIR covers all such
        // bodies (fallback count 0) and reproduces the previous writer byte-for-byte
        // (EmitDifferentialTests 1914/1914; EmitFlagOnTests 164/164 files; regen-plato.ps1 gates
        // the checked-in golden). Set false (or pass --no-tir to the CLI) to fall back to the
        // legacy symbol-graph body writer, which also still serves the non-default styles and
        // static/extension bodies.
        public bool UseTir = true;

        // UseTir measurement counters (no effect on output): member-instance bodies emitted from
        // the monomorphized TIR, and eligible bodies that fell back to the current writer because
        // no fully-ground TIR was available for that (function, concrete type).
        public int TirBodiesEmitted;
        public int TirFallbackBodies;

        // Lazily built the first time a ground TIR is requested (UseTir only): the fully-ground,
        // bodied monomorphized TIR for each (source FunctionDef, concrete type name), keyed exactly
        // as EmitDifferentialTests aligns them. Never built on the default path.
        private Dictionary<(FunctionDef, string), TirFunction> _groundTirByKey;

        /// <summary>The fully-ground monomorphized TIR body for a (source function, concrete type),
        /// or null when none exists (non-ground / unresolved / not bodied) — in which case the
        /// caller uses the current writer. Returns null unless <see cref="UseTir"/> is set, so the
        /// (expensive) monomorphization pass is never triggered on the default path.</summary>
        public TirFunction TryGetGroundTir(FunctionDef original, TypeDef concreteType)
        {
            if (!UseTir || original == null || concreteType == null)
                return null;
            if (_groundTirByKey == null)
                BuildGroundTirLookup();
            return _groundTirByKey.TryGetValue((original, concreteType.Name), out var tir) ? tir : null;
        }

        private void BuildGroundTirLookup()
        {
            _groundTirByKey = new Dictionary<(FunctionDef, string), TirFunction>();
            // MonomorphizeAll is total (never throws) and runs in the same shadow-mode passes the
            // checker tests exercise; it is only reached when UseTir is on.
            foreach (var m in new Monomorphizer(Compilation).MonomorphizeAll())
            {
                if (!m.HasBody || !m.IsFullyGround)
                    continue;
                var key = (m.Original, m.ConcreteType?.Name);
                if (m.Original != null && key.Item2 != null && !_groundTirByKey.ContainsKey(key))
                    _groundTirByKey[key] = m.Tir;
            }
        }

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


#if CHANGE_PRECISION
        public string OtherPrecisionFloatType;
        public string OtherPrecisionNamespace;
#endif

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
            FloatType = floatType;
            Namespace = floatType == "float"
                ? "Ara3D.Geometry"
                : floatType == "double"
                    ? "Ara3D.Geometry.DoublePrecision"
                    : throw new NotImplementedException("Only 'float' and 'double' are supported");
#if CHANGE_PRECISION
            OtherPrecisionFloatType = floatType == "float" ? "double" : "float";
            OtherPrecisionNamespace = floatType == "float" ? "Plato.DoublePrecision" : "Plato";
#endif 

            if (ExtensionStyle)
                BuildExtensionPlans();

            WriteFile("Interfaces.g.cs", WriteConceptInterfaces);
            WriteFile("Constants.g.cs", WriteConstantLibraryMethods);
            WriteFile("Extensions.g.cs", WriteInterfaceLibraryMethods);
            
            //WriteFile("Constructors.g.cs", WriteConstructors);

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

        public CSharpWriter WriteConstructors()
        {
            WriteLine($"public static class Constructors");
            WriteStartBlock();
            foreach (var ct in Compilation.ConcreteTypes)
            {
                // TODO: 
                // Write all constructors as a static extension method
            }
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