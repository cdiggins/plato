using System;
using Plato;

public class VectorAttribute : Attribute { }
public class ValueAttribute : Attribute { }
public class MeasureAttribute : Attribute { }
public class NumberAttribute : Attribute { }
public class IntervalAttribute : Attribute { }

namespace Plato     
{
    [Number]
    class Float
    {
        float Value;
    }

    [Number]
    class Double
    {
        double Value;
    }

    [Number]
    class Int
    {
        int Value;
    }

    [Number]
    class Long
    {
        long Value;
    }

    [Number]
    class Byte
    {
        byte Value;
    }

    [Vector]
    class Float2 
    {
        Float X, Y;
    }

    [Vector]
    class Float3 
    {
        Float X, Y, Z;
    }

    [Vector]
    class Float4
    {
        Float X, Y, Z, W;
    }

    [Vector]
    class Double2 
    {
        Double X, Y;
    }

    [Vector]
    class Double3 
    {
        Double X, Y, Z;
    }

    [Vector]
    class Double4 
    {
        Double X, Y, Z, W;
    }

    [Value]
    class Quaternion 
    {
        Double X, Y, Z, W;
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
        Byte A, B, C, D;
    }

    [Value]
    class Byte3 
    {
        Byte A, B, C, D;
    }

    [Value]
    class Byte4 
    {
        Byte A, B, C, D;
    }

    [Vector]
    class Int2 
    {
        Int A, B;
    }

    [Vector]
    class Int3 
    {
        Int A, B, C;
    }

    [Vector]
    class Int4 
    {
        Int A, B, C, D;
    }

    [Vector]
    class Long2
    {
        Long A, B;
    }

    [Vector]
    class Long3
    {
        Long A, B, C;
    }

    [Vector]
    class Long4
    {
        Long A, B, C, D;
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
        Double Real, Imaginary;
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
        Double Radius;
    }

    [Value]
    class Plane 
    {
        Double3 Normal;
        Double D;
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
        Double R, G, B, A;
    }

    [Value]
    class ColorHSV 
    {
        Double H, S, V;
    }

    [Value]
    class ColorHSL
    {
        Double Hue;
        Double Saturation;
        Double Luminance;
    }

    [Value]
    class ColorYCbCr
    {
        Double Y, Cb, Cr;
    }

    [Value]
    class SphericalCoordinate
    {
        Double Radius;
        Angle Azimuth;
        Angle Inclination;
    }

    [Value]
    class PolarCoordinate
    {
        Double Radius;
        Angle Azimuth;
    }

    [Value]
    class LogPolarCoordinate
    {
        Double Rho;
        Angle Azimuth;
    }

    [Value]
    class HorizontalCoordinate
    {
        Double Radius;
        Angle Azimuth;
        Double Height;
    }

    [Value]
    class GeoCoordinate
    {
        Double Latitude, Longitude, Altitude;
    }

    [Value]
    class Circle
    {
        Point2D Center;
        Double Radius;
    }

    [Value]
    class Size
    {
        Double Width;
        Double Height;
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
        Double Value;
    }

    [Number]
    class Unit 
    {
        Double Value;
    }

    [Value]
    class Amount
    {
        Double Mole;
    }

    [Value]
    class Fraction 
    {
        Double Numerator, Denominator;
    }

    [Measure]
    class Angle
    {
        Double Radians;
    }

    [Measure]
    class Distance 
    {
        Double Meters;
    }

    [Measure]
    class Mass 
    {
        Double Kilograms;
    }

    [Number]
    class Temperature 
    {
        Double Celsius;
    }

    [Interval]
    class Time
    {
        Double Seconds;
    }

    [Interval]
    class TimeInterval 
    {
        Time Start, End;
    }

    [Interval]
    class Interval 
    {
        Double A, B;
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
        Double Radius;
    }

    [Value]
    class Cylinder 
    {
        Line Line;
        Double Radius;
    }

    [Value]
    class Cone 
    {
        Line Line;
        Double Radius;
    }

    [Value]
    class Tube 
    {
        Line Line;
        Double InnerRadius; 
        Double OuterRadius;
    }

    [Value]
    class ConeSegment 
    {
        Line Line;
        Double Radius1, Radius2;
    }

    [Value]
    class Box 
    {
        Position Center;
        Rotation Rotation;
        Double3 Extent;
    }
}