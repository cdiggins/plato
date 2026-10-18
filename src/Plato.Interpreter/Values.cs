using System.Text;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Interpreter;

// The interpreter's value representation (docs/interpreter.md, "Values"). Scalars are the host's
// boxed primitives: Number is a double under every profile, rounded to the profile's width after
// each operation (see Numbers); Integer is a long; Boolean a bool; Character a char; String a
// string. Everything else is one of the classes below. Every value carries enough type to
// dispatch on at run time, which is what lets generic bodies run unspecialized.

/// <summary>A product value: the type (with its type arguments when known) and one slot per
/// declared field, in declaration order.</summary>
public sealed class PlatoRecord
{
    public TypeExpression Type { get; }
    public object[] Fields { get; }
    public TypeDef Def => Type.Def;

    public PlatoRecord(TypeExpression type, object[] fields)
        => (Type, Fields) = (type, fields);

    public int IndexOf(string fieldName)
    {
        var fs = Def.Fields;
        for (var i = 0; i < fs.Count; i++)
            if (fs[i].Name == fieldName)
                return i;
        return -1;
    }

    public override string ToString() => ValueFormat.Format(this);
}

/// <summary>A sum value: the type, the case it carries, and the case's payload in declaration
/// order (empty for a payload-free case).</summary>
public sealed class PlatoSum
{
    public TypeExpression Type { get; }
    public SumCaseDef Case { get; }
    public object[] Fields { get; }
    public TypeDef Def => Type.Def;

    public PlatoSum(TypeExpression type, SumCaseDef sumCase, object[] fields)
        => (Type, Case, Fields) = (type, sumCase, fields);

    public override string ToString() => ValueFormat.Format(this);
}

/// <summary>An immutable Array&lt;T&gt;. Eager: every element is materialized when the array is
/// built, so a MapRange over a million elements costs a million calls up front (the C# runtime's
/// array is lazy; the law runner's inputs are small enough that eagerness is the simpler choice).
/// The element type is the static type of the node that built it, or null when the interpreter
/// could not tell (an empty literal in an unresolved position).</summary>
public sealed class PlatoArray
{
    public TypeExpression ElementType { get; }
    public IReadOnlyList<object> Items { get; }
    public int Count => Items.Count;
    public object this[int i] => Items[i];

    public PlatoArray(TypeExpression elementType, IReadOnlyList<object> items)
        => (ElementType, Items) = (elementType, items);

    public override string ToString() => ValueFormat.Format(this);
}

/// <summary>The two affine builders, List&lt;T&gt; (growable) and Buffer&lt;T&gt; (fixed-size).
/// A mutator returns the same builder; Freeze hands the storage to an Array and invalidates the
/// builder, exactly as PlatoList and PlatoBuffer in the C# runtime do. A new Buffer's slots hold
/// <c>fill</c>, the element type's default, as the runtime's <c>new T[n]</c> does.</summary>
public sealed class PlatoBuilder
{
    public bool IsBuffer { get; }
    public TypeExpression ElementType { get; }
    private List<object> _items;
    private bool _frozen;

    public PlatoBuilder(TypeExpression elementType, bool isBuffer, int capacity, object fill = null)
    {
        ElementType = elementType;
        IsBuffer = isBuffer;
        _items = new List<object>(capacity);
        if (isBuffer)
            for (var i = 0; i < capacity; i++)
                _items.Add(fill);
    }

    private void CheckLive()
    {
        if (_frozen)
            throw new EvaluationException("use of a builder after Freeze");
    }

    public int Count { get { CheckLive(); return _items.Count; } }
    public object At(int i) { CheckLive(); return _items[i]; }
    public PlatoBuilder Add(object x) { CheckLive(); _items.Add(x); return this; }
    public PlatoBuilder AddRange(PlatoArray xs) { CheckLive(); _items.AddRange(xs.Items); return this; }
    public PlatoBuilder Set(int i, object x) { CheckLive(); _items[i] = x; return this; }

    public PlatoArray Freeze()
    {
        CheckLive();
        _frozen = true;
        var items = _items;
        _items = null;
        return new PlatoArray(ElementType, items);
    }

    public override string ToString()
        => (IsBuffer ? "Buffer" : "List") + (_frozen ? "(frozen)" : $"[{_items.Count}]");
}

/// <summary>A function value: a lambda closed over the environment it was created in, or a host
/// delegate. Type is the FunctionN&lt;...&gt; instance when known.</summary>
public sealed class PlatoFunction
{
    public TypeExpression Type { get; }
    public int Arity { get; }
    public Func<IReadOnlyList<object>, object> Body { get; }
    public string Name { get; }

    public PlatoFunction(TypeExpression type, int arity, Func<IReadOnlyList<object>, object> body, string name = null)
        => (Type, Arity, Body, Name) = (type, arity, body, name ?? "<lambda>");

    public object Invoke(IReadOnlyList<object> args) => Body(args);
    public override string ToString() => $"{Name}/{Arity}";
}

/// <summary>A type used as a value: the receiver of a <c>_</c>-parameter function such as
/// <c>Number.Pi</c> or <c>BrepModel.Witness_UnitBox</c>.</summary>
public sealed class TypeToken
{
    public TypeExpression Type { get; }
    public TypeToken(TypeExpression type) => Type = type;
    public override string ToString() => $"{Type}";
}

/// <summary>Structural equality and hashing over interpreter values, mirroring what the C# backend
/// generates for a struct: field-wise <c>Equals</c>, so a Number compares with
/// <c>double.Equals</c> (NaN equals NaN, 0.0 equals -0.0). Arrays compare element-wise here, where
/// the generated C# compares them by reference: the one deliberate divergence
/// (docs/interpreter.md, "Where the interpreter and the C# backend differ").</summary>
public static class ValueEquality
{
    public static bool Equals(object a, object b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;
        switch (a)
        {
            case double x: return b is double dy && x.Equals(dy);
            case long x: return b is long ly && x == ly;
            case bool x: return b is bool by && x == by;
            case char x: return b is char cy && x == cy;
            case string x: return b is string sy && string.Equals(x, sy, StringComparison.Ordinal);
            case PlatoRecord r:
                return b is PlatoRecord s && r.Def.Name == s.Def.Name && SequenceEquals(r.Fields, s.Fields);
            case PlatoSum u:
                return b is PlatoSum v && u.Def.Name == v.Def.Name && u.Case.Tag == v.Case.Tag && SequenceEquals(u.Fields, v.Fields);
            case PlatoArray xs:
                return b is PlatoArray ys && SequenceEquals(xs.Items, ys.Items);
            case TypeToken t:
                return b is TypeToken o && t.Type.Name == o.Type.Name;
            default:
                return a.Equals(b);
        }
    }

    private static bool SequenceEquals(IReadOnlyList<object> a, IReadOnlyList<object> b)
    {
        if (a.Count != b.Count) return false;
        for (var i = 0; i < a.Count; i++)
            if (!Equals(a[i], b[i]))
                return false;
        return true;
    }

    /// <summary>A content hash. The C# backend combines field hashes with HashCode.Combine, whose
    /// seed is per process, so no hash value is comparable across runs in either engine; this one
    /// is stable within a process and consistent with Equals.</summary>
    public static int Hash(object v)
    {
        switch (v)
        {
            case null: return 0;
            case double x: return x.GetHashCode();
            case long x: return x.GetHashCode();
            case bool x: return x.GetHashCode();
            case char x: return x.GetHashCode();
            case string x: return string.GetHashCode(x, StringComparison.Ordinal);
            case PlatoRecord r: return Combine(r.Def.Name.GetHashCode(), r.Fields);
            case PlatoSum u: return Combine(u.Case.Tag, u.Fields);
            case PlatoArray xs: return Combine(xs.Count, xs.Items);
            default: return v.GetHashCode();
        }
    }

    private static int Combine(int seed, IReadOnlyList<object> items)
    {
        var h = new HashCode();
        h.Add(seed);
        foreach (var x in items)
            h.Add(Hash(x));
        return h.ToHashCode();
    }
}

/// <summary>Renders a value for a report: round-trip numbers, nested records as
/// <c>Type(f1, f2)</c>, arrays as <c>[a, b]</c>, truncated past a size budget so a failing
/// witness over a large model stays readable.</summary>
public static class ValueFormat
{
    public const int DefaultBudget = 400;

    public static string Format(object v, int budget = DefaultBudget)
    {
        var sb = new StringBuilder();
        Append(sb, v, budget);
        return sb.ToString();
    }

    private static void Append(StringBuilder sb, object v, int budget)
    {
        if (sb.Length > budget)
        {
            if (sb[sb.Length - 1] != '~') sb.Append('~');
            return;
        }
        switch (v)
        {
            case null: sb.Append("null"); break;
            case double x: sb.Append(x.ToString("R", System.Globalization.CultureInfo.InvariantCulture)); break;
            case long x: sb.Append(x); break;
            case bool x: sb.Append(x ? "true" : "false"); break;
            case char x: sb.Append('\'').Append(x).Append('\''); break;
            case string x: sb.Append('"').Append(x).Append('"'); break;
            case PlatoRecord r:
                sb.Append(r.Def.Name).Append('(');
                AppendList(sb, r.Fields, budget);
                sb.Append(')');
                break;
            case PlatoSum u:
                sb.Append(u.Def.Name).Append('.').Append(u.Case.Name);
                if (u.Fields.Length > 0) { sb.Append('('); AppendList(sb, u.Fields, budget); sb.Append(')'); }
                break;
            case PlatoArray xs:
                sb.Append('[');
                AppendList(sb, xs.Items, budget);
                sb.Append(']');
                break;
            case TypeToken t: sb.Append(t.Type.Name); break;
            default: sb.Append(v); break;
        }
    }

    private static void AppendList(StringBuilder sb, IReadOnlyList<object> items, int budget)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            if (sb.Length > budget) { sb.Append('~'); return; }
            Append(sb, items[i], budget);
        }
    }
}
