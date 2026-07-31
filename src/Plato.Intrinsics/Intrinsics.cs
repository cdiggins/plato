using System.Runtime.CompilerServices;
using Ara3D.Collections;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    public static partial class Intrinsics
    {
        // Returns the INTERFACE, not T[]. An array literal is the seed of folds whose step
        // function returns some other Array implementation (`ring.Append(...)` is a
        // ReadOnlyList<T>, `CutByPlane` an IReadOnlyList<T>), and a T[]-typed accumulator makes
        // every one of those a conversion error. It is also what lets a nested literal unify with
        // a nested ReadOnlyList in generic inference (`caps.Concatenate(walls)`).
        [MethodImpl(AggressiveInlining)] public static IReadOnlyList<T> MakeArray<T>(params T[] args) => args;

        [MethodImpl(AggressiveInlining)] public static ReadOnlyList2D<T> MakeArray2D<T>(this Integer columns, Integer rows, Func<Integer, Integer, T> f) 
            => new ReadOnlyList2D<T>((columns * rows).MapRange(i => f(i % columns, i / columns)), columns, rows);

        [MethodImpl(AggressiveInlining)] public static Integer CombineHashCodes() => 17;
        [MethodImpl(AggressiveInlining)] public static Integer CombineHashCodes<T0>(T0 x0) => HashCode.Combine(x0);
        [MethodImpl(AggressiveInlining)] public static Integer CombineHashCodes<T0, T1>(T0 x0, T1 x1) => HashCode.Combine(x0, x1);
        [MethodImpl(AggressiveInlining)] public static Integer CombineHashCodes<T0, T1, T2>(T0 x0, T1 x1, T2 x2) => HashCode.Combine(x0, x1, x2);
        [MethodImpl(AggressiveInlining)] public static Integer CombineHashCodes<T0, T1, T2, T3>(T0 x0, T1 x1, T2 x2, T3 x3) => HashCode.Combine(x0, x1, x2, x3);
        [MethodImpl(AggressiveInlining)] public static Integer CombineHashCodes<T0, T1, T2, T3, T4>(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4) => HashCode.Combine(x0, x1, x2, x3, x4);
        [MethodImpl(AggressiveInlining)] public static Integer CombineHashCodes<T0, T1, T2, T3, T4, T5>(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5) => HashCode.Combine(x0, x1, x2, x3, x4, x5);
        [MethodImpl(AggressiveInlining)] public static Integer CombineHashCodes<T0, T1, T2, T3, T4, T5, T6>(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6) => HashCode.Combine(x0, x1, x2, x3, x4, x5, x6);
        [MethodImpl(AggressiveInlining)] public static Integer CombineHashCodes<T0, T1, T2, T3, T4, T5, T6, T7>(T0 x0, T1 x1, T2 x2, T3 x3, T4 x4, T5 x5, T6 x6, T7 x7) => HashCode.Combine(x0, x1, x2, x3, x4, x5, x6, x7);

        // Beyond 8 fields System.HashCode.Combine runs out of overloads, and the generated
        // GetHashCode of a wide struct passes one argument per field — the forward stdlib has
        // structs with 9, 10 and 28 (plato-323, 23 x CS1501 "no overload takes 9 arguments").
        // The accumulator form has no arity limit. It boxes, so it is deliberately the LAST
        // resort: every arity up to 8 keeps its exact generic overload, which C# prefers over
        // params, so no existing call site changes.
        [MethodImpl(AggressiveInlining)]
        public static Integer CombineHashCodes(params object[] xs)
        {
            var h = new HashCode();
            foreach (var x in xs)
                h.Add(x);
            return h.ToHashCode();
        }

        [MethodImpl(AggressiveInlining)] public static (T0, T1) Tuple2<T0, T1>(this T0 item0, T1 item1) => (item0, item1);
        [MethodImpl(AggressiveInlining)] public static (T0, T1, T2) Tuple3<T0, T1, T2>(this T0 item0, T1 item1, T2 item2) => (item0, item1, item2);
        [MethodImpl(AggressiveInlining)] public static (T0, T1, T2, T3) Tuple4<T0, T1, T2, T3>(this T0 item0, T1 item1, T2 item2, T3 item3) => (item0, item1, item2, item3);
    }
}
