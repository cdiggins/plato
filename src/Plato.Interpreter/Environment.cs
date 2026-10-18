using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Interpreter;

/// <summary>
/// The bindings in scope while a body evaluates: parameters and <c>var</c> locals keyed by their
/// definition symbol (references resolve by identity in the TIR), a parent for nested scopes, and
/// the concrete type <c>Self</c> denotes in this activation (the receiver's run-time type).
/// A lambda captures a snapshot (see <see cref="Snapshot"/>), the value-at-creation semantics the
/// C# writer reproduces by hoisting captured locals into <c>_var</c> copies.
/// </summary>
public sealed class Environment
{
    private readonly Dictionary<DefSymbol, object> _bindings = new Dictionary<DefSymbol, object>();
    public Environment Parent { get; }
    public TypeExpression Self { get; }

    public Environment(Environment parent, TypeExpression self = null)
        => (Parent, Self) = (parent, self ?? parent?.Self);

    public void Define(DefSymbol def, object value) => _bindings[def] = value;

    public bool TryGet(DefSymbol def, out object value)
    {
        for (var e = this; e != null; e = e.Parent)
            if (e._bindings.TryGetValue(def, out value))
                return true;
        value = null;
        return false;
    }

    public object Get(DefSymbol def)
    {
        if (TryGet(def, out var v))
            return v;
        // The TIR of a normalized body can reference a parameter symbol the caller bound under
        // its original definition; the two share a name and a position, so fall back to the
        // name within the nearest scope that has one.
        for (var e = this; e != null; e = e.Parent)
            foreach (var kv in e._bindings)
                if (kv.Key.Name == def?.Name && kv.Key.GetType() == def.GetType())
                    return kv.Value;
        throw new EvaluationException($"unbound {def?.GetType().Name} '{def?.Name}'");
    }

    /// <summary>Rebinds an existing <c>var</c> local (an assignment), in whichever scope holds it.</summary>
    public void Set(DefSymbol def, object value)
    {
        for (var e = this; e != null; e = e.Parent)
            if (e._bindings.ContainsKey(def))
            {
                e._bindings[def] = value;
                return;
            }
        _bindings[def] = value;
    }

    /// <summary>A flat copy of every binding visible here, as a lambda captures it.</summary>
    public Environment Snapshot()
    {
        var copy = new Environment(null, Self);
        var chain = new List<Environment>();
        for (var e = this; e != null; e = e.Parent)
            chain.Add(e);
        for (var i = chain.Count - 1; i >= 0; i--)
            foreach (var kv in chain[i]._bindings)
                copy._bindings[kv.Key] = kv.Value;
        return copy;
    }
}
