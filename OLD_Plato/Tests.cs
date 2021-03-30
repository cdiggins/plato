using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plato
{
    public class TestResult
    {
        public string Name;
        public int InputSize;
        public object ResultA;
        public object ResultB;
        public TimeSpan SpanA;
        public TimeSpan SpanB;
    }

    public static class Tests
    {
        public static void Measure()
        {
        }

        public static void CompareForLoopToForEach<T, U>(T[] input, Func<T[], U> f)
        {
            
        }
        public static float Sqr(float x)
            => x * x;

        public static string[] SqrToString(float[] vals)
        {
            var tmp = new float[vals.Length];
            var r = new string[vals.Length];
            for (var i = 0; i < vals.Length; ++i)
                tmp[i] = vals[i];
            for (var i = 0; i < vals.Length; ++i)
                r[i] = tmp[i].ToString();
            return r;
        }

        public static string[] SqrToString_WithoutTemp(float[] vals)
        {
            var r = new string[vals.Length];
            for (var i = 0; i < vals.Length; ++i)
                r[i] = (vals[i] * vals[i]).ToString();
            return r;
        }

        public static List<string> SqrToString_WithArray(float[] vals)
        {
            var r = new List<string>();
            for (var i = 0; i < vals.Length; ++i)
                r.Add((vals[i] * vals[i]).ToString());
            return r;
        }

        public static float[] MakeInputFloatArray(int count)
            => Enumerable.Range(0, count).Select(x => x / 1000f).ToArray();

        public static TestResult Compare<T, TOutput1, TOutput2>(T input, Func<T, Output1> f1, Func<T, TOutput2> f2)
        {
            var r = new TestResult();            
            (r.ResultA, r.SpanA) = f1.TimeIt(input);
            (r.ResultB, r.SpanB) = f2.TimeIt(input);
            return r;
        }

        public static void OutputFormatted(this TestResult result)
            => Console.WriteLine($"Result A = {result.ResultA}, Result B = {result.ResultB}, TimeSpane 1 = {result.SpanA.")
    }
}
