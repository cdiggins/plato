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

    public class Number : INumerical<Number>, IComparable<Number>
    {
        public int x;
        public Number Add(Number b) => new Number() { x = x + b.x };
        public Number Negate() => new() { x = -x };
        public int Compare(Number b) => x - b.x;
    }

    public class Vector : INumerical<Vector>
    {
        public float x;
        public Vector Add(Vector b) => new Vector() { x = x + b.x };
        public Vector Negate() => new Vector() { x = -x };
    }

    public static class Extensions
    {
        public static T Subtract<T>(this INumerical<T> a, INumerical<T> b) 
        {
            return a.Add(b.Negate());
        }

        [Test]
        public static void TestConcept()
        {
            var a = new Number() { x = 3 };
            var b = new Vector() { x =  3.14f };
            a.Subtract(a);
        }
    }
}
