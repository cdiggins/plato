// Reflection-driven law runner for the FORWARD stdlib.
// Adapted (namespace only) from conformance/Ara3D.SDK.ConformanceTests/LawTests.cs.
//
// stdlib-tests/foundation.laws.plato defines generic Boolean Law_* functions over the
// forward interfaces/types; the compiler monomorphizes each onto every concrete implementor.
// This runner discovers every (type, Law_*) pair in the generated code, constructs N seeded
// pseudorandom instances per parameter, invokes the law, and asserts it holds -- modulo
// KnownFailures.json.
using System.Reflection;

namespace Plato.ForwardConformanceTests
{
    [TestFixture]
    public class LawTests
    {
        const string GeneratedNamespace = "Ara3D.Geometry";

        public static IEnumerable<Type> GeneratedValueTypes()
            => typeof(LawTests).Assembly.GetTypes()
                .Where(t => t.Namespace == GeneratedNamespace && t.IsValueType && t.IsPublic && !t.IsGenericTypeDefinition)
                .OrderBy(t => t.Name, StringComparer.Ordinal);

        static readonly Dictionary<Type, string> PrimitiveTypeNames = new()
        {
            { typeof(float), "Number" },
            { typeof(int), "Integer" },
            { typeof(bool), "Boolean" },
            { typeof(char), "Character" },
            { typeof(string), "String" },
        };

        public static string MappedTypeName(Type t)
            => PrimitiveTypeNames.TryGetValue(t, out var n) ? n : t.Name;

        public static IEnumerable<TestCaseData> LawCases()
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var type in GeneratedValueTypes())
            {
                var members = new List<(MemberInfo Member, Type ReturnType)>();
                foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    if (p.Name.StartsWith("Law_") && p.GetMethod != null)
                        members.Add((p, p.PropertyType));
                foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    if (m.Name.StartsWith("Law_") && !m.IsSpecialName)
                        members.Add((m, m.ReturnType));

                foreach (var (member, returnType) in members.OrderBy(m => m.Member.Name, StringComparer.Ordinal))
                {
                    if (!ConformanceRunner.IsBooleanLike(returnType))
                        continue;
                    if (!seen.Add($"{type.Name}.{member.Name}"))
                        continue;
                    yield return new TestCaseData(type, member)
                        .SetName($"Law.{type.Name}.{member.Name}");
                }
            }

            // Static laws over the native primitives (scalar-erased output).
            var staticCases = new List<(string Key, Type ReceiverType, MethodInfo Method)>();
            foreach (var type in typeof(LawTests).Assembly.GetTypes()
                         .Where(t => t.Namespace == GeneratedNamespace && t.IsAbstract && t.IsSealed && t.IsPublic))
            {
                foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (!m.Name.StartsWith("Law_") || m.IsSpecialName || m.IsGenericMethodDefinition)
                        continue;
                    if (!ConformanceRunner.IsBooleanLike(m.ReturnType))
                        continue;
                    var ps = m.GetParameters();
                    if (ps.Length == 0 || !PrimitiveTypeNames.ContainsKey(ps[0].ParameterType))
                        continue;
                    staticCases.Add(($"{MappedTypeName(ps[0].ParameterType)}.{m.Name}", ps[0].ParameterType, m));
                }
            }
            foreach (var (key, receiverType, method) in staticCases.OrderBy(c => c.Key, StringComparer.Ordinal))
            {
                if (!seen.Add(key))
                    continue;
                yield return new TestCaseData(receiverType, method)
                    .SetName($"Law.{key}");
            }
        }

        [TestCaseSource(nameof(LawCases))]
        public void Law(Type type, MemberInfo member)
        {
            var key = $"{MappedTypeName(type)}.{member.Name}";
            var isStaticLaw = member is MethodInfo smi && smi.IsStatic;
            var paramTypes = member is MethodInfo mi
                ? mi.GetParameters().Select(p => p.ParameterType).Skip(isStaticLaw ? 1 : 0).ToArray()
                : Array.Empty<Type>();

            if (!ValueGen.CanCreate(type))
                Assert.Ignore($"Cannot construct instances of {type.Name}.");
            foreach (var pt in paramTypes)
                if (!ValueGen.CanCreate(pt))
                    Assert.Ignore($"Cannot construct law parameter of type {pt.Name}.");

            var rng = new Random(ValueGen.StableSeed(key));
            var failures = new List<string>();
            for (var i = 0; i < ValueGen.N; i++)
            {
                var self = ValueGen.Create(type, rng);
                var args = paramTypes.Select(pt => ValueGen.Create(pt, rng)).ToArray();
                try
                {
                    var result = member is PropertyInfo pi
                        ? pi.GetValue(self)
                        : isStaticLaw
                            ? ((MethodInfo)member).Invoke(null, args.Prepend(self).ToArray())
                            : ((MethodInfo)member).Invoke(self, args);
                    if (!ConformanceRunner.ToBool(result))
                        failures.Add($"trial {i}: false for self={ConformanceRunner.Describe(self)}"
                            + (args.Length > 0 ? $", args=[{string.Join("; ", args.Select(ConformanceRunner.Describe))}]" : ""));
                }
                catch (TargetInvocationException e)
                {
                    failures.Add($"trial {i}: threw {e.InnerException?.GetType().Name}: {e.InnerException?.Message} "
                        + $"for self={ConformanceRunner.Describe(self)}"
                        + (args.Length > 0 ? $", args=[{string.Join("; ", args.Select(ConformanceRunner.Describe))}]" : ""));
                }
            }
            ConformanceRunner.AssertWithManifest(key, failures, ValueGen.N);
        }
    }
}
