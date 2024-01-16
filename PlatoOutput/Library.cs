using System;
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
    public Number ClampOne =>
        this.Clamp(((Integer)0).Tuple(((Integer)1)));
    public Boolean AlmostZero =>
        this.Abs.LessThan(Epsilon);
    public Angle Radians =>
        this;
    public Angle Degrees =>
        this.Divide(((Integer)360)).Turns;
    public Angle Turns =>
        this.Multiply(TwoPi);
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
        this.Divide(((Integer)2));
    public Number Quarter =>
        this.Divide(((Integer)4));
    public Number Eighth =>
        this.Divide(((Integer)8));
    public Number Tenth =>
        this.Divide(((Integer)10));
    public Number Twice =>
        this.Multiply(((Integer)2));
    public Number SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public Number Pow2 =>
        this.Multiply(this);
    public Number Lerp(Number b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Number min, Number max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public Integer Square =>
        this.Multiply(this);
    public Integer PlusOne =>
        this.Add(this.One);
    public Integer MinusOne =>
        this.Subtract(this.One);
    public Integer FromOne =>
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
    public Integer Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Integer Abs =>
        this.LtZ ? this.Negative : this;
    public Integer Half =>
        this.Divide(((Integer)2));
    public Integer Quarter =>
        this.Divide(((Integer)4));
    public Integer Eighth =>
        this.Divide(((Integer)8));
    public Integer Tenth =>
        this.Divide(((Integer)10));
    public Integer Twice =>
        this.Multiply(((Integer)2));
    public Integer SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public Integer Pow2 =>
        this.Multiply(this);
    public Integer Lerp(Integer b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Integer min, Integer max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
public readonly partial struct Count
{
    public Count Square =>
        this.Multiply(this);
    public Count PlusOne =>
        this.Add(this.One);
    public Count MinusOne =>
        this.Subtract(this.One);
    public Count FromOne =>
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
    public Count Sign =>
        this.LtZ ? this.One.Negative : this.GtZ ? this.One : this.Zero;
    public Count Abs =>
        this.LtZ ? this.Negative : this;
    public Count Half =>
        this.Divide(((Integer)2));
    public Count Quarter =>
        this.Divide(((Integer)4));
    public Count Eighth =>
        this.Divide(((Integer)8));
    public Count Tenth =>
        this.Divide(((Integer)10));
    public Count Twice =>
        this.Multiply(((Integer)2));
    public Count SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public Count Pow2 =>
        this.Multiply(this);
    public Count Lerp(Count b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Count min, Count max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Boolean Equals(Count b) =>
        this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Count a, Count b) => a.Equals(b);
    public Boolean NotEquals(Count b) =>
        this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Count a, Count b) => a.NotEquals(b);
    public Boolean LessThan(Count b) =>
        this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Count a, Count b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Count b) =>
        this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Count a, Count b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Count b) =>
        this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Count a, Count b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Count b) =>
        this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Count a, Count b) => a.GreaterThanOrEquals(b);
    public Count Lesser(Count b) =>
        this.LessThanOrEquals(b) ? this : b;
    public Count Greater(Count b) =>
        this.GreaterThanOrEquals(b) ? this : b;
}
public readonly partial struct Index
{
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
        this.Divide(((Integer)2));
    public Unit Quarter =>
        this.Divide(((Integer)4));
    public Unit Eighth =>
        this.Divide(((Integer)8));
    public Unit Tenth =>
        this.Divide(((Integer)10));
    public Unit Twice =>
        this.Multiply(((Integer)2));
    public Unit SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public Unit Pow2 =>
        this.Multiply(this);
    public Unit Lerp(Unit b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Unit min, Unit max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
        this.Divide(((Integer)2));
    public Percent Quarter =>
        this.Divide(((Integer)4));
    public Percent Eighth =>
        this.Divide(((Integer)8));
    public Percent Tenth =>
        this.Divide(((Integer)10));
    public Percent Twice =>
        this.Multiply(((Integer)2));
    public Percent SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public Percent Pow2 =>
        this.Multiply(this);
    public Percent Lerp(Percent b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Percent min, Percent max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public TR Aggregate<TR>(Function2<Number, TR, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(((Integer)0), Add);
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
        this.Divide(((Integer)2));
    public Vector2D Quarter =>
        this.Divide(((Integer)4));
    public Vector2D Eighth =>
        this.Divide(((Integer)8));
    public Vector2D Tenth =>
        this.Divide(((Integer)10));
    public Vector2D Twice =>
        this.Multiply(((Integer)2));
    public Vector2D SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public Vector2D Pow2 =>
        this.Multiply(this);
    public Vector2D Lerp(Vector2D b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Vector2D min, Vector2D max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public TR Aggregate<TR>(Function2<Number, TR, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(((Integer)0), Add);
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
        this.Divide(((Integer)2));
    public Vector3D Quarter =>
        this.Divide(((Integer)4));
    public Vector3D Eighth =>
        this.Divide(((Integer)8));
    public Vector3D Tenth =>
        this.Divide(((Integer)10));
    public Vector3D Twice =>
        this.Multiply(((Integer)2));
    public Vector3D SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public Vector3D Pow2 =>
        this.Multiply(this);
    public Vector3D Lerp(Vector3D b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Vector3D min, Vector3D max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public TR Aggregate<TR>(Function2<Number, TR, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(((Integer)0), Add);
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
        this.Divide(((Integer)2));
    public Vector4D Quarter =>
        this.Divide(((Integer)4));
    public Vector4D Eighth =>
        this.Divide(((Integer)8));
    public Vector4D Tenth =>
        this.Divide(((Integer)10));
    public Vector4D Twice =>
        this.Multiply(((Integer)2));
    public Vector4D SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public Vector4D Pow2 =>
        this.Multiply(this);
    public Vector4D Lerp(Vector4D b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Vector4D min, Vector4D max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public TR Aggregate<TR>(Function2<Number, TR, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(((Integer)0), Add);
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
        this.Divide(((Integer)2));
    public Complex Quarter =>
        this.Divide(((Integer)4));
    public Complex Eighth =>
        this.Divide(((Integer)8));
    public Complex Tenth =>
        this.Divide(((Integer)10));
    public Complex Twice =>
        this.Multiply(((Integer)2));
    public Complex SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public Complex Pow2 =>
        this.Multiply(this);
    public Complex Lerp(Complex b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Complex min, Complex max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
        this.Divide(((Integer)2));
    public Proportion Quarter =>
        this.Divide(((Integer)4));
    public Proportion Eighth =>
        this.Divide(((Integer)8));
    public Proportion Tenth =>
        this.Divide(((Integer)10));
    public Proportion Twice =>
        this.Multiply(((Integer)2));
    public Proportion SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public Proportion Pow2 =>
        this.Multiply(this);
    public Proportion Lerp(Proportion b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Proportion min, Proportion max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
        this.Divide(TwoPi);
    public Boolean Between(Angle min, Angle max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public Boolean Between(Length min, Length max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public Boolean Between(Mass min, Mass max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public Boolean Between(Temperature min, Temperature max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public Boolean Between(Time min, Time max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public TR Aggregate<TR>(Function2<Number, TR, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(((Integer)0), Add);
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
        this.Divide(((Integer)2));
    public UV Quarter =>
        this.Divide(((Integer)4));
    public UV Eighth =>
        this.Divide(((Integer)8));
    public UV Tenth =>
        this.Divide(((Integer)10));
    public UV Twice =>
        this.Multiply(((Integer)2));
    public UV SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public UV Pow2 =>
        this.Multiply(this);
    public UV Lerp(UV b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(UV min, UV max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public TR Aggregate<TR>(Function2<Number, TR, TR> f) => throw new NotImplementedException();
    public Number Sum =>
        this.Aggregate(((Integer)0), Add);
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
        this.Divide(((Integer)2));
    public UVW Quarter =>
        this.Divide(((Integer)4));
    public UVW Eighth =>
        this.Divide(((Integer)8));
    public UVW Tenth =>
        this.Divide(((Integer)10));
    public UVW Twice =>
        this.Multiply(((Integer)2));
    public UVW SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public UVW Pow2 =>
        this.Multiply(this);
    public UVW Lerp(UVW b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(UVW min, UVW max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public Boolean Between(Area min, Area max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public Boolean Between(Volume min, Volume max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public Boolean Between(Velocity min, Velocity max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
    public Boolean Between(Acceleration min, Acceleration max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
        this.Divide(((Integer)2));
    public Probability Quarter =>
        this.Divide(((Integer)4));
    public Probability Eighth =>
        this.Divide(((Integer)8));
    public Probability Tenth =>
        this.Divide(((Integer)10));
    public Probability Twice =>
        this.Multiply(((Integer)2));
    public Probability SmoothStep =>
        this.Square.Multiply(((Integer)3).Subtract(this.Twice));
    public Probability Pow2 =>
        this.Multiply(this);
    public Probability Lerp(Probability b, Number t) =>
        this.One.Subtract(t).Multiply(this).Add(t.Multiply(b));
    public Boolean Between(Probability min, Probability max) =>
        this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
