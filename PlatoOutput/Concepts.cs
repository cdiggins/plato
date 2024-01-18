/*
public interface Any
{
    Array<String>  FieldNames { get; }
    Array<Dynamic>  FieldValues { get; }
    String  TypeName { get; }
}
public interface Value<Self>: Any, Equatable<Self>
{
    Self  Zero { get; }
    Self  One { get; }
    Self  MinValue { get; }
    Self  MaxValue { get; }
}
public interface Numerical<Self>: Value<Self>, Arithmetic<Self>, Magnitudinal, Betweenable<Self>, Equatable<Self>
{
}
public interface Real<Self>: Magnitudinal, Interpolatable<Self>, Betweenable<Self>, Equatable<Self>, Comparable<Self>, ScalarArithmetic<Self>
{
    Number  Value { get; }
}
public interface Array<T>
{
    Integer  Count { get; }
    T  At(Integer n);
}
public interface Vector<Self>: Array<Number>, Numerical<Self>, ScalarArithmetic<Self>, Interpolatable<Self>
{
}
public interface Coordinate<Self>: Value<Self>, Interpolatable<Self>, Betweenable<Self>
{
}
public interface Measure<Self>: Real<Self>
{
}
public interface WholeNumber<Self>: Numerical<Self>, Comparable<Self>
{
}
public interface Magnitudinal
{
    Number  Magnitude { get; }
}
public interface Comparable<Self>: Value<Self>, Equatable<Self>
{
    Integer  Compare(Self y);
}
public interface Equatable<Self>
{
    Boolean  Equals(Self b);
    Boolean  NotEquals(Self b);
}
public interface Arithmetic<Self>: AdditiveArithmetic<Self, Self>, MultiplicativeArithmetic<Self, Self>, AdditiveInverse<Self>, MultiplicativeInverse<Self>
{
}
public interface AdditiveInverse<Self>
{
    Self  Negative { get; }
}
public interface MultiplicativeInverse<Self>
{
    Self  Reciprocal { get; }
}
public interface AdditiveArithmetic<Self, T>
{
    Self  Add(T other);
    Self  Subtract(T other);
}
public interface MultiplicativeArithmetic<Self, T>
{
    Self  Multiply(T other);
    Self  Divide(T other);
    Self  Modulo(T other);
}
public interface ScalarArithmetic<Self>: AdditiveArithmetic<Self, Number>, MultiplicativeArithmetic<Self, Number>
{
}
public interface BooleanOperations<Self>
{
    Self  And(Self b);
    Self  Or(Self b);
    Self  Not { get; }
}
public interface Interval<Self, TValue, TSize>: Equatable<Self>, Value<Self>
{
    TValue  Min { get; }
    TValue  Max { get; }
    TSize  Size { get; }
}
public interface Interpolatable<Self>
{
    Self  Lerp(Self b, Number amount);
}
public interface Betweenable<Self>
{
    Boolean  Between(Self a, Self b);
    Self  Clamp(Self a, Self b);
}
*/
