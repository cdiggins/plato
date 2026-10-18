using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Interpreter;

/// <summary>
/// The host half of the intrinsic contract (<c>stdlib/foundation/intrinsics.library.plato</c>):
/// one entry per bodiless library signature, keyed by the name and the declared parameter type
/// names, <c>Add(Number, Number)</c>, <c>Count(Array)</c>. A bodiless function with no entry is a
/// <see cref="HostObligationException"/>, never a silent default. The table mirrors the C#
/// runtime (<c>src/Plato.Intrinsics</c>) operation for operation; the profile decides the width.
/// </summary>
public sealed class HostIntrinsics
{
    public delegate object Impl(Interpreter interp, IReadOnlyList<object> args);

    private readonly Dictionary<string, Impl> _table = new Dictionary<string, Impl>(StringComparer.Ordinal);
    public Numbers Numbers { get; }

    public HostIntrinsics(Numbers numbers)
    {
        Numbers = numbers;
        Register();
    }

    /// <summary>The table key of a declared signature: the name and the parameter types' head
    /// names, no type arguments.</summary>
    public static string KeyOf(FunctionDef f)
        => $"{f.Name}({string.Join(", ", f.Parameters.Select(p => p.Type?.Def?.Name ?? "?"))})";

    public bool TryGet(FunctionDef f, out Impl impl)
        => _table.TryGetValue(KeyOf(f), out impl);

    /// <summary>The entry for a key built from run-time types rather than a declaration: the
    /// members the C# runtime gives a primitive struct with no Plato declaration behind them
    /// (<c>String.Count</c>, <c>String.At</c>), reached through an interface the primitive
    /// declares (<c>ICountable</c>).</summary>
    public bool TryGet(string key, out Impl impl)
        => _table.TryGetValue(key, out impl);

    public IEnumerable<string> Keys => _table.Keys;

    private void Add(string key, Impl impl) => _table[key] = impl;

    private static double D(object o) => (double)o;
    private static long L(object o) => (long)o;
    private static bool B(object o) => (bool)o;

    private void Register()
    {
        var n = Numbers;

        // Number
        Add("Add(Number, Number)", (_, a) => n.Add(D(a[0]), D(a[1])));
        Add("Subtract(Number, Number)", (_, a) => n.Subtract(D(a[0]), D(a[1])));
        Add("Multiply(Number, Number)", (_, a) => n.Multiply(D(a[0]), D(a[1])));
        Add("Divide(Number, Number)", (_, a) => n.Divide(D(a[0]), D(a[1])));
        Add("Modulo(Number, Number)", (_, a) => n.Modulo(D(a[0]), D(a[1])));
        Add("Negative(Number)", (_, a) => n.Negative(D(a[0])));
        Add("LessThanOrEquals(Number, Number)", (_, a) => D(a[0]) <= D(a[1]));
        Add("Equals(Number, Number)", (_, a) => D(a[0]).Equals(D(a[1])));
        Add("Hash(Number)", (_, a) => n.HashNumber(D(a[0])));
        Add("Floor(Number)", (_, a) => n.Floor(D(a[0])));
        Add("Round(Number, Integer)", (_, a) => n.Round(D(a[0]), L(a[1])));
        Add("Sqrt(Number)", (_, a) => n.Sqrt(D(a[0])));
        Add("Exp(Number)", (_, a) => n.Exp(D(a[0])));
        Add("NaturalLog(Number)", (_, a) => n.NaturalLog(D(a[0])));
        Add("Pow(Number, Number)", (_, a) => n.Pow(D(a[0]), D(a[1])));
        Add("FusedMultiplyAdd(Number, Number, Number)", (_, a) => n.FusedMultiplyAdd(D(a[0]), D(a[1]), D(a[2])));
        Add("Sin(Number)", (_, a) => n.Sin(D(a[0])));
        Add("Cos(Number)", (_, a) => n.Cos(D(a[0])));
        Add("Atan2Radians(Number, Number)", (_, a) => n.Atan2(D(a[0]), D(a[1])));
        Add("MinValue(Number)", (_, a) => n.MinValue);
        Add("MaxValue(Number)", (_, a) => n.MaxValue);
        Add("Epsilon(Number)", (_, a) => n.Epsilon);
        Add("Pi(Number)", (_, a) => n.Pi);
        Add("IsNaN(Number)", (_, a) => double.IsNaN(D(a[0])));
        Add("IsInfinite(Number)", (_, a) => double.IsInfinity(D(a[0])));
        Add("ToInteger(Number)", (_, a) => n.ToInteger(D(a[0])));

        // Integer
        Add("Add(Integer, Integer)", (_, a) => n.I(L(a[0]) + L(a[1])));
        Add("Subtract(Integer, Integer)", (_, a) => n.I(L(a[0]) - L(a[1])));
        Add("Multiply(Integer, Integer)", (_, a) => n.I(L(a[0]) * L(a[1])));
        Add("Divide(Integer, Integer)", (_, a) => n.I(L(a[0]) / L(a[1])));
        Add("Modulo(Integer, Integer)", (_, a) => n.I(L(a[0]) % L(a[1])));
        Add("Negative(Integer)", (_, a) => n.I(-L(a[0])));
        Add("LessThanOrEquals(Integer, Integer)", (_, a) => L(a[0]) <= L(a[1]));
        Add("Equals(Integer, Integer)", (_, a) => L(a[0]) == L(a[1]));
        Add("Hash(Integer)", (_, a) => n.HashInteger(L(a[0])));
        Add("BitwiseAnd(Integer, Integer)", (_, a) => n.I(L(a[0]) & L(a[1])));
        Add("BitwiseOr(Integer, Integer)", (_, a) => n.I(L(a[0]) | L(a[1])));
        Add("BitwiseXor(Integer, Integer)", (_, a) => n.I(L(a[0]) ^ L(a[1])));
        Add("BitwiseNot(Integer)", (_, a) => n.I(~L(a[0])));
        Add("ShiftLeft(Integer, Integer)", (_, a) => n.I(L(a[0]) << (int)L(a[1])));
        Add("ShiftRight(Integer, Integer)", (_, a) => n.I(L(a[0]) >> (int)L(a[1])));
        Add("MinValue(Integer)", (_, a) => n.IntegerMinValue);
        Add("MaxValue(Integer)", (_, a) => n.IntegerMaxValue);
        Add("ToNumber(Integer)", (_, a) => n.ToNumber(L(a[0])));

        // Boolean
        Add("And(Boolean, Boolean)", (_, a) => B(a[0]) && B(a[1]));
        Add("Or(Boolean, Boolean)", (_, a) => B(a[0]) || B(a[1]));
        Add("Not(Boolean)", (_, a) => !B(a[0]));

        // Character and String: the C# runtime orders strings with string.CompareTo (the current
        // culture) and characters by code point.
        Add("LessThanOrEquals(Character, Character)", (_, a) => (char)a[0] <= (char)a[1]);
        Add("LessThanOrEquals(String, String)", (_, a) => string.Compare((string)a[0], (string)a[1]) <= 0);
        Add("Equals(String, String)", (_, a) => string.Equals((string)a[0], (string)a[1], StringComparison.Ordinal));
        Add("Equals(Character, Character)", (_, a) => (char)a[0] == (char)a[1]);
        Add("Count(String)", (_, a) => (long)((string)a[0]).Length);
        Add("At(String, Integer)", (_, a) => ((string)a[0])[(int)L(a[1])]);

        // Array: the five-function kernel.
        Add("Count(Array)", (_, a) => (long)((PlatoArray)a[0]).Count);
        Add("At(Array, Integer)", (_, a) => ((PlatoArray)a[0]).Items[(int)L(a[1])]);
        Add("MapRange(Integer, Function1)", (interp, a) =>
        {
            var count = (int)L(a[0]);
            var f = (PlatoFunction)a[1];
            var items = new object[Math.Max(count, 0)];
            for (var i = 0; i < count; i++)
                items[i] = f.Invoke(new object[] { (long)i });
            return new PlatoArray(ElementTypeOf(f.Type, 1), items);
        });
        Add("Reduce(Array, $U, Function2)", (interp, a) =>
        {
            var xs = (PlatoArray)a[0];
            var acc = a[1];
            var f = (PlatoFunction)a[2];
            for (var i = 0; i < xs.Count; i++)
                acc = f.Invoke(new[] { acc, xs[i] });
            return acc;
        });
        Add("FlatMap(Array, Function1)", (interp, a) =>
        {
            var xs = (PlatoArray)a[0];
            var f = (PlatoFunction)a[1];
            var r = new List<object>();
            TypeExpression elem = null;
            for (var i = 0; i < xs.Count; i++)
            {
                var ys = (PlatoArray)f.Invoke(new[] { xs[i] });
                elem ??= ys.ElementType;
                r.AddRange(ys.Items);
            }
            return new PlatoArray(elem ?? ElementTypeOf(ElementTypeOf(f.Type, 1), 0), r);
        });

        // List<T>, the growable builder.
        Add("Count(List)", (_, a) => (long)((PlatoBuilder)a[0]).Count);
        Add("At(List, Integer)", (_, a) => ((PlatoBuilder)a[0]).At((int)L(a[1])));
        Add("Add(List, $T)", (_, a) => ((PlatoBuilder)a[0]).Add(a[1]));
        Add("AddRange(List, Array)", (_, a) => ((PlatoBuilder)a[0]).AddRange((PlatoArray)a[1]));
        Add("Set(List, Integer, $T)", (_, a) => ((PlatoBuilder)a[0]).Set((int)L(a[1]), a[2]));
        Add("Freeze(List)", (_, a) => ((PlatoBuilder)a[0]).Freeze());
        Add("EmptyList(Array)", (_, a) => new PlatoBuilder(((PlatoArray)a[0]).ElementType, false, 0));

        // Buffer<T>, the fixed-size builder.
        Add("Count(Buffer)", (_, a) => (long)((PlatoBuilder)a[0]).Count);
        Add("At(Buffer, Integer)", (_, a) => ((PlatoBuilder)a[0]).At((int)L(a[1])));
        Add("Set(Buffer, Integer, $T)", (_, a) => ((PlatoBuilder)a[0]).Set((int)L(a[1]), a[2]));
        Add("Freeze(Buffer)", (_, a) => ((PlatoBuilder)a[0]).Freeze());
    }

    /// <summary>The i-th type argument of a type instance, or null.</summary>
    public static TypeExpression ElementTypeOf(TypeExpression t, int i)
        => t != null && i < t.TypeArgs.Count ? t.TypeArgs[i] : null;
}
