using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// <see cref="Linter.MembersImplementedByWriter"/> is documented as "kept in sync with
    /// Plato.CSharpWriter CSharpWriter.IgnoredFunctions". The real relationship is a strict
    /// superset: the linter additionally skips At/Count, which the writer SYNTHESIZES from the
    /// type's shape rather than ignoring outright. These tests pin that relationship, so either
    /// set drifting fails here instead of silently changing what LINT001 reports.
    /// </summary>
    public static class LinterWriterSyncTests
    {
        public static readonly string[] SynthesizedByWriter = { "At", "Count" };

        [Test]
        public static void LinterSkipsEveryFunctionTheWriterIgnores()
            => CollectionAssert.IsSubsetOf(CSharpWriter.IgnoredFunctions, Linter.MembersImplementedByWriter,
                "CSharpWriter.IgnoredFunctions gained an entry that Linter.MembersImplementedByWriter does not " +
                "skip; LINT001 will now report a member the writer never emits.");

        [Test]
        public static void LinterSkipsNothingBeyondTheSynthesizedMembers()
            => CollectionAssert.AreEquivalent(SynthesizedByWriter,
                Linter.MembersImplementedByWriter.Except(CSharpWriter.IgnoredFunctions).ToArray(),
                "Linter.MembersImplementedByWriter may exceed CSharpWriter.IgnoredFunctions only by the members " +
                "the writer synthesizes from a type's shape (At/Count); anything else suppresses a real LINT001.");
    }
}
