using System.Drawing;

namespace Plato.Math;

public interface IArithmetic<TSelf, TOther>
{
    TSelf Add(TOther value);
    TSelf Subtract(TOther value);
    TSelf Multiply(TOther value);
    TSelf Divide(TOther value);
    TSelf Negate();
}

public interface IComparable<T>
{
    int CompareTo(T value);
}

public interface IEquatable<T>
{
    bool Equals(T value);
}

public interface IArray<out T>
{
    int Count { get; }
    T this[int index] { get; }
}

public interface IVector<out TScalar> : IArray<TScalar>
{
    TScalar Magnitude { get; }
}

/// <summary>
/// Tells the Plato code generator to create a default implementation for an interface.
/// Kind of like how Records work.
/// </summary>
public class ImplementAttribute : Attribute
{
}

// User written interface describes the type to be implemented.  
// If a property does not support init, no "With" functions 
public interface IPoint : IComparable<IPoint>, IEquatable<IPoint>, IVector<double>, IArithmetic<Point, Point>, IArithmetic<Point, double>, IArithmetic<double, Point>
{
    double X { get; init; }
    double Y { get; init; }
}

