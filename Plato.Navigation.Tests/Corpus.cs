using Ara3D.Utils;

namespace Ara3D.Geometry.Navigation.Tests;

/// <summary>The gated corpus (D9): the production stdlib plus the law/witness libraries, parsed,
/// bound and indexed exactly once for the whole test run.</summary>
public static class Corpus
{
    public static DirectoryPath RepoRoot { get; } = FindRepoRoot();

    public static IReadOnlyList<DirectoryPath> Roots { get; } = new[]
    {
        RepoRoot.RelativeFolder("stdlib-legacy"),
        RepoRoot.RelativeFolder("stdlib-legacy-tests")
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
            if (dir.RelativeFolder("stdlib-legacy").Exists())
                return dir;
            dir = dir.GetParent();
        }
        throw new DirectoryNotFoundException("Could not locate the Plato repo root (no stdlib-legacy above the test binary).");
    }
}
