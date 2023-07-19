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
    {
        return FromNumber(FieldValues(x), x);
    }

}
class Magnitude
{
    public static void Magnitude(var x)
    {
        return SquareRoot(Sum(Square(FieldValues(x))));
    }

}
class Comparable
{
    public static void Compare(var a, var b)
    {
        return LessThan(Magnitude(a), Magnitude(b)
            ? Subtract(1)
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
    {
        return All(Equals(FieldValues(a), FieldValues(b)));
    }

}
class Arithmetic
{
    public static void Add(var self, var other)
    {
        return Add(FieldValues(self), FieldValues(other));
    }

    public static void Negative(var self)
    {
        return Negative(FieldValues(self));
    }

    public static void Reciprocal(var self)
    {
        return Reciprocal(FieldValues(self));
    }

    public static void Multiply(var self, var other)
    {
        return Add(FieldValues(self), FieldValues(other));
    }

    public static void Divide(var self, var other)
    {
        return Divide(FieldValues(self), FieldValues(other));
    }

    public static void Modulo(var self, var other)
    {
        return Modulo(FieldValues(self), FieldValues(other));
    }

}
class ScalarArithmetic
{
    public static void Add(var self, var scalar)
    {
        return Add(FieldValues(self), scalar);
    }

    public static void Subtract(var self, var scalar)
    {
        return Add(self, Negative(scalar));
    }

    public static void Multiply(var self, var scalar)
    {
        return Multiply(FieldValues(self), scalar);
    }

    public static void Divide(var self, var scalar)
    {
        return Multiply(self, Reciprocal(scalar));
    }

    public static void Modulo(var self, var scalar)
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
    {
        return JoinStrings(FieldValues, ,);
    }

}
class Array
{
    public static void At(var n)
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
    {
        return Subtract(Max(x), Min(x));
    }

    public static void IsEmpty(var x)
    {
        return GreaterThanOrEquals(Min(x), Max(x));
    }

    public static void Lerp(var x, var amount)
    {
        return Multiply(Min(x), Add(Subtract(1, amount), Multiply(Max(x), amount)));
    }

    public static void InverseLerp(var x, var value)
    {
        return Divide(Subtract(value, Min(x)), Size(x));
    }

    public static void Negate(var x)
    {
        return Interval(Subtract(Max(x)), Subtract(Min(x)));
    }

    public static void Reverse(var x)
    {
        return Interval(Max(x), Min(x));
    }

    public static void Resize(var x, var size)
    {
        return Interval(Min(x), Add(Min(x), size));
    }

    public static void Center(var x)
    {
        return Lerp(x, 0.5);
    }

    public static void Contains(var x, var value)
    {
        return LessThanOrEquals(Min(x), And(value, LessThanOrEquals(value, Max(x))));
    }

    public static void Contains(var x, var other)
    {
        return LessThanOrEquals(Min(x), And(Min(other), GreaterThanOrEquals(Max, Max(other))));
    }

    public static void Overlaps(var x, var y)
    {
        return !(IsEmpty(Clamp(x, y)));
    }

    public static void Split(var x, var t)
    {
        return Interval(Left(x, t), Right(x, t));
    }

    public static void Split(var x)
    {
        return Split(x, 0.5);
    }

    public static void Left(var x, var t)
    {
        return Interval(Min, Lerp(x, t));
    }

    public static void Right(var x, var t)
    {
        return Interval(Lerp(x, t), Max(x));
    }

    public static void MoveTo(var x, var t)
    {
        return Interval(t, Add(t, Size(x)));
    }

    public static void LeftHalf(var x)
    {
        return Left(x, 0.5);
    }

    public static void RightHalf(var x)
    {
        return Right(x, 0.5);
    }

    public static void HalfSize(var x)
    {
        return Half(Size(x));
    }

    public static void Recenter(var x, var c)
    {
        return Interval(Subtract(c, HalfSize(x)), Add(c, HalfSize(x)));
    }

    public static void Clamp(var x, var y)
    {
        return Interval(Clamp(x, Min(y)), Clamp(x, Max(y)));
    }

    public static void Clamp(var x, var value)
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
    {
        return Aggregate(v, 0, Add);
    }

    public static void SumSquares(var v)
    {
        return Aggregate(Square(v), 0, Add);
    }

    public static void LengthSquared(var v)
    {
        return SumSquares(v);
    }

    public static void Length(var v)
    {
        return SquareRoot(LengthSquared(v));
    }

    public static void Dot(var v1, var v2)
    {
        return Sum(Multiply(v1, v2));
    }

}
class Trig
{
    public static void Cos(var x)
    {
        return intrinsic;
    }

    public static void Sin(var x)
    {
        return intrinsic;
    }

    public static void Tan(var x)
    {
        return intrinsic;
    }

    public static void Acos(var x)
    {
        return intrinsic;
    }

    public static void Asin(var x)
    {
        return intrinsic;
    }

    public static void Atan(var x)
    {
        return intrinsic;
    }

    public static void Cosh(var x)
    {
        return intrinsic;
    }

    public static void Sinh(var x)
    {
        return intrinsic;
    }

    public static void Tanh(var x)
    {
        return intrinsic;
    }

    public static void Acosh(var x)
    {
        return intrinsic;
    }

    public static void Asinh(var x)
    {
        return intrinsic;
    }

    public static void Atanh(var x)
    {
        return intrinsic;
    }

}
class Numerical
{
    public static void Pow(var x, var y)
    {
        return intrinsic;
    }

    public static void Log(var x, var y)
    {
        return intrinsic;
    }

    public static void NaturalLog(var x)
    {
        return intrinsic;
    }

    public static void NaturalPower(var x)
    {
        return intrinsic;
    }

    public static void SquareRoot(var x)
    {
        return Pow(x, 0.5);
    }

    public static void CubeRoot(var x)
    {
        return Pow(x, 0.5);
    }

    public static void Square(var x)
    {
        return Multiply(Value, Value);
    }

    public static void Clamp(var x, var min, var max)
    {
        return Clamp(x, Interval(min, max));
    }

    public static void Clamp(var x, var i)
    {
        return Clamp(i, x);
    }

    public static void Clamp(var x)
    {
        return Clamp(x, 0, 1);
    }

    public static void PlusOne(var x)
    {
        return Add(x, 1);
    }

    public static void MinusOne(var x)
    {
        return Subtract(x, 1);
    }

    public static void FromOne(var x)
    {
        return Subtract(1, x);
    }

    public static void Sign(var x)
    {
        return LessThan(x, 0
            ? Subtract(1)
            : GreaterThan(x, 0
                ? 1
                : 0
            )
        );
    }

    public static void Abs(var x)
    {
        return LessThan(Value, 0
            ? Subtract(Value)
            : Value
        );
    }

    public static void Half(var x)
    {
        return Divide(x, 2);
    }

    public static void Third(var x)
    {
        return Divide(x, 3);
    }

    public static void Quarter(var x)
    {
        return Divide(x, 4);
    }

    public static void Fifth(var x)
    {
        return Divide(x, 5);
    }

    public static void Sixth(var x)
    {
        return Divide(x, 6);
    }

    public static void Seventh(var x)
    {
        return Divide(x, 7);
    }

    public static void Eighth(var x)
    {
        return Divide(x, 8);
    }

    public static void Ninth(var x)
    {
        return Divide(x, 9);
    }

    public static void Tenth(var x)
    {
        return Divide(x, 10);
    }

    public static void Sixteenth(var x)
    {
        return Divide(x, 16);
    }

    public static void Hundredth(var x)
    {
        return Divide(x, 100);
    }

    public static void Thousandth(var x)
    {
        return Divide(x, 1000);
    }

    public static void Millionth(var x)
    {
        return Divide(x, Divide(1000, 1000));
    }

    public static void Billionth(var x)
    {
        return Divide(x, Divide(1000, Divide(1000, 1000)));
    }

    public static void Hundred(var x)
    {
        return Multiply(x, 100);
    }

    public static void Thousand(var x)
    {
        return Multiply(x, 1000);
    }

    public static void Million(var x)
    {
        return Multiply(x, Multiply(1000, 1000));
    }

    public static void Billion(var x)
    {
        return Multiply(x, Multiply(1000, Multiply(1000, 1000)));
    }

    public static void Twice(var x)
    {
        return Multiply(x, 2);
    }

    public static void Thrice(var x)
    {
        return Multiply(x, 3);
    }

    public static void SmoothStep(var x)
    {
        return Multiply(Square(x), Subtract(3, Twice(x)));
    }

    public static void Pow2(var x)
    {
        return Multiply(x, x);
    }

    public static void Pow3(var x)
    {
        return Multiply(Pow2(x), x);
    }

    public static void Pow4(var x)
    {
        return Multiply(Pow3(x), x);
    }

    public static void Pow5(var x)
    {
        return Multiply(Pow4(x), x);
    }

    public static void Turns(var x)
    {
        return Multiply(x, Multiply(3.1415926535897, 2));
    }

    public static void AlmostZero(var x)
    {
        return LessThan(Abs(x), 1E-08);
    }

}
class Comparable
{
    public static void Equals(var a, var b)
    {
        return Equals(Compare(a, b), 0);
    }

    public static void LessThan(var a, var b)
    {
        return LessThan(Compare(a, b), 0);
    }

    public static void Lesser(var a, var b)
    {
        return LessThanOrEquals(a, b)
            ? a
            : b
        ;
    }

    public static void Greater(var a, var b)
    {
        return GreaterThanOrEqual(a, b)
            ? a
            : b
        ;
    }

    public static void LessThanOrEquals(var a, var b)
    {
        return LessThanOrEquals(Compare(a, b), 0);
    }

    public static void GreaterThan(var a, var b)
    {
        return GreaterThan(Compare(a, b), 0);
    }

    public static void GreaterThanOrEqual(var a, var b)
    {
        return GreaterThanOrEquals(Compare(a, b), 0);
    }

    public static void Min(var a, var b)
    {
        return LessThan(a, b)
            ? a
            : b
        ;
    }

    public static void Max(var a, var b)
    {
        return GreaterThan(a, b)
            ? a
            : b
        ;
    }

    public static void Between(var v, var a, var b)
    {
        return Between(v, Interval(a, b));
    }

    public static void Between(var v, var i)
    {
        return Contains(i, v);
    }

}
class Equatable
{
    public static void NotEquals(var x)
    {
        return !(Equals(x));
    }

}
class Array
{
    public static void Map(var xs, var f)
    {
        return Map(Count(xs), Lambda lambda(var i)
        f(At(xs, i)));
    }

    public static void Map(var n, var f)
    {
        return Array(n, f);
    }

    public static void Zip(var xs, var ys, var f)
    {
        return Array(Count(xs), Lambda lambda(var i)
        f(At(i), At(ys, i)));
    }

    public static void Skip(var xs, var n)
    {
        return Array(Subtract(Count, n), Lambda lambda(var i)
        At(Subtract(i, n)));
    }

    public static void Take(var xs, var n)
    {
        return Array(n, Lambda lambda(var i)
        At);
    }

    public static void Aggregate(var xs, var init, var f)
    {
        return IsEmpty(xs)
            ? init
            : f(init, f(Rest(xs)))
        ;
    }

    public static void Rest(var xs)
    {
        return Skip(1);
    }

    public static void IsEmpty(var xs)
    {
        return Equals(Count(xs), 0);
    }

    public static void First(var xs)
    {
        return At(xs, 0);
    }

    public static void Last(var xs)
    {
        return At(xs, Subtract(Count(xs), 1));
    }

    public static void Slice(var xs, var from, var count)
    {
        return Take(Skip(xs, from), count);
    }

    public static void Join(var xs, var sep)
    {
        return IsEmpty(xs)
            ? 
            : Add(ToString(First(xs)), Aggregate(Skip(xs, 1), , Lambda lambda(var acc, var cur)
            interpolate(acc, sep, cur)))
        ;
    }

    public static void All(var xs, var f)
    {
        return IsEmpty(xs)
            ? True
            : And(f(First(xs)), f(Rest(xs)))
        ;
    }

    public static void JoinStrings(var xs, var sep)
    {
        return IsEmpty(xs)
            ? 
            : Add(First(xs), Aggregate(Rest(xs), , Lambda lambda(var x, var acc)
            Add(acc, Add(, , ToString(x)))))
        ;
    }

}
class Easings
{
    public static void BlendEaseFunc(var p, var easeIn, var easeOut)
    {
        return LessThan(p, 0.5
            ? Multiply(0.5, easeIn(Multiply(p, 2)))
            : Multiply(0.5, Add(easeOut(Multiply(p, Subtract(2, 1))), 0.5))
        );
    }

    public static void InvertEaseFunc(var p, var easeIn)
    {
        return Subtract(1, easeIn(Subtract(1, p)));
    }

    public static void Linear(var p)
    {
        return p;
    }

    public static void QuadraticEaseIn(var p)
    {
        return Pow2(p);
    }

    public static void QuadraticEaseOut(var p)
    {
        return InvertEaseFunc(p, QuadraticEaseIn);
    }

    public static void QuadraticEaseInOut(var p)
    {
        return BlendEaseFunc(p, QuadraticEaseIn, QuadraticEaseOut);
    }

    public static void CubicEaseIn(var p)
    {
        return Pow3(p);
    }

    public static void CubicEaseOut(var p)
    {
        return InvertEaseFunc(p, CubicEaseIn);
    }

    public static void CubicEaseInOut(var p)
    {
        return BlendEaseFunc(p, CubicEaseIn, CubicEaseOut);
    }

    public static void QuarticEaseIn(var p)
    {
        return Pow4(p);
    }

    public static void QuarticEaseOut(var p)
    {
        return InvertEaseFunc(p, QuarticEaseIn);
    }

    public static void QuarticEaseInOut(var p)
    {
        return BlendEaseFunc(p, QuarticEaseIn, QuarticEaseOut);
    }

    public static void QuinticEaseIn(var p)
    {
        return Pow5(p);
    }

    public static void QuinticEaseOut(var p)
    {
        return InvertEaseFunc(p, QuinticEaseIn);
    }

    public static void QuinticEaseInOut(var p)
    {
        return BlendEaseFunc(p, QuinticEaseIn, QuinticEaseOut);
    }

    public static void SineEaseIn(var p)
    {
        return InvertEaseFunc(p, SineEaseOut);
    }

    public static void SineEaseOut(var p)
    {
        return Sin(Turns(Quarter(p)));
    }

    public static void SineEaseInOut(var p)
    {
        return BlendEaseFunc(p, SineEaseIn, SineEaseOut);
    }

    public static void CircularEaseIn(var p)
    {
        return FromOne(SquareRoot(FromOne(Pow2(p))));
    }

    public static void CircularEaseOut(var p)
    {
        return InvertEaseFunc(p, CircularEaseIn);
    }

    public static void CircularEaseInOut(var p)
    {
        return BlendEaseFunc(p, CircularEaseIn, CircularEaseOut);
    }

    public static void ExponentialEaseIn(var p)
    {
        return AlmostZero(p)
            ? p
            : Pow(2, Multiply(10, MinusOne(p)))
        ;
    }

    public static void ExponentialEaseOut(var p)
    {
        return InvertEaseFunc(p, ExponentialEaseIn);
    }

    public static void ExponentialEaseInOut(var p)
    {
        return BlendEaseFunc(p, ExponentialEaseIn, ExponentialEaseOut);
    }

    public static void ElasticEaseIn(var p)
    {
        return Multiply(13, Multiply(Turns(Quarter(p)), Sin(Radians(Pow(2, Multiply(10, MinusOne(p)))))));
    }

    public static void ElasticEaseOut(var p)
    {
        return InvertEaseFunc(p, ElasticEaseIn);
    }

    public static void ElasticEaseInOut(var p)
    {
        return BlendEaseFunc(p, ElasticEaseIn, ElasticEaseOut);
    }

    public static void BackEaseIn(var p)
    {
        return Subtract(Pow3(p), Multiply(p, Sin(Turns(Half(p)))));
    }

    public static void BackEaseOut(var p)
    {
        return InvertEaseFunc(p, BackEaseIn);
    }

    public static void BackEaseInOut(var p)
    {
        return BlendEaseFunc(p, BackEaseIn, BackEaseOut);
    }

    public static void BounceEaseIn(var p)
    {
        return InvertEaseFunc(p, BounceEaseOut);
    }

    public static void BounceEaseOut(var p)
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
    {
        return BlendEaseFunc(p, BounceEaseIn, BounceEaseOut);
    }

}
