// The writers' optimisation switches: one table of names, the targets each applies to, per-target defaults,
// dependencies and parameters, and the resolution of --x / --no-x / --x=value spellings against it, shared by
// generate and run.
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Optimizer;

namespace Ara3D.Geometry.CLI;

/// <summary>The outputs a switch can apply to. <see cref="Cpu"/> is <c>--cpp --profile=cpu64i32</c>;
/// <see cref="Other"/> is every other writer (TypeScript, Rust, GLSL, plain C++, CUDA).</summary>
[Flags]
public enum WriterTarget
{
    None = 0,
    CSharp = 1,
    Cpu = 2,
    Other = 4,
}

/// <summary>The value a parameterised switch takes (<c>--name=value</c>): what it must be, as the error names it.</summary>
public sealed record SwitchParameter(string Expectation, Func<string, bool> IsValid)
{
    public static SwitchParameter PositiveInt { get; } = new("a positive integer", v => int.TryParse(v, out var n) && n >= 1);
    public static SwitchParameter Text { get; } = new("a value", v => v.Length > 0);
    public static SwitchParameter AnyText { get; } = new("a value", _ => true);
}

/// <summary>One writer switch: its name without dashes, the targets it applies to, the targets it is on by default
/// for, the switch it depends on (null for none), and for a parameterised switch the value it takes.</summary>
public sealed record WriterSwitch(string Name, WriterTarget Targets, WriterTarget Defaults = WriterTarget.None, string? Requires = null)
{
    /// <summary>Non-null for a switch spelled <c>--name=value</c>; it is on when a value is given.</summary>
    public SwitchParameter? Parameter { get; init; }

    /// <summary>On by default for C# output.</summary>
    public bool Default => Defaults.HasFlag(WriterTarget.CSharp);
}

/// <summary>
/// The writer switches. Each is accepted as <c>--name</c> and <c>--no-name</c> (a parameterised one as
/// <c>--name=value</c>, and <c>--no-name</c> to drop a value given earlier); the last spelling wins. A switch is on
/// by default for the targets in its <see cref="WriterSwitch.Defaults"/>; elsewhere it starts off. A defaulted
/// switch whose dependency is off is off too, silently; a switch the user passed explicitly stays on, and
/// <see cref="Check"/> reports the missing dependency or a target the switch does not apply to.
/// <c>--no-writer-defaults</c> turns every default off (the output before defaults existed); an explicit
/// <c>--name</c> still turns that one switch on.
/// <para>Adding a switch is one row in <see cref="Table"/> plus the pass or emitter that reads it through
/// <see cref="SwitchSet.On"/> (a pass: <c>ctx.On("name")</c>; a parameterised switch: <c>ctx.Switches.Int("name", fallback)</c>).
/// A C++ switch names <see cref="WriterTarget.Cpu"/> in its targets; a pass already in both backends' lists that
/// becomes valid for C++ needs only <see cref="WriterTarget.Cpu"/> added to its row.</para>
/// </summary>
public static class WriterSwitches
{
    public const string Umbrella = "--no-writer-defaults";

    private const WriterTarget CS = WriterTarget.CSharp;
    private const WriterTarget CsCpu = WriterTarget.CSharp | WriterTarget.Cpu;
    private const WriterTarget Cpu = WriterTarget.Cpu;

    // Order matters: a switch comes after the one it requires.
    public static readonly IReadOnlyList<WriterSwitch> Table = new WriterSwitch[]
    {
        new("loops", CsCpu, CS),
        new("fuse", CsCpu, CS),
        new("fuse-consumers", CsCpu, CS, "loops"),
        new("exact-output", CsCpu, CS, "loops"),
        new("lean-inlining", CS, CS),
        new("share-calls", CsCpu, CS),
        new("lower-scalars", CS, CS),
        new("concrete-arrays", CS, CS),
        new("presize", CS, CS),
        new("in-place", CS, CS),
        new("sink-bindings", CS, CS),
        new("bind-scrutinee", CS, CS),
        new("share-calls-wide", CS, CS),
        new("closure-free", CS, CS),
        new("in-params", CS, CS),
        new("switch-match", CS, CS),
        new("hoist-invariants", CS, CS, "loops"),
        new("cache-constants", CS, CS),
        new("primitive-compare", CS, CS),
        new("lower-more", CS, CS, "loops"),
        new("specialize-callbacks", CS, CS),
        new("specialize-callbacks-lean", CS, Requires: "specialize-callbacks"),
        // TKT-1152: off by default until each is measured and pinned in plato-brep.
        new("scalar-arrays", CS),
        new("owned-arrays", CS),
        new("stack-arrays", CS, Requires: "owned-arrays"),
        new("span-params", CS, Requires: "owned-arrays"),
        // C++ (Cpu dialect) only. --cpp-roots names the functions whose reachable set must all be emitted.
        new("cpp-roots", Cpu) { Parameter = SwitchParameter.AnyText },
        // Native copy removal (off by default; see CppCpuOptions): the parameter borrow rule and borrowed reads.
        new("cpp-borrow-params", Cpu),
        new("cpp-borrow-reads", Cpu),
        new("cpp-borrow-nontrivial", Cpu),
        // The record size at or over which a read-only parameter borrows; CppCpuOptions.BorrowMinBytes (256) when absent.
        new("cpp-borrow-min-bytes", Cpu) { Parameter = SwitchParameter.PositiveInt },
        // Sum layout (off by default): a chain of case predicates on one sum becomes switch (Kind); a sum is Kind plus one union of per-case structs.
        new("cpp-sum-switch", Cpu),
        new("cpp-sum-compact", Cpu),
        // The most lines of definitions in one emitted function unit; CppCpuOptions.DefaultUnitLines (5000) when absent.
        new("cpp-unit-lines", Cpu) { Parameter = SwitchParameter.PositiveInt },
    };

    private static WriterSwitch? Find(string name) => Table.FirstOrDefault(s => s.Name == name);

    public static bool IsSwitch(string name) => Find(name) != null;

    /// <summary>The names the unknown-option check accepts for the table: <c>--x</c>, <c>--no-x</c>, <c>--x=value</c> for a
    /// parameterised switch, and the umbrella.</summary>
    public static bool IsSpelling(string arg)
    {
        if (arg == Umbrella) return true;
        if (!arg.StartsWith("--", StringComparison.Ordinal)) return false;
        if (arg.StartsWith("--no-", StringComparison.Ordinal) && IsSwitch(arg[5..])) return true;
        var eq = arg.IndexOf('=');
        var row = Find(eq < 0 ? arg[2..] : arg[2..eq]);
        return row != null && (eq < 0) == (row.Parameter == null);
    }

    /// <summary>The first <c>--no-</c> option that names no writer switch, or null.</summary>
    public static string? UnknownNegation(IEnumerable<string> args)
        => args.FirstOrDefault(a => a.StartsWith("--no-", StringComparison.Ordinal) && !IsSpelling(a));

    /// <summary>Resolves the switches of an argument list. <paramref name="csharpTarget"/> says whether the
    /// output is C# (C# defaults apply) or something else (every switch starts off).</summary>
    public static ResolvedWriterSwitches Resolve(IEnumerable<string> args, bool csharpTarget)
        => Resolve(args, csharpTarget ? WriterTarget.CSharp : WriterTarget.Other);

    /// <summary>Resolves the switches of an argument list for one target: its defaults, then the explicit spellings.</summary>
    public static ResolvedWriterSwitches Resolve(IEnumerable<string> args, WriterTarget target)
    {
        var list = args.ToList();
        var defaults = !list.Contains(Umbrella);
        var explicitState = new Dictionary<string, bool>();
        var values = new Dictionary<string, string>();
        foreach (var a in list)
        {
            if (!a.StartsWith("--", StringComparison.Ordinal)) continue;
            if (a.StartsWith("--no-", StringComparison.Ordinal) && IsSwitch(a[5..]))
            {
                explicitState[a[5..]] = false;
                values.Remove(a[5..]);
                continue;
            }
            var eq = a.IndexOf('=');
            var row = Find(eq < 0 ? a[2..] : a[2..eq]);
            if (row == null || (eq < 0) != (row.Parameter == null)) continue;
            explicitState[row.Name] = true;
            if (eq >= 0) values[row.Name] = a[(eq + 1)..];
        }
        var on = new HashSet<string>();
        foreach (var s in Table.Where(s => s.Parameter == null))
        {
            var value = explicitState.TryGetValue(s.Name, out var e)
                ? e
                : defaults && s.Defaults.HasFlag(target) && (s.Requires == null || on.Contains(s.Requires));
            if (value) on.Add(s.Name);
        }
        return new ResolvedWriterSwitches(new SwitchSet(on, values));
    }

    /// <summary>The first problem with the resolved switches for <paramref name="target"/>, or null: a switch on that does
    /// not apply to the target, a switch on whose dependency is off, or a parameter value of the wrong form.</summary>
    public static string? Check(ResolvedWriterSwitches resolved, WriterTarget target)
    {
        foreach (var s in Table.Where(s => resolved.Switches.On(s.Name)))
        {
            if (!s.Targets.HasFlag(target))
                return $"--{s.Name} applies to {TargetNames(s.Targets)} generation only.";
            if (s.Requires != null && !resolved.Switches.On(s.Requires))
                return $"--{s.Name} requires --{s.Requires}.";
            if (s.Parameter != null && !s.Parameter.IsValid(resolved.Switches.Value(s.Name)!))
                return $"--{s.Name} requires {s.Parameter.Expectation}.";
        }
        return null;
    }

    private static string TargetNames(WriterTarget targets)
        => string.Join(" and ", new[] { (WriterTarget.CSharp, "C#"), (WriterTarget.Cpu, "C++ (--cpp --profile=cpu64i32)"), (WriterTarget.Other, "other") }
            .Where(t => targets.HasFlag(t.Item1)).Select(t => t.Item2));

    /// <summary>The loop-optimisation options the resolved switches select; they carry the whole set too.</summary>
    public static LoopOptimizationOptions ToLoopOptions(ResolvedWriterSwitches s)
        => LoopOptimizationOptions.From(s.Switches);
}

/// <summary>The switches that are on after defaults, dependencies and explicit spellings.</summary>
public sealed class ResolvedWriterSwitches
{
    internal ResolvedWriterSwitches(SwitchSet switches) => Switches = switches;

    /// <summary>The resolved set, as writers and passes read it.</summary>
    public SwitchSet Switches { get; }

    public bool this[string name]
        => WriterSwitches.IsSwitch(name) ? Switches.On(name) : throw new ArgumentException($"Not a writer switch: {name}");

    /// <summary>The value of a parameterised switch, or null when it was not given.</summary>
    public string? Value(string name)
        => WriterSwitches.IsSwitch(name) ? Switches.Value(name) : throw new ArgumentException($"Not a writer switch: {name}");
}
