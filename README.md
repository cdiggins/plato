
# Plato Overview

**Plato** is a cross-platform programming language by [Christopher Diggins](https://github.com/cdiggins) 
inspired by JavaScript, TypeScript, and C#. 
Plato is designed to be easy to teach and learn while being efficient and robust enough for 
professional coding, particularly in the realm of 3D graphics. 

## About

Plato is a statically typed functional language that looks and behaves in many ways like an 
object-oriented scripting language, but with a lot less complexity.  

The most notable features of Plato are that all types are immutable, and there are no implicit `this` parameters. 
The means that there is no need to have a distinction between methods and functions, 
no need for visibility modifiers (e.g., public, private, protected, internal), and no need for 
virtual or abstract methods. 

## More Details

The top-level abstractions in Plato are: 

* Types - data structures
* Concepts - abstract data types 
* Libraries - modules 

### Library 

A library contains functions. It is similar to a static class with extension methods in C#. 
Library functions can be called using a dot syntax using the first parameter on the left of a dot. 
Furthermore, any function with no arguments, can be invoked as if it was a parameter. 

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
It contain fields, and decares the concepts that it implements. 
Unlike traditional object-oriented languages, it does not have member functions (methods). 
All functions (operations) which can be called on a type are declared in libraries. 

```plato
type Vector2D
    implements Vector
{
    X: Number;
    Y: Number;
}
```

### Concepts

A concept is an abstract data type. 
It declares a list of operations that can be performed with certain types. 
It is similar to an interface, mixin, or trait in other languages. 
Concepts can inherit from other concepts. 
There is no implicit `this` parameter in Plato, but there is an implicit `Self`
type parameter. 

```plato
concept Array<T>
{
    Count(xs: Self): Integer;
    At(xs: Self, n: Integer): T;
}
concept Vector
    inherits Array<Number>, Numerical, Magnitudinal, Equatable, Coordinate
{
}
```

## Plato Examples

The Plato standard library is the best place to start for Plato examples:

* [libraries.plato](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/libraries.plato)
* [types.plato](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/types.plato)
* [concepts.plato](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/concepts.plato)

## Motivation 

Plato is being used to develop cross-platform libraries that form the basis for software we are developing at 
[Ara 3D](https://ara3d.com). We do a lot of computational geometry work in different languages
and recreating and maintaining the same code for multiple platforms and languages is too costly and burdensome for a
small company. 

## Building Plato 

Plato tools and libraries are not intended for general usage yet as it is undergoing a lot of churn. 
This repository is currently intended to be compiled only as a submodule from within 
the [Ara3D main repository](https://github.com/ara3d/ara3d).   

## Learning about Plato 

Right now the best way to learn about Plato is via the 
[Plato.Mathematics.NET](https://github.com/cdiggins/Plato.Mathematics.NET) project. This is where it is being 
put into a real-world use case. 

You can also check out the various [blog articles in this folder](https://github.com/cdiggins/plato/tree/main/blog).

## Status

While the language design is still undergoing a few minor tweaks, it is starting to reach a stable equilibrium. 

The first library currently being built with Plato is [Plato.Mathematics.NET](https://github.com/cdiggins/Plato.Mathematics.NET), a C# math library that can be used as a feature-rich replacement for System.Numerics. 

Once the Plato.Mathematics.NET library is stable and ready for publishing on Nuget I will write a language specification document for Plato.  

## Ideas for Contributions
 
Some ideas for highly motivated and patient people who are interested in contributing to the Plato project:

* Optimizers - in particular Plato to Plato rewriting tools
* Type checking and inference tools 
* Transpilers to other languages (e.g., JavaScript, GO, Rust, Dart, Java, C++, GLSL, etc.) 
* Support for Plato syntax in popular coding tools (e.g., VS Code, Visual Studio, Avalon Edit, Scintilla, etc.)

## Feedback

I am always interested in feedback in any form. You can find me on twitter as 
[@cdiggins](https://twitter.com/cdiggins) or mail me at [cdiggins@gmail.com](mailto:cdiggins@gmail.com).


