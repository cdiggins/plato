using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
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
    public class CppWriter : CodeBuilder<CppWriter>
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
            "FieldNames", "FieldValues", "TypeName",
            "Equals", "NotEquals", "GetHashCode", "ToString", "GetType",
            // Components returns an array by value, which C/C++ cannot do without a wrapper.
            "Components", "CreateFromComponents", "CreateFromComponent",
            "Range", "MakeArray2D", "MapRange",
        };

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
            if (c.Types.Any(t => t == null))
                return false;
            if (known.Contains(SignatureKey(c.Name, c.Types)))
                return true;
            var promoted = c.Types.Select(t => t == "int" ? "float" : t);
            return known.Contains(SignatureKey(c.Name, promoted));
        }

        // ---- Top level ----------------------------------------------------------

        public CppWriter WriteAll()
        {
            StartNewFile(Dialect.FileName());
            WriteLine($"// Autogenerated by Plato.CppWriter ({Dialect.DisplayName()}): DO NOT EDIT");
            WriteLine($"// Created on {DateTime.Now}");
            WriteTrimmed(CppPrelude.Preamble(Dialect));
            WriteLine();

            ComputeStructs();
            WriteStructs();
            CollectConstants();
            CollectMemberFunctions();
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

            return handled;
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

                if (fi.ParameterNames[0] == "_")
                {
                    Skipped.Add($"{label}: static member functions not emitted (POC)");
                    return;
                }

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
                    if (!TryGetIntrinsicBody(owner, fi.Name, paramNames, paramTypes, ret, out var body))
                    {
                        _claimedSignatures.Remove(sigKey);
                        Skipped.Add($"{label}: intrinsic without a {Dialect.DisplayName()} mapping");
                        return;
                    }
                    definition = $"{sig} {{ return {body}; }}";
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
            IReadOnlyList<string> ps, IReadOnlyList<string> paramTypes, string returnType, out string body)
        {
            body = null;
            var a = ps.Count > 0 ? ps[0] : null;
            var b = ps.Count > 1 ? ps[1] : null;

            // The native lowerings below are only valid on the types <cmath>, the operators and
            // the plato:: helpers actually accept. An intrinsic over a user struct (Modulo on a
            // Point2D, Dot on a Point3D) has no native form and must be skipped instead.
            var allNative = paramTypes.All(t => t == "float" || t == "int" || IsVector(t));
            var allScalarNumeric = paramTypes.All(t => t == "float" || t == "int");
            var allBool = paramTypes.All(t => t == "bool");

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

            // Owner-agnostic mappings. The plato:: helpers are overloaded for float and the
            // vector types, so a bodiless Vector3.Normalize and a Number.Clamp both land here.
            if (allNative)
            switch (name)
            {
                case "Min" when ps.Count == 2: body = $"plato::min_({a}, {b})"; return true;
                case "Max" when ps.Count == 2: body = $"plato::max_({a}, {b})"; return true;
                case "Clamp" when ps.Count == 3: body = $"plato::clamp_({a}, {b}, {ps[2]})"; return true;
                case "Lerp" when ps.Count == 3 && paramTypes[2] == "float": body = $"plato::mix_({a}, {b}, {ps[2]})"; return true;
                case "Mix" when ps.Count == 3 && paramTypes[2] == "float": body = $"plato::mix_({a}, {b}, {ps[2]})"; return true;
                // The <cmath> lowerings below have no componentwise form: scalars only.
                case "Step" when allScalarNumeric && ps.Count == 2: body = $"plato::step_({a}, {b})"; return true;
                case "SmoothStep" when allScalarNumeric && ps.Count == 3: body = $"plato::smoothstep_({a}, {b}, {ps[2]})"; return true;
                case "Saturate" when allScalarNumeric && ps.Count == 1: body = $"plato::saturate_({a})"; return true;
                case "Sign" when allScalarNumeric && ps.Count == 1: body = $"plato::sign_({a})"; return true;
                case "Fract" when allScalarNumeric && ps.Count == 1: body = $"plato::fract_({a})"; return true;
                case "Sqrt" when allScalarNumeric && ps.Count == 1: body = $"sqrtf({a})"; return true;
                case "Abs" when allScalarNumeric && ps.Count == 1: body = $"fabsf({a})"; return true;
                case "Floor" when allScalarNumeric && ps.Count == 1: body = $"floorf({a})"; return true;
                case "Ceiling" when allScalarNumeric && ps.Count == 1: body = $"ceilf({a})"; return true;
                case "Length" when ps.Count == 1: body = $"plato::length_({a})"; return true;
                case "Magnitude" when ps.Count == 1: body = $"plato::length_({a})"; return true;
                case "Dot" when ps.Count == 2: body = $"plato::dot_({a}, {b})"; return true;
                case "Cross" when ps.Count == 2: body = $"plato::cross_({a}, {b})"; return true;
                case "Normalize" when ps.Count == 1: body = $"plato::normalize_({a})"; return true;
                case "Distance" when ps.Count == 2: body = $"plato::distance_({a}, {b})"; return true;
                case "Reflect" when ps.Count == 2: body = $"plato::reflect_({a}, {b})"; return true;
                case "Atan2" when ps.Count == 2: return TryWrapAngle($"atan2f({a}, {b})", returnType, out body);
            }

            // Operator-named intrinsics on the native scalars: native C++ operators.
            var op = ps.Count == 1 ? Operators.NameToUnaryOperator(name)
                : ps.Count == 2 ? Operators.NameToBinaryOperator(name) : null;
            if (op == null)
                return false;

            // The preamble defines vector arithmetic; comparison, modulo and logic stay scalar.
            var vectorish = paramTypes.Any(IsVector)
                            && paramTypes.All(t => IsVector(t) || t == "float")
                            && paramTypes.Where(IsVector).Distinct().Count() == 1;
            switch (op)
            {
                case "+": case "-": case "*": case "/":
                    if (!allScalarNumeric && !vectorish) return false;
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

            if (ps.Count == 1)
            {
                body = $"({op}{a})";
                return true;
            }

            // C++ % is integer only; float modulo is fmodf.
            if (op == "%" && owner == "Number")
            {
                body = $"fmodf({a}, {b})";
                return true;
            }
            body = $"({a} {op} {b})";
            return true;
        }
    }
}
