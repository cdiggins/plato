// Supplies shared runtime helpers used by generated constructors and generic operations.
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

        /// <summary>
        /// <c>SetAt</c> on an array the writer's --in-place pass proved unshared and not read again as
        /// the old value (TKT-1132): the element is written into the array itself and the same array is
        /// returned. An index outside the array writes nothing, as the Plato body of <c>SetAt</c>
        /// returns an equal array. Only a <c>T[]</c> is written; any other list (a list the lowered
        /// FlatMap built, a view) is copied, so the call is correct on every value it can meet and the
        /// proof only decides whether the copy is saved.
        /// </summary>
        public static IReadOnlyList<T> SetAtInPlace<T>(IReadOnlyList<T> xs, Integer index, T value)
        {
            long at = index;
            // An array of a derived reference type typed as T[] would throw on the store.
            if (xs is T[] items && (typeof(T).IsValueType || items.GetType() == typeof(T[])))
            {
                if ((ulong)at < (ulong)items.Length)
                    items[at] = value;
                return items;
            }
            return SetAtCopy(xs, at, value);
        }

        private static IReadOnlyList<T> SetAtCopy<T>(IReadOnlyList<T> xs, long at, T value)
        {
            var count = xs.Count;
            if (count <= 0)
                return System.Array.Empty<T>();
            var copy = new T[count];
            for (var i = 0; i < count; i++)
                copy[i] = xs[i];
            if ((ulong)at < (ulong)count)
                copy[at] = value;
            return copy;
        }

        // Plato arrays are values: two arrays are equal when their elements are, as the
        // interpreter compares them (ValueEquality). A generated Equals compares an array field
        // through this, not IReadOnlyList's Equals, which is reference equality (TKT-355: a
        // B-spline model built twice differed from itself compiled). Nested arrays compare
        // element-wise too; other elements use their own Equals.
        public static bool ArrayEquals<T>(IReadOnlyList<T> a, IReadOnlyList<T> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Count != b.Count) return false;
            for (var i = 0; i < a.Count; i++)
                if (!ElementEquals(a[i], b[i]))
                    return false;
            return true;
        }

        // The hash that agrees with ArrayEquals: the count and every element's hash.
        public static int ArrayHash<T>(IReadOnlyList<T> a)
        {
            if (a == null) return 0;
            var h = new HashCode();
            h.Add(a.Count);
            for (var i = 0; i < a.Count; i++)
                h.Add(ElementHash(a[i]));
            return h.ToHashCode();
        }

        private static bool ElementEquals<T>(T x, T y)
            => x is System.Collections.IEnumerable xs && !(x is string) && y is System.Collections.IEnumerable ys
                ? SequenceEquals(xs, ys)
                : EqualityComparer<T>.Default.Equals(x, y);

        private static int ElementHash<T>(T x)
        {
            if (!(x is System.Collections.IEnumerable xs) || x is string)
                return EqualityComparer<T>.Default.GetHashCode(x);
            var h = new HashCode();
            foreach (var e in xs)
                h.Add(ElementHash(e));
            return h.ToHashCode();
        }

        private static bool SequenceEquals(System.Collections.IEnumerable xs, System.Collections.IEnumerable ys)
        {
            var a = xs.GetEnumerator();
            var b = ys.GetEnumerator();
            while (true)
            {
                var more = a.MoveNext();
                if (more != b.MoveNext()) return false;
                if (!more) return true;
                if (!ElementEquals(a.Current, b.Current)) return false;
            }
        }

        // MakeArray2D left this file 2026-08-01: Array2D is an ordinary Plato type with an
        // honest layout since plato-378, so `primitives.library.plato` carries the
        // reference body and a duplicate here made every call site ambiguous (CS0121).

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
