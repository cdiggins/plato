# Plato

Plato is a pure functional programming language based on C# syntax. 
The current Plato implementation generates cross-platform (.NET Standard 2.0) managed assemblies,
that are compatible with Unity. 

Plato is designed as a general purpose cross-platform functional programming language that 
is appropriate for high-performance computing (e.g., 3D real-time graphics) and that is 
easy to teach and learn. 

Another goal of Plato is to develop a visual syntax so that Plato code 
can be viewed, manipulated, and created in either a node graph editor or 
as a text based language. 

A Plato to JavaScript compiler is in progress as well.

# Examples

Here are two example of Plato code: 

* [math.types.plato.cs](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.types.plato.cs)
* [math.funcs.plato.cs](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.funcs.plato.cs)

and the generated C# code 

* [math.types.plato.g.cs](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.types.plato.g.cs).
* [math.funcs.plato.g.cs](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/math.funcs.plato.g.cs).

```csharp
    [Operations]
    class UnitOperations
    {
        Angle Radians(double d) => d;
        Angle Turns(double d) => d * 2 * Math.PI;
        Angle Degrees(double d) => Turns(d / 360.0);
        Angle Grads(double d) => Turns(d / 400.0);
        Angle ArcMinutes(double d) => Degrees(d / 60);
        Angle ArcSeconds(double d) => ArcMinutes(d / 60);
        double ToTurns(Angle a) => a * 2 * Math.PI;
        double ToDegrees(Angle a) => ToTurns(a) * 360;
        double ToGrads(Angle a) => ToTurns(a) * 400;
        double ToArcMinutes(Angle a) => ToDegrees(a) * 60;
        double ToArcSeconds(Angle a) => ToArcMinutes(a) * 60;
    }
```

# Notable Features of Plato

Plato is very similar in syntax to C# but with a number of simplifications:

1. No structs or records, just classes - the compiler may generate a struct, but this does not change the behavior of a program 
1. No visibility specifiers - all types and members are public 
1. No setters - all properties are getters
1. No readonly keyword - all fields are readonly unless in a Unique type
1. No static keyword - static classes and methods are inferred 
1. All static methods are extensions - any function that is inferred to be static is an extension method.
1. Boilerplate code generates for all classes 
1. Extra boilerplate code generated for special types - value, number, measure, interval, vector

See the following article for more information:

* [wiki/Static-Classes-and-Methods](https://github.com/cdiggins/plato/wiki/Static-Classes-and-Methods)
* [wiki/Static-Classes-and-Methods](https://github.com/cdiggins/plato/wiki/Static-Classes-and-Methods)



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
This allows for in-place mutation and strict ordering of side effects (like a monad),
while maintaining referential transparency (aka purity).

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
