
# Plato Overview

**Plato** is a cross-platform programming language by [Christopher Diggins](https://github.com/cdiggins) inspired by JavaScript, TypeScript, and C#. 
Plato is designed to be easy to teach and learn while being efficient and robust enough for 
professional coding, particularly in the realm of 3D graphics. 

## About

Plato is a statically typed functional language that looks and behaves in many ways like an 
object-oriented scripting language, but with a lot less complexity.  

The most notable features of Plato are that all types are immutable, and there are no implicit this parameters. The trickle down result is that there is no distinction between methods and functions, no need for visibility modifiers (e.g., public, private, protected, internal), no virtual or abstract methods. 

## Motivation 

Plato is being used to develop cross-platform libraries that form the basis for software we are developing at 
[Ara 3D](https://ara3d.com). We do a lot of computational geometry work in different languages
and recreating and maintaining the same code for multiple platforms and languages is too costly and burdensome for a
small company. 

## Building Plato 

Plato tools and libraries are not intended for general usage yet as it is undergoing a lot of churn. This repository is designed to be used only as a submodule within the [Ara3D main repository](https://github.com/ara3d/ara3d).   


## Learning about Plato 

Right now the best way to learn about Plato is via the 
[Plato.Mathematics.NET](https://github.com/cdiggins/Plato.Mathematics.NET) project. This is where it is being put into a real-world use case. 

You can also check out the various [blog articles in this folder](https://github.com/cdiggins/plato/tree/main/blog).

## Status

While the language design is still undergoing a few minor tweaks, it is starting to reach a stable equilibrium. 

The first library currently being built with Plato is [Plato.Mathematics.NET](https://github.com/cdiggins/Plato.Mathematics.NET), a C# math library that can be used as a feature-rich replacement for System.Numerics. 

Once the Plato.Mathematics.NET library is stable and ready for publishing on Nuget I will write a language specification document for Plato.  

## Ideas for Projects
 
Some ideas for highly motivated and patient people who are interested in contributing to the Plato project:

* Optimizers - in particular Plato to Plato rewriting tools
* Type checking and inference tools 
* Transpilers to other languages (e.g., JavaScript, GO, Rust, Dart, Java, C++, GLSL, etc.) 
* Support for Plato syntax in popular coding tools (e.g., VS Code, Visual Studio, Avalon Edit, Scintilla, etc.)

## Feedback

I am always interested in feedback in any form. You can find me on twitter as 
[@cdiggins](https://twitter.com/cdiggins) or mail me at [cdiggins@gmail.com](mailto:cdiggins@gmail.com).


