using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plato
{
    public class TestResult
    {
        string Name;
        int InputSize;
        TimeSpan SpanA;
        TimeSpan SpanB;
    }

    public static class Program
    {
        static void Main(string[] args)
        {
            
        }

        public static TimeResult CompareAggregate()
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

        /*
        public static IEnumerable<T> ComposedSelect<T>(this IEnumerable<Task> )
        */
        public static void TestIt()
        {
            // Temporary allocations
            // 

            var loopSize = 10000;
            for (var i=0; i < loopSize; ++i)
            {
            }
        }
    }
}
