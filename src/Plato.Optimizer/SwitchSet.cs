// The optimisation switches in force for one generation: a set of switch names that are on, and the values of
// the parameterised ones (--name=value). Every backend and every pass reads switches through this one type.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// Switches by name (without dashes, as the CLI table spells them: <c>"share-calls"</c>). A pass asks
/// <see cref="On"/>; a parameterised switch is on when it has a value and is read with <see cref="Value"/> or
/// <see cref="Int"/>. The set is mutable so a writer's legacy boolean properties can be views on it; a generation
/// hands each writer its own copy.
/// </summary>
public sealed class SwitchSet
{
    private readonly HashSet<string> _on;
    private readonly Dictionary<string, string> _values;

    public SwitchSet() : this(Array.Empty<string>()) { }

    public SwitchSet(IEnumerable<string> on, IReadOnlyDictionary<string, string> values = null)
    {
        _on = new HashSet<string>(on, StringComparer.Ordinal);
        _values = values == null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(values.ToDictionary(kv => kv.Key, kv => kv.Value), StringComparer.Ordinal);
    }

    /// <summary>True when the switch is on (a parameterised switch: when it has a value).</summary>
    public bool On(string name) => _on.Contains(name) || _values.ContainsKey(name);

    /// <summary>The value of a parameterised switch, or null when it was not given.</summary>
    public string Value(string name) => _values.TryGetValue(name, out var v) ? v : null;

    /// <summary>The integer value of a parameterised switch, or <paramref name="fallback"/> when it was not given.</summary>
    public int Int(string name, int fallback) => _values.TryGetValue(name, out var v) && int.TryParse(v, out var n) ? n : fallback;

    public void Set(string name, bool on)
    {
        if (on) _on.Add(name);
        else _on.Remove(name);
    }

    public void SetValue(string name, string value)
    {
        if (value == null) _values.Remove(name);
        else _values[name] = value;
    }

    /// <summary>Turns on every switch of <paramref name="other"/> and copies its values; switches already on stay on.</summary>
    public void UnionWith(SwitchSet other)
    {
        if (other == null) return;
        _on.UnionWith(other._on);
        foreach (var kv in other._values)
            _values[kv.Key] = kv.Value;
    }

    public SwitchSet Copy() => new SwitchSet(_on, _values);

    /// <summary>The names that are on, sorted (parameterised ones as name=value).</summary>
    public IReadOnlyList<string> Names
        => _on.OrderBy(n => n, StringComparer.Ordinal).Concat(_values.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => $"{kv.Key}={kv.Value}")).ToList();

    public override string ToString() => string.Join(" ", Names);
}
