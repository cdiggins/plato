// The Cpu dialect (cpu64i32, TKT-1097): double/int32_t/std::string types, split output files, JSON readers and the reachable-set check.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Utils;

namespace Ara3D.Geometry.CppWriter
{
    /// <summary>
    /// Flags the Cpu dialect reads (CLI: --loops --fuse --fuse-consumers --exact-output --cpp-roots=A,B).
    /// <see cref="CppTirPipeline.Run"/> consults the first four; the writer checks <see cref="Roots"/>.
    /// </summary>
    public sealed record CppCpuOptions(bool Loops, bool Fuse, bool FuseConsumers, bool ExactOutput, IReadOnlyList<string> Roots)
    {
        public static readonly CppCpuOptions Default = new CppCpuOptions(false, false, false, false, Array.Empty<string>());
    }

    /// <summary>One data field of an emitted struct: a record field, or the Kind/flattened field of a closed sum.</summary>
    public sealed class CppField
    {
        public CppField(string name, TypeExpression type, string cppOverride = null)
        {
            Name = name;
            Type = type;
            CppOverride = cppOverride;
        }
        public string Name { get; }
        public TypeExpression Type { get; }
        public string CppOverride { get; }
    }

    public partial class CppWriter
    {
        public CppCpuOptions CpuOptions { get; set; } = CppCpuOptions.Default;

        public bool IsCpu => Dialect == CppDialect.Cpu;

        public static readonly Dictionary<string, string> CpuPrimitives = new Dictionary<string, string>
        {
            { "Number", "double" },
            { "Integer", "int32_t" },
            { "Boolean", "bool" },
            { "Character", "char" },
        };

        /// <summary>C++ spelling of Number / Integer in this dialect ("float"/"int" in Cpp and Cuda).</summary>
        public string NumberCpp => IsCpu ? "double" : "float";
        public string IntegerCpp => IsCpu ? "int32_t" : "int";

        public bool IsNumberCpp(string cpp) => cpp == NumberCpp;
        public bool IsIntegerCpp(string cpp) => cpp == IntegerCpp;
        public bool IsScalarCpp(string cpp)
            => cpp == NumberCpp || cpp == IntegerCpp || cpp == "bool" || cpp == "char";

        /// <summary>Vector2D/3D/4D are native float2/3/4 in Cpp and Cuda, ordinary structs in Cpu.</summary>
        public bool IsNativeVectorName(string name) => !IsCpu && NativeVectors.ContainsKey(name);

        /// <summary>Owners whose bodiless functions the prelude defines under the same names (plato-brep Intrinsics/*.cs).</summary>
        private static readonly HashSet<string> CpuPreludeOwners = new HashSet<string>
        {
            "Number", "Integer", "Boolean", "Character", "String",
        };

        // ---- fields -------------------------------------------------------------

        /// <summary>
        /// The data fields of a struct. A closed sum lays out as CSharpConcreteTypeWriter.WriteSumType does:
        /// an <c>int32_t Kind</c> (0-based, declaration order) and every case's fields flattened as
        /// <c>Case_Field</c>. Only the Cpu dialect emits sums.
        /// </summary>
        private List<CppField> StructFieldsOf(ConcreteType c)
        {
            if (IsCpu && c.TypeDef.IsSum)
            {
                var list = new List<CppField> { new CppField("Kind", null, "int32_t") };
                list.AddRange(c.TypeDef.Cases.SelectMany(cs => cs.Fields).Select(f => new CppField(f.FlatName, f.Type)));
                return list;
            }
            return c.TypeDef.Fields.Select(f => new CppField(f.Name, f.Type)).ToList();
        }

        private string FieldCppOrNull(CppField f)
            => f.CppOverride ?? CppTypeNameOrNull(f.Type);

        private string CpuDefaultValueExpr(string cppType)
        {
            switch (cppType)
            {
                case "double": return "0.0";
                case "int32_t": return "0";
                case "bool": return "false";
                case "char": return "'\\0'";
                case "String": return "String()";
                default: return $"{cppType}{{}}";
            }
        }

        // ---- signatures ---------------------------------------------------------

        private static string StripQualifier(string text)
        {
            var i = text.IndexOf("PLATO_FN ", StringComparison.Ordinal);
            return i < 0 ? text : text.Remove(i, "PLATO_FN ".Length);
        }

        private static string TemplateHeader(IReadOnlyList<string> names)
            => names.Count == 0 ? "" : $"template<{names.Select(n => "class " + n).JoinStringsWithComma()}> ";

        /// <summary>
        /// Parameter types for a Cpu function. A function-typed parameter becomes a template parameter
        /// (F0, F1, ...), so the body writer can pass lambdas at call sites with no std::function.
        /// </summary>
        private static List<string> CpuParameterTypes(List<(string Plato, Func<string> Cpp)> ps, List<string> templateNames)
        {
            var result = new List<string>();
            foreach (var (plato, cpp) in ps)
            {
                if (plato != null && plato.StartsWith("Function") && plato.Length > 8 && char.IsDigit(plato[plato.Length - 1]))
                {
                    var name = "F" + templateNames.Count;
                    templateNames.Add(name);
                    result.Add(name);
                }
                else
                {
                    result.Add(cpp());
                }
            }
            return result;
        }

        private static bool IsTemplateParamType(string t)
            => t != null && t.Length >= 2 && t[0] == 'F' && t.Skip(1).All(char.IsDigit);

        /// <summary>Array operations plato_cpu.hpp defines as templates on Array&lt;T&gt;; emitting the Plato body would collide.</summary>
        private static readonly HashSet<string> CpuPreludeArrayNames = new HashSet<string>
        {
            "Slice", "Zip", "IndexOf", "Freeze", "Take", "Skip", "Reverse", "Reduce", "Range", "Prepend", "MapRange", "Map",
            "GetHashCode", "FlatMap", "EmptyList", "Count", "Contains", "Concatenate", "At", "Append", "Any", "All", "AddRange",
        };

        private static bool IsPreludeArrayTemplate(string name, IReadOnlyList<string> paramTypes)
            => CpuPreludeArrayNames.Contains(name)
               && (paramTypes.Any(t => t.StartsWith("Array<")) || name == "Range" || name == "MapRange" || name == "EmptyList");

        // ---- bodiless functions --------------------------------------------------

        private void EmitCpuBodiless(ConcreteType ct, FunctionInstance fi, string label,
            IReadOnlyList<string> paramNames, IReadOnlyList<string> paramTypes, string ret)
        {
            var owner = ct.TypeDef.Name;
            var sigKey = SignatureKey(fi.Name, paramTypes);
            if (!_claimedSignatures.Add(sigKey))
                return;

            var parameters = paramTypes.Zip(paramNames, (t, n) => $"{t} {n}").JoinStringsWithComma();
            var sig = $"PLATO_FN {ret} {FunctionName(fi.Name)}({parameters})";
            var a = paramNames.Count > 0 ? paramNames[0] : null;
            var b = paramNames.Count > 1 ? paramNames[1] : null;

            // A case factory of a closed sum: set Kind and the case's own flattened fields.
            if (ct.TypeDef.IsSum)
            {
                var cs = ct.TypeDef.Cases.FirstOrDefault(c => c.Name == fi.Name);
                if (cs != null && cs.Fields.Count == paramNames.Count)
                {
                    var sets = cs.Fields.Select((f, i) => $" r.{EscapeName(f.FlatName)} = {paramNames[i]};").JoinStrings("");
                    Add(label, sigKey, sig, $"{sig} {{ {owner} r{{}}; r.Kind = {owner}::{cs.TagConstName};{sets} return r; }}", null);
                    return;
                }
            }

            // Angle-valued intrinsics need the Angle struct, which the prelude cannot see.
            var mentionsAngle = ret == "Angle" || paramTypes.Any(t => t == "Angle");
            if (mentionsAngle)
            {
                string body = null;
                switch ($"{owner}.{fi.Name}")
                {
                    case "Number.Acos": body = $"Angle{{ std::acos({a}) }}"; break;
                    case "Number.Asin": body = $"Angle{{ std::asin({a}) }}"; break;
                    case "Number.Atan": body = $"Angle{{ std::atan({a}) }}"; break;
                    case "Number.Atan2": body = $"Angle{{ std::atan2({a}, {b}) }}"; break;
                    case "Angle.Radians": body = $"{a}.Radians"; break;
                    case "Angle.Cos": body = $"std::cos({a}.Radians)"; break;
                    case "Angle.Sin": body = $"std::sin({a}.Radians)"; break;
                    case "Angle.Tan": body = $"std::tan({a}.Radians)"; break;
                }
                if (body != null)
                {
                    Add(label, sigKey, sig, $"{sig} {{ return {body}; }}", null);
                    return;
                }
                _claimedSignatures.Remove(sigKey);
                Skipped.Add($"{label}: Angle intrinsic without a {Dialect.DisplayName()} mapping");
                return;
            }

            if (CpuPreludeOwners.Contains(owner))
            {
                // The prelude defines it under the same name; record the key so callers resolve.
                _emitted.Add(new Emitted { Label = label, Key = sigKey, Prototype = "", Definition = "", Provided = true });
                return;
            }

            _claimedSignatures.Remove(sigKey);
            Skipped.Add($"{label}: intrinsic without a {Dialect.DisplayName()} mapping");
        }

        // ---- fixed-size IArrayLike members ---------------------------------------

        private HashSet<string> EmitCpuArrayLikeMembers(string name, IReadOnlyList<string> fields, string elem, HashSet<string> handled)
        {
            var n = fields.Count;
            var chain = "self." + fields[n - 1];
            for (var i = n - 2; i >= 0; i--)
                chain = $"(i <= {i} ? self.{fields[i]} : {chain})";
            AddGenerated("At", new[] { name, "int32_t" }, $"PLATO_FN {elem} At({name} self, int32_t i)", $" {{ return {chain}; }}");
            handled.Add("At");
            AddGenerated("Count", new[] { name }, $"PLATO_FN int32_t Count({name} self)", $" {{ return {n}; }}");
            handled.Add("Count");
            AddGenerated("NumComponents", new[] { name }, $"PLATO_FN int32_t NumComponents({name} self)", $" {{ return {n}; }}");
            handled.Add("NumComponents");

            var stores = fields.Select((f, i) => $" p[{i}] = self.{f};").JoinStrings("");
            AddGenerated("Components", new[] { name }, $"PLATO_FN Array<{elem}> Components({name} self)",
                $" {{ {elem}* p = arena_alloc<{elem}>({n});{stores} return make_array(p, {n}); }}");
            handled.Add("Components");

            var fromArr = Enumerable.Range(0, n).Select(i => $"c.data[{i}]").JoinStringsWithComma();
            AddGenerated("CreateFromComponents", new[] { name, $"Array<{elem}>" },
                $"PLATO_FN {name} CreateFromComponents({name} _, Array<{elem}> c)", $" {{ return {name}{{ {fromArr} }}; }}");
            handled.Add("CreateFromComponents");
            var broadcast = Enumerable.Range(0, n).Select(_ => "x").JoinStringsWithComma();
            AddGenerated("CreateFromComponent", new[] { name, elem },
                $"PLATO_FN {name} CreateFromComponent({name} _, {elem} x)", $" {{ return {name}{{ {broadcast} }}; }}");
            handled.Add("CreateFromComponent");

            AddTemplate($"template <typename F> PLATO_FN {name} MapComponents({name} x, F f)",
                $" {{ return {name}{{ {fields.Select(f => $"f(x.{f})").JoinStringsWithComma()} }}; }}");
            handled.Add("MapComponents");
            AddTemplate($"template <typename F> PLATO_FN {name} ZipComponents({name} a, {name} b, F f)",
                $" {{ return {name}{{ {fields.Select(f => $"f(a.{f}, b.{f})").JoinStringsWithComma()} }}; }}");
            AddTemplate($"template <typename F> PLATO_FN {name} ZipComponents({name} a, {name} b, {name} c, F f)",
                $" {{ return {name}{{ {fields.Select(f => $"f(a.{f}, b.{f}, c.{f})").JoinStringsWithComma()} }}; }}");
            handled.Add("ZipComponents");
            AddTemplate($"template <typename F> PLATO_FN bool AllZipComponents({name} a, {name} b, F f)",
                $" {{ return {string.Join(" && ", fields.Select(f => $"f(a.{f}, b.{f})"))}; }}");
            AddTemplate($"template <typename F> PLATO_FN bool AllZipComponents({name} a, {name} b, {name} c, F f)",
                $" {{ return {string.Join(" && ", fields.Select(f => $"f(a.{f}, b.{f}, c.{f})"))}; }}");
            handled.Add("AllZipComponents");
            AddTemplate($"template <typename F> PLATO_FN bool AnyZipComponents({name} a, {name} b, F f)",
                $" {{ return {string.Join(" || ", fields.Select(f => $"f(a.{f}, b.{f})"))}; }}");
            AddTemplate($"template <typename F> PLATO_FN bool AnyZipComponents({name} a, {name} b, {name} c, F f)",
                $" {{ return {string.Join(" || ", fields.Select(f => $"f(a.{f}, b.{f}, c.{f})"))}; }}");
            handled.Add("AnyZipComponents");
            AddTemplate($"template <typename F> PLATO_FN bool AllComponents({name} x, F f)",
                $" {{ return {string.Join(" && ", fields.Select(f => $"f(x.{f})"))}; }}");
            handled.Add("AllComponents");
            AddTemplate($"template <typename F> PLATO_FN bool AnyComponent({name} x, F f)",
                $" {{ return {string.Join(" || ", fields.Select(f => $"f(x.{f})"))}; }}");
            handled.Add("AnyComponent");

            var revInits = Enumerable.Range(0, n).Select(i => $"self.{fields[n - 1 - i]}").JoinStringsWithComma();
            AddGenerated("Reverse", new[] { name }, $"PLATO_FN {name} Reverse({name} self)", $" {{ return {name}{{ {revInits} }}; }}");
            handled.Add("Reverse");
            return handled;
        }

        // ---- top level -----------------------------------------------------------

        private CppWriter WriteAllCpu()
        {
            ComputeStructs();
            WriteCpuReflection();
            CollectConstants();
            CollectMemberFunctions();
            PruneCpu();

            var live = _emitted.Where(e => !e.Pruned).ToList();
            FunctionsEmitted = live.Count(e => !e.Provided);

            Files["plato_cpu.hpp"] = new StringBuilder(CppCpuPrelude.Text);
            Files["plato_types.hpp"] = new StringBuilder(BuildCpuTypesHeader());
            Files["plato_functions.hpp"] = new StringBuilder(BuildCpuFunctionsHeader(live));
            var n = 0;
            foreach (var text in BuildCpuFunctionFiles(live))
                Files[$"plato_functions_{n++}.cpp"] = new StringBuilder(text);
            Files["plato_json.cpp"] = new StringBuilder(BuildCpuJson());
            Files["plato_skipped.txt"] = new StringBuilder(Skipped.Count == 0 ? "" : string.Join("\n", Skipped) + "\n");
            return this;
        }

        private const string Banner = "// Autogenerated by Plato.CppWriter (C++ cpu64i32): DO NOT EDIT\n";

        /// <summary>All emitted structs including the synthesized Angle, in dependency order.</summary>
        private IEnumerable<(string Name, List<CppField> Fields, ConcreteType Type)> CpuStructs()
        {
            yield return ("Angle", new List<CppField> { new CppField("Radians", null, "double") }, null);
            foreach (var c in _structTypes)
                yield return (c.TypeDef.Name, StructFieldsOf(c), c);
        }

        private string BuildCpuTypesHeader()
        {
            var sb = new StringBuilder();
            sb.Append(Banner).Append("#pragma once\n#include \"plato_cpu.hpp\"\n#include <cstring>\n\nnamespace plato {\n\n");
            foreach (var s in CpuStructs())
                sb.Append($"struct {s.Name};\n");
            sb.Append('\n');

            foreach (var s in CpuStructs())
            {
                sb.Append($"struct {s.Name}\n{{\n");
                if (s.Type != null && s.Type.TypeDef.IsSum)
                    foreach (var cs in s.Type.TypeDef.Cases)
                        sb.Append($"    static constexpr int32_t {cs.TagConstName} = {cs.Tag};\n");
                foreach (var f in s.Fields)
                    sb.Append($"    {CppTypeName(f)} {EscapeName(f.Name)};\n");
                sb.Append("};\n\n");
            }

            sb.Append(@"// Structural equality and hash helpers (arrays compare element-wise).
template <class T> inline bool eq_(const T& a, const T& b) { return a == b; }
template <class T> inline bool eq_(const Array<T>& a, const Array<T>& b)
{
    if (a.count != b.count) return false;
    for (int32_t i = 0; i < a.count; i++) if (!eq_(a.data[i], b.data[i])) return false;
    return true;
}
inline int32_t mix_(int32_t h, int32_t v) { return (int32_t)(((uint32_t)h * 16777619u) ^ (uint32_t)v); }
inline int32_t hash_(double x) { uint64_t u; std::memcpy(&u, &x, sizeof u); return (int32_t)(u ^ (u >> 32)); }
inline int32_t hash_(int32_t x) { return x; }
inline int32_t hash_(bool x) { return x ? 1 : 0; }
inline int32_t hash_(char x) { return (int32_t)(unsigned char)x; }
inline int32_t hash_(const String& s) { int32_t h = 17; for (char c : s) h = mix_(h, (int32_t)(unsigned char)c); return h; }
template <class T> inline int32_t hash_(const Array<T>& a) { return a.count; }

");
            foreach (var s in CpuStructs())
            {
                var n = s.Name;
                string cond;
                if (s.Type != null && s.Type.TypeDef.IsSum)
                {
                    var terms = new List<string> { "a.Kind == b.Kind" };
                    foreach (var cs in s.Type.TypeDef.Cases.Where(c => c.Fields.Count > 0))
                    {
                        var eq = cs.Fields.Select(f => $"eq_(a.{EscapeName(f.FlatName)}, b.{EscapeName(f.FlatName)})").JoinStrings(" && ");
                        terms.Add($"(a.Kind != {n}::{cs.TagConstName} || ({eq}))");
                    }
                    cond = string.Join(" && ", terms);
                }
                else
                {
                    cond = s.Fields.Count == 0 ? "true"
                        : s.Fields.Select(f => $"eq_(a.{EscapeName(f.Name)}, b.{EscapeName(f.Name)})").JoinStrings(" && ");
                }
                sb.Append($"inline bool operator==(const {n}& a, const {n}& b) {{ return {cond}; }}\n");
                sb.Append($"inline bool operator!=(const {n}& a, const {n}& b) {{ return !(a == b); }}\n");
            }
            sb.Append("\n// JSON (definitions in plato_json.cpp)\n");
            foreach (var s in CpuStructs())
            {
                sb.Append($"void from_json(const Json& j, {s.Name}& v);\n");
                sb.Append($"Json to_json(const {s.Name}& v);\n");
            }
            sb.Append("\n} // namespace plato\n");
            return sb.ToString();
        }

        private string CppTypeName(CppField f)
            => FieldCppOrNull(f) ?? throw new CppUnsupportedException($"field '{f.Name}' is not representable in {Dialect.DisplayName()}");

        private string BuildCpuFunctionsHeader(List<Emitted> live)
        {
            var sb = new StringBuilder();
            sb.Append(Banner).Append("#pragma once\n#include \"plato_types.hpp\"\n\nnamespace plato {\n\n// ---- Prototypes ----\n");
            foreach (var e in live.Where(e => !e.Provided))
                sb.Append(e.Prototype).Append(";\n");
            sb.Append("\n// ---- Template definitions (a template must be defined where it is instantiated) ----\n");
            foreach (var e in live.Where(e => !e.Provided && e.IsTemplate))
                sb.Append(e.Definition.TrimEnd('\r', '\n')).Append("\n\n");
            sb.Append("} // namespace plato\n");
            return sb.ToString();
        }

        private const int MaxLinesPerFile = 20000;

        private IEnumerable<string> BuildCpuFunctionFiles(List<Emitted> live)
        {
            const string head = Banner + "#include \"plato_types.hpp\"\n#include \"plato_functions.hpp\"\n\nnamespace plato {\n\n";
            const string tail = "} // namespace plato\n";
            var sb = new StringBuilder(head);
            var lines = 0;
            var any = false;
            foreach (var e in live.Where(e => !e.Provided && !e.IsTemplate))
            {
                var def = e.Definition.TrimEnd('\r', '\n');
                var defLines = def.Count(c => c == '\n') + 2;
                if (lines > 0 && lines + defLines > MaxLinesPerFile)
                {
                    yield return sb.Append(tail).ToString();
                    sb = new StringBuilder(head);
                    lines = 0;
                }
                sb.Append(def).Append("\n\n");
                lines += defLines;
                any = true;
            }
            if (any || lines == 0)
                yield return sb.Append(tail).ToString();
        }

        private string BuildCpuJson()
        {
            var sb = new StringBuilder();
            sb.Append(Banner).Append("// Same encoding as plato-brep generated/Ara3D.StepBrep/Intrinsics/PlatoJson.cs: one object per struct, field names as keys;\n");
            sb.Append("// a closed sum writes its Kind and every flattened Case_Field.\n");
            sb.Append("#include \"plato_types.hpp\"\n\nnamespace plato {\n\n");
            foreach (var s in CpuStructs())
            {
                sb.Append($"void from_json(const Json& j, {s.Name}& v)\n{{\n");
                foreach (var f in s.Fields)
                    sb.Append($"    from_json(j[\"{f.Name}\"], v.{EscapeName(f.Name)});\n");
                sb.Append("}\n\n");
                sb.Append($"Json to_json(const {s.Name}& v)\n{{\n    Json j = Json::object();\n");
                foreach (var f in s.Fields)
                    sb.Append($"    j.set(\"{f.Name}\", to_json(v.{EscapeName(f.Name)}));\n");
                sb.Append("    return j;\n}\n\n");
            }
            sb.Append("} // namespace plato\n");
            return sb.ToString();
        }

        // ---- reflection helpers ----------------------------------------------------

        private void WriteCpuReflection()
        {
            // Angle: radians as a one-field struct; the Plato library sees it through these.
            AddGenerated("CreateFromComponent", new[] { "Angle", "double" },
                "PLATO_FN Angle CreateFromComponent(Angle _, double x)", " { return Angle{ x }; }");
            AddGenerated("CreateFromComponents", new[] { "Angle", "double" },
                "PLATO_FN Angle CreateFromComponents(Angle _, double x)", " { return Angle{ x }; }");
            AddGenerated("At", new[] { "Angle", "int32_t" },
                "PLATO_FN double At(Angle self, int32_t i)", " { return self.Radians; }");
            AddGenerated("Count", new[] { "Angle" }, "PLATO_FN int32_t Count(Angle self)", " { return 1; }");
            AddGenerated("NumComponents", new[] { "Angle" }, "PLATO_FN int32_t NumComponents(Angle self)", " { return 1; }");
            AddGenerated("Components", new[] { "Angle" }, "PLATO_FN double Components(Angle self)", " { return self.Radians; }");

            foreach (var s in CpuStructs())
            {
                var n = s.Name;
                AddGenerated("Equals", new[] { n, n }, $"PLATO_FN bool Equals(const {n}& a, const {n}& b)", " { return a == b; }");
                AddGenerated("NotEquals", new[] { n, n }, $"PLATO_FN bool NotEquals(const {n}& a, const {n}& b)", " { return a != b; }");
                AddGenerated("TypeName", new[] { n }, $"PLATO_FN String TypeName(const {n}& _)", $" {{ return String(\"{EscapeCppString(n)}\"); }}");

                string HashOf(CppField f)
                {
                    var cpp = FieldCppOrNull(f);
                    var member = "a." + EscapeName(f.Name);
                    return cpp != null && StructNames.Contains(cpp)
                        ? $"GetHashCode({member})"
                        : $"hash_({member})";
                }

                string body;
                if (s.Type != null && s.Type.TypeDef.IsSum)
                {
                    var sb = new StringBuilder(" { int32_t h = hash_(a.Kind);");
                    foreach (var cs in s.Type.TypeDef.Cases.Where(c => c.Fields.Count > 0))
                    {
                        sb.Append($" if (a.Kind == {n}::{cs.TagConstName}) {{");
                        foreach (var f in cs.Fields)
                            sb.Append($" h = mix_(h, {HashOf(new CppField(f.FlatName, f.Type))});");
                        sb.Append(" }");
                    }
                    sb.Append(" return h; }");
                    body = sb.ToString();
                }
                else
                {
                    body = " { int32_t h = 17;" + s.Fields.Select(f => $" h = mix_(h, {HashOf(f)});").JoinStrings("") + " return h; }";
                }
                AddGenerated("GetHashCode", new[] { n }, $"PLATO_FN int32_t GetHashCode(const {n}& a)", body);

                if (s.Type != null && s.Type.TypeDef.IsSum)
                    foreach (var cs in s.Type.TypeDef.Cases)
                        AddGenerated(cs.PredicateName, new[] { n }, $"PLATO_FN bool {cs.PredicateName}(const {n}& s)",
                            $" {{ return s.Kind == {n}::{cs.TagConstName}; }}");
            }
        }

        // ---- pruning and reachability ------------------------------------------------

        private Emitted FindCpu(CallSite c, Dictionary<string, Emitted> byKey, ILookup<string, Emitted> byName, bool aliveOnly)
        {
            bool Ok(Emitted e) => e != null && (!aliveOnly || !e.Pruned);
            if (c.Types.All(t => t != null))
            {
                if (byKey.TryGetValue(SignatureKey(c.Name, c.Types), out var hit) && Ok(hit))
                    return hit;
                var promoted = c.Types.Select(t => t == "int32_t" ? "double" : t);
                if (byKey.TryGetValue(SignatureKey(c.Name, promoted), out hit) && Ok(hit))
                    return hit;
            }
            foreach (var e in byName[c.Name])
            {
                if (!Ok(e))
                    continue;
                var open = e.Key.IndexOf('(');
                if (open < 0 || !e.Key.EndsWith(")"))
                    continue;
                var inner = e.Key.Substring(open + 1, e.Key.Length - open - 2);
                var ps = inner.Length == 0 ? new string[0] : inner.Split(new[] { ", " }, StringSplitOptions.None);
                if (ps.Length != c.Types.Count || !ps.Any(IsTemplateParamType))
                    continue;
                var match = true;
                for (var i = 0; i < ps.Length && match; i++)
                {
                    var t = c.Types[i];
                    if (IsTemplateParamType(ps[i]))
                        match = t == null;
                    else
                        match = t == ps[i] || t == "int32_t" && ps[i] == "double";
                }
                if (match)
                    return e;
            }
            return null;
        }

        /// <summary>A call the prelude answers: higher-order array helpers, and any array method no Plato function of that name shadows.</summary>
        private bool PreludeAnswers(CallSite c, ILookup<string, Emitted> byName)
        {
            if (UntrackedHofNames.Contains(c.Name) || IsPreambleArrayMethod(c))
                return true;
            return c.Types.Count > 0 && c.Types[0] != null && c.Types[0].StartsWith("Array<")
                   && !byName[c.Name].Any();
        }

        private void PruneCpu()
        {
            var byKey = new Dictionary<string, Emitted>();
            foreach (var e in _emitted)
                if (!byKey.ContainsKey(e.Key))
                    byKey[e.Key] = e;
            var byName = _emitted.Where(e => e.Label != "generated.template").ToLookup(e => e.Name);
            bool changed;
            do
            {
                changed = false;
                foreach (var e in _emitted)
                {
                    if (e.Pruned)
                        continue;
                    var missing = e.Callees.FirstOrDefault(c => !PreludeAnswers(c, byName) && FindCpu(c, byKey, byName, true) == null);
                    if (missing == null)
                        continue;
                    e.Pruned = true;
                    e.PruneReason = $"calls '{missing}', which is not emitted";
                    Skipped.Add($"{e.Label}: {e.PruneReason}");
                    changed = true;
                }
            } while (changed);
        }

        /// <summary>
        /// Functions reachable from <see cref="CppCpuOptions.Roots"/> through the recorded call sites that
        /// were skipped or pruned, as "Label: reason" lines. Empty when there are no roots or all are emitted.
        /// </summary>
        public List<string> UnemittedReachableFromRoots()
        {
            var report = new List<string>();
            var roots = CpuOptions.Roots;
            if (roots == null || roots.Count == 0)
                return report;

            var byKey = new Dictionary<string, Emitted>();
            foreach (var e in _emitted)
                if (!byKey.ContainsKey(e.Key))
                    byKey[e.Key] = e;
            var byName = _emitted.Where(e => e.Label != "generated.template").ToLookup(e => e.Name);

            var skippedByName = Skipped
                .Where(s => s.IndexOf(':') > 0)
                .ToLookup(s => { var label = s.Substring(0, s.IndexOf(':')); return label.Substring(label.LastIndexOf('.') + 1); });

            var seen = new HashSet<Emitted>();
            var reported = new HashSet<string>();
            var queue = new Queue<Emitted>();

            void Report(string line)
            {
                if (reported.Add(line))
                    report.Add(line);
            }

            void Visit(Emitted e)
            {
                if (e != null && seen.Add(e))
                    queue.Enqueue(e);
            }

            foreach (var root in roots)
            {
                var entries = byName[root].ToList();
                foreach (var e in entries)
                    Visit(e);
                if (entries.Count == 0)
                {
                    var skipped = skippedByName[root].ToList();
                    if (skipped.Count == 0)
                        Report($"root '{root}': no such function was emitted or skipped");
                    foreach (var s in skipped)
                        Report(s);
                }
            }

            while (queue.Count > 0)
            {
                var e = queue.Dequeue();
                if (e.Pruned)
                    Report($"{e.Label}: {e.PruneReason}");
                foreach (var c in e.Callees)
                {
                    var target = FindCpu(c, byKey, byName, false);
                    if (target != null)
                    {
                        Visit(target);
                        continue;
                    }
                    if (PreludeAnswers(c, byName))
                        continue;
                    foreach (var s in skippedByName[c.Name])
                        Report(s);
                }
            }
            return report;
        }
    }
}
