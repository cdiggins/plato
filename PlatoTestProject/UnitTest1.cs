namespace PlatoTestProject
{
    // Here is the basic class 
    public class Vector2_ClassNaive
    {
        public double x, y;
    }

    // Once we decide immutability is a good thing 
    public class Vector2_ClassWithFields
    {
        public readonly double x, y;
    }

    // Same thing, I guess?
    public class Vector2_ClassWithProperties
    {
        public double x { get; init; }
        public double y { get; init; }
    }

    // Instead of a class we can also use a struct (might be faster)
    public struct Vector2_Struct
    {
        public readonly double x, y;
    }

    // Structs let us declare the whole things as "Readonly" (which they should be)
    public readonly struct Vector2_ReadOnlyStruct
    {
        public readonly double x, y;
    }

    // What is a "ref struct" and does it have advantages? Not clear from the docs. Answer is no. 
    public readonly ref struct Vector2_ReadOnlyRefStruct
    {
        public readonly double x, y;
    }

    // This is similar to the class, but gives us some boilerplate 
    public record Vector2_Record(double x, double y);

    // Can't use readonly, so could write instead 
    public record Vector2_RecordLongForm 
    {
        public readonly double x, y;
    }

    // Now in C# 10 we can also do this, which is great but not widely supported
    public readonly record struct Vector2_RecordStruct(double x, double y);

    // In Plato we write, get a lot more boilerplate, and are backwards compatible
    // See: https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.types.plato.cs
    // And: https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.types.plato.g.cs
    [Vector]
    class Vector2
    {
        double x, y;
    }

    class VectorAttribute : Attribute { }

    public static class TesClass
    {
        public static void test()
        {
            var xs = Enumerable.Range(0, 100);
            
            var exponent = 2;
            xs.Select(x => Math.Pow(x, exponent))
              .Aggregate((x, y) => x + y);
        }

        public readonly record struct Polar(double Angle, double Radius);

        public static Polar MultiplyRadius(Polar polar, double scalar)
            => polar with { Radius = polar.Radius * scalar };
    }
}