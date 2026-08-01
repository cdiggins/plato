using Ara3D.Geometry.Navigation;
using Ara3D.Utils;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

/// <summary>The safety clause of the plato_check seam: feeding the parse cache's ASTs to a full
/// <see cref="Ara3D.Geometry.Compiler.Compilation"/> must not change what the index would build —
/// the binder and the compilation each write only to their own symbols, never to AST nodes.</summary>
[TestFixture]
public static class CheckSupportTests
{
    private static SourceSnapshot Snapshot()
        => SourceSnapshot.FromTexts(new Dictionary<FilePath, string>
        {
            [new FilePath("mem/points.types.plato")] =
                "// A 2D point.\ntype Point2D\n{\n    X: Number;\n    Y: Number;\n}\n",
            [new FilePath("mem/points.library.plato")] =
                "// Derived point ops.\nlibrary Points\n{\n    Swap(p: Point2D): Point2D => Point2D(p.Y, p.X);\n}\n"
        });

    [Test]
    public static void CompileDoesNotPerturbTheIndex()
    {
        var indexer = new IncrementalIndexer();
        var snapshot = Snapshot();

        var before = indexer.Update(snapshot);
        var beforeDefs = before.Defs.Select(d => d.ToString()).ToList();

        // A two-file corpus has no primitives (Number lives in the stdlib intrinsics), so the
        // compilation will not COMPLETE — irrelevant here: the clause under test is that running
        // one leaves the cached ASTs, and therefore the index, untouched.
        CheckSupport.Compile(snapshot, indexer.Cache);

        var after = indexer.Update(snapshot);
        Assert.AreEqual(before.Generation, after.Generation);
        Assert.AreEqual(beforeDefs, after.Defs.Select(d => d.ToString()).ToList(),
            "rebinding the same cached ASTs after a Compilation must reproduce the index exactly");
    }

    [Test]
    public static void CompileReusesTheParseCache()
    {
        var cache = new ParseCache();
        var snapshot = Snapshot();

        CheckSupport.Parse(snapshot, cache);
        var misses = cache.Misses;

        CheckSupport.Compile(snapshot, cache);
        Assert.AreEqual(misses, cache.Misses, "a warm check must not reparse anything");
    }
}
