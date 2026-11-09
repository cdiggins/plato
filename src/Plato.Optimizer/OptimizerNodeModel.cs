// The optimizer's node kinds described to the fact table it cannot otherwise see (compiler-546).
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// The <see cref="TirNodeModel"/> for the optimizer's own node kinds, which <c>Plato.Compiler</c> cannot name: an
/// inline binding and an inline region bind the name of their local; a temporary reference mentions its name and is
/// one use of it, keyed by that name; a guarded loop repeats its children. Everything else is the checker's answer.
/// Stateless, so one <see cref="Instance"/> serves every table; <see cref="PassContext.ForBody"/> hands it to the
/// per-body <see cref="TirFacts"/>.
/// </summary>
public sealed class OptimizerNodeModel : TirNodeModel
{
    public static readonly OptimizerNodeModel Instance = new();

    private OptimizerNodeModel() { }

    public override IEnumerable<string> Names(TirNode n) => n switch
    {
        TirInlineBinding b => new[] { b.Def.Name },
        TirInlineRegion r => new[] { r.Result.Name },
        TirTempRef t => new[] { t.Name },
        _ => base.Names(n)
    };

    public override object UseKey(TirNode n) => n is TirTempRef t ? t.Name : base.UseKey(n);

    public override bool IsLoop(TirNode n) => n is TirGuardedLoop || base.IsLoop(n);
}
