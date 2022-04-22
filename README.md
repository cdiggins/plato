# Plato

Plato is a pure functional programming language derived from C#.  

Some features:

* Syntax is a strict subset of C# and works with Visual Studio 
* Support for referential transparency is provided through affine types
* Current implementation is targetting .NET and JavaScript 

## History and Motivation

I have been working in C# for over 15 years, using it real-time 3D applications. 

I used to be a reasonably competent C++ developer. I co-authored the [C++ Cookbook from O'Reilly press](https://www.amazon.ca/Cookbook-Solutions-Examples-Programmers/dp/0596007612) 
and was a regular contributor to the [C++ Users Journal](https://en.wikipedia.org/wiki/C/C%2B%2B_Users_Journal).

I was orignally quite skeptical of the C# language when it was introduced, dismissing it as a Java clone, but I 
eventually found that C# provided me with a significant productivity boost. As the language matured it became my 
primary go-to language. 

I have always been fascinated with programming language theory and design, and even created a couple: 
[Cat](https://github.com/cdiggins/cat-language), [Heron](https://github.com/cdiggins/heron-language), 
[Chickadee](https://github.com/Clemex/chickadee), and [Max Creation Graph](https://knowledge.autodesk.com/support/3ds-max/learn-explore/caas/CloudHelp/cloudhelp/2017/ENU/3DSMax/files/GUID-608EC963-75ED-4F63-96B7-D8AE57E75959-htm.html). 

While I enjoyed creating and using these languages, I found that when I switch hats from language designer to
professional softare developer there wasn't a very satisfying reason for me to use any of them in commerical products. 

As a developer I realized that look for several things when choosing a programming language for a project.

1. *Familiarity* - how familiar is the development team with the syntax, semantics, and idioms of the langauge, and how
long would it take for them to ramp up. 
2. *Expressiveness* - how easy is it to write algorithms and data-structures for the relevant problem domain. 
3. *Performance* - does common implementations of the language provide adequates performance for the problem domain

Beyond the language itself I usually consider the following 

1. Tooling 
1. Libraries 
1. Documentation 
1. Community 

These were all things that C# did more than adequately for the majority of work I did. The times that I needed to 
target the browser I opted to use JavaScript or TypeScript. I will confess it has been frustrating tryin to switch 
between two different languages and tool-chains, and maintaining two code-bases which resemble each other very closely
apart from the syntax. 

As C# evolved and introduced new features, many tended to fall into one of two categories: improving support for 
functional programming or for improved performance. It has gotten to the point that I think it fair to say that 
their exists two language within C#:

1. A high-level cross-platform language with support for functional programming and immutable data-structure 
2. A low-level language that emphasizes low-level control over memory and performance   

One problem is that the two sides of the language don't work together well. High-level features have poor performance 
and low-level features are complex and unsafe. 

As a developer I want to work with a single high-level cross-platform langauge that is reliable and has good performance. 

Plato attempts to solve the problem by:

1. Restricting the language to a pure-functional subset
2. Providing an optimizer that rewrites Plato so that it can be executed efficiently by the run-time 
3. Provide a cross-compiler, to generate efficient JavaScript code

## Status: April 22nd, 2022

Right now I am working on the Plato to JavaScript compiler which is being implemented via a Roslyn 
Source generator. 

Previously I was designing the standard library and experimenting with Plato semantics using 
various C# projects. 
