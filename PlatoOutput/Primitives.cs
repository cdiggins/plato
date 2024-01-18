
using System;

public readonly partial struct String
{
    public Integer Compare(String other) => Value.CompareTo(other.Value);
    public Character At(Integer n) => Value[n];
    public Integer Count => Value.Length;
}

public readonly partial struct Number
{
    public Number Zero => 0;
    public Number One => 1;
    public Number MinValue => double.MinValue;
    public Number MaxValue => double.MaxValue;
    public Number Magnitude => Value;
    public Number Reciprocal => 1.0 / Value;
    public Integer Compare(Number other) => Value.CompareTo(other.Value);
    public Number Unlerp(Number a, Number b) => (double)(this - a) / (double)(b = a);
}

public readonly partial struct Integer
{
    public Integer Zero => 0;
    public Integer One => 1;
    public Integer MinValue => int.MinValue;
    public Integer MaxValue => int.MaxValue;
    public Number Magnitude => Value;
    public Integer Reciprocal => 1 / Value;
    public static implicit operator Number(Integer self) => self.Value;
    public Integer Compare(Integer other) => Value.CompareTo(other.Value);
    public Number Unlerp(Integer a, Integer b) => (double)(this - a) / (double)(b = a);
}

public readonly partial struct Character
{
    public Character Zero => (char)0;
    public Character One => (char)1;
    public Character MinValue => char.MinValue;
    public Character MaxValue => char.MaxValue;
    public Number Magnitude => Value;
    public static implicit operator Number(Character self) => self.Value;
    public Integer Compare(Character other) => Value.CompareTo(other.Value);
    public Number Unlerp(Character a, Character b) => (double)(this - a) / (double)(b = a);
    public Boolean Equals(Character x) => Value.Equals(x.Value);
    public Boolean NotEquals(Character x) => !Equals(x);
}

public readonly partial struct Array<T>
{
    private readonly T[] _data;
    public Integer Count => _data.Length;
    public T At(Integer n) => _data[n];
    public static implicit operator T[](Array<T> self) => self._data;
    public static implicit operator Array<T>(T[] self) => new Array<T>(self);
    public Array(T[] data) => _data = data;
}

public readonly partial struct Dynamic
{
    public readonly object Value;
    public Dynamic WithValue(object value) => new Dynamic(value);
    public Dynamic(object value) => (Value) = (value);
    public static Dynamic Default = new Dynamic();
    public static Dynamic New(object value) => new Dynamic(value);
    public String TypeName => "Dynamic";
    public Array FieldNames => (Array<String>)new[] { (String)"Value" };
    public Array FieldValues => (Array<Dynamic>)new[] { (Dynamic)Value };
    public T As<T>() => (T)Value;
}

public static class PrimitiveExtensions
{
    public static (T0, T1) Tuple<T0, T1>(this T0 self, T1 other)
        => (self, other);
}