interface Interval<Self> where Self : Interval<Self>
{
    T Min;
    T Max;
}
interface Vector<Self> where Self : Vector<Self>
{
}
interface Measure<Self> where Self : Measure<Self>
{
    Number Value;
}
interface Numerical<Self> where Self : Numerical<Self>
{
    public static Self FromNumber(Number x)
    // ParameterSymbol=x$1418:Number Declared:Number, Argument:RefSymbol=FromNumber$157:(1/2), Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Number
    {
        return FromNumber(FieldValues(x), x);
    }

}
interface Magnitude<Self> where Self : Magnitude<Self>
{
    public static Number Magnitude(Self x)
    // ParameterSymbol=x$1420:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    {
        return SquareRoot(Sum(Square(FieldValues(x))));
    }

}
interface Comparable<Self> where Self : Comparable<Self>
{
    public static Integer Compare(Self a, Self b)
    // ParameterSymbol=a$1422:Self Declared:Self, Argument:RefSymbol=Magnitude$159:(0/1), Argument:RefSymbol=Magnitude$159:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$1423:Self Declared:Self, Argument:RefSymbol=Magnitude$159:(0/1), Argument:RefSymbol=Magnitude$159:(0/1)
    // Candidates = Self
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
    public static Boolean Equals(Self a, Self b)
    // ParameterSymbol=a$1425:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$1426:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    {
        return All(Equals(FieldValues(a), FieldValues(b)));
    }

}
interface Arithmetic<Self> where Self : Arithmetic<Self>
{
    public static Self Add(Self self, Self other)
    // ParameterSymbol=self$1428:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1429:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    {
        return Add(FieldValues(self), FieldValues(other));
    }

    public static Self Negative(Self self)
    // ParameterSymbol=self$1431:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    {
        return Negative(FieldValues(self));
    }

    public static Self Reciprocal(Self self)
    // ParameterSymbol=self$1433:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    {
        return Reciprocal(FieldValues(self));
    }

    public static Self Multiply(Self self, Self other)
    // ParameterSymbol=self$1435:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1436:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    {
        return Add(FieldValues(self), FieldValues(other));
    }

    public static Self Divide(Self self, Self other)
    // ParameterSymbol=self$1438:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1439:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    {
        return Divide(FieldValues(self), FieldValues(other));
    }

    public static Self Modulo(Self self, Self other)
    // ParameterSymbol=self$1441:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1442:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    {
        return Modulo(FieldValues(self), FieldValues(other));
    }

}
interface ScalarArithmetic<Self> where Self : ScalarArithmetic<Self>
{
    public static Self Add(Self self, T scalar)
    // ParameterSymbol=self$1445:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$1446:T Declared:T, Argument:RefSymbol=Add$178:(1/2)
    // Candidates = T
    {
        return Add(FieldValues(self), scalar);
    }

    public static Self Subtract(Self self, T scalar)
    // ParameterSymbol=self$1448:Self Declared:Self, Argument:RefSymbol=Add$178:(0/2)
    // Candidates = Self
    // ParameterSymbol=scalar$1449:T Declared:T, Argument:RefSymbol=Negative$167:(0/1)
    // Candidates = T
    {
        return Add(self, Negative(scalar));
    }

    public static Self Multiply(Self self, T scalar)
    // ParameterSymbol=self$1451:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$1452:T Declared:T, Argument:RefSymbol=Multiply$182:(1/2)
    // Candidates = T
    {
        return Multiply(FieldValues(self), scalar);
    }

    public static Self Divide(Self self, T scalar)
    // ParameterSymbol=self$1454:Self Declared:Self, Argument:RefSymbol=Multiply$182:(0/2)
    // Candidates = Self
    // ParameterSymbol=scalar$1455:T Declared:T, Argument:RefSymbol=Reciprocal$169:(0/1)
    // Candidates = T
    {
        return Multiply(self, Reciprocal(scalar));
    }

    public static Self Modulo(Self self, T scalar)
    // ParameterSymbol=self$1457:Self Declared:Self, Argument:RefSymbol=FieldValues$200:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$1458:T Declared:T, Argument:RefSymbol=Modulo$186:(1/2)
    // Candidates = T
    {
        return Modulo(FieldValues(self), scalar);
    }

}
interface Boolean<Self> where Self : Boolean<Self>
{
    public static Self And(Self a, Self b)
    // ParameterSymbol=a$1460:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=b$1461:Self Declared:Self
    // Candidates = Self
    {
        return intrinsic;
    }

    public static void Or(Self a, Self b)
    // ParameterSymbol=a$1463:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=b$1464:Self Declared:Self
    // Candidates = Self
    {
        return intrinsic;
    }

    public static Self Not(Self a)
    // ParameterSymbol=a$1466:Self Declared:Self
    // Candidates = Self
    {
        return intrinsic;
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

    public static Array FieldValues(Self self)
    // ParameterSymbol=self$1471:Self Declared:Self
    // Candidates = Self
    {
        return intrinsic;
    }

    public static Self Zero()
    {
        return Zero(FieldTypes);
    }

    public static Self One()
    {
        return One(FieldTypes);
    }

    public static Self Default()
    {
        return Default(FieldTypes);
    }

    public static Self MinValue()
    {
        return MinValue(FieldTypes);
    }

    public static Self MaxValue()
    {
        return MaxValue(FieldTypes);
    }

    public static void ToString(Self x)
    // ParameterSymbol=x$1478:Self Declared:Self
    // Candidates = Self
    {
        return JoinStrings(FieldValues, ,);
    }

}
interface Array<Self> where Self : Array<Self>
{
    public static T At(Index n)
    // ParameterSymbol=n$1481:Index Declared:Index
    // Candidates = Index
    null
    Count Count;
}
class Integer
{
    Integer Value;
}
class Count
{
    Integer Value;
}
class Index
{
    Integer Value;
}
class Number
{
    var Value;
}
class Unit
{
    Number Value;
}
class Percent
{
    Number Value;
}
class Quaternion
{
    Number X;
    Number Y;
    Number Z;
    Number W;
}
class Unit2D
{
    Unit X;
    Unit Y;
}
class Unit3D
{
    Unit X;
    Unit Y;
    Unit Z;
}
class Direction3D
{
    Unit3D Value;
}
class AxisAngle
{
    Unit3D Axis;
    Angle Angle;
}
class EulerAngles
{
    Angle Yaw;
    Angle Pitch;
    Angle Roll;
}
class Rotation3D
{
    Quaternion Quaternion;
}
class Vector2D
{
    Number X;
    Number Y;
}
class Vector3D
{
    Number X;
    Number Y;
    Number Z;
}
class Vector4D
{
    Number X;
    Number Y;
    Number Z;
    Number W;
}
class Orientation3D
{
    Rotation3D Value;
}
class Pose2D
{
    Vector3D Position;
    Orientation3D Orientation;
}
class Pose3D
{
    Vector3D Position;
    Orientation3D Orientation;
}
class Transform3D
{
    Vector3D Translation;
    Rotation3D Rotation;
    Vector3D Scale;
}
class Transform2D
{
    Vector2D Translation;
    Angle Rotation;
    Vector2D Scale;
}
class AlignedBox2D
{
    Vector2D A;
    Vector2D B;
}
class AlignedBox3D
{
    Vector3D A;
    Vector3D B;
}
class Complex
{
    Number Real;
    Number Imaginary;
}
class Ray3D
{
    Vector3D Direction;
    Point3D Position;
}
class Ray2D
{
    Vector2D Direction;
    Point2D Position;
}
class Sphere
{
    Point3D Center;
    Number Radius;
}
class Plane
{
    Unit3D Normal;
    Number D;
}
class Triangle3D
{
    Point3D A;
    Point3D B;
    Point3D C;
}
class Triangle2D
{
    Point2D A;
    Point2D B;
    Point2D C;
}
class Quad3D
{
    Point3D A;
    Point3D B;
    Point3D C;
    Point3D D;
}
class Quad2D
{
    Point2D A;
    Point2D B;
    Point2D C;
    Point2D D;
}
class Point3D
{
    Vector3D Value;
}
class Point2D
{
    Vector2D Value;
}
class Line3D
{
    Point3D A;
    Point3D B;
}
class Line2D
{
    Point2D A;
    Point2D B;
}
class Color
{
    Unit R;
    Unit G;
    Unit B;
    Unit A;
}
class ColorLUV
{
    Percent Lightness;
    Unit U;
    Unit V;
}
class ColorLAB
{
    Percent Lightness;
    Integer A;
    Integer B;
}
class ColorLCh
{
    Percent Lightness;
    PolarCoordinate ChromaHue;
}
class ColorHSV
{
    Angle Hue;
    Unit S;
    Unit V;
}
class ColorHSL
{
    Angle Hue;
    Unit Saturation;
    Unit Luminance;
}
class ColorYCbCr
{
    Unit Y;
    Unit Cb;
    Unit Cr;
}
class SphericalCoordinate
{
    Number Radius;
    Angle Azimuth;
    Angle Polar;
}
class PolarCoordinate
{
    Number Radius;
    Angle Angle;
}
class LogPolarCoordinate
{
    Number Rho;
    Angle Azimuth;
}
class CylindricalCoordinate
{
    Number RadialDistance;
    Angle Azimuth;
    Number Height;
}
class HorizontalCoordinate
{
    Number Radius;
    Angle Azimuth;
    Number Height;
}
class GeoCoordinate
{
    Angle Latitude;
    Angle Longitude;
}
class GeoCoordinateWithAltitude
{
    GeoCoordinate Coordinate;
    Number Altitude;
}
class Circle
{
    Point2D Center;
    Number Radius;
}
class Chord
{
    Circle Circle;
    Arc Arc;
}
class Size2D
{
    Number Width;
    Number Height;
}
class Size3D
{
    Number Width;
    Number Height;
    Number Depth;
}
class Rectangle2D
{
    Point2D Center;
    Size2D Size;
}
class Proportion
{
    Number Value;
}
class Fraction
{
    Number Numerator;
    Number Denominator;
}
class Angle
{
    Number Radians;
}
class Length
{
    Number Meters;
}
class Mass
{
    Number Kilograms;
}
class Temperature
{
    Number Celsius;
}
class TimeSpan
{
    Number Seconds;
}
class TimeRange
{
    DateTime Min;
    DateTime Max;
}
class DateTime
{
}
class AnglePair
{
    Angle Start;
    Angle End;
}
class Ring
{
    Circle Circle;
    Number InnerRadius;
}
class Arc
{
    AnglePair Angles;
    Circle Cirlce;
}
class TimeInterval
{
    TimeSpan Start;
    TimeSpan End;
}
class RealInterval
{
    Number A;
    Number B;
}
class Interval2D
{
    Vector2D A;
    Vector2D B;
}
class Interval3D
{
    Vector3D A;
    Vector3D B;
}
class Capsule
{
    Line3D Line;
    Number Radius;
}
class Matrix3D
{
    Vector4D Column1;
    Vector4D Column2;
    Vector4D Column3;
    Vector4D Column4;
}
class Cylinder
{
    Line3D Line;
    Number Radius;
}
class Cone
{
    Line3D Line;
    Number Radius;
}
class Tube
{
    Line3D Line;
    Number InnerRadius;
    Number OuterRadius;
}
class ConeSegment
{
    Line3D Line;
    Number Radius1;
    Number Radius2;
}
class Box2D
{
    Point2D Center;
    Angle Rotation;
    Size2D Extent;
}
class Box3D
{
    Point3D Center;
    Rotation3D Rotation;
    Size3D Extent;
}
class CubicBezierTriangle3D
{
    Point3D A;
    Point3D B;
    Point3D C;
    Point3D A2B;
    Point3D AB2;
    Point3D B2C;
    Point3D BC2;
    Point3D AC2;
    Point3D A2C;
    Point3D ABC;
}
class CubicBezier2D
{
    Point2D A;
    Point2D B;
    Point2D C;
    Point2D D;
}
class UV
{
    Unit U;
    Unit V;
}
class UVW
{
    Unit U;
    Unit V;
    Unit W;
}
class CubicBezier3D
{
    Point3D A;
    Point3D B;
    Point3D C;
    Point3D D;
}
class QuadraticBezier2D
{
    Point2D A;
    Point2D B;
    Point2D C;
}
class QuadraticBezier3D
{
    Point3D A;
    Point3D B;
    Point3D C;
}
class Area
{
    Number MetersSquared;
}
class Volume
{
    Number MetersCubed;
}
class Velocity
{
    Number MetersPerSecond;
}
class Acceleration
{
    Number MetersPerSecondSquared;
}
class Force
{
    Number Newtons;
}
class Pressure
{
    Number Pascals;
}
class Energy
{
    Number Joules;
}
class Memory
{
    Count Bytes;
}
class Frequency
{
    Number Hertz;
}
class Loudness
{
    Number Decibels;
}
class LuminousIntensity
{
    Number Candelas;
}
class ElectricPotential
{
    Number Volts;
}
class ElectricCharge
{
    Number Columbs;
}
class ElectricCurrent
{
    Number Amperes;
}
class ElectricResistance
{
    Number Ohms;
}
class Power
{
    Number Watts;
}
class Density
{
    Number KilogramsPerMeterCubed;
}
class NormalDistribution
{
    Number Mean;
    Number StandardDeviation;
}
class PoissonDistribution
{
    Number Expected;
    Count Occurrences;
}
class BernoulliDistribution
{
    Probability P;
}
class Probability
{
    Number Value;
}
class BinomialDistribution
{
    Count Trials;
    Probability P;
}
class Interval
{
    public static void Size(var x)
    // ParameterSymbol=x$1483: Argument:RefSymbol=Max$1303:(0/1), Argument:RefSymbol=Min$1301:(0/1)
    // Candidates = Comparable
    {
        return Subtract(Max(x), Min(x));
    }

    public static void IsEmpty(var x)
    // ParameterSymbol=x$1485: Argument:RefSymbol=Min$1301:(0/1), Argument:RefSymbol=Max$1303:(0/1)
    // Candidates = Comparable
    {
        return GreaterThanOrEquals(Min(x), Max(x));
    }

    public static void Lerp(var x, var amount)
    // ParameterSymbol=x$1487: Argument:RefSymbol=Min$1301:(0/1), Argument:RefSymbol=Max$1303:(0/1)
    // Candidates = Comparable
    // ParameterSymbol=amount$1488: Argument:RefSymbol=Subtract$180:(1/2), Argument:RefSymbol=Multiply$182:(1/2)
    // Candidates = ScalarArithmetic,Arithmetic
    {
        return Multiply(Min(x), Add(Subtract(1, amount), Multiply(Max(x), amount)));
    }

    public static void InverseLerp(var x, var value)
    // ParameterSymbol=x$1490: Argument:RefSymbol=Min$1301:(0/1), Argument:RefSymbol=Size$1121:(0/1)
    // Candidates = Comparable,Interval
    // ParameterSymbol=value$1491: Argument:RefSymbol=Subtract$180:(0/2)
    // Candidates = ScalarArithmetic
    {
        return Divide(Subtract(value, Min(x)), Size(x));
    }

    public static void Negate(var x)
    // ParameterSymbol=x$1493: Argument:RefSymbol=Max$1303:(0/1), Argument:RefSymbol=Min$1301:(0/1)
    // Candidates = Comparable
    {
        return Tuple(Negative(Max(x)), Negative(Min(x)));
    }

    public static void Reverse(var x)
    // ParameterSymbol=x$1495: Argument:RefSymbol=Max$1303:(0/1), Argument:RefSymbol=Min$1301:(0/1)
    // Candidates = Comparable
    {
        return Tuple(Max(x), Min(x));
    }

    public static void Resize(var x, var size)
    // ParameterSymbol=x$1497: Argument:RefSymbol=Min$1301:(0/1), Argument:RefSymbol=Min$1301:(0/1)
    // Candidates = Comparable
    // ParameterSymbol=size$1498: Argument:RefSymbol=Add$178:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Tuple(Min(x), Add(Min(x), size));
    }

    public static void Center(var x)
    // ParameterSymbol=x$1500: Argument:RefSymbol=Lerp$1125:(0/2)
    // Candidates = Interval
    {
        return Lerp(x, 0.5);
    }

    public static void Contains(var x, var value)
    // ParameterSymbol=x$1502: Argument:RefSymbol=Min$1301:(0/1), Argument:RefSymbol=Max$1303:(0/1)
    // Candidates = Comparable
    // ParameterSymbol=value$1503: Argument:RefSymbol=And$188:(0/2), Argument:RefSymbol=LessThanOrEquals$1295:(0/2)
    // Candidates = Boolean,Comparable
    {
        return LessThanOrEquals(Min(x), And(value, LessThanOrEquals(value, Max(x))));
    }

    public static void Contains(var x, var other)
    // ParameterSymbol=x$1505: Argument:RefSymbol=Min$1301:(0/1)
    // Candidates = Comparable
    // ParameterSymbol=other$1506: Argument:RefSymbol=Min$1301:(0/1), Argument:RefSymbol=Max$1303:(0/1)
    // Candidates = Comparable
    {
        return LessThanOrEquals(Min(x), And(Min(other), GreaterThanOrEquals(Max, Max(other))));
    }

    public static void Overlaps(var x, var y)
    // ParameterSymbol=x$1508: Argument:RefSymbol=Clamp$1221:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=y$1509: Argument:RefSymbol=Clamp$1221:(1/2)
    // Candidates = Interval,Numerical
    {
        return Not(IsEmpty(Clamp(x, y)));
    }

    public static void Split(var x, var t)
    // ParameterSymbol=x$1511: Argument:RefSymbol=Left$1147:(0/2), Argument:RefSymbol=Right$1149:(0/2)
    // Candidates = Interval
    // ParameterSymbol=t$1512: Argument:RefSymbol=Left$1147:(1/2), Argument:RefSymbol=Right$1149:(1/2)
    // Candidates = Interval
    {
        return Tuple(Left(x, t), Right(x, t));
    }

    public static void Split(var x)
    // ParameterSymbol=x$1514: Argument:RefSymbol=Split$1145:(0/2)
    // Candidates = Interval
    {
        return Split(x, 0.5);
    }

    public static void Left(var x, var t)
    // ParameterSymbol=x$1516: Argument:RefSymbol=Lerp$1125:(0/2)
    // Candidates = Interval
    // ParameterSymbol=t$1517: Argument:RefSymbol=Lerp$1125:(1/2)
    // Candidates = Interval
    {
        return Tuple(Min, Lerp(x, t));
    }

    public static void Right(var x, var t)
    // ParameterSymbol=x$1519: Argument:RefSymbol=Lerp$1125:(0/2), Argument:RefSymbol=Max$1303:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=t$1520: Argument:RefSymbol=Lerp$1125:(1/2)
    // Candidates = Interval
    {
        return Tuple(Lerp(x, t), Max(x));
    }

    public static void MoveTo(var x, var t)
    // ParameterSymbol=x$1522: Argument:RefSymbol=Size$1121:(0/1)
    // Candidates = Interval
    // ParameterSymbol=t$1523: Argument:RefSymbol=Tuple$1:(0/2), Argument:RefSymbol=Add$178:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Tuple(t, Add(t, Size(x)));
    }

    public static void LeftHalf(var x)
    // ParameterSymbol=x$1525: Argument:RefSymbol=Left$1147:(0/2)
    // Candidates = Interval
    {
        return Left(x, 0.5);
    }

    public static void RightHalf(var x)
    // ParameterSymbol=x$1527: Argument:RefSymbol=Right$1149:(0/2)
    // Candidates = Interval
    {
        return Right(x, 0.5);
    }

    public static void HalfSize(var x)
    // ParameterSymbol=x$1529: Argument:RefSymbol=Size$1121:(0/1)
    // Candidates = Interval
    {
        return Half(Size(x));
    }

    public static void Recenter(var x, var c)
    // ParameterSymbol=x$1531: Argument:RefSymbol=HalfSize$1157:(0/1), Argument:RefSymbol=HalfSize$1157:(0/1)
    // Candidates = Interval
    // ParameterSymbol=c$1532: Argument:RefSymbol=Subtract$180:(0/2), Argument:RefSymbol=Add$178:(0/2)
    // Candidates = ScalarArithmetic,Arithmetic
    {
        return Tuple(Subtract(c, HalfSize(x)), Add(c, HalfSize(x)));
    }

    public static void Clamp(var x, var y)
    // ParameterSymbol=x$1534: Argument:RefSymbol=Clamp$1221:(0/2), Argument:RefSymbol=Clamp$1221:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=y$1535: Argument:RefSymbol=Min$1301:(0/1), Argument:RefSymbol=Max$1303:(0/1)
    // Candidates = Comparable
    {
        return Tuple(Clamp(x, Min(y)), Clamp(x, Max(y)));
    }

    public static void Clamp(var x, var value)
    // ParameterSymbol=x$1537: Argument:RefSymbol=Min$1301:(0/1), Argument:RefSymbol=Min$1301:(0/1), Argument:RefSymbol=Max$1303:(0/1), Argument:RefSymbol=Max$1303:(0/1)
    // Candidates = Comparable
    // ParameterSymbol=value$1538: Argument:RefSymbol=LessThan$1289:(0/2), Argument:RefSymbol=GreaterThan$1297:(0/2)
    // Candidates = Comparable
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
    // ParameterSymbol=x$1540: Argument:RefSymbol=Min$1301:(0/1), Argument:RefSymbol=Max$1303:(0/1)
    // Candidates = Comparable
    // ParameterSymbol=value$1541: Argument:RefSymbol=GreaterThanOrEquals$1299:(0/2), Argument:RefSymbol=LessThanOrEquals$1295:(0/2)
    // Candidates = Comparable
    {
        return GreaterThanOrEquals(value, And(Min(x), LessThanOrEquals(value, Max(x))));
    }

    public static void Unit()
    {
        return Tuple(0, 1);
    }

}
class Vector
{
    public static void Sum(var v)
    // ParameterSymbol=v$1544: Argument:RefSymbol=Aggregate$1325:(0/3)
    // Candidates = Array
    {
        return Aggregate(v, 0, Add);
    }

    public static void SumSquares(var v)
    // ParameterSymbol=v$1546: Argument:RefSymbol=Square$1215:(0/1)
    // Candidates = Numerical
    {
        return Aggregate(Square(v), 0, Add);
    }

    public static void LengthSquared(var v)
    // ParameterSymbol=v$1548: Argument:RefSymbol=SumSquares$1171:(0/1)
    // Candidates = Vector
    {
        return SumSquares(v);
    }

    public static void Length(var v)
    // ParameterSymbol=v$1550: Argument:RefSymbol=LengthSquared$1173:(0/1)
    // Candidates = Vector
    {
        return SquareRoot(LengthSquared(v));
    }

    public static void Dot(var v1, var v2)
    // ParameterSymbol=v1$1552: Argument:RefSymbol=Multiply$182:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // ParameterSymbol=v2$1553: Argument:RefSymbol=Multiply$182:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Sum(Multiply(v1, v2));
    }

}
class Numerical
{
    public static void Cos(var x)
    // ParameterSymbol=x$1555: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Sin(var x)
    // ParameterSymbol=x$1557: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Tan(var x)
    // ParameterSymbol=x$1559: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Acos(var x)
    // ParameterSymbol=x$1561: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Asin(var x)
    // ParameterSymbol=x$1563: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Atan(var x)
    // ParameterSymbol=x$1565: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Cosh(var x)
    // ParameterSymbol=x$1567: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Sinh(var x)
    // ParameterSymbol=x$1569: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Tanh(var x)
    // ParameterSymbol=x$1571: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Acosh(var x)
    // ParameterSymbol=x$1573: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Asinh(var x)
    // ParameterSymbol=x$1575: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Atanh(var x)
    // ParameterSymbol=x$1577: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Pow(var x, var y)
    // ParameterSymbol=x$1579: 
    // Candidates = Any
    // ParameterSymbol=y$1580: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Log(var x, var y)
    // ParameterSymbol=x$1582: 
    // Candidates = Any
    // ParameterSymbol=y$1583: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void NaturalLog(var x)
    // ParameterSymbol=x$1585: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void NaturalPower(var x)
    // ParameterSymbol=x$1587: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void SquareRoot(var x)
    // ParameterSymbol=x$1589: Argument:RefSymbol=Pow$1203:(0/2)
    // Candidates = Numerical
    {
        return Pow(x, 0.5);
    }

    public static void CubeRoot(var x)
    // ParameterSymbol=x$1591: Argument:RefSymbol=Pow$1203:(0/2)
    // Candidates = Numerical
    {
        return Pow(x, 0.5);
    }

    public static void Square(var x)
    // ParameterSymbol=x$1593: 
    // Candidates = Any
    {
        return Multiply(Value, Value);
    }

    public static void Clamp(var x, var min, var max)
    // ParameterSymbol=x$1595: Argument:RefSymbol=Clamp$1221:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=min$1596: Argument:RefSymbol=Interval$133:(0/2)
    // Candidates = Any
    // ParameterSymbol=max$1597: Argument:RefSymbol=Interval$133:(1/2)
    // Candidates = Any
    {
        return Clamp(x, Interval(min, max));
    }

    public static void Clamp(var x, var i)
    // ParameterSymbol=x$1599: Argument:RefSymbol=Clamp$1221:(1/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=i$1600: Argument:RefSymbol=Clamp$1221:(0/2)
    // Candidates = Interval,Numerical
    {
        return Clamp(i, x);
    }

    public static void Clamp(var x)
    // ParameterSymbol=x$1602: Argument:RefSymbol=Clamp$1221:(0/3)
    // Candidates = Interval,Numerical
    {
        return Clamp(x, 0, 1);
    }

    public static void PlusOne(var x)
    // ParameterSymbol=x$1604: Argument:RefSymbol=Add$178:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Add(x, 1);
    }

    public static void MinusOne(var x)
    // ParameterSymbol=x$1606: Argument:RefSymbol=Subtract$180:(0/2)
    // Candidates = ScalarArithmetic
    {
        return Subtract(x, 1);
    }

    public static void FromOne(var x)
    // ParameterSymbol=x$1608: Argument:RefSymbol=Subtract$180:(1/2)
    // Candidates = ScalarArithmetic
    {
        return Subtract(1, x);
    }

    public static void Sign(var x)
    // ParameterSymbol=x$1610: Argument:RefSymbol=LessThan$1289:(0/2), Argument:RefSymbol=GreaterThan$1297:(0/2)
    // Candidates = Comparable
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
    // ParameterSymbol=x$1612: 
    // Candidates = Any
    {
        return LessThan(Value, 0
            ? Negative(Value)
            : Value
        );
    }

    public static void Half(var x)
    // ParameterSymbol=x$1614: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 2);
    }

    public static void Third(var x)
    // ParameterSymbol=x$1616: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 3);
    }

    public static void Quarter(var x)
    // ParameterSymbol=x$1618: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 4);
    }

    public static void Fifth(var x)
    // ParameterSymbol=x$1620: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 5);
    }

    public static void Sixth(var x)
    // ParameterSymbol=x$1622: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 6);
    }

    public static void Seventh(var x)
    // ParameterSymbol=x$1624: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 7);
    }

    public static void Eighth(var x)
    // ParameterSymbol=x$1626: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 8);
    }

    public static void Ninth(var x)
    // ParameterSymbol=x$1628: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 9);
    }

    public static void Tenth(var x)
    // ParameterSymbol=x$1630: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 10);
    }

    public static void Sixteenth(var x)
    // ParameterSymbol=x$1632: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 16);
    }

    public static void Hundredth(var x)
    // ParameterSymbol=x$1634: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 100);
    }

    public static void Thousandth(var x)
    // ParameterSymbol=x$1636: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, 1000);
    }

    public static void Millionth(var x)
    // ParameterSymbol=x$1638: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, Divide(1000, 1000));
    }

    public static void Billionth(var x)
    // ParameterSymbol=x$1640: Argument:RefSymbol=Divide$184:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Divide(x, Divide(1000, Divide(1000, 1000)));
    }

    public static void Hundred(var x)
    // ParameterSymbol=x$1642: Argument:RefSymbol=Multiply$182:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Multiply(x, 100);
    }

    public static void Thousand(var x)
    // ParameterSymbol=x$1644: Argument:RefSymbol=Multiply$182:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Multiply(x, 1000);
    }

    public static void Million(var x)
    // ParameterSymbol=x$1646: Argument:RefSymbol=Multiply$182:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Multiply(x, Multiply(1000, 1000));
    }

    public static void Billion(var x)
    // ParameterSymbol=x$1648: Argument:RefSymbol=Multiply$182:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Multiply(x, Multiply(1000, Multiply(1000, 1000)));
    }

    public static void Twice(var x)
    // ParameterSymbol=x$1650: Argument:RefSymbol=Multiply$182:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Multiply(x, 2);
    }

    public static void Thrice(var x)
    // ParameterSymbol=x$1652: Argument:RefSymbol=Multiply$182:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Multiply(x, 3);
    }

    public static void SmoothStep(var x)
    // ParameterSymbol=x$1654: Argument:RefSymbol=Square$1215:(0/1), Argument:RefSymbol=Twice$1269:(0/1)
    // Candidates = Numerical
    {
        return Multiply(Square(x), Subtract(3, Twice(x)));
    }

    public static void Pow2(var x)
    // ParameterSymbol=x$1656: Argument:RefSymbol=Multiply$182:(0/2), Argument:RefSymbol=Multiply$182:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Multiply(x, x);
    }

    public static void Pow3(var x)
    // ParameterSymbol=x$1658: Argument:RefSymbol=Multiply$182:(1/2), Argument:RefSymbol=Pow2$1275:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    {
        return Multiply(Pow2(x), x);
    }

    public static void Pow4(var x)
    // ParameterSymbol=x$1660: Argument:RefSymbol=Multiply$182:(1/2), Argument:RefSymbol=Pow3$1277:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    {
        return Multiply(Pow3(x), x);
    }

    public static void Pow5(var x)
    // ParameterSymbol=x$1662: Argument:RefSymbol=Multiply$182:(1/2), Argument:RefSymbol=Pow4$1279:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    {
        return Multiply(Pow4(x), x);
    }

    public static void Turns(var x)
    // ParameterSymbol=x$1664: Argument:RefSymbol=Multiply$182:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    {
        return Multiply(x, Multiply(3.1415926535897, 2));
    }

    public static void AlmostZero(var x)
    // ParameterSymbol=x$1666: Argument:RefSymbol=Abs$1231:(0/1)
    // Candidates = Numerical
    {
        return LessThan(Abs(x), 1E-08);
    }

}
class Comparable
{
    public static void Equals(var a, var b)
    // ParameterSymbol=a$1668: Argument:RefSymbol=Compare$161:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1669: Argument:RefSymbol=Compare$161:(1/2)
    // Candidates = Comparable
    {
        return Equals(Compare(a, b), 0);
    }

    public static void LessThan(var a, var b)
    // ParameterSymbol=a$1671: Argument:RefSymbol=Compare$161:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1672: Argument:RefSymbol=Compare$161:(1/2)
    // Candidates = Comparable
    {
        return LessThan(Compare(a, b), 0);
    }

    public static void Lesser(var a, var b)
    // ParameterSymbol=a$1674: Argument:RefSymbol=LessThanOrEquals$1295:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1675: Argument:RefSymbol=LessThanOrEquals$1295:(1/2)
    // Candidates = Comparable
    {
        return LessThanOrEquals(a, b)
            ? a
            : b
        ;
    }

    public static void Greater(var a, var b)
    // ParameterSymbol=a$1677: Argument:RefSymbol=GreaterThanOrEquals$1299:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1678: Argument:RefSymbol=GreaterThanOrEquals$1299:(1/2)
    // Candidates = Comparable
    {
        return GreaterThanOrEquals(a, b)
            ? a
            : b
        ;
    }

    public static void LessThanOrEquals(var a, var b)
    // ParameterSymbol=a$1680: Argument:RefSymbol=Compare$161:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1681: Argument:RefSymbol=Compare$161:(1/2)
    // Candidates = Comparable
    {
        return LessThanOrEquals(Compare(a, b), 0);
    }

    public static void GreaterThan(var a, var b)
    // ParameterSymbol=a$1683: Argument:RefSymbol=Compare$161:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1684: Argument:RefSymbol=Compare$161:(1/2)
    // Candidates = Comparable
    {
        return GreaterThan(Compare(a, b), 0);
    }

    public static void GreaterThanOrEquals(var a, var b)
    // ParameterSymbol=a$1686: Argument:RefSymbol=Compare$161:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1687: Argument:RefSymbol=Compare$161:(1/2)
    // Candidates = Comparable
    {
        return GreaterThanOrEquals(Compare(a, b), 0);
    }

    public static void Min(var a, var b)
    // ParameterSymbol=a$1689: Argument:RefSymbol=LessThan$1289:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1690: Argument:RefSymbol=LessThan$1289:(1/2)
    // Candidates = Comparable
    {
        return LessThan(a, b)
            ? a
            : b
        ;
    }

    public static void Max(var a, var b)
    // ParameterSymbol=a$1692: Argument:RefSymbol=GreaterThan$1297:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1693: Argument:RefSymbol=GreaterThan$1297:(1/2)
    // Candidates = Comparable
    {
        return GreaterThan(a, b)
            ? a
            : b
        ;
    }

    public static void Between(var v, var a, var b)
    // ParameterSymbol=v$1695: Argument:RefSymbol=Between$1307:(0/2)
    // Candidates = Interval,Comparable
    // ParameterSymbol=a$1696: Argument:RefSymbol=Interval$133:(0/2)
    // Candidates = Any
    // ParameterSymbol=b$1697: Argument:RefSymbol=Interval$133:(1/2)
    // Candidates = Any
    {
        return Between(v, Interval(a, b));
    }

    public static void Between(var v, var i)
    // ParameterSymbol=v$1699: Argument:RefSymbol=Contains$1139:(1/2)
    // Candidates = Interval
    // ParameterSymbol=i$1700: Argument:RefSymbol=Contains$1139:(0/2)
    // Candidates = Interval
    {
        return Contains(i, v);
    }

}
class Boolean
{
    public static void XOr(var a, var b)
    // ParameterSymbol=a$1702: 
    // Candidates = Any
    // ParameterSymbol=b$1703: Argument:RefSymbol=Not$192:(0/1)
    // Candidates = Boolean
    {
        return a
            ? Not(b)
            : b
        ;
    }

    public static void NAnd(var a, var b)
    // ParameterSymbol=a$1705: Argument:RefSymbol=And$188:(0/2)
    // Candidates = Boolean
    // ParameterSymbol=b$1706: Argument:RefSymbol=And$188:(1/2)
    // Candidates = Boolean
    {
        return Not(And(a, b));
    }

    public static void NOr(var a, var b)
    // ParameterSymbol=a$1708: Argument:RefSymbol=Or$190:(0/2)
    // Candidates = Boolean
    // ParameterSymbol=b$1709: Argument:RefSymbol=Or$190:(1/2)
    // Candidates = Boolean
    {
        return Not(Or(a, b));
    }

}
class Equatable
{
    public static void NotEquals(var x)
    // ParameterSymbol=x$1711: Argument:RefSymbol=Equals$1287:(0/1)
    // Candidates = Equatable,Comparable
    {
        return Not(Equals(x));
    }

}
class Array
{
    public static void Map(var xs, var f)
    // ParameterSymbol=xs$1713: Argument:RefSymbol=Count$26:(0/1), Argument:RefSymbol=At$219:(0/2)
    // Candidates = Array
    // ParameterSymbol=f$1714: Invoked:(ArgumentSymbol)
    // Candidates = Function
    {
        return Map(Count(xs), (i) => 
        // ParameterSymbol=i$1715: Argument:RefSymbol=At$219:(1/2)
        // Candidates = Array
        f(At(xs, i)));
    }

    public static void Zip(var xs, var ys, var f)
    // ParameterSymbol=xs$1718: Argument:RefSymbol=Count$26:(0/1)
    // Candidates = Any
    // ParameterSymbol=ys$1719: Argument:RefSymbol=At$219:(0/2)
    // Candidates = Array
    // ParameterSymbol=f$1720: Invoked:(ArgumentSymbol,ArgumentSymbol)
    // Candidates = Function
    {
        return Array(Count(xs), (i) => 
        // ParameterSymbol=i$1721: Argument:RefSymbol=At$219:(0/1), Argument:RefSymbol=At$219:(1/2)
        // Candidates = Array
        f(At(i), At(ys, i)));
    }

    public static void Skip(var xs, var n)
    // ParameterSymbol=xs$1724: 
    // Candidates = Any
    // ParameterSymbol=n$1725: Argument:RefSymbol=Subtract$180:(1/2), Argument:RefSymbol=Subtract$180:(1/2)
    // Candidates = ScalarArithmetic
    {
        return Array(Subtract(Count, n), (i) => 
        // ParameterSymbol=i$1726: Argument:RefSymbol=Subtract$180:(0/2)
        // Candidates = ScalarArithmetic
        At(Subtract(i, n)));
    }

    public static void Take(var xs, var n)
    // ParameterSymbol=xs$1729: 
    // Candidates = Any
    // ParameterSymbol=n$1730: Argument:RefSymbol=Array$139:(0/2)
    // Candidates = Any
    {
        return Array(n, (i) => 
        // ParameterSymbol=i$1731: 
        // Candidates = Any
        At);
    }

    public static void Aggregate(var xs, var init, var f)
    // ParameterSymbol=xs$1734: Argument:RefSymbol=IsEmpty$1329:(0/1), Argument:RefSymbol=Rest$1327:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=init$1735: Argument:RefSymbol=f$1736:(0/2)
    // Candidates = Any
    // ParameterSymbol=f$1736: Invoked:(ArgumentSymbol,ArgumentSymbol), Invoked:(ArgumentSymbol)
    // Candidates = Function
    {
        return IsEmpty(xs)
            ? init
            : f(init, f(Rest(xs)))
        ;
    }

    public static void Rest(var xs)
    // ParameterSymbol=xs$1738: 
    // Candidates = Any
    {
        return Skip(1);
    }

    public static void IsEmpty(var xs)
    // ParameterSymbol=xs$1740: Argument:RefSymbol=Count$26:(0/1)
    // Candidates = Any
    {
        return Equals(Count(xs), 0);
    }

    public static void First(var xs)
    // ParameterSymbol=xs$1742: Argument:RefSymbol=At$219:(0/2)
    // Candidates = Array
    {
        return At(xs, 0);
    }

    public static void Last(var xs)
    // ParameterSymbol=xs$1744: Argument:RefSymbol=At$219:(0/2), Argument:RefSymbol=Count$26:(0/1)
    // Candidates = Array
    {
        return At(xs, Subtract(Count(xs), 1));
    }

    public static void Slice(var xs, var from, var count)
    // ParameterSymbol=xs$1746: Argument:RefSymbol=Skip$1321:(0/2)
    // Candidates = Array
    // ParameterSymbol=from$1747: Argument:RefSymbol=Skip$1321:(1/2)
    // Candidates = Array
    // ParameterSymbol=count$1748: Argument:RefSymbol=Take$1323:(1/2)
    // Candidates = Array
    {
        return Take(Skip(xs, from), count);
    }

    public static void Join(var xs, var sep)
    // ParameterSymbol=xs$1750: Argument:RefSymbol=IsEmpty$1329:(0/1), Argument:RefSymbol=First$1331:(0/1), Argument:RefSymbol=Skip$1321:(0/2)
    // Candidates = Interval,Array
    // ParameterSymbol=sep$1751: Argument:RefSymbol=Interpolate$1409:(1/3)
    // Candidates = Intrinsics
    {
        return IsEmpty(xs)
            ? 
            : Add(ToString(First(xs)), Aggregate(Skip(xs, 1), , (acc, cur) => 
            // ParameterSymbol=acc$1752: Argument:RefSymbol=Interpolate$1409:(0/3)
            // Candidates = Intrinsics
            // ParameterSymbol=cur$1753: Argument:RefSymbol=Interpolate$1409:(2/3)
            // Candidates = Intrinsics
            Interpolate(acc, sep, cur)))
        ;
    }

    public static void All(var xs, var f)
    // ParameterSymbol=xs$1756: Argument:RefSymbol=IsEmpty$1329:(0/1), Argument:RefSymbol=First$1331:(0/1), Argument:RefSymbol=Rest$1327:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=f$1757: Invoked:(ArgumentSymbol), Invoked:(ArgumentSymbol)
    // Candidates = Function
    {
        return IsEmpty(xs)
            ? True
            : And(f(First(xs)), f(Rest(xs)))
        ;
    }

    public static void JoinStrings(var xs, var sep)
    // ParameterSymbol=xs$1759: Argument:RefSymbol=IsEmpty$1329:(0/1), Argument:RefSymbol=First$1331:(0/1), Argument:RefSymbol=Rest$1327:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=sep$1760: 
    // Candidates = Any
    {
        return IsEmpty(xs)
            ? 
            : Add(First(xs), Aggregate(Rest(xs), , (x, acc) => 
            // ParameterSymbol=x$1761: Argument:RefSymbol=ToString$212:(0/1)
            // Candidates = Value
            // ParameterSymbol=acc$1762: Argument:RefSymbol=Add$178:(0/2)
            // Candidates = Arithmetic,ScalarArithmetic
            Add(acc, Add(, , ToString(x)))))
        ;
    }

}
class Easings
{
    public static void BlendEaseFunc(var p, var easeIn, var easeOut)
    // ParameterSymbol=p$1765: Argument:RefSymbol=LessThan$1289:(0/2), Argument:RefSymbol=Multiply$182:(0/2), Argument:RefSymbol=Multiply$182:(0/2)
    // Candidates = Comparable,Arithmetic,ScalarArithmetic
    // ParameterSymbol=easeIn$1766: Invoked:(ArgumentSymbol)
    // Candidates = Function
    // ParameterSymbol=easeOut$1767: Invoked:(ArgumentSymbol)
    // Candidates = Function
    {
        return LessThan(p, 0.5
            ? Multiply(0.5, easeIn(Multiply(p, 2)))
            : Multiply(0.5, Add(easeOut(Multiply(p, Subtract(2, 1))), 0.5))
        );
    }

    public static void InvertEaseFunc(var p, var easeIn)
    // ParameterSymbol=p$1769: Argument:RefSymbol=Subtract$180:(1/2)
    // Candidates = ScalarArithmetic
    // ParameterSymbol=easeIn$1770: Invoked:(ArgumentSymbol)
    // Candidates = Function
    {
        return Subtract(1, easeIn(Subtract(1, p)));
    }

    public static void Linear(var p)
    // ParameterSymbol=p$1772: 
    // Candidates = Any
    {
        return p;
    }

    public static void QuadraticEaseIn(var p)
    // ParameterSymbol=p$1774: Argument:RefSymbol=Pow2$1275:(0/1)
    // Candidates = Numerical
    {
        return Pow2(p);
    }

    public static void QuadraticEaseOut(var p)
    // ParameterSymbol=p$1776: Argument:RefSymbol=InvertEaseFunc$1345:(0/2)
    // Candidates = Easings
    {
        return InvertEaseFunc(p, QuadraticEaseIn);
    }

    public static void QuadraticEaseInOut(var p)
    // ParameterSymbol=p$1778: Argument:RefSymbol=BlendEaseFunc$1343:(0/3)
    // Candidates = Easings
    {
        return BlendEaseFunc(p, QuadraticEaseIn, QuadraticEaseOut);
    }

    public static void CubicEaseIn(var p)
    // ParameterSymbol=p$1780: Argument:RefSymbol=Pow3$1277:(0/1)
    // Candidates = Numerical
    {
        return Pow3(p);
    }

    public static void CubicEaseOut(var p)
    // ParameterSymbol=p$1782: Argument:RefSymbol=InvertEaseFunc$1345:(0/2)
    // Candidates = Easings
    {
        return InvertEaseFunc(p, CubicEaseIn);
    }

    public static void CubicEaseInOut(var p)
    // ParameterSymbol=p$1784: Argument:RefSymbol=BlendEaseFunc$1343:(0/3)
    // Candidates = Easings
    {
        return BlendEaseFunc(p, CubicEaseIn, CubicEaseOut);
    }

    public static void QuarticEaseIn(var p)
    // ParameterSymbol=p$1786: Argument:RefSymbol=Pow4$1279:(0/1)
    // Candidates = Numerical
    {
        return Pow4(p);
    }

    public static void QuarticEaseOut(var p)
    // ParameterSymbol=p$1788: Argument:RefSymbol=InvertEaseFunc$1345:(0/2)
    // Candidates = Easings
    {
        return InvertEaseFunc(p, QuarticEaseIn);
    }

    public static void QuarticEaseInOut(var p)
    // ParameterSymbol=p$1790: Argument:RefSymbol=BlendEaseFunc$1343:(0/3)
    // Candidates = Easings
    {
        return BlendEaseFunc(p, QuarticEaseIn, QuarticEaseOut);
    }

    public static void QuinticEaseIn(var p)
    // ParameterSymbol=p$1792: Argument:RefSymbol=Pow5$1281:(0/1)
    // Candidates = Numerical
    {
        return Pow5(p);
    }

    public static void QuinticEaseOut(var p)
    // ParameterSymbol=p$1794: Argument:RefSymbol=InvertEaseFunc$1345:(0/2)
    // Candidates = Easings
    {
        return InvertEaseFunc(p, QuinticEaseIn);
    }

    public static void QuinticEaseInOut(var p)
    // ParameterSymbol=p$1796: Argument:RefSymbol=BlendEaseFunc$1343:(0/3)
    // Candidates = Easings
    {
        return BlendEaseFunc(p, QuinticEaseIn, QuinticEaseOut);
    }

    public static void SineEaseIn(var p)
    // ParameterSymbol=p$1798: Argument:RefSymbol=InvertEaseFunc$1345:(0/2)
    // Candidates = Easings
    {
        return InvertEaseFunc(p, SineEaseOut);
    }

    public static void SineEaseOut(var p)
    // ParameterSymbol=p$1800: Argument:RefSymbol=Quarter$1237:(0/1)
    // Candidates = Numerical
    {
        return Sin(Turns(Quarter(p)));
    }

    public static void SineEaseInOut(var p)
    // ParameterSymbol=p$1802: Argument:RefSymbol=BlendEaseFunc$1343:(0/3)
    // Candidates = Easings
    {
        return BlendEaseFunc(p, SineEaseIn, SineEaseOut);
    }

    public static void CircularEaseIn(var p)
    // ParameterSymbol=p$1804: Argument:RefSymbol=Pow2$1275:(0/1)
    // Candidates = Numerical
    {
        return FromOne(SquareRoot(FromOne(Pow2(p))));
    }

    public static void CircularEaseOut(var p)
    // ParameterSymbol=p$1806: Argument:RefSymbol=InvertEaseFunc$1345:(0/2)
    // Candidates = Easings
    {
        return InvertEaseFunc(p, CircularEaseIn);
    }

    public static void CircularEaseInOut(var p)
    // ParameterSymbol=p$1808: Argument:RefSymbol=BlendEaseFunc$1343:(0/3)
    // Candidates = Easings
    {
        return BlendEaseFunc(p, CircularEaseIn, CircularEaseOut);
    }

    public static void ExponentialEaseIn(var p)
    // ParameterSymbol=p$1810: Argument:RefSymbol=AlmostZero$1285:(0/1), Argument:RefSymbol=MinusOne$1225:(0/1)
    // Candidates = Numerical
    {
        return AlmostZero(p)
            ? p
            : Pow(2, Multiply(10, MinusOne(p)))
        ;
    }

    public static void ExponentialEaseOut(var p)
    // ParameterSymbol=p$1812: Argument:RefSymbol=InvertEaseFunc$1345:(0/2)
    // Candidates = Easings
    {
        return InvertEaseFunc(p, ExponentialEaseIn);
    }

    public static void ExponentialEaseInOut(var p)
    // ParameterSymbol=p$1814: Argument:RefSymbol=BlendEaseFunc$1343:(0/3)
    // Candidates = Easings
    {
        return BlendEaseFunc(p, ExponentialEaseIn, ExponentialEaseOut);
    }

    public static void ElasticEaseIn(var p)
    // ParameterSymbol=p$1816: Argument:RefSymbol=Quarter$1237:(0/1), Argument:RefSymbol=MinusOne$1225:(0/1)
    // Candidates = Numerical
    {
        return Multiply(13, Multiply(Turns(Quarter(p)), Sin(Radians(Pow(2, Multiply(10, MinusOne(p)))))));
    }

    public static void ElasticEaseOut(var p)
    // ParameterSymbol=p$1818: Argument:RefSymbol=InvertEaseFunc$1345:(0/2)
    // Candidates = Easings
    {
        return InvertEaseFunc(p, ElasticEaseIn);
    }

    public static void ElasticEaseInOut(var p)
    // ParameterSymbol=p$1820: Argument:RefSymbol=BlendEaseFunc$1343:(0/3)
    // Candidates = Easings
    {
        return BlendEaseFunc(p, ElasticEaseIn, ElasticEaseOut);
    }

    public static void BackEaseIn(var p)
    // ParameterSymbol=p$1822: Argument:RefSymbol=Pow3$1277:(0/1), Argument:RefSymbol=Multiply$182:(0/2), Argument:RefSymbol=Half$1233:(0/1)
    // Candidates = Numerical,Arithmetic,ScalarArithmetic
    {
        return Subtract(Pow3(p), Multiply(p, Sin(Turns(Half(p)))));
    }

    public static void BackEaseOut(var p)
    // ParameterSymbol=p$1824: Argument:RefSymbol=InvertEaseFunc$1345:(0/2)
    // Candidates = Easings
    {
        return InvertEaseFunc(p, BackEaseIn);
    }

    public static void BackEaseInOut(var p)
    // ParameterSymbol=p$1826: Argument:RefSymbol=BlendEaseFunc$1343:(0/3)
    // Candidates = Easings
    {
        return BlendEaseFunc(p, BackEaseIn, BackEaseOut);
    }

    public static void BounceEaseIn(var p)
    // ParameterSymbol=p$1828: Argument:RefSymbol=InvertEaseFunc$1345:(0/2)
    // Candidates = Easings
    {
        return InvertEaseFunc(p, BounceEaseOut);
    }

    public static void BounceEaseOut(var p)
    // ParameterSymbol=p$1830: Argument:RefSymbol=LessThan$1289:(0/2), Argument:RefSymbol=Pow2$1275:(0/1), Argument:RefSymbol=LessThan$1289:(0/2), Argument:RefSymbol=Pow2$1275:(0/1), Argument:RefSymbol=Add$178:(0/2), Argument:RefSymbol=LessThan$1289:(0/2), Argument:RefSymbol=Pow2$1275:(0/1), Argument:RefSymbol=Add$178:(0/2), Argument:RefSymbol=Pow2$1275:(0/1), Argument:RefSymbol=Add$178:(0/2)
    // Candidates = Comparable,Numerical,Arithmetic,ScalarArithmetic
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
    // ParameterSymbol=p$1832: Argument:RefSymbol=BlendEaseFunc$1343:(0/3)
    // Candidates = Easings
    {
        return BlendEaseFunc(p, BounceEaseIn, BounceEaseOut);
    }

}
class Intrinsics
{
    public static void Interpolate(var xs)
    // ParameterSymbol=xs$1834: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void Throw(var x)
    // ParameterSymbol=x$1836: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void TypeOf(var x)
    // ParameterSymbol=x$1838: 
    // Candidates = Any
    {
        return intrinsic;
    }

    public static void New(var x)
    // ParameterSymbol=x$1840: 
    // Candidates = Any
    {
        return intrinsic;
    }

}
