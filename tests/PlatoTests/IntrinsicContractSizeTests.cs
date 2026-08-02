using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// The size ratchet for the host intrinsic contract (plato-378).
    ///
    /// `intrinsics.library.plato` is the porting checklist every backend must satisfy — C#, then
    /// C++, CUDA, TypeScript, GLSL, Rust. It drifted to 141 signatures, most of which were not
    /// irreducible: formulas over other intrinsics, or exact duplicates of generic bodies the
    /// stdlib already derives on Orderable / Equatable. plato-378 cut it to 65 and stated the
    /// admission rule:
    ///
    ///   <b>An intrinsic must not be expressible in Plato from the other intrinsics.</b>
    ///
    /// Nothing enforced the rule, so nothing stopped the next drift. This fixture counts the
    /// bodiless declarations textually — no compilation, so it costs milliseconds and cannot be
    /// broken by an unrelated compiler regression — and fails when the contract GROWS.
    ///
    /// If you are here because this test failed: adding an intrinsic is a real decision, not a
    /// formality. Write the Plato body first. If it compiles, that is your answer and the
    /// function belongs in a `*.library.plato` file. If it genuinely cannot be written — it needs
    /// a loop, bit-level access, or a representation constant — raise the ceiling here in the
    /// same commit, say which of those it is, and add the counterpart to `src/Plato.Intrinsics`
    /// (IntrinsicObligationTests enforces that half).
    /// </summary>
    [TestFixture]
    public static class IntrinsicContractSizeTests
    {
        // Measured 2026-07-31 after plato-378: 65 (was 141).
        //   Number 26, Integer 18, Array 5, Boolean 3, Character/String 2, List 7, Buffer 4.
        // A ceiling to LOWER, never to raise without a stated reason.
        private const int MaxIntrinsics = 65;

        /// <summary>A bodiless signature: `Name(params): ReturnType;` at library-member indent,
        /// with no `=>`. Comment lines are stripped first so a signature quoted in a doc comment
        /// cannot inflate the count.</summary>
        private static readonly Regex BodilessSignature = new Regex(
            @"^\s+[A-Za-z_]\w*\s*\(.*\)\s*:\s*[^;=]+;\s*$", RegexOptions.Compiled);

        private static string IntrinsicsFile
            => Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation", "intrinsics.library.plato");

        private static string[] SignatureLines()
            => File.ReadAllLines(IntrinsicsFile)
                .Where(l => !l.TrimStart().StartsWith("//"))
                .Where(l => BodilessSignature.IsMatch(l))
                .Select(l => l.Trim())
                .ToArray();

        [Test]
        public static void IntrinsicContractDoesNotGrow()
        {
            var sigs = SignatureLines();
            TestContext.WriteLine($"{IntrinsicsFile}: {sigs.Length} intrinsics (ceiling {MaxIntrinsics})");

            Assert.LessOrEqual(sigs.Length, MaxIntrinsics,
                "The host intrinsic contract grew. An intrinsic must not be expressible in Plato "
                + "from the other intrinsics (plato-378) — write the body first; if it compiles, "
                + "it belongs in a *.library.plato file, not here.");
        }

        /// <summary>The contract is one file, and stays that way: every other bodiless signature
        /// in the stdlib sits inside a `interface`, where it is an obligation on the implementer
        /// rather than a promise the host runtime must keep.</summary>
        [Test]
        public static void OnlyIntrinsicsFileDeclaresHostIntrinsics()
        {
            var offenders = Directory
                .EnumerateFiles(CheckerTestSupport.FindForwardStdLib(), "*.library.plato", SearchOption.AllDirectories)
                .Where(f => !string.Equals(Path.GetFullPath(f), Path.GetFullPath(IntrinsicsFile)))
                .Where(f => File.ReadAllLines(f)
                    .Where(l => !l.TrimStart().StartsWith("//"))
                    .Any(l => BodilessSignature.IsMatch(l)))
                .ToArray();

            Assert.IsEmpty(offenders,
                "A *.library.plato file other than intrinsics.library.plato declares a bodiless "
                + "signature. A library function needs a body; a host contract belongs in the one "
                + "intrinsics file.");
        }

        /// <summary>Not an assertion — the checklist. Run it to see the contract a new backend
        /// must satisfy.</summary>
        [Test, Explicit]
        public static void SummarizeIntrinsicContract()
        {
            foreach (var s in SignatureLines())
                TestContext.WriteLine(s);
        }
    }
}
