class Interval{
    var Min;
    var Max;
}
class Vector{
}
class Measure{
    Number Value;
}
class Numerical{
    public static var FromNumber(var x){
        {
            {
                FromNumber(null, x);
            }
            ;
        }
        ;
    }
    ;
}
class Magnitude{
    public static Number Magnitude(var x){
        {
            {
                null(null(null(null(x))));
            }
            ;
        }
        ;
    }
    ;
}
class Comparable{
    public static Integer Compare(var a, var b){
        {
            {
                < (Magnitude(a), if (Magnitude(b))
                {
                    -(1);
                }
                else
                {
                    > (Magnitude(a), if (Magnitude(b))
                    {
                        1;
                    }
                    else
                    {
                        0;
                    }
                    );
                }
                );
            }
            ;
        }
        ;
    }
    ;
}
class Equatable{
    public static var Equals(var a, var b){
        {
            {
                null(Equals(null(a), null(b)));
            }
            ;
        }
        ;
    }
    ;
}
class Arithmetic{
    public static var Add(var self, var other){
        {
            {
                Add(null(self), null(other));
            }
            ;
        }
        ;
    }
    ;
    public static var Negative(var self){
        {
            {
                Negative(null(self));
            }
            ;
        }
        ;
    }
    ;
    public static var Reciprocal(var self){
        {
            {
                Reciprocal(null(self));
            }
            ;
        }
        ;
    }
    ;
    public static var Multiply(var self, var other){
        {
            {
                Add(null(self), null(other));
            }
            ;
        }
        ;
    }
    ;
    public static var Divide(var self, var other){
        {
            {
                Divide(null(self), null(other));
            }
            ;
        }
        ;
    }
    ;
    public static var Modulo(var self, var other){
        {
            {
                Modulo(null(self), null(other));
            }
            ;
        }
        ;
    }
    ;
}
class ScalarArithmetic{
    public static var Add(var self, var scalar){
        {
            {
                Add(null(self), scalar);
            }
            ;
        }
        ;
    }
    ;
    public static var Subtract(var self, var scalar){
        {
            {
                Add(self, null(scalar));
            }
            ;
        }
        ;
    }
    ;
    public static var Multiply(var self, var scalar){
        {
            {
                Multiply(null(self), scalar);
            }
            ;
        }
        ;
    }
    ;
    public static var Divide(var self, var scalar){
        {
            {
                Multiply(self, null(scalar));
            }
            ;
        }
        ;
    }
    ;
    public static var Modulo(var self, var scalar){
        {
            {
                Modulo(null(self), scalar);
            }
            ;
        }
        ;
    }
    ;
}
class Value{
    public static var Type(){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static Array FieldTypes(){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static Array FieldNames(){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static Array FieldValues(var self){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Zero(){
        {
            {
                Zero(null);
            }
            ;
        }
        ;
    }
    ;
    public static var One(){
        {
            {
                One(null);
            }
            ;
        }
        ;
    }
    ;
    public static var Default(){
        {
            {
                Default(null);
            }
            ;
        }
        ;
    }
    ;
    public static var MinValue(){
        {
            {
                MinValue(null);
            }
            ;
        }
        ;
    }
    ;
    public static var MaxValue(){
        {
            {
                MaxValue(null);
            }
            ;
        }
        ;
    }
    ;
    public static var ToString(var x){
        {
            {
                null(null, ,);
            }
            ;
        }
        ;
    }
    ;
}
class Array{
    public static var At(var n){
        null;
    }
    ;
    Count Count;
}
class Integer{
    Integer Value;
}
class Count{
    Integer Value;
}
class Index{
    Integer Value;
}
class Number{
    var Value;
}
class Unit{
    Number Value;
}
class Percent{
    Number Value;
}
class Quaternion{
    Number X;
    Number Y;
    Number Z;
    Number W;
}
class Unit2D{
    Unit X;
    Unit Y;
}
class Unit3D{
    Unit X;
    Unit Y;
    Unit Z;
}
class Direction3D{
    Unit3D Value;
}
class AxisAngle{
    Unit3D Axis;
    Angle Angle;
}
class EulerAngles{
    Angle Yaw;
    Angle Pitch;
    Angle Roll;
}
class Rotation3D{
    Quaternion Quaternion;
}
class Vector2D{
    Number X;
    Number Y;
}
class Vector3D{
    Number X;
    Number Y;
    Number Z;
}
class Vector4D{
    Number X;
    Number Y;
    Number Z;
    Number W;
}
class Orientation3D{
    Rotation3D Value;
}
class Pose2D{
    Vector3D Position;
    Orientation3D Orientation;
}
class Pose3D{
    Vector3D Position;
    Orientation3D Orientation;
}
class Transform3D{
    Vector3D Translation;
    Rotation3D Rotation;
    Vector3D Scale;
}
class Transform2D{
    Vector2D Translation;
    Angle Rotation;
    Vector2D Scale;
}
class AlignedBox2D{
    Vector2D A;
    Vector2D B;
}
class AlignedBox3D{
    Vector3D A;
    Vector3D B;
}
class Complex{
    Number Real;
    Number Imaginary;
}
class Ray3D{
    Vector3D Direction;
    Point3D Position;
}
class Ray2D{
    Vector2D Direction;
    Point2D Position;
}
class Sphere{
    Point3D Center;
    Number Radius;
}
class Plane{
    Unit3D Normal;
    Number D;
}
class Triangle3D{
    Point3D A;
    Point3D B;
    Point3D C;
}
class Triangle2D{
    Point2D A;
    Point2D B;
    Point2D C;
}
class Quad3D{
    Point3D A;
    Point3D B;
    Point3D C;
    Point3D D;
}
class Quad2D{
    Point2D A;
    Point2D B;
    Point2D C;
    Point2D D;
}
class Point3D{
    Vector3D Value;
}
class Point2D{
    Vector2D Value;
}
class Line3D{
    Point3D A;
    Point3D B;
}
class Line2D{
    Point2D A;
    Point2D B;
}
class Color{
    Unit R;
    Unit G;
    Unit B;
    Unit A;
}
class ColorLUV{
    Percent Lightness;
    Unit U;
    Unit V;
}
class ColorLAB{
    Percent Lightness;
    Integer A;
    Integer B;
}
class ColorLCh{
    Percent Lightness;
    PolarCoordinate ChromaHue;
}
class ColorHSV{
    Angle Hue;
    Unit S;
    Unit V;
}
class ColorHSL{
    Angle Hue;
    Unit Saturation;
    Unit Luminance;
}
class ColorYCbCr{
    Unit Y;
    Unit Cb;
    Unit Cr;
}
class SphericalCoordinate{
    Number Radius;
    Angle Azimuth;
    Angle Polar;
}
class PolarCoordinate{
    Number Radius;
    Angle Angle;
}
class LogPolarCoordinate{
    Number Rho;
    Angle Azimuth;
}
class CylindricalCoordinate{
    Number RadialDistance;
    Angle Azimuth;
    Number Height;
}
class HorizontalCoordinate{
    Number Radius;
    Angle Azimuth;
    Number Height;
}
class GeoCoordinate{
    Angle Latitude;
    Angle Longitude;
}
class GeoCoordinateWithAltitude{
    GeoCoordinate Coordinate;
    Number Altitude;
}
class Circle{
    Point2D Center;
    Number Radius;
}
class Chord{
    Circle Circle;
    Arc Arc;
}
class Size2D{
    Number Width;
    Number Height;
}
class Size3D{
    Number Width;
    Number Height;
    Number Depth;
}
class Rectangle2D{
    Point2D Center;
    Size2D Size;
}
class Proportion{
    Number Value;
}
class Fraction{
    Number Numerator;
    Number Denominator;
}
class Angle{
    Number Radians;
}
class Length{
    Number Meters;
}
class Mass{
    Number Kilograms;
}
class Temperature{
    Number Celsius;
}
class TimeSpan{
    Number Seconds;
}
class TimeRange{
    DateTime Min;
    DateTime Max;
}
class DateTime{
}
class AnglePair{
    Angle Start;
    Angle End;
}
class Ring{
    Circle Circle;
    Number InnerRadius;
}
class Arc{
    AnglePair Angles;
    Circle Cirlce;
}
class TimeInterval{
    TimeSpan Start;
    TimeSpan End;
}
class RealInterval{
    Number A;
    Number B;
}
class Interval2D{
    Vector2D A;
    Vector2D B;
}
class Interval3D{
    Vector3D A;
    Vector3D B;
}
class Capsule{
    Line3D Line;
    Number Radius;
}
class Matrix3D{
    Vector4D Column1;
    Vector4D Column2;
    Vector4D Column3;
    Vector4D Column4;
}
class Cylinder{
    Line3D Line;
    Number Radius;
}
class Cone{
    Line3D Line;
    Number Radius;
}
class Tube{
    Line3D Line;
    Number InnerRadius;
    Number OuterRadius;
}
class ConeSegment{
    Line3D Line;
    Number Radius1;
    Number Radius2;
}
class Box2D{
    Point2D Center;
    Angle Rotation;
    Size2D Extent;
}
class Box3D{
    Point3D Center;
    Rotation3D Rotation;
    Size3D Extent;
}
class CubicBezierTriangle3D{
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
class CubicBezier2D{
    Point2D A;
    Point2D B;
    Point2D C;
    Point2D D;
}
class UV{
    Unit U;
    Unit V;
}
class UVW{
    Unit U;
    Unit V;
    Unit W;
}
class CubicBezier3D{
    Point3D A;
    Point3D B;
    Point3D C;
    Point3D D;
}
class QuadraticBezier2D{
    Point2D A;
    Point2D B;
    Point2D C;
}
class QuadraticBezier3D{
    Point3D A;
    Point3D B;
    Point3D C;
}
class Area{
    Number MetersSquared;
}
class Volume{
    Number MetersCubed;
}
class Velocity{
    Number MetersPerSecond;
}
class Acceleration{
    Number MetersPerSecondSquared;
}
class Force{
    Number Newtons;
}
class Pressure{
    Number Pascals;
}
class Energy{
    Number Joules;
}
class Memory{
    Count Bytes;
}
class Frequency{
    Number Hertz;
}
class Loudness{
    Number Decibels;
}
class LuminousIntensity{
    Number Candelas;
}
class ElectricPotential{
    Number Volts;
}
class ElectricCharge{
    Number Columbs;
}
class ElectricCurrent{
    Number Amperes;
}
class ElectricResistance{
    Number Ohms;
}
class Power{
    Number Watts;
}
class Density{
    Number KilogramsPerMeterCubed;
}
class NormalDistribution{
    Number Mean;
    Number StandardDeviation;
}
class PoissonDistribution{
    Number Expected;
    Count Occurrences;
}
class BernoulliDistribution{
    Probability P;
}
class Probability{
    Number Value;
}
class BinomialDistribution{
    Count Trials;
    Probability P;
}
class Interval{
    public static var Size(var x){
        {
            {
                - (null(x), null(x));
            }
            ;
        }
        ;
    }
    ;
    public static var IsEmpty(var x){
        {
            {
                >= (null(x), null(x));
            }
            ;
        }
        ;
    }
    ;
    public static var Lerp(var x, var amount){
        {
            {
                * (null(x), + (- (1, amount), * (null(x), amount)));
            }
            ;
        }
        ;
    }
    ;
    public static var InverseLerp(var x, var value){
        {
            {
                / (- (value, null(x)), Size(x));
            }
            ;
        }
        ;
    }
    ;
    public static var Negate(var x){
        {
            {
                Interval(-(null(x)), -(null(x)));
            }
            ;
        }
        ;
    }
    ;
    public static var Reverse(var x){
        {
            {
                Interval(null(x), null(x));
            }
            ;
        }
        ;
    }
    ;
    public static var Resize(var x, var size){
        {
            {
                Interval(null(x), + (null(x), size));
            }
            ;
        }
        ;
    }
    ;
    public static var Center(var x){
        {
            {
                Lerp(x, 0.5);
            }
            ;
        }
        ;
    }
    ;
    public static var Contains(var x, var value){
        {
            {
                <= (null(x), && (value, <= (value, null(x))));
            }
            ;
        }
        ;
    }
    ;
    public static var Contains(var x, var other){
        {
            {
                <= (null(x), && (null(other), >= (null, null(other))));
            }
            ;
        }
        ;
    }
    ;
    public static var Overlaps(var x, var y){
        {
            {
                !(IsEmpty(Clamp(x, y)));
            }
            ;
        }
        ;
    }
    ;
    public static var Split(var x, var t){
        {
            {
                Interval(Left(x, t), Right(x, t));
            }
            ;
        }
        ;
    }
    ;
    public static var Split(var x){
        {
            {
                Split(x, 0.5);
            }
            ;
        }
        ;
    }
    ;
    public static var Left(var x, var t){
        {
            {
                Interval(null, Lerp(x, t));
            }
            ;
        }
        ;
    }
    ;
    public static var Right(var x, var t){
        {
            {
                Interval(Lerp(x, t), null(x));
            }
            ;
        }
        ;
    }
    ;
    public static var MoveTo(var x, var t){
        {
            {
                Interval(t, + (t, Size(x)));
            }
            ;
        }
        ;
    }
    ;
    public static var LeftHalf(var x){
        {
            {
                Left(x, 0.5);
            }
            ;
        }
        ;
    }
    ;
    public static var RightHalf(var x){
        {
            {
                Right(x, 0.5);
            }
            ;
        }
        ;
    }
    ;
    public static var HalfSize(var x){
        {
            {
                null(Size(x));
            }
            ;
        }
        ;
    }
    ;
    public static var Recenter(var x, var c){
        {
            {
                Interval(- (c, HalfSize(x)), + (c, HalfSize(x)));
            }
            ;
        }
        ;
    }
    ;
    public static var Clamp(var x, var y){
        {
            {
                Interval(Clamp(x, null(y)), Clamp(x, null(y)));
            }
            ;
        }
        ;
    }
    ;
    public static var Clamp(var x, var value){
        {
            {
                < (value, if (null(x))
                {
                    null(x);
                }
                else
                {
                    > (value, if (null(x))
                    {
                        null(x);
                    }
                    else
                    {
                        value;
                    }
                    );
                }
                );
            }
            ;
        }
        ;
    }
    ;
    public static var Between(var x, var value){
        {
            {
                >= (value, && (null(x), <= (value, null(x))));
            }
            ;
        }
        ;
    }
    ;
    public static var Unit(){
        {
            {
                Interval(0, 1);
            }
            ;
        }
        ;
    }
    ;
}
class Vector{
    public static var Sum(var v){
        {
            {
                null(v, 0, null);
            }
            ;
        }
        ;
    }
    ;
    public static var SumSquares(var v){
        {
            {
                null(null(v), 0, null);
            }
            ;
        }
        ;
    }
    ;
    public static var LengthSquared(var v){
        {
            {
                SumSquares(v);
            }
            ;
        }
        ;
    }
    ;
    public static var Length(var v){
        {
            {
                null(LengthSquared(v));
            }
            ;
        }
        ;
    }
    ;
    public static var Dot(var v1, var v2){
        {
            {
                Sum(* (v1, v2));
            }
            ;
        }
        ;
    }
    ;
}
class Trig{
    public static var Cos(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Sin(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Tan(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Acos(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Asin(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Atan(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Cosh(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Sinh(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Tanh(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Acosh(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Asinh(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Atanh(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
}
class Numerical{
    public static var Pow(var x, var y){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var Log(var x, var y){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var NaturalLog(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var NaturalPower(var x){
        {
            {
                null;
            }
            ;
        }
        ;
    }
    ;
    public static var SquareRoot(var x){
        {
            {
                Pow(x, 0.5);
            }
            ;
        }
        ;
    }
    ;
    public static var CubeRoot(var x){
        {
            {
                Pow(x, 0.5);
            }
            ;
        }
        ;
    }
    ;
    public static var Square(var x){
        {
            {
                * (Value, Value);
            }
            ;
        }
        ;
    }
    ;
    public static var Clamp(var x, var min, var max){
        {
            {
                Clamp(x, Interval(min, max));
            }
            ;
        }
        ;
    }
    ;
    public static var Clamp(var x, var i){
        {
            {
                Clamp(i, x);
            }
            ;
        }
        ;
    }
    ;
    public static var Clamp(var x){
        {
            {
                Clamp(x, 0, 1);
            }
            ;
        }
        ;
    }
    ;
    public static var PlusOne(var x){
        {
            {
                + (x, 1);
            }
            ;
        }
        ;
    }
    ;
    public static var MinusOne(var x){
        {
            {
                - (x, 1);
            }
            ;
        }
        ;
    }
    ;
    public static var FromOne(var x){
        {
            {
                - (1, x);
            }
            ;
        }
        ;
    }
    ;
    public static var Sign(var x){
        {
            {
                < (x, if (0)
                {
                    -(1);
                }
                else
                {
                    > (x, if (0)
                    {
                        1;
                    }
                    else
                    {
                        0;
                    }
                    );
                }
                );
            }
            ;
        }
        ;
    }
    ;
    public static var Abs(var x){
        {
            {
                < (Value, if (0)
                {
                    -(Value);
                }
                else
                {
                    Value;
                }
                );
            }
            ;
        }
        ;
    }
    ;
    public static var Half(var x){
        {
            {
                / (x, 2);
            }
            ;
        }
        ;
    }
    ;
    public static var Third(var x){
        {
            {
                / (x, 3);
            }
            ;
        }
        ;
    }
    ;
    public static var Quarter(var x){
        {
            {
                / (x, 4);
            }
            ;
        }
        ;
    }
    ;
    public static var Fifth(var x){
        {
            {
                / (x, 5);
            }
            ;
        }
        ;
    }
    ;
    public static var Sixth(var x){
        {
            {
                / (x, 6);
            }
            ;
        }
        ;
    }
    ;
    public static var Seventh(var x){
        {
            {
                / (x, 7);
            }
            ;
        }
        ;
    }
    ;
    public static var Eighth(var x){
        {
            {
                / (x, 8);
            }
            ;
        }
        ;
    }
    ;
    public static var Ninth(var x){
        {
            {
                / (x, 9);
            }
            ;
        }
        ;
    }
    ;
    public static var Tenth(var x){
        {
            {
                / (x, 10);
            }
            ;
        }
        ;
    }
    ;
    public static var Sixteenth(var x){
        {
            {
                / (x, 16);
            }
            ;
        }
        ;
    }
    ;
    public static var Hundredth(var x){
        {
            {
                / (x, 100);
            }
            ;
        }
        ;
    }
    ;
    public static var Thousandth(var x){
        {
            {
                / (x, 1000);
            }
            ;
        }
        ;
    }
    ;
    public static var Millionth(var x){
        {
            {
                / (x, / (1000, 1000));
            }
            ;
        }
        ;
    }
    ;
    public static var Billionth(var x){
        {
            {
                / (x, / (1000, / (1000, 1000)));
            }
            ;
        }
        ;
    }
    ;
    public static var Hundred(var x){
        {
            {
                * (x, 100);
            }
            ;
        }
        ;
    }
    ;
    public static var Thousand(var x){
        {
            {
                * (x, 1000);
            }
            ;
        }
        ;
    }
    ;
    public static var Million(var x){
        {
            {
                * (x, * (1000, 1000));
            }
            ;
        }
        ;
    }
    ;
    public static var Billion(var x){
        {
            {
                * (x, * (1000, * (1000, 1000)));
            }
            ;
        }
        ;
    }
    ;
    public static var Twice(var x){
        {
            {
                * (x, 2);
            }
            ;
        }
        ;
    }
    ;
    public static var Thrice(var x){
        {
            {
                * (x, 3);
            }
            ;
        }
        ;
    }
    ;
    public static var SmoothStep(var x){
        {
            {
                * (Square(x), - (3, Twice(x)));
            }
            ;
        }
        ;
    }
    ;
    public static var Pow2(var x){
        {
            {
                * (x, x);
            }
            ;
        }
        ;
    }
    ;
    public static var Pow3(var x){
        {
            {
                * (Pow2(x), x);
            }
            ;
        }
        ;
    }
    ;
    public static var Pow4(var x){
        {
            {
                * (Pow3(x), x);
            }
            ;
        }
        ;
    }
    ;
    public static var Pow5(var x){
        {
            {
                * (Pow4(x), x);
            }
            ;
        }
        ;
    }
    ;
}
class Comparable{
    public static var Equals(var a, var b){
        {
            {
                == (null(a, b), 0);
            }
            ;
        }
        ;
    }
    ;
    public static var LessThan(var a, var b){
        {
            {
                < (null(a, b), 0);
            }
            ;
        }
        ;
    }
    ;
    public static var Lesser(var a, var b){
        {
            {
                if (LessThanOrEqual(a, b))
                {
                    a;
                }
                else
                {
                    b;
                }
                ;
            }
            ;
        }
        ;
    }
    ;
    public static var Greater(var a, var b){
        {
            {
                if (GreaterThanOrEqual(a, b))
                {
                    a;
                }
                else
                {
                    b;
                }
                ;
            }
            ;
        }
        ;
    }
    ;
    public static var LessThanOrEqual(var a, var b){
        {
            {
                <= (null(a, b), 0);
            }
            ;
        }
        ;
    }
    ;
    public static var GreaterThan(var a, var b){
        {
            {
                > (null(a, b), 0);
            }
            ;
        }
        ;
    }
    ;
    public static var GreaterThanOrEqual(var a, var b){
        {
            {
                >= (null(a, b), 0);
            }
            ;
        }
        ;
    }
    ;
    public static var Min(var a, var b){
        {
            {
                if (LessThan(a, b))
                {
                    a;
                }
                else
                {
                    b;
                }
                ;
            }
            ;
        }
        ;
    }
    ;
    public static var Max(var a, var b){
        {
            {
                if (GreaterThan(a, b))
                {
                    a;
                }
                else
                {
                    b;
                }
                ;
            }
            ;
        }
        ;
    }
    ;
    public static var Between(var v, var a, var b){
        {
            {
                Between(v, Interval(a, b));
            }
            ;
        }
        ;
    }
    ;
    public static var Between(var v, var i){
        {
            {
                null(i, v);
            }
            ;
        }
        ;
    }
    ;
}
class Equatable{
    public static var NotEquals(var x){
        {
            {
                !(null(x));
            }
            ;
        }
        ;
    }
    ;
}
class Array{
    public static var Map(var xs, var f){
        {
            {
                Map(Count(xs), Lambda lambda(var i){
                    f(null(xs, i));
                }
                );
            }
            ;
        }
        ;
    }
    ;
    public static var Map(var n, var f){
        {
            {
                Array(n, f);
            }
            ;
        }
        ;
    }
    ;
    public static var Zip(var xs, var ys, var f){
        {
            {
                Array(Count(xs), Lambda lambda(var i){
                    f(null(i), null(ys, i));
                }
                );
            }
            ;
        }
        ;
    }
    ;
    public static var Skip(var xs, var n){
        {
            {
                Array(- (Count, n), Lambda lambda(var i){
                    null(- (i, n));
                }
                );
            }
            ;
        }
        ;
    }
    ;
    public static var Take(var xs, var n){
        {
            {
                Array(n, Lambda lambda(var i){
                    null;
                }
                );
            }
            ;
        }
        ;
    }
    ;
    public static var Accumulate(var xs, var init, var f){
        {
            {
                if (IsEmpty(xs))
                {
                    init;
                }
                else
                {
                    f(init, f(Rest));
                }
                ;
            }
            ;
        }
        ;
    }
    ;
    public static var Rest(var xs){
        {
            {
                Skip(1);
            }
            ;
        }
        ;
    }
    ;
    public static var IsEmpty(var xs){
        {
            {
                == (Count(xs), 0);
            }
            ;
        }
        ;
    }
    ;
    public static var First(var xs){
        {
            {
                null(xs, 0);
            }
            ;
        }
        ;
    }
    ;
    public static var Last(var xs){
        {
            {
                null(xs, - (Count(xs), 1));
            }
            ;
        }
        ;
    }
    ;
    public static var Slice(var xs, var from, var count){
        {
            {
                Take(Skip(xs, from), count);
            }
            ;
        }
        ;
    }
    ;
    public static var Join(var xs, var sep){
        {
            {
                if (IsEmpty(xs))
                {
                    ;
                }
                else
                {
                    + (null(First(xs)), Accumulate(Skip(xs, 1), , Lambda lambda(var acc, var cur){
                        interpolate(acc, sep, cur);
                    }
                    ));
                }
                ;
            }
            ;
        }
        ;
    }
    ;
}
class Easings{
    public static var BlendEaseFunc(var p, var easeIn, var easeOut){
        {
            {
                < (p, if (0.5)
                {
                    * (0.5, easeIn(* (p, 2)));
                }
                else
                {
                    * (0.5, + (easeOut(* (p, - (2, 1))), 0.5));
                }
                );
            }
            ;
        }
        ;
    }
    ;
    public static var InvertEaseFunc(var p, var easeIn){
        {
            {
                - (1, easeIn(- (1, p)));
            }
            ;
        }
        ;
    }
    ;
    public static var Linear(var p){
        {
            {
                p;
            }
            ;
        }
        ;
    }
    ;
    public static var QuadraticEaseIn(var p){
        {
            {
                null(p);
            }
            ;
        }
        ;
    }
    ;
    public static var QuadraticEaseOut(var p){
        {
            {
                InvertEaseFunc(p, QuadraticEaseIn);
            }
            ;
        }
        ;
    }
    ;
    public static var QuadraticEaseInOut(var p){
        {
            {
                BlendEaseFunc(p, QuadraticEaseIn, QuadraticEaseOut);
            }
            ;
        }
        ;
    }
    ;
    public static var CubicEaseIn(var p){
        {
            {
                null(p);
            }
            ;
        }
        ;
    }
    ;
    public static var CubicEaseOut(var p){
        {
            {
                InvertEaseFunc(p, CubicEaseIn);
            }
            ;
        }
        ;
    }
    ;
    public static var CubicEaseInOut(var p){
        {
            {
                BlendEaseFunc(p, CubicEaseIn, CubicEaseOut);
            }
            ;
        }
        ;
    }
    ;
    public static var QuarticEaseIn(var p){
        {
            {
                null(p);
            }
            ;
        }
        ;
    }
    ;
    public static var QuarticEaseOut(var p){
        {
            {
                InvertEaseFunc(p, QuarticEaseIn);
            }
            ;
        }
        ;
    }
    ;
    public static var QuarticEaseInOut(var p){
        {
            {
                BlendEaseFunc(p, QuarticEaseIn, QuarticEaseOut);
            }
            ;
        }
        ;
    }
    ;
    public static var QuinticEaseIn(var p){
        {
            {
                null(p);
            }
            ;
        }
        ;
    }
    ;
    public static var QuinticEaseOut(var p){
        {
            {
                InvertEaseFunc(p, QuinticEaseIn);
            }
            ;
        }
        ;
    }
    ;
    public static var QuinticEaseInOut(var p){
        {
            {
                BlendEaseFunc(p, QuinticEaseIn, QuinticEaseOut);
            }
            ;
        }
        ;
    }
    ;
    public static var SineEaseIn(var p){
        {
            {
                InvertEaseFunc(p, SineEaseOut);
            }
            ;
        }
        ;
    }
    ;
    public static var SineEaseOut(var p){
        {
            {
                null(null(null(p)));
            }
            ;
        }
        ;
    }
    ;
    public static var SineEaseInOut(var p){
        {
            {
                BlendEaseFunc(p, SineEaseIn, SineEaseOut);
            }
            ;
        }
        ;
    }
    ;
    public static var CircularEaseIn(var p){
        {
            {
                null(null(null(null(p))));
            }
            ;
        }
        ;
    }
    ;
    public static var CircularEaseOut(var p){
        {
            {
                InvertEaseFunc(p, CircularEaseIn);
            }
            ;
        }
        ;
    }
    ;
    public static var CircularEaseInOut(var p){
        {
            {
                BlendEaseFunc(p, CircularEaseIn, CircularEaseOut);
            }
            ;
        }
        ;
    }
    ;
    public static var ExponentialEaseIn(var p){
        {
            {
                if (null(p))
                {
                    p;
                }
                else
                {
                    null(2, * (10, null(p)));
                }
                ;
            }
            ;
        }
        ;
    }
    ;
    public static var ExponentialEaseOut(var p){
        {
            {
                InvertEaseFunc(p, ExponentialEaseIn);
            }
            ;
        }
        ;
    }
    ;
    public static var ExponentialEaseInOut(var p){
        {
            {
                BlendEaseFunc(p, ExponentialEaseIn, ExponentialEaseOut);
            }
            ;
        }
        ;
    }
    ;
    public static var ElasticEaseIn(var p){
        {
            {
                * (13, * (null(null(p)), null(null(null(2, * (10, null(p)))))));
            }
            ;
        }
        ;
    }
    ;
    public static var ElasticEaseOut(var p){
        {
            {
                InvertEaseFunc(p, ElasticEaseIn);
            }
            ;
        }
        ;
    }
    ;
    public static var ElasticEaseInOut(var p){
        {
            {
                BlendEaseFunc(p, ElasticEaseIn, ElasticEaseOut);
            }
            ;
        }
        ;
    }
    ;
    public static var BackEaseIn(var p){
        {
            {
                - (null(p), * (p, null(null(null(p)))));
            }
            ;
        }
        ;
    }
    ;
    public static var BackEaseOut(var p){
        {
            {
                InvertEaseFunc(p, BackEaseIn);
            }
            ;
        }
        ;
    }
    ;
    public static var BackEaseInOut(var p){
        {
            {
                BlendEaseFunc(p, BackEaseIn, BackEaseOut);
            }
            ;
        }
        ;
    }
    ;
    public static var BounceEaseIn(var p){
        {
            {
                InvertEaseFunc(p, BounceEaseOut);
            }
            ;
        }
        ;
    }
    ;
    public static var BounceEaseOut(var p){
        {
            {
                if (< (p, / (4, 11)))
                {
                    * (121, / (null(p), 16));
                }
                else
                {
                    if (< (p, / (8, 11)))
                    {
                        / (363, * (40, - (null(p), / (99, * (10, + (p, / (17, 5)))))));
                    }
                    else
                    {
                        if (< (p, / (9, 10)))
                        {
                            / (4356, * (361, - (null(p), / (35442, * (1805, + (p, / (16061, 1805)))))));
                        }
                        else
                        {
                            / (54, * (5, - (null(p), / (513, * (25, + (p, / (268, 25)))))));
                        }
                        ;
                    }
                    ;
                }
                ;
            }
            ;
        }
        ;
    }
    ;
    public static var BounceEaseInOut(var p){
        {
            {
                BlendEaseFunc(p, BounceEaseIn, BounceEaseOut);
            }
            ;
        }
        ;
    }
    ;
}
