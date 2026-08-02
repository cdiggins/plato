using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Pins every implicit conversion the forward stdlib defines.
    ///
    /// Nothing in the language marks a function as a cast. A one-parameter function whose name
    /// equals its concrete return type IS one — <c>FunctionInstance.IsImplicitCast</c> infers it
    /// from the spelling, <c>TypeRelations.ComputeCasts</c> turns it into a cast relation the
    /// solver uses for overload resolution, and the C# writer emits an <c>implicit operator</c>
    /// for it. So a conversion can appear without anyone deciding to write one: rename a function,
    /// or widen a member's return type to a type of the same name, and every implementer silently
    /// gains a cast. <c>ILength.Length</c> returning <c>Number</c> is an accessor; returning
    /// <c>Length</c> (<c>quantities.types.plato</c>) it is a conversion on every implementer.
    ///
    /// This test makes that visible. The pin is the SET OF PAIRS (source type, target type), not
    /// the functions behind them, because the pair is what changes the type system: two overloads
    /// converting the same pair add no new coercion, while one new pair does.
    ///
    /// SCOPE: the shipping tiers (<see cref="CheckerTestSupport.ShippingTiers"/>) — the vocabulary
    /// that is linted and converted to C#. <c>stdlib/future</c> is deliberately out: it is
    /// aspirational, it emits nothing, and its churn would make this golden noise.
    ///
    /// NOT covered: the C# writer's single-field mirror, which emits an implicit operator both
    /// ways between a one-field type and its field's type. Those come from the type's shape, not
    /// from any Plato declaration, so they are in no cast relation and in no line below
    /// (tracked as compiler-399).
    ///
    /// The rule these conversions must satisfy is in <c>stdlib/CONVENTIONS.md</c>, section
    /// "Conversions": a type-named conversion must be faithful — same mathematical object, no
    /// invented parameter, nothing dropped. Anything approximate or lossy is spelled <c>ToX</c>
    /// and stays explicit. A diff here is a question to answer, not a number to bump.
    ///
    /// Re-baseline (after deciding the new conversions are intended): set env
    /// <c>PLATO_UPDATE_CAST_INVENTORY=1</c>, run once, review the diff, commit it with the change
    /// that earned it.
    /// </summary>
    [TestFixture]
    public static class ImplicitCastInventoryTests
    {
        private static readonly string Header =
            "# Implicit cast inventory - forward stdlib shipping tiers.\n"
            + "# One 'Source -> Target' per line, sorted. Derived from TypeRelations.ComputeCasts.\n"
            + "# The rule these must satisfy: stdlib/CONVENTIONS.md, section 'Conversions'.\n"
            + "# Re-baseline with PLATO_UPDATE_CAST_INVENTORY=1, and only for a reviewed change.\n";

        private static string BuildInventory()
        {
            var comp = CheckerTestSupport.CompileForwardStdLibShippingTiers();
            Assert.IsTrue(comp.CompletedCompilation,
                "forward stdlib did not compile; the inventory would be silently short");

            var pairs = comp.TypeRelations.ComputeCasts()
                .Select(r => $"{r.Source.Name} -> {r.Dest.Name}")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(s => s, StringComparer.Ordinal)
                .ToList();

            Assert.IsNotEmpty(pairs,
                "no implicit casts found - the corpus failed to load rather than the stdlib having none");

            var sb = new StringBuilder(Header);
            foreach (var p in pairs)
                sb.Append(p).Append('\n');
            return sb.ToString();
        }

        private static string BaselinePath([CallerFilePath] string thisFile = "")
            => Path.Combine(Path.GetDirectoryName(thisFile), "implicit-cast-inventory.txt");

        [Test]
        public static void ImplicitCastsMatchBaseline()
        {
            var actual = BuildInventory();
            var path = BaselinePath();

            if (Environment.GetEnvironmentVariable("PLATO_UPDATE_CAST_INVENTORY") == "1")
            {
                File.WriteAllText(path, actual);
                Assert.Pass($"Re-baselined implicit cast inventory: {path}");
                return;
            }

            if (!File.Exists(path))
            {
                File.WriteAllText(path, actual);
                Assert.Fail($"No baseline existed; wrote initial inventory to {path}. Review and commit it.");
                return;
            }

            var expected = File.ReadAllText(path).Replace("\r\n", "\n");
            if (expected == actual)
                return;

            var e = expected.Split('\n').Where(l => !l.StartsWith("#")).ToHashSet(StringComparer.Ordinal);
            var a = actual.Split('\n').Where(l => !l.StartsWith("#")).ToHashSet(StringComparer.Ordinal);
            foreach (var added in a.Except(e).OrderBy(s => s, StringComparer.Ordinal))
                TestContext.WriteLine($"  NEW cast: {added}");
            foreach (var gone in e.Except(a).OrderBy(s => s, StringComparer.Ordinal))
                TestContext.WriteLine($"  GONE    : {gone}");
            Assert.Fail("The set of implicit conversions changed. Every line above is a coercion the "
                + "type checker now does (or no longer does) on its own. Confirm each new one is "
                + "faithful per stdlib/CONVENTIONS.md section 'Conversions', then re-run with "
                + "PLATO_UPDATE_CAST_INVENTORY=1.");
        }
    }
}
