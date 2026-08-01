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
    /// <c>primitives-arrays.library.plato</c> and must NOT be duplicated here - a duplicate
    /// makes each generated call site ambiguous (CS0121).
    ///
    /// <c>Count</c> and <c>At</c> are not declared: <c>IReadOnlyList{T}.Count</c> and its
    /// indexer already are them. <c>MapRange</c> appears on <c>Integer</c> (as an instance
    /// method, see Integer.cs) and on <c>int</c> here, because <c>xs.Count</c> is int-typed and
    /// C# does not apply a user-defined implicit conversion to an extension method's receiver.
    /// </summary>
    public static class ArrayIntrinsics
    {
        [MethodImpl(AggressiveInlining)]
        public static IReadOnlyList<T> MapRange<T>(this int count, Func<Integer, T> f)
            => new ReadOnlyList<T>(count, i => f(i));

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
