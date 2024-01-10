using System;
using System.Linq;

namespace PlatoStandardLibrary
{
    public interface Array<Element> 
     {
        Element At(int index);
        int Count { get; }
    }

    public class ArrayImpl<T> : Array<T>
    {
        public Func<int, T> Func { get; }
        public int Count { get; }

        public ArrayImpl(Func<int, T> f, int count)
        {
            Func = f;
            Count = count;
        }

        public T At(int index) => Func(index);
    }

    public static partial class Extensions
    {
        public static Array<T> Take<T>(this Array<T> self, int n) 
        {
            return new ArrayImpl<T>(i => self.At(i), n);
        }
    }

    public interface Value<Self>
    {
        object[] FieldValues { get; }

        // Static members
        string TypeName { get; }
        Self One { get; }
        Self Zero { get; }
        Self MinValue { get; }
        Self MaxValue { get; }
        Self Default { get; }
        string[] FieldNames { get; }
    }

    public interface Numerical<Self>
    {
        Self Add(Self other);
        Self Negate();
    }

    public static partial class Extensions
    {
        public static Self Subtract<Self>(this Self a, Self b) where Self : Numerical<Self>
            => a.Add(b.Negate());

        public static void Test()
        {
            Number a = 3;
            Number b = 4;
            a.Subtract(b);
        }
    }

    public class Number : Numerical<Number>
    {
        public double Value { get; }

        public Number Add(Number other)
            => Value + other.Value;

        public Number Negate()
            => -Value;

        public static StaticValue<Number> StaticData = new StaticValue<Number>()
        {
            TypeName = "Number",
            One = 1,
            Zero = 0,
            MinValue = double.MinValue,
            MaxValue = double.MaxValue,
            Default = 0,
        };

        // Movement of static data to also working on instance 
        public string TypeName => StaticData.TypeName;
        public Number One => StaticData.One;
        public Number Zero => StaticData.Zero;
        public Number MinValue => StaticData.MinValue;
        public Number MaxValue => StaticData.MaxValue;
        public Number Default => StaticData.Default;
        public string[] FieldNames => StaticData.FieldNames;

        public Number(double value) => Value = value;
        public static implicit operator Number(double v) => new Number(v);
        public static implicit operator double(Number v) => v.Value;
        public object[] FieldValues => new[] { (object)Value };
    }

    public class StaticValue<T>
    {
        public string TypeName { get; set; }
        public T One { get; set; }
        public T Zero { get; set; }
        public T MinValue { get; set; }
        public T MaxValue { get; set; }
        public T Default { get; set; }
        public string[] FieldNames { get; set; }
    }

    public static partial class Extensions
    {
        public static string ToString<T>(this T value) where T : Value<T>
        {
            return value.TypeName + "(" + string.Join(", ", value.FieldNames.Zip(value.FieldValues, (a, b) => $"{a} = {b}") + ")");
        }

        public static T One<T>(this T value) where T : Value<T> => value.One;
        public static T Zero<T>(this T value) where T : Value<T> => value.Zero;
        public static T MinValue<T>(this T value) where T : Value<T> => value.MinValue;
        public static T MaxValue<T>(this T value) where T : Value<T> => value.MaxValue;
        public static T Default<T>(this T value) where T : Value<T> => value.Default;
        public static string TypeName<T>(this T value) where T : Value<T> => value.TypeName;
        public static string[] FieldNames<T>(this T value) where T : Value<T> => value.FieldNames;
    }
}
