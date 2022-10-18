using System;
using System.Diagnostics;

namespace Plato
{
    public static class TestUtils
    {
        static readonly string[] ByteSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB

        public static string BytesToString(long byteCount, int numPlacesToRound = 1)
        {
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), numPlacesToRound);
            return $"{Math.Sign(byteCount) * num}{ByteSuffixes[place]}";
        }

        public static string ToFormattedString(this TimeSpan ts)
        {
            return $"{ts.Minutes}:{ts.Seconds}.{ts.Milliseconds:D3}";
        }

        public static (T, TimeSpan) InvokeWithTiming<T>(this Func<T> function)
        {
            var sw = Stopwatch.StartNew();
            return (function(), sw.Elapsed);
        }

        public static (U, TimeSpan) InvokeWithTiming<T, U>(this Func<T, U> function, T input)
        {
            var sw = Stopwatch.StartNew();
            return (function(input), sw.Elapsed);
        }
    }
}
