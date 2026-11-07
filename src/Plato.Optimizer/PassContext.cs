// What an optimizer pass may read: the compilation, the switches in force, the inlining host, the counters it
// adds to, the body's fact table, and (for a backend's own passes) the backend and the body being optimised.
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// The context every <see cref="ITirPass"/> runs in. One context serves a whole generation; <see cref="ForBody"/>
/// gives the per-body copy that carries the owner type name, the backend's function record and a fresh
/// <see cref="Facts"/> table.
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
        Facts = new TirFacts();
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
    /// the nodes are immutable. <see cref="ForBody"/> makes a fresh table, so a table belongs to one body and one thread
    /// (the Cpu prepass optimises one body per worker thread, each through its own per-body context); the generation
    /// context's own table serves only a caller that runs passes without <see cref="ForBody"/>, sequentially.</summary>
    public TirFacts Facts { get; }

    /// <summary>The name of the type that owns the body being optimised (null for a static function).</summary>
    public string OwnerTypeName { get; private init; }

    /// <summary>The backend's record of the function being optimised (C#: its CSharpFunctionInfo), or null.</summary>
    public object FunctionInfo { get; private init; }

    public bool On(string name) => Switches.On(name);

    /// <summary>This context for one body.</summary>
    public PassContext ForBody(string ownerTypeName, object functionInfo = null)
        => new(Compilation, Switches, InlineHost, Stats, Backend) { OwnerTypeName = ownerTypeName, FunctionInfo = functionInfo };
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
