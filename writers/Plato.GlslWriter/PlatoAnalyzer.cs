// Preserves the backend analyzer API while sharing function-selection policy.
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Analysis;

namespace Ara3D.Geometry.GlslWriter;

public class PlatoAnalyzer
{
    public PlatoAnalyzer(Compiler.Compilation compilation)
    {
        Compilation = compilation;
    }

    public Compiler.Compilation Compilation { get; }

    public int ScoreFunction(FunctionInstance f)
        => FunctionSelection.ScoreFunction(f);

    public FunctionInstance ChooseBestFunction(IReadOnlyList<FunctionInstance> xs, out int count)
        => FunctionSelection.ChooseBestFunction(xs, out count);
}
