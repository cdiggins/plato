# Plato

Plato is a pure functional programming language based on C# syntax. 
The current Plato implementation generates cross-platform (.NET Standard 2.0) managed assemblies,
that are compatible with Unity. 
Support is planned for generating JavaScript in the near future. 

Plato is designed as a general purpose cross-platform functional programming language that 
is appropriate for high-performance computing (e.g., 3D real-time graphics) and that is 
easy to teach and learn. 

The other goal of Plato is to develop a visual syntax so that Plato code 
can be viewed, manipulated, and created in either a node graph editor or 
as a text based language. 

# Examples

Here are two example of Plato code: 

* [math.types.plato.cs](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.types.plato.cs)
* [math.funcs.plato.cs](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.funcs.plato.cs)

and the generated C# code 

* [math.types.plato.g.cs](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.types.plato.g.cs).
* [math.funcs.plato.g.cs](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.funcs.plato.g.cs).

# Motivation 

Plato strives to tackle a few things:

1. Enforce immutability at the language level
2. Use uniqueness typing to track side-effects formally and allow safe usage of imperative coding patterns  
3. Generate assemblies that work across the widest number of platforms (e.g., Unity, and legacy .NET Framework)
4. Reduce boiler plate code 
5. Target web-browsers directly, without requiring the .NET runtime embedded  
6. Produce more efficient code while using functional programming paradigms 

# About Uniqueness Typing

In Plato a unique type is a type whose values cannot be aliased (shared). 
It allows for in-place mutation and strict ordering of side effects (like a monad).

A unique type instance cannot be captured by a lambda or stored in an an array or a 
non-unique type instance .

Unique types can be used for representing external libraries with side effects, 
interactions with the real world (e.g. Console, Files), 
or writing code that manipulates structures (e.g., Lists and read-write Arrays) in a safe way. 

# Performance 

Early experiments with Plato have demonstrated that it can produce significantly more 
efficient byte-code than a regular C# compiler
for comparable code. This is because Plato can safely make a number of assumptions 
about aliasing and side effects. It is also possible because the C# compiler is not optimized 
for functional programming patterns. 

# Current Status 

Right now I am building a large math and geometry library using Plato. 

# Final Words

I am always interested in feedback in any form. 
You can find me on twitter as [@cdiggins](https://twitter.com/cdiggins).
