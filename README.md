
# Plato Overview

**Plato** is a high-performance cross-platform programming language inspired by JavaScript and C#. 

[Join Discord](https://discord.gg/cjVfPBtJ).

## About the Language

Plato is a statically typed functional language that looks and behaves like an 
object-oriented scripting language, but with a lot less complexity.  

The most notable features of Plato are that all types are immutable, and there are no implicit `this` parameters. 
There is no distinction between methods and functions, no  visibility modifiers (e.g., public, private, protected, internal), 
and no virtual or abstract methods. 

## Motivation 

Plato is being used to develop cross-platform libraries that form the basis for software we are developing at 
[Ara 3D](https://ara3d.com). We do a lot of computational geometry work in different languages
and recreating and maintaining high-performance code for multiple platforms and languages was too costly and burdensome for a
small company. 

No existing language provides the appropriate mix of simplicity, abstraction, and performance. 

## About the Implementation and Tooling

Plato is being designed to allow multiple implementations. We are actively developing and using a Plato to C# compiler.

The Plato to C# compiler is all open-source, and was built in tandem with the [Parakeet parsing library](https://github.com/ara3d/parakeet). 

## Why Plato Instead of C# 

Using Plato instead of C# has the following advantages:

1. Code can be reused in other contexts (e.g., JavaScript)
2. Significantly reduces the amount of boilerplate code a user has to write
3. Code is easier to read and maintain 
4. _WIP:_ Code will be faster  

## WIP: Plato Performance 

Currently the Plato compiler produces code which is comparable to well-written code in C# in terms of performance.

In the future we will be leveraging the fact that Plato code:

1. Is immutable
2. Does not support reflection

This enables a set of interesting and powerful optimizations that can be performed on the AST before code generation occurs.

# Plato in Depth

The top-level abstractions in Plato are: 

* Libraries - collections of functions  
* Types - data structures
* Concepts - abstract data types 

## Libraries

A Plato library contains only functions. It is similar to a static class with extension methods in C#. 
Library functions can be called using a dot syntax using the first parameter on the left of a dot. 
Furthermore, any function with no arguments, can be invoked as if it was a parameter. 
Function parameters can be a mix of either types or concepts. 

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
Unlike traditional object-oriented languages, it does not have member functions (aka methods). 

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
It declares a list of functions that can be called on any type implementing the concept.
In the context of the concept, the implementing type is called `Self`.

Concepts are similar to an interface, mixin, or trait in other languages. 
Concepts can inherit from other concepts. 

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

The [Plato standard library](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary) is the best place to start for Plato examples:

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

I am always interested in feedback in any form. You can find me on twitter as 
[@cdiggins](https://twitter.com/cdiggins) or mail me at [cdiggins@gmail.com](mailto:cdiggins@gmail.com).


