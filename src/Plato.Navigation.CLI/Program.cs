using System.Diagnostics;
using Ara3D.Utils;

namespace Ara3D.Geometry.Navigation.CLI;

public static class Program
{
    private const string Usage = """
        Usage: Plato.Navigation.CLI <verb> [args] --root <dir> [--root <dir> ...]

          serve                          long-lived NDJSON stdio host for IDE clients
          stats                          index the roots and print counts and timings
          bench                          v1 rebuild vs v2 incremental update, best of two runs
          search <name> [--kind K]       find definitions by name (K = exact|prefix|all)
          outline <file>                 list every definition in one file, in source order
          def <file> <line> <col>        definitions of the symbol at a position (0-based)
          refs <file> <line> <col>       references to the symbol at a position (0-based)
          export <out.json>              write the whole index, sources included, as JSON
          simplify [--code C] [--write]  propose (or apply) redundancy-removing rewrites
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
        if (verb == "serve")
            return Serve.Run(roots);
        if (verb == "stats")
            return Stats(roots);
        if (verb == "bench")
            return Bench(roots);
        if (verb == "simplify")
            return Simplify(roots, Option(args, "--code"), args.Contains("--write"));

        var index = NavigationIndex.Build(SourceSnapshot.FromDirectories(roots));
        return verb switch
        {
            "search" => Search(index, positional, Option(args, "--kind")),
            "outline" => Outline(index, positional),
            "def" => Def(index, positional),
            "refs" => Refs(index, positional),
            "export" => Export(index, positional),
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

    /// <summary>Best of two runs per scenario — this machine is rarely quiet enough for a single
    /// sample to mean anything. Every scenario builds its snapshot from text already in memory, so
    /// the numbers are parse + bind + index and never disk.</summary>
    private static int Bench(IReadOnlyList<DirectoryPath> roots)
    {
        var texts = SourceSnapshot.FromDirectories(roots).Files.ToDictionary(f => f.Path, f => f.Text);
        var victim = texts.Keys.OrderBy(SourceSnapshot.PathKey, StringComparer.Ordinal).First();
        var baseline = SourceSnapshot.FromTexts(texts);

        Console.WriteLine($"files          : {baseline.Files.Count}");
        Console.WriteLine($"lines          : {baseline.Files.Sum(f => f.Text.Count(c => c == '\n') + 1)}");

        Report("v1 build (cold or reload)", () => NavigationIndex.Build(SourceSnapshot.FromTexts(texts)));
        Report("v2 cold build", () => new IncrementalIndexer().Update(SourceSnapshot.FromTexts(texts)));

        var indexer = new IncrementalIndexer();
        indexer.Update(baseline);
        Report("v2 reload, no changes", () => indexer.Update(SourceSnapshot.FromTexts(texts)));

        var n = 0;
        Report("v2 reload, 1 file changed", () =>
            indexer.Update(SourceSnapshot.FromTexts(
                new Dictionary<FilePath, string>(texts) { [victim] = texts[victim] + $"\n// {n++}\n" })));

        var last = indexer.LastUpdate;
        Console.WriteLine($"  last update    : parsed {last.FilesParsed} of {last.Files}, reused {last.FilesReused}, "
                          + $"parse {last.ParseTime.TotalMilliseconds:F0} ms, bind {last.BindTime.TotalMilliseconds:F0} ms");
        return 0;
    }

    private static void Report(string label, Action run)
    {
        var best = double.MaxValue;
        for (var i = 0; i < 2; i++)
        {
            var timer = Stopwatch.StartNew();
            run();
            best = Math.Min(best, timer.Elapsed.TotalMilliseconds);
        }
        Console.WriteLine($"{label,-28}: {best,7:F0} ms");
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

    private static int Export(NavigationIndex index, IReadOnlyList<string> positional)
    {
        if (positional.Count < 1)
            return Fail("export needs an output path");

        var json = NavigationJson.Write(index);
        File.WriteAllText(positional[0], json);
        Console.WriteLine($"{positional[0]}: {json.Length} bytes, {index.Defs.Count} defs, {index.Refs.Count} refs");
        return 0;
    }

    /// <summary>Preview by default; <c>--write</c> rewrites the files in place. Every edit is
    /// re-validated against the text read back from disk, so a stale offset throws rather than
    /// corrupting a file. Verify with the check gates afterwards.</summary>
    private static int Simplify(IReadOnlyList<DirectoryPath> roots, string? code, bool write)
    {
        var parsed = CheckSupport.Parse(SourceSnapshot.FromDirectories(roots), new ParseCache());
        var edits = Simplifier.Check(parsed)
            .Where(e => code == null || string.Equals(e.Code, code, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var e in edits)
            Console.WriteLine(e.Applicable
                ? $"{e.File}:{e.Line}:{e.Column}\t{e.Code}\t{OneLine(e.Before)}\t=>\t{OneLine(e.After)}"
                : $"{e.File}:{e.Line}:{e.Column}\t{e.Code}\t{OneLine(e.Before)}\treport\t{e.Message}");

        if (write)
            foreach (var group in edits.GroupBy(e => e.File))
            {
                var text = File.ReadAllText(group.Key);
                var next = Simplifier.Apply(text, group.ToList());
                if (next != text)
                    File.WriteAllText(group.Key, next);
            }

        var reports = edits.Count(e => !e.Applicable);
        Console.WriteLine($"{edits.Count - reports} edit(s) and {reports} report(s) in {edits.Select(e => e.File).Distinct().Count()} file(s)"
                          + (write ? ", edits written" : ", preview only (--write applies the edits; reports are never applied)"));
        return 0;
    }

    private static string OneLine(string text)
        => string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

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
