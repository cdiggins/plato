using System;
using Plato;

public class VectorAttribute : Attribute { }
public class ValueAttribute : Attribute { }
public class MeasureAttribute : Attribute { }
public class NumberAttribute : Attribute { }
public class IntervalAttribute : Attribute { }

namespace Plato 
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

    [Vector]
    class Line 
    {
        Point A, B;
    }

    [Vector]
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
        Angle Azimuth;
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

    [Value]
    class Percent
    {
        double Value;
    }

    [Number]
    class Unit 
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
    class Distance 
    {
        double Meters;
    }

    [Measure]
    class Mass 
    {
        double Kilograms;
    }

    [Number]
    class Temperature 
    {
        double Celsius;
    }

    [Interval]
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
        Double3 Position;
        Rotation Rotation;
        Double3 Extent;
    }
}