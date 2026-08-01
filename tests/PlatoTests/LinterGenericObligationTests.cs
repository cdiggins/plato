using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// LINT001 on a GENERIC concrete type (plato-376). The obligation is substituted with the
    /// type's own parameter — ColumnCount(Grid&lt;T&gt;):Integer — while the implementation can only
    /// be written over a type variable — ColumnCount(xs: Grid&lt;$T&gt;):Integer. Pairing used to be
    /// a textual lookup on the rendered signature, so no library function could ever discharge
    /// an obligation of a generic type and every one of them generated a throwing member.
    ///
    /// ConcreteType.ImplementationFor now falls back to a one-way positional unification. These
    /// tests pin the match and the two non-matches that keep the fallback from admitting an
    /// unrelated overload.
    /// </summary>
    [TestFixture]
    public static class LinterGenericObligationTests
    {
        // The primitives the compiler assumes, plus a concept whose obligations are keyed by the
        // implementing type's own parameter: one over the parameter as a type ARGUMENT, one that
        // REPEATS the parameter across a parameter and the return type.
        private const string Preamble = @"
type Number { }
type Integer { }
type Boolean { }
type Object { }

concept Extentful<T>
{
    ColumnCount(x: Self): Integer;
    Combine(x: Self, y: T): T;
}

type Grid<T>
    implements Extentful<T>
{
    Item: T;
}
";

        // The candidate is written over a deliberately ODDLY NAMED type variable: the fallback
        // renames by first occurrence, so the name must not matter. 'Combine' is filled by a
        // candidate with TWO distinct variables where the obligation repeats one — a different
        // repetition pattern, which must NOT discharge it.
        private const string GenericCandidates = Preamble + @"
library GridLibrary
{
    ColumnCount(xs: Grid<$Zzz>): Integer
        => 0;

    Combine(xs: Grid<$A>, y: $B): $B
        => y;
}
";

        // A candidate naming a CONCRETE type where the obligation names the type's parameter.
        // It implements Grid<Integer> only, so it may not discharge the obligation of Grid<T>.
        private const string ConcreteCandidate = Preamble + @"
library GridLibrary
{
    ColumnCount(xs: Grid<Integer>): Integer
        => 0;
}
";

        private static string[] UnimplementedNames(string source)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-lint376-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), source);
                var findings = new Linter(CheckerTestSupport.CompileFolder(dir)).Findings
                    .Where(f => f.Code == "LINT001")
                    .ToArray();
                foreach (var f in findings)
                    TestContext.WriteLine(f.ToString());
                return findings.Select(f => f.Message).ToArray();
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        [Test]
        public static void LibraryFunctionOverATypeVariableDischargesAGenericObligation()
            => Assert.That(string.Join(" | ", UnimplementedNames(GenericCandidates)),
                Does.Not.Contain("ColumnCount"),
                "ColumnCount(xs: Grid<$Zzz>) is identical to the obligation ColumnCount(Grid<T>) " +
                "after canonical renaming, so it discharges it whatever the variable is called");

        [Test]
        public static void AnInconsistentVariableRepetitionPatternDoesNotDischarge()
            => Assert.That(string.Join(" | ", UnimplementedNames(GenericCandidates)),
                Does.Contain("Combine"),
                "Combine(Grid<$A>, $B):$B has two independent variables where the obligation " +
                "Combine(Grid<T>, T):T repeats one; the repetition pattern differs, so it is a " +
                "different function and the obligation stays unimplemented");

        [Test]
        public static void AConcreteTypeArgumentDoesNotDischargeAGenericObligation()
            => Assert.That(string.Join(" | ", UnimplementedNames(ConcreteCandidate)),
                Does.Contain("ColumnCount"),
                "the unification is one-way: a candidate VARIABLE may stand in for the " +
                "obligation's type parameter, but ColumnCount(Grid<Integer>) implements one " +
                "instantiation only and may not discharge the obligation of Grid<T>");
    }
}
