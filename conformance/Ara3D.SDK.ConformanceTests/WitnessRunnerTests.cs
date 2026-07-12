// Witness (formula-vs-citation) runner (docs/plato-roadmap.md Phase 1.7).
//
// witness.plato defines no-argument Boolean functions prefixed Witness_. The C#
// emitter places all no-argument library functions into the generated `Constants`
// static class as static properties; each becomes one test case here, checked
// against the KnownFailures.json manifest.
using System.Reflection;

namespace Ara3D.SDK.ConformanceTests
{
    [TestFixture]
    public class WitnessRunnerTests
    {
        public static Type ConstantsType =>
            typeof(WitnessRunnerTests).Assembly.GetType("Ara3D.Geometry.Constants")
            ?? throw new InvalidOperationException("Generated Ara3D.Geometry.Constants class not found. Run tools\\regen-conformance.ps1.");

        // Constants members are static PROPERTIES in the classic styles and static METHODS
        // under --methods; the runner accepts both shapes with identical case names.
        public static IEnumerable<TestCaseData> WitnessCases()
            => ConstantsType
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.Name.StartsWith("Witness_") && ConformanceRunner.IsBooleanLike(p.PropertyType))
                .Select(p => (MemberInfo)p)
                .Concat(ConstantsType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name.StartsWith("Witness_") && m.GetParameters().Length == 0
                        && ConformanceRunner.IsBooleanLike(m.ReturnType)))
                .OrderBy(m => m.Name, StringComparer.Ordinal)
                .Select(m => new TestCaseData(m).SetName($"Witness.{m.Name}"));

        [TestCaseSource(nameof(WitnessCases))]
        public void Witness(MemberInfo member)
        {
            var key = member.Name;
            var failures = new List<string>();
            try
            {
                var value = member is PropertyInfo p ? p.GetValue(null) : ((MethodInfo)member).Invoke(null, null);
                if (!ConformanceRunner.ToBool(value))
                    failures.Add("witness returned false");
            }
            catch (TargetInvocationException e)
            {
                failures.Add($"threw {e.InnerException?.GetType().Name}: {e.InnerException?.Message}");
            }
            ConformanceRunner.AssertWithManifest(key, failures, 1);
        }

        [Test]
        public void AtLeastTwentyWitnessesExist()
            => Assert.That(WitnessCases().Count(), Is.GreaterThanOrEqualTo(20));
    }

    [TestFixture]
    public class CoverageTests
    {
        /// <summary>
        /// Diagnostic: lists generated types carrying Law_ members that the value
        /// generator cannot construct (reported, per roadmap 1.3). Always passes.
        /// </summary>
        [Test]
        public void ReportUnconstructibleTypes()
        {
            var lawTypes = LawTests.LawCases()
                .Select(c => (Type)c.Arguments[0]).Distinct().ToList();
            var bad = lawTypes.Where(t => !ValueGen.CanCreate(t)).ToList();
            TestContext.Out.WriteLine($"{lawTypes.Count} generated types carry Law_ members.");
            TestContext.Out.WriteLine(bad.Count == 0
                ? "All law-bearing types are constructible."
                : "Unconstructible law-bearing types:");
            foreach (var t in bad)
                TestContext.Out.WriteLine($"  {t.Name}: {(ValueGen.Unconstructible.TryGetValue(t.Name, out var why) ? why : "unknown")}");
            if (ValueGen.Unconstructible.Count > 0)
            {
                TestContext.Out.WriteLine("All types the generator gave up on (incl. parameter types):");
                foreach (var kv in ValueGen.Unconstructible)
                    TestContext.Out.WriteLine($"  {kv.Key}: {kv.Value}");
            }
            Assert.Pass();
        }

        /// <summary>Every manifest entry must correspond to an existing test case (no stale keys).</summary>
        [Test]
        public void ManifestKeysAreValid()
        {
            var lawKeys = LawTests.LawCases().Select(c => $"{LawTests.MappedTypeName((Type)c.Arguments[0])}.{((System.Reflection.MemberInfo)c.Arguments[1]).Name}");
            var witnessKeys = WitnessRunnerTests.WitnessCases().Select(c => ((MemberInfo)c.Arguments[0]).Name);
            var valid = new HashSet<string>(lawKeys.Concat(witnessKeys), StringComparer.Ordinal);
            var stale = KnownFailures.Map.Keys.Where(k => !valid.Contains(k)).ToList();
            Assert.That(stale, Is.Empty, "KnownFailures.json contains entries with no matching test case: " + string.Join(", ", stale));
        }
    }
}
