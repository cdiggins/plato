# Plato

Plato is a pure functional programming language derived from C#.  

Some features:

* Syntax is a strict subset of C# and works with Visual Studio 
* Support for referential transparency is provided through affine types
* Current implementation is targetting .NET and JavaScript 

# Quick Overview

In general Plato supports most of the syntax and semantics of C# with some notable 
differences: 

### Classes and User Defined Type 

Plato supports only two kind of user-defined data type: enums and classes. Structs and records
are not allowed. All classes must have the partial keyword as part of their declaration so
that the code generator can generate necessary additional code (e.g. With functions). 

The Plato compiler may decide to implement the class as a struct or class, but this is invisble
to the users. 

### Arrays

Arrays cannot be modified once they are created.  

### Reflection

The only run-time reflection capability provided is querying the type of an object. 
Both `is` and `as` operations work as expected. 

### Immutability

All classes in Plato are immutable by default, unless they are labeled as affine types.  
This means they cannot be modified at run-time. 

### Imported Types

All types imported from non-Plato libraries are treated as affine types. 

### Affine Types 

Classes with the `[Affine]` attribute are mutable. 
Functions on an affine class, may
modify fields of the class. Instances of an Affine class:

* Cannot be captured by a lambda
* Cannot be stored in an array
* Cannot be stored as a member of a non-affine type

They can however be passed to a function by argument or returned from a function. 

### Properties 

Plato properties cannot have setters. 

### Init Properties and With Functions

Plato properties declared with an `init` keyword trigger the generation of a corresponding `With<Name>()` function.

### Lambdas

Plato lambdas capture values not variables. 

### LINQ Query Syntax

LINQ Query Syntax is not allowed. 

### Unsafe

Unsafe code blocks are not allowed. 

# History and Motivation

I have been working with C# for over 15 years, using it in various domains such as real-time 3D applications. 

I used to be a reasonably competent C++ developer. I co-authored the [C++ Cookbook from O'Reilly press](https://www.amazon.ca/Cookbook-Solutions-Examples-Programmers/dp/0596007612) 
and was a regular contributor to the [C++ Users Journal](https://en.wikipedia.org/wiki/C/C%2B%2B_Users_Journal).

I was orignally quite skeptical of the C# language when it was introduced, dismissing it as a Java clone, but I 
eventually found that C# provided me with a significant productivity boost. As the language matured it became my 
primary go-to language. 

I have always been fascinated with programming language theory and design, and even created a couple: 
[Cat](https://github.com/cdiggins/cat-language), [Heron](https://github.com/cdiggins/heron-language), 
[Chickadee](https://github.com/Clemex/chickadee), and [Max Creation Graph](https://knowledge.autodesk.com/support/3ds-max/learn-explore/caas/CloudHelp/cloudhelp/2017/ENU/3DSMax/files/GUID-608EC963-75ED-4F63-96B7-D8AE57E75959-htm.html). 

While I enjoyed creating and using these languages,I found that when I switch hats from language designer to
professional softare developer there wasn't a very satisfying reason for me to use any of them in commerical products. 

As a developer there are several things I look for when choosing a programming language for a project.

1. **Familiarity** - how familiar is the development team with the syntax, semantics, and idioms of the langauge, and how
long would it take for them to ramp up. 
2. **Expressiveness** - how easy is it to write algorithms and data-structures for the relevant problem domain. 
3. **Performance** - does common implementations of the language provide adequates performance for the problem domain

Beyond the actual language itself I also consider the following factors: 

1. **Tooling** 
1. **Libraries** 
1. **Documentation** 
1. **Community** 

These were all things that C# did adequately for the majority of work I did, except when need to develop web-clients,
for which I would use either JavaScript or TypeScript. 

Supporting two code bases and separate tool-chains, creates challenges for us in time, cost, and people. 
This has become one of the primary motivators for implementing a new language: to enable myself and my 
colleagues to use single shared a code-base for both desktop applications and web browser apps. 

As C# evolved and introduced new features, many tended to fall into one of two categories: improving support for 
functional programming or for improved performance. It has gotten to the point that I think it fair to say that 
their exists two language within C#:

1. A **high-level** cross-platform language with support for functional programming and immutable data-structure 
2. A **low-level** language that emphasizes low-level control over memory and performance   

One problem is that the two sides of the language don't work together well. The high-level features have poor performance 
and the low-level features are complex and unsafe. 

As a developer I want to work with a single high-level cross-platform langauge that is reliable and has good performance. 
As a software development lead, I want my team to be able to produce high quality code with low effort, and to be 
comfortable with tooling environment. 

Plato attempts to solve the problem by:

1. Restricting the language to a pure-functional subset
2. Providing an optimizer that rewrites Plato so that it can be executed efficiently by the run-time 
3. Provide a cross-compiler, to generate efficient JavaScript code

## Status: April 22nd, 2022

Right now I am working on the Plato to JavaScript compiler which is being implemented via a Roslyn 
Source generator. 

Previously I was designing the standard library and experimenting with Plato semantics using 
various C# projects. 

## Final Words

I am always interested in feedback in any form. 
You can find me on twitter as [@cdiggins](https://twitter.com/cdiggins).
