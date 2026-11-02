// Defines the accepted generation option spellings at the command boundary.
namespace Ara3D.Geometry.CLI;

internal static class GenerationArguments
{
    private static readonly HashSet<string> Switches = new(StringComparer.Ordinal)
    {
        "--typescript", "--rust", "--glsl", "--cpp", "--cuda",
        "--optimize", "--optimize-arrays", "--inline", "--static-abstract", "--inline-report",
        "--cpp-borrow-reads", "--cpp-borrow-nontrivial"
    };

    private static readonly HashSet<string> ValueOptions = new(StringComparer.Ordinal)
    {
        "--dump-tir", "--csharp-style", "--profile", "--out", "--inline-policy", "--inline-growth", "--cpp-roots", "--cpp-borrow-min-bytes"
    };

    public static string? UnknownOption(IReadOnlyList<string> args)
        => args.FirstOrDefault(arg => arg.StartsWith("--", StringComparison.Ordinal) && !IsKnown(arg));

    private static bool IsKnown(string arg)
    {
        var separator = arg.IndexOf('=');
        return Switches.Contains(arg) || WriterSwitches.IsSpelling(arg) || separator >= 0 && ValueOptions.Contains(arg[..separator]);
    }
}
