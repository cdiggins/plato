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

namespace Ara3D.Geometry.GlslWriter
{
    /// <summary>
    /// Thrown by the body writer when a TIR node cannot be represented in GLSL
    /// (lambdas, function values, dynamic arrays, strings, ...). The function is
    /// then skipped and the reason recorded in <see cref="GlslWriter.Skipped"/>.
    /// </summary>
    public class GlslUnsupportedException : Exception
    {
        public GlslUnsupportedException(string message) : base(message) { }
    }

    /// <summary>
    /// Proof-of-concept GLSL backend (target: GLSL ES 3.00 / #version 300 es).
    ///
    /// Output model — GLSL is a C-like language with no methods, no generics, no
    /// closures and no heap, which maps to Plato as follows:
    /// - Number -> float, Integer -> int, Boolean -> bool. (GLSL ES has no double:
    ///   Plato Numbers lose precision, see README.)
    /// - Angle -> float (radians): a fieldless primitive with no GLSL newtype to wrap it.
    /// - Vector2D/3D/4D map to the native vec2/vec3/vec4; field access X/Y/Z/W is
    ///   rewritten to the .x/.y/.z/.w swizzles, and constructors line up
    ///   positionally (new Vector3D(a,b,c) -> vec3(a,b,c)).
    /// - Every other non-generic concrete type becomes a GLSL struct; GLSL structs
    ///   come with a positional constructor for free, which matches TirNew exactly.
    /// - Every Plato function becomes a FREE function (GLSL has no methods):
    ///   v.Length becomes Length(v). GLSL supports overloading by parameter types,
    ///   so Add(vec2,vec2) and Add(vec3,vec3) coexist; only exact duplicate
    ///   signatures are skipped.
    /// - Operator-named calls on native scalars and vectors are inlined to native
    ///   operators (a + b); float Modulo becomes mod(a, b).
    /// - Constants become zero-argument functions (Pi()).
    /// - All function prototypes are emitted before all definitions, so
    ///   definition order never matters (GLSL requires declaration before use).
    ///
    /// Not representable (functions using these are skipped, with the reason
    /// recorded as a trailing comment block in the output):
    /// - Lambdas, function-typed values and parameters (no function pointers).
    /// - IArray and array literals (no dynamically sized arrays).
    /// - String / Character (no string type).
    /// - Generic functions that were not monomorphized to ground types.
    /// - Recursion is FORBIDDEN by GLSL but not yet detected here: the GLSL
    ///   compiler itself reports it.
    /// </summary>
    public class GlslWriter : CodeBuilder<GlslWriter>
    {
        public GlslWriter(Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            Compilation = compilation;
            Analyzer = new PlatoAnalyzer(compilation);
            OutputFolder = outputFolder;

            foreach (var ct in compilation.ConcreteTypes)
                foreach (var f in ct.TypeDef.Fields)
                    AllFieldNames.Add(f.Name);
        }

        public Compiler.Compilation Compilation { get; }
        public PlatoAnalyzer Analyzer { get; }
        public DirectoryPath OutputFolder { get; }
        public Dictionary<string, StringBuilder> Files { get; } = new Dictionary<string, StringBuilder>();

        /// <summary>Every declared field name: used to tell field access from calls.</summary>
        public HashSet<string> AllFieldNames { get; } = new HashSet<string>();

        /// <summary>Names of the structs actually emitted (types outside this set and the native maps are unsupported).</summary>
        public HashSet<string> StructNames { get; } = new HashSet<string>();

        /// <summary>"Owner.Function: reason" for everything that could not be emitted.</summary>
        public List<string> Skipped { get; } = new List<string>();

        public int FunctionsEmitted;

        private TirEmitSource _tirSource;
        private TirEmitSource TirSource
            => _tirSource ?? (_tirSource = new TirEmitSource(Compilation));

        // Collected output: signature -> definition text, in emission order.
        private readonly List<string> _prototypes = new List<string>();
        private readonly List<string> _definitions = new List<string>();
        private readonly HashSet<string> _claimedSignatures = new HashSet<string>();

        // ---- Type mapping -------------------------------------------------------

        // Angle is a fieldless compiler-known primitive stored as radians, so its GLSL
        // representation IS a float: GLSL has no newtypes, and a one-field struct would cost
        // an emitted free function for every +, *, and < (GLSL has no operator overloading).
        // The type distinction is therefore erased: overloads separated only by Angle vs
        // Number collapse to one signature (see the duplicate-signature skip in TryEmitFunction).
        public static Dictionary<string, string> NativePrimitives = new Dictionary<string, string>()
        {
            { "Number", "float" },
            { "Integer", "int" },
            { "Boolean", "bool" },
            { "Angle", "float" },
        };

        public static Dictionary<string, string> NativeVectors = new Dictionary<string, string>()
        {
            { "Vector2D", "vec2" },
            { "Vector3D", "vec3" },
            { "Vector4D", "vec4" },
            { "Vector2", "vec2" },
            { "Vector3", "vec3" },
            { "Vector4", "vec4" },
        };

        public static HashSet<string> IgnoredTypes = new HashSet<string>()
        {
            "Type", "Error", "String", "Character",
            "Array", "Array2D", "Array3D",
        };

        public static HashSet<string> IgnoredFunctions = new HashSet<string>()
        {
            "FieldNames", "FieldValues", "TypeName",
            "Equals", "NotEquals", "GetHashCode", "ToString", "GetType",
            "Components", "CreateFromComponents", "CreateFromComponent", "NumComponents",
            "Range", "MakeArray2D", "MapRange",
        };

        public static HashSet<string> GlslKeywords = new HashSet<string>()
        {
            "attribute", "uniform", "varying", "layout", "centroid", "flat", "smooth",
            "noperspective", "patch", "sample", "break", "continue", "do", "for", "while",
            "switch", "case", "default", "if", "else", "in", "out", "inout", "float",
            "double", "int", "void", "bool", "true", "false", "invariant", "precise",
            "discard", "return", "uint", "lowp", "mediump", "highp", "precision",
            "struct", "common", "partition", "active", "asm", "class", "union", "enum",
            "typedef", "template", "this", "resource", "goto", "inline", "noinline",
            "public", "static", "extern", "external", "interface", "long", "short",
            "half", "fixed", "unsigned", "superp", "input", "output", "filter",
            "sizeof", "cast", "namespace", "using", "main",
        };

        public static string EscapeName(string name)
            => GlslKeywords.Contains(name) || name.StartsWith("gl_") ? name + "_" : name;

        /// <summary>True when a type name is a generated tuple or function type (unsupported).</summary>
        private static bool IsTupleOrFunctionType(string name)
            => (name.StartsWith("Tuple") || name.StartsWith("Function"))
               && name.Length > 5 && char.IsDigit(name[name.Length - 1]);

        public string GlslTypeNameOrNull(TypeExpression te)
            => te == null ? null : GlslNameFor(TypeInstance.Create(te).Name);

        public string GlslTypeNameOrNull(TypeInstance ti)
            => ti == null ? null : GlslNameFor(ti.Name);

        private string GlslNameFor(string name)
        {
            if (NativePrimitives.TryGetValue(name, out var prim))
                return prim;
            if (NativeVectors.TryGetValue(name, out var vec))
                return vec;
            if (StructNames.Contains(name))
                return name;
            return null;
        }

        public string GlslTypeName(TypeExpression te)
            => GlslTypeNameOrNull(te)
               ?? throw new GlslUnsupportedException($"type '{(te == null ? "?" : TypeInstance.Create(te).Name)}' is not representable in GLSL");

        public string GlslTypeName(TypeInstance ti)
            => GlslTypeNameOrNull(ti)
               ?? throw new GlslUnsupportedException($"type '{ti?.Name ?? "?"}' is not representable in GLSL");

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

        public static bool IsScalar(string glslType)
            => glslType == "float" || glslType == "int" || glslType == "bool";

        public static bool IsVector(string glslType)
            => glslType == "vec2" || glslType == "vec3" || glslType == "vec4";

        // ---- Top level ----------------------------------------------------------

        public GlslWriter WriteAll()
        {
            StartNewFile("plato.glsl");
            WriteLine("#version 300 es");
            WriteLine("// Autogenerated by Plato.GlslWriter: DO NOT EDIT");
            WriteLine($"// Created on {DateTime.Now}");
            WriteLine("precision highp float;");
            WriteLine("precision highp int;");
            WriteLine();

            ComputeStructs();
            WriteStructs();
            CollectConstants();
            CollectMemberFunctions();

            WriteLine("// ---- Function prototypes ----");
            foreach (var p in _prototypes)
                WriteLine(p + ";");
            WriteLine();

            WriteLine("// ---- Function definitions ----");
            foreach (var d in _definitions)
            {
                WriteTrimmed(d);
                WriteLine();
            }

            if (Skipped.Count > 0)
            {
                WriteLine("// ---- Skipped (not representable in GLSL) ----");
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

        public GlslWriter WriteTrimmed(string s)
        {
            s = s.TrimEnd('\r', '\n');
            if (s.Length == 0)
                return this;
            return Write(s).WriteLine();
        }

        // ---- Structs ------------------------------------------------------------

        private List<ConcreteType> _structTypes = new List<ConcreteType>();

        /// <summary>
        /// A concrete type becomes a struct when it is not natively mapped, not
        /// generic, and every field type is representable. Field support depends on
        /// which other structs survive, so iterate to a fixpoint, then order the
        /// survivors so that field types precede their users (GLSL requires
        /// declaration before use).
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

            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var c in candidates.ToList())
                {
                    if (c.TypeDef.Fields.Any(f => GlslTypeNameOrNull(f.Type) == null))
                    {
                        Skipped.Add($"type {c.TypeDef.Name}: field type not representable in GLSL");
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
                    break; // cyclic field types: impossible in GLSL; emit the rest anyway
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
                    WriteLine($"{GlslTypeName(f.Type)} {EscapeName(f.Name)};");
                IndentLevel--;
                WriteLine("};");
                WriteLine();
            }
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
                        ? GlslTypeName(tir.ZonkedReturnType)
                        : GlslTypeName(fi.ReturnType);
                    var sig = $"{ret} {EscapeName(fi.Name)}()";
                    if (!_claimedSignatures.Add($"{fi.Name}()"))
                        continue;
                    var body = new TirGlslBodyWriter(this, tir).ToString();
                    _prototypes.Add(sig);
                    _definitions.Add(sig + body);
                    FunctionsEmitted++;
                }
                catch (GlslUnsupportedException e)
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

                // Tier-1/Tier-2 fixed arrays: an IArrayLike struct with N homogeneous fields is
                // emitted with structurally generated accessors (At/Count/NumComponents/Components).
                // These are the only forms of "array" GLSL can express: the size is a compile-time
                // constant and At is an index chain that clamps to the last element on out-of-range
                // (GLSL cannot trap, so clamping is the closest thing to a safe failure).
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
        /// A concrete struct that implements IArrayLike over N homogeneous fields is a
        /// fixed-size array. Emit its accessors directly from the fields (the stdlib
        /// bodies route through the dynamic IArray, which GLSL has no equivalent for).
        /// Returns the set of member names handled here, so the normal path skips them.
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
            var fieldGlslTypes = ct.TypeDef.Fields.Select(f => GlslTypeNameOrNull(f.Type)).ToList();
            if (fields.Count == 0 || fieldGlslTypes.Any(t => t == null))
                return handled;
            var elem = fieldGlslTypes[0];
            if (fieldGlslTypes.Any(t => t != elem))
                return handled; // heterogeneous: not an array

            var n = fields.Count;

            // At(self, i): index chain, clamped to the last field on out-of-range.
            var chain = fields[n - 1];
            for (var i = n - 2; i >= 0; i--)
                chain = $"(i <= {i} ? self.{fields[i]} : {chain})";
            AddGenerated($"{elem} At({name} self, int i)", $" {{ return {chain}; }}");
            handled.Add("At");

            AddGenerated($"int Count({name} self)", $" {{ return {n}; }}");
            handled.Add("Count");
            AddGenerated($"int NumComponents({name} self)", $" {{ return {n}; }}");
            handled.Add("NumComponents");

            var comps = fields.Select(f => $"self.{f}").JoinStringsWithComma();
            AddGenerated($"{elem}[{n}] Components({name} self)", $" {{ return {elem}[{n}]({comps}); }}");
            handled.Add("Components");

            return handled;
        }

        private void AddGenerated(string sig, string bodyWithBraces)
        {
            if (!_claimedSignatures.Add(sig))
                return;
            _prototypes.Add(sig);
            _definitions.Add(sig + bodyWithBraces);
            FunctionsEmitted++;
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
                    ? tir.ZonkedParameterTypes.Select(GlslTypeName).ToList()
                    : fi.ParameterTypes.Select(GlslTypeName).ToList();

                if (paramTypes.Count != paramNames.Count)
                {
                    Skipped.Add($"{label}: parameter name/type count mismatch");
                    return;
                }

                var ret = tir?.ZonkedReturnType != null
                    ? GlslTypeName(tir.ZonkedReturnType)
                    : GlslTypeName(fi.ReturnType);

                // Two Plato overloads can erase to one GLSL signature — Plato distinguishes
                // Turns(Number):Angle from Turns(Angle):Number, GLSL sees float(float) twice.
                // First wins, which means the loser's callers silently get the winner's body,
                // so the drop is recorded rather than swallowed.
                var sigKey = $"{fi.Name}({paramTypes.JoinStringsWithComma()})";
                if (!_claimedSignatures.Add(sigKey))
                {
                    Skipped.Add($"{label}: overload erases to an already-emitted GLSL signature {ret} {sigKey}");
                    return;
                }

                var parameters = paramTypes.Zip(paramNames, (t, n) => $"{t} {n}").JoinStringsWithComma();
                var sig = $"{ret} {EscapeName(fi.Name)}({parameters})";

                string definition;
                if (tir == null)
                {
                    // Bodiless intrinsic: map to a GLSL built-in when we know how.
                    if (!TryGetIntrinsicBody(owner, fi.Name, paramNames, ret, out var body))
                    {
                        _claimedSignatures.Remove(sigKey);
                        Skipped.Add($"{label}: intrinsic without a GLSL mapping");
                        return;
                    }
                    definition = $"{sig} {{ return {body}; }}";
                }
                else
                {
                    definition = sig + new TirGlslBodyWriter(this, tir).ToString();
                }

                _prototypes.Add(sig);
                _definitions.Add(definition);
                FunctionsEmitted++;
            }
            catch (GlslUnsupportedException e)
            {
                Skipped.Add($"{label}: {e.Message}");
            }
        }

        // ---- Intrinsics ---------------------------------------------------------

        /// <summary>
        /// Native GLSL bodies for the bodiless Plato intrinsics (the C# writer
        /// relies on hand-written runtime structs for these).
        /// </summary>
        public bool TryGetIntrinsicBody(string owner, string name,
            IReadOnlyList<string> ps, string returnType, out string body)
        {
            body = null;
            var a = ps.Count > 0 ? ps[0] : null;
            var b = ps.Count > 1 ? ps[1] : null;

            switch ($"{owner}.{name}")
            {
                case "Number.Sqrt": body = $"sqrt({a})"; return true;
                case "Number.Abs": body = $"abs({a})"; return true;
                case "Number.Floor": body = $"floor({a})"; return true;
                case "Number.Ceiling": body = $"ceil({a})"; return true;
                case "Number.Round": body = $"round({a})"; return true;
                case "Number.Truncate": body = $"trunc({a})"; return true;
                case "Number.Exp": body = $"exp({a})"; return true;
                case "Number.Ln": body = $"log({a})"; return true;
                case "Number.Log": body = $"(log({a}) / log({b}))"; return true;
                case "Number.Pow": body = $"pow({a}, {b})"; return true;
                // Angle is float (radians), so the Angle(...) conversion is the identity.
                case "Number.Acos": body = $"acos({a})"; return true;
                case "Number.Asin": body = $"asin({a})"; return true;
                case "Number.Atan": body = $"atan({a})"; return true;
                case "Number.Compare":
                case "Integer.Compare":
                    body = $"({a} < {b} ? -1 : ({a} > {b} ? 1 : 0))"; return true;
                case "Integer.Divide": body = $"({a} / {b})"; return true;
                case "Integer.Modulo": body = $"({a} % {b})"; return true;
                case "Integer.ToNumber": body = $"float({a})"; return true;
                case "Angle.Radians": body = a; return true;
                case "Angle.Cos": body = $"cos({a})"; return true;
                case "Angle.Sin": body = $"sin({a})"; return true;
                case "Angle.Tan": body = $"tan({a})"; return true;
            }

            // Owner-agnostic mappings onto GLSL built-ins. These work uniformly for float
            // and vecN (GLSL overloads the built-ins), so a bodiless Plato intrinsic like
            // Vector3.Normalize or Number.Clamp lowers straight to the native call. This is
            // both faster GLSL and how a shader author would write it by hand.
            switch (name)
            {
                case "Min" when ps.Count == 2: body = $"min({a}, {b})"; return true;
                case "Max" when ps.Count == 2: body = $"max({a}, {b})"; return true;
                case "Clamp" when ps.Count == 3: body = $"clamp({a}, {b}, {ps[2]})"; return true;
                case "Saturate" when ps.Count == 1: body = $"clamp({a}, 0.0, 1.0)"; return true;
                case "Lerp" when ps.Count == 3: body = $"mix({a}, {b}, {ps[2]})"; return true;
                case "Mix" when ps.Count == 3: body = $"mix({a}, {b}, {ps[2]})"; return true;
                case "Step" when ps.Count == 2: body = $"step({a}, {b})"; return true;
                case "SmoothStep" when ps.Count == 3: body = $"smoothstep({a}, {b}, {ps[2]})"; return true;
                case "Sign" when ps.Count == 1: body = $"sign({a})"; return true;
                case "Fract" when ps.Count == 1: body = $"fract({a})"; return true;
                case "Sqrt" when ps.Count == 1: body = $"sqrt({a})"; return true;
                case "Abs" when ps.Count == 1: body = $"abs({a})"; return true;
                case "Floor" when ps.Count == 1: body = $"floor({a})"; return true;
                case "Ceiling" when ps.Count == 1: body = $"ceil({a})"; return true;
                case "Length" when ps.Count == 1: body = $"length({a})"; return true;
                case "Magnitude" when ps.Count == 1: body = $"length({a})"; return true;
                case "Dot" when ps.Count == 2: body = $"dot({a}, {b})"; return true;
                case "Cross" when ps.Count == 2: body = $"cross({a}, {b})"; return true;
                case "Normalize" when ps.Count == 1: body = $"normalize({a})"; return true;
                case "Distance" when ps.Count == 2: body = $"distance({a}, {b})"; return true;
                case "Reflect" when ps.Count == 2: body = $"reflect({a}, {b})"; return true;
                case "Atan2" when ps.Count == 2: body = $"atan({a}, {b})"; return true;
            }

            // Operator-named intrinsics on the native scalars: native GLSL operators.
            var op = ps.Count == 1 ? Operators.NameToUnaryOperator(name)
                : ps.Count == 2 ? Operators.NameToBinaryOperator(name) : null;
            if (op == null)
                return false;

            if (ps.Count == 1)
            {
                body = $"({op}{a})";
                return true;
            }

            // GLSL % only works on integers; float modulo is the mod() builtin.
            if (op == "%" && owner == "Number")
            {
                body = $"mod({a}, {b})";
                return true;
            }
            body = $"({a} {op} {b})";
            return true;
        }
    }
}
