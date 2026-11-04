// One optimizer pass over a function's typed IR, and the name a backend's pass list uses for it.
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// A TIR-to-TIR rewrite. A pass reads the switches it needs from <see cref="PassContext"/> and returns
/// <c>tir</c> unchanged when they are off, so a backend's pass list can name every pass it supports and the
/// switch table decides which of them act. A pass that needs one backend's type knowledge lives with that
/// backend (it reads <see cref="PassContext.Backend"/>); every other pass lives in this project.
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
