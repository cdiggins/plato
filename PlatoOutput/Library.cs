using System;
public static class Constants
{
    public static Number TwoPi => Constants.Pi.Twice;
    public static Number Pi => ((Number)3.1415926535897);
    public static Number Epsilon => ((Number)1E-15);
}
public readonly partial struct Number
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
    public Number SquareRoot =>
        this.Pow(((Number)0.5));
    public Number SmoothStep =>
        this.Square.Multiply(((Number)3).Subtract(this.Twice));
    public Number ClampOne =>
        this.Clamp(((Number)0), ((Number)1));
    public Boolean AlmostZero =>
        this.Abs.LessThan(Constants.Epsilon);
    public Angle Radians =>
        this;
    public Angle Degrees =>
        this.Divide(((Integer)360)).Turns;
    public Angle Turns =>
        this.Multiply(Constants.TwoPi);
    public Temperature Fahrenheit =>
        this.Subtract(((Number)32)).Multiply(((Number)5).Divide(((Number)9)));
    public Temperature Kelvin =>
        this.Subtract(((Number)273.15));
    public Number Square =>
        this.Multiply(this);
    public Number PlusOne =>
        this.Add(this.One);
    public Number MinusOne =>
        this.Subtract(this.One);
    public Number FromOne =>
        this.One.Subtract(this);
    public Boolean IsPositive =>
        this.GtEqZ;
    public Boolean GtZ =>
        this.GreaterThan(this.Zero);
    public Boolean LtZ =>
        this.LessThan(this.Zero);
    public Boolean GtEqZ =>
        this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ =>
        this.LessThanOrEquals(this.Zero);
    public Boolean IsNegative =>
        this.LessThan(this.Zero);
    public Number Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Number Abs =>
        this.LtZ ? this.Negative : this;
    public Number Half =>
        this.Divide(((Number)2));
    public Number Quarter =>
        this.Divide(((Number)4));
    public Number Eighth =>
        this.Divide(((Number)8));
    public Number Tenth =>
        this.Divide(((Number)10));
    public Number Twice =>
        this.Multiply(((Number)2));
    public Number Pow2 =>
        this.Multiply(this);
    public Number MultiplyEpsilon(Number y) =>
        this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(Number y) =>
        this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public Number Lerp(Number b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Number min, Number max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Number Clamp(Number a, Number b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Number b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Number a, Number b) => a.Equals(b);
    public Boolean NotEquals(Number b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Number a, Number b) => a.NotEquals(b);
    public Boolean LessThan(Number b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Number a, Number b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Number b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Number a, Number b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Number b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Number a, Number b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Number b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Number a, Number b) => a.GreaterThanOrEquals(b);
    public Number Lesser(Number b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Number Greater(Number b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Integer
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
    public Boolean Between(Integer min, Integer max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Integer Clamp(Integer a, Integer b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Integer b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Integer a, Integer b) => a.Equals(b);
    public Boolean NotEquals(Integer b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Integer a, Integer b) => a.NotEquals(b);
    public Boolean LessThan(Integer b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Integer a, Integer b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Integer b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Integer a, Integer b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Integer b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Integer a, Integer b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Integer b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Integer a, Integer b) => a.GreaterThanOrEquals(b);
    public Integer Lesser(Integer b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Integer Greater(Integer b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct String
{
    public Boolean Between(String min, String max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public String Clamp(String a, String b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(String b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(String a, String b) => a.Equals(b);
    public Boolean NotEquals(String b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(String a, String b) => a.NotEquals(b);
    public Boolean LessThan(String b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(String a, String b) => a.LessThan(b);
    public Boolean LessThanOrEquals(String b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(String a, String b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(String b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(String a, String b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(String b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(String a, String b) => a.GreaterThanOrEquals(b);
    public String Lesser(String b) =>
        this.LessThanOrEquals(b) ? this : b;
    public String Greater(String b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Boolean
{
    public Boolean And(Boolean y) => throw new NotImplementedException();
    public static Boolean operator &(Boolean x, Boolean y) => x.And(y);
    public Boolean Or(Boolean y) => throw new NotImplementedException();
    public static Boolean operator |(Boolean x, Boolean y) => x.Or(y);
    public Boolean Not => throw new NotImplementedException();
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
    public Boolean Between(Cardinal min, Cardinal max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Cardinal Clamp(Cardinal a, Cardinal b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Cardinal b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Cardinal a, Cardinal b) => a.Equals(b);
    public Boolean NotEquals(Cardinal b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Cardinal a, Cardinal b) => a.NotEquals(b);
    public Boolean LessThan(Cardinal b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Cardinal a, Cardinal b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Cardinal b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Cardinal a, Cardinal b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Cardinal b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Cardinal a, Cardinal b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Cardinal b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Cardinal a, Cardinal b) => a.GreaterThanOrEquals(b);
    public Cardinal Lesser(Cardinal b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Cardinal Greater(Cardinal b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Index
{
    public Boolean Between(Index min, Index max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Index Clamp(Index a, Index b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Index b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Index a, Index b) => a.Equals(b);
    public Boolean NotEquals(Index b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Index a, Index b) => a.NotEquals(b);
    public Boolean LessThan(Index b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Index a, Index b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Index b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Index a, Index b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Index b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Index a, Index b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Index b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Index a, Index b) => a.GreaterThanOrEquals(b);
    public Index Lesser(Index b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Index Greater(Index b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Unit
{
    public Unit Square =>
        this.Multiply(this);
    public Unit PlusOne =>
        this.Add(this.One);
    public Unit MinusOne =>
        this.Subtract(this.One);
    public Unit FromOne =>
        this.One.Subtract(this);
    public Boolean IsPositive =>
        this.GtEqZ;
    public Boolean GtZ =>
        this.GreaterThan(this.Zero);
    public Boolean LtZ =>
        this.LessThan(this.Zero);
    public Boolean GtEqZ =>
        this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ =>
        this.LessThanOrEquals(this.Zero);
    public Boolean IsNegative =>
        this.LessThan(this.Zero);
    public Unit Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Unit Abs =>
        this.LtZ ? this.Negative : this;
    public Unit Half =>
        this.Divide(((Number)2));
    public Unit Quarter =>
        this.Divide(((Number)4));
    public Unit Eighth =>
        this.Divide(((Number)8));
    public Unit Tenth =>
        this.Divide(((Number)10));
    public Unit Twice =>
        this.Multiply(((Number)2));
    public Unit Pow2 =>
        this.Multiply(this);
    public Unit MultiplyEpsilon(Unit y) =>
        this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(Unit y) =>
        this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public Unit Lerp(Unit b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Unit min, Unit max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Unit Clamp(Unit a, Unit b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Unit b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Unit a, Unit b) => a.Equals(b);
    public Boolean NotEquals(Unit b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Unit a, Unit b) => a.NotEquals(b);
    public Boolean LessThan(Unit b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Unit a, Unit b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Unit b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Unit a, Unit b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Unit b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Unit a, Unit b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Unit b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Unit a, Unit b) => a.GreaterThanOrEquals(b);
    public Unit Lesser(Unit b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Unit Greater(Unit b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Percent
{
    public Percent Square =>
        this.Multiply(this);
    public Percent PlusOne =>
        this.Add(this.One);
    public Percent MinusOne =>
        this.Subtract(this.One);
    public Percent FromOne =>
        this.One.Subtract(this);
    public Boolean IsPositive =>
        this.GtEqZ;
    public Boolean GtZ =>
        this.GreaterThan(this.Zero);
    public Boolean LtZ =>
        this.LessThan(this.Zero);
    public Boolean GtEqZ =>
        this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ =>
        this.LessThanOrEquals(this.Zero);
    public Boolean IsNegative =>
        this.LessThan(this.Zero);
    public Percent Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Percent Abs =>
        this.LtZ ? this.Negative : this;
    public Percent Half =>
        this.Divide(((Number)2));
    public Percent Quarter =>
        this.Divide(((Number)4));
    public Percent Eighth =>
        this.Divide(((Number)8));
    public Percent Tenth =>
        this.Divide(((Number)10));
    public Percent Twice =>
        this.Multiply(((Number)2));
    public Percent Pow2 =>
        this.Multiply(this);
    public Percent MultiplyEpsilon(Percent y) =>
        this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(Percent y) =>
        this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public Percent Lerp(Percent b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Percent min, Percent max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Percent Clamp(Percent a, Percent b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Percent b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Percent a, Percent b) => a.Equals(b);
    public Boolean NotEquals(Percent b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Percent a, Percent b) => a.NotEquals(b);
    public Boolean LessThan(Percent b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Percent a, Percent b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Percent b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Percent a, Percent b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Percent b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Percent a, Percent b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Percent b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Percent a, Percent b) => a.GreaterThanOrEquals(b);
    public Percent Lesser(Percent b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Percent Greater(Percent b) =>
        this.GreaterThanOrEquals(b) ? this : b;
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
    public TR Aggregate<TR>(TR r, Function2<TR, Number, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(Add);
    public Number SumSquares =>
        this.Square.Sum;
    public Number MagnitudeSquared =>
        this.SumSquares;
    public Number Magnitude =>
        this.MagnitudeSquared.SquareRoot;
    public Number Dot(Vector2D v2) =>
        this.Multiply(v2).Sum;
    public Vector2D Normal =>
        this.Divide(this.Magnitude);
    public Number Average =>
        this.Sum.Divide(this.Count);
    public Vector2D Square =>
        this.Multiply(this);
    public Vector2D PlusOne =>
        this.Add(this.One);
    public Vector2D MinusOne =>
        this.Subtract(this.One);
    public Vector2D FromOne =>
        this.One.Subtract(this);
    public Boolean IsPositive =>
        this.GtEqZ;
    public Boolean GtZ =>
        this.GreaterThan(this.Zero);
    public Boolean LtZ =>
        this.LessThan(this.Zero);
    public Boolean GtEqZ =>
        this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ =>
        this.LessThanOrEquals(this.Zero);
    public Boolean IsNegative =>
        this.LessThan(this.Zero);
    public Vector2D Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Vector2D Abs =>
        this.LtZ ? this.Negative : this;
    public Vector2D Half =>
        this.Divide(((Number)2));
    public Vector2D Quarter =>
        this.Divide(((Number)4));
    public Vector2D Eighth =>
        this.Divide(((Number)8));
    public Vector2D Tenth =>
        this.Divide(((Number)10));
    public Vector2D Twice =>
        this.Multiply(((Number)2));
    public Vector2D Pow2 =>
        this.Multiply(this);
    public Vector2D MultiplyEpsilon(Vector2D y) =>
        this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(Vector2D y) =>
        this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public Vector2D Lerp(Vector2D b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Vector2D min, Vector2D max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Vector2D Clamp(Vector2D a, Vector2D b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Vector2D b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Vector2D a, Vector2D b) => a.Equals(b);
    public Boolean NotEquals(Vector2D b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Vector2D a, Vector2D b) => a.NotEquals(b);
    public Boolean LessThan(Vector2D b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Vector2D a, Vector2D b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Vector2D b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Vector2D a, Vector2D b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Vector2D b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Vector2D a, Vector2D b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Vector2D b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Vector2D a, Vector2D b) => a.GreaterThanOrEquals(b);
    public Vector2D Lesser(Vector2D b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Vector2D Greater(Vector2D b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Vector3D
{
    public TR Aggregate<TR>(TR r, Function2<TR, Number, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(Add);
    public Number SumSquares =>
        this.Square.Sum;
    public Number MagnitudeSquared =>
        this.SumSquares;
    public Number Magnitude =>
        this.MagnitudeSquared.SquareRoot;
    public Number Dot(Vector3D v2) =>
        this.Multiply(v2).Sum;
    public Vector3D Normal =>
        this.Divide(this.Magnitude);
    public Number Average =>
        this.Sum.Divide(this.Count);
    public Vector3D Square =>
        this.Multiply(this);
    public Vector3D PlusOne =>
        this.Add(this.One);
    public Vector3D MinusOne =>
        this.Subtract(this.One);
    public Vector3D FromOne =>
        this.One.Subtract(this);
    public Boolean IsPositive =>
        this.GtEqZ;
    public Boolean GtZ =>
        this.GreaterThan(this.Zero);
    public Boolean LtZ =>
        this.LessThan(this.Zero);
    public Boolean GtEqZ =>
        this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ =>
        this.LessThanOrEquals(this.Zero);
    public Boolean IsNegative =>
        this.LessThan(this.Zero);
    public Vector3D Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Vector3D Abs =>
        this.LtZ ? this.Negative : this;
    public Vector3D Half =>
        this.Divide(((Number)2));
    public Vector3D Quarter =>
        this.Divide(((Number)4));
    public Vector3D Eighth =>
        this.Divide(((Number)8));
    public Vector3D Tenth =>
        this.Divide(((Number)10));
    public Vector3D Twice =>
        this.Multiply(((Number)2));
    public Vector3D Pow2 =>
        this.Multiply(this);
    public Vector3D MultiplyEpsilon(Vector3D y) =>
        this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(Vector3D y) =>
        this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public Vector3D Lerp(Vector3D b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Vector3D min, Vector3D max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Vector3D Clamp(Vector3D a, Vector3D b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Vector3D b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Vector3D a, Vector3D b) => a.Equals(b);
    public Boolean NotEquals(Vector3D b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Vector3D a, Vector3D b) => a.NotEquals(b);
    public Boolean LessThan(Vector3D b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Vector3D a, Vector3D b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Vector3D b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Vector3D a, Vector3D b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Vector3D b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Vector3D a, Vector3D b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Vector3D b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Vector3D a, Vector3D b) => a.GreaterThanOrEquals(b);
    public Vector3D Lesser(Vector3D b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Vector3D Greater(Vector3D b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Vector4D
{
    public TR Aggregate<TR>(TR r, Function2<TR, Number, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(Add);
    public Number SumSquares =>
        this.Square.Sum;
    public Number MagnitudeSquared =>
        this.SumSquares;
    public Number Magnitude =>
        this.MagnitudeSquared.SquareRoot;
    public Number Dot(Vector4D v2) =>
        this.Multiply(v2).Sum;
    public Vector4D Normal =>
        this.Divide(this.Magnitude);
    public Number Average =>
        this.Sum.Divide(this.Count);
    public Vector4D Square =>
        this.Multiply(this);
    public Vector4D PlusOne =>
        this.Add(this.One);
    public Vector4D MinusOne =>
        this.Subtract(this.One);
    public Vector4D FromOne =>
        this.One.Subtract(this);
    public Boolean IsPositive =>
        this.GtEqZ;
    public Boolean GtZ =>
        this.GreaterThan(this.Zero);
    public Boolean LtZ =>
        this.LessThan(this.Zero);
    public Boolean GtEqZ =>
        this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ =>
        this.LessThanOrEquals(this.Zero);
    public Boolean IsNegative =>
        this.LessThan(this.Zero);
    public Vector4D Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Vector4D Abs =>
        this.LtZ ? this.Negative : this;
    public Vector4D Half =>
        this.Divide(((Number)2));
    public Vector4D Quarter =>
        this.Divide(((Number)4));
    public Vector4D Eighth =>
        this.Divide(((Number)8));
    public Vector4D Tenth =>
        this.Divide(((Number)10));
    public Vector4D Twice =>
        this.Multiply(((Number)2));
    public Vector4D Pow2 =>
        this.Multiply(this);
    public Vector4D MultiplyEpsilon(Vector4D y) =>
        this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(Vector4D y) =>
        this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public Vector4D Lerp(Vector4D b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Vector4D min, Vector4D max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Vector4D Clamp(Vector4D a, Vector4D b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Vector4D b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Vector4D a, Vector4D b) => a.Equals(b);
    public Boolean NotEquals(Vector4D b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Vector4D a, Vector4D b) => a.NotEquals(b);
    public Boolean LessThan(Vector4D b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Vector4D a, Vector4D b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Vector4D b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Vector4D a, Vector4D b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Vector4D b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Vector4D a, Vector4D b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Vector4D b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Vector4D a, Vector4D b) => a.GreaterThanOrEquals(b);
    public Vector4D Lesser(Vector4D b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Vector4D Greater(Vector4D b) =>
        this.GreaterThanOrEquals(b) ? this : b;
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
    public Boolean IsEmpty =>
        this.Min.GreaterThanOrEquals(this.Max);
    public Point2D Lerp(Number amount) =>
        this.Min.Lerp(this.Max, amount);
    public Number Unlerp(Point2D value) =>
        value.Subtract(this.Min).Divide(this.Size);
    public AlignedBox2D Negate =>
        this.Max.Negative.Tuple(this.Min.Negative);
    public AlignedBox2D Reverse =>
        this.Max.Tuple(this.Min);
    public Point2D Center =>
        this.Lerp(((Number)0.5));
    public Boolean Contains(Point2D value) =>
        this.Min.LessThanOrEquals(value).And(value.LessThanOrEquals(this.Max));
    public Boolean Contains(AlignedBox2D other) =>
        this.Min.LessThanOrEquals(other.Min).And(this.Max.GreaterThanOrEquals(other.Max));
    public Boolean Overlaps(AlignedBox2D y) =>
        this.Clamp(y).IsEmpty.Not;
    public Tuple2<AlignedBox2D, AlignedBox2D> SplitAt(Number t) =>
        this.Left(t).Tuple(this.Right(t));
    public Tuple2<AlignedBox2D, AlignedBox2D> Split =>
        this.SplitAt(((Number)0.5));
    public AlignedBox2D Left(Number t) =>
        this.Min.Tuple(this.Lerp(t));
    public AlignedBox2D Right(Number t) =>
        this.Lerp(t).Tuple(this.Max);
    public AlignedBox2D MoveTo(Point2D v) =>
        v.Tuple(v.Add(this.Size));
    public AlignedBox2D LeftHalf =>
        this.Left(((Number)0.5));
    public AlignedBox2D RightHalf =>
        this.Right(((Number)0.5));
    public AlignedBox2D Recenter(Point2D c) =>
        c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public AlignedBox2D Clamp(AlignedBox2D y) =>
        this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Point2D Clamp(Point2D value) =>
        value.Lerp(this.Min, this.Max, value.Unlerp(this.Min, this.Max).Clamp(((Integer)0), ((Integer)1)));
    public Boolean Within(Point2D value) =>
        value.GreaterThanOrEquals(this.Min).And(value.LessThanOrEquals(this.Max));
}
public readonly partial struct AlignedBox3D
{
    public Boolean IsEmpty =>
        this.Min.GreaterThanOrEquals(this.Max);
    public Point3D Lerp(Number amount) =>
        this.Min.Lerp(this.Max, amount);
    public Number Unlerp(Point3D value) =>
        value.Subtract(this.Min).Divide(this.Size);
    public AlignedBox3D Negate =>
        this.Max.Negative.Tuple(this.Min.Negative);
    public AlignedBox3D Reverse =>
        this.Max.Tuple(this.Min);
    public Point3D Center =>
        this.Lerp(((Number)0.5));
    public Boolean Contains(Point3D value) =>
        this.Min.LessThanOrEquals(value).And(value.LessThanOrEquals(this.Max));
    public Boolean Contains(AlignedBox3D other) =>
        this.Min.LessThanOrEquals(other.Min).And(this.Max.GreaterThanOrEquals(other.Max));
    public Boolean Overlaps(AlignedBox3D y) =>
        this.Clamp(y).IsEmpty.Not;
    public Tuple2<AlignedBox3D, AlignedBox3D> SplitAt(Number t) =>
        this.Left(t).Tuple(this.Right(t));
    public Tuple2<AlignedBox3D, AlignedBox3D> Split =>
        this.SplitAt(((Number)0.5));
    public AlignedBox3D Left(Number t) =>
        this.Min.Tuple(this.Lerp(t));
    public AlignedBox3D Right(Number t) =>
        this.Lerp(t).Tuple(this.Max);
    public AlignedBox3D MoveTo(Point3D v) =>
        v.Tuple(v.Add(this.Size));
    public AlignedBox3D LeftHalf =>
        this.Left(((Number)0.5));
    public AlignedBox3D RightHalf =>
        this.Right(((Number)0.5));
    public AlignedBox3D Recenter(Point3D c) =>
        c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public AlignedBox3D Clamp(AlignedBox3D y) =>
        this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Point3D Clamp(Point3D value) =>
        value.Lerp(this.Min, this.Max, value.Unlerp(this.Min, this.Max).Clamp(((Integer)0), ((Integer)1)));
    public Boolean Within(Point3D value) =>
        value.GreaterThanOrEquals(this.Min).And(value.LessThanOrEquals(this.Max));
}
public readonly partial struct Complex
{
    public TR Aggregate<TR>(TR r, Function2<TR, Number, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(Add);
    public Number SumSquares =>
        this.Square.Sum;
    public Number MagnitudeSquared =>
        this.SumSquares;
    public Number Magnitude =>
        this.MagnitudeSquared.SquareRoot;
    public Number Dot(Complex v2) =>
        this.Multiply(v2).Sum;
    public Complex Normal =>
        this.Divide(this.Magnitude);
    public Number Average =>
        this.Sum.Divide(this.Count);
    public Complex Square =>
        this.Multiply(this);
    public Complex PlusOne =>
        this.Add(this.One);
    public Complex MinusOne =>
        this.Subtract(this.One);
    public Complex FromOne =>
        this.One.Subtract(this);
    public Boolean IsPositive =>
        this.GtEqZ;
    public Boolean GtZ =>
        this.GreaterThan(this.Zero);
    public Boolean LtZ =>
        this.LessThan(this.Zero);
    public Boolean GtEqZ =>
        this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ =>
        this.LessThanOrEquals(this.Zero);
    public Boolean IsNegative =>
        this.LessThan(this.Zero);
    public Complex Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Complex Abs =>
        this.LtZ ? this.Negative : this;
    public Complex Half =>
        this.Divide(((Number)2));
    public Complex Quarter =>
        this.Divide(((Number)4));
    public Complex Eighth =>
        this.Divide(((Number)8));
    public Complex Tenth =>
        this.Divide(((Number)10));
    public Complex Twice =>
        this.Multiply(((Number)2));
    public Complex Pow2 =>
        this.Multiply(this);
    public Complex MultiplyEpsilon(Complex y) =>
        this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(Complex y) =>
        this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public Complex Lerp(Complex b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Complex min, Complex max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Complex Clamp(Complex a, Complex b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Complex b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Complex a, Complex b) => a.Equals(b);
    public Boolean NotEquals(Complex b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Complex a, Complex b) => a.NotEquals(b);
    public Boolean LessThan(Complex b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Complex a, Complex b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Complex b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Complex a, Complex b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Complex b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Complex a, Complex b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Complex b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Complex a, Complex b) => a.GreaterThanOrEquals(b);
    public Complex Lesser(Complex b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Complex Greater(Complex b) =>
        this.GreaterThanOrEquals(b) ? this : b;
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
    public Boolean Between(Point2D min, Point2D max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Point2D Clamp(Point2D a, Point2D b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Point2D b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Point2D a, Point2D b) => a.Equals(b);
    public Boolean NotEquals(Point2D b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Point2D a, Point2D b) => a.NotEquals(b);
    public Boolean LessThan(Point2D b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Point2D a, Point2D b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Point2D b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Point2D a, Point2D b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Point2D b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Point2D a, Point2D b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Point2D b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Point2D a, Point2D b) => a.GreaterThanOrEquals(b);
    public Point2D Lesser(Point2D b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Point2D Greater(Point2D b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Point3D
{
    public Boolean Between(Point3D min, Point3D max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Point3D Clamp(Point3D a, Point3D b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Point3D b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Point3D a, Point3D b) => a.Equals(b);
    public Boolean NotEquals(Point3D b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Point3D a, Point3D b) => a.NotEquals(b);
    public Boolean LessThan(Point3D b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Point3D a, Point3D b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Point3D b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Point3D a, Point3D b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Point3D b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Point3D a, Point3D b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Point3D b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Point3D a, Point3D b) => a.GreaterThanOrEquals(b);
    public Point3D Lesser(Point3D b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Point3D Greater(Point3D b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Point4D
{
    public Boolean Between(Point4D min, Point4D max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Point4D Clamp(Point4D a, Point4D b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Point4D b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Point4D a, Point4D b) => a.Equals(b);
    public Boolean NotEquals(Point4D b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Point4D a, Point4D b) => a.NotEquals(b);
    public Boolean LessThan(Point4D b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Point4D a, Point4D b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Point4D b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Point4D a, Point4D b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Point4D b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Point4D a, Point4D b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Point4D b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Point4D a, Point4D b) => a.GreaterThanOrEquals(b);
    public Point4D Lesser(Point4D b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Point4D Greater(Point4D b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Line2D
{
    public Boolean IsEmpty =>
        this.Min.GreaterThanOrEquals(this.Max);
    public Point2D Lerp(Number amount) =>
        this.Min.Lerp(this.Max, amount);
    public Number Unlerp(Point2D value) =>
        value.Subtract(this.Min).Divide(this.Size);
    public Line2D Negate =>
        this.Max.Negative.Tuple(this.Min.Negative);
    public Line2D Reverse =>
        this.Max.Tuple(this.Min);
    public Point2D Center =>
        this.Lerp(((Number)0.5));
    public Boolean Contains(Point2D value) =>
        this.Min.LessThanOrEquals(value).And(value.LessThanOrEquals(this.Max));
    public Boolean Contains(Line2D other) =>
        this.Min.LessThanOrEquals(other.Min).And(this.Max.GreaterThanOrEquals(other.Max));
    public Boolean Overlaps(Line2D y) =>
        this.Clamp(y).IsEmpty.Not;
    public Tuple2<Line2D, Line2D> SplitAt(Number t) =>
        this.Left(t).Tuple(this.Right(t));
    public Tuple2<Line2D, Line2D> Split =>
        this.SplitAt(((Number)0.5));
    public Line2D Left(Number t) =>
        this.Min.Tuple(this.Lerp(t));
    public Line2D Right(Number t) =>
        this.Lerp(t).Tuple(this.Max);
    public Line2D MoveTo(Point2D v) =>
        v.Tuple(v.Add(this.Size));
    public Line2D LeftHalf =>
        this.Left(((Number)0.5));
    public Line2D RightHalf =>
        this.Right(((Number)0.5));
    public Line2D Recenter(Point2D c) =>
        c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public Line2D Clamp(Line2D y) =>
        this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Point2D Clamp(Point2D value) =>
        value.Lerp(this.Min, this.Max, value.Unlerp(this.Min, this.Max).Clamp(((Integer)0), ((Integer)1)));
    public Boolean Within(Point2D value) =>
        value.GreaterThanOrEquals(this.Min).And(value.LessThanOrEquals(this.Max));
}
public readonly partial struct Line3D
{
    public Boolean IsEmpty =>
        this.Min.GreaterThanOrEquals(this.Max);
    public Point3D Lerp(Number amount) =>
        this.Min.Lerp(this.Max, amount);
    public Number Unlerp(Point3D value) =>
        value.Subtract(this.Min).Divide(this.Size);
    public Line3D Negate =>
        this.Max.Negative.Tuple(this.Min.Negative);
    public Line3D Reverse =>
        this.Max.Tuple(this.Min);
    public Point3D Center =>
        this.Lerp(((Number)0.5));
    public Boolean Contains(Point3D value) =>
        this.Min.LessThanOrEquals(value).And(value.LessThanOrEquals(this.Max));
    public Boolean Contains(Line3D other) =>
        this.Min.LessThanOrEquals(other.Min).And(this.Max.GreaterThanOrEquals(other.Max));
    public Boolean Overlaps(Line3D y) =>
        this.Clamp(y).IsEmpty.Not;
    public Tuple2<Line3D, Line3D> SplitAt(Number t) =>
        this.Left(t).Tuple(this.Right(t));
    public Tuple2<Line3D, Line3D> Split =>
        this.SplitAt(((Number)0.5));
    public Line3D Left(Number t) =>
        this.Min.Tuple(this.Lerp(t));
    public Line3D Right(Number t) =>
        this.Lerp(t).Tuple(this.Max);
    public Line3D MoveTo(Point3D v) =>
        v.Tuple(v.Add(this.Size));
    public Line3D LeftHalf =>
        this.Left(((Number)0.5));
    public Line3D RightHalf =>
        this.Right(((Number)0.5));
    public Line3D Recenter(Point3D c) =>
        c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public Line3D Clamp(Line3D y) =>
        this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Point3D Clamp(Point3D value) =>
        value.Lerp(this.Min, this.Max, value.Unlerp(this.Min, this.Max).Clamp(((Integer)0), ((Integer)1)));
    public Boolean Within(Point3D value) =>
        value.GreaterThanOrEquals(this.Min).And(value.LessThanOrEquals(this.Max));
}
public readonly partial struct Color
{
    public Boolean Between(Color min, Color max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Color Clamp(Color a, Color b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Color b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Color a, Color b) => a.Equals(b);
    public Boolean NotEquals(Color b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Color a, Color b) => a.NotEquals(b);
    public Boolean LessThan(Color b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Color a, Color b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Color b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Color a, Color b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Color b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Color a, Color b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Color b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Color a, Color b) => a.GreaterThanOrEquals(b);
    public Color Lesser(Color b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Color Greater(Color b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct ColorLUV
{
    public Boolean Between(ColorLUV min, ColorLUV max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ColorLUV Clamp(ColorLUV a, ColorLUV b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(ColorLUV b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ColorLUV a, ColorLUV b) => a.Equals(b);
    public Boolean NotEquals(ColorLUV b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ColorLUV a, ColorLUV b) => a.NotEquals(b);
    public Boolean LessThan(ColorLUV b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ColorLUV a, ColorLUV b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ColorLUV b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ColorLUV a, ColorLUV b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ColorLUV b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ColorLUV a, ColorLUV b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ColorLUV b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ColorLUV a, ColorLUV b) => a.GreaterThanOrEquals(b);
    public ColorLUV Lesser(ColorLUV b) =>
        this.LessThanOrEquals(b) ? this : b;
    public ColorLUV Greater(ColorLUV b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct ColorLAB
{
    public Boolean Between(ColorLAB min, ColorLAB max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ColorLAB Clamp(ColorLAB a, ColorLAB b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(ColorLAB b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ColorLAB a, ColorLAB b) => a.Equals(b);
    public Boolean NotEquals(ColorLAB b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ColorLAB a, ColorLAB b) => a.NotEquals(b);
    public Boolean LessThan(ColorLAB b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ColorLAB a, ColorLAB b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ColorLAB b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ColorLAB a, ColorLAB b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ColorLAB b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ColorLAB a, ColorLAB b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ColorLAB b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ColorLAB a, ColorLAB b) => a.GreaterThanOrEquals(b);
    public ColorLAB Lesser(ColorLAB b) =>
        this.LessThanOrEquals(b) ? this : b;
    public ColorLAB Greater(ColorLAB b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct ColorLCh
{
    public Boolean Between(ColorLCh min, ColorLCh max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ColorLCh Clamp(ColorLCh a, ColorLCh b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(ColorLCh b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ColorLCh a, ColorLCh b) => a.Equals(b);
    public Boolean NotEquals(ColorLCh b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ColorLCh a, ColorLCh b) => a.NotEquals(b);
    public Boolean LessThan(ColorLCh b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ColorLCh a, ColorLCh b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ColorLCh b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ColorLCh a, ColorLCh b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ColorLCh b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ColorLCh a, ColorLCh b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ColorLCh b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ColorLCh a, ColorLCh b) => a.GreaterThanOrEquals(b);
    public ColorLCh Lesser(ColorLCh b) =>
        this.LessThanOrEquals(b) ? this : b;
    public ColorLCh Greater(ColorLCh b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct ColorHSV
{
    public Boolean Between(ColorHSV min, ColorHSV max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ColorHSV Clamp(ColorHSV a, ColorHSV b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(ColorHSV b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ColorHSV a, ColorHSV b) => a.Equals(b);
    public Boolean NotEquals(ColorHSV b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ColorHSV a, ColorHSV b) => a.NotEquals(b);
    public Boolean LessThan(ColorHSV b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ColorHSV a, ColorHSV b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ColorHSV b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ColorHSV a, ColorHSV b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ColorHSV b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ColorHSV a, ColorHSV b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ColorHSV b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ColorHSV a, ColorHSV b) => a.GreaterThanOrEquals(b);
    public ColorHSV Lesser(ColorHSV b) =>
        this.LessThanOrEquals(b) ? this : b;
    public ColorHSV Greater(ColorHSV b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct ColorHSL
{
    public Boolean Between(ColorHSL min, ColorHSL max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ColorHSL Clamp(ColorHSL a, ColorHSL b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(ColorHSL b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ColorHSL a, ColorHSL b) => a.Equals(b);
    public Boolean NotEquals(ColorHSL b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ColorHSL a, ColorHSL b) => a.NotEquals(b);
    public Boolean LessThan(ColorHSL b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ColorHSL a, ColorHSL b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ColorHSL b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ColorHSL a, ColorHSL b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ColorHSL b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ColorHSL a, ColorHSL b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ColorHSL b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ColorHSL a, ColorHSL b) => a.GreaterThanOrEquals(b);
    public ColorHSL Lesser(ColorHSL b) =>
        this.LessThanOrEquals(b) ? this : b;
    public ColorHSL Greater(ColorHSL b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct ColorYCbCr
{
    public Boolean Between(ColorYCbCr min, ColorYCbCr max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ColorYCbCr Clamp(ColorYCbCr a, ColorYCbCr b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(ColorYCbCr b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ColorYCbCr a, ColorYCbCr b) => a.Equals(b);
    public Boolean NotEquals(ColorYCbCr b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ColorYCbCr a, ColorYCbCr b) => a.NotEquals(b);
    public Boolean LessThan(ColorYCbCr b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ColorYCbCr a, ColorYCbCr b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ColorYCbCr b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ColorYCbCr a, ColorYCbCr b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ColorYCbCr b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ColorYCbCr a, ColorYCbCr b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ColorYCbCr b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ColorYCbCr a, ColorYCbCr b) => a.GreaterThanOrEquals(b);
    public ColorYCbCr Lesser(ColorYCbCr b) =>
        this.LessThanOrEquals(b) ? this : b;
    public ColorYCbCr Greater(ColorYCbCr b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct SphericalCoordinate
{
    public Boolean Between(SphericalCoordinate min, SphericalCoordinate max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public SphericalCoordinate Clamp(SphericalCoordinate a, SphericalCoordinate b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(SphericalCoordinate b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(SphericalCoordinate a, SphericalCoordinate b) => a.Equals(b);
    public Boolean NotEquals(SphericalCoordinate b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(SphericalCoordinate a, SphericalCoordinate b) => a.NotEquals(b);
    public Boolean LessThan(SphericalCoordinate b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(SphericalCoordinate a, SphericalCoordinate b) => a.LessThan(b);
    public Boolean LessThanOrEquals(SphericalCoordinate b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(SphericalCoordinate a, SphericalCoordinate b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(SphericalCoordinate b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(SphericalCoordinate a, SphericalCoordinate b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(SphericalCoordinate b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(SphericalCoordinate a, SphericalCoordinate b) => a.GreaterThanOrEquals(b);
    public SphericalCoordinate Lesser(SphericalCoordinate b) =>
        this.LessThanOrEquals(b) ? this : b;
    public SphericalCoordinate Greater(SphericalCoordinate b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct PolarCoordinate
{
    public Boolean Between(PolarCoordinate min, PolarCoordinate max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public PolarCoordinate Clamp(PolarCoordinate a, PolarCoordinate b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(PolarCoordinate b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(PolarCoordinate a, PolarCoordinate b) => a.Equals(b);
    public Boolean NotEquals(PolarCoordinate b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(PolarCoordinate a, PolarCoordinate b) => a.NotEquals(b);
    public Boolean LessThan(PolarCoordinate b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(PolarCoordinate a, PolarCoordinate b) => a.LessThan(b);
    public Boolean LessThanOrEquals(PolarCoordinate b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(PolarCoordinate a, PolarCoordinate b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(PolarCoordinate b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(PolarCoordinate a, PolarCoordinate b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(PolarCoordinate b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(PolarCoordinate a, PolarCoordinate b) => a.GreaterThanOrEquals(b);
    public PolarCoordinate Lesser(PolarCoordinate b) =>
        this.LessThanOrEquals(b) ? this : b;
    public PolarCoordinate Greater(PolarCoordinate b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct LogPolarCoordinate
{
    public Boolean Between(LogPolarCoordinate min, LogPolarCoordinate max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public LogPolarCoordinate Clamp(LogPolarCoordinate a, LogPolarCoordinate b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(LogPolarCoordinate b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(LogPolarCoordinate a, LogPolarCoordinate b) => a.Equals(b);
    public Boolean NotEquals(LogPolarCoordinate b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(LogPolarCoordinate a, LogPolarCoordinate b) => a.NotEquals(b);
    public Boolean LessThan(LogPolarCoordinate b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(LogPolarCoordinate a, LogPolarCoordinate b) => a.LessThan(b);
    public Boolean LessThanOrEquals(LogPolarCoordinate b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(LogPolarCoordinate a, LogPolarCoordinate b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(LogPolarCoordinate b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(LogPolarCoordinate a, LogPolarCoordinate b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(LogPolarCoordinate b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(LogPolarCoordinate a, LogPolarCoordinate b) => a.GreaterThanOrEquals(b);
    public LogPolarCoordinate Lesser(LogPolarCoordinate b) =>
        this.LessThanOrEquals(b) ? this : b;
    public LogPolarCoordinate Greater(LogPolarCoordinate b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct CylindricalCoordinate
{
    public Boolean Between(CylindricalCoordinate min, CylindricalCoordinate max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public CylindricalCoordinate Clamp(CylindricalCoordinate a, CylindricalCoordinate b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(CylindricalCoordinate b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(CylindricalCoordinate a, CylindricalCoordinate b) => a.Equals(b);
    public Boolean NotEquals(CylindricalCoordinate b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(CylindricalCoordinate a, CylindricalCoordinate b) => a.NotEquals(b);
    public Boolean LessThan(CylindricalCoordinate b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(CylindricalCoordinate a, CylindricalCoordinate b) => a.LessThan(b);
    public Boolean LessThanOrEquals(CylindricalCoordinate b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(CylindricalCoordinate a, CylindricalCoordinate b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(CylindricalCoordinate b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(CylindricalCoordinate a, CylindricalCoordinate b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(CylindricalCoordinate b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(CylindricalCoordinate a, CylindricalCoordinate b) => a.GreaterThanOrEquals(b);
    public CylindricalCoordinate Lesser(CylindricalCoordinate b) =>
        this.LessThanOrEquals(b) ? this : b;
    public CylindricalCoordinate Greater(CylindricalCoordinate b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct HorizontalCoordinate
{
    public Boolean Between(HorizontalCoordinate min, HorizontalCoordinate max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public HorizontalCoordinate Clamp(HorizontalCoordinate a, HorizontalCoordinate b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(HorizontalCoordinate b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(HorizontalCoordinate a, HorizontalCoordinate b) => a.Equals(b);
    public Boolean NotEquals(HorizontalCoordinate b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(HorizontalCoordinate a, HorizontalCoordinate b) => a.NotEquals(b);
    public Boolean LessThan(HorizontalCoordinate b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(HorizontalCoordinate a, HorizontalCoordinate b) => a.LessThan(b);
    public Boolean LessThanOrEquals(HorizontalCoordinate b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(HorizontalCoordinate a, HorizontalCoordinate b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(HorizontalCoordinate b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(HorizontalCoordinate a, HorizontalCoordinate b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(HorizontalCoordinate b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(HorizontalCoordinate a, HorizontalCoordinate b) => a.GreaterThanOrEquals(b);
    public HorizontalCoordinate Lesser(HorizontalCoordinate b) =>
        this.LessThanOrEquals(b) ? this : b;
    public HorizontalCoordinate Greater(HorizontalCoordinate b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct GeoCoordinate
{
    public Boolean Between(GeoCoordinate min, GeoCoordinate max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public GeoCoordinate Clamp(GeoCoordinate a, GeoCoordinate b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(GeoCoordinate b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(GeoCoordinate a, GeoCoordinate b) => a.Equals(b);
    public Boolean NotEquals(GeoCoordinate b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(GeoCoordinate a, GeoCoordinate b) => a.NotEquals(b);
    public Boolean LessThan(GeoCoordinate b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(GeoCoordinate a, GeoCoordinate b) => a.LessThan(b);
    public Boolean LessThanOrEquals(GeoCoordinate b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(GeoCoordinate a, GeoCoordinate b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(GeoCoordinate b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(GeoCoordinate a, GeoCoordinate b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(GeoCoordinate b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(GeoCoordinate a, GeoCoordinate b) => a.GreaterThanOrEquals(b);
    public GeoCoordinate Lesser(GeoCoordinate b) =>
        this.LessThanOrEquals(b) ? this : b;
    public GeoCoordinate Greater(GeoCoordinate b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct GeoCoordinateWithAltitude
{
    public Boolean Between(GeoCoordinateWithAltitude min, GeoCoordinateWithAltitude max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public GeoCoordinateWithAltitude Clamp(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(GeoCoordinateWithAltitude b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => a.Equals(b);
    public Boolean NotEquals(GeoCoordinateWithAltitude b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => a.NotEquals(b);
    public Boolean LessThan(GeoCoordinateWithAltitude b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => a.LessThan(b);
    public Boolean LessThanOrEquals(GeoCoordinateWithAltitude b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(GeoCoordinateWithAltitude b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(GeoCoordinateWithAltitude b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(GeoCoordinateWithAltitude a, GeoCoordinateWithAltitude b) => a.GreaterThanOrEquals(b);
    public GeoCoordinateWithAltitude Lesser(GeoCoordinateWithAltitude b) =>
        this.LessThanOrEquals(b) ? this : b;
    public GeoCoordinateWithAltitude Greater(GeoCoordinateWithAltitude b) =>
        this.GreaterThanOrEquals(b) ? this : b;
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
public readonly partial struct Proportion
{
    public Proportion Square =>
        this.Multiply(this);
    public Proportion PlusOne =>
        this.Add(this.One);
    public Proportion MinusOne =>
        this.Subtract(this.One);
    public Proportion FromOne =>
        this.One.Subtract(this);
    public Boolean IsPositive =>
        this.GtEqZ;
    public Boolean GtZ =>
        this.GreaterThan(this.Zero);
    public Boolean LtZ =>
        this.LessThan(this.Zero);
    public Boolean GtEqZ =>
        this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ =>
        this.LessThanOrEquals(this.Zero);
    public Boolean IsNegative =>
        this.LessThan(this.Zero);
    public Proportion Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Proportion Abs =>
        this.LtZ ? this.Negative : this;
    public Proportion Half =>
        this.Divide(((Number)2));
    public Proportion Quarter =>
        this.Divide(((Number)4));
    public Proportion Eighth =>
        this.Divide(((Number)8));
    public Proportion Tenth =>
        this.Divide(((Number)10));
    public Proportion Twice =>
        this.Multiply(((Number)2));
    public Proportion Pow2 =>
        this.Multiply(this);
    public Proportion MultiplyEpsilon(Proportion y) =>
        this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(Proportion y) =>
        this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public Proportion Lerp(Proportion b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Proportion min, Proportion max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Proportion Clamp(Proportion a, Proportion b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Proportion b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Proportion a, Proportion b) => a.Equals(b);
    public Boolean NotEquals(Proportion b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Proportion a, Proportion b) => a.NotEquals(b);
    public Boolean LessThan(Proportion b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Proportion a, Proportion b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Proportion b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Proportion a, Proportion b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Proportion b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Proportion a, Proportion b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Proportion b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Proportion a, Proportion b) => a.GreaterThanOrEquals(b);
    public Proportion Lesser(Proportion b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Proportion Greater(Proportion b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Fraction
{
}
public readonly partial struct Angle
{
    public Number Cos => throw new NotImplementedException();
    public Number Sin => throw new NotImplementedException();
    public Number Tan => throw new NotImplementedException();
    public Number Degrees =>
        this.Turns.Multiply(((Integer)360));
    public Number Turns =>
        this.Divide(Constants.TwoPi);
    public Boolean Between(Angle min, Angle max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Angle Clamp(Angle a, Angle b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Angle b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Angle a, Angle b) => a.Equals(b);
    public Boolean NotEquals(Angle b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Angle a, Angle b) => a.NotEquals(b);
    public Boolean LessThan(Angle b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Angle a, Angle b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Angle b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Angle a, Angle b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Angle b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Angle a, Angle b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Angle b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Angle a, Angle b) => a.GreaterThanOrEquals(b);
    public Angle Lesser(Angle b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Angle Greater(Angle b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Length
{
    public Area Multiply(Length y) =>
        this.Value.Multiply(y.Value);
    public static Area operator *(Length x, Length y) => x.Multiply(y);
    public Volume Multiply(Area y) =>
        this.Value.Multiply(y.Value);
    public static Volume operator *(Length x, Area y) => x.Multiply(y);
    public Velocity Multiply(Time y) =>
        this.Value.Multiply(y.Value);
    public static Velocity operator *(Length x, Time y) => x.Multiply(y);
    public Boolean Between(Length min, Length max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Length Clamp(Length a, Length b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Length b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Length a, Length b) => a.Equals(b);
    public Boolean NotEquals(Length b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Length a, Length b) => a.NotEquals(b);
    public Boolean LessThan(Length b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Length a, Length b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Length b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Length a, Length b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Length b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Length a, Length b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Length b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Length a, Length b) => a.GreaterThanOrEquals(b);
    public Length Lesser(Length b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Length Greater(Length b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Mass
{
    public Force Multiply(Acceleration y) =>
        this.Value.Multiply(y.Value);
    public static Force operator *(Mass x, Acceleration y) => x.Multiply(y);
    public Boolean Between(Mass min, Mass max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Mass Clamp(Mass a, Mass b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Mass b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Mass a, Mass b) => a.Equals(b);
    public Boolean NotEquals(Mass b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Mass a, Mass b) => a.NotEquals(b);
    public Boolean LessThan(Mass b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Mass a, Mass b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Mass b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Mass a, Mass b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Mass b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Mass a, Mass b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Mass b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Mass a, Mass b) => a.GreaterThanOrEquals(b);
    public Mass Lesser(Mass b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Mass Greater(Mass b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Temperature
{
    public Number Fahrenheit =>
        this.Celsius.Multiply(((Number)9).Divide(((Number)5))).Add(((Number)32));
    public Number Kelvin =>
        this.Celsius.Add(((Number)273.15));
    public Boolean Between(Temperature min, Temperature max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Temperature Clamp(Temperature a, Temperature b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Temperature b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Temperature a, Temperature b) => a.Equals(b);
    public Boolean NotEquals(Temperature b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Temperature a, Temperature b) => a.NotEquals(b);
    public Boolean LessThan(Temperature b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Temperature a, Temperature b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Temperature b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Temperature a, Temperature b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Temperature b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Temperature a, Temperature b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Temperature b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Temperature a, Temperature b) => a.GreaterThanOrEquals(b);
    public Temperature Lesser(Temperature b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Temperature Greater(Temperature b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Time
{
    public Velocity Multiply(Length y) =>
        this.Value.Multiply(y.Value);
    public static Velocity operator *(Time x, Length y) => x.Multiply(y);
    public Boolean Between(Time min, Time max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Time Clamp(Time a, Time b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Time b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Time a, Time b) => a.Equals(b);
    public Boolean NotEquals(Time b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Time a, Time b) => a.NotEquals(b);
    public Boolean LessThan(Time b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Time a, Time b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Time b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Time a, Time b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Time b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Time a, Time b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Time b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Time a, Time b) => a.GreaterThanOrEquals(b);
    public Time Lesser(Time b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Time Greater(Time b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct TimeRange
{
    public Boolean IsEmpty =>
        this.Min.GreaterThanOrEquals(this.Max);
    public DateTime Lerp(Number amount) =>
        this.Min.Lerp(this.Max, amount);
    public Number Unlerp(DateTime value) =>
        value.Subtract(this.Min).Divide(this.Size);
    public TimeRange Negate =>
        this.Max.Negative.Tuple(this.Min.Negative);
    public TimeRange Reverse =>
        this.Max.Tuple(this.Min);
    public DateTime Center =>
        this.Lerp(((Number)0.5));
    public Boolean Contains(DateTime value) =>
        this.Min.LessThanOrEquals(value).And(value.LessThanOrEquals(this.Max));
    public Boolean Contains(TimeRange other) =>
        this.Min.LessThanOrEquals(other.Min).And(this.Max.GreaterThanOrEquals(other.Max));
    public Boolean Overlaps(TimeRange y) =>
        this.Clamp(y).IsEmpty.Not;
    public Tuple2<TimeRange, TimeRange> SplitAt(Number t) =>
        this.Left(t).Tuple(this.Right(t));
    public Tuple2<TimeRange, TimeRange> Split =>
        this.SplitAt(((Number)0.5));
    public TimeRange Left(Number t) =>
        this.Min.Tuple(this.Lerp(t));
    public TimeRange Right(Number t) =>
        this.Lerp(t).Tuple(this.Max);
    public TimeRange MoveTo(DateTime v) =>
        v.Tuple(v.Add(this.Size));
    public TimeRange LeftHalf =>
        this.Left(((Number)0.5));
    public TimeRange RightHalf =>
        this.Right(((Number)0.5));
    public TimeRange Recenter(DateTime c) =>
        c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public TimeRange Clamp(TimeRange y) =>
        this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public DateTime Clamp(DateTime value) =>
        value.Lerp(this.Min, this.Max, value.Unlerp(this.Min, this.Max).Clamp(((Integer)0), ((Integer)1)));
    public Boolean Within(DateTime value) =>
        value.GreaterThanOrEquals(this.Min).And(value.LessThanOrEquals(this.Max));
}
public readonly partial struct DateTime
{
    public Boolean Between(DateTime min, DateTime max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public DateTime Clamp(DateTime a, DateTime b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(DateTime b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(DateTime a, DateTime b) => a.Equals(b);
    public Boolean NotEquals(DateTime b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(DateTime a, DateTime b) => a.NotEquals(b);
    public Boolean LessThan(DateTime b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(DateTime a, DateTime b) => a.LessThan(b);
    public Boolean LessThanOrEquals(DateTime b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(DateTime a, DateTime b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(DateTime b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(DateTime a, DateTime b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(DateTime b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(DateTime a, DateTime b) => a.GreaterThanOrEquals(b);
    public DateTime Lesser(DateTime b) =>
        this.LessThanOrEquals(b) ? this : b;
    public DateTime Greater(DateTime b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct AnglePair
{
    public Boolean IsEmpty =>
        this.Min.GreaterThanOrEquals(this.Max);
    public Angle Lerp(Number amount) =>
        this.Min.Lerp(this.Max, amount);
    public Number Unlerp(Angle value) =>
        value.Subtract(this.Min).Divide(this.Size);
    public AnglePair Negate =>
        this.Max.Negative.Tuple(this.Min.Negative);
    public AnglePair Reverse =>
        this.Max.Tuple(this.Min);
    public Angle Center =>
        this.Lerp(((Number)0.5));
    public Boolean Contains(Angle value) =>
        this.Min.LessThanOrEquals(value).And(value.LessThanOrEquals(this.Max));
    public Boolean Contains(AnglePair other) =>
        this.Min.LessThanOrEquals(other.Min).And(this.Max.GreaterThanOrEquals(other.Max));
    public Boolean Overlaps(AnglePair y) =>
        this.Clamp(y).IsEmpty.Not;
    public Tuple2<AnglePair, AnglePair> SplitAt(Number t) =>
        this.Left(t).Tuple(this.Right(t));
    public Tuple2<AnglePair, AnglePair> Split =>
        this.SplitAt(((Number)0.5));
    public AnglePair Left(Number t) =>
        this.Min.Tuple(this.Lerp(t));
    public AnglePair Right(Number t) =>
        this.Lerp(t).Tuple(this.Max);
    public AnglePair MoveTo(Angle v) =>
        v.Tuple(v.Add(this.Size));
    public AnglePair LeftHalf =>
        this.Left(((Number)0.5));
    public AnglePair RightHalf =>
        this.Right(((Number)0.5));
    public AnglePair Recenter(Angle c) =>
        c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public AnglePair Clamp(AnglePair y) =>
        this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Angle Clamp(Angle value) =>
        value.Lerp(this.Min, this.Max, value.Unlerp(this.Min, this.Max).Clamp(((Integer)0), ((Integer)1)));
    public Boolean Within(Angle value) =>
        value.GreaterThanOrEquals(this.Min).And(value.LessThanOrEquals(this.Max));
}
public readonly partial struct Ring
{
}
public readonly partial struct Arc
{
}
public readonly partial struct RealInterval
{
    public Boolean IsEmpty =>
        this.Min.GreaterThanOrEquals(this.Max);
    public Number Lerp(Number amount) =>
        this.Min.Lerp(this.Max, amount);
    public Number Unlerp(Number value) =>
        value.Subtract(this.Min).Divide(this.Size);
    public RealInterval Negate =>
        this.Max.Negative.Tuple(this.Min.Negative);
    public RealInterval Reverse =>
        this.Max.Tuple(this.Min);
    public Number Center =>
        this.Lerp(((Number)0.5));
    public Boolean Contains(Number value) =>
        this.Min.LessThanOrEquals(value).And(value.LessThanOrEquals(this.Max));
    public Boolean Contains(RealInterval other) =>
        this.Min.LessThanOrEquals(other.Min).And(this.Max.GreaterThanOrEquals(other.Max));
    public Boolean Overlaps(RealInterval y) =>
        this.Clamp(y).IsEmpty.Not;
    public Tuple2<RealInterval, RealInterval> SplitAt(Number t) =>
        this.Left(t).Tuple(this.Right(t));
    public Tuple2<RealInterval, RealInterval> Split =>
        this.SplitAt(((Number)0.5));
    public RealInterval Left(Number t) =>
        this.Min.Tuple(this.Lerp(t));
    public RealInterval Right(Number t) =>
        this.Lerp(t).Tuple(this.Max);
    public RealInterval MoveTo(Number v) =>
        v.Tuple(v.Add(this.Size));
    public RealInterval LeftHalf =>
        this.Left(((Number)0.5));
    public RealInterval RightHalf =>
        this.Right(((Number)0.5));
    public RealInterval Recenter(Number c) =>
        c.Subtract(this.Size.Half).Tuple(c.Add(this.Size.Half));
    public RealInterval Clamp(RealInterval y) =>
        this.Clamp(y.Min).Tuple(this.Clamp(y.Max));
    public Number Clamp(Number value) =>
        value.Lerp(this.Min, this.Max, value.Unlerp(this.Min, this.Max).Clamp(((Integer)0), ((Integer)1)));
    public Boolean Within(Number value) =>
        value.GreaterThanOrEquals(this.Min).And(value.LessThanOrEquals(this.Max));
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
    public TR Aggregate<TR>(TR r, Function2<TR, Number, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(Add);
    public Number SumSquares =>
        this.Square.Sum;
    public Number MagnitudeSquared =>
        this.SumSquares;
    public Number Magnitude =>
        this.MagnitudeSquared.SquareRoot;
    public Number Dot(UV v2) =>
        this.Multiply(v2).Sum;
    public UV Normal =>
        this.Divide(this.Magnitude);
    public Number Average =>
        this.Sum.Divide(this.Count);
    public UV Square =>
        this.Multiply(this);
    public UV PlusOne =>
        this.Add(this.One);
    public UV MinusOne =>
        this.Subtract(this.One);
    public UV FromOne =>
        this.One.Subtract(this);
    public Boolean IsPositive =>
        this.GtEqZ;
    public Boolean GtZ =>
        this.GreaterThan(this.Zero);
    public Boolean LtZ =>
        this.LessThan(this.Zero);
    public Boolean GtEqZ =>
        this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ =>
        this.LessThanOrEquals(this.Zero);
    public Boolean IsNegative =>
        this.LessThan(this.Zero);
    public UV Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public UV Abs =>
        this.LtZ ? this.Negative : this;
    public UV Half =>
        this.Divide(((Number)2));
    public UV Quarter =>
        this.Divide(((Number)4));
    public UV Eighth =>
        this.Divide(((Number)8));
    public UV Tenth =>
        this.Divide(((Number)10));
    public UV Twice =>
        this.Multiply(((Number)2));
    public UV Pow2 =>
        this.Multiply(this);
    public UV MultiplyEpsilon(UV y) =>
        this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(UV y) =>
        this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public UV Lerp(UV b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(UV min, UV max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public UV Clamp(UV a, UV b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(UV b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(UV a, UV b) => a.Equals(b);
    public Boolean NotEquals(UV b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(UV a, UV b) => a.NotEquals(b);
    public Boolean LessThan(UV b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(UV a, UV b) => a.LessThan(b);
    public Boolean LessThanOrEquals(UV b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(UV a, UV b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(UV b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(UV a, UV b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(UV b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(UV a, UV b) => a.GreaterThanOrEquals(b);
    public UV Lesser(UV b) =>
        this.LessThanOrEquals(b) ? this : b;
    public UV Greater(UV b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct UVW
{
    public TR Aggregate<TR>(TR r, Function2<TR, Number, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(Add);
    public Number SumSquares =>
        this.Square.Sum;
    public Number MagnitudeSquared =>
        this.SumSquares;
    public Number Magnitude =>
        this.MagnitudeSquared.SquareRoot;
    public Number Dot(UVW v2) =>
        this.Multiply(v2).Sum;
    public UVW Normal =>
        this.Divide(this.Magnitude);
    public Number Average =>
        this.Sum.Divide(this.Count);
    public UVW Square =>
        this.Multiply(this);
    public UVW PlusOne =>
        this.Add(this.One);
    public UVW MinusOne =>
        this.Subtract(this.One);
    public UVW FromOne =>
        this.One.Subtract(this);
    public Boolean IsPositive =>
        this.GtEqZ;
    public Boolean GtZ =>
        this.GreaterThan(this.Zero);
    public Boolean LtZ =>
        this.LessThan(this.Zero);
    public Boolean GtEqZ =>
        this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ =>
        this.LessThanOrEquals(this.Zero);
    public Boolean IsNegative =>
        this.LessThan(this.Zero);
    public UVW Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public UVW Abs =>
        this.LtZ ? this.Negative : this;
    public UVW Half =>
        this.Divide(((Number)2));
    public UVW Quarter =>
        this.Divide(((Number)4));
    public UVW Eighth =>
        this.Divide(((Number)8));
    public UVW Tenth =>
        this.Divide(((Number)10));
    public UVW Twice =>
        this.Multiply(((Number)2));
    public UVW Pow2 =>
        this.Multiply(this);
    public UVW MultiplyEpsilon(UVW y) =>
        this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(UVW y) =>
        this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public UVW Lerp(UVW b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(UVW min, UVW max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public UVW Clamp(UVW a, UVW b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(UVW b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(UVW a, UVW b) => a.Equals(b);
    public Boolean NotEquals(UVW b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(UVW a, UVW b) => a.NotEquals(b);
    public Boolean LessThan(UVW b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(UVW a, UVW b) => a.LessThan(b);
    public Boolean LessThanOrEquals(UVW b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(UVW a, UVW b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(UVW b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(UVW a, UVW b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(UVW b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(UVW a, UVW b) => a.GreaterThanOrEquals(b);
    public UVW Lesser(UVW b) =>
        this.LessThanOrEquals(b) ? this : b;
    public UVW Greater(UVW b) =>
        this.GreaterThanOrEquals(b) ? this : b;
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
    public Length Divide(Length y) =>
        this.Value.Divide(y.Value);
    public static Length operator /(Area x, Length y) => x.Divide(y);
    public Volume Multiply(Length y) =>
        this.Value.Multiply(y.Value);
    public static Volume operator *(Area x, Length y) => x.Multiply(y);
    public Boolean Between(Area min, Area max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Area Clamp(Area a, Area b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Area b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Area a, Area b) => a.Equals(b);
    public Boolean NotEquals(Area b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Area a, Area b) => a.NotEquals(b);
    public Boolean LessThan(Area b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Area a, Area b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Area b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Area a, Area b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Area b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Area a, Area b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Area b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Area a, Area b) => a.GreaterThanOrEquals(b);
    public Area Lesser(Area b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Area Greater(Area b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Volume
{
    public Area Divide(Length y) =>
        this.Value.Divide(y.Value);
    public static Area operator /(Volume x, Length y) => x.Divide(y);
    public Length Divide(Area y) =>
        this.Value.Divide(y.Value);
    public static Length operator /(Volume x, Area y) => x.Divide(y);
    public Boolean Between(Volume min, Volume max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Volume Clamp(Volume a, Volume b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Volume b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Volume a, Volume b) => a.Equals(b);
    public Boolean NotEquals(Volume b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Volume a, Volume b) => a.NotEquals(b);
    public Boolean LessThan(Volume b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Volume a, Volume b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Volume b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Volume a, Volume b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Volume b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Volume a, Volume b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Volume b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Volume a, Volume b) => a.GreaterThanOrEquals(b);
    public Volume Lesser(Volume b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Volume Greater(Volume b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Velocity
{
    public Length Divide(Time y) =>
        this.Value.Divide(y.Value);
    public static Length operator /(Velocity x, Time y) => x.Divide(y);
    public Time Divide(Length y) =>
        this.Value.Divide(y.Value);
    public static Time operator /(Velocity x, Length y) => x.Divide(y);
    public Acceleration Multiply(Time y) =>
        this.Value.Multiply(y.Value);
    public static Acceleration operator *(Velocity x, Time y) => x.Multiply(y);
    public Boolean Between(Velocity min, Velocity max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Velocity Clamp(Velocity a, Velocity b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Velocity b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Velocity a, Velocity b) => a.Equals(b);
    public Boolean NotEquals(Velocity b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Velocity a, Velocity b) => a.NotEquals(b);
    public Boolean LessThan(Velocity b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Velocity a, Velocity b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Velocity b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Velocity a, Velocity b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Velocity b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Velocity a, Velocity b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Velocity b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Velocity a, Velocity b) => a.GreaterThanOrEquals(b);
    public Velocity Lesser(Velocity b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Velocity Greater(Velocity b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Acceleration
{
    public Velocity Divide(Time y) =>
        this.Value.Divide(y.Value);
    public static Velocity operator /(Acceleration x, Time y) => x.Divide(y);
    public Time Divide(Velocity y) =>
        this.Value.Divide(y.Value);
    public static Time operator /(Acceleration x, Velocity y) => x.Divide(y);
    public Boolean Between(Acceleration min, Acceleration max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Acceleration Clamp(Acceleration a, Acceleration b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Acceleration b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Acceleration a, Acceleration b) => a.Equals(b);
    public Boolean NotEquals(Acceleration b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Acceleration a, Acceleration b) => a.NotEquals(b);
    public Boolean LessThan(Acceleration b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Acceleration a, Acceleration b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Acceleration b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Acceleration a, Acceleration b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Acceleration b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Acceleration a, Acceleration b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Acceleration b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Acceleration a, Acceleration b) => a.GreaterThanOrEquals(b);
    public Acceleration Lesser(Acceleration b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Acceleration Greater(Acceleration b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Force
{
    public Boolean Between(Force min, Force max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Force Clamp(Force a, Force b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Force b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Force a, Force b) => a.Equals(b);
    public Boolean NotEquals(Force b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Force a, Force b) => a.NotEquals(b);
    public Boolean LessThan(Force b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Force a, Force b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Force b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Force a, Force b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Force b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Force a, Force b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Force b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Force a, Force b) => a.GreaterThanOrEquals(b);
    public Force Lesser(Force b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Force Greater(Force b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Pressure
{
    public Boolean Between(Pressure min, Pressure max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Pressure Clamp(Pressure a, Pressure b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Pressure b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Pressure a, Pressure b) => a.Equals(b);
    public Boolean NotEquals(Pressure b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Pressure a, Pressure b) => a.NotEquals(b);
    public Boolean LessThan(Pressure b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Pressure a, Pressure b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Pressure b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Pressure a, Pressure b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Pressure b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Pressure a, Pressure b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Pressure b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Pressure a, Pressure b) => a.GreaterThanOrEquals(b);
    public Pressure Lesser(Pressure b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Pressure Greater(Pressure b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Energy
{
    public Boolean Between(Energy min, Energy max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Energy Clamp(Energy a, Energy b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Energy b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Energy a, Energy b) => a.Equals(b);
    public Boolean NotEquals(Energy b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Energy a, Energy b) => a.NotEquals(b);
    public Boolean LessThan(Energy b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Energy a, Energy b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Energy b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Energy a, Energy b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Energy b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Energy a, Energy b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Energy b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Energy a, Energy b) => a.GreaterThanOrEquals(b);
    public Energy Lesser(Energy b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Energy Greater(Energy b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Memory
{
    public Boolean Between(Memory min, Memory max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Memory Clamp(Memory a, Memory b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Memory b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Memory a, Memory b) => a.Equals(b);
    public Boolean NotEquals(Memory b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Memory a, Memory b) => a.NotEquals(b);
    public Boolean LessThan(Memory b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Memory a, Memory b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Memory b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Memory a, Memory b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Memory b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Memory a, Memory b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Memory b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Memory a, Memory b) => a.GreaterThanOrEquals(b);
    public Memory Lesser(Memory b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Memory Greater(Memory b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Frequency
{
    public Boolean Between(Frequency min, Frequency max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Frequency Clamp(Frequency a, Frequency b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Frequency b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Frequency a, Frequency b) => a.Equals(b);
    public Boolean NotEquals(Frequency b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Frequency a, Frequency b) => a.NotEquals(b);
    public Boolean LessThan(Frequency b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Frequency a, Frequency b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Frequency b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Frequency a, Frequency b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Frequency b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Frequency a, Frequency b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Frequency b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Frequency a, Frequency b) => a.GreaterThanOrEquals(b);
    public Frequency Lesser(Frequency b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Frequency Greater(Frequency b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Loudness
{
    public Boolean Between(Loudness min, Loudness max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Loudness Clamp(Loudness a, Loudness b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Loudness b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Loudness a, Loudness b) => a.Equals(b);
    public Boolean NotEquals(Loudness b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Loudness a, Loudness b) => a.NotEquals(b);
    public Boolean LessThan(Loudness b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Loudness a, Loudness b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Loudness b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Loudness a, Loudness b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Loudness b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Loudness a, Loudness b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Loudness b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Loudness a, Loudness b) => a.GreaterThanOrEquals(b);
    public Loudness Lesser(Loudness b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Loudness Greater(Loudness b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct LuminousIntensity
{
    public Boolean Between(LuminousIntensity min, LuminousIntensity max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public LuminousIntensity Clamp(LuminousIntensity a, LuminousIntensity b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(LuminousIntensity b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(LuminousIntensity a, LuminousIntensity b) => a.Equals(b);
    public Boolean NotEquals(LuminousIntensity b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(LuminousIntensity a, LuminousIntensity b) => a.NotEquals(b);
    public Boolean LessThan(LuminousIntensity b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(LuminousIntensity a, LuminousIntensity b) => a.LessThan(b);
    public Boolean LessThanOrEquals(LuminousIntensity b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(LuminousIntensity a, LuminousIntensity b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(LuminousIntensity b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(LuminousIntensity a, LuminousIntensity b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(LuminousIntensity b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(LuminousIntensity a, LuminousIntensity b) => a.GreaterThanOrEquals(b);
    public LuminousIntensity Lesser(LuminousIntensity b) =>
        this.LessThanOrEquals(b) ? this : b;
    public LuminousIntensity Greater(LuminousIntensity b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct ElectricPotential
{
    public Boolean Between(ElectricPotential min, ElectricPotential max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ElectricPotential Clamp(ElectricPotential a, ElectricPotential b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(ElectricPotential b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ElectricPotential a, ElectricPotential b) => a.Equals(b);
    public Boolean NotEquals(ElectricPotential b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ElectricPotential a, ElectricPotential b) => a.NotEquals(b);
    public Boolean LessThan(ElectricPotential b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ElectricPotential a, ElectricPotential b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricPotential b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ElectricPotential a, ElectricPotential b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricPotential b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ElectricPotential a, ElectricPotential b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricPotential b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ElectricPotential a, ElectricPotential b) => a.GreaterThanOrEquals(b);
    public ElectricPotential Lesser(ElectricPotential b) =>
        this.LessThanOrEquals(b) ? this : b;
    public ElectricPotential Greater(ElectricPotential b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct ElectricCharge
{
    public Boolean Between(ElectricCharge min, ElectricCharge max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ElectricCharge Clamp(ElectricCharge a, ElectricCharge b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(ElectricCharge b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ElectricCharge a, ElectricCharge b) => a.Equals(b);
    public Boolean NotEquals(ElectricCharge b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ElectricCharge a, ElectricCharge b) => a.NotEquals(b);
    public Boolean LessThan(ElectricCharge b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ElectricCharge a, ElectricCharge b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricCharge b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ElectricCharge a, ElectricCharge b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricCharge b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ElectricCharge a, ElectricCharge b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricCharge b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ElectricCharge a, ElectricCharge b) => a.GreaterThanOrEquals(b);
    public ElectricCharge Lesser(ElectricCharge b) =>
        this.LessThanOrEquals(b) ? this : b;
    public ElectricCharge Greater(ElectricCharge b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct ElectricCurrent
{
    public Boolean Between(ElectricCurrent min, ElectricCurrent max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ElectricCurrent Clamp(ElectricCurrent a, ElectricCurrent b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(ElectricCurrent b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ElectricCurrent a, ElectricCurrent b) => a.Equals(b);
    public Boolean NotEquals(ElectricCurrent b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ElectricCurrent a, ElectricCurrent b) => a.NotEquals(b);
    public Boolean LessThan(ElectricCurrent b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ElectricCurrent a, ElectricCurrent b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricCurrent b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ElectricCurrent a, ElectricCurrent b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricCurrent b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ElectricCurrent a, ElectricCurrent b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricCurrent b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ElectricCurrent a, ElectricCurrent b) => a.GreaterThanOrEquals(b);
    public ElectricCurrent Lesser(ElectricCurrent b) =>
        this.LessThanOrEquals(b) ? this : b;
    public ElectricCurrent Greater(ElectricCurrent b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct ElectricResistance
{
    public Boolean Between(ElectricResistance min, ElectricResistance max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public ElectricResistance Clamp(ElectricResistance a, ElectricResistance b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(ElectricResistance b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(ElectricResistance a, ElectricResistance b) => a.Equals(b);
    public Boolean NotEquals(ElectricResistance b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(ElectricResistance a, ElectricResistance b) => a.NotEquals(b);
    public Boolean LessThan(ElectricResistance b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(ElectricResistance a, ElectricResistance b) => a.LessThan(b);
    public Boolean LessThanOrEquals(ElectricResistance b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(ElectricResistance a, ElectricResistance b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(ElectricResistance b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(ElectricResistance a, ElectricResistance b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(ElectricResistance b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(ElectricResistance a, ElectricResistance b) => a.GreaterThanOrEquals(b);
    public ElectricResistance Lesser(ElectricResistance b) =>
        this.LessThanOrEquals(b) ? this : b;
    public ElectricResistance Greater(ElectricResistance b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Power
{
    public Boolean Between(Power min, Power max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Power Clamp(Power a, Power b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Power b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Power a, Power b) => a.Equals(b);
    public Boolean NotEquals(Power b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Power a, Power b) => a.NotEquals(b);
    public Boolean LessThan(Power b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Power a, Power b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Power b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Power a, Power b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Power b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Power a, Power b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Power b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Power a, Power b) => a.GreaterThanOrEquals(b);
    public Power Lesser(Power b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Power Greater(Power b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Density
{
    public Boolean Between(Density min, Density max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Density Clamp(Density a, Density b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Density b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Density a, Density b) => a.Equals(b);
    public Boolean NotEquals(Density b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Density a, Density b) => a.NotEquals(b);
    public Boolean LessThan(Density b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Density a, Density b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Density b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Density a, Density b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Density b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Density a, Density b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Density b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Density a, Density b) => a.GreaterThanOrEquals(b);
    public Density Lesser(Density b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Density Greater(Density b) =>
        this.GreaterThanOrEquals(b) ? this : b;
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
public readonly partial struct Probability
{
    public Probability Square =>
        this.Multiply(this);
    public Probability PlusOne =>
        this.Add(this.One);
    public Probability MinusOne =>
        this.Subtract(this.One);
    public Probability FromOne =>
        this.One.Subtract(this);
    public Boolean IsPositive =>
        this.GtEqZ;
    public Boolean GtZ =>
        this.GreaterThan(this.Zero);
    public Boolean LtZ =>
        this.LessThan(this.Zero);
    public Boolean GtEqZ =>
        this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ =>
        this.LessThanOrEquals(this.Zero);
    public Boolean IsNegative =>
        this.LessThan(this.Zero);
    public Probability Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Probability Abs =>
        this.LtZ ? this.Negative : this;
    public Probability Half =>
        this.Divide(((Number)2));
    public Probability Quarter =>
        this.Divide(((Number)4));
    public Probability Eighth =>
        this.Divide(((Number)8));
    public Probability Tenth =>
        this.Divide(((Number)10));
    public Probability Twice =>
        this.Multiply(((Number)2));
    public Probability Pow2 =>
        this.Multiply(this);
    public Probability MultiplyEpsilon(Probability y) =>
        this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(Probability y) =>
        this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public Probability Lerp(Probability b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Probability min, Probability max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Probability Clamp(Probability a, Probability b) =>
        this.Greater(a).Lesser(b);
    public Boolean Equals(Probability b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Probability a, Probability b) => a.Equals(b);
    public Boolean NotEquals(Probability b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Probability a, Probability b) => a.NotEquals(b);
    public Boolean LessThan(Probability b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Probability a, Probability b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Probability b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Probability a, Probability b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Probability b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Probability a, Probability b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Probability b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Probability a, Probability b) => a.GreaterThanOrEquals(b);
    public Probability Lesser(Probability b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Probability Greater(Probability b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct BinomialDistribution
{
}
public readonly partial struct Array1<T>
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
