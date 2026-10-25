// Selects function implementations consistently across language backends.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Analysis;

public static class FunctionSelection
{
    public static int ScoreFunction(FunctionInstance f)
        => ScoreFunction(f.Implementation, f.OwnerType, f.Interface);

    internal static int ScoreFunction(FunctionDef implementation, TypeDef a, TypeDef b)
    {
        // Intrinsics are always preferred
        if (implementation.OwnerType.Name == "Intrinsics")
            return -200;

        // We assume that if there is no interface, then the function implementation originated as a concrete type.
        // Concrete types provide a better score than any interface.
        if (b == null)
            return -100;

        var depth = a.DepthTo(b);
        if (depth < 0)
            throw new Exception($"Expected {b} to be a base type of {a}");
        return depth;
    }

    public static FunctionInstance ChooseBestFunction(IReadOnlyList<FunctionInstance> xs, out int count)
    {
        // We only want distinct implementations.
        count = 1;
        xs = xs.Distinct(x => x.Implementation.Id).ToList();
        if (xs.Count == 1)
            return xs[0];
        if (xs.Count == 0)
            throw new Exception("No results: could not find a best function.");

        var groups = xs.GroupBy(ScoreFunction).OrderBy(g => g.Key).ToList();
        var group0 = groups[0].ToList();
        Debug.Assert(group0.Count > 0);

        count = group0.Count;
        return group0[0];
    }
}
