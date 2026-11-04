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

    public bool On(string name) => Switches.On(name);

    /// <summary>This context for one body.</summary>
    public PassContext ForBody(string ownerTypeName, object functionInfo = null)
        => new(Compilation, Switches, InlineHost, Stats, Backend) { OwnerTypeName = ownerTypeName, FunctionInfo = functionInfo };
}

/// <summary>The counts the passes keep across a generation.</summary>
public sealed class PassStats
{
    /// <summary>Sites the post-inlining rewrites changed (sinking, call sharing).</summary>
    public WriterRewriteStats Rewrites { get; } = new WriterRewriteStats();

    // --hoist-invariants: invariants moved out of loops, loops changed, forwarding calls expanded to expose them.
    public int HoistedInvariants;
    public int HoistedLoops;
    public int HoistExposingInlines;
}
