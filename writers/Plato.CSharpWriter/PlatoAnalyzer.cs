using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter;

public class PlatoAnalyzer
{
    public PlatoAnalyzer(Compiler.Compilation compilation)
    {
        Compilation = compilation;
    }

    public Compiler.Compilation Compilation { get; }

    public int ScoreFunction(FunctionInstance f)
    {
        var a = f.OwnerType;
        var b = f.Interface;

        // Intrinsics are always preferred 
        if (f.Implementation.OwnerType.Name == "Intrinsics")
            return -200;

        // We assume that if there is no concept, then the function implementation originated as a concrete type.
        // Concrete types provide a better score than any concept. 
        if (b == null)
            return -100;

        var depth = a.DepthTo(b);
        if (depth < 0)
            throw new Exception($"Expected {b} to be a base type of {a}");
        return depth;
    }

    public FunctionInstance ChooseBestFunction(IReadOnlyList<FunctionInstance> xs, out int count)
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