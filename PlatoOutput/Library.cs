using System;
public partial class Number
{
    public Angle Acos => throw new NotImplementedException();
    public Angle Asin => throw new NotImplementedException();
    public Angle Atan => throw new NotImplementedException();
    public Number Pow(Number y) => throw new NotImplementedException();
    public Number Log(Number y) => throw new NotImplementedException();
    public Number NaturalLog => throw new NotImplementedException();
    public Number NaturalPower => throw new NotImplementedException();
    public Number Add(Number y) => throw new NotImplementedException();
    public static Number operator +(Number x, Number y) => x.Add(y);
    public Number Subtract(Number y) => throw new NotImplementedException();
    public static Number operator -(Number x, Number y) => x.Subtract(y);
    public Number Divide(Number y) => throw new NotImplementedException();
    public static Number operator /(Number x, Number y) => x.Divide(y);
    public Number Multiply(Number y) => throw new NotImplementedException();
    public static Number operator *(Number x, Number y) => x.Multiply(y);
    public Number Modulo(Number y) => throw new NotImplementedException();
    public static Number operator %(Number x, Number y) => x.Modulo(y);
    public Number Negative => throw new NotImplementedException();
    public static Number operator -(Number x) => x.Negative;
    public Number SquareRoot => throw new NotImplementedException();
    public Number ClampOne => throw new NotImplementedException();
    public Boolean AlmostZero => throw new NotImplementedException();
    public Angle Radians => throw new NotImplementedException();
    public Angle Degrees => throw new NotImplementedException();
    public Angle Turns => throw new NotImplementedException();
    public Number Square => throw new NotImplementedException();
    public Number PlusOne => throw new NotImplementedException();
    public Number MinusOne => throw new NotImplementedException();
    public Number FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public Number Sign => throw new NotImplementedException();
    public Number Abs => throw new NotImplementedException();
    public Number Half => throw new NotImplementedException();
    public Number Quarter => throw new NotImplementedException();
    public Number Eighth => throw new NotImplementedException();
    public Number Tenth => throw new NotImplementedException();
    public Number Twice => throw new NotImplementedException();
    public Number SmoothStep => throw new NotImplementedException();
    public Number Pow2 => throw new NotImplementedException();
    public Number Lerp(Number b, Number t) => throw new NotImplementedException();
    public Boolean Between(Number min, Number max) => throw new NotImplementedException();
    public Boolean Equals(Number b) => throw new NotImplementedException();
    public static Boolean operator ==(Number a, Number b) => a.Equals(b);
    public Boolean NotEquals(Number b) => throw new NotImplementedException();
    public static Boolean operator !=(Number a, Number b) => a.NotEquals(b);
    public Boolean LessThan(Number b) => throw new NotImplementedException();
    public static Boolean operator <(Number a, Number b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Number b) => throw new NotImplementedException();
    public static Boolean operator <=(Number a, Number b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Number b) => throw new NotImplementedException();
    public static Boolean operator >(Number a, Number b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Number b) => throw new NotImplementedException();
    public static Boolean operator >=(Number a, Number b) => a.GreaterThanOrEquals(b);
    public Number Lesser(Number b) => throw new NotImplementedException();
    public Number Greater(Number b) => throw new NotImplementedException();
}
public partial class Integer
{
    public Integer Add(Integer y) => throw new NotImplementedException();
    public static Integer operator +(Integer x, Integer y) => x.Add(y);
    public Integer Subtract(Integer y) => throw new NotImplementedException();
    public static Integer operator -(Integer x, Integer y) => x.Subtract(y);
    public Integer Divide(Integer y) => throw new NotImplementedException();
    public static Integer operator /(Integer x, Integer y) => x.Divide(y);
    public Integer Multiply(Integer y) => throw new NotImplementedException();
    public static Integer operator *(Integer x, Integer y) => x.Multiply(y);
    public Integer Modulo(Integer y) => throw new NotImplementedException();
    public static Integer operator %(Integer x, Integer y) => x.Modulo(y);
    public Integer Negative => throw new NotImplementedException();
    public static Integer operator -(Integer x) => x.Negative;
    public Number ToNumber => throw new NotImplementedException();
    public Integer Square => throw new NotImplementedException();
    public Integer PlusOne => throw new NotImplementedException();
    public Integer MinusOne => throw new NotImplementedException();
    public Integer FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public Integer Sign => throw new NotImplementedException();
    public Integer Abs => throw new NotImplementedException();
    public Integer Half => throw new NotImplementedException();
    public Integer Quarter => throw new NotImplementedException();
    public Integer Eighth => throw new NotImplementedException();
    public Integer Tenth => throw new NotImplementedException();
    public Integer Twice => throw new NotImplementedException();
    public Integer SmoothStep => throw new NotImplementedException();
    public Integer Pow2 => throw new NotImplementedException();
    public Integer Lerp(Integer b, Number t) => throw new NotImplementedException();
    public Boolean Between(Integer min, Integer max) => throw new NotImplementedException();
    public Boolean Equals(Integer b) => throw new NotImplementedException();
    public static Boolean operator ==(Integer a, Integer b) => a.Equals(b);
    public Boolean NotEquals(Integer b) => throw new NotImplementedException();
    public static Boolean operator !=(Integer a, Integer b) => a.NotEquals(b);
    public Boolean LessThan(Integer b) => throw new NotImplementedException();
    public static Boolean operator <(Integer a, Integer b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Integer b) => throw new NotImplementedException();
    public static Boolean operator <=(Integer a, Integer b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Integer b) => throw new NotImplementedException();
    public static Boolean operator >(Integer a, Integer b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Integer b) => throw new NotImplementedException();
    public static Boolean operator >=(Integer a, Integer b) => a.GreaterThanOrEquals(b);
    public Integer Lesser(Integer b) => throw new NotImplementedException();
    public Integer Greater(Integer b) => throw new NotImplementedException();
}
public partial class String
{
}
public partial class Boolean
{
    public Boolean And(Boolean y) => throw new NotImplementedException();
    public static Boolean operator &(Boolean x, Boolean y) => x.And(y);
    public Boolean Or(Boolean y) => throw new NotImplementedException();
    public static Boolean operator |(Boolean x, Boolean y) => x.Or(y);
    public Boolean Not => throw new NotImplementedException();
    public static Boolean operator !(Boolean x) => x.Not;
}
public partial class Character
{
}
public partial class Dynamic
{
}
public partial class Count
{
    public Count Square => throw new NotImplementedException();
    public Count PlusOne => throw new NotImplementedException();
    public Count MinusOne => throw new NotImplementedException();
    public Count FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public Count Sign => throw new NotImplementedException();
    public Count Abs => throw new NotImplementedException();
    public Count Half => throw new NotImplementedException();
    public Count Quarter => throw new NotImplementedException();
    public Count Eighth => throw new NotImplementedException();
    public Count Tenth => throw new NotImplementedException();
    public Count Twice => throw new NotImplementedException();
    public Count SmoothStep => throw new NotImplementedException();
    public Count Pow2 => throw new NotImplementedException();
    public Count Lerp(Count b, Number t) => throw new NotImplementedException();
    public Boolean Between(Count min, Count max) => throw new NotImplementedException();
    public Boolean Equals(Count b) => throw new NotImplementedException();
    public static Boolean operator ==(Count a, Count b) => a.Equals(b);
    public Boolean NotEquals(Count b) => throw new NotImplementedException();
    public static Boolean operator !=(Count a, Count b) => a.NotEquals(b);
    public Boolean LessThan(Count b) => throw new NotImplementedException();
    public static Boolean operator <(Count a, Count b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Count b) => throw new NotImplementedException();
    public static Boolean operator <=(Count a, Count b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Count b) => throw new NotImplementedException();
    public static Boolean operator >(Count a, Count b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Count b) => throw new NotImplementedException();
    public static Boolean operator >=(Count a, Count b) => a.GreaterThanOrEquals(b);
    public Count Lesser(Count b) => throw new NotImplementedException();
    public Count Greater(Count b) => throw new NotImplementedException();
}
public partial class Index
{
}
public partial class Unit
{
    public Unit Square => throw new NotImplementedException();
    public Unit PlusOne => throw new NotImplementedException();
    public Unit MinusOne => throw new NotImplementedException();
    public Unit FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public Unit Sign => throw new NotImplementedException();
    public Unit Abs => throw new NotImplementedException();
    public Unit Half => throw new NotImplementedException();
    public Unit Quarter => throw new NotImplementedException();
    public Unit Eighth => throw new NotImplementedException();
    public Unit Tenth => throw new NotImplementedException();
    public Unit Twice => throw new NotImplementedException();
    public Unit SmoothStep => throw new NotImplementedException();
    public Unit Pow2 => throw new NotImplementedException();
    public Unit Lerp(Unit b, Number t) => throw new NotImplementedException();
    public Boolean Between(Unit min, Unit max) => throw new NotImplementedException();
    public Boolean Equals(Unit b) => throw new NotImplementedException();
    public static Boolean operator ==(Unit a, Unit b) => a.Equals(b);
    public Boolean NotEquals(Unit b) => throw new NotImplementedException();
    public static Boolean operator !=(Unit a, Unit b) => a.NotEquals(b);
    public Boolean LessThan(Unit b) => throw new NotImplementedException();
    public static Boolean operator <(Unit a, Unit b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Unit b) => throw new NotImplementedException();
    public static Boolean operator <=(Unit a, Unit b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Unit b) => throw new NotImplementedException();
    public static Boolean operator >(Unit a, Unit b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Unit b) => throw new NotImplementedException();
    public static Boolean operator >=(Unit a, Unit b) => a.GreaterThanOrEquals(b);
    public Unit Lesser(Unit b) => throw new NotImplementedException();
    public Unit Greater(Unit b) => throw new NotImplementedException();
}
public partial class Percent
{
    public Percent Square => throw new NotImplementedException();
    public Percent PlusOne => throw new NotImplementedException();
    public Percent MinusOne => throw new NotImplementedException();
    public Percent FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public Percent Sign => throw new NotImplementedException();
    public Percent Abs => throw new NotImplementedException();
    public Percent Half => throw new NotImplementedException();
    public Percent Quarter => throw new NotImplementedException();
    public Percent Eighth => throw new NotImplementedException();
    public Percent Tenth => throw new NotImplementedException();
    public Percent Twice => throw new NotImplementedException();
    public Percent SmoothStep => throw new NotImplementedException();
    public Percent Pow2 => throw new NotImplementedException();
    public Percent Lerp(Percent b, Number t) => throw new NotImplementedException();
    public Boolean Between(Percent min, Percent max) => throw new NotImplementedException();
    public Boolean Equals(Percent b) => throw new NotImplementedException();
    public static Boolean operator ==(Percent a, Percent b) => a.Equals(b);
    public Boolean NotEquals(Percent b) => throw new NotImplementedException();
    public static Boolean operator !=(Percent a, Percent b) => a.NotEquals(b);
    public Boolean LessThan(Percent b) => throw new NotImplementedException();
    public static Boolean operator <(Percent a, Percent b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Percent b) => throw new NotImplementedException();
    public static Boolean operator <=(Percent a, Percent b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Percent b) => throw new NotImplementedException();
    public static Boolean operator >(Percent a, Percent b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Percent b) => throw new NotImplementedException();
    public static Boolean operator >=(Percent a, Percent b) => a.GreaterThanOrEquals(b);
    public Percent Lesser(Percent b) => throw new NotImplementedException();
    public Percent Greater(Percent b) => throw new NotImplementedException();
}
public partial class Quaternion
{
}
public partial class Unit2D
{
}
public partial class Unit3D
{
}
public partial class Direction3D
{
}
public partial class AxisAngle
{
}
public partial class EulerAngles
{
}
public partial class Rotation3D
{
}
public partial class Vector2D
{
    public T1 Aggregate<T1>(Function2<Number, T1, T1> f) => throw new NotImplementedException();
    public Number Sum => throw new NotImplementedException();
    public Number SumSquares => throw new NotImplementedException();
    public Number MagnitudeSquared => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Number Dot(Vector2D v2) => throw new NotImplementedException();
    public Vector2D Normal => throw new NotImplementedException();
    public Number Average => throw new NotImplementedException();
    public Vector2D Square => throw new NotImplementedException();
    public Vector2D PlusOne => throw new NotImplementedException();
    public Vector2D MinusOne => throw new NotImplementedException();
    public Vector2D FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public Vector2D Sign => throw new NotImplementedException();
    public Vector2D Abs => throw new NotImplementedException();
    public Vector2D Half => throw new NotImplementedException();
    public Vector2D Quarter => throw new NotImplementedException();
    public Vector2D Eighth => throw new NotImplementedException();
    public Vector2D Tenth => throw new NotImplementedException();
    public Vector2D Twice => throw new NotImplementedException();
    public Vector2D SmoothStep => throw new NotImplementedException();
    public Vector2D Pow2 => throw new NotImplementedException();
    public Vector2D Lerp(Vector2D b, Number t) => throw new NotImplementedException();
    public Boolean Between(Vector2D min, Vector2D max) => throw new NotImplementedException();
    public Boolean Equals(Vector2D b) => throw new NotImplementedException();
    public static Boolean operator ==(Vector2D a, Vector2D b) => a.Equals(b);
    public Boolean NotEquals(Vector2D b) => throw new NotImplementedException();
    public static Boolean operator !=(Vector2D a, Vector2D b) => a.NotEquals(b);
    public Boolean LessThan(Vector2D b) => throw new NotImplementedException();
    public static Boolean operator <(Vector2D a, Vector2D b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Vector2D b) => throw new NotImplementedException();
    public static Boolean operator <=(Vector2D a, Vector2D b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Vector2D b) => throw new NotImplementedException();
    public static Boolean operator >(Vector2D a, Vector2D b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Vector2D b) => throw new NotImplementedException();
    public static Boolean operator >=(Vector2D a, Vector2D b) => a.GreaterThanOrEquals(b);
    public Vector2D Lesser(Vector2D b) => throw new NotImplementedException();
    public Vector2D Greater(Vector2D b) => throw new NotImplementedException();
}
public partial class Vector3D
{
    public T1 Aggregate<T1>(Function2<Number, T1, T1> f) => throw new NotImplementedException();
    public Number Sum => throw new NotImplementedException();
    public Number SumSquares => throw new NotImplementedException();
    public Number MagnitudeSquared => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Number Dot(Vector3D v2) => throw new NotImplementedException();
    public Vector3D Normal => throw new NotImplementedException();
    public Number Average => throw new NotImplementedException();
    public Vector3D Square => throw new NotImplementedException();
    public Vector3D PlusOne => throw new NotImplementedException();
    public Vector3D MinusOne => throw new NotImplementedException();
    public Vector3D FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public Vector3D Sign => throw new NotImplementedException();
    public Vector3D Abs => throw new NotImplementedException();
    public Vector3D Half => throw new NotImplementedException();
    public Vector3D Quarter => throw new NotImplementedException();
    public Vector3D Eighth => throw new NotImplementedException();
    public Vector3D Tenth => throw new NotImplementedException();
    public Vector3D Twice => throw new NotImplementedException();
    public Vector3D SmoothStep => throw new NotImplementedException();
    public Vector3D Pow2 => throw new NotImplementedException();
    public Vector3D Lerp(Vector3D b, Number t) => throw new NotImplementedException();
    public Boolean Between(Vector3D min, Vector3D max) => throw new NotImplementedException();
    public Boolean Equals(Vector3D b) => throw new NotImplementedException();
    public static Boolean operator ==(Vector3D a, Vector3D b) => a.Equals(b);
    public Boolean NotEquals(Vector3D b) => throw new NotImplementedException();
    public static Boolean operator !=(Vector3D a, Vector3D b) => a.NotEquals(b);
    public Boolean LessThan(Vector3D b) => throw new NotImplementedException();
    public static Boolean operator <(Vector3D a, Vector3D b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Vector3D b) => throw new NotImplementedException();
    public static Boolean operator <=(Vector3D a, Vector3D b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Vector3D b) => throw new NotImplementedException();
    public static Boolean operator >(Vector3D a, Vector3D b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Vector3D b) => throw new NotImplementedException();
    public static Boolean operator >=(Vector3D a, Vector3D b) => a.GreaterThanOrEquals(b);
    public Vector3D Lesser(Vector3D b) => throw new NotImplementedException();
    public Vector3D Greater(Vector3D b) => throw new NotImplementedException();
}
public partial class Vector4D
{
    public T1 Aggregate<T1>(Function2<Number, T1, T1> f) => throw new NotImplementedException();
    public Number Sum => throw new NotImplementedException();
    public Number SumSquares => throw new NotImplementedException();
    public Number MagnitudeSquared => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Number Dot(Vector4D v2) => throw new NotImplementedException();
    public Vector4D Normal => throw new NotImplementedException();
    public Number Average => throw new NotImplementedException();
    public Vector4D Square => throw new NotImplementedException();
    public Vector4D PlusOne => throw new NotImplementedException();
    public Vector4D MinusOne => throw new NotImplementedException();
    public Vector4D FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public Vector4D Sign => throw new NotImplementedException();
    public Vector4D Abs => throw new NotImplementedException();
    public Vector4D Half => throw new NotImplementedException();
    public Vector4D Quarter => throw new NotImplementedException();
    public Vector4D Eighth => throw new NotImplementedException();
    public Vector4D Tenth => throw new NotImplementedException();
    public Vector4D Twice => throw new NotImplementedException();
    public Vector4D SmoothStep => throw new NotImplementedException();
    public Vector4D Pow2 => throw new NotImplementedException();
    public Vector4D Lerp(Vector4D b, Number t) => throw new NotImplementedException();
    public Boolean Between(Vector4D min, Vector4D max) => throw new NotImplementedException();
    public Boolean Equals(Vector4D b) => throw new NotImplementedException();
    public static Boolean operator ==(Vector4D a, Vector4D b) => a.Equals(b);
    public Boolean NotEquals(Vector4D b) => throw new NotImplementedException();
    public static Boolean operator !=(Vector4D a, Vector4D b) => a.NotEquals(b);
    public Boolean LessThan(Vector4D b) => throw new NotImplementedException();
    public static Boolean operator <(Vector4D a, Vector4D b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Vector4D b) => throw new NotImplementedException();
    public static Boolean operator <=(Vector4D a, Vector4D b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Vector4D b) => throw new NotImplementedException();
    public static Boolean operator >(Vector4D a, Vector4D b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Vector4D b) => throw new NotImplementedException();
    public static Boolean operator >=(Vector4D a, Vector4D b) => a.GreaterThanOrEquals(b);
    public Vector4D Lesser(Vector4D b) => throw new NotImplementedException();
    public Vector4D Greater(Vector4D b) => throw new NotImplementedException();
}
public partial class Orientation3D
{
}
public partial class Pose2D
{
}
public partial class Pose3D
{
}
public partial class Transform3D
{
}
public partial class Transform2D
{
}
public partial class AlignedBox2D
{
    public Boolean IsEmpty => throw new NotImplementedException();
    public Point2D Lerp(Number amount) => throw new NotImplementedException();
    public Number Unlerp(Point2D value) => throw new NotImplementedException();
    public AlignedBox2D Negate => throw new NotImplementedException();
    public AlignedBox2D Reverse => throw new NotImplementedException();
    public Point2D Center => throw new NotImplementedException();
    public Boolean Contains(Point2D value) => throw new NotImplementedException();
    public Boolean Contains(AlignedBox2D other) => throw new NotImplementedException();
    public Boolean Overlaps(AlignedBox2D y) => throw new NotImplementedException();
    public Tuple2<AlignedBox2D, AlignedBox2D> Split(Number t) => throw new NotImplementedException();
    public Tuple2<AlignedBox2D, AlignedBox2D> Split => throw new NotImplementedException();
    public AlignedBox2D Left(Number t) => throw new NotImplementedException();
    public AlignedBox2D Right(Number t) => throw new NotImplementedException();
    public AlignedBox2D MoveTo(Point2D v) => throw new NotImplementedException();
    public AlignedBox2D LeftHalf => throw new NotImplementedException();
    public AlignedBox2D RightHalf => throw new NotImplementedException();
    public AlignedBox2D Recenter(Point2D c) => throw new NotImplementedException();
    public AlignedBox2D Clamp(AlignedBox2D y) => throw new NotImplementedException();
    public Point2D Clamp(Point2D value) => throw new NotImplementedException();
    public Boolean Within(Point2D value) => throw new NotImplementedException();
}
public partial class AlignedBox3D
{
    public Boolean IsEmpty => throw new NotImplementedException();
    public Point3D Lerp(Number amount) => throw new NotImplementedException();
    public Number Unlerp(Point3D value) => throw new NotImplementedException();
    public AlignedBox3D Negate => throw new NotImplementedException();
    public AlignedBox3D Reverse => throw new NotImplementedException();
    public Point3D Center => throw new NotImplementedException();
    public Boolean Contains(Point3D value) => throw new NotImplementedException();
    public Boolean Contains(AlignedBox3D other) => throw new NotImplementedException();
    public Boolean Overlaps(AlignedBox3D y) => throw new NotImplementedException();
    public Tuple2<AlignedBox3D, AlignedBox3D> Split(Number t) => throw new NotImplementedException();
    public Tuple2<AlignedBox3D, AlignedBox3D> Split => throw new NotImplementedException();
    public AlignedBox3D Left(Number t) => throw new NotImplementedException();
    public AlignedBox3D Right(Number t) => throw new NotImplementedException();
    public AlignedBox3D MoveTo(Point3D v) => throw new NotImplementedException();
    public AlignedBox3D LeftHalf => throw new NotImplementedException();
    public AlignedBox3D RightHalf => throw new NotImplementedException();
    public AlignedBox3D Recenter(Point3D c) => throw new NotImplementedException();
    public AlignedBox3D Clamp(AlignedBox3D y) => throw new NotImplementedException();
    public Point3D Clamp(Point3D value) => throw new NotImplementedException();
    public Boolean Within(Point3D value) => throw new NotImplementedException();
}
public partial class Complex
{
    public T1 Aggregate<T1>(Function2<Number, T1, T1> f) => throw new NotImplementedException();
    public Number Sum => throw new NotImplementedException();
    public Number SumSquares => throw new NotImplementedException();
    public Number MagnitudeSquared => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Number Dot(Complex v2) => throw new NotImplementedException();
    public Complex Normal => throw new NotImplementedException();
    public Number Average => throw new NotImplementedException();
    public Complex Square => throw new NotImplementedException();
    public Complex PlusOne => throw new NotImplementedException();
    public Complex MinusOne => throw new NotImplementedException();
    public Complex FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public Complex Sign => throw new NotImplementedException();
    public Complex Abs => throw new NotImplementedException();
    public Complex Half => throw new NotImplementedException();
    public Complex Quarter => throw new NotImplementedException();
    public Complex Eighth => throw new NotImplementedException();
    public Complex Tenth => throw new NotImplementedException();
    public Complex Twice => throw new NotImplementedException();
    public Complex SmoothStep => throw new NotImplementedException();
    public Complex Pow2 => throw new NotImplementedException();
    public Complex Lerp(Complex b, Number t) => throw new NotImplementedException();
    public Boolean Between(Complex min, Complex max) => throw new NotImplementedException();
    public Boolean Equals(Complex b) => throw new NotImplementedException();
    public static Boolean operator ==(Complex a, Complex b) => a.Equals(b);
    public Boolean NotEquals(Complex b) => throw new NotImplementedException();
    public static Boolean operator !=(Complex a, Complex b) => a.NotEquals(b);
    public Boolean LessThan(Complex b) => throw new NotImplementedException();
    public static Boolean operator <(Complex a, Complex b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Complex b) => throw new NotImplementedException();
    public static Boolean operator <=(Complex a, Complex b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Complex b) => throw new NotImplementedException();
    public static Boolean operator >(Complex a, Complex b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Complex b) => throw new NotImplementedException();
    public static Boolean operator >=(Complex a, Complex b) => a.GreaterThanOrEquals(b);
    public Complex Lesser(Complex b) => throw new NotImplementedException();
    public Complex Greater(Complex b) => throw new NotImplementedException();
}
public partial class Ray3D
{
}
public partial class Ray2D
{
}
public partial class Sphere
{
}
public partial class Plane
{
}
public partial class Triangle2D
{
}
public partial class Triangle3D
{
}
public partial class Quad2D
{
}
public partial class Quad3D
{
}
public partial class Point2D
{
}
public partial class Point3D
{
}
public partial class Line2D
{
    public Boolean IsEmpty => throw new NotImplementedException();
    public Point2D Lerp(Number amount) => throw new NotImplementedException();
    public Number Unlerp(Point2D value) => throw new NotImplementedException();
    public Line2D Negate => throw new NotImplementedException();
    public Line2D Reverse => throw new NotImplementedException();
    public Point2D Center => throw new NotImplementedException();
    public Boolean Contains(Point2D value) => throw new NotImplementedException();
    public Boolean Contains(Line2D other) => throw new NotImplementedException();
    public Boolean Overlaps(Line2D y) => throw new NotImplementedException();
    public Tuple2<Line2D, Line2D> Split(Number t) => throw new NotImplementedException();
    public Tuple2<Line2D, Line2D> Split => throw new NotImplementedException();
    public Line2D Left(Number t) => throw new NotImplementedException();
    public Line2D Right(Number t) => throw new NotImplementedException();
    public Line2D MoveTo(Point2D v) => throw new NotImplementedException();
    public Line2D LeftHalf => throw new NotImplementedException();
    public Line2D RightHalf => throw new NotImplementedException();
    public Line2D Recenter(Point2D c) => throw new NotImplementedException();
    public Line2D Clamp(Line2D y) => throw new NotImplementedException();
    public Point2D Clamp(Point2D value) => throw new NotImplementedException();
    public Boolean Within(Point2D value) => throw new NotImplementedException();
}
public partial class Line3D
{
    public Boolean IsEmpty => throw new NotImplementedException();
    public Point3D Lerp(Number amount) => throw new NotImplementedException();
    public Number Unlerp(Point3D value) => throw new NotImplementedException();
    public Line3D Negate => throw new NotImplementedException();
    public Line3D Reverse => throw new NotImplementedException();
    public Point3D Center => throw new NotImplementedException();
    public Boolean Contains(Point3D value) => throw new NotImplementedException();
    public Boolean Contains(Line3D other) => throw new NotImplementedException();
    public Boolean Overlaps(Line3D y) => throw new NotImplementedException();
    public Tuple2<Line3D, Line3D> Split(Number t) => throw new NotImplementedException();
    public Tuple2<Line3D, Line3D> Split => throw new NotImplementedException();
    public Line3D Left(Number t) => throw new NotImplementedException();
    public Line3D Right(Number t) => throw new NotImplementedException();
    public Line3D MoveTo(Point3D v) => throw new NotImplementedException();
    public Line3D LeftHalf => throw new NotImplementedException();
    public Line3D RightHalf => throw new NotImplementedException();
    public Line3D Recenter(Point3D c) => throw new NotImplementedException();
    public Line3D Clamp(Line3D y) => throw new NotImplementedException();
    public Point3D Clamp(Point3D value) => throw new NotImplementedException();
    public Boolean Within(Point3D value) => throw new NotImplementedException();
}
public partial class Color
{
}
public partial class ColorLUV
{
}
public partial class ColorLAB
{
}
public partial class ColorLCh
{
}
public partial class ColorHSV
{
}
public partial class ColorHSL
{
}
public partial class ColorYCbCr
{
}
public partial class SphericalCoordinate
{
}
public partial class PolarCoordinate
{
}
public partial class LogPolarCoordinate
{
}
public partial class CylindricalCoordinate
{
}
public partial class HorizontalCoordinate
{
}
public partial class GeoCoordinate
{
}
public partial class GeoCoordinateWithAltitude
{
}
public partial class Circle
{
}
public partial class Chord
{
}
public partial class Size2D
{
}
public partial class Size3D
{
}
public partial class Rectangle2D
{
}
public partial class Proportion
{
    public Proportion Square => throw new NotImplementedException();
    public Proportion PlusOne => throw new NotImplementedException();
    public Proportion MinusOne => throw new NotImplementedException();
    public Proportion FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public Proportion Sign => throw new NotImplementedException();
    public Proportion Abs => throw new NotImplementedException();
    public Proportion Half => throw new NotImplementedException();
    public Proportion Quarter => throw new NotImplementedException();
    public Proportion Eighth => throw new NotImplementedException();
    public Proportion Tenth => throw new NotImplementedException();
    public Proportion Twice => throw new NotImplementedException();
    public Proportion SmoothStep => throw new NotImplementedException();
    public Proportion Pow2 => throw new NotImplementedException();
    public Proportion Lerp(Proportion b, Number t) => throw new NotImplementedException();
    public Boolean Between(Proportion min, Proportion max) => throw new NotImplementedException();
    public Boolean Equals(Proportion b) => throw new NotImplementedException();
    public static Boolean operator ==(Proportion a, Proportion b) => a.Equals(b);
    public Boolean NotEquals(Proportion b) => throw new NotImplementedException();
    public static Boolean operator !=(Proportion a, Proportion b) => a.NotEquals(b);
    public Boolean LessThan(Proportion b) => throw new NotImplementedException();
    public static Boolean operator <(Proportion a, Proportion b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Proportion b) => throw new NotImplementedException();
    public static Boolean operator <=(Proportion a, Proportion b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Proportion b) => throw new NotImplementedException();
    public static Boolean operator >(Proportion a, Proportion b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Proportion b) => throw new NotImplementedException();
    public static Boolean operator >=(Proportion a, Proportion b) => a.GreaterThanOrEquals(b);
    public Proportion Lesser(Proportion b) => throw new NotImplementedException();
    public Proportion Greater(Proportion b) => throw new NotImplementedException();
}
public partial class Fraction
{
}
public partial class Angle
{
    public Number Cos => throw new NotImplementedException();
    public Number Sin => throw new NotImplementedException();
    public Number Tan => throw new NotImplementedException();
    public Number Degrees => throw new NotImplementedException();
    public Number Turns => throw new NotImplementedException();
}
public partial class Length
{
    public Boolean Between(Length min, Length max) => throw new NotImplementedException();
    public Boolean Equals(Length b) => throw new NotImplementedException();
    public static Boolean operator ==(Length a, Length b) => a.Equals(b);
    public Boolean NotEquals(Length b) => throw new NotImplementedException();
    public static Boolean operator !=(Length a, Length b) => a.NotEquals(b);
    public Boolean LessThan(Length b) => throw new NotImplementedException();
    public static Boolean operator <(Length a, Length b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Length b) => throw new NotImplementedException();
    public static Boolean operator <=(Length a, Length b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Length b) => throw new NotImplementedException();
    public static Boolean operator >(Length a, Length b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Length b) => throw new NotImplementedException();
    public static Boolean operator >=(Length a, Length b) => a.GreaterThanOrEquals(b);
    public Length Lesser(Length b) => throw new NotImplementedException();
    public Length Greater(Length b) => throw new NotImplementedException();
}
public partial class Mass
{
    public Boolean Between(Mass min, Mass max) => throw new NotImplementedException();
    public Boolean Equals(Mass b) => throw new NotImplementedException();
    public static Boolean operator ==(Mass a, Mass b) => a.Equals(b);
    public Boolean NotEquals(Mass b) => throw new NotImplementedException();
    public static Boolean operator !=(Mass a, Mass b) => a.NotEquals(b);
    public Boolean LessThan(Mass b) => throw new NotImplementedException();
    public static Boolean operator <(Mass a, Mass b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Mass b) => throw new NotImplementedException();
    public static Boolean operator <=(Mass a, Mass b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Mass b) => throw new NotImplementedException();
    public static Boolean operator >(Mass a, Mass b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Mass b) => throw new NotImplementedException();
    public static Boolean operator >=(Mass a, Mass b) => a.GreaterThanOrEquals(b);
    public Mass Lesser(Mass b) => throw new NotImplementedException();
    public Mass Greater(Mass b) => throw new NotImplementedException();
}
public partial class Temperature
{
    public Boolean Between(Temperature min, Temperature max) => throw new NotImplementedException();
    public Boolean Equals(Temperature b) => throw new NotImplementedException();
    public static Boolean operator ==(Temperature a, Temperature b) => a.Equals(b);
    public Boolean NotEquals(Temperature b) => throw new NotImplementedException();
    public static Boolean operator !=(Temperature a, Temperature b) => a.NotEquals(b);
    public Boolean LessThan(Temperature b) => throw new NotImplementedException();
    public static Boolean operator <(Temperature a, Temperature b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Temperature b) => throw new NotImplementedException();
    public static Boolean operator <=(Temperature a, Temperature b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Temperature b) => throw new NotImplementedException();
    public static Boolean operator >(Temperature a, Temperature b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Temperature b) => throw new NotImplementedException();
    public static Boolean operator >=(Temperature a, Temperature b) => a.GreaterThanOrEquals(b);
    public Temperature Lesser(Temperature b) => throw new NotImplementedException();
    public Temperature Greater(Temperature b) => throw new NotImplementedException();
}
public partial class Time
{
    public Boolean Between(Time min, Time max) => throw new NotImplementedException();
    public Boolean Equals(Time b) => throw new NotImplementedException();
    public static Boolean operator ==(Time a, Time b) => a.Equals(b);
    public Boolean NotEquals(Time b) => throw new NotImplementedException();
    public static Boolean operator !=(Time a, Time b) => a.NotEquals(b);
    public Boolean LessThan(Time b) => throw new NotImplementedException();
    public static Boolean operator <(Time a, Time b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Time b) => throw new NotImplementedException();
    public static Boolean operator <=(Time a, Time b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Time b) => throw new NotImplementedException();
    public static Boolean operator >(Time a, Time b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Time b) => throw new NotImplementedException();
    public static Boolean operator >=(Time a, Time b) => a.GreaterThanOrEquals(b);
    public Time Lesser(Time b) => throw new NotImplementedException();
    public Time Greater(Time b) => throw new NotImplementedException();
}
public partial class TimeRange
{
    public Boolean IsEmpty => throw new NotImplementedException();
    public DateTime Lerp(Number amount) => throw new NotImplementedException();
    public Number Unlerp(DateTime value) => throw new NotImplementedException();
    public TimeRange Negate => throw new NotImplementedException();
    public TimeRange Reverse => throw new NotImplementedException();
    public DateTime Center => throw new NotImplementedException();
    public Boolean Contains(DateTime value) => throw new NotImplementedException();
    public Boolean Contains(TimeRange other) => throw new NotImplementedException();
    public Boolean Overlaps(TimeRange y) => throw new NotImplementedException();
    public Tuple2<TimeRange, TimeRange> Split(Number t) => throw new NotImplementedException();
    public Tuple2<TimeRange, TimeRange> Split => throw new NotImplementedException();
    public TimeRange Left(Number t) => throw new NotImplementedException();
    public TimeRange Right(Number t) => throw new NotImplementedException();
    public TimeRange MoveTo(DateTime v) => throw new NotImplementedException();
    public TimeRange LeftHalf => throw new NotImplementedException();
    public TimeRange RightHalf => throw new NotImplementedException();
    public TimeRange Recenter(DateTime c) => throw new NotImplementedException();
    public TimeRange Clamp(TimeRange y) => throw new NotImplementedException();
    public DateTime Clamp(DateTime value) => throw new NotImplementedException();
    public Boolean Within(DateTime value) => throw new NotImplementedException();
}
public partial class DateTime
{
}
public partial class AnglePair
{
    public Boolean IsEmpty => throw new NotImplementedException();
    public Angle Lerp(Number amount) => throw new NotImplementedException();
    public Number Unlerp(Angle value) => throw new NotImplementedException();
    public AnglePair Negate => throw new NotImplementedException();
    public AnglePair Reverse => throw new NotImplementedException();
    public Angle Center => throw new NotImplementedException();
    public Boolean Contains(Angle value) => throw new NotImplementedException();
    public Boolean Contains(AnglePair other) => throw new NotImplementedException();
    public Boolean Overlaps(AnglePair y) => throw new NotImplementedException();
    public Tuple2<AnglePair, AnglePair> Split(Number t) => throw new NotImplementedException();
    public Tuple2<AnglePair, AnglePair> Split => throw new NotImplementedException();
    public AnglePair Left(Number t) => throw new NotImplementedException();
    public AnglePair Right(Number t) => throw new NotImplementedException();
    public AnglePair MoveTo(Angle v) => throw new NotImplementedException();
    public AnglePair LeftHalf => throw new NotImplementedException();
    public AnglePair RightHalf => throw new NotImplementedException();
    public AnglePair Recenter(Angle c) => throw new NotImplementedException();
    public AnglePair Clamp(AnglePair y) => throw new NotImplementedException();
    public Angle Clamp(Angle value) => throw new NotImplementedException();
    public Boolean Within(Angle value) => throw new NotImplementedException();
}
public partial class Ring
{
}
public partial class Arc
{
}
public partial class RealInterval
{
    public Boolean IsEmpty => throw new NotImplementedException();
    public Number Lerp(Number amount) => throw new NotImplementedException();
    public Number Unlerp(Number value) => throw new NotImplementedException();
    public RealInterval Negate => throw new NotImplementedException();
    public RealInterval Reverse => throw new NotImplementedException();
    public Number Center => throw new NotImplementedException();
    public Boolean Contains(Number value) => throw new NotImplementedException();
    public Boolean Contains(RealInterval other) => throw new NotImplementedException();
    public Boolean Overlaps(RealInterval y) => throw new NotImplementedException();
    public Tuple2<RealInterval, RealInterval> Split(Number t) => throw new NotImplementedException();
    public Tuple2<RealInterval, RealInterval> Split => throw new NotImplementedException();
    public RealInterval Left(Number t) => throw new NotImplementedException();
    public RealInterval Right(Number t) => throw new NotImplementedException();
    public RealInterval MoveTo(Number v) => throw new NotImplementedException();
    public RealInterval LeftHalf => throw new NotImplementedException();
    public RealInterval RightHalf => throw new NotImplementedException();
    public RealInterval Recenter(Number c) => throw new NotImplementedException();
    public RealInterval Clamp(RealInterval y) => throw new NotImplementedException();
    public Number Clamp(Number value) => throw new NotImplementedException();
    public Boolean Within(Number value) => throw new NotImplementedException();
}
public partial class Capsule
{
}
public partial class Matrix3D
{
}
public partial class Cylinder
{
}
public partial class Cone
{
}
public partial class Tube
{
}
public partial class ConeSegment
{
}
public partial class Box2D
{
}
public partial class Box3D
{
}
public partial class UV
{
    public T1 Aggregate<T1>(Function2<Number, T1, T1> f) => throw new NotImplementedException();
    public Number Sum => throw new NotImplementedException();
    public Number SumSquares => throw new NotImplementedException();
    public Number MagnitudeSquared => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Number Dot(UV v2) => throw new NotImplementedException();
    public UV Normal => throw new NotImplementedException();
    public Number Average => throw new NotImplementedException();
    public UV Square => throw new NotImplementedException();
    public UV PlusOne => throw new NotImplementedException();
    public UV MinusOne => throw new NotImplementedException();
    public UV FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public UV Sign => throw new NotImplementedException();
    public UV Abs => throw new NotImplementedException();
    public UV Half => throw new NotImplementedException();
    public UV Quarter => throw new NotImplementedException();
    public UV Eighth => throw new NotImplementedException();
    public UV Tenth => throw new NotImplementedException();
    public UV Twice => throw new NotImplementedException();
    public UV SmoothStep => throw new NotImplementedException();
    public UV Pow2 => throw new NotImplementedException();
    public UV Lerp(UV b, Number t) => throw new NotImplementedException();
    public Boolean Between(UV min, UV max) => throw new NotImplementedException();
    public Boolean Equals(UV b) => throw new NotImplementedException();
    public static Boolean operator ==(UV a, UV b) => a.Equals(b);
    public Boolean NotEquals(UV b) => throw new NotImplementedException();
    public static Boolean operator !=(UV a, UV b) => a.NotEquals(b);
    public Boolean LessThan(UV b) => throw new NotImplementedException();
    public static Boolean operator <(UV a, UV b) => a.LessThan(b);
    public Boolean LessThanOrEquals(UV b) => throw new NotImplementedException();
    public static Boolean operator <=(UV a, UV b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(UV b) => throw new NotImplementedException();
    public static Boolean operator >(UV a, UV b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(UV b) => throw new NotImplementedException();
    public static Boolean operator >=(UV a, UV b) => a.GreaterThanOrEquals(b);
    public UV Lesser(UV b) => throw new NotImplementedException();
    public UV Greater(UV b) => throw new NotImplementedException();
}
public partial class UVW
{
    public T1 Aggregate<T1>(Function2<Number, T1, T1> f) => throw new NotImplementedException();
    public Number Sum => throw new NotImplementedException();
    public Number SumSquares => throw new NotImplementedException();
    public Number MagnitudeSquared => throw new NotImplementedException();
    public Number Magnitude => throw new NotImplementedException();
    public Number Dot(UVW v2) => throw new NotImplementedException();
    public UVW Normal => throw new NotImplementedException();
    public Number Average => throw new NotImplementedException();
    public UVW Square => throw new NotImplementedException();
    public UVW PlusOne => throw new NotImplementedException();
    public UVW MinusOne => throw new NotImplementedException();
    public UVW FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public UVW Sign => throw new NotImplementedException();
    public UVW Abs => throw new NotImplementedException();
    public UVW Half => throw new NotImplementedException();
    public UVW Quarter => throw new NotImplementedException();
    public UVW Eighth => throw new NotImplementedException();
    public UVW Tenth => throw new NotImplementedException();
    public UVW Twice => throw new NotImplementedException();
    public UVW SmoothStep => throw new NotImplementedException();
    public UVW Pow2 => throw new NotImplementedException();
    public UVW Lerp(UVW b, Number t) => throw new NotImplementedException();
    public Boolean Between(UVW min, UVW max) => throw new NotImplementedException();
    public Boolean Equals(UVW b) => throw new NotImplementedException();
    public static Boolean operator ==(UVW a, UVW b) => a.Equals(b);
    public Boolean NotEquals(UVW b) => throw new NotImplementedException();
    public static Boolean operator !=(UVW a, UVW b) => a.NotEquals(b);
    public Boolean LessThan(UVW b) => throw new NotImplementedException();
    public static Boolean operator <(UVW a, UVW b) => a.LessThan(b);
    public Boolean LessThanOrEquals(UVW b) => throw new NotImplementedException();
    public static Boolean operator <=(UVW a, UVW b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(UVW b) => throw new NotImplementedException();
    public static Boolean operator >(UVW a, UVW b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(UVW b) => throw new NotImplementedException();
    public static Boolean operator >=(UVW a, UVW b) => a.GreaterThanOrEquals(b);
    public UVW Lesser(UVW b) => throw new NotImplementedException();
    public UVW Greater(UVW b) => throw new NotImplementedException();
}
public partial class CubicBezier2D
{
}
public partial class CubicBezier3D
{
}
public partial class QuadraticBezier2D
{
}
public partial class QuadraticBezier3D
{
}
public partial class Area
{
    public Boolean Between(Area min, Area max) => throw new NotImplementedException();
    public Boolean Equals(Area b) => throw new NotImplementedException();
    public static Boolean operator ==(Area a, Area b) => a.Equals(b);
    public Boolean NotEquals(Area b) => throw new NotImplementedException();
    public static Boolean operator !=(Area a, Area b) => a.NotEquals(b);
    public Boolean LessThan(Area b) => throw new NotImplementedException();
    public static Boolean operator <(Area a, Area b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Area b) => throw new NotImplementedException();
    public static Boolean operator <=(Area a, Area b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Area b) => throw new NotImplementedException();
    public static Boolean operator >(Area a, Area b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Area b) => throw new NotImplementedException();
    public static Boolean operator >=(Area a, Area b) => a.GreaterThanOrEquals(b);
    public Area Lesser(Area b) => throw new NotImplementedException();
    public Area Greater(Area b) => throw new NotImplementedException();
}
public partial class Volume
{
    public Boolean Between(Volume min, Volume max) => throw new NotImplementedException();
    public Boolean Equals(Volume b) => throw new NotImplementedException();
    public static Boolean operator ==(Volume a, Volume b) => a.Equals(b);
    public Boolean NotEquals(Volume b) => throw new NotImplementedException();
    public static Boolean operator !=(Volume a, Volume b) => a.NotEquals(b);
    public Boolean LessThan(Volume b) => throw new NotImplementedException();
    public static Boolean operator <(Volume a, Volume b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Volume b) => throw new NotImplementedException();
    public static Boolean operator <=(Volume a, Volume b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Volume b) => throw new NotImplementedException();
    public static Boolean operator >(Volume a, Volume b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Volume b) => throw new NotImplementedException();
    public static Boolean operator >=(Volume a, Volume b) => a.GreaterThanOrEquals(b);
    public Volume Lesser(Volume b) => throw new NotImplementedException();
    public Volume Greater(Volume b) => throw new NotImplementedException();
}
public partial class Velocity
{
    public Boolean Between(Velocity min, Velocity max) => throw new NotImplementedException();
    public Boolean Equals(Velocity b) => throw new NotImplementedException();
    public static Boolean operator ==(Velocity a, Velocity b) => a.Equals(b);
    public Boolean NotEquals(Velocity b) => throw new NotImplementedException();
    public static Boolean operator !=(Velocity a, Velocity b) => a.NotEquals(b);
    public Boolean LessThan(Velocity b) => throw new NotImplementedException();
    public static Boolean operator <(Velocity a, Velocity b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Velocity b) => throw new NotImplementedException();
    public static Boolean operator <=(Velocity a, Velocity b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Velocity b) => throw new NotImplementedException();
    public static Boolean operator >(Velocity a, Velocity b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Velocity b) => throw new NotImplementedException();
    public static Boolean operator >=(Velocity a, Velocity b) => a.GreaterThanOrEquals(b);
    public Velocity Lesser(Velocity b) => throw new NotImplementedException();
    public Velocity Greater(Velocity b) => throw new NotImplementedException();
}
public partial class Acceleration
{
    public Boolean Between(Acceleration min, Acceleration max) => throw new NotImplementedException();
    public Boolean Equals(Acceleration b) => throw new NotImplementedException();
    public static Boolean operator ==(Acceleration a, Acceleration b) => a.Equals(b);
    public Boolean NotEquals(Acceleration b) => throw new NotImplementedException();
    public static Boolean operator !=(Acceleration a, Acceleration b) => a.NotEquals(b);
    public Boolean LessThan(Acceleration b) => throw new NotImplementedException();
    public static Boolean operator <(Acceleration a, Acceleration b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Acceleration b) => throw new NotImplementedException();
    public static Boolean operator <=(Acceleration a, Acceleration b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Acceleration b) => throw new NotImplementedException();
    public static Boolean operator >(Acceleration a, Acceleration b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Acceleration b) => throw new NotImplementedException();
    public static Boolean operator >=(Acceleration a, Acceleration b) => a.GreaterThanOrEquals(b);
    public Acceleration Lesser(Acceleration b) => throw new NotImplementedException();
    public Acceleration Greater(Acceleration b) => throw new NotImplementedException();
}
public partial class Force
{
    public Boolean Between(Force min, Force max) => throw new NotImplementedException();
    public Boolean Equals(Force b) => throw new NotImplementedException();
    public static Boolean operator ==(Force a, Force b) => a.Equals(b);
    public Boolean NotEquals(Force b) => throw new NotImplementedException();
    public static Boolean operator !=(Force a, Force b) => a.NotEquals(b);
    public Boolean LessThan(Force b) => throw new NotImplementedException();
    public static Boolean operator <(Force a, Force b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Force b) => throw new NotImplementedException();
    public static Boolean operator <=(Force a, Force b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Force b) => throw new NotImplementedException();
    public static Boolean operator >(Force a, Force b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Force b) => throw new NotImplementedException();
    public static Boolean operator >=(Force a, Force b) => a.GreaterThanOrEquals(b);
    public Force Lesser(Force b) => throw new NotImplementedException();
    public Force Greater(Force b) => throw new NotImplementedException();
}
public partial class Pressure
{
    public Boolean Between(Pressure min, Pressure max) => throw new NotImplementedException();
    public Boolean Equals(Pressure b) => throw new NotImplementedException();
    public static Boolean operator ==(Pressure a, Pressure b) => a.Equals(b);
    public Boolean NotEquals(Pressure b) => throw new NotImplementedException();
    public static Boolean operator !=(Pressure a, Pressure b) => a.NotEquals(b);
    public Boolean LessThan(Pressure b) => throw new NotImplementedException();
    public static Boolean operator <(Pressure a, Pressure b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Pressure b) => throw new NotImplementedException();
    public static Boolean operator <=(Pressure a, Pressure b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Pressure b) => throw new NotImplementedException();
    public static Boolean operator >(Pressure a, Pressure b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Pressure b) => throw new NotImplementedException();
    public static Boolean operator >=(Pressure a, Pressure b) => a.GreaterThanOrEquals(b);
    public Pressure Lesser(Pressure b) => throw new NotImplementedException();
    public Pressure Greater(Pressure b) => throw new NotImplementedException();
}
public partial class Energy
{
    public Boolean Between(Energy min, Energy max) => throw new NotImplementedException();
    public Boolean Equals(Energy b) => throw new NotImplementedException();
    public static Boolean operator ==(Energy a, Energy b) => a.Equals(b);
    public Boolean NotEquals(Energy b) => throw new NotImplementedException();
    public static Boolean operator !=(Energy a, Energy b) => a.NotEquals(b);
    public Boolean LessThan(Energy b) => throw new NotImplementedException();
    public static Boolean operator <(Energy a, Energy b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Energy b) => throw new NotImplementedException();
    public static Boolean operator <=(Energy a, Energy b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Energy b) => throw new NotImplementedException();
    public static Boolean operator >(Energy a, Energy b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Energy b) => throw new NotImplementedException();
    public static Boolean operator >=(Energy a, Energy b) => a.GreaterThanOrEquals(b);
    public Energy Lesser(Energy b) => throw new NotImplementedException();
    public Energy Greater(Energy b) => throw new NotImplementedException();
}
public partial class Memory
{
    public Boolean Between(Memory min, Memory max) => throw new NotImplementedException();
    public Boolean Equals(Memory b) => throw new NotImplementedException();
    public static Boolean operator ==(Memory a, Memory b) => a.Equals(b);
    public Boolean NotEquals(Memory b) => throw new NotImplementedException();
    public static Boolean operator !=(Memory a, Memory b) => a.NotEquals(b);
    public Boolean LessThan(Memory b) => throw new NotImplementedException();
    public static Boolean operator <(Memory a, Memory b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Memory b) => throw new NotImplementedException();
    public static Boolean operator <=(Memory a, Memory b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Memory b) => throw new NotImplementedException();
    public static Boolean operator >(Memory a, Memory b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Memory b) => throw new NotImplementedException();
    public static Boolean operator >=(Memory a, Memory b) => a.GreaterThanOrEquals(b);
    public Memory Lesser(Memory b) => throw new NotImplementedException();
    public Memory Greater(Memory b) => throw new NotImplementedException();
}
public partial class Frequency
{
    public Boolean Between(Frequency min, Frequency max) => throw new NotImplementedException();
    public Boolean Equals(Frequency b) => throw new NotImplementedException();
    public static Boolean operator ==(Frequency a, Frequency b) => a.Equals(b);
    public Boolean NotEquals(Frequency b) => throw new NotImplementedException();
    public static Boolean operator !=(Frequency a, Frequency b) => a.NotEquals(b);
    public Boolean LessThan(Frequency b) => throw new NotImplementedException();
    public static Boolean operator <(Frequency a, Frequency b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Frequency b) => throw new NotImplementedException();
    public static Boolean operator <=(Frequency a, Frequency b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Frequency b) => throw new NotImplementedException();
    public static Boolean operator >(Frequency a, Frequency b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Frequency b) => throw new NotImplementedException();
    public static Boolean operator >=(Frequency a, Frequency b) => a.GreaterThanOrEquals(b);
    public Frequency Lesser(Frequency b) => throw new NotImplementedException();
    public Frequency Greater(Frequency b) => throw new NotImplementedException();
}
public partial class Loudness
{
    public Boolean Between(Loudness min, Loudness max) => throw new NotImplementedException();
    public Boolean Equals(Loudness b) => throw new NotImplementedException();
    public static Boolean operator ==(Loudness a, Loudness b) => a.Equals(b);
    public Boolean NotEquals(Loudness b) => throw new NotImplementedException();
    public static Boolean operator !=(Loudness a, Loudness b) => a.NotEquals(b);
    public Boolean LessThan(Loudness b) => throw new NotImplementedException();
    public static Boolean operator <(Loudness a, Loudness b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Loudness b) => throw new NotImplementedException();
    public static Boolean operator <=(Loudness a, Loudness b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Loudness b) => throw new NotImplementedException();
    public static Boolean operator >(Loudness a, Loudness b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Loudness b) => throw new NotImplementedException();
    public static Boolean operator >=(Loudness a, Loudness b) => a.GreaterThanOrEquals(b);
    public Loudness Lesser(Loudness b) => throw new NotImplementedException();
    public Loudness Greater(Loudness b) => throw new NotImplementedException();
}
public partial class LuminousIntensity
{
    public Boolean Between(LuminousIntensity min, LuminousIntensity max) => throw new NotImplementedException();
    public Boolean Equals(LuminousIntensity b) => throw new NotImplementedException();
    public static Boolean operator ==(LuminousIntensity a, LuminousIntensity b) => a.Equals(b);
    public Boolean NotEquals(LuminousIntensity b) => throw new NotImplementedException();
    public static Boolean operator !=(LuminousIntensity a, LuminousIntensity b) => a.NotEquals(b);
    public Boolean LessThan(LuminousIntensity b) => throw new NotImplementedException();
    public static Boolean operator <(LuminousIntensity a, LuminousIntensity b) => a.LessThan(b);
    public Boolean LessThanOrEquals(LuminousIntensity b) => throw new NotImplementedException();
    public static Boolean operator <=(LuminousIntensity a, LuminousIntensity b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(LuminousIntensity b) => throw new NotImplementedException();
    public static Boolean operator >(LuminousIntensity a, LuminousIntensity b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(LuminousIntensity b) => throw new NotImplementedException();
    public static Boolean operator >=(LuminousIntensity a, LuminousIntensity b) => a.GreaterThanOrEquals(b);
    public LuminousIntensity Lesser(LuminousIntensity b) => throw new NotImplementedException();
    public LuminousIntensity Greater(LuminousIntensity b) => throw new NotImplementedException();
}
public partial class ElectricPotential
{
    public Boolean Between(ElectricPotential min, ElectricPotential max) => throw new NotImplementedException();
    public Boolean Equals(ElectricPotential b) => throw new NotImplementedException();
    public static Boolean operator ==(ElectricPotential a, ElectricPotential b) => a.Equals(b);
    public Boolean NotEquals(ElectricPotential b) => throw new NotImplementedException();
    public static Boolean operator !=(ElectricPotential a, ElectricPotential b) => a.NotEquals(b);
    public Boolean LessThan(ElectricPotential b) => throw new NotImplementedException();
    public static Boolean operator <(ElectricPotential a, ElectricPotential b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricPotential b) => throw new NotImplementedException();
    public static Boolean operator <=(ElectricPotential a, ElectricPotential b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricPotential b) => throw new NotImplementedException();
    public static Boolean operator >(ElectricPotential a, ElectricPotential b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricPotential b) => throw new NotImplementedException();
    public static Boolean operator >=(ElectricPotential a, ElectricPotential b) => a.GreaterThanOrEquals(b);
    public ElectricPotential Lesser(ElectricPotential b) => throw new NotImplementedException();
    public ElectricPotential Greater(ElectricPotential b) => throw new NotImplementedException();
}
public partial class ElectricCharge
{
    public Boolean Between(ElectricCharge min, ElectricCharge max) => throw new NotImplementedException();
    public Boolean Equals(ElectricCharge b) => throw new NotImplementedException();
    public static Boolean operator ==(ElectricCharge a, ElectricCharge b) => a.Equals(b);
    public Boolean NotEquals(ElectricCharge b) => throw new NotImplementedException();
    public static Boolean operator !=(ElectricCharge a, ElectricCharge b) => a.NotEquals(b);
    public Boolean LessThan(ElectricCharge b) => throw new NotImplementedException();
    public static Boolean operator <(ElectricCharge a, ElectricCharge b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricCharge b) => throw new NotImplementedException();
    public static Boolean operator <=(ElectricCharge a, ElectricCharge b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricCharge b) => throw new NotImplementedException();
    public static Boolean operator >(ElectricCharge a, ElectricCharge b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricCharge b) => throw new NotImplementedException();
    public static Boolean operator >=(ElectricCharge a, ElectricCharge b) => a.GreaterThanOrEquals(b);
    public ElectricCharge Lesser(ElectricCharge b) => throw new NotImplementedException();
    public ElectricCharge Greater(ElectricCharge b) => throw new NotImplementedException();
}
public partial class ElectricCurrent
{
    public Boolean Between(ElectricCurrent min, ElectricCurrent max) => throw new NotImplementedException();
    public Boolean Equals(ElectricCurrent b) => throw new NotImplementedException();
    public static Boolean operator ==(ElectricCurrent a, ElectricCurrent b) => a.Equals(b);
    public Boolean NotEquals(ElectricCurrent b) => throw new NotImplementedException();
    public static Boolean operator !=(ElectricCurrent a, ElectricCurrent b) => a.NotEquals(b);
    public Boolean LessThan(ElectricCurrent b) => throw new NotImplementedException();
    public static Boolean operator <(ElectricCurrent a, ElectricCurrent b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricCurrent b) => throw new NotImplementedException();
    public static Boolean operator <=(ElectricCurrent a, ElectricCurrent b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricCurrent b) => throw new NotImplementedException();
    public static Boolean operator >(ElectricCurrent a, ElectricCurrent b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricCurrent b) => throw new NotImplementedException();
    public static Boolean operator >=(ElectricCurrent a, ElectricCurrent b) => a.GreaterThanOrEquals(b);
    public ElectricCurrent Lesser(ElectricCurrent b) => throw new NotImplementedException();
    public ElectricCurrent Greater(ElectricCurrent b) => throw new NotImplementedException();
}
public partial class ElectricResistance
{
    public Boolean Between(ElectricResistance min, ElectricResistance max) => throw new NotImplementedException();
    public Boolean Equals(ElectricResistance b) => throw new NotImplementedException();
    public static Boolean operator ==(ElectricResistance a, ElectricResistance b) => a.Equals(b);
    public Boolean NotEquals(ElectricResistance b) => throw new NotImplementedException();
    public static Boolean operator !=(ElectricResistance a, ElectricResistance b) => a.NotEquals(b);
    public Boolean LessThan(ElectricResistance b) => throw new NotImplementedException();
    public static Boolean operator <(ElectricResistance a, ElectricResistance b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricResistance b) => throw new NotImplementedException();
    public static Boolean operator <=(ElectricResistance a, ElectricResistance b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricResistance b) => throw new NotImplementedException();
    public static Boolean operator >(ElectricResistance a, ElectricResistance b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricResistance b) => throw new NotImplementedException();
    public static Boolean operator >=(ElectricResistance a, ElectricResistance b) => a.GreaterThanOrEquals(b);
    public ElectricResistance Lesser(ElectricResistance b) => throw new NotImplementedException();
    public ElectricResistance Greater(ElectricResistance b) => throw new NotImplementedException();
}
public partial class Power
{
    public Boolean Between(Power min, Power max) => throw new NotImplementedException();
    public Boolean Equals(Power b) => throw new NotImplementedException();
    public static Boolean operator ==(Power a, Power b) => a.Equals(b);
    public Boolean NotEquals(Power b) => throw new NotImplementedException();
    public static Boolean operator !=(Power a, Power b) => a.NotEquals(b);
    public Boolean LessThan(Power b) => throw new NotImplementedException();
    public static Boolean operator <(Power a, Power b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Power b) => throw new NotImplementedException();
    public static Boolean operator <=(Power a, Power b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Power b) => throw new NotImplementedException();
    public static Boolean operator >(Power a, Power b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Power b) => throw new NotImplementedException();
    public static Boolean operator >=(Power a, Power b) => a.GreaterThanOrEquals(b);
    public Power Lesser(Power b) => throw new NotImplementedException();
    public Power Greater(Power b) => throw new NotImplementedException();
}
public partial class Density
{
    public Boolean Between(Density min, Density max) => throw new NotImplementedException();
    public Boolean Equals(Density b) => throw new NotImplementedException();
    public static Boolean operator ==(Density a, Density b) => a.Equals(b);
    public Boolean NotEquals(Density b) => throw new NotImplementedException();
    public static Boolean operator !=(Density a, Density b) => a.NotEquals(b);
    public Boolean LessThan(Density b) => throw new NotImplementedException();
    public static Boolean operator <(Density a, Density b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Density b) => throw new NotImplementedException();
    public static Boolean operator <=(Density a, Density b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Density b) => throw new NotImplementedException();
    public static Boolean operator >(Density a, Density b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Density b) => throw new NotImplementedException();
    public static Boolean operator >=(Density a, Density b) => a.GreaterThanOrEquals(b);
    public Density Lesser(Density b) => throw new NotImplementedException();
    public Density Greater(Density b) => throw new NotImplementedException();
}
public partial class NormalDistribution
{
}
public partial class PoissonDistribution
{
}
public partial class BernoulliDistribution
{
}
public partial class Probability
{
    public Probability Square => throw new NotImplementedException();
    public Probability PlusOne => throw new NotImplementedException();
    public Probability MinusOne => throw new NotImplementedException();
    public Probability FromOne => throw new NotImplementedException();
    public Boolean IsPositive => throw new NotImplementedException();
    public Boolean GtZ => throw new NotImplementedException();
    public Boolean LtZ => throw new NotImplementedException();
    public Boolean GtEqZ => throw new NotImplementedException();
    public Boolean LtEqZ => throw new NotImplementedException();
    public Boolean IsNegative => throw new NotImplementedException();
    public Probability Sign => throw new NotImplementedException();
    public Probability Abs => throw new NotImplementedException();
    public Probability Half => throw new NotImplementedException();
    public Probability Quarter => throw new NotImplementedException();
    public Probability Eighth => throw new NotImplementedException();
    public Probability Tenth => throw new NotImplementedException();
    public Probability Twice => throw new NotImplementedException();
    public Probability SmoothStep => throw new NotImplementedException();
    public Probability Pow2 => throw new NotImplementedException();
    public Probability Lerp(Probability b, Number t) => throw new NotImplementedException();
    public Boolean Between(Probability min, Probability max) => throw new NotImplementedException();
    public Boolean Equals(Probability b) => throw new NotImplementedException();
    public static Boolean operator ==(Probability a, Probability b) => a.Equals(b);
    public Boolean NotEquals(Probability b) => throw new NotImplementedException();
    public static Boolean operator !=(Probability a, Probability b) => a.NotEquals(b);
    public Boolean LessThan(Probability b) => throw new NotImplementedException();
    public static Boolean operator <(Probability a, Probability b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Probability b) => throw new NotImplementedException();
    public static Boolean operator <=(Probability a, Probability b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Probability b) => throw new NotImplementedException();
    public static Boolean operator >(Probability a, Probability b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Probability b) => throw new NotImplementedException();
    public static Boolean operator >=(Probability a, Probability b) => a.GreaterThanOrEquals(b);
    public Probability Lesser(Probability b) => throw new NotImplementedException();
    public Probability Greater(Probability b) => throw new NotImplementedException();
}
public partial class BinomialDistribution
{
}
public partial class Array1<T>
{
}
public partial class Tuple2<T0, T1>
{
}
public partial class Tuple3<T0, T1, T2>
{
}
public partial class Function0<TR>
{
}
public partial class Function1<T0, TR>
{
}
public partial class Function2<T0, T1, TR>
{
}
public partial class Function3<T0, T1, T2, TR>
{
}
