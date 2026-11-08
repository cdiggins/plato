// What an optimizer pass may read: the compilation, the switches in force, the inlining host, the counters it
// adds to, and (for a backend's own passes) the backend and the body being optimised.
using Ara3D.Geometry.Compiler;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// The context every <see cref="ITirPass"/> runs in. One context serves a whole generation; <see cref="ForBody"/>
/// gives the per-body copy that carries the owner type name and the backend's function record.
/// </summary>
public sealed class PassContext
{
    public PassContext(Compilation compilation, SwitchSet switches, ITirInlineHost inlineHost, PassStats stats = null, object backend = null)
    {
        Compilation = compilation;
        Switches = switches ?? new SwitchSet();
        InlineHost = inlineHost;
        Stats = stats ?? new PassStats();
        Backend = backend;
    }

    public Compilation Compilation { get; }

    /// <summary>The switches in force (the CLI's resolved table, or a writer's own set).</summary>
    public SwitchSet Switches { get; }

    /// <summary>The services the inliner needs from the backend (ground TIR lookup, concrete type names).</summary>
    public ITirInlineHost InlineHost { get; }

    /// <summary>The counts the passes add to (reported by the CLI).</summary>
    public PassStats Stats { get; }

    /// <summary>The backend writer, for the passes that live with one backend (C#: the CSharpWriter); null otherwise.</summary>
    public object Backend { get; }

    /// <summary>The name of the type that owns the body being optimised (null for a static function).</summary>
    public string OwnerTypeName { get; private init; }

    /// <summary>The backend's record of the function being optimised (C#: its CSharpFunctionInfo), or null.</summary>
    public object FunctionInfo { get; private init; }

    /// <summary>For a clone body (<see cref="ISumRefinementHost.RequestClone"/>): the facts about its parameters the
    /// refinement pass starts from; null for an ordinary body.</summary>
    public System.Collections.Generic.IReadOnlyList<SumFact> Seed { get; private init; }

    public bool On(string name) => Switches.On(name);

    /// <summary>This context for one body.</summary>
    public PassContext ForBody(string ownerTypeName, object functionInfo = null, System.Collections.Generic.IReadOnlyList<SumFact> seed = null)
        => new(Compilation, Switches, InlineHost, Stats, Backend) { OwnerTypeName = ownerTypeName, FunctionInfo = functionInfo, Seed = seed, _functionIds = _functionIds };

    private System.Lazy<System.Collections.Generic.Dictionary<Compiler.Symbols.FunctionDef, int>> _functionIds;

    /// <summary>A number for <paramref name="f"/> that is the same in every body of this generation (its position in the
    /// compilation's function list), or -1 for a function outside that list (a lambda's, a generated getter's).</summary>
    public int StableId(Compiler.Symbols.FunctionDef f)
    {
        _functionIds ??= new(() =>
        {
            var d = new System.Collections.Generic.Dictionary<Compiler.Symbols.FunctionDef, int>(System.Collections.Generic.ReferenceEqualityComparer.Instance);
            var i = 0;
            foreach (var fd in Compilation?.FunctionDefinitions ?? System.Linq.Enumerable.Empty<Compiler.Symbols.FunctionDef>())
                d.TryAdd(fd, i++);
            return d;
        }, true);
        return f != null && _functionIds.Value.TryGetValue(f, out var id) ? id : -1;
    }
}

/// <summary>The counts the passes keep across a generation.</summary>
public sealed class PassStats
{
    // The C++ prepass runs passes on several threads, so these are added to with Interlocked.
    /// <summary>Sites the post-inlining rewrites changed (sinking, call sharing).</summary>
    public WriterRewriteStats Rewrites { get; } = new WriterRewriteStats();

    // --hoist-invariants: invariants moved out of loops, loops changed, forwarding calls expanded to expose them.
    public int HoistedInvariants;
    public int HoistedLoops;
    public int HoistExposingInlines;
}
