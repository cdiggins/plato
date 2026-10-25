using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>A loop whose condition prerequisites execute on every test, including the last.</summary>
public sealed class TirGuardedLoop : TirNode
{
    public TirEvaluation Test { get; }
    public TirNode Body { get; }

    public TirGuardedLoop(TirEvaluation test, TirNode body)
        : base(null, body.Origin)
        => (Test, Body) = (test, body);

    public override IEnumerable<TirNode> Children
        => Test.Statements.Append(Test.Value).Append(Body);

    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
        => new TirGuardedLoop(new TirEvaluation(children.Take(children.Count - 2).ToArray(),
            children[children.Count - 2]), children[children.Count - 1]);

    public override string ToString()
        => $"while-test {{ {string.Join("; ", Test.Statements)}; {Test.Value} }} {Body}";
}
