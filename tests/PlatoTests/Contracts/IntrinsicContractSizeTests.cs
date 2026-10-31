// Enforces the declared intrinsic kernel contract.
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
        // A ceiling to LOWER, never to raise without a stated reason. The reasons so far:
        //   TKT-1125 (75 to 76): Cbrt, a correctly rounded host function that Pow(x, 1/3) is not.
        //   TKT-1143 (76 to 83): Bytes, the byte-sequence primitive the STEP front end reads (the owner asked for
        //   the type). Seven: Count, At, SubBytes and FromIntegers are its kernel, as for Array; LessThanOrEquals
        //   is operator-named, so the C# writer wraps the runtime's operator (Equals, Compare and LessThan are
        //   derived from it); NumberFromBytes is a correctly rounded decimal-to-binary conversion; and Hash is
        //   expressible over At but needs a 0xFFFFFFFF mask for FNV-1a that a 32-bit Integer cannot spell, so a
        //   Plato body would differ between profiles. ToBytes, Slice, ParseNumber and ParseInteger are
        //   Plato (bytes.library.plato).
        //   TKT-1143, P7 (83 to 84): ToText, Bytes to String in one allocation and one copy. Expressible over At,
        //   but as a closure call per byte and a character array first; the STEP parser converts every string,
        //   binary and enumeration value (5.6% inclusive on PAM.STEP). The owner's request for Bytes authorizes it.
        private static readonly int MaxIntrinsics = CheckerTestSupport.RatchetCeiling("intrinsics");

        /// <summary>A bodiless signature: `Name(params): ReturnType;` at library-member indent,
        /// with no body syntax. Excluding conditionals and arrows from the parameter portion
        /// prevents a call in a function body from consuming the colon in a ternary expression.
        /// Comment lines are stripped first so quoted signatures cannot inflate the count.</summary>
        private static readonly Regex BodilessSignature = new Regex(
            @"^\s+[A-Za-z_]\w*\s*\([^;?={}]*\)\s*:\s*[^;=?{}]+;\s*$", RegexOptions.Compiled);

        private static string IntrinsicsFile
            => Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation", "intrinsics.library.plato");

        private static string[] SignatureLines()
            => File.ReadAllLines(IntrinsicsFile)
                .Where(l => !l.TrimStart().StartsWith("//"))
                .Where(l => BodilessSignature.IsMatch(l))
                .Select(l => l.Trim())
                .ToArray();

        [TestCase("    Add(a: Number, b: Number): Number;", true)]
        [TestCase("    Empty(): Array<Number>;", true)]
        [TestCase("    Map(xs: Array<T>, f: Function<T, U>): Array<U>;", true)]
        [TestCase("    Value(): Number => Other();", false)]
        [TestCase("    Value(): Number { return Other(); }", false)]
        [TestCase("    SameContact(acc, old, tolerance) ? WithImpulsesOf(acc, old) : acc));", false)]
        [TestCase("    Check(x) ? Wrap(Other(x)) : fallback;", false)]
        [TestCase("    Map(xs, x => Check(x) ? Wrap(x) : x);", false)]
        public static void BodilessSignaturesExcludeFunctionBodies(string line, bool expected)
            => Assert.That(BodilessSignature.IsMatch(line), Is.EqualTo(expected));

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
        [Test, Category("Diagnostic"), Explicit]
        public static void SummarizeIntrinsicContract()
        {
            foreach (var s in SignatureLines())
                TestContext.WriteLine(s);
        }
    }
}
