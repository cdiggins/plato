
using System;

public readonly partial struct Number
{
    public Number Zero => 0;
    public Number One => 1;
    public Number MinValue => double.MinValue;
    public Number MaxValue => double.MaxValue;
    public Number Magnitude => Value;
    public Number Reciprocal => 1.0 / Value;
}

public readonly partial struct Integer
{
    public Integer Zero => 0;
    public Integer One => 1;
    public Integer MinValue => int.MinValue;
    public Integer MaxValue => int.MaxValue;
    public Number Magnitude => Value;
    public Integer Reciprocal => 1 / Value;
}

public readonly partial struct Array1<T> : Array<T>
{
    private readonly T[] _data;
    public Integer Count => _data.Length;
    public T At(Integer n) => _data[n];
    public static implicit operator T[](Array1<T> self) => self._data;
    public static implicit operator Array1<T>(T[] self) => new Array1<T>(self);
    public Array1(T[] data) => _data = data;
}

public readonly partial struct Dynamic
{
    public readonly object Value;
    public Dynamic WithValue(object value) => new Dynamic(value);
    public Dynamic(object value) => (Value) = (value);
    public static Dynamic Default = new Dynamic();
    public static Dynamic New(object value) => new Dynamic(value);
    public String TypeName => "Dynamic";
    public Array<String> FieldNames => (Array1<String>)new[] { (String)"Value" };
    public Array<Dynamic> FieldValues => (Array1<Dynamic>)new[] { (Dynamic)Value };
}