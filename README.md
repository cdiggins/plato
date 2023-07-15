# Plato

Plato is an efficient and fun programming language inspired by JavaScript, TypeScript, C#, and Haskell.
Plato is designed to be easy to teach and learn while being efficient and robust enough for 
production quality coding. Plato can be translated into C#, JavaScript, and other platforms. 

# About Plato 

Plato is a statically typed compiled functional language that looks like a dynamic scripting language. 
Type annotations are optional, except when defining new types or concepts. 

Plato behaves like an object oriented language in that you can use fluent (aka method chaining) syntax.
In other words any defined function can be invoked by writing the first argument followed by a "." 
then the rest of the arguments. `Clamp(x, min, max)` can be invoked as `x.Clamp(min, max)`.

If a function does not require additional arguments then the `()` can be omitted. This means that any
function with only one parameter (e.g. `Cos(x: Number)`) can be invoked as if it was a property: `x.Cos`.

## Functions and Modules

Plato functions can be defined with or without type declarations. If type declarations are omitted then the type is inferred.

    Note: Plato functions are not methods. They have no implicit "this" parameter.   

Functions are organized in groups called modules. A module is like a class with no state, and only static methods in languages like C#, Java, or C++.  

For some [examples of functions and modules see the standard library](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/modules.plato).

## Types and Fields

A type assigned a name to a set of fields (member variables). Types do not contain functions. A type usually implements at 
least one or more concepts. 

For some [examples of types see the standard library](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/types.plato).

## Concepts 

A concept is a set of fields and functions that describe a particular type. They are similar to 
interfaces, traits, protocols, and mixins. A type that "implements" a concept must have the fields 
declared in the concept. The functions within a concept are automatically implemented for the types that declare that they 
implement the concept.  

For some [examples of concepts see the standard library](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/concepts.plato).

# Feedback

I am always interested in feedback in any form. You can find me on twitter as [@cdiggins](https://twitter.com/cdiggins) or mail me at [cdiggins@gmail.com](mailto:cdiggins@gmail.com).
