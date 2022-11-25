
namespace Plato.__TYPES__     
{
    [Vector]
    class Float2 
    {
        float X, Y;
    }

    [Vector]
    class Float3 
    {
        float X, Y, Z;
    }

    [Vector]
    class Float4
    {
        float X, Y, Z, W;
    }

    [Vector]
    class Double2 
    {
        double X, Y;
    }

    [Vector]
    class Double3 
    {
        double X, Y, Z;
    }

    [Vector]
    class Double4 
    {
        double X, Y, Z, W;
    }

    [Value]
    class Quaternion 
    {
        double X, Y, Z, W;
    }

    [Value]
    class AxisAngle 
    {
        Double3 Axis;
        Angle Angle;
    }

    [Value]
    class EulerAngles 
    {
        Angle Yaw, Pitch, Roll;
    }

    [Value]
    class Rotation 
    {
        Quaternion Quaternion;
    }

    [Value]
    class Byte2 
    {
        byte A, B, C, D;
    }

    [Value]
    class Byte3 
    {
        byte A, B, C, D;
    }

    [Value]
    class Byte4 
    {
        byte A, B, C, D;
    }

    [Vector]
    class Int2 
    {
        int A, B;
    }

    [Vector]
    class Int3 
    {
        int A, B, C;
    }

    [Vector]
    class Int4 
    {
        int A, B, C, D;
    }

    [Vector]
    class Long2
    {
        long A, B;
    }

    [Vector]
    class Long3
    {
        long A, B, C;
    }

    [Vector]
    class Long4
    {
        long A, B, C, D;
    }

    [Value]
    class Pose 
    {
        Double3 Position;
        Rotation Orientation;
    }

    [Value]
    class Transform 
    {
        Double3 Translation;
        Rotation Rotation;
        Double3 Scale;
    }

    [Interval]
    class AABBox2D 
    {
        Double2 A, B;
    }

    [Interval]
    class AABBox3D 
    {
        Double3 A, B;
    }

    [Vector]
    class Complex 
    {
        double Real, Imaginary;
    }

    [Value]
    class Ray 
    {
        Double3 Direction;
        Point Position;
    }

    [Value]
    class Ray2D
    {
        Double2 Direction;
        Point2D Position;
    }

    [Value]
    class Sphere
    {
        Point Center;
        double Radius;
    }

    [Value]
    class Plane 
    {
        Double3 Normal;
        double D;
    }

    [Value]
    class Triangle 
    {
        Double3 A, B, C;
    }

    [Value]
    class Triangle2D 
    {
        Double2 A, B, C;
    }

    [Value]
    class Quad 
    {
        Double3 A, B, C, D;
    }

    [Value]
    class Point 
    {
        Double3 Value;
    }

    [Value]
    class Point2D 
    {
        Double2 Value;
    }

    [Value]
    class Line 
    {
        Point A, B;
    }

    [Value]
    class Line2D 
    {
        Point2D A, B;
    }

    [Value]
    class Color 
    {
        double R, G, B, A;
    }

    [Value]
    class ColorHSV 
    {
        double H, S, V;
    }

    [Value]
    class ColorHSL
    {
        double Hue;
        double Saturation;
        double Luminance;
    }

    [Value]
    class ColorYCbCr
    {
        double Y, Cb, Cr;
    }

    [Value]
    class SphericalCoordinate
    {
        double Radius;
        Angle Azimuth;
        Angle Inclination;
    }

    [Value]
    class PolarCoordinate
    {
        double Radius;
        Angle Angle;
    }

    [Value]
    class LogPolarCoordinate
    {
        double Rho;
        Angle Azimuth;
    }

    [Value]
    class HorizontalCoordinate
    {
        double Radius;
        Angle Azimuth;
        double Height;
    }

    [Value]
    class GeoCoordinate
    {
        double Latitude, Longitude, Altitude;
    }

    [Value]
    class Circle
    {
        Point2D Center;
        double Radius;
    }

    [Value]
    class Size
    {
        double Width;
        double Height;
    }

    [Value]
    class Rectangle
    {
        Double2 TopLeft;
        Size Size;
    }

    [Number]
    class Proportion 
    {
        double Value;
    }

    [Value]
    class Amount
    {
        double Mole;
    }

    [Value]
    class Fraction 
    {
        double Numerator, Denominator;
    }

    [Measure]
    class Angle
    {
        double Radians;
    }

    [Measure]
    class Length
    {
        double Meters;
    }

    [Measure]
    class Mass 
    {
        double Kilograms;
    }

    [Measure]
    class Temperature 
    {
        double Celsius;
    }

    [Measure]
    class Time
    {
        double Seconds;
    }

    [Interval]
    class TimeInterval 
    {
        Time Start, End;
    }

    [Interval]
    class Interval 
    {
        double A, B;
    }

    [Interval]
    class Interval2D 
    {
        Double2 A, B;
    }

    [Interval]
    class Interval3D 
    {
        Double3 A, B;
    }

    [Value]
    class Capsule 
    {
        Line Line;
        double Radius;
    }

    [Value]
    class Cylinder 
    {
        Line Line;
        double Radius;
    }

    [Value]
    class Cone 
    {
        Line Line;
        double Radius;
    }

    [Value]
    class Tube 
    {
        Line Line;
        double InnerRadius; 
        double OuterRadius;
    }

    [Value]
    class ConeSegment 
    {
        Line Line;
        double Radius1, Radius2;
    }

    [Value]
    class Box 
    {
        Point Center;
        Rotation Rotation;
        Double3 Extent;
    }

    [Value]
    class CubicBezier2D
    {
        Point2D A;
        Point2D B;
        Point2D C;
        Point2D D;
    }

    [Value]
    class CubicBezier
    {
        Point A;
        Point B;
        Point C;
        Point D;
    }

    [Value]
    class QuadraticBezier2D
    {
        Point2D A;
        Point2D B;
        Point2D C;
    }

    [Value]
    class QuadraticBezier
    {
        Point A;
        Point B;
        Point C;
    }

    [Measure]
    class Area
    {
        double MetersSquared;
    }

    [Measure]
    class Volume
    {
        double MetersCubed;
    }

    [Measure]
    class Velocity
    {
        double MetersPerSecond;
    }

    [Measure]
    class Acceleration
    {
        double MetersPerSecondSquared;
    }

    [Measure]
    class Force
    {
        double Newtons; 
    }

    [Measure]
    class Pressure
    {
        double Pascals;
    }

    [Measure]
    class Energy
    {
        double Joules;
    }

    [Measure]
    class Memory
    {
        double Bytes;
    }

    [Measure]
    class Frequency
    {
        double Hertz;
    }

    [Measure]
    class Loudness
    {
        double Decibels;
    }

    [Measure]
    class LuminousIntensity
    {
        double Candelas;
    }

    [Measure]
    class ElectricPotential
    {
        double Volts;
    }

    [Measure]
    class ElectricCharge
    {
        double Columbs;
    }

    [Measure]
    class ElectricCurrent
    {
        double Amperes;
    }

    [Measure]
    class ElectricResistance
    {
        double Ohms;
    }

    [Measure]
    class Power
    {
        double Watts;
    }

    [Measure]
    class Density
    {
        double KilogramsPerMeterCubed;
    }
}