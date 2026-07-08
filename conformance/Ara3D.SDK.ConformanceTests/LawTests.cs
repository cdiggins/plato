// Reflection-driven law runner (docs/plato-roadmap.md Phase 1.2).
//
// laws.plato defines generic Boolean functions over Plato interfaces; the compiler
// monomorphizes each law onto every concrete type implementing the interface.
// This runner discovers every (type, Law_*) pair in the generated code, constructs
// N seeded pseudorandom instances per parameter, invokes the law, and asserts it
// holds — modulo the KnownFailures.json quarantine manifest.
using System.Reflection;

namespace Ara3D.SDK.ConformanceTests
{
    [TestFixture]
    public class LawTests
    {
        const string GeneratedNamespace = "Ara3D.Geometry";

        public static IEnumerable<Type> GeneratedValueTypes()
            => typeof(LawTests).Assembly.GetTypes()
                .Where(t => t.Namespace == GeneratedNamespace && t.IsValueType && t.IsPublic && !t.IsGenericTypeDefinition)
                .OrderBy(t => t.Name, StringComparer.Ordinal);

        public static IEnumerable<TestCaseData> LawCases()
        {
            foreach (var type in GeneratedValueTypes())
            {
                // Single-parameter laws become instance properties; multi-parameter laws
                // become instance methods. (Static extension mirrors in *Extensions
                // classes are intentionally skipped: same bodies, System.Numerics interop.)
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
                    yield return new TestCaseData(type, member)
                        .SetName($"Law.{type.Name}.{member.Name}");
                }
            }
        }

        [TestCaseSource(nameof(LawCases))]
        public void Law(Type type, MemberInfo member)
        {
            var key = $"{type.Name}.{member.Name}";
            var paramTypes = member is MethodInfo mi
                ? mi.GetParameters().Select(p => p.ParameterType).ToArray()
                : Array.Empty<Type>();

            if (!ValueGen.CanCreate(type))
                Assert.Ignore($"Cannot construct instances of {type.Name} (see CoverageTests.ReportUnconstructibleTypes).");
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
