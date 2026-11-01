// What the body does with each array binding: the facts the in-place pass and the storage resolver ask.
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// The facts about one binding of one function body: a local variable, a parameter, or the temporary of a
/// lowered array loop. Collected by the walk of <see cref="TirUpdateFlow"/>; read-only to everyone else.
/// </summary>
public sealed class BindingFacts
{
    private readonly List<TirUpdateFlow.Use> _uses = new();
    private readonly List<int[]> _assignedLoops = new();

    /// <summary>The binding is declared once as a new array that nothing else names (an array literal, a
    /// fresh producer, a <c>SetAt</c> result, or a temporary referenced once). False for a parameter.</summary>
    public bool Fresh { get; internal set; }

    /// <summary>Referenced under a <see cref="TirLambda"/> that prints as a C# lambda. A lambda literal given
    /// to a lowered loop is written into the loop body and does not count. A binding declared inside the
    /// lambda is also marked, which is conservative.</summary>
    public bool Captured { get; internal set; }

    /// <summary>Assigned a value that is not provably a new array: another name may hold the same array.</summary>
    public bool Aliased { get; internal set; }

    public int Declarations { get; internal set; }

    /// <summary>The loops around the declaration, outermost first (ids are unique per body).</summary>
    public int[] DeclaredLoops { get; internal set; } = Array.Empty<int>();

    /// <summary>Every reference to the binding, classified as borrow, update or escape.</summary>
    public IReadOnlyList<TirUpdateFlow.Use> Uses => _uses;

    /// <summary>The loops around each assignment to the binding after its declaration.</summary>
    public IReadOnlyList<int[]> AssignedLoops => _assignedLoops;

    /// <summary>A use or an assignment sits in a loop that does not also hold the declaration, so the
    /// binding keeps its value from one pass of that loop to the next.</summary>
    public bool LoopCarried
        => Uses.Any(u => u.Loops.Any(l => !DeclaredLoops.Contains(l)))
           || AssignedLoops.Any(loops => loops.Any(l => !DeclaredLoops.Contains(l)));

    internal List<TirUpdateFlow.Use> UseList => _uses;
    internal void AddAssignment(int[] loops) => _assignedLoops.Add(loops);
}

/// <summary>
/// The per-binding half of <see cref="TirUpdateFlow"/>, kept as an analysis of its own so a second pass
/// (the storage resolver of --owned-arrays) can ask whether an array binding is a fresh owned
/// temporary, aliased, captured by a real lambda, or carried round a loop.
/// </summary>
public sealed class TirOwnership
{
    private readonly Dictionary<VariableDef, BindingFacts> _variables = new();
    private readonly Dictionary<string, BindingFacts> _temps = new(StringComparer.Ordinal);
    private readonly Dictionary<ParameterDef, BindingFacts> _parameters = new();

    internal TirOwnership() { }

    public static TirOwnership Analyze(TirNode body, TirArrayIdentity identity)
        => TirUpdateFlow.Analyze(body, identity).Ownership;

    /// <summary>The facts of a local variable; null when the body never mentions it.</summary>
    public BindingFacts Of(VariableDef def)
        => def != null && _variables.TryGetValue(def, out var f) ? f : null;

    /// <summary>The facts of the temporary of a lowered loop (<see cref="TirLoweredLoop.TempName"/>).</summary>
    public BindingFacts OfTemp(string tempName)
        => tempName != null && _temps.TryGetValue(tempName, out var f) ? f : null;

    public BindingFacts OfParameter(ParameterDef p)
        => p != null && _parameters.TryGetValue(p, out var f) ? f : null;

    internal BindingFacts Get(VariableDef def) => GetOrAdd(_variables, def);
    internal BindingFacts GetTemp(string name) => GetOrAdd(_temps, name);
    internal BindingFacts GetParameter(ParameterDef p) => GetOrAdd(_parameters, p);

    private static BindingFacts GetOrAdd<TKey>(Dictionary<TKey, BindingFacts> map, TKey key)
    {
        if (!map.TryGetValue(key, out var f))
            map[key] = f = new BindingFacts();
        return f;
    }
}
