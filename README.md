# Plato Overview

**Plato** is a high-performance cross-platform programming language designed for 2D and 3D geometry algorithms and data structures.  

## Plato Examples

The [Plato.Geometry](https://github.com/ara3d/Plato.Geometry) contains a large library of 
Plato functions and types in the [`/plato-src`](https://github.com/ara3d/Plato.Geometry/tree/main/plato-src) folder.

## About the Language

Plato is a statically typed functional language that looks and behaves like an 
object-oriented scripting language, but with a lot less complexity.  

The most notable features of Plato are:

- all types are immutable
- there are no implicit `this` parameters   
- no distinction between methods and functions -- all functions are like C# extension methods  
- no visibility modifiers (e.g., public, private, protected, internal), 
- no virtual or abstract methods. 
- support for generic types and generic methods
- default implementations common functions (e.g., like C# records) 
    - constructors
    - conversion to string
    - implicit casts to/from tuples
    - immutable setters (i.e., `WithX` functions) 
- operator overloading -- implied by special named functions
- functions with no parameters are treated like properties

## Motivation 

Plato is being used to **develop cross-platform libraries** that form the basis for software we are developing at 
[Ara 3D](https://ara3d.com). We do a lot of computational geometry work in different languages
and recreating and maintaining high-performance code for multiple platforms and languages was too costly and burdensome for a
small company. 

No existing language provides the appropriate mix of simplicity, abstraction, and performance. 

## About the Implementation and Tooling

Plato is being designed to allow multiple implementations. We are actively developing and using a Plato to C# compiler.

The Plato to C# compiler is all open-source, and was built in tandem with the [Parakeet parsing library](https://github.com/ara3d/parakeet). 

## Why Plato Instead of C# 

Using Plato instead of C# has the following advantages:

1. Code can be reused in other contexts (e.g., JavaScript, WASM, Rust, GLSL)
2. Significantly reduces the amount of boilerplate code a user has to write
3. Code is easier to read and maintain 
4. _WIP:_ Code will be faster  

## Plato Performance 

Currently the Plato compiler produces code which is on par with well-written code in C# in terms of performance. 

# Plato in Depth

The top-level abstractions in Plato are: 

* Libraries - collections of functions  
* Types - data structures
* Interfaces - abstract data types 

## Libraries

A Plato library contains only functions. It is similar to a static class with extension methods in C#. 
Library functions can be called using a dot syntax using the first parameter on the left of a dot. 
Furthermore, any function with no arguments, can be invoked as if it was a parameter. 
Function parameters can be a mix of either types or interfaces. 

For example:

```
library MyLibrary
{ 
    Square(x: Number): Number
        => x * x;

    SumSquares(v: Vector): Number
    {
        var r = v[0].Square;
        for (var i=1; i < v.Count; ++i)
            r += v[i].Square;
        return r;
    }

    Magnitude(v: Vector): Number
        => v.SumSquares.Sqrt;
} 
```

### Types

A type is a concrete data structure. 
It is similar to a record, struct, or class in other languages. 
It contain fields, and decares the interfaces that it implements. 
Unlike traditional object-oriented languages, it does not have member functions (aka methods). 

```plato
type Vector2D
    implements Vector
{
    X: Number;
    Y: Number;
}
```

### Interface

An interface is an abstract data type. 
It declares a list of functions that can be called on any type implementing the interface.
In the context of the interface, the implementing type is called `Self`.

Interface can inherit from other interfaces. 

```plato
interface IArray<T>
{
    Count(xs: Self): Integer;
    At(xs: Self, n: Integer): T;
}
interface IVector
    inherits IArray<Number>, INumerical, IEquatable, ICoordinate
{
}
```

## Generic Functions  

Plato transforms any function that uses interfaces into a generic function. Every interface reference becomes a constrained type parameter. 

So for example:

 ```typescript
	    //  https://en.wikipedia.org/wiki/Quadratic_function
	    Quadratic(x: Number, a: INumerical, b: INumerical, c: INumerical): INumerical
	        => a * x.Sqr + b * x + c;
```

Becomes in C# :

```csharp
	    //  https://en.wikipedia.org/wiki/Quadratic_function
	    public static T Quadratic<T>(this double x, T a, T b, T c) where T : INumerical<T>
	        => a * x.Sqr() + b * x + c;
```

This is a simplification made for users who don't understand type parameter, that has a trade-off in that things are less precise. 

The original function signature doesn't strictly say that the return type and the parameters a, b, and c, are all the same. This is just something the language assumes. 

If you have two parameters that can be different but use the same interface, usually they won't be type constrained. In this case you can explicitly use a system called Type Variables. 

For example:

```typescript
	Zip(xs: IArray<$T0>, ys; IArray<$T1>, f: Function2<$T0, $T1, $TR>): IArray<$TR> 
		=> xs.Count.MapRange(i => f(xs[i], ys[i])); 
```

In this case the Type parameters are implied, by the use of TypeVariables (type names preceded by a $). 

```csharp
	public static IArray<TR> Zip<T0, T1, TR>(this IArray<T0> xs, IArray<T1> ys, System.Func<T0, T1, TR> f) => xs.Count.MapRange(i => f(xs[i], ys[i])); 
```

## Visual Syntax 

Plato is designed so that it can be translated to/from a visual data-flow graph. This visual syntax is called `PlatoFlow`, and is under development.  

## Building Plato 

Plato tools and libraries are not intended for general usage yet. 
This repository currently compiles only when used as a submodule from within 
the [Ara3D main repository](https://github.com/ara3d/ara3d).   

## Status

While the language design is still undergoing a few minor tweaks, it is starting to reach a stable equilibrium. 

## Ideas for Contributions
 
Some ideas for highly motivated and patient people who are interested in contributing to the Plato project:

* Optimizers - in particular Plato to Plato rewriting tools
* Type checking and inference tools 
* Transpilers to other languages (e.g., JavaScript, GO, Rust, Dart, Java, C++, GLSL, etc.) 
* Support for Plato syntax in popular coding tools (e.g., VS Code, Visual Studio, Avalon Edit, Scintilla, etc.)

## Feedback

I am always interested in feedback in any form. You can find [me on LinkedIn](https://www.linkedin.com/in/cdiggins/) 
or mail me at [cdiggins@gmail.com](mailto:cdiggins@gmail.com).


