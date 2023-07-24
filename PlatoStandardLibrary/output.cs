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
    // ParameterSymbol=x$1418:Number Declared:Number
    // Candidates = Number
    {
        return null;
    }

}
interface Magnitude<Self> where Self : Magnitude<Self>
{
    public static Number Magnitude(Self x)
    // ParameterSymbol=x$1420:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

}
interface Comparable<Self> where Self : Comparable<Self>
{
    public static Integer Compare(Self a, Self b)
    // ParameterSymbol=a$1422:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=b$1423:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

}
interface Equatable<Self> where Self : Equatable<Self>
{
    public static Boolean Equals(Self a, Self b)
    // ParameterSymbol=a$1425:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=b$1426:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

}
interface Arithmetic<Self> where Self : Arithmetic<Self>
{
    public static Self Add(Self self, Self other)
    // ParameterSymbol=self$1428:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=other$1429:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

    public static Self Negative(Self self)
    // ParameterSymbol=self$1431:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

    public static Self Reciprocal(Self self)
    // ParameterSymbol=self$1433:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

    public static Self Multiply(Self self, Self other)
    // ParameterSymbol=self$1435:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=other$1436:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

    public static Self Divide(Self self, Self other)
    // ParameterSymbol=self$1438:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=other$1439:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

    public static Self Modulo(Self self, Self other)
    // ParameterSymbol=self$1441:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=other$1442:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

}
interface ScalarArithmetic<Self> where Self : ScalarArithmetic<Self>
{
    public static Self Add(Self self, T scalar)
    // ParameterSymbol=self$1445:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=scalar$1446:T Declared:T
    // Candidates = T
    {
        return null;
    }

    public static Self Subtract(Self self, T scalar)
    // ParameterSymbol=self$1448:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=scalar$1449:T Declared:T
    // Candidates = T
    {
        return null;
    }

    public static Self Multiply(Self self, T scalar)
    // ParameterSymbol=self$1451:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=scalar$1452:T Declared:T
    // Candidates = T
    {
        return null;
    }

    public static Self Divide(Self self, T scalar)
    // ParameterSymbol=self$1454:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=scalar$1455:T Declared:T
    // Candidates = T
    {
        return null;
    }

    public static Self Modulo(Self self, T scalar)
    // ParameterSymbol=self$1457:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=scalar$1458:T Declared:T
    // Candidates = T
    {
        return null;
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
        return null;
    }

    public static void Or(Self a, Self b)
    // ParameterSymbol=a$1463:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=b$1464:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

    public static Self Not(Self a)
    // ParameterSymbol=a$1466:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

}
interface Value<Self> where Self : Value<Self>
{
    public static void Type()
    {
        return null;
    }

    public static Array FieldTypes()
    {
        return null;
    }

    public static Array FieldNames()
    {
        return null;
    }

    public static Array FieldValues(Self self)
    // ParameterSymbol=self$1471:Self Declared:Self
    // Candidates = Self
    {
        return null;
    }

    public static Self Zero()
    {
        return null;
    }

    public static Self One()
    {
        return null;
    }

    public static Self Default()
    {
        return null;
    }

    public static Self MinValue()
    {
        return null;
    }

    public static Self MaxValue()
    {
        return null;
    }

    public static void ToString(Self x)
    // ParameterSymbol=x$1478:Self Declared:Self
    // Candidates = Self
    {
        return null;
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
    // ParameterSymbol=x$1483: 
    // Candidates = Any
    {
        return null;
    }

    public static void IsEmpty(var x)
    // ParameterSymbol=x$1485: 
    // Candidates = Any
    {
        return null;
    }

    public static void Lerp(var x, var amount)
    // ParameterSymbol=x$1487: 
    // Candidates = Any
    // ParameterSymbol=amount$1488: 
    // Candidates = Any
    {
        return null;
    }

    public static void InverseLerp(var x, var value)
    // ParameterSymbol=x$1490: 
    // Candidates = Any
    // ParameterSymbol=value$1491: 
    // Candidates = Any
    {
        return null;
    }

    public static void Negate(var x)
    // ParameterSymbol=x$1493: 
    // Candidates = Any
    {
        return null;
    }

    public static void Reverse(var x)
    // ParameterSymbol=x$1495: 
    // Candidates = Any
    {
        return null;
    }

    public static void Resize(var x, var size)
    // ParameterSymbol=x$1497: 
    // Candidates = Any
    // ParameterSymbol=size$1498: 
    // Candidates = Any
    {
        return null;
    }

    public static void Center(var x)
    // ParameterSymbol=x$1500: 
    // Candidates = Any
    {
        return null;
    }

    public static void Contains(var x, var value)
    // ParameterSymbol=x$1502: 
    // Candidates = Any
    // ParameterSymbol=value$1503: 
    // Candidates = Any
    {
        return null;
    }

    public static void Contains(var x, var other)
    // ParameterSymbol=x$1505: 
    // Candidates = Any
    // ParameterSymbol=other$1506: 
    // Candidates = Any
    {
        return null;
    }

    public static void Overlaps(var x, var y)
    // ParameterSymbol=x$1508: 
    // Candidates = Any
    // ParameterSymbol=y$1509: 
    // Candidates = Any
    {
        return null;
    }

    public static void Split(var x, var t)
    // ParameterSymbol=x$1511: 
    // Candidates = Any
    // ParameterSymbol=t$1512: 
    // Candidates = Any
    {
        return null;
    }

    public static void Split(var x)
    // ParameterSymbol=x$1514: 
    // Candidates = Any
    {
        return null;
    }

    public static void Left(var x, var t)
    // ParameterSymbol=x$1516: 
    // Candidates = Any
    // ParameterSymbol=t$1517: 
    // Candidates = Any
    {
        return null;
    }

    public static void Right(var x, var t)
    // ParameterSymbol=x$1519: 
    // Candidates = Any
    // ParameterSymbol=t$1520: 
    // Candidates = Any
    {
        return null;
    }

    public static void MoveTo(var x, var t)
    // ParameterSymbol=x$1522: 
    // Candidates = Any
    // ParameterSymbol=t$1523: 
    // Candidates = Any
    {
        return null;
    }

    public static void LeftHalf(var x)
    // ParameterSymbol=x$1525: 
    // Candidates = Any
    {
        return null;
    }

    public static void RightHalf(var x)
    // ParameterSymbol=x$1527: 
    // Candidates = Any
    {
        return null;
    }

    public static void HalfSize(var x)
    // ParameterSymbol=x$1529: 
    // Candidates = Any
    {
        return null;
    }

    public static void Recenter(var x, var c)
    // ParameterSymbol=x$1531: 
    // Candidates = Any
    // ParameterSymbol=c$1532: 
    // Candidates = Any
    {
        return null;
    }

    public static void Clamp(var x, var y)
    // ParameterSymbol=x$1534: 
    // Candidates = Any
    // ParameterSymbol=y$1535: 
    // Candidates = Any
    {
        return null;
    }

    public static void Clamp(var x, var value)
    // ParameterSymbol=x$1537: 
    // Candidates = Any
    // ParameterSymbol=value$1538: 
    // Candidates = Any
    {
        return null;
    }

    public static void Between(var x, var value)
    // ParameterSymbol=x$1540: 
    // Candidates = Any
    // ParameterSymbol=value$1541: 
    // Candidates = Any
    {
        return null;
    }

    public static void Unit()
    {
        return null;
    }

}
class Vector
{
    public static void Sum(var v)
    // ParameterSymbol=v$1544: 
    // Candidates = Any
    {
        return null;
    }

    public static void SumSquares(var v)
    // ParameterSymbol=v$1546: 
    // Candidates = Any
    {
        return null;
    }

    public static void LengthSquared(var v)
    // ParameterSymbol=v$1548: 
    // Candidates = Any
    {
        return null;
    }

    public static void Length(var v)
    // ParameterSymbol=v$1550: 
    // Candidates = Any
    {
        return null;
    }

    public static void Dot(var v1, var v2)
    // ParameterSymbol=v1$1552: 
    // Candidates = Any
    // ParameterSymbol=v2$1553: 
    // Candidates = Any
    {
        return null;
    }

}
class Numerical
{
    public static void Cos(var x)
    // ParameterSymbol=x$1555: 
    // Candidates = Any
    {
        return null;
    }

    public static void Sin(var x)
    // ParameterSymbol=x$1557: 
    // Candidates = Any
    {
        return null;
    }

    public static void Tan(var x)
    // ParameterSymbol=x$1559: 
    // Candidates = Any
    {
        return null;
    }

    public static void Acos(var x)
    // ParameterSymbol=x$1561: 
    // Candidates = Any
    {
        return null;
    }

    public static void Asin(var x)
    // ParameterSymbol=x$1563: 
    // Candidates = Any
    {
        return null;
    }

    public static void Atan(var x)
    // ParameterSymbol=x$1565: 
    // Candidates = Any
    {
        return null;
    }

    public static void Cosh(var x)
    // ParameterSymbol=x$1567: 
    // Candidates = Any
    {
        return null;
    }

    public static void Sinh(var x)
    // ParameterSymbol=x$1569: 
    // Candidates = Any
    {
        return null;
    }

    public static void Tanh(var x)
    // ParameterSymbol=x$1571: 
    // Candidates = Any
    {
        return null;
    }

    public static void Acosh(var x)
    // ParameterSymbol=x$1573: 
    // Candidates = Any
    {
        return null;
    }

    public static void Asinh(var x)
    // ParameterSymbol=x$1575: 
    // Candidates = Any
    {
        return null;
    }

    public static void Atanh(var x)
    // ParameterSymbol=x$1577: 
    // Candidates = Any
    {
        return null;
    }

    public static void Pow(var x, var y)
    // ParameterSymbol=x$1579: 
    // Candidates = Any
    // ParameterSymbol=y$1580: 
    // Candidates = Any
    {
        return null;
    }

    public static void Log(var x, var y)
    // ParameterSymbol=x$1582: 
    // Candidates = Any
    // ParameterSymbol=y$1583: 
    // Candidates = Any
    {
        return null;
    }

    public static void NaturalLog(var x)
    // ParameterSymbol=x$1585: 
    // Candidates = Any
    {
        return null;
    }

    public static void NaturalPower(var x)
    // ParameterSymbol=x$1587: 
    // Candidates = Any
    {
        return null;
    }

    public static void SquareRoot(var x)
    // ParameterSymbol=x$1589: 
    // Candidates = Any
    {
        return null;
    }

    public static void CubeRoot(var x)
    // ParameterSymbol=x$1591: 
    // Candidates = Any
    {
        return null;
    }

    public static void Square(var x)
    // ParameterSymbol=x$1593: 
    // Candidates = Any
    {
        return null;
    }

    public static void Clamp(var x, var min, var max)
    // ParameterSymbol=x$1595: 
    // Candidates = Any
    // ParameterSymbol=min$1596: 
    // Candidates = Any
    // ParameterSymbol=max$1597: 
    // Candidates = Any
    {
        return null;
    }

    public static void Clamp(var x, var i)
    // ParameterSymbol=x$1599: 
    // Candidates = Any
    // ParameterSymbol=i$1600: 
    // Candidates = Any
    {
        return null;
    }

    public static void Clamp(var x)
    // ParameterSymbol=x$1602: 
    // Candidates = Any
    {
        return null;
    }

    public static void PlusOne(var x)
    // ParameterSymbol=x$1604: 
    // Candidates = Any
    {
        return null;
    }

    public static void MinusOne(var x)
    // ParameterSymbol=x$1606: 
    // Candidates = Any
    {
        return null;
    }

    public static void FromOne(var x)
    // ParameterSymbol=x$1608: 
    // Candidates = Any
    {
        return null;
    }

    public static void Sign(var x)
    // ParameterSymbol=x$1610: 
    // Candidates = Any
    {
        return null;
    }

    public static void Abs(var x)
    // ParameterSymbol=x$1612: 
    // Candidates = Any
    {
        return null;
    }

    public static void Half(var x)
    // ParameterSymbol=x$1614: 
    // Candidates = Any
    {
        return null;
    }

    public static void Third(var x)
    // ParameterSymbol=x$1616: 
    // Candidates = Any
    {
        return null;
    }

    public static void Quarter(var x)
    // ParameterSymbol=x$1618: 
    // Candidates = Any
    {
        return null;
    }

    public static void Fifth(var x)
    // ParameterSymbol=x$1620: 
    // Candidates = Any
    {
        return null;
    }

    public static void Sixth(var x)
    // ParameterSymbol=x$1622: 
    // Candidates = Any
    {
        return null;
    }

    public static void Seventh(var x)
    // ParameterSymbol=x$1624: 
    // Candidates = Any
    {
        return null;
    }

    public static void Eighth(var x)
    // ParameterSymbol=x$1626: 
    // Candidates = Any
    {
        return null;
    }

    public static void Ninth(var x)
    // ParameterSymbol=x$1628: 
    // Candidates = Any
    {
        return null;
    }

    public static void Tenth(var x)
    // ParameterSymbol=x$1630: 
    // Candidates = Any
    {
        return null;
    }

    public static void Sixteenth(var x)
    // ParameterSymbol=x$1632: 
    // Candidates = Any
    {
        return null;
    }

    public static void Hundredth(var x)
    // ParameterSymbol=x$1634: 
    // Candidates = Any
    {
        return null;
    }

    public static void Thousandth(var x)
    // ParameterSymbol=x$1636: 
    // Candidates = Any
    {
        return null;
    }

    public static void Millionth(var x)
    // ParameterSymbol=x$1638: 
    // Candidates = Any
    {
        return null;
    }

    public static void Billionth(var x)
    // ParameterSymbol=x$1640: 
    // Candidates = Any
    {
        return null;
    }

    public static void Hundred(var x)
    // ParameterSymbol=x$1642: 
    // Candidates = Any
    {
        return null;
    }

    public static void Thousand(var x)
    // ParameterSymbol=x$1644: 
    // Candidates = Any
    {
        return null;
    }

    public static void Million(var x)
    // ParameterSymbol=x$1646: 
    // Candidates = Any
    {
        return null;
    }

    public static void Billion(var x)
    // ParameterSymbol=x$1648: 
    // Candidates = Any
    {
        return null;
    }

    public static void Twice(var x)
    // ParameterSymbol=x$1650: 
    // Candidates = Any
    {
        return null;
    }

    public static void Thrice(var x)
    // ParameterSymbol=x$1652: 
    // Candidates = Any
    {
        return null;
    }

    public static void SmoothStep(var x)
    // ParameterSymbol=x$1654: 
    // Candidates = Any
    {
        return null;
    }

    public static void Pow2(var x)
    // ParameterSymbol=x$1656: 
    // Candidates = Any
    {
        return null;
    }

    public static void Pow3(var x)
    // ParameterSymbol=x$1658: 
    // Candidates = Any
    {
        return null;
    }

    public static void Pow4(var x)
    // ParameterSymbol=x$1660: 
    // Candidates = Any
    {
        return null;
    }

    public static void Pow5(var x)
    // ParameterSymbol=x$1662: 
    // Candidates = Any
    {
        return null;
    }

    public static void Turns(var x)
    // ParameterSymbol=x$1664: 
    // Candidates = Any
    {
        return null;
    }

    public static void AlmostZero(var x)
    // ParameterSymbol=x$1666: 
    // Candidates = Any
    {
        return null;
    }

}
class Comparable
{
    public static void Equals(var a, var b)
    // ParameterSymbol=a$1668: 
    // Candidates = Any
    // ParameterSymbol=b$1669: 
    // Candidates = Any
    {
        return null;
    }

    public static void LessThan(var a, var b)
    // ParameterSymbol=a$1671: 
    // Candidates = Any
    // ParameterSymbol=b$1672: 
    // Candidates = Any
    {
        return null;
    }

    public static void Lesser(var a, var b)
    // ParameterSymbol=a$1674: 
    // Candidates = Any
    // ParameterSymbol=b$1675: 
    // Candidates = Any
    {
        return null;
    }

    public static void Greater(var a, var b)
    // ParameterSymbol=a$1677: 
    // Candidates = Any
    // ParameterSymbol=b$1678: 
    // Candidates = Any
    {
        return null;
    }

    public static void LessThanOrEquals(var a, var b)
    // ParameterSymbol=a$1680: 
    // Candidates = Any
    // ParameterSymbol=b$1681: 
    // Candidates = Any
    {
        return null;
    }

    public static void GreaterThan(var a, var b)
    // ParameterSymbol=a$1683: 
    // Candidates = Any
    // ParameterSymbol=b$1684: 
    // Candidates = Any
    {
        return null;
    }

    public static void GreaterThanOrEquals(var a, var b)
    // ParameterSymbol=a$1686: 
    // Candidates = Any
    // ParameterSymbol=b$1687: 
    // Candidates = Any
    {
        return null;
    }

    public static void Min(var a, var b)
    // ParameterSymbol=a$1689: 
    // Candidates = Any
    // ParameterSymbol=b$1690: 
    // Candidates = Any
    {
        return null;
    }

    public static void Max(var a, var b)
    // ParameterSymbol=a$1692: 
    // Candidates = Any
    // ParameterSymbol=b$1693: 
    // Candidates = Any
    {
        return null;
    }

    public static void Between(var v, var a, var b)
    // ParameterSymbol=v$1695: 
    // Candidates = Any
    // ParameterSymbol=a$1696: 
    // Candidates = Any
    // ParameterSymbol=b$1697: 
    // Candidates = Any
    {
        return null;
    }

    public static void Between(var v, var i)
    // ParameterSymbol=v$1699: 
    // Candidates = Any
    // ParameterSymbol=i$1700: 
    // Candidates = Any
    {
        return null;
    }

}
class Boolean
{
    public static void XOr(var a, var b)
    // ParameterSymbol=a$1702: 
    // Candidates = Any
    // ParameterSymbol=b$1703: 
    // Candidates = Any
    {
        return null;
    }

    public static void NAnd(var a, var b)
    // ParameterSymbol=a$1705: 
    // Candidates = Any
    // ParameterSymbol=b$1706: 
    // Candidates = Any
    {
        return null;
    }

    public static void NOr(var a, var b)
    // ParameterSymbol=a$1708: 
    // Candidates = Any
    // ParameterSymbol=b$1709: 
    // Candidates = Any
    {
        return null;
    }

}
class Equatable
{
    public static void NotEquals(var x)
    // ParameterSymbol=x$1711: 
    // Candidates = Any
    {
        return null;
    }

}
class Array
{
    public static void Map(var xs, var f)
    // ParameterSymbol=xs$1713: 
    // Candidates = Any
    // ParameterSymbol=f$1714: 
    // Candidates = Any
    {
        return null;
    }

    public static void Zip(var xs, var ys, var f)
    // ParameterSymbol=xs$1718: 
    // Candidates = Any
    // ParameterSymbol=ys$1719: 
    // Candidates = Any
    // ParameterSymbol=f$1720: 
    // Candidates = Any
    {
        return null;
    }

    public static void Skip(var xs, var n)
    // ParameterSymbol=xs$1724: 
    // Candidates = Any
    // ParameterSymbol=n$1725: 
    // Candidates = Any
    {
        return null;
    }

    public static void Take(var xs, var n)
    // ParameterSymbol=xs$1729: 
    // Candidates = Any
    // ParameterSymbol=n$1730: 
    // Candidates = Any
    {
        return null;
    }

    public static void Aggregate(var xs, var init, var f)
    // ParameterSymbol=xs$1734: 
    // Candidates = Any
    // ParameterSymbol=init$1735: 
    // Candidates = Any
    // ParameterSymbol=f$1736: 
    // Candidates = Any
    {
        return null;
    }

    public static void Rest(var xs)
    // ParameterSymbol=xs$1738: 
    // Candidates = Any
    {
        return null;
    }

    public static void IsEmpty(var xs)
    // ParameterSymbol=xs$1740: 
    // Candidates = Any
    {
        return null;
    }

    public static void First(var xs)
    // ParameterSymbol=xs$1742: 
    // Candidates = Any
    {
        return null;
    }

    public static void Last(var xs)
    // ParameterSymbol=xs$1744: 
    // Candidates = Any
    {
        return null;
    }

    public static void Slice(var xs, var from, var count)
    // ParameterSymbol=xs$1746: 
    // Candidates = Any
    // ParameterSymbol=from$1747: 
    // Candidates = Any
    // ParameterSymbol=count$1748: 
    // Candidates = Any
    {
        return null;
    }

    public static void Join(var xs, var sep)
    // ParameterSymbol=xs$1750: 
    // Candidates = Any
    // ParameterSymbol=sep$1751: 
    // Candidates = Any
    {
        return null;
    }

    public static void All(var xs, var f)
    // ParameterSymbol=xs$1756: 
    // Candidates = Any
    // ParameterSymbol=f$1757: 
    // Candidates = Any
    {
        return null;
    }

    public static void JoinStrings(var xs, var sep)
    // ParameterSymbol=xs$1759: 
    // Candidates = Any
    // ParameterSymbol=sep$1760: 
    // Candidates = Any
    {
        return null;
    }

}
class Easings
{
    public static void BlendEaseFunc(var p, var easeIn, var easeOut)
    // ParameterSymbol=p$1765: 
    // Candidates = Any
    // ParameterSymbol=easeIn$1766: 
    // Candidates = Any
    // ParameterSymbol=easeOut$1767: 
    // Candidates = Any
    {
        return null;
    }

    public static void InvertEaseFunc(var p, var easeIn)
    // ParameterSymbol=p$1769: 
    // Candidates = Any
    // ParameterSymbol=easeIn$1770: 
    // Candidates = Any
    {
        return null;
    }

    public static void Linear(var p)
    // ParameterSymbol=p$1772: 
    // Candidates = Any
    {
        return null;
    }

    public static void QuadraticEaseIn(var p)
    // ParameterSymbol=p$1774: 
    // Candidates = Any
    {
        return null;
    }

    public static void QuadraticEaseOut(var p)
    // ParameterSymbol=p$1776: 
    // Candidates = Any
    {
        return null;
    }

    public static void QuadraticEaseInOut(var p)
    // ParameterSymbol=p$1778: 
    // Candidates = Any
    {
        return null;
    }

    public static void CubicEaseIn(var p)
    // ParameterSymbol=p$1780: 
    // Candidates = Any
    {
        return null;
    }

    public static void CubicEaseOut(var p)
    // ParameterSymbol=p$1782: 
    // Candidates = Any
    {
        return null;
    }

    public static void CubicEaseInOut(var p)
    // ParameterSymbol=p$1784: 
    // Candidates = Any
    {
        return null;
    }

    public static void QuarticEaseIn(var p)
    // ParameterSymbol=p$1786: 
    // Candidates = Any
    {
        return null;
    }

    public static void QuarticEaseOut(var p)
    // ParameterSymbol=p$1788: 
    // Candidates = Any
    {
        return null;
    }

    public static void QuarticEaseInOut(var p)
    // ParameterSymbol=p$1790: 
    // Candidates = Any
    {
        return null;
    }

    public static void QuinticEaseIn(var p)
    // ParameterSymbol=p$1792: 
    // Candidates = Any
    {
        return null;
    }

    public static void QuinticEaseOut(var p)
    // ParameterSymbol=p$1794: 
    // Candidates = Any
    {
        return null;
    }

    public static void QuinticEaseInOut(var p)
    // ParameterSymbol=p$1796: 
    // Candidates = Any
    {
        return null;
    }

    public static void SineEaseIn(var p)
    // ParameterSymbol=p$1798: 
    // Candidates = Any
    {
        return null;
    }

    public static void SineEaseOut(var p)
    // ParameterSymbol=p$1800: 
    // Candidates = Any
    {
        return null;
    }

    public static void SineEaseInOut(var p)
    // ParameterSymbol=p$1802: 
    // Candidates = Any
    {
        return null;
    }

    public static void CircularEaseIn(var p)
    // ParameterSymbol=p$1804: 
    // Candidates = Any
    {
        return null;
    }

    public static void CircularEaseOut(var p)
    // ParameterSymbol=p$1806: 
    // Candidates = Any
    {
        return null;
    }

    public static void CircularEaseInOut(var p)
    // ParameterSymbol=p$1808: 
    // Candidates = Any
    {
        return null;
    }

    public static void ExponentialEaseIn(var p)
    // ParameterSymbol=p$1810: 
    // Candidates = Any
    {
        return null;
    }

    public static void ExponentialEaseOut(var p)
    // ParameterSymbol=p$1812: 
    // Candidates = Any
    {
        return null;
    }

    public static void ExponentialEaseInOut(var p)
    // ParameterSymbol=p$1814: 
    // Candidates = Any
    {
        return null;
    }

    public static void ElasticEaseIn(var p)
    // ParameterSymbol=p$1816: 
    // Candidates = Any
    {
        return null;
    }

    public static void ElasticEaseOut(var p)
    // ParameterSymbol=p$1818: 
    // Candidates = Any
    {
        return null;
    }

    public static void ElasticEaseInOut(var p)
    // ParameterSymbol=p$1820: 
    // Candidates = Any
    {
        return null;
    }

    public static void BackEaseIn(var p)
    // ParameterSymbol=p$1822: 
    // Candidates = Any
    {
        return null;
    }

    public static void BackEaseOut(var p)
    // ParameterSymbol=p$1824: 
    // Candidates = Any
    {
        return null;
    }

    public static void BackEaseInOut(var p)
    // ParameterSymbol=p$1826: 
    // Candidates = Any
    {
        return null;
    }

    public static void BounceEaseIn(var p)
    // ParameterSymbol=p$1828: 
    // Candidates = Any
    {
        return null;
    }

    public static void BounceEaseOut(var p)
    // ParameterSymbol=p$1830: 
    // Candidates = Any
    {
        return null;
    }

    public static void BounceEaseInOut(var p)
    // ParameterSymbol=p$1832: 
    // Candidates = Any
    {
        return null;
    }

}
class Intrinsics
{
    public static void Interpolate(var xs)
    // ParameterSymbol=xs$1834: 
    // Candidates = Any
    {
        return null;
    }

    public static void Throw(var x)
    // ParameterSymbol=x$1836: 
    // Candidates = Any
    {
        return null;
    }

    public static void TypeOf(var x)
    // ParameterSymbol=x$1838: 
    // Candidates = Any
    {
        return null;
    }

    public static void New(var x)
    // ParameterSymbol=x$1840: 
    // Candidates = Any
    {
        return null;
    }

}
