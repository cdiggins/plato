using System;
public static class Constants
{
    public static Number TwoPi => Constants.Pi.Twice;
    public static Number Pi => ((Number)3.1415926535897);
    public static Number Epsilon => ((Number)1E-15);
}
public readonly partial struct Number
{
    public Angle Acos => Intrinsics.Acos(this);
    public Angle Asin => Intrinsics.Asin(this);
    public Angle Atan => Intrinsics.Atan(this);
    public Number Pow(Number y) => Intrinsics.Pow(this, y);
    public Number Log(Number y) => Intrinsics.Log(this, y);
    public Number NaturalLog => Intrinsics.NaturalLog(this);
    public Number NaturalPower => Intrinsics.NaturalPower(this);
    public Number Add(Number y) => Intrinsics.Add(this, y);
    public static Number operator +(Number x, Number y) => x.Add(y);
    public Number Subtract(Number y) => Intrinsics.Subtract(this, y);
    public static Number operator -(Number x, Number y) => x.Subtract(y);
    public Number Divide(Number y) => Intrinsics.Divide(this, y);
    public static Number operator /(Number x, Number y) => x.Divide(y);
    public Number Multiply(Number y) => Intrinsics.Multiply(this, y);
    public static Number operator *(Number x, Number y) => x.Multiply(y);
    public Number Modulo(Number y) => Intrinsics.Modulo(this, y);
    public static Number operator %(Number x, Number y) => x.Modulo(y);
    public Number Negative => Intrinsics.Negative(this);
    public static Number operator -(Number x) => x.Negative;
    public Number SquareRoot => this.Pow(((Number)0.5));
    public Number SmoothStep => this.Square.Multiply(((Number)3).Subtract(this.Twice));
    public Number ClampOne => this.Clamp(((Number)0), ((Number)1));
    public Number Sign => this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Number Abs => this.LtZ ? this.Negative : this;
    public Number MultiplyEpsilon(Number y) => this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(Number y) => this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public Boolean AlmostZero => this.Abs.LessThan(Constants.Epsilon);
    public Number Nearest(Number b, Number t) => t.LessThanOrEquals(((Number)0.5)) ? this : b;
    public Unit Percent => this.Divide(((Number)100));
    public Number InverseLerp(Number b, Number v) => v.Subtract(this).Divide(b.Subtract(this));
    public Number Remap(Number bIn, Number aOut, Number bOut, Number v) => aOut.Lerp(bOut, this.InverseLerp(aOut, v));
    public Angle Radians => this;
    public Angle Degrees => this.Divide(((Integer)360)).Turns;
    public Angle Turns => this.Multiply(Constants.TwoPi);
    public Temperature Fahrenheit => this.Subtract(((Number)32)).Multiply(((Number)5).Divide(((Number)9)));
    public Temperature Kelvin => this.Subtract(((Number)273.15));
    public Time Days => this.Multiply(((Number)86400));
    public Time Milliseconds => this.Divide(((Number)1000)).Seconds;
    public Time Seconds => this;
    public Time Minutes => this.Multiply(((Number)60)).Seconds;
    public Time Hours => this.Multiply(((Integer)60)).Minutes;
    public Number HeavisideStep => this.LtZ ? ((Number)0) : ((Number)1);
    public Number Rectangular(Number width) => this.Abs.GreaterThanOrEquals(width.Half) ? ((Number)0) : ((Number)1);
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Number min, Number max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Number Clamp(Number a, Number b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Number b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Number a, Number b) => a.Equals(b);
    public Boolean NotEquals(Number b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Number a, Number b) => a.NotEquals(b);
    public Boolean LessThan(Number b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Number a, Number b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Number b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Number a, Number b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Number b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Number a, Number b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Number b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Number a, Number b) => a.GreaterThanOrEquals(b);
    public Number Lesser(Number b) => this.LessThanOrEquals(b) ? this : b;
    public Number Greater(Number b) => this.GreaterThanOrEquals(b) ? this : b;
    public Number Half => this.Divide(((Number)2));
    public Number Quarter => this.Divide(((Number)4));
    public Number Eighth => this.Divide(((Number)8));
    public Number Tenth => this.Divide(((Number)10));
    public Number Twice => this.Multiply(((Number)2));
    public Number Lerp(Number b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    public Number PlusOne => this.Add(this.One);
    public Number MinusOne => this.Subtract(this.One);
    public Number FromOne => this.One.Subtract(this);
    public Number Square => this.Multiply(this);
}
public readonly partial struct Integer
{
    public Integer Add(Integer y) => Intrinsics.Add(this, y);
    public static Integer operator +(Integer x, Integer y) => x.Add(y);
    public Integer Subtract(Integer y) => Intrinsics.Subtract(this, y);
    public static Integer operator -(Integer x, Integer y) => x.Subtract(y);
    public Integer Divide(Integer y) => Intrinsics.Divide(this, y);
    public static Integer operator /(Integer x, Integer y) => x.Divide(y);
    public Integer Multiply(Integer y) => Intrinsics.Multiply(this, y);
    public static Integer operator *(Integer x, Integer y) => x.Multiply(y);
    public Integer Modulo(Integer y) => Intrinsics.Modulo(this, y);
    public static Integer operator %(Integer x, Integer y) => x.Modulo(y);
    public Integer Negative => Intrinsics.Negative(this);
    public static Integer operator -(Integer x) => x.Negative;
    public Number ToNumber => Intrinsics.ToNumber(this);
    public Integer PlusOne => this.Add(this.One);
    public Integer MinusOne => this.Subtract(this.One);
    public Integer FromOne => this.One.Subtract(this);
    public Integer Square => this.Multiply(this);
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Integer min, Integer max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Integer Clamp(Integer a, Integer b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Integer b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Integer a, Integer b) => a.Equals(b);
    public Boolean NotEquals(Integer b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Integer a, Integer b) => a.NotEquals(b);
    public Boolean LessThan(Integer b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Integer a, Integer b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Integer b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Integer a, Integer b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Integer b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Integer a, Integer b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Integer b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Integer a, Integer b) => a.GreaterThanOrEquals(b);
    public Integer Lesser(Integer b) => this.LessThanOrEquals(b) ? this : b;
    public Integer Greater(Integer b) => this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct String
{
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(String min, String max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public String Clamp(String a, String b) => this.Greater(a).Lesser(b);
    public Boolean Equals(String b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(String a, String b) => a.Equals(b);
    public Boolean NotEquals(String b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(String a, String b) => a.NotEquals(b);
    public Boolean LessThan(String b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(String a, String b) => a.LessThan(b);
    public Boolean LessThanOrEquals(String b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(String a, String b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(String b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(String a, String b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(String b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(String a, String b) => a.GreaterThanOrEquals(b);
    public String Lesser(String b) => this.LessThanOrEquals(b) ? this : b;
    public String Greater(String b) => this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Boolean
{
    public Boolean And(Boolean y) => Intrinsics.And(this, y);
    public static Boolean operator &(Boolean x, Boolean y) => x.And(y);
    public Boolean Or(Boolean y) => Intrinsics.Or(this, y);
    public static Boolean operator |(Boolean x, Boolean y) => x.Or(y);
    public Boolean Not => Intrinsics.Not(this);
    public static Boolean operator !(Boolean x) => x.Not;
}
public readonly partial struct Character
{
}
public readonly partial struct Dynamic
{
}
public readonly partial struct Cardinal
{
    public Cardinal PlusOne => this.Add(this.One);
    public Cardinal MinusOne => this.Subtract(this.One);
    public Cardinal FromOne => this.One.Subtract(this);
    public Cardinal Square => this.Multiply(this);
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Cardinal min, Cardinal max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Cardinal Clamp(Cardinal a, Cardinal b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Cardinal b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Cardinal a, Cardinal b) => a.Equals(b);
    public Boolean NotEquals(Cardinal b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Cardinal a, Cardinal b) => a.NotEquals(b);
    public Boolean LessThan(Cardinal b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Cardinal a, Cardinal b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Cardinal b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Cardinal a, Cardinal b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Cardinal b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Cardinal a, Cardinal b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Cardinal b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Cardinal a, Cardinal b) => a.GreaterThanOrEquals(b);
    public Cardinal Lesser(Cardinal b) => this.LessThanOrEquals(b) ? this : b;
    public Cardinal Greater(Cardinal b) => this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Index
{
    public Index PlusOne => this.Add(this.One);
    public Index MinusOne => this.Subtract(this.One);
    public Index FromOne => this.One.Subtract(this);
    public Index Square => this.Multiply(this);
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Index min, Index max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Index Clamp(Index a, Index b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Index b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Index a, Index b) => a.Equals(b);
    public Boolean NotEquals(Index b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Index a, Index b) => a.NotEquals(b);
    public Boolean LessThan(Index b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Index a, Index b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Index b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Index a, Index b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Index b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Index a, Index b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Index b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Index a, Index b) => a.GreaterThanOrEquals(b);
    public Index Lesser(Index b) => this.LessThanOrEquals(b) ? this : b;
    public Index Greater(Index b) => this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Unit
{
    public Number Percent => this.Value.Multiply(((Number)100));
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Unit min, Unit max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Unit Clamp(Unit a, Unit b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Unit b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Unit a, Unit b) => a.Equals(b);
    public Boolean NotEquals(Unit b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Unit a, Unit b) => a.NotEquals(b);
    public Boolean LessThan(Unit b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Unit a, Unit b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Unit b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Unit a, Unit b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Unit b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Unit a, Unit b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Unit b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Unit a, Unit b) => a.GreaterThanOrEquals(b);
    public Unit Lesser(Unit b) => this.LessThanOrEquals(b) ? this : b;
    public Unit Greater(Unit b) => this.GreaterThanOrEquals(b) ? this : b;
    public Unit Half => this.Divide(((Number)2));
    public Unit Quarter => this.Divide(((Number)4));
    public Unit Eighth => this.Divide(((Number)8));
    public Unit Tenth => this.Divide(((Number)10));
    public Unit Twice => this.Multiply(((Number)2));
    public Unit Lerp(Unit b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    public Unit PlusOne => this.Add(this.One);
    public Unit MinusOne => this.Subtract(this.One);
    public Unit FromOne => this.One.Subtract(this);
    public Unit Square => this.Multiply(this);
}
public readonly partial struct Probability
{
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Probability min, Probability max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Probability Clamp(Probability a, Probability b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Probability b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Probability a, Probability b) => a.Equals(b);
    public Boolean NotEquals(Probability b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Probability a, Probability b) => a.NotEquals(b);
    public Boolean LessThan(Probability b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Probability a, Probability b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Probability b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Probability a, Probability b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Probability b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Probability a, Probability b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Probability b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Probability a, Probability b) => a.GreaterThanOrEquals(b);
    public Probability Lesser(Probability b) => this.LessThanOrEquals(b) ? this : b;
    public Probability Greater(Probability b) => this.GreaterThanOrEquals(b) ? this : b;
    public Probability Half => this.Divide(((Number)2));
    public Probability Quarter => this.Divide(((Number)4));
    public Probability Eighth => this.Divide(((Number)8));
    public Probability Tenth => this.Divide(((Number)10));
    public Probability Twice => this.Multiply(((Number)2));
    public Probability Lerp(Probability b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    public Probability PlusOne => this.Add(this.One);
    public Probability MinusOne => this.Subtract(this.One);
    public Probability FromOne => this.One.Subtract(this);
    public Probability Square => this.Multiply(this);
}
public readonly partial struct Quaternion
{
}
public readonly partial struct Unit2D
{
}
public readonly partial struct Unit3D
{
}
public readonly partial struct Direction3D
{
}
public readonly partial struct AxisAngle
{
}
public readonly partial struct EulerAngles
{
}
public readonly partial struct Rotation3D
{
}
public readonly partial struct Vector2D
{
    public Integer Count => ((Integer)2);
    public Number At(Integer n) => n.Equals(((Integer)0)) ? this.X : this.Y;
    public Number this[Integer n] => At(n);
    public Number Sum
    {
        get
        {
            var r = ((Number)0);
            {
                var i = ((Integer)0);
                while (i.LessThan(this.Count))
                {
                    r = r.Add(this.At(i));
                    i = i.Add(((Integer)1));
                }

            }
            return r;
        }
    }
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(Vector2D v2) => this.Multiply(v2).Sum;
    public Vector2D Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public Vector2D PlusOne => this.Add(this.One);
    public Vector2D MinusOne => this.Subtract(this.One);
    public Vector2D FromOne => this.One.Subtract(this);
    public Vector2D Square => this.Multiply(this);
    public Vector2D Half => this.Divide(((Number)2));
    public Vector2D Quarter => this.Divide(((Number)4));
    public Vector2D Eighth => this.Divide(((Number)8));
    public Vector2D Tenth => this.Divide(((Number)10));
    public Vector2D Twice => this.Multiply(((Number)2));
    public Vector2D Lerp(Vector2D b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Vector3D
{
    public Integer Count => ((Integer)3);
    public Number At(Integer n) => n.Equals(((Integer)0)) ? this.X : n.Equals(((Integer)1)) ? this.Y : this.Z;
    public Number this[Integer n] => At(n);
    public Number Sum
    {
        get
        {
            var r = ((Number)0);
            {
                var i = ((Integer)0);
                while (i.LessThan(this.Count))
                {
                    r = r.Add(this.At(i));
                    i = i.Add(((Integer)1));
                }

            }
            return r;
        }
    }
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(Vector3D v2) => this.Multiply(v2).Sum;
    public Vector3D Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public Vector3D PlusOne => this.Add(this.One);
    public Vector3D MinusOne => this.Subtract(this.One);
    public Vector3D FromOne => this.One.Subtract(this);
    public Vector3D Square => this.Multiply(this);
    public Vector3D Half => this.Divide(((Number)2));
    public Vector3D Quarter => this.Divide(((Number)4));
    public Vector3D Eighth => this.Divide(((Number)8));
    public Vector3D Tenth => this.Divide(((Number)10));
    public Vector3D Twice => this.Multiply(((Number)2));
    public Vector3D Lerp(Vector3D b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Vector4D
{
    public Integer Count => ((Integer)4);
    public Number At(Integer n) => n.Equals(((Integer)0)) ? this.X : n.Equals(((Integer)1)) ? this.Y : n.Equals(((Integer)2)) ? this.Z : this.W;
    public Number this[Integer n] => At(n);
    public Number Sum
    {
        get
        {
            var r = ((Number)0);
            {
                var i = ((Integer)0);
                while (i.LessThan(this.Count))
                {
                    r = r.Add(this.At(i));
                    i = i.Add(((Integer)1));
                }

            }
            return r;
        }
    }
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(Vector4D v2) => this.Multiply(v2).Sum;
    public Vector4D Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public Vector4D PlusOne => this.Add(this.One);
    public Vector4D MinusOne => this.Subtract(this.One);
    public Vector4D FromOne => this.One.Subtract(this);
    public Vector4D Square => this.Multiply(this);
    public Vector4D Half => this.Divide(((Number)2));
    public Vector4D Quarter => this.Divide(((Number)4));
    public Vector4D Eighth => this.Divide(((Number)8));
    public Vector4D Tenth => this.Divide(((Number)10));
    public Vector4D Twice => this.Multiply(((Number)2));
    public Vector4D Lerp(Vector4D b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Orientation3D
{
}
public readonly partial struct Pose2D
{
}
public readonly partial struct Pose3D
{
}
public readonly partial struct Transform3D
{
}
public readonly partial struct Transform2D
{
}
public readonly partial struct AlignedBox2D
{
    public Point2D Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public AlignedBox2D Reverse => this.Max.Tuple(this.Min);
    public Point2D Center => this.Lerp(((Number)0.5));
    public Boolean Contains(Point2D value) => value.Between(this.Min, this.Max);
    public Boolean Contains(AlignedBox2D y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(AlignedBox2D y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<AlignedBox2D, AlignedBox2D> SplitAt(Number t) => this.Left(t).Tuple(this.Right(t));
    public Tuple2<AlignedBox2D, AlignedBox2D> Split => this.SplitAt(((Number)0.5));
    public AlignedBox2D Left(Number t) => this.Min.Tuple(this.Lerp(t));
    public AlignedBox2D Right(Number t) => this.Lerp(t).Tuple(this.Max);
    public AlignedBox2D MoveTo(Point2D v) => v.Tuple(v.Add(this.Size));
    public AlignedBox2D LeftHalf => this.Left(((Number)0.5));
    public AlignedBox2D RightHalf => this.Right(((Number)0.5));
    public AlignedBox2D Recenter(Point2D c) => c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public AlignedBox2D Clamp(AlignedBox2D y) => this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Point2D Clamp(Point2D value) => value.Clamp(this.Min, this.Max);
}
public readonly partial struct AlignedBox3D
{
    public Point3D Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public AlignedBox3D Reverse => this.Max.Tuple(this.Min);
    public Point3D Center => this.Lerp(((Number)0.5));
    public Boolean Contains(Point3D value) => value.Between(this.Min, this.Max);
    public Boolean Contains(AlignedBox3D y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(AlignedBox3D y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<AlignedBox3D, AlignedBox3D> SplitAt(Number t) => this.Left(t).Tuple(this.Right(t));
    public Tuple2<AlignedBox3D, AlignedBox3D> Split => this.SplitAt(((Number)0.5));
    public AlignedBox3D Left(Number t) => this.Min.Tuple(this.Lerp(t));
    public AlignedBox3D Right(Number t) => this.Lerp(t).Tuple(this.Max);
    public AlignedBox3D MoveTo(Point3D v) => v.Tuple(v.Add(this.Size));
    public AlignedBox3D LeftHalf => this.Left(((Number)0.5));
    public AlignedBox3D RightHalf => this.Right(((Number)0.5));
    public AlignedBox3D Recenter(Point3D c) => c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public AlignedBox3D Clamp(AlignedBox3D y) => this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Point3D Clamp(Point3D value) => value.Clamp(this.Min, this.Max);
}
public readonly partial struct Complex
{
    public Number Sum
    {
        get
        {
            var r = ((Number)0);
            {
                var i = ((Integer)0);
                while (i.LessThan(this.Count))
                {
                    r = r.Add(this.At(i));
                    i = i.Add(((Integer)1));
                }

            }
            return r;
        }
    }
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(Complex v2) => this.Multiply(v2).Sum;
    public Complex Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public Complex PlusOne => this.Add(this.One);
    public Complex MinusOne => this.Subtract(this.One);
    public Complex FromOne => this.One.Subtract(this);
    public Complex Square => this.Multiply(this);
    public Complex Half => this.Divide(((Number)2));
    public Complex Quarter => this.Divide(((Number)4));
    public Complex Eighth => this.Divide(((Number)8));
    public Complex Tenth => this.Divide(((Number)10));
    public Complex Twice => this.Multiply(((Number)2));
    public Complex Lerp(Complex b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Ray3D
{
}
public readonly partial struct Ray2D
{
}
public readonly partial struct Sphere
{
}
public readonly partial struct Plane
{
}
public readonly partial struct Triangle2D
{
}
public readonly partial struct Triangle3D
{
}
public readonly partial struct Quad2D
{
}
public readonly partial struct Quad3D
{
}
public readonly partial struct Point2D
{
}
public readonly partial struct Point3D
{
}
public readonly partial struct Point4D
{
}
public readonly partial struct Line2D
{
    public Point2D Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public Line2D Reverse => this.Max.Tuple(this.Min);
    public Point2D Center => this.Lerp(((Number)0.5));
    public Boolean Contains(Point2D value) => value.Between(this.Min, this.Max);
    public Boolean Contains(Line2D y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(Line2D y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<Line2D, Line2D> SplitAt(Number t) => this.Left(t).Tuple(this.Right(t));
    public Tuple2<Line2D, Line2D> Split => this.SplitAt(((Number)0.5));
    public Line2D Left(Number t) => this.Min.Tuple(this.Lerp(t));
    public Line2D Right(Number t) => this.Lerp(t).Tuple(this.Max);
    public Line2D MoveTo(Point2D v) => v.Tuple(v.Add(this.Size));
    public Line2D LeftHalf => this.Left(((Number)0.5));
    public Line2D RightHalf => this.Right(((Number)0.5));
    public Line2D Recenter(Point2D c) => c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public Line2D Clamp(Line2D y) => this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Point2D Clamp(Point2D value) => value.Clamp(this.Min, this.Max);
}
public readonly partial struct Line3D
{
    public Point3D Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public Line3D Reverse => this.Max.Tuple(this.Min);
    public Point3D Center => this.Lerp(((Number)0.5));
    public Boolean Contains(Point3D value) => value.Between(this.Min, this.Max);
    public Boolean Contains(Line3D y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(Line3D y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<Line3D, Line3D> SplitAt(Number t) => this.Left(t).Tuple(this.Right(t));
    public Tuple2<Line3D, Line3D> Split => this.SplitAt(((Number)0.5));
    public Line3D Left(Number t) => this.Min.Tuple(this.Lerp(t));
    public Line3D Right(Number t) => this.Lerp(t).Tuple(this.Max);
    public Line3D MoveTo(Point3D v) => v.Tuple(v.Add(this.Size));
    public Line3D LeftHalf => this.Left(((Number)0.5));
    public Line3D RightHalf => this.Right(((Number)0.5));
    public Line3D Recenter(Point3D c) => c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public Line3D Clamp(Line3D y) => this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Point3D Clamp(Point3D value) => value.Clamp(this.Min, this.Max);
}
public readonly partial struct Color
{
}
public readonly partial struct ColorLUV
{
}
public readonly partial struct ColorLAB
{
}
public readonly partial struct ColorLCh
{
}
public readonly partial struct ColorHSV
{
}
public readonly partial struct ColorHSL
{
}
public readonly partial struct ColorYCbCr
{
}
public readonly partial struct SphericalCoordinate
{
}
public readonly partial struct PolarCoordinate
{
}
public readonly partial struct LogPolarCoordinate
{
}
public readonly partial struct CylindricalCoordinate
{
}
public readonly partial struct HorizontalCoordinate
{
}
public readonly partial struct GeoCoordinate
{
}
public readonly partial struct GeoCoordinateWithAltitude
{
}
public readonly partial struct Circle
{
}
public readonly partial struct Chord
{
}
public readonly partial struct Size2D
{
}
public readonly partial struct Size3D
{
}
public readonly partial struct Rectangle2D
{
}
public readonly partial struct Fraction
{
}
public readonly partial struct Angle
{
    public Number Cos => Intrinsics.Cos(this);
    public Number Sin => Intrinsics.Sin(this);
    public Number Tan => Intrinsics.Tan(this);
    public Number Degrees => this.Turns.Multiply(((Integer)360));
    public Number Turns => this.Divide(Constants.TwoPi);
    public Number Sinc => this.Radians.AlmostZero ? ((Number)1) : this.Sin.Divide(this);
    public Number Value => this.FieldValues.At(((Integer)0));
    public Angle Add(Number y) => this.Value.Add(y);
    public static Angle operator +(Angle x, Number y) => x.Add(y);
    public Angle Subtract(Number y) => this.Value.Subtract(y);
    public static Angle operator -(Angle x, Number y) => x.Subtract(y);
    public Angle Multiply(Number y) => this.Value.Multiply(y);
    public static Angle operator *(Angle x, Number y) => x.Multiply(y);
    public Angle Divide(Number y) => this.Value.Divide(y);
    public static Angle operator /(Angle x, Number y) => x.Divide(y);
    public Angle Modulo(Number y) => this.Value.Modulo(y);
    public static Angle operator %(Angle x, Number y) => x.Modulo(y);
    public Angle Negative => this.Value.Negative;
    public static Angle operator -(Angle x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Angle min, Angle max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Angle Clamp(Angle a, Angle b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Angle b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Angle a, Angle b) => a.Equals(b);
    public Boolean NotEquals(Angle b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Angle a, Angle b) => a.NotEquals(b);
    public Boolean LessThan(Angle b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Angle a, Angle b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Angle b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Angle a, Angle b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Angle b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Angle a, Angle b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Angle b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Angle a, Angle b) => a.GreaterThanOrEquals(b);
    public Angle Lesser(Angle b) => this.LessThanOrEquals(b) ? this : b;
    public Angle Greater(Angle b) => this.GreaterThanOrEquals(b) ? this : b;
    public Angle Half => this.Divide(((Number)2));
    public Angle Quarter => this.Divide(((Number)4));
    public Angle Eighth => this.Divide(((Number)8));
    public Angle Tenth => this.Divide(((Number)10));
    public Angle Twice => this.Multiply(((Number)2));
    public Angle Lerp(Angle b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Length
{
    public Area Multiply(Length y) => this.Value.Multiply(y.Value);
    public static Area operator *(Length x, Length y) => x.Multiply(y);
    public Volume Multiply(Area y) => this.Value.Multiply(y.Value);
    public static Volume operator *(Length x, Area y) => x.Multiply(y);
    public Velocity Multiply(Time y) => this.Value.Multiply(y.Value);
    public static Velocity operator *(Length x, Time y) => x.Multiply(y);
    public Number Value => this.FieldValues.At(((Integer)0));
    public Length Add(Number y) => this.Value.Add(y);
    public static Length operator +(Length x, Number y) => x.Add(y);
    public Length Subtract(Number y) => this.Value.Subtract(y);
    public static Length operator -(Length x, Number y) => x.Subtract(y);
    public Length Multiply(Number y) => this.Value.Multiply(y);
    public static Length operator *(Length x, Number y) => x.Multiply(y);
    public Length Divide(Number y) => this.Value.Divide(y);
    public static Length operator /(Length x, Number y) => x.Divide(y);
    public Length Modulo(Number y) => this.Value.Modulo(y);
    public static Length operator %(Length x, Number y) => x.Modulo(y);
    public Length Negative => this.Value.Negative;
    public static Length operator -(Length x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Length min, Length max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Length Clamp(Length a, Length b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Length b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Length a, Length b) => a.Equals(b);
    public Boolean NotEquals(Length b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Length a, Length b) => a.NotEquals(b);
    public Boolean LessThan(Length b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Length a, Length b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Length b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Length a, Length b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Length b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Length a, Length b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Length b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Length a, Length b) => a.GreaterThanOrEquals(b);
    public Length Lesser(Length b) => this.LessThanOrEquals(b) ? this : b;
    public Length Greater(Length b) => this.GreaterThanOrEquals(b) ? this : b;
    public Length Half => this.Divide(((Number)2));
    public Length Quarter => this.Divide(((Number)4));
    public Length Eighth => this.Divide(((Number)8));
    public Length Tenth => this.Divide(((Number)10));
    public Length Twice => this.Multiply(((Number)2));
    public Length Lerp(Length b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Mass
{
    public Force Multiply(Acceleration y) => this.Value.Multiply(y.Value);
    public static Force operator *(Mass x, Acceleration y) => x.Multiply(y);
    public Number Value => this.FieldValues.At(((Integer)0));
    public Mass Add(Number y) => this.Value.Add(y);
    public static Mass operator +(Mass x, Number y) => x.Add(y);
    public Mass Subtract(Number y) => this.Value.Subtract(y);
    public static Mass operator -(Mass x, Number y) => x.Subtract(y);
    public Mass Multiply(Number y) => this.Value.Multiply(y);
    public static Mass operator *(Mass x, Number y) => x.Multiply(y);
    public Mass Divide(Number y) => this.Value.Divide(y);
    public static Mass operator /(Mass x, Number y) => x.Divide(y);
    public Mass Modulo(Number y) => this.Value.Modulo(y);
    public static Mass operator %(Mass x, Number y) => x.Modulo(y);
    public Mass Negative => this.Value.Negative;
    public static Mass operator -(Mass x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Mass min, Mass max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Mass Clamp(Mass a, Mass b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Mass b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Mass a, Mass b) => a.Equals(b);
    public Boolean NotEquals(Mass b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Mass a, Mass b) => a.NotEquals(b);
    public Boolean LessThan(Mass b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Mass a, Mass b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Mass b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Mass a, Mass b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Mass b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Mass a, Mass b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Mass b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Mass a, Mass b) => a.GreaterThanOrEquals(b);
    public Mass Lesser(Mass b) => this.LessThanOrEquals(b) ? this : b;
    public Mass Greater(Mass b) => this.GreaterThanOrEquals(b) ? this : b;
    public Mass Half => this.Divide(((Number)2));
    public Mass Quarter => this.Divide(((Number)4));
    public Mass Eighth => this.Divide(((Number)8));
    public Mass Tenth => this.Divide(((Number)10));
    public Mass Twice => this.Multiply(((Number)2));
    public Mass Lerp(Mass b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Temperature
{
    public Number Fahrenheit => this.Celsius.Multiply(((Number)9).Divide(((Number)5))).Add(((Number)32));
    public Number Kelvin => this.Celsius.Add(((Number)273.15));
    public Number Value => this.FieldValues.At(((Integer)0));
    public Temperature Add(Number y) => this.Value.Add(y);
    public static Temperature operator +(Temperature x, Number y) => x.Add(y);
    public Temperature Subtract(Number y) => this.Value.Subtract(y);
    public static Temperature operator -(Temperature x, Number y) => x.Subtract(y);
    public Temperature Multiply(Number y) => this.Value.Multiply(y);
    public static Temperature operator *(Temperature x, Number y) => x.Multiply(y);
    public Temperature Divide(Number y) => this.Value.Divide(y);
    public static Temperature operator /(Temperature x, Number y) => x.Divide(y);
    public Temperature Modulo(Number y) => this.Value.Modulo(y);
    public static Temperature operator %(Temperature x, Number y) => x.Modulo(y);
    public Temperature Negative => this.Value.Negative;
    public static Temperature operator -(Temperature x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Temperature min, Temperature max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Temperature Clamp(Temperature a, Temperature b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Temperature b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Temperature a, Temperature b) => a.Equals(b);
    public Boolean NotEquals(Temperature b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Temperature a, Temperature b) => a.NotEquals(b);
    public Boolean LessThan(Temperature b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Temperature a, Temperature b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Temperature b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Temperature a, Temperature b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Temperature b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Temperature a, Temperature b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Temperature b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Temperature a, Temperature b) => a.GreaterThanOrEquals(b);
    public Temperature Lesser(Temperature b) => this.LessThanOrEquals(b) ? this : b;
    public Temperature Greater(Temperature b) => this.GreaterThanOrEquals(b) ? this : b;
    public Temperature Half => this.Divide(((Number)2));
    public Temperature Quarter => this.Divide(((Number)4));
    public Temperature Eighth => this.Divide(((Number)8));
    public Temperature Tenth => this.Divide(((Number)10));
    public Temperature Twice => this.Multiply(((Number)2));
    public Temperature Lerp(Temperature b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Time
{
    public Velocity Multiply(Length y) => this.Value.Multiply(y.Value);
    public static Velocity operator *(Time x, Length y) => x.Multiply(y);
    public Number Days => this.Multiply(((Number)24)).Hours;
    public Number Milliseconds => this.Seconds.Multiply(((Number)1000));
    public Number Minutes => this.Seconds.Divide(((Number)60));
    public Number Hours => this.Minutes.Divide(((Number)60));
    public Number Value => this.FieldValues.At(((Integer)0));
    public Time Add(Number y) => this.Value.Add(y);
    public static Time operator +(Time x, Number y) => x.Add(y);
    public Time Subtract(Number y) => this.Value.Subtract(y);
    public static Time operator -(Time x, Number y) => x.Subtract(y);
    public Time Multiply(Number y) => this.Value.Multiply(y);
    public static Time operator *(Time x, Number y) => x.Multiply(y);
    public Time Divide(Number y) => this.Value.Divide(y);
    public static Time operator /(Time x, Number y) => x.Divide(y);
    public Time Modulo(Number y) => this.Value.Modulo(y);
    public static Time operator %(Time x, Number y) => x.Modulo(y);
    public Time Negative => this.Value.Negative;
    public static Time operator -(Time x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Time min, Time max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Time Clamp(Time a, Time b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Time b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Time a, Time b) => a.Equals(b);
    public Boolean NotEquals(Time b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Time a, Time b) => a.NotEquals(b);
    public Boolean LessThan(Time b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Time a, Time b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Time b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Time a, Time b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Time b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Time a, Time b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Time b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Time a, Time b) => a.GreaterThanOrEquals(b);
    public Time Lesser(Time b) => this.LessThanOrEquals(b) ? this : b;
    public Time Greater(Time b) => this.GreaterThanOrEquals(b) ? this : b;
    public Time Half => this.Divide(((Number)2));
    public Time Quarter => this.Divide(((Number)4));
    public Time Eighth => this.Divide(((Number)8));
    public Time Tenth => this.Divide(((Number)10));
    public Time Twice => this.Multiply(((Number)2));
    public Time Lerp(Time b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct TimeRange
{
    public DateTime Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public TimeRange Reverse => this.Max.Tuple(this.Min);
    public DateTime Center => this.Lerp(((Number)0.5));
    public Boolean Contains(DateTime value) => value.Between(this.Min, this.Max);
    public Boolean Contains(TimeRange y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(TimeRange y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<TimeRange, TimeRange> SplitAt(Number t) => this.Left(t).Tuple(this.Right(t));
    public Tuple2<TimeRange, TimeRange> Split => this.SplitAt(((Number)0.5));
    public TimeRange Left(Number t) => this.Min.Tuple(this.Lerp(t));
    public TimeRange Right(Number t) => this.Lerp(t).Tuple(this.Max);
    public TimeRange MoveTo(DateTime v) => v.Tuple(v.Add(this.Size));
    public TimeRange LeftHalf => this.Left(((Number)0.5));
    public TimeRange RightHalf => this.Right(((Number)0.5));
    public TimeRange Recenter(DateTime c) => c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public TimeRange Clamp(TimeRange y) => this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public DateTime Clamp(DateTime value) => value.Clamp(this.Min, this.Max);
}
public readonly partial struct DateTime
{
}
public readonly partial struct AnglePair
{
    public Angle Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public AnglePair Reverse => this.Max.Tuple(this.Min);
    public Angle Center => this.Lerp(((Number)0.5));
    public Boolean Contains(Angle value) => value.Between(this.Min, this.Max);
    public Boolean Contains(AnglePair y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(AnglePair y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<AnglePair, AnglePair> SplitAt(Number t) => this.Left(t).Tuple(this.Right(t));
    public Tuple2<AnglePair, AnglePair> Split => this.SplitAt(((Number)0.5));
    public AnglePair Left(Number t) => this.Min.Tuple(this.Lerp(t));
    public AnglePair Right(Number t) => this.Lerp(t).Tuple(this.Max);
    public AnglePair MoveTo(Angle v) => v.Tuple(v.Add(this.Size));
    public AnglePair LeftHalf => this.Left(((Number)0.5));
    public AnglePair RightHalf => this.Right(((Number)0.5));
    public AnglePair Recenter(Angle c) => c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public AnglePair Clamp(AnglePair y) => this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Angle Clamp(Angle value) => value.Clamp(this.Min, this.Max);
}
public readonly partial struct Ring
{
}
public readonly partial struct Arc
{
}
public readonly partial struct NumberInterval
{
    public Number Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public NumberInterval Reverse => this.Max.Tuple(this.Min);
    public Number Center => this.Lerp(((Number)0.5));
    public Boolean Contains(Number value) => value.Between(this.Min, this.Max);
    public Boolean Contains(NumberInterval y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(NumberInterval y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<NumberInterval, NumberInterval> SplitAt(Number t) => this.Left(t).Tuple(this.Right(t));
    public Tuple2<NumberInterval, NumberInterval> Split => this.SplitAt(((Number)0.5));
    public NumberInterval Left(Number t) => this.Min.Tuple(this.Lerp(t));
    public NumberInterval Right(Number t) => this.Lerp(t).Tuple(this.Max);
    public NumberInterval MoveTo(Number v) => v.Tuple(v.Add(this.Size));
    public NumberInterval LeftHalf => this.Left(((Number)0.5));
    public NumberInterval RightHalf => this.Right(((Number)0.5));
    public NumberInterval Recenter(Number c) => c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public NumberInterval Clamp(NumberInterval y) => this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Number Clamp(Number value) => value.Clamp(this.Min, this.Max);
}
public readonly partial struct Capsule
{
}
public readonly partial struct Matrix3D
{
}
public readonly partial struct Cylinder
{
}
public readonly partial struct Cone
{
}
public readonly partial struct Tube
{
}
public readonly partial struct ConeSegment
{
}
public readonly partial struct Box2D
{
}
public readonly partial struct Box3D
{
}
public readonly partial struct UV
{
    public Number Sum
    {
        get
        {
            var r = ((Number)0);
            {
                var i = ((Integer)0);
                while (i.LessThan(this.Count))
                {
                    r = r.Add(this.At(i));
                    i = i.Add(((Integer)1));
                }

            }
            return r;
        }
    }
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(UV v2) => this.Multiply(v2).Sum;
    public UV Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public UV PlusOne => this.Add(this.One);
    public UV MinusOne => this.Subtract(this.One);
    public UV FromOne => this.One.Subtract(this);
    public UV Square => this.Multiply(this);
    public UV Half => this.Divide(((Number)2));
    public UV Quarter => this.Divide(((Number)4));
    public UV Eighth => this.Divide(((Number)8));
    public UV Tenth => this.Divide(((Number)10));
    public UV Twice => this.Multiply(((Number)2));
    public UV Lerp(UV b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct UVW
{
    public Number Sum
    {
        get
        {
            var r = ((Number)0);
            {
                var i = ((Integer)0);
                while (i.LessThan(this.Count))
                {
                    r = r.Add(this.At(i));
                    i = i.Add(((Integer)1));
                }

            }
            return r;
        }
    }
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(UVW v2) => this.Multiply(v2).Sum;
    public UVW Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public UVW PlusOne => this.Add(this.One);
    public UVW MinusOne => this.Subtract(this.One);
    public UVW FromOne => this.One.Subtract(this);
    public UVW Square => this.Multiply(this);
    public UVW Half => this.Divide(((Number)2));
    public UVW Quarter => this.Divide(((Number)4));
    public UVW Eighth => this.Divide(((Number)8));
    public UVW Tenth => this.Divide(((Number)10));
    public UVW Twice => this.Multiply(((Number)2));
    public UVW Lerp(UVW b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct CubicBezier2D
{
}
public readonly partial struct CubicBezier3D
{
}
public readonly partial struct QuadraticBezier2D
{
}
public readonly partial struct QuadraticBezier3D
{
}
public readonly partial struct Area
{
    public Length Divide(Length y) => this.Value.Divide(y.Value);
    public static Length operator /(Area x, Length y) => x.Divide(y);
    public Volume Multiply(Length y) => this.Value.Multiply(y.Value);
    public static Volume operator *(Area x, Length y) => x.Multiply(y);
    public Number Value => this.FieldValues.At(((Integer)0));
    public Area Add(Number y) => this.Value.Add(y);
    public static Area operator +(Area x, Number y) => x.Add(y);
    public Area Subtract(Number y) => this.Value.Subtract(y);
    public static Area operator -(Area x, Number y) => x.Subtract(y);
    public Area Multiply(Number y) => this.Value.Multiply(y);
    public static Area operator *(Area x, Number y) => x.Multiply(y);
    public Area Divide(Number y) => this.Value.Divide(y);
    public static Area operator /(Area x, Number y) => x.Divide(y);
    public Area Modulo(Number y) => this.Value.Modulo(y);
    public static Area operator %(Area x, Number y) => x.Modulo(y);
    public Area Negative => this.Value.Negative;
    public static Area operator -(Area x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Area min, Area max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Area Clamp(Area a, Area b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Area b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Area a, Area b) => a.Equals(b);
    public Boolean NotEquals(Area b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Area a, Area b) => a.NotEquals(b);
    public Boolean LessThan(Area b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Area a, Area b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Area b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Area a, Area b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Area b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Area a, Area b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Area b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Area a, Area b) => a.GreaterThanOrEquals(b);
    public Area Lesser(Area b) => this.LessThanOrEquals(b) ? this : b;
    public Area Greater(Area b) => this.GreaterThanOrEquals(b) ? this : b;
    public Area Half => this.Divide(((Number)2));
    public Area Quarter => this.Divide(((Number)4));
    public Area Eighth => this.Divide(((Number)8));
    public Area Tenth => this.Divide(((Number)10));
    public Area Twice => this.Multiply(((Number)2));
    public Area Lerp(Area b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Volume
{
    public Area Divide(Length y) => this.Value.Divide(y.Value);
    public static Area operator /(Volume x, Length y) => x.Divide(y);
    public Length Divide(Area y) => this.Value.Divide(y.Value);
    public static Length operator /(Volume x, Area y) => x.Divide(y);
    public Number Value => this.FieldValues.At(((Integer)0));
    public Volume Add(Number y) => this.Value.Add(y);
    public static Volume operator +(Volume x, Number y) => x.Add(y);
    public Volume Subtract(Number y) => this.Value.Subtract(y);
    public static Volume operator -(Volume x, Number y) => x.Subtract(y);
    public Volume Multiply(Number y) => this.Value.Multiply(y);
    public static Volume operator *(Volume x, Number y) => x.Multiply(y);
    public Volume Divide(Number y) => this.Value.Divide(y);
    public static Volume operator /(Volume x, Number y) => x.Divide(y);
    public Volume Modulo(Number y) => this.Value.Modulo(y);
    public static Volume operator %(Volume x, Number y) => x.Modulo(y);
    public Volume Negative => this.Value.Negative;
    public static Volume operator -(Volume x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Volume min, Volume max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Volume Clamp(Volume a, Volume b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Volume b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Volume a, Volume b) => a.Equals(b);
    public Boolean NotEquals(Volume b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Volume a, Volume b) => a.NotEquals(b);
    public Boolean LessThan(Volume b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Volume a, Volume b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Volume b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Volume a, Volume b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Volume b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Volume a, Volume b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Volume b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Volume a, Volume b) => a.GreaterThanOrEquals(b);
    public Volume Lesser(Volume b) => this.LessThanOrEquals(b) ? this : b;
    public Volume Greater(Volume b) => this.GreaterThanOrEquals(b) ? this : b;
    public Volume Half => this.Divide(((Number)2));
    public Volume Quarter => this.Divide(((Number)4));
    public Volume Eighth => this.Divide(((Number)8));
    public Volume Tenth => this.Divide(((Number)10));
    public Volume Twice => this.Multiply(((Number)2));
    public Volume Lerp(Volume b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Velocity
{
    public Length Divide(Time y) => this.Value.Divide(y.Value);
    public static Length operator /(Velocity x, Time y) => x.Divide(y);
    public Time Divide(Length y) => this.Value.Divide(y.Value);
    public static Time operator /(Velocity x, Length y) => x.Divide(y);
    public Acceleration Multiply(Time y) => this.Value.Multiply(y.Value);
    public static Acceleration operator *(Velocity x, Time y) => x.Multiply(y);
    public Number Value => this.FieldValues.At(((Integer)0));
    public Velocity Add(Number y) => this.Value.Add(y);
    public static Velocity operator +(Velocity x, Number y) => x.Add(y);
    public Velocity Subtract(Number y) => this.Value.Subtract(y);
    public static Velocity operator -(Velocity x, Number y) => x.Subtract(y);
    public Velocity Multiply(Number y) => this.Value.Multiply(y);
    public static Velocity operator *(Velocity x, Number y) => x.Multiply(y);
    public Velocity Divide(Number y) => this.Value.Divide(y);
    public static Velocity operator /(Velocity x, Number y) => x.Divide(y);
    public Velocity Modulo(Number y) => this.Value.Modulo(y);
    public static Velocity operator %(Velocity x, Number y) => x.Modulo(y);
    public Velocity Negative => this.Value.Negative;
    public static Velocity operator -(Velocity x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Velocity min, Velocity max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Velocity Clamp(Velocity a, Velocity b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Velocity b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Velocity a, Velocity b) => a.Equals(b);
    public Boolean NotEquals(Velocity b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Velocity a, Velocity b) => a.NotEquals(b);
    public Boolean LessThan(Velocity b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Velocity a, Velocity b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Velocity b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Velocity a, Velocity b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Velocity b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Velocity a, Velocity b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Velocity b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Velocity a, Velocity b) => a.GreaterThanOrEquals(b);
    public Velocity Lesser(Velocity b) => this.LessThanOrEquals(b) ? this : b;
    public Velocity Greater(Velocity b) => this.GreaterThanOrEquals(b) ? this : b;
    public Velocity Half => this.Divide(((Number)2));
    public Velocity Quarter => this.Divide(((Number)4));
    public Velocity Eighth => this.Divide(((Number)8));
    public Velocity Tenth => this.Divide(((Number)10));
    public Velocity Twice => this.Multiply(((Number)2));
    public Velocity Lerp(Velocity b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Acceleration
{
    public Velocity Divide(Time y) => this.Value.Divide(y.Value);
    public static Velocity operator /(Acceleration x, Time y) => x.Divide(y);
    public Time Divide(Velocity y) => this.Value.Divide(y.Value);
    public static Time operator /(Acceleration x, Velocity y) => x.Divide(y);
    public Number Value => this.FieldValues.At(((Integer)0));
    public Acceleration Add(Number y) => this.Value.Add(y);
    public static Acceleration operator +(Acceleration x, Number y) => x.Add(y);
    public Acceleration Subtract(Number y) => this.Value.Subtract(y);
    public static Acceleration operator -(Acceleration x, Number y) => x.Subtract(y);
    public Acceleration Multiply(Number y) => this.Value.Multiply(y);
    public static Acceleration operator *(Acceleration x, Number y) => x.Multiply(y);
    public Acceleration Divide(Number y) => this.Value.Divide(y);
    public static Acceleration operator /(Acceleration x, Number y) => x.Divide(y);
    public Acceleration Modulo(Number y) => this.Value.Modulo(y);
    public static Acceleration operator %(Acceleration x, Number y) => x.Modulo(y);
    public Acceleration Negative => this.Value.Negative;
    public static Acceleration operator -(Acceleration x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Acceleration min, Acceleration max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Acceleration Clamp(Acceleration a, Acceleration b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Acceleration b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Acceleration a, Acceleration b) => a.Equals(b);
    public Boolean NotEquals(Acceleration b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Acceleration a, Acceleration b) => a.NotEquals(b);
    public Boolean LessThan(Acceleration b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Acceleration a, Acceleration b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Acceleration b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Acceleration a, Acceleration b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Acceleration b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Acceleration a, Acceleration b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Acceleration b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Acceleration a, Acceleration b) => a.GreaterThanOrEquals(b);
    public Acceleration Lesser(Acceleration b) => this.LessThanOrEquals(b) ? this : b;
    public Acceleration Greater(Acceleration b) => this.GreaterThanOrEquals(b) ? this : b;
    public Acceleration Half => this.Divide(((Number)2));
    public Acceleration Quarter => this.Divide(((Number)4));
    public Acceleration Eighth => this.Divide(((Number)8));
    public Acceleration Tenth => this.Divide(((Number)10));
    public Acceleration Twice => this.Multiply(((Number)2));
    public Acceleration Lerp(Acceleration b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Force
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public Force Add(Number y) => this.Value.Add(y);
    public static Force operator +(Force x, Number y) => x.Add(y);
    public Force Subtract(Number y) => this.Value.Subtract(y);
    public static Force operator -(Force x, Number y) => x.Subtract(y);
    public Force Multiply(Number y) => this.Value.Multiply(y);
    public static Force operator *(Force x, Number y) => x.Multiply(y);
    public Force Divide(Number y) => this.Value.Divide(y);
    public static Force operator /(Force x, Number y) => x.Divide(y);
    public Force Modulo(Number y) => this.Value.Modulo(y);
    public static Force operator %(Force x, Number y) => x.Modulo(y);
    public Force Negative => this.Value.Negative;
    public static Force operator -(Force x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Force min, Force max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Force Clamp(Force a, Force b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Force b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Force a, Force b) => a.Equals(b);
    public Boolean NotEquals(Force b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Force a, Force b) => a.NotEquals(b);
    public Boolean LessThan(Force b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Force a, Force b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Force b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Force a, Force b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Force b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Force a, Force b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Force b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Force a, Force b) => a.GreaterThanOrEquals(b);
    public Force Lesser(Force b) => this.LessThanOrEquals(b) ? this : b;
    public Force Greater(Force b) => this.GreaterThanOrEquals(b) ? this : b;
    public Force Half => this.Divide(((Number)2));
    public Force Quarter => this.Divide(((Number)4));
    public Force Eighth => this.Divide(((Number)8));
    public Force Tenth => this.Divide(((Number)10));
    public Force Twice => this.Multiply(((Number)2));
    public Force Lerp(Force b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Pressure
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public Pressure Add(Number y) => this.Value.Add(y);
    public static Pressure operator +(Pressure x, Number y) => x.Add(y);
    public Pressure Subtract(Number y) => this.Value.Subtract(y);
    public static Pressure operator -(Pressure x, Number y) => x.Subtract(y);
    public Pressure Multiply(Number y) => this.Value.Multiply(y);
    public static Pressure operator *(Pressure x, Number y) => x.Multiply(y);
    public Pressure Divide(Number y) => this.Value.Divide(y);
    public static Pressure operator /(Pressure x, Number y) => x.Divide(y);
    public Pressure Modulo(Number y) => this.Value.Modulo(y);
    public static Pressure operator %(Pressure x, Number y) => x.Modulo(y);
    public Pressure Negative => this.Value.Negative;
    public static Pressure operator -(Pressure x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Pressure min, Pressure max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Pressure Clamp(Pressure a, Pressure b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Pressure b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Pressure a, Pressure b) => a.Equals(b);
    public Boolean NotEquals(Pressure b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Pressure a, Pressure b) => a.NotEquals(b);
    public Boolean LessThan(Pressure b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Pressure a, Pressure b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Pressure b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Pressure a, Pressure b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Pressure b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Pressure a, Pressure b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Pressure b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Pressure a, Pressure b) => a.GreaterThanOrEquals(b);
    public Pressure Lesser(Pressure b) => this.LessThanOrEquals(b) ? this : b;
    public Pressure Greater(Pressure b) => this.GreaterThanOrEquals(b) ? this : b;
    public Pressure Half => this.Divide(((Number)2));
    public Pressure Quarter => this.Divide(((Number)4));
    public Pressure Eighth => this.Divide(((Number)8));
    public Pressure Tenth => this.Divide(((Number)10));
    public Pressure Twice => this.Multiply(((Number)2));
    public Pressure Lerp(Pressure b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Energy
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public Energy Add(Number y) => this.Value.Add(y);
    public static Energy operator +(Energy x, Number y) => x.Add(y);
    public Energy Subtract(Number y) => this.Value.Subtract(y);
    public static Energy operator -(Energy x, Number y) => x.Subtract(y);
    public Energy Multiply(Number y) => this.Value.Multiply(y);
    public static Energy operator *(Energy x, Number y) => x.Multiply(y);
    public Energy Divide(Number y) => this.Value.Divide(y);
    public static Energy operator /(Energy x, Number y) => x.Divide(y);
    public Energy Modulo(Number y) => this.Value.Modulo(y);
    public static Energy operator %(Energy x, Number y) => x.Modulo(y);
    public Energy Negative => this.Value.Negative;
    public static Energy operator -(Energy x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Energy min, Energy max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Energy Clamp(Energy a, Energy b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Energy b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Energy a, Energy b) => a.Equals(b);
    public Boolean NotEquals(Energy b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Energy a, Energy b) => a.NotEquals(b);
    public Boolean LessThan(Energy b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Energy a, Energy b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Energy b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Energy a, Energy b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Energy b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Energy a, Energy b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Energy b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Energy a, Energy b) => a.GreaterThanOrEquals(b);
    public Energy Lesser(Energy b) => this.LessThanOrEquals(b) ? this : b;
    public Energy Greater(Energy b) => this.GreaterThanOrEquals(b) ? this : b;
    public Energy Half => this.Divide(((Number)2));
    public Energy Quarter => this.Divide(((Number)4));
    public Energy Eighth => this.Divide(((Number)8));
    public Energy Tenth => this.Divide(((Number)10));
    public Energy Twice => this.Multiply(((Number)2));
    public Energy Lerp(Energy b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Memory
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public Memory Add(Number y) => this.Value.Add(y);
    public static Memory operator +(Memory x, Number y) => x.Add(y);
    public Memory Subtract(Number y) => this.Value.Subtract(y);
    public static Memory operator -(Memory x, Number y) => x.Subtract(y);
    public Memory Multiply(Number y) => this.Value.Multiply(y);
    public static Memory operator *(Memory x, Number y) => x.Multiply(y);
    public Memory Divide(Number y) => this.Value.Divide(y);
    public static Memory operator /(Memory x, Number y) => x.Divide(y);
    public Memory Modulo(Number y) => this.Value.Modulo(y);
    public static Memory operator %(Memory x, Number y) => x.Modulo(y);
    public Memory Negative => this.Value.Negative;
    public static Memory operator -(Memory x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Memory min, Memory max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Memory Clamp(Memory a, Memory b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Memory b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Memory a, Memory b) => a.Equals(b);
    public Boolean NotEquals(Memory b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Memory a, Memory b) => a.NotEquals(b);
    public Boolean LessThan(Memory b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Memory a, Memory b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Memory b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Memory a, Memory b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Memory b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Memory a, Memory b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Memory b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Memory a, Memory b) => a.GreaterThanOrEquals(b);
    public Memory Lesser(Memory b) => this.LessThanOrEquals(b) ? this : b;
    public Memory Greater(Memory b) => this.GreaterThanOrEquals(b) ? this : b;
    public Memory Half => this.Divide(((Number)2));
    public Memory Quarter => this.Divide(((Number)4));
    public Memory Eighth => this.Divide(((Number)8));
    public Memory Tenth => this.Divide(((Number)10));
    public Memory Twice => this.Multiply(((Number)2));
    public Memory Lerp(Memory b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Frequency
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public Frequency Add(Number y) => this.Value.Add(y);
    public static Frequency operator +(Frequency x, Number y) => x.Add(y);
    public Frequency Subtract(Number y) => this.Value.Subtract(y);
    public static Frequency operator -(Frequency x, Number y) => x.Subtract(y);
    public Frequency Multiply(Number y) => this.Value.Multiply(y);
    public static Frequency operator *(Frequency x, Number y) => x.Multiply(y);
    public Frequency Divide(Number y) => this.Value.Divide(y);
    public static Frequency operator /(Frequency x, Number y) => x.Divide(y);
    public Frequency Modulo(Number y) => this.Value.Modulo(y);
    public static Frequency operator %(Frequency x, Number y) => x.Modulo(y);
    public Frequency Negative => this.Value.Negative;
    public static Frequency operator -(Frequency x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Frequency min, Frequency max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Frequency Clamp(Frequency a, Frequency b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Frequency b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Frequency a, Frequency b) => a.Equals(b);
    public Boolean NotEquals(Frequency b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Frequency a, Frequency b) => a.NotEquals(b);
    public Boolean LessThan(Frequency b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Frequency a, Frequency b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Frequency b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Frequency a, Frequency b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Frequency b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Frequency a, Frequency b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Frequency b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Frequency a, Frequency b) => a.GreaterThanOrEquals(b);
    public Frequency Lesser(Frequency b) => this.LessThanOrEquals(b) ? this : b;
    public Frequency Greater(Frequency b) => this.GreaterThanOrEquals(b) ? this : b;
    public Frequency Half => this.Divide(((Number)2));
    public Frequency Quarter => this.Divide(((Number)4));
    public Frequency Eighth => this.Divide(((Number)8));
    public Frequency Tenth => this.Divide(((Number)10));
    public Frequency Twice => this.Multiply(((Number)2));
    public Frequency Lerp(Frequency b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Loudness
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public Loudness Add(Number y) => this.Value.Add(y);
    public static Loudness operator +(Loudness x, Number y) => x.Add(y);
    public Loudness Subtract(Number y) => this.Value.Subtract(y);
    public static Loudness operator -(Loudness x, Number y) => x.Subtract(y);
    public Loudness Multiply(Number y) => this.Value.Multiply(y);
    public static Loudness operator *(Loudness x, Number y) => x.Multiply(y);
    public Loudness Divide(Number y) => this.Value.Divide(y);
    public static Loudness operator /(Loudness x, Number y) => x.Divide(y);
    public Loudness Modulo(Number y) => this.Value.Modulo(y);
    public static Loudness operator %(Loudness x, Number y) => x.Modulo(y);
    public Loudness Negative => this.Value.Negative;
    public static Loudness operator -(Loudness x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Loudness min, Loudness max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Loudness Clamp(Loudness a, Loudness b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Loudness b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Loudness a, Loudness b) => a.Equals(b);
    public Boolean NotEquals(Loudness b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Loudness a, Loudness b) => a.NotEquals(b);
    public Boolean LessThan(Loudness b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Loudness a, Loudness b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Loudness b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Loudness a, Loudness b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Loudness b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Loudness a, Loudness b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Loudness b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Loudness a, Loudness b) => a.GreaterThanOrEquals(b);
    public Loudness Lesser(Loudness b) => this.LessThanOrEquals(b) ? this : b;
    public Loudness Greater(Loudness b) => this.GreaterThanOrEquals(b) ? this : b;
    public Loudness Half => this.Divide(((Number)2));
    public Loudness Quarter => this.Divide(((Number)4));
    public Loudness Eighth => this.Divide(((Number)8));
    public Loudness Tenth => this.Divide(((Number)10));
    public Loudness Twice => this.Multiply(((Number)2));
    public Loudness Lerp(Loudness b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct LuminousIntensity
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public LuminousIntensity Add(Number y) => this.Value.Add(y);
    public static LuminousIntensity operator +(LuminousIntensity x, Number y) => x.Add(y);
    public LuminousIntensity Subtract(Number y) => this.Value.Subtract(y);
    public static LuminousIntensity operator -(LuminousIntensity x, Number y) => x.Subtract(y);
    public LuminousIntensity Multiply(Number y) => this.Value.Multiply(y);
    public static LuminousIntensity operator *(LuminousIntensity x, Number y) => x.Multiply(y);
    public LuminousIntensity Divide(Number y) => this.Value.Divide(y);
    public static LuminousIntensity operator /(LuminousIntensity x, Number y) => x.Divide(y);
    public LuminousIntensity Modulo(Number y) => this.Value.Modulo(y);
    public static LuminousIntensity operator %(LuminousIntensity x, Number y) => x.Modulo(y);
    public LuminousIntensity Negative => this.Value.Negative;
    public static LuminousIntensity operator -(LuminousIntensity x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(LuminousIntensity min, LuminousIntensity max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public LuminousIntensity Clamp(LuminousIntensity a, LuminousIntensity b) => this.Greater(a).Lesser(b);
    public Boolean Equals(LuminousIntensity b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(LuminousIntensity a, LuminousIntensity b) => a.Equals(b);
    public Boolean NotEquals(LuminousIntensity b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(LuminousIntensity a, LuminousIntensity b) => a.NotEquals(b);
    public Boolean LessThan(LuminousIntensity b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(LuminousIntensity a, LuminousIntensity b) => a.LessThan(b);
    public Boolean LessThanOrEquals(LuminousIntensity b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(LuminousIntensity a, LuminousIntensity b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(LuminousIntensity b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(LuminousIntensity a, LuminousIntensity b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(LuminousIntensity b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(LuminousIntensity a, LuminousIntensity b) => a.GreaterThanOrEquals(b);
    public LuminousIntensity Lesser(LuminousIntensity b) => this.LessThanOrEquals(b) ? this : b;
    public LuminousIntensity Greater(LuminousIntensity b) => this.GreaterThanOrEquals(b) ? this : b;
    public LuminousIntensity Half => this.Divide(((Number)2));
    public LuminousIntensity Quarter => this.Divide(((Number)4));
    public LuminousIntensity Eighth => this.Divide(((Number)8));
    public LuminousIntensity Tenth => this.Divide(((Number)10));
    public LuminousIntensity Twice => this.Multiply(((Number)2));
    public LuminousIntensity Lerp(LuminousIntensity b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct ElectricPotential
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public ElectricPotential Add(Number y) => this.Value.Add(y);
    public static ElectricPotential operator +(ElectricPotential x, Number y) => x.Add(y);
    public ElectricPotential Subtract(Number y) => this.Value.Subtract(y);
    public static ElectricPotential operator -(ElectricPotential x, Number y) => x.Subtract(y);
    public ElectricPotential Multiply(Number y) => this.Value.Multiply(y);
    public static ElectricPotential operator *(ElectricPotential x, Number y) => x.Multiply(y);
    public ElectricPotential Divide(Number y) => this.Value.Divide(y);
    public static ElectricPotential operator /(ElectricPotential x, Number y) => x.Divide(y);
    public ElectricPotential Modulo(Number y) => this.Value.Modulo(y);
    public static ElectricPotential operator %(ElectricPotential x, Number y) => x.Modulo(y);
    public ElectricPotential Negative => this.Value.Negative;
    public static ElectricPotential operator -(ElectricPotential x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(ElectricPotential min, ElectricPotential max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ElectricPotential Clamp(ElectricPotential a, ElectricPotential b) => this.Greater(a).Lesser(b);
    public Boolean Equals(ElectricPotential b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ElectricPotential a, ElectricPotential b) => a.Equals(b);
    public Boolean NotEquals(ElectricPotential b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ElectricPotential a, ElectricPotential b) => a.NotEquals(b);
    public Boolean LessThan(ElectricPotential b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ElectricPotential a, ElectricPotential b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricPotential b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ElectricPotential a, ElectricPotential b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricPotential b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ElectricPotential a, ElectricPotential b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricPotential b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ElectricPotential a, ElectricPotential b) => a.GreaterThanOrEquals(b);
    public ElectricPotential Lesser(ElectricPotential b) => this.LessThanOrEquals(b) ? this : b;
    public ElectricPotential Greater(ElectricPotential b) => this.GreaterThanOrEquals(b) ? this : b;
    public ElectricPotential Half => this.Divide(((Number)2));
    public ElectricPotential Quarter => this.Divide(((Number)4));
    public ElectricPotential Eighth => this.Divide(((Number)8));
    public ElectricPotential Tenth => this.Divide(((Number)10));
    public ElectricPotential Twice => this.Multiply(((Number)2));
    public ElectricPotential Lerp(ElectricPotential b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct ElectricCharge
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public ElectricCharge Add(Number y) => this.Value.Add(y);
    public static ElectricCharge operator +(ElectricCharge x, Number y) => x.Add(y);
    public ElectricCharge Subtract(Number y) => this.Value.Subtract(y);
    public static ElectricCharge operator -(ElectricCharge x, Number y) => x.Subtract(y);
    public ElectricCharge Multiply(Number y) => this.Value.Multiply(y);
    public static ElectricCharge operator *(ElectricCharge x, Number y) => x.Multiply(y);
    public ElectricCharge Divide(Number y) => this.Value.Divide(y);
    public static ElectricCharge operator /(ElectricCharge x, Number y) => x.Divide(y);
    public ElectricCharge Modulo(Number y) => this.Value.Modulo(y);
    public static ElectricCharge operator %(ElectricCharge x, Number y) => x.Modulo(y);
    public ElectricCharge Negative => this.Value.Negative;
    public static ElectricCharge operator -(ElectricCharge x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(ElectricCharge min, ElectricCharge max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ElectricCharge Clamp(ElectricCharge a, ElectricCharge b) => this.Greater(a).Lesser(b);
    public Boolean Equals(ElectricCharge b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ElectricCharge a, ElectricCharge b) => a.Equals(b);
    public Boolean NotEquals(ElectricCharge b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ElectricCharge a, ElectricCharge b) => a.NotEquals(b);
    public Boolean LessThan(ElectricCharge b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ElectricCharge a, ElectricCharge b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricCharge b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ElectricCharge a, ElectricCharge b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricCharge b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ElectricCharge a, ElectricCharge b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricCharge b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ElectricCharge a, ElectricCharge b) => a.GreaterThanOrEquals(b);
    public ElectricCharge Lesser(ElectricCharge b) => this.LessThanOrEquals(b) ? this : b;
    public ElectricCharge Greater(ElectricCharge b) => this.GreaterThanOrEquals(b) ? this : b;
    public ElectricCharge Half => this.Divide(((Number)2));
    public ElectricCharge Quarter => this.Divide(((Number)4));
    public ElectricCharge Eighth => this.Divide(((Number)8));
    public ElectricCharge Tenth => this.Divide(((Number)10));
    public ElectricCharge Twice => this.Multiply(((Number)2));
    public ElectricCharge Lerp(ElectricCharge b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct ElectricCurrent
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public ElectricCurrent Add(Number y) => this.Value.Add(y);
    public static ElectricCurrent operator +(ElectricCurrent x, Number y) => x.Add(y);
    public ElectricCurrent Subtract(Number y) => this.Value.Subtract(y);
    public static ElectricCurrent operator -(ElectricCurrent x, Number y) => x.Subtract(y);
    public ElectricCurrent Multiply(Number y) => this.Value.Multiply(y);
    public static ElectricCurrent operator *(ElectricCurrent x, Number y) => x.Multiply(y);
    public ElectricCurrent Divide(Number y) => this.Value.Divide(y);
    public static ElectricCurrent operator /(ElectricCurrent x, Number y) => x.Divide(y);
    public ElectricCurrent Modulo(Number y) => this.Value.Modulo(y);
    public static ElectricCurrent operator %(ElectricCurrent x, Number y) => x.Modulo(y);
    public ElectricCurrent Negative => this.Value.Negative;
    public static ElectricCurrent operator -(ElectricCurrent x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(ElectricCurrent min, ElectricCurrent max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ElectricCurrent Clamp(ElectricCurrent a, ElectricCurrent b) => this.Greater(a).Lesser(b);
    public Boolean Equals(ElectricCurrent b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ElectricCurrent a, ElectricCurrent b) => a.Equals(b);
    public Boolean NotEquals(ElectricCurrent b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ElectricCurrent a, ElectricCurrent b) => a.NotEquals(b);
    public Boolean LessThan(ElectricCurrent b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ElectricCurrent a, ElectricCurrent b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricCurrent b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ElectricCurrent a, ElectricCurrent b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricCurrent b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ElectricCurrent a, ElectricCurrent b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricCurrent b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ElectricCurrent a, ElectricCurrent b) => a.GreaterThanOrEquals(b);
    public ElectricCurrent Lesser(ElectricCurrent b) => this.LessThanOrEquals(b) ? this : b;
    public ElectricCurrent Greater(ElectricCurrent b) => this.GreaterThanOrEquals(b) ? this : b;
    public ElectricCurrent Half => this.Divide(((Number)2));
    public ElectricCurrent Quarter => this.Divide(((Number)4));
    public ElectricCurrent Eighth => this.Divide(((Number)8));
    public ElectricCurrent Tenth => this.Divide(((Number)10));
    public ElectricCurrent Twice => this.Multiply(((Number)2));
    public ElectricCurrent Lerp(ElectricCurrent b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct ElectricResistance
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public ElectricResistance Add(Number y) => this.Value.Add(y);
    public static ElectricResistance operator +(ElectricResistance x, Number y) => x.Add(y);
    public ElectricResistance Subtract(Number y) => this.Value.Subtract(y);
    public static ElectricResistance operator -(ElectricResistance x, Number y) => x.Subtract(y);
    public ElectricResistance Multiply(Number y) => this.Value.Multiply(y);
    public static ElectricResistance operator *(ElectricResistance x, Number y) => x.Multiply(y);
    public ElectricResistance Divide(Number y) => this.Value.Divide(y);
    public static ElectricResistance operator /(ElectricResistance x, Number y) => x.Divide(y);
    public ElectricResistance Modulo(Number y) => this.Value.Modulo(y);
    public static ElectricResistance operator %(ElectricResistance x, Number y) => x.Modulo(y);
    public ElectricResistance Negative => this.Value.Negative;
    public static ElectricResistance operator -(ElectricResistance x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(ElectricResistance min, ElectricResistance max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ElectricResistance Clamp(ElectricResistance a, ElectricResistance b) => this.Greater(a).Lesser(b);
    public Boolean Equals(ElectricResistance b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ElectricResistance a, ElectricResistance b) => a.Equals(b);
    public Boolean NotEquals(ElectricResistance b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ElectricResistance a, ElectricResistance b) => a.NotEquals(b);
    public Boolean LessThan(ElectricResistance b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ElectricResistance a, ElectricResistance b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricResistance b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ElectricResistance a, ElectricResistance b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricResistance b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ElectricResistance a, ElectricResistance b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricResistance b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ElectricResistance a, ElectricResistance b) => a.GreaterThanOrEquals(b);
    public ElectricResistance Lesser(ElectricResistance b) => this.LessThanOrEquals(b) ? this : b;
    public ElectricResistance Greater(ElectricResistance b) => this.GreaterThanOrEquals(b) ? this : b;
    public ElectricResistance Half => this.Divide(((Number)2));
    public ElectricResistance Quarter => this.Divide(((Number)4));
    public ElectricResistance Eighth => this.Divide(((Number)8));
    public ElectricResistance Tenth => this.Divide(((Number)10));
    public ElectricResistance Twice => this.Multiply(((Number)2));
    public ElectricResistance Lerp(ElectricResistance b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Power
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public Power Add(Number y) => this.Value.Add(y);
    public static Power operator +(Power x, Number y) => x.Add(y);
    public Power Subtract(Number y) => this.Value.Subtract(y);
    public static Power operator -(Power x, Number y) => x.Subtract(y);
    public Power Multiply(Number y) => this.Value.Multiply(y);
    public static Power operator *(Power x, Number y) => x.Multiply(y);
    public Power Divide(Number y) => this.Value.Divide(y);
    public static Power operator /(Power x, Number y) => x.Divide(y);
    public Power Modulo(Number y) => this.Value.Modulo(y);
    public static Power operator %(Power x, Number y) => x.Modulo(y);
    public Power Negative => this.Value.Negative;
    public static Power operator -(Power x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Power min, Power max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Power Clamp(Power a, Power b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Power b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Power a, Power b) => a.Equals(b);
    public Boolean NotEquals(Power b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Power a, Power b) => a.NotEquals(b);
    public Boolean LessThan(Power b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Power a, Power b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Power b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Power a, Power b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Power b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Power a, Power b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Power b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Power a, Power b) => a.GreaterThanOrEquals(b);
    public Power Lesser(Power b) => this.LessThanOrEquals(b) ? this : b;
    public Power Greater(Power b) => this.GreaterThanOrEquals(b) ? this : b;
    public Power Half => this.Divide(((Number)2));
    public Power Quarter => this.Divide(((Number)4));
    public Power Eighth => this.Divide(((Number)8));
    public Power Tenth => this.Divide(((Number)10));
    public Power Twice => this.Multiply(((Number)2));
    public Power Lerp(Power b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Density
{
    public Number Value => this.FieldValues.At(((Integer)0));
    public Density Add(Number y) => this.Value.Add(y);
    public static Density operator +(Density x, Number y) => x.Add(y);
    public Density Subtract(Number y) => this.Value.Subtract(y);
    public static Density operator -(Density x, Number y) => x.Subtract(y);
    public Density Multiply(Number y) => this.Value.Multiply(y);
    public static Density operator *(Density x, Number y) => x.Multiply(y);
    public Density Divide(Number y) => this.Value.Divide(y);
    public static Density operator /(Density x, Number y) => x.Divide(y);
    public Density Modulo(Number y) => this.Value.Modulo(y);
    public static Density operator %(Density x, Number y) => x.Modulo(y);
    public Density Negative => this.Value.Negative;
    public static Density operator -(Density x) => x.Negative;
    public Number Magnitude => this.Value;
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Boolean Between(Density min, Density max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Density Clamp(Density a, Density b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Density b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Density a, Density b) => a.Equals(b);
    public Boolean NotEquals(Density b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Density a, Density b) => a.NotEquals(b);
    public Boolean LessThan(Density b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Density a, Density b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Density b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Density a, Density b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Density b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Density a, Density b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Density b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Density a, Density b) => a.GreaterThanOrEquals(b);
    public Density Lesser(Density b) => this.LessThanOrEquals(b) ? this : b;
    public Density Greater(Density b) => this.GreaterThanOrEquals(b) ? this : b;
    public Density Half => this.Divide(((Number)2));
    public Density Quarter => this.Divide(((Number)4));
    public Density Eighth => this.Divide(((Number)8));
    public Density Tenth => this.Divide(((Number)10));
    public Density Twice => this.Multiply(((Number)2));
    public Density Lerp(Density b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct NormalDistribution
{
}
public readonly partial struct PoissonDistribution
{
}
public readonly partial struct BernoulliDistribution
{
}
public readonly partial struct BinomialDistribution
{
}
public readonly partial struct Tuple2<T0, T1>
{
}
public readonly partial struct Tuple3<T0, T1, T2>
{
}
public readonly partial struct Function0<TR>
{
}
public readonly partial struct Function1<T0, TR>
{
}
public readonly partial struct Function2<T0, T1, TR>
{
}
public readonly partial struct Function3<T0, T1, T2, TR>
{
}
