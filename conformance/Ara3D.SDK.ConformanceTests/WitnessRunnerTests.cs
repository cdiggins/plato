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

        public static IEnumerable<TestCaseData> WitnessCases()
            => ConstantsType
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.Name.StartsWith("Witness_") && ConformanceRunner.IsBooleanLike(p.PropertyType))
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .Select(p => new TestCaseData(p).SetName($"Witness.{p.Name}"));

        [TestCaseSource(nameof(WitnessCases))]
        public void Witness(PropertyInfo property)
        {
            var key = property.Name;
            var failures = new List<string>();
            try
            {
                if (!ConformanceRunner.ToBool(property.GetValue(null)))
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
            var witnessKeys = WitnessRunnerTests.WitnessCases().Select(c => ((PropertyInfo)c.Arguments[0]).Name);
            var valid = new HashSet<string>(lawKeys.Concat(witnessKeys), StringComparer.Ordinal);
            var stale = KnownFailures.Map.Keys.Where(k => !valid.Contains(k)).ToList();
            Assert.That(stale, Is.Empty, "KnownFailures.json contains entries with no matching test case: " + string.Join(", ", stale));
        }
    }
}
