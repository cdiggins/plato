interface Vector<Self> where Self : Vector<Self>
{
    public static Count Count(Vector v)
    // ParameterSymbol=v$1411:Vector Declared:Vector
    // Candidates = Vector
    Count(FieldTypes(Self))
    public static T At(Vector v, Index n)
    // ParameterSymbol=v$1413:Vector Declared:Vector, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Vector
    // ParameterSymbol=n$1414:Index Declared:Index, Argument:Ref=>FunctionGroupSymbol=At$213:(1/2)
    // Candidates = Index
    At(FieldValues(v), n)
}
interface Measure<Self> where Self : Measure<Self>
{
    public static Number Value(Self x)
    // ParameterSymbol=x$1416:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    At(FieldValues(x), 0)
}
interface Numerical<Self> where Self : Numerical<Self>
{
}
interface Magnitude<Self> where Self : Magnitude<Self>
{
    public static Number Magnitude(Self x)
    // ParameterSymbol=x$1418:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    SquareRoot(Sum(Square(FieldValues(x))))
}
interface Comparable<Self> where Self : Comparable<Self>
{
    public static Integer Compare(Self a, Self b)
    // ParameterSymbol=a$1420:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=Magnitude$150:(0/1), Argument:Ref=>FunctionGroupSymbol=Magnitude$150:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$1421:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=Magnitude$150:(0/1), Argument:Ref=>FunctionGroupSymbol=Magnitude$150:(0/1)
    // Candidates = Self
    LessThan(Magnitude(a), Magnitude(b)
        ? Negative(1)
        : GreaterThan(Magnitude(a), Magnitude(b)
            ? 1
            : 0
        )
    )
}
interface Equatable<Self> where Self : Equatable<Self>
{
    public static Boolean Equals(Self a, Self b)
    // ParameterSymbol=a$1423:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$1424:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    All(Equals(FieldValues(a), FieldValues(b)))
}
interface Arithmetic<Self> where Self : Arithmetic<Self>
{
    public static Self Add(Self self, Self other)
    // ParameterSymbol=self$1426:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1427:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    Add(FieldValues(self), FieldValues(other))
    public static Self Negative(Self self)
    // ParameterSymbol=self$1429:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    Negative(FieldValues(self))
    public static Self Reciprocal(Self self)
    // ParameterSymbol=self$1431:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    Reciprocal(FieldValues(self))
    public static Self Multiply(Self self, Self other)
    // ParameterSymbol=self$1433:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1434:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    Add(FieldValues(self), FieldValues(other))
    public static Self Divide(Self self, Self other)
    // ParameterSymbol=self$1436:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1437:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    Divide(FieldValues(self), FieldValues(other))
    public static Self Modulo(Self self, Self other)
    // ParameterSymbol=self$1439:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$1440:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    Modulo(FieldValues(self), FieldValues(other))
}
interface ScalarArithmetic<Self> where Self : ScalarArithmetic<Self>
{
    public static Self Add(Self self, T scalar)
    // ParameterSymbol=self$1443:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$1444:T Declared:T, Argument:Ref=>FunctionGroupSymbol=Add$169:(1/2)
    // Candidates = T
    Add(FieldValues(self), scalar)
    public static Self Subtract(Self self, T scalar)
    // ParameterSymbol=self$1446:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2)
    // Candidates = Self
    // ParameterSymbol=scalar$1447:T Declared:T, Argument:Ref=>FunctionGroupSymbol=Negative$158:(0/1)
    // Candidates = T
    Add(self, Negative(scalar))
    public static Self Multiply(Self self, T scalar)
    // ParameterSymbol=self$1449:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$1450:T Declared:T, Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2)
    // Candidates = T
    Multiply(FieldValues(self), scalar)
    public static Self Divide(Self self, T scalar)
    // ParameterSymbol=self$1452:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Self
    // ParameterSymbol=scalar$1453:T Declared:T, Argument:Ref=>FunctionGroupSymbol=Reciprocal$160:(0/1)
    // Candidates = T
    Multiply(self, Reciprocal(scalar))
    public static Self Modulo(Self self, T scalar)
    // ParameterSymbol=self$1455:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$1456:T Declared:T, Argument:Ref=>FunctionGroupSymbol=Modulo$177:(1/2)
    // Candidates = T
    Modulo(FieldValues(self), scalar)
}
interface Boolean<Self> where Self : Boolean<Self>
{
    public static Self And(Self a, Self b)
    // ParameterSymbol=a$1458:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$1459:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    And(FieldValues(a), FieldValues(b))
    public static void Or(Self a, Self b)
    // ParameterSymbol=a$1461:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$1462:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    Or(FieldValues(a), FieldValues(b))
    public static Self Not(Self a)
    // ParameterSymbol=a$1464:Self Declared:Self, Argument:Ref=>FunctionGroupSymbol=FieldValues$191:(0/1)
    // Candidates = Self
    Not(FieldValues(a))
}
interface Value<Self> where Self : Value<Self>
{
    public static void Type()
    intrinsic
    public static Array FieldTypes()
    intrinsic
    public static Array FieldNames()
    intrinsic
    public static Array FieldValues(Self self)
    // ParameterSymbol=self$1469:Self Declared:Self
    // Candidates = Self
    intrinsic
    public static Self Zero()
    Zero(FieldTypes)
    public static Self One()
    One(FieldTypes)
    public static Self Default()
    Default(FieldTypes)
    public static Self MinValue()
    MinValue(FieldTypes)
    public static Self MaxValue()
    MaxValue(FieldTypes)
    public static void ToString(Self x)
    // ParameterSymbol=x$1476:Self Declared:Self
    // Candidates = Self
    JoinStrings(FieldValues, ,)
}
interface Interval<Self> where Self : Interval<Self>
{
    public static T Min(Self x)
    // ParameterSymbol=x$1479:Self Declared:Self
    // Candidates = Self
    null
    public static T Max(Self x)
    // ParameterSymbol=x$1481:Self Declared:Self
    // Candidates = Self
    null
}
interface Array<Self> where Self : Array<Self>
{
    public static Count Count(Self xs)
    // ParameterSymbol=xs$1484:Self Declared:Self
    // Candidates = Self
    null
    public static T At(Self xs, Index n)
    // ParameterSymbol=xs$1486:Self Declared:Self
    // Candidates = Self
    // ParameterSymbol=n$1487:Index Declared:Index
    // Candidates = Index
    null
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
    // ParameterSymbol=x$1489: Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1)
    // Candidates = Interval,Comparable
    Subtract(Max(x), Min(x))
    public static void IsEmpty(var x)
    // ParameterSymbol=x$1491: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    GreaterThanOrEquals(Min(x), Max(x))
    public static void Lerp(var x, var amount)
    // ParameterSymbol=x$1493: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=amount$1494: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(1/2), Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2)
    // Candidates = ScalarArithmetic,Arithmetic
    Multiply(Min(x), Add(Subtract(1, amount), Multiply(Max(x), amount)))
    public static void InverseLerp(var x, var value)
    // ParameterSymbol=x$1496: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Size$1115:(0/1)
    // Candidates = Interval,Comparable,Interval
    // ParameterSymbol=value$1497: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(0/2)
    // Candidates = ScalarArithmetic
    Divide(Subtract(value, Min(x)), Size(x))
    public static void Negate(var x)
    // ParameterSymbol=x$1499: Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1)
    // Candidates = Interval,Comparable
    Tuple(Negative(Max(x)), Negative(Min(x)))
    public static void Reverse(var x)
    // ParameterSymbol=x$1501: Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1)
    // Candidates = Interval,Comparable
    Tuple(Max(x), Min(x))
    public static void Resize(var x, var size)
    // ParameterSymbol=x$1503: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=size$1504: Argument:Ref=>FunctionGroupSymbol=Add$169:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Tuple(Min(x), Add(Min(x), size))
    public static void Center(var x)
    // ParameterSymbol=x$1506: Argument:Ref=>FunctionGroupSymbol=Lerp$1119:(0/2)
    // Candidates = Interval
    Lerp(x, 0.5)
    public static void Contains(var x, var value)
    // ParameterSymbol=x$1508: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=value$1509: Argument:Ref=>FunctionGroupSymbol=And$179:(0/2), Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$1289:(0/2)
    // Candidates = Boolean,Comparable
    LessThanOrEquals(Min(x), And(value, LessThanOrEquals(value, Max(x))))
    public static void Contains(var x, var other)
    // ParameterSymbol=x$1511: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=other$1512: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    LessThanOrEquals(Min(x), And(Min(other), GreaterThanOrEquals(Max, Max(other))))
    public static void Overlaps(var x, var y)
    // ParameterSymbol=x$1514: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=y$1515: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(1/2)
    // Candidates = Interval,Numerical
    Not(IsEmpty(Clamp(x, y)))
    public static void Split(var x, var t)
    // ParameterSymbol=x$1517: Argument:Ref=>FunctionGroupSymbol=Left$1141:(0/2), Argument:Ref=>FunctionGroupSymbol=Right$1143:(0/2)
    // Candidates = Interval
    // ParameterSymbol=t$1518: Argument:Ref=>FunctionGroupSymbol=Left$1141:(1/2), Argument:Ref=>FunctionGroupSymbol=Right$1143:(1/2)
    // Candidates = Interval
    Tuple(Left(x, t), Right(x, t))
    public static void Split(var x)
    // ParameterSymbol=x$1520: Argument:Ref=>FunctionGroupSymbol=Split$1139:(0/2)
    // Candidates = Interval
    Split(x, 0.5)
    public static void Left(var x, var t)
    // ParameterSymbol=x$1522: Argument:Ref=>FunctionGroupSymbol=Lerp$1119:(0/2)
    // Candidates = Interval
    // ParameterSymbol=t$1523: Argument:Ref=>FunctionGroupSymbol=Lerp$1119:(1/2)
    // Candidates = Interval
    Tuple(Min, Lerp(x, t))
    public static void Right(var x, var t)
    // ParameterSymbol=x$1525: Argument:Ref=>FunctionGroupSymbol=Lerp$1119:(0/2), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Interval,Comparable
    // ParameterSymbol=t$1526: Argument:Ref=>FunctionGroupSymbol=Lerp$1119:(1/2)
    // Candidates = Interval
    Tuple(Lerp(x, t), Max(x))
    public static void MoveTo(var x, var t)
    // ParameterSymbol=x$1528: Argument:Ref=>FunctionGroupSymbol=Size$1115:(0/1)
    // Candidates = Interval
    // ParameterSymbol=t$1529: Argument:Ref=>PredefinedSymbol=Tuple$1:(0/2), Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Tuple(t, Add(t, Size(x)))
    public static void LeftHalf(var x)
    // ParameterSymbol=x$1531: Argument:Ref=>FunctionGroupSymbol=Left$1141:(0/2)
    // Candidates = Interval
    Left(x, 0.5)
    public static void RightHalf(var x)
    // ParameterSymbol=x$1533: Argument:Ref=>FunctionGroupSymbol=Right$1143:(0/2)
    // Candidates = Interval
    Right(x, 0.5)
    public static void HalfSize(var x)
    // ParameterSymbol=x$1535: Argument:Ref=>FunctionGroupSymbol=Size$1115:(0/1)
    // Candidates = Interval
    Half(Size(x))
    public static void Recenter(var x, var c)
    // ParameterSymbol=x$1537: Argument:Ref=>FunctionGroupSymbol=HalfSize$1151:(0/1), Argument:Ref=>FunctionGroupSymbol=HalfSize$1151:(0/1)
    // Candidates = Interval
    // ParameterSymbol=c$1538: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(0/2), Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2)
    // Candidates = ScalarArithmetic,Arithmetic
    Tuple(Subtract(c, HalfSize(x)), Add(c, HalfSize(x)))
    public static void Clamp(var x, var y)
    // ParameterSymbol=x$1540: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/2), Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=y$1541: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    Tuple(Clamp(x, Min(y)), Clamp(x, Max(y)))
    public static void Clamp(var x, var value)
    // ParameterSymbol=x$1543: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=value$1544: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=GreaterThan$1291:(0/2)
    // Candidates = Comparable
    LessThan(value, Min(x)
        ? Min(x)
        : GreaterThan(value, Max(x)
            ? Max(x)
            : value
        )
    )
    public static void Between(var x, var value)
    // ParameterSymbol=x$1546: Argument:Ref=>FunctionGroupSymbol=Min$1295:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$1297:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=value$1547: Argument:Ref=>FunctionGroupSymbol=GreaterThanOrEquals$1293:(0/2), Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$1289:(0/2)
    // Candidates = Comparable
    GreaterThanOrEquals(value, And(Min(x), LessThanOrEquals(value, Max(x))))
    public static void Unit()
    Tuple(0, 1)
}
class Vector
{
    public static void Sum(var v)
    // ParameterSymbol=v$1550: Argument:Ref=>FunctionGroupSymbol=Aggregate$1319:(0/3)
    // Candidates = Array
    Aggregate(v, 0, Add)
    public static void SumSquares(var v)
    // ParameterSymbol=v$1552: Argument:Ref=>FunctionGroupSymbol=Square$1209:(0/1)
    // Candidates = Numerical
    Aggregate(Square(v), 0, Add)
    public static void LengthSquared(var v)
    // ParameterSymbol=v$1554: Argument:Ref=>FunctionGroupSymbol=SumSquares$1165:(0/1)
    // Candidates = Vector
    SumSquares(v)
    public static void Length(var v)
    // ParameterSymbol=v$1556: Argument:Ref=>FunctionGroupSymbol=LengthSquared$1167:(0/1)
    // Candidates = Vector
    SquareRoot(LengthSquared(v))
    public static void Dot(var v1, var v2)
    // ParameterSymbol=v1$1558: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // ParameterSymbol=v2$1559: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Sum(Multiply(v1, v2))
}
class Numerical
{
    public static void Cos(var x)
    // ParameterSymbol=x$1561: 
    // Candidates = Any
    intrinsic
    public static void Sin(var x)
    // ParameterSymbol=x$1563: 
    // Candidates = Any
    intrinsic
    public static void Tan(var x)
    // ParameterSymbol=x$1565: 
    // Candidates = Any
    intrinsic
    public static void Acos(var x)
    // ParameterSymbol=x$1567: 
    // Candidates = Any
    intrinsic
    public static void Asin(var x)
    // ParameterSymbol=x$1569: 
    // Candidates = Any
    intrinsic
    public static void Atan(var x)
    // ParameterSymbol=x$1571: 
    // Candidates = Any
    intrinsic
    public static void Cosh(var x)
    // ParameterSymbol=x$1573: 
    // Candidates = Any
    intrinsic
    public static void Sinh(var x)
    // ParameterSymbol=x$1575: 
    // Candidates = Any
    intrinsic
    public static void Tanh(var x)
    // ParameterSymbol=x$1577: 
    // Candidates = Any
    intrinsic
    public static void Acosh(var x)
    // ParameterSymbol=x$1579: 
    // Candidates = Any
    intrinsic
    public static void Asinh(var x)
    // ParameterSymbol=x$1581: 
    // Candidates = Any
    intrinsic
    public static void Atanh(var x)
    // ParameterSymbol=x$1583: 
    // Candidates = Any
    intrinsic
    public static void Pow(var x, var y)
    // ParameterSymbol=x$1585: 
    // Candidates = Any
    // ParameterSymbol=y$1586: 
    // Candidates = Any
    intrinsic
    public static void Log(var x, var y)
    // ParameterSymbol=x$1588: 
    // Candidates = Any
    // ParameterSymbol=y$1589: 
    // Candidates = Any
    intrinsic
    public static void NaturalLog(var x)
    // ParameterSymbol=x$1591: 
    // Candidates = Any
    intrinsic
    public static void NaturalPower(var x)
    // ParameterSymbol=x$1593: 
    // Candidates = Any
    intrinsic
    public static void SquareRoot(var x)
    // ParameterSymbol=x$1595: Argument:Ref=>FunctionGroupSymbol=Pow$1197:(0/2)
    // Candidates = Numerical
    Pow(x, 0.5)
    public static void CubeRoot(var x)
    // ParameterSymbol=x$1597: Argument:Ref=>FunctionGroupSymbol=Pow$1197:(0/2)
    // Candidates = Numerical
    Pow(x, 0.5)
    public static void Square(var x)
    // ParameterSymbol=x$1599: 
    // Candidates = Any
    Multiply(Value, Value)
    public static void Clamp(var x, var min, var max)
    // ParameterSymbol=x$1601: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=min$1602: Argument:Ref=>TypeDefSymbol=Interval$133:(0/2)
    // Candidates = Any
    // ParameterSymbol=max$1603: Argument:Ref=>TypeDefSymbol=Interval$133:(1/2)
    // Candidates = Any
    Clamp(x, Interval(min, max))
    public static void Clamp(var x, var i)
    // ParameterSymbol=x$1605: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(1/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=i$1606: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/2)
    // Candidates = Interval,Numerical
    Clamp(i, x)
    public static void Clamp(var x)
    // ParameterSymbol=x$1608: Argument:Ref=>FunctionGroupSymbol=Clamp$1215:(0/3)
    // Candidates = Interval,Numerical
    Clamp(x, 0, 1)
    public static void PlusOne(var x)
    // ParameterSymbol=x$1610: Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Add(x, 1)
    public static void MinusOne(var x)
    // ParameterSymbol=x$1612: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(0/2)
    // Candidates = ScalarArithmetic
    Subtract(x, 1)
    public static void FromOne(var x)
    // ParameterSymbol=x$1614: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(1/2)
    // Candidates = ScalarArithmetic
    Subtract(1, x)
    public static void Sign(var x)
    // ParameterSymbol=x$1616: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=GreaterThan$1291:(0/2)
    // Candidates = Comparable
    LessThan(x, 0
        ? Negative(1)
        : GreaterThan(x, 0
            ? 1
            : 0
        )
    )
    public static void Abs(var x)
    // ParameterSymbol=x$1618: 
    // Candidates = Any
    LessThan(Value, 0
        ? Negative(Value)
        : Value
    )
    public static void Half(var x)
    // ParameterSymbol=x$1620: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 2)
    public static void Third(var x)
    // ParameterSymbol=x$1622: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 3)
    public static void Quarter(var x)
    // ParameterSymbol=x$1624: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 4)
    public static void Fifth(var x)
    // ParameterSymbol=x$1626: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 5)
    public static void Sixth(var x)
    // ParameterSymbol=x$1628: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 6)
    public static void Seventh(var x)
    // ParameterSymbol=x$1630: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 7)
    public static void Eighth(var x)
    // ParameterSymbol=x$1632: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 8)
    public static void Ninth(var x)
    // ParameterSymbol=x$1634: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 9)
    public static void Tenth(var x)
    // ParameterSymbol=x$1636: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 10)
    public static void Sixteenth(var x)
    // ParameterSymbol=x$1638: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 16)
    public static void Hundredth(var x)
    // ParameterSymbol=x$1640: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 100)
    public static void Thousandth(var x)
    // ParameterSymbol=x$1642: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 1000)
    public static void Millionth(var x)
    // ParameterSymbol=x$1644: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, Divide(1000, 1000))
    public static void Billionth(var x)
    // ParameterSymbol=x$1646: Argument:Ref=>FunctionGroupSymbol=Divide$175:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, Divide(1000, Divide(1000, 1000)))
    public static void Hundred(var x)
    // ParameterSymbol=x$1648: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, 100)
    public static void Thousand(var x)
    // ParameterSymbol=x$1650: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, 1000)
    public static void Million(var x)
    // ParameterSymbol=x$1652: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, Multiply(1000, 1000))
    public static void Billion(var x)
    // ParameterSymbol=x$1654: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, Multiply(1000, Multiply(1000, 1000)))
    public static void Twice(var x)
    // ParameterSymbol=x$1656: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, 2)
    public static void Thrice(var x)
    // ParameterSymbol=x$1658: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, 3)
    public static void SmoothStep(var x)
    // ParameterSymbol=x$1660: Argument:Ref=>FunctionGroupSymbol=Square$1209:(0/1), Argument:Ref=>FunctionGroupSymbol=Twice$1263:(0/1)
    // Candidates = Numerical
    Multiply(Square(x), Subtract(3, Twice(x)))
    public static void Pow2(var x)
    // ParameterSymbol=x$1662: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2), Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, x)
    public static void Pow3(var x)
    // ParameterSymbol=x$1664: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    Multiply(Pow2(x), x)
    public static void Pow4(var x)
    // ParameterSymbol=x$1666: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2), Argument:Ref=>FunctionGroupSymbol=Pow3$1271:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    Multiply(Pow3(x), x)
    public static void Pow5(var x)
    // ParameterSymbol=x$1668: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(1/2), Argument:Ref=>FunctionGroupSymbol=Pow4$1273:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    Multiply(Pow4(x), x)
    public static void Turns(var x)
    // ParameterSymbol=x$1670: Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, Multiply(3.1415926535897, 2))
    public static void AlmostZero(var x)
    // ParameterSymbol=x$1672: Argument:Ref=>FunctionGroupSymbol=Abs$1225:(0/1)
    // Candidates = Numerical
    LessThan(Abs(x), 1E-08)
}
class Comparable
{
    public static void Equals(var a, var b)
    // ParameterSymbol=a$1674: Argument:Ref=>FunctionGroupSymbol=Compare$152:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1675: Argument:Ref=>FunctionGroupSymbol=Compare$152:(1/2)
    // Candidates = Comparable
    Compare(a, b)
    public static void LessThan(var a, var b)
    // ParameterSymbol=a$1677: Argument:Ref=>FunctionGroupSymbol=Compare$152:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1678: Argument:Ref=>FunctionGroupSymbol=Compare$152:(1/2)
    // Candidates = Comparable
    LessThan(Compare(a, b), 0)
    public static void Lesser(var a, var b)
    // ParameterSymbol=a$1680: Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$1289:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1681: Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$1289:(1/2)
    // Candidates = Comparable
    LessThanOrEquals(a, b)
        ? a
        : b

    public static void Greater(var a, var b)
    // ParameterSymbol=a$1683: Argument:Ref=>FunctionGroupSymbol=GreaterThanOrEquals$1293:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1684: Argument:Ref=>FunctionGroupSymbol=GreaterThanOrEquals$1293:(1/2)
    // Candidates = Comparable
    GreaterThanOrEquals(a, b)
        ? a
        : b

    public static void LessThanOrEquals(var a, var b)
    // ParameterSymbol=a$1686: Argument:Ref=>FunctionGroupSymbol=Compare$152:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1687: Argument:Ref=>FunctionGroupSymbol=Compare$152:(1/2)
    // Candidates = Comparable
    LessThanOrEquals(Compare(a, b), 0)
    public static void GreaterThan(var a, var b)
    // ParameterSymbol=a$1689: Argument:Ref=>FunctionGroupSymbol=Compare$152:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1690: Argument:Ref=>FunctionGroupSymbol=Compare$152:(1/2)
    // Candidates = Comparable
    GreaterThan(Compare(a, b), 0)
    public static void GreaterThanOrEquals(var a, var b)
    // ParameterSymbol=a$1692: Argument:Ref=>FunctionGroupSymbol=Compare$152:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1693: Argument:Ref=>FunctionGroupSymbol=Compare$152:(1/2)
    // Candidates = Comparable
    GreaterThanOrEquals(Compare(a, b), 0)
    public static void Min(var a, var b)
    // ParameterSymbol=a$1695: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1696: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(1/2)
    // Candidates = Comparable
    LessThan(a, b)
        ? a
        : b

    public static void Max(var a, var b)
    // ParameterSymbol=a$1698: Argument:Ref=>FunctionGroupSymbol=GreaterThan$1291:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$1699: Argument:Ref=>FunctionGroupSymbol=GreaterThan$1291:(1/2)
    // Candidates = Comparable
    GreaterThan(a, b)
        ? a
        : b

    public static void Between(var v, var a, var b)
    // ParameterSymbol=v$1701: Argument:Ref=>FunctionGroupSymbol=Between$1301:(0/2)
    // Candidates = Interval,Comparable
    // ParameterSymbol=a$1702: Argument:Ref=>TypeDefSymbol=Interval$133:(0/2)
    // Candidates = Any
    // ParameterSymbol=b$1703: Argument:Ref=>TypeDefSymbol=Interval$133:(1/2)
    // Candidates = Any
    Between(v, Interval(a, b))
    public static void Between(var v, var i)
    // ParameterSymbol=v$1705: Argument:Ref=>FunctionGroupSymbol=Contains$1133:(1/2)
    // Candidates = Interval
    // ParameterSymbol=i$1706: Argument:Ref=>FunctionGroupSymbol=Contains$1133:(0/2)
    // Candidates = Interval
    Contains(i, v)
}
class Boolean
{
    public static void XOr(var a, var b)
    // ParameterSymbol=a$1708: 
    // Candidates = Any
    // ParameterSymbol=b$1709: Argument:Ref=>FunctionGroupSymbol=Not$183:(0/1)
    // Candidates = Boolean
    a
        ? Not(b)
        : b

    public static void NAnd(var a, var b)
    // ParameterSymbol=a$1711: Argument:Ref=>FunctionGroupSymbol=And$179:(0/2)
    // Candidates = Boolean
    // ParameterSymbol=b$1712: Argument:Ref=>FunctionGroupSymbol=And$179:(1/2)
    // Candidates = Boolean
    Not(And(a, b))
    public static void NOr(var a, var b)
    // ParameterSymbol=a$1714: Argument:Ref=>FunctionGroupSymbol=Or$181:(0/2)
    // Candidates = Boolean
    // ParameterSymbol=b$1715: Argument:Ref=>FunctionGroupSymbol=Or$181:(1/2)
    // Candidates = Boolean
    Not(Or(a, b))
}
class Equatable
{
    public static void NotEquals(var x)
    // ParameterSymbol=x$1717: Argument:Ref=>FunctionGroupSymbol=Equals$1281:(0/1)
    // Candidates = Equatable,Comparable
    Not(Equals(x))
}
class Array
{
    public static void Map(var xs, var f)
    // ParameterSymbol=xs$1719: Argument:Ref=>TypeDefSymbol=Count$26:(0/1), Argument:Ref=>FunctionGroupSymbol=At$213:(0/2)
    // Candidates = Vector,Array
    // ParameterSymbol=f$1720: Invoked:(ArgumentSymbol)
    // Candidates = Function
    Map(Count(xs), (i) => 
    // ParameterSymbol=i$1721: Argument:Ref=>FunctionGroupSymbol=At$213:(1/2)
    // Candidates = Vector,Array
    f(At(xs, i)))
    public static void Zip(var xs, var ys, var f)
    // ParameterSymbol=xs$1724: Argument:Ref=>TypeDefSymbol=Count$26:(0/1)
    // Candidates = Vector,Array
    // ParameterSymbol=ys$1725: Argument:Ref=>FunctionGroupSymbol=At$213:(0/2)
    // Candidates = Vector,Array
    // ParameterSymbol=f$1726: Invoked:(ArgumentSymbol,ArgumentSymbol)
    // Candidates = Function
    Array(Count(xs), (i) => 
    // ParameterSymbol=i$1727: Argument:Ref=>FunctionGroupSymbol=At$213:(0/1), Argument:Ref=>FunctionGroupSymbol=At$213:(1/2)
    // Candidates = Vector,Array
    f(At(i), At(ys, i)))
    public static void Skip(var xs, var n)
    // ParameterSymbol=xs$1730: 
    // Candidates = Any
    // ParameterSymbol=n$1731: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(1/2), Argument:Ref=>FunctionGroupSymbol=Subtract$171:(1/2)
    // Candidates = ScalarArithmetic
    Array(Subtract(Count, n), (i) => 
    // ParameterSymbol=i$1732: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(0/2)
    // Candidates = ScalarArithmetic
    At(Subtract(i, n)))
    public static void Take(var xs, var n)
    // ParameterSymbol=xs$1735: 
    // Candidates = Any
    // ParameterSymbol=n$1736: Argument:Ref=>TypeDefSymbol=Array$139:(0/2)
    // Candidates = Any
    Array(n, (i) => 
    // ParameterSymbol=i$1737: 
    // Candidates = Any
    At)
    public static void Aggregate(var xs, var init, var f)
    // ParameterSymbol=xs$1740: Argument:Ref=>FunctionGroupSymbol=IsEmpty$1323:(0/1), Argument:Ref=>FunctionGroupSymbol=Rest$1321:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=init$1741: Argument:Ref=>ParameterSymbol=f$1742:(0/2)
    // Candidates = Any
    // ParameterSymbol=f$1742: Invoked:(ArgumentSymbol,ArgumentSymbol), Invoked:(ArgumentSymbol)
    // Candidates = Function
    IsEmpty(xs)
        ? init
        : f(init, f(Rest(xs)))

    public static void Rest(var xs)
    // ParameterSymbol=xs$1744: 
    // Candidates = Any
    Skip(1)
    public static void IsEmpty(var xs)
    // ParameterSymbol=xs$1746: Argument:Ref=>TypeDefSymbol=Count$26:(0/1)
    // Candidates = Vector,Array
    Equals(Count(xs), 0)
    public static void First(var xs)
    // ParameterSymbol=xs$1748: Argument:Ref=>FunctionGroupSymbol=At$213:(0/2)
    // Candidates = Vector,Array
    At(xs, 0)
    public static void Last(var xs)
    // ParameterSymbol=xs$1750: Argument:Ref=>FunctionGroupSymbol=At$213:(0/2), Argument:Ref=>TypeDefSymbol=Count$26:(0/1)
    // Candidates = Vector,Array
    At(xs, Subtract(Count(xs), 1))
    public static void Slice(var xs, var from, var count)
    // ParameterSymbol=xs$1752: Argument:Ref=>FunctionGroupSymbol=Skip$1315:(0/2)
    // Candidates = Array
    // ParameterSymbol=from$1753: Argument:Ref=>FunctionGroupSymbol=Skip$1315:(1/2)
    // Candidates = Array
    // ParameterSymbol=count$1754: Argument:Ref=>FunctionGroupSymbol=Take$1317:(1/2)
    // Candidates = Array
    Take(Skip(xs, from), count)
    public static void Join(var xs, var sep)
    // ParameterSymbol=xs$1756: Argument:Ref=>FunctionGroupSymbol=IsEmpty$1323:(0/1), Argument:Ref=>FunctionGroupSymbol=First$1325:(0/1), Argument:Ref=>FunctionGroupSymbol=Skip$1315:(0/2)
    // Candidates = Interval,Array
    // ParameterSymbol=sep$1757: 
    // Candidates = Any
    IsEmpty(xs)
        ? 
        : Add(ToString(First(xs)), Aggregate(Skip(xs, 1)))

    public static void All(var xs, var f)
    // ParameterSymbol=xs$1759: Argument:Ref=>FunctionGroupSymbol=IsEmpty$1323:(0/1), Argument:Ref=>FunctionGroupSymbol=First$1325:(0/1), Argument:Ref=>FunctionGroupSymbol=Rest$1321:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=f$1760: Invoked:(ArgumentSymbol), Invoked:(ArgumentSymbol)
    // Candidates = Function
    IsEmpty(xs)
        ? True
        : And(f(First(xs)), f(Rest(xs)))

    public static void JoinStrings(var xs, var sep)
    // ParameterSymbol=xs$1762: Argument:Ref=>FunctionGroupSymbol=IsEmpty$1323:(0/1), Argument:Ref=>FunctionGroupSymbol=First$1325:(0/1), Argument:Ref=>FunctionGroupSymbol=Rest$1321:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=sep$1763: 
    // Candidates = Any
    IsEmpty(xs)
        ? 
        : Add(First(xs), Aggregate(Rest(xs)))

}
class Easings
{
    public static void BlendEaseFunc(var p, var easeIn, var easeOut)
    // ParameterSymbol=p$1765: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2), Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2)
    // Candidates = Comparable,Arithmetic,ScalarArithmetic
    // ParameterSymbol=easeIn$1766: Invoked:(ArgumentSymbol)
    // Candidates = Function
    // ParameterSymbol=easeOut$1767: Invoked:(ArgumentSymbol)
    // Candidates = Function
    LessThan(p, 0.5
        ? Multiply(0.5, easeIn(Multiply(p, 2)))
        : Multiply(0.5, Add(easeOut(Multiply(p, Subtract(2, 1))), 0.5))
    )
    public static void InvertEaseFunc(var p, var easeIn)
    // ParameterSymbol=p$1769: Argument:Ref=>FunctionGroupSymbol=Subtract$171:(1/2)
    // Candidates = ScalarArithmetic
    // ParameterSymbol=easeIn$1770: Invoked:(ArgumentSymbol)
    // Candidates = Function
    Subtract(1, easeIn(Subtract(1, p)))
    public static void Linear(var p)
    // ParameterSymbol=p$1772: 
    // Candidates = Any
    p
    public static void QuadraticEaseIn(var p)
    // ParameterSymbol=p$1774: Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1)
    // Candidates = Numerical
    Pow2(p)
    public static void QuadraticEaseOut(var p)
    // ParameterSymbol=p$1776: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, QuadraticEaseIn)
    public static void QuadraticEaseInOut(var p)
    // ParameterSymbol=p$1778: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, QuadraticEaseIn, QuadraticEaseOut)
    public static void CubicEaseIn(var p)
    // ParameterSymbol=p$1780: Argument:Ref=>FunctionGroupSymbol=Pow3$1271:(0/1)
    // Candidates = Numerical
    Pow3(p)
    public static void CubicEaseOut(var p)
    // ParameterSymbol=p$1782: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, CubicEaseIn)
    public static void CubicEaseInOut(var p)
    // ParameterSymbol=p$1784: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, CubicEaseIn, CubicEaseOut)
    public static void QuarticEaseIn(var p)
    // ParameterSymbol=p$1786: Argument:Ref=>FunctionGroupSymbol=Pow4$1273:(0/1)
    // Candidates = Numerical
    Pow4(p)
    public static void QuarticEaseOut(var p)
    // ParameterSymbol=p$1788: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, QuarticEaseIn)
    public static void QuarticEaseInOut(var p)
    // ParameterSymbol=p$1790: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, QuarticEaseIn, QuarticEaseOut)
    public static void QuinticEaseIn(var p)
    // ParameterSymbol=p$1792: Argument:Ref=>FunctionGroupSymbol=Pow5$1275:(0/1)
    // Candidates = Numerical
    Pow5(p)
    public static void QuinticEaseOut(var p)
    // ParameterSymbol=p$1794: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, QuinticEaseIn)
    public static void QuinticEaseInOut(var p)
    // ParameterSymbol=p$1796: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, QuinticEaseIn, QuinticEaseOut)
    public static void SineEaseIn(var p)
    // ParameterSymbol=p$1798: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, SineEaseOut)
    public static void SineEaseOut(var p)
    // ParameterSymbol=p$1800: Argument:Ref=>FunctionGroupSymbol=Quarter$1231:(0/1)
    // Candidates = Numerical
    Sin(Turns(Quarter(p)))
    public static void SineEaseInOut(var p)
    // ParameterSymbol=p$1802: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, SineEaseIn, SineEaseOut)
    public static void CircularEaseIn(var p)
    // ParameterSymbol=p$1804: Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1)
    // Candidates = Numerical
    FromOne(SquareRoot(FromOne(Pow2(p))))
    public static void CircularEaseOut(var p)
    // ParameterSymbol=p$1806: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, CircularEaseIn)
    public static void CircularEaseInOut(var p)
    // ParameterSymbol=p$1808: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, CircularEaseIn, CircularEaseOut)
    public static void ExponentialEaseIn(var p)
    // ParameterSymbol=p$1810: Argument:Ref=>FunctionGroupSymbol=AlmostZero$1279:(0/1), Argument:Ref=>FunctionGroupSymbol=MinusOne$1219:(0/1)
    // Candidates = Numerical
    AlmostZero(p)
        ? p
        : Pow(2, Multiply(10, MinusOne(p)))

    public static void ExponentialEaseOut(var p)
    // ParameterSymbol=p$1812: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, ExponentialEaseIn)
    public static void ExponentialEaseInOut(var p)
    // ParameterSymbol=p$1814: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, ExponentialEaseIn, ExponentialEaseOut)
    public static void ElasticEaseIn(var p)
    // ParameterSymbol=p$1816: Argument:Ref=>FunctionGroupSymbol=Quarter$1231:(0/1), Argument:Ref=>FunctionGroupSymbol=MinusOne$1219:(0/1)
    // Candidates = Numerical
    Multiply(13, Multiply(Turns(Quarter(p)), Sin(Radians(Pow(2, Multiply(10, MinusOne(p)))))))
    public static void ElasticEaseOut(var p)
    // ParameterSymbol=p$1818: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, ElasticEaseIn)
    public static void ElasticEaseInOut(var p)
    // ParameterSymbol=p$1820: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, ElasticEaseIn, ElasticEaseOut)
    public static void BackEaseIn(var p)
    // ParameterSymbol=p$1822: Argument:Ref=>FunctionGroupSymbol=Pow3$1271:(0/1), Argument:Ref=>FunctionGroupSymbol=Multiply$173:(0/2), Argument:Ref=>FunctionGroupSymbol=Half$1227:(0/1)
    // Candidates = Numerical,Arithmetic,ScalarArithmetic
    Subtract(Pow3(p), Multiply(p, Sin(Turns(Half(p)))))
    public static void BackEaseOut(var p)
    // ParameterSymbol=p$1824: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, BackEaseIn)
    public static void BackEaseInOut(var p)
    // ParameterSymbol=p$1826: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, BackEaseIn, BackEaseOut)
    public static void BounceEaseIn(var p)
    // ParameterSymbol=p$1828: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$1339:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, BounceEaseOut)
    public static void BounceEaseOut(var p)
    // ParameterSymbol=p$1830: Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1), Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1), Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2), Argument:Ref=>FunctionGroupSymbol=LessThan$1283:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1), Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1269:(0/1), Argument:Ref=>FunctionGroupSymbol=Add$169:(0/2)
    // Candidates = Comparable,Numerical,Arithmetic,ScalarArithmetic
    LessThan(p, Divide(4, 11))
        ? Multiply(121, Divide(Pow2(p), 16))
        : LessThan(p, Divide(8, 11))
            ? Divide(363, Multiply(40, Subtract(Pow2(p), Divide(99, Multiply(10, Add(p, Divide(17, 5)))))))
            : LessThan(p, Divide(9, 10))
                ? Divide(4356, Multiply(361, Subtract(Pow2(p), Divide(35442, Multiply(1805, Add(p, Divide(16061, 1805)))))))
                : Divide(54, Multiply(5, Subtract(Pow2(p), Divide(513, Multiply(25, Add(p, Divide(268, 25)))))))



    public static void BounceEaseInOut(var p)
    // ParameterSymbol=p$1832: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$1337:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, BounceEaseIn, BounceEaseOut)
}
class Intrinsics
{
    public static void Interpolate(var xs)
    // ParameterSymbol=xs$1834: 
    // Candidates = Any
    intrinsic
    public static void Throw(var x)
    // ParameterSymbol=x$1836: 
    // Candidates = Any
    intrinsic
    public static void TypeOf(var x)
    // ParameterSymbol=x$1838: 
    // Candidates = Any
    intrinsic
    public static void New(var x)
    // ParameterSymbol=x$1840: 
    // Candidates = Any
    intrinsic
}
