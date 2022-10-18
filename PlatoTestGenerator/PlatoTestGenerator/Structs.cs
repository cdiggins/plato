using System;
using System.Diagnostics;

namespace Plato
{
    public partial struct Vector2 : IVector<float>
    {
        public float X { get; }
        public float Y { get; }
    }

    public partial struct Vector3 : IVector<float>
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
    }

    public partial struct Vector4 : IVector<float>
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float W { get; }
    }

    public partial struct Quaternion : INumber
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float W { get; }
    }

    public partial struct DVector2 : IVector<double>
    {
        public double X { get; }
        public double Y { get; }
    }

    public partial struct DVector3 : IVector<double>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
    }

    public partial struct DVector4 : IVector<double>
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public double W { get; }
    }

    public partial struct DQuaternion : INumber
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public double W { get; }
    }

    public partial struct Byte2 
    {
        public byte A { get; }
        public byte B { get; }
    }

    public partial struct Byte3
    {
        public byte A { get; }
        public byte B { get; }
        public byte C { get; }
    }

    public partial struct Byte4
    {
        public byte A { get; }
        public byte B { get; }
        public byte C { get; }
        public byte D { get; }
    }

    public partial struct Pose : INumber
    {
        public Vector3 Position { get; }
        public Quaternion Orientation { get; }
    }

    public partial struct DPose : INumber
    {
        public DVector3 Position { get; }
        public DQuaternion Orientation { get; }
    }

    public partial struct Transform : INumber
    {
        public Vector3 Translation { get; }
        public Quaternion Rotation { get; }
        public Vector3 Scale { get; }
    }

    public partial struct BoundingBox2D : IInterval<Vector2>
    {
        public Vector2 Lower { get; }
        public Vector2 Upper { get; }
    }

    public partial struct BoundingBox3D : IInterval<Vector3>
    {
        public Vector3 Lower { get; }
        public Vector3 Upper { get; }
    }

    public partial struct Interval : IInterval<float>
    {
        public float Lower { get; }
        public float Upper { get; }
    }

    public partial struct DInterval : IInterval<double>
    {
        public double Lower { get; }
        public double Upper { get; }
    }

    public partial struct Complex : IVector<double>
    {
        public double Real { get; }
        public double Imaginary { get; }
    }

    public partial struct Ray : IValue
    {
        public Vector3 Direction { get; }
        public Vector3 Position { get; }
    }

    public partial struct Sphere : IValue
    {
        public Vector3 Center { get; }
        public float Radius { get; }
    }

    public partial struct Plane : IValue
    {
        public Vector3 Normal { get; }
        public float D { get; }
    }

    public partial struct Triangle : IVector<Vector3>
    {
        public Vector3 A { get; }
        public Vector3 B { get; }
        public Vector3 C { get; }
    }

    public partial struct Triangle2D : IVector<Vector2>
    {
        public Vector2 A { get; }
        public Vector2 B { get; }
        public Vector2 C { get; }
    }

    public partial struct Quad : IVector<Vector3>
    {
        public Vector3 A { get; }
        public Vector3 B { get; }
        public Vector3 C { get; }
        public Vector3 D { get; }
    }

    public partial struct Line : IVector<Vector3>
    {
        public Vector3 A { get; }
        public Vector3 B { get; }
    }

    public partial struct Line2D : IVector<Vector2>
    {
        public Vector2 A { get; }
        public Vector2 B { get; }
    }

    public partial struct Color : IValue
    {
        public float R { get; }
        public float G { get; }
        public float B { get; }
        public float A { get; }
    }

    public partial struct ColorHSV : IValue
    {
        public float H { get; }
        public float S { get; }
        public float V { get; }
    }

    public partial struct ColorHSL : IValue
    {
        public float Hue { get; }
        public float Saturation { get; }
        public float Luminance { get; }
    }

    public partial struct ColorYCbCr : IValue
    {
        public float Y { get; }
        public float Cb { get; }
        public float Cr { get; }
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

    public partial struct AxisAngle : IValue
    {
        public DVector3 Axis { get; }
        public Angle Angle { get; }
    }

    public partial struct EulerAngles : IValue
    {
        public Angle Yaw { get; }
        public Angle Pitch { get; }
        public Angle Roll { get; }
    }

    public partial struct Circle : IValue
    {
        public DVector2 Position { get; }
        public double Radius { get; }
    }

    public partial struct Size : IValue
    {
        public double Width { get; }
        public double Height { get; }
    }

    public partial struct Rectangle : IValue
    {
        public DVector2 TopLeft { get; }
        public Size Size { get; } 
    }


    public partial struct Angle : INumber
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

    public partial struct Distance : INumber
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

    public partial struct Mass : INumber
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

    public partial struct Duration : INumber
    {
        public double Value { get; }

        public static Duration FromNanoseconds(double value) => value / 1000 / 1000 / 10000;
        public static Duration FromMicroseconds(double value) => value / 1000 / 1000;
        public static Duration FromMilliseconds(double value) => value / 1000;
        public static Duration FromSeconds(double value) => value;
        public static Duration FromMinutes(double value) => value * 60;
        public static Duration FromHours(double value) => value * 60 * 60;
        public static Duration FromDays(double value) => value * 60 * 60 * 24;
        public static Duration FromWeeks(double value) => value * 60 * 60 * 24 * 7;


        public double ToNanosecond() => Value * 1000 * 1000 * 1000;
        public double ToMicroeconds() => Value * 1000 * 1000;
        public double ToMilliseconds() => Value * 1000;
        public double ToSeconds() => Value;
        public double ToMinutes() => Value / 60;
        public double ToHours() => Value / (60 * 60);
        public double ToDays() => Value / (60 * 60 * 24);
        public double ToWeeks() => Value / (60 * 60 * 24 * 7);

        public static implicit operator double(Duration duration) => duration.Value;
        public static implicit operator Duration(double value) => new Duration(value);
    }

    // Temperature: C, K, F
    // Area: Square Distance 
    // Volume: Cube Distance 
    
    public static partial class Intrinsics
    {
        public static double ToDouble(this double self) => self;
        public static double ToDouble(this float self) => self;
        public static double ToDouble(this int self) => self;
        public static double ToDouble(this uint self) => self;
        public static double ToDouble(this short self) => self;
        public static double ToDouble(this ushort self) => self;
        public static double ToDouble(this byte self) => self;
        public static double ToDouble(this sbyte self) => self;
    }

    public static partial class Intrinsics
    {
        public static Angle ToDegrees(this double self) => Angle.FromDegrees(self);
        public static Angle ToRevolutions(this double self) => Angle.FromRevolutions(self);
        public static Angle ToRadians(this double self) => self;
    }

    public static class Tests
    {
        public static void ExampleAngle()
        {
            var halfCircleDegrees = 180.0.ToDegrees();
            var halfCircleRevolutions = 0.5.ToRevolutions();
            Debug.Assert(halfCircleDegrees == halfCircleRevolutions);
        }
    }
}