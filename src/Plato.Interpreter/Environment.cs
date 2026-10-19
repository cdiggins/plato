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
    // A scope holds a handful of bindings, so two parallel arrays searched by reference from the
    // newest binding back cost less than a dictionary per activation and per captured lambda.
    private static readonly DefSymbol[] NoKeys = Array.Empty<DefSymbol>();
    private static readonly object[] NoValues = Array.Empty<object>();
    private DefSymbol[] _keys = NoKeys;
    private object[] _values = NoValues;
    private int _count;
    public Environment Parent { get; }
    public TypeExpression Self { get; }

    public Environment(Environment parent, TypeExpression self = null)
        => (Parent, Self) = (parent, self ?? parent?.Self);

    /// <summary>An activation's or a lambda call's scope: <paramref name="keys"/> is shared by
    /// every activation of one function or lambda and never written (the first new binding copies it), and
    /// <paramref name="values"/> is this activation's own.</summary>
    public Environment(Environment parent, TypeExpression self, DefSymbol[] keys, object[] values)
        => (Parent, Self, _keys, _values, _count) = (parent, self ?? parent?.Self, keys, values, keys.Length);

    private int IndexOf(DefSymbol def)
    {
        for (var i = _count - 1; i >= 0; i--)
            if (ReferenceEquals(_keys[i], def))
                return i;
        return -1;
    }

    private void Append(DefSymbol def, object value)
    {
        if (_count == _keys.Length)
        {
            // Array.Resize allocates and copies, so a shared key array is never written.
            var n = Math.Max(4, _count * 2);
            Array.Resize(ref _keys, n);
            Array.Resize(ref _values, n);
        }
        _keys[_count] = def;
        _values[_count++] = value;
    }

    public void Define(DefSymbol def, object value)
    {
        var i = IndexOf(def);
        if (i >= 0)
            _values[i] = value;
        else
            Append(def, value);
    }

    public bool TryGet(DefSymbol def, out object value)
    {
        for (var e = this; e != null; e = e.Parent)
        {
            var i = e.IndexOf(def);
            if (i >= 0)
            {
                value = e._values[i];
                return true;
            }
        }
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
            for (var i = 0; i < e._count; i++)
                if (e._keys[i].Name == def?.Name && e._keys[i].GetType() == def.GetType())
                    return e._values[i];
        throw new EvaluationException($"unbound {def?.GetType().Name} '{def?.Name}'");
    }

    /// <summary>Rebinds an existing <c>var</c> local (an assignment), in whichever scope holds it.</summary>
    public void Set(DefSymbol def, object value)
    {
        for (var e = this; e != null; e = e.Parent)
        {
            var i = e.IndexOf(def);
            if (i >= 0)
            {
                e._values[i] = value;
                return;
            }
        }
        Append(def, value);
    }

    /// <summary>A flat copy of every binding visible here, as a lambda captures it: the outermost
    /// scope's bindings first, so a search from the newest finds the innermost of a shadowed name.</summary>
    public Environment Snapshot()
    {
        var n = 0;
        for (var e = this; e != null; e = e.Parent)
            n += e._count;
        var copy = new Environment(null, Self) { _keys = new DefSymbol[n], _values = new object[n], _count = n };
        var at = n;
        for (var e = this; e != null; e = e.Parent)
        {
            at -= e._count;
            Array.Copy(e._keys, 0, copy._keys, at, e._count);
            Array.Copy(e._values, 0, copy._values, at, e._count);
        }
        return copy;
    }
}
