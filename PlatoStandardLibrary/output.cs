class Interval
{
    var Min
    var Max
}
class Vector
{
}
class Measure
{
    var Value
}
class Numerical
{
    public static void FromNumber(var x)
    // ParameterSymbol=x$944: = Argument:RefSymbol=FromNumber$12:(1/2), Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for x: 
    // Numerical.FromNumber
    // Candidate types for x: 
    // Value.FieldValues
    {
        return FromNumber(FieldValues(x), x);
    }

}
class Magnitude
{
    public static void Magnitude(var x)
    // ParameterSymbol=x$946: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for x: 
    // Value.FieldValues
    {
        return SquareRoot(Sum(Square(FieldValues(x))));
    }

}
class Comparable
{
    public static void Compare(var a, var b)
    // ParameterSymbol=a$948: = Argument:RefSymbol=Magnitude$15:(0/1), Argument:RefSymbol=Magnitude$15:(0/1)
    // Candidate types for a: 
    // Magnitude.Magnitude
    // Candidate types for a: 
    // Magnitude.Magnitude
    // ParameterSymbol=b$949: = Argument:RefSymbol=Magnitude$15:(0/1), Argument:RefSymbol=Magnitude$15:(0/1)
    // Candidate types for b: 
    // Magnitude.Magnitude
    // Candidate types for b: 
    // Magnitude.Magnitude
    {
        return LessThan(Magnitude(a), Magnitude(b)
            ? Negative(1)
            : GreaterThan(Magnitude(a), Magnitude(b)
                ? 1
                : 0
            )
        );
    }

}
class Equatable
{
    public static void Equals(var a, var b)
    // ParameterSymbol=a$951: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for a: 
    // Value.FieldValues
    // ParameterSymbol=b$952: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for b: 
    // Value.FieldValues
    {
        return All(Equals(FieldValues(a), FieldValues(b)));
    }

}
class Arithmetic
{
    public static void Add(var self, var other)
    // ParameterSymbol=self$954: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for self: 
    // Value.FieldValues
    // ParameterSymbol=other$955: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for other: 
    // Value.FieldValues
    {
        return Add(FieldValues(self), FieldValues(other));
    }

    public static void Negative(var self)
    // ParameterSymbol=self$957: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for self: 
    // Value.FieldValues
    {
        return Negative(FieldValues(self));
    }

    public static void Reciprocal(var self)
    // ParameterSymbol=self$959: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for self: 
    // Value.FieldValues
    {
        return Reciprocal(FieldValues(self));
    }

    public static void Multiply(var self, var other)
    // ParameterSymbol=self$961: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for self: 
    // Value.FieldValues
    // ParameterSymbol=other$962: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for other: 
    // Value.FieldValues
    {
        return Add(FieldValues(self), FieldValues(other));
    }

    public static void Divide(var self, var other)
    // ParameterSymbol=self$964: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for self: 
    // Value.FieldValues
    // ParameterSymbol=other$965: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for other: 
    // Value.FieldValues
    {
        return Divide(FieldValues(self), FieldValues(other));
    }

    public static void Modulo(var self, var other)
    // ParameterSymbol=self$967: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for self: 
    // Value.FieldValues
    // ParameterSymbol=other$968: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for other: 
    // Value.FieldValues
    {
        return Modulo(FieldValues(self), FieldValues(other));
    }

}
class ScalarArithmetic
{
    public static void Add(var self, var scalar)
    // ParameterSymbol=self$970: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for self: 
    // Value.FieldValues
    // ParameterSymbol=scalar$971: = Argument:RefSymbol=Add$37:(1/2)
    // Candidate types for scalar: 
    // Arithmetic.Add | ScalarArithmetic.Add
    {
        return Add(FieldValues(self), scalar);
    }

    public static void Subtract(var self, var scalar)
    // ParameterSymbol=self$973: = Argument:RefSymbol=Add$37:(0/2)
    // Candidate types for self: 
    // Arithmetic.Add | ScalarArithmetic.Add
    // ParameterSymbol=scalar$974: = Argument:RefSymbol=Negative$26:(0/1)
    // Candidate types for scalar: 
    // Arithmetic.Negative
    {
        return Add(self, Negative(scalar));
    }

    public static void Multiply(var self, var scalar)
    // ParameterSymbol=self$976: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for self: 
    // Value.FieldValues
    // ParameterSymbol=scalar$977: = Argument:RefSymbol=Multiply$41:(1/2)
    // Candidate types for scalar: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    {
        return Multiply(FieldValues(self), scalar);
    }

    public static void Divide(var self, var scalar)
    // ParameterSymbol=self$979: = Argument:RefSymbol=Multiply$41:(0/2)
    // Candidate types for self: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    // ParameterSymbol=scalar$980: = Argument:RefSymbol=Reciprocal$28:(0/1)
    // Candidate types for scalar: 
    // Arithmetic.Reciprocal
    {
        return Multiply(self, Reciprocal(scalar));
    }

    public static void Modulo(var self, var scalar)
    // ParameterSymbol=self$982: = Argument:RefSymbol=FieldValues$54:(0/1)
    // Candidate types for self: 
    // Value.FieldValues
    // ParameterSymbol=scalar$983: = Argument:RefSymbol=Modulo$45:(1/2)
    // Candidate types for scalar: 
    // Arithmetic.Modulo | ScalarArithmetic.Modulo
    {
        return Modulo(FieldValues(self), scalar);
    }

}
class Value
{
    public static void Type()
    {
        return intrinsic;
    }

    public static void FieldTypes()
    {
        return intrinsic;
    }

    public static void FieldNames()
    {
        return intrinsic;
    }

    public static void FieldValues(var self)
    // ParameterSymbol=self$988: = 
    {
        return intrinsic;
    }

    public static void Zero()
    {
        return Zero(FieldTypes);
    }

    public static void One()
    {
        return One(FieldTypes);
    }

    public static void Default()
    {
        return Default(FieldTypes);
    }

    public static void MinValue()
    {
        return MinValue(FieldTypes);
    }

    public static void MaxValue()
    {
        return MaxValue(FieldTypes);
    }

    public static void ToString(var x)
    // ParameterSymbol=x$995: = 
    {
        return JoinStrings(FieldValues, ,);
    }

}
class Array
{
    public static void At(var n)
    // ParameterSymbol=n$997: = 
    null
    var Count
}
class Integer
{
    Integer Value
}
class Count
{
    Integer Value
}
class Index
{
    Integer Value
}
class Number
{
    var Value
}
class Unit
{
    Number Value
}
class Percent
{
    Number Value
}
class Quaternion
{
    Number X
    Number Y
    Number Z
    Number W
}
class Unit2D
{
    Unit X
    Unit Y
}
class Unit3D
{
    Unit X
    Unit Y
    Unit Z
}
class Direction3D
{
    Unit3D Value
}
class AxisAngle
{
    Unit3D Axis
    var Angle
}
class EulerAngles
{
    unresolved Yaw
    unresolved Pitch
    unresolved Roll
}
class Rotation3D
{
    Quaternion Quaternion
}
class Vector2D
{
    Number X
    Number Y
}
class Vector3D
{
    Number X
    Number Y
    Number Z
}
class Vector4D
{
    Number X
    Number Y
    Number Z
    Number W
}
class Orientation3D
{
    Rotation3D Value
}
class Pose2D
{
    Vector3D Position
    Orientation3D Orientation
}
class Pose3D
{
    Vector3D Position
    Orientation3D Orientation
}
class Transform3D
{
    Vector3D Translation
    Rotation3D Rotation
    Vector3D Scale
}
class Transform2D
{
    Vector2D Translation
    unresolved Rotation
    Vector2D Scale
}
class AlignedBox2D
{
    Vector2D A
    Vector2D B
}
class AlignedBox3D
{
    Vector3D A
    Vector3D B
}
class Complex
{
    Number Real
    Number Imaginary
}
class Ray3D
{
    Vector3D Direction
    var Position
}
class Ray2D
{
    Vector2D Direction
    var Position
}
class Sphere
{
    var Center
    Number Radius
}
class Plane
{
    Unit3D Normal
    Number D
}
class Triangle3D
{
    var A
    var B
    var C
}
class Triangle2D
{
    var A
    var B
    var C
}
class Quad3D
{
    var A
    var B
    var C
    var D
}
class Quad2D
{
    var A
    var B
    var C
    var D
}
class Point3D
{
    Vector3D Value
}
class Point2D
{
    Vector2D Value
}
class Line3D
{
    Point3D A
    Point3D B
}
class Line2D
{
    Point2D A
    Point2D B
}
class Color
{
    Unit R
    Unit G
    Unit B
    Unit A
}
class ColorLUV
{
    Percent Lightness
    Unit U
    Unit V
}
class ColorLAB
{
    Percent Lightness
    Integer A
    Integer B
}
class ColorLCh
{
    Percent Lightness
    var ChromaHue
}
class ColorHSV
{
    unresolved Hue
    Unit S
    Unit V
}
class ColorHSL
{
    unresolved Hue
    Unit Saturation
    Unit Luminance
}
class ColorYCbCr
{
    Unit Y
    Unit Cb
    Unit Cr
}
class SphericalCoordinate
{
    Number Radius
    unresolved Azimuth
    unresolved Polar
}
class PolarCoordinate
{
    Number Radius
    unresolved Angle
}
class LogPolarCoordinate
{
    Number Rho
    unresolved Azimuth
}
class CylindricalCoordinate
{
    Number RadialDistance
    unresolved Azimuth
    Number Height
}
class HorizontalCoordinate
{
    Number Radius
    unresolved Azimuth
    Number Height
}
class GeoCoordinate
{
    unresolved Latitude
    unresolved Longitude
}
class GeoCoordinateWithAltitude
{
    GeoCoordinate Coordinate
    Number Altitude
}
class Circle
{
    Point2D Center
    Number Radius
}
class Chord
{
    Circle Circle
    var Arc
}
class Size2D
{
    Number Width
    Number Height
}
class Size3D
{
    Number Width
    Number Height
    Number Depth
}
class Rectangle2D
{
    Point2D Center
    Size2D Size
}
class Proportion
{
    Number Value
}
class Fraction
{
    Number Numerator
    Number Denominator
}
class Angle
{
    Number Radians
}
class Length
{
    Number Meters
}
class Mass
{
    Number Kilograms
}
class Temperature
{
    Number Celsius
}
class TimeSpan
{
    Number Seconds
}
class TimeRange
{
    var Min
    var Max
}
class DateTime
{
}
class AnglePair
{
    Angle Start
    Angle End
}
class Ring
{
    unresolved Circle
    Number InnerRadius
}
class Arc
{
    AnglePair Angles
    unresolved Cirlce
}
class TimeInterval
{
    TimeSpan Start
    TimeSpan End
}
class RealInterval
{
    Number A
    Number B
}
class Interval2D
{
    Vector2D A
    Vector2D B
}
class Interval3D
{
    Vector3D A
    Vector3D B
}
class Capsule
{
    Line3D Line
    Number Radius
}
class Matrix3D
{
    Vector4D Column1
    Vector4D Column2
    Vector4D Column3
    Vector4D Column4
}
class Cylinder
{
    Line3D Line
    Number Radius
}
class Cone
{
    Line3D Line
    Number Radius
}
class Tube
{
    Line3D Line
    Number InnerRadius
    Number OuterRadius
}
class ConeSegment
{
    Line3D Line
    Number Radius1
    Number Radius2
}
class Box2D
{
    Point2D Center
    Angle Rotation
    Size2D Extent
}
class Box3D
{
    Point3D Center
    Rotation3D Rotation
    Size3D Extent
}
class CubicBezierTriangle3D
{
    Point3D A
    Point3D B
    Point3D C
    Point3D A2B
    Point3D AB2
    Point3D B2C
    Point3D BC2
    Point3D AC2
    Point3D A2C
    Point3D ABC
}
class CubicBezier2D
{
    Point2D A
    Point2D B
    Point2D C
    Point2D D
}
class UV
{
    Unit U
    Unit V
}
class UVW
{
    Unit U
    Unit V
    Unit W
}
class CubicBezier3D
{
    Point3D A
    Point3D B
    Point3D C
    Point3D D
}
class QuadraticBezier2D
{
    Point2D A
    Point2D B
    Point2D C
}
class QuadraticBezier3D
{
    Point3D A
    Point3D B
    Point3D C
}
class Area
{
    Number MetersSquared
}
class Volume
{
    Number MetersCubed
}
class Velocity
{
    Number MetersPerSecond
}
class Acceleration
{
    Number MetersPerSecondSquared
}
class Force
{
    Number Newtons
}
class Pressure
{
    Number Pascals
}
class Energy
{
    Number Joules
}
class Memory
{
    Count Bytes
}
class Frequency
{
    Number Hertz
}
class Loudness
{
    Number Decibels
}
class LuminousIntensity
{
    Number Candelas
}
class ElectricPotential
{
    Number Volts
}
class ElectricCharge
{
    Number Columbs
}
class ElectricCurrent
{
    Number Amperes
}
class ElectricResistance
{
    Number Ohms
}
class Power
{
    Number Watts
}
class Density
{
    Number KilogramsPerMeterCubed
}
class NormalDistribution
{
    Number Mean
    Number StandardDeviation
}
class PoissonDistribution
{
    Number Expected
    Count Occurrences
}
class BernoulliDistribution
{
    var P
}
class Probability
{
    Number Value
}
class BinomialDistribution
{
    Count Trials
    Probability P
}
class Interval
{
    public static void Size(var x)
    // ParameterSymbol=x$999: = Argument:RefSymbol=Max$818:(0/1), Argument:RefSymbol=Min$816:(0/1)
    // Candidate types for x: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    {
        return Subtract(Max(x), Min(x));
    }

    public static void IsEmpty(var x)
    // ParameterSymbol=x$1001: = Argument:RefSymbol=Min$816:(0/1), Argument:RefSymbol=Max$818:(0/1)
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // Candidate types for x: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    {
        return GreaterThanOrEquals(Min(x), Max(x));
    }

    public static void Lerp(var x, var amount)
    // ParameterSymbol=x$1003: = Argument:RefSymbol=Min$816:(0/1), Argument:RefSymbol=Max$818:(0/1)
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // Candidate types for x: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    // ParameterSymbol=amount$1004: = Argument:RefSymbol=Subtract$39:(1/2), Argument:RefSymbol=Multiply$41:(1/2)
    // Candidate types for amount: 
    // ScalarArithmetic.Subtract
    // Candidate types for amount: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    {
        return Multiply(Min(x), Add(Subtract(1, amount), Multiply(Max(x), amount)));
    }

    public static void InverseLerp(var x, var value)
    // ParameterSymbol=x$1006: = Argument:RefSymbol=Min$816:(0/1), Argument:RefSymbol=Size$632:(0/1)
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // Candidate types for x: 
    // Rectangle2D.Size | Interval.Size
    // ParameterSymbol=value$1007: = Argument:RefSymbol=Subtract$39:(0/2)
    // Candidate types for value: 
    // ScalarArithmetic.Subtract
    {
        return Divide(Subtract(value, Min(x)), Size(x));
    }

    public static void Negate(var x)
    // ParameterSymbol=x$1009: = Argument:RefSymbol=Max$818:(0/1), Argument:RefSymbol=Min$816:(0/1)
    // Candidate types for x: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    {
        return Interval(Negative(Max(x)), Negative(Min(x)));
    }

    public static void Reverse(var x)
    // ParameterSymbol=x$1011: = Argument:RefSymbol=Max$818:(0/1), Argument:RefSymbol=Min$816:(0/1)
    // Candidate types for x: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    {
        return Interval(Max(x), Min(x));
    }

    public static void Resize(var x, var size)
    // ParameterSymbol=x$1013: = Argument:RefSymbol=Min$816:(0/1), Argument:RefSymbol=Min$816:(0/1)
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // ParameterSymbol=size$1014: = Argument:RefSymbol=Add$37:(1/2)
    // Candidate types for size: 
    // Arithmetic.Add | ScalarArithmetic.Add
    {
        return Interval(Min(x), Add(Min(x), size));
    }

    public static void Center(var x)
    // ParameterSymbol=x$1016: = Argument:RefSymbol=Lerp$636:(0/2)
    // Candidate types for x: 
    // Interval.Lerp
    {
        return Lerp(x, 0.5);
    }

    public static void Contains(var x, var value)
    // ParameterSymbol=x$1018: = Argument:RefSymbol=Min$816:(0/1), Argument:RefSymbol=Max$818:(0/1)
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // Candidate types for x: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    // ParameterSymbol=value$1019: = Argument:RefSymbol=And$825:(0/2), Argument:RefSymbol=LessThanOrEquals$810:(0/2)
    // Candidate types for value: 
    // Booleans.And
    // Candidate types for value: 
    // Comparable.LessThanOrEquals
    {
        return LessThanOrEquals(Min(x), And(value, LessThanOrEquals(value, Max(x))));
    }

    public static void Contains(var x, var other)
    // ParameterSymbol=x$1021: = Argument:RefSymbol=Min$816:(0/1)
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // ParameterSymbol=other$1022: = Argument:RefSymbol=Min$816:(0/1), Argument:RefSymbol=Max$818:(0/1)
    // Candidate types for other: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // Candidate types for other: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    {
        return LessThanOrEquals(Min(x), And(Min(other), GreaterThanOrEquals(Max, Max(other))));
    }

    public static void Overlaps(var x, var y)
    // ParameterSymbol=x$1024: = Argument:RefSymbol=Clamp$735:(0/2)
    // Candidate types for x: 
    // Interval.Clamp | Interval.Clamp | Numerical.Clamp | Numerical.Clamp | Numerical.Clamp
    // ParameterSymbol=y$1025: = Argument:RefSymbol=Clamp$735:(1/2)
    // Candidate types for y: 
    // Interval.Clamp | Interval.Clamp | Numerical.Clamp | Numerical.Clamp | Numerical.Clamp
    {
        return Not(IsEmpty(Clamp(x, y)));
    }

    public static void Split(var x, var t)
    // ParameterSymbol=x$1027: = Argument:RefSymbol=Left$658:(0/2), Argument:RefSymbol=Right$660:(0/2)
    // Candidate types for x: 
    // Interval.Left
    // Candidate types for x: 
    // Interval.Right
    // ParameterSymbol=t$1028: = Argument:RefSymbol=Left$658:(1/2), Argument:RefSymbol=Right$660:(1/2)
    // Candidate types for t: 
    // Interval.Left
    // Candidate types for t: 
    // Interval.Right
    {
        return Interval(Left(x, t), Right(x, t));
    }

    public static void Split(var x)
    // ParameterSymbol=x$1030: = Argument:RefSymbol=Split$656:(0/2)
    // Candidate types for x: 
    // Interval.Split | Interval.Split
    {
        return Split(x, 0.5);
    }

    public static void Left(var x, var t)
    // ParameterSymbol=x$1032: = Argument:RefSymbol=Lerp$636:(0/2)
    // Candidate types for x: 
    // Interval.Lerp
    // ParameterSymbol=t$1033: = Argument:RefSymbol=Lerp$636:(1/2)
    // Candidate types for t: 
    // Interval.Lerp
    {
        return Interval(Min, Lerp(x, t));
    }

    public static void Right(var x, var t)
    // ParameterSymbol=x$1035: = Argument:RefSymbol=Lerp$636:(0/2), Argument:RefSymbol=Max$818:(0/1)
    // Candidate types for x: 
    // Interval.Lerp
    // Candidate types for x: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    // ParameterSymbol=t$1036: = Argument:RefSymbol=Lerp$636:(1/2)
    // Candidate types for t: 
    // Interval.Lerp
    {
        return Interval(Lerp(x, t), Max(x));
    }

    public static void MoveTo(var x, var t)
    // ParameterSymbol=x$1038: = Argument:RefSymbol=Size$632:(0/1)
    // Candidate types for x: 
    // Rectangle2D.Size | Interval.Size
    // ParameterSymbol=t$1039: = Argument:RefSymbol=Interval$630:(0/2), Argument:RefSymbol=Add$37:(0/2)
    // Candidate types for t: 
    // Any
    // Candidate types for t: 
    // Arithmetic.Add | ScalarArithmetic.Add
    {
        return Interval(t, Add(t, Size(x)));
    }

    public static void LeftHalf(var x)
    // ParameterSymbol=x$1041: = Argument:RefSymbol=Left$658:(0/2)
    // Candidate types for x: 
    // Interval.Left
    {
        return Left(x, 0.5);
    }

    public static void RightHalf(var x)
    // ParameterSymbol=x$1043: = Argument:RefSymbol=Right$660:(0/2)
    // Candidate types for x: 
    // Interval.Right
    {
        return Right(x, 0.5);
    }

    public static void HalfSize(var x)
    // ParameterSymbol=x$1045: = Argument:RefSymbol=Size$632:(0/1)
    // Candidate types for x: 
    // Rectangle2D.Size | Interval.Size
    {
        return Half(Size(x));
    }

    public static void Recenter(var x, var c)
    // ParameterSymbol=x$1047: = Argument:RefSymbol=HalfSize$668:(0/1), Argument:RefSymbol=HalfSize$668:(0/1)
    // Candidate types for x: 
    // Interval.HalfSize
    // Candidate types for x: 
    // Interval.HalfSize
    // ParameterSymbol=c$1048: = Argument:RefSymbol=Subtract$39:(0/2), Argument:RefSymbol=Add$37:(0/2)
    // Candidate types for c: 
    // ScalarArithmetic.Subtract
    // Candidate types for c: 
    // Arithmetic.Add | ScalarArithmetic.Add
    {
        return Interval(Subtract(c, HalfSize(x)), Add(c, HalfSize(x)));
    }

    public static void Clamp(var x, var y)
    // ParameterSymbol=x$1050: = Argument:RefSymbol=Clamp$735:(0/2), Argument:RefSymbol=Clamp$735:(0/2)
    // Candidate types for x: 
    // Interval.Clamp | Interval.Clamp | Numerical.Clamp | Numerical.Clamp | Numerical.Clamp
    // Candidate types for x: 
    // Interval.Clamp | Interval.Clamp | Numerical.Clamp | Numerical.Clamp | Numerical.Clamp
    // ParameterSymbol=y$1051: = Argument:RefSymbol=Min$816:(0/1), Argument:RefSymbol=Max$818:(0/1)
    // Candidate types for y: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // Candidate types for y: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    {
        return Interval(Clamp(x, Min(y)), Clamp(x, Max(y)));
    }

    public static void Clamp(var x, var value)
    // ParameterSymbol=x$1053: = Argument:RefSymbol=Min$816:(0/1), Argument:RefSymbol=Min$816:(0/1), Argument:RefSymbol=Max$818:(0/1), Argument:RefSymbol=Max$818:(0/1)
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // Candidate types for x: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    // Candidate types for x: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    // ParameterSymbol=value$1054: = Argument:RefSymbol=LessThan$804:(0/2), Argument:RefSymbol=GreaterThan$812:(0/2)
    // Candidate types for value: 
    // Comparable.LessThan
    // Candidate types for value: 
    // Comparable.GreaterThan
    {
        return LessThan(value, Min(x)
            ? Min(x)
            : GreaterThan(value, Max(x)
                ? Max(x)
                : value
            )
        );
    }

    public static void Between(var x, var value)
    // ParameterSymbol=x$1056: = Argument:RefSymbol=Min$816:(0/1), Argument:RefSymbol=Max$818:(0/1)
    // Candidate types for x: 
    // Interval.Min | TimeRange.Min | Comparable.Min
    // Candidate types for x: 
    // Interval.Max | TimeRange.Max | Comparable.Max
    // ParameterSymbol=value$1057: = Argument:RefSymbol=GreaterThanOrEquals$814:(0/2), Argument:RefSymbol=LessThanOrEquals$810:(0/2)
    // Candidate types for value: 
    // Comparable.GreaterThanOrEquals
    // Candidate types for value: 
    // Comparable.LessThanOrEquals
    {
        return GreaterThanOrEquals(value, And(Min(x), LessThanOrEquals(value, Max(x))));
    }

    public static void Unit()
    {
        return Interval(0, 1);
    }

}
class Vector
{
    public static void Sum(var v)
    // ParameterSymbol=v$1060: = Argument:RefSymbol=Aggregate$851:(0/3)
    // Candidate types for v: 
    // Array.Aggregate
    {
        return Aggregate(v, 0, Add);
    }

    public static void SumSquares(var v)
    // ParameterSymbol=v$1062: = Argument:RefSymbol=Square$729:(0/1)
    // Candidate types for v: 
    // Numerical.Square
    {
        return Aggregate(Square(v), 0, Add);
    }

    public static void LengthSquared(var v)
    // ParameterSymbol=v$1064: = Argument:RefSymbol=SumSquares$683:(0/1)
    // Candidate types for v: 
    // Vector.SumSquares
    {
        return SumSquares(v);
    }

    public static void Length(var v)
    // ParameterSymbol=v$1066: = Argument:RefSymbol=LengthSquared$685:(0/1)
    // Candidate types for v: 
    // Vector.LengthSquared
    {
        return SquareRoot(LengthSquared(v));
    }

    public static void Dot(var v1, var v2)
    // ParameterSymbol=v1$1068: = Argument:RefSymbol=Multiply$41:(0/2)
    // Candidate types for v1: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    // ParameterSymbol=v2$1069: = Argument:RefSymbol=Multiply$41:(1/2)
    // Candidate types for v2: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    {
        return Sum(Multiply(v1, v2));
    }

}
class Trig
{
    public static void Cos(var x)
    // ParameterSymbol=x$1071: = 
    {
        return intrinsic;
    }

    public static void Sin(var x)
    // ParameterSymbol=x$1073: = 
    {
        return intrinsic;
    }

    public static void Tan(var x)
    // ParameterSymbol=x$1075: = 
    {
        return intrinsic;
    }

    public static void Acos(var x)
    // ParameterSymbol=x$1077: = 
    {
        return intrinsic;
    }

    public static void Asin(var x)
    // ParameterSymbol=x$1079: = 
    {
        return intrinsic;
    }

    public static void Atan(var x)
    // ParameterSymbol=x$1081: = 
    {
        return intrinsic;
    }

    public static void Cosh(var x)
    // ParameterSymbol=x$1083: = 
    {
        return intrinsic;
    }

    public static void Sinh(var x)
    // ParameterSymbol=x$1085: = 
    {
        return intrinsic;
    }

    public static void Tanh(var x)
    // ParameterSymbol=x$1087: = 
    {
        return intrinsic;
    }

    public static void Acosh(var x)
    // ParameterSymbol=x$1089: = 
    {
        return intrinsic;
    }

    public static void Asinh(var x)
    // ParameterSymbol=x$1091: = 
    {
        return intrinsic;
    }

    public static void Atanh(var x)
    // ParameterSymbol=x$1093: = 
    {
        return intrinsic;
    }

}
class Numerical
{
    public static void Pow(var x, var y)
    // ParameterSymbol=x$1095: = 
    // ParameterSymbol=y$1096: = 
    {
        return intrinsic;
    }

    public static void Log(var x, var y)
    // ParameterSymbol=x$1098: = 
    // ParameterSymbol=y$1099: = 
    {
        return intrinsic;
    }

    public static void NaturalLog(var x)
    // ParameterSymbol=x$1101: = 
    {
        return intrinsic;
    }

    public static void NaturalPower(var x)
    // ParameterSymbol=x$1103: = 
    {
        return intrinsic;
    }

    public static void SquareRoot(var x)
    // ParameterSymbol=x$1105: = Argument:RefSymbol=Pow$717:(0/2)
    // Candidate types for x: 
    // Numerical.Pow
    {
        return Pow(x, 0.5);
    }

    public static void CubeRoot(var x)
    // ParameterSymbol=x$1107: = Argument:RefSymbol=Pow$717:(0/2)
    // Candidate types for x: 
    // Numerical.Pow
    {
        return Pow(x, 0.5);
    }

    public static void Square(var x)
    // ParameterSymbol=x$1109: = 
    {
        return Multiply(Value, Value);
    }

    public static void Clamp(var x, var min, var max)
    // ParameterSymbol=x$1111: = Argument:RefSymbol=Clamp$735:(0/2)
    // Candidate types for x: 
    // Interval.Clamp | Interval.Clamp | Numerical.Clamp | Numerical.Clamp | Numerical.Clamp
    // ParameterSymbol=min$1112: = Argument:RefSymbol=Interval$630:(0/2)
    // Candidate types for min: 
    // Any
    // ParameterSymbol=max$1113: = Argument:RefSymbol=Interval$630:(1/2)
    // Candidate types for max: 
    // Any
    {
        return Clamp(x, Interval(min, max));
    }

    public static void Clamp(var x, var i)
    // ParameterSymbol=x$1115: = Argument:RefSymbol=Clamp$735:(1/2)
    // Candidate types for x: 
    // Interval.Clamp | Interval.Clamp | Numerical.Clamp | Numerical.Clamp | Numerical.Clamp
    // ParameterSymbol=i$1116: = Argument:RefSymbol=Clamp$735:(0/2)
    // Candidate types for i: 
    // Interval.Clamp | Interval.Clamp | Numerical.Clamp | Numerical.Clamp | Numerical.Clamp
    {
        return Clamp(i, x);
    }

    public static void Clamp(var x)
    // ParameterSymbol=x$1118: = Argument:RefSymbol=Clamp$735:(0/3)
    // Candidate types for x: 
    // Interval.Clamp | Interval.Clamp | Numerical.Clamp | Numerical.Clamp | Numerical.Clamp
    {
        return Clamp(x, 0, 1);
    }

    public static void PlusOne(var x)
    // ParameterSymbol=x$1120: = Argument:RefSymbol=Add$37:(0/2)
    // Candidate types for x: 
    // Arithmetic.Add | ScalarArithmetic.Add
    {
        return Add(x, 1);
    }

    public static void MinusOne(var x)
    // ParameterSymbol=x$1122: = Argument:RefSymbol=Subtract$39:(0/2)
    // Candidate types for x: 
    // ScalarArithmetic.Subtract
    {
        return Subtract(x, 1);
    }

    public static void FromOne(var x)
    // ParameterSymbol=x$1124: = Argument:RefSymbol=Subtract$39:(1/2)
    // Candidate types for x: 
    // ScalarArithmetic.Subtract
    {
        return Subtract(1, x);
    }

    public static void Sign(var x)
    // ParameterSymbol=x$1126: = Argument:RefSymbol=LessThan$804:(0/2), Argument:RefSymbol=GreaterThan$812:(0/2)
    // Candidate types for x: 
    // Comparable.LessThan
    // Candidate types for x: 
    // Comparable.GreaterThan
    {
        return LessThan(x, 0
            ? Negative(1)
            : GreaterThan(x, 0
                ? 1
                : 0
            )
        );
    }

    public static void Abs(var x)
    // ParameterSymbol=x$1128: = 
    {
        return LessThan(Value, 0
            ? Negative(Value)
            : Value
        );
    }

    public static void Half(var x)
    // ParameterSymbol=x$1130: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 2);
    }

    public static void Third(var x)
    // ParameterSymbol=x$1132: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 3);
    }

    public static void Quarter(var x)
    // ParameterSymbol=x$1134: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 4);
    }

    public static void Fifth(var x)
    // ParameterSymbol=x$1136: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 5);
    }

    public static void Sixth(var x)
    // ParameterSymbol=x$1138: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 6);
    }

    public static void Seventh(var x)
    // ParameterSymbol=x$1140: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 7);
    }

    public static void Eighth(var x)
    // ParameterSymbol=x$1142: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 8);
    }

    public static void Ninth(var x)
    // ParameterSymbol=x$1144: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 9);
    }

    public static void Tenth(var x)
    // ParameterSymbol=x$1146: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 10);
    }

    public static void Sixteenth(var x)
    // ParameterSymbol=x$1148: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 16);
    }

    public static void Hundredth(var x)
    // ParameterSymbol=x$1150: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 100);
    }

    public static void Thousandth(var x)
    // ParameterSymbol=x$1152: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, 1000);
    }

    public static void Millionth(var x)
    // ParameterSymbol=x$1154: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, Divide(1000, 1000));
    }

    public static void Billionth(var x)
    // ParameterSymbol=x$1156: = Argument:RefSymbol=Divide$43:(0/2)
    // Candidate types for x: 
    // Arithmetic.Divide | ScalarArithmetic.Divide
    {
        return Divide(x, Divide(1000, Divide(1000, 1000)));
    }

    public static void Hundred(var x)
    // ParameterSymbol=x$1158: = Argument:RefSymbol=Multiply$41:(0/2)
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    {
        return Multiply(x, 100);
    }

    public static void Thousand(var x)
    // ParameterSymbol=x$1160: = Argument:RefSymbol=Multiply$41:(0/2)
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    {
        return Multiply(x, 1000);
    }

    public static void Million(var x)
    // ParameterSymbol=x$1162: = Argument:RefSymbol=Multiply$41:(0/2)
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    {
        return Multiply(x, Multiply(1000, 1000));
    }

    public static void Billion(var x)
    // ParameterSymbol=x$1164: = Argument:RefSymbol=Multiply$41:(0/2)
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    {
        return Multiply(x, Multiply(1000, Multiply(1000, 1000)));
    }

    public static void Twice(var x)
    // ParameterSymbol=x$1166: = Argument:RefSymbol=Multiply$41:(0/2)
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    {
        return Multiply(x, 2);
    }

    public static void Thrice(var x)
    // ParameterSymbol=x$1168: = Argument:RefSymbol=Multiply$41:(0/2)
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    {
        return Multiply(x, 3);
    }

    public static void SmoothStep(var x)
    // ParameterSymbol=x$1170: = Argument:RefSymbol=Square$729:(0/1), Argument:RefSymbol=Twice$783:(0/1)
    // Candidate types for x: 
    // Numerical.Square
    // Candidate types for x: 
    // Numerical.Twice
    {
        return Multiply(Square(x), Subtract(3, Twice(x)));
    }

    public static void Pow2(var x)
    // ParameterSymbol=x$1172: = Argument:RefSymbol=Multiply$41:(0/2), Argument:RefSymbol=Multiply$41:(1/2)
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    {
        return Multiply(x, x);
    }

    public static void Pow3(var x)
    // ParameterSymbol=x$1174: = Argument:RefSymbol=Multiply$41:(1/2), Argument:RefSymbol=Pow2$789:(0/1)
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    // Candidate types for x: 
    // Numerical.Pow2
    {
        return Multiply(Pow2(x), x);
    }

    public static void Pow4(var x)
    // ParameterSymbol=x$1176: = Argument:RefSymbol=Multiply$41:(1/2), Argument:RefSymbol=Pow3$791:(0/1)
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    // Candidate types for x: 
    // Numerical.Pow3
    {
        return Multiply(Pow3(x), x);
    }

    public static void Pow5(var x)
    // ParameterSymbol=x$1178: = Argument:RefSymbol=Multiply$41:(1/2), Argument:RefSymbol=Pow4$793:(0/1)
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    // Candidate types for x: 
    // Numerical.Pow4
    {
        return Multiply(Pow4(x), x);
    }

    public static void Turns(var x)
    // ParameterSymbol=x$1180: = Argument:RefSymbol=Multiply$41:(0/2)
    // Candidate types for x: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    {
        return Multiply(x, Multiply(3.1415926535897, 2));
    }

    public static void AlmostZero(var x)
    // ParameterSymbol=x$1182: = Argument:RefSymbol=Abs$745:(0/1)
    // Candidate types for x: 
    // Numerical.Abs
    {
        return LessThan(Abs(x), 1E-08);
    }

}
class Comparable
{
    public static void Equals(var a, var b)
    // ParameterSymbol=a$1184: = Argument:RefSymbol=Compare$18:(0/2)
    // Candidate types for a: 
    // Comparable.Compare
    // ParameterSymbol=b$1185: = Argument:RefSymbol=Compare$18:(1/2)
    // Candidate types for b: 
    // Comparable.Compare
    {
        return Equals(Compare(a, b), 0);
    }

    public static void LessThan(var a, var b)
    // ParameterSymbol=a$1187: = Argument:RefSymbol=Compare$18:(0/2)
    // Candidate types for a: 
    // Comparable.Compare
    // ParameterSymbol=b$1188: = Argument:RefSymbol=Compare$18:(1/2)
    // Candidate types for b: 
    // Comparable.Compare
    {
        return LessThan(Compare(a, b), 0);
    }

    public static void Lesser(var a, var b)
    // ParameterSymbol=a$1190: = Argument:RefSymbol=LessThanOrEquals$810:(0/2)
    // Candidate types for a: 
    // Comparable.LessThanOrEquals
    // ParameterSymbol=b$1191: = Argument:RefSymbol=LessThanOrEquals$810:(1/2)
    // Candidate types for b: 
    // Comparable.LessThanOrEquals
    {
        return LessThanOrEquals(a, b)
            ? a
            : b
        ;
    }

    public static void Greater(var a, var b)
    // ParameterSymbol=a$1193: = Argument:RefSymbol=GreaterThanOrEquals$814:(0/2)
    // Candidate types for a: 
    // Comparable.GreaterThanOrEquals
    // ParameterSymbol=b$1194: = Argument:RefSymbol=GreaterThanOrEquals$814:(1/2)
    // Candidate types for b: 
    // Comparable.GreaterThanOrEquals
    {
        return GreaterThanOrEquals(a, b)
            ? a
            : b
        ;
    }

    public static void LessThanOrEquals(var a, var b)
    // ParameterSymbol=a$1196: = Argument:RefSymbol=Compare$18:(0/2)
    // Candidate types for a: 
    // Comparable.Compare
    // ParameterSymbol=b$1197: = Argument:RefSymbol=Compare$18:(1/2)
    // Candidate types for b: 
    // Comparable.Compare
    {
        return LessThanOrEquals(Compare(a, b), 0);
    }

    public static void GreaterThan(var a, var b)
    // ParameterSymbol=a$1199: = Argument:RefSymbol=Compare$18:(0/2)
    // Candidate types for a: 
    // Comparable.Compare
    // ParameterSymbol=b$1200: = Argument:RefSymbol=Compare$18:(1/2)
    // Candidate types for b: 
    // Comparable.Compare
    {
        return GreaterThan(Compare(a, b), 0);
    }

    public static void GreaterThanOrEquals(var a, var b)
    // ParameterSymbol=a$1202: = Argument:RefSymbol=Compare$18:(0/2)
    // Candidate types for a: 
    // Comparable.Compare
    // ParameterSymbol=b$1203: = Argument:RefSymbol=Compare$18:(1/2)
    // Candidate types for b: 
    // Comparable.Compare
    {
        return GreaterThanOrEquals(Compare(a, b), 0);
    }

    public static void Min(var a, var b)
    // ParameterSymbol=a$1205: = Argument:RefSymbol=LessThan$804:(0/2)
    // Candidate types for a: 
    // Comparable.LessThan
    // ParameterSymbol=b$1206: = Argument:RefSymbol=LessThan$804:(1/2)
    // Candidate types for b: 
    // Comparable.LessThan
    {
        return LessThan(a, b)
            ? a
            : b
        ;
    }

    public static void Max(var a, var b)
    // ParameterSymbol=a$1208: = Argument:RefSymbol=GreaterThan$812:(0/2)
    // Candidate types for a: 
    // Comparable.GreaterThan
    // ParameterSymbol=b$1209: = Argument:RefSymbol=GreaterThan$812:(1/2)
    // Candidate types for b: 
    // Comparable.GreaterThan
    {
        return GreaterThan(a, b)
            ? a
            : b
        ;
    }

    public static void Between(var v, var a, var b)
    // ParameterSymbol=v$1211: = Argument:RefSymbol=Between$822:(0/2)
    // Candidate types for v: 
    // Interval.Between | Comparable.Between | Comparable.Between
    // ParameterSymbol=a$1212: = Argument:RefSymbol=Interval$630:(0/2)
    // Candidate types for a: 
    // Any
    // ParameterSymbol=b$1213: = Argument:RefSymbol=Interval$630:(1/2)
    // Candidate types for b: 
    // Any
    {
        return Between(v, Interval(a, b));
    }

    public static void Between(var v, var i)
    // ParameterSymbol=v$1215: = Argument:RefSymbol=Contains$650:(1/2)
    // Candidate types for v: 
    // Interval.Contains | Interval.Contains
    // ParameterSymbol=i$1216: = Argument:RefSymbol=Contains$650:(0/2)
    // Candidate types for i: 
    // Interval.Contains | Interval.Contains
    {
        return Contains(i, v);
    }

}
class Booleans
{
    public static void And(var a, var b)
    // ParameterSymbol=a$1218: = 
    // ParameterSymbol=b$1219: = 
    {
        return intrinsic;
    }

    public static void Or(var a, var b)
    // ParameterSymbol=a$1221: = 
    // ParameterSymbol=b$1222: = 
    {
        return intrinsic;
    }

    public static void Not(var a)
    // ParameterSymbol=a$1224: = 
    {
        return intrinsic;
    }

    public static void XOr(var a, var b)
    // ParameterSymbol=a$1226: = 
    // ParameterSymbol=b$1227: = Argument:RefSymbol=Not$829:(0/1)
    // Candidate types for b: 
    // Booleans.Not
    {
        return a
            ? Not(b)
            : b
        ;
    }

    public static void NAnd(var a, var b)
    // ParameterSymbol=a$1229: = Argument:RefSymbol=And$825:(0/2)
    // Candidate types for a: 
    // Booleans.And
    // ParameterSymbol=b$1230: = Argument:RefSymbol=And$825:(1/2)
    // Candidate types for b: 
    // Booleans.And
    {
        return Not(And(a, b));
    }

    public static void NOr(var a, var b)
    // ParameterSymbol=a$1232: = Argument:RefSymbol=Or$827:(0/2)
    // Candidate types for a: 
    // Booleans.Or
    // ParameterSymbol=b$1233: = Argument:RefSymbol=Or$827:(1/2)
    // Candidate types for b: 
    // Booleans.Or
    {
        return Not(Or(a, b));
    }

}
class Equatable
{
    public static void NotEquals(var x)
    // ParameterSymbol=x$1235: = Argument:RefSymbol=Equals$802:(0/1)
    // Candidate types for x: 
    // Equatable.Equals | Comparable.Equals
    {
        return Not(Equals(x));
    }

}
class Array
{
    public static void Map(var xs, var f)
    // ParameterSymbol=xs$1237: = Argument:RefSymbol=Count$75:(0/1), Argument:RefSymbol=At$71:(0/2)
    // Candidate types for xs: 
    // Array.Count
    // Candidate types for xs: 
    // Array.At
    // ParameterSymbol=f$1238: = Invoked:(ArgumentSymbol)
    {
        return Map(Count(xs), (i) => 
        // ParameterSymbol=i$1239: = Argument:RefSymbol=At$71:(1/2)
        // Candidate types for i: 
        // Array.At
        f(At(xs, i)));
    }

    public static void Map(var n, var f)
    // ParameterSymbol=n$1252: = Argument:RefSymbol=Array$839:(0/2)
    // Candidate types for n: 
    // Any
    // ParameterSymbol=f$1253: = Argument:RefSymbol=Array$839:(1/2)
    // Candidate types for f: 
    // Any
    {
        return Array(n, f);
    }

    public static void Zip(var xs, var ys, var f)
    // ParameterSymbol=xs$1255: = Argument:RefSymbol=Count$75:(0/1)
    // Candidate types for xs: 
    // Array.Count
    // ParameterSymbol=ys$1256: = Argument:RefSymbol=At$71:(0/2)
    // Candidate types for ys: 
    // Array.At
    // ParameterSymbol=f$1257: = Invoked:(ArgumentSymbol,ArgumentSymbol)
    {
        return Array(Count(xs), (i) => 
        // ParameterSymbol=i$1258: = Argument:RefSymbol=At$71:(0/1), Argument:RefSymbol=At$71:(1/2)
        // Candidate types for i: 
        // Array.At
        // Candidate types for i: 
        // Array.At
        f(At(i), At(ys, i)));
    }

    public static void Skip(var xs, var n)
    // ParameterSymbol=xs$1261: = 
    // ParameterSymbol=n$1262: = Argument:RefSymbol=Subtract$39:(1/2), Argument:RefSymbol=Subtract$39:(1/2)
    // Candidate types for n: 
    // ScalarArithmetic.Subtract
    // Candidate types for n: 
    // ScalarArithmetic.Subtract
    {
        return Array(Subtract(Count, n), (i) => 
        // ParameterSymbol=i$1263: = Argument:RefSymbol=Subtract$39:(0/2)
        // Candidate types for i: 
        // ScalarArithmetic.Subtract
        At(Subtract(i, n)));
    }

    public static void Take(var xs, var n)
    // ParameterSymbol=xs$1266: = 
    // ParameterSymbol=n$1267: = Argument:RefSymbol=Array$839:(0/2)
    // Candidate types for n: 
    // Any
    {
        return Array(n, (i) => 
        // ParameterSymbol=i$1268: = 
        At);
    }

    public static void Aggregate(var xs, var init, var f)
    // ParameterSymbol=xs$1271: = Argument:RefSymbol=IsEmpty$855:(0/1), Argument:RefSymbol=Rest$853:(0/1)
    // Candidate types for xs: 
    // Interval.IsEmpty | Array.IsEmpty
    // Candidate types for xs: 
    // Array.Rest
    // ParameterSymbol=init$1272: = Argument:RefSymbol=f$1273:(0/2)
    // Candidate types for init: 
    // Any
    // ParameterSymbol=f$1273: = Invoked:(ArgumentSymbol,ArgumentSymbol), Invoked:(ArgumentSymbol)
    {
        return IsEmpty(xs)
            ? init
            : f(init, f(Rest(xs)))
        ;
    }

    public static void Rest(var xs)
    // ParameterSymbol=xs$1275: = 
    {
        return Skip(1);
    }

    public static void IsEmpty(var xs)
    // ParameterSymbol=xs$1277: = Argument:RefSymbol=Count$75:(0/1)
    // Candidate types for xs: 
    // Array.Count
    {
        return Equals(Count(xs), 0);
    }

    public static void First(var xs)
    // ParameterSymbol=xs$1279: = Argument:RefSymbol=At$71:(0/2)
    // Candidate types for xs: 
    // Array.At
    {
        return At(xs, 0);
    }

    public static void Last(var xs)
    // ParameterSymbol=xs$1281: = Argument:RefSymbol=At$71:(0/2), Argument:RefSymbol=Count$75:(0/1)
    // Candidate types for xs: 
    // Array.At
    // Candidate types for xs: 
    // Array.Count
    {
        return At(xs, Subtract(Count(xs), 1));
    }

    public static void Slice(var xs, var from, var count)
    // ParameterSymbol=xs$1283: = Argument:RefSymbol=Skip$847:(0/2)
    // Candidate types for xs: 
    // Array.Skip
    // ParameterSymbol=from$1284: = Argument:RefSymbol=Skip$847:(1/2)
    // Candidate types for from: 
    // Array.Skip
    // ParameterSymbol=count$1285: = Argument:RefSymbol=Take$849:(1/2)
    // Candidate types for count: 
    // Array.Take
    {
        return Take(Skip(xs, from), count);
    }

    public static void Join(var xs, var sep)
    // ParameterSymbol=xs$1287: = Argument:RefSymbol=IsEmpty$855:(0/1), Argument:RefSymbol=First$857:(0/1), Argument:RefSymbol=Skip$847:(0/2)
    // Candidate types for xs: 
    // Interval.IsEmpty | Array.IsEmpty
    // Candidate types for xs: 
    // Array.First
    // Candidate types for xs: 
    // Array.Skip
    // ParameterSymbol=sep$1288: = Argument:RefSymbol=Interpolate$937:(1/3)
    // Candidate types for sep: 
    // Intrinsics.Interpolate
    {
        return IsEmpty(xs)
            ? 
            : Add(ToString(First(xs)), Aggregate(Skip(xs, 1), , (acc, cur) => 
            // ParameterSymbol=acc$1289: = Argument:RefSymbol=Interpolate$937:(0/3)
            // Candidate types for acc: 
            // Intrinsics.Interpolate
            // ParameterSymbol=cur$1290: = Argument:RefSymbol=Interpolate$937:(2/3)
            // Candidate types for cur: 
            // Intrinsics.Interpolate
            Interpolate(acc, sep, cur)))
        ;
    }

    public static void All(var xs, var f)
    // ParameterSymbol=xs$1293: = Argument:RefSymbol=IsEmpty$855:(0/1), Argument:RefSymbol=First$857:(0/1), Argument:RefSymbol=Rest$853:(0/1)
    // Candidate types for xs: 
    // Interval.IsEmpty | Array.IsEmpty
    // Candidate types for xs: 
    // Array.First
    // Candidate types for xs: 
    // Array.Rest
    // ParameterSymbol=f$1294: = Invoked:(ArgumentSymbol), Invoked:(ArgumentSymbol)
    {
        return IsEmpty(xs)
            ? True
            : And(f(First(xs)), f(Rest(xs)))
        ;
    }

    public static void JoinStrings(var xs, var sep)
    // ParameterSymbol=xs$1296: = Argument:RefSymbol=IsEmpty$855:(0/1), Argument:RefSymbol=First$857:(0/1), Argument:RefSymbol=Rest$853:(0/1)
    // Candidate types for xs: 
    // Interval.IsEmpty | Array.IsEmpty
    // Candidate types for xs: 
    // Array.First
    // Candidate types for xs: 
    // Array.Rest
    // ParameterSymbol=sep$1297: = 
    {
        return IsEmpty(xs)
            ? 
            : Add(First(xs), Aggregate(Rest(xs), , (x, acc) => 
            // ParameterSymbol=x$1298: = Argument:RefSymbol=ToString$66:(0/1)
            // Candidate types for x: 
            // Value.ToString
            // ParameterSymbol=acc$1299: = Argument:RefSymbol=Add$37:(0/2)
            // Candidate types for acc: 
            // Arithmetic.Add | ScalarArithmetic.Add
            Add(acc, Add(, , ToString(x)))))
        ;
    }

}
class Easings
{
    public static void BlendEaseFunc(var p, var easeIn, var easeOut)
    // ParameterSymbol=p$1302: = Argument:RefSymbol=LessThan$804:(0/2), Argument:RefSymbol=Multiply$41:(0/2), Argument:RefSymbol=Multiply$41:(0/2)
    // Candidate types for p: 
    // Comparable.LessThan
    // Candidate types for p: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    // Candidate types for p: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    // ParameterSymbol=easeIn$1303: = Invoked:(ArgumentSymbol)
    // ParameterSymbol=easeOut$1304: = Invoked:(ArgumentSymbol)
    {
        return LessThan(p, 0.5
            ? Multiply(0.5, easeIn(Multiply(p, 2)))
            : Multiply(0.5, Add(easeOut(Multiply(p, Subtract(2, 1))), 0.5))
        );
    }

    public static void InvertEaseFunc(var p, var easeIn)
    // ParameterSymbol=p$1306: = Argument:RefSymbol=Subtract$39:(1/2)
    // Candidate types for p: 
    // ScalarArithmetic.Subtract
    // ParameterSymbol=easeIn$1307: = Invoked:(ArgumentSymbol)
    {
        return Subtract(1, easeIn(Subtract(1, p)));
    }

    public static void Linear(var p)
    // ParameterSymbol=p$1309: = 
    {
        return p;
    }

    public static void QuadraticEaseIn(var p)
    // ParameterSymbol=p$1311: = Argument:RefSymbol=Pow2$789:(0/1)
    // Candidate types for p: 
    // Numerical.Pow2
    {
        return Pow2(p);
    }

    public static void QuadraticEaseOut(var p)
    // ParameterSymbol=p$1313: = Argument:RefSymbol=InvertEaseFunc$872:(0/2)
    // Candidate types for p: 
    // Easings.InvertEaseFunc
    {
        return InvertEaseFunc(p, QuadraticEaseIn);
    }

    public static void QuadraticEaseInOut(var p)
    // ParameterSymbol=p$1315: = Argument:RefSymbol=BlendEaseFunc$870:(0/3)
    // Candidate types for p: 
    // Easings.BlendEaseFunc
    {
        return BlendEaseFunc(p, QuadraticEaseIn, QuadraticEaseOut);
    }

    public static void CubicEaseIn(var p)
    // ParameterSymbol=p$1317: = Argument:RefSymbol=Pow3$791:(0/1)
    // Candidate types for p: 
    // Numerical.Pow3
    {
        return Pow3(p);
    }

    public static void CubicEaseOut(var p)
    // ParameterSymbol=p$1319: = Argument:RefSymbol=InvertEaseFunc$872:(0/2)
    // Candidate types for p: 
    // Easings.InvertEaseFunc
    {
        return InvertEaseFunc(p, CubicEaseIn);
    }

    public static void CubicEaseInOut(var p)
    // ParameterSymbol=p$1321: = Argument:RefSymbol=BlendEaseFunc$870:(0/3)
    // Candidate types for p: 
    // Easings.BlendEaseFunc
    {
        return BlendEaseFunc(p, CubicEaseIn, CubicEaseOut);
    }

    public static void QuarticEaseIn(var p)
    // ParameterSymbol=p$1323: = Argument:RefSymbol=Pow4$793:(0/1)
    // Candidate types for p: 
    // Numerical.Pow4
    {
        return Pow4(p);
    }

    public static void QuarticEaseOut(var p)
    // ParameterSymbol=p$1325: = Argument:RefSymbol=InvertEaseFunc$872:(0/2)
    // Candidate types for p: 
    // Easings.InvertEaseFunc
    {
        return InvertEaseFunc(p, QuarticEaseIn);
    }

    public static void QuarticEaseInOut(var p)
    // ParameterSymbol=p$1327: = Argument:RefSymbol=BlendEaseFunc$870:(0/3)
    // Candidate types for p: 
    // Easings.BlendEaseFunc
    {
        return BlendEaseFunc(p, QuarticEaseIn, QuarticEaseOut);
    }

    public static void QuinticEaseIn(var p)
    // ParameterSymbol=p$1329: = Argument:RefSymbol=Pow5$795:(0/1)
    // Candidate types for p: 
    // Numerical.Pow5
    {
        return Pow5(p);
    }

    public static void QuinticEaseOut(var p)
    // ParameterSymbol=p$1331: = Argument:RefSymbol=InvertEaseFunc$872:(0/2)
    // Candidate types for p: 
    // Easings.InvertEaseFunc
    {
        return InvertEaseFunc(p, QuinticEaseIn);
    }

    public static void QuinticEaseInOut(var p)
    // ParameterSymbol=p$1333: = Argument:RefSymbol=BlendEaseFunc$870:(0/3)
    // Candidate types for p: 
    // Easings.BlendEaseFunc
    {
        return BlendEaseFunc(p, QuinticEaseIn, QuinticEaseOut);
    }

    public static void SineEaseIn(var p)
    // ParameterSymbol=p$1335: = Argument:RefSymbol=InvertEaseFunc$872:(0/2)
    // Candidate types for p: 
    // Easings.InvertEaseFunc
    {
        return InvertEaseFunc(p, SineEaseOut);
    }

    public static void SineEaseOut(var p)
    // ParameterSymbol=p$1337: = Argument:RefSymbol=Quarter$751:(0/1)
    // Candidate types for p: 
    // Numerical.Quarter
    {
        return Sin(Turns(Quarter(p)));
    }

    public static void SineEaseInOut(var p)
    // ParameterSymbol=p$1339: = Argument:RefSymbol=BlendEaseFunc$870:(0/3)
    // Candidate types for p: 
    // Easings.BlendEaseFunc
    {
        return BlendEaseFunc(p, SineEaseIn, SineEaseOut);
    }

    public static void CircularEaseIn(var p)
    // ParameterSymbol=p$1341: = Argument:RefSymbol=Pow2$789:(0/1)
    // Candidate types for p: 
    // Numerical.Pow2
    {
        return FromOne(SquareRoot(FromOne(Pow2(p))));
    }

    public static void CircularEaseOut(var p)
    // ParameterSymbol=p$1343: = Argument:RefSymbol=InvertEaseFunc$872:(0/2)
    // Candidate types for p: 
    // Easings.InvertEaseFunc
    {
        return InvertEaseFunc(p, CircularEaseIn);
    }

    public static void CircularEaseInOut(var p)
    // ParameterSymbol=p$1345: = Argument:RefSymbol=BlendEaseFunc$870:(0/3)
    // Candidate types for p: 
    // Easings.BlendEaseFunc
    {
        return BlendEaseFunc(p, CircularEaseIn, CircularEaseOut);
    }

    public static void ExponentialEaseIn(var p)
    // ParameterSymbol=p$1347: = Argument:RefSymbol=AlmostZero$799:(0/1), Argument:RefSymbol=MinusOne$739:(0/1)
    // Candidate types for p: 
    // Numerical.AlmostZero
    // Candidate types for p: 
    // Numerical.MinusOne
    {
        return AlmostZero(p)
            ? p
            : Pow(2, Multiply(10, MinusOne(p)))
        ;
    }

    public static void ExponentialEaseOut(var p)
    // ParameterSymbol=p$1349: = Argument:RefSymbol=InvertEaseFunc$872:(0/2)
    // Candidate types for p: 
    // Easings.InvertEaseFunc
    {
        return InvertEaseFunc(p, ExponentialEaseIn);
    }

    public static void ExponentialEaseInOut(var p)
    // ParameterSymbol=p$1351: = Argument:RefSymbol=BlendEaseFunc$870:(0/3)
    // Candidate types for p: 
    // Easings.BlendEaseFunc
    {
        return BlendEaseFunc(p, ExponentialEaseIn, ExponentialEaseOut);
    }

    public static void ElasticEaseIn(var p)
    // ParameterSymbol=p$1353: = Argument:RefSymbol=Quarter$751:(0/1), Argument:RefSymbol=MinusOne$739:(0/1)
    // Candidate types for p: 
    // Numerical.Quarter
    // Candidate types for p: 
    // Numerical.MinusOne
    {
        return Multiply(13, Multiply(Turns(Quarter(p)), Sin(Radians(Pow(2, Multiply(10, MinusOne(p)))))));
    }

    public static void ElasticEaseOut(var p)
    // ParameterSymbol=p$1355: = Argument:RefSymbol=InvertEaseFunc$872:(0/2)
    // Candidate types for p: 
    // Easings.InvertEaseFunc
    {
        return InvertEaseFunc(p, ElasticEaseIn);
    }

    public static void ElasticEaseInOut(var p)
    // ParameterSymbol=p$1357: = Argument:RefSymbol=BlendEaseFunc$870:(0/3)
    // Candidate types for p: 
    // Easings.BlendEaseFunc
    {
        return BlendEaseFunc(p, ElasticEaseIn, ElasticEaseOut);
    }

    public static void BackEaseIn(var p)
    // ParameterSymbol=p$1359: = Argument:RefSymbol=Pow3$791:(0/1), Argument:RefSymbol=Multiply$41:(0/2), Argument:RefSymbol=Half$747:(0/1)
    // Candidate types for p: 
    // Numerical.Pow3
    // Candidate types for p: 
    // Arithmetic.Multiply | ScalarArithmetic.Multiply
    // Candidate types for p: 
    // Numerical.Half
    {
        return Subtract(Pow3(p), Multiply(p, Sin(Turns(Half(p)))));
    }

    public static void BackEaseOut(var p)
    // ParameterSymbol=p$1361: = Argument:RefSymbol=InvertEaseFunc$872:(0/2)
    // Candidate types for p: 
    // Easings.InvertEaseFunc
    {
        return InvertEaseFunc(p, BackEaseIn);
    }

    public static void BackEaseInOut(var p)
    // ParameterSymbol=p$1363: = Argument:RefSymbol=BlendEaseFunc$870:(0/3)
    // Candidate types for p: 
    // Easings.BlendEaseFunc
    {
        return BlendEaseFunc(p, BackEaseIn, BackEaseOut);
    }

    public static void BounceEaseIn(var p)
    // ParameterSymbol=p$1365: = Argument:RefSymbol=InvertEaseFunc$872:(0/2)
    // Candidate types for p: 
    // Easings.InvertEaseFunc
    {
        return InvertEaseFunc(p, BounceEaseOut);
    }

    public static void BounceEaseOut(var p)
    // ParameterSymbol=p$1367: = Argument:RefSymbol=LessThan$804:(0/2), Argument:RefSymbol=Pow2$789:(0/1), Argument:RefSymbol=LessThan$804:(0/2), Argument:RefSymbol=Pow2$789:(0/1), Argument:RefSymbol=Add$37:(0/2), Argument:RefSymbol=LessThan$804:(0/2), Argument:RefSymbol=Pow2$789:(0/1), Argument:RefSymbol=Add$37:(0/2), Argument:RefSymbol=Pow2$789:(0/1), Argument:RefSymbol=Add$37:(0/2)
    // Candidate types for p: 
    // Comparable.LessThan
    // Candidate types for p: 
    // Numerical.Pow2
    // Candidate types for p: 
    // Comparable.LessThan
    // Candidate types for p: 
    // Numerical.Pow2
    // Candidate types for p: 
    // Arithmetic.Add | ScalarArithmetic.Add
    // Candidate types for p: 
    // Comparable.LessThan
    // Candidate types for p: 
    // Numerical.Pow2
    // Candidate types for p: 
    // Arithmetic.Add | ScalarArithmetic.Add
    // Candidate types for p: 
    // Numerical.Pow2
    // Candidate types for p: 
    // Arithmetic.Add | ScalarArithmetic.Add
    {
        return LessThan(p, Divide(4, 11))
            ? Multiply(121, Divide(Pow2(p), 16))
            : LessThan(p, Divide(8, 11))
                ? Divide(363, Multiply(40, Subtract(Pow2(p), Divide(99, Multiply(10, Add(p, Divide(17, 5)))))))
                : LessThan(p, Divide(9, 10))
                    ? Divide(4356, Multiply(361, Subtract(Pow2(p), Divide(35442, Multiply(1805, Add(p, Divide(16061, 1805)))))))
                    : Divide(54, Multiply(5, Subtract(Pow2(p), Divide(513, Multiply(25, Add(p, Divide(268, 25)))))))


        ;
    }

    public static void BounceEaseInOut(var p)
    // ParameterSymbol=p$1369: = Argument:RefSymbol=BlendEaseFunc$870:(0/3)
    // Candidate types for p: 
    // Easings.BlendEaseFunc
    {
        return BlendEaseFunc(p, BounceEaseIn, BounceEaseOut);
    }

}
class Intrinsics
{
    public static void Interpolate(var xs)
    // ParameterSymbol=xs$1371: = 
    {
        return intrinsic;
    }

    public static void Throw(var x)
    // ParameterSymbol=x$1373: = 
    {
        return intrinsic;
    }

    public static void TypeOf(var x)
    // ParameterSymbol=x$1375: = 
    {
        return intrinsic;
    }

    public static void New(var x)
    // ParameterSymbol=x$1377: = 
    {
        return intrinsic;
    }

}
