// Implements the irreducible array operations used by generated Plato programs.
using System.Runtime.CompilerServices;
using Ara3D.Collections;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// The Array half of the host contract (<c>stdlib/foundation/intrinsics.library.plato</c>):
    /// <c>Count</c> and <c>At</c> observe, <c>MapRange</c> is the only constructor,
    /// <c>Reduce</c> the only sequential fold, <c>FlatMap</c> the only length-varying producer.
    /// Every other array function has a Plato reference body in
    /// <c>primitives.library.plato</c> and must NOT be duplicated here - a duplicate
    /// makes each generated call site ambiguous (CS0121).
    ///
    /// <c>Count</c> and <c>At</c> are not declared: <c>IReadOnlyList{T}.Count</c> and its
    /// indexer already are them. <c>MapRange</c> appears on <c>Integer</c> (as an instance
    /// method, see Integer.cs) and on <c>int</c> here, because <c>xs.Count</c> is int-typed and
    /// C# does not apply a user-defined implicit conversion to an extension method's receiver.
    /// </summary>
    public static class ArrayIntrinsics
    {
        /// <summary>The <c>count</c> values <c>f(0)</c> to <c>f(count - 1)</c>, each computed once,
        /// here, in index order; a count of zero or less gives the empty array. Materialised, not
        /// lazy: a lazy list called <c>f</c> on every read, so a value read from several places
        /// was computed again at each (TKT-350: MeshModel meshed every face once per read of its
        /// outcomes). The interpreter's MapRange is eager the same way, so both backends run
        /// <c>f</c> on the same indices and fault at the same element.</summary>
        public static IReadOnlyList<T> MapRange<T>(this int count, Func<Integer, T> f)
        {
            if (count <= 0)
                return Array.Empty<T>();
            var items = new T[count];
            for (var i = 0; i < count; i++)
                items[i] = f(i);
            return items;
        }

        [MethodImpl(AggressiveInlining)]
        public static TAcc Reduce<T, TAcc>(this IReadOnlyList<T> xs, TAcc acc, Func<TAcc, T, TAcc> f)
        {
            for (var i = 0; i < xs.Count; i++)
                acc = f(acc, xs[i]);
            return acc;
        }

        [MethodImpl(AggressiveInlining)]
        public static IReadOnlyList<TResult> FlatMap<T, TResult>(this IReadOnlyList<T> xs, Func<T, IReadOnlyList<TResult>> f)
        {
            var r = new List<TResult>();
            for (var i = 0; i < xs.Count; i++)
            {
                var ys = f(xs[i]);
                for (var j = 0; j < ys.Count; j++)
                    r.Add(ys[j]);
            }
            return r;
        }
    }
}
