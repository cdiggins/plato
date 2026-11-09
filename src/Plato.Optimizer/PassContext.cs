// What an optimizer pass may read: the compilation, the switches in force, the inlining host, the counters it
// adds to, the body's fact table, and (for a backend's own passes) the backend and the body being optimised.
using System;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// The context every <see cref="ITirPass"/> runs in. One context serves a whole generation; <see cref="ForBody"/>
/// gives the per-body copy that carries the owner type name, the backend's function record and a fresh
/// <see cref="Facts"/> table (the only context that has one).
/// </summary>
public sealed class PassContext
{
    private readonly TirFacts _facts;

    public PassContext(Compilation compilation, SwitchSet switches, ITirInlineHost inlineHost, PassStats stats = null, object backend = null)
        : this(compilation, switches, inlineHost, stats, backend, null)
    {
    }

    private PassContext(Compilation compilation, SwitchSet switches, ITirInlineHost inlineHost, PassStats stats, object backend, TirFacts facts)
    {
        Compilation = compilation;
        Switches = switches ?? new SwitchSet();
        InlineHost = inlineHost;
        Stats = stats ?? new PassStats();
        Backend = backend;
        _facts = facts;
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

    /// <summary>The subtree facts of the body being optimised, memoized by node identity (compiler-546). A pass asks
    /// it for a size, a mention or a structural key instead of walking the tree; the answers hold across passes because
    /// the nodes are immutable. Only a per-body context has a table, a fresh one from <see cref="ForBody"/>: a table
    /// belongs to one body and one thread (the Cpu prepass optimises one body per worker thread, each through its own
    /// per-body context), so the generation context, which many threads share, throws instead of lending one.</summary>
    /// <exception cref="InvalidOperationException">This is the generation context; call <see cref="ForBody"/> first.</exception>
    public TirFacts Facts => _facts ?? throw new InvalidOperationException(
        "The generation-wide PassContext has no fact table; run the pass on the context PassContext.ForBody returns.");

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
        => new(Compilation, Switches, InlineHost, Stats, Backend, new TirFacts(OptimizerNodeModel.Instance)) { OwnerTypeName = ownerTypeName, FunctionInfo = functionInfo, Seed = seed, _functionIds = _functionIds };

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
