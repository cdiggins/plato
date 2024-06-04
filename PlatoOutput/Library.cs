using System;
public static class Constants
{
    public static Number Pi => ((Number)3.1415926535897);
    public static Number TwoPi => Constants.Pi.Twice;
    public static Number HalfPi => Constants.Pi.Half;
    public static Number Epsilon => ((Number)1E-15);
    public static Number FeetPerMeter => ((Number)3.280839895);
    public static Number FeetPerMile => ((Integer)5280);
    public static Number MetersPerLightyear => ((Number)9460730472580000);
    public static Number MetersPerAU => ((Number)149597870691);
    public static Number DaltonPerKilogram => ((Number)1.66053E-27);
    public static Number PoundPerKilogram => ((Number)0.45359237);
    public static Number PoundPerTon => ((Integer)2000);
    public static Number KilogramPerSolarMass => ((Number)1.9889200011446E+30);
    public static Number JulianYearSeconds => ((Integer)31557600);
    public static Number GregorianYearDays => ((Number)365.2425);
}
public readonly partial struct Point2D
{
    public static implicit operator Point3D(Point2D x)  => x.X.Tuple3(x.Y, ((Number)0));
    public static implicit operator Vector2D(Point2D x)  => x.X.Tuple2(x.Y);
    public Point3D Deform(System.Func<Vector3D, Vector3D> f) => f(this.X.Tuple3(this.Y, ((Integer)0)));
    public Vector2D ToVector => this;
    public Point2D Add(Vector2D b) => this.ToVector.Add(b);
    public static Point2D operator +(Point2D a, Vector2D b) => a.Add(b);
    public Point2D Subtract(Vector2D b) => this.ToVector.Subtract(b);
    public static Point2D operator -(Point2D a, Vector2D b) => a.Subtract(b);
    public Vector2D Subtract(Point2D b) => this.ToVector.Subtract(b.ToVector);
    public static Vector2D operator -(Point2D a, Point2D b) => a.Subtract(b);
}
public readonly partial struct Transform2D
{
}
public readonly partial struct Pose2D
{
}
public readonly partial struct Bounds2D
{
    public Vector2D Size => this.Max.Subtract(this.Min);
    public Point2D Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public Bounds2D Reverse => this.Max.Tuple2(this.Min);
    public Point2D Center => this.Lerp(((Number)0.5));
    public Boolean Contains(Point2D value) => value.Between(this.Min, this.Max);
    public Boolean Contains(Bounds2D y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(Bounds2D y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<Bounds2D, Bounds2D> SplitAt(Number t) => this.Left(t).Tuple2(this.Right(t));
    public Tuple2<Bounds2D, Bounds2D> Split => this.SplitAt(((Number)0.5));
    public Bounds2D Left(Number t) => this.Min.Tuple2(this.Lerp(t));
    public Bounds2D Right(Number t) => this.Lerp(t).Tuple2(this.Max);
    public Bounds2D MoveTo(Point2D v) => v.Tuple2(v.Add(this.Size));
    public Bounds2D LeftHalf => this.Left(((Number)0.5));
    public Bounds2D RightHalf => this.Right(((Number)0.5));
    public Bounds2D Recenter(Point2D c) => c.Subtract(this.Size.Half).Tuple2(c.Add(this.Size.Half));
    public Bounds2D Clamp(Bounds2D y) => this.Clamp(y.Min).Tuple2(this.Clamp(y.Max));
    public Point2D Clamp(Point2D value) => value.Clamp(this.Min, this.Max);
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
    public Array<Point2D> Points => ((Integer)3).CirclePoints;
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point2D, TAcc> f) => f(f(f(init, A), B), C);
    public Triangle2D Map(System.Func<Point2D, Point2D> f) => (f(A), f(B), f(C));
}
public readonly partial struct Quad2D
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point2D, TAcc> f) => f(f(f(f(init, A), B), C), D);
    public Quad2D Map(System.Func<Point2D, Point2D> f) => (f(A), f(B), f(C), f(D));
}
public readonly partial struct Line2D
{
    public Boolean Closed => ((Boolean)false);
    public Array<Point2D> Points => this;
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point2D, TAcc> f) => f(f(init, A), B);
    public Line2D Map(System.Func<Point2D, Point2D> f) => (f(A), f(B));
}
public readonly partial struct Circle
{
    public Boolean Closed => ((Boolean)true);
}
public readonly partial struct Lens
{
    public Boolean Closed => ((Boolean)true);
}
public readonly partial struct PolygonFace
{
}
public readonly partial struct Rect
{
    public Number Width => this.Size.Width;
    public Number Height => this.Size.Height;
    public Number HalfWidth => this.Width.Half;
    public Number HalfHeight => this.Height.Half;
    public Number Top => this.Center.Y.Add(HalfHeight);
    public Number Bottom => this.Top.Add(this.Height);
    public Number Left => this.Center.X.Subtract(HalfWidth);
    public Number Right => this.Left.Add(this.Width);
    public Point2D TopLeft => this.Left.Tuple2(this.Top);
    public Point2D TopRight => this.Right.Tuple2(this.Top);
    public Point2D BottomRight => this.Right.Tuple2(this.Bottom);
    public Point2D BottomLeft => this.Left.Tuple2(this.Bottom);
    public Array<Point2D> Points => Intrinsics.MakeArray(this.TopLeft, this.TopRight, this.BottomRight, this.BottomLeft);
    public Boolean Closed => ((Boolean)true);
}
public readonly partial struct Ellipse
{
    public Boolean Closed => ((Boolean)true);
    public Point2D Eval(Number t) => t.Turns.CirclePoint.ToVector.Multiply(this.Size).Add(this.Center);
}
public readonly partial struct Ring
{
    public Boolean Closed => ((Boolean)true);
}
public readonly partial struct Arc
{
    public Boolean Closed => ((Boolean)false);
}
public readonly partial struct Sector
{
    public Boolean Closed => ((Boolean)true);
}
public readonly partial struct Chord
{
    public Boolean Closed => ((Boolean)true);
}
public readonly partial struct Segment
{
    public Boolean Closed => ((Boolean)true);
}
public readonly partial struct RegularPolygon
{
    public Array<Point2D> Points => this.NumPoints.CirclePoints;
    public Boolean Closed => ((Boolean)true);
}
public readonly partial struct Box2D
{
}
public readonly partial struct Point3D
{
    public static implicit operator Vector3D(Point3D x)  => x.X.Tuple3(x.Y, x.Z);
    public Point3D Deform(System.Func<Vector3D, Vector3D> f) => f(this);
    public Vector3D ToVector => this;
    public Point3D Add(Vector3D b) => this.ToVector.Add(b);
    public static Point3D operator +(Point3D a, Vector3D b) => a.Add(b);
    public Point3D Subtract(Vector3D b) => this.ToVector.Subtract(b);
    public static Point3D operator -(Point3D a, Vector3D b) => a.Subtract(b);
    public Vector3D Subtract(Point3D b) => this.ToVector.Subtract(b.ToVector);
    public static Vector3D operator -(Point3D a, Point3D b) => a.Subtract(b);
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(init, X), Y), Z);
    public Point3D Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z));
}
public readonly partial struct Transform3D
{
}
public readonly partial struct Pose3D
{
}
public readonly partial struct Frame3D
{
}
public readonly partial struct Bounds3D
{
    public Vector3D Size => this.Max.Subtract(this.Min);
    public Point3D Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public Bounds3D Reverse => this.Max.Tuple2(this.Min);
    public Point3D Center => this.Lerp(((Number)0.5));
    public Boolean Contains(Point3D value) => value.Between(this.Min, this.Max);
    public Boolean Contains(Bounds3D y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(Bounds3D y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<Bounds3D, Bounds3D> SplitAt(Number t) => this.Left(t).Tuple2(this.Right(t));
    public Tuple2<Bounds3D, Bounds3D> Split => this.SplitAt(((Number)0.5));
    public Bounds3D Left(Number t) => this.Min.Tuple2(this.Lerp(t));
    public Bounds3D Right(Number t) => this.Lerp(t).Tuple2(this.Max);
    public Bounds3D MoveTo(Point3D v) => v.Tuple2(v.Add(this.Size));
    public Bounds3D LeftHalf => this.Left(((Number)0.5));
    public Bounds3D RightHalf => this.Right(((Number)0.5));
    public Bounds3D Recenter(Point3D c) => c.Subtract(this.Size.Half).Tuple2(c.Add(this.Size.Half));
    public Bounds3D Clamp(Bounds3D y) => this.Clamp(y.Min).Tuple2(this.Clamp(y.Max));
    public Point3D Clamp(Point3D value) => value.Clamp(this.Min, this.Max);
}
public readonly partial struct Line3D
{
    public Boolean Closed => ((Boolean)false);
    public Array<Point3D> Points => this;
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point3D, TAcc> f) => f(f(init, A), B);
    public Line3D Map(System.Func<Point3D, Point3D> f) => (f(A), f(B));
}
public readonly partial struct Ray3D
{
}
public readonly partial struct Triangle3D
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point3D, TAcc> f) => f(f(f(init, A), B), C);
    public Triangle3D Map(System.Func<Point3D, Point3D> f) => (f(A), f(B), f(C));
}
public readonly partial struct Quad3D
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point3D, TAcc> f) => f(f(f(f(init, A), B), C), D);
    public Quad3D Map(System.Func<Point3D, Point3D> f) => (f(A), f(B), f(C), f(D));
}
public readonly partial struct Capsule
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
public readonly partial struct Box3D
{
}
public readonly partial struct CubicBezier2D
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point2D, TAcc> f) => f(f(f(f(init, A), B), C), D);
    public CubicBezier2D Map(System.Func<Point2D, Point2D> f) => (f(A), f(B), f(C), f(D));
}
public readonly partial struct CubicBezier3D
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point3D, TAcc> f) => f(f(f(f(init, A), B), C), D);
    public CubicBezier3D Map(System.Func<Point3D, Point3D> f) => (f(A), f(B), f(C), f(D));
}
public readonly partial struct QuadraticBezier2D
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point2D, TAcc> f) => f(f(f(init, A), B), C);
    public QuadraticBezier2D Map(System.Func<Point2D, Point2D> f) => (f(A), f(B), f(C));
}
public readonly partial struct QuadraticBezier3D
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point3D, TAcc> f) => f(f(f(init, A), B), C);
    public QuadraticBezier3D Map(System.Func<Point3D, Point3D> f) => (f(A), f(B), f(C));
}
public readonly partial struct Quaternion
{
    public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(Quaternion v2) => this.Multiply(v2).Sum;
    public Quaternion Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(f(init, X), Y), Z), W);
    public Quaternion Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z), f(W));
    public Quaternion PlusOne => this.Add(this.One);
    public Quaternion MinusOne => this.Subtract(this.One);
    public Quaternion FromOne => this.One.Subtract(this);
    public Quaternion Pow2 => this.Multiply(this);
    public Quaternion Pow3 => this.Pow2.Multiply(this);
    public Quaternion Pow4 => this.Pow3.Multiply(this);
    public Quaternion Pow5 => this.Pow4.Multiply(this);
    public Quaternion Square => this.Pow2;
    public Quaternion Half => this.Divide(((Number)2));
    public Quaternion Quarter => this.Divide(((Number)4));
    public Quaternion Tenth => this.Divide(((Number)10));
    public Quaternion Twice => this.Multiply(((Number)2));
    public Quaternion Lerp(Quaternion b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
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
public readonly partial struct Orientation3D
{
}
public readonly partial struct Point4D
{
    public static implicit operator Vector4D(Point4D x)  => x.X.Tuple4(x.Y, x.Z, x.W);
    public Vector4D ToVector => this;
    public Point4D Add(Vector4D b) => this.ToVector.Add(b);
    public static Point4D operator +(Point4D a, Vector4D b) => a.Add(b);
    public Point4D Subtract(Vector4D b) => this.ToVector.Subtract(b);
    public static Point4D operator -(Point4D a, Vector4D b) => a.Subtract(b);
    public Vector4D Subtract(Point4D b) => this.ToVector.Subtract(b.ToVector);
    public static Vector4D operator -(Point4D a, Point4D b) => a.Subtract(b);
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(f(init, X), Y), Z), W);
    public Point4D Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z), f(W));
}
public readonly partial struct Line4D
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Point4D, TAcc> f) => f(f(init, A), B);
    public Line4D Map(System.Func<Point4D, Point4D> f) => (f(A), f(B));
}
public readonly partial struct Quadray
{
    public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(Quadray v2) => this.Multiply(v2).Sum;
    public Quadray Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(f(init, X), Y), Z), W);
    public Quadray Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z), f(W));
    public Quadray PlusOne => this.Add(this.One);
    public Quadray MinusOne => this.Subtract(this.One);
    public Quadray FromOne => this.One.Subtract(this);
    public Quadray Pow2 => this.Multiply(this);
    public Quadray Pow3 => this.Pow2.Multiply(this);
    public Quadray Pow4 => this.Pow3.Multiply(this);
    public Quadray Pow5 => this.Pow4.Multiply(this);
    public Quadray Square => this.Pow2;
    public Quadray Half => this.Divide(((Number)2));
    public Quadray Quarter => this.Divide(((Number)4));
    public Quadray Tenth => this.Divide(((Number)10));
    public Quadray Twice => this.Multiply(((Number)2));
    public Quadray Lerp(Quadray b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Number
{
    public Angle Turns => this;
    public Angle Acos => Intrinsics.Acos(this);
    public Angle Asin => Intrinsics.Asin(this);
    public Angle Atan => Intrinsics.Atan(this);
    public Number Pow(Number y) => Intrinsics.Pow(this, y);
    public Number Log(Number y) => Intrinsics.Log(this, y);
    public Number Ln => Intrinsics.Ln(this);
    public Number Exp => Intrinsics.Exp(this);
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
    public Number Hundred => this.Multiply(((Integer)100));
    public Number Thousand => this.Multiply(((Integer)1000));
    public Number Million => this.Thousand.Thousand;
    public Number Billion => this.Thousand.Million;
    public Number OunceToGram => this.Multiply(((Number)28.349523125));
    public Number TroyOunceToGram => this.Multiply(((Number)31.1034768));
    public Number GrainToMilligram => this.Multiply(((Number)64.79891));
    public Number Mole => this.Multiply(((Number)6.02214076E+23));
    public Number Inverse => ((Number)1).Divide(this);
    public Number Reciprocal => this.Inverse;
    public Number SquareRoot => this.Pow(((Number)0.5));
    public Number Sqrt => this.SquareRoot;
    public Number SmoothStep => this.Square.Multiply(((Number)3).Subtract(this.Twice));
    public Number MultiplyEpsilon(Number y) => this.Abs.Greater(y.Abs).Multiply(Constants.Epsilon);
    public Boolean AlmostEqual(Number y) => this.Subtract(y).Abs.LessThanOrEquals(this.MultiplyEpsilon(y));
    public Boolean AlmostZero => this.Abs.LessThan(Constants.Epsilon);
    public Boolean AlmostZeroOrOne => this.AlmostEqual(((Integer)0)).Or(this.AlmostEqual(((Integer)1)));
    public Number InverseLerp(Number b, Number v) => v.Subtract(this).Divide(b.Subtract(this));
    public Number Remap(Number bIn, Number aOut, Number bOut, Number v) => aOut.Lerp(bOut, this.InverseLerp(aOut, v));
    public Number Magnitude => this.Value;
    public Number ClampOne => this.Clamp(this.Zero, this.One);
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
    public Number Abs => this.LtZ ? this.Negative : this;
    public Number PlusOne => this.Add(this.One);
    public Number MinusOne => this.Subtract(this.One);
    public Number FromOne => this.One.Subtract(this);
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
    public Boolean Between(Number min, Number max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Number Half => this.Divide(((Number)2));
    public Number Quarter => this.Divide(((Number)4));
    public Number Tenth => this.Divide(((Number)10));
    public Number Twice => this.Multiply(((Number)2));
    public Number Lerp(Number b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
    public Number Pow2 => this.Multiply(this);
    public Number Pow3 => this.Pow2.Multiply(this);
    public Number Pow4 => this.Pow3.Multiply(this);
    public Number Pow5 => this.Pow4.Multiply(this);
    public Number Square => this.Pow2;
}
public readonly partial struct Integer
{
    public Number FloatDivision(Integer y) => this.ToNumber.Divide(y.ToNumber);
    public Array<Number> Fractions
    {
        get
        {
            var _var0 = this;
            return this.Range.Map((i) => i.FloatDivision(_var0));
        }
    }
    public Array<Point2D> CirclePoints => this.Fractions.Map((x) => x.Turns.CirclePoint);
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
    public Array<Integer> Range => Intrinsics.Range(this);
    public Integer PlusOne => this.Add(this.One);
    public Integer MinusOne => this.Subtract(this.One);
    public Integer FromOne => this.One.Subtract(this);
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
    public Boolean Between(Integer min, Integer max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Integer Pow2 => this.Multiply(this);
    public Integer Pow3 => this.Pow2.Multiply(this);
    public Integer Pow4 => this.Pow3.Multiply(this);
    public Integer Pow5 => this.Pow4.Multiply(this);
    public Integer Square => this.Pow2;
}
public readonly partial struct String
{
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
    public Boolean Between(String min, String max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
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
public readonly partial struct Type
{
}
public readonly partial struct Error
{
}
public readonly partial struct Tuple2<T0, T1>
{
}
public readonly partial struct Tuple3<T0, T1, T2>
{
}
public readonly partial struct Tuple4<T0, T1, T2, T3>
{
}
public readonly partial struct Tuple5<T0, T1, T2, T3, T4>
{
}
public readonly partial struct Tuple6<T0, T1, T2, T3, T4, T5>
{
}
public readonly partial struct Tuple7<T0, T1, T2, T3, T4, T5, T6>
{
}
public readonly partial struct Tuple8<T0, T1, T2, T3, T4, T5, T6, T7>
{
}
public readonly partial struct Tuple9<T0, T1, T2, T3, T4, T5, T6, T7, T8>
{
}
public readonly partial struct Tuple10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
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
public readonly partial struct Function4<T0, T1, T2, T3, TR>
{
}
public readonly partial struct Function5<T0, T1, T2, T3, T4, TR>
{
}
public readonly partial struct Function6<T0, T1, T2, T3, T4, T5, TR>
{
}
public readonly partial struct Function7<T0, T1, T2, T3, T4, T5, T6, TR>
{
}
public readonly partial struct Function8<T0, T1, T2, T3, T4, T5, T6, T7, TR>
{
}
public readonly partial struct Function9<T0, T1, T2, T3, T4, T5, T6, T7, T8, TR>
{
}
public readonly partial struct Function10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, TR>
{
}
public readonly partial struct Count
{
    public Count PlusOne => this.Add(this.One);
    public Count MinusOne => this.Subtract(this.One);
    public Count FromOne => this.One.Subtract(this);
    public Count Clamp(Count a, Count b) => this.Greater(a).Lesser(b);
    public Boolean Equals(Count b) => this.Compare(b).Equals(((Integer)0));
    public static Boolean operator ==(Count a, Count b) => a.Equals(b);
    public Boolean NotEquals(Count b) => this.Compare(b).NotEquals(((Integer)0));
    public static Boolean operator !=(Count a, Count b) => a.NotEquals(b);
    public Boolean LessThan(Count b) => this.Compare(b).LessThan(((Integer)0));
    public static Boolean operator <(Count a, Count b) => a.LessThan(b);
    public Boolean LessThanOrEquals(Count b) => this.Compare(b).LessThanOrEquals(((Integer)0));
    public static Boolean operator <=(Count a, Count b) => a.LessThanOrEquals(b);
    public Boolean GreaterThan(Count b) => this.Compare(b).GreaterThan(((Integer)0));
    public static Boolean operator >(Count a, Count b) => a.GreaterThan(b);
    public Boolean GreaterThanOrEquals(Count b) => this.Compare(b).GreaterThanOrEquals(((Integer)0));
    public static Boolean operator >=(Count a, Count b) => a.GreaterThanOrEquals(b);
    public Count Lesser(Count b) => this.LessThanOrEquals(b) ? this : b;
    public Count Greater(Count b) => this.GreaterThanOrEquals(b) ? this : b;
    public Boolean Between(Count min, Count max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Count Pow2 => this.Multiply(this);
    public Count Pow3 => this.Pow2.Multiply(this);
    public Count Pow4 => this.Pow3.Multiply(this);
    public Count Pow5 => this.Pow4.Multiply(this);
    public Count Square => this.Pow2;
}
public readonly partial struct Cardinal
{
    public Cardinal PlusOne => this.Add(this.One);
    public Cardinal MinusOne => this.Subtract(this.One);
    public Cardinal FromOne => this.One.Subtract(this);
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
    public Boolean Between(Cardinal min, Cardinal max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Cardinal Pow2 => this.Multiply(this);
    public Cardinal Pow3 => this.Pow2.Multiply(this);
    public Cardinal Pow4 => this.Pow3.Multiply(this);
    public Cardinal Pow5 => this.Pow4.Multiply(this);
    public Cardinal Square => this.Pow2;
}
public readonly partial struct Index
{
    public Index PlusOne => this.Add(this.One);
    public Index MinusOne => this.Subtract(this.One);
    public Index FromOne => this.One.Subtract(this);
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
    public Boolean Between(Index min, Index max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Index Pow2 => this.Multiply(this);
    public Index Pow3 => this.Pow2.Multiply(this);
    public Index Pow4 => this.Pow3.Multiply(this);
    public Index Pow5 => this.Pow4.Multiply(this);
    public Index Square => this.Pow2;
}
public readonly partial struct Unit
{
    public Number Magnitude => this.Value;
    public Unit ClampOne => this.Clamp(this.Zero, this.One);
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
    public Unit Abs => this.LtZ ? this.Negative : this;
    public Unit PlusOne => this.Add(this.One);
    public Unit MinusOne => this.Subtract(this.One);
    public Unit FromOne => this.One.Subtract(this);
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
    public Boolean Between(Unit min, Unit max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Unit Half => this.Divide(((Number)2));
    public Unit Quarter => this.Divide(((Number)4));
    public Unit Tenth => this.Divide(((Number)10));
    public Unit Twice => this.Multiply(((Number)2));
    public Unit Lerp(Unit b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Probability
{
    public Number Magnitude => this.Value;
    public Probability ClampOne => this.Clamp(this.Zero, this.One);
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
    public Probability Abs => this.LtZ ? this.Negative : this;
    public Probability PlusOne => this.Add(this.One);
    public Probability MinusOne => this.Subtract(this.One);
    public Probability FromOne => this.One.Subtract(this);
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
    public Boolean Between(Probability min, Probability max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Probability Half => this.Divide(((Number)2));
    public Probability Quarter => this.Divide(((Number)4));
    public Probability Tenth => this.Divide(((Number)10));
    public Probability Twice => this.Multiply(((Number)2));
    public Probability Lerp(Probability b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Complex
{
    public Integer Count => ((Integer)2);
    public Number At(Integer n) => n.Equals(((Integer)0)) ? this.Real : this.Imaginary;
    public Number this[Integer n] => At(n);
    public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(Complex v2) => this.Multiply(v2).Sum;
    public Complex Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(init, Real), Imaginary);
    public Complex Map(System.Func<Number, Number> f) => (f(Real), f(Imaginary));
    public Complex PlusOne => this.Add(this.One);
    public Complex MinusOne => this.Subtract(this.One);
    public Complex FromOne => this.One.Subtract(this);
    public Complex Pow2 => this.Multiply(this);
    public Complex Pow3 => this.Pow2.Multiply(this);
    public Complex Pow4 => this.Pow3.Multiply(this);
    public Complex Pow5 => this.Pow4.Multiply(this);
    public Complex Square => this.Pow2;
    public Complex Half => this.Divide(((Number)2));
    public Complex Quarter => this.Divide(((Number)4));
    public Complex Tenth => this.Divide(((Number)10));
    public Complex Twice => this.Multiply(((Number)2));
    public Complex Lerp(Complex b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Integer2
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Integer, TAcc> f) => f(f(init, A), B);
    public Integer2 Map(System.Func<Integer, Integer> f) => (f(A), f(B));
}
public readonly partial struct Integer3
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Integer, TAcc> f) => f(f(f(init, A), B), C);
    public Integer3 Map(System.Func<Integer, Integer> f) => (f(A), f(B), f(C));
}
public readonly partial struct Integer4
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Integer, TAcc> f) => f(f(f(f(init, A), B), C), D);
    public Integer4 Map(System.Func<Integer, Integer> f) => (f(A), f(B), f(C), f(D));
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
public readonly partial struct Size2D
{
    public static implicit operator Vector2D(Size2D a)  => a.Width.Tuple2(a.Height);
}
public readonly partial struct Size3D
{
}
public readonly partial struct Fraction
{
}
public readonly partial struct Angle
{
    public Point2D CirclePoint => this.Sin.Tuple2(this.Cos);
    public Number Cos => Intrinsics.Cos(this);
    public Number Sin => Intrinsics.Sin(this);
    public Number Tan => Intrinsics.Tan(this);
    public Number Magnitude => this.Value;
    public Angle ClampOne => this.Clamp(this.Zero, this.One);
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
    public Angle Abs => this.LtZ ? this.Negative : this;
    public Angle PlusOne => this.Add(this.One);
    public Angle MinusOne => this.Subtract(this.One);
    public Angle FromOne => this.One.Subtract(this);
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
    public Boolean Between(Angle min, Angle max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Angle Half => this.Divide(((Number)2));
    public Angle Quarter => this.Divide(((Number)4));
    public Angle Tenth => this.Divide(((Number)10));
    public Angle Twice => this.Multiply(((Number)2));
    public Angle Lerp(Angle b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Length
{
    public Number Magnitude => this.Value;
    public Length ClampOne => this.Clamp(this.Zero, this.One);
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
    public Length Abs => this.LtZ ? this.Negative : this;
    public Length PlusOne => this.Add(this.One);
    public Length MinusOne => this.Subtract(this.One);
    public Length FromOne => this.One.Subtract(this);
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
    public Boolean Between(Length min, Length max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Length Half => this.Divide(((Number)2));
    public Length Quarter => this.Divide(((Number)4));
    public Length Tenth => this.Divide(((Number)10));
    public Length Twice => this.Multiply(((Number)2));
    public Length Lerp(Length b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Mass
{
    public Number Magnitude => this.Value;
    public Mass ClampOne => this.Clamp(this.Zero, this.One);
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
    public Mass Abs => this.LtZ ? this.Negative : this;
    public Mass PlusOne => this.Add(this.One);
    public Mass MinusOne => this.Subtract(this.One);
    public Mass FromOne => this.One.Subtract(this);
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
    public Boolean Between(Mass min, Mass max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Mass Half => this.Divide(((Number)2));
    public Mass Quarter => this.Divide(((Number)4));
    public Mass Tenth => this.Divide(((Number)10));
    public Mass Twice => this.Multiply(((Number)2));
    public Mass Lerp(Mass b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Temperature
{
    public Number Magnitude => this.Value;
    public Temperature ClampOne => this.Clamp(this.Zero, this.One);
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
    public Temperature Abs => this.LtZ ? this.Negative : this;
    public Temperature PlusOne => this.Add(this.One);
    public Temperature MinusOne => this.Subtract(this.One);
    public Temperature FromOne => this.One.Subtract(this);
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
    public Boolean Between(Temperature min, Temperature max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Temperature Half => this.Divide(((Number)2));
    public Temperature Quarter => this.Divide(((Number)4));
    public Temperature Tenth => this.Divide(((Number)10));
    public Temperature Twice => this.Multiply(((Number)2));
    public Temperature Lerp(Temperature b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Time
{
    public Number Magnitude => this.Value;
    public Time ClampOne => this.Clamp(this.Zero, this.One);
    public Boolean GtZ => this.GreaterThan(this.Zero);
    public Boolean LtZ => this.LessThan(this.Zero);
    public Boolean GtEqZ => this.GreaterThanOrEquals(this.Zero);
    public Boolean LtEqZ => this.LessThanOrEquals(this.Zero);
    public Boolean IsPositive => this.GtEqZ;
    public Boolean IsNegative => this.LtZ;
    public Integer Sign => this.LtZ ? ((Integer)1).Negative : this.GtZ ? ((Integer)1) : ((Integer)0);
    public Time Abs => this.LtZ ? this.Negative : this;
    public Time PlusOne => this.Add(this.One);
    public Time MinusOne => this.Subtract(this.One);
    public Time FromOne => this.One.Subtract(this);
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
    public Boolean Between(Time min, Time max) => this.GreaterThanOrEquals(min).And(this.LessThanOrEquals(max));
    public Time Half => this.Divide(((Number)2));
    public Time Quarter => this.Divide(((Number)4));
    public Time Tenth => this.Divide(((Number)10));
    public Time Twice => this.Multiply(((Number)2));
    public Time Lerp(Time b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct TimeRange
{
    public Time Size => this.Max.Subtract(this.Min);
    public DateTime Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public TimeRange Reverse => this.Max.Tuple2(this.Min);
    public DateTime Center => this.Lerp(((Number)0.5));
    public Boolean Contains(DateTime value) => value.Between(this.Min, this.Max);
    public Boolean Contains(TimeRange y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(TimeRange y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<TimeRange, TimeRange> SplitAt(Number t) => this.Left(t).Tuple2(this.Right(t));
    public Tuple2<TimeRange, TimeRange> Split => this.SplitAt(((Number)0.5));
    public TimeRange Left(Number t) => this.Min.Tuple2(this.Lerp(t));
    public TimeRange Right(Number t) => this.Lerp(t).Tuple2(this.Max);
    public TimeRange MoveTo(DateTime v) => v.Tuple2(v.Add(this.Size));
    public TimeRange LeftHalf => this.Left(((Number)0.5));
    public TimeRange RightHalf => this.Right(((Number)0.5));
    public TimeRange Recenter(DateTime c) => c.Subtract(this.Size.Half).Tuple2(c.Add(this.Size.Half));
    public TimeRange Clamp(TimeRange y) => this.Clamp(y.Min).Tuple2(this.Clamp(y.Max));
    public DateTime Clamp(DateTime value) => value.Clamp(this.Min, this.Max);
}
public readonly partial struct DateTime
{
}
public readonly partial struct AnglePair
{
    public Angle Size => this.Max.Subtract(this.Min);
    public Angle Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public AnglePair Reverse => this.Max.Tuple2(this.Min);
    public Angle Center => this.Lerp(((Number)0.5));
    public Boolean Contains(Angle value) => value.Between(this.Min, this.Max);
    public Boolean Contains(AnglePair y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(AnglePair y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<AnglePair, AnglePair> SplitAt(Number t) => this.Left(t).Tuple2(this.Right(t));
    public Tuple2<AnglePair, AnglePair> Split => this.SplitAt(((Number)0.5));
    public AnglePair Left(Number t) => this.Min.Tuple2(this.Lerp(t));
    public AnglePair Right(Number t) => this.Lerp(t).Tuple2(this.Max);
    public AnglePair MoveTo(Angle v) => v.Tuple2(v.Add(this.Size));
    public AnglePair LeftHalf => this.Left(((Number)0.5));
    public AnglePair RightHalf => this.Right(((Number)0.5));
    public AnglePair Recenter(Angle c) => c.Subtract(this.Size.Half).Tuple2(c.Add(this.Size.Half));
    public AnglePair Clamp(AnglePair y) => this.Clamp(y.Min).Tuple2(this.Clamp(y.Max));
    public Angle Clamp(Angle value) => value.Clamp(this.Min, this.Max);
}
public readonly partial struct NumberInterval
{
    public Number Size => this.Max.Subtract(this.Min);
    public Number Lerp(Number amount) => this.Min.Lerp(this.Max, amount);
    public NumberInterval Reverse => this.Max.Tuple2(this.Min);
    public Number Center => this.Lerp(((Number)0.5));
    public Boolean Contains(Number value) => value.Between(this.Min, this.Max);
    public Boolean Contains(NumberInterval y) => this.Contains(y.Min).And(this.Contains(y.Max));
    public Boolean Overlaps(NumberInterval y) => this.Contains(y.Min).Or(this.Contains(y.Max).Or(y.Contains(this.Min).Or(y.Contains(this.Max))));
    public Tuple2<NumberInterval, NumberInterval> SplitAt(Number t) => this.Left(t).Tuple2(this.Right(t));
    public Tuple2<NumberInterval, NumberInterval> Split => this.SplitAt(((Number)0.5));
    public NumberInterval Left(Number t) => this.Min.Tuple2(this.Lerp(t));
    public NumberInterval Right(Number t) => this.Lerp(t).Tuple2(this.Max);
    public NumberInterval MoveTo(Number v) => v.Tuple2(v.Add(this.Size));
    public NumberInterval LeftHalf => this.Left(((Number)0.5));
    public NumberInterval RightHalf => this.Right(((Number)0.5));
    public NumberInterval Recenter(Number c) => c.Subtract(this.Size.Half).Tuple2(c.Add(this.Size.Half));
    public NumberInterval Clamp(NumberInterval y) => this.Clamp(y.Min).Tuple2(this.Clamp(y.Max));
    public Number Clamp(Number value) => value.Clamp(this.Min, this.Max);
}
public readonly partial struct Matrix2D
{
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Vector3D, TAcc> f) => f(f(f(init, Column1), Column2), Column3);
    public Matrix2D Map(System.Func<Vector3D, Vector3D> f) => (f(Column1), f(Column2), f(Column3));
}
public readonly partial struct Matrix3D
{
    public Number M11 => this.Column1.X;
    public Number M12 => this.Column2.X;
    public Number M13 => this.Column3.X;
    public Number M14 => this.Column4.X;
    public Number M21 => this.Column1.Y;
    public Number M22 => this.Column2.Y;
    public Number M23 => this.Column3.Y;
    public Number M24 => this.Column4.Y;
    public Number M31 => this.Column1.Z;
    public Number M32 => this.Column2.Z;
    public Number M33 => this.Column3.Z;
    public Number M34 => this.Column4.Z;
    public Number M41 => this.Column1.W;
    public Number M42 => this.Column2.W;
    public Number M43 => this.Column3.W;
    public Number M44 => this.Column4.W;
    public Vector3D Multiply(Vector3D v) => v.X.Multiply(this.M11).Add(v.Y.Multiply(this.M21).Add(v.Z.Multiply(this.M31).Add(this.M41))).Tuple3(v.X.Multiply(this.M12).Add(v.Y.Multiply(this.M22).Add(v.Z.Multiply(this.M32).Add(this.M42))), v.X.Multiply(this.M13).Add(v.Y.Multiply(this.M23).Add(v.Z.Multiply(this.M33).Add(this.M43))));
    public static Vector3D operator *(Matrix3D m, Vector3D v) => m.Multiply(v);
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Vector4D, TAcc> f) => f(f(f(f(init, Column1), Column2), Column3), Column4);
    public Matrix3D Map(System.Func<Vector4D, Vector4D> f) => (f(Column1), f(Column2), f(Column3), f(Column4));
}
public readonly partial struct UV
{
    public Integer Count => ((Integer)2);
    public Number At(Integer n) => n.Equals(((Integer)0)) ? this.U : this.V;
    public Number this[Integer n] => At(n);
    public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(UV v2) => this.Multiply(v2).Sum;
    public UV Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(init, U), V);
    public UV Map(System.Func<Number, Number> f) => (f(U), f(V));
    public UV PlusOne => this.Add(this.One);
    public UV MinusOne => this.Subtract(this.One);
    public UV FromOne => this.One.Subtract(this);
    public UV Pow2 => this.Multiply(this);
    public UV Pow3 => this.Pow2.Multiply(this);
    public UV Pow4 => this.Pow3.Multiply(this);
    public UV Pow5 => this.Pow4.Multiply(this);
    public UV Square => this.Pow2;
    public UV Half => this.Divide(((Number)2));
    public UV Quarter => this.Divide(((Number)4));
    public UV Tenth => this.Divide(((Number)10));
    public UV Twice => this.Multiply(((Number)2));
    public UV Lerp(UV b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct UVW
{
    public Integer Count => ((Integer)3);
    public Number At(Integer n) => n.Equals(((Integer)0)) ? this.U : n.Equals(((Integer)1)) ? this.V : this.W;
    public Number this[Integer n] => At(n);
    public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(UVW v2) => this.Multiply(v2).Sum;
    public UVW Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(init, U), V), W);
    public UVW Map(System.Func<Number, Number> f) => (f(U), f(V), f(W));
    public UVW PlusOne => this.Add(this.One);
    public UVW MinusOne => this.Subtract(this.One);
    public UVW FromOne => this.One.Subtract(this);
    public UVW Pow2 => this.Multiply(this);
    public UVW Pow3 => this.Pow2.Multiply(this);
    public UVW Pow4 => this.Pow3.Multiply(this);
    public UVW Pow5 => this.Pow4.Multiply(this);
    public UVW Square => this.Pow2;
    public UVW Half => this.Divide(((Number)2));
    public UVW Quarter => this.Divide(((Number)4));
    public UVW Tenth => this.Divide(((Number)10));
    public UVW Twice => this.Multiply(((Number)2));
    public UVW Lerp(UVW b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Vector2D
{
    public static implicit operator Point2D(Vector2D x)  => x.X.Tuple2(x.Y);
    public Integer Count => ((Integer)2);
    public Number At(Integer n) => n.Equals(((Integer)0)) ? this.X : this.Y;
    public Number this[Integer n] => At(n);
    public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(Vector2D v2) => this.Multiply(v2).Sum;
    public Vector2D Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(init, X), Y);
    public Vector2D Map(System.Func<Number, Number> f) => (f(X), f(Y));
    public Vector2D PlusOne => this.Add(this.One);
    public Vector2D MinusOne => this.Subtract(this.One);
    public Vector2D FromOne => this.One.Subtract(this);
    public Vector2D Pow2 => this.Multiply(this);
    public Vector2D Pow3 => this.Pow2.Multiply(this);
    public Vector2D Pow4 => this.Pow3.Multiply(this);
    public Vector2D Pow5 => this.Pow4.Multiply(this);
    public Vector2D Square => this.Pow2;
    public Vector2D Half => this.Divide(((Number)2));
    public Vector2D Quarter => this.Divide(((Number)4));
    public Vector2D Tenth => this.Divide(((Number)10));
    public Vector2D Twice => this.Multiply(((Number)2));
    public Vector2D Lerp(Vector2D b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Vector3D
{
    public static implicit operator Point3D(Vector3D x)  => x.X.Tuple3(x.Y, x.Z);
    public Integer Count => ((Integer)3);
    public Number At(Integer n) => n.Equals(((Integer)0)) ? this.X : n.Equals(((Integer)1)) ? this.Y : this.Z;
    public Number this[Integer n] => At(n);
    public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(Vector3D v2) => this.Multiply(v2).Sum;
    public Vector3D Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(init, X), Y), Z);
    public Vector3D Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z));
    public Vector3D PlusOne => this.Add(this.One);
    public Vector3D MinusOne => this.Subtract(this.One);
    public Vector3D FromOne => this.One.Subtract(this);
    public Vector3D Pow2 => this.Multiply(this);
    public Vector3D Pow3 => this.Pow2.Multiply(this);
    public Vector3D Pow4 => this.Pow3.Multiply(this);
    public Vector3D Pow5 => this.Pow4.Multiply(this);
    public Vector3D Square => this.Pow2;
    public Vector3D Half => this.Divide(((Number)2));
    public Vector3D Quarter => this.Divide(((Number)4));
    public Vector3D Tenth => this.Divide(((Number)10));
    public Vector3D Twice => this.Multiply(((Number)2));
    public Vector3D Lerp(Vector3D b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
public readonly partial struct Vector4D
{
    public static implicit operator Point4D(Vector4D x)  => x.X.Tuple4(x.Y, x.Z, x.W);
    public Integer Count => ((Integer)4);
    public Number At(Integer n) => n.Equals(((Integer)0)) ? this.X : n.Equals(((Integer)1)) ? this.Y : n.Equals(((Integer)2)) ? this.Z : this.W;
    public Number this[Integer n] => At(n);
    public Number Sum => this.Reduce(((Number)0), (a, b) => a.Add(b));
    public Number SumSquares => this.Square.Sum;
    public Number MagnitudeSquared => this.SumSquares;
    public Number Magnitude => this.MagnitudeSquared.SquareRoot;
    public Number Dot(Vector4D v2) => this.Multiply(v2).Sum;
    public Vector4D Normal => this.Divide(this.Magnitude);
    public Number Average => this.Sum.Divide(this.Count);
    public TAcc Reduce<TAcc>(TAcc init, System.Func<TAcc, Number, TAcc> f) => f(f(f(f(init, X), Y), Z), W);
    public Vector4D Map(System.Func<Number, Number> f) => (f(X), f(Y), f(Z), f(W));
    public Vector4D PlusOne => this.Add(this.One);
    public Vector4D MinusOne => this.Subtract(this.One);
    public Vector4D FromOne => this.One.Subtract(this);
    public Vector4D Pow2 => this.Multiply(this);
    public Vector4D Pow3 => this.Pow2.Multiply(this);
    public Vector4D Pow4 => this.Pow3.Multiply(this);
    public Vector4D Pow5 => this.Pow4.Multiply(this);
    public Vector4D Square => this.Pow2;
    public Vector4D Half => this.Divide(((Number)2));
    public Vector4D Quarter => this.Divide(((Number)4));
    public Vector4D Tenth => this.Divide(((Number)10));
    public Vector4D Twice => this.Multiply(((Number)2));
    public Vector4D Lerp(Vector4D b, Number t) => this.Multiply(t.FromOne).Add(b.Multiply(t));
}
