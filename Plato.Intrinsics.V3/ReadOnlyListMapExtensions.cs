namespace Ara3D.Geometry
{
    public static class ReadOnlyListMapExtensions
    {
        // TODO: I have to decide what to do here. 

        /*
        public static IReadOnlyList<T> Repeat<T>(this T value, int count) => 
            new FunctionalArray<T>(count, _ => value);

        public static IReadOnlyList<TR> Map<T0, TR>(this IReadOnlyList<T0> xs0, Func<T0, TR> func) => 
            new FunctionalArray<TR>(xs0.Count, i => func(xs0[i]));

        public static IReadOnlyList<TR> Map<T0, T1, TR>(this IReadOnlyList<T0> xs0, IReadOnlyList<T1> xs1, Func<T0, T1, TR> func) =>
            xs0.Count != xs1.Count
                ? throw new InvalidOperationException("Arrays are not the same length")
                : new FunctionalArray<TR>(xs0.Count, i => func(xs0[i], xs1[i]));

        public static IReadOnlyList<TR> Map<T0, T1, TR>(this IReadOnlyList<T0> xs0, T1 x1, Func<T0, T1, TR> func) => 
            Map(xs0, x1.Repeat(xs0.Count), func);
        
        public static IReadOnlyList<TR> Map<T0, T1, TR>(this T0 x0, IReadOnlyList<T1> xs1, Func<T0, T1, TR> func) => 
            Map(x0.Repeat(xs1.Count), xs1, func);

        public static IReadOnlyList<TR> Map<T0, T1, T2, TR>(this IReadOnlyList<T0> xs0, IReadOnlyList<T1> xs1, IReadOnlyList<T2> xs2, Func<T0, T1, T2, TR> func) =>
            xs0.Count != xs1.Count || xs0.Count != xs2.Count 
                ? throw new InvalidOperationException("Arrays are not the same length")
                : new FunctionalArray<TR>(xs0.Count, i => func(xs0[i], xs1[i], xs2[i]));

        public static IReadOnlyList<TR> Map<T0, T1, T2, TR>(this IReadOnlyList<T0> xs0, IReadOnlyList<T1> xs1, T2 x2,Func<T0, T1, T2, TR> func) => 
            Map(xs0, xs1, x2.Repeat(xs0.Count), func);
        
        public static IReadOnlyList<TR> Map<T0, T1, T2, TR>(this IReadOnlyList<T0> xs0, T1 x1, IReadOnlyList<T2> xs2, Func<T0, T1, T2, TR> func) =>
            Map(xs0, x1.Repeat(xs0.Count), xs2, func);
        
        public static IReadOnlyList<TR> Map<T0, T1, T2, TR>(this T0 x0, IReadOnlyList<T1> xs1, IReadOnlyList<T2> xs2, Func<T0, T1, T2, TR> func) =>
            Map(x0.Repeat(xs1.Count), xs1, xs2, func);
        
        public static IReadOnlyList<TR> Map<T0, T1, T2, TR>(this IReadOnlyList<T0> xs0, T1 x1, T2 x2, Func<T0, T1, T2, TR> func) =>
            Map(xs0, x1.Repeat(xs0.Count), x2.Repeat(xs0.Count), func);
        
        public static IReadOnlyList<TR> Map<T0, T1, T2, TR>(this T0 x0, IReadOnlyList<T1> xs1, T2 x2, Func<T0, T1, T2, TR> func) =>
            Map(x0.Repeat(xs1.Count), xs1, x2.Repeat(xs1.Count), func);

        public static IReadOnlyList<TR> Map<T0, T1, T2, TR>(this T0 x0, T1 x1, IReadOnlyList<T2> xs2,
            Func<T0, T1, T2, TR> func) =>
            Map(x0.Repeat(xs2.Count), x1.Repeat(xs2.Count), xs2, func);
        */
    }
}