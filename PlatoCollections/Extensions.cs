namespace Plato;

public static class Extensions
{
    public static IGenerator<T> Unit<T>(this T x)
        => new UnitGenerator<T>(x);

    public static EmptyGenerator<T> Empty<T>(this T _)
        => Empty<T>();

    public static EmptyGenerator<T> Empty<T>()
        => EmptyGenerator<T>.Default;

    public static RangeGenerator Range(this int n)
        => new(0, n);

    public static RangeGenerator Range(this int n, int upto)
        => new(n, upto);

    public static RepeatGenerator<T> Repeat<T>(this T x, int value)
        => new(x, value);

    public static FunctionalGenerator<T> Create<T>(T value, Func<T, bool> hasValueFunc, Func<T, T> nextFunc)
        => new(value, hasValueFunc, nextFunc);

    public static IGenerator<T> Empty<T>(this IGenerator<T> _)
        => Empty<T>();

    public static IArray<T> Select<T>(this int count, Func<int, T> func)
        => new FunctionalArray<T>(count, func);

    public static ISet<T> ToSet<T>(this Func<T, bool> predicate) 
        => new Set<T>(predicate);

    public static Func<T, bool> Negate<T>(this Func<T, bool> func)
        => x => !func(x);
}
