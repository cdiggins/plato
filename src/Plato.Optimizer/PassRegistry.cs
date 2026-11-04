// The passes by name, and the loop that runs a backend's list of names over one body.
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// Passes by name. <see cref="Shared"/> holds the backend-neutral passes of this project (<see cref="SharedPasses"/>);
/// a backend adds its own with <see cref="With"/>. A backend's pass order is a list of names
/// (<c>CSharpWriter.PassOrder</c>, <c>CppTirPipeline.PassOrder</c>); <see cref="Run"/> applies them in that order.
/// </summary>
public sealed class PassRegistry
{
    private readonly Dictionary<string, ITirPass> _passes;

    private PassRegistry(Dictionary<string, ITirPass> passes) => _passes = passes;

    /// <summary>The backend-neutral passes.</summary>
    public static PassRegistry Shared { get; } = new(SharedPasses.All.ToDictionary(p => p.Name, StringComparer.Ordinal));

    /// <summary>This registry plus <paramref name="passes"/> (a name already present is replaced).</summary>
    public PassRegistry With(params ITirPass[] passes)
    {
        var d = new Dictionary<string, ITirPass>(_passes, StringComparer.Ordinal);
        foreach (var p in passes)
            d[p.Name] = p;
        return new PassRegistry(d);
    }

    public ITirPass this[string name]
        => _passes.TryGetValue(name, out var p) ? p : throw new ArgumentException($"No optimizer pass named '{name}'");

    public bool Contains(string name) => _passes.ContainsKey(name);

    public IEnumerable<string> Names => _passes.Keys;

    /// <summary>The passes of <paramref name="order"/>, looked up once so a misspelt name fails before any body runs.</summary>
    public IReadOnlyList<ITirPass> Resolve(IEnumerable<string> order) => order.Select(n => this[n]).ToList();

    /// <summary>Applies <paramref name="passes"/> in order; <paramref name="phase"/> (may be null) sees each pass's label
    /// and result, for --dump-tir.</summary>
    public static TirFunction Run(TirFunction tir, IReadOnlyList<ITirPass> passes, PassContext ctx, Action<string, TirFunction> phase = null)
    {
        foreach (var p in passes)
        {
            tir = p.Apply(tir, ctx);
            phase?.Invoke(p.Label, tir);
        }
        return tir;
    }
}
