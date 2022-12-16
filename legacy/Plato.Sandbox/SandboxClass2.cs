using System;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

[Concept]
public class Scalable<TScalar>
{
    public Scalable<TScalar> Multiply(TScalar scalar);
    public Scalable<TScalar> Divide(TScalar scalar);
    public Scalable<TScalar> Inversion();
}

[Implements(Concept)]
class Vector3
{
    float X, Y, X;
    Vector3 Add(Vector3 v) => new Vector3(X + v.X, Y + v.Y, Z + v.Z);
    Vector3 Multiply(Vector3 v) => new Vector3(X * v.X, Y * v.Y, Z * v.Z);
}

[Concept]
class Number
{
    Number One();
    Number Two();
    Number Add(Number num);
    Number Multiply(Number num);
}

[Extends(Concept)]
class Extensions
{
    Number operator +(Number a, Number b) => a.Add(b);
    Number operator *(Number a, Number b) => a.Multiply(b);
}

[Implements(typeof(Concept))]
class Vector3
{
    float X, Y, X;
    Vector3 Add(Vector3 v) => new Vector3(X + v.X, Y + v.Y, Z + v.Z);
    Vector3 Multiply(Vector3 v) => new Vector3(X * v.X, Y * v.Y, Z * v.Z);
}

[Concept]
public class Number 
{
    public static Number One;
    public static Number Zero;
    public static Number operator +(Number a, Number b) => a.Add(b);
    public static Number operator -(Number a, Number b) => a.Subtract(b);
    public static Number operator -(Number a) => a.Negate();
    public static Number operator *(Number a, Number b) => a.Multiply(b);
    public static Number operator /(Number a, Number b) => a.Divide(b);
    public static Number operator %(Number a, Number b) => a.Modulo(b);
}


[Concept]
public class Vector<T> : Number, IArray<T>
{
}

public class IArray<T>
{
}


[Concept]
class Vector
{

}

[Concept]
class Number
{
    public static Number operator +(Number a, Number b) => a.Add(b);
    public static Number operator -(Number a, Number b) => a.Subtract(b);
    public static Number operator *(Number a, Number b) => a.Multiply(b);
    public static Number operator /(Number a, Number b) => a.Divide(b);
    public static Number operator %(Number a, Number b) => a.Modulo(b);
}

[Concept]
public class Vector : Number, IArray<Number>
{
    public Vector Zip(Vector v, Func<Number, Number, Number> f)
    {
        var r = default(Number);
        for (var i = 0; i < Count; i++)
            r = f(v[i], this[i]);
        return r;
    }

    public Vector Multiply(Vector v) => Zip(v, (a,b) => a * b);
    public Vector Modulo(Vector v) => Zip(v, (a, b) => a % b);
    public Vector Divide(Vector v) => Zip(v, (a, b) => a / b);
    public Vector Add(Vector v) => Zip(v, (a, b) => a + b);
    public Vector Subtract(Vector v) => Zip(v, (a, b) => a - b);

    public static Vector operator *(Vector v, Number b) => v1.Multiply(v2);
    public static Vector operator /(Vector v, Number b) => v1.Divide(v2);
    public static Vector operator %(Vector v, Number n) => v1.Modulo(v2);

    spublic int Count 
        => throw new NotImplementedException();
    
    public Number this[int index] 
        => throw new NotImplementedException();

    public Number Sum()
    {
        var r = default(Number);
        for (var i = 0; i < Count; i++)
            r += this[i];
        return r;
    }
    
    public Number MagnitudeSquared()
        => Sum() * Sum();

    public Number Magnitude()
        => Math.Sqrt(MagnitudeSquared());

    public Number Distance(Vector v)
        => (v - this).Magnitude();

    public Vector Dot(Vector v)
        => (v * this).Sum();

    public Vector 
}

public class Methods
{
    Vector Dot(Vector v, Vector v) => v * v;
}


class Algorithms
{

}

[Implements(typeof(Number))]
public class Unit
{
    public double Value { get; }
    public Unit(double x) => Value = x;
    public Unit Add(Unit x) => new Unit(Value + x.Value);
    public Unit Subtract(Unit x) => new Unit(Value - x.Value);
    public Unit Negate() => new Unit(-Value);
}

// The problem is that this is not executable. 
// I want to have operators on the INumber implementations
// Or at least I want to have literal syntax. That is not a problem. 
// To get a numeric value I would have to use a "Number" type placeholder. 

// I think the idea of using "Number" in a type is fine. 
// It is a special placeholder. When using actual types it won't work.
// It is a way of saying: I want the self type to be used throughout.
// The nice thing is that it can be used with: 
// float/double/float2/float3/float4/Double2/Double3/Double4/Complex 