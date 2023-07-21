interface Interval<Self> where Self : Interval<Self>
{
    var Min
    var Max
}
interface Vector<Self> where Self : Vector<Self>
{
}
interface Measure<Self> where Self : Measure<Self>
{
    Number Value
}
interface Numerical<Self> where Self : Numerical<Self>
{
    public static void FromNumber(var x)
    // ParameterSymbol=x$944: = Argument:RefSymbol=FromNumber$137:(1/2), Argument:RefSymbol=FieldValues$173:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    {
        return FromNumber(FieldValues(x), x);
    }

}
interface Magnitude<Self> where Self : Magnitude<Self>
{
    public static Number Magnitude(var x)
    // ParameterSymbol=x$946: = Argument:RefSymbol=FieldValues$173:(0/1)
    // x Best type is dynamic
    {
        return SquareRoot(Sum(Square(FieldValues(x))));
    }

}
interface Comparable<Self> where Self : Comparable<Self>
{
    public static Integer Compare(var a, var b)
    // ParameterSymbol=a$948: = Argument:RefSymbol=Magnitude$139:(0/1), Argument:RefSymbol=Magnitude$139:(0/1)
    // a Best type is dynamic
    // a Best type is dynamic
    // ParameterSymbol=b$949: = Argument:RefSymbol=Magnitude$139:(0/1), Argument:RefSymbol=Magnitude$139:(0/1)
    // b Best type is dynamic
    // b Best type is dynamic
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
interface Equatable<Self> where Self : Equatable<Self>
{
    public static void Equals(var a, var b)
    // ParameterSymbol=a$951: = Argument:RefSymbol=FieldValues$173:(0/1)
    // a Best type is dynamic
    // ParameterSymbol=b$952: = Argument:RefSymbol=FieldValues$173:(0/1)
    // b Best type is dynamic
    {
        return All(Equals(FieldValues(a), FieldValues(b)));
    }

}
interface Arithmetic<Self> where Self : Arithmetic<Self>
{
    public static void Add(var self, var other)
    // ParameterSymbol=self$954: = Argument:RefSymbol=FieldValues$173:(0/1)
    // self Best type is dynamic
    // ParameterSymbol=other$955: = Argument:RefSymbol=FieldValues$173:(0/1)
    // other Best type is dynamic
    {
        return Add(FieldValues(self), FieldValues(other));
    }

    public static void Negative(var self)
    // ParameterSymbol=self$957: = Argument:RefSymbol=FieldValues$173:(0/1)
    // self Best type is dynamic
    {
        return Negative(FieldValues(self));
    }

    public static void Reciprocal(var self)
    // ParameterSymbol=self$959: = Argument:RefSymbol=FieldValues$173:(0/1)
    // self Best type is dynamic
    {
        return Reciprocal(FieldValues(self));
    }

    public static void Multiply(var self, var other)
    // ParameterSymbol=self$961: = Argument:RefSymbol=FieldValues$173:(0/1)
    // self Best type is dynamic
    // ParameterSymbol=other$962: = Argument:RefSymbol=FieldValues$173:(0/1)
    // other Best type is dynamic
    {
        return Add(FieldValues(self), FieldValues(other));
    }

    public static void Divide(var self, var other)
    // ParameterSymbol=self$964: = Argument:RefSymbol=FieldValues$173:(0/1)
    // self Best type is dynamic
    // ParameterSymbol=other$965: = Argument:RefSymbol=FieldValues$173:(0/1)
    // other Best type is dynamic
    {
        return Divide(FieldValues(self), FieldValues(other));
    }

    public static void Modulo(var self, var other)
    // ParameterSymbol=self$967: = Argument:RefSymbol=FieldValues$173:(0/1)
    // self Best type is dynamic
    // ParameterSymbol=other$968: = Argument:RefSymbol=FieldValues$173:(0/1)
    // other Best type is dynamic
    {
        return Modulo(FieldValues(self), FieldValues(other));
    }

}
interface ScalarArithmetic<Self> where Self : ScalarArithmetic<Self>
{
    public static void Add(var self, var scalar)
    // ParameterSymbol=self$970: = Argument:RefSymbol=FieldValues$173:(0/1)
    // self Best type is dynamic
    // ParameterSymbol=scalar$971: = Argument:RefSymbol=Add$157:(1/2)
    // scalar Best type is dynamic
    {
        return Add(FieldValues(self), scalar);
    }

    public static void Subtract(var self, var scalar)
    // ParameterSymbol=self$973: = Argument:RefSymbol=Add$157:(0/2)
    // self Best type is dynamic
    // ParameterSymbol=scalar$974: = Argument:RefSymbol=Negative$147:(0/1)
    // scalar Best type is dynamic
    {
        return Add(self, Negative(scalar));
    }

    public static void Multiply(var self, var scalar)
    // ParameterSymbol=self$976: = Argument:RefSymbol=FieldValues$173:(0/1)
    // self Best type is dynamic
    // ParameterSymbol=scalar$977: = Argument:RefSymbol=Multiply$161:(1/2)
    // scalar Best type is dynamic
    {
        return Multiply(FieldValues(self), scalar);
    }

    public static void Divide(var self, var scalar)
    // ParameterSymbol=self$979: = Argument:RefSymbol=Multiply$161:(0/2)
    // self Best type is dynamic
    // ParameterSymbol=scalar$980: = Argument:RefSymbol=Reciprocal$149:(0/1)
    // scalar Best type is dynamic
    {
        return Multiply(self, Reciprocal(scalar));
    }

    public static void Modulo(var self, var scalar)
    // ParameterSymbol=self$982: = Argument:RefSymbol=FieldValues$173:(0/1)
    // self Best type is dynamic
    // ParameterSymbol=scalar$983: = Argument:RefSymbol=Modulo$165:(1/2)
    // scalar Best type is dynamic
    {
        return Modulo(FieldValues(self), scalar);
    }

}
interface Value<Self> where Self : Value<Self>
{
    public static void Type()
    {
        return intrinsic;
    }

    public static Array FieldTypes()
    {
        return intrinsic;
    }

    public static Array FieldNames()
    {
        return intrinsic;
    }

    public static Array FieldValues(var self)
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
interface Array<Self> where Self : Array<Self>
{
    public static void At(var n)
    // ParameterSymbol=n$997: = 
    null
    Count Count
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
    Angle Angle
}
class EulerAngles
{
    Angle Yaw
    Angle Pitch
    Angle Roll
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
    Angle Rotation
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
    Point3D Position
}
class Ray2D
{
    Vector2D Direction
    Point2D Position
}
class Sphere
{
    Point3D Center
    Number Radius
}
class Plane
{
    Unit3D Normal
    Number D
}
class Triangle3D
{
    Point3D A
    Point3D B
    Point3D C
}
class Triangle2D
{
    Point2D A
    Point2D B
    Point2D C
}
class Quad3D
{
    Point3D A
    Point3D B
    Point3D C
    Point3D D
}
class Quad2D
{
    Point2D A
    Point2D B
    Point2D C
    Point2D D
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
    PolarCoordinate ChromaHue
}
class ColorHSV
{
    Angle Hue
    Unit S
    Unit V
}
class ColorHSL
{
    Angle Hue
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
    Angle Azimuth
    Angle Polar
}
class PolarCoordinate
{
    Number Radius
    Angle Angle
}
class LogPolarCoordinate
{
    Number Rho
    Angle Azimuth
}
class CylindricalCoordinate
{
    Number RadialDistance
    Angle Azimuth
    Number Height
}
class HorizontalCoordinate
{
    Number Radius
    Angle Azimuth
    Number Height
}
class GeoCoordinate
{
    Angle Latitude
    Angle Longitude
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
    Arc Arc
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
    DateTime Min
    DateTime Max
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
    Circle Circle
    Number InnerRadius
}
class Arc
{
    AnglePair Angles
    Circle Cirlce
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
    Probability P
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
    // ParameterSymbol=x$999: = Argument:RefSymbol=Max$823:(0/1), Argument:RefSymbol=Min$821:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    {
        return Subtract(Max(x), Min(x));
    }

    public static void IsEmpty(var x)
    // ParameterSymbol=x$1001: = Argument:RefSymbol=Min$821:(0/1), Argument:RefSymbol=Max$823:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    {
        return GreaterThanOrEquals(Min(x), Max(x));
    }

    public static void Lerp(var x, var amount)
    // ParameterSymbol=x$1003: = Argument:RefSymbol=Min$821:(0/1), Argument:RefSymbol=Max$823:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    // ParameterSymbol=amount$1004: = Argument:RefSymbol=Subtract$159:(1/2), Argument:RefSymbol=Multiply$161:(1/2)
    // amount Best type is dynamic
    // amount Best type is dynamic
    {
        return Multiply(Min(x), Add(Subtract(1, amount), Multiply(Max(x), amount)));
    }

    public static void InverseLerp(var x, var value)
    // ParameterSymbol=x$1006: = Argument:RefSymbol=Min$821:(0/1), Argument:RefSymbol=Size$641:(0/1)
    // x Best type is dynamic
    // x Best type is Size2D
    // ParameterSymbol=value$1007: = Argument:RefSymbol=Subtract$159:(0/2)
    // value Best type is dynamic
    {
        return Divide(Subtract(value, Min(x)), Size(x));
    }

    public static void Negate(var x)
    // ParameterSymbol=x$1009: = Argument:RefSymbol=Max$823:(0/1), Argument:RefSymbol=Min$821:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    {
        return Interval(Negative(Max(x)), Negative(Min(x)));
    }

    public static void Reverse(var x)
    // ParameterSymbol=x$1011: = Argument:RefSymbol=Max$823:(0/1), Argument:RefSymbol=Min$821:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    {
        return Interval(Max(x), Min(x));
    }

    public static void Resize(var x, var size)
    // ParameterSymbol=x$1013: = Argument:RefSymbol=Min$821:(0/1), Argument:RefSymbol=Min$821:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    // ParameterSymbol=size$1014: = Argument:RefSymbol=Add$157:(1/2)
    // size Best type is dynamic
    {
        return Interval(Min(x), Add(Min(x), size));
    }

    public static void Center(var x)
    // ParameterSymbol=x$1016: = Argument:RefSymbol=Lerp$645:(0/2)
    // x Best type is dynamic
    {
        return Lerp(x, 0.5);
    }

    public static void Contains(var x, var value)
    // ParameterSymbol=x$1018: = Argument:RefSymbol=Min$821:(0/1), Argument:RefSymbol=Max$823:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    // ParameterSymbol=value$1019: = Argument:RefSymbol=And$829:(0/2), Argument:RefSymbol=LessThanOrEquals$815:(0/2)
    // value Best type is dynamic
    // value Best type is dynamic
    {
        return LessThanOrEquals(Min(x), And(value, LessThanOrEquals(value, Max(x))));
    }

    public static void Contains(var x, var other)
    // ParameterSymbol=x$1021: = Argument:RefSymbol=Min$821:(0/1)
    // x Best type is dynamic
    // ParameterSymbol=other$1022: = Argument:RefSymbol=Min$821:(0/1), Argument:RefSymbol=Max$823:(0/1)
    // other Best type is dynamic
    // other Best type is dynamic
    {
        return LessThanOrEquals(Min(x), And(Min(other), GreaterThanOrEquals(Max, Max(other))));
    }

    public static void Overlaps(var x, var y)
    // ParameterSymbol=x$1024: = Argument:RefSymbol=Clamp$741:(0/2)
    // x Best type is dynamic
    // ParameterSymbol=y$1025: = Argument:RefSymbol=Clamp$741:(1/2)
    // y Best type is dynamic
    {
        return Not(IsEmpty(Clamp(x, y)));
    }

    public static void Split(var x, var t)
    // ParameterSymbol=x$1027: = Argument:RefSymbol=Left$667:(0/2), Argument:RefSymbol=Right$669:(0/2)
    // x Best type is dynamic
    // x Best type is dynamic
    // ParameterSymbol=t$1028: = Argument:RefSymbol=Left$667:(1/2), Argument:RefSymbol=Right$669:(1/2)
    // t Best type is dynamic
    // t Best type is dynamic
    {
        return Interval(Left(x, t), Right(x, t));
    }

    public static void Split(var x)
    // ParameterSymbol=x$1030: = Argument:RefSymbol=Split$665:(0/2)
    // x Best type is dynamic
    {
        return Split(x, 0.5);
    }

    public static void Left(var x, var t)
    // ParameterSymbol=x$1032: = Argument:RefSymbol=Lerp$645:(0/2)
    // x Best type is dynamic
    // ParameterSymbol=t$1033: = Argument:RefSymbol=Lerp$645:(1/2)
    // t Best type is dynamic
    {
        return Interval(Min, Lerp(x, t));
    }

    public static void Right(var x, var t)
    // ParameterSymbol=x$1035: = Argument:RefSymbol=Lerp$645:(0/2), Argument:RefSymbol=Max$823:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    // ParameterSymbol=t$1036: = Argument:RefSymbol=Lerp$645:(1/2)
    // t Best type is dynamic
    {
        return Interval(Lerp(x, t), Max(x));
    }

    public static void MoveTo(var x, var t)
    // ParameterSymbol=x$1038: = Argument:RefSymbol=Size$641:(0/1)
    // x Best type is Size2D
    // ParameterSymbol=t$1039: = Argument:RefSymbol=Interval$120:(0/2), Argument:RefSymbol=Add$157:(0/2)
    // t Best type is Any
    // t Best type is dynamic
    {
        return Interval(t, Add(t, Size(x)));
    }

    public static void LeftHalf(var x)
    // ParameterSymbol=x$1041: = Argument:RefSymbol=Left$667:(0/2)
    // x Best type is dynamic
    {
        return Left(x, 0.5);
    }

    public static void RightHalf(var x)
    // ParameterSymbol=x$1043: = Argument:RefSymbol=Right$669:(0/2)
    // x Best type is dynamic
    {
        return Right(x, 0.5);
    }

    public static void HalfSize(var x)
    // ParameterSymbol=x$1045: = Argument:RefSymbol=Size$641:(0/1)
    // x Best type is Size2D
    {
        return Half(Size(x));
    }

    public static void Recenter(var x, var c)
    // ParameterSymbol=x$1047: = Argument:RefSymbol=HalfSize$677:(0/1), Argument:RefSymbol=HalfSize$677:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    // ParameterSymbol=c$1048: = Argument:RefSymbol=Subtract$159:(0/2), Argument:RefSymbol=Add$157:(0/2)
    // c Best type is dynamic
    // c Best type is dynamic
    {
        return Interval(Subtract(c, HalfSize(x)), Add(c, HalfSize(x)));
    }

    public static void Clamp(var x, var y)
    // ParameterSymbol=x$1050: = Argument:RefSymbol=Clamp$741:(0/2), Argument:RefSymbol=Clamp$741:(0/2)
    // x Best type is dynamic
    // x Best type is dynamic
    // ParameterSymbol=y$1051: = Argument:RefSymbol=Min$821:(0/1), Argument:RefSymbol=Max$823:(0/1)
    // y Best type is dynamic
    // y Best type is dynamic
    {
        return Interval(Clamp(x, Min(y)), Clamp(x, Max(y)));
    }

    public static void Clamp(var x, var value)
    // ParameterSymbol=x$1053: = Argument:RefSymbol=Min$821:(0/1), Argument:RefSymbol=Min$821:(0/1), Argument:RefSymbol=Max$823:(0/1), Argument:RefSymbol=Max$823:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    // x Best type is dynamic
    // x Best type is dynamic
    // ParameterSymbol=value$1054: = Argument:RefSymbol=LessThan$809:(0/2), Argument:RefSymbol=GreaterThan$817:(0/2)
    // value Best type is dynamic
    // value Best type is dynamic
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
    // ParameterSymbol=x$1056: = Argument:RefSymbol=Min$821:(0/1), Argument:RefSymbol=Max$823:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    // ParameterSymbol=value$1057: = Argument:RefSymbol=GreaterThanOrEquals$819:(0/2), Argument:RefSymbol=LessThanOrEquals$815:(0/2)
    // value Best type is dynamic
    // value Best type is dynamic
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
    // ParameterSymbol=v$1060: = Argument:RefSymbol=Aggregate$853:(0/3)
    // v Best type is dynamic
    {
        return Aggregate(v, 0, Add);
    }

    public static void SumSquares(var v)
    // ParameterSymbol=v$1062: = Argument:RefSymbol=Square$735:(0/1)
    // v Best type is dynamic
    {
        return Aggregate(Square(v), 0, Add);
    }

    public static void LengthSquared(var v)
    // ParameterSymbol=v$1064: = Argument:RefSymbol=SumSquares$691:(0/1)
    // v Best type is dynamic
    {
        return SumSquares(v);
    }

    public static void Length(var v)
    // ParameterSymbol=v$1066: = Argument:RefSymbol=LengthSquared$693:(0/1)
    // v Best type is dynamic
    {
        return SquareRoot(LengthSquared(v));
    }

    public static void Dot(var v1, var v2)
    // ParameterSymbol=v1$1068: = Argument:RefSymbol=Multiply$161:(0/2)
    // v1 Best type is dynamic
    // ParameterSymbol=v2$1069: = Argument:RefSymbol=Multiply$161:(1/2)
    // v2 Best type is dynamic
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
    // ParameterSymbol=x$1105: = Argument:RefSymbol=Pow$723:(0/2)
    // x Best type is dynamic
    {
        return Pow(x, 0.5);
    }

    public static void CubeRoot(var x)
    // ParameterSymbol=x$1107: = Argument:RefSymbol=Pow$723:(0/2)
    // x Best type is dynamic
    {
        return Pow(x, 0.5);
    }

    public static void Square(var x)
    // ParameterSymbol=x$1109: = 
    {
        return Multiply(Value, Value);
    }

    public static void Clamp(var x, var min, var max)
    // ParameterSymbol=x$1111: = Argument:RefSymbol=Clamp$741:(0/2)
    // x Best type is dynamic
    // ParameterSymbol=min$1112: = Argument:RefSymbol=Interval$120:(0/2)
    // min Best type is Any
    // ParameterSymbol=max$1113: = Argument:RefSymbol=Interval$120:(1/2)
    // max Best type is Any
    {
        return Clamp(x, Interval(min, max));
    }

    public static void Clamp(var x, var i)
    // ParameterSymbol=x$1115: = Argument:RefSymbol=Clamp$741:(1/2)
    // x Best type is dynamic
    // ParameterSymbol=i$1116: = Argument:RefSymbol=Clamp$741:(0/2)
    // i Best type is dynamic
    {
        return Clamp(i, x);
    }

    public static void Clamp(var x)
    // ParameterSymbol=x$1118: = Argument:RefSymbol=Clamp$741:(0/3)
    // x Best type is dynamic
    {
        return Clamp(x, 0, 1);
    }

    public static void PlusOne(var x)
    // ParameterSymbol=x$1120: = Argument:RefSymbol=Add$157:(0/2)
    // x Best type is dynamic
    {
        return Add(x, 1);
    }

    public static void MinusOne(var x)
    // ParameterSymbol=x$1122: = Argument:RefSymbol=Subtract$159:(0/2)
    // x Best type is dynamic
    {
        return Subtract(x, 1);
    }

    public static void FromOne(var x)
    // ParameterSymbol=x$1124: = Argument:RefSymbol=Subtract$159:(1/2)
    // x Best type is dynamic
    {
        return Subtract(1, x);
    }

    public static void Sign(var x)
    // ParameterSymbol=x$1126: = Argument:RefSymbol=LessThan$809:(0/2), Argument:RefSymbol=GreaterThan$817:(0/2)
    // x Best type is dynamic
    // x Best type is dynamic
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
    // ParameterSymbol=x$1130: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 2);
    }

    public static void Third(var x)
    // ParameterSymbol=x$1132: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 3);
    }

    public static void Quarter(var x)
    // ParameterSymbol=x$1134: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 4);
    }

    public static void Fifth(var x)
    // ParameterSymbol=x$1136: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 5);
    }

    public static void Sixth(var x)
    // ParameterSymbol=x$1138: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 6);
    }

    public static void Seventh(var x)
    // ParameterSymbol=x$1140: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 7);
    }

    public static void Eighth(var x)
    // ParameterSymbol=x$1142: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 8);
    }

    public static void Ninth(var x)
    // ParameterSymbol=x$1144: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 9);
    }

    public static void Tenth(var x)
    // ParameterSymbol=x$1146: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 10);
    }

    public static void Sixteenth(var x)
    // ParameterSymbol=x$1148: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 16);
    }

    public static void Hundredth(var x)
    // ParameterSymbol=x$1150: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 100);
    }

    public static void Thousandth(var x)
    // ParameterSymbol=x$1152: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, 1000);
    }

    public static void Millionth(var x)
    // ParameterSymbol=x$1154: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, Divide(1000, 1000));
    }

    public static void Billionth(var x)
    // ParameterSymbol=x$1156: = Argument:RefSymbol=Divide$163:(0/2)
    // x Best type is dynamic
    {
        return Divide(x, Divide(1000, Divide(1000, 1000)));
    }

    public static void Hundred(var x)
    // ParameterSymbol=x$1158: = Argument:RefSymbol=Multiply$161:(0/2)
    // x Best type is dynamic
    {
        return Multiply(x, 100);
    }

    public static void Thousand(var x)
    // ParameterSymbol=x$1160: = Argument:RefSymbol=Multiply$161:(0/2)
    // x Best type is dynamic
    {
        return Multiply(x, 1000);
    }

    public static void Million(var x)
    // ParameterSymbol=x$1162: = Argument:RefSymbol=Multiply$161:(0/2)
    // x Best type is dynamic
    {
        return Multiply(x, Multiply(1000, 1000));
    }

    public static void Billion(var x)
    // ParameterSymbol=x$1164: = Argument:RefSymbol=Multiply$161:(0/2)
    // x Best type is dynamic
    {
        return Multiply(x, Multiply(1000, Multiply(1000, 1000)));
    }

    public static void Twice(var x)
    // ParameterSymbol=x$1166: = Argument:RefSymbol=Multiply$161:(0/2)
    // x Best type is dynamic
    {
        return Multiply(x, 2);
    }

    public static void Thrice(var x)
    // ParameterSymbol=x$1168: = Argument:RefSymbol=Multiply$161:(0/2)
    // x Best type is dynamic
    {
        return Multiply(x, 3);
    }

    public static void SmoothStep(var x)
    // ParameterSymbol=x$1170: = Argument:RefSymbol=Square$735:(0/1), Argument:RefSymbol=Twice$789:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    {
        return Multiply(Square(x), Subtract(3, Twice(x)));
    }

    public static void Pow2(var x)
    // ParameterSymbol=x$1172: = Argument:RefSymbol=Multiply$161:(0/2), Argument:RefSymbol=Multiply$161:(1/2)
    // x Best type is dynamic
    // x Best type is dynamic
    {
        return Multiply(x, x);
    }

    public static void Pow3(var x)
    // ParameterSymbol=x$1174: = Argument:RefSymbol=Multiply$161:(1/2), Argument:RefSymbol=Pow2$795:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    {
        return Multiply(Pow2(x), x);
    }

    public static void Pow4(var x)
    // ParameterSymbol=x$1176: = Argument:RefSymbol=Multiply$161:(1/2), Argument:RefSymbol=Pow3$797:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    {
        return Multiply(Pow3(x), x);
    }

    public static void Pow5(var x)
    // ParameterSymbol=x$1178: = Argument:RefSymbol=Multiply$161:(1/2), Argument:RefSymbol=Pow4$799:(0/1)
    // x Best type is dynamic
    // x Best type is dynamic
    {
        return Multiply(Pow4(x), x);
    }

    public static void Turns(var x)
    // ParameterSymbol=x$1180: = Argument:RefSymbol=Multiply$161:(0/2)
    // x Best type is dynamic
    {
        return Multiply(x, Multiply(3.1415926535897, 2));
    }

    public static void AlmostZero(var x)
    // ParameterSymbol=x$1182: = Argument:RefSymbol=Abs$751:(0/1)
    // x Best type is dynamic
    {
        return LessThan(Abs(x), 1E-08);
    }

}
class Comparable
{
    public static void Equals(var a, var b)
    // ParameterSymbol=a$1184: = Argument:RefSymbol=Compare$141:(0/2)
    // a Best type is dynamic
    // ParameterSymbol=b$1185: = Argument:RefSymbol=Compare$141:(1/2)
    // b Best type is dynamic
    {
        return Equals(Compare(a, b), 0);
    }

    public static void LessThan(var a, var b)
    // ParameterSymbol=a$1187: = Argument:RefSymbol=Compare$141:(0/2)
    // a Best type is dynamic
    // ParameterSymbol=b$1188: = Argument:RefSymbol=Compare$141:(1/2)
    // b Best type is dynamic
    {
        return LessThan(Compare(a, b), 0);
    }

    public static void Lesser(var a, var b)
    // ParameterSymbol=a$1190: = Argument:RefSymbol=LessThanOrEquals$815:(0/2)
    // a Best type is dynamic
    // ParameterSymbol=b$1191: = Argument:RefSymbol=LessThanOrEquals$815:(1/2)
    // b Best type is dynamic
    {
        return LessThanOrEquals(a, b)
            ? a
            : b
        ;
    }

    public static void Greater(var a, var b)
    // ParameterSymbol=a$1193: = Argument:RefSymbol=GreaterThanOrEquals$819:(0/2)
    // a Best type is dynamic
    // ParameterSymbol=b$1194: = Argument:RefSymbol=GreaterThanOrEquals$819:(1/2)
    // b Best type is dynamic
    {
        return GreaterThanOrEquals(a, b)
            ? a
            : b
        ;
    }

    public static void LessThanOrEquals(var a, var b)
    // ParameterSymbol=a$1196: = Argument:RefSymbol=Compare$141:(0/2)
    // a Best type is dynamic
    // ParameterSymbol=b$1197: = Argument:RefSymbol=Compare$141:(1/2)
    // b Best type is dynamic
    {
        return LessThanOrEquals(Compare(a, b), 0);
    }

    public static void GreaterThan(var a, var b)
    // ParameterSymbol=a$1199: = Argument:RefSymbol=Compare$141:(0/2)
    // a Best type is dynamic
    // ParameterSymbol=b$1200: = Argument:RefSymbol=Compare$141:(1/2)
    // b Best type is dynamic
    {
        return GreaterThan(Compare(a, b), 0);
    }

    public static void GreaterThanOrEquals(var a, var b)
    // ParameterSymbol=a$1202: = Argument:RefSymbol=Compare$141:(0/2)
    // a Best type is dynamic
    // ParameterSymbol=b$1203: = Argument:RefSymbol=Compare$141:(1/2)
    // b Best type is dynamic
    {
        return GreaterThanOrEquals(Compare(a, b), 0);
    }

    public static void Min(var a, var b)
    // ParameterSymbol=a$1205: = Argument:RefSymbol=LessThan$809:(0/2)
    // a Best type is dynamic
    // ParameterSymbol=b$1206: = Argument:RefSymbol=LessThan$809:(1/2)
    // b Best type is dynamic
    {
        return LessThan(a, b)
            ? a
            : b
        ;
    }

    public static void Max(var a, var b)
    // ParameterSymbol=a$1208: = Argument:RefSymbol=GreaterThan$817:(0/2)
    // a Best type is dynamic
    // ParameterSymbol=b$1209: = Argument:RefSymbol=GreaterThan$817:(1/2)
    // b Best type is dynamic
    {
        return GreaterThan(a, b)
            ? a
            : b
        ;
    }

    public static void Between(var v, var a, var b)
    // ParameterSymbol=v$1211: = Argument:RefSymbol=Between$827:(0/2)
    // v Best type is dynamic
    // ParameterSymbol=a$1212: = Argument:RefSymbol=Interval$120:(0/2)
    // a Best type is Any
    // ParameterSymbol=b$1213: = Argument:RefSymbol=Interval$120:(1/2)
    // b Best type is Any
    {
        return Between(v, Interval(a, b));
    }

    public static void Between(var v, var i)
    // ParameterSymbol=v$1215: = Argument:RefSymbol=Contains$659:(1/2)
    // v Best type is dynamic
    // ParameterSymbol=i$1216: = Argument:RefSymbol=Contains$659:(0/2)
    // i Best type is dynamic
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
    // ParameterSymbol=b$1227: = Argument:RefSymbol=Not$833:(0/1)
    // b Best type is dynamic
    {
        return a
            ? Not(b)
            : b
        ;
    }

    public static void NAnd(var a, var b)
    // ParameterSymbol=a$1229: = Argument:RefSymbol=And$829:(0/2)
    // a Best type is dynamic
    // ParameterSymbol=b$1230: = Argument:RefSymbol=And$829:(1/2)
    // b Best type is dynamic
    {
        return Not(And(a, b));
    }

    public static void NOr(var a, var b)
    // ParameterSymbol=a$1232: = Argument:RefSymbol=Or$831:(0/2)
    // a Best type is dynamic
    // ParameterSymbol=b$1233: = Argument:RefSymbol=Or$831:(1/2)
    // b Best type is dynamic
    {
        return Not(Or(a, b));
    }

}
class Equatable
{
    public static void NotEquals(var x)
    // ParameterSymbol=x$1235: = Argument:RefSymbol=Equals$807:(0/1)
    // x Best type is dynamic
    {
        return Not(Equals(x));
    }

}
class Array
{
    public static void Map(var xs, var f)
    // ParameterSymbol=xs$1237: = Argument:RefSymbol=Count$13:(0/1), Argument:RefSymbol=At$189:(0/2)
    // xs Best type is Count
    // xs Best type is dynamic
    // ParameterSymbol=f$1238: = Invoked:(ArgumentSymbol)
    {
        return Map(Count(xs), (i) => 
        // ParameterSymbol=i$1239: = Argument:RefSymbol=At$189:(1/2)
        // i Best type is dynamic
        f(At(xs, i)));
    }

    public static void Map(var n, var f)
    // ParameterSymbol=n$1252: = Argument:RefSymbol=Array$127:(0/2)
    // n Best type is Any
    // ParameterSymbol=f$1253: = Argument:RefSymbol=Array$127:(1/2)
    // f Best type is Any
    {
        return Array(n, f);
    }

    public static void Zip(var xs, var ys, var f)
    // ParameterSymbol=xs$1255: = Argument:RefSymbol=Count$13:(0/1)
    // xs Best type is Count
    // ParameterSymbol=ys$1256: = Argument:RefSymbol=At$189:(0/2)
    // ys Best type is dynamic
    // ParameterSymbol=f$1257: = Invoked:(ArgumentSymbol,ArgumentSymbol)
    {
        return Array(Count(xs), (i) => 
        // ParameterSymbol=i$1258: = Argument:RefSymbol=At$189:(0/1), Argument:RefSymbol=At$189:(1/2)
        // i Best type is dynamic
        // i Best type is dynamic
        f(At(i), At(ys, i)));
    }

    public static void Skip(var xs, var n)
    // ParameterSymbol=xs$1261: = 
    // ParameterSymbol=n$1262: = Argument:RefSymbol=Subtract$159:(1/2), Argument:RefSymbol=Subtract$159:(1/2)
    // n Best type is dynamic
    // n Best type is dynamic
    {
        return Array(Subtract(Count, n), (i) => 
        // ParameterSymbol=i$1263: = Argument:RefSymbol=Subtract$159:(0/2)
        // i Best type is dynamic
        At(Subtract(i, n)));
    }

    public static void Take(var xs, var n)
    // ParameterSymbol=xs$1266: = 
    // ParameterSymbol=n$1267: = Argument:RefSymbol=Array$127:(0/2)
    // n Best type is Any
    {
        return Array(n, (i) => 
        // ParameterSymbol=i$1268: = 
        At);
    }

    public static void Aggregate(var xs, var init, var f)
    // ParameterSymbol=xs$1271: = Argument:RefSymbol=IsEmpty$857:(0/1), Argument:RefSymbol=Rest$855:(0/1)
    // xs Best type is dynamic
    // xs Best type is dynamic
    // ParameterSymbol=init$1272: = Argument:RefSymbol=f$1273:(0/2)
    // init Best type is Any
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
    // ParameterSymbol=xs$1277: = Argument:RefSymbol=Count$13:(0/1)
    // xs Best type is Count
    {
        return Equals(Count(xs), 0);
    }

    public static void First(var xs)
    // ParameterSymbol=xs$1279: = Argument:RefSymbol=At$189:(0/2)
    // xs Best type is dynamic
    {
        return At(xs, 0);
    }

    public static void Last(var xs)
    // ParameterSymbol=xs$1281: = Argument:RefSymbol=At$189:(0/2), Argument:RefSymbol=Count$13:(0/1)
    // xs Best type is dynamic
    // xs Best type is Count
    {
        return At(xs, Subtract(Count(xs), 1));
    }

    public static void Slice(var xs, var from, var count)
    // ParameterSymbol=xs$1283: = Argument:RefSymbol=Skip$849:(0/2)
    // xs Best type is dynamic
    // ParameterSymbol=from$1284: = Argument:RefSymbol=Skip$849:(1/2)
    // from Best type is dynamic
    // ParameterSymbol=count$1285: = Argument:RefSymbol=Take$851:(1/2)
    // count Best type is dynamic
    {
        return Take(Skip(xs, from), count);
    }

    public static void Join(var xs, var sep)
    // ParameterSymbol=xs$1287: = Argument:RefSymbol=IsEmpty$857:(0/1), Argument:RefSymbol=First$859:(0/1), Argument:RefSymbol=Skip$849:(0/2)
    // xs Best type is dynamic
    // xs Best type is dynamic
    // xs Best type is dynamic
    // ParameterSymbol=sep$1288: = Argument:RefSymbol=Interpolate$937:(1/3)
    // sep Best type is dynamic
    {
        return IsEmpty(xs)
            ? 
            : Add(ToString(First(xs)), Aggregate(Skip(xs, 1), , (acc, cur) => 
            // ParameterSymbol=acc$1289: = Argument:RefSymbol=Interpolate$937:(0/3)
            // acc Best type is dynamic
            // ParameterSymbol=cur$1290: = Argument:RefSymbol=Interpolate$937:(2/3)
            // cur Best type is dynamic
            Interpolate(acc, sep, cur)))
        ;
    }

    public static void All(var xs, var f)
    // ParameterSymbol=xs$1293: = Argument:RefSymbol=IsEmpty$857:(0/1), Argument:RefSymbol=First$859:(0/1), Argument:RefSymbol=Rest$855:(0/1)
    // xs Best type is dynamic
    // xs Best type is dynamic
    // xs Best type is dynamic
    // ParameterSymbol=f$1294: = Invoked:(ArgumentSymbol), Invoked:(ArgumentSymbol)
    {
        return IsEmpty(xs)
            ? True
            : And(f(First(xs)), f(Rest(xs)))
        ;
    }

    public static void JoinStrings(var xs, var sep)
    // ParameterSymbol=xs$1296: = Argument:RefSymbol=IsEmpty$857:(0/1), Argument:RefSymbol=First$859:(0/1), Argument:RefSymbol=Rest$855:(0/1)
    // xs Best type is dynamic
    // xs Best type is dynamic
    // xs Best type is dynamic
    // ParameterSymbol=sep$1297: = 
    {
        return IsEmpty(xs)
            ? 
            : Add(First(xs), Aggregate(Rest(xs), , (x, acc) => 
            // ParameterSymbol=x$1298: = Argument:RefSymbol=ToString$185:(0/1)
            // x Best type is dynamic
            // ParameterSymbol=acc$1299: = Argument:RefSymbol=Add$157:(0/2)
            // acc Best type is dynamic
            Add(acc, Add(, , ToString(x)))))
        ;
    }

}
class Easings
{
    public static void BlendEaseFunc(var p, var easeIn, var easeOut)
    // ParameterSymbol=p$1302: = Argument:RefSymbol=LessThan$809:(0/2), Argument:RefSymbol=Multiply$161:(0/2), Argument:RefSymbol=Multiply$161:(0/2)
    // p Best type is dynamic
    // p Best type is dynamic
    // p Best type is dynamic
    // ParameterSymbol=easeIn$1303: = Invoked:(ArgumentSymbol)
    // ParameterSymbol=easeOut$1304: = Invoked:(ArgumentSymbol)
    {
        return LessThan(p, 0.5
            ? Multiply(0.5, easeIn(Multiply(p, 2)))
            : Multiply(0.5, Add(easeOut(Multiply(p, Subtract(2, 1))), 0.5))
        );
    }

    public static void InvertEaseFunc(var p, var easeIn)
    // ParameterSymbol=p$1306: = Argument:RefSymbol=Subtract$159:(1/2)
    // p Best type is dynamic
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
    // ParameterSymbol=p$1311: = Argument:RefSymbol=Pow2$795:(0/1)
    // p Best type is dynamic
    {
        return Pow2(p);
    }

    public static void QuadraticEaseOut(var p)
    // ParameterSymbol=p$1313: = Argument:RefSymbol=InvertEaseFunc$873:(0/2)
    // p Best type is dynamic
    {
        return InvertEaseFunc(p, QuadraticEaseIn);
    }

    public static void QuadraticEaseInOut(var p)
    // ParameterSymbol=p$1315: = Argument:RefSymbol=BlendEaseFunc$871:(0/3)
    // p Best type is dynamic
    {
        return BlendEaseFunc(p, QuadraticEaseIn, QuadraticEaseOut);
    }

    public static void CubicEaseIn(var p)
    // ParameterSymbol=p$1317: = Argument:RefSymbol=Pow3$797:(0/1)
    // p Best type is dynamic
    {
        return Pow3(p);
    }

    public static void CubicEaseOut(var p)
    // ParameterSymbol=p$1319: = Argument:RefSymbol=InvertEaseFunc$873:(0/2)
    // p Best type is dynamic
    {
        return InvertEaseFunc(p, CubicEaseIn);
    }

    public static void CubicEaseInOut(var p)
    // ParameterSymbol=p$1321: = Argument:RefSymbol=BlendEaseFunc$871:(0/3)
    // p Best type is dynamic
    {
        return BlendEaseFunc(p, CubicEaseIn, CubicEaseOut);
    }

    public static void QuarticEaseIn(var p)
    // ParameterSymbol=p$1323: = Argument:RefSymbol=Pow4$799:(0/1)
    // p Best type is dynamic
    {
        return Pow4(p);
    }

    public static void QuarticEaseOut(var p)
    // ParameterSymbol=p$1325: = Argument:RefSymbol=InvertEaseFunc$873:(0/2)
    // p Best type is dynamic
    {
        return InvertEaseFunc(p, QuarticEaseIn);
    }

    public static void QuarticEaseInOut(var p)
    // ParameterSymbol=p$1327: = Argument:RefSymbol=BlendEaseFunc$871:(0/3)
    // p Best type is dynamic
    {
        return BlendEaseFunc(p, QuarticEaseIn, QuarticEaseOut);
    }

    public static void QuinticEaseIn(var p)
    // ParameterSymbol=p$1329: = Argument:RefSymbol=Pow5$801:(0/1)
    // p Best type is dynamic
    {
        return Pow5(p);
    }

    public static void QuinticEaseOut(var p)
    // ParameterSymbol=p$1331: = Argument:RefSymbol=InvertEaseFunc$873:(0/2)
    // p Best type is dynamic
    {
        return InvertEaseFunc(p, QuinticEaseIn);
    }

    public static void QuinticEaseInOut(var p)
    // ParameterSymbol=p$1333: = Argument:RefSymbol=BlendEaseFunc$871:(0/3)
    // p Best type is dynamic
    {
        return BlendEaseFunc(p, QuinticEaseIn, QuinticEaseOut);
    }

    public static void SineEaseIn(var p)
    // ParameterSymbol=p$1335: = Argument:RefSymbol=InvertEaseFunc$873:(0/2)
    // p Best type is dynamic
    {
        return InvertEaseFunc(p, SineEaseOut);
    }

    public static void SineEaseOut(var p)
    // ParameterSymbol=p$1337: = Argument:RefSymbol=Quarter$757:(0/1)
    // p Best type is dynamic
    {
        return Sin(Turns(Quarter(p)));
    }

    public static void SineEaseInOut(var p)
    // ParameterSymbol=p$1339: = Argument:RefSymbol=BlendEaseFunc$871:(0/3)
    // p Best type is dynamic
    {
        return BlendEaseFunc(p, SineEaseIn, SineEaseOut);
    }

    public static void CircularEaseIn(var p)
    // ParameterSymbol=p$1341: = Argument:RefSymbol=Pow2$795:(0/1)
    // p Best type is dynamic
    {
        return FromOne(SquareRoot(FromOne(Pow2(p))));
    }

    public static void CircularEaseOut(var p)
    // ParameterSymbol=p$1343: = Argument:RefSymbol=InvertEaseFunc$873:(0/2)
    // p Best type is dynamic
    {
        return InvertEaseFunc(p, CircularEaseIn);
    }

    public static void CircularEaseInOut(var p)
    // ParameterSymbol=p$1345: = Argument:RefSymbol=BlendEaseFunc$871:(0/3)
    // p Best type is dynamic
    {
        return BlendEaseFunc(p, CircularEaseIn, CircularEaseOut);
    }

    public static void ExponentialEaseIn(var p)
    // ParameterSymbol=p$1347: = Argument:RefSymbol=AlmostZero$805:(0/1), Argument:RefSymbol=MinusOne$745:(0/1)
    // p Best type is dynamic
    // p Best type is dynamic
    {
        return AlmostZero(p)
            ? p
            : Pow(2, Multiply(10, MinusOne(p)))
        ;
    }

    public static void ExponentialEaseOut(var p)
    // ParameterSymbol=p$1349: = Argument:RefSymbol=InvertEaseFunc$873:(0/2)
    // p Best type is dynamic
    {
        return InvertEaseFunc(p, ExponentialEaseIn);
    }

    public static void ExponentialEaseInOut(var p)
    // ParameterSymbol=p$1351: = Argument:RefSymbol=BlendEaseFunc$871:(0/3)
    // p Best type is dynamic
    {
        return BlendEaseFunc(p, ExponentialEaseIn, ExponentialEaseOut);
    }

    public static void ElasticEaseIn(var p)
    // ParameterSymbol=p$1353: = Argument:RefSymbol=Quarter$757:(0/1), Argument:RefSymbol=MinusOne$745:(0/1)
    // p Best type is dynamic
    // p Best type is dynamic
    {
        return Multiply(13, Multiply(Turns(Quarter(p)), Sin(Radians(Pow(2, Multiply(10, MinusOne(p)))))));
    }

    public static void ElasticEaseOut(var p)
    // ParameterSymbol=p$1355: = Argument:RefSymbol=InvertEaseFunc$873:(0/2)
    // p Best type is dynamic
    {
        return InvertEaseFunc(p, ElasticEaseIn);
    }

    public static void ElasticEaseInOut(var p)
    // ParameterSymbol=p$1357: = Argument:RefSymbol=BlendEaseFunc$871:(0/3)
    // p Best type is dynamic
    {
        return BlendEaseFunc(p, ElasticEaseIn, ElasticEaseOut);
    }

    public static void BackEaseIn(var p)
    // ParameterSymbol=p$1359: = Argument:RefSymbol=Pow3$797:(0/1), Argument:RefSymbol=Multiply$161:(0/2), Argument:RefSymbol=Half$753:(0/1)
    // p Best type is dynamic
    // p Best type is dynamic
    // p Best type is dynamic
    {
        return Subtract(Pow3(p), Multiply(p, Sin(Turns(Half(p)))));
    }

    public static void BackEaseOut(var p)
    // ParameterSymbol=p$1361: = Argument:RefSymbol=InvertEaseFunc$873:(0/2)
    // p Best type is dynamic
    {
        return InvertEaseFunc(p, BackEaseIn);
    }

    public static void BackEaseInOut(var p)
    // ParameterSymbol=p$1363: = Argument:RefSymbol=BlendEaseFunc$871:(0/3)
    // p Best type is dynamic
    {
        return BlendEaseFunc(p, BackEaseIn, BackEaseOut);
    }

    public static void BounceEaseIn(var p)
    // ParameterSymbol=p$1365: = Argument:RefSymbol=InvertEaseFunc$873:(0/2)
    // p Best type is dynamic
    {
        return InvertEaseFunc(p, BounceEaseOut);
    }

    public static void BounceEaseOut(var p)
    // ParameterSymbol=p$1367: = Argument:RefSymbol=LessThan$809:(0/2), Argument:RefSymbol=Pow2$795:(0/1), Argument:RefSymbol=LessThan$809:(0/2), Argument:RefSymbol=Pow2$795:(0/1), Argument:RefSymbol=Add$157:(0/2), Argument:RefSymbol=LessThan$809:(0/2), Argument:RefSymbol=Pow2$795:(0/1), Argument:RefSymbol=Add$157:(0/2), Argument:RefSymbol=Pow2$795:(0/1), Argument:RefSymbol=Add$157:(0/2)
    // p Best type is dynamic
    // p Best type is dynamic
    // p Best type is dynamic
    // p Best type is dynamic
    // p Best type is dynamic
    // p Best type is dynamic
    // p Best type is dynamic
    // p Best type is dynamic
    // p Best type is dynamic
    // p Best type is dynamic
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
    // ParameterSymbol=p$1369: = Argument:RefSymbol=BlendEaseFunc$871:(0/3)
    // p Best type is dynamic
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
