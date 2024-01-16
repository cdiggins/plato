public interface Any
{
    Array<String>  FieldNames { get; }
    Array<Dynamic>  FieldValues { get; }
}
public interface Value<Self>: Any, Equatable<Self>
{
    Self  Zero { get; }
    Self  One { get; }
    Self  MinValue { get; }
    Self  MaxValue { get; }
}
public interface Array<T>
{
    Integer  Count { get; }
    T  At(Integer n);
}
public interface Vector<Self>: Array<Number>, Numerical<Self>, Magnitudinal<Self>, Equatable<Self>, Interpolatable<Self>
{
}
public interface Coordinate<Self>: Interpolatable<Self>, Comparable<Self>
{
}
public interface Measure<Self>: Value<Self>, ScalarArithmetic<Self>, Comparable<Self>, Magnitudinal<Self>, Interpolatable<Self>
{
    Number  Value { get; }
}
public interface Numerical<Self>: Value<Self>, Arithmetic<Self>, Equatable<Self>, Comparable<Self>, Magnitudinal<Self>, Interpolatable<Self>
{
}
public interface Magnitudinal<Self>: Comparable<Self>
{
    Number  Magnitude { get; }
}
public interface Comparable<Self>: Equatable<Self>
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
    Self  Multiply(Self other);
    Self  Divide(Self other);
    Self  Modulo(Self other);
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
public interface Interval<Self, TValue, TSize>: Equatable<Self>
{
    TValue  Min { get; }
    TValue  Max { get; }
    TSize  Size { get; }
}
public interface Interpolatable<Self>
{
    Self  Lerp(Self b, Number amount);
    Number  Unlerp(Self a, Self b);
}
