// This is a proposal for how automated code generation of immutable types 
// will work in Plato. The motivating example are simple mathematical structures
// like Points. 
//
// Plato is a pure functional subset of C#. It is designed to be enforced
// and leveraged through Roslyn compiler tools including:
// a code generator, static code analyzer, and optimizer.  
//
// Some of the ideas in this proposal are heavily influenced by the new records
// feature of C# 9.0 and later, and my own experience using immutable 
// structures in C# in large scale performance intensive applications.
//
// The goal of this proposal and Plato in general is to minimize the writing of
// boiler plate code, and provide safety guarantees for the programmer,
// that can be validated by static analyzers, and leveraged by an optimizer. 
//
// Compared to records the two biggest difference are:
// 1. strictly enforced immutability,
// 2. the ability to use a more compact imperative (mutable) syntax in special cases.
// 
// There are a number of extra functions provided that are intended to simplify usage 
// of these classes and reduce boiler plate. 
//
// A follow-up to this proposal will include how mathematical operators are generated 
// automatically for numerical types, and how those types signal to the generator 
// which types to automatically generate. It will likely have to do with some pre-knowledge
// built-in regarding certain interfaces. 
//
// So why not just use records? 
// * Records do not guarantee immutability.
// * The `with` expression syntax is hard to read and write when records are nested 
// * Records make it hard to guarantee invariants which are validated and enforced by a constructor
// * Records could benefit from more functions (e.g., string parsing, 
// * Interface implementations in a Record have to be explicit for each type of record. 
// * Records are not interfaces, so you still have to define the interface separately if you want one.
// * Generated code is not supported in .NET Standard 2.0 and Unity
//
// I'd greatly appreciate any comments/suggestions/criticisms at cdiggins@gmail.com. 

namespace Plato.Math2D;

public interface INumericOps
{
    INumericOps op_Add(INumericOps v);
    INumericOps op_Subtract(INumericOps v);
    INumericOps op_Multiply(INumericOps v);
    INumericOps op_Divide(INumericOps v);
    INumericOps op_Negate();
}

public interface IComparableOps
{
    IComparableOps op_Equals(IComparableOps v);
    IComparableOps op_NotEquals(IComparableOps v);
    IComparableOps op_LessThan(IComparableOps v);
    IComparableOps op_LessThanOrEquals(IComparableOps v);
    IComparableOps op_GreaterThan(IComparableOps v);
    IComparableOps op_GreaterThanOrEquals(IComparableOps v);
}

public interface IBooleanOps
{
    IBooleanOps op_Not();
    IBooleanOps op_And(IBooleanOps x);
    IBooleanOps op_Or(IBooleanOps x);
    IBooleanOps op_Xor(IBooleanOps x);
}

// User written interface describes the type to be implemented.  
// If a property does not support init, no "With" functions 
[Implement]
public interface IPoint : IComparable<IPoint>, IEquatable<IPoint>, IVector<double>
{
    double X { get; init; }
    double Y { get; init; }
}

// Parts of the Plato standard library 

public interface IArray<out T>
{
    int Count { get; }
    T this[int index] { get; }
}

public interface IVector<out TScalar> : IArray<TScalar>
{
    TScalar Magnitude { get; }
}

// In addition to being present as extension methods. 
// These methods are added automatically to the generated class.
// If a method has no arguments, then it is added as a property.
// This allows you to provide additional methods and properties
// necessary to implement additional interfaces. 

public static class IPointExtensions
{
    // Implementation of IVector 
    public static double MagnitudeSquared(this IPoint p) => p.X * p.X + p.Y * p.Y;
    public static double Magnitude(this IPoint p) => System.Math.Sqrt(p.MagnitudeSquared());

    // Implementation of ICompareTo 
    public static int CompareTo(this IPoint p, IPoint? other) => p.MagnitudeSquared().CompareTo(other?.MagnitudeSquared() ?? double.MinValue);

    // Implementation of IArray
    public static int Count(this IPoint p) => 2;
    public static double _this(this IPoint p, int index) => index switch { 0 => p.X, 1 => p.Y, _ => throw new IndexOutOfRangeException() };
}

//==========================================================
// Begin Plato auto-generated code 
// 
// This code is derived and inferred from the IPoint interface,
// as well the extension methods found in IPointExtensions.

// Ideas: 
// * JSON serialization (maybe for the type?)
// * Binary serialization
// * Data arrangement
// * Transformable 
// * Strict layout
// * Aggressive inlining 

public class Point : IPoint
{
    // Fundamental properties  
    public double X { get; init; }
    public double Y { get; init; }

    // Constructors 
    public Point(double x, double y) => (X, Y) = (x, y);
    public Point(IPoint p) : this(p.X, p.Y) { }
    public Point(IArray<double> xs) : this(xs[0], xs[1]) { }

    // Helper static constructor functions
    public static Point Create(double x, double y) => new(x, y);
    public static Point Create(IPoint p) => new(p);

    // Enables duck-typing/structural typing conversions
    // For example when converting between one math library and another
    public static Point CreateDynamic(dynamic d) => new(d.X, d.Y);

    // Replacement for with expressions
    public Point With(double? x, double? y) => new(x ?? X, y ?? Y);
    public Point WithX(double x) => new(x, Y);
    public Point WithY(double y) => new(X, y);

    // Allows users to define a new point, by imperatively changing a mutable point
    // Provides a much more convenient syntax for complex types. 
    public Point Apply(Action<MutablePoint> mutatingAction) => throw new NotImplementedException("TBD");

    // Syntactic sugar. It is sometimes more ergonomic to write code this way than having to put
    // the function application around the outside of an expression 
    public Point Apply(Func<Point, Point> f) => f(this);

    // Value tuple support
    public static implicit operator (double, double)(Point p) => (p.X, p.Y);
    public static implicit operator Point((double, double) tuple) => new(tuple.Item1, tuple.Item2);
    public void Deconstruct(out double x, out double y) => (x, y) = (X, Y);

    // Conversion to/from arrays provided for all types implementing IArray
    public static implicit operator double[](Point p) => new[] { p.X, p.Y };
    public static explicit operator Point(double[] p) => new(p[0], p[1]);

    // String conversion support for all types 
    public static implicit operator string(Point p) => p.ToString();
    public override string ToString() => $"Point {{ X = {X}, Y = {Y} }}";
    public static Point Parse(string s) => throw new NotImplementedException("TBD");

    // Equality and hash-code functions
    public override bool Equals(object? obj) => obj is IPoint p && Equals(p);
    public bool Equals(IPoint? p) => p != null && p.X.Equals(X) && p.Y.Equals(Y);
    public static bool operator ==(Point p0, Point p1) => p0.Equals(p1);
    public static bool operator !=(Point p0, Point p1) => !p0.Equals(p1);
    public override int GetHashCode() => throw new NotImplementedException("TBD");

    // Additional properties and methods added from extension methods 
    public double Magnitude => this.Magnitude();
    public double MagnitudeSquared => this.MagnitudeSquared();
    public int CompareTo(IPoint? p) => IPointExtensions.CompareTo(this, p);
    public int Count => this.Count();
    public double this[int index] => this._this(index);

    // TODO: when and how these mathematical constants are generated is to be determined
    public static Point Zero = new(0, 0);
    public static Point Unit = new(1, 1);
    public static Point Default = Zero;
    public static Point Identity = Unit;
    public static Point MinValue = new(double.MinValue, double.MinValue);
    public static Point MaxValue = new(double.MaxValue, double.MaxValue);

    // TODO: when and how these mathematical operators are generated is to be determined
    // Perhaps it is signaled through the use of a particular interface (IVector)
    public static Point operator +(Point p1, Point p2) => new(p1.X + p2.Y, p1.Y + p2.Y);
    public static Point operator -(Point p1, Point p2) => new(p1.X - p2.Y, p1.Y - p2.Y);
    public static Point operator *(Point p1, Point p2) => new(p1.X * p2.Y, p1.Y * p2.Y);
    public static Point operator /(Point p1, Point p2) => new(p1.X / p2.Y, p1.Y / p2.Y);
    public static Point operator %(Point p1, Point p2) => new(p1.X % p2.Y, p1.Y % p2.Y);
    public static Point operator -(Point p1) => new(-p1.X, -p1.Y);
    public static Point operator +(Point p1, double x) => new(p1.X + x, p1.Y + x);
    public static Point operator -(Point p1, double x) => new(p1.X - x, p1.Y - x);
    public static Point operator *(Point p1, double x) => new(p1.X * x, p1.Y * x);
    public static Point operator /(Point p1, double x) => new(p1.X / x, p1.Y / x);
    public static Point operator %(Point p1, double x) => new(p1.X % x, p1.Y % x);
    public static Point operator +(double x, Point p1) => new(x + p1.X, x + p1.Y);
    public static Point operator -(double x, Point p1) => new(x - p1.X, x - p1.Y);
    public static Point operator *(double x, Point p1) => new(x * p1.X, x * p1.Y);
    public static Point operator /(double x, Point p1) => new(x / p1.X, x / p1.Y);
    public static Point operator %(double x, Point p1) => new(x % p1.X, x % p1.Y);
}

/// <summary>
/// A mutable version of the type is generated for the purpose of easy updates
/// of complex structures using an imperative syntax.
/// The Unique attribute is enforced by a code analyzer to assure that it can't leak out. 
/// </summary>
[Unique]
public class MutablePoint : Point
{
    public MutablePoint(double x, double y) : base(x, y) { }
    public new double X { set => throw new NotImplementedException("TBD"); }
    public new double Y { set => throw new NotImplementedException("TBD"); }
}

// end auto-generated code
//==========================================================

