/*
public interface Any
{
    Array<String>  FieldNames { get; }
    Array<Dynamic>  FieldValues { get; }
    String  TypeName { get; }
}
public interface Value: Any, Equatable
{
    Value Zero { get; }
    Value One { get; }
    Value MinValue { get; }
    Value MaxValue { get; }
}
public interface Numerical: Value, Arithmetic, Magnitudinal, Betweenable, Equatable
{
}
public interface Real: Magnitudinal, Interpolatable, Betweenable, Equatable, Comparable, ScalarArithmetic
{
    Number  Value { get; }
}
public interface IArray<T>
{
    Integer  Count { get; }
    T  At(Integer n);
}
public interface Vector: IArray<Number>, Numerical, ScalarArithmetic, Interpolatable
{
}
public interface Coordinate: Value, Interpolatable, Betweenable
{
}
public interface Measure: Real
{
}
public interface WholeNumber: Numerical, Comparable
{
}
public interface Magnitudinal
{
    Number  Magnitude { get; }
}
public interface Comparable: Value, Equatable
{
    Integer  Compare(Value y);
}
public interface Equatable
{
    Boolean  Equals(Value b);
    Boolean  NotEquals(Value b);
}
public interface Arithmetic: AdditiveArithmetic<Self, Self>, MultiplicativeArithmetic<Self, Self>, AdditiveInverse, MultiplicativeInverse
{
}
public interface AdditiveInverse
{
    Self  Negative { get; }
}
public interface MultiplicativeInverse
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
public interface ScalarArithmetic: AdditiveArithmetic<Self, Number>, MultiplicativeArithmetic<Self, Number>
{
}
public interface BooleanOperations
{
    Self  And(Self b);
    Self  Or(Self b);
    Self  Not { get; }
}
public interface Interval<Self, TValue, TSize>: Equatable, Value
{
    TValue  Min { get; }
    TValue  Max { get; }
    TSize  Size { get; }
}
public interface Interpolatable
{
    Self  Lerp(Self b, Number amount);
}
public interface Betweenable
{
    Boolean  Between(Self a, Self b);
    Self  Clamp(Self a, Self b);
}
*/