public static partial class IntrinsicsLibrary
{
    public static Number Cos(this Angle x) => throw new NotImplementedException();
    public static Number Sin(this Angle x) => throw new NotImplementedException();
    public static Number Tan(this Angle x) => throw new NotImplementedException();
    public static Angle Acos(this Number x) => throw new NotImplementedException();
    public static Angle Asin(this Number x) => throw new NotImplementedException();
    public static Angle Atan(this Number x) => throw new NotImplementedException();
    public static Number Pow(this Number x, Number y) => throw new NotImplementedException();
    public static Number Log(this Number x, Number y) => throw new NotImplementedException();
    public static Number NaturalLog(this Number x) => throw new NotImplementedException();
    public static Number NaturalPower(this Number x) => throw new NotImplementedException();
    public static String Interpolate<T>(this Array<T> xs) => throw new NotImplementedException();
    public static Any Throw(this Any x) => throw new NotImplementedException();
    public static Number Add(this Number x, Number y) => throw new NotImplementedException();
    public static Number Subtract(this Number x, Number y) => throw new NotImplementedException();
    public static Number Divide(this Number x, Number y) => throw new NotImplementedException();
    public static Number Multiply(this Number x, Number y) => throw new NotImplementedException();
    public static Number Modulo(this Number x, Number y) => throw new NotImplementedException();
    public static Number Negative(this Number x) => throw new NotImplementedException();
    public static Integer Add(this Integer x, Integer y) => throw new NotImplementedException();
    public static Integer Subtract(this Integer x, Integer y) => throw new NotImplementedException();
    public static Integer Divide(this Integer x, Integer y) => throw new NotImplementedException();
    public static Integer Multiply(this Integer x, Integer y) => throw new NotImplementedException();
    public static Integer Modulo(this Integer x, Integer y) => throw new NotImplementedException();
    public static Integer Negative(this Integer x) => throw new NotImplementedException();
    public static Boolean And(this Boolean x, Boolean y) => throw new NotImplementedException();
    public static Boolean Or(this Boolean x, Boolean y) => throw new NotImplementedException();
    public static Boolean Not(this Boolean x) => throw new NotImplementedException();
    public static Number ToNumber(this Integer x) => throw new NotImplementedException();
}
public static partial class IntervalLibrary
{
    public static Numerical<Self> Size<T>(this Interval<T> x) => throw new NotImplementedException();
    public static Boolean IsEmpty<T>(this Interval<T> x) => throw new NotImplementedException();
    public static Numerical<Self> Lerp<T>(this Interval<T> x, Number amount) => throw new NotImplementedException();
    public static Unit InverseLerp<T>(this Interval<T> x, Numerical<Self> value) => throw new NotImplementedException();
    public static Interval<T> Negate<T>(this Interval<T> x) => throw new NotImplementedException();
    public static Interval<T> Reverse<T>(this Interval<T> x) => throw new NotImplementedException();
    public static Numerical<Self> Center<T>(this Interval<T> x) => throw new NotImplementedException();
    public static Boolean Contains<T>(this Interval<T> x, Numerical<Self> value) => throw new NotImplementedException();
    public static Boolean Contains<T>(this Interval<T> x, Interval<T> other) => throw new NotImplementedException();
    public static Boolean Overlaps<T>(this Interval<T> x, Interval<T> y) => throw new NotImplementedException();
    public static Tuple2<T0, T1> Split<T>(this Interval<T> x, Number t) => throw new NotImplementedException();
    public static Tuple2<T0, T1> Split<T>(this Interval<T> x) => throw new NotImplementedException();
    public static Interval<T> Left<T>(this Interval<T> x, Number t) => throw new NotImplementedException();
    public static Interval<T> Right<T>(this Interval<T> x, Number t) => throw new NotImplementedException();
    public static Interval<T> MoveTo<T>(this Interval<T> x, Numerical<Self> v) => throw new NotImplementedException();
    public static Interval<T> LeftHalf<T>(this Interval<T> x) => throw new NotImplementedException();
    public static Interval<T> RightHalf<T>(this Interval<T> x) => throw new NotImplementedException();
    public static Numerical<Self> HalfSize<T>(this Interval<T> x) => throw new NotImplementedException();
    public static Interval<T> Recenter<T>(this Interval<T> x, Numerical<Self> c) => throw new NotImplementedException();
    public static Interval<T> Clamp<T>(this Interval<T> x, Interval<T> y) => throw new NotImplementedException();
    public static Interpolatable<Self> Clamp<T>(this Interval<T> x, Interpolatable<Self> value) => throw new NotImplementedException();
    public static Boolean Within<T>(this Interval<T> x, Numerical<Self> value) => throw new NotImplementedException();
}
public static partial class VectorLibrary
{
    public static Numerical<Self> Aggregate<Self, T>(this Vector<Self, T> v, Function2<T0, T1, TR> f) => throw new NotImplementedException();
    public static Numerical<Self> Sum<Self, T>(this Vector<Self, T> v) => throw new NotImplementedException();
    public static Numerical<Self> SumSquares<Self, T>(this Vector<Self, T> v) => throw new NotImplementedException();
    public static Numerical<Self> MagnitudeSquared<Self, T>(this Vector<Self, T> v) => throw new NotImplementedException();
    public static Numerical<Self> Magnitude<Self, T>(this Vector<Self, T> v) => throw new NotImplementedException();
    public static Numerical<Self> Dot<Self, T>(this Vector<Self, T> v1, Vector<Self, T> v2) => throw new NotImplementedException();
    public static Vector<Self, T> Normal<Self, T>(this Vector<Self, T> v) => throw new NotImplementedException();
    public static Numerical<Self> Average<Self, T>(this Vector<Self, T> v) => throw new NotImplementedException();
}
public static partial class NumericalLibrary
{
    public static Number SquareRoot(this Number x) => throw new NotImplementedException();
    public static Numerical<Self> Square<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Number Clamp(this Number x) => throw new NotImplementedException();
    public static Numerical<Self> PlusOne<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> MinusOne<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> FromOne<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Boolean IsPositive<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Boolean GtZ<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Boolean LtZ<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Boolean GtEqZ<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Boolean LtEqZ<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Boolean IsNegative<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Sign<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Abs<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Half<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Third<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Quarter<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Fifth<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Sixth<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Seventh<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Eighth<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Ninth<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Tenth<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Sixteenth<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Hundredth<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Thousandth<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Millionth<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Billionth<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Hundred<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Thousand<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Million<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Billion<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Twice<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Thrice<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> SmoothStep<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Pow2<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Pow3<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Pow4<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Numerical<Self> Pow5<Self>(this Numerical<Self> x) => throw new NotImplementedException();
    public static Number Pi => throw new NotImplementedException();
    public static Boolean AlmostZero(this Number x) => throw new NotImplementedException();
    public static Number Lerp<Self>(this Numerical<Self> a, Numerical<Self> b, Number t) => throw new NotImplementedException();
    public static Boolean Between<Self>(this Comparable<Self> self, Comparable<Self> min, Comparable<Self> max) => throw new NotImplementedException();
}
public static partial class AnglesLibrary
{
    public static Angle Radians(this Number x) => throw new NotImplementedException();
    public static Angle Degrees(this Number x) => throw new NotImplementedException();
    public static Angle Turns(this Number x) => throw new NotImplementedException();
}
public static partial class ComparableLibrary
{
    public static Boolean Equals<Self>(this Comparable<Self> a, Comparable<Self> b) => throw new NotImplementedException();
    public static Boolean LessThan<Self>(this Comparable<Self> a, Comparable<Self> b) => throw new NotImplementedException();
    public static Boolean LessThanOrEquals<Self>(this Comparable<Self> a, Comparable<Self> b) => throw new NotImplementedException();
    public static Boolean GreaterThan<Self>(this Comparable<Self> a, Comparable<Self> b) => throw new NotImplementedException();
    public static Boolean GreaterThanOrEquals<Self>(this Comparable<Self> a, Comparable<Self> b) => throw new NotImplementedException();
    public static Value<Self> Between<Self>(this Comparable<Self> v, Comparable<Self> a, Comparable<Self> b) => throw new NotImplementedException();
    public static Comparable<Self> Min<Self>(this Comparable<Self> a, Comparable<Self> b) => throw new NotImplementedException();
    public static Comparable<Self> Max<Self>(this Comparable<Self> a, Comparable<Self> b) => throw new NotImplementedException();
}
