![Plato-logo-edit](https://github.com/cdiggins/plato/assets/1759994/378437d5-9ddb-4028-812b-02be2c71279e)

# Plato Overview

## About

Plato is an efficient and (hopefully!) fun programming language inspired by JavaScript, TypeScript, C#, and Haskell. 
Plato is designed to be easy to teach and learn while being efficient and robust enough for 
professional coding, particularly in the realm of 3D graphics. 

Plato is a statically typed functional language that looks and behaves in many ways like an 
object-oriented scripting language, but with a lot less complexity.  

## Use Case 

The first use case for Plato is as the basis for cross-platform 3D math and geometry libraries that form the basis
for software we are developing at [Ara 3D](https://ara3d.com). We do a lot of computational geometry work in different langauges
and recreating and maintaining the same code for multuple platforms and languages is too costly and burdensome for a
small company. 

## Status

As of today, September 6th, some of the final touches are being put on the type system to be able to properly deal with
function overload resolution and start generating some code. We hope to be able to start publishing the first .NET 2.0 
compatible math and geometry libraries authored in Plato by October 1st. 

# Language Details

## Functions 

A function takes zero, one, or more inputs and returns a single output. Unlike object-oriented 
languages Plato functions are never instance methods. They have no implicit "this" parameter.   

An example of a Factorial function is :

```Typescript
Factorial(n: Integer): Integer => 
   n <= 1 
      ? 1 
      : n * Factorial(n-1);
```

Functions can equivalently be written in a statement form:

```Typescript
Factorial(n: Integer): Integer => 
{
    if (n <= 1)
        return 1;
    return n * Factorial(n-1);
}
```
## Calling Functions

Plato supports both prefix and method chaining syntax of functions. 

In other words any function can be invoked by writing the first argument followed by a "." 
then the rest of the arguments. For example the expression `Clamp(x, min, max)` can be written equivalently as `x.Clamp(min, max)`.

If a function does not require additional arguments then the `()` can be omitted when called as if it is method. This means that any
function with only one parameter (e.g. `Cos(x: Number)`) can be invoked as if it was a property: `x.Cos`.

## Libraries 

Functions are organized in groups called libraries. A `library` is just a list of functions. It has no
no state (variables). 

For some [examples of functions and libraries see](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/libraries.plato).

## Expressions

The following are some of the valid kinds of expressions in Plato:

* Literals: integer, float, text, boolean 
* Function calls
* Tuples
* Conditional expression - aka ternary operator `?` and `:`
* Parenthesized expressions
* Lambda expressions - anonymous functions
* Binary operations 
* Unary operations
* Throw operations

## Statements

In idiomatic Plato code (aka Platonic code) it is more common to use expressions than 
statements. Statements do not have a value. 
Functions with statement bodies are converted into expression bodies 
by the compiler. 

Plato supports the following statements:

* Looping - `while` statements  
* Conditional - `if` / `else` statements
* Control flow - `return`, `break`, `continue` statements
* Expression statements - assignment, function call, throw
* Variable declarations - `var` statement 
* Statement blocks delimited by `{` and `}`

## Types, Values, and Fields

A `type` assigns a name to a set of fields (member variables). Types do not contain methods. 

A type may `implement` one or more concepts.
When a type declares that it implements a concept, it will support all functions 
defined within the concept. 

For some [examples of types see the standard library](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/types.plato).

## Concepts 

A `concept` is a set of functions that describes an
abstract data type. They are similar to 
interfaces, traits, protocols, and mixins in other languages. 

A concept can contain a mix of defined functions and unimplemented functions, 
i.e., function signatures with no bodies. All functions in a concept require
explicit type annotations. 

A type that implements a concept must provide implementations of any unimplemented functions 
declared in the concept. Those functions can be defined in any library defined in a project.

For example the Plato array concept is:

```typescript
concept Array<T: Any>
{
    Count(xs: Self): Count;
    At(xs: Self, n: Index): T;
}
```

Within a concept the `Self` keyword refers to the implementing type. 

For some [examples of concepts see the standard library](https://github.com/cdiggins/plato/blob/main/PlatoStandardLibrary/concepts.plato).

## Operator Overloading

Plato binary and unary operators are mapped to functions with a specific name. Another words, if a function exists 
with the given name that supports the types of the provided operators, it will be called. 
This makes defining new operators quick and easy. 

### Binary Operators 

* `Add` -> `+`
* `Subtract` -> `-`
* `Multiply` -> `*`
* `Divide` -> `/`
* `Modulo` -> `%`
* `LessThan` -> `<`
* `LessThanOrEquals` -> `<=`
* `GreaterThan` -> `>`
* `GreaterThanOrEquals` -> `>=`
* `And` -> `&&`
* `Or` -> `||`
* `XOr` -> `^`
* `Equals` -> `==`
* `NotEquals` -> `!=`
* `At` -> `[]`

### Unary Operators 

* `Negative` -> `-`
* `Not` -> `!`

## Type Inference 

Plato supports a limited form of type inference. 
Plato supports generics (types with parameters), but type parameters are not specified explicitly: they are inferred based on the function definition.   

## Features that Plato does not have

Plato does not have many features found in common object-oriented languages.
This is by design, to keep the language simple
and consistent.  

The following are features that Plato will never have:

* Visibility specifiers - `public`/`private`/`protected`/`internal`
* Additional modifiers `sealed`/`abstract`/`virtual`/`static`
* Implicit `this` parameters 
* `ref`/`in`/`out` parameters
* unsafe mode 
* Interfaces / traits / mixins / prototcols

Features that we can exepct Plato to support in the near future:

* Support for unique types - types which have only one reference and can be mutated. 
* A visual syntax - a representation of Plato code as a dataflow graph. 
* Default concept implementations - concepts have auto-generated default implementations
that are a tuple of fields and functions. 
* Implicit tuple conversions 
* Partial generics - being able to just use "Func" or "Array" in annotation instead of requiring whole annotation.

Features that may be on the longer term roadmap:

* asyncronous language support (`async`/`await`)
* generators/iterator methods/coroutines (`yield`)
* covariance and contravariance of generic type parameters 

## In Development: A Text Language with a Visual Representation 

Plato is being developed with a regular text-based syntax, with an alternative visual 
representation as a data-flow graph. The goal is to be able to switch seamlessly between visual and 
textual representations with no change in semantics or layout.   

This UI for data-flow graphs is in its early stages, but here is a preview:
![DataFlowEx1](https://github.com/cdiggins/plato/assets/1759994/c8d4d6e4-a138-42d1-99c9-6fa891254ede)

The data flow representation is inspired by the [MCG visual programming language](https://help.autodesk.com/view/MAXDEV/2023/ENU/?guid=MAXDEV_MCG_getting_started_creating_a_tool_html) I helped design for 3ds Max. 

# Backstory 

I have been programming for over 30 years, and writing 3D graphics code in various languages for the last 15 years.
I have either written or referenced the same algorithms written over and over again to varying degrees of 
correctness and efficiency. 

I worked at Autodesk for several years, and they had easily over a hundred different libraries that each 
have their own way of representing basic 3D vectors and their operations.  

Most algorithms in computer science, once written correctly, shouldn't need to be changed, 
so clearly the problem lies in the tools and languages that we, as developers, have available to use.

I set out to design a language which in I could use to write a comprehensive 3D graphics library that could be 
translated automatically into different targets, such as JavaScript, C#, C++, and even shader languages like GLSL. 

I believe that a carefully designed modern programming language can be robust and easy to use for both newcomers
and experienced professionals alike. 

I also believe that languages and tools with quick feedback and graphics are the best way to introduce programming 
concepts to new programmers.    

# Feedback

I am always interested in feedback in any form. You can find me on twitter as 
[@cdiggins](https://twitter.com/cdiggins) or mail me at [cdiggins@gmail.com](mailto:cdiggins@gmail.com).

