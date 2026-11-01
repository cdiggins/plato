// The storage decisions for one function body: what the resolver chose and the checker verifies.
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>A hoisted <c>stackalloc</c> buffer holding the array of the lowered-loop temporary <see cref="Temp"/>.
/// <see cref="Loop"/> is the <see cref="TirLoweredLoop.Id"/> of the outermost loop around the allocation, where
/// the slot is declared, or null for the body scope.</summary>
public sealed record StackSlot(string Name, TypeExpression Element, int Capacity, string Temp = null, int? Loop = null);

/// <summary>
/// Storage per binding for one body. Everything the plan does not name is <see cref="ArrayStorage.List"/>, so
/// an empty plan is today's output. Keyed per binding (variable, parameter, lowered-loop temporary) and, for
/// nodes the printer creates, per node.
/// </summary>
public sealed class StoragePlan
{
    private readonly Dictionary<VariableDef, ArrayStorage> _variables = new();
    private readonly Dictionary<string, ArrayStorage> _temps = new(System.StringComparer.Ordinal);
    private readonly Dictionary<ParameterDef, ArrayStorage> _parameters = new();
    private readonly Dictionary<TirNode, ArrayStorage> _nodes = new(ReferenceEqualityComparer.Instance);
    private readonly List<StackSlot> _slots = new();
    private readonly List<string> _refusals = new();

    public StoragePlan(FunctionDef owner = null) => Owner = owner;

    /// <summary>The function whose body this plan covers (the checker asks whether it is on a call cycle).</summary>
    public FunctionDef Owner { get; }

    public ArrayStorage Of(VariableDef def)
        => def != null && _variables.TryGetValue(def, out var s) ? s : ArrayStorage.List;

    public ArrayStorage OfTemp(string tempName)
        => tempName != null && _temps.TryGetValue(tempName, out var s) ? s : ArrayStorage.List;

    public ArrayStorage OfParameter(ParameterDef p)
        => p != null && _parameters.TryGetValue(p, out var s) ? s : ArrayStorage.List;

    /// <summary>The storage of the value a node produces: a reference reads its binding's entry, a
    /// <see cref="TirRepresent"/> is its target, any other node is what was assigned to it, else List.</summary>
    public ArrayStorage Of(TirNode node) => node switch
    {
        null => ArrayStorage.List,
        TirVariable v => Of(v.Def),
        TirParameter p => OfParameter(p.Def),
        TirTempRef t => OfTemp(t.Name),
        TirRepresent r => r.To,
        _ => _nodes.TryGetValue(node, out var s) ? s : ArrayStorage.List,
    };

    /// <summary>The slots, each declared once at the loop (or body scope) its <see cref="StackSlot.Loop"/> names.</summary>
    public IReadOnlyList<StackSlot> Slots => _slots;

    /// <summary>"Name: reason" for each binding that was eligible in kind but kept List.</summary>
    public IReadOnlyList<string> Refusals => _refusals;

    public void Assign(VariableDef def, ArrayStorage storage) => _variables[def] = storage;
    public void AssignTemp(string tempName, ArrayStorage storage) => _temps[tempName] = storage;
    public void AssignParameter(ParameterDef p, ArrayStorage storage) => _parameters[p] = storage;
    public void AssignNode(TirNode node, ArrayStorage storage) => _nodes[node] = storage;
    public void AddSlot(StackSlot slot) => _slots.Add(slot);
    public void Refuse(string name, string reason) => _refusals.Add($"{name}: {reason}");

    public override string ToString()
        => $"storage[{_variables.Count + _temps.Count + _parameters.Count + _nodes.Count} bindings, {_slots.Count} slots, {_refusals.Count} refusals]";
}
