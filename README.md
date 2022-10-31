# Plato

Plato is a pure functional programming language based on C# syntax. 
The current Plato implementation generates cross-platform (.NET Standard 2.0) managed assemblies,
but will also support the generation of JavaScript code. 

Plato is designed as a general purpose cross-platform functional programming language that 
is appropriate for high-performance computing (e.g., 3D real-time graphics) and that is 
easy to teach and learn. 

The other goal of Plato is to develop a visual syntax so that Plato code 
can be viewed, manipulated, and created in either a node graph editor or 
as a text based language. 

# Examples

Here is an example of an input file written in Plato: [math.types.plato.cs](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.types.plato.cs)
and the generated C# code [math.types.plato.g.cs](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.types.plato.g.cs).

# Motivation 

Plato strives to tackle a few things:

1. Enforce immutability at the language level
2. Introduce an affine type system to track side-effects formally and allow safe usage of imperative coding patterns  
3. Generate assemblies that work across the widest number of platforms (e.g., Unity, and legacy .NET Framework)
4. Reduce boiler plate code 
5. Target web-browsers directly, without requiring the .NET runtime embedded  
6. Produce more efficient code while using functional programming paradigms 

# About the Affine Type System

In Plato an affine type is a type whose values cannot be aliased (shared). It allows for apparent in-place mutation
and strict ordering of side effects (like a monad). 

Only fields of an affine type can be modified. 
An affine type instance cannot be captured by a lambda or stored in an an array or a non-affine type instance .

Affine types can be used for representing external libraries with side effects, interactions with the real world (e.g. Console, Files), 
or writing code that manipulates structures (e.g., Lists and read-write Arrays) in a safe way. 

# Performance 

Early experiments with Plato demonstrated that it could produce much more efficient byte-code than a regular C# compiler
for comparable code. This makes sense given that Plato can safely make a number of assumptions about aliasing and side effects,
and the fact that the C# compiler is not optimized for functional programming patterns. 

# Current Status 

Right now I am building a large math and geometry library using Plato. 

# Final Words

I am always interested in feedback in any form. 
You can find me on twitter as [@cdiggins](https://twitter.com/cdiggins).
