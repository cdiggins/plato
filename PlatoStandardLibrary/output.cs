public static partial class Extensions
{
}
public interface Any<Self>
    where Self : Any<Self>
{
    Array FieldNames(Any self);
    Array FieldValues(Any x);
    Array FieldTypes(Any x);
    Type TypeOf(Any self);
}
public static partial class Extensions
{
}
public interface Value<Self>: Any<Self>
    where Self : Value<Self>
{
    Value Default(Value self);
}
public static partial class Extensions
{
}
public interface Array<Self>: Any<Self>
    where Self : Array<Self>
{
    Integer Count(Array xs);
    Any At(Array xs, Integer n);
}
public static partial class Extensions
{
}
public interface Vector<Self>: Array<Self>, Numerical<Self>, ScalarArithmetic<Self>
    where Self : Vector<Self>
{
}
public static partial class Extensions
{
    public static Integer Count<Self, T>(this Vector v) where Self: Vector<Self, T>
    {
        return Count(FieldTypes(Self));
    }
    public static Numerical At<Self, T>(this Vector v, Integer n) where Self: Vector<Self, T>
    {
        return At(FieldValues(v), n);
    }
}
public interface Measure<Self>: Value<Self>, ScalarArithmetic<Self>, Equatable<Self>, Comparable<Self>, Magnitudinal<Self>
    where Self : Measure<Self>
{
}
public static partial class Extensions
{
    public static Number Value<Self>(this Measure x) where Self: Measure<Self>
    {
        return At(FieldValues(x), 0);
    }
}
public interface Numerical<Self>: Value<Self>, Arithmetic<Self>, Equatable<Self>, Comparable<Self>, Magnitudinal<Self>
    where Self : Numerical<Self>
{
    Numerical Zero(Numerical x);
    Numerical One(Numerical x);
    Numerical MinValue(Numerical x);
    Numerical MaxValue(Numerical x);
}
public static partial class Extensions
{
}
public interface Magnitudinal<Self>: Value<Self>
    where Self : Magnitudinal<Self>
{
}
public static partial class Extensions
{
    public static Number Magnitude<Self>(this Magnitudinal x) where Self: Magnitudinal<Self>
    {
        return SquareRoot(Sum(Square(FieldValues(x))));
    }
}
public interface Comparable<Self>: Value<Self>
    where Self : Comparable<Self>
{
    Integer Compare(Comparable x);
}
public static partial class Extensions
{
}
public interface Equatable<Self>: Value<Self>
    where Self : Equatable<Self>
{
}
public static partial class Extensions
{
    public static Boolean Equals<Self>(this Equatable a, Equatable b) where Self: Equatable<Self>
    {
        return All(Equals(FieldValues(a), FieldValues(b)));
    }
}
public interface Arithmetic<Self>: Value<Self>
    where Self : Arithmetic<Self>
{
}
public static partial class Extensions
{
    public static Arithmetic Add<Self>(this Arithmetic self, Arithmetic other) where Self: Arithmetic<Self>
    {
        return Add(FieldValues(self), FieldValues(other));
    }
    public static Arithmetic Negative<Self>(this Arithmetic self) where Self: Arithmetic<Self>
    {
        return Negative(FieldValues(self));
    }
    public static Arithmetic Reciprocal<Self>(this Arithmetic self) where Self: Arithmetic<Self>
    {
        return Reciprocal(FieldValues(self));
    }
    public static Arithmetic Multiply<Self>(this Arithmetic self, Arithmetic other) where Self: Arithmetic<Self>
    {
        return Add(FieldValues(self), FieldValues(other));
    }
    public static Arithmetic Divide<Self>(this Arithmetic self, Arithmetic other) where Self: Arithmetic<Self>
    {
        return Divide(FieldValues(self), FieldValues(other));
    }
    public static Arithmetic Modulo<Self>(this Arithmetic self, Arithmetic other) where Self: Arithmetic<Self>
    {
        return Modulo(FieldValues(self), FieldValues(other));
    }
}
public interface ScalarArithmetic<Self>: Value<Self>
    where Self : ScalarArithmetic<Self>
{
}
public static partial class Extensions
{
    public static ScalarArithmetic Add<Self>(this ScalarArithmetic self, Number scalar) where Self: ScalarArithmetic<Self>
    {
        return Add(FieldValues(self), scalar);
    }
    public static ScalarArithmetic Subtract<Self>(this ScalarArithmetic self, Number scalar) where Self: ScalarArithmetic<Self>
    {
        return Add(self, Negative(scalar));
    }
    public static ScalarArithmetic Multiply<Self>(this ScalarArithmetic self, Number scalar) where Self: ScalarArithmetic<Self>
    {
        return Multiply(FieldValues(self), scalar);
    }
    public static ScalarArithmetic Divide<Self>(this ScalarArithmetic self, Number scalar) where Self: ScalarArithmetic<Self>
    {
        return Multiply(self, Reciprocal(scalar));
    }
    public static ScalarArithmetic Modulo<Self>(this ScalarArithmetic self, Number scalar) where Self: ScalarArithmetic<Self>
    {
        return Modulo(FieldValues(self), scalar);
    }
}
public interface BooleanOperations<Self>
    where Self : BooleanOperations<Self>
{
}
public static partial class Extensions
{
    public static BooleanOperations And<Self>(this BooleanOperations a, BooleanOperations b) where Self: BooleanOperations<Self>
    {
        return And(FieldValues(a), FieldValues(b));
    }
    public static BooleanOperations Or<Self>(this BooleanOperations a, BooleanOperations b) where Self: BooleanOperations<Self>
    {
        return Or(FieldValues(a), FieldValues(b));
    }
    public static BooleanOperations Not<Self>(this BooleanOperations a) where Self: BooleanOperations<Self>
    {
        return Not(FieldValues(a));
    }
}
public interface Interval<Self>: Vector<Self>
    where Self : Interval<Self>
{
    Numerical Min(Interval x);
    Numerical Max(Interval x);
}
public static partial class Extensions
{
}
public class Number: Numerical<Number>
{
    public Number() => () = ();
    public static Number New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
}
public class Integer: Numerical<Integer>
{
    public Integer() => () = ();
    public static Integer New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
}
public class String: Value<String>, Array<String>
{
    public String() => () = ();
    public static String New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
}
public class Boolean: Value<Boolean>, BooleanOperations<Boolean>
{
    public Boolean() => () = ();
    public static Boolean New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static BooleanOperations operator &&(BooleanOperations a, BooleanOperations b) => Extensions.And(a, b);
    public static BooleanOperations operator ||(BooleanOperations a, BooleanOperations b) => Extensions.Or(a, b);
    public static BooleanOperations operator !(BooleanOperations a) => Extensions.Not(a);
}
public class Type: Value<Type>
{
    public Type(String _Name) => (Name) = (_Name);
    public static Type New(String _Name) => new(_Name);
    public static implicit operator String(Type self) => self.Name;
    public static implicit operator Type(String value) => new String(value);
    public string[] FieldNames() => new[] { "Name" };
    public object[] FieldValues() => new[] { Name };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public String Name { get; }
}
public class Character: Value<Character>
{
    public Character() => () = ();
    public static Character New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
}
public class Count: Numerical<Count>
{
    public Count(Integer _Value) => (Value) = (_Value);
    public static Count New(Integer _Value) => new(_Value);
    public static implicit operator Integer(Count self) => self.Value;
    public static implicit operator Count(Integer value) => new Integer(value);
    public string[] FieldNames() => new[] { "Value" };
    public object[] FieldValues() => new[] { Value };
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Integer Value { get; }
}
public class Index: Value<Index>
{
    public Index(Integer _Value) => (Value) = (_Value);
    public static Index New(Integer _Value) => new(_Value);
    public static implicit operator Integer(Index self) => self.Value;
    public static implicit operator Index(Integer value) => new Integer(value);
    public string[] FieldNames() => new[] { "Value" };
    public object[] FieldValues() => new[] { Value };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Integer Value { get; }
}
public class Unit: Numerical<Unit>
{
    public Unit(Number _Value) => (Value) = (_Value);
    public static Unit New(Number _Value) => new(_Value);
    public static implicit operator Number(Unit self) => self.Value;
    public static implicit operator Unit(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Value" };
    public object[] FieldValues() => new[] { Value };
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Value { get; }
}
public class Percent: Numerical<Percent>
{
    public Percent(Number _Value) => (Value) = (_Value);
    public static Percent New(Number _Value) => new(_Value);
    public static implicit operator Number(Percent self) => self.Value;
    public static implicit operator Percent(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Value" };
    public object[] FieldValues() => new[] { Value };
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Value { get; }
}
public class Quaternion: Value<Quaternion>
{
    public Quaternion(Number _X, Number _Y, Number _Z, Number _W) => (X, Y, Z, W) = (_X, _Y, _Z, _W);
    public static Quaternion New(Number _X, Number _Y, Number _Z, Number _W) => new(_X, _Y, _Z, _W);
    public static implicit operator (Number, Number, Number, Number)(Quaternion self) => (self.X, self.Y, self.Z, self.W);
    public static implicit operator Quaternion((Number, Number, Number, Number) value) => new Quaternion(value.Item1, value.Item2, value.Item3, value.Item4);
    public string[] FieldNames() => new[] { "X", "Y", "Z", "W" };
    public object[] FieldValues() => new[] { X, Y, Z, W };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number X { get; }
    public Number Y { get; }
    public Number Z { get; }
    public Number W { get; }
}
public class Unit2D: Value<Unit2D>
{
    public Unit2D(Unit _X, Unit _Y) => (X, Y) = (_X, _Y);
    public static Unit2D New(Unit _X, Unit _Y) => new(_X, _Y);
    public static implicit operator (Unit, Unit)(Unit2D self) => (self.X, self.Y);
    public static implicit operator Unit2D((Unit, Unit) value) => new Unit2D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "X", "Y" };
    public object[] FieldValues() => new[] { X, Y };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Unit X { get; }
    public Unit Y { get; }
}
public class Unit3D: Value<Unit3D>
{
    public Unit3D(Unit _X, Unit _Y, Unit _Z) => (X, Y, Z) = (_X, _Y, _Z);
    public static Unit3D New(Unit _X, Unit _Y, Unit _Z) => new(_X, _Y, _Z);
    public static implicit operator (Unit, Unit, Unit)(Unit3D self) => (self.X, self.Y, self.Z);
    public static implicit operator Unit3D((Unit, Unit, Unit) value) => new Unit3D(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "X", "Y", "Z" };
    public object[] FieldValues() => new[] { X, Y, Z };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Unit X { get; }
    public Unit Y { get; }
    public Unit Z { get; }
}
public class Direction3D: Value<Direction3D>
{
    public Direction3D(Unit3D _Value) => (Value) = (_Value);
    public static Direction3D New(Unit3D _Value) => new(_Value);
    public static implicit operator Unit3D(Direction3D self) => self.Value;
    public static implicit operator Direction3D(Unit3D value) => new Unit3D(value);
    public string[] FieldNames() => new[] { "Value" };
    public object[] FieldValues() => new[] { Value };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Unit3D Value { get; }
}
public class AxisAngle: Value<AxisAngle>
{
    public AxisAngle(Unit3D _Axis, Angle _Angle) => (Axis, Angle) = (_Axis, _Angle);
    public static AxisAngle New(Unit3D _Axis, Angle _Angle) => new(_Axis, _Angle);
    public static implicit operator (Unit3D, Angle)(AxisAngle self) => (self.Axis, self.Angle);
    public static implicit operator AxisAngle((Unit3D, Angle) value) => new AxisAngle(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Axis", "Angle" };
    public object[] FieldValues() => new[] { Axis, Angle };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Unit3D Axis { get; }
    public Angle Angle { get; }
}
public class EulerAngles: Value<EulerAngles>
{
    public EulerAngles(Angle _Yaw, Angle _Pitch, Angle _Roll) => (Yaw, Pitch, Roll) = (_Yaw, _Pitch, _Roll);
    public static EulerAngles New(Angle _Yaw, Angle _Pitch, Angle _Roll) => new(_Yaw, _Pitch, _Roll);
    public static implicit operator (Angle, Angle, Angle)(EulerAngles self) => (self.Yaw, self.Pitch, self.Roll);
    public static implicit operator EulerAngles((Angle, Angle, Angle) value) => new EulerAngles(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Yaw", "Pitch", "Roll" };
    public object[] FieldValues() => new[] { Yaw, Pitch, Roll };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Angle Yaw { get; }
    public Angle Pitch { get; }
    public Angle Roll { get; }
}
public class Rotation3D: Value<Rotation3D>
{
    public Rotation3D(Quaternion _Quaternion) => (Quaternion) = (_Quaternion);
    public static Rotation3D New(Quaternion _Quaternion) => new(_Quaternion);
    public static implicit operator Quaternion(Rotation3D self) => self.Quaternion;
    public static implicit operator Rotation3D(Quaternion value) => new Quaternion(value);
    public string[] FieldNames() => new[] { "Quaternion" };
    public object[] FieldValues() => new[] { Quaternion };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Quaternion Quaternion { get; }
}
public class Vector2D: Vector<Vector2D>
{
    public Vector2D(Number _X, Number _Y) => (X, Y) = (_X, _Y);
    public static Vector2D New(Number _X, Number _Y) => new(_X, _Y);
    public static implicit operator (Number, Number)(Vector2D self) => (self.X, self.Y);
    public static implicit operator Vector2D((Number, Number) value) => new Vector2D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "X", "Y" };
    public object[] FieldValues() => new[] { X, Y };
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number X { get; }
    public Number Y { get; }
}
public class Vector3D: Vector<Vector3D>
{
    public Vector3D(Number _X, Number _Y, Number _Z) => (X, Y, Z) = (_X, _Y, _Z);
    public static Vector3D New(Number _X, Number _Y, Number _Z) => new(_X, _Y, _Z);
    public static implicit operator (Number, Number, Number)(Vector3D self) => (self.X, self.Y, self.Z);
    public static implicit operator Vector3D((Number, Number, Number) value) => new Vector3D(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "X", "Y", "Z" };
    public object[] FieldValues() => new[] { X, Y, Z };
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number X { get; }
    public Number Y { get; }
    public Number Z { get; }
}
public class Vector4D: Vector<Vector4D>
{
    public Vector4D(Number _X, Number _Y, Number _Z, Number _W) => (X, Y, Z, W) = (_X, _Y, _Z, _W);
    public static Vector4D New(Number _X, Number _Y, Number _Z, Number _W) => new(_X, _Y, _Z, _W);
    public static implicit operator (Number, Number, Number, Number)(Vector4D self) => (self.X, self.Y, self.Z, self.W);
    public static implicit operator Vector4D((Number, Number, Number, Number) value) => new Vector4D(value.Item1, value.Item2, value.Item3, value.Item4);
    public string[] FieldNames() => new[] { "X", "Y", "Z", "W" };
    public object[] FieldValues() => new[] { X, Y, Z, W };
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number X { get; }
    public Number Y { get; }
    public Number Z { get; }
    public Number W { get; }
}
public class Orientation3D: Value<Orientation3D>
{
    public Orientation3D(Rotation3D _Value) => (Value) = (_Value);
    public static Orientation3D New(Rotation3D _Value) => new(_Value);
    public static implicit operator Rotation3D(Orientation3D self) => self.Value;
    public static implicit operator Orientation3D(Rotation3D value) => new Rotation3D(value);
    public string[] FieldNames() => new[] { "Value" };
    public object[] FieldValues() => new[] { Value };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Rotation3D Value { get; }
}
public class Pose2D: Value<Pose2D>
{
    public Pose2D(Vector3D _Position, Orientation3D _Orientation) => (Position, Orientation) = (_Position, _Orientation);
    public static Pose2D New(Vector3D _Position, Orientation3D _Orientation) => new(_Position, _Orientation);
    public static implicit operator (Vector3D, Orientation3D)(Pose2D self) => (self.Position, self.Orientation);
    public static implicit operator Pose2D((Vector3D, Orientation3D) value) => new Pose2D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Position", "Orientation" };
    public object[] FieldValues() => new[] { Position, Orientation };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector3D Position { get; }
    public Orientation3D Orientation { get; }
}
public class Pose3D: Value<Pose3D>
{
    public Pose3D(Vector3D _Position, Orientation3D _Orientation) => (Position, Orientation) = (_Position, _Orientation);
    public static Pose3D New(Vector3D _Position, Orientation3D _Orientation) => new(_Position, _Orientation);
    public static implicit operator (Vector3D, Orientation3D)(Pose3D self) => (self.Position, self.Orientation);
    public static implicit operator Pose3D((Vector3D, Orientation3D) value) => new Pose3D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Position", "Orientation" };
    public object[] FieldValues() => new[] { Position, Orientation };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector3D Position { get; }
    public Orientation3D Orientation { get; }
}
public class Transform3D: Value<Transform3D>
{
    public Transform3D(Vector3D _Translation, Rotation3D _Rotation, Vector3D _Scale) => (Translation, Rotation, Scale) = (_Translation, _Rotation, _Scale);
    public static Transform3D New(Vector3D _Translation, Rotation3D _Rotation, Vector3D _Scale) => new(_Translation, _Rotation, _Scale);
    public static implicit operator (Vector3D, Rotation3D, Vector3D)(Transform3D self) => (self.Translation, self.Rotation, self.Scale);
    public static implicit operator Transform3D((Vector3D, Rotation3D, Vector3D) value) => new Transform3D(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Translation", "Rotation", "Scale" };
    public object[] FieldValues() => new[] { Translation, Rotation, Scale };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector3D Translation { get; }
    public Rotation3D Rotation { get; }
    public Vector3D Scale { get; }
}
public class Transform2D: Value<Transform2D>
{
    public Transform2D(Vector2D _Translation, Angle _Rotation, Vector2D _Scale) => (Translation, Rotation, Scale) = (_Translation, _Rotation, _Scale);
    public static Transform2D New(Vector2D _Translation, Angle _Rotation, Vector2D _Scale) => new(_Translation, _Rotation, _Scale);
    public static implicit operator (Vector2D, Angle, Vector2D)(Transform2D self) => (self.Translation, self.Rotation, self.Scale);
    public static implicit operator Transform2D((Vector2D, Angle, Vector2D) value) => new Transform2D(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Translation", "Rotation", "Scale" };
    public object[] FieldValues() => new[] { Translation, Rotation, Scale };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector2D Translation { get; }
    public Angle Rotation { get; }
    public Vector2D Scale { get; }
}
public class AlignedBox2D: Interval<AlignedBox2D>
{
    public AlignedBox2D(Vector2D _A, Vector2D _B) => (A, B) = (_A, _B);
    public static AlignedBox2D New(Vector2D _A, Vector2D _B) => new(_A, _B);
    public static implicit operator (Vector2D, Vector2D)(AlignedBox2D self) => (self.A, self.B);
    public static implicit operator AlignedBox2D((Vector2D, Vector2D) value) => new AlignedBox2D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "A", "B" };
    public object[] FieldValues() => new[] { A, B };
    public static Numerical Min(Interval x) => Extensions.Min(x);
    public static Numerical Max(Interval x) => Extensions.Max(x);
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector2D A { get; }
    public Vector2D B { get; }
}
public class AlignedBox3D: Interval<AlignedBox3D>
{
    public AlignedBox3D(Vector3D _A, Vector3D _B) => (A, B) = (_A, _B);
    public static AlignedBox3D New(Vector3D _A, Vector3D _B) => new(_A, _B);
    public static implicit operator (Vector3D, Vector3D)(AlignedBox3D self) => (self.A, self.B);
    public static implicit operator AlignedBox3D((Vector3D, Vector3D) value) => new AlignedBox3D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "A", "B" };
    public object[] FieldValues() => new[] { A, B };
    public static Numerical Min(Interval x) => Extensions.Min(x);
    public static Numerical Max(Interval x) => Extensions.Max(x);
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector3D A { get; }
    public Vector3D B { get; }
}
public class Complex: Vector<Complex>
{
    public Complex(Number _Real, Number _Imaginary) => (Real, Imaginary) = (_Real, _Imaginary);
    public static Complex New(Number _Real, Number _Imaginary) => new(_Real, _Imaginary);
    public static implicit operator (Number, Number)(Complex self) => (self.Real, self.Imaginary);
    public static implicit operator Complex((Number, Number) value) => new Complex(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Real", "Imaginary" };
    public object[] FieldValues() => new[] { Real, Imaginary };
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Real { get; }
    public Number Imaginary { get; }
}
public class Ray3D: Value<Ray3D>
{
    public Ray3D(Vector3D _Direction, Point3D _Position) => (Direction, Position) = (_Direction, _Position);
    public static Ray3D New(Vector3D _Direction, Point3D _Position) => new(_Direction, _Position);
    public static implicit operator (Vector3D, Point3D)(Ray3D self) => (self.Direction, self.Position);
    public static implicit operator Ray3D((Vector3D, Point3D) value) => new Ray3D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Direction", "Position" };
    public object[] FieldValues() => new[] { Direction, Position };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector3D Direction { get; }
    public Point3D Position { get; }
}
public class Ray2D: Value<Ray2D>
{
    public Ray2D(Vector2D _Direction, Point2D _Position) => (Direction, Position) = (_Direction, _Position);
    public static Ray2D New(Vector2D _Direction, Point2D _Position) => new(_Direction, _Position);
    public static implicit operator (Vector2D, Point2D)(Ray2D self) => (self.Direction, self.Position);
    public static implicit operator Ray2D((Vector2D, Point2D) value) => new Ray2D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Direction", "Position" };
    public object[] FieldValues() => new[] { Direction, Position };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector2D Direction { get; }
    public Point2D Position { get; }
}
public class Sphere: Value<Sphere>
{
    public Sphere(Point3D _Center, Number _Radius) => (Center, Radius) = (_Center, _Radius);
    public static Sphere New(Point3D _Center, Number _Radius) => new(_Center, _Radius);
    public static implicit operator (Point3D, Number)(Sphere self) => (self.Center, self.Radius);
    public static implicit operator Sphere((Point3D, Number) value) => new Sphere(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Center", "Radius" };
    public object[] FieldValues() => new[] { Center, Radius };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point3D Center { get; }
    public Number Radius { get; }
}
public class Plane: Value<Plane>
{
    public Plane(Unit3D _Normal, Number _D) => (Normal, D) = (_Normal, _D);
    public static Plane New(Unit3D _Normal, Number _D) => new(_Normal, _D);
    public static implicit operator (Unit3D, Number)(Plane self) => (self.Normal, self.D);
    public static implicit operator Plane((Unit3D, Number) value) => new Plane(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Normal", "D" };
    public object[] FieldValues() => new[] { Normal, D };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Unit3D Normal { get; }
    public Number D { get; }
}
public class Triangle3D: Value<Triangle3D>
{
    public Triangle3D(Point3D _A, Point3D _B, Point3D _C) => (A, B, C) = (_A, _B, _C);
    public static Triangle3D New(Point3D _A, Point3D _B, Point3D _C) => new(_A, _B, _C);
    public static implicit operator (Point3D, Point3D, Point3D)(Triangle3D self) => (self.A, self.B, self.C);
    public static implicit operator Triangle3D((Point3D, Point3D, Point3D) value) => new Triangle3D(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "A", "B", "C" };
    public object[] FieldValues() => new[] { A, B, C };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point3D A { get; }
    public Point3D B { get; }
    public Point3D C { get; }
}
public class Triangle2D: Value<Triangle2D>
{
    public Triangle2D(Point2D _A, Point2D _B, Point2D _C) => (A, B, C) = (_A, _B, _C);
    public static Triangle2D New(Point2D _A, Point2D _B, Point2D _C) => new(_A, _B, _C);
    public static implicit operator (Point2D, Point2D, Point2D)(Triangle2D self) => (self.A, self.B, self.C);
    public static implicit operator Triangle2D((Point2D, Point2D, Point2D) value) => new Triangle2D(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "A", "B", "C" };
    public object[] FieldValues() => new[] { A, B, C };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point2D A { get; }
    public Point2D B { get; }
    public Point2D C { get; }
}
public class Quad3D: Value<Quad3D>
{
    public Quad3D(Point3D _A, Point3D _B, Point3D _C, Point3D _D) => (A, B, C, D) = (_A, _B, _C, _D);
    public static Quad3D New(Point3D _A, Point3D _B, Point3D _C, Point3D _D) => new(_A, _B, _C, _D);
    public static implicit operator (Point3D, Point3D, Point3D, Point3D)(Quad3D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator Quad3D((Point3D, Point3D, Point3D, Point3D) value) => new Quad3D(value.Item1, value.Item2, value.Item3, value.Item4);
    public string[] FieldNames() => new[] { "A", "B", "C", "D" };
    public object[] FieldValues() => new[] { A, B, C, D };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point3D A { get; }
    public Point3D B { get; }
    public Point3D C { get; }
    public Point3D D { get; }
}
public class Quad2D: Value<Quad2D>
{
    public Quad2D(Point2D _A, Point2D _B, Point2D _C, Point2D _D) => (A, B, C, D) = (_A, _B, _C, _D);
    public static Quad2D New(Point2D _A, Point2D _B, Point2D _C, Point2D _D) => new(_A, _B, _C, _D);
    public static implicit operator (Point2D, Point2D, Point2D, Point2D)(Quad2D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator Quad2D((Point2D, Point2D, Point2D, Point2D) value) => new Quad2D(value.Item1, value.Item2, value.Item3, value.Item4);
    public string[] FieldNames() => new[] { "A", "B", "C", "D" };
    public object[] FieldValues() => new[] { A, B, C, D };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point2D A { get; }
    public Point2D B { get; }
    public Point2D C { get; }
    public Point2D D { get; }
}
public class Point3D: Value<Point3D>
{
    public Point3D(Vector3D _Value) => (Value) = (_Value);
    public static Point3D New(Vector3D _Value) => new(_Value);
    public static implicit operator Vector3D(Point3D self) => self.Value;
    public static implicit operator Point3D(Vector3D value) => new Vector3D(value);
    public string[] FieldNames() => new[] { "Value" };
    public object[] FieldValues() => new[] { Value };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector3D Value { get; }
}
public class Point2D: Value<Point2D>
{
    public Point2D(Vector2D _Value) => (Value) = (_Value);
    public static Point2D New(Vector2D _Value) => new(_Value);
    public static implicit operator Vector2D(Point2D self) => self.Value;
    public static implicit operator Point2D(Vector2D value) => new Vector2D(value);
    public string[] FieldNames() => new[] { "Value" };
    public object[] FieldValues() => new[] { Value };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector2D Value { get; }
}
public class Line3D: Interval<Line3D>
{
    public Line3D(Point3D _A, Point3D _B) => (A, B) = (_A, _B);
    public static Line3D New(Point3D _A, Point3D _B) => new(_A, _B);
    public static implicit operator (Point3D, Point3D)(Line3D self) => (self.A, self.B);
    public static implicit operator Line3D((Point3D, Point3D) value) => new Line3D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "A", "B" };
    public object[] FieldValues() => new[] { A, B };
    public static Numerical Min(Interval x) => Extensions.Min(x);
    public static Numerical Max(Interval x) => Extensions.Max(x);
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point3D A { get; }
    public Point3D B { get; }
}
public class Line2D: Interval<Line2D>
{
    public Line2D(Point2D _A, Point2D _B) => (A, B) = (_A, _B);
    public static Line2D New(Point2D _A, Point2D _B) => new(_A, _B);
    public static implicit operator (Point2D, Point2D)(Line2D self) => (self.A, self.B);
    public static implicit operator Line2D((Point2D, Point2D) value) => new Line2D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "A", "B" };
    public object[] FieldValues() => new[] { A, B };
    public static Numerical Min(Interval x) => Extensions.Min(x);
    public static Numerical Max(Interval x) => Extensions.Max(x);
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point2D A { get; }
    public Point2D B { get; }
}
public class Color: Value<Color>
{
    public Color(Unit _R, Unit _G, Unit _B, Unit _A) => (R, G, B, A) = (_R, _G, _B, _A);
    public static Color New(Unit _R, Unit _G, Unit _B, Unit _A) => new(_R, _G, _B, _A);
    public static implicit operator (Unit, Unit, Unit, Unit)(Color self) => (self.R, self.G, self.B, self.A);
    public static implicit operator Color((Unit, Unit, Unit, Unit) value) => new Color(value.Item1, value.Item2, value.Item3, value.Item4);
    public string[] FieldNames() => new[] { "R", "G", "B", "A" };
    public object[] FieldValues() => new[] { R, G, B, A };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Unit R { get; }
    public Unit G { get; }
    public Unit B { get; }
    public Unit A { get; }
}
public class ColorLUV: Value<ColorLUV>
{
    public ColorLUV(Percent _Lightness, Unit _U, Unit _V) => (Lightness, U, V) = (_Lightness, _U, _V);
    public static ColorLUV New(Percent _Lightness, Unit _U, Unit _V) => new(_Lightness, _U, _V);
    public static implicit operator (Percent, Unit, Unit)(ColorLUV self) => (self.Lightness, self.U, self.V);
    public static implicit operator ColorLUV((Percent, Unit, Unit) value) => new ColorLUV(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Lightness", "U", "V" };
    public object[] FieldValues() => new[] { Lightness, U, V };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Percent Lightness { get; }
    public Unit U { get; }
    public Unit V { get; }
}
public class ColorLAB: Value<ColorLAB>
{
    public ColorLAB(Percent _Lightness, Integer _A, Integer _B) => (Lightness, A, B) = (_Lightness, _A, _B);
    public static ColorLAB New(Percent _Lightness, Integer _A, Integer _B) => new(_Lightness, _A, _B);
    public static implicit operator (Percent, Integer, Integer)(ColorLAB self) => (self.Lightness, self.A, self.B);
    public static implicit operator ColorLAB((Percent, Integer, Integer) value) => new ColorLAB(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Lightness", "A", "B" };
    public object[] FieldValues() => new[] { Lightness, A, B };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Percent Lightness { get; }
    public Integer A { get; }
    public Integer B { get; }
}
public class ColorLCh: Value<ColorLCh>
{
    public ColorLCh(Percent _Lightness, PolarCoordinate _ChromaHue) => (Lightness, ChromaHue) = (_Lightness, _ChromaHue);
    public static ColorLCh New(Percent _Lightness, PolarCoordinate _ChromaHue) => new(_Lightness, _ChromaHue);
    public static implicit operator (Percent, PolarCoordinate)(ColorLCh self) => (self.Lightness, self.ChromaHue);
    public static implicit operator ColorLCh((Percent, PolarCoordinate) value) => new ColorLCh(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Lightness", "ChromaHue" };
    public object[] FieldValues() => new[] { Lightness, ChromaHue };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Percent Lightness { get; }
    public PolarCoordinate ChromaHue { get; }
}
public class ColorHSV: Value<ColorHSV>
{
    public ColorHSV(Angle _Hue, Unit _S, Unit _V) => (Hue, S, V) = (_Hue, _S, _V);
    public static ColorHSV New(Angle _Hue, Unit _S, Unit _V) => new(_Hue, _S, _V);
    public static implicit operator (Angle, Unit, Unit)(ColorHSV self) => (self.Hue, self.S, self.V);
    public static implicit operator ColorHSV((Angle, Unit, Unit) value) => new ColorHSV(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Hue", "S", "V" };
    public object[] FieldValues() => new[] { Hue, S, V };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Angle Hue { get; }
    public Unit S { get; }
    public Unit V { get; }
}
public class ColorHSL: Value<ColorHSL>
{
    public ColorHSL(Angle _Hue, Unit _Saturation, Unit _Luminance) => (Hue, Saturation, Luminance) = (_Hue, _Saturation, _Luminance);
    public static ColorHSL New(Angle _Hue, Unit _Saturation, Unit _Luminance) => new(_Hue, _Saturation, _Luminance);
    public static implicit operator (Angle, Unit, Unit)(ColorHSL self) => (self.Hue, self.Saturation, self.Luminance);
    public static implicit operator ColorHSL((Angle, Unit, Unit) value) => new ColorHSL(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Hue", "Saturation", "Luminance" };
    public object[] FieldValues() => new[] { Hue, Saturation, Luminance };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Angle Hue { get; }
    public Unit Saturation { get; }
    public Unit Luminance { get; }
}
public class ColorYCbCr: Value<ColorYCbCr>
{
    public ColorYCbCr(Unit _Y, Unit _Cb, Unit _Cr) => (Y, Cb, Cr) = (_Y, _Cb, _Cr);
    public static ColorYCbCr New(Unit _Y, Unit _Cb, Unit _Cr) => new(_Y, _Cb, _Cr);
    public static implicit operator (Unit, Unit, Unit)(ColorYCbCr self) => (self.Y, self.Cb, self.Cr);
    public static implicit operator ColorYCbCr((Unit, Unit, Unit) value) => new ColorYCbCr(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Y", "Cb", "Cr" };
    public object[] FieldValues() => new[] { Y, Cb, Cr };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Unit Y { get; }
    public Unit Cb { get; }
    public Unit Cr { get; }
}
public class SphericalCoordinate: Value<SphericalCoordinate>
{
    public SphericalCoordinate(Number _Radius, Angle _Azimuth, Angle _Polar) => (Radius, Azimuth, Polar) = (_Radius, _Azimuth, _Polar);
    public static SphericalCoordinate New(Number _Radius, Angle _Azimuth, Angle _Polar) => new(_Radius, _Azimuth, _Polar);
    public static implicit operator (Number, Angle, Angle)(SphericalCoordinate self) => (self.Radius, self.Azimuth, self.Polar);
    public static implicit operator SphericalCoordinate((Number, Angle, Angle) value) => new SphericalCoordinate(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Radius", "Azimuth", "Polar" };
    public object[] FieldValues() => new[] { Radius, Azimuth, Polar };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Radius { get; }
    public Angle Azimuth { get; }
    public Angle Polar { get; }
}
public class PolarCoordinate: Value<PolarCoordinate>
{
    public PolarCoordinate(Number _Radius, Angle _Angle) => (Radius, Angle) = (_Radius, _Angle);
    public static PolarCoordinate New(Number _Radius, Angle _Angle) => new(_Radius, _Angle);
    public static implicit operator (Number, Angle)(PolarCoordinate self) => (self.Radius, self.Angle);
    public static implicit operator PolarCoordinate((Number, Angle) value) => new PolarCoordinate(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Radius", "Angle" };
    public object[] FieldValues() => new[] { Radius, Angle };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Radius { get; }
    public Angle Angle { get; }
}
public class LogPolarCoordinate: Value<LogPolarCoordinate>
{
    public LogPolarCoordinate(Number _Rho, Angle _Azimuth) => (Rho, Azimuth) = (_Rho, _Azimuth);
    public static LogPolarCoordinate New(Number _Rho, Angle _Azimuth) => new(_Rho, _Azimuth);
    public static implicit operator (Number, Angle)(LogPolarCoordinate self) => (self.Rho, self.Azimuth);
    public static implicit operator LogPolarCoordinate((Number, Angle) value) => new LogPolarCoordinate(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Rho", "Azimuth" };
    public object[] FieldValues() => new[] { Rho, Azimuth };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Rho { get; }
    public Angle Azimuth { get; }
}
public class CylindricalCoordinate: Value<CylindricalCoordinate>
{
    public CylindricalCoordinate(Number _RadialDistance, Angle _Azimuth, Number _Height) => (RadialDistance, Azimuth, Height) = (_RadialDistance, _Azimuth, _Height);
    public static CylindricalCoordinate New(Number _RadialDistance, Angle _Azimuth, Number _Height) => new(_RadialDistance, _Azimuth, _Height);
    public static implicit operator (Number, Angle, Number)(CylindricalCoordinate self) => (self.RadialDistance, self.Azimuth, self.Height);
    public static implicit operator CylindricalCoordinate((Number, Angle, Number) value) => new CylindricalCoordinate(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "RadialDistance", "Azimuth", "Height" };
    public object[] FieldValues() => new[] { RadialDistance, Azimuth, Height };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number RadialDistance { get; }
    public Angle Azimuth { get; }
    public Number Height { get; }
}
public class HorizontalCoordinate: Value<HorizontalCoordinate>
{
    public HorizontalCoordinate(Number _Radius, Angle _Azimuth, Number _Height) => (Radius, Azimuth, Height) = (_Radius, _Azimuth, _Height);
    public static HorizontalCoordinate New(Number _Radius, Angle _Azimuth, Number _Height) => new(_Radius, _Azimuth, _Height);
    public static implicit operator (Number, Angle, Number)(HorizontalCoordinate self) => (self.Radius, self.Azimuth, self.Height);
    public static implicit operator HorizontalCoordinate((Number, Angle, Number) value) => new HorizontalCoordinate(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Radius", "Azimuth", "Height" };
    public object[] FieldValues() => new[] { Radius, Azimuth, Height };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Radius { get; }
    public Angle Azimuth { get; }
    public Number Height { get; }
}
public class GeoCoordinate: Value<GeoCoordinate>
{
    public GeoCoordinate(Angle _Latitude, Angle _Longitude) => (Latitude, Longitude) = (_Latitude, _Longitude);
    public static GeoCoordinate New(Angle _Latitude, Angle _Longitude) => new(_Latitude, _Longitude);
    public static implicit operator (Angle, Angle)(GeoCoordinate self) => (self.Latitude, self.Longitude);
    public static implicit operator GeoCoordinate((Angle, Angle) value) => new GeoCoordinate(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Latitude", "Longitude" };
    public object[] FieldValues() => new[] { Latitude, Longitude };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Angle Latitude { get; }
    public Angle Longitude { get; }
}
public class GeoCoordinateWithAltitude: Value<GeoCoordinateWithAltitude>
{
    public GeoCoordinateWithAltitude(GeoCoordinate _Coordinate, Number _Altitude) => (Coordinate, Altitude) = (_Coordinate, _Altitude);
    public static GeoCoordinateWithAltitude New(GeoCoordinate _Coordinate, Number _Altitude) => new(_Coordinate, _Altitude);
    public static implicit operator (GeoCoordinate, Number)(GeoCoordinateWithAltitude self) => (self.Coordinate, self.Altitude);
    public static implicit operator GeoCoordinateWithAltitude((GeoCoordinate, Number) value) => new GeoCoordinateWithAltitude(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Coordinate", "Altitude" };
    public object[] FieldValues() => new[] { Coordinate, Altitude };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public GeoCoordinate Coordinate { get; }
    public Number Altitude { get; }
}
public class Circle: Value<Circle>
{
    public Circle(Point2D _Center, Number _Radius) => (Center, Radius) = (_Center, _Radius);
    public static Circle New(Point2D _Center, Number _Radius) => new(_Center, _Radius);
    public static implicit operator (Point2D, Number)(Circle self) => (self.Center, self.Radius);
    public static implicit operator Circle((Point2D, Number) value) => new Circle(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Center", "Radius" };
    public object[] FieldValues() => new[] { Center, Radius };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point2D Center { get; }
    public Number Radius { get; }
}
public class Chord: Value<Chord>
{
    public Chord(Circle _Circle, Arc _Arc) => (Circle, Arc) = (_Circle, _Arc);
    public static Chord New(Circle _Circle, Arc _Arc) => new(_Circle, _Arc);
    public static implicit operator (Circle, Arc)(Chord self) => (self.Circle, self.Arc);
    public static implicit operator Chord((Circle, Arc) value) => new Chord(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Circle", "Arc" };
    public object[] FieldValues() => new[] { Circle, Arc };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Circle Circle { get; }
    public Arc Arc { get; }
}
public class Size2D: Value<Size2D>
{
    public Size2D(Number _Width, Number _Height) => (Width, Height) = (_Width, _Height);
    public static Size2D New(Number _Width, Number _Height) => new(_Width, _Height);
    public static implicit operator (Number, Number)(Size2D self) => (self.Width, self.Height);
    public static implicit operator Size2D((Number, Number) value) => new Size2D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Width", "Height" };
    public object[] FieldValues() => new[] { Width, Height };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Width { get; }
    public Number Height { get; }
}
public class Size3D: Value<Size3D>
{
    public Size3D(Number _Width, Number _Height, Number _Depth) => (Width, Height, Depth) = (_Width, _Height, _Depth);
    public static Size3D New(Number _Width, Number _Height, Number _Depth) => new(_Width, _Height, _Depth);
    public static implicit operator (Number, Number, Number)(Size3D self) => (self.Width, self.Height, self.Depth);
    public static implicit operator Size3D((Number, Number, Number) value) => new Size3D(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Width", "Height", "Depth" };
    public object[] FieldValues() => new[] { Width, Height, Depth };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Width { get; }
    public Number Height { get; }
    public Number Depth { get; }
}
public class Rectangle2D: Value<Rectangle2D>
{
    public Rectangle2D(Point2D _Center, Size2D _Size) => (Center, Size) = (_Center, _Size);
    public static Rectangle2D New(Point2D _Center, Size2D _Size) => new(_Center, _Size);
    public static implicit operator (Point2D, Size2D)(Rectangle2D self) => (self.Center, self.Size);
    public static implicit operator Rectangle2D((Point2D, Size2D) value) => new Rectangle2D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Center", "Size" };
    public object[] FieldValues() => new[] { Center, Size };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point2D Center { get; }
    public Size2D Size { get; }
}
public class Proportion: Numerical<Proportion>
{
    public Proportion(Number _Value) => (Value) = (_Value);
    public static Proportion New(Number _Value) => new(_Value);
    public static implicit operator Number(Proportion self) => self.Value;
    public static implicit operator Proportion(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Value" };
    public object[] FieldValues() => new[] { Value };
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Value { get; }
}
public class Fraction: Value<Fraction>
{
    public Fraction(Number _Numerator, Number _Denominator) => (Numerator, Denominator) = (_Numerator, _Denominator);
    public static Fraction New(Number _Numerator, Number _Denominator) => new(_Numerator, _Denominator);
    public static implicit operator (Number, Number)(Fraction self) => (self.Numerator, self.Denominator);
    public static implicit operator Fraction((Number, Number) value) => new Fraction(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Numerator", "Denominator" };
    public object[] FieldValues() => new[] { Numerator, Denominator };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Numerator { get; }
    public Number Denominator { get; }
}
public class Angle: Measure<Angle>
{
    public Angle(Number _Radians) => (Radians) = (_Radians);
    public static Angle New(Number _Radians) => new(_Radians);
    public static implicit operator Number(Angle self) => self.Radians;
    public static implicit operator Angle(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Radians" };
    public object[] FieldValues() => new[] { Radians };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Radians { get; }
}
public class Length: Measure<Length>
{
    public Length(Number _Meters) => (Meters) = (_Meters);
    public static Length New(Number _Meters) => new(_Meters);
    public static implicit operator Number(Length self) => self.Meters;
    public static implicit operator Length(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Meters" };
    public object[] FieldValues() => new[] { Meters };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Meters { get; }
}
public class Mass: Measure<Mass>
{
    public Mass(Number _Kilograms) => (Kilograms) = (_Kilograms);
    public static Mass New(Number _Kilograms) => new(_Kilograms);
    public static implicit operator Number(Mass self) => self.Kilograms;
    public static implicit operator Mass(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Kilograms" };
    public object[] FieldValues() => new[] { Kilograms };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Kilograms { get; }
}
public class Temperature: Measure<Temperature>
{
    public Temperature(Number _Celsius) => (Celsius) = (_Celsius);
    public static Temperature New(Number _Celsius) => new(_Celsius);
    public static implicit operator Number(Temperature self) => self.Celsius;
    public static implicit operator Temperature(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Celsius" };
    public object[] FieldValues() => new[] { Celsius };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Celsius { get; }
}
public class TimeSpan: Measure<TimeSpan>
{
    public TimeSpan(Number _Seconds) => (Seconds) = (_Seconds);
    public static TimeSpan New(Number _Seconds) => new(_Seconds);
    public static implicit operator Number(TimeSpan self) => self.Seconds;
    public static implicit operator TimeSpan(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Seconds" };
    public object[] FieldValues() => new[] { Seconds };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Seconds { get; }
}
public class TimeRange: Interval<TimeRange>
{
    public TimeRange(DateTime _Min, DateTime _Max) => (Min, Max) = (_Min, _Max);
    public static TimeRange New(DateTime _Min, DateTime _Max) => new(_Min, _Max);
    public static implicit operator (DateTime, DateTime)(TimeRange self) => (self.Min, self.Max);
    public static implicit operator TimeRange((DateTime, DateTime) value) => new TimeRange(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Min", "Max" };
    public object[] FieldValues() => new[] { Min, Max };
    public static Numerical Min(Interval x) => Extensions.Min(x);
    public static Numerical Max(Interval x) => Extensions.Max(x);
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public DateTime Min { get; }
    public DateTime Max { get; }
}
public class DateTime: Value<DateTime>
{
    public DateTime() => () = ();
    public static DateTime New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
}
public class AnglePair: Interval<AnglePair>
{
    public AnglePair(Angle _Start, Angle _End) => (Start, End) = (_Start, _End);
    public static AnglePair New(Angle _Start, Angle _End) => new(_Start, _End);
    public static implicit operator (Angle, Angle)(AnglePair self) => (self.Start, self.End);
    public static implicit operator AnglePair((Angle, Angle) value) => new AnglePair(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Start", "End" };
    public object[] FieldValues() => new[] { Start, End };
    public static Numerical Min(Interval x) => Extensions.Min(x);
    public static Numerical Max(Interval x) => Extensions.Max(x);
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Angle Start { get; }
    public Angle End { get; }
}
public class Ring: Numerical<Ring>
{
    public Ring(Circle _Circle, Number _InnerRadius) => (Circle, InnerRadius) = (_Circle, _InnerRadius);
    public static Ring New(Circle _Circle, Number _InnerRadius) => new(_Circle, _InnerRadius);
    public static implicit operator (Circle, Number)(Ring self) => (self.Circle, self.InnerRadius);
    public static implicit operator Ring((Circle, Number) value) => new Ring(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Circle", "InnerRadius" };
    public object[] FieldValues() => new[] { Circle, InnerRadius };
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Circle Circle { get; }
    public Number InnerRadius { get; }
}
public class Arc: Value<Arc>
{
    public Arc(AnglePair _Angles, Circle _Cirlce) => (Angles, Cirlce) = (_Angles, _Cirlce);
    public static Arc New(AnglePair _Angles, Circle _Cirlce) => new(_Angles, _Cirlce);
    public static implicit operator (AnglePair, Circle)(Arc self) => (self.Angles, self.Cirlce);
    public static implicit operator Arc((AnglePair, Circle) value) => new Arc(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Angles", "Cirlce" };
    public object[] FieldValues() => new[] { Angles, Cirlce };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public AnglePair Angles { get; }
    public Circle Cirlce { get; }
}
public class TimeInterval: Interval<TimeInterval>
{
    public TimeInterval(TimeSpan _Start, TimeSpan _End) => (Start, End) = (_Start, _End);
    public static TimeInterval New(TimeSpan _Start, TimeSpan _End) => new(_Start, _End);
    public static implicit operator (TimeSpan, TimeSpan)(TimeInterval self) => (self.Start, self.End);
    public static implicit operator TimeInterval((TimeSpan, TimeSpan) value) => new TimeInterval(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Start", "End" };
    public object[] FieldValues() => new[] { Start, End };
    public static Numerical Min(Interval x) => Extensions.Min(x);
    public static Numerical Max(Interval x) => Extensions.Max(x);
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public TimeSpan Start { get; }
    public TimeSpan End { get; }
}
public class RealInterval: Interval<RealInterval>
{
    public RealInterval(Number _A, Number _B) => (A, B) = (_A, _B);
    public static RealInterval New(Number _A, Number _B) => new(_A, _B);
    public static implicit operator (Number, Number)(RealInterval self) => (self.A, self.B);
    public static implicit operator RealInterval((Number, Number) value) => new RealInterval(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "A", "B" };
    public object[] FieldValues() => new[] { A, B };
    public static Numerical Min(Interval x) => Extensions.Min(x);
    public static Numerical Max(Interval x) => Extensions.Max(x);
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number A { get; }
    public Number B { get; }
}
public class Interval2D: Interval<Interval2D>
{
    public Interval2D(Vector2D _A, Vector2D _B) => (A, B) = (_A, _B);
    public static Interval2D New(Vector2D _A, Vector2D _B) => new(_A, _B);
    public static implicit operator (Vector2D, Vector2D)(Interval2D self) => (self.A, self.B);
    public static implicit operator Interval2D((Vector2D, Vector2D) value) => new Interval2D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "A", "B" };
    public object[] FieldValues() => new[] { A, B };
    public static Numerical Min(Interval x) => Extensions.Min(x);
    public static Numerical Max(Interval x) => Extensions.Max(x);
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector2D A { get; }
    public Vector2D B { get; }
}
public class Interval3D: Interval<Interval3D>
{
    public Interval3D(Vector3D _A, Vector3D _B) => (A, B) = (_A, _B);
    public static Interval3D New(Vector3D _A, Vector3D _B) => new(_A, _B);
    public static implicit operator (Vector3D, Vector3D)(Interval3D self) => (self.A, self.B);
    public static implicit operator Interval3D((Vector3D, Vector3D) value) => new Interval3D(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "A", "B" };
    public object[] FieldValues() => new[] { A, B };
    public static Numerical Min(Interval x) => Extensions.Min(x);
    public static Numerical Max(Interval x) => Extensions.Max(x);
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector3D A { get; }
    public Vector3D B { get; }
}
public class Capsule: Value<Capsule>
{
    public Capsule(Line3D _Line, Number _Radius) => (Line, Radius) = (_Line, _Radius);
    public static Capsule New(Line3D _Line, Number _Radius) => new(_Line, _Radius);
    public static implicit operator (Line3D, Number)(Capsule self) => (self.Line, self.Radius);
    public static implicit operator Capsule((Line3D, Number) value) => new Capsule(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Line", "Radius" };
    public object[] FieldValues() => new[] { Line, Radius };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Line3D Line { get; }
    public Number Radius { get; }
}
public class Matrix3D: Value<Matrix3D>
{
    public Matrix3D(Vector4D _Column1, Vector4D _Column2, Vector4D _Column3, Vector4D _Column4) => (Column1, Column2, Column3, Column4) = (_Column1, _Column2, _Column3, _Column4);
    public static Matrix3D New(Vector4D _Column1, Vector4D _Column2, Vector4D _Column3, Vector4D _Column4) => new(_Column1, _Column2, _Column3, _Column4);
    public static implicit operator (Vector4D, Vector4D, Vector4D, Vector4D)(Matrix3D self) => (self.Column1, self.Column2, self.Column3, self.Column4);
    public static implicit operator Matrix3D((Vector4D, Vector4D, Vector4D, Vector4D) value) => new Matrix3D(value.Item1, value.Item2, value.Item3, value.Item4);
    public string[] FieldNames() => new[] { "Column1", "Column2", "Column3", "Column4" };
    public object[] FieldValues() => new[] { Column1, Column2, Column3, Column4 };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Vector4D Column1 { get; }
    public Vector4D Column2 { get; }
    public Vector4D Column3 { get; }
    public Vector4D Column4 { get; }
}
public class Cylinder: Value<Cylinder>
{
    public Cylinder(Line3D _Line, Number _Radius) => (Line, Radius) = (_Line, _Radius);
    public static Cylinder New(Line3D _Line, Number _Radius) => new(_Line, _Radius);
    public static implicit operator (Line3D, Number)(Cylinder self) => (self.Line, self.Radius);
    public static implicit operator Cylinder((Line3D, Number) value) => new Cylinder(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Line", "Radius" };
    public object[] FieldValues() => new[] { Line, Radius };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Line3D Line { get; }
    public Number Radius { get; }
}
public class Cone: Value<Cone>
{
    public Cone(Line3D _Line, Number _Radius) => (Line, Radius) = (_Line, _Radius);
    public static Cone New(Line3D _Line, Number _Radius) => new(_Line, _Radius);
    public static implicit operator (Line3D, Number)(Cone self) => (self.Line, self.Radius);
    public static implicit operator Cone((Line3D, Number) value) => new Cone(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Line", "Radius" };
    public object[] FieldValues() => new[] { Line, Radius };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Line3D Line { get; }
    public Number Radius { get; }
}
public class Tube: Value<Tube>
{
    public Tube(Line3D _Line, Number _InnerRadius, Number _OuterRadius) => (Line, InnerRadius, OuterRadius) = (_Line, _InnerRadius, _OuterRadius);
    public static Tube New(Line3D _Line, Number _InnerRadius, Number _OuterRadius) => new(_Line, _InnerRadius, _OuterRadius);
    public static implicit operator (Line3D, Number, Number)(Tube self) => (self.Line, self.InnerRadius, self.OuterRadius);
    public static implicit operator Tube((Line3D, Number, Number) value) => new Tube(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Line", "InnerRadius", "OuterRadius" };
    public object[] FieldValues() => new[] { Line, InnerRadius, OuterRadius };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Line3D Line { get; }
    public Number InnerRadius { get; }
    public Number OuterRadius { get; }
}
public class ConeSegment: Value<ConeSegment>
{
    public ConeSegment(Line3D _Line, Number _Radius1, Number _Radius2) => (Line, Radius1, Radius2) = (_Line, _Radius1, _Radius2);
    public static ConeSegment New(Line3D _Line, Number _Radius1, Number _Radius2) => new(_Line, _Radius1, _Radius2);
    public static implicit operator (Line3D, Number, Number)(ConeSegment self) => (self.Line, self.Radius1, self.Radius2);
    public static implicit operator ConeSegment((Line3D, Number, Number) value) => new ConeSegment(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Line", "Radius1", "Radius2" };
    public object[] FieldValues() => new[] { Line, Radius1, Radius2 };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Line3D Line { get; }
    public Number Radius1 { get; }
    public Number Radius2 { get; }
}
public class Box2D: Value<Box2D>
{
    public Box2D(Point2D _Center, Angle _Rotation, Size2D _Extent) => (Center, Rotation, Extent) = (_Center, _Rotation, _Extent);
    public static Box2D New(Point2D _Center, Angle _Rotation, Size2D _Extent) => new(_Center, _Rotation, _Extent);
    public static implicit operator (Point2D, Angle, Size2D)(Box2D self) => (self.Center, self.Rotation, self.Extent);
    public static implicit operator Box2D((Point2D, Angle, Size2D) value) => new Box2D(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Center", "Rotation", "Extent" };
    public object[] FieldValues() => new[] { Center, Rotation, Extent };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point2D Center { get; }
    public Angle Rotation { get; }
    public Size2D Extent { get; }
}
public class Box3D: Value<Box3D>
{
    public Box3D(Point3D _Center, Rotation3D _Rotation, Size3D _Extent) => (Center, Rotation, Extent) = (_Center, _Rotation, _Extent);
    public static Box3D New(Point3D _Center, Rotation3D _Rotation, Size3D _Extent) => new(_Center, _Rotation, _Extent);
    public static implicit operator (Point3D, Rotation3D, Size3D)(Box3D self) => (self.Center, self.Rotation, self.Extent);
    public static implicit operator Box3D((Point3D, Rotation3D, Size3D) value) => new Box3D(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Center", "Rotation", "Extent" };
    public object[] FieldValues() => new[] { Center, Rotation, Extent };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point3D Center { get; }
    public Rotation3D Rotation { get; }
    public Size3D Extent { get; }
}
public class CubicBezierTriangle3D: Value<CubicBezierTriangle3D>
{
    public CubicBezierTriangle3D(Point3D _A, Point3D _B, Point3D _C, Point3D _A2B, Point3D _AB2, Point3D _B2C, Point3D _BC2, Point3D _AC2, Point3D _A2C, Point3D _ABC) => (A, B, C, A2B, AB2, B2C, BC2, AC2, A2C, ABC) = (_A, _B, _C, _A2B, _AB2, _B2C, _BC2, _AC2, _A2C, _ABC);
    public static CubicBezierTriangle3D New(Point3D _A, Point3D _B, Point3D _C, Point3D _A2B, Point3D _AB2, Point3D _B2C, Point3D _BC2, Point3D _AC2, Point3D _A2C, Point3D _ABC) => new(_A, _B, _C, _A2B, _AB2, _B2C, _BC2, _AC2, _A2C, _ABC);
    public static implicit operator (Point3D, Point3D, Point3D, Point3D, Point3D, Point3D, Point3D, Point3D, Point3D, Point3D)(CubicBezierTriangle3D self) => (self.A, self.B, self.C, self.A2B, self.AB2, self.B2C, self.BC2, self.AC2, self.A2C, self.ABC);
    public static implicit operator CubicBezierTriangle3D((Point3D, Point3D, Point3D, Point3D, Point3D, Point3D, Point3D, Point3D, Point3D, Point3D) value) => new CubicBezierTriangle3D(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7, value.Item8, value.Item9, value.Item10);
    public string[] FieldNames() => new[] { "A", "B", "C", "A2B", "AB2", "B2C", "BC2", "AC2", "A2C", "ABC" };
    public object[] FieldValues() => new[] { A, B, C, A2B, AB2, B2C, BC2, AC2, A2C, ABC };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point3D A { get; }
    public Point3D B { get; }
    public Point3D C { get; }
    public Point3D A2B { get; }
    public Point3D AB2 { get; }
    public Point3D B2C { get; }
    public Point3D BC2 { get; }
    public Point3D AC2 { get; }
    public Point3D A2C { get; }
    public Point3D ABC { get; }
}
public class CubicBezier2D: Value<CubicBezier2D>
{
    public CubicBezier2D(Point2D _A, Point2D _B, Point2D _C, Point2D _D) => (A, B, C, D) = (_A, _B, _C, _D);
    public static CubicBezier2D New(Point2D _A, Point2D _B, Point2D _C, Point2D _D) => new(_A, _B, _C, _D);
    public static implicit operator (Point2D, Point2D, Point2D, Point2D)(CubicBezier2D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator CubicBezier2D((Point2D, Point2D, Point2D, Point2D) value) => new CubicBezier2D(value.Item1, value.Item2, value.Item3, value.Item4);
    public string[] FieldNames() => new[] { "A", "B", "C", "D" };
    public object[] FieldValues() => new[] { A, B, C, D };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point2D A { get; }
    public Point2D B { get; }
    public Point2D C { get; }
    public Point2D D { get; }
}
public class UV: Vector<UV>
{
    public UV(Unit _U, Unit _V) => (U, V) = (_U, _V);
    public static UV New(Unit _U, Unit _V) => new(_U, _V);
    public static implicit operator (Unit, Unit)(UV self) => (self.U, self.V);
    public static implicit operator UV((Unit, Unit) value) => new UV(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "U", "V" };
    public object[] FieldValues() => new[] { U, V };
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Unit U { get; }
    public Unit V { get; }
}
public class UVW: Vector<UVW>
{
    public UVW(Unit _U, Unit _V, Unit _W) => (U, V, W) = (_U, _V, _W);
    public static UVW New(Unit _U, Unit _V, Unit _W) => new(_U, _V, _W);
    public static implicit operator (Unit, Unit, Unit)(UVW self) => (self.U, self.V, self.W);
    public static implicit operator UVW((Unit, Unit, Unit) value) => new UVW(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "U", "V", "W" };
    public object[] FieldValues() => new[] { U, V, W };
    public Numerical this[Integer n]
        => At(FieldValues(v), n);
    public static Integer Count(Array xs) => Extensions.Count(xs);
    public static Any At(Array xs, Integer n) => Extensions.At(xs, n);
    public Any this[Integer n]
        => null;
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Unit U { get; }
    public Unit V { get; }
    public Unit W { get; }
}
public class CubicBezier3D: Value<CubicBezier3D>
{
    public CubicBezier3D(Point3D _A, Point3D _B, Point3D _C, Point3D _D) => (A, B, C, D) = (_A, _B, _C, _D);
    public static CubicBezier3D New(Point3D _A, Point3D _B, Point3D _C, Point3D _D) => new(_A, _B, _C, _D);
    public static implicit operator (Point3D, Point3D, Point3D, Point3D)(CubicBezier3D self) => (self.A, self.B, self.C, self.D);
    public static implicit operator CubicBezier3D((Point3D, Point3D, Point3D, Point3D) value) => new CubicBezier3D(value.Item1, value.Item2, value.Item3, value.Item4);
    public string[] FieldNames() => new[] { "A", "B", "C", "D" };
    public object[] FieldValues() => new[] { A, B, C, D };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point3D A { get; }
    public Point3D B { get; }
    public Point3D C { get; }
    public Point3D D { get; }
}
public class QuadraticBezier2D: Value<QuadraticBezier2D>
{
    public QuadraticBezier2D(Point2D _A, Point2D _B, Point2D _C) => (A, B, C) = (_A, _B, _C);
    public static QuadraticBezier2D New(Point2D _A, Point2D _B, Point2D _C) => new(_A, _B, _C);
    public static implicit operator (Point2D, Point2D, Point2D)(QuadraticBezier2D self) => (self.A, self.B, self.C);
    public static implicit operator QuadraticBezier2D((Point2D, Point2D, Point2D) value) => new QuadraticBezier2D(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "A", "B", "C" };
    public object[] FieldValues() => new[] { A, B, C };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point2D A { get; }
    public Point2D B { get; }
    public Point2D C { get; }
}
public class QuadraticBezier3D: Value<QuadraticBezier3D>
{
    public QuadraticBezier3D(Point3D _A, Point3D _B, Point3D _C) => (A, B, C) = (_A, _B, _C);
    public static QuadraticBezier3D New(Point3D _A, Point3D _B, Point3D _C) => new(_A, _B, _C);
    public static implicit operator (Point3D, Point3D, Point3D)(QuadraticBezier3D self) => (self.A, self.B, self.C);
    public static implicit operator QuadraticBezier3D((Point3D, Point3D, Point3D) value) => new QuadraticBezier3D(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "A", "B", "C" };
    public object[] FieldValues() => new[] { A, B, C };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Point3D A { get; }
    public Point3D B { get; }
    public Point3D C { get; }
}
public class Area: Measure<Area>
{
    public Area(Number _MetersSquared) => (MetersSquared) = (_MetersSquared);
    public static Area New(Number _MetersSquared) => new(_MetersSquared);
    public static implicit operator Number(Area self) => self.MetersSquared;
    public static implicit operator Area(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "MetersSquared" };
    public object[] FieldValues() => new[] { MetersSquared };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number MetersSquared { get; }
}
public class Volume: Measure<Volume>
{
    public Volume(Number _MetersCubed) => (MetersCubed) = (_MetersCubed);
    public static Volume New(Number _MetersCubed) => new(_MetersCubed);
    public static implicit operator Number(Volume self) => self.MetersCubed;
    public static implicit operator Volume(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "MetersCubed" };
    public object[] FieldValues() => new[] { MetersCubed };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number MetersCubed { get; }
}
public class Velocity: Measure<Velocity>
{
    public Velocity(Number _MetersPerSecond) => (MetersPerSecond) = (_MetersPerSecond);
    public static Velocity New(Number _MetersPerSecond) => new(_MetersPerSecond);
    public static implicit operator Number(Velocity self) => self.MetersPerSecond;
    public static implicit operator Velocity(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "MetersPerSecond" };
    public object[] FieldValues() => new[] { MetersPerSecond };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number MetersPerSecond { get; }
}
public class Acceleration: Measure<Acceleration>
{
    public Acceleration(Number _MetersPerSecondSquared) => (MetersPerSecondSquared) = (_MetersPerSecondSquared);
    public static Acceleration New(Number _MetersPerSecondSquared) => new(_MetersPerSecondSquared);
    public static implicit operator Number(Acceleration self) => self.MetersPerSecondSquared;
    public static implicit operator Acceleration(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "MetersPerSecondSquared" };
    public object[] FieldValues() => new[] { MetersPerSecondSquared };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number MetersPerSecondSquared { get; }
}
public class Force: Measure<Force>
{
    public Force(Number _Newtons) => (Newtons) = (_Newtons);
    public static Force New(Number _Newtons) => new(_Newtons);
    public static implicit operator Number(Force self) => self.Newtons;
    public static implicit operator Force(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Newtons" };
    public object[] FieldValues() => new[] { Newtons };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Newtons { get; }
}
public class Pressure: Measure<Pressure>
{
    public Pressure(Number _Pascals) => (Pascals) = (_Pascals);
    public static Pressure New(Number _Pascals) => new(_Pascals);
    public static implicit operator Number(Pressure self) => self.Pascals;
    public static implicit operator Pressure(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Pascals" };
    public object[] FieldValues() => new[] { Pascals };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Pascals { get; }
}
public class Energy: Measure<Energy>
{
    public Energy(Number _Joules) => (Joules) = (_Joules);
    public static Energy New(Number _Joules) => new(_Joules);
    public static implicit operator Number(Energy self) => self.Joules;
    public static implicit operator Energy(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Joules" };
    public object[] FieldValues() => new[] { Joules };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Joules { get; }
}
public class Memory: Measure<Memory>
{
    public Memory(Count _Bytes) => (Bytes) = (_Bytes);
    public static Memory New(Count _Bytes) => new(_Bytes);
    public static implicit operator Count(Memory self) => self.Bytes;
    public static implicit operator Memory(Count value) => new Count(value);
    public string[] FieldNames() => new[] { "Bytes" };
    public object[] FieldValues() => new[] { Bytes };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Count Bytes { get; }
}
public class Frequency: Measure<Frequency>
{
    public Frequency(Number _Hertz) => (Hertz) = (_Hertz);
    public static Frequency New(Number _Hertz) => new(_Hertz);
    public static implicit operator Number(Frequency self) => self.Hertz;
    public static implicit operator Frequency(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Hertz" };
    public object[] FieldValues() => new[] { Hertz };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Hertz { get; }
}
public class Loudness: Measure<Loudness>
{
    public Loudness(Number _Decibels) => (Decibels) = (_Decibels);
    public static Loudness New(Number _Decibels) => new(_Decibels);
    public static implicit operator Number(Loudness self) => self.Decibels;
    public static implicit operator Loudness(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Decibels" };
    public object[] FieldValues() => new[] { Decibels };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Decibels { get; }
}
public class LuminousIntensity: Measure<LuminousIntensity>
{
    public LuminousIntensity(Number _Candelas) => (Candelas) = (_Candelas);
    public static LuminousIntensity New(Number _Candelas) => new(_Candelas);
    public static implicit operator Number(LuminousIntensity self) => self.Candelas;
    public static implicit operator LuminousIntensity(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Candelas" };
    public object[] FieldValues() => new[] { Candelas };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Candelas { get; }
}
public class ElectricPotential: Measure<ElectricPotential>
{
    public ElectricPotential(Number _Volts) => (Volts) = (_Volts);
    public static ElectricPotential New(Number _Volts) => new(_Volts);
    public static implicit operator Number(ElectricPotential self) => self.Volts;
    public static implicit operator ElectricPotential(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Volts" };
    public object[] FieldValues() => new[] { Volts };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Volts { get; }
}
public class ElectricCharge: Measure<ElectricCharge>
{
    public ElectricCharge(Number _Columbs) => (Columbs) = (_Columbs);
    public static ElectricCharge New(Number _Columbs) => new(_Columbs);
    public static implicit operator Number(ElectricCharge self) => self.Columbs;
    public static implicit operator ElectricCharge(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Columbs" };
    public object[] FieldValues() => new[] { Columbs };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Columbs { get; }
}
public class ElectricCurrent: Measure<ElectricCurrent>
{
    public ElectricCurrent(Number _Amperes) => (Amperes) = (_Amperes);
    public static ElectricCurrent New(Number _Amperes) => new(_Amperes);
    public static implicit operator Number(ElectricCurrent self) => self.Amperes;
    public static implicit operator ElectricCurrent(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Amperes" };
    public object[] FieldValues() => new[] { Amperes };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Amperes { get; }
}
public class ElectricResistance: Measure<ElectricResistance>
{
    public ElectricResistance(Number _Ohms) => (Ohms) = (_Ohms);
    public static ElectricResistance New(Number _Ohms) => new(_Ohms);
    public static implicit operator Number(ElectricResistance self) => self.Ohms;
    public static implicit operator ElectricResistance(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Ohms" };
    public object[] FieldValues() => new[] { Ohms };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Ohms { get; }
}
public class Power: Measure<Power>
{
    public Power(Number _Watts) => (Watts) = (_Watts);
    public static Power New(Number _Watts) => new(_Watts);
    public static implicit operator Number(Power self) => self.Watts;
    public static implicit operator Power(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Watts" };
    public object[] FieldValues() => new[] { Watts };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Watts { get; }
}
public class Density: Measure<Density>
{
    public Density(Number _KilogramsPerMeterCubed) => (KilogramsPerMeterCubed) = (_KilogramsPerMeterCubed);
    public static Density New(Number _KilogramsPerMeterCubed) => new(_KilogramsPerMeterCubed);
    public static implicit operator Number(Density self) => self.KilogramsPerMeterCubed;
    public static implicit operator Density(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "KilogramsPerMeterCubed" };
    public object[] FieldValues() => new[] { KilogramsPerMeterCubed };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static ScalarArithmetic operator +(ScalarArithmetic self, Number scalar) => Extensions.Add(self, scalar);
    public static ScalarArithmetic operator -(ScalarArithmetic self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ScalarArithmetic operator *(ScalarArithmetic self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ScalarArithmetic operator /(ScalarArithmetic self, Number scalar) => Extensions.Divide(self, scalar);
    public static ScalarArithmetic operator %(ScalarArithmetic self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number KilogramsPerMeterCubed { get; }
}
public class NormalDistribution: Value<NormalDistribution>
{
    public NormalDistribution(Number _Mean, Number _StandardDeviation) => (Mean, StandardDeviation) = (_Mean, _StandardDeviation);
    public static NormalDistribution New(Number _Mean, Number _StandardDeviation) => new(_Mean, _StandardDeviation);
    public static implicit operator (Number, Number)(NormalDistribution self) => (self.Mean, self.StandardDeviation);
    public static implicit operator NormalDistribution((Number, Number) value) => new NormalDistribution(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Mean", "StandardDeviation" };
    public object[] FieldValues() => new[] { Mean, StandardDeviation };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Mean { get; }
    public Number StandardDeviation { get; }
}
public class PoissonDistribution: Value<PoissonDistribution>
{
    public PoissonDistribution(Number _Expected, Count _Occurrences) => (Expected, Occurrences) = (_Expected, _Occurrences);
    public static PoissonDistribution New(Number _Expected, Count _Occurrences) => new(_Expected, _Occurrences);
    public static implicit operator (Number, Count)(PoissonDistribution self) => (self.Expected, self.Occurrences);
    public static implicit operator PoissonDistribution((Number, Count) value) => new PoissonDistribution(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Expected", "Occurrences" };
    public object[] FieldValues() => new[] { Expected, Occurrences };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Expected { get; }
    public Count Occurrences { get; }
}
public class BernoulliDistribution: Value<BernoulliDistribution>
{
    public BernoulliDistribution(Probability _P) => (P) = (_P);
    public static BernoulliDistribution New(Probability _P) => new(_P);
    public static implicit operator Probability(BernoulliDistribution self) => self.P;
    public static implicit operator BernoulliDistribution(Probability value) => new Probability(value);
    public string[] FieldNames() => new[] { "P" };
    public object[] FieldValues() => new[] { P };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Probability P { get; }
}
public class Probability: Numerical<Probability>
{
    public Probability(Number _Value) => (Value) = (_Value);
    public static Probability New(Number _Value) => new(_Value);
    public static implicit operator Number(Probability self) => self.Value;
    public static implicit operator Probability(Number value) => new Number(value);
    public string[] FieldNames() => new[] { "Value" };
    public object[] FieldValues() => new[] { Value };
    public static Numerical Zero(Numerical x) => Extensions.Zero(x);
    public static Numerical One(Numerical x) => Extensions.One(x);
    public static Numerical MinValue(Numerical x) => Extensions.MinValue(x);
    public static Numerical MaxValue(Numerical x) => Extensions.MaxValue(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Arithmetic operator +(Arithmetic self, Arithmetic other) => Extensions.Add(self, other);
    public static Arithmetic operator -(Arithmetic self) => Extensions.Negative(self);
    public static Arithmetic operator *(Arithmetic self, Arithmetic other) => Extensions.Multiply(self, other);
    public static Arithmetic operator /(Arithmetic self, Arithmetic other) => Extensions.Divide(self, other);
    public static Arithmetic operator %(Arithmetic self, Arithmetic other) => Extensions.Modulo(self, other);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Boolean operator ==(Equatable a, Equatable b) => Extensions.Equals(a, b);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Integer Compare(Comparable x) => Extensions.Compare(x);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Number Value { get; }
}
public class BinomialDistribution: Value<BinomialDistribution>
{
    public BinomialDistribution(Count _Trials, Probability _P) => (Trials, P) = (_Trials, _P);
    public static BinomialDistribution New(Count _Trials, Probability _P) => new(_Trials, _P);
    public static implicit operator (Count, Probability)(BinomialDistribution self) => (self.Trials, self.P);
    public static implicit operator BinomialDistribution((Count, Probability) value) => new BinomialDistribution(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Trials", "P" };
    public object[] FieldValues() => new[] { Trials, P };
    public static Value Default(Value self) => Extensions.Default(self);
    public static Array FieldNames(Any self) => Extensions.FieldNames(self);
    public static Array FieldValues(Any x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Any x) => Extensions.FieldTypes(x);
    public static Type TypeOf(Any self) => Extensions.TypeOf(self);
    public Count Trials { get; }
    public Probability P { get; }
}
public static partial class Extensions
{
    public static Array Map<T>(Array xs, Function f) {
        return Tuple(Count(xs), (i) => 
        f(At(xs, i)));
    }
    public static Array Reverse<T>(Array xs) {
        return Tuple(Count(xs), (i) => 
        f(At(xs, Subtract(Count(xs), Subtract(1, i)))));
    }
    public static Array Zip<T>(Array xs, Array ys, Function f) {
        return Tuple(Count(xs), (i) => 
        f(At(i), At(ys, i)));
    }
    public static Array Zip<T>(Array xs, Array ys, Array zs, Function f) {
        return Tuple(Count(xs), (i) => 
        f(At(i), At(ys, i), At(zs, i)));
    }
    public static Array Skip<T>(Array xs, Integer n) {
        return Tuple(Subtract(Count, n), (i) => 
        At(Subtract(i, n)));
    }
    public static Array Take<T>(Array xs, Integer n) {
        return Tuple(n, (i) => 
        At(i));
    }
    public static Any Aggregate<T>(Array xs, Any init, Function f) {
        return IsEmpty(xs)
            ? init
            : f(init, f(Rest(xs)))
        ;
    }
    public static Array Rest<T>(Array xs) {
        return Skip(xs, 1);
    }
    public static Boolean IsEmpty<T>(Array xs) {
        return Equals(Count(xs), 0);
    }
    public static Any First<T>(Array xs) {
        return At(xs, 0);
    }
    public static Any Last<T>(Array xs) {
        return At(xs, Subtract(Count(xs), 1));
    }
    public static Array Slice<T>(Array xs, Integer from, Integer count) {
        return Take(Skip(xs, from), count);
    }
    public static String Join<T>(Array xs, String sep) {
        return IsEmpty(xs)
            ? 
            : Add(ToString(First(xs)), Aggregate(Rest(xs), , (acc, cur) => 
            Interpolate(acc, sep, cur)))
        ;
    }
    public static Boolean All<T>(Array xs, Function f) {
        return IsEmpty(xs)
            ? True
            : And(f(First(xs)), f(Rest(xs)))
        ;
    }
    public static Boolean All<T>(Array xs) {
        return All(xs, (b) => 
        b);
    }
}
public static partial class Extensions
{
    public static Numerical Size<T>(Interval x) {
        return Subtract(Max(x), Min(x));
    }
    public static Boolean IsEmpty<T>(Interval x) {
        return GreaterThanOrEquals(Min(x), Max(x));
    }
    public static Numerical Lerp<T>(Interval x, Unit amount) {
        return Multiply(Min(x), Add(Subtract(1, amount), Multiply(Max(x), amount)));
    }
    public static Unit InverseLerp<T>(Interval x, Numerical value) {
        return Divide(Subtract(value, Min(x)), Size(x));
    }
    public static Interval Negate<T>(Interval x) {
        return Tuple(Negative(Max(x)), Negative(Min(x)));
    }
    public static Interval Reverse<T>(Interval x) {
        return Tuple(Max(x), Min(x));
    }
    public static Numerical Center<T>(Interval x) {
        return Lerp(x, 0.5);
    }
    public static Boolean Contains<T>(Interval x, Numerical value) {
        return LessThanOrEquals(Min(x), And(value, LessThanOrEquals(value, Max(x))));
    }
    public static Boolean Contains<T>(Interval x, Interval other) {
        return LessThanOrEquals(Min(x), And(Min(other), GreaterThanOrEquals(Max, Max(other))));
    }
    public static Boolean Overlaps<T>(Interval x, Interval y) {
        return Not(IsEmpty(Clamp(x, y)));
    }
    public static Tuple Split<T>(Interval x, Unit t) {
        return Tuple(Left(x, t), Right(x, t));
    }
    public static Tuple Split<T>(Interval x) {
        return Split(x, 0.5);
    }
    public static Interval Left<T>(Interval x, Unit t) {
        return Tuple(Min(x), Lerp(x, t));
    }
    public static Interval Right<T>(Interval x, Unit t) {
        return Tuple(Lerp(x, t), Max(x));
    }
    public static Interval MoveTo<T>(Interval x, Numerical v) {
        return Tuple(v, Add(v, Size(x)));
    }
    public static Interval LeftHalf<T>(Interval x) {
        return Left(x, 0.5);
    }
    public static Interval RightHalf<T>(Interval x) {
        return Right(x, 0.5);
    }
    public static Numerical HalfSize<T>(Interval x) {
        return Half(Size(x));
    }
    public static Interval Recenter<T>(Interval x, Numerical c) {
        return Tuple(Subtract(c, HalfSize(x)), Add(c, HalfSize(x)));
    }
    public static Interval Clamp<T>(Interval x, Interval y) {
        return Tuple(Clamp(x, Min(y)), Clamp(x, Max(y)));
    }
    public static Numerical Clamp<T>(Interval x, Numerical value) {
        return LessThan(value, Min(x)
            ? Min(x)
            : GreaterThan(value, Max(x)
                ? Max(x)
                : value
            )
        );
    }
    public static Boolean Within<T>(Interval x, Numerical value) {
        return GreaterThanOrEquals(value, And(Min(x), LessThanOrEquals(value, Max(x))));
    }
}
public static partial class Extensions
{
    public static String ToString(Value x) {
        return Join(FieldValues(x), , );
    }
}
public static partial class Extensions
{
    public static Number Sum<T>(Array v) {
        return Aggregate(v, 0, Add);
    }
    public static Number SumSquares<T>(Array v) {
        return Aggregate(Square(v), 0, Add);
    }
    public static Number LengthSquared<T>(Array v) {
        return SumSquares(v);
    }
    public static Number Length<T>(Array v) {
        return SquareRoot(LengthSquared(v));
    }
    public static Number Dot<T>(Vector v1, Vector v2) {
        return Sum(Multiply(v1, v2));
    }
    public static Vector Normal<T>(Vector v) {
        return Divide(v, Length(v));
    }
}
public static partial class Extensions
{
    public static Number SquareRoot(Number x) {
        return Pow(x, 0.5);
    }
    public static Number Square(Number x) {
        return Multiply(x, x);
    }
    public static Number Clamp(Number x) {
        return Clamp(x, Tuple(0, 1));
    }
    public static Number PlusOne(Number x) {
        return Add(x, One(x));
    }
    public static Number MinusOne(Number x) {
        return Subtract(x, One(x));
    }
    public static Number FromOne(Number x) {
        return Subtract(One(x), x);
    }
    public static Boolean IsPositive(Number x) {
        return GreaterThanOrEquals(x, 0);
    }
    public static Boolean GtZ(Number x) {
        return GreaterThan(x, 0);
    }
    public static Boolean LtZ(Number x) {
        return LessThan(x, 0);
    }
    public static Boolean GtEqZ(Number x) {
        return GreaterThanOrEquals(x, 0);
    }
    public static Boolean LtEqZ(Number x) {
        return LessThanOrEquals(x, 0);
    }
    public static Boolean IsNegative(Number x) {
        return LessThan(x, 0);
    }
    public static Number Sign(Number x) {
        return LtZ(x)
            ? Negative(One(x))
            : GtZ(x)
                ? One(x)
                : Zero(x)

        ;
    }
    public static Number Abs(Number x) {
        return LtZ(x)
            ? Negative(x)
            : x
        ;
    }
    public static Number Half(Number x) {
        return Divide(x, 2);
    }
    public static Number Third(Number x) {
        return Divide(x, 3);
    }
    public static Number Quarter(Number x) {
        return Divide(x, 4);
    }
    public static Number Fifth(Number x) {
        return Divide(x, 5);
    }
    public static Number Sixth(Number x) {
        return Divide(x, 6);
    }
    public static Number Seventh(Number x) {
        return Divide(x, 7);
    }
    public static Number Eighth(Number x) {
        return Divide(x, 8);
    }
    public static Number Ninth(Number x) {
        return Divide(x, 9);
    }
    public static Number Tenth(Number x) {
        return Divide(x, 10);
    }
    public static Number Sixteenth(Number x) {
        return Divide(x, 16);
    }
    public static Number Hundredth(Number x) {
        return Divide(x, 100);
    }
    public static Number Thousandth(Number x) {
        return Divide(x, 1000);
    }
    public static Number Millionth(Number x) {
        return Divide(x, Divide(1000, 1000));
    }
    public static Number Billionth(Number x) {
        return Divide(x, Divide(1000, Divide(1000, 1000)));
    }
    public static Number Hundred(Number x) {
        return Multiply(x, 100);
    }
    public static Number Thousand(Number x) {
        return Multiply(x, 1000);
    }
    public static Number Million(Number x) {
        return Multiply(x, Multiply(1000, 1000));
    }
    public static Number Billion(Number x) {
        return Multiply(x, Multiply(1000, Multiply(1000, 1000)));
    }
    public static Number Twice(Number x) {
        return Multiply(x, 2);
    }
    public static Number Thrice(Number x) {
        return Multiply(x, 3);
    }
    public static Number SmoothStep(Number x) {
        return Multiply(Square(x), Subtract(3, Twice(x)));
    }
    public static Number Pow2(Number x) {
        return Multiply(x, x);
    }
    public static Number Pow3(Number x) {
        return Multiply(Pow2(x), x);
    }
    public static Number Pow4(Number x) {
        return Multiply(Pow3(x), x);
    }
    public static Number Pow5(Number x) {
        return Multiply(Pow4(x), x);
    }
    public static Number Pi(Numerical self) {
        return 3.1415926535897;
    }
    public static Boolean AlmostZero(Number x) {
        return LessThan(Abs(x), 1E-08);
    }
    public static Number Lerp(Number a, Number b, Unit t) {
        return Multiply(Subtract(1, t), Add(a, Multiply(t, b)));
    }
    public static Boolean Between(Number self, Number min, Number max) {
        return Zip(FieldValues(self), FieldValues(min), FieldValues(max), (x, y, z) => 
        Between(x, y, z));
    }
}
public static partial class Extensions
{
    public static Angle Radians(Number x) {
        return x;
    }
    public static Angle Degrees(Number x) {
        return Multiply(x, Divide(Pi, 180));
    }
    public static Angle Turns(Number x) {
        return Multiply(x, Multiply(2, Pi));
    }
}
public static partial class Extensions
{
    public static Boolean Equals(Comparable a, Comparable b) {
        return Equals(Compare(a, b), 0);
    }
    public static Boolean LessThan(Comparable a, Comparable b) {
        return LessThan(Compare(a, b), 0);
    }
    public static Boolean LessThanOrEquals(Comparable a, Comparable b) {
        return LessThanOrEquals(Compare(a, b), 0);
    }
    public static Boolean GreaterThan(Comparable a, Comparable b) {
        return GreaterThan(Compare(a, b), 0);
    }
    public static Boolean GreaterThanOrEquals(Comparable a, Comparable b) {
        return GreaterThanOrEquals(Compare(a, b), 0);
    }
    public static Value Between(Comparable v, Comparable a, Comparable b) {
        return GreaterThanOrEquals(v, And(a, LessThanOrEquals(v, b)));
    }
    public static Interval Between(Value v, Interval i) {
        return Contains(i, v);
    }
    public static Comparable Min(Comparable a, Comparable b) {
        return LessThanOrEquals(a, b
            ? a
            : b
        );
    }
    public static Comparable Max(Comparable a, Comparable b) {
        return GreaterThanOrEquals(a, b
            ? a
            : b
        );
    }
}
public static partial class Extensions
{
    public static Boolean NotEquals(Equatable x, Equatable y) {
        return Not(Equals(x, y));
    }
}
public static partial class Extensions
{
    public static Number BlendEaseFunc(Number p, Function easeIn, Function easeOut) {
        return LessThan(p, 0.5
            ? Multiply(0.5, easeIn(Multiply(p, 2)))
            : Multiply(0.5, Add(easeOut(Multiply(p, Subtract(2, 1))), 0.5))
        );
    }
    public static Number InvertEaseFunc(Number p, Function easeIn) {
        return Subtract(1, easeIn(Subtract(1, p)));
    }
    public static Number Linear(Number p) {
        return p;
    }
    public static Number QuadraticEaseIn(Number p) {
        return Pow2(p);
    }
    public static Number QuadraticEaseOut(Number p) {
        return InvertEaseFunc(p, QuadraticEaseIn);
    }
    public static Number QuadraticEaseInOut(Number p) {
        return BlendEaseFunc(p, QuadraticEaseIn, QuadraticEaseOut);
    }
    public static Number CubicEaseIn(Number p) {
        return Pow3(p);
    }
    public static Number CubicEaseOut(Number p) {
        return InvertEaseFunc(p, CubicEaseIn);
    }
    public static Number CubicEaseInOut(Number p) {
        return BlendEaseFunc(p, CubicEaseIn, CubicEaseOut);
    }
    public static Number QuarticEaseIn(Number p) {
        return Pow4(p);
    }
    public static Number QuarticEaseOut(Number p) {
        return InvertEaseFunc(p, QuarticEaseIn);
    }
    public static Number QuarticEaseInOut(Number p) {
        return BlendEaseFunc(p, QuarticEaseIn, QuarticEaseOut);
    }
    public static Number QuinticEaseIn(Number p) {
        return Pow5(p);
    }
    public static Number QuinticEaseOut(Number p) {
        return InvertEaseFunc(p, QuinticEaseIn);
    }
    public static Number QuinticEaseInOut(Number p) {
        return BlendEaseFunc(p, QuinticEaseIn, QuinticEaseOut);
    }
    public static Number SineEaseIn(Number p) {
        return InvertEaseFunc(p, SineEaseOut);
    }
    public static Number SineEaseOut(Number p) {
        return Sin(Turns(Quarter(p)));
    }
    public static Number SineEaseInOut(Number p) {
        return BlendEaseFunc(p, SineEaseIn, SineEaseOut);
    }
    public static Number CircularEaseIn(Number p) {
        return FromOne(SquareRoot(FromOne(Pow2(p))));
    }
    public static Number CircularEaseOut(Number p) {
        return InvertEaseFunc(p, CircularEaseIn);
    }
    public static Number CircularEaseInOut(Number p) {
        return BlendEaseFunc(p, CircularEaseIn, CircularEaseOut);
    }
    public static Number ExponentialEaseIn(Number p) {
        return AlmostZero(p)
            ? p
            : Pow(2, Multiply(10, MinusOne(p)))
        ;
    }
    public static Number ExponentialEaseOut(Number p) {
        return InvertEaseFunc(p, ExponentialEaseIn);
    }
    public static Number ExponentialEaseInOut(Number p) {
        return BlendEaseFunc(p, ExponentialEaseIn, ExponentialEaseOut);
    }
    public static Number ElasticEaseIn(Number p) {
        return Multiply(13, Multiply(Turns(Quarter(p)), Sin(Radians(Pow(2, Multiply(10, MinusOne(p)))))));
    }
    public static Number ElasticEaseOut(Number p) {
        return InvertEaseFunc(p, ElasticEaseIn);
    }
    public static Number ElasticEaseInOut(Number p) {
        return BlendEaseFunc(p, ElasticEaseIn, ElasticEaseOut);
    }
    public static Number BackEaseIn(Number p) {
        return Subtract(Pow3(p), Multiply(p, Sin(Turns(Half(p)))));
    }
    public static Number BackEaseOut(Number p) {
        return InvertEaseFunc(p, BackEaseIn);
    }
    public static Number BackEaseInOut(Number p) {
        return BlendEaseFunc(p, BackEaseIn, BackEaseOut);
    }
    public static Number BounceEaseIn(Number p) {
        return InvertEaseFunc(p, BounceEaseOut);
    }
    public static Number BounceEaseOut(Number p) {
        return LessThan(p, Divide(4, 11))
            ? Multiply(121, Divide(Pow2(p), 16))
            : LessThan(p, Divide(8, 11))
                ? Divide(363, Multiply(40, Subtract(Pow2(p), Divide(99, Multiply(10, Add(p, Divide(17, 5)))))))
                : LessThan(p, Divide(9, 10))
                    ? Divide(4356, Multiply(361, Subtract(Pow2(p), Divide(35442, Multiply(1805, Add(p, Divide(16061, 1805)))))))
                    : Divide(54, Multiply(5, Subtract(Pow2(p), Divide(513, Multiply(25, Add(p, Divide(268, 25)))))))


        ;
    }
    public static Number BounceEaseInOut(Number p) {
        return BlendEaseFunc(p, BounceEaseIn, BounceEaseOut);
    }
}
