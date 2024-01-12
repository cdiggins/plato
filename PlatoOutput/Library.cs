using System;
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
    public static String Interpolate(this Array xs) => throw new NotImplementedException();
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
    public static Boolean IsEmpty<TValue, TSize>(this Interval<TValue,TSize> x) => throw new NotImplementedException();
    public static TValue Lerp<TValue, TSize>(this Interval<TValue,TSize> x, Number amount) => throw new NotImplementedException();
    public static Number Unlerp<TValue, TSize>(this Interval<TValue,TSize> x, TValue value) => throw new NotImplementedException();
    public static Interval<TValue,TSize> Negate<TValue, TSize>(this Interval<TValue,TSize> x) => throw new NotImplementedException();
    public static Interval<TValue,TSize> Reverse<TValue, TSize>(this Interval<TValue,TSize> x) => throw new NotImplementedException();
    public static TValue Center<TValue, TSize>(this Interval<TValue,TSize> x) => throw new NotImplementedException();
    public static Boolean Contains<TValue, TSize>(this Interval<TValue,TSize> x, TValue value) => throw new NotImplementedException();
    public static Boolean Contains<TValue, TSize>(this Interval<TValue,TSize> x, Interval<TValue,TSize> other) => throw new NotImplementedException();
    public static Boolean Overlaps<TValue, TSize>(this Interval<TValue,TSize> x, Interval<TValue,TSize> y) => throw new NotImplementedException();
    public static Tuple2<Interval<TValue,TSize>,Interval<TValue,TSize>> Split<TValue, TSize>(this Interval<TValue,TSize> x, Number t) => throw new NotImplementedException();
    public static Tuple2<Interval<TValue,TSize>,Interval<TValue,TSize>> Split<TValue, TSize>(this Interval<TValue,TSize> x) => throw new NotImplementedException();
    public static Interval<TValue,TSize> Left<TValue, TSize>(this Interval<TValue,TSize> x, Number t) => throw new NotImplementedException();
    public static Interval<TValue,TSize> Right<TValue, TSize>(this Interval<TValue,TSize> x, Number t) => throw new NotImplementedException();
    public static Interval<TValue,TSize> MoveTo<TValue, TSize>(this Interval<TValue,TSize> x, TValue v) => throw new NotImplementedException();
    public static Interval<TValue,TSize> LeftHalf<TValue, TSize>(this Interval<TValue,TSize> x) => throw new NotImplementedException();
    public static Interval<TValue,TSize> RightHalf<TValue, TSize>(this Interval<TValue,TSize> x) => throw new NotImplementedException();
    public static TSize HalfSize<TValue, TSize>(this Interval<TValue,TSize> x) => throw new NotImplementedException();
    public static Interval<TValue,TSize> Recenter<TValue, TSize>(this Interval<TValue,TSize> x, TValue c) => throw new NotImplementedException();
    public static Interval<TValue,TSize> Clamp<TValue, TSize>(this Interval<TValue,TSize> x, Interval<TValue,TSize> y) => throw new NotImplementedException();
    public static TValue Clamp<TValue, TSize>(this Interval<TValue,TSize> x, TValue value) => throw new NotImplementedException();
    public static Boolean Within<TValue, TSize>(this Interval<TValue,TSize> x, TValue value) => throw new NotImplementedException();
}
public static partial class VectorLibrary
{
    public static TT Aggregate<TSelf, TT>(this Vector<TSelf> v, Function2<Number,TT,TT> f) => throw new NotImplementedException();
    public static Number Sum<TSelf>(this Vector<TSelf> v) => throw new NotImplementedException();
    public static Number SumSquares<TSelf>(this Vector<TSelf> v) => throw new NotImplementedException();
    public static Number MagnitudeSquared<TSelf>(this Vector<TSelf> v) => throw new NotImplementedException();
    public static Number Magnitude<TSelf>(this Vector<TSelf> v) => throw new NotImplementedException();
    public static Number Dot<TSelf>(this Vector<TSelf> v1, Vector<TSelf> v2) => throw new NotImplementedException();
    public static Vector<TSelf> Normal<TSelf>(this Vector<TSelf> v) => throw new NotImplementedException();
    public static Number Average<TSelf>(this Vector<TSelf> v) => throw new NotImplementedException();
}
public static partial class NumericalLibrary
{
    public static Number SquareRoot(this Number x) => throw new NotImplementedException();
    public static Numerical<TSelf> Square<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Number Clamp(this Number x) => throw new NotImplementedException();
    public static Numerical<TSelf> PlusOne<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> MinusOne<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> FromOne<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Boolean IsPositive<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Boolean GtZ<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Boolean LtZ<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Boolean GtEqZ<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Boolean LtEqZ<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Boolean IsNegative<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Sign<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Abs<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Half<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Third<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Quarter<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Fifth<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Sixth<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Seventh<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Eighth<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Ninth<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Tenth<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Sixteenth<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Hundredth<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Thousandth<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Millionth<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Billionth<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Hundred<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Thousand<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Million<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Billion<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Twice<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Thrice<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> SmoothStep<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Pow2<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Pow3<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Pow4<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Numerical<TSelf> Pow5<TSelf>(this Numerical<TSelf> x) => throw new NotImplementedException();
    public static Number Pi => throw new NotImplementedException();
    public static Boolean AlmostZero(this Number x) => throw new NotImplementedException();
    public static Number Lerp<TSelf>(this Numerical<TSelf> a, Numerical<TSelf> b, Number t) => throw new NotImplementedException();
    public static Boolean Between<TSelf>(this Comparable<TSelf> self, Comparable<TSelf> min, Comparable<TSelf> max) => throw new NotImplementedException();
}
public static partial class AnglesLibrary
{
    public static Angle Radians(this Number x) => throw new NotImplementedException();
    public static Angle Degrees(this Number x) => throw new NotImplementedException();
    public static Angle Turns(this Number x) => throw new NotImplementedException();
}
public static partial class ComparableLibrary
{
    public static Boolean Equals<TSelf>(this Comparable<TSelf> a, Comparable<TSelf> b) => throw new NotImplementedException();
    public static Boolean LessThan<TSelf>(this Comparable<TSelf> a, Comparable<TSelf> b) => throw new NotImplementedException();
    public static Boolean LessThanOrEquals<TSelf>(this Comparable<TSelf> a, Comparable<TSelf> b) => throw new NotImplementedException();
    public static Boolean GreaterThan<TSelf>(this Comparable<TSelf> a, Comparable<TSelf> b) => throw new NotImplementedException();
    public static Boolean GreaterThanOrEquals<TSelf>(this Comparable<TSelf> a, Comparable<TSelf> b) => throw new NotImplementedException();
    public static Value<TSelf> Between<TSelf>(this Comparable<TSelf> v, Comparable<TSelf> a, Comparable<TSelf> b) => throw new NotImplementedException();
    public static Comparable<TSelf> Min<TSelf>(this Comparable<TSelf> a, Comparable<TSelf> b) => throw new NotImplementedException();
    public static Comparable<TSelf> Max<TSelf>(this Comparable<TSelf> a, Comparable<TSelf> b) => throw new NotImplementedException();
}
