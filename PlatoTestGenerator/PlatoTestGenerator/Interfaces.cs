using System;
using System.Runtime.InteropServices;

namespace Plato
{
    /// <summary>
    /// When a class or struct declares it implements the IValue interface Plato
    /// will provide a number of functions automatically. 
    /// - equality operators ==, !=
    /// - GetHashCode and Equals overrides 
    /// - Deconstruct function
    /// - ToString override, that produces a JSON formatted object 
    /// - implicit conversion to/from value tuples
    /// - static fields for "One", "Zero", "Default", "MinValue", "MaxValue"
    /// - WithX functions for each field, that create a copy with a new value for the corresponding field
    /// </summary>
    public interface IValue
    { }

    /// <summary>
    /// When a class declares that it implements the INumber interface, it is considered numeric.
    /// All properties must also be considered numeric or are primitive numbers. 
    /// Plato will automatically generate implementations of the following functions, fields, operators:
    /// - arithmetical operators: +, - (binary and unary), *, /
    /// - extension methods corresponding to: Addition, Subtraction, 
    /// </summary>
    public interface INumber : IValue 
    { }

    public interface IArray<T>
    {
        int Count { get; }
        T this[int index] { get; }
    }

    /// <summary>
    /// When a class declares that it implements the IVector interface, all fields must have the same type.
    /// and that type must be considered a numeric.
    /// Plato will automatically generate implementations for:
    /// - Count
    /// - T this[int index]
    /// - * and / operators given the scalar
    /// </summary>. 
    public interface IVector<T> : IArray<T>, INumber
    {
    }

    public interface IInterval<T> 
    {
        T Lower { get; }
        T Upper { get; }
    }

    public interface IMeasure : IValue
    {
    }

    public interface ISignedDistanceField
    {
        double Distance(Double3 pos);
    }

    public interface ISignedDistanceField2D
    {
        double Distance(Double2 pos);
    }

    public interface IShape : ISignedDistanceField
    {
        double Volume { get; }
    }

    public interface IShape2D : ISignedDistanceField2D
    {
        double Area { get; }
    }

    public interface IDifference<T>
    {
        T Subtract(T x, T y);
    }
}
