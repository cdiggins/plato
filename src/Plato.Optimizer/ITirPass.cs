// One optimizer pass over a function's typed IR, and the name a backend's pass list uses for it.
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// A TIR-to-TIR rewrite. A pass reads the switches it needs from <see cref="PassContext"/> and returns
/// <c>tir</c> unchanged when they are off, so a backend's pass list can name every pass it supports and the
/// switch table decides which of them act. A pass that needs one backend's type knowledge lives with that
/// backend (it reads <see cref="PassContext.Backend"/>); every other pass lives in this project.
/// <para>
/// Concurrency contract (TKT-1209). The C# writer applies a pass list to one body at a time; the Cpu writer's prepass
/// applies the same list to many bodies at once, one body per worker thread, with one pass instance and one
/// <see cref="PassContext"/> shared by every thread. A pass therefore holds to these rules:
/// <list type="bullet">
/// <item>It is a function of (tir, ctx). <see cref="Apply"/> writes no instance field and no static field, so one
/// instance is reentrant per body: two threads may apply it to two bodies at once, and the inliner may apply it to a
/// callee inside another body's optimisation. The only static state a pass may touch is the per-thread temporary
/// counters <see cref="PassCounters"/> restarts for every body (the inliner's renames, fusion's and the loop lowerer's
/// locals, <c>SymbolRewriter.NextId</c>): a new counter is added there, never declared on its own, so names stay a
/// function of the body and not of which thread or body came before.</item>
/// <item>What it shares, it only reads: the compilation and the TIR it reaches through
/// <see cref="PassContext.InlineHost"/> are never mutated; a rewrite builds new nodes. A switch is read through
/// <see cref="PassContext.On"/>.</item>
/// <item>What it counts goes through <see cref="PassContext.Stats"/>, added to with <c>Interlocked</c>
/// (<see cref="PassStats"/>, its <see cref="WriterRewriteStats"/>). Plain fields and dictionaries are sequential-only:
/// <see cref="InlineReport"/> and <see cref="IStatementInlineHost.RemainingStatementGrowth"/> are reached only
/// through a host, and a host that runs passes on worker threads returns null for the report and does not implement
/// the statement host (the Cpu pipeline's host does neither).</item>
/// <item><see cref="PassContext.FunctionInfo"/> and <see cref="PassContext.Backend"/> belong to the body's own
/// emission; a pass reads them and never writes to the backend.</item>
/// <item>A fact about a subtree (its size, what it mentions, a call's structural key) is asked of
/// <see cref="PassContext.Facts"/>, not recounted. The table is not thread-safe: it belongs to the per-body context
/// <see cref="PassContext.ForBody"/> makes, so each worker thread's body has its own; a pass never keeps one past the
/// body or hands it to another thread. Only a per-body context has a table: the generation-wide context throws
/// <see cref="System.InvalidOperationException"/> from <see cref="PassContext.Facts"/>, so a caller applies a pass
/// to a body through <see cref="PassContext.ForBody"/>, never through the generation context.</item>
/// <item>A pass that cannot handle a body throws. The Cpu prepass reruns such a body sequentially and fails the
/// generation when the rerun succeeds, since a result that depends on the thread is a broken rule above.</item>
/// </list>
/// </para>
/// </summary>
public interface ITirPass
{
    /// <summary>The name a backend's pass list uses (see <see cref="PassRegistry"/>).</summary>
    string Name { get; }

    /// <summary>What the pass did, as the --dump-tir phase header names it.</summary>
    string Label => Name;

    TirFunction Apply(TirFunction tir, PassContext ctx);
}

/// <summary>An <see cref="ITirPass"/> from a name, a label and a function.</summary>
public sealed class TirPass : ITirPass
{
    private readonly System.Func<TirFunction, PassContext, TirFunction> _apply;

    public TirPass(string name, string label, System.Func<TirFunction, PassContext, TirFunction> apply)
        => (Name, Label, _apply) = (name, label, apply);

    public string Name { get; }
    public string Label { get; }
    public TirFunction Apply(TirFunction tir, PassContext ctx) => _apply(tir, ctx);
}
