// The C# writer's optimisation switches: one table of names, defaults and dependencies, and the
// resolution of --x / --no-x spellings against it, shared by generate and run.
using Ara3D.Geometry.CSharpWriter;

namespace Ara3D.Geometry.CLI;

/// <summary>One writer switch: its name without dashes, whether it is on by default for C# output,
/// and the switch it depends on (null for none).</summary>
public sealed record WriterSwitch(string Name, bool Default, string? Requires = null);

/// <summary>
/// The writer switches. Each is accepted as <c>--name</c> and <c>--no-name</c>; the last of the two
/// wins. Defaults apply only when the target is C# (generate without another writer, or run with
/// the csharp or differential backend); elsewhere every switch starts off, so a default never turns
/// into an error. A defaulted switch whose dependency is off is off too, silently; a switch the
/// user passed explicitly stays on and the command's own checks report the missing dependency.
/// <c>--no-writer-defaults</c> turns every default off (the output before defaults existed); an
/// explicit <c>--name</c> still turns that one switch on. Adding a switch is adding one row to
/// <see cref="Table"/> and one assignment in <see cref="ToLoopOptions"/>.
/// </summary>
public static class WriterSwitches
{
    public const string Umbrella = "--no-writer-defaults";

    // Order matters: a switch comes after the one it requires.
    public static readonly IReadOnlyList<WriterSwitch> Table = new WriterSwitch[]
    {
        new("loops", true),
        new("fuse", true),
        new("fuse-consumers", true, "loops"),
        new("exact-output", true, "loops"),
        new("lean-inlining", true),
        new("share-calls", true),
        new("lower-scalars", true),
        new("concrete-arrays", true),
        new("presize", true),
        new("in-place", true),
        new("sink-bindings", true),
        new("bind-scrutinee", true),
        new("share-calls-wide", true),
        new("closure-free", true),
        new("in-params", true),
        new("switch-match", true),
        new("hoist-invariants", true, "loops"),
        new("cache-constants", true),
        new("primitive-compare", true),
        new("lower-more", true, "loops"),
        new("specialize-callbacks", true),
        new("specialize-callbacks-lean", false, "specialize-callbacks"),
        // TKT-1152: off by default until each is measured and pinned in plato-brep.
        new("scalar-arrays", false),
        new("owned-arrays", false),
        new("stack-arrays", false, "owned-arrays"),
        new("span-params", false, "owned-arrays"),
    };

    public static bool IsSwitch(string name) => Table.Any(s => s.Name == name);

    /// <summary>The names the unknown-option check accepts for the table: <c>--x</c>, <c>--no-x</c> and the umbrella.</summary>
    public static bool IsSpelling(string arg)
        => arg == Umbrella
           || arg.StartsWith("--no-", StringComparison.Ordinal) && IsSwitch(arg[5..])
           || arg.StartsWith("--", StringComparison.Ordinal) && IsSwitch(arg[2..]);

    /// <summary>The first <c>--no-</c> option that names no writer switch, or null.</summary>
    public static string? UnknownNegation(IEnumerable<string> args)
        => args.FirstOrDefault(a => a.StartsWith("--no-", StringComparison.Ordinal) && !IsSpelling(a));

    /// <summary>Resolves the switches of an argument list. <paramref name="csharpTarget"/> says whether the
    /// output is C# (defaults apply) or something else (every switch starts off).</summary>
    public static ResolvedWriterSwitches Resolve(IEnumerable<string> args, bool csharpTarget)
    {
        var list = args.ToList();
        var defaults = csharpTarget && !list.Contains(Umbrella);
        var explicitState = new Dictionary<string, bool>();
        foreach (var a in list)
        {
            if (!a.StartsWith("--", StringComparison.Ordinal)) continue;
            if (a.StartsWith("--no-", StringComparison.Ordinal) && IsSwitch(a[5..])) explicitState[a[5..]] = false;
            else if (IsSwitch(a[2..])) explicitState[a[2..]] = true;
        }
        var on = new HashSet<string>();
        foreach (var s in Table)
        {
            var value = explicitState.TryGetValue(s.Name, out var e)
                ? e
                : defaults && s.Default && (s.Requires == null || on.Contains(s.Requires));
            if (value) on.Add(s.Name);
        }
        return new ResolvedWriterSwitches(on);
    }

    /// <summary>The loop-optimisation options the resolved switches select (--loops and --fuse are read separately).</summary>
    public static LoopOptimizationOptions ToLoopOptions(ResolvedWriterSwitches s)
        => new(s["fuse-consumers"], s["exact-output"], s["concrete-arrays"], s["lean-inlining"], s["presize"], s["in-place"], s["share-calls"],
            LowerScalars: s["lower-scalars"],
            SinkBindings: s["sink-bindings"], BindScrutinees: s["bind-scrutinee"], ShareCallsWide: s["share-calls-wide"],
            ClosureFree: s["closure-free"],
            HoistInvariants: s["hoist-invariants"], CacheConstants: s["cache-constants"],
            PrimitiveCompare: s["primitive-compare"])
        {
            InParameters = s["in-params"],
            SwitchMatch = s["switch-match"],
            LowerMore = s["lower-more"],
            SpecializeCallbacks = s["specialize-callbacks"],
            SpecializeCallbacksLean = s["specialize-callbacks-lean"],
            ScalarArrays = s["scalar-arrays"],
            OwnedArrays = s["owned-arrays"],
            StackArrays = s["stack-arrays"],
            SpanParams = s["span-params"],
        };
}

/// <summary>The switches that are on after defaults, dependencies and explicit spellings.</summary>
public sealed class ResolvedWriterSwitches
{
    private readonly HashSet<string> _on;
    internal ResolvedWriterSwitches(HashSet<string> on) => _on = on;

    public bool this[string name]
        => WriterSwitches.IsSwitch(name) ? _on.Contains(name) : throw new ArgumentException($"Not a writer switch: {name}");
}
