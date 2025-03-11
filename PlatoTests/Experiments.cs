namespace PlatoTests
{
    public interface INumerical<TSelf>
    {
        TSelf Add(TSelf b);
        TSelf Negate(); 
    }

    public interface IComparable<TSelf>
    {
        int Compare(TSelf b);
    }
    
    public interface IVector
    {
        float X { get; }
        float Count { get; }
    }

    public class Number : INumerical<Number>, IComparable<Number>
    {
        public int x;
        public Number Add(Number b) => new Number() { x = x + b.x };
        public Number Negate() => new() { x = -x };
        public int Compare(Number b) => x - b.x;
    }

    public class Vector : INumerical<Vector>, IVector
    {
        public float X;
        float IVector.X => X;
        public Vector Add(Vector b) => new Vector() { X = X + b.X };
        public Vector Negate() => new Vector() { X = -X };
        

        public float Count => 1;
    }

    public static class Extensions
    {
        public static T Subtract<T>(this INumerical<T> a, INumerical<T> b) 
        {
            return a.Add(b.Negate());
        }

        public static int Count(this IVector v) => 2;

        public static float X(this Vector v) => 2;

        public static int AddCount(this Vector v, IVector other) => 13;

        public static float AddCount(this Vector v, Vector other) => 13.1f;

        [Test]
        public static void TestConcept()
        {
            var a = new Number() { x = 3 };
            var b = new Vector() { X =  3.14f };
            a.Subtract(a);
        }

        [Test]
        public static void TestExtension()
        {
            var b = new Vector() { X = 3.14f };

            Console.WriteLine("Expect 3.14 then 2");
            Console.WriteLine(b.X);
            Console.WriteLine(b.X());

            Console.WriteLine("Expect 1 then 2");
            Console.WriteLine(b.Count);
            Console.WriteLine(b.Count());

            Console.WriteLine("Expect 13.1 then 13");
            Console.WriteLine(b.AddCount((Vector)b));
            Console.WriteLine(b.AddCount((IVector)b));
        }
    }
}
