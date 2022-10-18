using Plato;
using static TestProject1.Intrinsics;

namespace TestProject1
{
    public static class Intrinsics
    {
        public static Fubar Add(Fubar a, Fubar b)
            => a;
    }

    public class Fubar { }

    public static class Tests
    {
        [Test]
        public static void Test()
        {
            /*
            var v = new Vector3(3, 4, 5);
            //var z = v.SumComponents();
            Console.WriteLine(v);

            float f = 3.14f;
            Add(f, f);

            Fubar f2 = new Fubar();
            Add(f2, f2);
            */
        }
    }
}