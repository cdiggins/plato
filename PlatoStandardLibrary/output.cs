public interface Any<Self>
    where Self : Any<Self>
{
    Array FieldNames();
    Array FieldValues(Self x);
    Array FieldTypes(Self x);
    Type TypeOf();
}
public static partial class Extensions
{
}
public interface Value<Self>: Any<Self>
    where Self : Value<Self>
{
    Self Default();
}
public static partial class Extensions
{
}
public interface Array<Self>: Any<Self>
    where Self : Array<Self>
{
    Integer Count(Self xs);
    T At(Self xs, Integer n);
}
public static partial class Extensions
{
}
public interface Vector<Self>: Array<Self>, Numerical<Self>
    where Self : Vector<Self>
{
}
public static partial class Extensions
{
}
public interface Measure<Self>: Value<Self>, ScalarArithmetic<Self>, Equatable<Self>, Comparable<Self>, Magnitudinal<Self>
    where Self : Measure<Self>
{
}
public static partial class Extensions
{
    public static Number Value<Self>(this Self x) where Self: Measure<Self>
    {
        return /*  */
        /*  */
        At(/*  */
        /*  */
        FieldValues(/*  */
        x), /*  */
        0);
    }
}
public interface Numerical<Self>: Value<Self>, Arithmetic<Self>, ScalarArithmetic<Self>, Equatable<Self>, Comparable<Self>, Magnitudinal<Self>
    where Self : Numerical<Self>
{
    Self Zero();
    Self One();
    Self MinValue();
    Self MaxValue();
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
    public static Number Magnitude<Self>(this Self x) where Self: Magnitudinal<Self>
    {
        return /*  */
        /*  */
        SquareRoot(/*  */
        /*  */
        Sum(/*  */
        /*  */
        Square(/*  */
        /*  */
        FieldValues(/*  */
        x))));
    }
}
public interface Comparable<Self>: Value<Self>
    where Self : Comparable<Self>
{
    Integer Compare(Self x, Self y);
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
    public static Boolean Equals<Self>(this Self a, Self b) where Self: Equatable<Self>
    {
        return /*  */
        /*  */
        All(/*  */
        /*  */
        Equals(/*  */
        /*  */
        FieldValues(/*  */
        a), /*  */
        /*  */
        FieldValues(/*  */
        b)));
    }
}
public interface Arithmetic<Self>: Value<Self>
    where Self : Arithmetic<Self>
{
}
public static partial class Extensions
{
    public static Self Add<Self>(this Self self, Self other) where Self: Arithmetic<Self>
    {
        return /*  */
        /*  */
        Add(/*  */
        /*  */
        FieldValues(/*  */
        self), /*  */
        /*  */
        FieldValues(/*  */
        other));
    }
    public static Self Subtract<Self>(this Self self, Self other) where Self: Arithmetic<Self>
    {
        return /*  */
        /*  */
        Subtract(/*  */
        /*  */
        FieldValues(/*  */
        self), /*  */
        /*  */
        FieldValues(/*  */
        other));
    }
    public static Self Negative<Self>(this Self self) where Self: Arithmetic<Self>
    {
        return /*  */
        /*  */
        Negative(/*  */
        /*  */
        FieldValues(/*  */
        self));
    }
    public static Self Reciprocal<Self>(this Self self) where Self: Arithmetic<Self>
    {
        return /*  */
        /*  */
        Reciprocal(/*  */
        /*  */
        FieldValues(/*  */
        self));
    }
    public static Self Multiply<Self>(this Self self, Self other) where Self: Arithmetic<Self>
    {
        return /*  */
        /*  */
        Add(/*  */
        /*  */
        FieldValues(/*  */
        self), /*  */
        /*  */
        FieldValues(/*  */
        other));
    }
    public static Self Divide<Self>(this Self self, Self other) where Self: Arithmetic<Self>
    {
        return /*  */
        /*  */
        Divide(/*  */
        /*  */
        FieldValues(/*  */
        self), /*  */
        /*  */
        FieldValues(/*  */
        other));
    }
    public static Self Modulo<Self>(this Self self, Self other) where Self: Arithmetic<Self>
    {
        return /*  */
        /*  */
        Modulo(/*  */
        /*  */
        FieldValues(/*  */
        self), /*  */
        /*  */
        FieldValues(/*  */
        other));
    }
}
public interface ScalarArithmetic<Self>: Value<Self>
    where Self : ScalarArithmetic<Self>
{
}
public static partial class Extensions
{
    public static Self Add<Self>(this Self self, Number scalar) where Self: ScalarArithmetic<Self>
    {
        return /*  */
        /*  */
        Add(/*  */
        /*  */
        FieldValues(/*  */
        self), /*  */
        scalar);
    }
    public static Self Subtract<Self>(this Self self, Number scalar) where Self: ScalarArithmetic<Self>
    {
        return /*  */
        /*  */
        Subtract(/*  */
        /*  */
        FieldValues(/*  */
        self), /*  */
        scalar);
    }
    public static Self Multiply<Self>(this Self self, Number scalar) where Self: ScalarArithmetic<Self>
    {
        return /*  */
        /*  */
        Multiply(/*  */
        /*  */
        FieldValues(/*  */
        self), /*  */
        scalar);
    }
    public static Self Divide<Self>(this Self self, Number scalar) where Self: ScalarArithmetic<Self>
    {
        return /*  */
        /*  */
        Divide(/*  */
        /*  */
        FieldValues(/*  */
        self), /*  */
        scalar);
    }
    public static Self Modulo<Self>(this Self self, Number scalar) where Self: ScalarArithmetic<Self>
    {
        return /*  */
        /*  */
        Modulo(/*  */
        /*  */
        FieldValues(/*  */
        self), /*  */
        scalar);
    }
}
public interface BooleanOperations<Self>: Value<Self>
    where Self : BooleanOperations<Self>
{
}
public static partial class Extensions
{
    public static Self And<Self>(this Self a, Self b) where Self: BooleanOperations<Self>
    {
        return /*  */
        /*  */
        And(/*  */
        /*  */
        FieldValues(/*  */
        a), /*  */
        /*  */
        FieldValues(/*  */
        b));
    }
    public static Self Or<Self>(this Self a, Self b) where Self: BooleanOperations<Self>
    {
        return /*  */
        /*  */
        Or(/*  */
        /*  */
        FieldValues(/*  */
        a), /*  */
        /*  */
        FieldValues(/*  */
        b));
    }
    public static Self Not<Self>(this Self a) where Self: BooleanOperations<Self>
    {
        return /*  */
        /*  */
        FieldValues(/*  */
        a);
    }
}
public interface Interval<Self>: Vector<Self>
    where Self : Interval<Self>
{
    T Min(Self x);
    T Max(Self x);
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
    public static Number Zero() => Extensions.Zero();
    public static Number One() => Extensions.One();
    public static Number MinValue() => Extensions.MinValue();
    public static Number MaxValue() => Extensions.MaxValue();
    public static Number Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Number x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Number x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Number operator +(Number self, Number other) => Extensions.Add(self, other);
    public static Number operator -(Number self, Number other) => Extensions.Subtract(self, other);
    public static Number operator -(Number self) => Extensions.Negative(self);
    public static Number operator *(Number self, Number other) => Extensions.Multiply(self, other);
    public static Number operator /(Number self, Number other) => Extensions.Divide(self, other);
    public static Number operator %(Number self, Number other) => Extensions.Modulo(self, other);
    public static Number operator +(Number self, Number scalar) => Extensions.Add(self, scalar);
    public static Number operator -(Number self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Number operator *(Number self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Number operator /(Number self, Number scalar) => Extensions.Divide(self, scalar);
    public static Number operator %(Number self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Number a, Number b) => Extensions.Equals(a, b);
    public static Integer Compare(Number x, Number y) => Extensions.Compare(x, y);
}
public class Integer: Numerical<Integer>
{
    public Integer() => () = ();
    public static Integer New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
    public static Integer Zero() => Extensions.Zero();
    public static Integer One() => Extensions.One();
    public static Integer MinValue() => Extensions.MinValue();
    public static Integer MaxValue() => Extensions.MaxValue();
    public static Integer Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Integer x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Integer x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Integer operator +(Integer self, Integer other) => Extensions.Add(self, other);
    public static Integer operator -(Integer self, Integer other) => Extensions.Subtract(self, other);
    public static Integer operator -(Integer self) => Extensions.Negative(self);
    public static Integer operator *(Integer self, Integer other) => Extensions.Multiply(self, other);
    public static Integer operator /(Integer self, Integer other) => Extensions.Divide(self, other);
    public static Integer operator %(Integer self, Integer other) => Extensions.Modulo(self, other);
    public static Integer operator +(Integer self, Number scalar) => Extensions.Add(self, scalar);
    public static Integer operator -(Integer self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Integer operator *(Integer self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Integer operator /(Integer self, Number scalar) => Extensions.Divide(self, scalar);
    public static Integer operator %(Integer self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Integer a, Integer b) => Extensions.Equals(a, b);
    public static Integer Compare(Integer x, Integer y) => Extensions.Compare(x, y);
}
public class String: Value<String>, Array<String>
{
    public String() => () = ();
    public static String New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
    public static String Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(String x) => Extensions.FieldValues(x);
    public static Array FieldTypes(String x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Integer Count(String xs) => Extensions.Count(xs);
    public static T At(String xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
}
public class Boolean: BooleanOperations<Boolean>
{
    public Boolean() => () = ();
    public static Boolean New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
    public static Boolean operator &&(Boolean a, Boolean b) => Extensions.And(a, b);
    public static Boolean operator ||(Boolean a, Boolean b) => Extensions.Or(a, b);
    public static Boolean operator !(Boolean a) => Extensions.Not(a);
    public static Boolean Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Boolean x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Boolean x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
}
public class Type: Value<Type>
{
    public Type(String _Name) => (Name) = (_Name);
    public static Type New(String _Name) => new(_Name);
    public static implicit operator String(Type self) => self.Name;
    public static implicit operator Type(String value) => new String(value);
    public string[] FieldNames() => new[] { "Name" };
    public object[] FieldValues() => new[] { Name };
    public static Type Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Type x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Type x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public String Name { get; }
}
public class Character: Value<Character>
{
    public Character() => () = ();
    public static Character New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
    public static Character Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Character x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Character x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
}
public class Count: Numerical<Count>
{
    public Count(Integer _Value) => (Value) = (_Value);
    public static Count New(Integer _Value) => new(_Value);
    public static implicit operator Integer(Count self) => self.Value;
    public static implicit operator Count(Integer value) => new Integer(value);
    public string[] FieldNames() => new[] { "Value" };
    public object[] FieldValues() => new[] { Value };
    public static Count Zero() => Extensions.Zero();
    public static Count One() => Extensions.One();
    public static Count MinValue() => Extensions.MinValue();
    public static Count MaxValue() => Extensions.MaxValue();
    public static Count Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Count x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Count x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Count operator +(Count self, Count other) => Extensions.Add(self, other);
    public static Count operator -(Count self, Count other) => Extensions.Subtract(self, other);
    public static Count operator -(Count self) => Extensions.Negative(self);
    public static Count operator *(Count self, Count other) => Extensions.Multiply(self, other);
    public static Count operator /(Count self, Count other) => Extensions.Divide(self, other);
    public static Count operator %(Count self, Count other) => Extensions.Modulo(self, other);
    public static Count operator +(Count self, Number scalar) => Extensions.Add(self, scalar);
    public static Count operator -(Count self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Count operator *(Count self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Count operator /(Count self, Number scalar) => Extensions.Divide(self, scalar);
    public static Count operator %(Count self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Count a, Count b) => Extensions.Equals(a, b);
    public static Integer Compare(Count x, Count y) => Extensions.Compare(x, y);
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
    public static Index Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Index x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Index x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Unit Zero() => Extensions.Zero();
    public static Unit One() => Extensions.One();
    public static Unit MinValue() => Extensions.MinValue();
    public static Unit MaxValue() => Extensions.MaxValue();
    public static Unit Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Unit x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Unit x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Unit operator +(Unit self, Unit other) => Extensions.Add(self, other);
    public static Unit operator -(Unit self, Unit other) => Extensions.Subtract(self, other);
    public static Unit operator -(Unit self) => Extensions.Negative(self);
    public static Unit operator *(Unit self, Unit other) => Extensions.Multiply(self, other);
    public static Unit operator /(Unit self, Unit other) => Extensions.Divide(self, other);
    public static Unit operator %(Unit self, Unit other) => Extensions.Modulo(self, other);
    public static Unit operator +(Unit self, Number scalar) => Extensions.Add(self, scalar);
    public static Unit operator -(Unit self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Unit operator *(Unit self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Unit operator /(Unit self, Number scalar) => Extensions.Divide(self, scalar);
    public static Unit operator %(Unit self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Unit a, Unit b) => Extensions.Equals(a, b);
    public static Integer Compare(Unit x, Unit y) => Extensions.Compare(x, y);
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
    public static Percent Zero() => Extensions.Zero();
    public static Percent One() => Extensions.One();
    public static Percent MinValue() => Extensions.MinValue();
    public static Percent MaxValue() => Extensions.MaxValue();
    public static Percent Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Percent x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Percent x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Percent operator +(Percent self, Percent other) => Extensions.Add(self, other);
    public static Percent operator -(Percent self, Percent other) => Extensions.Subtract(self, other);
    public static Percent operator -(Percent self) => Extensions.Negative(self);
    public static Percent operator *(Percent self, Percent other) => Extensions.Multiply(self, other);
    public static Percent operator /(Percent self, Percent other) => Extensions.Divide(self, other);
    public static Percent operator %(Percent self, Percent other) => Extensions.Modulo(self, other);
    public static Percent operator +(Percent self, Number scalar) => Extensions.Add(self, scalar);
    public static Percent operator -(Percent self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Percent operator *(Percent self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Percent operator /(Percent self, Number scalar) => Extensions.Divide(self, scalar);
    public static Percent operator %(Percent self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Percent a, Percent b) => Extensions.Equals(a, b);
    public static Integer Compare(Percent x, Percent y) => Extensions.Compare(x, y);
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
    public static Quaternion Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Quaternion x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Quaternion x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Unit2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Unit2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Unit2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Unit3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Unit3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Unit3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Direction3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Direction3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Direction3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static AxisAngle Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(AxisAngle x) => Extensions.FieldValues(x);
    public static Array FieldTypes(AxisAngle x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static EulerAngles Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(EulerAngles x) => Extensions.FieldValues(x);
    public static Array FieldTypes(EulerAngles x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Rotation3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Rotation3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Rotation3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Integer Count(Vector2D xs) => Extensions.Count(xs);
    public static T At(Vector2D xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Vector2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Vector2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Vector2D Zero() => Extensions.Zero();
    public static Vector2D One() => Extensions.One();
    public static Vector2D MinValue() => Extensions.MinValue();
    public static Vector2D MaxValue() => Extensions.MaxValue();
    public static Vector2D Default() => Extensions.Default();
    public static Vector2D operator +(Vector2D self, Vector2D other) => Extensions.Add(self, other);
    public static Vector2D operator -(Vector2D self, Vector2D other) => Extensions.Subtract(self, other);
    public static Vector2D operator -(Vector2D self) => Extensions.Negative(self);
    public static Vector2D operator *(Vector2D self, Vector2D other) => Extensions.Multiply(self, other);
    public static Vector2D operator /(Vector2D self, Vector2D other) => Extensions.Divide(self, other);
    public static Vector2D operator %(Vector2D self, Vector2D other) => Extensions.Modulo(self, other);
    public static Vector2D operator +(Vector2D self, Number scalar) => Extensions.Add(self, scalar);
    public static Vector2D operator -(Vector2D self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Vector2D operator *(Vector2D self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Vector2D operator /(Vector2D self, Number scalar) => Extensions.Divide(self, scalar);
    public static Vector2D operator %(Vector2D self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Vector2D a, Vector2D b) => Extensions.Equals(a, b);
    public static Integer Compare(Vector2D x, Vector2D y) => Extensions.Compare(x, y);
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
    public static Integer Count(Vector3D xs) => Extensions.Count(xs);
    public static T At(Vector3D xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Vector3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Vector3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Vector3D Zero() => Extensions.Zero();
    public static Vector3D One() => Extensions.One();
    public static Vector3D MinValue() => Extensions.MinValue();
    public static Vector3D MaxValue() => Extensions.MaxValue();
    public static Vector3D Default() => Extensions.Default();
    public static Vector3D operator +(Vector3D self, Vector3D other) => Extensions.Add(self, other);
    public static Vector3D operator -(Vector3D self, Vector3D other) => Extensions.Subtract(self, other);
    public static Vector3D operator -(Vector3D self) => Extensions.Negative(self);
    public static Vector3D operator *(Vector3D self, Vector3D other) => Extensions.Multiply(self, other);
    public static Vector3D operator /(Vector3D self, Vector3D other) => Extensions.Divide(self, other);
    public static Vector3D operator %(Vector3D self, Vector3D other) => Extensions.Modulo(self, other);
    public static Vector3D operator +(Vector3D self, Number scalar) => Extensions.Add(self, scalar);
    public static Vector3D operator -(Vector3D self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Vector3D operator *(Vector3D self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Vector3D operator /(Vector3D self, Number scalar) => Extensions.Divide(self, scalar);
    public static Vector3D operator %(Vector3D self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Vector3D a, Vector3D b) => Extensions.Equals(a, b);
    public static Integer Compare(Vector3D x, Vector3D y) => Extensions.Compare(x, y);
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
    public static Integer Count(Vector4D xs) => Extensions.Count(xs);
    public static T At(Vector4D xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Vector4D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Vector4D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Vector4D Zero() => Extensions.Zero();
    public static Vector4D One() => Extensions.One();
    public static Vector4D MinValue() => Extensions.MinValue();
    public static Vector4D MaxValue() => Extensions.MaxValue();
    public static Vector4D Default() => Extensions.Default();
    public static Vector4D operator +(Vector4D self, Vector4D other) => Extensions.Add(self, other);
    public static Vector4D operator -(Vector4D self, Vector4D other) => Extensions.Subtract(self, other);
    public static Vector4D operator -(Vector4D self) => Extensions.Negative(self);
    public static Vector4D operator *(Vector4D self, Vector4D other) => Extensions.Multiply(self, other);
    public static Vector4D operator /(Vector4D self, Vector4D other) => Extensions.Divide(self, other);
    public static Vector4D operator %(Vector4D self, Vector4D other) => Extensions.Modulo(self, other);
    public static Vector4D operator +(Vector4D self, Number scalar) => Extensions.Add(self, scalar);
    public static Vector4D operator -(Vector4D self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Vector4D operator *(Vector4D self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Vector4D operator /(Vector4D self, Number scalar) => Extensions.Divide(self, scalar);
    public static Vector4D operator %(Vector4D self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Vector4D a, Vector4D b) => Extensions.Equals(a, b);
    public static Integer Compare(Vector4D x, Vector4D y) => Extensions.Compare(x, y);
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
    public static Orientation3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Orientation3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Orientation3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Pose2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Pose2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Pose2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Pose3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Pose3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Pose3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Transform3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Transform3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Transform3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Transform2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Transform2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Transform2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static T Min(AlignedBox2D x) => Extensions.Min(x);
    public static T Max(AlignedBox2D x) => Extensions.Max(x);
    public static Integer Count(AlignedBox2D xs) => Extensions.Count(xs);
    public static T At(AlignedBox2D xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(AlignedBox2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(AlignedBox2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static AlignedBox2D Zero() => Extensions.Zero();
    public static AlignedBox2D One() => Extensions.One();
    public static AlignedBox2D MinValue() => Extensions.MinValue();
    public static AlignedBox2D MaxValue() => Extensions.MaxValue();
    public static AlignedBox2D Default() => Extensions.Default();
    public static AlignedBox2D operator +(AlignedBox2D self, AlignedBox2D other) => Extensions.Add(self, other);
    public static AlignedBox2D operator -(AlignedBox2D self, AlignedBox2D other) => Extensions.Subtract(self, other);
    public static AlignedBox2D operator -(AlignedBox2D self) => Extensions.Negative(self);
    public static AlignedBox2D operator *(AlignedBox2D self, AlignedBox2D other) => Extensions.Multiply(self, other);
    public static AlignedBox2D operator /(AlignedBox2D self, AlignedBox2D other) => Extensions.Divide(self, other);
    public static AlignedBox2D operator %(AlignedBox2D self, AlignedBox2D other) => Extensions.Modulo(self, other);
    public static AlignedBox2D operator +(AlignedBox2D self, Number scalar) => Extensions.Add(self, scalar);
    public static AlignedBox2D operator -(AlignedBox2D self, Number scalar) => Extensions.Subtract(self, scalar);
    public static AlignedBox2D operator *(AlignedBox2D self, Number scalar) => Extensions.Multiply(self, scalar);
    public static AlignedBox2D operator /(AlignedBox2D self, Number scalar) => Extensions.Divide(self, scalar);
    public static AlignedBox2D operator %(AlignedBox2D self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(AlignedBox2D a, AlignedBox2D b) => Extensions.Equals(a, b);
    public static Integer Compare(AlignedBox2D x, AlignedBox2D y) => Extensions.Compare(x, y);
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
    public static T Min(AlignedBox3D x) => Extensions.Min(x);
    public static T Max(AlignedBox3D x) => Extensions.Max(x);
    public static Integer Count(AlignedBox3D xs) => Extensions.Count(xs);
    public static T At(AlignedBox3D xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(AlignedBox3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(AlignedBox3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static AlignedBox3D Zero() => Extensions.Zero();
    public static AlignedBox3D One() => Extensions.One();
    public static AlignedBox3D MinValue() => Extensions.MinValue();
    public static AlignedBox3D MaxValue() => Extensions.MaxValue();
    public static AlignedBox3D Default() => Extensions.Default();
    public static AlignedBox3D operator +(AlignedBox3D self, AlignedBox3D other) => Extensions.Add(self, other);
    public static AlignedBox3D operator -(AlignedBox3D self, AlignedBox3D other) => Extensions.Subtract(self, other);
    public static AlignedBox3D operator -(AlignedBox3D self) => Extensions.Negative(self);
    public static AlignedBox3D operator *(AlignedBox3D self, AlignedBox3D other) => Extensions.Multiply(self, other);
    public static AlignedBox3D operator /(AlignedBox3D self, AlignedBox3D other) => Extensions.Divide(self, other);
    public static AlignedBox3D operator %(AlignedBox3D self, AlignedBox3D other) => Extensions.Modulo(self, other);
    public static AlignedBox3D operator +(AlignedBox3D self, Number scalar) => Extensions.Add(self, scalar);
    public static AlignedBox3D operator -(AlignedBox3D self, Number scalar) => Extensions.Subtract(self, scalar);
    public static AlignedBox3D operator *(AlignedBox3D self, Number scalar) => Extensions.Multiply(self, scalar);
    public static AlignedBox3D operator /(AlignedBox3D self, Number scalar) => Extensions.Divide(self, scalar);
    public static AlignedBox3D operator %(AlignedBox3D self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(AlignedBox3D a, AlignedBox3D b) => Extensions.Equals(a, b);
    public static Integer Compare(AlignedBox3D x, AlignedBox3D y) => Extensions.Compare(x, y);
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
    public static Integer Count(Complex xs) => Extensions.Count(xs);
    public static T At(Complex xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Complex x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Complex x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Complex Zero() => Extensions.Zero();
    public static Complex One() => Extensions.One();
    public static Complex MinValue() => Extensions.MinValue();
    public static Complex MaxValue() => Extensions.MaxValue();
    public static Complex Default() => Extensions.Default();
    public static Complex operator +(Complex self, Complex other) => Extensions.Add(self, other);
    public static Complex operator -(Complex self, Complex other) => Extensions.Subtract(self, other);
    public static Complex operator -(Complex self) => Extensions.Negative(self);
    public static Complex operator *(Complex self, Complex other) => Extensions.Multiply(self, other);
    public static Complex operator /(Complex self, Complex other) => Extensions.Divide(self, other);
    public static Complex operator %(Complex self, Complex other) => Extensions.Modulo(self, other);
    public static Complex operator +(Complex self, Number scalar) => Extensions.Add(self, scalar);
    public static Complex operator -(Complex self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Complex operator *(Complex self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Complex operator /(Complex self, Number scalar) => Extensions.Divide(self, scalar);
    public static Complex operator %(Complex self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Complex a, Complex b) => Extensions.Equals(a, b);
    public static Integer Compare(Complex x, Complex y) => Extensions.Compare(x, y);
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
    public static Ray3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Ray3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Ray3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Ray2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Ray2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Ray2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Sphere Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Sphere x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Sphere x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Plane Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Plane x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Plane x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Triangle3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Triangle3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Triangle3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Triangle2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Triangle2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Triangle2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Quad3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Quad3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Quad3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Quad2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Quad2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Quad2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Point3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Point3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Point3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Point2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Point2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Point2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static T Min(Line3D x) => Extensions.Min(x);
    public static T Max(Line3D x) => Extensions.Max(x);
    public static Integer Count(Line3D xs) => Extensions.Count(xs);
    public static T At(Line3D xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Line3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Line3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Line3D Zero() => Extensions.Zero();
    public static Line3D One() => Extensions.One();
    public static Line3D MinValue() => Extensions.MinValue();
    public static Line3D MaxValue() => Extensions.MaxValue();
    public static Line3D Default() => Extensions.Default();
    public static Line3D operator +(Line3D self, Line3D other) => Extensions.Add(self, other);
    public static Line3D operator -(Line3D self, Line3D other) => Extensions.Subtract(self, other);
    public static Line3D operator -(Line3D self) => Extensions.Negative(self);
    public static Line3D operator *(Line3D self, Line3D other) => Extensions.Multiply(self, other);
    public static Line3D operator /(Line3D self, Line3D other) => Extensions.Divide(self, other);
    public static Line3D operator %(Line3D self, Line3D other) => Extensions.Modulo(self, other);
    public static Line3D operator +(Line3D self, Number scalar) => Extensions.Add(self, scalar);
    public static Line3D operator -(Line3D self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Line3D operator *(Line3D self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Line3D operator /(Line3D self, Number scalar) => Extensions.Divide(self, scalar);
    public static Line3D operator %(Line3D self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Line3D a, Line3D b) => Extensions.Equals(a, b);
    public static Integer Compare(Line3D x, Line3D y) => Extensions.Compare(x, y);
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
    public static T Min(Line2D x) => Extensions.Min(x);
    public static T Max(Line2D x) => Extensions.Max(x);
    public static Integer Count(Line2D xs) => Extensions.Count(xs);
    public static T At(Line2D xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Line2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Line2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Line2D Zero() => Extensions.Zero();
    public static Line2D One() => Extensions.One();
    public static Line2D MinValue() => Extensions.MinValue();
    public static Line2D MaxValue() => Extensions.MaxValue();
    public static Line2D Default() => Extensions.Default();
    public static Line2D operator +(Line2D self, Line2D other) => Extensions.Add(self, other);
    public static Line2D operator -(Line2D self, Line2D other) => Extensions.Subtract(self, other);
    public static Line2D operator -(Line2D self) => Extensions.Negative(self);
    public static Line2D operator *(Line2D self, Line2D other) => Extensions.Multiply(self, other);
    public static Line2D operator /(Line2D self, Line2D other) => Extensions.Divide(self, other);
    public static Line2D operator %(Line2D self, Line2D other) => Extensions.Modulo(self, other);
    public static Line2D operator +(Line2D self, Number scalar) => Extensions.Add(self, scalar);
    public static Line2D operator -(Line2D self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Line2D operator *(Line2D self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Line2D operator /(Line2D self, Number scalar) => Extensions.Divide(self, scalar);
    public static Line2D operator %(Line2D self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Line2D a, Line2D b) => Extensions.Equals(a, b);
    public static Integer Compare(Line2D x, Line2D y) => Extensions.Compare(x, y);
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
    public static Color Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Color x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Color x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static ColorLUV Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(ColorLUV x) => Extensions.FieldValues(x);
    public static Array FieldTypes(ColorLUV x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static ColorLAB Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(ColorLAB x) => Extensions.FieldValues(x);
    public static Array FieldTypes(ColorLAB x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static ColorLCh Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(ColorLCh x) => Extensions.FieldValues(x);
    public static Array FieldTypes(ColorLCh x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static ColorHSV Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(ColorHSV x) => Extensions.FieldValues(x);
    public static Array FieldTypes(ColorHSV x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static ColorHSL Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(ColorHSL x) => Extensions.FieldValues(x);
    public static Array FieldTypes(ColorHSL x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static ColorYCbCr Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(ColorYCbCr x) => Extensions.FieldValues(x);
    public static Array FieldTypes(ColorYCbCr x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static SphericalCoordinate Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(SphericalCoordinate x) => Extensions.FieldValues(x);
    public static Array FieldTypes(SphericalCoordinate x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static PolarCoordinate Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(PolarCoordinate x) => Extensions.FieldValues(x);
    public static Array FieldTypes(PolarCoordinate x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static LogPolarCoordinate Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(LogPolarCoordinate x) => Extensions.FieldValues(x);
    public static Array FieldTypes(LogPolarCoordinate x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static CylindricalCoordinate Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(CylindricalCoordinate x) => Extensions.FieldValues(x);
    public static Array FieldTypes(CylindricalCoordinate x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static HorizontalCoordinate Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(HorizontalCoordinate x) => Extensions.FieldValues(x);
    public static Array FieldTypes(HorizontalCoordinate x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static GeoCoordinate Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(GeoCoordinate x) => Extensions.FieldValues(x);
    public static Array FieldTypes(GeoCoordinate x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static GeoCoordinateWithAltitude Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(GeoCoordinateWithAltitude x) => Extensions.FieldValues(x);
    public static Array FieldTypes(GeoCoordinateWithAltitude x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Circle Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Circle x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Circle x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Chord Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Chord x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Chord x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Size2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Size2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Size2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Size3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Size3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Size3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Rectangle2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Rectangle2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Rectangle2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Proportion Zero() => Extensions.Zero();
    public static Proportion One() => Extensions.One();
    public static Proportion MinValue() => Extensions.MinValue();
    public static Proportion MaxValue() => Extensions.MaxValue();
    public static Proportion Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Proportion x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Proportion x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Proportion operator +(Proportion self, Proportion other) => Extensions.Add(self, other);
    public static Proportion operator -(Proportion self, Proportion other) => Extensions.Subtract(self, other);
    public static Proportion operator -(Proportion self) => Extensions.Negative(self);
    public static Proportion operator *(Proportion self, Proportion other) => Extensions.Multiply(self, other);
    public static Proportion operator /(Proportion self, Proportion other) => Extensions.Divide(self, other);
    public static Proportion operator %(Proportion self, Proportion other) => Extensions.Modulo(self, other);
    public static Proportion operator +(Proportion self, Number scalar) => Extensions.Add(self, scalar);
    public static Proportion operator -(Proportion self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Proportion operator *(Proportion self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Proportion operator /(Proportion self, Number scalar) => Extensions.Divide(self, scalar);
    public static Proportion operator %(Proportion self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Proportion a, Proportion b) => Extensions.Equals(a, b);
    public static Integer Compare(Proportion x, Proportion y) => Extensions.Compare(x, y);
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
    public static Fraction Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Fraction x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Fraction x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Angle Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Angle x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Angle x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Angle operator +(Angle self, Number scalar) => Extensions.Add(self, scalar);
    public static Angle operator -(Angle self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Angle operator *(Angle self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Angle operator /(Angle self, Number scalar) => Extensions.Divide(self, scalar);
    public static Angle operator %(Angle self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Angle a, Angle b) => Extensions.Equals(a, b);
    public static Integer Compare(Angle x, Angle y) => Extensions.Compare(x, y);
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
    public static Length Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Length x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Length x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Length operator +(Length self, Number scalar) => Extensions.Add(self, scalar);
    public static Length operator -(Length self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Length operator *(Length self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Length operator /(Length self, Number scalar) => Extensions.Divide(self, scalar);
    public static Length operator %(Length self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Length a, Length b) => Extensions.Equals(a, b);
    public static Integer Compare(Length x, Length y) => Extensions.Compare(x, y);
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
    public static Mass Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Mass x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Mass x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Mass operator +(Mass self, Number scalar) => Extensions.Add(self, scalar);
    public static Mass operator -(Mass self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Mass operator *(Mass self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Mass operator /(Mass self, Number scalar) => Extensions.Divide(self, scalar);
    public static Mass operator %(Mass self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Mass a, Mass b) => Extensions.Equals(a, b);
    public static Integer Compare(Mass x, Mass y) => Extensions.Compare(x, y);
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
    public static Temperature Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Temperature x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Temperature x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Temperature operator +(Temperature self, Number scalar) => Extensions.Add(self, scalar);
    public static Temperature operator -(Temperature self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Temperature operator *(Temperature self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Temperature operator /(Temperature self, Number scalar) => Extensions.Divide(self, scalar);
    public static Temperature operator %(Temperature self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Temperature a, Temperature b) => Extensions.Equals(a, b);
    public static Integer Compare(Temperature x, Temperature y) => Extensions.Compare(x, y);
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
    public static TimeSpan Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(TimeSpan x) => Extensions.FieldValues(x);
    public static Array FieldTypes(TimeSpan x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static TimeSpan operator +(TimeSpan self, Number scalar) => Extensions.Add(self, scalar);
    public static TimeSpan operator -(TimeSpan self, Number scalar) => Extensions.Subtract(self, scalar);
    public static TimeSpan operator *(TimeSpan self, Number scalar) => Extensions.Multiply(self, scalar);
    public static TimeSpan operator /(TimeSpan self, Number scalar) => Extensions.Divide(self, scalar);
    public static TimeSpan operator %(TimeSpan self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(TimeSpan a, TimeSpan b) => Extensions.Equals(a, b);
    public static Integer Compare(TimeSpan x, TimeSpan y) => Extensions.Compare(x, y);
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
    public static T Min(TimeRange x) => Extensions.Min(x);
    public static T Max(TimeRange x) => Extensions.Max(x);
    public static Integer Count(TimeRange xs) => Extensions.Count(xs);
    public static T At(TimeRange xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(TimeRange x) => Extensions.FieldValues(x);
    public static Array FieldTypes(TimeRange x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static TimeRange Zero() => Extensions.Zero();
    public static TimeRange One() => Extensions.One();
    public static TimeRange MinValue() => Extensions.MinValue();
    public static TimeRange MaxValue() => Extensions.MaxValue();
    public static TimeRange Default() => Extensions.Default();
    public static TimeRange operator +(TimeRange self, TimeRange other) => Extensions.Add(self, other);
    public static TimeRange operator -(TimeRange self, TimeRange other) => Extensions.Subtract(self, other);
    public static TimeRange operator -(TimeRange self) => Extensions.Negative(self);
    public static TimeRange operator *(TimeRange self, TimeRange other) => Extensions.Multiply(self, other);
    public static TimeRange operator /(TimeRange self, TimeRange other) => Extensions.Divide(self, other);
    public static TimeRange operator %(TimeRange self, TimeRange other) => Extensions.Modulo(self, other);
    public static TimeRange operator +(TimeRange self, Number scalar) => Extensions.Add(self, scalar);
    public static TimeRange operator -(TimeRange self, Number scalar) => Extensions.Subtract(self, scalar);
    public static TimeRange operator *(TimeRange self, Number scalar) => Extensions.Multiply(self, scalar);
    public static TimeRange operator /(TimeRange self, Number scalar) => Extensions.Divide(self, scalar);
    public static TimeRange operator %(TimeRange self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(TimeRange a, TimeRange b) => Extensions.Equals(a, b);
    public static Integer Compare(TimeRange x, TimeRange y) => Extensions.Compare(x, y);
    public DateTime Min { get; }
    public DateTime Max { get; }
}
public class DateTime: Value<DateTime>
{
    public DateTime(Integer _Year, Integer _Month, Integer _TimeZoneOffset, Integer _Day, Integer _Minute, Integer _Second, Number _Milliseconds) => (Year, Month, TimeZoneOffset, Day, Minute, Second, Milliseconds) = (_Year, _Month, _TimeZoneOffset, _Day, _Minute, _Second, _Milliseconds);
    public static DateTime New(Integer _Year, Integer _Month, Integer _TimeZoneOffset, Integer _Day, Integer _Minute, Integer _Second, Number _Milliseconds) => new(_Year, _Month, _TimeZoneOffset, _Day, _Minute, _Second, _Milliseconds);
    public static implicit operator (Integer, Integer, Integer, Integer, Integer, Integer, Number)(DateTime self) => (self.Year, self.Month, self.TimeZoneOffset, self.Day, self.Minute, self.Second, self.Milliseconds);
    public static implicit operator DateTime((Integer, Integer, Integer, Integer, Integer, Integer, Number) value) => new DateTime(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7);
    public string[] FieldNames() => new[] { "Year", "Month", "TimeZoneOffset", "Day", "Minute", "Second", "Milliseconds" };
    public object[] FieldValues() => new[] { Year, Month, TimeZoneOffset, Day, Minute, Second, Milliseconds };
    public static DateTime Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(DateTime x) => Extensions.FieldValues(x);
    public static Array FieldTypes(DateTime x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public Integer Year { get; }
    public Integer Month { get; }
    public Integer TimeZoneOffset { get; }
    public Integer Day { get; }
    public Integer Minute { get; }
    public Integer Second { get; }
    public Number Milliseconds { get; }
}
public class AnglePair: Interval<AnglePair>
{
    public AnglePair(Angle _Start, Angle _End) => (Start, End) = (_Start, _End);
    public static AnglePair New(Angle _Start, Angle _End) => new(_Start, _End);
    public static implicit operator (Angle, Angle)(AnglePair self) => (self.Start, self.End);
    public static implicit operator AnglePair((Angle, Angle) value) => new AnglePair(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Start", "End" };
    public object[] FieldValues() => new[] { Start, End };
    public static T Min(AnglePair x) => Extensions.Min(x);
    public static T Max(AnglePair x) => Extensions.Max(x);
    public static Integer Count(AnglePair xs) => Extensions.Count(xs);
    public static T At(AnglePair xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(AnglePair x) => Extensions.FieldValues(x);
    public static Array FieldTypes(AnglePair x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static AnglePair Zero() => Extensions.Zero();
    public static AnglePair One() => Extensions.One();
    public static AnglePair MinValue() => Extensions.MinValue();
    public static AnglePair MaxValue() => Extensions.MaxValue();
    public static AnglePair Default() => Extensions.Default();
    public static AnglePair operator +(AnglePair self, AnglePair other) => Extensions.Add(self, other);
    public static AnglePair operator -(AnglePair self, AnglePair other) => Extensions.Subtract(self, other);
    public static AnglePair operator -(AnglePair self) => Extensions.Negative(self);
    public static AnglePair operator *(AnglePair self, AnglePair other) => Extensions.Multiply(self, other);
    public static AnglePair operator /(AnglePair self, AnglePair other) => Extensions.Divide(self, other);
    public static AnglePair operator %(AnglePair self, AnglePair other) => Extensions.Modulo(self, other);
    public static AnglePair operator +(AnglePair self, Number scalar) => Extensions.Add(self, scalar);
    public static AnglePair operator -(AnglePair self, Number scalar) => Extensions.Subtract(self, scalar);
    public static AnglePair operator *(AnglePair self, Number scalar) => Extensions.Multiply(self, scalar);
    public static AnglePair operator /(AnglePair self, Number scalar) => Extensions.Divide(self, scalar);
    public static AnglePair operator %(AnglePair self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(AnglePair a, AnglePair b) => Extensions.Equals(a, b);
    public static Integer Compare(AnglePair x, AnglePair y) => Extensions.Compare(x, y);
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
    public static Ring Zero() => Extensions.Zero();
    public static Ring One() => Extensions.One();
    public static Ring MinValue() => Extensions.MinValue();
    public static Ring MaxValue() => Extensions.MaxValue();
    public static Ring Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Ring x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Ring x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Ring operator +(Ring self, Ring other) => Extensions.Add(self, other);
    public static Ring operator -(Ring self, Ring other) => Extensions.Subtract(self, other);
    public static Ring operator -(Ring self) => Extensions.Negative(self);
    public static Ring operator *(Ring self, Ring other) => Extensions.Multiply(self, other);
    public static Ring operator /(Ring self, Ring other) => Extensions.Divide(self, other);
    public static Ring operator %(Ring self, Ring other) => Extensions.Modulo(self, other);
    public static Ring operator +(Ring self, Number scalar) => Extensions.Add(self, scalar);
    public static Ring operator -(Ring self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Ring operator *(Ring self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Ring operator /(Ring self, Number scalar) => Extensions.Divide(self, scalar);
    public static Ring operator %(Ring self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Ring a, Ring b) => Extensions.Equals(a, b);
    public static Integer Compare(Ring x, Ring y) => Extensions.Compare(x, y);
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
    public static Arc Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Arc x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Arc x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static T Min(TimeInterval x) => Extensions.Min(x);
    public static T Max(TimeInterval x) => Extensions.Max(x);
    public static Integer Count(TimeInterval xs) => Extensions.Count(xs);
    public static T At(TimeInterval xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(TimeInterval x) => Extensions.FieldValues(x);
    public static Array FieldTypes(TimeInterval x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static TimeInterval Zero() => Extensions.Zero();
    public static TimeInterval One() => Extensions.One();
    public static TimeInterval MinValue() => Extensions.MinValue();
    public static TimeInterval MaxValue() => Extensions.MaxValue();
    public static TimeInterval Default() => Extensions.Default();
    public static TimeInterval operator +(TimeInterval self, TimeInterval other) => Extensions.Add(self, other);
    public static TimeInterval operator -(TimeInterval self, TimeInterval other) => Extensions.Subtract(self, other);
    public static TimeInterval operator -(TimeInterval self) => Extensions.Negative(self);
    public static TimeInterval operator *(TimeInterval self, TimeInterval other) => Extensions.Multiply(self, other);
    public static TimeInterval operator /(TimeInterval self, TimeInterval other) => Extensions.Divide(self, other);
    public static TimeInterval operator %(TimeInterval self, TimeInterval other) => Extensions.Modulo(self, other);
    public static TimeInterval operator +(TimeInterval self, Number scalar) => Extensions.Add(self, scalar);
    public static TimeInterval operator -(TimeInterval self, Number scalar) => Extensions.Subtract(self, scalar);
    public static TimeInterval operator *(TimeInterval self, Number scalar) => Extensions.Multiply(self, scalar);
    public static TimeInterval operator /(TimeInterval self, Number scalar) => Extensions.Divide(self, scalar);
    public static TimeInterval operator %(TimeInterval self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(TimeInterval a, TimeInterval b) => Extensions.Equals(a, b);
    public static Integer Compare(TimeInterval x, TimeInterval y) => Extensions.Compare(x, y);
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
    public static T Min(RealInterval x) => Extensions.Min(x);
    public static T Max(RealInterval x) => Extensions.Max(x);
    public static Integer Count(RealInterval xs) => Extensions.Count(xs);
    public static T At(RealInterval xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(RealInterval x) => Extensions.FieldValues(x);
    public static Array FieldTypes(RealInterval x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static RealInterval Zero() => Extensions.Zero();
    public static RealInterval One() => Extensions.One();
    public static RealInterval MinValue() => Extensions.MinValue();
    public static RealInterval MaxValue() => Extensions.MaxValue();
    public static RealInterval Default() => Extensions.Default();
    public static RealInterval operator +(RealInterval self, RealInterval other) => Extensions.Add(self, other);
    public static RealInterval operator -(RealInterval self, RealInterval other) => Extensions.Subtract(self, other);
    public static RealInterval operator -(RealInterval self) => Extensions.Negative(self);
    public static RealInterval operator *(RealInterval self, RealInterval other) => Extensions.Multiply(self, other);
    public static RealInterval operator /(RealInterval self, RealInterval other) => Extensions.Divide(self, other);
    public static RealInterval operator %(RealInterval self, RealInterval other) => Extensions.Modulo(self, other);
    public static RealInterval operator +(RealInterval self, Number scalar) => Extensions.Add(self, scalar);
    public static RealInterval operator -(RealInterval self, Number scalar) => Extensions.Subtract(self, scalar);
    public static RealInterval operator *(RealInterval self, Number scalar) => Extensions.Multiply(self, scalar);
    public static RealInterval operator /(RealInterval self, Number scalar) => Extensions.Divide(self, scalar);
    public static RealInterval operator %(RealInterval self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(RealInterval a, RealInterval b) => Extensions.Equals(a, b);
    public static Integer Compare(RealInterval x, RealInterval y) => Extensions.Compare(x, y);
    public Number A { get; }
    public Number B { get; }
}
public class Capsule: Value<Capsule>
{
    public Capsule(Line3D _Line, Number _Radius) => (Line, Radius) = (_Line, _Radius);
    public static Capsule New(Line3D _Line, Number _Radius) => new(_Line, _Radius);
    public static implicit operator (Line3D, Number)(Capsule self) => (self.Line, self.Radius);
    public static implicit operator Capsule((Line3D, Number) value) => new Capsule(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Line", "Radius" };
    public object[] FieldValues() => new[] { Line, Radius };
    public static Capsule Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Capsule x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Capsule x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Matrix3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Matrix3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Matrix3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Cylinder Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Cylinder x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Cylinder x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Cone Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Cone x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Cone x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Tube Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Tube x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Tube x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static ConeSegment Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(ConeSegment x) => Extensions.FieldValues(x);
    public static Array FieldTypes(ConeSegment x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Box2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Box2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Box2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Box3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Box3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Box3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static CubicBezierTriangle3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(CubicBezierTriangle3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(CubicBezierTriangle3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static CubicBezier2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(CubicBezier2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(CubicBezier2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Integer Count(UV xs) => Extensions.Count(xs);
    public static T At(UV xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(UV x) => Extensions.FieldValues(x);
    public static Array FieldTypes(UV x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static UV Zero() => Extensions.Zero();
    public static UV One() => Extensions.One();
    public static UV MinValue() => Extensions.MinValue();
    public static UV MaxValue() => Extensions.MaxValue();
    public static UV Default() => Extensions.Default();
    public static UV operator +(UV self, UV other) => Extensions.Add(self, other);
    public static UV operator -(UV self, UV other) => Extensions.Subtract(self, other);
    public static UV operator -(UV self) => Extensions.Negative(self);
    public static UV operator *(UV self, UV other) => Extensions.Multiply(self, other);
    public static UV operator /(UV self, UV other) => Extensions.Divide(self, other);
    public static UV operator %(UV self, UV other) => Extensions.Modulo(self, other);
    public static UV operator +(UV self, Number scalar) => Extensions.Add(self, scalar);
    public static UV operator -(UV self, Number scalar) => Extensions.Subtract(self, scalar);
    public static UV operator *(UV self, Number scalar) => Extensions.Multiply(self, scalar);
    public static UV operator /(UV self, Number scalar) => Extensions.Divide(self, scalar);
    public static UV operator %(UV self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(UV a, UV b) => Extensions.Equals(a, b);
    public static Integer Compare(UV x, UV y) => Extensions.Compare(x, y);
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
    public static Integer Count(UVW xs) => Extensions.Count(xs);
    public static T At(UVW xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(UVW x) => Extensions.FieldValues(x);
    public static Array FieldTypes(UVW x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static UVW Zero() => Extensions.Zero();
    public static UVW One() => Extensions.One();
    public static UVW MinValue() => Extensions.MinValue();
    public static UVW MaxValue() => Extensions.MaxValue();
    public static UVW Default() => Extensions.Default();
    public static UVW operator +(UVW self, UVW other) => Extensions.Add(self, other);
    public static UVW operator -(UVW self, UVW other) => Extensions.Subtract(self, other);
    public static UVW operator -(UVW self) => Extensions.Negative(self);
    public static UVW operator *(UVW self, UVW other) => Extensions.Multiply(self, other);
    public static UVW operator /(UVW self, UVW other) => Extensions.Divide(self, other);
    public static UVW operator %(UVW self, UVW other) => Extensions.Modulo(self, other);
    public static UVW operator +(UVW self, Number scalar) => Extensions.Add(self, scalar);
    public static UVW operator -(UVW self, Number scalar) => Extensions.Subtract(self, scalar);
    public static UVW operator *(UVW self, Number scalar) => Extensions.Multiply(self, scalar);
    public static UVW operator /(UVW self, Number scalar) => Extensions.Divide(self, scalar);
    public static UVW operator %(UVW self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(UVW a, UVW b) => Extensions.Equals(a, b);
    public static Integer Compare(UVW x, UVW y) => Extensions.Compare(x, y);
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
    public static CubicBezier3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(CubicBezier3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(CubicBezier3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static QuadraticBezier2D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(QuadraticBezier2D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(QuadraticBezier2D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static QuadraticBezier3D Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(QuadraticBezier3D x) => Extensions.FieldValues(x);
    public static Array FieldTypes(QuadraticBezier3D x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Area Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Area x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Area x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Area operator +(Area self, Number scalar) => Extensions.Add(self, scalar);
    public static Area operator -(Area self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Area operator *(Area self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Area operator /(Area self, Number scalar) => Extensions.Divide(self, scalar);
    public static Area operator %(Area self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Area a, Area b) => Extensions.Equals(a, b);
    public static Integer Compare(Area x, Area y) => Extensions.Compare(x, y);
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
    public static Volume Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Volume x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Volume x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Volume operator +(Volume self, Number scalar) => Extensions.Add(self, scalar);
    public static Volume operator -(Volume self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Volume operator *(Volume self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Volume operator /(Volume self, Number scalar) => Extensions.Divide(self, scalar);
    public static Volume operator %(Volume self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Volume a, Volume b) => Extensions.Equals(a, b);
    public static Integer Compare(Volume x, Volume y) => Extensions.Compare(x, y);
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
    public static Velocity Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Velocity x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Velocity x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Velocity operator +(Velocity self, Number scalar) => Extensions.Add(self, scalar);
    public static Velocity operator -(Velocity self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Velocity operator *(Velocity self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Velocity operator /(Velocity self, Number scalar) => Extensions.Divide(self, scalar);
    public static Velocity operator %(Velocity self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Velocity a, Velocity b) => Extensions.Equals(a, b);
    public static Integer Compare(Velocity x, Velocity y) => Extensions.Compare(x, y);
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
    public static Acceleration Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Acceleration x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Acceleration x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Acceleration operator +(Acceleration self, Number scalar) => Extensions.Add(self, scalar);
    public static Acceleration operator -(Acceleration self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Acceleration operator *(Acceleration self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Acceleration operator /(Acceleration self, Number scalar) => Extensions.Divide(self, scalar);
    public static Acceleration operator %(Acceleration self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Acceleration a, Acceleration b) => Extensions.Equals(a, b);
    public static Integer Compare(Acceleration x, Acceleration y) => Extensions.Compare(x, y);
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
    public static Force Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Force x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Force x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Force operator +(Force self, Number scalar) => Extensions.Add(self, scalar);
    public static Force operator -(Force self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Force operator *(Force self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Force operator /(Force self, Number scalar) => Extensions.Divide(self, scalar);
    public static Force operator %(Force self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Force a, Force b) => Extensions.Equals(a, b);
    public static Integer Compare(Force x, Force y) => Extensions.Compare(x, y);
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
    public static Pressure Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Pressure x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Pressure x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Pressure operator +(Pressure self, Number scalar) => Extensions.Add(self, scalar);
    public static Pressure operator -(Pressure self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Pressure operator *(Pressure self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Pressure operator /(Pressure self, Number scalar) => Extensions.Divide(self, scalar);
    public static Pressure operator %(Pressure self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Pressure a, Pressure b) => Extensions.Equals(a, b);
    public static Integer Compare(Pressure x, Pressure y) => Extensions.Compare(x, y);
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
    public static Energy Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Energy x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Energy x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Energy operator +(Energy self, Number scalar) => Extensions.Add(self, scalar);
    public static Energy operator -(Energy self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Energy operator *(Energy self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Energy operator /(Energy self, Number scalar) => Extensions.Divide(self, scalar);
    public static Energy operator %(Energy self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Energy a, Energy b) => Extensions.Equals(a, b);
    public static Integer Compare(Energy x, Energy y) => Extensions.Compare(x, y);
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
    public static Memory Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Memory x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Memory x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Memory operator +(Memory self, Number scalar) => Extensions.Add(self, scalar);
    public static Memory operator -(Memory self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Memory operator *(Memory self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Memory operator /(Memory self, Number scalar) => Extensions.Divide(self, scalar);
    public static Memory operator %(Memory self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Memory a, Memory b) => Extensions.Equals(a, b);
    public static Integer Compare(Memory x, Memory y) => Extensions.Compare(x, y);
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
    public static Frequency Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Frequency x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Frequency x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Frequency operator +(Frequency self, Number scalar) => Extensions.Add(self, scalar);
    public static Frequency operator -(Frequency self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Frequency operator *(Frequency self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Frequency operator /(Frequency self, Number scalar) => Extensions.Divide(self, scalar);
    public static Frequency operator %(Frequency self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Frequency a, Frequency b) => Extensions.Equals(a, b);
    public static Integer Compare(Frequency x, Frequency y) => Extensions.Compare(x, y);
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
    public static Loudness Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Loudness x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Loudness x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Loudness operator +(Loudness self, Number scalar) => Extensions.Add(self, scalar);
    public static Loudness operator -(Loudness self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Loudness operator *(Loudness self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Loudness operator /(Loudness self, Number scalar) => Extensions.Divide(self, scalar);
    public static Loudness operator %(Loudness self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Loudness a, Loudness b) => Extensions.Equals(a, b);
    public static Integer Compare(Loudness x, Loudness y) => Extensions.Compare(x, y);
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
    public static LuminousIntensity Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(LuminousIntensity x) => Extensions.FieldValues(x);
    public static Array FieldTypes(LuminousIntensity x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static LuminousIntensity operator +(LuminousIntensity self, Number scalar) => Extensions.Add(self, scalar);
    public static LuminousIntensity operator -(LuminousIntensity self, Number scalar) => Extensions.Subtract(self, scalar);
    public static LuminousIntensity operator *(LuminousIntensity self, Number scalar) => Extensions.Multiply(self, scalar);
    public static LuminousIntensity operator /(LuminousIntensity self, Number scalar) => Extensions.Divide(self, scalar);
    public static LuminousIntensity operator %(LuminousIntensity self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(LuminousIntensity a, LuminousIntensity b) => Extensions.Equals(a, b);
    public static Integer Compare(LuminousIntensity x, LuminousIntensity y) => Extensions.Compare(x, y);
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
    public static ElectricPotential Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(ElectricPotential x) => Extensions.FieldValues(x);
    public static Array FieldTypes(ElectricPotential x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static ElectricPotential operator +(ElectricPotential self, Number scalar) => Extensions.Add(self, scalar);
    public static ElectricPotential operator -(ElectricPotential self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ElectricPotential operator *(ElectricPotential self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ElectricPotential operator /(ElectricPotential self, Number scalar) => Extensions.Divide(self, scalar);
    public static ElectricPotential operator %(ElectricPotential self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(ElectricPotential a, ElectricPotential b) => Extensions.Equals(a, b);
    public static Integer Compare(ElectricPotential x, ElectricPotential y) => Extensions.Compare(x, y);
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
    public static ElectricCharge Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(ElectricCharge x) => Extensions.FieldValues(x);
    public static Array FieldTypes(ElectricCharge x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static ElectricCharge operator +(ElectricCharge self, Number scalar) => Extensions.Add(self, scalar);
    public static ElectricCharge operator -(ElectricCharge self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ElectricCharge operator *(ElectricCharge self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ElectricCharge operator /(ElectricCharge self, Number scalar) => Extensions.Divide(self, scalar);
    public static ElectricCharge operator %(ElectricCharge self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(ElectricCharge a, ElectricCharge b) => Extensions.Equals(a, b);
    public static Integer Compare(ElectricCharge x, ElectricCharge y) => Extensions.Compare(x, y);
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
    public static ElectricCurrent Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(ElectricCurrent x) => Extensions.FieldValues(x);
    public static Array FieldTypes(ElectricCurrent x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static ElectricCurrent operator +(ElectricCurrent self, Number scalar) => Extensions.Add(self, scalar);
    public static ElectricCurrent operator -(ElectricCurrent self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ElectricCurrent operator *(ElectricCurrent self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ElectricCurrent operator /(ElectricCurrent self, Number scalar) => Extensions.Divide(self, scalar);
    public static ElectricCurrent operator %(ElectricCurrent self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(ElectricCurrent a, ElectricCurrent b) => Extensions.Equals(a, b);
    public static Integer Compare(ElectricCurrent x, ElectricCurrent y) => Extensions.Compare(x, y);
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
    public static ElectricResistance Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(ElectricResistance x) => Extensions.FieldValues(x);
    public static Array FieldTypes(ElectricResistance x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static ElectricResistance operator +(ElectricResistance self, Number scalar) => Extensions.Add(self, scalar);
    public static ElectricResistance operator -(ElectricResistance self, Number scalar) => Extensions.Subtract(self, scalar);
    public static ElectricResistance operator *(ElectricResistance self, Number scalar) => Extensions.Multiply(self, scalar);
    public static ElectricResistance operator /(ElectricResistance self, Number scalar) => Extensions.Divide(self, scalar);
    public static ElectricResistance operator %(ElectricResistance self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(ElectricResistance a, ElectricResistance b) => Extensions.Equals(a, b);
    public static Integer Compare(ElectricResistance x, ElectricResistance y) => Extensions.Compare(x, y);
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
    public static Power Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Power x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Power x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Power operator +(Power self, Number scalar) => Extensions.Add(self, scalar);
    public static Power operator -(Power self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Power operator *(Power self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Power operator /(Power self, Number scalar) => Extensions.Divide(self, scalar);
    public static Power operator %(Power self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Power a, Power b) => Extensions.Equals(a, b);
    public static Integer Compare(Power x, Power y) => Extensions.Compare(x, y);
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
    public static Density Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Density x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Density x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Density operator +(Density self, Number scalar) => Extensions.Add(self, scalar);
    public static Density operator -(Density self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Density operator *(Density self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Density operator /(Density self, Number scalar) => Extensions.Divide(self, scalar);
    public static Density operator %(Density self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Density a, Density b) => Extensions.Equals(a, b);
    public static Integer Compare(Density x, Density y) => Extensions.Compare(x, y);
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
    public static NormalDistribution Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(NormalDistribution x) => Extensions.FieldValues(x);
    public static Array FieldTypes(NormalDistribution x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static PoissonDistribution Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(PoissonDistribution x) => Extensions.FieldValues(x);
    public static Array FieldTypes(PoissonDistribution x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static BernoulliDistribution Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(BernoulliDistribution x) => Extensions.FieldValues(x);
    public static Array FieldTypes(BernoulliDistribution x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
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
    public static Probability Zero() => Extensions.Zero();
    public static Probability One() => Extensions.One();
    public static Probability MinValue() => Extensions.MinValue();
    public static Probability MaxValue() => Extensions.MaxValue();
    public static Probability Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Probability x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Probability x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public static Probability operator +(Probability self, Probability other) => Extensions.Add(self, other);
    public static Probability operator -(Probability self, Probability other) => Extensions.Subtract(self, other);
    public static Probability operator -(Probability self) => Extensions.Negative(self);
    public static Probability operator *(Probability self, Probability other) => Extensions.Multiply(self, other);
    public static Probability operator /(Probability self, Probability other) => Extensions.Divide(self, other);
    public static Probability operator %(Probability self, Probability other) => Extensions.Modulo(self, other);
    public static Probability operator +(Probability self, Number scalar) => Extensions.Add(self, scalar);
    public static Probability operator -(Probability self, Number scalar) => Extensions.Subtract(self, scalar);
    public static Probability operator *(Probability self, Number scalar) => Extensions.Multiply(self, scalar);
    public static Probability operator /(Probability self, Number scalar) => Extensions.Divide(self, scalar);
    public static Probability operator %(Probability self, Number scalar) => Extensions.Modulo(self, scalar);
    public static Boolean operator ==(Probability a, Probability b) => Extensions.Equals(a, b);
    public static Integer Compare(Probability x, Probability y) => Extensions.Compare(x, y);
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
    public static BinomialDistribution Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(BinomialDistribution x) => Extensions.FieldValues(x);
    public static Array FieldTypes(BinomialDistribution x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public Count Trials { get; }
    public Probability P { get; }
}
public class Array1: Array<Array1>
{
    public Array1(Integer _Count, Function1 _Map) => (Count, Map) = (_Count, _Map);
    public static Array1 New(Integer _Count, Function1 _Map) => new(_Count, _Map);
    public static implicit operator (Integer, Function1)(Array1 self) => (self.Count, self.Map);
    public static implicit operator Array1((Integer, Function1) value) => new Array1(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Count", "Map" };
    public object[] FieldValues() => new[] { Count, Map };
    public static Integer Count(Array1 xs) => Extensions.Count(xs);
    public static T At(Array1 xs, Integer n) => Extensions.At(xs, n);
    public T this[Integer n]
    {
        get{
            return ;
        }
    }
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Array1 x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Array1 x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public Integer Count { get; }
    public Function1 Map { get; }
}
public class Tuple2: Value<Tuple2>
{
    public Tuple2(T0 _Item0, T1 _Item1) => (Item0, Item1) = (_Item0, _Item1);
    public static Tuple2 New(T0 _Item0, T1 _Item1) => new(_Item0, _Item1);
    public static implicit operator (T0, T1)(Tuple2 self) => (self.Item0, self.Item1);
    public static implicit operator Tuple2((T0, T1) value) => new Tuple2(value.Item1, value.Item2);
    public string[] FieldNames() => new[] { "Item0", "Item1" };
    public object[] FieldValues() => new[] { Item0, Item1 };
    public static Tuple2 Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Tuple2 x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Tuple2 x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public T0 Item0 { get; }
    public T1 Item1 { get; }
}
public class Tuple3: Value<Tuple3>
{
    public Tuple3(T0 _Item0, T1 _Item1, T2 _Item2) => (Item0, Item1, Item2) = (_Item0, _Item1, _Item2);
    public static Tuple3 New(T0 _Item0, T1 _Item1, T2 _Item2) => new(_Item0, _Item1, _Item2);
    public static implicit operator (T0, T1, T2)(Tuple3 self) => (self.Item0, self.Item1, self.Item2);
    public static implicit operator Tuple3((T0, T1, T2) value) => new Tuple3(value.Item1, value.Item2, value.Item3);
    public string[] FieldNames() => new[] { "Item0", "Item1", "Item2" };
    public object[] FieldValues() => new[] { Item0, Item1, Item2 };
    public static Tuple3 Default() => Extensions.Default();
    public static Array FieldNames() => Extensions.FieldNames();
    public static Array FieldValues(Tuple3 x) => Extensions.FieldValues(x);
    public static Array FieldTypes(Tuple3 x) => Extensions.FieldTypes(x);
    public static Type TypeOf() => Extensions.TypeOf();
    public T0 Item0 { get; }
    public T1 Item1 { get; }
    public T2 Item2 { get; }
}
public class Function0
{
    public Function0() => () = ();
    public static Function0 New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
}
public class Function1
{
    public Function1() => () = ();
    public static Function1 New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
}
public class Function2
{
    public Function2() => () = ();
    public static Function2 New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
}
public class Function3
{
    public Function3() => () = ();
    public static Function3 New() => new();
    public string[] FieldNames() => new[] {  };
    public object[] FieldValues() => new[] {  };
}
