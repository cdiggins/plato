// Rewrites array updates whose old value is provably unused afterwards into in-place writes (--in-place).
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Pass 5 of the C# optimizer (--in-place, TKT-1132): functional-but-in-place array updates within one
/// body. Plato arrays are values and <c>xs.SetAt(i, v)</c> means a new array; the emitted C# builds one
/// by copying all of <c>xs</c>. When <see cref="TirUpdateFlow"/> proves nothing can observe the old value
/// afterwards, the call becomes a <see cref="TirInPlaceSet"/>, which the body writer prints as
/// <c>Intrinsics.SetAtInPlace(xs, i, v)</c>: one element written into <c>xs</c>'s own storage, the same
/// array returned. The rule and its limits are in README.md beside this file. Runs after the loop lowerer,
/// on the tree the writer prints, because that is where the loops and statements are. The interpreter is
/// not involved: it copies, so a law that passes in both engines is the check on every proof.
/// </summary>
public static class TirInPlaceUpdates
{
    public static TirFunction Rewrite(TirFunction tir, CSharpWriter writer)
    {
        if (!writer.InPlaceUpdates || tir?.Body == null)
            return tir;
        var body = Apply(tir.Body, new TirArrayIdentity(writer.Compilation), out _);
        return ReferenceEquals(body, tir.Body)
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    /// <summary>The body with every provable <c>SetAt</c> made in place, or the body itself when none is;
    /// <paramref name="count"/> is the number of calls rewritten.</summary>
    public static TirNode Apply(TirNode body, TirArrayIdentity identity, out int count)
    {
        count = 0;
        var flow = TirUpdateFlow.Analyze(body, identity);
        var decisions = flow.Sites.Select(flow.CanWriteInPlace).ToArray();
        if (!decisions.Any(d => d))
            return body;
        var seen = 0;
        var rewritten = 0;
        var result = TirRewrite.Rewrite(body, node =>
        {
            if (node is TirCall call && identity.IsSetAt(call) && decisions[seen++])
            {
                rewritten++;
                return new TirInPlaceSet(call.Args[0], call.Args[1], call.Args[2], call.Type, call.Origin);
            }
            return node;
        });
        count = rewritten;
        return result;
    }
}
