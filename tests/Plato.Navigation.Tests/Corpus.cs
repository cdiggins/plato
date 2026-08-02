using Ara3D.Geometry.Navigation;
using Ara3D.Utils;

namespace Plato.Navigation.Tests;

/// <summary>The gated corpus (D9): the FORWARD stdlib plus its law/witness libraries, parsed,
/// bound and indexed exactly once for the whole test run.
///
/// Deliberately not `stdlib-legacy`: the frozen vocabulary is not what anyone edits, so gating on
/// it proves the index against a corpus no longer representative of what agents query. Tests here
/// must therefore assert on SHAPE (ordering, span validity, resolution) rather than on particular
/// declarations, since the forward vocabulary is actively growing.</summary>
public static class Corpus
{
    public static DirectoryPath RepoRoot { get; } = FindRepoRoot();

    /// <summary>One root: the law/witness packet moved to `stdlib/tests/` (stdlib-398) and
    /// <see cref="SourceSnapshot.FromDirectories"/> recurses, so `stdlib` already covers it. Naming
    /// both would hand the snapshot the same file twice, which is a duplicate-key throw, not a
    /// no-op.</summary>
    public static IReadOnlyList<DirectoryPath> Roots { get; } = new[]
    {
        RepoRoot.RelativeFolder("stdlib")
    };

    private static readonly Lazy<BoundSnapshot> _bound =
        new(() => BoundSnapshot.Create(SourceSnapshot.FromDirectories(Roots)));

    private static readonly Lazy<NavigationIndex> _index =
        new(() => NavigationIndex.Build(_bound.Value));

    public static BoundSnapshot Bound => _bound.Value;
    public static NavigationIndex Index => _index.Value;

    public static string TextOf(int fileId) => Index.Snapshot.Files[fileId].Text;

    private static DirectoryPath FindRepoRoot()
    {
        var dir = new DirectoryPath(AppContext.BaseDirectory);
        for (var i = 0; i < 12 && dir.Value != null; i++)
        {
            if (dir.RelativeFolder("stdlib").Exists())
                return dir;
            dir = dir.GetParent();
        }
        throw new DirectoryNotFoundException("Could not locate the Plato repo root (no stdlib folder above the test binary).");
    }
}
