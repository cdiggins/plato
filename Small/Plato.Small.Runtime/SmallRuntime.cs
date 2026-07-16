// Minimal hand-written runtime for the self-contained Plato.Small.* generated projects.
// It provides ONLY the surface the small corpus emits: Intrinsics.MakeArray /
// CombineHashCodes and eager Map/Zip/Reduce/All/Any/Reverse over BCL IReadOnlyList<T>.
// The full library links Plato.Intrinsics.V2 instead; this stays deliberately tiny so the
// small optimizer-spike projects build with no external Plato/Ara3D dependencies.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

// The generated files carry a hardcoded `using Ara3D.Collections;`; a public type keeps the
// namespace visible across the assembly boundary so they compile without the real package.
// ReadOnlyList2D is only named by the emitter's always-present Integer.MakeArray2D bridge
// (never actually called here), so a stub suffices.
namespace Ara3D.Collections
{
    public sealed class ReadOnlyList2D<T> { }
}

namespace Ara3D.Geometry
{
    public static partial class Intrinsics
    {
        [MethodImpl(AggressiveInlining)] public static T[] MakeArray<T>(params T[] args) => args;

        [MethodImpl(AggressiveInlining)] public static int CombineHashCodes() => 17;
        [MethodImpl(AggressiveInlining)] public static int CombineHashCodes<T0>(T0 x0) => HashCode.Combine(x0);
        [MethodImpl(AggressiveInlining)] public static int CombineHashCodes<T0, T1>(T0 x0, T1 x1) => HashCode.Combine(x0, x1);
        [MethodImpl(AggressiveInlining)] public static int CombineHashCodes<T0, T1, T2>(T0 x0, T1 x1, T2 x2) => HashCode.Combine(x0, x1, x2);
        [MethodImpl(AggressiveInlining)] public static int CombineHashCodes<T0, T1, T2, T3>(T0 x0, T1 x1, T2 x2, T3 x3) => HashCode.Combine(x0, x1, x2, x3);
    }

    // Eager combinators over IReadOnlyList<T>. Only the UNOPTIMIZED variant reaches these; the
    // optimized variant lowers/unrolls every combinator, so it needs none of them at runtime.
    public static class SmallArrayExtensions
    {
        public static IReadOnlyList<TR> Map<T0, TR>(this IReadOnlyList<T0> xs, Func<T0, TR> f)
        {
            var r = new TR[xs.Count];
            for (var i = 0; i < r.Length; i++) r[i] = f(xs[i]);
            return r;
        }

        public static IReadOnlyList<TR> Zip<T0, T1, TR>(this IReadOnlyList<T0> xs, IReadOnlyList<T1> ys, Func<T0, T1, TR> f)
        {
            var n = Math.Min(xs.Count, ys.Count);
            var r = new TR[n];
            for (var i = 0; i < n; i++) r[i] = f(xs[i], ys[i]);
            return r;
        }

        public static IReadOnlyList<TR> Zip<T0, T1, T2, TR>(this IReadOnlyList<T0> xs, IReadOnlyList<T1> ys, IReadOnlyList<T2> zs, Func<T0, T1, T2, TR> f)
        {
            var n = Math.Min(xs.Count, Math.Min(ys.Count, zs.Count));
            var r = new TR[n];
            for (var i = 0; i < n; i++) r[i] = f(xs[i], ys[i], zs[i]);
            return r;
        }

        public static TAcc Reduce<T0, TAcc>(this IReadOnlyList<T0> xs, TAcc acc, Func<TAcc, T0, TAcc> f)
        {
            for (var i = 0; i < xs.Count; i++) acc = f(acc, xs[i]);
            return acc;
        }

        public static bool All<T0>(this IReadOnlyList<T0> xs, Func<T0, bool> f)
        {
            for (var i = 0; i < xs.Count; i++) if (!f(xs[i])) return false;
            return true;
        }

        public static bool Any<T0>(this IReadOnlyList<T0> xs, Func<T0, bool> f)
        {
            for (var i = 0; i < xs.Count; i++) if (f(xs[i])) return true;
            return false;
        }

        public static IReadOnlyList<T0> Reverse<T0>(this IReadOnlyList<T0> xs)
        {
            var n = xs.Count;
            var r = new T0[n];
            for (var i = 0; i < n; i++) r[i] = xs[n - 1 - i];
            return r;
        }

        // Named only by the emitter's always-present Integer.MakeArray2D bridge; never called
        // in the small corpus, so a stub satisfies the reference.
        public static Ara3D.Collections.ReadOnlyList2D<T> MakeArray2D<T>(this Integer columns, Integer rows, Func<Integer, Integer, T> f)
            => new();
    }
}
