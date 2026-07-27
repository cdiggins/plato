using Ara3D.Geometry.Navigation;
using Ara3D.Utils;

namespace Ara3D.Geometry.Navigation.CLI;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            Console.Error.WriteLine("Usage: Plato.Navigation.CLI stats <root> [<root> ...]");
            return 1;
        }

        var verb = args[0];
        var roots = args.Skip(1).Where(a => !a.StartsWith("--")).Select(a => new DirectoryPath(a)).ToList();
        if (roots.Count == 0)
        {
            Console.Error.WriteLine("Missing input root folder.");
            return 1;
        }

        return verb switch
        {
            "stats" => Stats(roots),
            _ => Unknown(verb)
        };
    }

    private static int Unknown(string verb)
    {
        Console.Error.WriteLine($"Unknown verb: {verb}");
        return 1;
    }

    private static int Stats(IReadOnlyList<DirectoryPath> roots)
    {
        var snapshot = SourceSnapshot.FromDirectories(roots);
        var bound = BoundSnapshot.Create(snapshot);

        Console.WriteLine($"files          : {snapshot.Files.Count}");
        Console.WriteLine($"lines          : {snapshot.Files.Sum(f => f.Text.Count(c => c == '\n') + 1)}");
        Console.WriteLine($"generation     : {snapshot.Generation[..12]}");
        Console.WriteLine($"parse          : {bound.ParseTime.TotalMilliseconds:F0} ms ({bound.Files.Count(f => f.Parsed)} ok, {bound.Files.Count(f => !f.Parsed)} failed)");
        Console.WriteLine($"bind           : {bound.BindTime.TotalMilliseconds:F0} ms ({(bound.Bound ? "ok" : "ABORTED: " + bound.BindAbortReason)})");
        Console.WriteLine($"type defs      : {bound.Factory?.TypeDefs.Count ?? 0}");
        Console.WriteLine($"symbols->nodes : {bound.Factory?.SymbolsToNodes.Count ?? 0}");
        Console.WriteLine($"resolution errs: {bound.ResolutionErrors.Count}");

        foreach (var f in bound.Files.Where(f => !f.Parsed))
            Console.WriteLine($"  parse failed: {f.File.Path}");

        return 0;
    }
}
