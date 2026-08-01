using System.Runtime.CompilerServices;
using Ara3D.Collections;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry;

// INDEX POSITIONS ARE `int`, NOT `Integer` (plato-323). The shipping recipe erases the scalar
// wrappers (--scalar=float), so every generated element-wise extension is declared on the
// primitive: `IntegerExtensions.ToNumber(this int)`. C# does NOT apply a user-defined implicit
// conversion to an extension method's RECEIVER, so exposing `Integer` in an index or element
// position here made every generated chain over it unbindable — `count.Range().Map(i => i.ToNumber())`
// failed with CS1929 "requires a receiver of type 'int'" 425 times in the forward stdlib, and csc
// blamed an unrelated candidate overload. The `Integer` RECEIVER overloads stay (a wrapper-typed
// receiver has no other way in); only what flows OUT to a caller's lambda is erased.
public static class ArrayExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static IEnumerable<T> Enumerate<T>(this IReadOnlyList<T> self) =>
        self;
    
    [MethodImpl(AggressiveInlining)]
    public static ReadOnlyList<T0> MapRange<T0>(this int count, Func<int, T0> f) =>
        new(count, i => f(i));

    [MethodImpl(AggressiveInlining)]
    public static ReadOnlyList<T0> MapRange<T0>(this Integer count, Func<int, T0> f) =>
        new(count, i => f(i));

    [MethodImpl(AggressiveInlining)]
    public static ReadOnlyList<int> Range(this int count) =>
        new(count, i => i);

    [MethodImpl(AggressiveInlining)]
    public static ReadOnlyList<int> Indices<T0>(this IReadOnlyList<T0> xs) =>
        xs.Count.Range();

    [MethodImpl(AggressiveInlining)]
    public static ReadOnlyList<T1> MapIndices<T0, T1>(this IReadOnlyList<T0> xs, Func<int, T1> f) =>
        xs.Count.MapRange(f);

    [MethodImpl(AggressiveInlining)]
    public static ReadOnlyList<T1> Map<T0, T1>(this IReadOnlyList<T0> xs, Func<T0, T1> f) =>
        xs.Select(f);

    [MethodImpl(AggressiveInlining)]
    public static ReadOnlyList<T1> Map<T0, T1>(this IReadOnlyList<T0> xs, Func<T0, int, T1> f) =>
        xs.MapIndices(i => f(xs[i], i));

    [MethodImpl(AggressiveInlining)]
    public static ReadOnlyList2D<T1> Map<T0, T1>(this IReadOnlyList2D<T0> xs, Func<T0, T1> f) =>
        xs.Select(f);

    [MethodImpl(AggressiveInlining)]
    public static ReadOnlyList3D<T1> Map<T0, T1>(this IReadOnlyList3D<T0> xs, Func<T0, T1> f) =>
        xs.Select(f);

    //==
    // Optimizer stage 2 (loop-into-buffer lowering, --optimize-arrays): EAGER variants of
    // Map/MapRange. Same semantics as the lazy versions for pure callbacks; the result is a
    // materialized array instead of a functional view, so a MULTI-consumed result (stored in a
    // struct, indexed repeatedly) evaluates each element exactly once. The compiler rewrites
    // call sites in materialization positions to these; never call the lazy/eager pair with an
    // impure callback and expect the same behavior. The overload set mirrors Map's exactly so
    // the rewrite is overload-transparent.
    //==

    public static IReadOnlyList<T1> MapEager<T0, T1>(this IReadOnlyList<T0> xs, Func<T0, T1> f)
    {
        var n = xs.Count;
        var r = new T1[n];
        for (var i = 0; i < n; i++)
            r[i] = f(xs[i]);
        return r;
    }

    public static IReadOnlyList<T1> MapEager<T0, T1>(this IReadOnlyList<T0> xs, Func<T0, int, T1> f)
    {
        var n = xs.Count;
        var r = new T1[n];
        for (var i = 0; i < n; i++)
            r[i] = f(xs[i], i);
        return r;
    }

    public static ReadOnlyList2D<T1> MapEager<T0, T1>(this IReadOnlyList2D<T0> xs, Func<T0, T1> f)
    {
        var n = xs.Count;
        var r = new T1[n];
        for (var i = 0; i < n; i++)
            r[i] = f(xs[i]);
        return new ReadOnlyList2D<T1>(r, xs.NumColumns, xs.NumRows);
    }

    public static ReadOnlyList3D<T1> MapEager<T0, T1>(this IReadOnlyList3D<T0> xs, Func<T0, T1> f)
    {
        var n = xs.Count;
        var r = new T1[n];
        for (var i = 0; i < n; i++)
            r[i] = f(xs[i]);
        return new ReadOnlyList3D<T1>(r, xs.NumColumns, xs.NumRows, xs.NumLayers);
    }

    public static IReadOnlyList<T0> MapRangeEager<T0>(this int count, Func<int, T0> f)
    {
        var r = new T0[count];
        for (var i = 0; i < count; i++)
            r[i] = f(i);
        return r;
    }

    [MethodImpl(AggressiveInlining)]
    public static IReadOnlyList<T0> MapRangeEager<T0>(this Integer count, Func<int, T0> f) =>
        ((int)count).MapRangeEager(f);

    [MethodImpl(AggressiveInlining)]
    public static T1 Reduce<T0, T1>(this IReadOnlyList<T0> xs, T1 acc, Func<T1, T0, T1> f)
        => xs.Aggregate(acc, f);

    [MethodImpl(AggressiveInlining)]
    public static Boolean All<T0>(this IReadOnlyList<T0> xs, Func<T0, Boolean> f)
        => xs.Enumerate().All(x => f(x));

    [MethodImpl(AggressiveInlining)]
    public static Boolean Any<T0>(this IReadOnlyList<T0> xs, Func<T0, Boolean> f)
        => xs.Enumerate().Any(x => f(x));

    [MethodImpl(AggressiveInlining)]
    public static List<T1> FlatMap<T0, T1>(this IReadOnlyList<T0> xs, Func<T0, IReadOnlyList<T1>> f)
    {
        var r = new List<T1>();
        foreach (var x in xs)
            r.AddRange(f(x));
        return r;
    }

    [MethodImpl(AggressiveInlining)]
    public static T1[] FlatMap<T0, T1>(this IReadOnlyList<T0> xs, Func<T0, (T1, T1)> f)
    {
        var r = new T1[xs.Count * 2];
        for (var i=0; i < xs.Count; i++)
        {
            var (a, b) = f(xs[i]);
            r[i * 2] = a;
            r[i * 2 + 1] = b;
        }

        return r;
    }

    [MethodImpl(AggressiveInlining)]
    public static T1[] FlatMap<T0, T1>(this IReadOnlyList<T0> xs, Func<T0, (T1, T1, T1)> f)
    {
        var r = new T1[xs.Count * 3];
        for (var i = 0; i < xs.Count; i++)
        {
            var (a, b, c) = f(xs[i]);
            r[i * 3] = a;
            r[i * 3 + 1] = b;
            r[i * 3 + 2] = c;
        }

        return r;
    }

    [MethodImpl(AggressiveInlining)]
    public static T1[] FlatMap<T0, T1>(this IReadOnlyList<T0> xs, Func<T0, (T1, T1, T1, T1)> f)
    {
        var r = new T1[xs.Count * 4];
        for (var i = 0; i < xs.Count; i++)
        {
            var (a, b, c, d) = f(xs[i]);
            r[i * 4] = a;
            r[i * 4 + 1] = b;
            r[i * 4 + 2] = c;
            r[i * 4 + 3] = d;
        }

        return r;
    }

    [MethodImpl(AggressiveInlining)]
    public static IReadOnlyList<T1> WithNext<T0, T1>(this IReadOnlyList<T0> xs, Func<T0, T0, T1> f, bool includeFirst)
        => includeFirst
            ? xs.MapIndices(i => i<xs.Count - 1 ? f(xs[i], xs[i + 1]) : f(xs[i], xs[0]))
            : (xs.Count - 1).MapRange(i => f(xs[i], xs[i + 1]));

    /// <summary>
    /// Maps pairs of elements to a new array.
    /// </summary>
    public static ReadOnlyList<U> MapPairs<T, U>(this IReadOnlyList<T> xs, Func<T, T, U> f)
        => xs.SelectPairs(f);

    /// <summary>
    /// Maps every 3 elements to a new array.
    /// </summary>
    public static ReadOnlyList<U> MapTriplets<T, U>(this IReadOnlyList<T> xs, Func<T, T, T, U> f)
        => xs.SelectTriplets(f);

    /// <summary>
    /// Maps every 4 elements to a new array.
    /// </summary>
    public static ReadOnlyList<U> MapQuartets<T, U>(this IReadOnlyList<T> xs, Func<T, T, T, T, U> f)
        => xs.SelectQuartets(f);

    /// <summary>
    /// The array intrinsic <c>Concatenate(xs: Array&lt;$T&gt;, ys: Array&lt;$T&gt;)</c>. Ara3D.Collections
    /// spells it <c>Concat</c>, so without this alias every call bound to the only Concatenate in
    /// scope — QuaternionExtensions.Concatenate — and reported a Quaternion receiver mismatch.
    /// </summary>
    [MethodImpl(AggressiveInlining)]
    public static ReadOnlyList<T> Concatenate<T>(this IReadOnlyList<T> xs, IReadOnlyList<T> ys)
        => xs.Concat(ys);
}
