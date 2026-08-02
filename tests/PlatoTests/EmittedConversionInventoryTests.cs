using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Pins every user-defined conversion the C# writer EMITS for the forward stdlib shipping tiers.
    ///
    /// The sibling pin, <see cref="ImplicitCastInventoryTests"/>, works from
    /// <c>TypeRelations.ComputeCasts</c> and so sees only conversions DECLARED in Plato. The writer
    /// mints conversions of its own from a type's shape, which appear in no Plato declaration and in
    /// no cast relation — for years that made the declared-cast pin a partial view of what the
    /// shipped C# actually coerces (compiler-399). This test closes that gap by reading the emitted
    /// text.
    ///
    /// The shape-derived rule the golden should show, decided in
    /// <c>tracker/decisions/2026-08-02-single-field-mirror-unwraps-only.md</c>:
    ///
    /// - a one-field type UNWRAPS implicitly to its field's type (and, for a <c>Number</c> payload,
    ///   to the built-in floating type) — reading the payload out loses nothing;
    /// - it does NOT wrap. A <c>Number</c> does not become a <c>Length</c>, an <c>Integer</c> does
    ///   not become a <c>VertexIndex</c>, a <c>Vector2D</c> does not become a <c>Direction2D</c>.
    ///   Wrapping asserts an invariant the source value was never checked against, so it is spelled
    ///   <c>new T(f)</c> or a named library function;
    /// - a multi-field type converts both ways with its value-tuple.
    ///
    /// So a NEW <c>implicit operator T(F)</c> line here, where F is T's single field, is the blind
    /// spot re-opening. A new declared conversion should appear in BOTH goldens.
    ///
    /// SCOPE: value-tuple converters are deliberately EXCLUDED. They are structural — the tuple is
    /// the field list, so the conversion asserts nothing a reader cannot already see in the type —
    /// and there are two per multi-field type, which would make this golden churn on every field
    /// added anywhere and train reviewers to re-baseline it without reading. What is left is every
    /// conversion between two named types, which is where an invariant can be lost.
    ///
    /// Re-baseline (after deciding the new conversions are intended): set env
    /// <c>PLATO_UPDATE_EMITTED_CONVERSIONS=1</c>, run once, review the diff, commit it with the
    /// change that earned it.
    /// </summary>
    [TestFixture]
    public static class EmittedConversionInventoryTests
    {
        private static readonly string Header =
            "# Emitted C# conversion inventory - forward stdlib shipping tiers.\n"
            + "# One conversion-operator signature per line, sorted, deduplicated across files.\n"
            + "# Covers BOTH writer-minted (shape-derived) and Plato-declared conversions; the\n"
            + "# declared subset is also in implicit-cast-inventory.txt.\n"
            + "# The shape rule: a one-field type unwraps implicitly and never wraps (compiler-399).\n"
            + "# Value-tuple converters are out of scope - see the test's summary for why.\n"
            + "# Re-baseline with PLATO_UPDATE_EMITTED_CONVERSIONS=1, and only for a reviewed change.\n";

        private static readonly Regex ConversionLine =
            new(@"(implicit|explicit) operator (?<sig>.+?)\s*=>", RegexOptions.Compiled);

        // A conversion signature is `Target(ParamType name)` — exactly one parenthesis pair. A
        // value-tuple on either side (`(Number, Number)(Vector2D self)`, `Vector2D((Number,
        // Number) v)`) adds a second, which is the cheap and exact test for the excluded pair.
        private static bool IsTupleConverter(string signature)
            => signature.Count(c => c == '(') > 1;

        private static string BuildInventory()
        {
            var w = new CSharpWriter(CheckerTestSupport.CompileForwardStdLibShippingTiers(), "unused-conversion-inventory")
            {
                ExtensionStyle = true,
            };
            w.WriteAll("float");

            var signatures = w.Files.Values
                .SelectMany(sb => sb.ToString().Replace("\r\n", "\n").Split('\n'))
                .Select(l => ConversionLine.Match(l))
                .Where(m => m.Success)
                .Select(m => (Kind: m.Groups[1].Value, Sig: m.Groups["sig"].Value.Trim()))
                .Where(x => !IsTupleConverter(x.Sig))
                .Select(x => $"{x.Kind} operator {x.Sig}")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(s => s, StringComparer.Ordinal)
                .ToList();

            Assert.IsNotEmpty(signatures,
                "no conversion operators emitted - the corpus failed to load rather than the writer emitting none");

            var sb2 = new StringBuilder(Header);
            foreach (var s in signatures)
                sb2.Append(s).Append('\n');
            return sb2.ToString();
        }

        private static string BaselinePath([CallerFilePath] string thisFile = "")
            => Path.Combine(Path.GetDirectoryName(thisFile), "emitted-conversion-inventory.txt");

        [Test]
        public static void EmittedConversionsMatchBaseline()
        {
            var actual = BuildInventory();
            var path = BaselinePath();

            if (Environment.GetEnvironmentVariable("PLATO_UPDATE_EMITTED_CONVERSIONS") == "1")
            {
                File.WriteAllText(path, actual);
                Assert.Pass($"Re-baselined emitted conversion inventory: {path}");
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
                TestContext.WriteLine($"  NEW conversion: {added}");
            foreach (var gone in e.Except(a).OrderBy(s => s, StringComparer.Ordinal))
                TestContext.WriteLine($"  GONE          : {gone}");
            Assert.Fail("The set of conversions the C# writer emits changed. An implicit one is a "
                + "coercion every consumer of the generated library gets for free. Confirm each new "
                + "line against stdlib/CONVENTIONS.md section 'Conversions' and the single-field rule "
                + "in this file's summary, then re-run with PLATO_UPDATE_EMITTED_CONVERSIONS=1.");
        }
    }
}
