using System.Runtime.InteropServices;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace UnitsOfMeasure
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }


    public class Unit { }
    public class Kilograms : Unit { }
    public class Seconds : Unit { }
    public class Meters : Unit { }
    public class Radians : Unit { }
    public class MultiplyUnits<TUnit1, TUnit2> : Unit where TUnit1 : Unit where TUnit2 : Unit { }
    public class DivideUnits<TUnit1, TUnit2> : Unit where TUnit1 : Unit where TUnit2 : Unit { }
    
    public class Measure<T> where T : Unit
    {
        public static readonly T Unit = default;
        public Measure(double q) => Quantity = q;
        public double Quantity { get; }

        public static Measure<MultiplyUnits<Kilograms, T>> operator *(Measure<Kilograms> other, Measure<T> self) => new Measure<MultiplyUnits<Kilograms, T>>(other.Quantity * self.Quantity);
        public static Measure<MultiplyUnits<T, Kilograms>> operator *(Measure<T> self, Measure<Kilograms> other) => new Measure<MultiplyUnits<T, Kilograms>>(self.Quantity * other.Quantity);
        public static Measure<DivideUnits<Kilograms, T>> operator /(Measure<Kilograms> other, Measure<T> self) => new Measure<DivideUnits<Kilograms, T>>(other.Quantity / self.Quantity);
        public static Measure<DivideUnits<T, Kilograms>> operator /(Measure<T> self, Measure<Kilograms> other) => new Measure<DivideUnits<T, Kilograms>>(self.Quantity / other.Quantity);

        public static Measure<MultiplyUnits<Seconds, T>> operator *(Measure<Seconds> other, Measure<T> self) => new Measure<MultiplyUnits<Seconds, T>>(other.Quantity * self.Quantity);
        public static Measure<MultiplyUnits<T, Seconds>> operator *(Measure<T> self, Measure<Seconds> other) => new Measure<MultiplyUnits<T, Seconds>>(self.Quantity * other.Quantity);
        public static Measure<DivideUnits<Seconds, T>> operator /(Measure<Seconds> other, Measure<T> self) => new Measure<DivideUnits<Seconds, T>>(other.Quantity / self.Quantity);
        public static Measure<DivideUnits<T, Seconds>> operator /(Measure<T> self, Measure<Seconds> other) => new Measure<DivideUnits<T, Seconds>>(self.Quantity / other.Quantity);

        public static Measure<MultiplyUnits<Meters, T>> operator *(Measure<Meters> other, Measure<T> self) => new Measure<MultiplyUnits<Meters, T>>(other.Quantity * self.Quantity);
        public static Measure<MultiplyUnits<T, Meters>> operator *(Measure<T> self, Measure<Meters> other) => new Measure<MultiplyUnits<T, Meters>>(self.Quantity * other.Quantity);
        public static Measure<DivideUnits<Meters, T>> operator /(Measure<Meters> other, Measure<T> self) => new Measure<DivideUnits<Meters, T>>(other.Quantity / self.Quantity);
        public static Measure<DivideUnits<T, Meters>> operator /(Measure<T> self, Measure<Meters> other) => new Measure<DivideUnits<T, Meters>>(self.Quantity / other.Quantity);

        public static Measure<MultiplyUnits<Radians, T>> operator *(Measure<Radians> other, Measure<T> self) => new Measure<MultiplyUnits<Radians, T>>(other.Quantity * self.Quantity);
        public static Measure<MultiplyUnits<T, Radians>> operator *(Measure<T> self, Measure<Radians> other) => new Measure<MultiplyUnits<T, Radians>>(self.Quantity * other.Quantity);
        public static Measure<DivideUnits<Radians, T>> operator /(Measure<Radians> other, Measure<T> self) => new Measure<DivideUnits<Radians, T>>(other.Quantity / self.Quantity);
        public static Measure<DivideUnits<T, Radians>> operator /(Measure<T> self, Measure<Radians> other) => new Measure<DivideUnits<T, Radians>>(self.Quantity / other.Quantity);

        public static implicit operator Measure<T>(double q) => new Measure<T>(q);
        public static implicit operator double(Measure<T> m) => m.Quantity;
    }
}