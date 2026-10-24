// Caches expensive pure calls using value equality while excluding body elaboration costs.
using System.Diagnostics;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Interpreter;

/// <summary>
/// Remembers the results of expensive calls. Plato is pure, so a bodied function applied to equal
/// arguments returns an equal value, and an evaluation that asks twice may be answered once. A law
/// that meshes its model three times in one expression, a constant written as a loop of a thousand
/// steps and asked for at every tolerance check, and a depth recursion that revisits the same
/// reference are all answered from here after the first time.
///
/// A function is remembered only once its mean cost, with time spent elaborating bodies left out,
/// reaches <see cref="ThresholdMicroseconds"/>: hashing the arguments of a call that costs a
/// microsecond would cost more than it saves. Arguments are compared by <see cref="MemoValue"/>,
/// which is stricter than Plato's own equality (Numbers bit for bit, so 0.0 and -0.0 are
/// different keys; values compared together with their run-time types), so a remembered answer is
/// exactly the one the call would give. A call whose arguments or result hold a List or Buffer
/// builder, the one mutable value, is never remembered. A call that faults is not remembered.
/// </summary>
public sealed class CallMemo
{
    /// <summary>The mean cost, per call, at which a function starts to be remembered.</summary>
    public double ThresholdMicroseconds { get; set; } = 20;

    /// <summary>A function's table is emptied when it reaches this many entries, which bounds the
    /// memory a long sweep keeps alive.</summary>
    public int MaxEntriesPerFunction { get; set; } = 4096;

    public long Hits { get; private set; }
    public long Stores { get; private set; }

    /// <summary>How many functions are remembered.</summary>
    public int RememberedFunctions => _stats.Values.Count(s => s.Table != null);

    internal sealed class Stats
    {
        public long Calls;
        public long Ticks;
        public Dictionary<object[], object> Table;
    }

    private readonly Dictionary<FunctionDef, Stats> _stats = new Dictionary<FunctionDef, Stats>(ReferenceEqualityComparer.Instance);

    internal Stats StatsOf(FunctionDef f)
    {
        if (!_stats.TryGetValue(f, out var s))
            _stats[f] = s = new Stats();
        return s;
    }

    /// <summary>The remembered result of f on these arguments, when there is one.</summary>
    internal bool TryGet(Stats s, IReadOnlyList<object> args, out object[] key, out object result)
    {
        key = null;
        result = null;
        if (s.Table == null)
            return false;
        key = MemoValue.KeyOf(args);
        if (key == null)
            return false;
        if (s.Table.TryGetValue(key, out result))
        {
            Hits++;
            return true;
        }
        return false;
    }

    /// <summary>Records one computed call of cost <paramref name="ticks"/> (Stopwatch ticks, with
    /// elaboration left out), and remembers its result when the function has become expensive.</summary>
    internal void Record(Stats s, IReadOnlyList<object> args, object[] key, object result, long ticks)
    {
        s.Calls++;
        s.Ticks += ticks;
        if (s.Table == null)
        {
            var meanMicroseconds = (double)s.Ticks / s.Calls * 1e6 / Stopwatch.Frequency;
            if (meanMicroseconds < ThresholdMicroseconds)
                return;
            s.Table = new Dictionary<object[], object>(MemoValue.KeyComparer.Instance);
        }
        key ??= MemoValue.KeyOf(args);
        if (key == null || !MemoValue.TryHash(result, out _))
            return;
        if (s.Table.Count >= MaxEntriesPerFunction)
            s.Table.Clear();
        s.Table[key] = result;
        Stores++;
    }
}

/// <summary>
/// The equality and hashing <see cref="CallMemo"/> keys use. Stricter than
/// <see cref="ValueEquality"/>: a Number compares bit for bit, and a record, sum or array compares
/// its run-time type too (an empty Array&lt;Integer&gt; is not an empty Array&lt;Number&gt;), because
/// dispatch inside the call can depend on either. Hashes of records, sums and arrays are cached in
/// the value, which is immutable, so a large model is hashed once however often it is passed.
/// A value holding a builder has no hash, and the call is not remembered.
/// </summary>
public static class MemoValue
{
    /// <summary>A copy of the arguments to use as a key, or null when one of them holds a builder.</summary>
    public static object[] KeyOf(IReadOnlyList<object> args)
    {
        var key = new object[args.Count];
        for (var i = 0; i < key.Length; i++)
        {
            if (!TryHash(args[i], out _))
                return null;
            key[i] = args[i];
        }
        return key;
    }

    public static bool TryHash(object v, out int hash)
    {
        switch (v)
        {
            case null: hash = 0; return true;
            case double x: hash = BitConverter.DoubleToInt64Bits(x).GetHashCode(); return true;
            case long x: hash = x.GetHashCode(); return true;
            case bool x: hash = x ? 1 : 2; return true;
            case char x: hash = x.GetHashCode(); return true;
            case string x: hash = string.GetHashCode(x, StringComparison.Ordinal); return true;
            case TypeToken t: hash = t.Type.Name.GetHashCode(); return true;
            case PlatoFunction f: hash = f.GetHashCode(); return true;
            case PlatoBuilder: hash = 0; return false;
            case PlatoRecord r: return r.TryMemoHash(out hash);
            case PlatoSum u: return u.TryMemoHash(out hash);
            case PlatoArray a: return a.TryMemoHash(out hash);
            default: hash = v.GetHashCode(); return true;
        }
    }

    /// <summary>The hash of a sequence of values, seeded; false when one of them holds a builder.</summary>
    internal static bool TryCombine(int seed, IReadOnlyList<object> items, out int hash)
    {
        var h = new HashCode();
        h.Add(seed);
        for (var i = 0; i < items.Count; i++)
        {
            if (!TryHash(items[i], out var x))
            {
                hash = 0;
                return false;
            }
            h.Add(x);
        }
        hash = h.ToHashCode();
        return true;
    }

    public static bool Equals(object a, object b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;
        switch (a)
        {
            case double x: return b is double dy && BitConverter.DoubleToInt64Bits(x) == BitConverter.DoubleToInt64Bits(dy);
            case long x: return b is long ly && x == ly;
            case bool x: return b is bool by && x == by;
            case char x: return b is char cy && x == cy;
            case string x: return b is string sy && string.Equals(x, sy, StringComparison.Ordinal);
            case TypeToken t: return b is TypeToken o && TypeEquals(t.Type, o.Type);
            case PlatoRecord r:
                return b is PlatoRecord s && r.MemoHashEquals(s) && TypeEquals(r.Type, s.Type) && SequenceEquals(r.Fields, s.Fields);
            case PlatoSum u:
                return b is PlatoSum v && u.MemoHashEquals(v) && u.Case.Tag == v.Case.Tag && TypeEquals(u.Type, v.Type)
                       && SequenceEquals(u.Fields, v.Fields);
            case PlatoArray xs:
                return b is PlatoArray ys && xs.MemoHashEquals(ys) && TypeEquals(xs.ElementType, ys.ElementType)
                       && SequenceEquals(xs.Items, ys.Items);
            default:
                return a.Equals(b);
        }
    }

    private static bool TypeEquals(TypeExpression a, TypeExpression b)
        => ReferenceEquals(a, b) || (a != null && b != null && a.Equals(b));

    private static bool SequenceEquals(IReadOnlyList<object> a, IReadOnlyList<object> b)
    {
        if (a.Count != b.Count) return false;
        for (var i = 0; i < a.Count; i++)
            if (!Equals(a[i], b[i]))
                return false;
        return true;
    }

    /// <summary>Compares argument lists element by element with <see cref="Equals(object, object)"/>.</summary>
    public sealed class KeyComparer : IEqualityComparer<object[]>
    {
        public static readonly KeyComparer Instance = new KeyComparer();

        public bool Equals(object[] a, object[] b)
            => ReferenceEquals(a, b) || (a != null && b != null && SequenceEquals(a, b));

        public int GetHashCode(object[] key)
            => TryCombine(key.Length, key, out var h) ? h : 0;
    }
}
