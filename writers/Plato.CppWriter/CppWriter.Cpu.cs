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

        // ---- generic record types (TKT-1097 B1b) ----------------------------------
        //
        // Rule: a generic concrete type is ONE C++ template struct, emitted once with its type
        // parameters (template<class T> struct Counted { T Value; WorkBudget Work; };). An instance is
        // spelled Counted<Vector3D> in signatures, locals and constructor calls; List<T> and Buffer<T> are
        // the prelude's PlatoList/PlatoBuffer aliases. Because C++17 does not deduce aggregate template
        // arguments, each template gets a deduction guide from its fields so the body writer's bare
        // Counted{ a, b } still compiles.

        /// <summary>Type parameters in scope while a generic struct's fields are being spelled; null elsewhere.</summary>
        private HashSet<string> _typeParamScope;

        private sealed class ScopeGuard : IDisposable
        {
            private readonly CppWriter _w;
            private readonly HashSet<string> _prev;
            public ScopeGuard(CppWriter w, HashSet<string> next) { _w = w; _prev = w._typeParamScope; w._typeParamScope = next; }
            public void Dispose() => _w._typeParamScope = _prev;
        }

        private IDisposable ParamScope(ConcreteType c)
            => new ScopeGuard(this, c == null || c.TypeDef.TypeParameters.Count == 0
                ? null
                : new HashSet<string>(c.TypeDef.TypeParameters.Select(p => p.Name)));

        private string FieldCppIn(ConcreteType c, CppField f)
        {
            using (ParamScope(c))
                return FieldCppOrNull(f);
        }

        private static IReadOnlyList<string> GenericParamNames(ConcreteType c)
            => c == null ? Array.Empty<string>() : c.TypeDef.TypeParameters.Select(p => p.Name).ToList();

        private static string TemplatePrefix(ConcreteType c)
        {
            var ps = GenericParamNames(c);
            return ps.Count == 0 ? "" : $"template <{ps.Select(p => "class " + p).JoinStringsWithComma()}> ";
        }

        /// <summary>The struct's own name as a type: Counted&lt;T&gt; inside its template, Name otherwise.</summary>
        private static string SelfTypeName(string name, ConcreteType c)
        {
            var ps = GenericParamNames(c);
            return ps.Count == 0 ? name : $"{name}<{ps.JoinStringsWithComma()}>";
        }

        // ---- type variables as template parameters ------------------------------------
        //
        // A generic function ($T, or the zonked _T0, or the T of a generic type such as Array<T>) is a
        // function template: every type variable the signature or body names becomes a template parameter
        // (T of the generic type keeps its name, $T/_T0 become TV0, TV1, ...). A call site resolves against
        // the template by matching its argument types to the parameter types (FindCpu).

        private Dictionary<string, string> _tvMap;
        private List<string> _tvOrder;
        private HashSet<string> _tvFixed;
        private ConcreteType _tvOwner;

        private void BeginCpuTemplateVars(ConcreteType ct)
        {
            if (!IsCpu)
                return;
            _tvMap = new Dictionary<string, string>();
            _tvOrder = new List<string>();
            _tvFixed = new HashSet<string>(GenericParamNames(ct));
            _tvOwner = ct;
        }

        private void EndCpuTemplateVars()
        {
            _tvMap = null;
            _tvOrder = null;
            _tvFixed = null;
            _tvOwner = null;
        }

        /// <summary>Array2D written without arguments inside Array2D's own member functions means Array2D&lt;T&gt;.</summary>
        private string OwnerSelfName(TypeInstance ti)
        {
            if (_tvOwner == null || ti.Args.Count != 0 || ti.Name != _tvOwner.TypeDef.Name || _tvOwner.TypeDef.TypeParameters.Count == 0)
                return null;
            var ps = GenericParamNames(_tvOwner);
            foreach (var p in ps)
                if (!_tvMap.ContainsKey(p))
                {
                    _tvMap[p] = p;
                    _tvOrder.Add(p);
                }
            return $"{ti.Name}<{ps.JoinStringsWithComma()}>";
        }

        private List<string> CpuTemplateVarsInUse() => _tvOrder == null ? new List<string>() : _tvOrder.ToList();

        private static bool IsTypeVariableName(string n)
            => n.StartsWith("$") || (n.Length > 2 && n[0] == '_' && n[1] == 'T' && n.Skip(2).All(char.IsDigit));

        private string TemplateVarFor(TypeInstance ti)
        {
            if (_tvMap == null || ti.Args.Count != 0)
                return null;
            var n = ti.Name;
            var isVar = ti.Def.Kind == Ara3D.Geometry.AST.TypeKind.TypeVariable || IsTypeVariableName(n);
            var isOwnerParam = ti.Def.Kind == Ara3D.Geometry.AST.TypeKind.TypeParameter && _tvFixed.Contains(n);
            if (!isVar && !isOwnerParam)
                return null;
            if (!_tvMap.TryGetValue(n, out var cpp))
            {
                cpp = isOwnerParam ? n : "TV" + _tvMap.Count;
                _tvMap[n] = cpp;
                _tvOrder.Add(cpp);
            }
            return cpp;
        }

        private bool IsIgnoredType(string name)
            => IgnoredTypes.Contains(name) && !(IsCpu && (name == "Array2D" || name == "Array3D"));

        private bool IsExcludedTupleOrFunction(string name)
            => IsCpu
                ? name.StartsWith("Function") && name.Length > 8 && char.IsDigit(name[name.Length - 1])
                : IsTupleOrFunctionType(name);

        /// <summary>Counted&lt;Vector3D&gt;, List&lt;T&gt; and friends; null when this is not a generic instance or an argument is not representable.</summary>
        private string CpuGenericInstanceName(TypeInstance ti)
        {
            var name = ti.Name;
            var tv = TemplateVarFor(ti) ?? OwnerSelfName(ti);
            if (tv != null)
                return tv;
            if (_typeParamScope != null && ti.Def.Kind == Ara3D.Geometry.AST.TypeKind.TypeParameter && ti.Args.Count == 0 && _typeParamScope.Contains(name))
                return name;
            var isBuilder = (name == "List" || name == "Buffer") && ti.Args.Count == 1;
            if (!isBuilder && !(ti.Def.TypeParameters.Count > 0 && StructNames.Contains(name)))
                return null;
            if (ti.Args.Count == 0 || (!isBuilder && ti.Args.Count != ti.Def.TypeParameters.Count))
                return null;
            var args = ti.Args.Select(CppNameForInstance).ToList();
            return args.Any(a => a == null) ? null : $"{name}<{args.JoinStringsWithComma()}>";
        }

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
        private List<string> CpuParameterTypes(List<(string Plato, Func<string> Cpp, bool Interface, TypeInstance Type)> ps, List<string> templateNames, ConcreteType owner)
        {
            var result = new List<string>();
            foreach (var (plato, cpp, isInterface, paramType) in ps)
            {
                if (plato != null && plato.StartsWith("Function") && plato.Length > 8 && char.IsDigit(plato[plato.Length - 1]))
                {
                    var name = "F" + templateNames.Count;
                    templateNames.Add(name);
                    result.Add(name);
                }
                else if (isInterface && result.Count == 0 && OwnerSelfForInterface(owner, plato) is string self)
                {
                    // The receiver of an interface member: one concrete overload per implementing type
                    // (Plato's own dispatch), so overloads of one name over different interfaces never collide.
                    BindInterfaceTypeArguments(owner, paramType);
                    result.Add(self);
                }
                else if (isInterface && _tvMap != null)
                {
                    // An interface-typed parameter is one template parameter; At, Count and the rest the
                    // interface promises resolve on the argument's concrete type by overload.
                    var name = "TV" + _tvMap.Count;
                    _tvMap["#interface" + _tvMap.Count] = name;
                    _tvOrder.Add(name);
                    result.Add(name);
                }
                else
                {
                    result.Add(cpp());
                }
            }
            return result;
        }

        /// <summary>
        /// The receiver of <c>First(self: IIndexable&lt;$T&gt;): $T</c> on Array&lt;T&gt;: the interface's type variable
        /// is the owner's own argument for that interface (T), not a template parameter of its own.
        /// </summary>
        private void BindInterfaceTypeArguments(ConcreteType owner, TypeInstance iface)
        {
            if (owner == null || iface == null || _tvMap == null || iface.Args.Count == 0)
                return;
            var impl = owner.AllInterfaces.FirstOrDefault(i => i.Name == iface.Name && i.TypeExpression.TypeArgs.Count == iface.Args.Count);
            if (impl == null)
                return;
            for (var k = 0; k < iface.Args.Count; k++)
            {
                var arg = iface.Args[k];
                if (arg.Args.Count != 0 || !(arg.Def.Kind == Ara3D.Geometry.AST.TypeKind.TypeVariable || IsTypeVariableName(arg.Name)) || _tvMap.ContainsKey(arg.Name))
                    continue;
                var bound = CppTypeName(impl.TypeExpression.TypeArgs[k]);
                if (bound == null)
                    continue;
                _tvMap[arg.Name] = bound;
            }
        }

        /// <summary>The owner as a C++ type when it implements the named interface; null otherwise.</summary>
        private string OwnerSelfForInterface(ConcreteType owner, string iface)
        {
            if (owner == null || _tvMap == null)
                return null;
            if (!owner.AllInterfaces.Any(i => { var t = i.ToString(); return t == iface || t.StartsWith(iface + "<"); }))
                return null;
            var ps = GenericParamNames(owner);
            if (ps.Count == 0)
                return CppNameFor(owner.TypeDef.Name);
            foreach (var p in ps)
                if (!_tvMap.ContainsKey(p))
                {
                    _tvMap[p] = p;
                    _tvOrder.Add(p);
                }
            return SelfTypeName(owner.TypeDef.Name, owner);
        }

        /// <summary>True for a parameter type that is an interface other than Array/IArray (those are plato::Array).</summary>
        private static bool IsInterfaceParameter(TypeInstance ti)
            => ti != null && ti.Def != null && ti.Def.Kind == Ara3D.Geometry.AST.TypeKind.Interface
               && ti.Name != "Array" && ti.Name != "IArray";

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

            if (CpuPreludeOwners.Contains(owner) && PreludeNames.Contains(FunctionName(fi.Name)))
            {
                // The prelude defines it under the same name; record the key so callers resolve.
                _emitted.Add(new Emitted { Label = label, Key = sigKey, Prototype = "", Definition = "", Provided = true });
                return;
            }

            // A function the library declares and defers ("OffsetSecondDerivative(...): Vector3D;"): the C#
            // output throws NotImplementedException; the C++ output aborts, with the same reach.
            if (!CpuPreludeOwners.Contains(owner) && StructNames.Contains(owner) && ret != null)
            {
                // A generic owner's member is a template over the owner's type parameters.
                var vars = CpuTemplateVarsInUse();
                var tsig = vars.Count == 0 ? sig : TemplateHeader(vars) + sig;
                // IEquatable's members on a record: the structural operators of plato_types.hpp.
                var structural = paramNames.Count == 2 && ret == "bool"
                    ? fi.Name == "Equals" ? $"{a} == {b}" : fi.Name == "NotEquals" ? $"{a} != {b}" : null
                    : null;
                Add(label, sigKey, tsig, structural != null ? $"{tsig} {{ return {structural}; }}" : $"{tsig} {{ std::abort(); }}", null, vars);
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
            AddGenerated("Count", new[] { name }, $"PLATO_FN int32_t ToCount({name} self)", $" {{ return {n}; }}");
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

        /// <summary>
        /// The IIndexable members of a type whose only field is an Array&lt;E&gt; (VectorN): the same set as the fixed-size
        /// generator, with the length read from the array instead of fixed.
        /// </summary>
        private HashSet<string> EmitCpuDynamicArrayMembers(string name, string field, string arrayType, HashSet<string> handled)
        {
            var elem = arrayType.Substring("Array<".Length, arrayType.Length - "Array<".Length - 1);
            AddGenerated("At", new[] { name, "int32_t" }, $"PLATO_FN {elem} At({name} self, int32_t i)", $" {{ return At(self.{field}, i); }}");
            handled.Add("At");
            AddGenerated("Count", new[] { name }, $"PLATO_FN int32_t ToCount({name} self)", $" {{ return self.{field}.count; }}");
            handled.Add("Count");
            AddGenerated("NumComponents", new[] { name }, $"PLATO_FN int32_t NumComponents({name} self)", $" {{ return self.{field}.count; }}");
            handled.Add("NumComponents");
            AddGenerated("Components", new[] { name }, $"PLATO_FN {arrayType} Components({name} self)", $" {{ return self.{field}; }}");
            handled.Add("Components");
            AddGenerated("CreateFromComponents", new[] { name, arrayType },
                $"PLATO_FN {name} CreateFromComponents({name} _, {arrayType} c)", $" {{ return {name}{{ c }}; }}");
            handled.Add("CreateFromComponents");
            AddTemplate($"template <typename F> PLATO_FN {name} MapComponents({name} x, F f)",
                $" {{ int32_t n = x.{field}.count; {elem}* p = arena_alloc<{elem}>(n); for (int32_t i = 0; i < n; i++) p[i] = f(x.{field}.data[i]); return {name}{{ make_array(p, n) }}; }}");
            handled.Add("MapComponents");
            AddTemplate($"template <typename F> PLATO_FN {name} ZipComponents({name} a, {name} b, F f)",
                $" {{ int32_t n = a.{field}.count < b.{field}.count ? a.{field}.count : b.{field}.count; {elem}* p = arena_alloc<{elem}>(n); for (int32_t i = 0; i < n; i++) p[i] = f(a.{field}.data[i], b.{field}.data[i]); return {name}{{ make_array(p, n) }}; }}");
            AddTemplate($"template <typename F> PLATO_FN {name} ZipComponents({name} a, {name} b, {name} c, F f)",
                $" {{ int32_t n = a.{field}.count; if (b.{field}.count < n) n = b.{field}.count; if (c.{field}.count < n) n = c.{field}.count; {elem}* p = arena_alloc<{elem}>(n); for (int32_t i = 0; i < n; i++) p[i] = f(a.{field}.data[i], b.{field}.data[i], c.{field}.data[i]); return {name}{{ make_array(p, n) }}; }}");
            handled.Add("ZipComponents");
            AddTemplate($"template <typename F> PLATO_FN bool AllZipComponents({name} a, {name} b, F f)",
                $" {{ int32_t n = a.{field}.count < b.{field}.count ? a.{field}.count : b.{field}.count; for (int32_t i = 0; i < n; i++) if (!f(a.{field}.data[i], b.{field}.data[i])) return false; return true; }}");
            AddTemplate($"template <typename F> PLATO_FN bool AllZipComponents({name} a, {name} b, {name} c, F f)",
                $" {{ int32_t n = a.{field}.count; if (b.{field}.count < n) n = b.{field}.count; if (c.{field}.count < n) n = c.{field}.count; for (int32_t i = 0; i < n; i++) if (!f(a.{field}.data[i], b.{field}.data[i], c.{field}.data[i])) return false; return true; }}");
            handled.Add("AllZipComponents");
            AddTemplate($"template <typename F> PLATO_FN bool AnyZipComponents({name} a, {name} b, F f)",
                $" {{ int32_t n = a.{field}.count < b.{field}.count ? a.{field}.count : b.{field}.count; for (int32_t i = 0; i < n; i++) if (f(a.{field}.data[i], b.{field}.data[i])) return true; return false; }}");
            handled.Add("AnyZipComponents");
            AddTemplate($"template <typename F> PLATO_FN bool AllComponents({name} x, F f)",
                $" {{ for (int32_t i = 0; i < x.{field}.count; i++) if (!f(x.{field}.data[i])) return false; return true; }}");
            handled.Add("AllComponents");
            AddTemplate($"template <typename F> PLATO_FN bool AnyComponent({name} x, F f)",
                $" {{ for (int32_t i = 0; i < x.{field}.count; i++) if (f(x.{field}.data[i])) return true; return false; }}");
            handled.Add("AnyComponent");
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
            sb.Append(Banner).Append("#pragma once\n#include \"plato_cpu.hpp\"\n#include <cstring>\n#include <utility>\n\nnamespace plato {\n\n");
            foreach (var s in CpuStructs())
                sb.Append($"{TemplatePrefix(s.Type)}struct {s.Name};\n");
            sb.Append('\n');

            foreach (var s in CpuStructs())
            {
                sb.Append($"{TemplatePrefix(s.Type)}struct {s.Name}\n{{\n");
                if (s.Type != null && s.Type.TypeDef.IsSum)
                    foreach (var cs in s.Type.TypeDef.Cases)
                        sb.Append($"    static constexpr int32_t {cs.TagConstName} = {cs.Tag};\n");
                using (ParamScope(s.Type))
                    foreach (var f in s.Fields)
                        sb.Append($"    {CppTypeName(f)} {EscapeName(f.Name)};\n");
                // A one-field wrapper unwraps implicitly, as the C# struct's implicit operator does
                // (a typed index passes where the intrinsic wants an int32_t).
                if (s.Name.StartsWith("Tuple") && s.Name.Length > 5 && char.IsDigit(s.Name[s.Name.Length - 1]) && GenericParamNames(s.Type).Count == s.Fields.Count && s.Fields.Count >= 2)
                {
                    // (a, b) converts to any record built from its components, as the C# implicit conversion does.
                    var ps = GenericParamNames(s.Type);
                    var decl = ps.Select(p => $"std::declval<{p}>()").JoinStringsWithComma();
                    var vals = s.Fields.Select(f => EscapeName(f.Name)).JoinStringsWithComma();
                    sb.Append($"    template <class R, class = decltype(R{{ {decl} }})> operator R() const {{ return R{{ {vals} }}; }}\n");
                }
                if (s.Name != "Angle" && s.Fields.Count == 1 && !(s.Type != null && s.Type.TypeDef.IsSum) && GenericParamNames(s.Type).Count == 0)
                    sb.Append($"    operator {CppTypeName(s.Fields[0])}() const {{ return {EscapeName(s.Fields[0].Name)}; }}\n");
                sb.Append("};\n");
                AppendDeductionGuide(sb, s);
                sb.Append("\n");
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
                var tn = SelfTypeName(n, s.Type);
                var tp = TemplatePrefix(s.Type);
                sb.Append($"{tp}inline bool operator==(const {tn}& a, const {tn}& b) {{ return {cond}; }}\n");
                sb.Append($"{tp}inline bool operator!=(const {tn}& a, const {tn}& b) {{ return !(a == b); }}\n");
                if (tp.Length > 0)
                    sb.Append($"{tp}inline int32_t hash_(const {tn}& a) {{ {GenericHashBody(s)} }}\n");
            }
            sb.Append("\n// JSON (definitions in plato_json.cpp)\n");
            foreach (var s in CpuStructs().Where(s => GenericParamNames(s.Type).Count == 0))
            {
                sb.Append($"void from_json(const Json& j, {s.Name}& v);\n");
                sb.Append($"Json to_json(const {s.Name}& v);\n");
            }
            // A template's JSON is defined here: it can only be instantiated where it is visible.
            foreach (var s in CpuStructs().Where(s => GenericParamNames(s.Type).Count > 0))
            {
                var tn = SelfTypeName(s.Name, s.Type);
                var tp = TemplatePrefix(s.Type);
                sb.Append($"{tp}inline void from_json(const Json& j, {tn}& v)\n{{\n");
                foreach (var f in s.Fields)
                    sb.Append($"    from_json(j[\"{f.Name}\"], v.{EscapeName(f.Name)});\n");
                sb.Append("}\n");
                sb.Append($"{tp}inline Json to_json(const {tn}& v)\n{{\n    Json j = Json::Object();\n");
                foreach (var f in s.Fields)
                    sb.Append($"    j.set(\"{f.Name}\", to_json(v.{EscapeName(f.Name)}));\n");
                sb.Append("    return j;\n}\n");
            }
            sb.Append("\n} // namespace plato\n");
            return sb.ToString();
        }

        /// <summary>template &lt;class T&gt; Counted(T, WorkBudget) -&gt; Counted&lt;T&gt;; so Counted{ v, w } deduces its arguments (C++17 has no aggregate deduction).</summary>
        private void AppendDeductionGuide(StringBuilder sb, (string Name, List<CppField> Fields, ConcreteType Type) s)
        {
            var ps = GenericParamNames(s.Type);
            if (ps.Count == 0 || s.Fields.Count == 0)
                return;
            string[] types;
            using (ParamScope(s.Type))
                types = s.Fields.Select(f => FieldCppOrNull(f)).ToArray();
            // Every parameter must appear in some field type, or it cannot be deduced.
            if (!ps.All(p => types.Any(t => System.Text.RegularExpressions.Regex.IsMatch(t, @"\b" + p + @"\b"))))
                return;
            sb.Append($"{TemplatePrefix(s.Type)}{s.Name}({types.JoinStringsWithComma()}) -> {SelfTypeName(s.Name, s.Type)};\n");
        }

        /// <summary>Hash of a generic instance: scalar, string and array-count fields only (equal values hash equal; fields of a type parameter are left out).</summary>
        private string GenericHashBody((string Name, List<CppField> Fields, ConcreteType Type) s)
        {
            var ps = GenericParamNames(s.Type);
            var sb = new StringBuilder("int32_t h = 17;");
            using (ParamScope(s.Type))
                foreach (var f in s.Fields)
                {
                    var t = FieldCppOrNull(f);
                    if (t != null && !ps.Any(p => System.Text.RegularExpressions.Regex.IsMatch(t, @"\b" + p + @"\b")))
                        sb.Append($" h = mix_(h, hash_(a.{EscapeName(f.Name)}));");
                }
            return sb.Append(" return h;").ToString();
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
            foreach (var s in CpuStructs().Where(s => GenericParamNames(s.Type).Count == 0))
            {
                sb.Append($"void from_json(const Json& j, {s.Name}& v)\n{{\n");
                foreach (var f in s.Fields)
                    sb.Append($"    from_json(j[\"{f.Name}\"], v.{EscapeName(f.Name)});\n");
                sb.Append("}\n\n");
                sb.Append($"Json to_json(const {s.Name}& v)\n{{\n    Json j = Json::Object();\n");
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
            AddGenerated("Count", new[] { "Angle" }, "PLATO_FN int32_t ToCount(Angle self)", " { return 1; }");
            AddGenerated("NumComponents", new[] { "Angle" }, "PLATO_FN int32_t NumComponents(Angle self)", " { return 1; }");
            AddGenerated("Components", new[] { "Angle" }, "PLATO_FN double Components(Angle self)", " { return self.Radians; }");

            foreach (var s in CpuStructs().Where(s => GenericParamNames(s.Type).Count == 0))
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

                // IIndex promises Value(x); the typed indices satisfy it with their field.
                if (s.Type != null && s.Fields.Count == 1 && s.Fields[0].Name == "Value"
                    && s.Type.AllInterfaces.Any(i => i.ToString() == "IIndex"))
                    AddGenerated("Value", new[] { n }, $"PLATO_FN int32_t Value(const {n}& x)", " { return x.Value; }");

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

                // Per-case factories, called as Type.Case(x) = Case(Type{}, x) like every static member.
                if (s.Type != null && s.Type.TypeDef.IsSum)
                    foreach (var cs in s.Type.TypeDef.Cases)
                    {
                        var fieldTypes = cs.Fields.Select(f => FieldCppOrNull(new CppField(f.FlatName, f.Type))).ToList();
                        var ps = fieldTypes.Select((t, i) => $"{t} p{i}").Prepend($"{n} _").JoinStringsWithComma();
                        var sets = cs.Fields.Select((f, i) => $" r.{EscapeName(f.FlatName)} = p{i};").JoinStrings("");
                        AddGenerated(cs.Name, fieldTypes.Prepend(n).ToArray(), $"PLATO_FN {n} {FunctionName(cs.Name)}({ps})",
                            $" {{ {n} r{{}}; r.Kind = {n}::{cs.TagConstName};{sets} return r; }}");
                    }
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
            foreach (var e in byName[c.Name])
            {
                if (!Ok(e) || e.TemplateVars == null)
                    continue;
                if (UnifiesWithTemplate(e, c))
                    return e;
            }
            return null;
        }

        /// <summary>A call's argument types against a function template's parameter types, with each type variable bound once.</summary>
        private static bool UnifiesWithTemplate(Emitted e, CallSite c)
        {
            var open = e.Key.IndexOf('(');
            if (open < 0 || !e.Key.EndsWith(")"))
                return false;
            var inner = e.Key.Substring(open + 1, e.Key.Length - open - 2);
            var vars = System.Text.RegularExpressions.Regex.Matches(inner, @"\b(?:F\d+|" + string.Join("|", e.TemplateVars.Select(System.Text.RegularExpressions.Regex.Escape)) + @")\b");
            var pattern = new StringBuilder("^");
            var bound = new HashSet<string>();
            var last = 0;
            foreach (System.Text.RegularExpressions.Match m in vars)
            {
                pattern.Append(System.Text.RegularExpressions.Regex.Escape(inner.Substring(last, m.Index - last)));
                pattern.Append(bound.Add(m.Value) ? $"(?<{m.Value}>[^,]+?(?:<.*>)?)" : $"\\k<{m.Value}>");
                last = m.Index + m.Length;
            }
            pattern.Append(System.Text.RegularExpressions.Regex.Escape(inner.Substring(last))).Append('$');
            var callTypes = string.Join(", ", c.Types.Select(t => t ?? "?"));
            return System.Text.RegularExpressions.Regex.IsMatch(callTypes, pattern.ToString());
        }

        /// <summary>A call the prelude answers: higher-order array helpers, and any array method no Plato function of that name shadows.</summary>
        private bool PreludeAnswers(CallSite c, ILookup<string, Emitted> byName)
        {
            var onArray2D = c.Types.Count > 0 && c.Types[0] != null && (c.Types[0].StartsWith("Array2D<") || c.Types[0].StartsWith("Array3D<"));
            if ((UntrackedHofNames.Contains(c.Name) && !onArray2D) || IsPreambleArrayMethod(c))
                return true;
            // Array operations the prelude defines as templates (Take, Skip, Slice, ...): the Provided entry
            // carries the template key, which a concrete call does not match by text.
            if (c.Types.Count > 0 && c.Types[0] != null && c.Types[0].StartsWith("Array<") && CpuPreludeArrayNames.Contains(c.Name))
                return true;
            // An argument whose type is an interface (written "?") resolves by C++ overload on the concrete
            // type at the instantiation, when the prelude or a generated member of that name exists.
            if (c.Types.Any(t => t == null) && (byName[c.Name].Any() || PreludeNames.Contains(c.Name) || PreludeNames.Contains(FunctionName(c.Name))))
                return true;
            // The prelude's PlatoList / PlatoBuffer define Count, At, Add, AddRange, Set, Freeze, EmptyList.
            if (c.Types.Count > 0 && c.Types[0] != null && (c.Types[0].StartsWith("List<") || c.Types[0].StartsWith("Buffer<"))
                && (PreludeNames.Contains(c.Name) || PreludeNames.Contains("To" + c.Name)))
                return true;
            // Any other array-first call is answered only when the prelude really defines that name;
            // otherwise a library function (PlainLoop(Array<UvCoordinate>)) that was never emitted would pass.
            return c.Types.Count > 0 && c.Types[0] != null && c.Types[0].StartsWith("Array<")
                   && !byName[c.Name].Any() && PreludeNames.Contains(c.Name);
        }

        private static HashSet<string> _preludeNames;

        private static HashSet<string> PreludeNames => _preludeNames ??= BuildPreludeNames();

        /// <summary>Every identifier the prelude text writes directly before a parenthesis.</summary>
        private static HashSet<string> BuildPreludeNames()
        {
            var text = CppCpuPrelude.Text;
            var names = new HashSet<string>();
            var i = 0;
            while (i < text.Length)
            {
                if (!(char.IsLetter(text[i]) || text[i] == '_'))
                {
                    i++;
                    continue;
                }
                var start = i;
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_'))
                    i++;
                var end = i;
                while (i < text.Length && char.IsWhiteSpace(text[i]))
                    i++;
                if (i < text.Length && text[i] == '(')
                    names.Add(text.Substring(start, end - start));
            }
            return names;
        }

        /// <summary>
        /// Map(xs: Array2D&lt;$T1&gt;, f) and the IIndexable Map(xs, f) instantiated for Array2D have one shape: as C++ templates
        /// a call is ambiguous. Plato's dispatch picks the concrete type's own function, so the interface instance goes.
        /// </summary>
        private void DropShadowedInterfaceInstances()
        {
            var B = (char)92;
            foreach (var g in _emitted.Where(e => !e.Provided && e.IsTemplate && e.Key.EndsWith(")") && e.TemplateVars != null)
                .GroupBy(e => System.Text.RegularExpressions.Regex.Replace(e.Key, B + "b(?:F" + B + "d+|" + string.Join("|", e.TemplateVars.Select(System.Text.RegularExpressions.Regex.Escape)) + ")" + B + "b", "@")))
            {
                if (g.Count() < 2 || g.All(e => e.FromInterface) || g.All(e => !e.FromInterface))
                    continue;
                foreach (var e in g.Where(e => e.FromInterface).ToList())
                    _emitted.Remove(e);
            }
        }

        /// <summary>
        /// A field the type's interface declares as a member (IJagged: Offsets(self), Values(self)) is read as a
        /// function by the stdlib bodies that go through the interface; the C# field function is this accessor.
        /// </summary>
        private void EmitFieldAccessors()
        {
            foreach (var s in CpuStructs())
            {
                var n = s.Name;
                if (s.Type == null || s.Type.TypeDef.IsSum)
                    continue;
                foreach (var f in s.Fields)
                {
                    if (!s.Type.DeclaredFunctions.Any(d => d.Name == f.Name && d.ParameterNames.Count == 1) || (f.Name == "Value" && s.Fields.Count == 1))
                        continue;
                    string ftype;
                    using (ParamScope(s.Type))
                        ftype = CppTypeName(f);
                    var self = SelfTypeName(n, s.Type);
                    var fkey = SignatureKey(f.Name, new[] { self });
                    if (!_claimedSignatures.Add(fkey))
                        continue;
                    var fsig = $"{TemplatePrefix(s.Type)}PLATO_FN {ftype} {FunctionName(f.Name)}({self} self)";
                    Add($"generated.{f.Name}", fkey, fsig, $"{fsig} {{ return self.{EscapeName(f.Name)}; }}", null, GenericParamNames(s.Type).ToList());
                }
            }
        }

        /// <summary>
        /// A tuple converted to a record by a call, <c>JaggedArray((offsets, values))</c>, where the body writer could not write the
        /// record's braced initializer (the tuple is a local, not a literal). One conversion function per
        /// record such a call names, over the tuple type of its field types (a template when the record is).
        /// </summary>
        private void EmitTupleConversions()
        {
            var wanted = new HashSet<string>();
            foreach (var e in _emitted)
                foreach (var c in e.Callees)
                    if (c.Types.Count == 1 && c.Types[0] != null && c.Types[0].StartsWith("Tuple") && StructNames.Contains(c.Name))
                        wanted.Add(c.Name);
            foreach (var s in CpuStructs().Where(s => wanted.Contains(s.Name) && s.Fields.Count >= 2 && s.Fields.Count <= 10))
            {
                string tupleType;
                using (ParamScope(s.Type))
                    tupleType = $"Tuple{s.Fields.Count}<{s.Fields.Select(f => CppTypeName(f)).JoinStringsWithComma()}>";
                var tn = SelfTypeName(s.Name, s.Type);
                var vars = GenericParamNames(s.Type);
                var key = SignatureKey(s.Name, new[] { tupleType });
                if (!_claimedSignatures.Add(key))
                    continue;
                var inits = Enumerable.Range(0, s.Fields.Count).Select(i => $"t.X{i}").JoinStringsWithComma();
                var sig = $"{TemplatePrefix(s.Type)}PLATO_FN {tn} {FunctionName(s.Name)}({tupleType} t)";
                Add($"generated.{s.Name}", key, sig, $"{sig} {{ return {tn}{{ {inits} }}; }}", null, vars.ToList());
            }
        }

        /// <summary>
        /// Two overloads that differ only in a function-typed parameter (a template parameter F0) and an interface-typed
        /// one (TV0): Deform(self, mapping) and Deform(self, deformation). As templates they redeclare each other, so each
        /// gets an enable_if on its return type: F0 must be a function value, TV0 must not be one.
        /// </summary>
        private void ConstrainFunctionInterfaceOverloads()
        {
            var rx = new System.Text.RegularExpressions.Regex(@"\b(?:F|TV)\d+\b");
            var groups = _emitted.Where(e => !e.Provided && e.Label != "generated.template" && e.IsTemplate && e.Key.EndsWith(")"))
                .GroupBy(e => rx.Replace(e.Key, "@")).Where(g => g.Count() > 1);
            foreach (var g in groups)
            {
                var withF = g.Where(e => System.Text.RegularExpressions.Regex.IsMatch(e.Key, @"\bF\d+\b")).ToList();
                var withTv = g.Where(e => !System.Text.RegularExpressions.Regex.IsMatch(e.Key, @"\bF\d+\b") && System.Text.RegularExpressions.Regex.IsMatch(e.Key, @"\bTV\d+\b")).ToList();
                if (withF.Count == 0 || withTv.Count == 0)
                    continue;
                var fParams = KeyParameters(withF[0].Key);
                foreach (var e in withF)
                    ConstrainReturn(e, KeyParameters(e.Key).Where(p => IsTemplateParamType(p)).Select(p => $"is_function_value<{p}>::value"));
                foreach (var e in withTv)
                {
                    var ps = KeyParameters(e.Key);
                    var conds = new List<string>();
                    for (var i = 0; i < ps.Count && i < fParams.Count; i++)
                        if (IsTemplateParamType(fParams[i]) && System.Text.RegularExpressions.Regex.IsMatch(ps[i], @"^TV\d+$"))
                            conds.Add($"!is_function_value<{ps[i]}>::value");
                    ConstrainReturn(e, conds);
                }
            }
        }

        private static List<string> KeyParameters(string key)
        {
            var open = key.IndexOf('(');
            var inner = key.Substring(open + 1, key.Length - open - 2);
            var result = new List<string>();
            var depth = 0;
            var start = 0;
            for (var i = 0; i < inner.Length; i++)
            {
                if (inner[i] == '<') depth++;
                else if (inner[i] == '>') depth--;
                else if (inner[i] == ',' && depth == 0)
                {
                    result.Add(inner.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }
            if (inner.Length > 0)
                result.Add(inner.Substring(start).Trim());
            return result;
        }

        /// <summary>template&lt;..&gt; R Name(..) becomes template&lt;..&gt; std::enable_if_t&lt;cond, R&gt; Name(..) in the prototype and the definition.</summary>
        private void ConstrainReturn(Emitted e, IEnumerable<string> conditions)
        {
            var cond = string.Join(" && ", conditions);
            if (cond.Length == 0)
                return;
            var cppName = FunctionName(e.Name);
            var rx = new System.Text.RegularExpressions.Regex(@"^(.*?) " + System.Text.RegularExpressions.Regex.Escape(cppName) + @"\(");
            string Rewrite(string text)
            {
                // The template header ends at the '>' that closes its '<'; defaulted parameters contain '>' too.
                var depth = 0;
                var end = -1;
                for (var k = text.IndexOf('<'); k >= 0 && k < text.Length; k++)
                {
                    if (text[k] == '<') depth++;
                    else if (text[k] == '>' && --depth == 0) { end = k + 1; break; }
                }
                if (end < 0 || !text.StartsWith("template"))
                    return text;
                var header = text.Substring(0, end) + " ";
                var rest = text.Substring(end).TrimStart();
                return header + rx.Replace(rest, m => $"std::enable_if_t<{cond}, {m.Groups[1].Value}> {cppName}(", 1);
            }
            e.Prototype = Rewrite(e.Prototype);
            e.Definition = Rewrite(e.Definition);
        }

        private void PruneCpu()
        {
            EmitFieldAccessors();
            DropShadowedInterfaceInstances();
            EmitTupleConversions();
            ConstrainFunctionInterfaceOverloads();
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
