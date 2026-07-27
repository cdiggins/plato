using Ara3D.Utils;

namespace Ara3D.Geometry.Navigation.CLI;

public static class Program
{
    private const string Usage = """
        Usage: Plato.Navigation.CLI <verb> [args] --root <dir> [--root <dir> ...]

          stats                          index the roots and print counts and timings
          search <name> [--kind K]       find definitions by name (K = exact|prefix|all)
          outline <file>                 list every definition in one file, in source order
          def <file> <line> <col>        definitions of the symbol at a position (0-based)
          refs <file> <line> <col>       references to the symbol at a position (0-based)
        """;

    public static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            Console.Error.WriteLine(Usage);
            return 1;
        }

        var roots = Options(args, "--root").Select(r => new DirectoryPath(r)).ToList();
        if (roots.Count == 0)
        {
            Console.Error.WriteLine("Missing --root.");
            return 1;
        }

        var positional = Positional(args);
        var verb = args[0];
        if (verb == "stats")
            return Stats(roots);

        var index = NavigationIndex.Build(SourceSnapshot.FromDirectories(roots));
        return verb switch
        {
            "search" => Search(index, positional, Option(args, "--kind")),
            "outline" => Outline(index, positional),
            "def" => Def(index, positional),
            "refs" => Refs(index, positional),
            _ => Fail($"Unknown verb: {verb}")
        };
    }

    private static int Stats(IReadOnlyList<DirectoryPath> roots)
    {
        var snapshot = SourceSnapshot.FromDirectories(roots);
        var bound = BoundSnapshot.Create(snapshot);
        var index = NavigationIndex.Build(bound);

        Console.WriteLine($"files          : {snapshot.Files.Count}");
        Console.WriteLine($"lines          : {snapshot.Files.Sum(f => f.Text.Count(c => c == '\n') + 1)}");
        Console.WriteLine($"generation     : {snapshot.Generation[..12]}");
        Console.WriteLine($"parse          : {bound.ParseTime.TotalMilliseconds:F0} ms ({bound.Files.Count(f => f.Parsed)} ok, {bound.Files.Count(f => !f.Parsed)} failed)");
        Console.WriteLine($"bind           : {bound.BindTime.TotalMilliseconds:F0} ms ({(bound.Bound ? "ok" : "ABORTED: " + bound.BindAbortReason)})");
        Console.WriteLine($"type defs      : {bound.Factory?.TypeDefs.Count ?? 0}");
        Console.WriteLine($"definitions    : {index.Defs.Count}");
        Console.WriteLine($"references     : {index.Refs.Count} ({index.Refs.Count(r => r.Kind == RefKind.Type)} type, {index.Refs.Count(r => r.Kind == RefKind.Value)} value)");
        Console.WriteLine($"diagnostics    : {index.Diagnostics.Count}");

        foreach (var kind in Enum.GetValues<DefKind>())
        {
            var n = index.Defs.Count(d => d.Kind == kind);
            if (n > 0)
                Console.WriteLine($"  {kind,-14}: {n}");
        }

        foreach (var d in index.Diagnostics.Take(20))
            Console.WriteLine($"  ! {d}");

        return 0;
    }

    private static int Search(NavigationIndex index, IReadOnlyList<string> positional, string? kind)
    {
        if (positional.Count < 1)
            return Fail("search needs a name");

        var searchKind = kind switch
        {
            "exact" => SearchKind.Exact,
            "prefix" => SearchKind.Prefix,
            _ => SearchKind.All
        };

        foreach (var d in index.Search(positional[0], searchKind))
            Console.WriteLine(Format(index, d));
        return 0;
    }

    private static int Outline(NavigationIndex index, IReadOnlyList<string> positional)
    {
        if (positional.Count < 1)
            return Fail("outline needs a file");

        foreach (var d in index.Outline(new FilePath(positional[0])))
            Console.WriteLine(Format(index, d));
        return 0;
    }

    private static int Def(NavigationIndex index, IReadOnlyList<string> positional)
    {
        var hit = HitAt(index, positional);
        if (hit == null)
            return Fail("nothing at that position");

        foreach (var d in index.GetDefinitions(hit))
            Console.WriteLine(Format(index, d));
        return 0;
    }

    private static int Refs(NavigationIndex index, IReadOnlyList<string> positional)
    {
        var hit = HitAt(index, positional);
        if (hit == null)
            return Fail("nothing at that position");

        foreach (var d in index.GetDefinitions(hit))
        foreach (var r in index.FindReferences(d.Id))
            Console.WriteLine($"{index.Files[r.FileId].Path}:{r.Span.BeginLine + 1}:{r.Span.BeginColumn + 1}\t{r.Kind}\t{r.Name}");
        return 0;
    }

    private static SymbolHit? HitAt(NavigationIndex index, IReadOnlyList<string> positional)
        => positional.Count < 3
            ? null
            : index.FindAt(new FilePath(positional[0]), int.Parse(positional[1]), int.Parse(positional[2]));

    private static string Format(NavigationIndex index, DefRecord d)
        => $"{index.Files[d.FileId].Path}:{d.NameSpan.BeginLine + 1}:{d.NameSpan.BeginColumn + 1}\t{d.Kind}\t{d.Signature ?? d.Name}";

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 1;
    }

    private static IReadOnlyList<string> Options(IReadOnlyList<string> args, string name)
        => args.Select((a, i) => (a, i)).Where(t => t.a == name && t.i + 1 < args.Count)
            .Select(t => args[t.i + 1]).ToList();

    private static string? Option(IReadOnlyList<string> args, string name)
        => Options(args, name).FirstOrDefault();

    /// <summary>Everything after the verb that is neither a flag nor a flag's value.</summary>
    private static IReadOnlyList<string> Positional(IReadOnlyList<string> args)
    {
        var result = new List<string>();
        for (var i = 1; i < args.Count; i++)
        {
            if (args[i].StartsWith("--"))
            {
                i++;
                continue;
            }
            result.Add(args[i]);
        }
        return result;
    }
}
