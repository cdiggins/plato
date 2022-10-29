using System;

namespace Plato
{
    public partial struct Float2 : 
        IVector<float>
    {
        public float X { get; }
        public float Y { get; }
    }

    public partial struct Float3 
        : IVector<float>
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
    }

    class Extensions
    {
        Float3 Multiply(Float3 a, Float3 b)
            => (a.X * b.X, a.Y * b.Y, a.Z * b.Z);

        float Sum(Float3 f)
            => f.X + f.Y + f.Z;

        float Dot(Float3 a, Float3 b)
            => Multiply(a, b).Sum();
    }

    public partial struct Float4 : IVector<float>
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float W { get; }
    }

    public partial struct Double2 : IVector<double>
    {
        public double X { get; }
        public double Y { get; }
    }

    public partial struct Double3 : IVector<double>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
    }

    public partial struct Double4 : IVector<double>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public double W { get; }
    }

    public partial struct Quaternion : INumber
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public double W { get; }
    }

    public partial struct AxisAngle : IValue
    {
        public Double3 Axis { get; }
        public Angle Angle { get; }
    }

    public partial struct EulerAngles : IValue
    {
        public Angle Yaw { get; }
        public Angle Pitch { get; }
        public Angle Roll { get; }
    }

    public partial struct EulerZXY
    { }

    public partial struct Rotation : IValue
    {
        public Quaternion Quaternion { get; }
        public static implicit operator Quaternion(Rotation rotation) => rotation.Quaternion;
        public static implicit operator Rotation(Quaternion quaternion) => new Rotation(quaternion);
        // TODO: ToMatrix 
        // TODO: To/From Euler Angles
        // TODO: To/From Axis Angles
        // TODO: Add Rotations
    }

    public partial struct Byte2 : IValue
    {
        public byte A { get; }
        public byte B { get; }
    }

    public partial struct Byte3 : IValue
    {
        public byte A { get; }
        public byte B { get; }
        public byte C { get; }
    }

    public partial struct Byte4 : IValue
    {
        public byte A { get; }
        public byte B { get; }
        public byte C { get; }
        public byte D { get; }
    }

    public partial struct Int2 : IVector<int>
    {
        public int A { get; }
        public int B { get; }
    }

    public partial struct Int3 : IVector<int>
    {
        public int A { get; }
        public int B { get; }
        public int C { get; }
    }

    public partial struct Int4 : IVector<int>
    {
        public int A { get; }
        public int B { get; }
        public int C { get; }
        public int D { get; }
    }

    public partial struct Long2 : IVector<long>
    {
        public long A { get; }
        public long B { get; }
    }

    public partial struct Long3 : IVector<long>
    {
        public long A { get; }
        public long B { get; }
        public long C { get; }
    }

    public partial struct Long4 : IVector<long>
    {
        public long A { get; }
        public long B { get; }
        public long C { get; }
        public long D { get; }
    }

    public partial struct Pose : INumber
    {
        public Double3 Position { get; }
        public Rotation Orientation { get; }
    }

    public partial struct Transform : INumber
    {
        public Double3 Translation { get; }
        public Quaternion Rotation { get; }
        public Double3 Scale { get; }
    }

    public partial struct AABBox2D : IInterval<Double2>
    {
        public Double2 Lower { get; }
        public Double2 Upper { get; }
    }

    public partial struct AABBox3D : IInterval<Double3>
    {
        public Double3 Lower { get; }
        public Double3 Upper { get; }
    }

    public partial struct Complex : IVector<double>
    {
        public double Real { get; }
        public double Imaginary { get; }
    }

    public partial struct Ray : IValue, IShape
    {
        public Double3 Direction { get; }
        public Point Position { get; }
    }

    public partial struct Ray2D : IValue, IShape2D
    {
        public Double2 Direction { get; }
        public Point2D Position { get; }
    }

    public partial struct Sphere : IValue
    {
        public Point Center { get; }
        public double Radius { get; }
    }

    public partial struct Plane : IValue
    {
        public Double3 Normal { get; }
        public double D { get; }
    }

    public partial struct Triangle : IVector<Double3>
    {
        public Double3 A { get; }
        public Double3 B { get; }
        public Double3 C { get; }
    }

    public partial struct Triangle2D : IVector<Double2>
    {
        public Double2 A { get; }
        public Double2 B { get; }
        public Double2 C { get; }
    }

    public partial struct Quad : IVector<Double3>
    {
        public Double3 A { get; }
        public Double3 B { get; }
        public Double3 C { get; }
        public Double3 D { get; }
    }

    public partial struct Point : IShape
    { 
        public Double3 Value { get; }       
    }

    public partial struct Point2D : IShape2D
    {
        public Double2 Value { get; }
    }

    public partial struct Line : IVector<Double3>, IShape
    {
        public Point A { get; }
        public Point B { get; }
    }

    public partial struct Line2D : IVector<Double2>, IShape2D
    {
        public Point2D A { get; }
        public Point2D B { get; }
    }

    public partial struct Color : IValue
    {
        public double R { get; }
        public double G { get; }
        public double B { get; }
        public double A { get; }
    }

    public partial struct ColorHSV : IValue
    {
        public double H { get; }
        public double S { get; }
        public double V { get; }
    }

    public partial struct ColorHSL : IValue
    {
        public double Hue { get; }
        public double Saturation { get; }
        public double Luminance { get; }
    }

    public partial struct ColorYCbCr : IValue
    {
        public double Y { get; }
        public double Cb { get; }
        public double Cr { get; }
    }

    public partial struct SphericalCoordinate : IValue
    {
        public double Radius { get; }
        public Angle Azimuth { get; }
        public Angle Inclination { get; }
    }

    public partial struct PolarCoordinate : IValue
    {
        public double Radius { get; }
        public Angle Azimuth { get; }
    }

    public partial struct LogPolarCoordinate : IValue
    {
        public double Rho { get; }
        public Angle Azimuth { get; }
    }

    public partial struct HorizontalCoordinate : IValue
    {
        public double Radius { get; }
        public Angle Azimuth { get; }
        public double Height { get; }
    }

    public partial struct GeoCoordinate : IValue
    {
        public double Latitude { get; }
        public double Longitude { get; }
        public double Altitude { get; }
    }

    public partial struct Circle : IValue
    {
        public Double2 Position { get; }
        public double Radius { get; }
    }

    public partial struct Size : IValue
    {
        public double Width { get; }
        public double Height { get; }
    }

    public partial struct Rectangle : IValue
    {
        public Double2 TopLeft { get; }
        public Size Size { get; }
    }

    public partial struct Percent : INumber
    {
        public double Value { get; }
        public static Percent FromUnit(double value) => value * 100.0;
        public Unit ToUnit() => Value / 100.0;
        public static implicit operator double(Percent amount) => amount.Value;
        public static implicit operator Percent(double value) => new Percent(value);
    }

    public partial struct Unit : INumber
    {
        public double Value { get; }
        public static Unit FromPercent(double percent) => percent / 100.0;
        public Percent ToPercent() => 100.0 * Value;
        public static implicit operator double(Unit amount) => amount.Value;
        public static implicit operator Unit(double value) => new Unit(value);
    }

    public partial struct Amount : IMeasure
    {
        public double Mole { get; }
        public static implicit operator double(Amount amount) => amount.Value;
        public static implicit operator Amount(double value) => new Amount(value);
    }

    public partial struct Fraction : INumber
    {
        public double Numerator { get; }
        public double Denominator { get; }
    }

    public partial struct Ratio : INumber
    {
        public long A { get; }
        public long B { get; }
    }

    public partial struct Angle : IMeasure
    {
        public double Radians { get; }
        public const double RadiansPerRevolution = Math.PI * 2;
        public const double DegreesPerRevolution = 360;
        public const string InternalUnit = nameof(Radians);
        public static Angle FromRadians(double radians) => new Angle(radians);
        public static Angle FromRevolutions(double revolutions) => revolutions * RadiansPerRevolution;
        public static Angle FromDegrees(double degrees) => FromRevolutions(DegreesPerRevolution / degrees);
        public double ToDegrees() => DegreesPerRevolution * ToRevolutions();
        public double ToRevolutions() => Radians / RadiansPerRevolution;
        public double ToRadians() => Radians;
        public static implicit operator double(Angle angle) => angle.Radians;
        public static implicit operator Angle(double radians) => new Angle(radians);
    }

    public partial struct Distance : IMeasure
    {
        public const double FeetPerMeter = 3.280839895;
        public const double FeetPerMile = 5280;
        public const double MetersPerLightyear = 9.46073047258e+15;
        public const double MetersPerAU = 149597870691;

        public double Meters { get; }
        public const string InternalUnit = nameof(Meters);
        public static Distance FromMeters(double value) => value;
        public static Distance FromKilometer(double value) => value * 1000;
        public static Distance FromCentimeters(double value) => value / 100;
        public static Distance FromMillimeters(double value) => value / 100;
        public static Distance FromMicrons(double value) => value / 1000 / 1000;
        public static Distance FromNanometers(double value) => value / 1000 / 1000 / 1000;
        public static Distance FromInches(double value) => FromFeet(value / 12);
        public static Distance FromFeet(double value) => value / FeetPerMeter;
        public static Distance FromYards(double value) => FromFeet(value * 3);
        public static Distance FromMiles(double value) => FromFeet(value * FeetPerMile);
        public static Distance FromLightyears(double value) => value * MetersPerLightyear;
        public static Distance FromAU(double value) => value * MetersPerAU;
        public double ToMeters() => Meters;
        public double ToKilometers() => Meters / 1000;
        public double ToCentimeters() => Meters * 100;
        public double ToMillimeters() => Meters * 1000;
        public double ToMicrons() => ToMillimeters() * 1000;
        public double ToNanometers() => ToMicrons() * 1000;
        public double ToInches() => ToFeet() * 12;
        public double ToFeet() => Meters * FeetPerMeter;
        public double ToYards() => ToFeet() / 3;
        public double ToMiles() => Meters * FeetPerMeter / FeetPerMile;
        public double ToLightuears() => Meters / MetersPerLightyear;
        public double ToAU() => Meters / MetersPerAU;

        public static implicit operator double(Distance distance) => distance.Meters;
        public static implicit operator Distance(double meters) => new Distance(meters);
    }

    public partial struct Mass : IMeasure
    {
        public double Kilograms { get; }
        public const double DaltonPerKilogram = 1.66053e-27;
        public const double PoundPerKilogram = 0.45359237;
        public const double PoundPerTon = 2000;
        public const double KilogramPerSolarMass = 1.9889200011446E+30;

        public const string InternalUnit = nameof(Kilograms);
        public static Mass FromMilligrams(double value) => value / 1000 / 1000;
        public static Mass FromGrams(double value) => value / 1000;
        public static Mass FromKilograms(double value) => value;
        public static Mass FromDalton(double value) => value * DaltonPerKilogram;
        public static Mass FromTonne(double value) => value * 1000;
        public static Mass FromPound(double value) => value * PoundPerKilogram;
        public static Mass FromTon(double value) => FromPound(value * PoundPerTon);
        public static Mass FromSolarMass(double value) => value * KilogramPerSolarMass;

        public double ToMilligrams() => ToGrams() * 1000;
        public double ToGrams() => Kilograms * 1000;
        public double ToKilograms() => Kilograms;
        public double ToDalton() => Kilograms / DaltonPerKilogram;
        public double ToTonne() => Kilograms / 1000;
        public double ToPound() => Kilograms / PoundPerKilogram;
        public double ToTon() => ToPound() / PoundPerTon;
        public double ToSolarMass() => Kilograms / KilogramPerSolarMass;

        public static implicit operator double(Mass mass) => mass.Kilograms;
        public static implicit operator Mass(double kilograms) => new Mass(kilograms);
    }

    public partial struct Temperature : INumber
    {
        public double Celsius { get; }

        public Temperature(double celsius) => Celsius = celsius;

        public static Temperature FromCelsius(double value) => value;
        public static Temperature FromKelvin(double value) => value - 273.15;
        public static Temperature FromFaranheit(double value) => (value - 32.0) * 5.0 / 9.0;

        public double ToCelsius() => Celsius;
        public double ToKelvin() => Celsius + 273.15;
        public double ToFaranheit() => (Celsius * 9.0 / 5.0) + 32.0;

        public static implicit operator double(Temperature temperature) => temperature.Celsius;
        public static implicit operator Temperature(double celsius) => new Temperature(celsius);
    }

    public partial struct Time : IMeasure
    {
        public double Seconds { get; }

        public static Time FromNanoseconds(double value) => value / 1000 / 1000 / 10000;
        public static Time FromMicroseconds(double value) => value / 1000 / 1000;
        public static Time FromMilliseconds(double value) => value / 1000;
        public static Time FromSeconds(double value) => value;
        public static Time FromMinutes(double value) => value * 60;
        public static Time FromHours(double value) => value * 60 * 60;
        public static Time FromDays(double value) => value * 60 * 60 * 24;
        public static Time FromWeeks(double value) => value * 60 * 60 * 24 * 7;

        public double ToNanosecond() => Seconds * 1000 * 1000 * 1000;
        public double ToMicroeconds() => Seconds * 1000 * 1000;
        public double ToMilliseconds() => Seconds * 1000;
        public double ToSeconds() => Seconds;
        public double ToMinutes() => Seconds / 60;
        public double ToHours() => Seconds / (60 * 60);
        public double ToDays() => Seconds / (60 * 60 * 24);
        public double ToWeeks() => Seconds / (60 * 60 * 24 * 7);

        public static implicit operator double(Time duration) => duration.Seconds;
        public static implicit operator Time(double value) => new Time(value);
    }

    public partial struct TimeInterval : IInterval<Time>
    {
        public Time Start { get; }
        public Time End { get; }
    }

    public partial struct Interval : IInterval<double>
    {
        public double A { get; }
        public double B { get; }
    }

    public partial struct Interval2D : IInterval<Double2>
    {
        public Double2 A { get; }
        public Double2 B { get; }
    }

    public partial struct Interval3D : IInterval<Double3>
    {
        public Double3 A { get; }
        public Double3 B { get; }
    }

    public interface IShape : ISignedDistanceField
    { }

    public interface IShape2D : ISignedDistanceField2D
    { }

    public partial struct Capsule : IShape, IValue
    {
        public Line Line { get; }
        public double Radius { get; }
    }

    public partial struct Cylinder : IShape, IValue
    {
        public Line Line { get; }
        public double Radius { get; }
    }

    public partial struct Cone : IShape, IValue
    {
        public Line Line { get; }
        public double Radius { get; }
    }

    public partial struct Tube : IShape, IValue
    {
        public Line Line { get; }
        public double InnerRadius { get; }
        public double OuterRadius { get; }
    }

    public partial struct ConeSegment : IShape, IValue
    {
        public Line Line { get; }
        public double Radius1 { get; }
        public double Radius2 { get; }
    }

    public partial struct Box : IShape, IValue
    {
        public Double3 Position { get; }
        public Rotation Rotation { get; }
        public Double3 Extent { get; }
    }

    public partial struct AnimatedValue<T>
    {
        public Time Time { get; }
        public T Value => Function(Time);
        public static implicit operator T(AnimatedValue<T> animatedValue) => animatedValue.Value;
        public Func<Time, T> Function { get; }
    }

    public partial struct Animation<T> : IValue
    {
        public Func<Time, T> Function { get; }
        public T GetValue(Time time) => Function(time);
    }

    public interface ISignedDistanceField
    {
        double Distance(Double3 pos); 
    }

    public interface ISignedDistanceField2D
    {
        double Distance(Double2 pos);
    }

    public class Test
    {
        public static float Diff(float a, float b) => a - b;
        public float Diff(float b) => b - b;
    }
}