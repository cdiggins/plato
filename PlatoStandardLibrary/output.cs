interface Vector<Self> where Self : Vector<Self>
{
    public static Count_27 Count(Vector_14 v)
    // ParameterSymbol=v$2125:Concept:Vector_14 Declared:Concept:Vector_14
    // Candidates = Vector
    Count(FieldTypes(Self))
    public static T_143 At(Vector_14 v, Index_28 n)
    // ParameterSymbol=v$2135:Concept:Vector_14 Declared:Concept:Vector_14, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Vector
    // ParameterSymbol=n$2137:Type:Index_28 Declared:Type:Index_28, Argument:Ref=>FunctionGroupSymbol=At$251:(1/2)
    // Candidates = Index
    At(FieldValues(v), n)
}
interface Measure<Self> where Self : Measure<Self>
{
    public static Number_29 Value(Self_7 x)
    // ParameterSymbol=x$2149:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    At(FieldValues(x), 0)
}
interface Numerical<Self> where Self : Numerical<Self>
{
}
interface Magnitude<Self> where Self : Magnitude<Self>
{
    public static Number_29 Magnitude(Self_7 x)
    // ParameterSymbol=x$2161:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    SquareRoot(Sum(Square(FieldValues(x))))
}
interface Comparable<Self> where Self : Comparable<Self>
{
    public static Integer_26 Compare(Self_7 a, Self_7 b)
    // ParameterSymbol=a$2177:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=Magnitude$155:(0/1), Argument:Ref=>FunctionGroupSymbol=Magnitude$155:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$2179:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=Magnitude$155:(0/1), Argument:Ref=>FunctionGroupSymbol=Magnitude$155:(0/1)
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
    public static Boolean_22 Equals(Self_7 a, Self_7 b)
    // ParameterSymbol=a$2214:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$2216:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    All(Equals(FieldValues(a), FieldValues(b)))
}
interface Arithmetic<Self> where Self : Arithmetic<Self>
{
    public static Self_7 Add(Self_7 self, Self_7 other)
    // ParameterSymbol=self$2234:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$2236:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    Add(FieldValues(self), FieldValues(other))
    public static Self_7 Negative(Self_7 self)
    // ParameterSymbol=self$2251:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    Negative(FieldValues(self))
    public static Self_7 Reciprocal(Self_7 self)
    // ParameterSymbol=self$2261:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    Reciprocal(FieldValues(self))
    public static Self_7 Multiply(Self_7 self, Self_7 other)
    // ParameterSymbol=self$2271:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$2273:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    Add(FieldValues(self), FieldValues(other))
    public static Self_7 Divide(Self_7 self, Self_7 other)
    // ParameterSymbol=self$2288:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$2290:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    Divide(FieldValues(self), FieldValues(other))
    public static Self_7 Modulo(Self_7 self, Self_7 other)
    // ParameterSymbol=self$2305:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    // ParameterSymbol=other$2307:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    Modulo(FieldValues(self), FieldValues(other))
}
interface ScalarArithmetic<Self> where Self : ScalarArithmetic<Self>
{
    public static Self_7 Add(Self_7 self, T_2321 scalar)
    // ParameterSymbol=self$2323:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$2325:Variable:T_2321 Declared:Variable:T_2321, Argument:Ref=>FunctionGroupSymbol=Add$183:(1/2)
    // Candidates = T
    Add(FieldValues(self), scalar)
    public static Self_7 Subtract(Self_7 self, T_2321 scalar)
    // ParameterSymbol=self$2337:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=Add$183:(0/2)
    // Candidates = Self
    // ParameterSymbol=scalar$2339:Variable:T_2321 Declared:Variable:T_2321, Argument:Ref=>FunctionGroupSymbol=Negative$167:(0/1)
    // Candidates = T
    Add(self, Negative(scalar))
    public static Self_7 Multiply(Self_7 self, T_2321 scalar)
    // ParameterSymbol=self$2351:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$2353:Variable:T_2321 Declared:Variable:T_2321, Argument:Ref=>FunctionGroupSymbol=Multiply$189:(1/2)
    // Candidates = T
    Multiply(FieldValues(self), scalar)
    public static Self_7 Divide(Self_7 self, T_2321 scalar)
    // ParameterSymbol=self$2365:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2)
    // Candidates = Self
    // ParameterSymbol=scalar$2367:Variable:T_2321 Declared:Variable:T_2321, Argument:Ref=>FunctionGroupSymbol=Reciprocal$170:(0/1)
    // Candidates = T
    Multiply(self, Reciprocal(scalar))
    public static Self_7 Modulo(Self_7 self, T_2321 scalar)
    // ParameterSymbol=self$2379:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    // ParameterSymbol=scalar$2381:Variable:T_2321 Declared:Variable:T_2321, Argument:Ref=>FunctionGroupSymbol=Modulo$195:(1/2)
    // Candidates = T
    Modulo(FieldValues(self), scalar)
}
interface Boolean<Self> where Self : Boolean<Self>
{
    public static Self_7 And(Self_7 a, Self_7 b)
    // ParameterSymbol=a$2393:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$2395:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    And(FieldValues(a), FieldValues(b))
    public static Self_7 Or(Self_7 a, Self_7 b)
    // ParameterSymbol=a$2410:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    // ParameterSymbol=b$2412:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    Or(FieldValues(a), FieldValues(b))
    public static Self_7 Not(Self_7 a)
    // ParameterSymbol=a$2427:Primitive:Self_7 Declared:Primitive:Self_7, Argument:Ref=>FunctionGroupSymbol=FieldValues$219:(0/1)
    // Candidates = Self
    Not(FieldValues(a))
}
interface Value<Self> where Self : Value<Self>
{
    public static Type_13 Type()
    intrinsic
    public static Array_25 FieldTypes()
    intrinsic
    public static Array_25 FieldNames()
    intrinsic
    public static Array_25 FieldValues(Self_7 self)
    // ParameterSymbol=self$2443:Primitive:Self_7 Declared:Primitive:Self_7
    // Candidates = Self
    intrinsic
    public static Self_7 Zero()
    Zero(FieldTypes)
    public static Self_7 One()
    One(FieldTypes)
    public static Self_7 Default()
    Default(FieldTypes)
    public static Self_7 MinValue()
    MinValue(FieldTypes)
    public static Self_7 MaxValue()
    MaxValue(FieldTypes)
    public static String_9 ToString(Self_7 x)
    // ParameterSymbol=x$2472:Primitive:Self_7 Declared:Primitive:Self_7
    // Candidates = Self
    JoinStrings(FieldValues, ,)
}
interface Interval<Self> where Self : Interval<Self>
{
    public static T_238 Min(Self_7 x)
    // ParameterSymbol=x$2482:Primitive:Self_7 Declared:Primitive:Self_7
    // Candidates = Self
    null
    public static T_238 Max(Self_7 x)
    // ParameterSymbol=x$2485:Primitive:Self_7 Declared:Primitive:Self_7
    // Candidates = Self
    null
}
interface Array<Self> where Self : Array<Self>
{
    public static Count_27 Count(Self_7 xs)
    // ParameterSymbol=xs$2489:Primitive:Self_7 Declared:Primitive:Self_7
    // Candidates = Self
    null
    public static T_245 At(Self_7 xs, Index_28 n)
    // ParameterSymbol=xs$2492:Primitive:Self_7 Declared:Primitive:Self_7
    // Candidates = Self
    // ParameterSymbol=n$2494:Type:Index_28 Declared:Type:Index_28
    // Candidates = Index
    null
}
class Integer
{
    Integer_26 Value;
}
class Count
{
    Integer_26 Value;
}
class Index
{
    Integer_26 Value;
}
class Number
{
    Float64_12 Value;
}
class Unit
{
    Number_29 Value;
}
class Percent
{
    Number_29 Value;
}
class Quaternion
{
    Number_29 X;
    Number_29 Y;
    Number_29 Z;
    Number_29 W;
}
class Unit2D
{
    Unit_30 X;
    Unit_30 Y;
}
class Unit3D
{
    Unit_30 X;
    Unit_30 Y;
    Unit_30 Z;
}
class Direction3D
{
    Unit3D_34 Value;
}
class AxisAngle
{
    Unit3D_34 Axis;
    Angle_83 Angle;
}
class EulerAngles
{
    Angle_83 Yaw;
    Angle_83 Pitch;
    Angle_83 Roll;
}
class Rotation3D
{
    Quaternion_32 Quaternion;
}
class Vector2D
{
    Number_29 X;
    Number_29 Y;
}
class Vector3D
{
    Number_29 X;
    Number_29 Y;
    Number_29 Z;
}
class Vector4D
{
    Number_29 X;
    Number_29 Y;
    Number_29 Z;
    Number_29 W;
}
class Orientation3D
{
    Rotation3D_38 Value;
}
class Pose2D
{
    Vector3D_40 Position;
    Orientation3D_42 Orientation;
}
class Pose3D
{
    Vector3D_40 Position;
    Orientation3D_42 Orientation;
}
class Transform3D
{
    Vector3D_40 Translation;
    Rotation3D_38 Rotation;
    Vector3D_40 Scale;
}
class Transform2D
{
    Vector2D_39 Translation;
    Angle_83 Rotation;
    Vector2D_39 Scale;
}
class AlignedBox2D
{
    Vector2D_39 A;
    Vector2D_39 B;
}
class AlignedBox3D
{
    Vector3D_40 A;
    Vector3D_40 B;
}
class Complex
{
    Number_29 Real;
    Number_29 Imaginary;
}
class Ray3D
{
    Vector3D_40 Direction;
    Point3D_58 Position;
}
class Ray2D
{
    Vector2D_39 Direction;
    Point2D_59 Position;
}
class Sphere
{
    Point3D_58 Center;
    Number_29 Radius;
}
class Plane
{
    Unit3D_34 Normal;
    Number_29 D;
}
class Triangle3D
{
    Point3D_58 A;
    Point3D_58 B;
    Point3D_58 C;
}
class Triangle2D
{
    Point2D_59 A;
    Point2D_59 B;
    Point2D_59 C;
}
class Quad3D
{
    Point3D_58 A;
    Point3D_58 B;
    Point3D_58 C;
    Point3D_58 D;
}
class Quad2D
{
    Point2D_59 A;
    Point2D_59 B;
    Point2D_59 C;
    Point2D_59 D;
}
class Point3D
{
    Vector3D_40 Value;
}
class Point2D
{
    Vector2D_39 Value;
}
class Line3D
{
    Point3D_58 A;
    Point3D_58 B;
}
class Line2D
{
    Point2D_59 A;
    Point2D_59 B;
}
class Color
{
    Unit_30 R;
    Unit_30 G;
    Unit_30 B;
    Unit_30 A;
}
class ColorLUV
{
    Percent_31 Lightness;
    Unit_30 U;
    Unit_30 V;
}
class ColorLAB
{
    Percent_31 Lightness;
    Integer_26 A;
    Integer_26 B;
}
class ColorLCh
{
    Percent_31 Lightness;
    PolarCoordinate_70 ChromaHue;
}
class ColorHSV
{
    Angle_83 Hue;
    Unit_30 S;
    Unit_30 V;
}
class ColorHSL
{
    Angle_83 Hue;
    Unit_30 Saturation;
    Unit_30 Luminance;
}
class ColorYCbCr
{
    Unit_30 Y;
    Unit_30 Cb;
    Unit_30 Cr;
}
class SphericalCoordinate
{
    Number_29 Radius;
    Angle_83 Azimuth;
    Angle_83 Polar;
}
class PolarCoordinate
{
    Number_29 Radius;
    Angle_83 Angle;
}
class LogPolarCoordinate
{
    Number_29 Rho;
    Angle_83 Azimuth;
}
class CylindricalCoordinate
{
    Number_29 RadialDistance;
    Angle_83 Azimuth;
    Number_29 Height;
}
class HorizontalCoordinate
{
    Number_29 Radius;
    Angle_83 Azimuth;
    Number_29 Height;
}
class GeoCoordinate
{
    Angle_83 Latitude;
    Angle_83 Longitude;
}
class GeoCoordinateWithAltitude
{
    GeoCoordinate_74 Coordinate;
    Number_29 Altitude;
}
class Circle
{
    Point2D_59 Center;
    Number_29 Radius;
}
class Chord
{
    Circle_76 Circle;
    Arc_92 Arc;
}
class Size2D
{
    Number_29 Width;
    Number_29 Height;
}
class Size3D
{
    Number_29 Width;
    Number_29 Height;
    Number_29 Depth;
}
class Rectangle2D
{
    Point2D_59 Center;
    Size2D_78 Size;
}
class Proportion
{
    Number_29 Value;
}
class Fraction
{
    Number_29 Numerator;
    Number_29 Denominator;
}
class Angle
{
    Number_29 Radians;
}
class Length
{
    Number_29 Meters;
}
class Mass
{
    Number_29 Kilograms;
}
class Temperature
{
    Number_29 Celsius;
}
class TimeSpan
{
    Number_29 Seconds;
}
class TimeRange
{
    DateTime_89 Min;
    DateTime_89 Max;
}
class DateTime
{
}
class AnglePair
{
    Angle_83 Start;
    Angle_83 End;
}
class Ring
{
    Circle_76 Circle;
    Number_29 InnerRadius;
}
class Arc
{
    AnglePair_90 Angles;
    Circle_76 Cirlce;
}
class TimeInterval
{
    TimeSpan_87 Start;
    TimeSpan_87 End;
}
class RealInterval
{
    Number_29 A;
    Number_29 B;
}
class Interval2D
{
    Vector2D_39 A;
    Vector2D_39 B;
}
class Interval3D
{
    Vector3D_40 A;
    Vector3D_40 B;
}
class Capsule
{
    Line3D_60 Line;
    Number_29 Radius;
}
class Matrix3D
{
    Vector4D_41 Column1;
    Vector4D_41 Column2;
    Vector4D_41 Column3;
    Vector4D_41 Column4;
}
class Cylinder
{
    Line3D_60 Line;
    Number_29 Radius;
}
class Cone
{
    Line3D_60 Line;
    Number_29 Radius;
}
class Tube
{
    Line3D_60 Line;
    Number_29 InnerRadius;
    Number_29 OuterRadius;
}
class ConeSegment
{
    Line3D_60 Line;
    Number_29 Radius1;
    Number_29 Radius2;
}
class Box2D
{
    Point2D_59 Center;
    Angle_83 Rotation;
    Size2D_78 Extent;
}
class Box3D
{
    Point3D_58 Center;
    Rotation3D_38 Rotation;
    Size3D_79 Extent;
}
class CubicBezierTriangle3D
{
    Point3D_58 A;
    Point3D_58 B;
    Point3D_58 C;
    Point3D_58 A2B;
    Point3D_58 AB2;
    Point3D_58 B2C;
    Point3D_58 BC2;
    Point3D_58 AC2;
    Point3D_58 A2C;
    Point3D_58 ABC;
}
class CubicBezier2D
{
    Point2D_59 A;
    Point2D_59 B;
    Point2D_59 C;
    Point2D_59 D;
}
class UV
{
    Unit_30 U;
    Unit_30 V;
}
class UVW
{
    Unit_30 U;
    Unit_30 V;
    Unit_30 W;
}
class CubicBezier3D
{
    Point3D_58 A;
    Point3D_58 B;
    Point3D_58 C;
    Point3D_58 D;
}
class QuadraticBezier2D
{
    Point2D_59 A;
    Point2D_59 B;
    Point2D_59 C;
}
class QuadraticBezier3D
{
    Point3D_58 A;
    Point3D_58 B;
    Point3D_58 C;
}
class Area
{
    Number_29 MetersSquared;
}
class Volume
{
    Number_29 MetersCubed;
}
class Velocity
{
    Number_29 MetersPerSecond;
}
class Acceleration
{
    Number_29 MetersPerSecondSquared;
}
class Force
{
    Number_29 Newtons;
}
class Pressure
{
    Number_29 Pascals;
}
class Energy
{
    Number_29 Joules;
}
class Memory
{
    Count_27 Bytes;
}
class Frequency
{
    Number_29 Hertz;
}
class Loudness
{
    Number_29 Decibels;
}
class LuminousIntensity
{
    Number_29 Candelas;
}
class ElectricPotential
{
    Number_29 Volts;
}
class ElectricCharge
{
    Number_29 Columbs;
}
class ElectricCurrent
{
    Number_29 Amperes;
}
class ElectricResistance
{
    Number_29 Ohms;
}
class Power
{
    Number_29 Watts;
}
class Density
{
    Number_29 KilogramsPerMeterCubed;
}
class NormalDistribution
{
    Number_29 Mean;
    Number_29 StandardDeviation;
}
class PoissonDistribution
{
    Number_29 Expected;
    Count_27 Occurrences;
}
class BernoulliDistribution
{
    Probability_132 P;
}
class Probability
{
    Number_29 Value;
}
class BinomialDistribution
{
    Count_27 Trials;
    Probability_132 P;
}
class Interval
{
    public static void Size(var x)
    // ParameterSymbol=x$2496: Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1)
    // Candidates = Interval,Comparable
    Subtract(Max(x), Min(x))
    public static void IsEmpty(var x)
    // ParameterSymbol=x$2510: Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1)
    // Candidates = Interval,Comparable
    GreaterThanOrEquals(Min(x), Max(x))
    public static void Lerp(var x, var amount)
    // ParameterSymbol=x$2524: Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=amount$2525: Argument:Ref=>FunctionGroupSymbol=Subtract$186:(1/2), Argument:Ref=>FunctionGroupSymbol=Multiply$189:(1/2)
    // Candidates = ScalarArithmetic,Arithmetic
    Multiply(Min(x), Add(Subtract(1, amount), Multiply(Max(x), amount)))
    public static void InverseLerp(var x, var value)
    // ParameterSymbol=x$2554: Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1), Argument:Ref=>FunctionGroupSymbol=Size$1828:(0/1)
    // Candidates = Interval,Comparable,Interval
    // ParameterSymbol=value$2555: Argument:Ref=>FunctionGroupSymbol=Subtract$186:(0/2)
    // Candidates = ScalarArithmetic
    Divide(Subtract(value, Min(x)), Size(x))
    public static void Negate(var x)
    // ParameterSymbol=x$2574: Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1)
    // Candidates = Interval,Comparable
    Tuple(Negative(Max(x)), Negative(Min(x)))
    public static void Reverse(var x)
    // ParameterSymbol=x$2594: Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1)
    // Candidates = Interval,Comparable
    Tuple(Max(x), Min(x))
    public static void Resize(var x, var size)
    // ParameterSymbol=x$2608: Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=size$2609: Argument:Ref=>FunctionGroupSymbol=Add$183:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Tuple(Min(x), Add(Min(x), size))
    public static void Center(var x)
    // ParameterSymbol=x$2628: Argument:Ref=>FunctionGroupSymbol=Lerp$1832:(0/2)
    // Candidates = Interval
    Lerp(x, 0.5)
    public static void Contains(var x, var value)
    // ParameterSymbol=x$2636: Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=value$2637: Argument:Ref=>FunctionGroupSymbol=And$198:(0/2), Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$2002:(0/2)
    // Candidates = Boolean,Comparable
    LessThanOrEquals(Min(x), And(value, LessThanOrEquals(value, Max(x))))
    public static void Contains(var x, var other)
    // ParameterSymbol=x$2661: Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=other$2662: Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1)
    // Candidates = Interval,Comparable
    LessThanOrEquals(Min(x), And(Min(other), GreaterThanOrEquals(Max, Max(other))))
    public static void Overlaps(var x, var y)
    // ParameterSymbol=x$2689: Argument:Ref=>FunctionGroupSymbol=Clamp$1928:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=y$2690: Argument:Ref=>FunctionGroupSymbol=Clamp$1928:(1/2)
    // Candidates = Interval,Numerical
    Not(IsEmpty(Clamp(x, y)))
    public static void Split(var x, var t)
    // ParameterSymbol=x$2704: Argument:Ref=>FunctionGroupSymbol=Left$1854:(0/2), Argument:Ref=>FunctionGroupSymbol=Right$1856:(0/2)
    // Candidates = Interval
    // ParameterSymbol=t$2705: Argument:Ref=>FunctionGroupSymbol=Left$1854:(1/2), Argument:Ref=>FunctionGroupSymbol=Right$1856:(1/2)
    // Candidates = Interval
    Tuple(Left(x, t), Right(x, t))
    public static void Split(var x)
    // ParameterSymbol=x$2723: Argument:Ref=>FunctionGroupSymbol=Split$1852:(0/2)
    // Candidates = Interval
    Split(x, 0.5)
    public static void Left(var x, var t)
    // ParameterSymbol=x$2731: Argument:Ref=>FunctionGroupSymbol=Lerp$1832:(0/2)
    // Candidates = Interval
    // ParameterSymbol=t$2732: Argument:Ref=>FunctionGroupSymbol=Lerp$1832:(1/2)
    // Candidates = Interval
    Tuple(Min, Lerp(x, t))
    public static void Right(var x, var t)
    // ParameterSymbol=x$2745: Argument:Ref=>FunctionGroupSymbol=Lerp$1832:(0/2), Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1)
    // Candidates = Interval,Interval,Comparable
    // ParameterSymbol=t$2746: Argument:Ref=>FunctionGroupSymbol=Lerp$1832:(1/2)
    // Candidates = Interval
    Tuple(Lerp(x, t), Max(x))
    public static void MoveTo(var x, var t)
    // ParameterSymbol=x$2762: Argument:Ref=>FunctionGroupSymbol=Size$1828:(0/1)
    // Candidates = Interval
    // ParameterSymbol=t$2763: Argument:Ref=>PredefinedSymbol=Tuple$1:(0/2), Argument:Ref=>FunctionGroupSymbol=Add$183:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Tuple(t, Add(t, Size(x)))
    public static void LeftHalf(var x)
    // ParameterSymbol=x$2779: Argument:Ref=>FunctionGroupSymbol=Left$1854:(0/2)
    // Candidates = Interval
    Left(x, 0.5)
    public static void RightHalf(var x)
    // ParameterSymbol=x$2787: Argument:Ref=>FunctionGroupSymbol=Right$1856:(0/2)
    // Candidates = Interval
    Right(x, 0.5)
    public static void HalfSize(var x)
    // ParameterSymbol=x$2795: Argument:Ref=>FunctionGroupSymbol=Size$1828:(0/1)
    // Candidates = Interval
    Half(Size(x))
    public static void Recenter(var x, var c)
    // ParameterSymbol=x$2804: Argument:Ref=>FunctionGroupSymbol=HalfSize$1864:(0/1), Argument:Ref=>FunctionGroupSymbol=HalfSize$1864:(0/1)
    // Candidates = Interval
    // ParameterSymbol=c$2805: Argument:Ref=>FunctionGroupSymbol=Subtract$186:(0/2), Argument:Ref=>FunctionGroupSymbol=Add$183:(0/2)
    // Candidates = ScalarArithmetic,Arithmetic
    Tuple(Subtract(c, HalfSize(x)), Add(c, HalfSize(x)))
    public static void Clamp(var x, var y)
    // ParameterSymbol=x$2829: Argument:Ref=>FunctionGroupSymbol=Clamp$1928:(0/2), Argument:Ref=>FunctionGroupSymbol=Clamp$1928:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=y$2830: Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1)
    // Candidates = Interval,Comparable
    Tuple(Clamp(x, Min(y)), Clamp(x, Max(y)))
    public static void Clamp(var x, var value)
    // ParameterSymbol=x$2854: Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1), Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=value$2855: Argument:Ref=>FunctionGroupSymbol=LessThan$1996:(0/2), Argument:Ref=>FunctionGroupSymbol=GreaterThan$2004:(0/2)
    // Candidates = Comparable
    LessThan(value, Min(x)
        ? Min(x)
        : GreaterThan(value, Max(x)
            ? Max(x)
            : value
        )
    )
    public static void Between(var x, var value)
    // ParameterSymbol=x$2886: Argument:Ref=>FunctionGroupSymbol=Min$2008:(0/1), Argument:Ref=>FunctionGroupSymbol=Max$2010:(0/1)
    // Candidates = Interval,Comparable
    // ParameterSymbol=value$2887: Argument:Ref=>FunctionGroupSymbol=GreaterThanOrEquals$2006:(0/2), Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$2002:(0/2)
    // Candidates = Comparable
    GreaterThanOrEquals(value, And(Min(x), LessThanOrEquals(value, Max(x))))
    public static void Unit()
    Tuple(0, 1)
}
class Vector
{
    public static void Sum(var v)
    // ParameterSymbol=v$2918: Argument:Ref=>FunctionGroupSymbol=Aggregate$2032:(0/3)
    // Candidates = Array
    Aggregate(v, 0, Add)
    public static void SumSquares(var v)
    // ParameterSymbol=v$2928: Argument:Ref=>FunctionGroupSymbol=Square$1922:(0/1)
    // Candidates = Numerical
    Aggregate(Square(v), 0, Add)
    public static void LengthSquared(var v)
    // ParameterSymbol=v$2941: Argument:Ref=>FunctionGroupSymbol=SumSquares$1878:(0/1)
    // Candidates = Vector
    SumSquares(v)
    public static void Length(var v)
    // ParameterSymbol=v$2947: Argument:Ref=>FunctionGroupSymbol=LengthSquared$1880:(0/1)
    // Candidates = Vector
    SquareRoot(LengthSquared(v))
    public static void Dot(var v1, var v2)
    // ParameterSymbol=v1$2956: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    // ParameterSymbol=v2$2957: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Sum(Multiply(v1, v2))
}
class Numerical
{
    public static void Cos(var x)
    // ParameterSymbol=x$2968: 
    // Candidates = Any
    intrinsic
    public static void Sin(var x)
    // ParameterSymbol=x$2971: 
    // Candidates = Any
    intrinsic
    public static void Tan(var x)
    // ParameterSymbol=x$2974: 
    // Candidates = Any
    intrinsic
    public static void Acos(var x)
    // ParameterSymbol=x$2977: 
    // Candidates = Any
    intrinsic
    public static void Asin(var x)
    // ParameterSymbol=x$2980: 
    // Candidates = Any
    intrinsic
    public static void Atan(var x)
    // ParameterSymbol=x$2983: 
    // Candidates = Any
    intrinsic
    public static void Cosh(var x)
    // ParameterSymbol=x$2986: 
    // Candidates = Any
    intrinsic
    public static void Sinh(var x)
    // ParameterSymbol=x$2989: 
    // Candidates = Any
    intrinsic
    public static void Tanh(var x)
    // ParameterSymbol=x$2992: 
    // Candidates = Any
    intrinsic
    public static void Acosh(var x)
    // ParameterSymbol=x$2995: 
    // Candidates = Any
    intrinsic
    public static void Asinh(var x)
    // ParameterSymbol=x$2998: 
    // Candidates = Any
    intrinsic
    public static void Atanh(var x)
    // ParameterSymbol=x$3001: 
    // Candidates = Any
    intrinsic
    public static void Pow(var x, var y)
    // ParameterSymbol=x$3004: 
    // Candidates = Any
    // ParameterSymbol=y$3005: 
    // Candidates = Any
    intrinsic
    public static void Log(var x, var y)
    // ParameterSymbol=x$3008: 
    // Candidates = Any
    // ParameterSymbol=y$3009: 
    // Candidates = Any
    intrinsic
    public static void NaturalLog(var x)
    // ParameterSymbol=x$3012: 
    // Candidates = Any
    intrinsic
    public static void NaturalPower(var x)
    // ParameterSymbol=x$3015: 
    // Candidates = Any
    intrinsic
    public static void SquareRoot(var x)
    // ParameterSymbol=x$3018: Argument:Ref=>FunctionGroupSymbol=Pow$1910:(0/2)
    // Candidates = Numerical
    Pow(x, 0.5)
    public static void CubeRoot(var x)
    // ParameterSymbol=x$3026: Argument:Ref=>FunctionGroupSymbol=Pow$1910:(0/2)
    // Candidates = Numerical
    Pow(x, 0.5)
    public static void Square(var x)
    // ParameterSymbol=x$3034: 
    // Candidates = Any
    Multiply(Value, Value)
    public static void Clamp(var x, var min, var max)
    // ParameterSymbol=x$3042: Argument:Ref=>FunctionGroupSymbol=Clamp$1928:(0/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=min$3043: Argument:Ref=>TypeDefSymbol=Interval$134:(0/2)
    // Candidates = Any
    // ParameterSymbol=max$3044: Argument:Ref=>TypeDefSymbol=Interval$134:(1/2)
    // Candidates = Any
    Clamp(x, Interval(min, max))
    public static void Clamp(var x, var i)
    // ParameterSymbol=x$3057: Argument:Ref=>FunctionGroupSymbol=Clamp$1928:(1/2)
    // Candidates = Interval,Numerical
    // ParameterSymbol=i$3058: Argument:Ref=>FunctionGroupSymbol=Clamp$1928:(0/2)
    // Candidates = Interval,Numerical
    Clamp(i, x)
    public static void Clamp(var x)
    // ParameterSymbol=x$3066: Argument:Ref=>FunctionGroupSymbol=Clamp$1928:(0/3)
    // Candidates = Interval,Numerical
    Clamp(x, 0, 1)
    public static void PlusOne(var x)
    // ParameterSymbol=x$3076: Argument:Ref=>FunctionGroupSymbol=Add$183:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Add(x, 1)
    public static void MinusOne(var x)
    // ParameterSymbol=x$3084: Argument:Ref=>FunctionGroupSymbol=Subtract$186:(0/2)
    // Candidates = ScalarArithmetic
    Subtract(x, 1)
    public static void FromOne(var x)
    // ParameterSymbol=x$3092: Argument:Ref=>FunctionGroupSymbol=Subtract$186:(1/2)
    // Candidates = ScalarArithmetic
    Subtract(1, x)
    public static void Sign(var x)
    // ParameterSymbol=x$3100: Argument:Ref=>FunctionGroupSymbol=LessThan$1996:(0/2), Argument:Ref=>FunctionGroupSymbol=GreaterThan$2004:(0/2)
    // Candidates = Comparable
    LessThan(x, 0
        ? Negative(1)
        : GreaterThan(x, 0
            ? 1
            : 0
        )
    )
    public static void Abs(var x)
    // ParameterSymbol=x$3122: 
    // Candidates = Any
    LessThan(Value, 0
        ? Negative(Value)
        : Value
    )
    public static void Half(var x)
    // ParameterSymbol=x$3136: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 2)
    public static void Third(var x)
    // ParameterSymbol=x$3144: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 3)
    public static void Quarter(var x)
    // ParameterSymbol=x$3152: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 4)
    public static void Fifth(var x)
    // ParameterSymbol=x$3160: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 5)
    public static void Sixth(var x)
    // ParameterSymbol=x$3168: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 6)
    public static void Seventh(var x)
    // ParameterSymbol=x$3176: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 7)
    public static void Eighth(var x)
    // ParameterSymbol=x$3184: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 8)
    public static void Ninth(var x)
    // ParameterSymbol=x$3192: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 9)
    public static void Tenth(var x)
    // ParameterSymbol=x$3200: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 10)
    public static void Sixteenth(var x)
    // ParameterSymbol=x$3208: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 16)
    public static void Hundredth(var x)
    // ParameterSymbol=x$3216: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 100)
    public static void Thousandth(var x)
    // ParameterSymbol=x$3224: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, 1000)
    public static void Millionth(var x)
    // ParameterSymbol=x$3232: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, Divide(1000, 1000))
    public static void Billionth(var x)
    // ParameterSymbol=x$3245: Argument:Ref=>FunctionGroupSymbol=Divide$192:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Divide(x, Divide(1000, Divide(1000, 1000)))
    public static void Hundred(var x)
    // ParameterSymbol=x$3263: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, 100)
    public static void Thousand(var x)
    // ParameterSymbol=x$3271: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, 1000)
    public static void Million(var x)
    // ParameterSymbol=x$3279: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, Multiply(1000, 1000))
    public static void Billion(var x)
    // ParameterSymbol=x$3292: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, Multiply(1000, Multiply(1000, 1000)))
    public static void Twice(var x)
    // ParameterSymbol=x$3310: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, 2)
    public static void Thrice(var x)
    // ParameterSymbol=x$3318: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, 3)
    public static void SmoothStep(var x)
    // ParameterSymbol=x$3326: Argument:Ref=>FunctionGroupSymbol=Square$1922:(0/1), Argument:Ref=>FunctionGroupSymbol=Twice$1976:(0/1)
    // Candidates = Numerical
    Multiply(Square(x), Subtract(3, Twice(x)))
    public static void Pow2(var x)
    // ParameterSymbol=x$3345: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2), Argument:Ref=>FunctionGroupSymbol=Multiply$189:(1/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, x)
    public static void Pow3(var x)
    // ParameterSymbol=x$3353: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(1/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1982:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    Multiply(Pow2(x), x)
    public static void Pow4(var x)
    // ParameterSymbol=x$3364: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(1/2), Argument:Ref=>FunctionGroupSymbol=Pow3$1984:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    Multiply(Pow3(x), x)
    public static void Pow5(var x)
    // ParameterSymbol=x$3375: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(1/2), Argument:Ref=>FunctionGroupSymbol=Pow4$1986:(0/1)
    // Candidates = Arithmetic,ScalarArithmetic,Numerical
    Multiply(Pow4(x), x)
    public static void Turns(var x)
    // ParameterSymbol=x$3386: Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2)
    // Candidates = Arithmetic,ScalarArithmetic
    Multiply(x, Multiply(3.1415926535897, 2))
    public static void AlmostZero(var x)
    // ParameterSymbol=x$3399: Argument:Ref=>FunctionGroupSymbol=Abs$1938:(0/1)
    // Candidates = Numerical
    LessThan(Abs(x), 1E-08)
}
class Comparable
{
    public static void Equals(var a, var b)
    // ParameterSymbol=a$3410: Argument:Ref=>FunctionGroupSymbol=Compare$158:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$3411: Argument:Ref=>FunctionGroupSymbol=Compare$158:(1/2)
    // Candidates = Comparable
    Equals(Compare(a, b), 0)
    public static void LessThan(var a, var b)
    // ParameterSymbol=a$3424: Argument:Ref=>FunctionGroupSymbol=Compare$158:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$3425: Argument:Ref=>FunctionGroupSymbol=Compare$158:(1/2)
    // Candidates = Comparable
    LessThan(Compare(a, b), 0)
    public static void Lesser(var a, var b)
    // ParameterSymbol=a$3438: Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$2002:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$3439: Argument:Ref=>FunctionGroupSymbol=LessThanOrEquals$2002:(1/2)
    // Candidates = Comparable
    LessThanOrEquals(a, b)
        ? a
        : b

    public static void Greater(var a, var b)
    // ParameterSymbol=a$3450: Argument:Ref=>FunctionGroupSymbol=GreaterThanOrEquals$2006:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$3451: Argument:Ref=>FunctionGroupSymbol=GreaterThanOrEquals$2006:(1/2)
    // Candidates = Comparable
    GreaterThanOrEquals(a, b)
        ? a
        : b

    public static void LessThanOrEquals(var a, var b)
    // ParameterSymbol=a$3462: Argument:Ref=>FunctionGroupSymbol=Compare$158:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$3463: Argument:Ref=>FunctionGroupSymbol=Compare$158:(1/2)
    // Candidates = Comparable
    LessThanOrEquals(Compare(a, b), 0)
    public static void GreaterThan(var a, var b)
    // ParameterSymbol=a$3476: Argument:Ref=>FunctionGroupSymbol=Compare$158:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$3477: Argument:Ref=>FunctionGroupSymbol=Compare$158:(1/2)
    // Candidates = Comparable
    GreaterThan(Compare(a, b), 0)
    public static void GreaterThanOrEquals(var a, var b)
    // ParameterSymbol=a$3490: Argument:Ref=>FunctionGroupSymbol=Compare$158:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$3491: Argument:Ref=>FunctionGroupSymbol=Compare$158:(1/2)
    // Candidates = Comparable
    GreaterThanOrEquals(Compare(a, b), 0)
    public static void Min(var a, var b)
    // ParameterSymbol=a$3504: Argument:Ref=>FunctionGroupSymbol=LessThan$1996:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$3505: Argument:Ref=>FunctionGroupSymbol=LessThan$1996:(1/2)
    // Candidates = Comparable
    LessThan(a, b)
        ? a
        : b

    public static void Max(var a, var b)
    // ParameterSymbol=a$3516: Argument:Ref=>FunctionGroupSymbol=GreaterThan$2004:(0/2)
    // Candidates = Comparable
    // ParameterSymbol=b$3517: Argument:Ref=>FunctionGroupSymbol=GreaterThan$2004:(1/2)
    // Candidates = Comparable
    GreaterThan(a, b)
        ? a
        : b

    public static void Between(var v, var a, var b)
    // ParameterSymbol=v$3528: Argument:Ref=>FunctionGroupSymbol=Between$2014:(0/2)
    // Candidates = Interval,Comparable
    // ParameterSymbol=a$3529: Argument:Ref=>TypeDefSymbol=Interval$134:(0/2)
    // Candidates = Any
    // ParameterSymbol=b$3530: Argument:Ref=>TypeDefSymbol=Interval$134:(1/2)
    // Candidates = Any
    Between(v, Interval(a, b))
    public static void Between(var v, var i)
    // ParameterSymbol=v$3543: Argument:Ref=>FunctionGroupSymbol=Contains$1846:(1/2)
    // Candidates = Interval
    // ParameterSymbol=i$3544: Argument:Ref=>FunctionGroupSymbol=Contains$1846:(0/2)
    // Candidates = Interval
    Contains(i, v)
}
class Boolean
{
    public static void XOr(var a, var b)
    // ParameterSymbol=a$3552: 
    // Candidates = Any
    // ParameterSymbol=b$3553: Argument:Ref=>FunctionGroupSymbol=Not$204:(0/1)
    // Candidates = Boolean
    a
        ? Not(b)
        : b

    public static void NAnd(var a, var b)
    // ParameterSymbol=a$3562: Argument:Ref=>FunctionGroupSymbol=And$198:(0/2)
    // Candidates = Boolean
    // ParameterSymbol=b$3563: Argument:Ref=>FunctionGroupSymbol=And$198:(1/2)
    // Candidates = Boolean
    Not(And(a, b))
    public static void NOr(var a, var b)
    // ParameterSymbol=a$3574: Argument:Ref=>FunctionGroupSymbol=Or$201:(0/2)
    // Candidates = Boolean
    // ParameterSymbol=b$3575: Argument:Ref=>FunctionGroupSymbol=Or$201:(1/2)
    // Candidates = Boolean
    Not(Or(a, b))
}
class Equatable
{
    public static void NotEquals(var x)
    // ParameterSymbol=x$3586: Argument:Ref=>FunctionGroupSymbol=Equals$1994:(0/1)
    // Candidates = Equatable,Comparable
    Not(Equals(x))
}
class Array
{
    public static void Map(var xs, var f)
    // ParameterSymbol=xs$3595: Argument:Ref=>TypeDefSymbol=Count$27:(0/1), Argument:Ref=>FunctionGroupSymbol=At$251:(0/2)
    // Candidates = Vector,Array
    // ParameterSymbol=f$3596: Invoked:(ArgumentSymbol)
    // Candidates = Function
    Map(Count(xs), (i) => 
    // ParameterSymbol=i$3602: Argument:Ref=>FunctionGroupSymbol=At$251:(1/2)
    // Candidates = Vector,Array
    f(At(xs, i)))
    public static void Zip(var xs, var ys, var f)
    // ParameterSymbol=xs$3618: Argument:Ref=>TypeDefSymbol=Count$27:(0/1)
    // Candidates = Vector,Array
    // ParameterSymbol=ys$3619: Argument:Ref=>FunctionGroupSymbol=At$251:(0/2)
    // Candidates = Vector,Array
    // ParameterSymbol=f$3620: Invoked:(ArgumentSymbol,ArgumentSymbol)
    // Candidates = Function
    Array(Count(xs), (i) => 
    // ParameterSymbol=i$3626: Argument:Ref=>FunctionGroupSymbol=At$251:(0/1), Argument:Ref=>FunctionGroupSymbol=At$251:(1/2)
    // Candidates = Vector,Array
    f(At(i), At(ys, i)))
    public static void Skip(var xs, var n)
    // ParameterSymbol=xs$3647: 
    // Candidates = Any
    // ParameterSymbol=n$3648: Argument:Ref=>FunctionGroupSymbol=Subtract$186:(1/2), Argument:Ref=>FunctionGroupSymbol=Subtract$186:(1/2)
    // Candidates = ScalarArithmetic
    Array(Subtract(Count, n), (i) => 
    // ParameterSymbol=i$3656: Argument:Ref=>FunctionGroupSymbol=Subtract$186:(0/2)
    // Candidates = ScalarArithmetic
    At(Subtract(i, n)))
    public static void Take(var xs, var n)
    // ParameterSymbol=xs$3672: 
    // Candidates = Any
    // ParameterSymbol=n$3673: Argument:Ref=>TypeDefSymbol=Array$140:(0/2)
    // Candidates = Any
    Array(n, (i) => 
    // ParameterSymbol=i$3676: 
    // Candidates = Any
    At)
    public static void Aggregate(var xs, var init, var f)
    // ParameterSymbol=xs$3684: Argument:Ref=>FunctionGroupSymbol=IsEmpty$2036:(0/1), Argument:Ref=>FunctionGroupSymbol=Rest$2034:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=init$3685: Argument:Ref=>ParameterSymbol=f$3686:(0/2)
    // Candidates = Any
    // ParameterSymbol=f$3686: Invoked:(ArgumentSymbol,ArgumentSymbol), Invoked:(ArgumentSymbol)
    // Candidates = Function
    IsEmpty(xs)
        ? init
        : f(init, f(Rest(xs)))

    public static void Rest(var xs)
    // ParameterSymbol=xs$3706: 
    // Candidates = Any
    Skip(1)
    public static void IsEmpty(var xs)
    // ParameterSymbol=xs$3712: Argument:Ref=>TypeDefSymbol=Count$27:(0/1)
    // Candidates = Vector,Array
    Equals(Count(xs), 0)
    public static void First(var xs)
    // ParameterSymbol=xs$3723: Argument:Ref=>FunctionGroupSymbol=At$251:(0/2)
    // Candidates = Vector,Array
    At(xs, 0)
    public static void Last(var xs)
    // ParameterSymbol=xs$3731: Argument:Ref=>FunctionGroupSymbol=At$251:(0/2), Argument:Ref=>TypeDefSymbol=Count$27:(0/1)
    // Candidates = Vector,Array
    At(xs, Subtract(Count(xs), 1))
    public static void Slice(var xs, var from, var count)
    // ParameterSymbol=xs$3747: Argument:Ref=>FunctionGroupSymbol=Skip$2028:(0/2)
    // Candidates = Array
    // ParameterSymbol=from$3748: Argument:Ref=>FunctionGroupSymbol=Skip$2028:(1/2)
    // Candidates = Array
    // ParameterSymbol=count$3749: Argument:Ref=>FunctionGroupSymbol=Take$2030:(1/2)
    // Candidates = Array
    Take(Skip(xs, from), count)
    public static void Join(var xs, var sep)
    // ParameterSymbol=xs$3762: Argument:Ref=>FunctionGroupSymbol=IsEmpty$2036:(0/1), Argument:Ref=>FunctionGroupSymbol=First$2038:(0/1), Argument:Ref=>FunctionGroupSymbol=Skip$2028:(0/2)
    // Candidates = Interval,Array
    // ParameterSymbol=sep$3763: Argument:Ref=>FunctionGroupSymbol=Interpolate$2116:(1/3)
    // Candidates = Intrinsics
    IsEmpty(xs)
        ? 
        : Add(ToString(First(xs)), Aggregate(Skip(xs, 1), , (acc, cur) => 
        // ParameterSymbol=acc$3786: Argument:Ref=>FunctionGroupSymbol=Interpolate$2116:(0/3)
        // Candidates = Intrinsics
        // ParameterSymbol=cur$3787: Argument:Ref=>FunctionGroupSymbol=Interpolate$2116:(2/3)
        // Candidates = Intrinsics
        Interpolate(acc, sep, cur)))

    public static void All(var xs, var f)
    // ParameterSymbol=xs$3806: Argument:Ref=>FunctionGroupSymbol=IsEmpty$2036:(0/1), Argument:Ref=>FunctionGroupSymbol=First$2038:(0/1), Argument:Ref=>FunctionGroupSymbol=Rest$2034:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=f$3807: Invoked:(ArgumentSymbol), Invoked:(ArgumentSymbol)
    // Candidates = Function
    IsEmpty(xs)
        ? True
        : And(f(First(xs)), f(Rest(xs)))

    public static void JoinStrings(var xs, var sep)
    // ParameterSymbol=xs$3833: Argument:Ref=>FunctionGroupSymbol=IsEmpty$2036:(0/1), Argument:Ref=>FunctionGroupSymbol=First$2038:(0/1), Argument:Ref=>FunctionGroupSymbol=Rest$2034:(0/1)
    // Candidates = Interval,Array
    // ParameterSymbol=sep$3834: 
    // Candidates = Any
    IsEmpty(xs)
        ? 
        : Add(First(xs), Aggregate(Rest(xs), , (x, acc) => 
        // ParameterSymbol=x$3852: Argument:Ref=>FunctionGroupSymbol=ToString$237:(0/1)
        // Candidates = Value
        // ParameterSymbol=acc$3853: Argument:Ref=>FunctionGroupSymbol=Add$183:(0/2)
        // Candidates = Arithmetic,ScalarArithmetic
        Add(acc, Add(, , ToString(x)))))

}
class Easings
{
    public static void BlendEaseFunc(var p, var easeIn, var easeOut)
    // ParameterSymbol=p$3878: Argument:Ref=>FunctionGroupSymbol=LessThan$1996:(0/2), Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2), Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2)
    // Candidates = Comparable,Arithmetic,ScalarArithmetic
    // ParameterSymbol=easeIn$3879: Invoked:(ArgumentSymbol)
    // Candidates = Function
    // ParameterSymbol=easeOut$3880: Invoked:(ArgumentSymbol)
    // Candidates = Function
    LessThan(p, 0.5
        ? Multiply(0.5, easeIn(Multiply(p, 2)))
        : Multiply(0.5, Add(easeOut(Multiply(p, Subtract(2, 1))), 0.5))
    )
    public static void InvertEaseFunc(var p, var easeIn)
    // ParameterSymbol=p$3927: Argument:Ref=>FunctionGroupSymbol=Subtract$186:(1/2)
    // Candidates = ScalarArithmetic
    // ParameterSymbol=easeIn$3928: Invoked:(ArgumentSymbol)
    // Candidates = Function
    Subtract(1, easeIn(Subtract(1, p)))
    public static void Linear(var p)
    // ParameterSymbol=p$3944: 
    // Candidates = Any
    p
    public static void QuadraticEaseIn(var p)
    // ParameterSymbol=p$3947: Argument:Ref=>FunctionGroupSymbol=Pow2$1982:(0/1)
    // Candidates = Numerical
    Pow2(p)
    public static void QuadraticEaseOut(var p)
    // ParameterSymbol=p$3953: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$2052:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, QuadraticEaseIn)
    public static void QuadraticEaseInOut(var p)
    // ParameterSymbol=p$3961: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$2050:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, QuadraticEaseIn, QuadraticEaseOut)
    public static void CubicEaseIn(var p)
    // ParameterSymbol=p$3971: Argument:Ref=>FunctionGroupSymbol=Pow3$1984:(0/1)
    // Candidates = Numerical
    Pow3(p)
    public static void CubicEaseOut(var p)
    // ParameterSymbol=p$3977: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$2052:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, CubicEaseIn)
    public static void CubicEaseInOut(var p)
    // ParameterSymbol=p$3985: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$2050:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, CubicEaseIn, CubicEaseOut)
    public static void QuarticEaseIn(var p)
    // ParameterSymbol=p$3995: Argument:Ref=>FunctionGroupSymbol=Pow4$1986:(0/1)
    // Candidates = Numerical
    Pow4(p)
    public static void QuarticEaseOut(var p)
    // ParameterSymbol=p$4001: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$2052:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, QuarticEaseIn)
    public static void QuarticEaseInOut(var p)
    // ParameterSymbol=p$4009: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$2050:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, QuarticEaseIn, QuarticEaseOut)
    public static void QuinticEaseIn(var p)
    // ParameterSymbol=p$4019: Argument:Ref=>FunctionGroupSymbol=Pow5$1988:(0/1)
    // Candidates = Numerical
    Pow5(p)
    public static void QuinticEaseOut(var p)
    // ParameterSymbol=p$4025: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$2052:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, QuinticEaseIn)
    public static void QuinticEaseInOut(var p)
    // ParameterSymbol=p$4033: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$2050:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, QuinticEaseIn, QuinticEaseOut)
    public static void SineEaseIn(var p)
    // ParameterSymbol=p$4043: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$2052:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, SineEaseOut)
    public static void SineEaseOut(var p)
    // ParameterSymbol=p$4051: Argument:Ref=>FunctionGroupSymbol=Quarter$1944:(0/1)
    // Candidates = Numerical
    Sin(Turns(Quarter(p)))
    public static void SineEaseInOut(var p)
    // ParameterSymbol=p$4063: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$2050:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, SineEaseIn, SineEaseOut)
    public static void CircularEaseIn(var p)
    // ParameterSymbol=p$4073: Argument:Ref=>FunctionGroupSymbol=Pow2$1982:(0/1)
    // Candidates = Numerical
    FromOne(SquareRoot(FromOne(Pow2(p))))
    public static void CircularEaseOut(var p)
    // ParameterSymbol=p$4088: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$2052:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, CircularEaseIn)
    public static void CircularEaseInOut(var p)
    // ParameterSymbol=p$4096: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$2050:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, CircularEaseIn, CircularEaseOut)
    public static void ExponentialEaseIn(var p)
    // ParameterSymbol=p$4106: Argument:Ref=>FunctionGroupSymbol=AlmostZero$1992:(0/1), Argument:Ref=>FunctionGroupSymbol=MinusOne$1932:(0/1)
    // Candidates = Numerical
    AlmostZero(p)
        ? p
        : Pow(2, Multiply(10, MinusOne(p)))

    public static void ExponentialEaseOut(var p)
    // ParameterSymbol=p$4128: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$2052:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, ExponentialEaseIn)
    public static void ExponentialEaseInOut(var p)
    // ParameterSymbol=p$4136: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$2050:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, ExponentialEaseIn, ExponentialEaseOut)
    public static void ElasticEaseIn(var p)
    // ParameterSymbol=p$4146: Argument:Ref=>FunctionGroupSymbol=Quarter$1944:(0/1), Argument:Ref=>FunctionGroupSymbol=MinusOne$1932:(0/1)
    // Candidates = Numerical
    Multiply(13, Multiply(Turns(Quarter(p)), Sin(Radians(Pow(2, Multiply(10, MinusOne(p)))))))
    public static void ElasticEaseOut(var p)
    // ParameterSymbol=p$4184: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$2052:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, ElasticEaseIn)
    public static void ElasticEaseInOut(var p)
    // ParameterSymbol=p$4192: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$2050:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, ElasticEaseIn, ElasticEaseOut)
    public static void BackEaseIn(var p)
    // ParameterSymbol=p$4202: Argument:Ref=>FunctionGroupSymbol=Pow3$1984:(0/1), Argument:Ref=>FunctionGroupSymbol=Multiply$189:(0/2), Argument:Ref=>FunctionGroupSymbol=Half$1940:(0/1)
    // Candidates = Numerical,Arithmetic,ScalarArithmetic
    Subtract(Pow3(p), Multiply(p, Sin(Turns(Half(p)))))
    public static void BackEaseOut(var p)
    // ParameterSymbol=p$4227: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$2052:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, BackEaseIn)
    public static void BackEaseInOut(var p)
    // ParameterSymbol=p$4235: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$2050:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, BackEaseIn, BackEaseOut)
    public static void BounceEaseIn(var p)
    // ParameterSymbol=p$4245: Argument:Ref=>FunctionGroupSymbol=InvertEaseFunc$2052:(0/2)
    // Candidates = Easings
    InvertEaseFunc(p, BounceEaseOut)
    public static void BounceEaseOut(var p)
    // ParameterSymbol=p$4253: Argument:Ref=>FunctionGroupSymbol=LessThan$1996:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1982:(0/1), Argument:Ref=>FunctionGroupSymbol=LessThan$1996:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1982:(0/1), Argument:Ref=>FunctionGroupSymbol=Add$183:(0/2), Argument:Ref=>FunctionGroupSymbol=LessThan$1996:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1982:(0/1), Argument:Ref=>FunctionGroupSymbol=Add$183:(0/2), Argument:Ref=>FunctionGroupSymbol=Pow2$1982:(0/1), Argument:Ref=>FunctionGroupSymbol=Add$183:(0/2)
    // Candidates = Comparable,Numerical,Arithmetic,ScalarArithmetic
    LessThan(p, Divide(4, 11))
        ? Multiply(121, Divide(Pow2(p), 16))
        : LessThan(p, Divide(8, 11))
            ? Divide(363, Multiply(40, Subtract(Pow2(p), Divide(99, Multiply(10, Add(p, Divide(17, 5)))))))
            : LessThan(p, Divide(9, 10))
                ? Divide(4356, Multiply(361, Subtract(Pow2(p), Divide(35442, Multiply(1805, Add(p, Divide(16061, 1805)))))))
                : Divide(54, Multiply(5, Subtract(Pow2(p), Divide(513, Multiply(25, Add(p, Divide(268, 25)))))))



    public static void BounceEaseInOut(var p)
    // ParameterSymbol=p$4422: Argument:Ref=>FunctionGroupSymbol=BlendEaseFunc$2050:(0/3)
    // Candidates = Easings
    BlendEaseFunc(p, BounceEaseIn, BounceEaseOut)
}
class Intrinsics
{
    public static void Interpolate(var xs)
    // ParameterSymbol=xs$4432: 
    // Candidates = Any
    intrinsic
    public static void Throw(var x)
    // ParameterSymbol=x$4435: 
    // Candidates = Any
    intrinsic
    public static void TypeOf(var x)
    // ParameterSymbol=x$4438: 
    // Candidates = Any
    intrinsic
    public static void New(var x)
    // ParameterSymbol=x$4441: 
    // Candidates = Any
    intrinsic
}
