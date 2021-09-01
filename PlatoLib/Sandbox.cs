namespace PlatoLib
{
    /*
    public static class Sandbox
    {
        public static int Sum(this IEnumerable<int> source) => throw new NotImplementedException();

        public static int? Sum(this IEnumerable<int?> source) => throw new NotImplementedException();

        public static long Sum(this IEnumerable<long> source) => throw new NotImplementedException();

        public static long? Sum(this IEnumerable<long?> source) => throw new NotImplementedException();

        public static float Sum(this IEnumerable<float> source) => throw new NotImplementedException();

        public static float? Sum(this IEnumerable<float?> source) => throw new NotImplementedException();

        public static double Sum(this IEnumerable<double> source) => throw new NotImplementedException();

        public static double? Sum(this IEnumerable<double?> source) => throw new NotImplementedException();

        public static decimal Sum(this IEnumerable<decimal> source) => throw new NotImplementedException();

        public static decimal? Sum(this IEnumerable<decimal?> source) => throw new NotImplementedException();

        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => throw new NotImplementedException();

        public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => throw new NotImplementedException();

        public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => throw new NotImplementedException();

        public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => throw new NotImplementedException();

        public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => throw new NotImplementedException();

        public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => throw new NotImplementedException();

        public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => throw new NotImplementedException();

        public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => throw new NotImplementedException();

        public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => throw new NotImplementedException();

        public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => throw new NotImplementedException();

        public static int Min(this IEnumerable<int> source) => throw new NotImplementedException();

        public static int? Min(this IEnumerable<int?> source) => throw new NotImplementedException();

        public static long Min(this IEnumerable<long> source) => throw new NotImplementedException();

        public static long? Min(this IEnumerable<long?> source) => throw new NotImplementedException();

        public static float Min(this IEnumerable<float> source) => throw new NotImplementedException();

        public static float? Min(this IEnumerable<float?> source) => throw new NotImplementedException();

        public static double Min(this IEnumerable<double> source) => throw new NotImplementedException();

        public static double? Min(this IEnumerable<double?> source) => throw new NotImplementedException();

        public static decimal Min(this IEnumerable<decimal> source) => throw new NotImplementedException();

        public static decimal? Min(this IEnumerable<decimal?> source) => throw new NotImplementedException();

        public static TSource Min<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => throw new NotImplementedException();

        public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => throw new NotImplementedException();

        public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => throw new NotImplementedException();

        public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => throw new NotImplementedException();

        public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => throw new NotImplementedException();

        public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => throw new NotImplementedException();

        public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => throw new NotImplementedException();

        public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => throw new NotImplementedException();

        public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => throw new NotImplementedException();

        public static decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => throw new NotImplementedException();

        public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) => throw new NotImplementedException();

        public static int Max(this IEnumerable<int> source) => throw new NotImplementedException();

        public static int? Max(this IEnumerable<int?> source) => throw new NotImplementedException();

        public static long Max(this IEnumerable<long> source) => throw new NotImplementedException();

        public static long? Max(this IEnumerable<long?> source) => throw new NotImplementedException();

        public static double Max(this IEnumerable<double> source) => throw new NotImplementedException();

        public static double? Max(this IEnumerable<double?> source) => throw new NotImplementedException();

        public static float Max(this IEnumerable<float> source) => throw new NotImplementedException();

        public static float? Max(this IEnumerable<float?> source) => throw new NotImplementedException();

        public static decimal Max(this IEnumerable<decimal> source) => throw new NotImplementedException();

        public static decimal? Max(this IEnumerable<decimal?> source) => throw new NotImplementedException();

        public static TSource Max<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => throw new NotImplementedException();

        public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => throw new NotImplementedException();

        public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => throw new NotImplementedException();

        public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => throw new NotImplementedException();

        public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => throw new NotImplementedException();

        public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => throw new NotImplementedException();

        public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => throw new NotImplementedException();

        public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => throw new NotImplementedException();

        public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => throw new NotImplementedException();

        public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => throw new NotImplementedException();

        public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) => throw new NotImplementedException();

        public static double Average(this IEnumerable<int> source) => throw new NotImplementedException();

        public static double? Average(this IEnumerable<int?> source) => throw new NotImplementedException();

        public static double Average(this IEnumerable<long> source) => throw new NotImplementedException();

        public static double? Average(this IEnumerable<long?> source) => throw new NotImplementedException();

        public static float Average(this IEnumerable<float> source) => throw new NotImplementedException();

        public static float? Average(this IEnumerable<float?> source) => throw new NotImplementedException();

        public static double Average(this IEnumerable<double> source) => throw new NotImplementedException();

        public static double? Average(this IEnumerable<double?> source) => throw new NotImplementedException();

        public static decimal Average(this IEnumerable<decimal> source) => throw new NotImplementedException();

        public static decimal? Average(this IEnumerable<decimal?> source) => throw new NotImplementedException();

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => throw new NotImplementedException();

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => throw new NotImplementedException();

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => throw new NotImplementedException();

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => throw new NotImplementedException();

        public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => throw new NotImplementedException();

        public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => throw new NotImplementedException();

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => throw new NotImplementedException();

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => throw new NotImplementedException();

        public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => throw new NotImplementedException();

        public static decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => throw new NotImplementedException();
    }
    */
}
