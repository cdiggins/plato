using Ara3D.Geometry;
using System.Runtime.CompilerServices;

namespace OptimizerExploration;

public readonly record struct Moments(Number Sum, Number Squares);

// Handwritten candidate emission, not output of a new compiler pass.
public static class Kernels
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Moments Separate(IReadOnlyList<Number> xs)
    {
        Number sum = 0;
        var n = xs.Count;
        for (var i = 0; i < n; i++)
            sum = sum + xs[i];
        Number squares = 0;
        var m = xs.Count;
        for (var i = 0; i < m; i++)
        {
            var x = xs[i];
            squares = squares + x * x;
        }
        return new(sum, squares);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Moments Combined(IReadOnlyList<Number> xs)
    {
        Number sum = 0;
        Number squares = 0;
        var n = xs.Count;
        for (var i = 0; i < n; i++)
        {
            var x = xs[i];
            sum = sum + x;
            squares = squares + x * x;
        }
        return new(sum, squares);
    }

    public static Moments Runtime(IReadOnlyList<Number> xs)
        => new(xs.Reduce((Number)0, (a, x) => a + x),
            xs.Reduce((Number)0, (a, x) => a + x * x));
}
