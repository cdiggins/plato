using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Utils;

namespace Ara3D.Geometry.CppWriter
{
    /// <summary>
    /// Thrown by the body writer when a TIR node cannot be represented in the C++ family
    /// output (lambdas, function values, dynamic arrays, strings, ...). The function is then
    /// skipped and the reason recorded in <see cref="CppWriter.Skipped"/>.
    /// </summary>
    public class CppUnsupportedException : Exception
    {
        public CppUnsupportedException(string message) : base(message) { }
    }

    /// <summary>
    /// Proof-of-concept C++ / CUDA backend. TIR-only: every body renders from the
    /// monomorphized Typed IR, there is no legacy symbol-graph fallback.
    ///
    /// Output model:
    /// - Number -> float, Integer -> int, Boolean -> bool. (float, not double: CUDA device
    ///   code is float-first and the two dialects must agree.)
    /// - Vector2D/3D/4D map to float2/float3/float4 — the CUDA vector types, which the C++
    ///   preamble redeclares layout-compatibly. X/Y/Z/W field access becomes .x/.y/.z/.w.
    /// - Every other non-generic concrete type becomes an aggregate struct, constructed
    ///   positionally with braced init (T{a, b}), which matches TirNew exactly. Each struct
    ///   also gets field-wise operator==/operator!= so Plato equality lowers to ==.
    /// - Every Plato function becomes a FREE function (v.Length -> Length(v)). C++ overloads
    ///   by parameter type, so Add(float2,float2) and Add(float3,float3) coexist.
    /// - Constants become zero-argument functions (Pi()).
    /// - Bodiless intrinsics lower onto &lt;cmath&gt; and the plato:: helpers in the preamble.
    ///
    /// The emitted code is IDENTICAL for both dialects; only the preamble differs (see
    /// <see cref="CppPrelude"/>).
    ///
    /// Not representable (functions using these are skipped, with the reason recorded as a
    /// trailing comment block in the output): lambdas / function values, IArray and array
    /// literals, String / Character, and generic functions that were not monomorphized.
    /// </summary>
    public class CppWriter : CodeBuilder<CppWriter>, ITirInlineHost
    {
        public CppWriter(Compiler.Compilation compilation, DirectoryPath outputFolder, CppDialect dialect)
        {
            Compilation = compilation;
            Analyzer = new PlatoAnalyzer(compilation);
            OutputFolder = outputFolder;
            Dialect = dialect;

            foreach (var ct in compilation.ConcreteTypes)
            {
                var fields = new HashSet<string>();
                foreach (var f in ct.TypeDef.Fields)
                {
                    AllFieldNames.Add(f.Name);
                    fields.Add(f.Name);
                }
                FieldsByType[ct.TypeDef.Name] = fields;
            }
        }

        public Compiler.Compilation Compilation { get; }
        public PlatoAnalyzer Analyzer { get; }
        public DirectoryPath OutputFolder { get; }
        public CppDialect Dialect { get; }
        public Dictionary<string, StringBuilder> Files { get; } = new Dictionary<string, StringBuilder>();

        /// <summary>When true, run shared <see cref="TirInliner"/> before body emit (CLI <c>--inline</c>).</summary>
        public bool InlineCalls { get; set; }

        /// <summary>Optional inliner diagnostics; null means off.</summary>
        public InlineReport InlineReport { get; set; }

        public TirFunction TryGetGroundTirByTypeName(FunctionDef original, string concreteTypeName)
            => TirSource.TryGetGroundTir(original, concreteTypeName);

        public TirFunction TryGetStaticTir(FunctionDef original)
            => TirSource.TryGetStaticTir(original);

        public bool IsConcreteTypeName(string name)
            => name != null
               && (NativePrimitives.ContainsKey(name)
                   || NativeVectors.ContainsKey(name)
                   || StructNames.Contains(name)
                   || _fixedArrays.ContainsKey(name));

        public bool IsScalarPrimitiveName(string name)
            => name != null && NativePrimitives.ContainsKey(name);

        /// <summary>Null = trust all member names (no C# extension plan in this backend).</summary>
        public ISet<string> TryGetKnownMemberNames(string typeName)
            => null;

        /// <summary>Every declared field name: used to tell field access from calls.</summary>
        public HashSet<string> AllFieldNames { get; } = new HashSet<string>();

        /// <summary>Field names per Plato type: a member read is only a field read on its OWN type.</summary>
        public Dictionary<string, HashSet<string>> FieldsByType { get; } = new Dictionary<string, HashSet<string>>();

        /// <summary>Constants, which emit as zero-argument functions; a bare reference needs the "()".</summary>
        public HashSet<string> ConstantNames { get; } = new HashSet<string>();

        /// <summary>True when reading <paramref name="member"/> off a value of this Plato type is a field read.</summary>
        public bool IsFieldOf(string platoType, string member)
        {
            if (platoType != null && NativeVectors.ContainsKey(platoType))
                return member == "X" || member == "Y" || member == "Z" || member == "W";
            return platoType != null
                   && FieldsByType.TryGetValue(platoType, out var fields)
                   && fields.Contains(member);
        }

        /// <summary>Names of the structs actually emitted (types outside this set and the native maps are unsupported).</summary>
        public HashSet<string> StructNames { get; } = new HashSet<string>();

        /// <summary>"Owner.Function: reason" for everything that could not be emitted.</summary>
        public List<string> Skipped { get; } = new List<string>();

        public int FunctionsEmitted;

        private TirEmitSource _tirSource;
        private TirEmitSource TirSource
            => _tirSource ?? (_tirSource = new TirEmitSource(Compilation));

        /// <summary>One emitted free function: its prototype, its definition, and the callees it needs.</summary>
        private class Emitted
        {
            public string Label;
            public string Key;          // Name(paramType, ...) — what a call site must resolve to
            public string Prototype;
            public string Definition;
            public List<CallSite> Callees = new List<CallSite>();
        }

        /// <summary>A call the body writer emitted, recorded so it can be resolved against the emitted set.</summary>
        public class CallSite
        {
            public string Name;
            /// <summary>Argument types in C++ terms; a null entry means the type is not representable.</summary>
            public List<string> Types;
            public override string ToString() => SignatureKey(Name, Types);
        }

        private readonly List<Emitted> _emitted = new List<Emitted>();
        private readonly HashSet<string> _claimedSignatures = new HashSet<string>();

        // ---- Type mapping -------------------------------------------------------

        public static Dictionary<string, string> NativePrimitives = new Dictionary<string, string>()
        {
            { "Number", "float" },
            { "Integer", "int" },
            { "Boolean", "bool" },
        };

        public static Dictionary<string, string> NativeVectors = new Dictionary<string, string>()
        {
            { "Vector2D", "float2" },
            { "Vector3D", "float3" },
            { "Vector4D", "float4" },
            { "Vector2", "float2" },
            { "Vector3", "float3" },
            { "Vector4", "float4" },
        };

        public static HashSet<string> IgnoredTypes = new HashSet<string>()
        {
            "Dynamic", "Type", "Error", "String", "Character",
            "Array", "Array2D", "Array3D",
        };

        public static HashSet<string> IgnoredFunctions = new HashSet<string>()
        {
            // Reflection that needs String / dynamic IArray (M3/M4) — not emitted yet.
            "FieldNames", "FieldValues", "TypeName", "ToString", "GetType",
            // CreateFrom* needs an IArray value type (M4); Components is generated for IArrayLike.
            "CreateFromComponents", "CreateFromComponent",
            "Range", "MakeArray2D", "MapRange",
        };

        /// <summary>Fixed-size array PODs synthesized for <c>Components</c> returns (not Plato Array).</summary>
        private readonly Dictionary<string, (string Elem, int N)> _fixedArrays = new Dictionary<string, (string, int)>();

        public static HashSet<string> CppKeywords = new HashSet<string>()
        {
            "alignas", "alignof", "and", "and_eq", "asm", "auto", "bitand", "bitor", "bool",
            "break", "case", "catch", "char", "char8_t", "char16_t", "char32_t", "class",
            "compl", "concept", "const", "consteval", "constexpr", "constinit", "const_cast",
            "continue", "co_await", "co_return", "co_yield", "decltype", "default", "delete",
            "do", "double", "dynamic_cast", "else", "enum", "explicit", "export", "extern",
            "false", "float", "for", "friend", "goto", "if", "inline", "int", "long",
            "mutable", "namespace", "new", "noexcept", "not", "not_eq", "nullptr", "operator",
            "or", "or_eq", "private", "protected", "public", "register", "reinterpret_cast",
            "requires", "return", "short", "signed", "sizeof", "static", "static_assert",
            "static_cast", "struct", "switch", "template", "this", "thread_local", "throw",
            "true", "try", "typedef", "typeid", "typename", "union", "unsigned", "using",
            "virtual", "void", "volatile", "wchar_t", "while", "xor", "xor_eq",
            // CUDA and preamble identifiers we must not shadow.
            "main", "plato", "PLATO_FN", "float2", "float3", "float4",
            "make_float2", "make_float3", "make_float4",
        };

        public static string EscapeName(string name)
            => CppKeywords.Contains(name) ? name + "_" : name;

        /// <summary>
        /// The C++ name of a Plato free function. A function whose name is also an emitted
        /// struct name (the type-named conversions: <c>Matrix4x4(Transform3D)</c>) would hide
        /// the type, so later uses of the type as a parameter would not name a type at all.
        /// Those become <c>ToMatrix4x4</c>. Call sites go through the same mapping.
        /// </summary>
        public string FunctionName(string name)
            => StructNames.Contains(name) ? "To" + EscapeName(name) : EscapeName(name);

        /// <summary>True when a type name is a generated tuple or function type (unsupported).</summary>
        private static bool IsTupleOrFunctionType(string name)
            => (name.StartsWith("Tuple") || name.StartsWith("Function"))
               && name.Length > 5 && char.IsDigit(name[name.Length - 1]);

        public string CppTypeNameOrNull(TypeExpression te)
            => te == null ? null : CppNameFor(TypeInstance.Create(te).Name);

        public string CppTypeNameOrNull(TypeInstance ti)
            => ti == null ? null : CppNameFor(ti.Name);

        private string CppNameFor(string name)
        {
            if (NativePrimitives.TryGetValue(name, out var prim))
                return prim;
            if (NativeVectors.TryGetValue(name, out var vec))
                return vec;
            if (StructNames.Contains(name))
                return name;
            return null;
        }

        public string CppTypeName(TypeExpression te)
            => CppTypeNameOrNull(te)
               ?? throw new CppUnsupportedException($"type '{(te == null ? "?" : TypeInstance.Create(te).Name)}' is not representable in {Dialect.DisplayName()}");

        public string CppTypeName(TypeInstance ti)
            => CppTypeNameOrNull(ti)
               ?? throw new CppUnsupportedException($"type '{ti?.Name ?? "?"}' is not representable in {Dialect.DisplayName()}");

        /// <summary>The Plato-level type name of a TIR node (for field-name mapping).</summary>
        public static string PlatoTypeName(TypeExpression te)
            => te == null ? null : TypeInstance.Create(te).Name;

        /// <summary>Maps a field access: X/Y/Z/W become .x/.y/.z/.w on the native vector types.</summary>
        public string MapFieldName(string receiverPlatoType, string fieldName)
        {
            if (receiverPlatoType != null && NativeVectors.ContainsKey(receiverPlatoType))
            {
                switch (fieldName)
                {
                    case "X": return "x";
                    case "Y": return "y";
                    case "Z": return "z";
                    case "W": return "w";
                }
            }
            return EscapeName(fieldName);
        }

        public static bool IsScalar(string cppType)
            => cppType == "float" || cppType == "int" || cppType == "bool";

        public static bool IsVector(string cppType)
            => cppType == "float2" || cppType == "float3" || cppType == "float4";

        public static IReadOnlyList<string> VectorFieldNames(string cppType)
            => cppType == "float2" ? new[] { "x", "y" }
                : cppType == "float3" ? new[] { "x", "y", "z" }
                : cppType == "float4" ? new[] { "x", "y", "z", "w" }
                : (IReadOnlyList<string>)Array.Empty<string>();

        /// <summary>
        /// Homogeneous float component accessors for a native vector or an emitted struct whose
        /// fields are all <c>float</c>. Used to lower Dot/Length/ops on Point2D-like types.
        /// </summary>
        public bool TryFloatComponents(string cppType, out IReadOnlyList<string> fields)
        {
            fields = VectorFieldNames(cppType);
            if (fields.Count > 0)
                return true;
            var st = _structTypes.FirstOrDefault(c => c.TypeDef.Name == cppType);
            if (st == null)
                return false;
            var names = new List<string>();
            foreach (var f in st.TypeDef.Fields)
            {
                if (CppTypeNameOrNull(f.Type) != "float")
                    return false;
                names.Add(EscapeName(f.Name));
            }
            if (names.Count == 0)
                return false;
            fields = names;
            return true;
        }

        public static string MakeFloatVector(int arity, IEnumerable<string> args)
            => $"make_float{arity}({args.JoinStringsWithComma()})";

        public string FloatVectorExpr(string expr, string cppType)
        {
            if (!TryFloatComponents(cppType, out var fields))
                return expr;
            if (IsVector(cppType))
                return expr;
            return MakeFloatVector(fields.Count, fields.Select(f => $"{expr}.{f}"));
        }

        public string StructFromFloatVector(string cppType, string floatExpr)
        {
            if (IsVector(cppType) || !TryFloatComponents(cppType, out var fields))
                return floatExpr;
            var sw = new[] { "x", "y", "z", "w" };
            var comps = fields.Select((_, i) => $"{floatExpr}.{sw[i]}");
            return $"{cppType}{{ {comps.JoinStringsWithComma()} }}";
        }

        /// <summary>
        /// True when <paramref name="structCppName"/> is an emitted struct with exactly one field
        /// whose C++ type is <paramref name="fieldCppType"/> (wrapper types like Translation3D).
        /// </summary>
        public bool TrySingleFieldOfType(string structCppName, string fieldCppType)
        {
            if (structCppName == null || fieldCppType == null || !StructNames.Contains(structCppName))
                return false;
            var st = _structTypes.FirstOrDefault(c => c.TypeDef.Name == structCppName);
            if (st == null || st.TypeDef.Fields.Count != 1)
                return false;
            return CppTypeNameOrNull(st.TypeDef.Fields[0].Type) == fieldCppType;
        }

        /// <summary>Default value expression for a C++ type (static-call type tags, etc.).</summary>
        public string DefaultValueExpr(string cppType)
        {
            switch (cppType)
            {
                case "float": return "0.0f";
                case "int": return "0";
                case "bool": return "false";
                case "float2": return "make_float2(0.0f, 0.0f)";
                case "float3": return "make_float3(0.0f, 0.0f, 0.0f)";
                case "float4": return "make_float4(0.0f, 0.0f, 0.0f, 0.0f)";
                default: return $"{cppType}{{}}";
            }
        }

        /// <summary>The key at which a call is resolved against the emitted set: name and parameter types.</summary>
        public static string SignatureKey(string name, IEnumerable<string> types)
            => $"{name}({types.Select(t => t ?? "?").JoinStringsWithComma()})";

        /// <summary>
        /// True when a recorded call site would find an emitted overload. Exact match first;
        /// failing that, the int-to-float promotion C++ would apply on its own is tried, so a
        /// Lerp(int, int, float) call still resolves to Lerp(float, float, float).
        /// </summary>
        private static bool Resolves(CallSite c, HashSet<string> known)
        {
            // Map/Zip/Reduce/All/Any and CreateFrom* are emitted as templates / structural
            // helpers; functor args have no C++ type, so they are not prune-tracked.
            if (UntrackedHofNames.Contains(c.Name))
                return true;
            if (c.Types.Any(t => t == null))
                return false;
            if (known.Contains(SignatureKey(c.Name, c.Types)))
                return true;
            var promoted = c.Types.Select(t => t == "int" ? "float" : t);
            return known.Contains(SignatureKey(c.Name, promoted));
        }

        /// <summary>Higher-order / structural helpers that accept functors or IArray stand-ins.</summary>
        public static readonly HashSet<string> UntrackedHofNames = new HashSet<string>
        {
            "Map", "Zip", "Reduce", "All", "Any",
            "CreateFromComponents", "CreateFromComponent",
        };

        // ---- Top level ----------------------------------------------------------

        public CppWriter WriteAll()
        {
            // Collision-rename ids are process-global; reset per generation for stable output.
            TirInliner.NextRenameId = 0;

            StartNewFile(Dialect.FileName());
            WriteLine($"// Autogenerated by Plato.CppWriter ({Dialect.DisplayName()}): DO NOT EDIT");
            WriteLine($"// Created on {DateTime.Now}");
            WriteTrimmed(CppPrelude.Preamble(Dialect));
            WriteLine();

            ComputeStructs();
            WriteStructs();
            WriteReflectionHelpers();
            CollectConstants();
            CollectMemberFunctions();
            WriteFixedArrayStructs();
            EmitNativeVectorHofTemplates();
            PruneUnresolvedCalls();

            WriteLine("// ---- Function prototypes ----");
            foreach (var e in _emitted)
                WriteLine(e.Prototype + ";");
            WriteLine();

            WriteLine("// ---- Function definitions ----");
            foreach (var e in _emitted)
            {
                WriteTrimmed(e.Definition);
                WriteLine();
            }

            FunctionsEmitted = _emitted.Count;

            if (Skipped.Count > 0)
            {
                WriteLine($"// ---- Skipped (not representable in {Dialect.DisplayName()}) ----");
                foreach (var s in Skipped)
                    WriteLine("// " + s);
            }
            return this;
        }

        public void StartNewFile(string fileName)
        {
            sb = new StringBuilder();
            Files.Add(fileName, sb);
        }

        public CppWriter WriteTrimmed(string s)
        {
            s = s.TrimEnd('\r', '\n');
            if (s.Length == 0)
                return this;
            return Write(s).WriteLine();
        }

        /// <summary>
        /// A function whose body calls something that was itself skipped would not compile.
        /// Drop those too, to a fixpoint. (Calls onto the preamble — operators, plato::
        /// helpers, struct construction — are never recorded as callees, so only Plato-level
        /// free functions participate.)
        /// </summary>
        private void PruneUnresolvedCalls()
        {
            var known = new HashSet<string>(_emitted.Select(e => e.Key));
            bool changed;
            do
            {
                changed = false;
                foreach (var e in _emitted.ToList())
                {
                    var missing = e.Callees.FirstOrDefault(c => !Resolves(c, known));
                    if (missing == null)
                        continue;
                    Skipped.Add($"{e.Label}: calls '{missing}', which is not emitted");
                    _emitted.Remove(e);
                    // An identical signature cannot be emitted twice, so the key is gone with it.
                    known.Remove(e.Key);
                    changed = true;
                }
            } while (changed);
        }

        // ---- Structs ------------------------------------------------------------

        private List<ConcreteType> _structTypes = new List<ConcreteType>();

        /// <summary>
        /// A concrete type becomes a struct when it is not natively mapped, not generic, and
        /// every field type is representable. Field support depends on which other structs
        /// survive, so iterate to a fixpoint, then order the survivors so that field types
        /// precede their users (C++ needs a complete type to declare a member).
        /// </summary>
        private void ComputeStructs()
        {
            var candidates = Compilation.ConcreteTypes
                .Where(c => !NativePrimitives.ContainsKey(c.TypeDef.Name)
                            && !NativeVectors.ContainsKey(c.TypeDef.Name)
                            && !IgnoredTypes.Contains(c.TypeDef.Name)
                            && !IsTupleOrFunctionType(c.TypeDef.Name)
                            && !c.TypeDef.Name.StartsWith("Function")
                            && c.TypeDef.TypeParameters.Count == 0
                            && c.TypeDef.Fields.Count > 0)
                .ToList();

            foreach (var c in candidates)
                StructNames.Add(c.TypeDef.Name);

            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var c in candidates.ToList())
                {
                    if (c.TypeDef.Fields.Any(f => CppTypeNameOrNull(f.Type) == null))
                    {
                        Skipped.Add($"type {c.TypeDef.Name}: field type not representable in {Dialect.DisplayName()}");
                        StructNames.Remove(c.TypeDef.Name);
                        candidates.Remove(c);
                        changed = true;
                    }
                }
            }

            // Topological order: emit a struct only after the structs its fields use.
            var emitted = new HashSet<string>();
            var ordered = new List<ConcreteType>();
            while (ordered.Count < candidates.Count)
            {
                var progressed = false;
                foreach (var c in candidates)
                {
                    if (emitted.Contains(c.TypeDef.Name))
                        continue;
                    var deps = c.TypeDef.Fields
                        .Select(f => PlatoTypeName(f.Type))
                        .Where(n => StructNames.Contains(n) && n != c.TypeDef.Name);
                    if (deps.All(emitted.Contains))
                    {
                        ordered.Add(c);
                        emitted.Add(c.TypeDef.Name);
                        progressed = true;
                    }
                }
                if (!progressed)
                    break; // cyclic field types: impossible by value; emit the rest anyway
            }
            foreach (var c in candidates)
                if (!emitted.Contains(c.TypeDef.Name))
                    ordered.Add(c);

            _structTypes = ordered;
        }

        private void WriteStructs()
        {
            WriteLine("// ---- Structs ----");
            foreach (var c in _structTypes)
            {
                WriteLine($"struct {c.TypeDef.Name}");
                WriteLine("{");
                IndentLevel++;
                foreach (var f in c.TypeDef.Fields)
                    WriteLine($"{CppTypeName(f.Type)} {EscapeName(f.Name)};");
                IndentLevel--;
                WriteLine("};");
                WriteLine();
            }

            // Field-wise equality, so Plato's Equals/NotEquals can lower to == / != on any
            // emitted type (C++ has no implicit structural equality; every field type either
            // is a native scalar or is itself an emitted struct with these operators).
            WriteLine("// ---- Structural equality ----");
            foreach (var c in _structTypes)
            {
                var n = c.TypeDef.Name;
                var cmp = c.TypeDef.Fields
                    .Select(f => $"a.{EscapeName(f.Name)} == b.{EscapeName(f.Name)}")
                    .JoinStrings(" && ");
                WriteLine($"PLATO_FN bool operator==({n} a, {n} b) {{ return {cmp}; }}");
                WriteLine($"PLATO_FN bool operator!=({n} a, {n} b) {{ return !(a == b); }}");
            }
            WriteLine();
        }

        private void WriteFixedArrayStructs()
        {
            if (_fixedArrays.Count == 0)
                return;
            WriteLine("// ---- Fixed-size Components return types ----");
            foreach (var kv in _fixedArrays.OrderBy(k => k.Key))
            {
                var name = kv.Key;
                var (elem, n) = kv.Value;
                WriteLine($"struct {name}");
                WriteLine("{");
                IndentLevel++;
                for (var i = 0; i < n; i++)
                    WriteLine($"{elem} e{i};");
                IndentLevel--;
                WriteLine("};");
                WriteLine();

                var chain = $"a.e{n - 1}";
                for (var i = n - 2; i >= 0; i--)
                    chain = $"(i <= {i} ? a.e{i} : {chain})";
                AddGenerated("At", new[] { name, "int" }, $"PLATO_FN {elem} At({name} a, int i)",
                    $" {{ return {chain}; }}");
                AddGenerated("Count", new[] { name }, $"PLATO_FN int Count({name} a)",
                    $" {{ return {n}; }}");
                AddGenerated("NumComponents", new[] { name }, $"PLATO_FN int NumComponents({name} a)",
                    $" {{ return {n}; }}");

                EmitHofTemplatesForFixedArray(name, elem, n);
            }
        }

        /// <summary>
        /// Map/Zip/Reduce/All/Any over float2/3/4 so residual lambdas on Vector2D-style
        /// Components can compile after --inline.
        /// </summary>
        private void EmitNativeVectorHofTemplates()
        {
            WriteLine("// ---- Map / Zip / Reduce / All / Any (floatN + functors) ----");
            EmitHofTemplatesForFloatN(2, "float2", new[] { "x", "y" });
            EmitHofTemplatesForFloatN(3, "float3", new[] { "x", "y", "z" });
            EmitHofTemplatesForFloatN(4, "float4", new[] { "x", "y", "z", "w" });
            WriteLine();
        }

        private void EmitHofTemplatesForFloatN(int n, string type, string[] fields)
        {
            var atChain = $"{type} a";
            // At with clamp
            var chain = $"a.{fields[n - 1]}";
            for (var i = n - 2; i >= 0; i--)
                chain = $"(i <= {i} ? a.{fields[i]} : {chain})";
            AddGenerated("At", new[] { type, "int" }, $"PLATO_FN float At({type} a, int i)",
                $" {{ return {chain}; }}");
            AddGenerated("Count", new[] { type }, $"PLATO_FN int Count({type} a)",
                $" {{ return {n}; }}");
            AddGenerated("NumComponents", new[] { type }, $"PLATO_FN int NumComponents({type} a)",
                $" {{ return {n}; }}");

            var mapArgs = fields.Select(f => $"f(xs.{f})").JoinStringsWithComma();
            AddTemplate(
                $"template <typename F> PLATO_FN {type} Map({type} xs, F f)",
                $" {{ return make_{type}({mapArgs}); }}");

            var zip2Args = fields.Select(f => $"f(a.{f}, b.{f})").JoinStringsWithComma();
            AddTemplate(
                $"template <typename F> PLATO_FN auto Zip({type} a, {type} b, F f) -> plato::Array{n}<decltype(f(a.{fields[0]}, b.{fields[0]}))>",
                $" {{ return plato::Array{n}<decltype(f(a.{fields[0]}, b.{fields[0]}))>{{ {zip2Args} }}; }}");

            var zip3Args = fields.Select(f => $"f(a.{f}, b.{f}, c.{f})").JoinStringsWithComma();
            AddTemplate(
                $"template <typename F> PLATO_FN auto Zip({type} a, {type} b, {type} c, F f) -> plato::Array{n}<decltype(f(a.{fields[0]}, b.{fields[0]}, c.{fields[0]}))>",
                $" {{ return plato::Array{n}<decltype(f(a.{fields[0]}, b.{fields[0]}, c.{fields[0]}))>{{ {zip3Args} }}; }}");

            var reduceBody = string.Join(" ", fields.Select(f => $"acc = f(acc, xs.{f});"));
            AddTemplate(
                $"template <typename Acc, typename F> PLATO_FN Acc Reduce({type} xs, Acc acc, F f)",
                $" {{ {reduceBody} return acc; }}");

            // All/Any over plato::ArrayN<T> (Zip→bool results) and over floatN (rare).
            var allArr = string.Join(" && ", Enumerable.Range(0, n).Select(i => $"f(xs.e{i})"));
            var anyArr = string.Join(" || ", Enumerable.Range(0, n).Select(i => $"f(xs.e{i})"));
            AddTemplate(
                $"template <typename T, typename F> PLATO_FN bool All(plato::Array{n}<T> xs, F f)",
                $" {{ return {allArr}; }}");
            AddTemplate(
                $"template <typename T, typename F> PLATO_FN bool Any(plato::Array{n}<T> xs, F f)",
                $" {{ return {anyArr}; }}");
            var allF = string.Join(" && ", fields.Select(f => $"f(xs.{f})"));
            var anyF = string.Join(" || ", fields.Select(f => $"f(xs.{f})"));
            AddTemplate(
                $"template <typename F> PLATO_FN bool All({type} xs, F f)",
                $" {{ return {allF}; }}");
            AddTemplate(
                $"template <typename F> PLATO_FN bool Any({type} xs, F f)",
                $" {{ return {anyF}; }}");

            var broadcast = fields.Select(_ => "x").JoinStringsWithComma();
            AddGenerated("CreateFromComponent", new[] { type, "float" },
                $"PLATO_FN {type} CreateFromComponent({type} _, float x)",
                $" {{ return make_{type}({broadcast}); }}");
            AddGenerated("CreateFromComponents", new[] { type, type },
                $"PLATO_FN {type} CreateFromComponents({type} _, {type} c)",
                $" {{ return c; }}");
            var fromArr = Enumerable.Range(0, n).Select(i => $"c.e{i}").JoinStringsWithComma();
            AddGenerated("CreateFromComponents", new[] { type, $"plato::Array{n}<float>" },
                $"PLATO_FN {type} CreateFromComponents({type} _, plato::Array{n}<float> c)",
                $" {{ return make_{type}({fromArr}); }}");
        }

        private void EmitHofTemplatesForFixedArray(string name, string elem, int n)
        {
            if (n < 1 || n > 8)
                return;
            var mapArgs = Enumerable.Range(0, n).Select(i => $"f(xs.e{i})").JoinStringsWithComma();
            AddTemplate(
                $"template <typename F> PLATO_FN {name} Map({name} xs, F f)",
                $" {{ return {name}{{ {mapArgs} }}; }}");

            var zip2Args = Enumerable.Range(0, n).Select(i => $"f(a.e{i}, b.e{i})").JoinStringsWithComma();
            AddTemplate(
                $"template <typename F> PLATO_FN auto Zip({name} a, {name} b, F f) -> plato::Array{n}<decltype(f(a.e0, b.e0))>",
                $" {{ return plato::Array{n}<decltype(f(a.e0, b.e0))>{{ {zip2Args} }}; }}");

            var zip3Args = Enumerable.Range(0, n).Select(i => $"f(a.e{i}, b.e{i}, c.e{i})").JoinStringsWithComma();
            AddTemplate(
                $"template <typename F> PLATO_FN auto Zip({name} a, {name} b, {name} c, F f) -> plato::Array{n}<decltype(f(a.e0, b.e0, c.e0))>",
                $" {{ return plato::Array{n}<decltype(f(a.e0, b.e0, c.e0))>{{ {zip3Args} }}; }}");

            var reduceBody = string.Join(" ", Enumerable.Range(0, n).Select(i => $"acc = f(acc, xs.e{i});"));
            AddTemplate(
                $"template <typename Acc, typename F> PLATO_FN Acc Reduce({name} xs, Acc acc, F f)",
                $" {{ {reduceBody} return acc; }}");

            var allArr = string.Join(" && ", Enumerable.Range(0, n).Select(i => $"f(xs.e{i})"));
            var anyArr = string.Join(" || ", Enumerable.Range(0, n).Select(i => $"f(xs.e{i})"));
            // All/Any on FixedArray itself (elem may be bool after Zip into FixedArray — rare)
            AddTemplate(
                $"template <typename F> PLATO_FN bool All({name} xs, F f)",
                $" {{ return {allArr}; }}");
            AddTemplate(
                $"template <typename F> PLATO_FN bool Any({name} xs, F f)",
                $" {{ return {anyArr}; }}");
            if (n >= 1 && n <= 8)
            {
                AddTemplate(
                    $"template <typename T, typename F> PLATO_FN bool All(plato::Array{n}<T> xs, F f)",
                    $" {{ return {allArr}; }}");
                AddTemplate(
                    $"template <typename T, typename F> PLATO_FN bool Any(plato::Array{n}<T> xs, F f)",
                    $" {{ return {anyArr}; }}");
            }
        }

        private readonly HashSet<string> _claimedTemplates = new HashSet<string>();

        private void AddTemplate(string signature, string bodyWithBraces)
        {
            if (!_claimedTemplates.Add(signature))
                return;
            // Templates are written eagerly into the file (before prototypes), not via _emitted,
            // so they are visible to every later free function. Track as known for diagnostics.
            WriteLine(signature + bodyWithBraces);
            WriteLine();
        }

        /// <summary>
        /// Equals/NotEquals forward to ==/!=; GetHashCode is a structural mix of field hashes.
        /// Emitted for every representable type so call sites that name these helpers compile.
        /// </summary>
        private void WriteReflectionHelpers()
        {
            WriteLine("// ---- Equals / NotEquals / GetHashCode ----");
            void EmitPair(string type, string hashBody)
            {
                var eqKey = SignatureKey("Equals", new[] { type, type });
                if (_claimedSignatures.Add($"Equals({type}, {type})"))
                {
                    var sig = $"PLATO_FN bool Equals({type} a, {type} b)";
                    Add($"generated.Equals", eqKey, sig, $"{sig} {{ return a == b; }}", null);
                }
                if (_claimedSignatures.Add($"NotEquals({type}, {type})"))
                {
                    var sig = $"PLATO_FN bool NotEquals({type} a, {type} b)";
                    Add($"generated.NotEquals", SignatureKey("NotEquals", new[] { type, type }),
                        sig, $"{sig} {{ return a != b; }}", null);
                }
                if (_claimedSignatures.Add($"GetHashCode({type})"))
                {
                    var sig = $"PLATO_FN int GetHashCode({type} a)";
                    Add($"generated.GetHashCode", SignatureKey("GetHashCode", new[] { type }),
                        sig, $"{sig} {{ return {hashBody}; }}", null);
                }
            }

            EmitPair("float", "plato::hash_float(a)");
            EmitPair("int", "a");
            EmitPair("bool", "(a ? 1 : 0)");
            EmitPair("float2", "plato::mix_hash(plato::hash_float(a.x), plato::hash_float(a.y))");
            EmitPair("float3", "plato::mix_hash(plato::mix_hash(plato::hash_float(a.x), plato::hash_float(a.y)), plato::hash_float(a.z))");
            EmitPair("float4", "plato::mix_hash(plato::mix_hash(plato::mix_hash(plato::hash_float(a.x), plato::hash_float(a.y)), plato::hash_float(a.z)), plato::hash_float(a.w))");

            foreach (var c in _structTypes)
            {
                var n = c.TypeDef.Name;
                var fields = c.TypeDef.Fields.Select(f => EscapeName(f.Name)).ToList();
                string hash;
                if (fields.Count == 0)
                    hash = "0";
                else if (fields.Count == 1)
                    hash = $"GetHashCode(a.{fields[0]})";
                else
                {
                    hash = $"GetHashCode(a.{fields[0]})";
                    for (var i = 1; i < fields.Count; i++)
                        hash = $"plato::mix_hash({hash}, GetHashCode(a.{fields[i]}))";
                }
                EmitPair(n, hash);
            }
            WriteLine();
        }

        // ---- Functions ----------------------------------------------------------

        private void CollectConstants()
        {
            foreach (var f in Compilation.Libraries.AllConstants())
            {
                var fi = new FunctionInstance(f, null, null, FunctionInstanceKind.Constant);
                var label = $"Constants.{fi.Name}";
                try
                {
                    var tir = TirSource.TryGetStaticTir(fi.Implementation);
                    if (tir == null)
                    {
                        Skipped.Add($"{label}: no TIR available");
                        continue;
                    }
                    tir = TirInliner.Inline(tir, this, null, out _);
                    var ret = tir.ZonkedReturnType != null
                        ? CppTypeName(tir.ZonkedReturnType)
                        : CppTypeName(fi.ReturnType);
                    var sig = $"PLATO_FN {ret} {FunctionName(fi.Name)}()";
                    if (!_claimedSignatures.Add($"{fi.Name}()"))
                        continue;
                    var body = new TirCppBodyWriter(this, tir);
                    Add(label, SignatureKey(fi.Name, new string[0]), sig, sig + body, body.Callees);
                    ConstantNames.Add(fi.Name);
                }
                catch (CppUnsupportedException e)
                {
                    Skipped.Add($"{label}: {e.Message}");
                }
            }
        }

        private bool SkipFunction(ConcreteType ct, FunctionInstance f)
        {
            if (ct.TypeDef.Fields.Any(fd => fd.Name == f.Name))
                return true;
            if (IgnoredFunctions.Contains(f.Name))
                return true;
            if (f.InterfaceName == "IArray" || f.InterfaceName == "Array")
                return true;
            // Do not implement functions that cast to themselves.
            if (f.Name == ct.TypeDef.Name)
                return true;
            return false;
        }

        private void CollectMemberFunctions()
        {
            foreach (var ct in Compilation.ConcreteTypes)
            {
                var name = ct.TypeDef.Name;
                if (IgnoredTypes.Contains(name) || IsTupleOrFunctionType(name) || name.StartsWith("Function"))
                    continue;
                var supported = NativePrimitives.ContainsKey(name)
                                || NativeVectors.ContainsKey(name)
                                || StructNames.Contains(name);
                if (!supported)
                    continue;

                // A concrete type implementing IArrayLike over N homogeneous fields is a
                // fixed-size array: its accessors are generated structurally from the fields,
                // because the stdlib bodies route through the dynamic IArray.
                var handled = EmitArrayLikeMembers(ct);

                foreach (var g in ct.InterfaceFunctionGroups)
                {
                    FunctionInstance f;
                    try { f = Analyzer.ChooseBestFunction(g, out _); }
                    catch (Exception) { continue; }
                    if (handled.Contains(f.Name)) continue;
                    if (!SkipFunction(ct, f))
                        TryEmitFunction(f, ct);
                }
                foreach (var f in ct.UnimplementedFunctions)
                {
                    if (handled.Contains(f.Name)) continue;
                    if (!SkipFunction(ct, f))
                        TryEmitFunction(f, ct);
                }
            }
        }

        /// <summary>
        /// Emit At/Count/NumComponents for a fixed-size IArrayLike struct directly from its
        /// fields. At(i) is an index chain that CLAMPS on an out-of-range index: device code
        /// has no exceptions, so clamping is the closest thing to a safe failure.
        /// Returns the member names handled here, so the normal path skips them.
        /// </summary>
        private HashSet<string> EmitArrayLikeMembers(ConcreteType ct)
        {
            var handled = new HashSet<string>();
            var name = ct.TypeDef.Name;
            if (!StructNames.Contains(name))
                return handled;
            if (!ct.AllInterfaces.Any(i => i.ToString().StartsWith("IArrayLike")
                                           || i.ToString().StartsWith("FixedArray")))
                return handled;

            var fields = ct.TypeDef.Fields.Select(f => EscapeName(f.Name)).ToList();
            var fieldCppTypes = ct.TypeDef.Fields.Select(f => CppTypeNameOrNull(f.Type)).ToList();
            if (fields.Count == 0 || fieldCppTypes.Any(t => t == null))
                return handled;
            var elem = fieldCppTypes[0];
            if (fieldCppTypes.Any(t => t != elem))
                return handled; // heterogeneous: not an array

            var n = fields.Count;

            var chain = "self." + fields[n - 1];
            for (var i = n - 2; i >= 0; i--)
                chain = $"(i <= {i} ? self.{fields[i]} : {chain})";
            AddGenerated("At", new[] { name, "int" }, $"PLATO_FN {elem} At({name} self, int i)", $" {{ return {chain}; }}");
            handled.Add("At");

            AddGenerated("Count", new[] { name }, $"PLATO_FN int Count({name} self)", $" {{ return {n}; }}");
            handled.Add("Count");
            AddGenerated("NumComponents", new[] { name }, $"PLATO_FN int NumComponents({name} self)", $" {{ return {n}; }}");
            handled.Add("NumComponents");

            // Components: fixed-size value (GLSL T[N]). float×2/3/4 → floatN; else a FixedArray POD.
            var comps = fields.Select(f => $"self.{f}").JoinStringsWithComma();
            string compsRet;
            string compsBody;
            if (elem == "float" && n >= 2 && n <= 4)
            {
                compsRet = $"float{n}";
                compsBody = $" {{ return make_float{n}({comps}); }}";
            }
            else
            {
                compsRet = FixedArrayTypeName(elem, n);
                var inits = fields.Select(f => $"self.{f}").JoinStringsWithComma();
                compsBody = $" {{ return {compsRet}{{ {inits} }}; }}";
            }
            AddGenerated("Components", new[] { name }, $"PLATO_FN {compsRet} Components({name} self)", compsBody);
            handled.Add("Components");

            // CreateFromComponents / CreateFromComponent: inverse of Components (fixed-size only).
            var fieldInitsFromFloat = elem == "float" && n >= 2 && n <= 4
                ? Enumerable.Range(0, n).Select(i => $"c.{(new[] { "x", "y", "z", "w" })[i]}").JoinStringsWithComma()
                : null;
            var fieldInitsFromFixed = Enumerable.Range(0, n).Select(i => $"c.e{i}").JoinStringsWithComma();
            var fieldInitsFromArray = fieldInitsFromFixed;
            var broadcast = Enumerable.Range(0, n).Select(_ => "x").JoinStringsWithComma();

            if (fieldInitsFromFloat != null)
            {
                AddGenerated("CreateFromComponents", new[] { name, compsRet },
                    $"PLATO_FN {name} CreateFromComponents({name} _, {compsRet} c)",
                    $" {{ return {name}{{ {fieldInitsFromFloat} }}; }}");
            }
            else
            {
                AddGenerated("CreateFromComponents", new[] { name, compsRet },
                    $"PLATO_FN {name} CreateFromComponents({name} _, {compsRet} c)",
                    $" {{ return {name}{{ {fieldInitsFromFixed} }}; }}");
            }
            // Zip→plato::ArrayN<elem> overload (Lerp / Map-style paths that don't stay on floatN).
            var arrayN = $"plato::Array{n}<{elem}>";
            AddGenerated("CreateFromComponents", new[] { name, arrayN },
                $"PLATO_FN {name} CreateFromComponents({name} _, {arrayN} c)",
                $" {{ return {name}{{ {fieldInitsFromArray} }}; }}");
            handled.Add("CreateFromComponents");

            AddGenerated("CreateFromComponent", new[] { name, elem },
                $"PLATO_FN {name} CreateFromComponent({name} _, {elem} x)",
                $" {{ return {name}{{ {broadcast} }}; }}");
            handled.Add("CreateFromComponent");

            return handled;
        }

        private string FixedArrayTypeName(string elem, int n)
        {
            var sanitized = elem.Replace("::", "_").Replace(" ", "_");
            var name = $"FixedArray_{n}_{sanitized}";
            if (!_fixedArrays.ContainsKey(name))
                _fixedArrays[name] = (elem, n);
            return name;
        }

        private void AddGenerated(string name, string[] paramTypes, string sig, string bodyWithBraces)
        {
            if (!_claimedSignatures.Add(sig))
                return;
            Add($"generated.{name}", SignatureKey(name, paramTypes), sig, sig + bodyWithBraces, null);
        }

        private void Add(string label, string key, string prototype, string definition, List<CallSite> callees)
        {
            _emitted.Add(new Emitted
            {
                Label = label,
                Key = key,
                Prototype = prototype,
                Definition = definition,
                Callees = callees ?? new List<CallSite>(),
            });
        }

        private void TryEmitFunction(FunctionInstance fi, ConcreteType ct)
        {
            var owner = ct.TypeDef.Name;
            var label = $"{owner}.{fi.Name}";
            try
            {
                if (fi.ParameterNames.Count == 0)
                    return; // constants are handled by CollectConstants

                // Static members (first param "_"): still emitted as free functions. The "_"
                // parameter stays in the signature so overloads like UnitX(Vector2) /
                // UnitX(Vector3) remain distinct; call sites pass a default-constructed tag.

                TirFunction tir = null;
                if (fi.Implementation?.Body != null)
                {
                    tir = TirSource.TryGetGroundTir(fi.Implementation, ct.TypeDef)
                          ?? TirSource.TryGetStaticTir(fi.Implementation);
                    if (tir == null)
                    {
                        Skipped.Add($"{label}: no ground TIR (unresolved generics?)");
                        return;
                    }
                    tir = TirInliner.Inline(tir, this, owner, out _);
                }

                var paramNames = (tir != null
                        ? tir.Parameters.Select(p => p.Name)
                        : fi.ParameterNames)
                    .Select(EscapeName).ToList();

                var useZonked = tir?.ZonkedParameterTypes != null
                                && tir.ZonkedParameterTypes.Count == paramNames.Count;
                var paramTypes = useZonked
                    ? tir.ZonkedParameterTypes.Select(CppTypeName).ToList()
                    : fi.ParameterTypes.Select(CppTypeName).ToList();

                if (paramTypes.Count != paramNames.Count)
                {
                    Skipped.Add($"{label}: parameter name/type count mismatch");
                    return;
                }

                var ret = tir?.ZonkedReturnType != null
                    ? CppTypeName(tir.ZonkedReturnType)
                    : CppTypeName(fi.ReturnType);
                var retPlato = tir?.ZonkedReturnType != null
                    ? PlatoTypeName(tir.ZonkedReturnType)
                    : fi.ReturnType?.Name;

                var sigKey = $"{fi.Name}({paramTypes.JoinStringsWithComma()})";
                if (!_claimedSignatures.Add(sigKey))
                    return; // identical overload already emitted (first wins)

                var parameters = paramTypes.Zip(paramNames, (t, n) => $"{t} {n}").JoinStringsWithComma();
                var sig = $"PLATO_FN {ret} {FunctionName(fi.Name)}({parameters})";

                string definition;
                List<CallSite> callees = null;
                if (tir == null)
                {
                    // Bodiless intrinsic: map onto <cmath> / the plato:: helpers when we know how.
                    if (!TryGetIntrinsicBody(owner, fi.Name, paramNames, paramTypes, ret, out var body, out var fullBody))
                    {
                        _claimedSignatures.Remove(sigKey);
                        Skipped.Add($"{label}: intrinsic without a {Dialect.DisplayName()} mapping");
                        return;
                    }
                    definition = fullBody ? $"{sig} {body}" : $"{sig} {{ return {body}; }}";
                }
                else
                {
                    var bodyWriter = new TirCppBodyWriter(this, tir, ret, retPlato);
                    definition = sig + bodyWriter;
                    callees = bodyWriter.Callees;
                }

                Add(label, SignatureKey(fi.Name, paramTypes), sig, definition, callees);
            }
            catch (CppUnsupportedException e)
            {
                Skipped.Add($"{label}: {e.Message}");
            }
        }

        // ---- Intrinsics ---------------------------------------------------------

        /// <summary>
        /// Native bodies for the bodiless Plato intrinsics (the C# writer relies on
        /// hand-written runtime structs for these). Everything here resolves against
        /// &lt;cmath&gt; or the plato:: helpers in the preamble, so it is valid in both
        /// dialects and in CUDA device code.
        /// </summary>
        /// <summary>
        /// The inverse-trig intrinsics return a radian value wrapped in the library's Angle
        /// type. Only valid when that type really was emitted as a single-field struct.
        /// </summary>
        private bool TryWrapAngle(string radians, string returnType, out string body)
        {
            body = null;
            if (returnType != "Angle" || !StructNames.Contains("Angle"))
                return false;
            body = $"Angle{{ {radians} }}";
            return true;
        }

        public bool TryGetIntrinsicBody(string owner, string name,
            IReadOnlyList<string> ps, IReadOnlyList<string> paramTypes, string returnType,
            out string body, out bool fullBody)
        {
            body = null;
            fullBody = false;
            var a = ps.Count > 0 ? ps[0] : null;
            var b = ps.Count > 1 ? ps[1] : null;

            var allNative = paramTypes.All(t => t == "float" || t == "int" || IsVector(t));
            var allScalarNumeric = paramTypes.All(t => t == "float" || t == "int");
            var allBool = paramTypes.All(t => t == "bool");
            IReadOnlyList<string> comps0 = null;
            var floatStruct = paramTypes.Count > 0
                              && TryFloatComponents(paramTypes[0], out comps0)
                              && comps0.Count >= 2 && comps0.Count <= 4
                              && paramTypes.All(t => t == paramTypes[0] || t == "float");
            var arity = floatStruct ? comps0.Count : 0;

            switch ($"{owner}.{name}")
            {
                case "Number.Sqrt": body = $"sqrtf({a})"; return true;
                case "Number.Abs": body = $"fabsf({a})"; return true;
                case "Number.Floor": body = $"floorf({a})"; return true;
                case "Number.Ceiling": body = $"ceilf({a})"; return true;
                case "Number.Round": body = $"roundf({a})"; return true;
                case "Number.Truncate": body = $"truncf({a})"; return true;
                case "Number.Exp": body = $"expf({a})"; return true;
                case "Number.Ln": body = $"logf({a})"; return true;
                case "Number.Log": body = $"(logf({a}) / logf({b}))"; return true;
                case "Number.Pow": body = $"powf({a}, {b})"; return true;
                case "Number.Acos": return TryWrapAngle($"acosf({a})", returnType, out body);
                case "Number.Asin": return TryWrapAngle($"asinf({a})", returnType, out body);
                case "Number.Atan": return TryWrapAngle($"atanf({a})", returnType, out body);
                case "Number.Compare":
                case "Integer.Compare":
                    body = $"({a} < {b} ? -1 : ({a} > {b} ? 1 : 0))"; return true;
                case "Integer.Divide": body = $"({a} / {b})"; return true;
                case "Integer.Modulo": body = $"({a} % {b})"; return true;
                case "Integer.ToNumber": body = $"float({a})"; return true;
                case "Angle.Cos": body = $"cosf({a}.Radians)"; return true;
                case "Angle.Sin": body = $"sinf({a}.Radians)"; return true;
                case "Angle.Tan": body = $"tanf({a}.Radians)"; return true;
            }

            // Owner-agnostic mappings. Native float/floatN first; float-field structs (Point2D…)
            // convert through make_floatN so Dot/Length/Normalize work without a C# runtime.
            if (allNative || floatStruct)
            switch (name)
            {
                case "Min" when ps.Count == 2:
                    return TryVectorishBinary("plato::min_", a, b, paramTypes, returnType, out body, out fullBody);
                case "Max" when ps.Count == 2:
                    return TryVectorishBinary("plato::max_", a, b, paramTypes, returnType, out body, out fullBody);
                case "Clamp" when ps.Count == 3:
                    return TryVectorishTernary("plato::clamp_", a, b, ps[2], paramTypes, returnType, out body, out fullBody);
                case "Lerp" when ps.Count == 3 && paramTypes[2] == "float":
                case "Mix" when ps.Count == 3 && paramTypes[2] == "float":
                    return TryVectorishTernary("plato::mix_", a, b, ps[2], paramTypes, returnType, out body, out fullBody);
                case "Step" when allScalarNumeric && ps.Count == 2: body = $"plato::step_({a}, {b})"; return true;
                case "SmoothStep" when allScalarNumeric && ps.Count == 3: body = $"plato::smoothstep_({a}, {b}, {ps[2]})"; return true;
                case "Saturate" when allScalarNumeric && ps.Count == 1: body = $"plato::saturate_({a})"; return true;
                case "Sign" when allScalarNumeric && ps.Count == 1: body = $"plato::sign_({a})"; return true;
                case "Fract" when allScalarNumeric && ps.Count == 1: body = $"plato::fract_({a})"; return true;
                case "Sqrt" when allScalarNumeric && ps.Count == 1: body = $"sqrtf({a})"; return true;
                case "Abs" when allScalarNumeric && ps.Count == 1: body = $"fabsf({a})"; return true;
                case "Floor" when allScalarNumeric && ps.Count == 1: body = $"floorf({a})"; return true;
                case "Ceiling" when allScalarNumeric && ps.Count == 1: body = $"ceilf({a})"; return true;
                case "Length" when ps.Count == 1:
                case "Magnitude" when ps.Count == 1:
                    body = $"plato::length_({FloatVectorExpr(a, paramTypes[0])})"; return true;
                case "Dot" when ps.Count == 2:
                    body = $"plato::dot_({FloatVectorExpr(a, paramTypes[0])}, {FloatVectorExpr(b, paramTypes[1])})"; return true;
                case "Cross" when ps.Count == 2 && arity == 3:
                    return TryFloatResultToStruct($"plato::cross_({FloatVectorExpr(a, paramTypes[0])}, {FloatVectorExpr(b, paramTypes[1])})",
                        returnType, out body, out fullBody);
                case "Cross" when ps.Count == 2 && allNative:
                    body = $"plato::cross_({a}, {b})"; return true;
                case "Normalize" when ps.Count == 1:
                    return TryFloatResultToStruct($"plato::normalize_({FloatVectorExpr(a, paramTypes[0])})",
                        returnType, out body, out fullBody);
                case "Distance" when ps.Count == 2:
                    body = $"plato::distance_({FloatVectorExpr(a, paramTypes[0])}, {FloatVectorExpr(b, paramTypes[1])})"; return true;
                case "Reflect" when ps.Count == 2:
                    return TryFloatResultToStruct($"plato::reflect_({FloatVectorExpr(a, paramTypes[0])}, {FloatVectorExpr(b, paramTypes[1])})",
                        returnType, out body, out fullBody);
                case "Atan2" when allScalarNumeric && ps.Count == 2:
                    return TryWrapAngle($"atan2f({a}, {b})", returnType, out body);
            }

            // Componentwise Abs/Floor/… on float-field structs.
            if (floatStruct && ps.Count == 1 && returnType == paramTypes[0]
                && (name == "Abs" || name == "Floor" || name == "Ceiling" || name == "Sqrt"
                    || name == "Saturate" || name == "Sign" || name == "Fract"))
            {
                var fn = name == "Abs" ? "fabsf" : name == "Floor" ? "floorf" : name == "Ceiling" ? "ceilf"
                    : name == "Sqrt" ? "sqrtf" : name == "Saturate" ? "plato::saturate_"
                    : name == "Sign" ? "plato::sign_" : "plato::fract_";
                var parts = comps0.Select(f => $"{fn}({a}.{f})");
                body = $"{returnType}{{ {parts.JoinStringsWithComma()} }}";
                return true;
            }

            var op = ps.Count == 1 ? Operators.NameToUnaryOperator(name)
                : ps.Count == 2 ? Operators.NameToBinaryOperator(name) : null;
            if (op == null)
                return false;

            var vectorish = paramTypes.Any(IsVector)
                            && paramTypes.All(t => IsVector(t) || t == "float")
                            && paramTypes.Where(IsVector).Distinct().Count() == 1;
            var structOps = floatStruct && paramTypes.All(t => t == paramTypes[0] || t == "float");
            switch (op)
            {
                case "+": case "-": case "*": case "/":
                    if (!allScalarNumeric && !vectorish && !structOps) return false;
                    break;
                case "%":
                    if (!allScalarNumeric && !structOps) return false;
                    break;
                case "==": case "!=":
                    if (paramTypes.Distinct().Count() != 1) return false;
                    break;
                case "&&": case "||": case "!":
                    if (!allBool) return false;
                    break;
                default:
                    if (!allScalarNumeric) return false;
                    break;
            }

            if (structOps && (op == "+" || op == "-" || op == "*" || op == "/" || op == "%"))
            {
                if (ps.Count == 1)
                {
                    var parts = comps0.Select(f => $"({op}{a}.{f})");
                    body = $"{returnType}{{ {parts.JoinStringsWithComma()} }}";
                    return true;
                }
                IEnumerable<string> parts2;
                if (paramTypes[1] == "float")
                {
                    parts2 = op == "%"
                        ? comps0.Select(f => $"fmodf({a}.{f}, {b})")
                        : comps0.Select(f => $"({a}.{f} {op} {b})");
                }
                else if (paramTypes[0] == "float")
                {
                    parts2 = comps0.Select(f => $"({a} {op} {b}.{f})");
                }
                else
                {
                    parts2 = op == "%"
                        ? comps0.Select(f => $"fmodf({a}.{f}, {b}.{f})")
                        : comps0.Select(f => $"({a}.{f} {op} {b}.{f})");
                }
                body = $"{returnType}{{ {parts2.JoinStringsWithComma()} }}";
                return true;
            }

            if (ps.Count == 1)
            {
                body = $"({op}{a})";
                return true;
            }

            if (op == "%" && (owner == "Number" || paramTypes.Any(t => t == "float")))
            {
                body = $"fmodf({a}, {b})";
                return true;
            }
            body = $"({a} {op} {b})";
            return true;
        }

        private bool TryVectorishBinary(string fn, string a, string b,
            IReadOnlyList<string> paramTypes, string returnType, out string body, out bool fullBody)
        {
            if (IsVector(paramTypes[0]) || paramTypes[0] == "float" || paramTypes[0] == "int")
            {
                body = $"{fn}({a}, {b})";
                fullBody = false;
                return true;
            }
            return TryFloatResultToStruct($"{fn}({FloatVectorExpr(a, paramTypes[0])}, {FloatVectorExpr(b, paramTypes[1])})",
                returnType, out body, out fullBody);
        }

        private bool TryVectorishTernary(string fn, string a, string b, string c,
            IReadOnlyList<string> paramTypes, string returnType, out string body, out bool fullBody)
        {
            if (IsVector(paramTypes[0]) || paramTypes[0] == "float" || paramTypes[0] == "int")
            {
                body = $"{fn}({a}, {b}, {c})";
                fullBody = false;
                return true;
            }
            var args = paramTypes[2] == "float"
                ? $"{FloatVectorExpr(a, paramTypes[0])}, {FloatVectorExpr(b, paramTypes[1])}, {c}"
                : $"{FloatVectorExpr(a, paramTypes[0])}, {FloatVectorExpr(b, paramTypes[1])}, {FloatVectorExpr(c, paramTypes[2])}";
            return TryFloatResultToStruct($"{fn}({args})", returnType, out body, out fullBody);
        }

        private bool TryFloatResultToStruct(string floatExpr, string returnType, out string body, out bool fullBody)
        {
            fullBody = false;
            body = null;
            if (IsVector(returnType) || returnType == "float")
            {
                body = floatExpr;
                return true;
            }
            if (!TryFloatComponents(returnType, out var fields) || fields.Count < 2 || fields.Count > 4)
                return false;
            fullBody = true;
            var sw = new[] { "x", "y", "z", "w" };
            var comps = fields.Select((_, i) => $"_t.{sw[i]}").JoinStringsWithComma();
            body = $"{{ float{fields.Count} _t = {floatExpr}; return {returnType}{{ {comps} }}; }}";
            return true;
        }
    }
}
